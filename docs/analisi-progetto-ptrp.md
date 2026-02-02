# Analisi Completa del Progetto PTRP

Questo documento raccoglie l'analisi strutturale e funzionale del progetto PTRP, con particolare attenzione a:
- dominio applicativo (Progetti Terapeutici, Pazienti, Educatori Professionali)
- architettura MVVM e architettura UI
- stato attuale del codice e delle issue
- issue esistenti e mancanti (con riferimento alla meta-issue #76)
- roadmap di evoluzione.

---

## 1. Contesto e Dominio

PTRP (Progetti Terapeutici in Rete per la Psichiatria) è un'applicazione WPF offline-first per la gestione di:
- **Pazienti** e dati anagrafici clinicamente rilevanti
- **Progetti Terapeutici** (un solo progetto attivo per paziente, storico progetti completati)
- **Educatori Professionali** e loro coinvolgimento nei progetti
- **Appuntamenti programmati** (Scheduled Visit) legati ai progetti
- **Visite effettive** (Actual Visit) registrate dagli educatori a partire dagli appuntamenti
- **Sincronizzazione** sicura tra coordinatore e dispositivi degli educatori tramite pacchetti cifrati e firmati.

Il dominio è descritto principalmente in:
- `docs/USER-WORKFLOW.md` (flussi Coordinatore/Educatore - **source of truth**)
- `docs/DATABASE.md` (schema SQLite e relazioni)
- `docs/PROGETTO_PTRP_SYNC.md` (analisi tecnica sincronizzazione)
- `docs/SECURITY.md` (modello di sicurezza e compliance GDPR).

### 1.1 Attori principali

- **Coordinatore**
  - gestisce anagrafiche pazienti
  - crea e gestisce Progetti Terapeutici
  - pianifica appuntamenti canonici e aggiuntivi
  - esporta e importa pacchetti di sincronizzazione
  - ha visione globale tramite Dashboard e Calendario.

- **Educatore Professionale**
  - lavora principalmente su calendario personale
  - registra le visite effettive a partire dagli appuntamenti
  - esporta visite registrate verso il Coordinatore
  - ha permessi limitati su anagrafiche e progetti.

- **Supervisor / ruoli di sola lettura** (in prospettiva, vedi issue #4)
  - consultano dati aggregati e audit trail
  - non modificano contenuti clinici.

### 1.2 Entità di dominio chiave

- **Patient**: anagrafica paziente
- **TherapyProject**: progetto terapeutico associato a un paziente
  - stato clinico (`Active`, `Suspended`, `Completed`, `Deceased`) è proprietà del progetto, non del paziente
  - associazione N:N con ProfessionalEducator
  - **vincolo**: un paziente può avere **UN SOLO** progetto con stato `Active` contemporaneamente
- **ScheduledVisit**: appuntamento programmato
  - tipologia (`Intake`, `Intermediate`, `Final`, `Discharge`, `ExtraVisit`)
  - stato appuntamento (`Scheduled`, `Completed`, `Missed`, `Rescheduled`)
  - legato a TherapyProject
  - **4 appuntamenti canonici** generati automaticamente alla creazione progetto:
    - Prima Apertura (INTAKE): +3 mesi dall'assegnazione
    - Verifica Intermedia: +6 mesi dalla Prima Apertura
    - Verifica Finale: +6 mesi dalla Verifica Intermedia
    - Dimissioni: +1 mese dalla Verifica Finale
- **ActualVisit**: visita effettiva
  - **relazione 1:1 OBBLIGATORIA** con ScheduledVisit
  - contiene contenuto clinico, presenza paziente (`PresentCollaborative`, `PresentNonCollaborative`, `AbsentJustified`, `AbsentUnjustified`), operatori presenti
  - **vincolo critico**: può essere creata **SOLO** a partire da un appuntamento esistente
  - discriminazione origine (`EducatorImport`, `CoordinatorDirect`)
- **ProfessionalEducator**: operatore che partecipa ai progetti e alle visite
- **VisitOperator**: entità di join Many-to-Many tra ActualVisit ed Educator
- **Audit / SyncLog** (issue #1): tracciamento azioni e sincronizzazioni.

Questi elementi sono in parte già modellati e completati tramite le issue #71 (modelli dominio appuntamenti/visite) e correlate.

---

## 2. Architettura MVVM e UI

L'applicazione adotta un'architettura **MVVM** con separazione chiara tra:
- `PTRP.Models` – modelli di dominio
- `PTRP.Data` – accesso dati (DbContext, repositories)
- `PTRP.Services` – servizi di dominio/applicativi
- `PTRP.ViewModels` – ViewModel MVVM
- `PTRP.App` – applicazione WPF (Views, Styles, Behaviors, Converters).

La struttura UI e i flussi principali sono documentati in:
- `docs/ARCHITECTURE.md`
- issue #55 (Architettura UI completa)
- `docs/WORKFLOW.md` / `docs/USER-WORKFLOW.md`.

### 2.1 Shell e Navigazione

- `MainWindow.xaml` funge da **Shell** con:
  - top bar (titolo app, utente, impostazioni)
  - sidebar di navigazione (Dashboard, Pazienti, Progetti, Educatori, Calendario, Sync, Report, Impostazioni)
  - content area con `Frame` o equivalente per caricare le view
  - snackbar centrale per feedback (success/error/warning).
- Un `INavigationService` (issue #46) gestisce la navigazione tra Views, evitando logica di routing nelle ViewModels.

### 2.2 Views e ViewModels principali

Macro-moduli UI (da issue #55, #51, #71–#75):

- **Dashboard**
  - `DashboardView` / `DashboardViewModel`
  - KPI cards (pazienti totali, progetti attivi, educatori operativi, visite completate)
  - grafici di andamento visite e Top Educators.

- **Pazienti**
  - `PatientListView` / `PatientListViewModel` (master-detail)
  - `PatientFormView` per CRUD paziente
  - pannello destro con anagrafica, progetto attivo, progetti completati, prossimo appuntamento
  - integrazione con `ITherapyProjectService` (#73) e servizi pazienti (#73).

- **Progetti Terapeutici**
  - `ProjectFormView` / `ProjectFormViewModel` (issue #74)
  - integrata in `PatientListView` (detail panel): creazione nuovo progetto, change state, visualizzazione storico
  - generazione automatica degli appuntamenti canonici attraverso `ITherapyProjectService` (#73).

- **Educatori**
  - `EducatorListView` + `EducatorFormView` (documentata in #55, implementazione da completare)
  - gestione anagrafiche educatori, specializzazioni, assegnazioni.

- **Calendario e Visite**
  - `CalendarView` / `CalendarViewModel` (issue #75)
    - calendario mensile con badge per giorni con appuntamenti
    - **codice colore per STATO PROGETTO** (non per tipo appuntamento):
      - 🟢 Active (progetto in corso)
      - 🟡 Suspended (progetto sospeso)
      - ⚫ Deceased (paziente deceduto)
      - ⚪ Completed (progetto concluso)
    - filtri per tipo appuntamento, educatore, stato progetto
    - lista appuntamenti del giorno selezionato con azioni rapide
  - `VisitFormView` / `VisitFormViewModel` (issue #75)
    - registrazione visita a partire da `ScheduledVisit` (vincolo obbligatorio)
    - validazioni: date/ore, almeno un operatore presente, note cliniche obbligatorie
    - selezione multipla operatori presenti dalla lista educatori del progetto

- **Sincronizzazione**
  - `SyncView` con tab Export/Import (issue #52)
  - `ConflictResolutionView` (issue #53) per gestione conflitti durante import.

- **Dialogs e infrastruttura UI**
  - `ConfirmationDialog`, `ErrorDetailsDialog` (issue #54)
  - Behaviors di validazione e componenti condivisi (ValidationBehavior, converters, ecc.).

### 2.3 Design System

Descritta in `Styles/*.xaml` e issue #55:
- palette colori primari/secondari/semantic (success/warning/error/info)
- tipografia e spacing system
- stili per pulsanti, DataGrid, card KPI, layout responsive
- focus su **accessibilità** (contrasto, navigazione tastiera, focus visibile – da completare in FASE 4).

---

## 3. Stato Attuale del Codice e delle Issue

### 3.1 Foundation dati e sicurezza

Issue aperte principali:
- #71 – completamento modelli dominio per appuntamenti e visite (✅ **COMPLETATA**)
- #72 – repositories per progetti, appuntamenti e visite
- #73 – PatientService e TherapyProjectService
- #13 – data seeding per ambiente di sviluppo
- #14 – gestione migrations e versioning database
- #15 – error handling e logging per operazioni DB
- #16 – ottimizzazione query e caching
- #1 – audit trail per entità cliniche e log di sincronizzazione
- #2 – crittografia AES-256 per DB SQLite e payload sync
- #3 – gestione chiavi crittografiche (master key, HMAC, rotazione)
- #4 – AuthorizationService e RBAC
- #5 – workflow CI di sicurezza (secret scanning, dependency scan, CodeQL)
- #6 – GDPRComplianceService (diritto all'oblio e portabilità dati).

Queste issue coprono le fondamenta **Data Foundation** e **Security/Privacy**, ma non esauriscono tutte le esigenze (si veda §5). 

### 3.2 UI e UX

Issue chiave:
- #55 – Documentazione architettura UI completa
- #54 – Validation UI e Error Handling globale
- #68 – Revisione completa documentazione in `/docs`
- #71–#75 – pipeline completa domain → data → services → UI per progetti, calendario e visite.

Stato qualitativo:
- La **visione UI** è molto dettagliata (grazie a #55)
- Esiste già una scomposizione in **fasi** (0–4)
- Mancano però alcune *issue di collegamento* e *issue per micro-componenti UI riusabili* (es. controlli custom, skeleton loading, messaggistica di stato coerente su tutte le view).

### 3.3 Sincronizzazione

La parte di sincronizzazione è ben descritta in `docs/PROGETTO_PTRP_SYNC.md` e `docs/SECURITY.md`, ma lato issue sono
principalmente presenti:
- #1 (audit trail, che tocca anche sync)
- #2 (crittografia payload)
- #3 (gestione chiavi)
- #5 (CI di sicurezza).

Le UI di sync e conflict resolution sono coperte da:
- #52 – UI Sincronizzazione
- #53 – Conflict Resolution UI.

Mancano però issue specifiche per:
- compatibilità di versione del protocollo
- strumenti di diagnostica/troubleshooting per pacchetti
- test end-to-end di roundtrip (Export → Transfer → Import) con dataset realistici.

---

## 4. Issue Esistenti (sintesi per area)

Questa sezione fornisce una vista di alto livello (non esaustiva al 100%) delle issue attualmente aperte, raggruppate per area funzionale/tecnica.

### 4.1 Domain & Data Foundation

- #71 – Modelli dominio per appuntamenti/visite (✅ **COMPLETATA**)
- #72 – Repositories progetti/appuntamenti/visite
- #73 – PatientService e TherapyProjectService
- #13 – Data seeding per sviluppo
- #14 – Migrations e versioning DB
- #16 – Performance query e caching.

### 4.2 Security, Privacy, Audit

- #1 – Audit trail per entità cliniche e log sync
- #2 – AES-256 per DB SQLite e payload sync
- #3 – Gestione chiavi crittografiche
- #4 – AuthorizationService e RBAC
- #5 – Workflow CI sicurezza
- #6 – GDPRComplianceService.

### 4.3 UI & UX (Shell, Views, Validation)

- #55 – Documentazione architettura UI (con roadmap FASE 1–4)
- #54 – Validation UI e error handling globale
- #71–#75 – pipeline completa UI per progetti/calendario/visite
- #68 – Revisione documentazione in `/docs`.

_(Sono inoltre presenti issue precedenti a #45 per FASE 1: MainWindow, NavigationService, Design System, ecc., che non sono riportate qui per brevità.)_

### 4.4 Sincronizzazione & DevOps

- #1 – Audit & log sync
- #2 – Crittografia payload
- #3 – Gestione chiavi
- #5 – CI di sicurezza.

---

## 5. Issue Mancanti (gap emersi dall'analisi)

Durante la lettura combinata di `docs/` e delle issue aperte emergono alcune aree non ancora coperte da issue specifiche. La issue meta #76 è stata creata per **censire e aprire tutte le issue mancanti**; qui ne elenchiamo i cluster principali, che andranno trasformati in issue atomiche.

### 5.1 Domain & Data

- Modellazione/reportistica per **statistiche aggregate** (es. numero visite per educatore, andamento mensile, breakdown per stato progetto)
- Estensioni del modello per **tagging/categorizzazione** dei progetti (es. tipologia intervento) se previsti nel dominio reale
- Miglioramento supporto a **audit trail avanzato** (filtri per periodo, operatore, paziente, tipo operazione) oltre a quanto descritto in #1.

### 5.2 Services & Orchestrazione

- Servizi per **reporting e dashboard** (es. `IReportingService` per preparare i KPI della Dashboard)
- Servizi di **notifica interna** (es. reminder visite imminenti, segnalazione progetti in scadenza)
- Orchestrazioni tra **sync service** e UI (progress state, rollback parziali, retry guidati).

### 5.3 UI & UX

- Componenti UI riusabili:
  - controlli per **date/time** coerenti su tutte le view
  - componenti per **card KPI** e **badge di stato**
  - componenti di **loading/skeleton** per ridurre percezione di latenza.
- Miglioramento **accessibilità** (FASE 4): navigazione tastiera completa, screen reader, high-contrast theme.
- Micro-interazioni di **feedback** (es. animazioni leggere per success/error, highlight di righe/record aggiornati dopo operazioni di sync o salvataggio).

### 5.4 Sincronizzazione & Diagnostica

- Issue dedicate a:
  - compatibilità tra versioni di protocollo (`protocol_version` nei pacchetti)
  - strumenti di diagnostica: visualizzazione dettagli pacchetto, log di import/esportazione, test tool interno per validare un file `.ptrp`
  - scenari di **disallineamento parziale** (pacchetti persi, doppio import, ecc.).

### 5.5 Security & Compliance (oltre le issue esistenti)

- Hardening specifico per **posture offline-first** (es. gestione sicura di file temporanei, log cifrati)
- Eventuale integrazione con **policy di retention** dei dati clinici (che possono interagire con GDPR e requisiti legali locali)
- Strumenti per **verifica periodica della configurazione di sicurezza** (self-check o checklist supportata dall'app).

### 5.6 Observability, Telemetry, DevEx

- Logging strutturato con correlazione tra operazioni utente e sync
- Metriche interne (es. numero sync/giorno, tempo medio operazioni critiche)
- Script/tooling per sviluppatori: seed avanzati, reset ambiente, generazione pacchetti di test.

Questi gruppi saranno tradotti in issue specifiche nel contesto della issue meta #76, con etichette e priorità adeguate.

---

## 6. Roadmap di Evoluzione

La roadmap proposta si basa sulle FASI già delineate (0–4) e sulle issue esistenti, integrate con i gap di cui sopra.

### 6.1 FASE 0 – Data Foundation

Obiettivo: avere un modello dati stabile e coerente con il dominio.

- ✅ Completare modelli dominio (issue #71 - **COMPLETATA**)
- Implementare repositories progetti/appuntamenti/visite (#72)
- Seeding realista per sviluppo (#13)
- Migrations/versioning schema DB (#14)
- Audit trail base (#1).

### 6.2 FASE 1 – Shell, Navigazione, Design System

Obiettivo: fornire un contenitore UI consistente su cui innestare le funzionalità.

- MainWindow con sidebar, top bar, snackbar
- NavigationService (#46) e integrazione View/ViewModel
- Design System (Colors, Typography, Styles)
- Schermata Primo Avvio (import chiavi/pacchetti iniziali).

### 6.3 FASE 2A – Pazienti & Progetti

Obiettivo: gestire anagrafiche pazienti e progetti terapeutici end-to-end.

- `PatientListView` master-detail (#51)
- `PatientFormView` CRUD
- `ProjectFormView` + integrazione in Patient detail (#74)
- `IPatientService` + `ITherapyProjectService` (#73)
- Regole di business (unicità progetto attivo, generazione appuntamenti canonici).

### 6.4 FASE 2B – Calendario & Visite

Obiettivo: fornire calendario operativo per coordinatore ed educatori, e registrazione visite.

- `CalendarView` + `VisitFormView` (#75)
  - Calendario con **codice colore per stato progetto** (Active/Suspended/Deceased/Completed)
  - Filtri per educatore, tipo appuntamento, stato progetto
- `IScheduledVisitRepository`, `IActualVisitRepository` (#72)
- Validazioni visite, enforce relazione 1:1 Scheduled↔Actual (#71)
- Supporto a filtri per educatore, tipo appuntamento, stato progetto.

### 6.5 FASE 3 – Sync & Conflict Resolution

Obiettivo: completare il ciclo di sincronizzazione sicuro coordinatore↔educatori.

- `SyncView` (Export/Import) (#52)
- `ConflictResolutionView` (#53)
- Crittografia DB e payload (#2)
- Gestione chiavi (#3)
- Audit/log sync (#1)
- Strumenti diagnostici (nuove issue da creare sotto #76).

### 6.6 FASE 4 – Polish, Perf, Deploy

Obiettivo: rendere il sistema robusto, usabile e pronto a deployment controllato.

- Validation UI & Error Handling globale (#54)
- Ottimizzazione performance e caching (#16)
- CI sicurezza (#5)
- GDPRComplianceService (#6)
- Accessibilità e UX polishing (nuove issue)
- Deploy con Velopack e pipeline di rilascio.

---

## 7. Meta-issue per il tracciamento delle issue mancanti

Per rendere questa analisi "viva" e collegata allo stato del repository GitHub è stata creata la issue meta:
- **#76 – Meta: censire ed aprire tutte le issue mancanti emerse dall'analisi completa di PTRP**
  - funge da contenitore per tutte le nuove issue che nasceranno dai gap elencati in §5
  - garantirà che ogni flusso utente definito in `docs/USER-WORKFLOW.md` abbia copertura completa di issue (domain, data, services, UI, sync, security)
  - permetterà di leggere la roadmap futura direttamente tramite GitHub Issues senza dover ricostruire l'analisi manualmente.

Questo documento andrà aggiornato man mano che:
- nuove issue vengono create e chiuse
- la documentazione in `/docs` viene revisionata (#68)
- vengono introdotte nuove esigenze di dominio o requisiti normativi.

---

## Riferimenti

- [USER-WORKFLOW.md](USER-WORKFLOW.md) - Flussi utente dettagliati (source of truth)
- [ARCHITECTURE.md](ARCHITECTURE.md) - Pattern MVVM e modelli di dominio
- [PROGETTO_PTRP_SYNC.md](PROGETTO_PTRP_SYNC.md) - Architettura sincronizzazione
- [DATABASE.md](DATABASE.md) - Schema database SQLite completo
- [SECURITY.md](SECURITY.md) - Modello di sicurezza e GDPR
- [DEVELOPMENT.md](DEVELOPMENT.md) - Guida sviluppatori

---

**Documento aggiornato:** 02 Febbraio 2026  
**Versione:** 1.1 (Allineato con USER-WORKFLOW.md)  
**Autore:** Marco Cavallo (@artcava)
