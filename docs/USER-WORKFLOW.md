# USER-WORKFLOW.md - Flusso dei Dati Utente PTRP

## 📋 Panoramica

Questo documento descrive il **flusso dei dati dal punto di vista dell'utente applicativo** per il sistema PTRP. L'applicazione gestisce Pazienti, Progetti Terapeutici Riabilitativi Personalizzati, Educatori Professionali e Visite.

### Profili Utente

L'applicazione supporta **due profili utente** con permessi differenziati:

1. **Coordinatore**
   - Gestione completa anagrafiche pazienti
   - Creazione e assegnazione progetti terapeutici
   - Assegnazione educatori ai progetti
   - Visualizzazione globale di tutti i dati

2. **Educatore Professionale**
   - Visualizzazione pazienti e progetti assegnati
   - Registrazione visite effettive per i propri progetti
   - Accesso limitato ai soli dati di competenza

---

## 🏗️ Modello Dati Semplificato

### Relazioni Fondamentali

```
Paziente (1) ←───── (1) Progetto Terapeutico Attivo
                          ↓
                          ↓ (N:N)
                          ↓
                    Educatori Professionali
                          ↓
                          ↓ (1:N)
                          ↓
                    Visite Programmate (4 canoniche)
                          ↓
                          ↓ (1:1)
                          ↓
                    Visite Effettive
```

### Regole di Business Critiche

1. **Unicità Progetto Attivo**: Un paziente può avere **UN SOLO** progetto terapeutico attivo contemporaneamente
2. **Assegnazione Educatori**: Gli educatori sono assegnati al **Progetto Terapeutico**, non direttamente al paziente
3. **Relazione Implicita**: Gli educatori di un paziente si desumono dal progetto attivo
4. **Visite Canoniche**: Ogni progetto genera automaticamente 4 visite programmate:
   - Prima Apertura (INTAKE) - dopo 3 mesi dall'assegnazione
   - Verifica Intermedia - dopo 6 mesi dalla Prima Apertura
   - Verifica Finale - dopo 6 mesi dalla Verifica Intermedia
   - Dimissioni - dopo 1 mese dalla Verifica Finale

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
│ Nome Cognome        │ Stato      │ Educatori Assegnati    │
├────────────────────────────────────────────────────────────┤
│ CALAMITA Daniele    │ Active     │ Corrias, Gargiulo      │
│ DISTANTE Andrea     │ Active     │ Lapaglia               │
│ CORAGLIA Debora     │ Suspended  │ Foschiano, Perziano    │
│ BETTI Fabrizio      │ Deceased   │ -                      │
│ BIAGIONE Rosaria    │ Active     │ Foschiano, Perziano    │
│ ...                                                        │
└────────────────────────────────────────────────────────────┘
```

**Comportamento:**
- La colonna "Educatori Assegnati" mostra gli educatori del progetto attivo corrente
- Se il paziente non ha progetti attivi, la colonna mostra "-" o "Nessun progetto"
- Stati possibili: `Active`, `Suspended`, `Deceased`

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
│ Stato:    [▼ Active            ]           │
│                                            │
│           [Annulla]  [Salva]               │
└────────────────────────────────────────────┘
```

**Validazioni:**
- Nome e Cognome obbligatori
- Avviso se esiste paziente con stesso nome/cognome
- Stato di default: `Active`

**Risultato:**
- Paziente creato e visibile nella lista
- Nessun progetto ancora assegnato → colonna educatori vuota
- Sistema pronto per creazione progetto terapeutico

---

#### Step 3: Ricerca e Filtri

**Azione Utente:** Digitare nel box ricerca per trovare pazienti

**Comportamento UI:**
- Ricerca in tempo reale su Nome e Cognome
- Filtri per stato paziente
- Ricerca case-insensitive

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
│ │ Stato: In Progress                                │    │
│ │ Periodo: 02/01/2025 - 02/01/2027                  │    │
│ │ Educatori: Corrias, Gargiulo                      │    │
│ │ Prossima Visita: 02/04/2025 (Prima Apertura)     │    │
│ │                                                    │    │
│ │ [Visualizza Dettagli] [Modifica Educatori]       │    │
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
- Titolo e stato del progetto
- Periodo di validità
- Educatori assegnati
- Prossima visita programmata

---

#### Step 5: Creazione Nuovo Progetto Terapeutico

**Azione Utente:** Click su `[+ Nuovo Progetto Terapeutico]`

**Validazione Pre-Creazione:**
- Sistema verifica che NON esista già un progetto attivo per questo paziente
- Se esiste, mostra messaggio: "Il paziente ha già un progetto attivo. Chiudi o completa il progetto corrente prima di crearne uno nuovo."

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
│ Stato:           [▼ In Progress      ]                    │
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
│ [✓] Genera automaticamente visite programmate (4)         │
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
- Controllo unicità progetto attivo per paziente

**Operazioni Eseguite dal Sistema:**

1. Crea il progetto terapeutico
2. Assegna gli educatori al progetto
3. Genera automaticamente 4 visite programmate con le scadenze corrette

**Risultato:**
- Progetto creato e visibile nella scheda paziente
- Educatori ora visibili nella lista pazienti (colonna "Educatori Assegnati")
- 4 visite automaticamente programmate nel calendario

---

## 📅 FLUSSO 3: Visualizzazione Calendario Visite

### Scenario: Coordinatore consulta visite programmate

#### Step 6: Calendario Visite Mensile

**Azione Utente:** Navigazione a sezione "Calendario"

**UI Display:**
```
┌────────────────────────────────────────────────────────────┐
│ Calendario Visite - Aprile 2025                           │
├────────────────────────────────────────────────────────────┤
│  L    M    M    G    V    S    D                          │
│       1    2🔵  3    4    5    6                          │
│  7    8    9   10   11   12   13                          │
│ 14   15   16   17🟢 18   19   20                          │
│ 21   22   23🟠 24   25   26   27                          │
│ 28   29   30                                              │
│                                                            │
│ Legenda:                                                  │
│ 🔵 Prima Apertura  🟢 Verifica Int.  🟡 Verifica Finale   │
│ 🟠 Dimissioni      ⚪ Nessuna visita                       │
│                                                            │
│ Filtri:                                                   │
│ [✓] Prima Apertura  [✓] Verifiche  [✓] Dimissioni        │
│ Educatore: [▼ Tutti               ]                       │
│ Stato: [▼ Programmate             ]                       │
└────────────────────────────────────────────────────────────┘

┌────────────────────────────────────────────────────────────┐
│ Visite del 02 Aprile 2025                                 │
├────────────────────────────────────────────────────────────┤
│                                                            │
│ 🔵 Prima Apertura - CALAMITA Daniele                      │
│    Educatori: Corrias, Gargiulo                           │
│    Progetto: PTRP 2025-2027                               │
│    [Registra Visita] [Riprogramma] [Segna Mancata]       │
│                                                            │
│ 🔵 Prima Apertura - DISTANTE Andrea                       │
│    Educatore: Lapaglia                                    │
│    Progetto: PTRP 2025-2027                               │
│    [Registra Visita] [Riprogramma] [Segna Mancata]       │
│                                                            │
└────────────────────────────────────────────────────────────┘
```

**Funzionalità:**
- Visualizzazione calendario mensile con codice colore per tipo visita
- Lista giornaliera di visite con dettagli
- Filtri per tipo visita, educatore, stato
- Azioni rapide: Registra, Riprogramma, Segna Mancata

---

## ✍️ FLUSSO 4: Registrazione Visita Effettiva (Educatore)

### Scenario: Educatore registra visita dopo incontro con paziente

#### Step 7: Selezione Visita da Registrare

**Azione Utente:** Educatore Corrias accede alla lista "Mie Visite"

**UI Display (Vista Educatore):**
```
┌────────────────────────────────────────────────────────────┐
│ Le Mie Visite Programmate                   [Educatore]   │
├────────────────────────────────────────────────────────────┤
│ Oggi: 02/04/2025                                          │
│                                                            │
│ 🔵 CALAMITA Daniele - Prima Apertura                      │
│    Ore: 10:00 (stimato 90 min)                           │
│    Co-educatore: Gargiulo                                 │
│    [✓ Registra Visita]                                    │
│                                                            │
│ ─────────────────────────────────────────────────────     │
│ Prossime Visite (7 giorni)                                │
│                                                            │
│ 05/04/2025 - BIAGIONE Rosaria - Prima Apertura           │
│ 09/04/2025 - PALIERI Franca - Prima Apertura             │
│ ...                                                        │
└────────────────────────────────────────────────────────────┘
```

**Comportamento:**
- Educatore vede solo le visite dei propri progetti assegnati
- Sono mostrate visite di oggi e i prossimi 7 giorni
- Possibilità di aprire il modulo di registrazione visita

---

#### Step 8: Form Registrazione Visita Effettiva

**Azione Utente:** Click su `[✓ Registra Visita]`

**UI Dialog:**
```
┌────────────────────────────────────────────────────────────┐
│ Registrazione Visita Effettiva                      [X]   │
├────────────────────────────────────────────────────────────┤
│                                                            │
│ Paziente: CALAMITA Daniele                                │
│ Tipo Visita: 🔵 Prima Apertura (INTAKE)                   │
│ Data Programmata: 02/04/2025                              │
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
│  del progetto terapeutico. Il paziente ha manifestato...  │
│  ________________________________________________         │
│  ________________________________________________]        │
│                                                            │
│ ─────────────────────────────────────────────────────     │
│ ESITI E OBIETTIVI                                         │
│ ─────────────────────────────────────────────────────     │
│                                                            │
│ [________________________________________________         │
│  Obiettivo 1: Migliorare autonomia nelle attività...     │
│  Obiettivo 2: Rafforzare competenze relazionali...       │
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
- Data effettiva non può essere futura
- Ora fine deve essere successiva all'ora inizio
- Almeno un operatore deve essere selezionato
- Note cliniche obbligatorie
- Presenza paziente obbligatoria

**Operazioni Eseguite dal Sistema:**

1. Crea la registrazione della visita effettiva
2. Registra gli operatori presenti
3. Aggiorna lo stato della visita programmata a "Completata"
4. Salva nel database locale

**Risultato UI:**
```
┌────────────────────────────────────────────┐
│ ✅ Visita Registrata con Successo          │
│                                            │
│ La visita è stata salvata nel database    │
│ locale. Ricorda di sincronizzare i dati   │
│ con il Coordinatore.                      │
│                                            │
│ [OK]  [Vai a Sincronizzazione]           │
└────────────────────────────────────────────┘
```

---

## 🔄 FLUSSO 5: Sincronizzazione Dati (Offline-First)

### Scenario: Educatore sincronizza visite registrate con Coordinatore

#### Step 9: Esportazione Pacchetto Sync (Educatore)

**Azione Utente:** Navigazione a sezione "Sincronizzazione"

**UI Display:**
```
┌────────────────────────────────────────────────────────────┐
│ Sincronizzazione Dati                       [Educatore]   │
├────────────────────────────────────────────────────────────┤
│ [Esporta Dati] [Importa Dati] [Storico Sync]             │
├────────────────────────────────────────────────────────────┤
│                                                            │
│ DATI DA SINCRONIZZARE                                     │
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
│ [Crea Pacchetto di Sincronizzazione]                     │
│                                                            │
└────────────────────────────────────────────────────────────┘
```

**Comportamento del Sistema:**
- Raccoglie tutte le visite registrate dall'educatore
- Cripta e protegge i dati (nessuno può leggerli senza la password corretta)
- Genera un file da inviare al Coordinatore

**Risultato:**
```
┌────────────────────────────────────────────────────────────┐
│ ✅ Pacchetto Creato con Successo                          │
├────────────────────────────────────────────────────────────┤
│                                                            │
│ File: sync_corrias_20250405_183000.ptrp                   │
│ Dimensione: 287 KB                                        │
│ Creato: 05/04/2025 18:30:00                               │
│                                                            │
│ Il pacchetto contiene:                                    │
│ • 5 visite effettive                                      │
│ • 12 relazioni operatori                                  │
│                                                            │
│ [💾 Salva su USB]  [📧 Invia Email]  [☁️ Condividi]     │
│                                                            │
└────────────────────────────────────────────────────────────┘
```

---

#### Step 10: Importazione Pacchetto (Coordinatore)

**Azione Utente:** Coordinatore riceve file e apre sezione Sincronizzazione

**UI Display:**
```
┌────────────────────────────────────────────────────────────┐
│ Importazione Pacchetto Sincronizzazione  [Coordinatore]  │
├────────────────────────────────────────────────────────────┤
│                                                            │
│ [📁 Seleziona File .ptrp...]                              │
│                                                            │
│ File selezionato:                                         │
│ sync_corrias_20250405_183000.ptrp (287 KB)               │
│                                                            │
│ ─────────────────────────────────────────────────────     │
│ VERIFICA PACCHETTO                                        │
│ ─────────────────────────────────────────────────────     │
│                                                            │
│ ✅ Firma verificata (integrità confermata)                 │
│ ✅ Crittografia verificata (accesso autorizzato)           │
│ ✅ Schema compatibile (versione v1.0)                      │
│ ✅ Operatore riconosciuto: Corrias                         │
│                                                            │
│ ─────────────────────────────────────────────────────     │
│ CONTENUTO PACCHETTO                                       │
│ ─────────────────────────────────────────────────────     │
│                                                            │
│ • 5 visite effettive da importare                         │
│ • 12 relazioni operatori-visite                           │
│                                                            │
│ Conflitti rilevati: 0                                     │
│                                                            │
│ ⚠️ L'importazione aggiornerà il database locale           │
│                                                            │
│                    [Annulla]  [Importa Dati]              │
│                                                            │
└────────────────────────────────────────────────────────────┘
```

**Comportamento del Sistema:**

1. Verifica l'integrità del pacchetto
2. Decripta i dati
3. Controlla se ci sono conflitti (es. stessa visita modificata in due posti)
4. In caso di conflitto: priorità a quanto registrato dal Coordinatore
5. Importa i dati nel database

**Risultato UI:**
```
┌────────────────────────────────────────────┐
│ ✅ Importazione Completata                 │
│                                            │
│ Risultati:                                │
│ • 5 visite registrate importate           │
│ • 12 relazioni operatori importate        │
│ • 0 conflitti risolti                     │
│ • 0 record saltati                        │
│                                            │
│ Database aggiornato con successo.         │
│                                            │
│ [Visualizza Report] [Chiudi]             │
└────────────────────────────────────────────┘
```

---

## 🔐 FLUSSO 6: Controllo Accessi (Permessi Utente)

### Scenario: Educatore tenta operazione non autorizzata

#### Step 11: Verifica Permessi

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
| **Progetti** |
| Visualizzare assegnati | ✅ | ✅ |
| Visualizzare tutti | ❌ | ✅ |
| Creare e assegnare | ❌ | ✅ |
| **Visite** |
| Registrare le proprie | ✅ | ✅ |
| Registrare altrui | ❌ | ✅ |
| Visualizzare tutte | ❌ | ✅ |
| **Sincronizzazione** |
| Esportare | ✅ | ✅ |
| Importare | ❌ | ✅ |

---

## 📊 FLUSSO 7: Reportistica (Coordinatore)

### Scenario: Generazione report mensile

#### Step 12: Dashboard Riepilogativa

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
│ │   Attivi    │ │  In Corso   │ │  Operativi  │         │
│ └─────────────┘ └─────────────┘ └─────────────┘         │
│                                                            │
│ ─────────────────────────────────────────────────────     │
│ VISITE MESE CORRENTE                                      │
│ ─────────────────────────────────────────────────────     │
│                                                            │
│ Programmate: 42                                           │
│ Completate:  32   ████████████████░░░░░  76%             │
│ Sospese:     7    ████░░░░░░░░░░░░░░░░░  17%             │
│ Mancate:     3    ██░░░░░░░░░░░░░░░░░░░   7%             │
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
- KPI principali (pazienti, progetti, educatori attivi)
- Statistiche visite del mese
- Top educatori per numero visite
- Possibilità di esportare report in PDF

---

## 📝 RIEPILOGO VISTE UI DA IMPLEMENTARE

### Priorità Implementazione

#### FASE 1 - MVP (Minimum Viable Product)

1. **MainWindow** ✅
   - Menu navigazione
   - Header con ruolo utente
   - Area contenuto dinamica

2. **PatientListView + PatientDetailView** 🚧
   - Lista pazienti con ricerca
   - Form CRUD paziente
   - Tab progetti associati
   - Educatori mostrati via progetto attivo

3. **ProjectDetailView** 🚧
   - Form creazione progetto
   - Assegnazione educatori
   - Generazione automatica 4 visite
   - Validazione unicità progetto attivo

4. **CalendarView** 🔲
   - Calendario mensile visite
   - Lista giornaliera visite
   - Filtri per tipo visita ed educatore

5. **VisitRegistrationView** 🔲
   - Form registrazione visita
   - Selezione operatori presenti
   - Note cliniche obbligatorie

#### FASE 2 - Core Functionality

6. **SyncView** 🔲
   - Tab Esporta pacchetto
   - Tab Importa pacchetto
   - Visualizzazione conflitti
   - Log sincronizzazioni

7. **AuthorizationLayer** 🔲
   - Implementazione permessi utente
   - Controlli accesso su tutte le view
   - Messaggi errore user-friendly

8. **OperatorManagementView** 🔲
   - Lista educatori
   - Form CRUD educatore
   - Progetti assegnati

#### FASE 3 - Advanced Features

9. **DashboardView** 🔲
   - KPI principali
   - Grafici trend mensili
   - Top educatori

10. **ReportingModule** 🔲
    - Report personalizzabili
    - Export Excel/PDF
    - Grafici interattivi

---

## 🎯 PUNTI CHIAVE DEL MODELLO

### 1. Relazione Paziente-Educatore

**Importante:** Non esiste una tabella diretta "Paziente ↔ Educatore"

**Come funziona:**
- Gli educatori sono assegnati al **Progetto Terapeutico**
- Non al paziente direttamente
- Gli educatori di un paziente si ottengono guardando il suo progetto attivo

### 2. Unicità Progetto Attivo

**Un paziente può avere UN SOLO progetto attivo contemporaneamente**
- Se provi a crearne uno nuovo mentre ce n'è uno attivo, il sistema ti avvisa
- Devi completare o sospendere il progetto precedente prima

### 3. Visite Canoniche Automatiche

**Quando crei un progetto, il sistema automaticamente genera 4 visite:**
- Prima Apertura: 3 mesi dopo l'inizio
- Verifica Intermedia: 6 mesi dopo la Prima Apertura
- Verifica Finale: 6 mesi dopo la Verifica Intermedia
- Dimissioni: 1 mese dopo la Verifica Finale

### 4. Fonte Dati per Audit

**Ogni visita traccia chi l'ha registrata:**
- Se registrata da un Educatore → "EducatorImport"
- Se registrata dal Coordinatore → "CoordinatorDirect"
- Questo serve per tracciabilità e risoluzione conflitti

---

## 🔗 RIFERIMENTI DOCUMENTAZIONE

- [README.md](../README.md) - Panoramica progetto
- [DATABASE.md](DATABASE.md) - Struttura dati
- [SECURITY.md](SECURITY.md) - Permessi e sicurezza
- [DEVELOPMENT.md](DEVELOPMENT.md) - Guida sviluppatori

---

**Documento creato:** 30 Gennaio 2026  
**Versione:** 2.0 (Versione per Equipe PTRP)  
**Autore:** Marco Cavallo (@artcava)
