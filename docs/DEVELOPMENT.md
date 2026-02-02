# DEVELOPMENT - Guida Sviluppo PTRP-Sync

## 📋 Prerequisiti

### Strumenti Richiesti
- **Visual Studio 2022** (Community Edition è sufficiente)
- **.NET 10 SDK** (https://dotnet.microsoft.com/download/dotnet/10.0)
- **Git** (https://git-scm.com)
- **GitHub Desktop** (opzionale, ma consigliato per Windows)

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
| **Database** | Persistenza locale SQLite + migrazioni | PtrpDbContext, Migrations/ |

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

> **Nota UI**: prevedere stili diversi in griglie/elenco per distinguere visualmente `EducatorImport` da `CoordinatorDirect`.

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

### ⚠️ REGOLA CRITICA: Manutenzione Test Durante Sviluppo

**Ogni modifica al codice DEVE essere accompagnata dall'aggiornamento dei test corrispondenti.**

#### Checklist Test Obbligatoria

**Prima di creare una PR, VERIFICA:**

- [ ] **Tutti i test esistenti passano** (`dotnet test`)
- [ ] **Se hai modificato un modello**:
  - [ ] Aggiornati i test unit del modello in `tests/PTRP.Tests/Models/`
  - [ ] Verificata la configurazione EF Core in `PTRPDbContext.cs`
  - [ ] Test dei repository ancora funzionanti (se esistono)
- [ ] **Se hai aggiunto un nuovo modello**:
  - [ ] Creati test unit completi (proprietà, validazioni, default values)
  - [ ] Aggiunto DbSet in `PTRPDbContext.cs`
  - [ ] Configurate relazioni EF Core (chiavi primarie, foreign keys, indici)
  - [ ] Test dei repository esistenti ancora funzionanti
- [ ] **Se hai modificato DbContext**:
  - [ ] Tutti i test di Repository/Services passano
  - [ ] Verificato che non ci siano errori di "missing primary key"
  - [ ] Migration EF Core creata (se necessario)
- [ ] **Se hai aggiunto navigation properties**:
  - [ ] Test di inizializzazione collezioni aggiornati
  - [ ] Relazioni configurate in `OnModelCreating()`

#### Errori Comuni da Evitare

**❌ ERRORE: "The entity type 'XModel' requires a primary key"**
- **Causa**: Modello aggiunto senza configurazione EF Core
- **Fix**: Aggiungi configurazione `HasKey()` in `PTRPDbContext.OnModelCreating()`
- **Prevenzione**: Ogni nuovo modello DEVE avere configurazione EF Core

**❌ ERRORE: Test di repository falliscono dopo modifica modello**
- **Causa**: Modello cambiato ma test non aggiornati
- **Fix**: Aggiorna test mock/setup per riflettere nuove proprietà
- **Prevenzione**: Esegui `dotnet test` dopo OGNI modifica al modello

**❌ ERRORE: Navigation property null durante test**
- **Causa**: Relazione non configurata in `OnModelCreating()`
- **Fix**: Aggiungi `HasOne()/HasMany()` nel DbContext
- **Prevenzione**: Configura relazioni quando aggiungi navigation property

### Unit Tests
```bash
# Esegui tutti i test
dotnet test

# Esegui test specifico
dotnet test --filter ClassName=DataMergeServiceTests

# Con output dettagliato
dotnet test --verbosity detailed

# Solo test Models
dotnet test --filter FullyQualifiedName~Models

# Solo test Repositories
dotnet test --filter FullyQualifiedName~Repositories
```

### Test Structure
```
tests/PTRP.Tests/
├── Models/
│   ├── PatientModelTests.cs
│   ├── TherapyProjectModelTests.cs
│   ├── ScheduledVisitModelTests.cs
│   ├── ActualVisitModelTests.cs
│   └── VisitOperatorModelTests.cs
├── ViewModels/
│   └── PatientViewModelTests.cs
├── Services/
│   ├── PatientServiceTests.cs
│   ├── ProjectServiceTests.cs
│   ├── VisitServiceTests.cs
│   └── ConflictResolutionServiceTests.cs
├── Repositories/
│   ├── PatientRepositoryTests.cs
│   └── ProjectRepositoryTests.cs
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

### Best Practice Test

1. **Test Naming**: `MethodName_Scenario_ExpectedBehavior`
   ```csharp
   [Fact]
   public void Constructor_SetsPropertiesCorrectly() { }
   
   [Fact]
   public void ClinicalNotes_Validation_RequiresMinimum10Characters() { }
   ```

2. **Arrange-Act-Assert**: Struttura chiara in 3 sezioni
   ```csharp
   [Fact]
   public void Test_Example()
   {
       // Arrange
       var model = new PatientModel { FirstName = "Test" };
       
       // Act
       var result = model.ToString();
       
       // Assert
       Assert.Equal("Test", result);
   }
   ```

3. **Coverage Minima**:
   - Ogni modello: 10+ test (proprietà, validazioni, relazioni)
   - Ogni service: 5+ test (CRUD operations, edge cases)
   - Ogni repository: 3+ test (basic CRUD)

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
- [SEED.md](SEED.md) - Strategia di data seeding
- [MVVM Toolkit Docs](https://learn.microsoft.com/en-us/windows/communitytoolkit/mvvm/)
- [WinUI 3 Docs](https://learn.microsoft.com/en-us/windows/apps/winui/)
- [EF Core SQLite](https://learn.microsoft.com/en-us/ef/core/providers/sqlite)
- [Velopack](https://github.com/velopack/velopack)
- [C# Best Practices](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)

---

**Domande?** Apri un [GitHub Issue](https://github.com/artcava/PTRP/issues)