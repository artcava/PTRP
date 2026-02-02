# Architettura PTRP

## Pattern MVVM

L'applicazione segue il pattern **Model-View-ViewModel (MVVM)** con la seguente struttura:

### Layers

```
PTRP.App (WPF UI)
    ↓ usa
PTRP.ViewModels (Logic + Commands)
    ↓ usa
PTRP.Services (Business Logic)
    ↓ usa
PTRP.Models (Domain Models)
```

---

## Profili Utente e Riconoscimento

Il sistema supporta **due profili utente** con permessi differenziati:

### 1. Coordinatore
- Gestione completa anagrafiche pazienti
- Gestione anagrafica educatori professionali
- Creazione e assegnazione progetti terapeutici
- Assegnazione educatori ai progetti
- Visualizzazione globale di tutti i dati
- Esportazione appuntamenti per educatori

### 2. Educatore Professionale
- Visualizzazione pazienti e progetti assegnati
- Registrazione visite a partire dagli appuntamenti
- Importazione appuntamenti dal Coordinatore
- Esportazione visite registrate
- Accesso limitato ai soli dati di competenza

### Meccanismo di Auto-Configurazione

Il profilo utente viene configurato automaticamente al primo avvio attraverso l'importazione di un pacchetto di configurazione specifico:

**Due Tipi di Pacchetti:**
1. **admin.ptrp** - Configurazione Coordinatore (fornito durante deployment)
2. **appointments_{cognome}_{YYYYMMDD}.ptrp** - Configurazione Educatore (esportato dal Coordinatore)

Il sistema riconosce il profilo dal tipo di pacchetto importato e configura automaticamente i permessi.

Per dettagli completi sul flusso di setup, vedere `docs/USER-WORKFLOW.md` sezione "RICONOSCIMENTO PROFILO UTENTE".

---

## Enumerazioni di Dominio

### ProjectStatus

Rappresenta lo **stato del Progetto Terapeutico** (non del paziente).

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

### VisitType

Rappresenta la **tipologia di appuntamento/visita** nel percorso terapeutico.

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

**Appuntamenti Canonici:** Ogni progetto genera automaticamente 4 appuntamenti programmati con le tempistiche sopra indicate.

---

### AppointmentStatus

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

### PatientAttendance

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

### VisitSource

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

## Modelli di Dominio

### PatientModel

Rappresenta un **Paziente** nel sistema.

**Proprietà:**
- `Guid Id`: Identificatore univoco (autogenerato)
- `string FirstName`: Nome
- `string LastName`: Cognome
- `DateTime CreatedAt`: Data di creazione (default: DateTime.Now)
- `DateTime? UpdatedAt`: Data di ultimo aggiornamento (nullable)
- `ICollection<TherapyProjectModel> TherapyProjects`: Progetti terapeutici associati

**ToString:** `"{FirstName} {LastName}"`

---

### TherapyProjectModel

Rappresenta un **Progetto Terapeutico** associato a un paziente.

**Proprietà:**
- `Guid Id`: Identificatore univoco (autogenerato)
- `Guid PatientId`: FK al paziente
- `string Title`: Titolo del progetto
- `string Description`: Descrizione dettagliata
- `DateTime StartDate`: Data di inizio
- `DateTime? EndDate`: Data di fine (nullable, per progetti in corso)
- `ProjectStatus Status`: Stato del progetto (enum: Active, Suspended, Completed, Deceased)
- `DateTime CreatedAt`: Data di creazione (default: DateTime.Now)
- `DateTime? UpdatedAt`: Data di ultimo aggiornamento (nullable)

**Relazioni:**
- `PatientModel Patient`: Navigazione al paziente
- `ICollection<ProfessionalEducatorModel> ProfessionalEducators`: Educatori assegnati (N:N)
- `ICollection<ScheduledVisitModel> ScheduledVisits`: Appuntamenti programmati

**Vincoli:**
- Un paziente può avere **un solo** progetto con `Status = Active` contemporaneamente
- Alla creazione di un progetto vengono generati automaticamente 4 appuntamenti canonici

**ToString:** `"{Title} (Paziente ID: {PatientId})"`

---

### ProfessionalEducatorModel

Rappresenta un **Educatore Professionale** che può essere assegnato a molteplici progetti.

**Proprietà:**
- `Guid Id`: Identificatore univoco (autogenerato)
- `string FirstName`: Nome
- `string LastName`: Cognome
- `string Email`: Email di contatto
- `string PhoneNumber`: Numero di telefono
- `DateTime DateOfBirth`: Data di nascita
- `string Specialization`: Specializzazione professionale (es. "Psicologo", "Fisioterapista")
- `string LicenseNumber`: Numero di licenza/albo professionale
- `DateTime HireDate`: Data di assunzione/collaborazione
- `string Status`: Stato (default: "Active")
- `DateTime CreatedAt`: Data di creazione (default: DateTime.Now)
- `DateTime? UpdatedAt`: Data di ultimo aggiornamento (nullable)

**Relazioni:**
- `ICollection<TherapyProjectModel> AssignedTherapyProjects`: Progetti assegnati (N:N)
- `ICollection<VisitOperatorModel> VisitParticipations`: Visite a cui ha partecipato

**ToString:** `"{FirstName} {LastName} ({Specialization})"`

---

### ScheduledVisitModel

Rappresenta un **Appuntamento Programmato** nel calendario.

**Proprietà:**
- `Guid Id`: Identificatore univoco (autogenerato)
- `Guid TherapyProjectId`: FK al progetto terapeutico
- `VisitType Type`: Tipologia appuntamento (Intake, Intermediate, Final, Discharge, ExtraVisit)
- `DateTime ScheduledDate`: Data programmata
- `TimeSpan? ScheduledTime`: Ora programmata (opzionale)
- `int? EstimatedDurationMinutes`: Durata stimata in minuti
- `AppointmentStatus Status`: Stato appuntamento (Scheduled, Completed, Missed, Rescheduled)
- `string Notes`: Note sull'appuntamento
- `DateTime CreatedAt`: Data di creazione
- `DateTime? UpdatedAt`: Data di ultimo aggiornamento

**Relazioni:**
- `TherapyProjectModel TherapyProject`: Navigazione al progetto
- `ActualVisitModel? ActualVisit`: Visita effettiva associata (relazione 1:1, nullable)

**Vincoli:**
- Un `ScheduledVisit` può avere **al massimo una** `ActualVisit` associata (relazione 1:1)

**ToString:** `"{Type} - {ScheduledDate:dd/MM/yyyy}"`

---

### ActualVisitModel

Rappresenta una **Visita Effettiva** registrata a partire da un appuntamento.

**⚠️ VINCOLO CRITICO:** Una visita può essere creata **SOLO** a partire da un `ScheduledVisit` esistente (relazione 1:1 obbligatoria).

**Proprietà:**
- `Guid Id`: Identificatore univoco (autogenerato)
- `Guid ScheduledVisitId`: FK all'appuntamento programmato (obbligatorio, 1:1)
- `DateTime ActualDate`: Data effettiva della visita
- `TimeSpan? ActualStartTime`: Ora inizio effettiva
- `TimeSpan? ActualEndTime`: Ora fine effettiva
- `VisitSource Source`: Origine registrazione (EducatorImport, CoordinatorDirect)
- `DateTime RegistrationDate`: Data di registrazione nel sistema
- `string ClinicalNotes`: Note cliniche (obbligatorio)
- `string Outcomes`: Esiti e obiettivi raggiunti
- `PatientAttendance AttendanceStatus`: Presenza paziente (enum)
- `DateTime CreatedAt`: Data di creazione
- `DateTime? UpdatedAt`: Data di ultimo aggiornamento

**Relazioni:**
- `ScheduledVisitModel ScheduledVisit`: Navigazione all'appuntamento (1:1 obbligatorio)
- `ICollection<VisitOperatorModel> Operators`: Operatori presenti (N:N)

**Validazioni:**
- `ScheduledVisitId` obbligatorio (non nullable)
- `ClinicalNotes` obbligatorio (non vuoto)
- `ActualDate` non può essere futura
- `ActualEndTime` deve essere successiva a `ActualStartTime`
- Almeno un operatore deve essere presente

**ToString:** `"Visita {ScheduledVisit.Type} del {ActualDate:dd/MM/yyyy}"`

---

### VisitOperatorModel

Rappresenta la **partecipazione di un Operatore** a una visita effettiva (relazione N:N).

**Proprietà:**
- `Guid ActualVisitId`: FK alla visita effettiva (chiave composita)
- `Guid OperatorId`: FK all'operatore (chiave composita)
- `string RoleInVisit`: Ruolo nella visita (es. "Lead", "Assistant")
- `string Notes`: Note sulla partecipazione

**Relazioni:**
- `ActualVisitModel ActualVisit`: Navigazione alla visita
- `ProfessionalEducatorModel Operator`: Navigazione all'operatore

**Chiave Primaria Composita:** (`ActualVisitId`, `OperatorId`)

**ToString:** `"{Operator.FirstName} {Operator.LastName} - {RoleInVisit}"`

---

## Relazioni tra Modelli

```
PatientModel (1) ────────────── (N) TherapyProjectModel
                                       ↓ (ha stato: Active/Suspended/Completed/Deceased)
                                       ↓ (N:N)
                                       ↓
                                ProfessionalEducatorModel
                                       ↓
                                       ↓ (1:N)
                                       ↓
                                ScheduledVisitModel (4 canonici + extra)
                                       ↓
                                       ↓ (1:1 vincolo obbligatorio)
                                       ↓
                                ActualVisitModel
                                       ↓
                                       ↓ (N:N)
                                       ↓
                                VisitOperatorModel ───── ProfessionalEducatorModel
```

**Regole Fondamentali:**
1. Un **Paziente** ha **molti Progetti Terapeutici**, ma **uno solo** può essere `Active`
2. Un **Progetto Terapeutico** ha **molti Educatori Professionali** (N:N)
3. Un **Progetto** genera automaticamente **4 appuntamenti canonici** alla creazione
4. Un **Appuntamento** può avere **al massimo una Visita** (1:1)
5. Una **Visita** deve essere creata **sempre** a partire da un Appuntamento (vincolo obbligatorio)
6. Una **Visita** può avere **molti Operatori** presenti (N:N)

---

## Regole di Schedulazione Appuntamenti

### Appuntamenti Canonici

Ogni progetto terapeutico genera automaticamente 4 appuntamenti programmati con le seguenti tempistiche:

1. **Prima Apertura (INTAKE)**
   - Tipo: `VisitType.Intake`
   - Tempistica: +3 mesi dalla data di inizio progetto (`StartDate`)
   - Durata stimata: 90 minuti

2. **Verifica Intermedia**
   - Tipo: `VisitType.Intermediate`
   - Tempistica: +6 mesi dalla Prima Apertura
   - Durata stimata: 60 minuti

3. **Verifica Finale**
   - Tipo: `VisitType.Final`
   - Tempistica: +6 mesi dalla Verifica Intermedia
   - Durata stimata: 60 minuti

4. **Dimissioni (DISCHARGE)**
   - Tipo: `VisitType.Discharge`
   - Tempistica: +1 mese dalla Verifica Finale
   - Durata stimata: 45 minuti

### Visite Extra

Oltre ai 4 appuntamenti canonici, il sistema permette la creazione di:
- **Visite aggiuntive** (`VisitType.ExtraVisit`)
- **Follow-up** su richiesta
- **Urgenze** cliniche

Queste visite extra non seguono uno schema temporale predefinito e vengono create manualmente dal Coordinatore.

---

## Servizi

### PatientService

**Interfaccia:** `IPatientService`

**Metodi:**
- `Task<IEnumerable<PatientModel>> GetAllAsync()`
- `Task<PatientModel> GetByIdAsync(Guid id)`
- `Task AddAsync(PatientModel patient)`
- `Task UpdateAsync(PatientModel patient)`
- `Task DeleteAsync(Guid id)`
- `Task<IEnumerable<PatientModel>> SearchAsync(string searchTerm)`

---

### TherapyProjectService

**Interfaccia:** `ITherapyProjectService`

**Metodi:**
- `Task<TherapyProjectModel> CreateProjectAsync(TherapyProjectModel project)`
  - Valida unicità progetto attivo per paziente
  - Genera automaticamente 4 appuntamenti canonici
- `Task<TherapyProjectModel> UpdateProjectStatusAsync(Guid projectId, ProjectStatus newStatus)`
  - Valida transizioni di stato
- `Task<IEnumerable<ScheduledVisitModel>> GetCanonicalAppointmentsAsync(Guid projectId)`

---

### ScheduledVisitService

**Interfaccia:** `IScheduledVisitService`

**Metodi:**
- `Task<IEnumerable<ScheduledVisitModel>> GetByProjectAsync(Guid projectId)`
- `Task<IEnumerable<ScheduledVisitModel>> GetByEducatorAsync(Guid educatorId, DateTime from, DateTime to)`
- `Task<ScheduledVisitModel> RescheduleAsync(Guid visitId, DateTime newDate)`
- `Task MarkAsMissedAsync(Guid visitId)`

---

### ActualVisitService

**Interfaccia:** `IActualVisitService`

**Metodi:**
- `Task<ActualVisitModel> RegisterVisitAsync(Guid scheduledVisitId, ActualVisitModel visitData)`
  - Valida vincolo 1:1 con ScheduledVisit
  - Valida presenza di almeno un operatore
  - Aggiorna stato appuntamento a Completed
- `Task AddOperatorAsync(Guid visitId, Guid operatorId, string role)`
- `Task<IEnumerable<ActualVisitModel>> GetByEducatorAsync(Guid educatorId, DateTime from, DateTime to)`

---

## ViewModels

### MainWindowViewModel

**Responsabilità:**
- Gestione lista pazienti
- Ricerca pazienti
- Comandi CRUD (Add, Update, Delete)
- Stato UI (loading, messaggi)

**Proprietà:**
- `ObservableCollection<PatientModel> Patients`
- `PatientModel SelectedPatient`
- `string SearchTerm`
- `string StatusMessage`
- `bool IsLoading`

**Comandi:**
- `SearchPatientsCommand`
- `ClearSearchCommand`
- `AddPatientCommand`
- `UpdatePatientCommand`
- `DeletePatientCommand`

---

### CalendarViewModel

**Responsabilità:**
- Visualizzazione calendario mensile
- Filtri per educatore, tipo visita, stato progetto
- Lista appuntamenti giornalieri
- Azioni rapide (Registra Visita, Riprogramma, Segna Mancato)

**Proprietà:**
- `DateTime SelectedMonth`
- `DateTime SelectedDate`
- `ObservableCollection<ScheduledVisitModel> DayAppointments`
- Filtri vari

**Logica di Colorazione:**
- Calendario usa **codice colore per STATO PROGETTO**, non per tipo appuntamento:
  - 🟢 Active (Progetto in corso)
  - 🟡 Suspended (Progetto sospeso)
  - ⚫ Deceased (Paziente deceduto)
  - ⚪ Completed (Progetto concluso)

---

### VisitFormViewModel

**Responsabilità:**
- Registrazione visita effettiva a partire da appuntamento
- Validazione vincoli (date, operatori, note obbligatorie)
- Selezione multipla operatori presenti

**Proprietà:**
- `ScheduledVisitModel ScheduledVisit` (obbligatorio)
- `ActualVisitModel ActualVisit`
- `ObservableCollection<ProfessionalEducatorModel> AvailableOperators`
- `ObservableCollection<ProfessionalEducatorModel> SelectedOperators`

**Comandi:**
- `SaveVisitCommand` (valida e chiama `IActualVisitService.RegisterVisitAsync`)

---

## Calendario e Visualizzazione

### Logica di Codifica Colori

Il calendario mensile utilizza badge colorati basati sullo **stato del progetto** associato all'appuntamento:

- **🟢 Verde (Active)**: Progetto attivo in corso
- **🟡 Giallo (Suspended)**: Progetto temporaneamente sospeso
- **⚫ Nero (Deceased)**: Progetto chiuso per decesso paziente
- **⚪ Bianco (Completed)**: Progetto completato con successo

Questa scelta facilita l'identificazione immediata dello stato clinico del paziente associato all'appuntamento.

---

## Note di Design

1. **Tutti gli ID sono `Guid`** per supportare scenari distribuiti e evitare collisioni
2. **Timestamp di audit** (`CreatedAt`, `UpdatedAt`) su tutti i modelli per tracciabilità
3. **Relazioni bidirezionali** tra modelli per navigazione ORM-friendly
4. **ToString() significativi** per debugging e logging
5. **Proprietà di default** per semplificare la creazione degli oggetti
6. **Enum fortemente tipizzati** per stati, tipologie e source tracking
7. **Vincolo 1:1 obbligatorio** tra ScheduledVisit e ActualVisit per integrità dati
8. **Unicità progetto attivo** garantita a livello applicativo e database

---

## Riferimenti

- [USER-WORKFLOW.md](USER-WORKFLOW.md) - Flussi utente dettagliati (source of truth)
- [DATABASE.md](DATABASE.md) - Schema database SQLite completo
- [PROGETTO_PTRP_SYNC.md](PROGETTO_PTRP_SYNC.md) - Architettura sincronizzazione
- [SECURITY.md](SECURITY.md) - Modello di sicurezza e GDPR
- [DEVELOPMENT.md](DEVELOPMENT.md) - Guida sviluppatori

---

**Documento aggiornato:** 02 Febbraio 2026  
**Versione:** 2.0 (Allineato con USER-WORKFLOW.md)  
**Autore:** Marco Cavallo (@artcava)
