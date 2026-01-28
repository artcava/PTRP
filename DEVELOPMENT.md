# DEVELOPMENT - Guida Sviluppo PTRP-Sync

## 📋 Prerequisiti

### Strumenti Richiesti
- **Visual Studio 2022** (Community Edition è sufficiente)
- **.NET 10 SDK** (https://dotnet.microsoft.com/download/dotnet/10.0)
- **Git** (https://git-scm.com)
- **GitHub Desktop** (opzionale, ma consigliato per Windows)

> ⚠️ **Non è richiesto SQL Server**: l'app usa un database **SQLite locale criptato** gestito da Entity Framework Core.

### Verificare Installazione
```bash
# Verificare .NET
dotnet --version

# Verificare Git
git --version
```

---

## 🔄 Workflow Git

### Branching Strategy (Git Flow)

```
main (release offline)  ← stabile, distributa via Velopack
   ↑
   ├─ develop (integrazione)
   │  ↑
   │  ├─ feature/patient-management
   │  ├─ feature/project-tracking
   │  ├─ feature/sync-protocol
   │  ├─ bugfix/conflict-resolution
   │  └─ chore/update-deps
```

### Creazione Feature Branch

```bash
# Assicurati di essere su develop
git checkout develop
git pull origin develop

# Crea nuovo branch
git checkout -b feature/your-feature-name

# Nomi convenzione
# feature/patient-crud              ← Nuova feature dominio pazienti
# feature/offline-sync              ← Sincronizzazione offline
# feature/visit-tracking            ← Tracciabilità visite
# bugfix/login-error                ← Bug fix
# bugfix/merge-conflict             ← Correzione conflitti sync
# chore/update-dependencies         ← Maintenance
# docs/architecture                 ← Documentazione tecnica
```

### Commit Convention

Usare commit message semantici:

```bash
# Formato: <type>: <subject>
# Types: feat, fix, docs, style, refactor, test, chore

git commit -m "feat: implement offline sync for educator packets"
git commit -m "fix: resolve conflict resolution edge cases"
git commit -m "docs: align README with PTRP-Sync architecture"
git commit -m "refactor: extract SyncPacketService from ViewModel"
git commit -m "test: add unit tests for DataMergeService"
```

### Pull Request Workflow

```bash
# 1. Push feature branch
git push origin feature/your-feature-name

# 2. Crea Pull Request su GitHub
#    - Titolo descrittivo
#    - Descrizione della feature
#    - Link a issue correlate (es: #12)
#    - Screenshots se UI changes

# 3. Code Review
#    - Aspetta approvazione
#    - Rispondi ai commenti

# 4. Merge
#    - Squash commits se necessario
#    - Delete branch dopo merge

# 5. Pulizia locale
git checkout develop
git pull origin develop
git branch -D feature/your-feature-name
```

---

## 🏗️ Struttura Soluzione e Modello Offline-First

### Project Layers

```
PTRP.App (WinUI 3, Velopack entrypoint)
  ↓
PTRP.ViewModels (MVVM Logic + Sync UX)
  ↓
PTRP.Services (Business Logic + Sync Engine)
  ↓
PTRP.Models (Data Models, DTOs, Sync Contracts)
  ↓
SQLite Encrypted DB (EF Core, Migrations)
```

### Responsabilità per Layer

| Layer | Responsabilità | Esempi |
|-------|----------------|--------|
| **App** | Bootstrap, DI, Navigation, Shell | App.xaml, MainWindow.xaml, Bootstrapper.cs |
| **ViewModels** | Logica di presentazione, orchestrazione sync | PatientListViewModel, ProjectDetailViewModel, SyncViewModel |
| **Services** | Accesso dati, business rules, sync engine | PatientService, ProjectService, VisitService, SyncPacketService, DataMergeService, ConflictResolutionService |
| **Models** | Entità dominio, DTO di sync | Patient, TherapeuticProject, ScheduledVisit, ActualVisit, SyncPacket |
| **Database** | Persistenza locale SQLite + migrazioni | PtrpDbContext, Migrations/

### Modello Dati per Tracciabilità Visite

```csharp
public enum VisitSource
{
    EducatorImport,    // Dato originato dall'app Educatore
    CoordinatorDirect  // Inserimento manuale Coordinatore (verifiche d'ufficio, emergenze)
}

public record ActualVisit
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid ScheduledVisitId { get; init; }
    public VisitSource Source { get; init; }
    public string RegisteredBy { get; init; } = string.Empty;
    public DateTime RegistrationDate { get; init; }
    public string ClinicalNotes { get; init; } = string.Empty;
}
```

> **Nota UI**: prevedere stili diversi in griglie/elenco per distinguere visivamente `EducatorImport` da `CoordinatorDirect`.

---

## 🏃 Workflow Tipico Sviluppo

### 1. Preparazione
```bash
git clone https://github.com/artcava/PTRP.git
cd PTRP
start PTRP.sln          # Visual Studio
```

### 2. Feature Development
```bash
# Crea feature branch
git checkout -b feature/new-feature

# Modifica codice, testa localmente
# Concentrati su una responsabilità per branch (SRP a livello di feature)

# Commit regolari
git add .
git commit -m "feat: implement conflict resolution service"
```

### 3. Testing
```bash
# Build solution
dotnet build

# Esegui tests unitaria e di integrazione
dotnet test

# Esegui app
dotnet run --project src/PTRP.App
```

### 4. Push e PR
```bash
git push origin feature/new-feature
# Crea Pull Request su GitHub
```

---

## 📦 Dipendenze Principali

Installate automaticamente tramite NuGet:

```
CommunityToolkit.Mvvm                 # MVVM Toolkit
Microsoft.UI.Xaml                     # WinUI 3
MaterialDesignInXamlToolkit           # Design System
Microsoft.EntityFrameworkCore         # ORM
Microsoft.EntityFrameworkCore.Sqlite  # SQLite Provider
System.Data.SQLite.Core               # SQLite engine (se usato)
System.Security.Cryptography          # AES + HMAC
Velopack                              # Distribuzione e update
xUnit                                 # Testing Framework
Moq                                   # Mocking Library
```

---

## 🧪 Testing

### Unit Tests
```bash
# Esegui tutti i test
dotnet test

# Esegui test specifico
dotnet test --filter ClassName=DataMergeServiceTests

# Con output dettagliato
dotnet test --verbosity detailed
```

### Test Structure
```
tests/PTRP.Tests/
├── ViewModels/
│   └── PatientViewModelTests.cs
├── Services/
│   ├── PatientServiceTests.cs
│   ├── ProjectServiceTests.cs
│   ├── VisitServiceTests.cs
│   └── ConflictResolutionServiceTests.cs
├── Sync/
│   ├── SyncPacketServiceTests.cs
│   └── DataMergeServiceTests.cs
└── Utilities/
    └── TestDataBuilder.cs
```

### Scenario di Test Critici (da PROGETTO_PTRP_SYNC.md)
- ✅ Merge idempotente dei pacchetti (stesso pacchetto N volte → stato invariato)
- ✅ Conflitti tra Coordinatore e Educatore sulle anagrafiche (Coordinatore vince)
- ✅ Conflitti sulle visite (merge non distruttivo, mantiene storico)
- ✅ Migrazione schema DB tra versioni app (V1 → V2 con dati reali)

---

## 🐛 Debug

### Visual Studio Debug
1. Imposta breakpoint (F9) su ViewModel e Services
2. Premi F5 per avviare debug
3. Usa Debug toolbar per step-through
4. Ispeziona variabili e stato del DB locale (SQLite)

### Logging e Trace

```csharp
// Logging nelle zone critiche di sync
_logger.LogDebug("Merging packet {PacketId} from {Source}", packet.Id, packet.Source);

// Trace per conflict resolution
_logger.LogInformation("Conflict resolved: coordinatorWins={CoordinatorWins}", coordinatorWins);
```

---

## 📝 Code Style

### C# Conventions
- **PascalCase**: Classes, Methods, Properties
- **camelCase**: Private fields, local variables, parameters
- **SCREAMING_SNAKE_CASE**: Constants

```csharp
public class PatientViewModel
{
    private readonly IPatientService _patientService;
    private readonly IVisitService _visitService;

    public ObservableCollection<PatientModel> Patients { get; } = new();

    public async Task LoadPatientsAsync()
    {
        var patients = await _patientService.GetAllAsync();
        Patients.Clear();
        foreach (var patient in patients)
        {
            Patients.Add(patient);
        }
    }
}
```

### XAML Conventions
- Usa `x:` prefix per namespace
- Nomi property in PascalCase
- Indentazione 4 spazi

```xaml
<Grid x:Name="MainGrid" Padding="16">
    <StackPanel Spacing="8">
        <TextBlock Text="Patient List" 
                   Style="{ThemeResource HeadlineTextBlockStyle}" />
    </StackPanel>
</Grid>
```

---

## 🔐 Sicurezza nello Sviluppo

- ❌ **Mai** committare chiavi o segreti (AES key, HMAC key)
- ✅ Usare `dotnet user-secrets` o variabili di ambiente durante sviluppo
- ✅ I pacchetti di scambio vanno sempre firmati (HMAC) nelle build reali
- ✅ Sanificare log (mai loggare dati sensibili del paziente)

---

## 📚 Risorse

- [PROGETTO_PTRP_SYNC.md](PROGETTO_PTRP_SYNC.md) - Documento di Analisi Tecnica base
- [MVVM Toolkit Docs](https://learn.microsoft.com/en-us/windows/communitytoolkit/mvvm/)
- [WinUI 3 Docs](https://learn.microsoft.com/en-us/windows/apps/winui/)
- [EF Core SQLite](https://learn.microsoft.com/en-us/ef/core/providers/sqlite)
- [Velopack](https://github.com/velopack/velopack)
- [C# Best Practices](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)

---

**Domande?** Apri un [GitHub Issue](https://github.com/artcava/PTRP/issues)
