# Documento di Analisi Tecnica: Sistema di Gestione PTRP
## Progetto: PTRP-Sync (Architettura Disconnessa)

---

## 1. Visione Architetturale: Paradigma Offline-First
Il sistema è progettato come un'applicazione desktop distribuita per .NET 10 che opera in assenza di un database centrale. La sincronizzazione dei dati avviene tramite lo scambio asincrono di pacchetti crittografati tra il Coordinatore e gli Educatori.

### Considerazioni critiche per l'Architetto Software:
* **Integrità e Conflitti:** In un ambiente disconnesso, il rischio di "Data Drift" (deriva dei dati) è sistemico. È imperativo implementare una logica di **Conflict Resolution** basata su timestamp e gerarchia di permessi (il Coordinatore ha priorità assoluta sulle anagrafiche).
* **Idempotenza:** Ogni processo di importazione deve essere progettato per essere eseguito n-volte senza alterare lo stato del database locale (UPSERT basato su GUID).
* **Master-Slave Logic:** L'applicativo del Coordinatore funge da "Master" per le anagrafiche e gli stati dei PTRP. L'applicativo dell'Educatore è "Master" solo per le visite da lui registrate fino al momento del merge.

---

## 2. Modello Dati e Tracciabilità delle Visite
Per garantire la trasparenza e l'audit dei dati clinici, il sistema deve discriminare rigorosamente l'origine di ogni record di visita.

### Discriminazione della Sorgente e Inserimento Manuale
Il Coordinatore deve avere la possibilità di inserire visite "fuori sacco" (es. verifiche d'ufficio o emergenze) che non provengono dal flusso di import degli educatori.

**Specifica del Modello C#:**
```csharp
public enum VisitSource {
    EducatorImport,    // Dato originato dall'applicativo dell'Educatore
    CoordinatorDirect  // Inserimento manuale effettuato dal Coordinatore
}

public record ActualVisit {
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid ScheduledVisitId { get; init; }
    public VisitSource Source { get; init; } 
    public string RegisteredBy { get; init; } 
    public DateTime RegistrationDate { get; init; }
    public string ClinicalNotes { get; init; }
}
Nota per lo sviluppo UI: Il sistema deve utilizzare stili visuali differenti (es. badge o colori di sfondo diversi nel DataGrid) per separare le visite CoordinatorDirect da quelle EducatorImport.

3. Distribuzione e Lifecycle: Analisi di Velopack
Per la gestione degli aggiornamenti e del deployment su .NET 10, si propone l'utilizzo di Velopack.

Analisi per l'Architetto:
Vantaggi: Velopack gestisce gli aggiornamenti delta (solo i bit modificati), rendendo l'update rapido anche su connessioni instabili. Supporta la pubblicazione Self-Contained di .NET 10, eliminando la necessità che l'utente installi manualmente il runtime.
Rischi: Poiché il sistema lavora offline, l'auto-update deve prevedere un meccanismo di Schema Migration. Se la versione $V_2$ dell'app modifica il DB SQLite, il codice deve gestire la migrazione dei dati locali senza perdita di informazioni prima di consentire l'apertura dell'app.
Hosting: Sarà necessario un endpoint pubblico (GitHub Releases) per ospitare i file dei metadati di Velopack.
4. Gestione Repository e CI/CD
Il repository GitHub dovrà essere configurato per supportare lo sviluppo collaborativo e la qualità del codice.

Pipeline di Automazione (GitHub Actions):
Validation Workflow: Esecuzione di unit test sulla logica di calcolo delle scadenze basata sulla regola $T_{target} - T_{now}$.
Security Scan: Analisi statica del codice (SAST) per prevenire l'esposizione accidentale di chiavi di crittografia.
Deployment Workflow: Compilazione del pacchetto Velopack e generazione della release su GitHub al superamento di tutti i test su branch main.
5. Sicurezza e Protezione del Dato
Trattandosi di dati sensibili (PTRP e pazienti), la sicurezza è un requisito non funzionale primario.

Crittografia: Il database locale SQLite deve essere criptato.
Firma dei pacchetti: Ogni file di interscambio deve contenere un hash di verifica (HMAC) per garantire che il file non sia stato manipolato manualmente durante il transito (es. via email o chiavetta USB).
Documento tecnico ad uso interno - Progetto PTRP-Sync
