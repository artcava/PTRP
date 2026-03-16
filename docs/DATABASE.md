# DATABASE.md - Schema Logico SQLite PTRP

## 📋 Panoramica

Questo documento descrive lo **schema logico del database SQLite** utilizzato da PTRP per la gestione offline-first di Pazienti, Progetti Terapeutici, Operatori e Visite.

### Caratteristiche Architetturali
- **Database Locale**: SQLite criptato con AES-256
- **Nessun SQL Server Esterno**: Ogni installazione è completamente autonoma
- **Migrazioni EF Core**: Schema versionato e migrabile
- **Idempotenza**: Seeding automatico alla prima esecuzione (vedi [SEED.md](./SEED.md))
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
   Un progetto può coinvolgere più operatori, e ogni operatore può lavorare su più progetti. Ruoli e durata dell’assegnazione sono rappresentati nella tabella di giunzione.

3. **Progetto ← 1:N → Visita Programmata**  
   Un progetto può avere molte visite programmate. **Il numero e la tipologia delle visite sono determinati da regole di business nel codice**, non codificati a livello di schema (niente vincolo “4 visite canoniche” qui). Le visite programmate puntano ad una tipologia predefinita (tabella `visit_types`).

4. **Visita Programmata ← 1:1 → Visita Effettiva**  
   Una visita programmata può avere **al massimo una** visita effettiva. Quando la visita effettiva viene registrata, sancisce l’esecuzione della visita programmata. Se l’utente (o le regole di schedulazione) decide di riprogrammare, verrà creata **una nuova** visita programmata.

5. **Visita Effettiva ← N:N → Operatore**  
   Una visita effettiva può essere svolta da più operatori; ogni operatore partecipa a molte visite. Questa relazione è modellata tramite una tabella di giunzione (`actual_visit_operators`).

### ER Diagram ASCII (alto livello)

> **Nota**: lo schema qui è intenzionalmente compatto; dettagli aggiuntivi su regole di schedulazione e casi d’uso sono descritti in `PROGETTO_PTRP_SYNC.md`.

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

## 📊 Schema Dettagliato Tabelle (estratto)

> Qui riportiamo solo le parti che cambiano rispetto alla versione precedente (nuove tabelle/relazioni). Il resto dello schema rimane invariato.

### 4. **VISIT_TYPES** (Tipologie di Visita)

**Descrizione**: Tabella di lookup con le tipologie di visita. Queste tipologie sono usate dalle **regole di schedulazione** nel codice per generare le `scheduled_visits`.

Esempi:
- `INTAKE` – Prima valutazione / apertura
- `INTERMEDIATE` – Verifica intermedia
- `FINAL` – Verifica finale
- `DISCHARGE` – Dimissioni
- `EXTRA` – Visita aggiuntiva straordinaria

```sql
CREATE TABLE visit_types (
    id              TEXT PRIMARY KEY,            -- GUID o codice
    code            TEXT NOT NULL UNIQUE,        -- Es. INTAKE, INTERMEDIATE, FINAL, ...
    description     TEXT NOT NULL,              -- Descrizione leggibile
    is_active       INTEGER NOT NULL DEFAULT 1, -- Per permettere di "disabilitare" tipi
    created_at      DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at      DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_visit_types_is_active ON visit_types(is_active);
```

---

### 5. **SCHEDULED_VISITS** (Visite Programmate) – modifica

La tabella esiste già; ora **non** assume più che le visite siano 4 canoniche, ma le collega a `visit_types`.

```sql
CREATE TABLE scheduled_visits (
    id                      TEXT PRIMARY KEY,              -- GUID
    project_id              TEXT NOT NULL,                 -- FK → therapeutic_projects
    visit_type_id           TEXT NOT NULL,                 -- FK → visit_types
    scheduled_date          DATE NOT NULL,                 -- Data programmata
    status                  TEXT NOT NULL DEFAULT 'Scheduled', -- Scheduled | Completed | Suspended | Missed
    notes                   TEXT,
    room_number             TEXT,
    expected_duration_minutes INTEGER,
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

> **Nota**: la logica “quando e quante visite generare” (es. INTAKE, poi INTERMEDIATE a 6 mesi, ecc.) vive **nel codice applicativo**, non nello schema. Lo schema espone solo:
> - il collegamento al `project_id`
> - il riferimento alla `visit_type`
> - la data programmata e lo stato.

---

### 6. **ACTUAL_VISITS** (Visite Effettive) – relazione 1:1 con `scheduled_visits`

**Modifica rispetto alla versione precedente**: la relazione con `scheduled_visits` è ora **1:1** (una visita programmata → al massimo una visita effettiva).
In pratica:
- Se esiste una riga in `actual_visits` per una `scheduled_visit_id`, quella visita programmata è stata eseguita
- Se non esiste, la visita programmata è ancora pendente/mancata/sospesa

```sql
CREATE TABLE actual_visits (
    id                      TEXT PRIMARY KEY,              -- GUID
    scheduled_visit_id      TEXT NOT NULL UNIQUE,          -- FK → scheduled_visits, 1:1
    actual_date             DATE NOT NULL,
    actual_start_time       TIME,
    actual_end_time         TIME,
    source                  TEXT NOT NULL DEFAULT 'CoordinatorDirect', -- EducatorImport | CoordinatorDirect
    registration_date       DATETIME NOT NULL,
    clinical_notes          TEXT,
    outcomes                TEXT,
    attendance_status       TEXT,
    signature_hash          TEXT,
    created_at              DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at              DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by              TEXT,
    updated_by              TEXT,
    version                 INTEGER DEFAULT 1,
    
    FOREIGN KEY (scheduled_visit_id) REFERENCES scheduled_visits(id) ON DELETE CASCADE
);

CREATE INDEX idx_actual_visits_scheduled_visit_id ON actual_visits(scheduled_visit_id);
CREATE INDEX idx_actual_visits_actual_date ON actual_visits(actual_date);
CREATE INDEX idx_actual_visits_registration_date ON actual_visits(registration_date);
CREATE INDEX idx_actual_visits_updated_at ON actual_visits(updated_at);
```

---

### 7. **ACTUAL_VISIT_OPERATORS** (N:N tra Visita Effettiva e Operatori)

**Descrizione**: Una visita effettiva può essere effettuata da **più operatori** (team), e ogni operatore partecipa a molte visite. Questa tabella di giunzione cattura:
- quali operatori hanno partecipato a quale visita effettiva
- il ruolo dell’operatore nella visita (es. `Lead`, `Assistant`)

```sql
CREATE TABLE actual_visit_operators (
    id                      TEXT PRIMARY KEY,      -- GUID
    actual_visit_id         TEXT NOT NULL,         -- FK → actual_visits
    operator_id             TEXT NOT NULL,         -- FK → operators
    role_in_visit           TEXT NOT NULL,         -- Es. Lead | Assistant
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

## 📋 Vincoli di Integrità Referenziale (estratto aggiornato)

| Relazione | Tipo | ON DELETE | Note |
|-----------|------|-----------|------|
| `therapeutic_projects → patients` | 1:N | CASCADE | Se paziente cancellato, cancella progetti |
| `project_operators → therapeutic_projects` | N:1 | CASCADE | Se progetto cancellato, cancella assegnazioni operatori |
| `project_operators → operators` | N:1 | RESTRICT | Non permette cancellazione operatore se ancora assegnato a progetti |
| `scheduled_visits → therapeutic_projects` | 1:N | CASCADE | Se progetto cancellato, cancella visite programmate |
| `scheduled_visits → visit_types` | N:1 | RESTRICT | Non si può cancellare una tipologia se usata |
| `actual_visits → scheduled_visits` | 1:1 | CASCADE | Una visita effettiva esiste solo se esiste la programmata |
| `actual_visit_operators → actual_visits` | N:1 | CASCADE | Cancellare una visita effettiva rimuove tutti i suoi operatori |
| `actual_visit_operators → operators` | N:1 | RESTRICT | Non permette cancellazione operatore se referenziato |

Il resto delle sezioni (crittografia, audit trail, query di esempio, dimensioni stimate) rimane invariato rispetto alla versione precedente, ma ora si appoggia a questo modello concettuale corretto.

Per dettagli ulteriori sulle **regole di schedulazione**, il **workflow clinico** e le **regole di business** (es. quando generare nuove visite programmate dopo una effettiva), fare riferimento a:
- `PROGETTO_PTRP_SYNC.md`
- `README.md` (sezione architetturale)
