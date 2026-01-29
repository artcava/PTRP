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
- `string Status`: Stato (default: "In Progress")
- `DateTime CreatedAt`: Data di creazione (default: DateTime.Now)
- `DateTime? UpdatedAt`: Data di ultimo aggiornamento (nullable)

**Relazioni:**
- `PatientModel Patient`: Navigazione al paziente
- `ICollection<ProfessionalEducatorModel> ProfessionalEducators`: Educatori assegnati

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
- `ICollection<TherapyProjectModel> AssignedTherapyProjects`: Progetti assegnati

**ToString:** `"{FirstName} {LastName} ({Specialization})"`

---

## Relazioni tra Modelli

```
PatientModel (1) ——— (N) TherapyProjectModel
TherapyProjectModel (N) ——— (N) ProfessionalEducatorModel
```

- Un **Paziente** ha **molti Progetti Terapeutici**
- Un **Progetto Terapeutico** ha **molti Educatori Professionali**
- Un **Educatore Professionale** può lavorare su **molti Progetti Terapeutici**

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

**Note:** Attualmente stub in-memory. Sarà sostituito con repository quando il database sarà pronto.

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

## Note di Design

1. **Tutti gli ID sono `Guid`** per supportare scenari distribuiti e evitare collisioni
2. **Timestamp di audit** (`CreatedAt`, `UpdatedAt`) su tutti i modelli per tracciabilità
3. **Relazioni bidirezionali** tra modelli per navigazione ORM-friendly
4. **ToString() significativi** per debugging e logging
5. **Proprietà di default** per semplificare la creazione degli oggetti

---

## Prossimi Step

- [ ] Implementare `TherapyProjectService` e `ProfessionalEducatorService`
- [ ] Aggiungere DbContext e migrazioni Entity Framework
- [ ] Implementare repository pattern per persistenza
- [ ] Aggiungere ViewModels per TherapyProject e ProfessionalEducator
- [ ] Implementare UI per gestione progetti e educatori
