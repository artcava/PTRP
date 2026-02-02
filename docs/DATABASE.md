# DATABASE.md - Schema Logico SQLite PTRP

## 📋 Panoramica

Questo documento descrive lo **schema logico del database SQLite** utilizzato da PTRP per la gestione offline-first di Pazienti, Progetti Terapeutici, Operatori e Visite.

### Caratteristiche Architetturali
- **Database Locale**: SQLite criptato con AES-256
- **Nessun SQL Server Esterno**: Ogni installazione è completamente autonoma
- **Migrazioni EF Core**: Schema versionato e migrabile
- **Idempotenza**: Seeding automatico alla prima esecuzione (vedi [SEED.md](../SEED.md))
- **Integrità Referenziale**: FK con ON DELETE CASCADE/SET NULL secondo logica di dominio
- **Audit Trail**: Timestamp CreatedAt/UpdatedAt su tutte le entità
- **Offline-First Sync**: GUID come identificatori per sincronizzazione cross-database

---

## 🗂️ Struttura Entità Principale (ER Diagram)

### Architettura Concettuale

**Relazioni Fondamentali** (semplificate, senza entrare nelle regole applicative):
1. **Paziente ← 1:N → Progetto**  
   Un paziente può avere più progetti nel tempo (sequenziali o paralleli), ma ogni progetto appartiene ad un solo paziente.

2. **Progetto ← N:N → Operatore**  
   Un progetto può coinvolgere più operatori, e ogni operatore può lavorare su più progetti. Ruoli e durata dell'assegnazione sono rappresentati nella tabella di giunzione.

3. **Progetto ← 1:N → Visita Programmata**  
   Un progetto può avere molte visite programmate. **Il numero e la tipologia delle visite sono determinati da regole di business nel codice**, non codificati a livello di schema (niente vincolo "4 visite canoniche" qui). Le visite programmate puntano ad una tipologia predefinita (tabella `visit_types`).

4. **Visita Programmata ← 1:1 → Visita Effettiva**  
   Una visita programmata può avere **al massimo una** visita effettiva. Quando la visita effettiva viene registrata, sancisce l'esecuzione della visita programmata. Se l'utente (o le regole di schedulazione) decide di riprogrammare, verrà creata **una nuova** visita programmata.

5. **Visita Effettiva ← N:N → Operatore**  
   Una visita effettiva può essere svolta da più operatori; ogni operatore partecipa a molte visite. Questa relazione è modellata tramite una tabella di giunzione (`actual_visit_operators`).

### ER Diagram ASCII (alto livello)

> **Nota**: lo schema qui è intenzionalmente compatto; dettagli aggiuntivi su regole di schedulazione e casi d'uso sono descritti in `PROGETTO_PTRP_SYNC.md`.

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                          PTRP DATABASE MODEL                                │
└─────────────────────────────────────────────────────────────────────────────┘

┌──────────────────────────┐                    ┌──────────────────────────┐
│        PATIENTS          │1                 N│   THERAPEUTIC_PROJECTS   │
│ (id, first_name, ...)    │◄──────────────────│ (id, patient_id, ...)    │
└──────────────────────────┘                    └───────────┬──────────────┘
                                                          1 │
                                                            │ N
                                                   ┌────────▼───────────┐
                                                   │   SCHEDULED_VISITS │
                                                   │ (id, project_id,   │
                                                   │  visit_type_id,...)│
                                                   └────────┬───────────┘
                                                            │ 1:1
                                                            │
                                                   ┌────────▼───────────┐
                                                   │   ACTUAL_VISITS    │
                                                   │ (id, scheduled_    │
                                                   │  visit_id, ...)    │
                                                   └────────┬───────────┘
                                                            │ N:N
                                                            │
┌──────────────────────────┐                    ┌───────────▼───────────┐
│        OPERATORS         │N                 N│ ACTUAL_VISIT_OPERATORS │
│ (id, first_name, ...)    │◄──────────────────│ (actual_visit_id,      │
└───────────┬──────────────┘                    │  operator_id, role)   │
            │                                   └───────────────────────┘
            │ N:N
            │
┌───────────▼──────────────┐
│    PROJECT_OPERATORS      │
│ (project_id, operator_id, │
│  role_in_project, ...)    │
└───────────┬──────────────┘
            │ N
            │
┌───────────▼──────────────┐
│       OPERATORS          │ (stessa tabella di sopra)
└──────────────────────────┘

┌──────────────────────────┐
│      VISIT_TYPES         │
│ (id, code, description)  │
└──────────────────────────┘
```

---

## 🎯 Enumerazioni di Dominio

### ProjectStatus (Stati Progetto Terapeutico)

```sql
-- Implementato come ENUM string in C#, memorizzato come TEXT
CREATE TABLE therapeutic_projects (
    ...
    status TEXT NOT NULL DEFAULT 'Active', -- 'Active' | 'Suspended' | 'Completed' | 'Deceased'
    ...
);
```

**Valori validi:**
- `Active` - Progetto in corso
- `Suspended` - Progetto temporaneamente sospeso
- `Completed` - Progetto concluso regolarmente
- `Deceased` - Paziente deceduto

**Vincolo applicativo critico:** Un paziente può avere UN SOLO progetto con stato `Active` contemporaneamente.

---

### VisitType (Tipologie Visita)

```sql
CREATE TABLE visit_types (
    id              TEXT PRIMARY KEY,
    code            TEXT NOT NULL UNIQUE,  -- 'Intake' | 'Intermediate' | 'Final' | 'Discharge' | 'ExtraVisit'
    description     TEXT NOT NULL,
    is_active       INTEGER NOT NULL DEFAULT 1,
    created_at      DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at      DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);
```

**Valori previsti (seeding iniziale):**
- `Intake` - Prima apertura (prima visita del progetto)
- `Intermediate` - Verifica intermedia (dopo 6 mesi)
- `Final` - Verifica finale (dopo ulteriori 6 mesi)
- `Discharge` - Dimissioni (dopo 1 mese dalla verifica finale)
- `ExtraVisit` - Visita aggiuntiva/straordinaria

**Regola schedulazione appuntamenti canonici (4 obbligatori):**
1. Prima Apertura (INTAKE): StartDate + 3 mesi (90 min)
2. Verifica Intermedia: INTAKE + 6 mesi (60 min)
3. Verifica Finale: Verifica Intermedia + 6 mesi (60 min)
4. Dimissioni (DISCHARGE): Verifica Finale + 1 mese (45 min)

---

### AppointmentStatus (Stati Appuntamento Programmato)

```sql
CREATE TABLE scheduled_visits (
    ...
    status TEXT NOT NULL DEFAULT 'Scheduled', -- 'Scheduled' | 'Completed' | 'Missed' | 'Rescheduled'
    ...
);
```

**Valori validi:**
- `Scheduled` - Appuntamento programmato (non ancora effettuato)
- `Completed` - Appuntamento completato (esiste ActualVisit collegata)
- `Missed` - Appuntamento mancato (paziente non si è presentato)
- `Rescheduled` - Appuntamento riprogrammato (creato nuovo ScheduledVisit)

---

### PatientAttendance (Presenza Paziente alla Visita)

```sql
CREATE TABLE actual_visits (
    ...
    attendance_status TEXT NOT NULL, -- 'PresentCollaborative' | 'PresentNonCollaborative' | 'AbsentJustified' | 'AbsentUnjustified'
    ...
);
```

**Valori validi:**
- `PresentCollaborative` - Paziente presente e collaborativo
- `PresentNonCollaborative` - Paziente presente ma non collaborativo
- `AbsentJustified` - Paziente assente con giustificazione
- `AbsentUnjustified` - Paziente assente senza giustificazione

**Uso:** Tracciamento partecipazione paziente durante visita effettiva.

---

### VisitSource (Origine Registrazione Visita)

```sql
CREATE TABLE actual_visits (
    ...
    source TEXT NOT NULL DEFAULT 'CoordinatorDirect', -- 'EducatorImport' | 'CoordinatorDirect'
    ...
);
```

**Valori validi:**
- `EducatorImport` - Visita registrata da educatore e importata via pacchetto sync
- `CoordinatorDirect` - Visita registrata direttamente da coordinatore

**Uso:** Audit trail per tracciare origine dati sincronizzazione.

---

## 📊 Schema Dettagliato Tabelle

### 1. **THERAPEUTIC_PROJECTS** (Progetti Terapeutici)

```sql
CREATE TABLE therapeutic_projects (
    id                      TEXT PRIMARY KEY,
    patient_id              TEXT NOT NULL,
    title                   TEXT NOT NULL,
    description             TEXT,
    start_date              DATE NOT NULL,
    planned_end_date        DATE,
    actual_end_date         DATE,
    status                  TEXT NOT NULL DEFAULT 'Active', -- ProjectStatus enum
    notes                   TEXT,
    created_at              DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at              DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by              TEXT,
    updated_by              TEXT,
    version                 INTEGER DEFAULT 1,
    
    FOREIGN KEY (patient_id) REFERENCES patients(id) ON DELETE CASCADE,
    
    -- Constraint applicativo: paziente può avere UN SOLO progetto Active
    -- Verificato a livello servizio (TherapyProjectService)
);

CREATE INDEX idx_therapeutic_projects_patient_id ON therapeutic_projects(patient_id);
CREATE INDEX idx_therapeutic_projects_status ON therapeutic_projects(status);
CREATE INDEX idx_therapeutic_projects_start_date ON therapeutic_projects(start_date);
CREATE INDEX idx_therapeutic_projects_updated_at ON therapeutic_projects(updated_at);
```

**Vincoli Critici:**
- `status` deve essere uno dei valori `ProjectStatus`
- Un paziente può avere UN SOLO progetto con `status = 'Active'` (validato applicativamente)
- `start_date` obbligatoria
- `planned_end_date` >= `start_date` (se specificata)

---

### 2. **VISIT_TYPES** (Tipologie di Visita)

```sql
CREATE TABLE visit_types (
    id              TEXT PRIMARY KEY,            -- GUID o codice
    code            TEXT NOT NULL UNIQUE,        -- Es. 'Intake', 'Intermediate', 'Final', 'Discharge', 'ExtraVisit'
    description     TEXT NOT NULL,               -- Descrizione leggibile
    is_active       INTEGER NOT NULL DEFAULT 1,  -- Per permettere di "disabilitare" tipi
    created_at      DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at      DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_visit_types_is_active ON visit_types(is_active);
```

**Seeding Iniziale:**
```sql
INSERT INTO visit_types (id, code, description) VALUES 
    ('intake-id', 'Intake', 'Prima apertura - valutazione iniziale'),
    ('intermediate-id', 'Intermediate', 'Verifica intermedia'),
    ('final-id', 'Final', 'Verifica finale'),
    ('discharge-id', 'Discharge', 'Dimissioni'),
    ('extra-id', 'ExtraVisit', 'Visita aggiuntiva/straordinaria');
```

---

### 3. **SCHEDULED_VISITS** (Visite Programmate)

```sql
CREATE TABLE scheduled_visits (
    id                      TEXT PRIMARY KEY,              -- GUID
    project_id              TEXT NOT NULL,                 -- FK → therapeutic_projects
    visit_type_id           TEXT NOT NULL,                 -- FK → visit_types
    scheduled_date          DATE NOT NULL,                 -- Data programmata
    scheduled_start_time    TIME,                          -- Ora inizio prevista
    expected_duration_min   INTEGER,                       -- Durata attesa in minuti
    status                  TEXT NOT NULL DEFAULT 'Scheduled', -- AppointmentStatus enum
    location                TEXT,                          -- Luogo visita (es. sede, domicilio)
    notes                   TEXT,
    created_at              DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at              DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by              TEXT,
    updated_by              TEXT,
    version                 INTEGER DEFAULT 1,
    
    FOREIGN KEY (project_id) REFERENCES therapeutic_projects(id) ON DELETE CASCADE,
    FOREIGN KEY (visit_type_id) REFERENCES visit_types(id) ON DELETE RESTRICT
);

CREATE INDEX idx_scheduled_visits_project_id ON scheduled_visits(project_id);
CREATE INDEX idx_scheduled_visits_visit_type_id ON scheduled_visits(visit_type_id);
CREATE INDEX idx_scheduled_visits_status ON scheduled_visits(status);
CREATE INDEX idx_scheduled_visits_scheduled_date ON scheduled_visits(scheduled_date);
CREATE INDEX idx_scheduled_visits_updated_at ON scheduled_visits(updated_at);
```

**Vincoli Critici:**
- `status` deve essere uno dei valori `AppointmentStatus`
- `scheduled_date` non può essere passata (validato applicativamente)
- `visit_type_id` deve esistere in `visit_types`

---

### 4. **ACTUAL_VISITS** (Visite Effettive)

**Modifica rispetto alla versione precedente**: la relazione con `scheduled_visits` è ora **1:1** (una visita programmata → al massimo una visita effettiva).

```sql
CREATE TABLE actual_visits (
    id                      TEXT PRIMARY KEY,              -- GUID
    scheduled_visit_id      TEXT NOT NULL UNIQUE,          -- FK → scheduled_visits, 1:1
    actual_date             DATE NOT NULL,
    actual_start_time       TIME,
    actual_end_time         TIME,
    source                  TEXT NOT NULL DEFAULT 'CoordinatorDirect', -- VisitSource enum
    registration_date       DATETIME NOT NULL,
    clinical_notes          TEXT NOT NULL,                 -- Obbligatorio
    outcomes                TEXT,
    attendance_status       TEXT NOT NULL,                 -- PatientAttendance enum
    signature_hash          TEXT,
    created_at              DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at              DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by              TEXT,
    updated_by              TEXT,
    version                 INTEGER DEFAULT 1,
    
    FOREIGN KEY (scheduled_visit_id) REFERENCES scheduled_visits(id) ON DELETE CASCADE,
    
    -- Constraint tempo: actual_end_time > actual_start_time (validato applicativamente)
    -- Constraint data: actual_date <= CURRENT_DATE (non futura)
);

CREATE INDEX idx_actual_visits_scheduled_visit_id ON actual_visits(scheduled_visit_id);
CREATE INDEX idx_actual_visits_actual_date ON actual_visits(actual_date);
CREATE INDEX idx_actual_visits_registration_date ON actual_visits(registration_date);
CREATE INDEX idx_actual_visits_updated_at ON actual_visits(updated_at);
CREATE INDEX idx_actual_visits_source ON actual_visits(source);
CREATE INDEX idx_actual_visits_attendance_status ON actual_visits(attendance_status);
```

**Vincoli Critici:**
- **Relazione 1:1 con ScheduledVisit**: `scheduled_visit_id` UNIQUE
- `actual_date` non può essere futura
- `actual_end_time` > `actual_start_time` (se entrambi specificati)
- `clinical_notes` obbligatorio (NOT NULL)
- `attendance_status` deve essere uno dei valori `PatientAttendance`
- `source` deve essere uno dei valori `VisitSource`

---

### 5. **ACTUAL_VISIT_OPERATORS** (N:N tra Visita Effettiva e Operatori)

**Descrizione**: Una visita effettiva può essere effettuata da **più operatori** (team), e ogni operatore partecipa a molte visite.

```sql
CREATE TABLE actual_visit_operators (
    id                      TEXT PRIMARY KEY,      -- GUID
    actual_visit_id         TEXT NOT NULL,         -- FK → actual_visits
    operator_id             TEXT NOT NULL,         -- FK → operators
    role_in_visit           TEXT NOT NULL,         -- Es. 'Lead' | 'Assistant'
    notes                   TEXT,
    created_at              DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at              DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by              TEXT,
    updated_by              TEXT,
    version                 INTEGER DEFAULT 1,
    
    FOREIGN KEY (actual_visit_id) REFERENCES actual_visits(id) ON DELETE CASCADE,
    FOREIGN KEY (operator_id) REFERENCES operators(id) ON DELETE RESTRICT,
    
    UNIQUE(actual_visit_id, operator_id) -- uno stesso operatore non compare due volte nella stessa visita
);

CREATE INDEX idx_actual_visit_operators_visit_id ON actual_visit_operators(actual_visit_id);
CREATE INDEX idx_actual_visit_operators_operator_id ON actual_visit_operators(operator_id);
```

**Esempi di query**:
```sql
-- Operatori che hanno partecipato a una visita
SELECT o.first_name, o.last_name, avo.role_in_visit
FROM actual_visit_operators avo
JOIN operators o ON avo.operator_id = o.id
WHERE avo.actual_visit_id = ?;

-- Tutte le visite effettive di un operatore
SELECT av.actual_date, vt.code as visit_type_code, p.first_name, p.last_name
FROM actual_visit_operators avo
JOIN actual_visits av ON avo.actual_visit_id = av.id
JOIN scheduled_visits sv ON av.scheduled_visit_id = sv.id
JOIN visit_types vt ON sv.visit_type_id = vt.id
JOIN therapeutic_projects tp ON sv.project_id = tp.id
JOIN patients p ON tp.patient_id = p.id
WHERE avo.operator_id = ?
ORDER BY av.actual_date DESC;
```

---

## 📋 Vincoli di Integrità Referenziale

| Relazione | Tipo | ON DELETE | Note |
|-----------|------|-----------|------|
| `therapeutic_projects → patients` | 1:N | CASCADE | Se paziente cancellato, cancella progetti |
| `project_operators → therapeutic_projects` | N:1 | CASCADE | Se progetto cancellato, cancella assegnazioni operatori |
| `project_operators → operators` | N:1 | RESTRICT | Non permette cancellazione operatore se ancora assegnato a progetti |
| `scheduled_visits → therapeutic_projects` | 1:N | CASCADE | Se progetto cancellato, cancella visite programmate |
| `scheduled_visits → visit_types` | N:1 | RESTRICT | Non si può cancellare una tipologia se usata |
| `actual_visits → scheduled_visits` | **1:1** | CASCADE | Una visita effettiva esiste solo se esiste la programmata (UNIQUE constraint) |
| `actual_visit_operators → actual_visits` | N:1 | CASCADE | Cancellare una visita effettiva rimuove tutti i suoi operatori |
| `actual_visit_operators → operators` | N:1 | RESTRICT | Non permette cancellazione operatore se referenziato |

---

## 🔐 Sicurezza e Crittografia

Vedi `docs/SECURITY.md` per dettagli completi.

**Database a Riposo:**
- AES-256 CBC
- Key derivation via PBKDF2 (≥10,000 iterazioni)
- Password utente come sorgente chiave

**Pacchetti Sincronizzazione:**
- HMAC-SHA256 per integrità
- AES-256 per payload cifrato
- Verifica firma prima di import

---

## 📊 Query di Esempio Comuni

### Progetti Attivi per Paziente
```sql
SELECT tp.*, 
       COUNT(DISTINCT sv.id) as total_scheduled_visits,
       COUNT(DISTINCT av.id) as total_completed_visits
FROM therapeutic_projects tp
LEFT JOIN scheduled_visits sv ON tp.id = sv.project_id
LEFT JOIN actual_visits av ON sv.id = av.scheduled_visit_id
WHERE tp.patient_id = ? AND tp.status = 'Active'
GROUP BY tp.id;
```

### Prossimi Appuntamenti Educatore
```sql
SELECT sv.scheduled_date, sv.scheduled_start_time,
       p.first_name, p.last_name,
       vt.description as visit_type
FROM scheduled_visits sv
JOIN therapeutic_projects tp ON sv.project_id = tp.id
JOIN patients p ON tp.patient_id = p.id
JOIN visit_types vt ON sv.visit_type_id = vt.id
JOIN project_operators po ON tp.id = po.project_id
WHERE po.operator_id = ?
  AND sv.status = 'Scheduled'
  AND sv.scheduled_date >= DATE('now')
ORDER BY sv.scheduled_date, sv.scheduled_start_time;
```

### Visite Completate con Presenza Non Collaborativa
```sql
SELECT av.actual_date, p.first_name, p.last_name,
       av.clinical_notes, av.attendance_status
FROM actual_visits av
JOIN scheduled_visits sv ON av.scheduled_visit_id = sv.id
JOIN therapeutic_projects tp ON sv.project_id = tp.id
JOIN patients p ON tp.patient_id = p.id
WHERE av.attendance_status = 'PresentNonCollaborative'
  AND av.actual_date BETWEEN ? AND ?
ORDER BY av.actual_date DESC;
```

---

## 📐 Dimensioni Stimate

**Assunzioni:**
- 500 pazienti
- Media 2 progetti per paziente (1 attivo + 1 storico)
- 4 appuntamenti canonici + 2 extra per progetto
- 80% appuntamenti completati

**Stime dimensioni:**
- `patients`: ~500 record × 2KB = 1MB
- `therapeutic_projects`: ~1,000 record × 1KB = 1MB
- `scheduled_visits`: ~6,000 record × 0.5KB = 3MB
- `actual_visits`: ~4,800 record × 1.5KB = 7.2MB
- `actual_visit_operators`: ~9,600 record × 0.3KB = 2.9MB

**Totale stimato:** ~15-20MB (escluse note cliniche lunghe)

---

## 🔄 Migrazioni e Versioning

Vedi `docs/DEVELOPMENT.md` per processo completo migrations.

**Convenzioni naming migrations:**
```
YYYYMMDDHHMMSS_DescrizioneMigrazione.cs
```

Esempio:
```
20260202140000_AddVisitTypesAndEnums.cs
```

---

## 📚 Riferimenti

- [USER-WORKFLOW.md](USER-WORKFLOW.md) - Source of truth per enumerazioni e vincoli business
- [PROGETTO_PTRP_SYNC.md](PROGETTO_PTRP_SYNC.md) - Protocollo sincronizzazione
- [SECURITY.md](SECURITY.md) - Crittografia e sicurezza
- [SEED.md](SEED.md) - Data seeding

---

**Versione**: 2.0  
**Ultimo aggiornamento**: 02 Febbraio 2026  
**Stato**: Allineato con USER-WORKFLOW.md (source of truth)
