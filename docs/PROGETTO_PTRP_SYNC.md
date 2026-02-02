# Documento di Analisi Tecnica: Sistema di Gestione PTRP
## Progetto: PTRP-Sync (Architettura Disconnessa)

---

## 1. Visione Architetturale: Paradigma Offline-First

Il sistema è progettato come un'applicazione desktop distribuita per .NET 10 che opera in assenza di un database centrale. La sincronizzazione dei dati avviene tramite lo scambio asincrono di pacchetti crittografati tra il Coordinatore e gli Educatori.

### Considerazioni critiche per l'Architetto Software

- **Integrità e Conflitti**  
  In un ambiente disconnesso, il rischio di "Data Drift" (deriva dei dati) è sistemico. È imperativo implementare una logica di **Conflict Resolution** basata su timestamp e gerarchia di permessi (il Coordinatore ha priorità assoluta sulle anagrafiche).

- **Idempotenza**  
  Ogni processo di importazione deve essere progettato per essere eseguito n-volte senza alterare lo stato del database locale (**UPSERT** basato su GUID).

- **Master-Slave Logic**  
  L'applicativo del Coordinatore funge da "Master" per le **anagrafiche** e gli **stati dei PTRP**. L'applicativo dell'Educatore è "Master" solo per le **visite effettive** da lui registrate fino al momento del merge.

---

## 2. Formati Pacchetti di Sincronizzazione

### 2.1 Struttura Generale Pacchetto .ptrp

Ogni file `.ptrp` è un pacchetto JSON crittografato (AES-256) e firmato (HMAC-SHA256) con la seguente struttura base:

```json
{
  "protocol_version": "1.0",
  "package_type": "admin_config" | "appointments_export" | "visits_export",
  "creation_timestamp": "2026-02-02T14:30:00Z",
  "signature": "<HMAC-SHA256>",
  "payload": {
    // Contenuto specifico per tipo pacchetto
  }
}
```

### 2.2 Pacchetto Configurazione Amministratore (admin.ptrp)

**Scopo:** Configurazione iniziale del profilo Coordinatore.

**Naming:** `admin.ptrp` (nome fisso)

**Contenuto Payload:**

```json
{
  "package_type": "admin_config",
  "protocol_version": "1.0",
  "creation_timestamp": "2026-01-15T10:00:00Z",
  "signature": "abc123...",
  "payload": {
    "user_profile": {
      "role": "Coordinator",
      "first_name": "Mario",
      "last_name": "Rossi",
      "is_admin": true
    },
    "initial_data": {
      "educators": [],
      "patients": [],
      "projects": []
    },
    "master_key_hint": "<Optional hint per recupero chiave>"
  }
}
```

**Flusso:**
1. Coordinatore installa app e importa `admin.ptrp`
2. Sistema crea profilo Coordinatore con permessi completi
3. Database inizializzato vuoto
4. Pronto per inserimento dati

---

### 2.3 Pacchetto Appuntamenti per Educatore

**Scopo:** Sincronizzazione appuntamenti Coordinatore → Educatore.

**Naming Convention:** `appointments_{cognome}_{YYYYMMDD}.ptrp`

**Esempio:** `appointments_bianchi_20260401.ptrp`

**Rationale Data Estesa:** Permette all'educatore di verificare se il pacchetto è più recente delle visite già registrate.

**Contenuto Payload:**

```json
{
  "package_type": "appointments_export",
  "protocol_version": "1.0",
  "creation_timestamp": "2026-04-01T09:00:00Z",
  "export_date": "2026-04-01",
  "signature": "def456...",
  "payload": {
    "target_educator": {
      "id": "<GUID educatore>",
      "first_name": "Marco",
      "last_name": "Bianchi",
      "role": "ProfessionalEducator"
    },
    "scheduled_visits": [
      {
        "id": "<GUID>",
        "therapy_project_id": "<GUID>",
        "visit_type": "Intake",
        "scheduled_date": "2026-04-02",
        "scheduled_time": "10:00:00",
        "estimated_duration_minutes": 90,
        "status": "Scheduled",
        "notes": "Prima apertura progetto PTRP 2025-2027"
      }
      // ... altri 11 appuntamenti
    ],
    "patients": [
      {
        "id": "<GUID>",
        "first_name": "Giovanni",
        "last_name": "Rossi",
        "created_at": "2025-01-15T10:00:00Z"
      }
      // ... altri 7 pazienti
    ],
    "therapy_projects": [
      {
        "id": "<GUID>",
        "patient_id": "<GUID>",
        "title": "PTRP 2025-2027",
        "description": "Progetto riabilitativo...",
        "start_date": "2025-01-02",
        "end_date": "2027-01-02",
        "status": "Active"
      }
      // ... altri progetti
    ],
    "associated_educators": [
      {
        "id": "<GUID educatore 1>",
        "first_name": "Luca",
        "last_name": "Verdi"
      },
      {
        "id": "<GUID educatore 2>",
        "first_name": "Sara",
        "last_name": "Neri"
      }
      // ... altri educatori coinvolti nei progetti
    ]
  }
}
```

**Dettaglio "associated_educators" (⚠️ IMPORTANTE):**

Questa sezione contiene l'elenco di **tutti** gli educatori assegnati ai progetti presenti nel pacchetto. È necessaria per consentire all'educatore destinatario di:
- Spuntare i colleghi presenti durante la registrazione delle visite
- Vedere il team completo del progetto

Senza questi dati, l'educatore non potrebbe selezionare correttamente gli operatori nella UI di registrazione visita.

**Comportamento Importazione:**
1. Sistema verifica `target_educator` corrisponda al profilo locale
2. Sistema verifica data pacchetto vs ultime visite registrate
3. **SOSTITUISCE COMPLETAMENTE** tutti gli appuntamenti dell'educatore
4. Importa anche educatori associati (necessari per spunta visite)
5. **PRESERVA** le visite già registrate (non vengono toccate)
6. Aggiorna database locale

---

### 2.4 Pacchetto Visite Registrate (Educatore → Coordinatore)

**Scopo:** Sincronizzazione visite registrate Educatore → Coordinatore.

**Naming Convention:** `visits_{cognome}_{YYYYMMDD}_{HHMMSS}.ptrp`

**Esempio:** `visits_bianchi_20260405_183000.ptrp`

**Contenuto Payload:**

```json
{
  "package_type": "visits_export",
  "protocol_version": "1.0",
  "creation_timestamp": "2026-04-05T18:30:00Z",
  "signature": "ghi789...",
  "payload": {
    "source_educator": {
      "id": "<GUID educatore>",
      "first_name": "Marco",
      "last_name": "Bianchi"
    },
    "last_sync_timestamp": "2026-03-28T18:30:00Z",
    "actual_visits": [
      {
        "id": "<GUID>",
        "scheduled_visit_id": "<GUID appuntamento>",
        "actual_date": "2026-04-02",
        "actual_start_time": "10:00:00",
        "actual_end_time": "11:30:00",
        "source": "EducatorImport",
        "registration_date": "2026-04-02T11:35:00Z",
        "clinical_notes": "Il paziente si è presentato puntuale...",
        "outcomes": "Obiettivo 1: Migliorare autonomia...",
        "attendance_status": "PresentCollaborative"
      }
      // ... altre 4 visite
    ],
    "visit_operators": [
      {
        "actual_visit_id": "<GUID visita>",
        "operator_id": "<GUID educatore Bianchi>",
        "role_in_visit": "Lead",
        "notes": ""
      },
      {
        "actual_visit_id": "<GUID visita>",
        "operator_id": "<GUID educatore Verdi>",
        "role_in_visit": "Assistant",
        "notes": "Co-educatore presente"
      }
      // ... altre 11 relazioni operatori-visite
    ]
  }
}
```

**Comportamento Importazione (Coordinatore):**
1. Verifica firma e integrità pacchetto
2. Per ogni `actual_visit`: UPSERT basato su `id` (GUID)
3. Per ogni `visit_operator`: UPSERT basato su chiave composita (`actual_visit_id`, `operator_id`)
4. Aggiorna `last_sync_timestamp` per educatore
5. Merge con eventuali visite dello stesso educatore già presenti

---

### 2.5 Verifica Integrità Pacchetti

**Controlli Obbligatori:**

1. **Verifica Firma HMAC-SHA256**
   ```csharp
   var computedHmac = ComputeHMAC(payload, masterKey);
   if (computedHmac != package.signature) 
       throw new SecurityException("Firma pacchetto non valida");
   ```

2. **Verifica Versione Protocollo**
   ```csharp
   if (package.protocol_version != "1.0") 
       throw new VersionMismatchException($"Versione {package.protocol_version} non supportata");
   ```

3. **Verifica Destinatario (per appointments_*.ptrp)**
   ```csharp
   if (package.payload.target_educator.id != currentUserProfile.Id)
       throw new UnauthorizedException("Pacchetto non destinato a questo educatore");
   ```

4. **Verifica Data Pacchetto vs Visite Registrate**
   ```csharp
   var lastVisitDate = await GetLastRegisteredVisitDate();
   if (package.export_date < lastVisitDate)
       ShowWarning("Pacchetto obsoleto rilevato");
   ```

---

## 3. Modello Dati e Tracciabilità delle Visite

L'obiettivo del modello dati è garantire **trasparenza**, **auditabilità** e **corretta modellazione clinica** di Progetti, Visite e Operatori, rispettando i vincoli discussi nel dominio.

### 3.1 Relazioni Chiave (allineate a `docs/DATABASE.md`)

A livello concettuale il dominio è organizzato così:

1. **Paziente ← 1:N → Progetto**  
   Un paziente può avere più progetti nel tempo (sequenziali o paralleli). Ogni **progetto terapeutico (PTRP)** appartiene ad un solo paziente.

2. **Progetto ← N:N → Operatore**  
   Un progetto può coinvolgere **più operatori** (educatori, coordinatore, supervisore), e ogni operatore può lavorare su **più progetti**.  
   Questa relazione è modellata da una tabella di giunzione `project_operators` che conserva anche ruoli e periodo di validità.

3. **Progetto ← 1:N → Visita Programmata**  
   Un progetto può avere **molte visite programmate**.  
   - Il **numero** e la **sequenza** delle visite canoniche sono **4 + eventuali extra**.
   - Ogni visita programmata ha un riferimento a una **tipologia** (`VisitType` enum).

4. **Visita Programmata ← 1:1 → Visita Effettiva**  
   Una visita programmata può avere **al massimo una** visita effettiva.  
   - Se esiste un record in `actual_visits` per quella `scheduled_visit`, la visita è stata eseguita.
   - Se non esiste, la visita è ancora aperta/mancata/sospesa.
   - Se, dopo l'esecuzione, serve un nuovo controllo, viene creata **una nuova riga in `scheduled_visits`** secondo le regole di schedulazione.

5. **Visita Effettiva ← N:N → Operatore**  
   Una visita effettiva può essere effettuata da **più operatori** (team), e ogni operatore può partecipare a **molte visite**.  
   - Questa relazione è modellata da una tabella di giunzione `actual_visit_operators`, che registra anche il ruolo dell'operatore nella visita (Lead/Assistant, ecc.).

> Tutti i dettagli strutturali (DDL) sono centralizzati in `docs/DATABASE.md`. Qui ci concentriamo sul **comportamento** e sulle **regole di sincronizzazione**.

---

### 3.2 Enumerazioni di Dominio

#### ProjectStatus

Rappresenta lo **stato del Progetto Terapeutico**.

```csharp
public enum ProjectStatus
{
    Active,      // Progetto attivo in corso
    Suspended,   // Progetto temporaneamente sospeso
    Completed,   // Progetto completato con successo
    Deceased     // Progetto chiuso per decesso paziente
}
```

**Regola Critica:** Un paziente può avere **UN SOLO** progetto con stato `Active` contemporaneamente.

---

#### VisitType

Rappresenta la **tipologia di appuntamento/visita**.

```csharp
public enum VisitType
{
    Intake,           // Prima Apertura (INTAKE) - dopo 3 mesi dall'assegnazione
    Intermediate,     // Verifica Intermedia - dopo 6 mesi dalla Prima Apertura
    Final,           // Verifica Finale - dopo 6 mesi dalla Verifica Intermedia
    Discharge,       // Dimissioni - dopo 1 mese dalla Verifica Finale
    ExtraVisit       // Visite aggiuntive non canoniche
}
```

---

#### AppointmentStatus

Rappresenta lo **stato di un appuntamento programmato**.

```csharp
public enum AppointmentStatus
{
    Scheduled,    // Appuntamento programmato (stato iniziale)
    Completed,    // Appuntamento completato (visita registrata)
    Missed,       // Appuntamento mancato (paziente non si è presentato)
    Rescheduled   // Appuntamento riprogrammato
}
```

---

#### PatientAttendance

Rappresenta lo **stato di presenza del paziente** durante una visita effettiva.

```csharp
public enum PatientAttendance
{
    PresentCollaborative,      // Presente e Collaborativo
    PresentNonCollaborative,   // Presente ma Non Collaborativo
    AbsentJustified,           // Assente Giustificato
    AbsentUnjustified          // Assente Non Giustificato
}
```

---

#### VisitSource

Rappresenta l'**origine della registrazione** di una visita effettiva.

```csharp
public enum VisitSource
{
    EducatorImport,    // Dato originato dall'applicativo dell'Educatore
    CoordinatorDirect  // Inserimento manuale effettuato dal Coordinatore
}
```

Questa distinzione è critica per l'audit trail e la tracciabilità delle visite.

---

### 3.3 Regole di Schedulazione e Visite Canoniche

#### Appuntamenti Canonici Obbligatori

Ogni progetto terapeutico genera **automaticamente** 4 appuntamenti programmati alla creazione:

1. **Prima Apertura (INTAKE)**
   - Tipo: `VisitType.Intake`
   - Tempistica: **+3 mesi** dalla data di inizio progetto (`StartDate`)
   - Durata stimata: 90 minuti
   - Stato iniziale: `AppointmentStatus.Scheduled`

2. **Verifica Intermedia**
   - Tipo: `VisitType.Intermediate`
   - Tempistica: **+6 mesi** dalla data Prima Apertura
   - Durata stimata: 60 minuti
   - Stato iniziale: `AppointmentStatus.Scheduled`

3. **Verifica Finale**
   - Tipo: `VisitType.Final`
   - Tempistica: **+6 mesi** dalla data Verifica Intermedia
   - Durata stimata: 60 minuti
   - Stato iniziale: `AppointmentStatus.Scheduled`

4. **Dimissioni (DISCHARGE)**
   - Tipo: `VisitType.Discharge`
   - Tempistica: **+1 mese** dalla data Verifica Finale
   - Durata stimata: 45 minuti
   - Stato iniziale: `AppointmentStatus.Scheduled`

**Totale temporale progetto:** ~16 mesi (3 + 6 + 6 + 1)

#### Visite Extra (Opzionali)

Oltre ai 4 appuntamenti canonici **obbligatori**, il sistema permette la creazione di:
- **Visite aggiuntive** (`VisitType.ExtraVisit`)
- **Follow-up** su richiesta
- **Urgenze** cliniche

Queste visite extra:
- Non seguono schema temporale predefinito
- Vengono create manualmente dal Coordinatore
- Possono essere pianificate in qualsiasi momento del progetto

**Implementazione:**

```csharp
public async Task<IEnumerable<ScheduledVisit>> GenerateCanonicalAppointments(
    Guid therapyProjectId, 
    DateTime startDate)
{
    var appointments = new List<ScheduledVisit>();
    
    // 1. Prima Apertura (INTAKE) - +3 mesi
    var intakeDate = startDate.AddMonths(3);
    appointments.Add(new ScheduledVisit {
        TherapyProjectId = therapyProjectId,
        Type = VisitType.Intake,
        ScheduledDate = intakeDate,
        EstimatedDurationMinutes = 90,
        Status = AppointmentStatus.Scheduled
    });
    
    // 2. Verifica Intermedia - +6 mesi da INTAKE
    var intermediateDate = intakeDate.AddMonths(6);
    appointments.Add(new ScheduledVisit {
        TherapyProjectId = therapyProjectId,
        Type = VisitType.Intermediate,
        ScheduledDate = intermediateDate,
        EstimatedDurationMinutes = 60,
        Status = AppointmentStatus.Scheduled
    });
    
    // 3. Verifica Finale - +6 mesi da Intermedia
    var finalDate = intermediateDate.AddMonths(6);
    appointments.Add(new ScheduledVisit {
        TherapyProjectId = therapyProjectId,
        Type = VisitType.Final,
        ScheduledDate = finalDate,
        EstimatedDurationMinutes = 60,
        Status = AppointmentStatus.Scheduled
    });
    
    // 4. Dimissioni - +1 mese da Finale
    var dischargeDate = finalDate.AddMonths(1);
    appointments.Add(new ScheduledVisit {
        TherapyProjectId = therapyProjectId,
        Type = VisitType.Discharge,
        ScheduledDate = dischargeDate,
        EstimatedDurationMinutes = 45,
        Status = AppointmentStatus.Scheduled
    });
    
    return appointments;
}
```

---

### 3.4 Visite Effettive e Discriminazione Origine

Il sistema deve discriminare rigorosamente l'origine di ogni record di visita effettiva.

Una `ActualVisit` rappresenta l'esecuzione **di una** `ScheduledVisit` (relazione 1:1):

```csharp
public sealed record ActualVisit
{
    public Guid Id { get; init; } = Guid.NewGuid();

    // 1:1 con ScheduledVisit (OBBLIGATORIO)
    public Guid ScheduledVisitId { get; init; }

    public DateTime ActualDate { get; init; }
    public TimeSpan? ActualStartTime { get; init; }
    public TimeSpan? ActualEndTime { get; init; }

    public VisitSource Source { get; init; } = VisitSource.CoordinatorDirect;
    public DateTime RegistrationDate { get; init; }

    public string ClinicalNotes { get; init; } = string.Empty;
    public string Outcomes { get; init; } = string.Empty;
    public PatientAttendance AttendanceStatus { get; init; } // Enum
}
```

**⚠️ VINCOLO CRITICO:** Una visita può essere creata **SOLO** a partire da un `ScheduledVisit` esistente.

Il **Coordinatore** può inserire visite "fuori sacco" marcate come `CoordinatorDirect`, ad esempio:
- verifiche d'ufficio,
- urgenze,
- colloqui informali ma clinicamente rilevanti.

L'UI deve rendere visibile la distinzione di `VisitSource` (badge/colori diversi) per facilitare l'audit.

---

### 3.5 Partecipazione degli Operatori alle Visite (N:N)

Per modellare correttamente il fatto che **una visita può essere svolta da più operatori**, e che ogni operatore partecipa a molte visite, usiamo una tabella di giunzione concettuale:

```csharp
public sealed record ActualVisitOperator
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public Guid ActualVisitId { get; init; }    // FK → ActualVisit
    public Guid OperatorId { get; init; }       // FK → Operator

    public string RoleInVisit { get; init; } = "Assistant"; // Es. "Lead", "Assistant"
    public string Notes { get; init; } = string.Empty;
}
```

Questa struttura consente di:
- sapere **chi** era coinvolto nella visita (anche team misti educatori+coordinatore),
- costruire report per **operatore** (carico di lavoro, partecipazione clinica),
- evitare di forzare un solo operatore "principale" nella `ActualVisit`.

> A livello di UI, la schermata di dettaglio visita può mostrare una lista di operatori con ruolo clinico, distinta da `CreatedBy`/`UpdatedBy` che indicano solo chi ha registrato o modificato il record.

---

### 3.6 Assegnazione Operatori ai Progetti (N:N nel tempo)

La relazione N:N tra **Progetti** e **Operatori** è anch'essa esplicitata nel modello logico (tabella di giunzione `project_operators`). Concettualmente:

- Un progetto può avere:
  - un **educatore primario**,
  - eventuali **assistenti**,
  - un **supervisore**.

- Le assegnazioni hanno **inizio** e **fine**, in modo da avere uno storico dei cambi di team nel tempo.

```csharp
public sealed record ProjectOperator
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public Guid ProjectId { get; init; }   // FK → TherapeuticProject
    public Guid OperatorId { get; init; }  // FK → Operator

    public string RoleInProject { get; init; } = "Primary"; // Primary | Assistant | Supervisor

    public DateTime AssignmentDate { get; init; }
    public DateTime? EndDate { get; init; }

    public string Notes { get; init; } = string.Empty;
}
```

Questo consente di determinare, per qualsiasi data **T**:
- quale operatore era responsabile di quale paziente/progetto,
- quali operatori sono attualmente attivi su un progetto.

---

## 4. Distribuzione e Lifecycle: Analisi di Velopack

Per la gestione degli aggiornamenti e del deployment su .NET 10, si propone l'utilizzo di **Velopack**.

### Analisi per l'Architetto

- **Vantaggi**  
  Velopack gestisce gli aggiornamenti **delta** (solo i bit modificati), rendendo l'update rapido anche su connessioni instabili. Supporta la pubblicazione **Self-Contained** di .NET 10, eliminando la necessità che l'utente installi manualmente il runtime.

- **Rischi**  
  Poiché il sistema lavora offline, l'auto-update deve prevedere un meccanismo di **Schema Migration**:  
  se la versione `V2` dell'app modifica il DB SQLite, il codice deve gestire la migrazione dei dati locali **senza perdita di informazioni** prima di consentire l'apertura dell'app.

- **Hosting**  
  Sarà necessario un endpoint pubblico (es. **GitHub Releases**) per ospitare i file dei metadati di Velopack.

---

## 5. Gestione Repository e CI/CD

Il repository GitHub dovrà essere configurato per supportare lo sviluppo collaborativo e la qualità del codice.

### Pipeline di Automazione (GitHub Actions)

- **Validation Workflow**  
  Esecuzione di unit test sulla logica di:
  - calcolo delle scadenze delle visite,
  - generazione del piano di visite programmate in base alle regole (`VisitType`, stato progetto, esito visite precedenti).

- **Security Scan**  
  Analisi statica del codice (SAST) per prevenire l'esposizione accidentale di chiavi di crittografia o segreti.

- **Deployment Workflow**  
  Compilazione del pacchetto Velopack e generazione della release su GitHub al superamento di tutti i test su branch `main`.

---

## 6. Sicurezza e Protezione del Dato

Trattandosi di dati sensibili (PTRP e pazienti), la sicurezza è un requisito non funzionale primario.

- **Crittografia**  
  Il database locale SQLite deve essere **criptato** (AES-256) per evitare letture dirette del file.

- **Firma dei pacchetti**  
  Ogni file di interscambio deve contenere un **hash di verifica (HMAC)** per garantire che il file non sia stato manipolato manualmente durante il transito (es. via email o chiavetta USB).

- **Audit e Tracciabilità**  
  Ogni entità clinica critica (Pazienti, Progetti, Visite) deve avere:
  - `CreatedAt`, `UpdatedAt`
  - `CreatedBy`, `UpdatedBy`
  - `Version` per la risoluzione dei conflitti in fase di sync.

Per i dettagli implementativi della crittografia e della struttura delle tabelle, vedere:
- `docs/DATABASE.md`
- `docs/SECURITY.md`

---

## Riferimenti

- [USER-WORKFLOW.md](USER-WORKFLOW.md) - Flussi utente dettagliati (source of truth)
- [ARCHITECTURE.md](ARCHITECTURE.md) - Pattern MVVM e modelli di dominio
- [DATABASE.md](DATABASE.md) - Schema database SQLite completo
- [SECURITY.md](SECURITY.md) - Modello di sicurezza e GDPR
- [DEVELOPMENT.md](DEVELOPMENT.md) - Guida sviluppatori

---

**Documento aggiornato:** 02 Febbraio 2026  
**Versione:** 2.0 (Allineato con USER-WORKFLOW.md)  
**Autore:** Marco Cavallo (@artcava)

---

*Documento tecnico ad uso interno - Progetto PTRP-Sync*