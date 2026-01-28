# SEED.md - Strategia Seeding Database PTRP

## 📋 Panoramica

Questo documento descrive la strategia di **data seeding** per inizializzare il database SQLite locale di PTRP con dati realistici derivati dal registro pazienti/operatori usato dal Coordinatore.

I dati seed vengono applicati automaticamente alla **prima esecuzione dell'applicazione**, durante le migrazioni EF Core, così che ogni installazione parta da uno stato coerente e completo.

---

## 🎯 Obiettivi del Seeding

1. **Inizializzazione automatica** – evitare configurazioni manuali del DB.
2. **Ambiente di sviluppo realistico** – dataset vicino a quello reale per test e debug.
3. **Audit trail completo** – tracciare pazienti, progetti, visite e operatori.
4. **Allineamento con l’architettura offline-first** – dati pronti per la sincronizzazione.
5. **Idempotenza** – lo stesso seed può essere eseguito più volte senza duplicare i dati.

---

## 📊 Struttura dei Dati Seed

In termini concettuali, il seeding popola cinque insiemi principali:

1. **Operatori** (educatori, coordinatori, eventuali supervisori)
2. **Pazienti**
3. **Progetti terapeutici (PTRP)** associati a paziente + operatore
4. **Visite programmate** (prima apertura, verifica intermedia, verifica finale, dimissioni)
5. **Visite effettive** (registrazioni reali, con `VisitSource`)

Le definizioni di dominio reali sono in `PTRP.Models`; in questo documento usiamo versioni semplificate per chiarire la struttura.

### 1. Operators (Educatori e Coordinatori)

Origine: colonna **“OPERATORE DI RIFERIMENTO”** del foglio Excel.

Esempio di modello concettuale:

```csharp
public enum OperatorRole
{
    Educator,      // Educatore di base
    Coordinator,   // Coordinatore (master per anagrafiche e stati PTRP)
    Supervisor
}

public record OperatorSeed
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string FirstName { get; init; } = string.Empty;   // es. "Daniele"
    public string LastName { get; init; } = string.Empty;    // es. "Corrias"
    public OperatorRole Role { get; init; } = OperatorRole.Educator;
    public bool IsActive { get; init; } = true;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
```

Esempi di operatori (estratti dal foglio):
- Daniele Corrias
- Andrea Lapaglia
- Debora Foschiano / Perziano
- Fabrizio Lapaglia / Possidente
- Michele Fatiga
- … (in totale ~50 operatori)

### 2. Patients (Pazienti)

Origine: colonna con i nominativi dei pazienti.

```csharp
public enum PatientStatus
{
    Active,        // Paziente attivo in programma
    Suspended,     // "sospeso" nel foglio
    Discharged,    // Dimesso
    Deceased       // "Deceduto" nel foglio
}

public record PatientSeed
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string FirstName { get; init; } = string.Empty;    // es. "Daniele"
    public string LastName { get; init; } = string.Empty;     // es. "Calamita"
    public PatientStatus Status { get; init; } = PatientStatus.Active;
    public string ClinicalNotes { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
```

Esempi (dal registro):
- Daniele Calamita – Active
- Andrea Distante – Active
- Debora Coraglia – Suspended
- Fabrizio Betti – Deceased
- Rosaria Biagione – Active
- … (~100 pazienti totali)

### 3. Therapeutic Projects (PTRP)

Per ogni paziente si genera almeno un progetto terapeutico, legato a un operatore di riferimento e a una data di assegnazione.

```csharp
public enum ProjectStatus
{
    Active,
    Suspended,
    Closed,
    Archived
}

public record TherapeuticProjectSeed
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid PatientId { get; init; }
    public Guid OperatorId { get; init; }          // Operatore di riferimento
    public DateTime AssignmentDate { get; init; }  // "DATA ASSEGNAZIONE" dal foglio
    public ProjectStatus Status { get; init; } = ProjectStatus.Active;
    public string PtDetails { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
```

### 4. Scheduled Visits (Visite Programmate)

Origine: colonne del foglio con le date **programmate** per:
- Prima apertura
- Verifica intermedia
- Verifica finale
- Dimissioni

```csharp
public enum VisitPhase
{
    InitialOpening,          // PRIMA APERTURA
    IntermediateVerification, // VERIFICA INTERMEDIA
    FinalVerification,        // VERIFICA FINALE
    Discharge                 // DIMISSIONI
}

public enum VisitStatus
{
    Scheduled,
    Completed,
    Suspended,
    Missed
}

public record ScheduledVisitSeed
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid ProjectId { get; init; }
    public VisitPhase Phase { get; init; }
    public DateTime ScheduledDate { get; init; }
    public VisitStatus Status { get; init; } = VisitStatus.Scheduled;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
```

### 5. Actual Visits (Visite Effettive)

Origine: colonne **“DATA … EFFETTIVA”**. Qui è importante la distinzione di origine (`VisitSource`) già definita in PROGETTO_PTRP_SYNC.md.

```csharp
public enum VisitSource
{
    EducatorImport,    // Dato importato dall’app Educatore
    CoordinatorDirect  // Inserimento diretto dal Coordinatore
}

public record ActualVisitSeed
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid ScheduledVisitId { get; init; }
    public DateTime ActualDate { get; init; }
    public VisitSource Source { get; init; } = VisitSource.CoordinatorDirect;
    public string RegisteredBy { get; init; } = string.Empty;
    public DateTime RegistrationDate { get; init; }
    public string ClinicalNotes { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
```

Nel seed iniziale si possono marcare tutte le visite effettive come `CoordinatorDirect`, lasciando alla sincronizzazione con le app Educatore l’introduzione di `EducatorImport`.

---

## 🛠️ Implementazione: DbContextSeeder (bozza)

**File suggerito**: `src/PTRP.Services/Database/DbContextSeeder.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using PTRP.Models;

namespace PTRP.Services.Database;

/// <summary>
/// Seeder per popolare il database SQLite con dati iniziali
/// derivati dal registro pazienti/operatori.
/// </summary>
public static class DbContextSeeder
{
    /// <summary>
    /// Seed dei dati iniziali.
    /// Idempotente: se esistono già pazienti, non fa nulla.
    /// </summary>
    public static async Task SeedAsync(PtrpDbContext context)
    {
        if (await context.Patients.AnyAsync())
            return;

        // 1. Operatori
        var operators = GetOperatorSeeds();
        await context.Operators.AddRangeAsync(operators);
        await context.SaveChangesAsync();

        // 2. Pazienti
        var patients = GetPatientSeeds();
        await context.Patients.AddRangeAsync(patients);
        await context.SaveChangesAsync();

        // 3. Progetti terapeutici
        var projects = GetTherapeuticProjectSeeds(patients, operators);
        await context.TherapeuticProjects.AddRangeAsync(projects);
        await context.SaveChangesAsync();

        // 4. Visite programmate
        var scheduledVisits = GetScheduledVisitSeeds(projects);
        await context.ScheduledVisits.AddRangeAsync(scheduledVisits);
        await context.SaveChangesAsync();

        // 5. Visite effettive
        var actualVisits = GetActualVisitSeeds(scheduledVisits, operators);
        await context.ActualVisits.AddRangeAsync(actualVisits);
        await context.SaveChangesAsync();
    }

    // --- Metodi helper (da riempire con mapping reale da Excel) ---

    private static List<Operator> GetOperatorSeeds()
    {
        var now = DateTime.UtcNow;

        return new List<Operator>
        {
            new() { Id = Guid.NewGuid(), FirstName = "Daniele", LastName = "Corrias",  Role = OperatorRole.Educator,    IsActive = true, CreatedAt = now },
            new() { Id = Guid.NewGuid(), FirstName = "Andrea",  LastName = "Lapaglia", Role = OperatorRole.Educator,    IsActive = true, CreatedAt = now },
            new() { Id = Guid.NewGuid(), FirstName = "Debora",  LastName = "Foschiano",Role = OperatorRole.Educator,    IsActive = true, CreatedAt = now },
            new() { Id = Guid.NewGuid(), FirstName = "Michele", LastName = "Fatiga",   Role = OperatorRole.Educator,    IsActive = true, CreatedAt = now },
            new() { Id = Guid.NewGuid(), FirstName = "PTRP",    LastName = "Coordinator", Role = OperatorRole.Coordinator, IsActive = true, CreatedAt = now },
            // … completare con gli altri operatori del foglio
        };
    }

    private static List<Patient> GetPatientSeeds()
    {
        var now = DateTime.UtcNow;

        return new List<Patient>
        {
            new() { Id = Guid.NewGuid(), FirstName = "Daniele", LastName = "Calamita", Status = PatientStatus.Active,    CreatedAt = now },
            new() { Id = Guid.NewGuid(), FirstName = "Andrea",  LastName = "Distante", Status = PatientStatus.Active,    CreatedAt = now },
            new() { Id = Guid.NewGuid(), FirstName = "Debora",  LastName = "Coraglia", Status = PatientStatus.Suspended, CreatedAt = now },
            new() { Id = Guid.NewGuid(), FirstName = "Fabrizio",LastName = "Betti",    Status = PatientStatus.Deceased,  CreatedAt = now },
            new() { Id = Guid.NewGuid(), FirstName = "Rosaria", LastName = "Biagione", Status = PatientStatus.Active,    CreatedAt = now },
            // … completare con gli altri pazienti
        };
    }

    private static List<TherapeuticProject> GetTherapeuticProjectSeeds(
        List<Patient> patients,
        List<Operator> operators)
    {
        var projects = new List<TherapeuticProject>();
        var rng = new Random(42);
        var now = DateTime.UtcNow;

        foreach (var patient in patients)
        {
            var op = operators[rng.Next(operators.Count)];
            var assignmentDate = new DateTime(2025, rng.Next(1, 13), rng.Next(1, 28));

            projects.Add(new TherapeuticProject
            {
                Id = Guid.NewGuid(),
                PatientId = patient.Id,
                OperatorId = op.Id,
                AssignmentDate = assignmentDate,
                Status = patient.Status == PatientStatus.Active
                    ? ProjectStatus.Active
                    : ProjectStatus.Closed,
                PtDetails = $"PTRP per {patient.FirstName} {patient.LastName} – {op.FirstName} {op.LastName}",
                CreatedAt = now
            });
        }

        return projects;
    }

    private static List<ScheduledVisit> GetScheduledVisitSeeds(List<TherapeuticProject> projects)
    {
        var visits = new List<ScheduledVisit>();
        var now = DateTime.UtcNow;

        foreach (var project in projects)
        {
            // Prima apertura: ~1 mese dopo assegnazione
            visits.Add(new ScheduledVisit
            {
                Id = Guid.NewGuid(),
                ProjectId = project.Id,
                Phase = VisitPhase.InitialOpening,
                ScheduledDate = project.AssignmentDate.AddMonths(1),
                Status = VisitStatus.Scheduled,
                CreatedAt = now
            });

            // Verifica intermedia: +6 mesi
            visits.Add(new ScheduledVisit
            {
                Id = Guid.NewGuid(),
                ProjectId = project.Id,
                Phase = VisitPhase.IntermediateVerification,
                ScheduledDate = project.AssignmentDate.AddMonths(7),
                Status = VisitStatus.Scheduled,
                CreatedAt = now
            });

            // Verifica finale: +12 mesi (rispetto alla prima apertura)
            visits.Add(new ScheduledVisit
            {
                Id = Guid.NewGuid(),
                ProjectId = project.Id,
                Phase = VisitPhase.FinalVerification,
                ScheduledDate = project.AssignmentDate.AddMonths(13),
                Status = VisitStatus.Scheduled,
                CreatedAt = now
            });

            // Dimissioni: +13 mesi
            visits.Add(new ScheduledVisit
            {
                Id = Guid.NewGuid(),
                ProjectId = project.Id,
                Phase = VisitPhase.Discharge,
                ScheduledDate = project.AssignmentDate.AddMonths(14),
                Status = VisitStatus.Scheduled,
                CreatedAt = now
            });
        }

        return visits;
    }

    private static List<ActualVisit> GetActualVisitSeeds(
        List<ScheduledVisit> scheduledVisits,
        List<Operator> operators)
    {
        var result = new List<ActualVisit>();
        var rng = new Random(42);
        var now = DateTime.UtcNow;
        var coordinator = operators.FirstOrDefault(o => o.Role == OperatorRole.Coordinator)
                          ?? operators.First();

        foreach (var scheduled in scheduledVisits.Where(v => v.ScheduledDate < now))
        {
            // 70% di probabilità che la visita sia stata effettivamente svolta
            if (rng.Next(100) >= 70)
                continue;

            result.Add(new ActualVisit
            {
                Id = Guid.NewGuid(),
                ScheduledVisitId = scheduled.Id,
                Source = VisitSource.CoordinatorDirect,
                RegisteredBy = $"{coordinator.FirstName} {coordinator.LastName}",
                RegistrationDate = now.AddDays(-rng.Next(1, 30)),
                ClinicalNotes = $"Visita {scheduled.Phase} effettuata.",
                // In un seed realistico si può introdurre un piccolo jitter
                // rispetto alla data programmata
                // (qui omesso per semplicità)
            });
        }

        return result;
    }
}
```

> Nota: il codice sopra è una **bozza concettuale**: va adattato alle firme reali delle entità (`Patient`, `Operator`, `TherapeuticProject`, ecc.) e ai namespace effettivi del progetto.

---

## 🔌 Integrazione con EF Core

### In `PtrpDbContext`

```csharp
public async Task InitializeAsync()
{
    await Database.MigrateAsync();
    await DbContextSeeder.SeedAsync(this);
}
```

### In Bootstrapper / Startup dell’app

```csharp
var dbContext = serviceProvider.GetRequiredService<PtrpDbContext>();
await dbContext.InitializeAsync();
```

Così, alla prima esecuzione, il database viene creato/migrato e popolato con i dati iniziali.

---

## 📊 Ordine di Grandezza del Dataset Seed

Stimato dal foglio "Nominativi PTRP – Progetto Terapeutico Riabilitativo Personalizzato.xlsx":[file:1]

- ~100 pazienti
- ~50+ operatori
- ~100 progetti terapeutici (in media 1 per paziente)
- ~400 visite programmate (4 fasi per progetto)
- ~280 visite effettive (circa 70% di completamento simulato)
- Orizzonte temporale: circa 2025–2028

Questi valori servono a dare un’idea del carico dati in sviluppo e test.

---

## 📌 Mapping Excel → Modello Dati (linee guida)

| Colonna Excel                         | Campo modello                     | Note                                                               |
|--------------------------------------|-----------------------------------|--------------------------------------------------------------------|
| Nome paziente                        | `Patient.FirstName`/`LastName`    | Parse da stringa completa (es. "CALAMITA Daniele").              |
| OPERATORE DI RIFERIMENTO             | `Operator` associato al progetto  | Match su nome/cognome.                                            |
| DATA ASSEGNAZIONE                    | `TherapeuticProject.AssignmentDate` | Data inizio progetto.                                         |
| DATA PRIMA APERTURA (programmata)    | `ScheduledVisit` (InitialOpening) | Se assente → non creare visita.                                   |
| DATA PRIMA APERTURA (effettiva)      | `ActualVisit` collegata           | Se "sospeso"/vuoto → nessuna visita effettiva.                    |
| VERIFICA INTERMEDIA / FINALE …       | Ulteriori `ScheduledVisit`/`ActualVisit` | Con `VisitPhase` adeguato.                             |
| stato ("sospeso", "Deceduto", …)   | `Patient.Status`                  | Mapping su enum `PatientStatus`.                                  |

Le funzioni di parsing vere e proprie (per esempio per date compresse tipo `21012025`) vanno implementate in un modulo separato, in modo testabile.

---

## ⚙️ Idempotenza e Reseeding

La prima riga del `SeedAsync`:

```csharp
if (await context.Patients.AnyAsync())
    return;
```

rende il seeding **sicuro** rispetto a più esecuzioni:

- Prima esecuzione → popola il database.
- Esecuzioni successive → non fanno nulla.

Per rifare completamente il seed in sviluppo:

```bash
# Esempio generico (da adattare al percorso reale)
rm database.db

dotnet ef database update --project src/PTRP.Services
```

---

## 🔐 Considerazioni di Sicurezza

- I dati di seed devono essere **pseudo-anonimizzati** se derivati da casi reali.
- Mai includere nel seed dati sensibili non necessari (es. codice fiscale completo, indirizzo, recapiti).
- In produzione è consigliabile limitare il seeding a:
  - operatori,
  - strutture statiche,
  - eventuali tabelle di configurazione,
  lasciando i dati clinici ai soli flussi operativi.

---

## 📚 Riferimenti

- Documentazione EF Core data seeding: <https://learn.microsoft.com/en-us/ef/core/modeling/data-seeding>
- `PROGETTO_PTRP_SYNC.md` – modello dati e architettura offline-first
- `DATABASE.md` – schema logico del database SQLite
