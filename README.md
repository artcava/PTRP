# PTRP - Gestione Progetti Terapeutici

![Status](https://img.shields.io/badge/status-active-brightgreen)
![.NET](https://img.shields.io/badge/.NET-8.0-blue)
![License](https://img.shields.io/badge/license-MIT-green)

## 📋 Descrizione Progetto

**PTRP** è un'applicazione desktop per la gestione integrata di:
- 👥 **Pazienti** - Anagrafe e dati clinici
- 📊 **Progetti Terapeutici** - Planning e tracking interventi
- 👨‍⚕️ **Operatori** - Assegnazione e responsabilità

L'applicazione fornisce un'interfaccia moderna e intuitiva simile a foglio di calcolo con funzionalità avanzate di gestione e reporting.

---

## 🛠️ Stack Tecnologico

| Componente | Tecnologia | Versione |
|-----------|-----------|----------|
| **UI Framework** | WinUI 3 | Latest |
| **Language** | C# | 12.0+ |
| **.NET Runtime** | .NET | 8.0+ |
| **Pattern** | MVVM + MVVM Toolkit | Microsoft |
| **Database** | SQL Server Express | 2019+ |
| **ORM** | Entity Framework Core | 8.0+ |
| **UI Design** | MaterialDesign XAML | 4.0+ |
| **Testing** | xUnit + Moq | Latest |

---

## 📁 Struttura Progetto

```
PTRP/
├── src/
│   ├── PTRP.Models/              # Entità dati (Patient, Project, Operator)
│   ├── PTRP.ViewModels/          # ViewModel - Logica presentazione
│   ├── PTRP.Views/               # Viste XAML - UI Pages
│   ├── PTRP.Services/            # Servizi - DB, Business Logic
│   │   ├── Database/             # Entity Framework DbContext
│   │   ├── Repositories/         # Data Access Pattern
│   │   └── Business/             # Business Logic Services
│   └── PTRP.App/                 # Applicazione principale WinUI 3
├── tests/
│   └── PTRP.Tests/               # Unit & Integration Tests
├── docs/
│   ├── ARCHITECTURE.md           # Spiegazione pattern MVVM
│   ├── SETUP-GUIDE.md            # Setup Visual Studio
│   ├── DATABASE.md               # Schema e design DB
│   ├── API.md                    # API Services
│   └── WORKFLOW.md               # Workflow applicativo
├── .github/
│   └── workflows/                # CI/CD Pipelines
└── [config files]
```

---

## 🚀 Quick Start

### Prerequisiti
- Visual Studio 2022 (Community, Pro, Enterprise)
- .NET 8 SDK
- SQL Server Express 2019+
- Git

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

4. **Configurazione Database**
   - Vedere `docs/DATABASE.md`

5. **Build & Run**
   ```bash
   dotnet build
   dotnet run --project src/PTRP.App
   ```

---

## 📚 Documentazione

- 📖 [Setup Guide](docs/SETUP-GUIDE.md) - Guida setup Visual Studio
- 🏗️ [Architecture](docs/ARCHITECTURE.md) - Pattern MVVM spiegato
- 💾 [Database](docs/DATABASE.md) - Schema e modello dati
- 🔄 [Workflow](docs/WORKFLOW.md) - Flusso lavoro applicativo
- 🛠️ [Development](DEVELOPMENT.md) - Guida per sviluppatori

---

## 📝 Workflow Sviluppo

```
Feature Branch → Pull Request → Code Review → Merge → Test → Release
```

Vedi [DEVELOPMENT.md](DEVELOPMENT.md) per dettagli.

---

## 👥 Contributors

- **Marco Cavallo** (@artcava) - Lead Developer

---

## 📄 License

MIT License - See [LICENSE](LICENSE) file for details

---

## 📞 Support

Per domande o segnalazioni:
- 📧 Email: cavallo.marco@gmail.com
- 🐛 Issues: [GitHub Issues](https://github.com/artcava/PTRP/issues)

---

**Last Updated**: January 12, 2026
