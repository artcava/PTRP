# MVVM Architecture - PTRP

## 🏗️ Pattern MVVM Spiegato

MVVM (Model-View-ViewModel) è un pattern architetturale che separa la logica dell'applicazione dalla presentazione.

```
┌────────────────────────────────────────────────────────────────┐
│                      PTRP Application                        │
├────────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌────────────┐          ┌────────────┐                │
│  │     VIEW     │◄────────┐ │  VIEWMODEL   │                │
│  │   (XAML)     │ Binding   │  (Logic)     │                │
│  │              │           │              │                │
│  └────────────╘           └────────────└                │
│         ↑                           │                        │
│         └────────────────────────────────────────────────────────────────│
│                 User Events                                  │
│                                                              │
│         ┌────────────────────────────────┐            │
│         │         MODEL (Entities)              │            │
│         │  Patient, Project, Operator, etc.    │            │
│         └────────────────────────────────┘            │
│                       ↑                                       │
│                       │                                       │
│         ┌────────────────────────────────┐            │
│         │      SERVICES (Business Logic)       │            │
│         │  PatientService, ProjectService...  │            │
│         └────────────────────────────────┘            │
│                       ↑                                       │
│                       │                                       │
│         ┌────────────────────────────────┐            │
│         │    DATABASE (SQLite + EF Core)      │            │
│         │     Persistence Layer (Encrypted)      │            │
│         └────────────────────────────────┘            │
│                                                              │
└────────────────────────────────────────────────────────────────┘
```

---

## 📆 Layer Definitions

### 1️⃣ VIEW (XAML/UserControl)
**Responsabilità**: Presentazione UI

```xaml
<!-- File: Views/PatientListView.xaml -->
<UserControl x:Class="PTRP.App.Views.PatientListView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Grid>
        <!-- UI Controls -->
        <DataGrid ItemsSource="{Binding Patients}" />
        <Button Content="Add Patient" Command="{Binding AddPatientCommand}" />
    </Grid>
</UserControl>
```

**Caratteristiche**:
- ✅ Solo elementi UI (Button, TextBox, DataGrid, etc)
- ✅ Binding ai ViewModels (WPF `{Binding}` syntax)
- ✅ NO logica di business
- ✅ NO accesso diretto al database
- ✅ Reattiva agli eventi dell'utente

---

### 2️⃣ VIEWMODEL (C#)
**Responsabilità**: Logica di presentazione e coordinamento

```csharp
// File: ViewModels/PatientListViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

public class PatientListViewModel : ObservableObject
{
    private readonly IPatientService _patientService;
    
    private ObservableCollection<PatientModel> _patients;
    public ObservableCollection<PatientModel> Patients
    {
        get => _patients;
        set => SetProperty(ref _patients, value);
    }
    
    public RelayCommand AddPatientCommand { get; }
    public RelayCommand<PatientModel> DeletePatientCommand { get; }
    
    public PatientListViewModel(IPatientService patientService)
    {
        _patientService = patientService;
        Patients = new ObservableCollection<PatientModel>();
        
        AddPatientCommand = new RelayCommand(AddPatient);
        DeletePatientCommand = new RelayCommand<PatientModel>(DeletePatient);
    }
    
    private async void AddPatient()
    {
        var patient = new PatientModel();
        await _patientService.AddAsync(patient);
        Patients.Add(patient);
    }
}
```

**Caratteristiche**:
- ✅ Espone dati al View tramite Properties
- ✅ Implementa comandi (ICommand, RelayCommand)
- ✅ Mantiene lo stato della UI
- ✅ Coordina le chiamate ai Services
- ✅ Implementa INotifyPropertyChanged (ObservableObject da MVVM Toolkit)

---

### 3️⃣ MODEL (C#)
**Responsabilità**: Entità dati

```csharp
// File: Models/PatientModel.cs
public class PatientModel
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    
    // Relationships
    public ICollection<TherapyProjectModel> Projects { get; set; }
}
```

**Caratteristiche**:
- ✅ Rappresenta i dati
- ✅ Strutture semplici
- ✅ NO logica complessa
- ✅ Usati dal Service layer

---

### 4️⃣ SERVICE (C#)
**Responsabilità**: Logica di business e accesso dati

```csharp
// File: Services/PatientService.cs
public interface IPatientService
{
    Task<IEnumerable<PatientModel>> GetAllAsync();
    Task<PatientModel> GetByIdAsync(int id);
    Task AddAsync(PatientModel patient);
    Task UpdateAsync(PatientModel patient);
    Task DeleteAsync(int id);
}

public class PatientService : IPatientService
{
    private readonly IPatientRepository _repository;
    
    public PatientService(IPatientRepository repository)
    {
        _repository = repository;
    }
    
    public async Task<IEnumerable<PatientModel>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }
    
    public async Task AddAsync(PatientModel patient)
    {
        // Validazioni
        if (string.IsNullOrEmpty(patient.FirstName))
            throw new ArgumentException("First name is required");
        
        // Business rules
        patient.CreatedAt = DateTime.Now;
        
        // Salva
        await _repository.AddAsync(patient);
    }
}
```

**Caratteristiche**:
- ✅ Contiene logica di business
- ✅ Valida i dati
- ✅ Coordina operazioni complesse
- ✅ Implementato come Interface (IPatientService)
- ✅ Injected nel ViewModel

---

### 5️⃣ DATABASE (Entity Framework Core + SQLite)
**Responsabilità**: Persistenza dati locale criptata

```csharp
// File: Services/Database/PtrpDbContext.cs
public class PtrpDbContext : DbContext
{
    public DbSet<PatientEntity> Patients { get; set; }
    public DbSet<ProjectEntity> Projects { get; set; }
    public DbSet<OperatorEntity> Operators { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // SQLite con crittografia locale (vedi docs/SECURITY.md)
        var connectionString = "Data Source=ptrp.db;";
        optionsBuilder.UseSqlite(connectionString);
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configurazioni
        modelBuilder.Entity<PatientEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
            entity.HasMany(e => e.Projects)
                .WithOne(p => p.Patient)
                .HasForeignKey(p => p.PatientId);
        });
    }
}

// File: Services/Repositories/PatientRepository.cs
public interface IPatientRepository
{
    Task<IEnumerable<PatientModel>> GetAllAsync();
    Task AddAsync(PatientModel patient);
}

public class PatientRepository : IPatientRepository
{
    private readonly PtrpDbContext _context;
    
    public PatientRepository(PtrpDbContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<PatientModel>> GetAllAsync()
    {
        var entities = await _context.Patients.ToListAsync();
        return entities.Select(e => MapToModel(e));
    }
    
    public async Task AddAsync(PatientModel patient)
    {
        var entity = MapToEntity(patient);
        _context.Patients.Add(entity);
        await _context.SaveChangesAsync();
    }
}
```

**Caratteristiche**:
- ✅ Entity Framework Core per ORM
- ✅ SQLite per database locale
- ✅ Repository pattern per data access
- ✅ Migrations per versionamento schema
- ✅ Crittografia AES per dati sensibili

---

## 🔄 Data Flow Esempio: Aggiungere un Paziente

```
1. USER ──────────────────────────────────────────────────
   Clicca bottone "Add Patient"
   ↓

2. VIEW (XAML) ─────────────────────────────────────────────────
   <Button Command="{Binding AddPatientCommand}" />
   ↓

3. VIEWMODEL (C#) ──────────────────────────────────────────────
   public RelayCommand AddPatientCommand { get; }
   
   private async void AddPatient()
   {
       var patient = new PatientModel { ... };
       await _patientService.AddAsync(patient);  ◄─────────────────────────────────────────────
       Patients.Add(patient);
   }
   ↓

4. SERVICE (C#) ─────────────────────────────────────────────────
   public async Task AddAsync(PatientModel patient)
   {
       // Validazioni
       ValidatePatient(patient);
       
       // Business logic
       patient.CreatedAt = DateTime.Now;
       
       // Chiama repository
       await _repository.AddAsync(patient);  ◄─────────────────────────────────────────────
   }
   ↓

5. REPOSITORY (C#) ─────────────────────────────────────────────────
   public async Task AddAsync(PatientModel patient)
   {
       var entity = MapToEntity(patient);
       _context.Patients.Add(entity);
       await _context.SaveChangesAsync();  ◄─────────────────────────────────────────────
   }
   ↓

6. DATABASE (SQLite) ─────────────────────────────────────────────────
   INSERT INTO Patients (FirstName, LastName, ...)
   VALUES ('Marco', 'Cavallo', ...)
   ↓

7. VIEWMODEL ──────────────────────────────────────────────────
   Aggiorna ObservableCollection<PatientModel>
   ↓

8. VIEW (XAML) ─────────────────────────────────────────────────
   DataGrid si aggiorna automaticamente (binding)
   ↓

9. USER ─────────────────────────────────────────────────
   Vede il nuovo paziente nella lista!
```

---

## 💡 Vantaggi MVVM

| Vantaggio | Spiegazione |
|-----------|-------------|
| **Separation of Concerns** | Ogni layer ha responsabilità chiare |
| **Testability** | ViewModels e Services sono facili da testare |
| **Reusability** | Services possono essere riutilizzati |
| **Maintainability** | Codice organizzato e facile da modificare |
| **Binding** | XAML binding automatico tra View e ViewModel |
| **Loose Coupling** | Componenti indipendenti grazie alle interfacce |
| **No Code-Behind** | Logica nel ViewModel, non nel .xaml.cs |

---

## 📋 Checklist per Nuove Features

Quando implementi una nuova feature:

- [ ] Creo Model (entità dati)
- [ ] Creo Entity (per EF Core)
- [ ] Creo Repository (data access)
- [ ] Creo Service (business logic)
- [ ] Creo ViewModel (logica presentazione)
- [ ] Creo View/UserControl (XAML UI)
- [ ] Aggiungo unit tests per Service/ViewModel
- [ ] Testo manualmente nell'app

---

## 🔧 Dependency Injection

Tutti i servizi sono registrati nel DI container in App.xaml.cs:

```csharp
// File: App.xaml.cs
public partial class App : Application
{
    private IServiceProvider _serviceProvider;
    
    private void ConfigureServices()
    {
        var services = new ServiceCollection();
        
        // Register Database
        services.AddScoped<PtrpDbContext>();
        
        // Register Repositories
        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        
        // Register Services
        services.AddScoped<IPatientService, PatientService>();
        services.AddScoped<IProjectService, ProjectService>();
        
        // Register ViewModels
        services.AddScoped<PatientListViewModel>();
        services.AddScoped<ProjectDetailViewModel>();
        
        _serviceProvider = services.BuildServiceProvider();
    }
}
```

Injected nel ViewModel costruttore:

```csharp
public PatientListViewModel(IPatientService patientService)
{
    _patientService = patientService; // Automatic injection
}
```

---

## 📚 Risorse

- [Microsoft MVVM Toolkit](https://learn.microsoft.com/en-us/windows/communitytoolkit/mvvm/)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [WPF Data Binding](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/data/)
- [Repository Pattern](https://martinfowler.com/eaaCatalog/repository.html)
- [Material Design for WPF](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit)

---

**Prossimo Step**: Vedi [SETUP-GUIDE.md](SETUP-GUIDE.md) per creare il progetto in Visual Studio.
