# ARCHITECTURE.md - Architettura Software PTRP

## 📋 Panoramica

Questo documento descrive l'architettura software di **PTRP (Progetti Terapeutici Riabilitativi Personalizzati)**, applicazione WPF desktop per la gestione offline-first di pazienti, progetti terapeutici e visite.

**Source of Truth per Workflow e Regole di Business:** [USER-WORKFLOW.md](USER-WORKFLOW.md)

---

## 🏗️ Pattern Architetturale: MVVM

L'applicazione segue rigorosamente il pattern **Model-View-ViewModel (MVVM)** con separazione netta delle responsabilità:

### Layer Stack

```
┌─────────────────────────────────────────────────────────┐
│  PTRP.App (WPF Views)                                   │
│  └─ XAML Views + Code-Behind minimale                   │
└────────────────────┬────────────────────────────────────┘
                     │ Data Binding + Commands
┌────────────────────▼────────────────────────────────────┐
│  PTRP.ViewModels (Presentation Logic)                   │
│  └─ Commands, ObservableCollections, UI State           │
└────────────────────┬────────────────────────────────────┘
                     │ Calls business logic
┌────────────────────▼────────────────────────────────────┐
│  PTRP.Services (Business Logic)                         │
│  └─ Orchestrazione, Validazioni, Regole di Dominio      │
└────────────────────┬────────────────────────────────────┘
                     │ CRUD operations
┌────────────────────▼────────────────────────────────────┐
│  PTRP.Data (Repositories)                               │
│  └─ EF Core DbContext, Query, Persistenza               │
└────────────────────┬────────────────────────────────────┘
                     │ Mappa su
┌────────────────────▼────────────────────────────────────┐
│  PTRP.Models (Domain Models)                            │
│  └─ Entità di dominio, Enumerazioni, Vincoli            │
└─────────────────────────────────────────────────────────┘
                     │ Persistite in
┌────────────────────▼────────────────────────────────────┐
│  SQLite Database (Criptato AES-256)                     │
│  └─ File locale .db cifrato                             │
└─────────────────────────────────────────────────────────┘
```

---

## 🎯 Modelli di Dominio (PTRP.Models)

### 1. **PatientModel** (Paziente)

Rappresenta un paziente nel sistema.

**Proprietà:**
- `Guid Id`: Identificatore univoco (autogenerato)
- `string FirstName`: Nome (obbligatorio, max 100 caratteri)
- `string LastName`: Cognome (obbligatorio, max 100 caratteri)
- `DateTime? DateOfBirth`: Data di nascita (opzionale)
- `string? FiscalCode`: Codice fiscale (opzionale, unique)
- `string? PhoneNumber`: Telefono (opzionale)
- `string? Email`: Email (opzionale)
- `string? Address`: Indirizzo (opzionale)
- `string? Notes`: Note aggiuntive (opzionale)
- `DateTime CreatedAt`: Timestamp creazione (default: DateTime.UtcNow)
- `DateTime? UpdatedAt`: Timestamp ultimo aggiornamento
- `string? CreatedBy`: Operatore che ha creato il record
- `string? UpdatedBy`: Operatore che ha aggiornato il record
- `int Version`: Versione record per ottimistic locking (default: 1)

**Relazioni:**
- `ICollection<TherapyProjectModel> TherapyProjects`: Progetti terapeutici associati (1:N)

**ToString:** `"{FirstName} {LastName}"`

---

### 2. **TherapyProjectModel** (Progetto Terapeutico)

Rappresenta un progetto terapeutico associato a un paziente.

**Proprietà:**
- `Guid Id`: Identificatore univoco
- `Guid PatientId`: FK al paziente
- `string Title`: Titolo progetto (obbligatorio)
- `string? Description`: Descrizione dettagliata
- `DateTime StartDate`: Data inizio progetto (obbligatorio)
- `DateTime? PlannedEndDate`: Data fine pianificata (opzionale)
- `DateTime? ActualEndDate`: Data fine effettiva (opzionale)
- `ProjectStatus Status`: Stato progetto (default: `Active`)
- `string? Notes`: Note aggiuntive
- `DateTime CreatedAt`: Timestamp creazione
- `DateTime? UpdatedAt`: Timestamp aggiornamento
- `string? CreatedBy`: Operatore creatore
- `string? UpdatedBy`: Operatore ultimo aggiornamento
- `int Version`: Versione per optimistic locking

**Relazioni:**
- `PatientModel Patient`: Paziente associato (N:1)
- `ICollection<ProfessionalEducatorModel> ProfessionalEducators`: Educatori assegnati (N:N via `ProjectOperatorModel`)
- `ICollection<ScheduledVisitModel> ScheduledVisits`: Visite programmate (1:N)

**Enumerazione `ProjectStatus`:**
```csharp
public enum ProjectStatus
{
    Active,      // Progetto attivo
    Suspended,   // Progetto sospeso temporaneamente
    Completed,   // Progetto concluso regolarmente
    Deceased     // Paziente deceduto
}
```

**Vincolo Critico:** Un paziente può avere **UN SOLO** progetto con `Status = Active` contemporaneamente (validato in `TherapyProjectService`).

**ToString:** `"{Title} (Paziente: {Patient?.FullName})"`

---

### 3. **ProfessionalEducatorModel** (Operatore/Educatore)

Rappresenta un educatore professionale.

**Proprietà:**
- `Guid Id`: Identificatore univoco
- `string FirstName`: Nome (obbligatorio, max 100 caratteri)
- `string LastName`: Cognome (obbligatorio, max 100 caratteri)
- `string Email`: Email (obbligatorio, unique)
- `string? PhoneNumber`: Telefono
- `DateTime? DateOfBirth`: Data di nascita
- `string? Specialization`: Specializzazione (es. "Psicologo", "Educatore Sociale")
- `string? LicenseNumber`: Numero albo professionale
- `DateTime? HireDate`: Data assunzione/collaborazione
- `string Status`: Stato (default: "Active")
- `DateTime CreatedAt`: Timestamp creazione
- `DateTime? UpdatedAt`: Timestamp aggiornamento
- `string? CreatedBy`: Operatore creatore
- `string? UpdatedBy`: Operatore ultimo aggiornamento
- `int Version`: Versione optimistic locking

**Relazioni:**
- `ICollection<TherapyProjectModel> AssignedTherapyProjects`: Progetti assegnati (N:N)
- `ICollection<ActualVisitModel> ConductedVisits`: Visite effettuate (N:N via `VisitOperatorModel`)

**ToString:** `"{FirstName} {LastName} ({Specialization})"`

---

### 4. **VisitTypeModel** (Tipologia Visita)

Definisce le tipologie di visite canoniche e straordinarie.

**Proprietà:**
- `Guid Id`: Identificatore univoco
- `string Code`: Codice tipologia (unique, es. "Intake", "Intermediate", "Final", "Discharge", "ExtraVisit")
- `string Description`: Descrizione leggibile
- `bool IsActive`: Flag per disabilitare tipologie obsolete
- `DateTime CreatedAt`: Timestamp creazione
- `DateTime? UpdatedAt`: Timestamp aggiornamento

**Relazioni:**
- `ICollection<ScheduledVisitModel> ScheduledVisits`: Visite programmate (1:N)

**Valori Predefiniti (Seeding):**
- `Intake` - Prima apertura (valutazione iniziale)
- `Intermediate` - Verifica intermedia (6 mesi)
- `Final` - Verifica finale (12 mesi)
- `Discharge` - Dimissioni (13 mesi)
- `ExtraVisit` - Visita aggiuntiva/straordinaria

**ToString:** `"{Code} - {Description}"`

---

### 5. **ScheduledVisitModel** (Visita Programmata)

Rappresenta una visita programmata/appuntamento.

**Proprietà:**
- `Guid Id`: Identificatore univoco
- `Guid ProjectId`: FK al progetto terapeutico
- `Guid VisitTypeId`: FK alla tipologia visita
- `DateTime ScheduledDate`: Data programmata
- `TimeSpan? ScheduledStartTime`: Ora inizio prevista (opzionale)
- `int? ExpectedDurationMinutes`: Durata attesa in minuti (opzionale)
- `AppointmentStatus Status`: Stato appuntamento (default: `Scheduled`)
- `string? Location`: Luogo visita (es. "Sede", "Domicilio paziente")
- `string? Notes`: Note aggiuntive
- `DateTime CreatedAt`: Timestamp creazione
- `DateTime? UpdatedAt`: Timestamp aggiornamento
- `string? CreatedBy`: Operatore creatore
- `string? UpdatedBy`: Operatore ultimo aggiornamento
- `int Version`: Versione optimistic locking

**Relazioni:**
- `TherapyProjectModel Project`: Progetto associato (N:1)
- `VisitTypeModel VisitType`: Tipologia visita (N:1)
- `ActualVisitModel? ActualVisit`: Visita effettiva (1:1, nullable)

**Enumerazione `AppointmentStatus`:**
```csharp
public enum AppointmentStatus
{
    Scheduled,     // Appuntamento programmato (non ancora effettuato)
    Completed,     // Appuntamento completato (esiste ActualVisit)
    Missed,        // Appuntamento mancato (paziente non si è presentato)
    Rescheduled    // Appuntamento riprogrammato (creato nuovo ScheduledVisit)
}
```

**Vincoli:**
- `ScheduledDate` non può essere nel passato (validato applicativamente)
- Un `ScheduledVisit` può avere **al massimo** una `ActualVisit` (relazione 1:1)

**ToString:** `"{VisitType?.Code} - {ScheduledDate:yyyy-MM-dd}"`

---

### 6. **ActualVisitModel** (Visita Effettiva)

Rappresenta una visita effettivamente svolta.

**Proprietà:**
- `Guid Id`: Identificatore univoco
- `Guid ScheduledVisitId`: FK alla visita programmata (UNIQUE - relazione 1:1)
- `DateTime ActualDate`: Data effettiva visita
- `TimeSpan? ActualStartTime`: Ora inizio effettiva (opzionale)
- `TimeSpan? ActualEndTime`: Ora fine effettiva (opzionale)
- `VisitSource Source`: Origine registrazione (default: `CoordinatorDirect`)
- `DateTime RegistrationDate`: Data/ora registrazione nel sistema
- `string ClinicalNotes`: Note cliniche (obbligatorio)
- `string? Outcomes`: Esiti/risultati visita (opzionale)
- `PatientAttendance AttendanceStatus`: Presenza paziente (obbligatorio)
- `string? SignatureHash`: Hash firma elettronica (opzionale)
- `DateTime CreatedAt`: Timestamp creazione
- `DateTime? UpdatedAt`: Timestamp aggiornamento
- `string? CreatedBy`: Operatore creatore
- `string? UpdatedBy`: Operatore ultimo aggiornamento
- `int Version`: Versione optimistic locking

**Relazioni:**
- `ScheduledVisitModel ScheduledVisit`: Visita programmata corrispondente (1:1)
- `ICollection<VisitOperatorModel> VisitOperators`: Operatori partecipanti (1:N → N:N con `ProfessionalEducatorModel`)

**Enumerazione `VisitSource`:**
```csharp
public enum VisitSource
{
    EducatorImport,      // Registrata da educatore e importata via sync
    CoordinatorDirect    // Registrata direttamente da coordinatore
}
```

**Enumerazione `PatientAttendance`:**
```csharp
public enum PatientAttendance
{
    PresentCollaborative,        // Paziente presente e collaborativo
    PresentNonCollaborative,     // Paziente presente ma non collaborativo
    AbsentJustified,             // Paziente assente con giustificazione
    AbsentUnjustified            // Paziente assente senza giustificazione
}
```

**Vincoli:**
- `ActualDate` non può essere futura (validato applicativamente)
- `ActualEndTime` > `ActualStartTime` (se entrambi specificati)
- `ClinicalNotes` obbligatorio (NOT NULL)
- `ScheduledVisitId` deve essere UNIQUE (una visita programmata → al massimo una visita effettiva)

**ToString:** `"Visita del {ActualDate:yyyy-MM-dd} - {AttendanceStatus}"`

---

### 7. **VisitOperatorModel** (Tabella di Giunzione N:N)

Rappresenta la partecipazione di un operatore a una visita effettiva (relazione N:N tra `ActualVisitModel` e `ProfessionalEducatorModel`).

**Proprietà:**
- `Guid Id`: Identificatore univoco
- `Guid ActualVisitId`: FK alla visita effettiva
- `Guid OperatorId`: FK all'operatore
- `string RoleInVisit`: Ruolo nella visita (es. "Lead", "Assistant", "Observer")
- `string? Notes`: Note aggiuntive su partecipazione
- `DateTime CreatedAt`: Timestamp creazione
- `DateTime? UpdatedAt`: Timestamp aggiornamento
- `string? CreatedBy`: Operatore creatore
- `string? UpdatedBy`: Operatore ultimo aggiornamento
- `int Version`: Versione optimistic locking

**Relazioni:**
- `ActualVisitModel ActualVisit`: Visita effettiva (N:1)
- `ProfessionalEducatorModel Operator`: Operatore partecipante (N:1)

**Vincolo Unique:** `(ActualVisitId, OperatorId)` → uno stesso operatore non può comparire due volte nella stessa visita.

**ToString:** `"{Operator?.FullName} - {RoleInVisit}"`

---

### 8. **ProjectOperatorModel** (Tabella di Giunzione N:N)

Rappresenta l'assegnazione di un operatore a un progetto terapeutico (relazione N:N tra `TherapyProjectModel` e `ProfessionalEducatorModel`).

**Proprietà:**
- `Guid Id`: Identificatore univoco
- `Guid ProjectId`: FK al progetto
- `Guid OperatorId`: FK all'operatore
- `string RoleInProject`: Ruolo nel progetto (es. "Coordinator", "Assistant", "Consultant")
- `DateTime AssignedAt`: Data assegnazione
- `DateTime? RemovedAt`: Data rimozione (nullable, se ancora assegnato)
- `string? Notes`: Note aggiuntive
- `DateTime CreatedAt`: Timestamp creazione
- `DateTime? UpdatedAt`: Timestamp aggiornamento
- `string? CreatedBy`: Operatore creatore
- `string? UpdatedBy`: Operatore ultimo aggiornamento
- `int Version`: Versione optimistic locking

**Relazioni:**
- `TherapyProjectModel Project`: Progetto terapeutico (N:1)
- `ProfessionalEducatorModel Operator`: Operatore assegnato (N:1)

**Vincolo Unique:** `(ProjectId, OperatorId)` per assegnazioni attive (RemovedAt IS NULL).

**ToString:** `"{Operator?.FullName} - {RoleInProject} su {Project?.Title}"`

---

## 🔄 Relazioni tra Modelli (ER Diagram Completo)

```
PatientModel (1) ────────── (N) TherapyProjectModel
                                       |
                                       | (N)
                                       |
                            ProjectOperatorModel (N:N)
                                       |
                                       | (N)
                                       |
                             ProfessionalEducatorModel
                                       |
                                       | (N)
                                       |
                             VisitOperatorModel (N:N)
                                       |
                                       | (N)
                                       |
TherapyProjectModel (1) ── (N) ScheduledVisitModel (1:1) ── ActualVisitModel
                                       |
                                       | (N)
                                       |
                                VisitTypeModel (1)
```

---

## 🧰 Servizi (PTRP.Services)

### PatientService

**Interfaccia:** `IPatientService`

**Metodi:**
- `Task<IReadOnlyList<PatientModel>> GetAllAsync(CancellationToken ct = default)`
- `Task<PatientModel?> GetByIdAsync(Guid id, CancellationToken ct = default)`
- `Task<PatientModel?> GetByIdWithProjectsAsync(Guid id, CancellationToken ct = default)` ✨ **v1.1**
- `Task<IReadOnlyList<PatientModel>> SearchAsync(string? searchTerm = null, ProjectStateFilter? stateFilter = null, CancellationToken ct = default)` ✨ **v1.1**
- `Task CreateAsync(PatientModel patient, CancellationToken ct = default)`
- `Task UpdateAsync(PatientModel patient, CancellationToken ct = default)`
- `Task DeleteAsync(Guid id, CancellationToken ct = default)`

**Validazioni:**
- `FirstName` e `LastName` obbligatori, max 100 caratteri
- `FiscalCode` unico (se specificato)
- `Email` formato valido (se specificata)

**Nuove Funzionalità (issue #73 - v1.1):**
- **Ricerca Avanzata con Filtro Stato Progetto**: `SearchAsync` supporta filtro per stato progetto tramite `ProjectStateFilter` enum (All/Active/Suspended/Completed/Deceased)
- **Caricamento Eager con Progetti**: `GetByIdWithProjectsAsync` carica paziente con tutti i progetti associati (ottimizzazione query)
- **Supporto CancellationToken**: Tutti i metodi supportano cancellazione asincrona

---

### TherapyProjectService

**Interfaccia:** `ITherapyProjectService`

**Metodi Query:**
- `Task<IEnumerable<TherapyProjectModel>> GetAllAsync(CancellationToken ct = default)`
- `Task<TherapyProjectModel?> GetByIdAsync(Guid id, CancellationToken ct = default)`
- `Task<TherapyProjectModel?> GetByIdWithPatientAsync(Guid id, CancellationToken ct = default)`
- `Task<TherapyProjectModel?> GetByIdWithRelationsAsync(Guid id, CancellationToken ct = default)`
- `Task<TherapyProjectModel?> GetActiveForPatientAsync(Guid patientId, CancellationToken ct = default)` ✨ **v1.1**
- `Task<IReadOnlyList<TherapyProjectModel>> GetCompletedForPatientAsync(Guid patientId, CancellationToken ct = default)` ✨ **v1.1**
- `Task<IEnumerable<TherapyProjectModel>> GetByPatientIdAsync(Guid patientId, CancellationToken ct = default)`
- `Task<IEnumerable<TherapyProjectModel>> GetByEducatorIdAsync(Guid educatorId, CancellationToken ct = default)`
- `Task<IEnumerable<TherapyProjectModel>> GetByStatusAsync(string status, CancellationToken ct = default)`
- `Task<IEnumerable<TherapyProjectModel>> SearchAsync(string searchTerm, CancellationToken ct = default)`

**Metodi Gestione Progetto:**
- `Task<Guid> CreateProjectAsync(CreateProjectRequest request, CancellationToken ct = default)` ✨ **v1.1**
- `Task ChangeProjectStateAsync(Guid projectId, TherapyProjectState newState, CancellationToken ct = default)` ✨ **v1.1**
- `Task AddAsync(TherapyProjectModel project, CancellationToken ct = default)`
- `Task UpdateAsync(TherapyProjectModel project, CancellationToken ct = default)`
- `Task DeleteAsync(Guid id, CancellationToken ct = default)`
- `Task AssignEducatorAsync(Guid projectId, Guid educatorId, CancellationToken ct = default)`
- `Task RemoveEducatorAsync(Guid projectId, Guid educatorId, CancellationToken ct = default)`
- `Task<bool> ValidateAsync(TherapyProjectModel project, CancellationToken ct = default)`
- `Task CompleteProjectAsync(Guid projectId, CancellationToken ct = default)`
- `Task PutOnHoldAsync(Guid projectId, CancellationToken ct = default)`
- `Task ResumeProjectAsync(Guid projectId, CancellationToken ct = default)`

**Regole di Business (issue #73 - v1.1):**

1. **Unicità Progetto Active**: Un paziente può avere **UN SOLO** progetto con stato `Active` contemporaneamente. Enforced in:
   - `CreateProjectAsync`: blocca creazione se esiste già progetto Active
   - `ChangeProjectStateAsync`: verifica unicità prima di cambiare stato ad Active
   - `ResumeProjectAsync`: verifica prima di riattivare progetto sospeso

2. **Stati Progetto e Transizioni** (`TherapyProjectState` enum):
   - `Active`: Progetto attivo (default, max 1 per paziente)
   - `Suspended`: Progetto sospeso temporaneamente
   - `Completed`: Progetto concluso (stato finale)
   - `Deceased`: Paziente deceduto (stato finale)
   
   **Transizioni valide**:
   - `Active` → `Suspended`, `Completed`, `Deceased`
   - `Suspended` → `Active`, `Completed`, `Deceased`
   - `Completed`/`Deceased` → **NESSUNA TRANSIZIONE** (stati finali immutabili)

3. **Creazione Progetto con `CreateProjectAsync`**:
   
   **Validazioni**:
   - Titolo: min 3 caratteri, obbligatorio
   - StartDate: obbligatoria
   - PlannedEndDate: deve essere ≥ StartDate (se specificata)
   - EducatorIds: almeno 1 educatore obbligatorio
   - Paziente ed educatori devono esistere nel database
   
   **Generazione Automatica Appuntamenti Canonici** (se `GenerateCanonicalAppointments = true`):
   - Crea automaticamente 4 appuntamenti:
     1. **INTAKE** (Prima Apertura): StartDate + 3 mesi (90 giorni)
     2. **INTERMEDIATE** (Verifica Intermedia): INTAKE + 6 mesi (180 giorni)
     3. **FINAL** (Verifica Finale): INTERMEDIATE + 6 mesi (180 giorni)
     4. **DISCHARGE** (Dimissioni): FINAL + 1 mese (30 giorni)
   - Gli offset temporali sono configurabili tramite `CanonicalAppointmentsConfiguration` (iniettabile via DI)
   - Tutti gli appuntamenti generati hanno `Status = Scheduled`
   
   **Esempio configurazione**:
   ```csharp
   // Default configuration
   var config = CanonicalAppointmentsConfiguration.Default;
   // Custom configuration
   var customConfig = new CanonicalAppointmentsConfiguration {
       Appointments = new List<CanonicalAppointmentDefinition> {
           new() { Type = VisitType.INTAKE, OffsetFromStart = TimeSpan.FromDays(90) },
           // ... altri appuntamenti
       }
   };
   ```

4. **Gestione Stati con `ChangeProjectStateAsync`**:
   - Blocca transizioni da stati finali (Completed/Deceased)
   - Verifica unicità Active quando si cambia stato ad Active
   - Aggiorna automaticamente `UpdatedAt` timestamp

**Nuovi Tipi (issue #73 - v1.1):**

**TherapyProjectState** (enum in `PTRP.Models.Enums`):
```csharp
public enum TherapyProjectState
{
    Active,      // Progetto attivo (max 1 per paziente)
    Suspended,   // Progetto sospeso
    Completed,   // Progetto completato (stato finale)
    Deceased     // Paziente deceduto (stato finale)
}
```

**ProjectStateFilter** (enum in `PTRP.Services.Enums`):
```csharp
public enum ProjectStateFilter
{
    All,         // Nessun filtro (default)
    Active,      // Solo pazienti con almeno un progetto Active
    Suspended,   // Solo pazienti con almeno un progetto Suspended
    Completed,   // Solo pazienti con almeno un progetto Completed
    Deceased     // Solo pazienti con almeno un progetto Deceased
}
```

**CreateProjectRequest** (record in `PTRP.Services.Models`):
```csharp
public record CreateProjectRequest(
    Guid PatientId,                    // ID paziente (deve esistere)
    string Title,                      // Titolo progetto (min 3 caratteri)
    string? Description,               // Descrizione opzionale
    DateTime StartDate,                // Data inizio (obbligatoria)
    DateTime? PlannedEndDate,          // Data fine pianificata (opzionale, >= StartDate)
    TherapyProjectState InitialState,  // Stato iniziale (default: Active)
    List<Guid> EducatorIds,            // Almeno 1 educatore (devono esistere)
    bool GenerateCanonicalAppointments // true = genera 4 appuntamenti automatici
);
```

**CanonicalAppointmentsConfiguration** (class in `PTRP.Services.Configuration`):
```csharp
public class CanonicalAppointmentsConfiguration
{
    public List<CanonicalAppointmentDefinition> Appointments { get; init; }
    
    public static CanonicalAppointmentsConfiguration Default => new()
    {
        Appointments = new()
        {
            new() { Type = VisitType.INTAKE, OffsetFromStart = TimeSpan.FromDays(90) },
            new() { Type = VisitType.INTERMEDIATE, OffsetFromPrevious = TimeSpan.FromDays(180) },
            new() { Type = VisitType.FINAL, OffsetFromPrevious = TimeSpan.FromDays(180) },
            new() { Type = VisitType.DISCHARGE, OffsetFromPrevious = TimeSpan.FromDays(30) }
        }
    };
}
```

---

### VisitService

**Interfaccia:** `IVisitService`

**Metodi:**
- `Task<IEnumerable<ScheduledVisitModel>> GetScheduledVisitsByProjectIdAsync(Guid projectId)`
- `Task<ScheduledVisitModel> GetScheduledVisitByIdAsync(Guid id)`
- `Task CreateScheduledVisitAsync(ScheduledVisitModel scheduledVisit)`
- `Task UpdateScheduledVisitAsync(ScheduledVisitModel scheduledVisit)`
- `Task DeleteScheduledVisitAsync(Guid id)`
- `Task<ActualVisitModel> RegisterActualVisitAsync(ActualVisitModel actualVisit)`
- `Task<IEnumerable<ActualVisitModel>> GetActualVisitsByProjectIdAsync(Guid projectId)`
- `Task<ActualVisitModel> GetActualVisitByIdAsync(Guid id)`

**Regole di Business:**
- Una `ScheduledVisit` può avere **al massimo** una `ActualVisit`
- `ActualDate` non può essere futura
- `ActualEndTime` > `ActualStartTime` (se entrambi specificati)
- Registrare `ActualVisit` imposta automaticamente `ScheduledVisit.Status = Completed`

---

## 🖥️ ViewModels (PTRP.ViewModels)

### MainWindowViewModel

**Responsabilità:**
- Navigazione principale sidebar
- Gestione autenticazione utente
- Stato globale applicazione

**Proprietà:**
- `string CurrentUserName`
- `bool IsAuthenticated`
- `string CurrentViewName`

**Comandi:**
- `NavigateToCommand`: Navigazione tra view
- `LogoutCommand`: Logout utente

---

### PatientListViewModel

**Responsabilità:**
- Lista pazienti
- Ricerca e filtri
- Comandi CRUD pazienti

**Proprietà:**
- `ObservableCollection<PatientModel> Patients`
- `PatientModel? SelectedPatient`
- `string SearchTerm`
- `bool IsLoading`

**Comandi:**
- `SearchPatientsCommand`
- `ClearSearchCommand`
- `AddPatientCommand`
- `EditPatientCommand`
- `DeletePatientCommand`

---

### ProjectFormViewModel

**Responsabilità:**
- Creazione/modifica progetto terapeutico
- Assegnazione educatori
- Gestione stato progetto

**Proprietà:**
- `TherapyProjectModel CurrentProject`
- `ObservableCollection<ProfessionalEducatorModel> AvailableEducators`
- `ObservableCollection<ProfessionalEducatorModel> AssignedEducators`

**Comandi:**
- `SaveProjectCommand`
- `CancelCommand`
- `AssignEducatorCommand`
- `RemoveEducatorCommand`

---

### VisitFormViewModel

**Responsabilità:**
- Registrazione visita effettiva
- Selezione operatori partecipanti
- Validazione campi obbligatori

**Proprietà:**
- `ActualVisitModel CurrentVisit`
- `ObservableCollection<ProfessionalEducatorModel> AvailableOperators`
- `ObservableCollection<VisitOperatorModel> SelectedOperators`

**Comandi:**
- `SaveVisitCommand`
- `CancelCommand`
- `AddOperatorCommand`
- `RemoveOperatorCommand`

---

## 🗄️ Persistenza (PTRP.Data)

### PtrpDbContext

**DbSets:**
- `DbSet<PatientModel> Patients`
- `DbSet<TherapyProjectModel> TherapyProjects`
- `DbSet<ProfessionalEducatorModel> Operators`
- `DbSet<VisitTypeModel> VisitTypes`
- `DbSet<ScheduledVisitModel> ScheduledVisits`
- `DbSet<ActualVisitModel> ActualVisits`
- `DbSet<VisitOperatorModel> VisitOperators`
- `DbSet<ProjectOperatorModel> ProjectOperators`

**Configurazioni Fluent API:**
- Unique constraints su `FiscalCode`, `Email`
- Cascade delete su relazioni 1:N
- Restrict delete su FK critiche (es. `VisitTypeModel`)
- Unique constraint su `(ActualVisitId, OperatorId)` in `VisitOperators`
- Index su campi frequentemente ricercati

---

## 🔐 Sicurezza e Crittografia

**Database a Riposo:**
- SQLite cifrato con **AES-256 CBC**
- Key derivation: **PBKDF2** (≥10,000 iterazioni)
- Password utente come sorgente chiave

**Pacchetti Sincronizzazione:**
- HMAC-SHA256 per integrità payload
- AES-256 per cifratura dati
- Verifica firma prima di import

Vedi [SECURITY.md](SECURITY.md) per dettagli completi.

---

## 📦 Dependency Injection

**Registrazione Servizi (`App.xaml.cs`):**

```csharp
services.AddDbContext<PtrpDbContext>(options =>
    options.UseSqlite("Data Source=ptrp.db"));

// Repositories
services.AddScoped<IPatientRepository, PatientRepository>();
services.AddScoped<ITherapyProjectRepository, TherapyProjectRepository>();
services.AddScoped<IVisitRepository, VisitRepository>();
services.AddScoped<IOperatorRepository, OperatorRepository>();

// Services
services.AddScoped<IPatientService, PatientService>();
services.AddScoped<ITherapyProjectService, TherapyProjectService>();
services.AddScoped<IVisitService, VisitService>();
services.AddScoped<IOperatorService, OperatorService>();

// Configuration (optional custom)
services.AddSingleton<CanonicalAppointmentsConfiguration>(
    CanonicalAppointmentsConfiguration.Default);

// ViewModels
services.AddTransient<MainWindowViewModel>();
services.AddTransient<PatientListViewModel>();
services.AddTransient<ProjectFormViewModel>();
services.AddTransient<VisitFormViewModel>();
```

---

## 📚 Convenzioni di Design

1. **GUID per tutti gli ID**: Supporto scenari distribuiti, evita collisioni durante sync
2. **Timestamp Audit**: `CreatedAt`, `UpdatedAt`, `CreatedBy`, `UpdatedBy` su tutte le entità
3. **Optimistic Locking**: Campo `Version` per gestione concorrenza
4. **Relazioni bidirezionali**: Navigation properties per facilità query EF Core
5. **ToString() significativi**: Debugging e logging leggibili
6. **Validazione a livello servizio**: Servizi business validano prima di passare a repository
7. **Enumerazioni per stati**: Evitare magic strings, type-safety compiletime
8. **Naming consistente**: PascalCase per proprietà, camelCase per parametri
9. **CQS Pattern**: Query methods ritornano `null` se non trovato, Command methods lanciano eccezioni

---

## 🔄 Ciclo di Vita Progetto Terapeutico (Workflow Canonico)

Vedi [USER-WORKFLOW.md](USER-WORKFLOW.md) per dettagli completi.

**Fasi principali:**
1. **Creazione Progetto**: Coordinatore crea progetto, assegna educatori
2. **Schedulazione Automatica**: Sistema schedula 4 visite canoniche:
   - Prima Apertura (INTAKE): StartDate + 3 mesi
   - Verifica Intermedia: INTAKE + 6 mesi
   - Verifica Finale: Verifica Intermedia + 6 mesi
   - Dimissioni (DISCHARGE): Verifica Finale + 1 mese
3. **Registrazione Visite**: Educatori registrano visite effettive
4. **Sincronizzazione**: Pacchetti sync importati dal coordinatore
5. **Chiusura Progetto**: Completamento o sospensione progetto

---

## 🧪 Testing

**Test Coverage Target:** ≥80% per servizi business

**Struttura Test:**
- `PTRP.Tests.Unit`: Unit test per servizi e validazioni
- `PTRP.Tests.Integration`: Integration test per repository e DbContext

**Framework:**
- xUnit per test runner
- Moq per mocking
- FluentAssertions per asserzioni leggibili

**Test Coverage (v1.1):**
- PatientService: 11 test (validazioni, CRUD, ricerca avanzata)
- TherapyProjectService: 8 test (regole business critiche, stati, appuntamenti)

---

## 📖 Riferimenti

- [USER-WORKFLOW.md](USER-WORKFLOW.md) - **Source of Truth** per workflow e regole business
- [DATABASE.md](DATABASE.md) - Schema database dettagliato
- [SECURITY.md](SECURITY.md) - Sicurezza e crittografia
- [PROGETTO_PTRP_SYNC.md](PROGETTO_PTRP_SYNC.md) - Protocollo sincronizzazione
- [SEED.md](SEED.md) - Data seeding

---

**Versione**: 3.1  
**Ultimo aggiornamento**: 03 Febbraio 2026  
**Stato**: Enhanced Patient & TherapyProject Services (#73) - Gestione stati, filtri avanzati, appuntamenti automatici
