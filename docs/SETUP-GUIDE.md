# SETUP GUIDE - WPF + MVVM Toolkit

Questa guida ti accompagna nella creazione del **progetto iniziale** in Visual Studio usando **WPF**, **MVVM Toolkit** e la struttura prevista per PTRP.

---

## 1️⃣ Creazione Solution in Visual Studio

1. Apri **Visual Studio 2022**
2. Vai su **Crea un nuovo progetto** (Create a new project)
3. Cerca: **"WPF Application (.NET)"** (la versione moderna per .NET 10, non WPF App Framework)
   - Se non la trovi, assicurati di avere il workload: **"Sviluppo desktop .NET"** installato
   - Tools → Get Tools and Features → Verifica checkbox ".NET Desktop Development"
4. Seleziona il template e clicca **Avanti** (Next)

### Parametri progetto
- **Project name**: `PTRP.App`
- **Solution name**: `PTRP`
- **Location**: cartella `src` del repository clonato (`PTRP\src`)
- **Place solution and project in the same directory**: **DISABILITATO** (nessuna spunta)
- **Target Framework**: **.NET 10** (o quello specificato dal progetto)

Clicca **Crea**.

Alla fine avrai:

```
PTRP/
  src/
    PTRP.App/
      PTRP.App.csproj
      App.xaml
      MainWindow.xaml
      ...
```

---

## 2️⃣ Aggiungere Progetti per MVVM

Nella Solution **PTRP** aggiungerai i progetti logici:

### 2.1. Aggiungi `PTRP.Models`

1. Right click sulla Solution → **Aggiungi → Nuovo progetto...**
2. Scegli template: **Libreria di classi (.NET)** / **Class Library (.NET)**
3. Nome progetto: `PTRP.Models`
4. Target Framework: **.NET 10** (o quello del progetto)

### 2.2. Aggiungi `PTRP.ViewModels`

1. Aggiungi → Nuovo progetto → **Libreria di classi (.NET)**
2. Nome: `PTRP.ViewModels`
3. Target: .NET 10

### 2.3. Aggiungi `PTRP.Services`

1. Aggiungi → Nuovo progetto → **Libreria di classi (.NET)**
2. Nome: `PTRP.Services`
3. Target: .NET 10

### 2.4. (Facoltativo per ora) `PTRP.Tests`

1. Aggiungi → Nuovo progetto → **Progetto di test xUnit (.NET)**
2. Nome: `PTRP.Tests`
3. Target: .NET 10

---

## 3️⃣ Allineare Struttura Cartelle al Repo

Dopo aver creato i progetti, assicurati che la struttura sia:

```
PTRP/
├── src/
│   ├── PTRP.App/
│   ├── PTRP.Models/
│   ├── PTRP.ViewModels/
│   └── PTRP.Services/
└── tests/
    └── PTRP.Tests/
```

Se Visual Studio ha creato `PTRP.Tests` sotto `src/`, puoi:
- Chiudere Visual Studio
- Spostare la cartella `PTRP.Tests` manualmente in `tests/`
- Aprire nuovamente la solution
- Rimuovere e riaggiungere il progetto alla solution (**Add → Existing Project**) in `tests/PTRP.Tests/PTRP.Tests.csproj`

---

## 4️⃣ Aggiungere NuGet Packages

### 4.1. MVVM Toolkit

Per il progetto **PTRP.ViewModels**:

1. Right click su `PTRP.ViewModels` → **Gestisci pacchetti NuGet...**
2. Tab **Sfoglia** (Browse), cerca: `CommunityToolkit.Mvvm`
3. Installa il pacchetto **CommunityToolkit.Mvvm** (Microsoft)

### 4.2. Entity Framework Core + SQLite

Per il progetto **PTRP.Services** (e dove risiederà il `DbContext`):

1. Right click su `PTRP.Services` → **Gestisci pacchetti NuGet...**
2. Installa i seguenti pacchetti:
   - `Microsoft.EntityFrameworkCore`
   - `Microsoft.EntityFrameworkCore.Sqlite`
   - `Microsoft.EntityFrameworkCore.Tools`

> 📌 **Nota**: PTRP utilizza **SQLite** come database locale **criptato**, non SQL Server. Questo significa:
> - nessun requisito di installare un'istanza di SQL Server o SQL Server Express
> - il file di database sarà un singolo `.db` (o `.sqlite`) locale
> - la connection string sarà del tipo `Data Source=ptrp.db;` (eventualmente con parametri di crittografia aggiunti)

In una fase successiva, al `DbContext` (es. `PtrpDbContext`) assocerai la configurazione:

```csharp
options.UseSqlite("Data Source=ptrp.db");
```

Tenendo conto delle estensioni necessarie per la crittografia (vedi `docs/SECURITY.md`).

### 4.3. Material Design for WPF (Opzionale)

Per il progetto **PTRP.App** (per ottenere un look design system moderno):

1. Right click su `PTRP.App` → **Gestisci pacchetti NuGet...**
2. Installa (opzionale, per Material Design 3):
   - `MaterialDesignThemes` (componenti e stili Material Design)
   - `MaterialDesignColors` (palette colori Material Design)

> 💡 **Nota**: Material Design for WPF è opzionale ma consigliato per un'interfaccia moderna e coerente. WPF vanilla in Windows 11 ha comunque un aspetto pulito.

---

## 5️⃣ Referenze tra Progetti

Imposta le dipendenze tra progetti in modo da rispettare il layering:

- `PTRP.ViewModels` **riferisce**:
  - `PTRP.Models`
  - `PTRP.Services`
- `PTRP.Services` **riferisce**:
  - `PTRP.Models`
- `PTRP.App` **riferisce**:
  - `PTRP.ViewModels`
  - `PTRP.Models`

### Come aggiungere una reference

1. Right click su `PTRP.ViewModels` → **Aggiungi → Riferimento a progetto...**
2. Seleziona `PTRP.Models` e `PTRP.Services`

Ripeti per `PTRP.App` e `PTRP.Services`.

---

## 6️⃣ Configurazione WPF Iniziale per MVVM

Questi step configurano WPF per il pattern MVVM:

### 6.1. App.xaml.cs - Dependency Injection Setup

Modifica il file `src/PTRP.App/App.xaml.cs`:

```csharp
using Microsoft.Extensions.DependencyInjection;
using PTRP.ViewModels;
using PTRP.Services;

namespace PTRP.App
{
    public partial class App : Application
    {
        private IServiceProvider _serviceProvider;

        public App()
        {
            InitializeComponent();
            ConfigureServices();
        }

        private void ConfigureServices()
        {
            var services = new ServiceCollection();

            // Register Services
            services.AddScoped<IPatientService, PatientService>();
            // ... aggiungi altri services qui

            // Register ViewModels
            services.AddScoped<MainWindowViewModel>();
            // ... aggiungi altri viewmodels qui

            _serviceProvider = services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var mainWindow = new MainWindow()
            {
                DataContext = _serviceProvider.GetRequiredService<MainWindowViewModel>()
            };
            mainWindow.Show();
        }
    }
}
```

### 6.2. App.xaml - Rimuovi StartupUri

Modifica il file `src/PTRP.App/App.xaml`:

```xml
<Application x:Class="PTRP.App.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <!-- RIMUOVI StartupUri="MainWindow.xaml" perché lo gestiremo in App.xaml.cs -->
    <Application.Resources>
    </Application.Resources>
</Application>
```

### 6.3. MainWindow.xaml.cs - CodeBehind Minimale

```csharp
namespace PTRP.App
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // DataContext sarà assegnato da App.xaml.cs
        }
    }
}
```

### 6.4. MainWindow.xaml - Setup XAML Binding

```xml
<Window x:Class="PTRP.App.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="PTRP - Gestione Progetti Terapeutici Riabilitativi Personalizzati" 
        Height="600" 
        Width="1000"
        WindowStartupLocation="CenterScreen"
        Background="White">
    <Grid>
        <StackPanel VerticalAlignment="Center" HorizontalAlignment="Center">
            <TextBlock Text="PTRP - Application Started" FontSize="24" Foreground="Black"/>
            <TextBlock Text="MVVM Architecture Ready" FontSize="14" Foreground="Gray" Margin="0,10,0,0"/>
            <!-- Aggiungerai le tue UserControl/View qui tramite binding -->
        </StackPanel>
    </Grid>
</Window>
```

---

## 7️⃣ Prima Build e Run

1. Imposta `PTRP.App` come **Progetto di avvio**
   - Right click su `PTRP.App` → **Imposta come progetto di avvio**
2. Seleziona configurazione `Debug` e piattaforma `x64`
3. Premi **F5** (Debug) o **Ctrl+F5** (Avvia senza debug)

Dovresti vedere la window WPF con il messaggio "PTRP - Application Started".

> 📌 **Importante**: WPF NON richiede **Developer Mode** come WinUI 3. L'applicazione si avvierà immediatamente senza alcun blocco di sicurezza.

---

## 8️⃣ Verifica Versionamento Git

Dalla root del repository (`PTRP/`):

```bash
git status

# Dovresti vedere i nuovi file .sln, .csproj, ecc.

# Aggiungi tutti i file di solution e progetti
git add .

git commit -m "feat: add initial WPF solution and MVVM projects

- Create WPF application with .NET 10
- Add MVVM Toolkit for ViewModels
- Configure EF Core + SQLite for database layer
- Setup DI container in App.xaml.cs
- Configure MVVM data binding in MainWindow.xaml
- Material Design for WPF (optional styling)"

git push origin main
```

---

## 9️⃣ Prossimo Step: Struttura ViewModel e Prima View

Una volta che:
- Hai completato i passi sopra
- Il progetto **PTRP.App** parte correttamente
- Hai pushato su GitHub

Allora potremo procedere a:
- Creare il **primo ViewModel** (es. `PatientListViewModel`)
- Creare la **prima UserControl/View** (XAML con DataGrid)
- Impostare il **binding** tra View ↔ ViewModel
- Implementare i servizi di base (PatientService, Repository)

---

## 📘 Differenze Chiave: WPF vs WinUI 3

| Aspetto | WPF | WinUI 3 |
|--------|-----|----------|
| **Requisito Developer Mode** | ❌ NO | ✅ SÌ |
| **Setup Locale** | Semplice, no blocchi | Richiede Dev Mode abilitato |
| **Distribuzione** | .exe standard | MSIX/AppX package |
| **XAML Binding** | `{Binding}` | `{x:Bind}` (compiled) |
| **MVVM Support** | Eccellente | Eccellente |
| **Design System** | Material Design 3rd party | Fluent integrato |
| **Performance** | Eccellente | Eccellente |
| **Maturità** | Stabile da 2006 | Moderno (2024+) |

---

Se qualcosa non torna in questi step, scrivimi esattamente il punto in cui ti blocchi (con eventuale screenshot o messaggio di errore), e lo risolviamo passo-passo.
