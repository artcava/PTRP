# Analisi Completa del Progetto PTRP

Questo documento raccoglie l'analisi strutturale e funzionale del progetto PTRP, con particolare attenzione a:
- dominio applicativo (Progetti Terapeutici, Pazienti, Educatori Professionali)
- architettura MVVM e architettura UI
- stato attuale del codice e delle issue
- issue esistenti e mancanti (con riferimento alla meta-issue #76)
- roadmap di evoluzione.

**🔄 Ultimo aggiornamento:** 02 Febbraio 2026 (post-merge issue #85 - modelli dominio visite)

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
- **`docs/USER-WORKFLOW.md`** - **SOURCE OF TRUTH** per flussi Coordinatore/Educatore, regole business ed enumerazioni
- `docs/DATABASE.md` - schema SQLite e relazioni
- `docs/ARCHITECTURE.md` - architettura MVVM e modelli dominio
- `docs/PROGETTO_PTRP_SYNC.md` - analisi tecnica sincronizzazione
- `docs/SECURITY.md` - modello di sicurezza e compliance GDPR.

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

**✅ Completate e testate (issue #71, #85):**
- **PatientModel**: anagrafica paziente
- **TherapyProjectModel**: progetto terapeutico associato a un paziente
  - stato clinico (Active, Suspended, Completed, Deceased) è proprietà del progetto, non del paziente
  - associazione N:N con ProfessionalEducator tramite `ProjectOperatorModel`
  - **Vincolo critico:** UN SOLO progetto `Active` per paziente (validato in `TherapyProjectService`)
- **ProfessionalEducatorModel**: operatore che partecipa ai progetti e alle visite
- **VisitTypeModel**: tipologie visite canoniche (Intake, Intermediate, Final, Discharge, ExtraVisit)
- **ScheduledVisitModel**: appuntamento programmato
  - tipologia via FK a `VisitTypeModel`
  - stato appuntamento (`AppointmentStatus`: Scheduled, Completed, Missed, Rescheduled)
  - legato a TherapyProject
- **ActualVisitModel**: visita effettiva
  - **relazione 1:1 con ScheduledVisit** (vincolo UNIQUE su `ScheduledVisitId`)
  - contiene contenuto clinico (`ClinicalNotes` obbligatorio), presenza paziente (`PatientAttendance`), operatori presenti
  - source tracking (`VisitSource`: EducatorImport | CoordinatorDirect)
- **VisitOperatorModel**: entità di join Many-to-Many tra ActualVisit ed Educator
  - ruolo nella visita (Lead, Assistant, Observer)
  - vincolo UNIQUE `(ActualVisitId, OperatorId)`
- **ProjectOperatorModel**: entità di join Many-to-Many tra TherapyProject ed Educator
  - ruolo nel progetto (Coordinator, Assistant, Consultant)
  - date assegnazione/rimozione

**🚧 In sviluppo/pianificate:**
- Audit / SyncLog (issue #1): tracciamento azioni e sincronizzazioni
- Notification (issue #81): notifiche interne per reminder visite
- SyncOperation (issue #82): orchestrazione operazioni sync con retry/rollback
- ProjectTag (issue #79): tagging/categorizzazione progetti (opzionale)

---

## 2. Architettura MVVM e UI

L'applicazione adotta un'architettura **MVVM** con separazione chiara tra:
- `PTRP.Models` – modelli di dominio ✅ **COMPLETI** per pazienti, progetti, visite (#71, #85)
- `PTRP.Data` – accesso dati (DbContext, repositories) 🚧 **IN SVILUPPO** (#72)
- `PTRP.Services` – servizi di dominio/applicativi 🚧 **IN SVILUPPO** (#73, #78, #80-82)
- `PTRP.ViewModels` – ViewModel MVVM 📅 **PIANIFICATI** (FASE 2)
- `PTRP.App` – applicazione WPF (Views, Styles, Behaviors, Converters) 📅 **PIANIFICATI** (FASE 1-2).

La struttura UI e i flussi principali sono documentati in:
- `docs/ARCHITECTURE.md` - ✅ **AGGIORNATO** con modelli visite (02/02/2026)
- issue #55 (Architettura UI completa)
- `docs/USER-WORKFLOW.md` - ✅ **SOURCE OF TRUTH** per workflow e regole business.

### 2.1 Shell e Navigazione

- `MainWindow.xaml` funge da **Shell** con:
  - top bar (titolo app, utente, impostazioni)
  - sidebar di navigazione (Dashboard, Pazienti, Progetti, Educatori, Calendario, Sync, Report, Impostazioni)
  - content area con `Frame` o equivalente per caricare le view
  - snackbar centrale per feedback (success/error/warning).
- Un `INavigationService` (issue #46) gestisce la navigazione tra Views, evitando logica di routing nelle ViewModels.

### 2.2 Views e ViewModels principali

Macro-moduli UI (da issue #55, #51, #71–#75):

- **Dashboard** 📅 PIANIFICATO (FASE 2)
  - `DashboardView` / `DashboardViewModel` (issue #50)
  - KPI cards (pazienti totali, progetti attivi, educatori operativi, visite completate)
  - grafici di andamento visite e Top Educators
  - richiede `IReportingService` (issue #78).

- **Pazienti** 📅 PIANIFICATO (FASE 2A)
  - `PatientListView` / `PatientListViewModel` (master-detail) (issue #51)
  - `PatientFormView` per CRUD paziente
  - pannello destro con anagrafica, progetto attivo, progetti completati, prossimo appuntamento
  - integrazione con `ITherapyProjectService` (issue #73) e servizi pazienti.

- **Progetti Terapeutici** 📅 PIANIFICATO (FASE 2A)
  - `ProjectFormView` / `ProjectFormViewModel` (issue #74)
  - integrata in `PatientListView` (detail panel): creazione nuovo progetto, change state, visualizzazione storico
  - generazione automatica degli appuntamenti canonici attraverso `ITherapyProjectService` (#73).

- **Educatori** 📅 PIANIFICATO (FASE 2)
  - `EducatorListView` + `EducatorFormView` (documentata in #55, implementazione da completare)
  - gestione anagrafiche educatori, specializzazioni, assegnazioni.

- **Calendario e Visite** 📅 PIANIFICATO (FASE 2B)
  - `CalendarView` / `CalendarViewModel` (issue #75)
    - calendario mensile con badge per giorni con appuntamenti
    - filtri per tipo appuntamento, educatore, stato progetto
    - lista appuntamenti del giorno selezionato con azioni rapide
  - `VisitFormView` / `VisitFormViewModel` (issue #75)
    - registrazione visita a partire da `ScheduledVisit`
    - vincoli di validazione (date/ore, operatori presenti, note cliniche)

- **Sincronizzazione** 📅 PIANIFICATO (FASE 3)
  - `SyncView` con tab Export/Import (issue #52)
  - `ConflictResolutionView` (issue #53) per gestione conflitti durante import
  - Orchestrazione via `ISyncOrchestrationService` (issue #82).

- **Dialogs e infrastruttura UI** 📅 PIANIFICATO (FASE 1-2)
  - `ConfirmationDialog`, `ErrorDetailsDialog` (issue #54)
  - Behaviors di validazione e componenti condivisi (ValidationBehavior, converters, ecc.)
  - Componenti riutilizzabili (issue #83): DatePicker, TimePicker, KpiCard, StatusBadge, SearchBox.

### 2.3 Design System

Descritto in `Styles/*.xaml` e issue #55:
- palette colori primari/secondari/semantic (success/warning/error/info)
- tipografia e spacing system
- stili per pulsanti, DataGrid, card KPI, layout responsive
- focus su **accessibilità** (contrasto, navigazione tastiera, focus visibile – issue #84 FASE 4).

---

## 3. Stato Attuale del Codice e delle Issue

### ✅ 3.1 Completato (al 02/02/2026)

**Issue chiuse/mergiate:**
- **#85 (feat/domain-models-visits)** - Completamento modelli dominio per appuntamenti e visite
  - Implementati: `VisitTypeModel`, `ScheduledVisitModel`, `ActualVisitModel`, `VisitOperatorModel`
  - Enumerazioni: `AppointmentStatus`, `VisitSource`, `PatientAttendance`
  - Configurazioni EF Core complete (DbSets, Fluent API, indici)
  - Test completi per tutti i modelli visite
  - **Vincolo critico 1:1 ScheduledVisit → ActualVisit** implementato e testato
  - Documentazione aggiornata in `ARCHITECTURE.md`, `DATABASE.md`, `USER-WORKFLOW.md`

**Documenti aggiornati:**
- ✅ `docs/USER-WORKFLOW.md` - Source of Truth per enumerazioni e regole business
- ✅ `docs/DATABASE.md` - Schema completo con vincoli relazionali 1:1
- ✅ `docs/ARCHITECTURE.md` - Modelli dominio completi con ER diagram aggiornato

### 🚧 3.2 In Sviluppo

**Issue aperte prioritarie (FASE 1-2):**

Nessuna issue attualmente in sviluppo attivo. Le prossime priorità sono:

**FASE 1 - Foundation & UI Shell:**
- #46 - NavigationService
- #47 - Design System (Styles/Controls.xaml)
- #48 - MainWindow Shell
- #49 - LoginView e gestione sessione

**FASE 2A - Domain Services & Repositories:**
- **#72 - Repositories per progetti, appuntamenti e visite** (PROSSIMA PRIORITÀ)
- **#73 - PatientService e TherapyProjectService** (BLOCCANTE per UI)
- #13 - Data seeding per sviluppo
- #14 - Migrations e versioning DB

**FASE 2B - Core UI:**
- #50 - Dashboard Coordinatore
- #51 - PatientListView master-detail
- #74 - ProjectFormView
- #75 - CalendarView e VisitFormView

### 📅 3.3 Pianificate (FASE 3-4)

**Sincronizzazione:**
- #52 - UI Sincronizzazione (Export/Import)
- #53 - Conflict Resolution UI
- #82 - SyncOrchestrationService (retry, rollback, progress)

**Security & Privacy:**
- #1 - Audit trail per entità cliniche e log sync
- #2 - AES-256 per DB SQLite e payload sync
- #3 - Gestione chiavi crittografiche
- #4 - AuthorizationService e RBAC
- #5 - Workflow CI sicurezza
- #6 - GDPRComplianceService

**Services avanzati:**
- #78 - ReportingService per KPI Dashboard
- #80 - AuditQueryService per filtri avanzati audit trail
- #81 - NotificationService per reminder interni

**UI & UX Polish:**
- #54 - Validation UI e Error Handling globale
- #83 - Componenti UI riutilizzabili (DatePicker, KpiCard, StatusBadge, SearchBox)
- #84 - Accessibilità (keyboard navigation, screen reader, high-contrast)

**Data Extensions (opzionali):**
- #79 - Tagging/categorizzazione progetti terapeutici

**Performance & DevOps:**
- #15 - Error handling e logging per operazioni DB
- #16 - Ottimizzazione query e caching

---

## 4. Issue Mancanti (gap emersi dall'analisi)

La issue meta **#76** traccia l'apertura di tutte le issue mancanti. I cluster principali già aperti:

### ✅ 4.1 Già aperti (issues #78-84)

- #78 - ReportingService per KPI e statistiche Dashboard
- #79 - Estensione modello per tagging/categorizzazione progetti
- #80 - AuditQueryService per filtri avanzati su audit trail
- #81 - NotificationService per reminder e segnalazioni interne
- #82 - SyncOrchestrationService per gestione state e retry sync
- #83 - Componenti UI riutilizzabili (date/time, KPI cards, badge stato)
- #84 - Accessibilità (keyboard, contrasto, screen reader) e micro-interazioni

### 📅 4.2 Da aprire (identificati ma non ancora creati)

**Sincronizzazione & Diagnostica:**
- Compatibilità versione protocollo sync (`protocol_version` nei pacchetti)
- Strumenti diagnostica pacchetti: visualizzazione dettagli, log import/export, test tool validazione `.ptrp`
- Scenari disallineamento parziale (pacchetti persi, doppio import, recovery)

**Security & Compliance (estensioni):**
- Hardening posture offline-first (gestione file temporanei, log cifrati)
- Policy retention dati clinici (interazione GDPR e requisiti legali)
- Self-check configurazione sicurezza

**Observability & DevEx:**
- Logging strutturato con correlazione operazioni/sync
- Metriche interne (sync/giorno, tempo operazioni critiche)
- Tooling sviluppatori: seed avanzati, reset ambiente, generazione pacchetti test

---

## 5. Roadmap di Evoluzione

La roadmap si basa sulle FASI già delineate (0–4) e sulle issue esistenti.

### ✅ 5.1 FASE 0 – Data Foundation (COMPLETATA)

Obiettivo: avere un modello dati stabile e coerente con il dominio.

**Completato:**
- ✅ Modelli dominio completi (issue #71, #85)
  - Patient, TherapyProject, ProfessionalEducator
  - VisitType, ScheduledVisit, ActualVisit, VisitOperator
  - ProjectOperator (N:N)
  - Enumerazioni complete: `ProjectStatus`, `AppointmentStatus`, `VisitSource`, `PatientAttendance`
- ✅ DbContext configurato con DbSets e Fluent API
- ✅ Test completi per tutti i modelli
- ✅ Documentazione allineata (DATABASE.md, ARCHITECTURE.md, USER-WORKFLOW.md)

**Prossimi step:**
- 🚧 Implementare repositories (#72)
- 🚧 Seeding realista per sviluppo (#13)
- 🚧 Migrations/versioning schema DB (#14)
- 📅 Audit trail base (#1)

### 🚧 5.2 FASE 1 – Shell, Navigazione, Design System

Obiettivo: fornire un contenitore UI consistente su cui innestare le funzionalità.

**Issue coinvolte:**
- #46 - NavigationService
- #47 - Design System (Colors, Typography, Styles)
- #48 - MainWindow Shell (sidebar, top bar, snackbar)
- #49 - LoginView e gestione sessione
- #54 - Validation framework e error handling globale

**Deliverable:**
- Shell WPF funzionante con navigazione sidebar
- Design system completo e applicato
- Infrastruttura validazione e messaging errori

### 📅 5.3 FASE 2A – Pazienti & Progetti

Obiettivo: gestire anagrafiche pazienti e progetti terapeutici end-to-end.

**Issue coinvolte:**
- #72 - Repositories (Patient, TherapyProject, ScheduledVisit, ActualVisit)
- #73 - PatientService + TherapyProjectService
- #51 - PatientListView master-detail
- #74 - ProjectFormView + integrazione Patient detail
- #13 - Data seeding

**Regole business critiche:**
- ✅ Modello: Vincolo UN SOLO progetto `Active` per paziente (validato in servizio)
- 📅 Servizio: Generazione automatica 4 appuntamenti canonici alla creazione progetto
- 📅 UI: Validazione form, gestione stati, feedback utente

### 📅 5.4 FASE 2B – Calendario & Visite

Obiettivo: fornire calendario operativo per coordinatore ed educatori, e registrazione visite.

**Issue coinvolte:**
- #75 - CalendarView + VisitFormView
- #72 - Repositories visite (già in FASE 2A)
- #83 - Componenti UI riutilizzabili (DatePicker, TimePicker, StatusBadge)

**Regole business critiche:**
- ✅ Modello: Relazione 1:1 ScheduledVisit ↔ ActualVisit (UNIQUE constraint)
- 📅 Servizio: Validazioni date/ore, operatori presenti, note cliniche obbligatorie
- 📅 UI: Calendario mensile con filtri, form registrazione visita con validazioni

### 📅 5.5 FASE 3 – Sync & Conflict Resolution

Obiettivo: completare il ciclo di sincronizzazione sicuro coordinatore↔educatori.

**Issue coinvolte:**
- #52 - SyncView (Export/Import)
- #53 - ConflictResolutionView
- #82 - SyncOrchestrationService (retry, rollback, progress)
- #2 - Crittografia DB e payload
- #3 - Gestione chiavi
- #1 - Audit/log sync

**Deliverable:**
- Pacchetti `.ptrp` cifrati e firmati
- UI export/import con progress e gestione conflitti
- Diagnostica e troubleshooting pacchetti

### 📅 5.6 FASE 4 – Polish, Perf, Deploy

Obiettivo: rendere il sistema robusto, usabile e pronto a deployment controllato.

**Issue coinvolte:**
- #50 - Dashboard con ReportingService (#78)
- #81 - NotificationService per reminder
- #80 - AuditQueryService per analisi audit
- #84 - Accessibilità e micro-interazioni
- #16 - Performance e caching
- #5 - CI sicurezza
- #6 - GDPRComplianceService

**Deliverable:**
- Dashboard completa con KPI e grafici
- Sistema notifiche interne
- Accessibilità completa (WCAG)
- Performance ottimizzate
- Deployment con Velopack

---

## 6. Metriche di Avanzamento

### 6.1 Stato Issue (al 02/02/2026)

**Totale issue rilevanti:** ~50+ (tra esistenti e da creare)

**Issue chiuse/completate:**
- #85 - Modelli dominio visite ✅
- #71 - Modelli dominio progetti (riferimento) ✅

**Issue aperte (priorità ALTA):**
- #72 - Repositories (PROSSIMA)
- #73 - Services (PROSSIMA)
- #46-49 - FASE 1 Foundation
- #50-51, #74-75 - FASE 2 Core UI

**Issue aperte (priorità MEDIA/BASSA):**
- #52-53 - FASE 3 Sync
- #78-84 - Services avanzati e UX polish
- #1-6 - Security & Privacy

### 6.2 Coverage Documentazione

**Documenti completi e aggiornati:**
- ✅ `docs/USER-WORKFLOW.md` - Source of Truth
- ✅ `docs/DATABASE.md` - Schema completo
- ✅ `docs/ARCHITECTURE.md` - Modelli e architettura
- ✅ `docs/PROGETTO_PTRP_SYNC.md` - Protocollo sync
- ✅ `docs/SECURITY.md` - Sicurezza e GDPR

**Documenti da aggiornare:**
- 🚧 `docs/DEVELOPMENT.md` - Procedure sviluppo (da allineare con workflow corrente)
- 🚧 `docs/TESTING.md` - Strategia testing (da aggiornare con best practices recenti)

### 6.3 Test Coverage

**PTRP.Models:**
- ✅ PatientModel: test completi
- ✅ TherapyProjectModel: test completi
- ✅ ProfessionalEducatorModel: test completi
- ✅ ScheduledVisitModel: test completi (issue #85)
- ✅ ActualVisitModel: test completi con validazioni (issue #85)
- ✅ VisitOperatorModel: test relazioni N:N (issue #85)

**PTRP.Data:**
- 📅 Repository tests (dipende da issue #72)

**PTRP.Services:**
- 📅 Service tests (dipende da issue #73)

**Target coverage:** ≥80% per servizi business, ≥70% per repositories.

---

## 7. Conclusioni e Prossimi Step Immediati

### ✅ Successi Recenti

1. **Modelli dominio completi** - Tutta la foundation dati per pazienti, progetti e visite è implementata e testata
2. **Vincoli critici implementati** - Relazione 1:1 ScheduledVisit↔ActualVisit, UNIQUE constraints, enumerazioni type-safe
3. **Documentazione allineata** - USER-WORKFLOW.md come source of truth, DATABASE.md e ARCHITECTURE.md aggiornati
4. **Test coverage solido** - Tutti i modelli dominio hanno test completi con validazioni

### 🎯 Prossimi Step (Priorità Immediata)

**Settimana corrente:**
1. **Issue #72** - Implementare repositories per Patient, TherapyProject, ScheduledVisit, ActualVisit
   - Pattern repository con interfacce
   - Query ottimizzate con Include per navigation properties
   - Test integration con DbContext in-memory

2. **Issue #73** - Implementare PatientService e TherapyProjectService
   - Validazioni business (es. UN SOLO progetto Active)
   - Generazione automatica 4 appuntamenti canonici
   - Gestione transazioni e rollback

**Prossime 2 settimane:**
3. **Issue #13** - Data seeding realistico per sviluppo
4. **Issue #14** - Setup migrations EF Core
5. **Issue #46-49** - FASE 1 Foundation (NavigationService, MainWindow Shell, Design System)

### 📊 Metriche Target Q1 2026

- ✅ FASE 0 completata (100%)
- 🎯 FASE 1 completata (80%)
- 🎯 FASE 2A completata (60%) - Pazienti & Progetti funzionanti end-to-end
- 🎯 FASE 2B avviata (30%) - Calendario in sviluppo
- 📅 FASE 3 pianificata - Sync per Q2 2026

---

## 8. Meta-issue per il tracciamento delle issue mancanti

Per rendere questa analisi "viva" e collegata allo stato del repository GitHub:

**Issue meta attiva:**
- **#76 – Meta: censire ed aprire tutte le issue mancanti emerse dall'analisi completa di PTRP**
  - Traccia creazione nuove issue dai gap identificati
  - Garantisce copertura completa di ogni flusso utente in `docs/USER-WORKFLOW.md`
  - Permette lettura roadmap direttamente tramite GitHub Issues

**Issue derivate già create da #76:**
- #78 - ReportingService
- #79 - Tagging progetti
- #80 - AuditQueryService
- #81 - NotificationService
- #82 - SyncOrchestrationService
- #83 - Componenti UI riutilizzabili
- #84 - Accessibilità e micro-interazioni

**Issue ancora da creare:**
- Diagnostica pacchetti sync
- Tooling sviluppatori
- Logging strutturato
- Metriche performance

---

**Questo documento viene aggiornato a ogni merge significativo o completamento di FASE.**

**Versione:** 2.1  
**Ultimo aggiornamento:** 02 Febbraio 2026  
**Branch reference:** develop (post-merge #85)  
**Prossimo aggiornamento previsto:** Completamento issue #72 (Repositories)
