# PTRP - Gestione Progetti Terapeutici Riabilitativi Personalizzati

![Status](https://img.shields.io/badge/status-active-brightgreen)
![.NET](https://img.shields.io/badge/.NET-10-blue)
![UI Framework](https://img.shields.io/badge/UI-WPF-orange)
![Architecture](https://img.shields.io/badge/architecture-offline--first-purple)
![License](https://img.shields.io/badge/license-MIT-green)

## 📋 Descrizione Progetto

**PTRP** è un'applicazione desktop distribuita per la gestione integrata di:
- 👥 **Pazienti** - Anagrafe e dati clinici
- 📊 **Progetti Terapeutici Riabilitativi** - Planning, tracking interventi e visite
- 👨‍⚕️ **Educatori Professionali** - Assegnazione, responsabilità e tracciabilità operazioni

L'applicazione opera con **paradigma offline-first**: ogni nodo (Coordinatore e Educatori) funziona in modo autonomo con database locale SQLite criptato, sincronizzando i dati tramite pacchetti crittografati e firmati digitalmente.

### Caratteristiche Architetturali Uniche
- 🔒 **Offline-First**: Funzionamento garantito senza connessione internet
- 🔐 **Crittografia End-to-End**: Database locale SQLite criptato + HMAC sui pacchetti di scambio
- ♻️ **Sincronizzazione Asincrona**: Risoluzione automatica dei conflitti tramite Master-Slave logic
- 📋 **Tracciabilità Clinica**: Discriminazione fonte dato (EducatorImport vs CoordinatorDirect)
- 🎯 **Conflict Resolution**: Timestamp-based con gerarchia di permessi (Coordinatore = Master per anagrafiche)

---

## 🛠️ Stack Tecnologico

| Componente | Tecnologia | Versione | Motivazione |
|-----------|-----------|----------|-------------|
| **Runtime** | .NET | 10 (LTS) | Supporto esteso, Self-Contained publishing con Velopack |
| **UI Framework** | WPF | .NET 10 | Desktop nativa Windows, MVVM-native, XAML data binding |
| **Language** | C# | 13.0+ | Type inference avanzato, pattern matching |
| **Pattern Architecture** | MVVM + MVVM Toolkit | Microsoft | Separazione concerns, testabilità |
| **Database Locale** | SQLite | Criptato | Assenza SQL Server, portabilità, crittografia nativa |
| **ORM** | Entity Framework Core | 10+ | Migrazioni schema, LINQ queries |
| **Distribuzione** | Velopack | Latest | Delta updates, zero-click deployment |
| **UI Design** | Material Design for WPF | Latest | Design system coerente e moderno |
| **Testing** | xUnit + Moq | Latest | Framework robusto |
| **Crittografia** | System.Security.Cryptography | Built-in | HMAC firma pacchetti, AES database |

---

## 📁 Struttura Progetto

```
PTRP/
├── src/
│   ├── PTRP.Models/                  # Modelli di Dominio
│   │   ├── PatientModel.cs           # Anagrafica paziente
│   │   ├── TherapyProjectModel.cs    # Progetto terapeutico con PTRP
│   │   └── ProfessionalEducatorModel.cs  # Educatore/Coordinatore
│   │
│   ├── PTRP.Data/                    # Data Access Layer
│   │   ├── PTRPDbContext.cs          # SQLite DbContext con crittografia
│   │   └── Repositories/             # Repository Pattern
│   │       ├── PatientRepository.cs
│   │       ├── TherapyProjectRepository.cs
│   │       └── EducatorRepository.cs
│   │
│   ├── PTRP.Services/                # Business Logic Layer
│   │   ├── PatientService.cs         # Gestione pazienti
│   │   ├── TherapyProjectService.cs  # Gestione progetti terapeutici
│   │   ├── EducatorService.cs        # Gestione educatori
│   │   ├── NavigationService.cs      # Navigazione tra viste
│   │   ├── ConfigurationService.cs   # Configurazione applicazione
│   │   └── Interfaces/               # Contratti servizi
│   │       ├── IPatientService.cs
│   │       ├── ITherapyProjectService.cs
│   │       ├── IEducatorService.cs
│   │       ├── INavigationService.cs
│   │       └── IConfigurationService.cs
│   │
│   ├── PTRP.ViewModels/              # Presentation Logic (MVVM)
│   │   ├── MainViewModel.cs          # ViewModel principale
│   │   ├── MainWindowViewModel.cs    # ViewModel finestra principale
│   │   ├── DashboardViewModel.cs     # Dashboard overview
│   │   ├── FirstRunViewModel.cs      # Configurazione primo avvio
│   │   ├── ViewModelBase.cs          # Base class per ViewModels
│   │   ├── Patients/                 # ViewModels pazienti
│   │   │   ├── PatientListViewModel.cs
│   │   │   └── PatientDetailViewModel.cs
│   │   ├── Projects/                 # ViewModels progetti
│   │   │   ├── ProjectListViewModel.cs
│   │   │   └── ProjectDetailViewModel.cs
│   │   └── Educators/                # ViewModels educatori
│   │       ├── EducatorListViewModel.cs
│   │       └── EducatorDetailViewModel.cs
│   │
│   └── PTRP.App/                     # WPF Application Layer
│       ├── App.xaml / App.xaml.cs    # Application entry point
│       ├── MainWindow.xaml / MainWindow.xaml.cs
│       ├── Views/                    # Viste XAML (UserControls)
│       │   ├── DashboardView.xaml
│       │   ├── FirstRunView.xaml
│       │   ├── Patients/
│       │   │   ├── PatientListView.xaml
│       │   │   └── PatientDetailView.xaml
│       │   ├── Projects/
│       │   │   ├── ProjectListView.xaml
│       │   │   └── ProjectDetailView.xaml
│       │   └── Educators/
│       │       ├── EducatorListView.xaml
│       │       └── EducatorDetailView.xaml
│       ├── Converters/               # Value Converters per XAML
│       │   ├── BoolToVisibilityConverter.cs
│       │   ├── StatusToColorConverter.cs
│       │   └── DateTimeConverter.cs
│       └── Models/                   # UI-specific models (es. NavigationItem)
│
├── tests/
│   ├── PTRP.UnitTests/               # Unit tests
│   │   ├── Models/
│   │   ├── Services/
│   │   ├── ViewModels/
│   │   └── Repositories/
│   └── PTRP.IntegrationTests/        # Integration tests
│       ├── Database/
│       ├── Services/
│       └── Security/
│
├── docs/
│   ├── ARCHITECTURE.md               # Pattern MVVM e offline-first
│   ├── SETUP-GUIDE.md                # Setup Visual Studio
│   ├── DATABASE.md                   # Schema SQLite, crittografia, ER diagram
│   ├── SYNC-PROTOCOL.md              # Protocollo sincronizzazione
│   ├── SECURITY.md                   # Crittografia, HMAC, key management
│   ├── API.md                        # Services API
│   ├── WORKFLOW.md                   # Workflow applicativo
│   ├── DEPLOYMENT.md                 # Velopack, distribution, updates
│   ├── DEVELOPMENT.md                # Guida sviluppatori, Git workflow
│   ├── PROGETTO_PTRP_SYNC.md         # Analisi tecnica architettura
│   └── SEED.md                       # Data seeding strategy
│
├── .github/
│   └── workflows/
│       ├── validate.yml              # Unit tests, SAST scan
│       ├── security.yml              # Security checks (chiavi, credenziali)
│       └── deploy-velopack.yml       # Compile + Velopack release
│
└── [config files]
    ├── .gitignore
    ├── .editorconfig
    ├── PTRP.sln                  # Solution file (in src/)
    ├── velopack.json
    └── LICENSE
```

---

## 🚀 Quick Start

### Prerequisiti
- Visual Studio 2022 (Community, Pro, Enterprise)
- **.NET 10 SDK** (https://dotnet.microsoft.com/download/dotnet/10.0)
- **Git** (https://git-scm.com)

### Setup Locale

1. **Clone repository**
   ```bash
   git clone https://github.com/artcava/PTRP.git
   cd PTRP
   ```

2. **Apri solution in Visual Studio**
   ```bash
   start src/PTRP.sln
   ```

3. **Restore NuGet packages**
   - Visual Studio lo farà automaticamente
   - Oppure: `dotnet restore src/PTRP.sln`

4. **Database Setup** (Automatic Migrations + Data Seeding)
   - Alla prima esecuzione, EF Core crea SQLite locale criptato
   - **Dati iniziali estratti automaticamente** dal registro pazienti Excel (DbContextSeeder):
     - ~100 pazienti con stati (Active/Suspended/Deceased)
     - ~50+ operatori/educatori assegnati
     - ~400+ visite programmate (4 fasi: apertura, verifica intermedia, verifica finale, dimissioni)
     - ~280 visite registrate effettive (70% completion rate)
   - Seeding idempotente: riavvii successivi non duplicano
   - Crittografia AES applicata automaticamente
   - 👉 **Leggi [docs/SEED.md](docs/SEED.md)** per dettagli completi sulla strategia di data initialization

5. **Build & Run**
   ```bash
   dotnet build src/PTRP.sln
   dotnet run --project src/PTRP.App
   ```

---

## 🔄 Concetti Architetturali Chiave

### Offline-First Paradigm
- Ogni nodo (Coordinatore + N Educatori) possiede copia locale SQLite criptata
- Sincronizzazione tramite scambio asincrono di pacchetti (email, USB, cloud)
- Nessun database centrale → Resilienza a guasti di connessione

### Master-Slave Logic
- **Coordinatore = Master** per: Anagrafiche pazienti, stati PTRP, autorizzazioni
- **Educatore = Master** per: Visite registrate personalmente fino al merge
- **Conflict Resolution**: Coordinatore ha priorità assoluta su conflitti di stato

### Visit Source Tracking
```csharp
public enum VisitSource {
    EducatorImport,    // Da app Educatore
    CoordinatorDirect  // Inserimento manuale Coordinatore (verifiche d'ufficio, emergenze)
}
```
Visualizzazione UI con badge/colori differenti per auditabilità.

### Sincronizzazione Crittografata
- Pacchetti firmati HMAC per integrità
- AES per confidenzialità dati sensibili
- UPSERT idempotente basato su GUID

---

## 📚 Documentazione

- 📖 [Setup Guide](docs/SETUP-GUIDE.md) - Setup Visual Studio e primo avvio
- 🏗️ [Architecture](docs/ARCHITECTURE.md) - Pattern MVVM, offline-first spiegato
- 💾 [Database](docs/DATABASE.md) - Schema SQLite, crittografia AES, ER diagram, query comuni
- 🔄 [Sync Protocol](docs/SYNC-PROTOCOL.md) - Algoritmo sincronizzazione, conflict resolution
- 🔐 [Security](docs/SECURITY.md) - Crittografia, HMAC, key management
- 🌱 [Seeding](docs/SEED.md) - Strategia data initialization, DbContextSeeder
- 🛠️ [Development](docs/DEVELOPMENT.md) - Guida sviluppatori, Git workflow
- 🚀 [Deployment](docs/DEPLOYMENT.md) - Velopack, zero-click updates
- 📄 [Technical Analysis](docs/PROGETTO_PTRP_SYNC.md) - Analisi tecnica completa (architetto)

---

## 📝 Workflow Sviluppo

```
Feature Branch → Pull Request → Code Review → Merge → Test Suite → Release (Velopack)
```

Vedi [docs/DEVELOPMENT.md](docs/DEVELOPMENT.md) per workflow Git dettagliato.

---

## 🔒 Sicurezza e Privacy

- 🔐 Database SQLite criptato con AES-256
- 📋 Firma HMAC su pacchetti scambio per integrità
- 🛡️ No chiavi hardcoded → Key derivation da password locale
- ✅ Auditabilità: traccia completa operazioni (VisitSource, timestamps, operatore)
- ⚠️ PTRP tratta dati sensibili → GDPR compliance nel roadmap

Dettagli: vedi [docs/SECURITY.md](docs/SECURITY.md)

---

## 📄 License

MIT License - See [LICENSE](LICENSE) file for details

---

## 📞 Support

Per bug report, feature requests o domande sull'utilizzo:
- 🐛 **Issues**: [GitHub Issues](https://github.com/artcava/PTRP/issues)
- 💬 **Discussions**: [GitHub Discussions](https://github.com/artcava/PTRP/discussions)
- 📧 **Email**: cavallo.marco@gmail.com

---

**Last Updated**: January 31, 2026
**Architecture Version**: PTRP-Sync v1.0 (Offline-First) - WPF Desktop
