# USER-WORKFLOW.md - Flusso dei Dati Utente PTRP

## 📋 Panoramica

Questo documento descrive il **flusso dei dati dal punto di vista dell'utente applicativo** per il sistema PTRP. L'applicazione gestisce Pazienti, Progetti Terapeutici Riabilitativi Personalizzati, Educatori Professionali, Appuntamenti e Visite.

### Profili Utente

L'applicazione supporta **due profili utente** con permessi differenziati:

1. **Coordinatore**
   - Gestione completa anagrafiche pazienti
   - Gestione anagrafica educatori professionali
   - Creazione e assegnazione progetti terapeutici
   - Assegnazione educatori ai progetti
   - Visualizzazione globale di tutti i dati
   - Esportazione appuntamenti per educatori

2. **Educatore Professionale**
   - Visualizzazione pazienti e progetti assegnati
   - Registrazione visite a partire dagli appuntamenti
   - Importazione appuntamenti dal Coordinatore
   - Esportazione visite registrate
   - Accesso limitato ai soli dati di competenza

---

## 🔐 RICONOSCIMENTO PROFILO UTENTE

### Come il Sistema Riconosce il Tuo Ruolo

**Principio:** Il profilo utente viene configurato automaticamente al primo avvio dell'applicazione attraverso l'importazione di un file di configurazione specifico.

#### 📦 Due Tipi di File di Configurazione

##### 1️⃣ File per il Coordinatore (admin.ptrp)

**Chi lo fornisce:**
- File speciale consegnato durante l'installazione iniziale del sistema
- Fornito dall'amministratore di sistema

**Cosa contiene:**
- Profilo di Coordinatore con permessi completi
- Database vuoto pronto per iniziare il lavoro

**Come si usa:**
1. Installi l'applicazione sul tuo PC
2. Al primo avvio, il sistema chiede di importare un file di configurazione
3. Selezioni il file `admin.ptrp` che ti è stato consegnato
4. Il sistema riconosce automaticamente che sei il Coordinatore
5. L'applicazione è pronta con tutti i permessi attivi

---

##### 2️⃣ File per gli Educatori (appointments_*.ptrp)

**Formato del nome file:**
- `appointments_cognome_data.ptrp`
- Esempio: `appointments_rossi_20260401.ptrp`
- La data nel nome aiuta a verificare che il file sia aggiornato

**Chi lo crea:**
- Il Coordinatore genera questo file per ogni educatore

**Cosa contiene:**
- I tuoi appuntamenti programmati del periodo
- Dati dei pazienti coinvolti
- Informazioni sui progetti terapeutici
- Elenco di tutti gli educatori dei progetti (necessario per registrare le visite)

**Come si usa:**
1. Installi l'applicazione sul tuo PC
2. Al primo avvio, il sistema chiede di importare un file
3. Selezioni il file che hai ricevuto dal Coordinatore (USB o email)
4. Il sistema legge il file e riconosce automaticamente che sei un Educatore
5. Vengono importati i tuoi appuntamenti e i dati necessari
6. L'applicazione è configurata con permessi educatore

---

#### 🔄 Flusso Completo di Configurazione

**Scenario A: Configurazione Coordinatore**
```
Amministratore sistema consegna file admin.ptrp
      ↓
Coordinatore installa applicazione
      ↓
Al primo avvio: "Importa file di configurazione"
      ↓
Selezione admin.ptrp
      ↓
Sistema riconosce ruolo Coordinatore
      ↓
Applicazione pronta con permessi completi
```

**Scenario B: Configurazione Educatore**
```
Coordinatore inserisce educatore Rossi in anagrafica
      ↓
Coordinatore assegna Rossi a progetti
      ↓
Coordinatore esporta appuntamenti per Rossi
      ↓
File generato: appointments_rossi_20260401.ptrp
      ↓
Coordinatore consegna file a Rossi (USB/Email)
      ↓
Rossi installa applicazione
      ↓
Al primo avvio: "Importa file dal Coordinatore"
      ↓
Rossi seleziona il file ricevuto
      ↓
Sistema riconosce ruolo Educatore
      ↓
Appuntamenti e dati importati
      ↓
Applicazione configurata per educatore
```

---

#### ⚠️ Controllo Aggiornamento File

**Problema:** Potresti ricevere un file non aggiornato

**Soluzione:** Il sistema controlla automaticamente la data del file

**Cosa succede se il file è vecchio:**
```
┌────────────────────────────────────────────┐
│ ⚠️  Attenzione: File Non Aggiornato        │
├────────────────────────────────────────────┤
│ Il file è datato 01/04/2026 ma hai        │
│ visite registrate fino al 05/04/2026.     │
│                                            │
│ Importare questo file potrebbe            │
│ sovrascrivere appuntamenti più recenti.   │
│                                            │
│ Contatta il Coordinatore per un file      │
│ aggiornato.                               │
│                                            │
│ [Annulla] [Importa Comunque]             │
└────────────────────────────────────────────┘
```

---

## 🏗️ Modello Dati Semplificato

### Regole di Business Critiche

1. **Unicità Progetto Attivo**: Un paziente può avere UN SOLO progetto terapeutico con stato "Active" contemporaneamente
2. **Stato sul Progetto**: Lo stato (Active, Suspended, Completed, Deceased) è applicato al Progetto Terapeutico, non al paziente
3. **Assegnazione Educatori**: Gli educatori sono assegnati al Progetto Terapeutico
4. **Appuntamenti Canonici**: Ogni progetto genera automaticamente 4 appuntamenti programmati:
   - Prima Apertura (INTAKE) - dopo 3 mesi
   - Verifica Intermedia - dopo 6 mesi dalla Prima Apertura
   - Verifica Finale - dopo 6 mesi dalla Verifica Intermedia  
   - Dimissioni - dopo 1 mese dalla Verifica Finale
5. **Vincolo Visita-Appuntamento**: Una Visita può essere creata SOLO a partire da un Appuntamento esistente

---

**Documento creato:** 30 Gennaio 2026  
**Versione:** 3.2 (Versione per Equipe PTRP - Solo Flussi Utente)  
**Autore:** Marco Cavallo (@artcava)  
**Ultime modifiche:**
- Rimossi tutti i dettagli tecnici (JSON, riferimenti tecnici)
- Focus esclusivo sui flussi operativi utente
- Linguaggio semplificato per equipe non tecnica
- Mantenuti solo i concetti necessari
