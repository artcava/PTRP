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

## 2. Modello Dati e Tracciabilità delle Visite

L'obiettivo del modello dati è garantire **trasparenza**, **auditabilità** e **corretta modellazione clinica** di Progetti, Visite e Operatori, rispettando i vincoli discussi nel dominio.

### 2.1 Relazioni Chiave (allineate a `docs/DATABASE.md`)

A livello concettuale il dominio è organizzato così:

1. **Paziente ← 1:N → Progetto**  
   Un paziente può avere più progetti nel tempo (sequenziali o paralleli). Ogni **progetto terapeutico (PTRP)** appartiene ad un solo paziente.

2. **Progetto ← N:N → Operatore**  
   Un progetto può coinvolgere **più operatori** (educatori, coordinatore, supervisore), e ogni operatore può lavorare su **più progetti**.  
   Questa relazione è modellata da una tabella di giunzione `project_operators` che conserva anche ruoli e periodo di validità.

3. **Progetto ← 1:N → Visita Programmata**  
   Un progetto può avere **molte visite programmate**.  
   - Il **numero** e la **sequenza** delle visite **non** sono codificati nello schema, ma nelle **regole di schedulazione** a livello applicativo.  
   - Ogni visita programmata ha un riferimento a una **tipologia** (`visit_types`), usata dal motore di schedulazione per applicare le regole (es. INTAKE, INTERMEDIATE, FINAL, DISCHARGE, EXTRA...).

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

### 2.2 Tipologie di Visita e Regole di Schedulazione

Le tipologie di visita non sono hardcoded nel database ma definite in una tabella di lookup:

```csharp
public sealed record VisitType
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;  // es. "INTAKE", "INTERMEDIATE", "FINAL", "DISCHARGE", "EXTRA"
    public string Description { get; init; } = string.Empty;
    public bool IsActive { get; init; } = true;
}
```

La **logica di schedulazione** vive lato codice (es. in un `VisitSchedulingService`) e, dati:
- il tipo di evento (es. apertura progetto, verifica intermedia completata, evento straordinario),
- lo stato corrente del progetto,

produce una serie di nuove `ScheduledVisit` collegate a:
- `ProjectId`
- `VisitTypeId`
- `ScheduledDate`

Lo schema non vincola a **4 visite fisse**: permette anche visite extra, follow-up aggiuntivi, ecc., purché governati dal motore di regole.

---

### 2.3 Visite Effettive e Discriminazione Origine

Il sistema deve discriminare rigorosamente l'origine di ogni record di visita effettiva.

```csharp
public enum VisitSource
{
    EducatorImport,   // Dato originato dall'applicativo dell'Educatore
    CoordinatorDirect // Inserimento manuale effettuato dal Coordinatore
}
```

Una `ActualVisit` rappresenta l'esecuzione **di una** `ScheduledVisit` (relazione 1:1):

```csharp
public sealed record ActualVisit
{
    public Guid Id { get; init; } = Guid.NewGuid();

    // 1:1 con ScheduledVisit
    public Guid ScheduledVisitId { get; init; }

    public DateTime ActualDate { get; init; }
    public TimeSpan? ActualStartTime { get; init; }
    public TimeSpan? ActualEndTime { get; init; }

    public VisitSource Source { get; init; } = VisitSource.CoordinatorDirect;
    public DateTime RegistrationDate { get; init; }

    public string ClinicalNotes { get; init; } = string.Empty;
    public string Outcomes { get; init; } = string.Empty;
    public string AttendanceStatus { get; init; } = string.Empty; // Presente/Assente/Rinviato
}
```

Il **Coordinatore** può inserire visite "fuori sacco" marcate come `CoordinatorDirect`, ad esempio:
- verifiche d'ufficio,
- urgenze,
- colloqui informali ma clinicamente rilevanti.

L'UI deve rendere visibile la distinzione di `VisitSource` (badge/colori diversi) per facilitare l'audit.

---

### 2.4 Partecipazione degli Operatori alle Visite (N:N)

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

### 2.5 Assegnazione Operatori ai Progetti (N:N nel tempo)

La relazione N:N tra **Progetti** e **Operatori** è anch’essa esplicitata nel modello logico (tabella di giunzione `project_operators`). Concettualmente:

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

## 3. Distribuzione e Lifecycle: Analisi di Velopack

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

## 4. Gestione Repository e CI/CD

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

## 5. Sicurezza e Protezione del Dato

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

*Documento tecnico ad uso interno - Progetto PTRP-Sync*