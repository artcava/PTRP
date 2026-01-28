# SETUP GUIDE - WinUI 3 + MVVM Toolkit

Questa guida ti accompagna nella creazione del **progetto iniziale** in Visual Studio usando **WinUI 3**, **MVVM Toolkit** e la struttura prevista per PTRP.

---

## 1️⃣ Creazione Solution in Visual Studio

1. Apri **Visual Studio 2022**
2. Vai su **Crea un nuovo progetto** (Create a new project)
3. Cerca: **"Blank App, Packaged (WinUI 3 in Desktop)"**
   - Se non la trovi, installa il workload: **"Sviluppo desktop con C++"** e **"Sviluppo desktop .NET"** + componenti WinUI 3
4. Seleziona il template e clicca **Avanti** (Next)

### Parametri progetto
- **Project name**: `PTRP.App`
- **Solution name**: `PTRP`
- **Location**: cartella `src` del repository clonato (`PTRP\src`)
- **Place solution and project in the same directory**: **DISABILITATO** (nessuna spunta)

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
4. Target Framework: **.NET 8.0** (o quello usato dal repo)

### 2.2. Aggiungi `PTRP.ViewModels`

1. Aggiungi → Nuovo progetto → **Libreria di classi (.NET)**
2. Nome: `PTRP.ViewModels`
3. Target: .NET 8.0

### 2.3. Aggiungi `PTRP.Services`

1. Aggiungi → Nuovo progetto → **Libreria di classi (.NET)**
2. Nome: `PTRP.Services`
3. Target: .NET 8.0

### 2.4. (Facoltativo per ora) `PTRP.Tests`

1. Aggiungi → Nuovo progetto → **Progetto di test xUnit (.NET)**
2. Nome: `PTRP.Tests`
3. Target: .NET 8.0

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

> 🔎 **Nota**: PTRP utilizza **SQLite** come database locale **criptato**, non SQL Server. Questo significa:
> - nessun requisito di installare un'istanza di SQL Server o SQL Server Express;
> - il file di database sarà un singolo `.db` (o `.sqlite`) locale;
> - la connection string sarà del tipo `Data Source=ptrp.db;` (eventualmente con parametri di crittografia aggiunti) e non avrà server/istanza.

In una fase successiva, al `DbContext` (es. `PtrpDbContext`) assocerai la configurazione:

```csharp
options.UseSqlite("Data Source=ptrp.db");
```

Tenendo conto delle estensioni necessarie per la crittografia (vedi `docs/SECURITY.md`).

### 4.3. UI / Design System

Per WinUI 3 non è possibile usare direttamente **MaterialDesignInXamlToolkit** (pensato per WPF).
Per ottenere un look & feel professionale in WinUI 3 useremo:
- Stili WinUI 3
- Eventuali risorse XAML condivise nel progetto `PTRP.App`
- In futuro, eventuali librerie UI di terze parti compatibili con WinUI 3.

Per ora ci concentriamo su una base "pulita" usando WinUI 3 vanilla.

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

## 6️⃣ Prima Build e Run

1. Imposta `PTRP.App` come **Progetto di avvio**
   - Right click → **Imposta come progetto di avvio**
2. Seleziona configurazione `Debug` e piattaforma `x64`
3. Premi **F5** (Debug) o **Ctrl+F5** (Avvia senza debug)

Dovresti vedere la window base di WinUI 3.

---

## 7️⃣ Verifica Versionamento Git

Dalla root del repository (`PTRP/`):

```bash
git status

# Dovresti vedere i nuovi file .sln, .csproj, ecc.

# Aggiungi tutti i file di solution e progetti
git add .

git commit -m "feat: add initial WinUI 3 solution and MVVM projects"

git push origin main
```

---

## 8️⃣ Prossimo Step: Struttura ViewModel e Prima View

Una volta che:
- Hai completato i passi sopra
- Il progetto **PTRP.App** parte correttamente
- Hai pushato su GitHub

Allora potremo procedere a:
- Creare il **primo ViewModel** (es. `ShellViewModel` / `MainViewModel`)
- Creare la **prima View** con layout simile a Excel (DataGrid)
- Impostare il **binding** tra View ↔ ViewModel

---

Se qualcosa non torna in questi step, scrivimi esattamente il punto in cui ti blocchi (con eventuale screenshot o messaggio di errore), e lo risolviamo passo-passo.
