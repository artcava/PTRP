# DEVELOPMENT - Guida Sviluppo PTRP

## 📋 Prerequisiti

### Strumenti Richiesti
- **Visual Studio 2022** (Community Edition è sufficiente)
- **.NET 8 SDK** (https://dotnet.microsoft.com/download)
- **SQL Server Express 2019+** (https://www.microsoft.com/sql-server/sql-server-express)
- **Git** (https://git-scm.com)
- **GitHub Desktop** (opzionale, ma consigliato per Windows)

### Verificare Installazione
```bash
# Verificare .NET
dotnet --version

# Verificare Git
git --version

# Verificare SQL Server
sqlcmd -S . -Q "SELECT @@VERSION"
```

---

## 🔄 Workflow Git

### Branching Strategy (Git Flow)

```
main (production)  ← stabile, pronto per release
   ↑
   ├─ develop (integrazione)
   │  ↑
   │  ├─ feature/patient-management
   │  ├─ feature/project-tracking
   │  ├─ bugfix/auth-issue
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
# feature/patient-crud          ← Nuova feature
# bugfix/login-error            ← Bug fix
# chore/update-dependencies     ← Maintenance
# docs/setup-guide              ← Documentazione
```

### Commit Convention

Usare commit message semantici:

```bash
# Formato: <type>: <subject>
# Types: feat, fix, docs, style, refactor, test, chore

git commit -m "feat: add patient list view with filtering"
git commit -m "fix: resolve database connection timeout"
git commit -m "docs: update README with setup instructions"
git commit -m "refactor: extract PatientService from ViewModel"
git commit -m "test: add unit tests for PatientViewModel"
```

### Pull Request Workflow

```bash
# 1. Push feature branch
git push origin feature/your-feature-name

# 2. Crea Pull Request su GitHub
#    - Titolo descrittivo
#    - Descrizione della feature
#    - Link a issue correlate (se presenti)
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

## 🏗️ Struttura Soluzione

### Project Layers

```
PTRP.App (WinUI 3)
  ↓
PTRP.ViewModels (MVVM Logic)
  ↓
PTRP.Services (Business Logic)
  ↓
PTRP.Models (Data Models)
  ↓
Database (SQL Server Express)
```

### Responsabilità per Layer

| Layer | Responsabilità | Esempi |
|-------|---|---|
| **App** | UI, Navigation, Window Management | MainWindow.xaml, App.xaml |
| **ViewModels** | Logic di presentazione, binding | PatientListViewModel, ProjectDetailViewModel |
| **Services** | Accesso dati, business rules | PatientService, ProjectService |
| **Models** | Entità e DTOs | Patient, Project, Operator |
| **Database** | Persistenza dati | DbContext, Migrations |

---

## 🏃 Workflow Tipico Sviluppo

### 1. Preparazione
```bash
git clone https://github.com/artcava/PTRP.git
cd PTRP
open PTRP.sln          # Visual Studio
```

### 2. Feature Development
```bash
# Crea feature branch
git checkout -b feature/new-feature

# Modifica codice, testa localmente
codice...

# Commit regolari
git add .
git commit -m "feat: implement feature X"
```

### 3. Testing
```bash
# Esegui solution
dotnet build

# Esegui tests
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

Verrò installate automaticamente tramite NuGet:

```
Microsoft.Toolkit.Mvvm             # MVVM Toolkit
Microsoft.UI.Xaml                  # WinUI 3
MaterialDesignInXamlToolkit         # Design System
MaterialDesignColors                # Design Colors
Microsoft.EntityFrameworkCore       # ORM
Microsoft.EntityFrameworkCore.SqlServer  # SQL Server Provider
System.Data.SqlClient               # SQL Server Client
xUnit                               # Testing Framework
Moq                                 # Mocking Library
```

---

## 🧪 Testing

### Unit Tests
```bash
# Esegui tutti i test
dotnet test

# Esegui test specifico
dotnet test --filter ClassName=PatientViewModelTests

# Con output dettagliato
dotnet test --verbosity detailed
```

### Test Structure
```
tests/PTRP.Tests/
├── ViewModels/
│   └── PatientViewModelTests.cs
├── Services/
│   └── PatientServiceTests.cs
└── Utilities/
    └── TestDataBuilder.cs
```

---

## 🐛 Debug

### Visual Studio Debug
1. Imposta breakpoint (F9)
2. Premi F5 per avviare debug
3. Usa Debug toolbar per step through
4. Ispeziona variabili in Watch window

### Common Debug Scenarios

```csharp
// Logging in ViewModel
Debug.WriteLine($"Patient loaded: {patient.Name}");

// Breakpoint condizionale
// Proprietà → Filter → Count > 10

// Immediate Window
// In Debug mode, Type: patient.Name
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
    
    public ObservableCollection<PatientModel> Patients { get; set; }
    
    public async Task LoadPatientsAsync()
    {
        var patients = await _patientService.GetAllAsync();
    }
}
```

### XAML Conventions
- Usa x: prefix per namespace
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

## 📚 Risorse

- [MVVM Toolkit Docs](https://learn.microsoft.com/en-us/windows/communitytoolkit/mvvm/)
- [WinUI 3 Docs](https://learn.microsoft.com/en-us/windows/apps/winui/)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [C# Best Practices](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)

---

**Domande?** Apri un [GitHub Issue](https://github.com/artcava/PTRP/issues)
