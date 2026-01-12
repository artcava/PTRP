# SETUP GUIDE - WinUI 3 + MVVM Toolkit

Questa guida ti accompagna nella creazione del **progetto iniziale** in Visual Studio usando **WinUI 3**, **MVVM Toolkit** e la struttura prevista per PTRP.

---

## 1️⃣ Creazione Solution in Visual Studio

1. Apri **Visual Studio 2022**
2. Vai su **Create a new project**
3. Cerca: **"Blank App, Packaged (WinUI 3 in Desktop)"**
   - Se non la trovi, installa il workload: **"Desktop development with C++"** e **".NET Desktop Development"** + componenti WinUI 3
4. Seleziona il template e clicca **Next**

### Parametri progetto
- **Project name**: `PTRP.App`
- **Solution name**: `PTRP`
- **Location**: cartella `src` del repository clonato (`...epos	runkbvepos	runkbv...` → per noi: `PTRP\src`)
- **Place solution and project in the same directory**: **DISABILITATO** (NO spunta)

Clicca **Create**.

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

1. Right click sulla Solution → **Add → New Project...**
2. Scegli template: **Class Library (.NET)**
3. Nome progetto: `PTRP.Models`
4. Target Framework: **.NET 8.0**

### 2.2. Aggiungi `PTRP.ViewModels`

1. Add → New Project → **Class Library (.NET)**
2. Nome: `PTRP.ViewModels`
3. Target: .NET 8.0

### 2.3. Aggiungi `PTRP.Services`

1. Add → New Project → **Class Library (.NET)**
2. Nome: `PTRP.Services`
3. Target: .NET 8.0

### 2.4. (Facoltativo per ora) `PTRP.Tests`

1. Add → New Project → **xUnit Test Project (.NET)**
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
- Rimuovere e riaggiungere il progetto alla solution (Add → Existing Project)

---

## 4️⃣ Aggiungere NuGet Packages

### 4.1. MVVM Toolkit

Per il progetto **PTRP.ViewModels**:

1. Right click su `PTRP.ViewModels` → **Manage NuGet Packages...**
2. Tab **Browse**, cerca: `CommunityToolkit.Mvvm`
3. Installa il pacchetto **CommunityToolkit.Mvvm** (Microsoft)

### 4.2. Entity Framework Core + SQL Server

Per il progetto **PTRP.Services**:

1. Manage NuGet Packages...
2. Installa:
   - `Microsoft.EntityFrameworkCore`
   - `Microsoft.EntityFrameworkCore.SqlServer`
   - `Microsoft.EntityFrameworkCore.Tools`

### 4.3. Material Design (UI)

Per WinUI 3 non possiamo usare direttamente **MaterialDesignInXamlToolkit** (pensato per WPF).
Per ottenere un look & feel professionale in WinUI 3 useremo:
- Stili WinUI 3
- Possibile integrazione futura di librerie UI di terze parti (se necessario)

Per ora ci concentriamo su una base "clean" usando WinUI 3 vanilla e DataGrid.

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

### Come aggiungere una reference:

1. Right click su `PTRP.ViewModels` → **Add → Project Reference...**
2. Seleziona `PTRP.Models` e `PTRP.Services`

Ripeti per `PTRP.App` e `PTRP.Services`.

---

## 6️⃣ Prima Build e Run

1. Imposta `PTRP.App` come **Startup Project**
   - Right click → **Set as Startup Project**
2. Seleziona configurazione `Debug` e `x64`
3. Premi **F5** (Debug) o **Ctrl+F5** (Run without debug)

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
