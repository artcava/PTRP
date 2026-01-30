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

### Soluzione: Profilo Derivato dall'Importazione Pacchetti

**Principio:** Il profilo utente viene configurato automaticamente al primo avvio dell'applicazione attraverso l'importazione di un pacchetto di configurazione specifico.

#### 📦 Due Tipi di Pacchetti

##### 1️⃣ admin.ptrp - Configurazione Coordinatore

**Creazione e Distribuzione:**
- File speciale fornito durante il deployment al Coordinatore
- Generato dallo sviluppatore del sistema
- Contiene profilo master e configurazione iniziale

**Contenuto Pacchetto:**
```json
{
  "package_type": "admin_bootstrap",
  "version": "1.0",
  "profile": {
    "role": "Coordinator",
    "first_name": "Nome",
    "last_name": "Coordinatore",
    "is_master": true
  },
  "initial_data": {
    "operators": [],
    "patients": [],
    "projects": []
  },
  "signature": "HASH_SICUREZZA",
  "created_at": "2026-01-30T17:00:00Z"
}
```

**Utilizzo:**
1. Coordinatore installa l'applicazione
2. Al primo avvio: sistema rileva assenza configurazione
3. Mostra schermata "Importa Configurazione Iniziale"
4. Coordinatore importa `admin.ptrp`
5. Sistema crea profilo Coordinatore e database vuoto
6. Applicazione pronta per l'uso con permessi completi

---

##### 2️⃣ appointments_{educatore}_{YYYYMMDD}.ptrp - Configurazione Educatore

**Naming Convention con Data Estesa:**
- Formato: `appointments_{cognome}_{data_esportazione}.ptrp`
- Esempio: `appointments_rossi_20260401.ptrp`
- **Rationale**: Data estesa permette verifica se pacchetto più recente delle visite registrate

**Contenuto Pacchetto (COMPLETO):**
```json
{
  "package_type": "appointments_sync",
  "version": "1.0",
  "export_date": "2026-04-01",
  "target_operator": {
    "id": "GUID-EDUCATORE",
    "first_name": "Mario",
    "last_name": "Rossi",
    "role": "Operator"
  },
  "appointments": [ /* 12 appuntamenti programmati */ ],
  "patients": [ /* dati 8 pazienti coinvolti */ ],
  "projects": [ /* dati progetti associati */ ],
  "operators": [ /* TUTTI gli educatori dei progetti */ ],
  "signature": "HASH_SICUREZZA",
  "created_at": "2026-04-01T12:00:00Z"
}
```

**Campo `operators` (IMPORTANTE):**
- Contiene TUTTI gli educatori assegnati ai progetti nel pacchetto
- Necessario per consentire spunta durante registrazione visite
- Esempio:
  ```json
  "operators": [
    {"id": "GUID-1", "first_name": "Mario", "last_name": "Rossi"},
    {"id": "GUID-2", "first_name": "Luigi", "last_name": "Bianchi"}
  ]
  ```

**Utilizzo:**
1. Educatore installa l'applicazione
2. Al primo avvio: sistema rileva assenza configurazione
3. Mostra schermata "Importa Pacchetto dal Coordinatore"
4. Educatore importa file ricevuto (es. `appointments_rossi_20260401.ptrp`)
5. Sistema legge `target_operator` → riconosce profilo Rossi
6. Crea profilo Educatore per Rossi nel database locale
7. Importa appuntamenti, pazienti, progetti, altri educatori
8. Applicazione configurata con permessi limitati

---

#### 🔄 Flusso Completo Setup

**Scenario A: Setup Coordinatore**
```
[Sviluppatore] Genera admin.ptrp
      ↓
[Coordinatore] Installa app su PC
      ↓
Primo avvio → "Importa admin.ptrp"
      ↓
Sistema crea profilo Coordinatore
      ↓
Coordinatore accede con permessi completi
```

**Scenario B: Setup Educatore**
```
[Coordinatore] Crea educatore "Rossi" in anagrafica
      ↓
[Coordinatore] Assegna Rossi a 3 progetti
      ↓
Sistema genera 12 appuntamenti per Rossi
      ↓
[Coordinatore] Esporta appuntamenti per Rossi
      ↓
Genera "appointments_rossi_20260401.ptrp"
      ↓
[Coordinatore] Consegna file a Rossi (USB/Email)
      ↓
[Rossi] Installa app su suo PC
      ↓
Primo avvio → "Importa pacchetto"
      ↓
Sistema legge target_operator: "Rossi"
      ↓
Crea profilo Educatore per Rossi
      ↓
Importa 12 appuntamenti + dati pazienti/progetti/educatori
      ↓
Rossi accede con permessi educatore
```

---

#### ⚠️ Verifica Data Pacchetto

**Problema:** Educatore potrebbe importare pacchetto obsoleto

**Soluzione:** Sistema controlla data nel nome file vs ultime visite registrate

**UI Warning:**
```
┌────────────────────────────────────────────┐
│ ⚠️  Attenzione: Pacchetto Obsoleto         │
├────────────────────────────────────────────┤
│ Il pacchetto è datato 01/04/2026 ma hai   │
│ visite registrate fino al 05/04/2026.     │
│                                            │
│ Continuare con l'importazione             │
│ sostituirà gli appuntamenti con dati      │
│ potenzialmente obsoleti.                  │
│                                            │
│ [Annulla] [Importa Comunque]             │
└────────────────────────────────────────────┘
```

---

#### ✅ Vantaggi Soluzione

1. **Coerenza Architetturale**: Stesso meccanismo import/export per tutto
2. **Zero Configurazione Manuale**: Importa file e sistema si auto-configura
3. **Sicurezza Intrinseca**: Firma criptografica impedisce manomissioni
4. **Controllo Centralizzato**: Coordinatore decide chi ha quale ruolo
5. **Verifica Automatica**: Sistema riconosce ruolo dal pacchetto
6. **Prevenzione Errori**: Educatore non può fingere di essere coordinatore

---

#### 🎨 UI Primo Avvio (Generico)

```
┌────────────────────────────────────────────────┐
│ PTRP - Configurazione Iniziale                 │
├────────────────────────────────────────────────┤
│                                                │
│  🔧 Questa istanza non è ancora configurata.   │
│                                                │
│  Importa un pacchetto di configurazione:      │
│                                                │
│  • admin.ptrp → per Coordinatore               │
│  • appointments_*.ptrp → per Educatore         │
│                                                │
│  [📁 Importa Pacchetto di Configurazione...]   │
│                                                │
│  ℹ️  Il file ti sarà fornito dal Coordinatore  │
│     o dall'amministratore di sistema.          │
│                                                │
└────────────────────────────────────────────────┘
```

---

## 🏗️ Modello Dati Semplificato

### Relazioni Fondamentali

```
Paziente (1) ←───── (1) Progetto Terapeutico Attivo
                          ↓ (ha stato: Active/Suspended/Completed)
                          ↓ (N:N)
                          ↓
                    Educatori Professionali
                          ↓
                          ↓ (1:N)
                          ↓
                    Appuntamenti (4 canonici)
                          ↓
                          ↓ (1:1 vincolo obbligatorio)
                          ↓
                    Visite Effettive
```

### Regole di Business Critiche

1. **Unicità Progetto Attivo**: Un paziente può avere **UN SOLO** progetto terapeutico con stato "Active" contemporaneamente
2. **Stato sul Progetto**: Lo stato (Active, Suspended, Completed, Deceased) è applicato al **Progetto Terapeutico**, non al paziente
3. **Assegnazione Educatori**: Gli educatori sono assegnati al **Progetto Terapeutico**, non direttamente al paziente
4. **Relazione Implicita**: Gli educatori di un paziente si desumono dal progetto attivo
5. **Appuntamenti Canonici**: Ogni progetto genera automaticamente 4 appuntamenti programmati:
   - Prima Apertura (INTAKE) - dopo 3 mesi dall'assegnazione
   - Verifica Intermedia - dopo 6 mesi dalla Prima Apertura
   - Verifica Finale - dopo 6 mesi dalla Verifica Intermedia
   - Dimissioni - dopo 1 mese dalla Verifica Finale
6. **Vincolo Visita-Appuntamento**: Una Visita può essere creata **SOLO** a partire da un Appuntamento esistente (relazione 1:1 obbligatoria)

### Terminologia

- **Appuntamento**: Incontro programmato nel calendario (stato: Scheduled, Completed, Missed, Rescheduled)
- **Visita**: Registrazione effettiva dell'incontro avvenuto con note cliniche, operatori presenti, esiti (sempre legata a un Appuntamento)

---

## 🔄 FLUSSO 1: Gestione Pazienti (Coordinatore)

### Scenario: Nuovo paziente assegnato in riunione d'equipe

#### Step 1: Visualizzazione Lista Pazienti

**Azione Utente:** Coordinatore apre l'applicazione

**UI Display:**
```
┌────────────────────────────────────────────────────────────┐
│ PTRP - Gestione Pazienti                    [Coordinatore] │
├────────────────────────────────────────────────────────────┤
│ [+ Nuovo Paziente]  [🔍 Ricerca: _________]  [⚙️ Filtri]  │
├────────────────────────────────────────────────────────────┤
│ Nome Cognome        │ Stato Progetto │ Educatori Assegnati    │
├────────────────────────────────────────────────────────────┤
│ ROSSI Mario         │ Active         │ Bianchi, Verdi         │
│ FERRARI Laura       │ Active         │ Neri                   │
│ COLOMBO Andrea      │ Suspended      │ Gialli, Viola          │
│ RUSSO Giovanni      │ Deceased       │ -                      │
│ ESPOSITO Maria      │ Active         │ Gialli, Viola          │
│ ...                                                        │
└────────────────────────────────────────────────────────────┘
```

**Comportamento:**
- La colonna "Stato Progetto" mostra lo stato del progetto attivo corrente (non del paziente)
- La colonna "Educatori Assegnati" mostra gli educatori del progetto attivo corrente
- Se il paziente non ha progetti attivi, mostra "-"
- Stati possibili del progetto: `Active`, `Suspended`, `Completed`, `Deceased`

---

#### Step 2: Creazione Nuovo Paziente

**Azione Utente:** Click su `[+ Nuovo Paziente]`

**UI Dialog:**
```
┌────────────────────────────────────────────┐
│ Nuovo Paziente                          [X] │
├────────────────────────────────────────────┤
│                                            │
│ Nome:     [_______________________________] │
│ Cognome:  [_______________________________] │
│                                            │
│           [Annulla]  [Salva]               │
└────────────────────────────────────────────┘
```

**Validazioni:**
- Nome e Cognome obbligatori
- Avviso se esiste paziente con stesso nome/cognome

**Risultato:**
- Paziente creato e visibile nella lista
- Nessun progetto ancora assegnato → colonna educatori vuota
- Sistema pronto per creazione progetto terapeutico

---

#### Step 3: Ricerca e Filtri

**Azione Utente:** Digitare nel box ricerca per trovare pazienti

**Comportamento UI:**
- Ricerca in tempo reale su Nome e Cognome
- Filtri per stato progetto (Active, Suspended, Completed, Deceased)
- Ricerca case-insensitive

---

## 👥 FLUSSO 1B: Gestione Educatori (Coordinatore)

### Scenario: Inserimento nuovo educatore nell'equipe

#### Step 3B: Lista Educatori

**Azione Utente:** Coordinatore naviga a sezione "Educatori"

**UI Display:**
```
┌────────────────────────────────────────────────────────────┐
│ PTRP - Gestione Educatori                  [Coordinatore] │
├────────────────────────────────────────────────────────────┤
│ [+ Nuovo Educatore]  [🔍 Ricerca: _________]             │
├────────────────────────────────────────────────────────────┤
│ Nome Cognome        │ Progetti Attivi │ Stato            │
├────────────────────────────────────────────────────────────┤
│ BIANCHI Marco       │ 8               │ Attivo           │
│ VERDI Luca          │ 6               │ Attivo           │
│ NERI Sara           │ 5               │ Attivo           │
│ GIALLI Paolo        │ 7               │ Attivo           │
│ VIOLA Anna          │ 4               │ Sospeso          │
│ ...                                                        │
└────────────────────────────────────────────────────────────┘
```

**Comportamento:**
- Mostra tutti gli educatori dell'equipe
- Conteggio progetti attivi per ciascun educatore
- Possibilità di aggiungere, modificare, sospendere educatori

---

## 🗂️ FLUSSO 2: Gestione Progetti Terapeutici (Coordinatore)

### Scenario: Apertura nuovo PTRP dopo 3 mesi di osservazione

#### Step 4: Visualizzazione Dettaglio Paziente

**Azione Utente:** Click su paziente nella lista

**UI Display:**
```
┌────────────────────────────────────────────────────────────┐
│ Paziente: ROSSI Mario                         [Modifica]   │
├────────────────────────────────────────────────────────────┤
│ [Anagrafica] [Progetti] [Storico Visite]                  │
├────────────────────────────────────────────────────────────┤
│ PROGETTI TERAPEUTICI                                       │
│                                                            │
│ ✅ Progetto Attivo (1)                                     │
│ ┌────────────────────────────────────────────────────┐    │
│ │ PTRP 2025-2027                                     │    │
│ │ Stato: Active                                     │    │
│ │ Periodo: 02/01/2025 - 02/01/2027                  │    │
│ │ Educatori: Bianchi, Verdi                         │    │
│ │ Prossimo Appuntamento: 02/04/2025 (Prima Apertura)│    │
│ │                                                    │    │
│ │ [Visualizza Dettagli] [Modifica Stato]           │    │
│ └────────────────────────────────────────────────────┘    │
│                                                            │
│ 📋 Progetti Completati (0)                                 │
│ Nessun progetto completato                                │
│                                                            │
│ [+ Nuovo Progetto Terapeutico]                            │
│                                                            │
│ ⚠️ Nota: Un paziente può avere un solo progetto attivo    │
└────────────────────────────────────────────────────────────┘
```

**Informazioni Visualizzate:**
- Titolo e **stato del progetto** (Active, Suspended, Completed, Deceased)
- Periodo di validità
- Educatori assegnati
- Prossimo appuntamento programmato

---

#### Step 5: Creazione Nuovo Progetto Terapeutico

**Azione Utente:** Click su `[+ Nuovo Progetto Terapeutico]`

**Validazione Pre-Creazione:**
- Sistema verifica che NON esista già un progetto con stato "Active" per questo paziente
- Se esiste, mostra messaggio: "Il paziente ha già un progetto attivo. Cambia lo stato del progetto corrente prima di crearne uno nuovo."

**UI Dialog:**
```
┌────────────────────────────────────────────────────────────┐
│ Nuovo Progetto Terapeutico - ROSSI Mario           [X]    │
├────────────────────────────────────────────────────────────┤
│                                                            │
│ Titolo Progetto:                                          │
│ [PTRP 2025-2027________________________________]          │
│                                                            │
│ Descrizione:                                              │
│ [________________________________________________         │
│  ________________________________________________         │
│  ________________________________________________]        │
│                                                            │
│ Data Inizio:     [📅 02/01/2025]                          │
│ Data Fine Prev.: [📅 02/01/2027]  (opzionale)             │
│                                                            │
│ Stato:           [▼ Active            ]                    │
│                     (Active, Suspended, Completed, Deceased)│
│                                                            │
│ ─────────────────────────────────────────────────────     │
│ EDUCATORI PROFESSIONALI ASSEGNATI                         │
│ ─────────────────────────────────────────────────────     │
│                                                            │
│ [+ Aggiungi Educatore]                                    │
│                                                            │
│ ┌────────────────────────────────────────────────────┐    │
│ │ Bianchi        [Rimuovi]                          │    │
│ │ Verdi          [Rimuovi]                          │    │
│ └────────────────────────────────────────────────────┘    │
│                                                            │
│ ⚠️ Almeno un educatore deve essere assegnato              │
│                                                            │
│ ─────────────────────────────────────────────────────     │
│                                                            │
│ [✓] Genera automaticamente appuntamenti programmati (4)   │
│     • Prima Apertura: +3 mesi (02/04/2025)                │
│     • Verifica Intermedia: +6 mesi (02/10/2025)           │
│     • Verifica Finale: +6 mesi (02/04/2026)               │
│     • Dimissioni: +1 mese (02/05/2026)                    │
│                                                            │
│                    [Annulla]  [Crea Progetto]             │
└────────────────────────────────────────────────────────────┘
```

**Validazioni:**
- Titolo obbligatorio
- Data inizio obbligatoria
- Data fine ≥ data inizio (se specificata)
- Almeno 1 educatore assegnato
- Controllo unicità progetto con stato "Active" per paziente

**Operazioni Eseguite dal Sistema:**

1. Crea il progetto terapeutico con stato selezionato
2. Assegna gli educatori al progetto
3. Genera automaticamente 4 appuntamenti programmati con le scadenze corrette

**Risultato:**
- Progetto creato e visibile nella scheda paziente
- Educatori ora visibili nella lista pazienti
- 4 appuntamenti automaticamente programmati nel calendario

---

## 📅 FLUSSO 3: Visualizzazione Calendario Appuntamenti

### Scenario: Coordinatore consulta appuntamenti programmati

#### Step 6: Calendario Appuntamenti Mensile

**Azione Utente:** Navigazione a sezione "Calendario"

**UI Display:**
```
┌────────────────────────────────────────────────────────────┐
│ Calendario Appuntamenti - Aprile 2025                     │
├────────────────────────────────────────────────────────────┤
│  L    M    M    G    V    S    D                          │
│       1    2🟢  3    4    5    6                          │
│  7    8    9   10   11   12   13                          │
│ 14   15   16   17🟡 18   19   20                          │
│ 21   22   23⚫ 24   25   26   27                          │
│ 28   29   30                                              │
│                                                            │
│ Legenda (Stato Progetto):                                 │
│ 🟢 Active (In corso)   🟡 Suspended (Sospeso)           │
│ ⚫ Deceased (Deceduto)  ⚪ Completed (Concluso)          │
│                                                            │
│ Filtri:                                                   │
│ [✓] Prima Apertura  [✓] Verifiche  [✓] Dimissioni        │
│ Educatore: [▼ Tutti               ]                       │
│ Stato Progetto: [▼ Tutti          ]                       │
└────────────────────────────────────────────────────────────┘

┌────────────────────────────────────────────────────────────┐
│ Appuntamenti del 02 Aprile 2025                           │
├────────────────────────────────────────────────────────────┤
│                                                            │
│ 🟢 Prima Apertura - ROSSI Mario                           │
│    Educatori: Bianchi, Verdi                              │
│    Progetto: PTRP 2025-2027 (Active)                      │
│    [Registra Visita] [Riprogramma] [Segna Mancato]       │
│                                                            │
│ 🟢 Prima Apertura - FERRARI Laura                         │
│    Educatore: Neri                                        │
│    Progetto: PTRP 2025-2027 (Active)                      │
│    [Registra Visita] [Riprogramma] [Segna Mancato]       │
│                                                            │
└────────────────────────────────────────────────────────────┘
```

**Funzionalità:**
- Visualizzazione calendario mensile con **codice colore per stato progetto** (non per tipo appuntamento)
- Lista giornaliera di appuntamenti con dettagli
- Filtri per tipo appuntamento, educatore, stato progetto
- Azioni rapide: Registra Visita, Riprogramma, Segna Mancato

---

## ✍️ FLUSSO 4: Registrazione Visita (Educatore)

### Scenario: Educatore registra visita dopo incontro con paziente

#### Step 7: Selezione Appuntamento da Registrare

**Azione Utente:** Educatore Bianchi accede alla lista "Miei Appuntamenti"

**UI Display (Vista Educatore):**
```
┌────────────────────────────────────────────────────────────┐
│ I Miei Appuntamenti                         [Educatore]   │
├────────────────────────────────────────────────────────────┤
│ Oggi: 02/04/2025                                          │
│                                                            │
│ 🟢 ROSSI Mario - Prima Apertura                           │
│    Ore: 10:00 (stimato 90 min)                           │
│    Co-educatore: Verdi                                    │
│    [✓ Registra Visita]                                    │
│                                                            │
│ ─────────────────────────────────────────────────────     │
│ Prossimi Appuntamenti (7 giorni)                          │
│                                                            │
│ 05/04/2025 - ESPOSITO Maria - Prima Apertura             │
│ 09/04/2025 - BRUNO Francesco - Prima Apertura            │
│ ...                                                        │
└────────────────────────────────────────────────────────────┘
```

**Comportamento:**
- Educatore vede solo gli appuntamenti dei propri progetti assegnati
- Sono mostrati appuntamenti di oggi e i prossimi 7 giorni
- Possibilità di aprire il modulo di registrazione visita

---

#### Step 8: Form Registrazione Visita

**Azione Utente:** Click su `[✓ Registra Visita]`

**⚠️ VINCOLO IMPORTANTE:** La visita può essere creata **SOLO** a partire da un appuntamento esistente.

**UI Dialog:**
```
┌────────────────────────────────────────────────────────────┐
│ Registrazione Visita                                [X]   │
├────────────────────────────────────────────────────────────┤
│                                                            │
│ Paziente: ROSSI Mario                                     │
│ Tipo Appuntamento: 🟢 Prima Apertura (INTAKE)             │
│ Data Programmata: 02/04/2025                              │
│ Appuntamento ID: #12345                                   │
│                                                            │
│ ─────────────────────────────────────────────────────     │
│ DATI VISITA EFFETTIVA                                     │
│ ─────────────────────────────────────────────────────     │
│                                                            │
│ Data Effettiva:  [📅 02/04/2025]                          │
│ Ora Inizio:      [🕐 10:00]                               │
│ Ora Fine:        [🕐 11:30]                               │
│                                                            │
│ ─────────────────────────────────────────────────────     │
│ OPERATORI PRESENTI                                        │
│ ─────────────────────────────────────────────────────     │
│                                                            │
│ [✓] Bianchi (io)                                          │
│ [✓] Verdi                                                 │
│                                                            │
│ ⚠️ Almeno un operatore deve essere selezionato            │
│                                                            │
│ ─────────────────────────────────────────────────────     │
│ NOTE CLINICHE (obbligatorio)                              │
│ ─────────────────────────────────────────────────────     │
│                                                            │
│ [________________________________________________         │
│  Il paziente si è presentato puntuale e collaborativo.    │
│  Durante il colloquio sono stati discussi gli obiettivi   │
│  del progetto terapeutico...                              │
│  ________________________________________________]        │
│                                                            │
│ ─────────────────────────────────────────────────────     │
│ ESITI E OBIETTIVI                                         │
│ ─────────────────────────────────────────────────────     │
│                                                            │
│ [________________________________________________         │
│  Obiettivo 1: Migliorare autonomia...                    │
│  ________________________________________________]        │
│                                                            │
│ ─────────────────────────────────────────────────────     │
│ PRESENZA PAZIENTE                                         │
│ ─────────────────────────────────────────────────────     │
│                                                            │
│ [▼ Presente e Collaborativo            ]                  │
│     • Presente e Collaborativo                            │
│     • Presente ma Non Collaborativo                       │
│     • Assente Giustificato                                │
│     • Assente Non Giustificato                            │
│                                                            │
│                    [Annulla]  [Salva Visita]              │
└────────────────────────────────────────────────────────────┘
```

**Validazioni:**
- **Vincolo obbligatorio**: La visita deve essere legata a un appuntamento esistente
- Data effettiva non può essere futura
- Ora fine deve essere successiva all'ora inizio
- Almeno un operatore deve essere selezionato
- Note cliniche obbligatorie
- Presenza paziente obbligatoria

**Operazioni Eseguite dal Sistema:**

1. Crea la registrazione della visita effettiva legata all'appuntamento (relazione 1:1)
2. Registra gli operatori presenti
3. Aggiorna lo stato dell'appuntamento a "Completed"
4. Salva nel database locale

**Risultato UI:**
```
┌────────────────────────────────────────────┐
│ ✅ Visita Registrata con Successo          │
│                                            │
│ La visita è stata salvata e collegata     │
│ all'appuntamento #12345.                  │
│                                            │
│ Ricorda di sincronizzare i dati con il    │
│ Coordinatore.                             │
│                                            │
│ [OK]  [Vai a Sincronizzazione]           │
└────────────────────────────────────────────┘
```

---

## 🔄 FLUSSO 5: Sincronizzazione Dati (Bidirezionale)

### Scenario A: Educatore sincronizza visite registrate con Coordinatore

#### Step 9: Esportazione Visite (Educatore → Coordinatore)

**Azione Utente:** Navigazione a sezione "Sincronizzazione"

**UI Display:**
```
┌────────────────────────────────────────────────────────────┐
│ Sincronizzazione Dati                       [Educatore]   │
├────────────────────────────────────────────────────────────┤
│ [Esporta Visite] [Importa Appuntamenti] [Storico]        │
├────────────────────────────────────────────────────────────┤
│                                                            │
│ VISITE DA SINCRONIZZARE                                   │
│                                                            │
│ Ultima sincronizzazione: 28/03/2025 18:30                │
│                                                            │
│ [✓] 5 Visite effettive registrate                         │
│     • ROSSI Mario - Prima Apertura (02/04)                │
│     • FERRARI Laura - Prima Apertura (02/04)              │
│     • ESPOSITO Maria - Prima Apertura (05/04)             │
│     • BRUNO Francesco - Prima Apertura (09/04)            │
│     • MARINO Elena - Verifica Intermedia (08/04)          │
│                                                            │
│ [✓] 12 Relazioni operatori-visite                         │
│                                                            │
│ ─────────────────────────────────────────────────────     │
│                                                            │
│ Destinatario: [▼ Coordinatore Principale  ]               │
│ Modalità:     [▼ File Criptato (.ptrp)   ]               │
│                                                            │
│ [Esporta Pacchetto Visite]                                │
│                                                            │
└────────────────────────────────────────────────────────────┘
```

**Risultato:**
```
┌────────────────────────────────────────────────────────────┐
│ ✅ Pacchetto Visite Creato                                │
├────────────────────────────────────────────────────────────┤
│ File: visits_bianchi_20250405_183000.ptrp                 │
│ Dimensione: 287 KB                                        │
│ Contiene: 5 visite, 12 relazioni operatori                │
│                                                            │
│ [💾 Salva su USB]  [📧 Invia Email]                       │
└────────────────────────────────────────────────────────────┘
```

---

### Scenario B: Coordinatore esporta appuntamenti per Educatore

#### Step 10: Esportazione Appuntamenti (Coordinatore → Educatore)

**🆕 NUOVO FLUSSO**

**Azione Utente:** Coordinatore naviga a sezione "Sincronizzazione"

**UI Display:**
```
┌────────────────────────────────────────────────────────────┐
│ Sincronizzazione Dati                     [Coordinatore]  │
├────────────────────────────────────────────────────────────┤
│ [Esporta Appuntamenti] [Importa Visite] [Storico]         │
├────────────────────────────────────────────────────────────┤
│                                                            │
│ ESPORTA APPUNTAMENTI PER EDUCATORE                        │
│                                                            │
│ Seleziona Educatore:                                      │
│ [▼ Bianchi Marco              ]                          │
│                                                            │
│ Periodo:                                                  │
│ Dal: [📅 01/04/2025]  Al: [📅 30/04/2025]              │
│                                                            │
│ ─────────────────────────────────────────────────────     │
│ ANTEPRIMA DATI                                            │
│ ─────────────────────────────────────────────────────     │
│                                                            │
│ Appuntamenti di Bianchi (01-30 Aprile 2025):             │
│ • 12 appuntamenti programmati                            │
│ • 8 pazienti coinvolti                                   │
│ • 3 co-educatori presenti nei progetti                   │
│                                                            │
│ ⚠️ L'educatore importando questo pacchetto sostituirà   │
│    TUTTI i suoi appuntamenti con questi nuovi dati.       │
│                                                            │
│ [Esporta Pacchetto Appuntamenti]                          │
│                                                            │
└────────────────────────────────────────────────────────────┘
```

**Comportamento:**
- Il Coordinatore seleziona un educatore specifico
- Il sistema filtra tutti gli appuntamenti assegnati a quell'educatore
- Include TUTTI gli educatori associati ai progetti per consentire spunta durante registrazione visite
- Genera un pacchetto con naming `appointments_{cognome}_{YYYYMMDD}.ptrp`

**Risultato:**
```
┌────────────────────────────────────────────────────────────┐
│ ✅ Pacchetto Appuntamenti Creato                          │
├────────────────────────────────────────────────────────────┤
│ File: appointments_bianchi_20260401.ptrp                   │
│ Dimensione: 145 KB                                        │
│ Contiene:                                                 │
│ • 12 appuntamenti per Bianchi                            │
│ • 8 pazienti                                             │
│ • 3 educatori associati ai progetti                      │
│                                                            │
│ Consegna questo file a Bianchi per l'importazione.        │
│                                                            │
│ [💾 Salva su USB]  [📧 Invia Email]                       │
└────────────────────────────────────────────────────────────┘
```

---

#### Step 11: Importazione Appuntamenti (Educatore)

**Azione Utente:** Educatore riceve file e apre sezione "Importa Appuntamenti"

**UI Display:**
```
┌────────────────────────────────────────────────────────────┐
│ Importazione Appuntamenti                   [Educatore]   │
├────────────────────────────────────────────────────────────┤
│                                                            │
│ [📁 Seleziona File .ptrp...]                              │
│                                                            │
│ File selezionato:                                         │
│ appointments_bianchi_20260401.ptrp (145 KB)               │
│                                                            │
│ ─────────────────────────────────────────────────────     │
│ VERIFICA PACCHETTO                                        │
│ ─────────────────────────────────────────────────────     │
│                                                            │
│ ✅ Firma verificata (integrità confermata)                 │
│ ✅ Crittografia verificata                                 │
│ ✅ Schema compatibile                                      │
│ ✅ Destinatario corretto: Bianchi                          │
│ ✅ Data pacchetto: 01/04/2026                              │
│                                                            │
│ ─────────────────────────────────────────────────────     │
│ CONTENUTO PACCHETTO                                       │
│ ─────────────────────────────────────────────────────     │
│                                                            │
│ • 12 appuntamenti per Bianchi                            │
│ • Periodo: 01-30 Aprile 2026                             │
│ • 8 pazienti                                             │
│ • 3 educatori associati (per spunta visite)              │
│                                                            │
│ ⚠️ ATTENZIONE: L'importazione SOSTITUIRÀ COMPLETAMENTE  │
│    tutti i tuoi appuntamenti attuali con questi nuovi.    │
│                                                            │
│ ⚠️ Le visite già registrate NON saranno modificate.       │
│                                                            │
│                    [Annulla]  [Importa Appuntamenti]      │
│                                                            │
└────────────────────────────────────────────────────────────┘
```

**Comportamento del Sistema:**

1. Verifica l'integrità e destinatario del pacchetto
2. Controlla data pacchetto vs ultime visite registrate (warning se obsoleto)
3. **SOSTITUISCE COMPLETAMENTE** tutti gli appuntamenti dell'educatore con i nuovi
4. Importa anche gli educatori associati ai progetti (necessari per spunta visite)
5. **Preserva** le visite già registrate (non vengono toccate)
6. Aggiorna il database locale

**Risultato UI:**
```
┌────────────────────────────────────────────┐
│ ✅ Appuntamenti Importati                 │
│                                            │
│ Risultati:                                │
│ • 12 appuntamenti importati              │
│ • 3 educatori associati importati        │
│ • Appuntamenti precedenti sostituiti    │
│ • Visite registrate preservate          │
│                                            │
│ Calendario aggiornato con successo.       │
│                                            │
│ [Visualizza Calendario] [Chiudi]         │
└────────────────────────────────────────────┘
```

---

## 🔐 FLUSSO 6: Controllo Accessi (Permessi Utente)

### Scenario: Educatore tenta operazione non autorizzata

#### Step 12: Verifica Permessi

**Azione Utente:** Educatore tenta di modificare dati paziente

**UI Messaggio di Errore:**
```
┌────────────────────────────────────────────┐
│ ⚠️ Autorizzazione Negata                   │
│                                            │
│ Non hai i permessi necessari per          │
│ modificare i dati dei pazienti.           │
│                                            │
│ Solo i Coordinatori possono effettuare   │
│ questa operazione.                        │
│                                            │
│ Se ritieni sia un errore, contatta il    │
│ Coordinatore del sistema.                │
│                                            │
│ [OK]                                      │
└────────────────────────────────────────────┘
```

**Matrice Permessi:**

| Operazione | Educatore | Coordinatore |
|------------|-----------|---------------|
| **Pazienti** |
| Visualizzare assegnati | ✅ | ✅ |
| Visualizzare tutti | ❌ | ✅ |
| Creare/Modificare | ❌ | ✅ |
| **Educatori** |
| Visualizzare elenco | ❌ | ✅ |
| Creare/Modificare | ❌ | ✅ |
| **Progetti** |
| Visualizzare assegnati | ✅ | ✅ |
| Visualizzare tutti | ❌ | ✅ |
| Creare e assegnare | ❌ | ✅ |
| Modificare stato | ❌ | ✅ |
| **Appuntamenti** |
| Visualizzare i propri | ✅ | ✅ |
| Visualizzare tutti | ❌ | ✅ |
| Modificare | ❌ | ✅ |
| **Visite** |
| Registrare da appuntamenti propri | ✅ | ✅ |
| Registrare da appuntamenti altrui | ❌ | ✅ |
| Visualizzare tutte | ❌ | ✅ |
| **Sincronizzazione** |
| Esportare visite | ✅ | ✅ |
| Importare visite | ❌ | ✅ |
| Esportare appuntamenti | ❌ | ✅ |
| Importare appuntamenti | ✅ | ✅ |

---

## 📊 FLUSSO 7: Reportistica (Coordinatore)

### Scenario: Generazione report mensile

#### Step 13: Dashboard Riepilogativa

**Azione Utente:** Coordinatore apre Dashboard

**UI Display:**
```
┌────────────────────────────────────────────────────────────┐
│ Dashboard PTRP - Aprile 2025                              │
├────────────────────────────────────────────────────────────┤
│                                                            │
│ ┌─────────────┐ ┌─────────────┐ ┌─────────────┐         │
│ │   103       │ │     45      │ │     12      │         │
│ │  Pazienti   │ │  Progetti   │ │  Educatori  │         │
│ │   Totali    │ │   Attivi    │ │  Operativi  │         │
│ └─────────────┘ └─────────────┘ └─────────────┘         │
│                                                            │
│ ─────────────────────────────────────────────────────     │
│ VISITE MESE CORRENTE                                      │
│ ─────────────────────────────────────────────────────     │
│                                                            │
│ Appuntamenti Programmati: 42                              │
│ Visite Completate:        32   ████████████████░░░░░  76%  │
│ Appuntamenti Sospesi:     7    ████░░░░░░░░░░░░░░░░░  17%  │
│ Appuntamenti Mancati:     3    ██░░░░░░░░░░░░░░░░░░░   7%  │
│                                                            │
│ ─────────────────────────────────────────────────────     │
│ TOP EDUCATORI (visite registrate)                         │
│ ─────────────────────────────────────────────────────     │
│                                                            │
│ 1. 👤 Bianchi     28 visite  ████████████████████         │
│ 2. 👤 Neri        24 visite  ████████████████░░░░         │
│ 3. 👤 Verdi       19 visite  ████████████░░░░░░░░         │
│ 4. 👤 Gialli      17 visite  ███████████░░░░░░░░░         │
│ 5. 👤 Viola       15 visite  █████████░░░░░░░░░░░         │
│                                                            │
│ ─────────────────────────────────────────────────────     │
│                                                            │
│ [📄 Esporta Report PDF] [📊 Dettagli Completi]           │
│                                                            │
└────────────────────────────────────────────────────────────┘
```

**Informazioni Visualizzate:**
- KPI principali (pazienti, progetti attivi, educatori operativi)
- Statistiche visite del mese (appuntamenti vs visite completate)
- Top educatori per numero visite registrate
- Possibilità di esportare report in PDF

---

## 🔗 RIFERIMENTI DOCUMENTAZIONE

- [README.md](../README.md) - Panoramica progetto
- [DATABASE.md](DATABASE.md) - Struttura dati
- [SECURITY.md](SECURITY.md) - Permessi e sicurezza
- [DEVELOPMENT.md](DEVELOPMENT.md) - Guida sviluppatori

---

**Documento creato:** 30 Gennaio 2026  
**Versione:** 3.2 (Versione per Equipe PTRP - Solo Flussi Utente)  
**Autore:** Marco Cavallo (@artcava)  
