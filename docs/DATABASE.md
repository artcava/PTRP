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

**Relazioni Fondamentali**:
1. **Paziente ← 1:N → Progetto**: Un paziente può avere più progetti nel tempo (sequenziali o paralleli)
2. **Progetto ← N:N → Operatore**: Un progetto può coinvolgere più operatori; un operatore può lavorare su più progetti
3. **Progetto ← 1:N → Visita Programmata**: Un progetto ha fino a 4 visite canoniche (apertura, intermedia, finale, dimissioni)
4. **Visita Programmata ← 1:N → Visita Effettiva**: Una visita programmata può avere più registrazioni (es. rinvii riprogrammati)
5. **Visita Effettiva ← N:1 → Operatore**: Traccia quale operatore ha registrato la visita

### ER Diagram ASCII

```
┌────────────────────────────────────────────────────────────────────────────────┐
│                          PTRP DATABASE MODEL                                   │
│                   (Offline-First, Many-to-Many Operators)                      │
└────────────────────────────────────────────────────────────────────────────────┘

┌──────────────────────────┐
│     OPERATORS            │  (Educatori, Coordinatori, Supervisori)
│  (id, first_name, ...) │
└──────────────┬───────────┘
               │
               │ N:N (relazione molti a molti)
               │ Ogni operatore può lavorare su più progetti
               │ Ogni progetto può coinvolgere più operatori
               │
        ┌──────▼─────────────────┐
        │  PROJECT_OPERATORS     │  (Tabella di giunzione)
        │  (Join Table)          │
        │ - project_id (FK)      │
        │ - operator_id (FK)     │
        │ - role_in_project      │  (Primary | Assistant | Supervisor)
        │ - assignment_date      │
        │ - end_date             │
        └──────┬──────────────────┘
               │
               │ 1:N (un progetto, molti operatori)
               │
        ┌──────▼────────────────────────────────────────────┐
        │    THERAPEUTIC_PROJECTS (PTRP)                     │
        │  - id                                              │
        │  - patient_id (FK 1:N) ◄─────┐                     │
        │  - assignment_date             │ 1:N (un paziente  │
        │  - status (Active/Closed...)   │ molti progetti)   │
        │  - objectives, details         │                   │
        └──────┬──────────────────────────┼──────────────────┘
               │ 1:N (un progetto, 4 visite canoniche)       ┌──────────────────────┐
               │                                              │  PATIENTS            │
               ▼                                              │ (id, first_name,..│
        ┌────────────────────┐                               │  status, notes)      │
        │ SCHEDULED_VISITS   │                               └──────────────────────┘
        │ - id               │
        │ - project_id (FK)  │
        │ - phase            │  (InitialOpening,
        │ - scheduled_date   │   IntermediateVerification,
        │ - status           │   FinalVerification,
        │ - notes            │   Discharge)
        └────────┬───────────┘
                 │ 1:N (una visita programmata,
                 │     multiple registrazioni)
                 │
                 ▼
        ┌────────────────────────┐
        │ ACTUAL_VISITS          │
        │ - id                   │
        │ - scheduled_visit_id   │
        │ - actual_date          │
        │ - source               │  (EducatorImport |
        │ - registered_by_operator_id (FK)  CoordinatorDirect)
        │ - clinical_notes       │
        │ - registration_date    │
        └────────────────────────┘

┌────────────────────────────────────────┐
│ SYNC_METADATA                          │
│ (Standalone - non ha FK esterne)      │
└────────────────────────────────────────┘
```

---

## 📊 Schema Dettagliato Tabelle

### 1. **OPERATORS** (Educatori, Coordinatori, Supervisori)

**Descrizione**: Anagrafica operatori autorizzati al sistema. Discriminazione di ruolo:
- `Educator` – Educatore base (registra visite personali)
- `Coordinator` – Coordinatore/Master (gestisce anagrafiche, autorizzazioni, verifiche)
- `Supervisor` – Supervisore (lettura + rapporti)

```sql
CREATE TABLE operators (
    id                      TEXT PRIMARY KEY,              -- GUID
    first_name              TEXT NOT NULL,
    last_name               TEXT NOT NULL,
    role                    TEXT NOT NULL,                 -- Educator | Coordinator | Supervisor
    is_active               INTEGER NOT NULL DEFAULT 1,   -- Boolean (0/1)
    email                   TEXT,                          -- Opzionale
    phone                   TEXT,                          -- Opzionale
    last_sync_timestamp     DATETIME,                      -- Ultima sincronizzazione
    created_at              DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at              DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by              TEXT,                          -- GUID dell'operatore che l'ha creato
    updated_by              TEXT,                          -- GUID dell'operatore che lo ha modificato
    version                 INTEGER DEFAULT 1,            -- Per conflict resolution in sync
    UNIQUE (first_name, last_name)
);

CREATE INDEX idx_operators_role ON operators(role);
CREATE INDEX idx_operators_is_active ON operators(is_active);
CREATE INDEX idx_operators_updated_at ON operators(updated_at);
```

**Relazioni**:
- N:N con `therapeutic_projects` tramite `project_operators` (tabella di giunzione)
- FK da `actual_visits` (RegisteredByOperatorId)

---

### 2. **PATIENTS** (Pazienti)

**Descrizione**: Anagrafica pazienti inseriti in programmi terapeutici.

**Enum PatientStatus**:
- `Active` – Paziente attivo in programma
- `Suspended` – Sospeso temporaneamente
- `Discharged` – Dimesso dal programma
- `Deceased` – Deceduto

```sql
CREATE TABLE patients (
    id                      TEXT PRIMARY KEY,              -- GUID
    first_name              TEXT NOT NULL,
    last_name               TEXT NOT NULL,
    date_of_birth           DATE,                          -- Opzionale
    status                  TEXT NOT NULL DEFAULT 'Active', -- Active | Suspended | Discharged | Deceased
    clinical_notes          TEXT,                          -- Note cliniche generali
    contact_email           TEXT,                          -- Contatto paziente/referente
    contact_phone           TEXT,                          -- Contatto paziente/referente
    internal_id             TEXT,                          -- ID esterno (da registro locale)
    created_at              DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at              DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by              TEXT,                          -- GUID operatore creatore
    updated_by              TEXT,                          -- GUID operatore ultimo edit
    version                 INTEGER DEFAULT 1,            -- Per conflict resolution
    UNIQUE (first_name, last_name, date_of_birth)
);

CREATE INDEX idx_patients_status ON patients(status);
CREATE INDEX idx_patients_updated_at ON patients(updated_at);
CREATE INDEX idx_patients_internal_id ON patients(internal_id);
```

**Relazioni**:
- 1:N con `therapeutic_projects` (PatientId) – Un paziente, molti progetti nel tempo

---

### 3. **THERAPEUTIC_PROJECTS** (Progetti Terapeutici PTRP)

**Descrizione**: Progetto terapeutico personalizzato assegnato a paziente. **NON contiene direttamente FK a operatore** – gli operatori sono collegati tramite la tabella di giunzione `project_operators`.

**Enum ProjectStatus**:
- `Active` – Progetto attivo
- `Suspended` – Sospeso
- `Closed` – Chiuso/Completato
- `Archived` – Archiviato

```sql
CREATE TABLE therapeutic_projects (
    id                      TEXT PRIMARY KEY,              -- GUID
    patient_id              TEXT NOT NULL,                 -- FK → patients (1:N)
    assignment_date         DATE NOT NULL,                 -- Data inizio primo progetto per paziente
    status                  TEXT NOT NULL DEFAULT 'Active', -- Active | Suspended | Closed | Archived
    pt_details              TEXT,                          -- Descrizione PTRP
    objectives              TEXT,                          -- Obiettivi terapeutici
    expected_duration_months INTEGER,                      -- Durata prevista in mesi
    diagnosis_notes         TEXT,                          -- Note diagnostiche
    created_at              DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at              DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by              TEXT,                          -- GUID operatore creatore
    updated_by              TEXT,                          -- GUID operatore ultimo edit
    version                 INTEGER DEFAULT 1,            -- Per conflict resolution
    
    FOREIGN KEY (patient_id) REFERENCES patients(id) ON DELETE CASCADE
);

CREATE INDEX idx_therapeutic_projects_patient_id ON therapeutic_projects(patient_id);
CREATE INDEX idx_therapeutic_projects_status ON therapeutic_projects(status);
CREATE INDEX idx_therapeutic_projects_assignment_date ON therapeutic_projects(assignment_date);
CREATE INDEX idx_therapeutic_projects_updated_at ON therapeutic_projects(updated_at);
```

**Relazioni**:
- 1:N con `patients` (PatientId) – Un paziente, molti progetti nel tempo
- N:N con `operators` tramite `project_operators` – Un progetto, molti operatori
- 1:N con `scheduled_visits`

---

### 3b. **PROJECT_OPERATORS** (Tabella di Giunzione - Many-to-Many)

**Descrizione**: Relazione molti-a-molti tra Progetti e Operatori. Traccia:
- Quali operatori sono assegnati a quale progetto
- Il ruolo dell'operatore nel progetto (primario, assistente, supervisore)
- Le date di assegnazione/fine incarico

**Enum OperatorRole In Project**:
- `Primary` – Educatore primario (responsabile principale)
- `Assistant` – Assistente (supporto)
- `Supervisor` – Supervisore (monitoraggio)

```sql
CREATE TABLE project_operators (
    id                      TEXT PRIMARY KEY,              -- GUID
    project_id              TEXT NOT NULL,                 -- FK → therapeutic_projects
    operator_id             TEXT NOT NULL,                 -- FK → operators
    role_in_project         TEXT NOT NULL,                 -- Primary | Assistant | Supervisor
    assignment_date         DATE NOT NULL,                 -- Data inizio incarico
    end_date                DATE,                          -- Data fine incarico (NULL = ancora attivo)
    notes                   TEXT,                          -- Note specifiche sul ruolo
    created_at              DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at              DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by              TEXT,
    updated_by              TEXT,
    version                 INTEGER DEFAULT 1,
    
    FOREIGN KEY (project_id) REFERENCES therapeutic_projects(id) ON DELETE CASCADE,
    FOREIGN KEY (operator_id) REFERENCES operators(id) ON DELETE RESTRICT,
    
    -- Vincolo: Un operatore non può avere due ruoli attivi contemporaneamente nello stesso progetto
    UNIQUE(project_id, operator_id, assignment_date)
);

CREATE INDEX idx_project_operators_project_id ON project_operators(project_id);
CREATE INDEX idx_project_operators_operator_id ON project_operators(operator_id);
CREATE INDEX idx_project_operators_role ON project_operators(role_in_project);
CREATE INDEX idx_project_operators_active ON project_operators(end_date) 
    WHERE end_date IS NULL;  -- Indice solo per assegnazioni attive
```

**Relazioni**:
- FK a `therapeutic_projects` (N:N side)
- FK a `operators` (N:N side)

**Utilità della tabella**:
```sql
-- Esempio: Trovare tutti gli operatori attivi su un progetto
SELECT o.first_name, o.last_name, po.role_in_project, po.assignment_date
FROM project_operators po
JOIN operators o ON po.operator_id = o.id
WHERE po.project_id = ? AND po.end_date IS NULL;

-- Esempio: Trovare tutti i progetti su cui lavora un operatore
SELECT p.first_name, p.last_name, tp.pt_details, po.role_in_project
FROM project_operators po
JOIN therapeutic_projects tp ON po.project_id = tp.id
JOIN patients p ON tp.patient_id = p.id
WHERE po.operator_id = ? AND po.end_date IS NULL;
```

---

### 4. **SCHEDULED_VISITS** (Visite Programmate)

**Descrizione**: Visite terapeutiche programmate per un progetto. Scandite in 4 fasi canoniche.

**Enum VisitPhase**:
- `InitialOpening` – Prima apertura
- `IntermediateVerification` – Verifica intermedia (~6 mesi)
- `FinalVerification` – Verifica finale (~12 mesi)
- `Discharge` – Dimissioni (~13 mesi)

**Enum VisitStatus**:
- `Scheduled` – Programmata
- `Completed` – Completata
- `Suspended` – Sospesa
- `Missed` – Mancata

```sql
CREATE TABLE scheduled_visits (
    id                      TEXT PRIMARY KEY,              -- GUID
    project_id              TEXT NOT NULL,                 -- FK → therapeutic_projects
    phase                   TEXT NOT NULL,                 -- InitialOpening | IntermediateVerification | FinalVerification | Discharge
    scheduled_date          DATE NOT NULL,                 -- Data programmata
    status                  TEXT NOT NULL DEFAULT 'Scheduled', -- Scheduled | Completed | Suspended | Missed
    notes                   TEXT,                          -- Note sulla programmazione
    room_number             TEXT,                          -- Aula/stanza programmata (opzionale)
    expected_duration_minutes INTEGER,                     -- Durata prevista in minuti
    created_at              DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at              DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by              TEXT,                          -- GUID operatore creatore
    updated_by              TEXT,                          -- GUID operatore ultimo edit
    version                 INTEGER DEFAULT 1,            -- Per conflict resolution
    
    FOREIGN KEY (project_id) REFERENCES therapeutic_projects(id) ON DELETE CASCADE
);

CREATE INDEX idx_scheduled_visits_project_id ON scheduled_visits(project_id);
CREATE INDEX idx_scheduled_visits_phase ON scheduled_visits(phase);
CREATE INDEX idx_scheduled_visits_status ON scheduled_visits(status);
CREATE INDEX idx_scheduled_visits_scheduled_date ON scheduled_visits(scheduled_date);
CREATE INDEX idx_scheduled_visits_updated_at ON scheduled_visits(updated_at);
CREATE UNIQUE INDEX idx_scheduled_visits_unique_phase_per_project ON scheduled_visits(project_id, phase)
    WHERE status != 'Missed';
```

**Relazioni**:
- FK a `therapeutic_projects` (1:N – un progetto, 4 visite canoniche)
- FK da `actual_visits` (1:N)

---

### 5. **ACTUAL_VISITS** (Visite Effettive / Registrate)

**Descrizione**: Registrazione reale di una visita effettuata. Collegata a visita programmata, con discriminazione di origine (`VisitSource`).

**Enum VisitSource**:
- `EducatorImport` – Registrata da app Educatore (sync da campo)
- `CoordinatorDirect` – Inserimento diretto dal Coordinatore (verifiche d'ufficio, emergenze)

```sql
CREATE TABLE actual_visits (
    id                      TEXT PRIMARY KEY,              -- GUID
    scheduled_visit_id      TEXT NOT NULL,                 -- FK → scheduled_visits
    actual_date             DATE NOT NULL,                 -- Data effettiva esecuzione
    actual_start_time       TIME,                          -- Ora inizio (opzionale)
    actual_end_time         TIME,                          -- Ora fine (opzionale)
    source                  TEXT NOT NULL DEFAULT 'CoordinatorDirect', -- EducatorImport | CoordinatorDirect
    registered_by           TEXT NOT NULL,                 -- Nome/descrizione di chi ha registrato (audit)
    registered_by_operator_id TEXT,                        -- FK → operators (l'operatore che ha registrato)
    registration_date       DATETIME NOT NULL,             -- Timestamp registrazione
    clinical_notes          TEXT,                          -- Note cliniche post-visita
    outcomes                TEXT,                          -- Risultati/outcome della visita
    attendance_status       TEXT,                          -- Presente | Assente | Rinviato
    signature_hash          TEXT,                          -- Hash firma digitale per integrità (opzionale)
    created_at              DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at              DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by              TEXT,                          -- GUID operatore creatore
    updated_by              TEXT,                          -- GUID operatore ultimo edit
    version                 INTEGER DEFAULT 1,            -- Per conflict resolution
    
    FOREIGN KEY (scheduled_visit_id) REFERENCES scheduled_visits(id) ON DELETE CASCADE,
    FOREIGN KEY (registered_by_operator_id) REFERENCES operators(id) ON DELETE SET NULL
);

CREATE INDEX idx_actual_visits_scheduled_visit_id ON actual_visits(scheduled_visit_id);
CREATE INDEX idx_actual_visits_source ON actual_visits(source);
CREATE INDEX idx_actual_visits_actual_date ON actual_visits(actual_date);
CREATE INDEX idx_actual_visits_registration_date ON actual_visits(registration_date);
CREATE INDEX idx_actual_visits_registered_by_operator_id ON actual_visits(registered_by_operator_id);
CREATE INDEX idx_actual_visits_updated_at ON actual_visits(updated_at);
CREATE UNIQUE INDEX idx_actual_visits_unique_per_scheduled ON actual_visits(scheduled_visit_id, actual_date);
```

**Relazioni**:
- FK a `scheduled_visits` (1:1 o 1:N se più registrazioni per stessa visita)
- FK a `operators` (N:1 – tante visite registrate, un operatore che le registra)

---

### 6. **SYNC_METADATA** (Metadata per Sincronizzazione Offline-First)

**Descrizione**: Traccia dello stato di sincronizzazione tra nodi. Essenziale per l'architettura offline-first.

```sql
CREATE TABLE sync_metadata (
    id                      TEXT PRIMARY KEY,              -- GUID
    node_id                 TEXT NOT NULL UNIQUE,         -- Identificativo nodo (Coordinatore o Educatore)
    node_role               TEXT NOT NULL,                 -- Coordinator | Educator | Supervisor
    last_sync_timestamp     DATETIME,                      -- Timestamp ultima sincronizzazione riuscita
    last_sync_direction     TEXT,                          -- Send | Receive | Bidirectional
    last_conflict_count     INTEGER DEFAULT 0,             -- Conteggio conflitti risolti in ultima sync
    database_version        TEXT,                          -- Versione schema database
    data_seeding_timestamp  DATETIME,                      -- Quando sono stati seedati i dati
    is_master               INTEGER DEFAULT 0,             -- Boolean: è master (Coordinator=1, Educator=0)
    created_at              DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at              DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_sync_metadata_node_role ON sync_metadata(node_role);
```

---

## 🔐 Crittografia Database

### AES-256 Encryption

Il database SQLite è crittografato con **AES-256** a livello di file:

```csharp
// In PtrpDbContext.OnConfiguring()
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    var encryptionKey = DeriveEncryptionKey("password");  // PBKDF2 derivation
    var connectionString = 
        $"Data Source={DbPath};" +
        $"Password={encryptionKey};" +  // SQLCipher via EF provider
        $"Encrypt=AEAD;";
    
    optionsBuilder.UseSqlite(connectionString);
}
```

**Implicazioni**:
- Senza password corretta, il file `.db` è illeggibile
- Password derivata da PBKDF2 (salted key derivation)
- **Ogni installazione ha chiave diversa** (locale security)

### HMAC per Integrità Pacchetti Sync

Pacchetti scambiati tra nodi sono firmati HMAC-SHA256:

```csharp
public record SyncPacket
{
    public Guid Id { get; init; }
    public DateTime CreatedAt { get; init; }
    public string PayloadJson { get; init; }      // Dati serializzati
    public string HmacSignature { get; init; }    // HMAC-SHA256(PayloadJson)
    public Guid SourceNodeId { get; init; }       // Nodo mittente
}
```

---

## 📋 Vincoli di Integrità Referenziale

| Relazione | Tipo | ON DELETE | Motivazione |
|-----------|------|-----------|-------------|
| `therapeutic_projects → patients` | 1:N | CASCADE | Se paziente cancellato, cancella progetti |
| `project_operators → therapeutic_projects` | N:1 | CASCADE | Se progetto cancellato, cancella assegnazioni operatori |
| `project_operators → operators` | N:1 | RESTRICT | Non permette cancellazione operatore se ancora assegnato a progetti |
| `scheduled_visits → therapeutic_projects` | 1:N | CASCADE | Se progetto cancellato, cancella visite programmate |
| `actual_visits → scheduled_visits` | 1:N | CASCADE | Se visita programmata cancellata, cancella registrazioni |
| `actual_visits → operators` | N:1 | SET NULL | Se operatore cancellato, azzera il campo registered_by_operator_id |
| `sync_metadata` | Standalone | – | Non ha FK esterne |

---

## 📊 Indici per Performance

### Indici Primari
- Tutti gli `id` sono PRIMARY KEY (GUID)
- Ricerca principale sempre per ID entità

### Indici Secondari

**Query frequenti in UI**:
1. **Pazienti attivi**: `idx_patients_status`
2. **Progetti recenti**: `idx_therapeutic_projects_updated_at`
3. **Operatori attivi su progetto**: `idx_project_operators_project_id`, `idx_project_operators_active`
4. **Progetti di un operatore**: `idx_project_operators_operator_id`
5. **Visite programmate per data**: `idx_scheduled_visits_scheduled_date`
6. **Visite registrate per operatore**: `idx_actual_visits_registered_by_operator_id`
7. **Sync delta queries**: `idx_*_updated_at` (su tutte le tabelle)

### Indici Compositi
- `idx_scheduled_visits_unique_phase_per_project`: Garantisce una sola visita per fase/progetto
- `idx_actual_visits_unique_per_scheduled`: Evita duplicati registrazioni per stessa visita
- `idx_project_operators_active`: Indice parziale solo per assegnazioni attive (end_date IS NULL)

---

## 🔄 Audit Trail e Versioning

Ogni entità principale ha:

```sql
created_at      DATETIME,  -- Timestamp creazione (immutabile)
updated_at      DATETIME,  -- Timestamp ultimo aggiornamento
created_by      TEXT,      -- GUID operatore creatore
updated_by      TEXT,      -- GUID operatore ultimo editor
version         INTEGER,   -- Numero versione per conflict resolution
```

**Utilizzo**:
- **Auditabilità completa**: Traccia chi ha fatto cosa e quando
- **Conflict Resolution**: In sync, compara `version` e `updated_at` per determinare vincitore
- **Soft Delete** (opzionale): Aggiungi colonna `deleted_at` per logical delete senza eliminare record

---

## 📈 Query Comuni

### 1. Pazienti Attivi con Progetti Aperti e Operatori Assegnati
```sql
SELECT 
    p.id as patient_id,
    p.first_name, p.last_name, 
    tp.id as project_id,
    tp.pt_details,
    o.first_name as operator_first_name,
    o.last_name as operator_last_name,
    po.role_in_project
FROM patients p
JOIN therapeutic_projects tp ON p.id = tp.patient_id
JOIN project_operators po ON tp.id = po.project_id
JOIN operators o ON po.operator_id = o.id
WHERE p.status = 'Active' 
  AND tp.status = 'Active'
  AND po.end_date IS NULL  -- Solo assegnazioni attive
ORDER BY p.last_name, p.first_name, po.role_in_project;
```

### 2. Visite Non Ancora Effettuate (Scadute)
```sql
SELECT 
    sv.id,
    sv.phase,
    sv.scheduled_date,
    p.first_name, p.last_name,
    tp.id as project_id,
    GROUP_CONCAT(o.first_name || ' ' || o.last_name, ', ') as assigned_operators
FROM scheduled_visits sv
JOIN therapeutic_projects tp ON sv.project_id = tp.id
JOIN patients p ON tp.patient_id = p.id
JOIN project_operators po ON tp.id = po.project_id
JOIN operators o ON po.operator_id = o.id
LEFT JOIN actual_visits av ON sv.id = av.scheduled_visit_id
WHERE sv.scheduled_date < DATE('now')
  AND sv.status = 'Scheduled'
  AND av.id IS NULL
  AND po.end_date IS NULL
GROUP BY sv.id
ORDER BY sv.scheduled_date ASC;
```

### 3. Visite Registrate per Operatore (Current Month)
```sql
SELECT 
    o.first_name, o.last_name,
    COUNT(av.id) as visit_count,
    strftime('%Y-%m', av.registration_date) as month
FROM actual_visits av
JOIN operators o ON av.registered_by_operator_id = o.id
WHERE strftime('%Y-%m', av.registration_date) = strftime('%Y-%m', 'now')
GROUP BY av.registered_by_operator_id, month
ORDER BY visit_count DESC;
```

### 4. Progetti di un Operatore (Attivi e Storici)
```sql
SELECT 
    p.first_name as patient_first_name,
    p.last_name as patient_last_name,
    tp.pt_details,
    po.role_in_project,
    po.assignment_date,
    po.end_date,
    tp.status
FROM project_operators po
JOIN therapeutic_projects tp ON po.project_id = tp.id
JOIN patients p ON tp.patient_id = p.id
WHERE po.operator_id = ?
ORDER BY po.assignment_date DESC;
```

### 5. Delta Sync (Modifiche dal Timestamp)
```sql
SELECT 'Patient' as entity_type, id, updated_at FROM patients WHERE updated_at > ?
UNION ALL
SELECT 'TherapeuticProject', id, updated_at FROM therapeutic_projects WHERE updated_at > ?
UNION ALL
SELECT 'ProjectOperator', id, updated_at FROM project_operators WHERE updated_at > ?
UNION ALL
SELECT 'ScheduledVisit', id, updated_at FROM scheduled_visits WHERE updated_at > ?
UNION ALL
SELECT 'ActualVisit', id, updated_at FROM actual_visits WHERE updated_at > ?
ORDER BY updated_at DESC;
```

---

## 🚀 Migrazioni EF Core

Le tabelle sono create/migrate automaticamente da EF Core. Sequenza:

1. **Inizializzazione**: `dotnet ef database update --project src/PTRP.Services`
2. **Schema creato**: EF Core genera DDL da DbContext
3. **Seeding**: `DbContextSeeder.SeedAsync()` popola dati iniziali
4. **Crittografia applicata**: Database file criptato con AES-256

Vedi [SEED.md](../SEED.md) per dettagli sul seeding automatico.

---

## 📊 Dimensioni Stimate Database

Con dataset seed standard (~100 pazienti, ~100 progetti, ~150 assegnazioni operatore, ~400 visite):

| Elemento | Stima | Note |
|----------|-------|-------|
| File database non criptato | ~5-10 MB | Dipende da lunghezza notes cliniche |
| File database criptato | ~5-10 MB | AES-256 non incrementa dimensioni |
| RAM in memoria (EF DbContext caricato) | ~50-100 MB | Cache EF + entities in memory |
| Pacchetto sync (ZIP + HMAC) | ~1-5 MB | Compressione lossless su JSON |

**SQLite non richiede server separato** → Deployment semplicissimo

---

## 🔒 Considerazioni di Sicurezza

### Best Practices Implementate
1. **Crittografia at-rest**: AES-256 sul file database
2. **Crittografia in-transit**: HMAC-SHA256 su pacchetti sync
3. **No chiavi hardcoded**: Key derivation da password locale (PBKDF2)
4. **FK constraints**: Integrità referenziale garantita a DB level
5. **Audit trail**: Traccia completa operazioni (CreatedBy, UpdatedBy, timestamps)
6. **Isolation per operatore**: Tabella `project_operators` permette audit fine-grained di chi fa cosa

### Non Implementato (Roadmap)
- Encryption field-level (sensibile solo per dati molto critici)
- Row-level security (ogni utente vede solo suoi dati)
- Token-based authentication (architettura attuale offline-first)

---

## 📚 Riferimenti

- [SEED.md](../SEED.md) – Strategia data seeding, DbContextSeeder
- [ARCHITECTURE.md](ARCHITECTURE.md) – Pattern MVVM, offline-first architettura
- [SYNC-PROTOCOL.md](SYNC-PROTOCOL.md) – Algoritmo sincronizzazione, conflict resolution
- [SECURITY.md](SECURITY.md) – Crittografia dettagli implementativi
- [EF Core Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [SQLite Encryption](https://www.sqlite.org/cli.html) – SQLCipher documentation

---

**Last Updated**: January 28, 2026
**Database Version**: PTRP-SQLite v1.1 (Many-to-Many Operators)
**EF Core Version**: 10+
