# USER-WORKFLOW.md - Flusso dei Dati Utente PTRP

## 📋 Panoramica

Questo documento descrive il **flusso dei dati dal punto di vista dell'utente applicativo** per il sistema PTRP. L'applicazione gestisce Pazienti, Progetti Terapeutici Riabilitativi Personalizzati, Educatori Professionali, Appuntamenti e Visite.

### Profili Utente

L'applicazione supporta **due profili utente** con permessi differenziati:

1. **Coordinatore**
   - Gestione completa anagrafiche pazienti
   - **Gestione anagrafica educatori professionali**
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
│ CALAMITA Daniele    │ Active         │ Corrias, Gargiulo      │
│ DISTANTE Andrea     │ Active         │ Lapaglia               │
│ CORAGLIA Debora     │ Suspended      │ Foschiano, Perziano    │
│ BETTI Fabrizio      │ Deceased       │ -                      │
│ BIAGIONE Rosaria    │ Active         │ Foschiano, Perziano    │
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
│ CORRIAS             │ 8               │ Attivo           │
│ GARGIULO            │ 6               │ Attivo           │
│ LAPAGLIA            │ 5               │ Attivo           │
│ FOSCHIANO           │ 7               │ Attivo           │
│ PERZIANO            │ 4               │ Sospeso          │
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
│ Paziente: CALAMITA Daniele                    [Modifica]   │
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
│ │ Educatori: Corrias, Gargiulo                      │    │
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
│ Nuovo Progetto Terapeutico - CALAMITA Daniele      [X]    │
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
│ │ Corrias        [Rimuovi]                          │    │
│ │ Gargiulo       [Rimuovi]                          │    │
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
│ 🟢 Prima Apertura - CALAMITA Daniele                      │
│    Educatori: Corrias, Gargiulo                           │
│    Progetto: PTRP 2025-2027 (Active)                      │
│    [Registra Visita] [Riprogramma] [Segna Mancato]       │
│                                                            │
│ 🟢 Prima Apertura - DISTANTE Andrea                       │
│    Educatore: Lapaglia                                    │
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

**Azione Utente:** Educatore Corrias accede alla lista "Miei Appuntamenti"

**UI Display (Vista Educatore):**
```
┌────────────────────────────────────────────────────────────┐
│ I Miei Appuntamenti                         [Educatore]   │
├────────────────────────────────────────────────────────────┤
│ Oggi: 02/04/2025                                          │
│                                                            │
│ 🟢 CALAMITA Daniele - Prima Apertura                      │
│    Ore: 10:00 (stimato 90 min)                           │
│    Co-educatore: Gargiulo                                 │
│    [✓ Registra Visita]                                    │
│                                                            │
│ ─────────────────────────────────────────────────────     │
│ Prossimi Appuntamenti (7 giorni)                          │
│                                                            │
│ 05/04/2025 - BIAGIONE Rosaria - Prima Apertura           │
│ 09/04/2025 - PALIERI Franca - Prima Apertura             │
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
│ Paziente: CALAMITA Daniele                                │
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
│ [✓] Corrias (io)                                          │
│ [✓] Gargiulo                                              │
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
│     • CALAMITA Daniele - Prima Apertura (02/04)          │
│     • DISTANTE Andrea - Prima Apertura (02/04)           │
│     • BIAGIONE Rosaria - Prima Apertura (05/04)          │
│     • PALIERI Franca - Prima Apertura (09/04)            │
│     • COTTONE Valeria - Verifica Intermedia (08/04)      │
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
│ File: visits_corrias_20250405_183000.ptrp                 │
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
│ [▼ Corrias                    ]                          │
│                                                            │
│ Periodo:                                                  │
│ Dal: [📅 01/04/2025]  Al: [📅 30/04/2025]              │
│                                                            │
│ ─────────────────────────────────────────────────────     │
│ ANTEPRIMA DATI                                            │
│ ─────────────────────────────────────────────────────     │
│                                                            │
│ Appuntamenti di Corrias (01-30 Aprile 2025):             │
│ • 12 appuntamenti programmati                            │
│ • 8 pazienti coinvolti                                   │
│ • 3 co-educatori presenti                                │
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
- Genera un pacchetto contenente SOLO gli appuntamenti di competenza

**Risultato:**
```
┌────────────────────────────────────────────────────────────┐
│ ✅ Pacchetto Appuntamenti Creato                          │
├────────────────────────────────────────────────────────────┤
│ File: appointments_corrias_202504.ptrp                     │
│ Dimensione: 145 KB                                        │
│ Contiene: 12 appuntamenti per Corrias                     │
│                                                            │
│ Consegna questo file a Corrias per l'importazione.        │
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
│ appointments_corrias_202504.ptrp (145 KB)                 │
│                                                            │
│ ─────────────────────────────────────────────────────     │
│ VERIFICA PACCHETTO                                        │
│ ─────────────────────────────────────────────────────     │
│                                                            │
│ ✅ Firma verificata (integrità confermata)                 │
│ ✅ Crittografia verificata                                 │
│ ✅ Schema compatibile                                      │
│ ✅ Destinatario corretto: Corrias                          │
│                                                            │
│ ─────────────────────────────────────────────────────     │
│ CONTENUTO PACCHETTO                                       │
│ ─────────────────────────────────────────────────────     │
│                                                            │
│ • 12 appuntamenti per Corrias                            │
│ • Periodo: 01-30 Aprile 2025                             │
│ • 8 pazienti                                             │
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
2. **SOSTITUISCE COMPLETAMENTE** tutti gli appuntamenti dell'educatore con i nuovi
3. **Preserva** le visite già registrate (non vengono toccate)
4. Aggiorna il database locale

**Risultato UI:**
```
┌────────────────────────────────────────────┐
│ ✅ Appuntamenti Importati                 │
│                                            │
│ Risultati:                                │
│ • 12 appuntamenti importati              │
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
│ 1. 👤 Fatiga      28 visite  ████████████████████         │
│ 2. 👤 Lapaglia    24 visite  ████████████████░░░░         │
│ 3. 👤 Corrias     19 visite  ████████████░░░░░░░░         │
│ 4. 👤 Foschiano   17 visite  ███████████░░░░░░░░░         │
│ 5. 👤 Gargiulo    15 visite  █████████░░░░░░░░░░░         │
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
**Versione:** 3.0 (Versione per Equipe PTRP - Aggiornata)  
**Autore:** Marco Cavallo (@artcava)  
**Ultime modifiche:**
- Aggiunta gestione anagrafica educatori (Coordinatore)
- Distinzione terminologica: Appuntamenti vs Visite
- Stato applicato al Progetto Terapeutico (non al Paziente)
- Calendario con codice colore per stato progetto
- Vincolo obbligatorio: Visita legata ad Appuntamento (relazione 1:1)
- Nuovo flusso sincronizzazione bidirezionale (Coordinatore → Educatore)
