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
│   ├── PTRP.Core/                        # Libreria Core - Entità e Logica Business
│   │   ├── Models/                       # Entità di dominio
│   │   │   ├── Entities/                 # Entità base del dominio
│   │   │   │   ├── Patient.cs            # Anagrafica paziente
│   │   │   │   ├── TherapeuticProject.cs # Progetto terapeutico con PTRP
│   │   │   │   ├── ScheduledVisit.cs     # Visita programmata
│   │   │   │   ├── ActualVisit.cs        # Visita registrata con VisitSource
│   │   │   │   ├── Operator.cs           # Educatore/Coordinatore
│   │   │   │   └── ProjectPhase.cs       # Fase progetto (Apertura, Verifica, etc.)
│   │   │   ├── Enums/                    # Enumerazioni
│   │   │   │   ├── PatientStatus.cs      # Active, Suspended, Deceased
│   │   │   │   ├── ProjectStatus.cs      # InProgress, Suspended, Concluded
│   │   │   │   ├── VisitSource.cs        # EducatorImport, CoordinatorDirect
│   │   │   │   └── PhaseType.cs          # Opening, MidReview, FinalReview, etc.
│   │   │   └── DTOs/                     # Data Transfer Objects
│   │   │       ├── PatientDto.cs
│   │   │       ├── ProjectDto.cs
│   │   │       └── SyncPacketDto.cs      # Pacchetto di scambio crittografato
│   │   ├── Interfaces/                   # Contratti servizi
│   │   │   ├── IPatientService.cs
│   │   │   ├── IProjectService.cs
│   │   │   ├── IVisitService.cs
│   │   │   ├── ISyncService.cs
│   │   │   └── IEncryptionService.cs
│   │   └── Exceptions/                   # Eccezioni custom
│   │       ├── PatientNotFoundException.cs
│   │       ├── SyncConflictException.cs
│   │       └── ValidationException.cs
│   │
│   ├── PTRP.Infrastructure/              # Data Access e Servizi Esterni
│   │   ├── Data/
│   │   │   ├── PtrpDbContext.cs          # SQLite DbContext con crittografia
│   │   │   ├── Configurations/           # Entity configurations (Fluent API)
│   │   │   │   ├── PatientConfiguration.cs
│   │   │   │   ├── ProjectConfiguration.cs
│   │   │   │   └── VisitConfiguration.cs
│   │   │   └── Migrations/               # Schema migrations
│   │   ├── Repositories/                 # Data Access Pattern
│   │   │   ├── PatientRepository.cs
│   │   │   ├── ProjectRepository.cs
│   │   │   ├── VisitRepository.cs
│   │   │   └── OperatorRepository.cs
│   │   ├── Services/                     # Implementazioni servizi
│   │   │   ├── PatientService.cs
│   │   │   ├── ProjectService.cs
│   │   │   ├── VisitService.cs
│   │   │   ├── ConflictResolutionService.cs  # Master-Slave sync logic
│   │   │   ├── SyncPacketService.cs          # Crittografia + HMAC
│   │   │   ├── DataMergeService.cs           # UPSERT logic
│   │   │   └── SchemaVersioningService.cs    # Migration handling
│   │   ├── Security/
│   │   │   ├── EncryptionService.cs          # AES database
│   │   │   └── HmacSigningService.cs         # Firma pacchetti
│   │   └── Seeding/
│   │       └── DbContextSeeder.cs            # Data seeding da registro pazienti
│   │
│   ├── PTRP.Application/                 # Application Layer - ViewModels
│   │   ├── ViewModels/                   # ViewModel - Logica presentazione
│   │   │   ├── MainViewModel.cs
│   │   │   ├── PatientListViewModel.cs
│   │   │   ├── PatientDetailViewModel.cs
│   │   │   ├── ProjectListViewModel.cs
│   │   │   ├── ProjectDetailViewModel.cs
│   │   │   ├── VisitListViewModel.cs
│   │   │   └── SyncViewModel.cs              # Gestione sincronizzazione
│   │   ├── Commands/                     # RelayCommand implementations
│   │   ├── Converters/                   # Value Converters per XAML
│   │   │   ├── StatusToColorConverter.cs
│   │   │   ├── VisitSourceToBadgeConverter.cs
│   │   │   └── DateTimeToStringConverter.cs
│   │   └── Validators/                   # Validazione input
│   │       ├── PatientValidator.cs
│   │       └── ProjectValidator.cs
│   │
│   └── PTRP.Presentation/                # Presentation Layer - WPF UI
│       ├── Views/                        # Viste XAML (UserControls)
│       │   ├── PatientListView.xaml
│       │   ├── PatientDetailView.xaml
│       │   ├── ProjectListView.xaml
│       │   ├── ProjectDetailView.xaml
│       │   ├── VisitListView.xaml
│       │   └── SyncStatusView.xaml
│       ├── Themes/                       # Material Design resources
│       │   ├── Generic.xaml
│       │   └── Colors.xaml
│       ├── App.xaml / App.xaml.cs        # Application entry point
│       ├── MainWindow.xaml / MainWindow.xaml.cs
│       └── DependencyInjection.cs        # DI configuration
│
├── tests/
│   ├── PTRP.UnitTests/                   # Unit tests
│   │   ├── Core/
│   │   │   ├── Models/
│   │   │   └── Validators/
│   │   ├── Infrastructure/
│   │   │   ├── Services/
│   │   │   └── Repositories/
│   │   └── Application/
│   │       └── ViewModels/
│   └── PTRP.IntegrationTests/            # Integration tests
│       ├── Database/
│       ├── Sync/                         # Test sincronizzazione e conflict resolution
│       └── Security/                     # Test crittografia e HMAC
│
├── docs/
│   ├── ARCHITECTURE.md                   # Pattern MVVM e offline-first
│   ├── SETUP-GUIDE.md                    # Setup Visual Studio
│   ├── DATABASE.md                       # Schema SQLite, crittografia, ER diagram
│   ├── SYNC-PROTOCOL.md                  # Protocollo sincronizzazione
│   ├── SECURITY.md                       # Crittografia, HMAC, key management
│   ├── API.md                            # Services API
│   ├── WORKFLOW.md                       # Workflow applicativo
│   ├── DEPLOYMENT.md                     # Velopack, distribution, updates
│   ├── DEVELOPMENT.md                    # Guida sviluppatori, Git workflow
│   ├── PROGETTO_PTRP_SYNC.md             # Analisi tecnica architettura
│   └── SEED.md                           # Data seeding strategy
│
├── .github/
│   └── workflows/
│       ├── validate.yml                  # Unit tests, SAST scan
│       ├── security.yml                  # Security checks (chiavi, credenziali)
│       └── deploy-velopack.yml           # Compile + Velopack release
│
└── [config files]
    ├── .gitignore
    ├── .editorconfig
    ├── PTRP.sln
    └── Directory.Build.props
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
   start PTRP.sln
   ```

3. **Restore NuGet packages**
   - Visual Studio lo farà automaticamente
   - Oppure: `dotnet restore`

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
   dotnet build
   dotnet run --project src/PTRP.Presentation
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
- 🛠️ [Development](docs/DEVELOPMENT.md) - Guida per sviluppatori, Git workflow
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

Per domande, bug o feature requests:
- 🐛 Issues: [GitHub Issues](https://github.com/artcava/PTRP/issues)
- 📧 Email: cavallo.marco@gmail.com
- 💬 Discussions: [GitHub Discussions](https://github.com/artcava/PTRP/discussions)

---

**Last Updated**: January 31, 2026
**Architecture Version**: PTRP-Sync v1.0 (Offline-First) - WPF Desktop
