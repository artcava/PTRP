# USER-WORKFLOW.md - Flusso Dati Utente PTRP

## 📋 Panoramica

Questo documento descrive il **flusso dei dati dal punto di vista dell'utente applicativo** per il sistema PTRP. L'applicazione gestisce Pazienti, Progetti Terapeutici Riabilitativi Personalizzati, Educatori Professionali e Visite in modalità **offline-first**.

### Profili Utente

L'applicazione supporta **due profili utente** con permessi differenziati:

1. **Coordinatore** (Master)
   - Gestione completa anagrafiche pazienti
   - Creazione e assegnazione progetti terapeutici
   - Assegnazione educatori ai progetti
   - Visualizzazione globale di tutti i dati
   - Priorità assoluta nella risoluzione conflitti di sincronizzazione

2. **Educatore Professionale** (Slave)
   - Visualizzazione pazienti e progetti assegnati
   - Registrazione visite effettive per i propri progetti
   - Esportazione dati per sincronizzazione con Coordinatore
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

1. **Unicità Progetto Attivo**: Un paziente può avere **UN SOLO** progetto terapeutico con stato "Active" contemporaneamente
2. **Assegnazione Educatori**: Gli educatori sono assegnati al **Progetto Terapeutico**, non direttamente al paziente
3. **Relazione Implicita**: Gli educatori di un paziente si desumono dal progetto attivo, senza tabella di relazione diretta Paziente↔Educatore
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

**Query Database:**
```sql
SELECT 
    p.id,
    p.first_name,
    p.last_name,
    p.status,
    GROUP_CONCAT(o.first_name || ' ' || o.last_name) as educators
FROM patients p
LEFT JOIN therapeutic_projects tp ON p.id = tp.patient_id 
    AND tp.status = 'Active'
LEFT JOIN project_operators po ON tp.id = po.project_id
LEFT JOIN operators o ON po.operator_id = o.id
GROUP BY p.id
ORDER BY p.last_name, p.first_name;
```

**Note Implementative:**
- La colonna "Educatori Assegnati" mostra gli educatori del **progetto attivo** corrente
- Se il paziente non ha progetti attivi, la colonna mostra "-" o "Nessun progetto"
- Stati possibili: `Active`, `Suspended`, `Deceased`

---

#### Step 2: Creazione Nuovo Paziente

**Azione Utente:** Click su `[+ Nuovo Paziente]`

**UI Dialog:**
```
┌────────────────────────────────────────────────┐
│ Nuovo Paziente                          [X]    │
├────────────────────────────────────────────────┤
│                                                │
│ Nome:     [_______________________________]    │
│ Cognome:  [_______________________________]    │
│ Stato:    [▼ Active            ]              │
│                                                │
│           [Annulla]  [Salva]                  │
└────────────────────────────────────────────────┘
```

**Validazioni:**
- Nome e Cognome obbligatori (min 2 caratteri)
- Check duplicati: avviso se esiste paziente con stesso nome/cognome
- Stato default: `Active`

**Operazione Database:**
```sql
INSERT INTO patients (
    id,
    first_name,
    last_name,
    status,
    created_at,
    created_by
) VALUES (
    'GUID-generato',
    'Marco',
    'Rossi',
    'Active',
    CURRENT_TIMESTAMP,
    'CoordinatorUsername'
);
```

**Risultato:**
- Paziente creato e visibile nella lista
- **Nessun progetto ancora assegnato** → colonna educatori vuota
- Sistema pronto per creazione progetto terapeutico

---

#### Step 3: Ricerca e Filtri

**Azione Utente:** Digitare nel box ricerca "CALAMITA"

**Comportamento UI:**
- Filtro real-time sulla lista (debounce 300ms)
- Ricerca case-insensitive su Nome e Cognome
- Evidenziazione match nel testo

**Query Database:**
```sql
SELECT 
    p.id,
    p.first_name,
    p.last_name,
    p.status,
    GROUP_CONCAT(o.first_name || ' ' || o.last_name) as educators
FROM patients p
LEFT JOIN therapeutic_projects tp ON p.id = tp.patient_id 
    AND tp.status = 'Active'
LEFT JOIN project_operators po ON tp.id = po.project_id
LEFT JOIN operators o ON po.operator_id = o.id
WHERE 
    LOWER(p.first_name) LIKE '%calamita%'
    OR LOWER(p.last_name) LIKE '%calamita%'
GROUP BY p.id;
```

---

## 🗂️ FLUSSO 2: Gestione Progetti Terapeutici (Coordinatore)

### Scenario: Apertura nuovo PTRP dopo 3 mesi di osservazione

#### Step 4: Visualizzazione Dettaglio Paziente

**Azione Utente:** Click su paziente "CALAMITA Daniele" nella lista

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

**Query Database:**
```sql
-- Recupera progetti del paziente
SELECT 
    tp.id,
    tp.title,
    tp.status,
    tp.start_date,
    tp.end_date,
    GROUP_CONCAT(o.first_name || ' ' || o.last_name) as educators,
    (
        SELECT sv.scheduled_date || ' (' || vt.description || ')'
        FROM scheduled_visits sv
        JOIN visit_types vt ON sv.visit_type_id = vt.id
        WHERE sv.project_id = tp.id
          AND sv.status = 'Scheduled'
          AND sv.scheduled_date >= DATE('now')
        ORDER BY sv.scheduled_date ASC
        LIMIT 1
    ) as next_visit
FROM therapeutic_projects tp
LEFT JOIN project_operators po ON tp.id = po.project_id
LEFT JOIN operators o ON po.operator_id = o.id
WHERE tp.patient_id = ?
GROUP BY tp.id
ORDER BY 
    CASE tp.status 
        WHEN 'Active' THEN 1 
        WHEN 'In Progress' THEN 2 
        ELSE 3 
    END,
    tp.start_date DESC;
```

---

#### Step 5: Creazione Nuovo Progetto Terapeutico

**Azione Utente:** Click su `[+ Nuovo Progetto Terapeutico]`

**Validazione Pre-Creazione:**
```csharp
// Business Logic Service
public async Task<bool> CanCreateNewProjectAsync(Guid patientId)
{
    // Verifica che NON esista già un progetto attivo
    var activeProject = await _projectRepository
        .GetActiveProjectByPatientIdAsync(patientId);
    
    if (activeProject != null)
    {
        throw new BusinessRuleException(
            "Il paziente ha già un progetto attivo. " +
            "Chiudi o completa il progetto corrente prima di crearne uno nuovo."
        );
    }
    
    return true;
}
```

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
- Titolo obbligatorio (min 5 caratteri)
- Data inizio obbligatoria
- Data fine > data inizio (se specificata)
- Almeno 1 educatore assegnato
- Verifica unicità progetto attivo per paziente

**Operazioni Database (Transazione Atomica):**

```sql
BEGIN TRANSACTION;

-- 1. Crea progetto terapeutico
INSERT INTO therapeutic_projects (
    id,
    patient_id,
    title,
    description,
    start_date,
    end_date,
    status,
    created_at,
    created_by
) VALUES (
    'GUID-progetto',
    'GUID-CALAMITA',
    'PTRP 2025-2027',
    'Progetto di riabilitazione...',
    '2025-01-02',
    '2027-01-02',
    'In Progress',
    CURRENT_TIMESTAMP,
    'CoordinatorUsername'
);

-- 2. Assegna educatori (N:N)
INSERT INTO project_operators (id, project_id, operator_id, created_at)
VALUES 
    ('GUID-1', 'GUID-progetto', 'GUID-Corrias', CURRENT_TIMESTAMP),
    ('GUID-2', 'GUID-progetto', 'GUID-Gargiulo', CURRENT_TIMESTAMP);

-- 3. Genera 4 visite programmate canoniche
-- Prima Apertura (+3 mesi da start_date)
INSERT INTO scheduled_visits (
    id,
    project_id,
    visit_type_id,
    scheduled_date,
    status,
    created_at,
    created_by
) VALUES (
    'GUID-visit-1',
    'GUID-progetto',
    'INTAKE',  -- ID della visit_type
    DATE('2025-01-02', '+3 months'),  -- 2025-04-02
    'Scheduled',
    CURRENT_TIMESTAMP,
    'CoordinatorUsername'
);

-- Verifica Intermedia (+6 mesi da Prima Apertura)
INSERT INTO scheduled_visits (
    id,
    project_id,
    visit_type_id,
    scheduled_date,
    status,
    created_at,
    created_by
) VALUES (
    'GUID-visit-2',
    'GUID-progetto',
    'INTERMEDIATE',
    DATE('2025-04-02', '+6 months'),  -- 2025-10-02
    'Scheduled',
    CURRENT_TIMESTAMP,
    'CoordinatorUsername'
);

-- Verifica Finale (+6 mesi da Verifica Intermedia)
INSERT INTO scheduled_visits (
    id,
    project_id,
    visit_type_id,
    scheduled_date,
    status,
    created_at,
    created_by
) VALUES (
    'GUID-visit-3',
    'GUID-progetto',
    'FINAL',
    DATE('2025-10-02', '+6 months'),  -- 2026-04-02
    'Scheduled',
    CURRENT_TIMESTAMP,
    'CoordinatorUsername'
);

-- Dimissioni (+1 mese da Verifica Finale)
INSERT INTO scheduled_visits (
    id,
    project_id,
    visit_type_id,
    scheduled_date,
    status,
    created_at,
    created_by
) VALUES (
    'GUID-visit-4',
    'GUID-progetto',
    'DISCHARGE',
    DATE('2026-04-02', '+1 month'),  -- 2026-05-02
    'Scheduled',
    CURRENT_TIMESTAMP,
    'CoordinatorUsername'
);

COMMIT;
```

**Risultato:**
- Progetto creato e associato al paziente
- Educatori assegnati al progetto
- 4 visite programmate generate automaticamente
- Paziente ora visibile nella lista educatori con i nomi assegnati

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

**Query Database:**
```sql
SELECT 
    sv.id,
    sv.scheduled_date,
    vt.code as visit_type_code,
    vt.description as visit_type_desc,
    p.first_name || ' ' || p.last_name as patient_name,
    tp.title as project_title,
    GROUP_CONCAT(o.first_name || ' ' || o.last_name) as educators
FROM scheduled_visits sv
JOIN visit_types vt ON sv.visit_type_id = vt.id
JOIN therapeutic_projects tp ON sv.project_id = tp.id
JOIN patients p ON tp.patient_id = p.id
JOIN project_operators po ON tp.id = po.project_id
JOIN operators o ON po.operator_id = o.id
WHERE 
    sv.scheduled_date BETWEEN '2025-04-01' AND '2025-04-30'
    AND sv.status = 'Scheduled'
GROUP BY sv.id
ORDER BY sv.scheduled_date, p.last_name;
```

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

**Query Database (Filtrata per Educatore):**
```sql
SELECT 
    sv.id,
    sv.scheduled_date,
    vt.code as visit_type_code,
    vt.description as visit_type_desc,
    p.first_name || ' ' || p.last_name as patient_name,
    tp.title as project_title,
    GROUP_CONCAT(
        CASE 
            WHEN o.id != ? THEN o.first_name || ' ' || o.last_name 
        END
    ) as co_educators
FROM scheduled_visits sv
JOIN visit_types vt ON sv.visit_type_id = vt.id
JOIN therapeutic_projects tp ON sv.project_id = tp.id
JOIN patients p ON tp.patient_id = p.id
JOIN project_operators po ON tp.id = po.project_id
JOIN operators o ON po.operator_id = o.id
WHERE 
    po.operator_id = ?  -- ID Educatore corrente
    AND sv.status = 'Scheduled'
    AND sv.scheduled_date >= DATE('now')
GROUP BY sv.id
ORDER BY sv.scheduled_date ASC;
```

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
- Data effettiva non futura
- Ora fine > ora inizio
- Almeno un operatore selezionato
- Note cliniche obbligatorie (min 50 caratteri)
- Presenza paziente obbligatoria

**Operazioni Database (Transazione Atomica):**

```sql
BEGIN TRANSACTION;

-- 1. Crea visita effettiva (1:1 con scheduled_visit)
INSERT INTO actual_visits (
    id,
    scheduled_visit_id,
    actual_date,
    actual_start_time,
    actual_end_time,
    source,  -- IMPORTANTE: 'EducatorImport' per audit
    registration_date,
    clinical_notes,
    outcomes,
    attendance_status,
    created_at,
    created_by
) VALUES (
    'GUID-actual-visit',
    'GUID-scheduled-visit',
    '2025-04-02',
    '10:00:00',
    '11:30:00',
    'EducatorImport',  -- ← Tracciamento fonte
    CURRENT_TIMESTAMP,
    'Il paziente si è presentato puntuale...',
    'Obiettivo 1: Migliorare autonomia...',
    'Present',
    CURRENT_TIMESTAMP,
    'Corrias'
);

-- 2. Associa operatori presenti (N:N)
INSERT INTO actual_visit_operators (
    id,
    actual_visit_id,
    operator_id,
    role_in_visit,
    created_at
) VALUES 
    (
        'GUID-1',
        'GUID-actual-visit',
        'GUID-Corrias',
        'Lead',  -- Registrante
        CURRENT_TIMESTAMP
    ),
    (
        'GUID-2',
        'GUID-actual-visit',
        'GUID-Gargiulo',
        'Assistant',
        CURRENT_TIMESTAMP
    );

-- 3. Aggiorna stato visita programmata
UPDATE scheduled_visits
SET 
    status = 'Completed',
    updated_at = CURRENT_TIMESTAMP,
    updated_by = 'Corrias'
WHERE id = 'GUID-scheduled-visit';

COMMIT;
```

**Audit Trail Automatico:**
- `source = 'EducatorImport'` identifica origine dati
- `created_by = 'Corrias'` traccia chi ha registrato
- Timestamp completo per risoluzione conflitti sync
- Relazione 1:1 garantisce una sola registrazione per visita programmata

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
│ [ ] 0 Nuovi pazienti (solo Coordinatore può crearli)     │
│ [ ] 0 Nuovi progetti (solo Coordinatore può crearli)     │
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

**Business Logic - Raccolta Dati:**

```csharp
public async Task<SyncPacket> CreateExportPacketAsync(
    string operatorUsername, 
    DateTime lastSyncDate)
{
    // Recupera solo le visite registrate dall'educatore
    var actualVisits = await _repository.GetActualVisitsAsync(
        createdBy: operatorUsername,
        updatedAfter: lastSyncDate
    );
    
    // Recupera relazioni operatori per quelle visite
    var visitOperators = await _repository
        .GetVisitOperatorsByActualVisitIdsAsync(
            actualVisits.Select(v => v.Id)
        );
    
    // Crea struttura JSON
    var packet = new SyncPacket
    {
        PacketId = Guid.NewGuid(),
        SourceOperator = operatorUsername,
        CreatedAt = DateTime.UtcNow,
        SchemaVersion = "1.0",
        Entities = new Dictionary<string, object>
        {
            ["actual_visits"] = actualVisits,
            ["actual_visit_operators"] = visitOperators
        }
    };
    
    return packet;
}
```

**Crittografia e Firma:**

```csharp
public byte[] EncryptAndSignPacket(SyncPacket packet)
{
    // 1. Serializza JSON
    var json = JsonSerializer.Serialize(packet);
    
    // 2. Cripta con AES-256
    var encrypted = _encryptionService.Encrypt(
        json, 
        _masterKey
    );
    
    // 3. Firma con HMAC-SHA256
    var signature = _hmacService.ComputeSignature(
        encrypted, 
        _hmacKey
    );
    
    // 4. Combina payload + firma
    var finalPacket = new byte[encrypted.Length + signature.Length];
    Buffer.BlockCopy(encrypted, 0, finalPacket, 0, encrypted.Length);
    Buffer.BlockCopy(signature, 0, finalPacket, encrypted.Length, signature.Length);
    
    return finalPacket;
}
```

**Risultato:**
```
┌────────────────────────────────────────────────────────────┐
│ ✅ Pacchetto Creato con Successo                          │
├────────────────────────────────────────────────────────────┤
│                                                            │
│ File: sync_corrias_20250405_183000.ptrp                   │
│ Dimensione: 287 KB                                        │
│ Firma: 3a7f2b9c... (verificabile)                         │
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
│ ✅ Firma HMAC valida                                       │
│ ✅ Crittografia verificata                                 │
│ ✅ Schema compatibile (v1.0)                               │
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

**Business Logic - Merge Dati:**

```csharp
public async Task<MergeResult> ImportAndMergePacketAsync(
    SyncPacket packet)
{
    var result = new MergeResult();
    
    // Verifica firma
    if (!_hmacService.VerifySignature(packet))
    {
        throw new SecurityException("Firma HMAC non valida");
    }
    
    using var transaction = await _dbContext.Database
        .BeginTransactionAsync();
    
    try
    {
        // Merge visite effettive
        foreach (var actualVisit in packet.Entities["actual_visits"])
        {
            var existingVisit = await _repository
                .FindByIdAsync(actualVisit.Id);
            
            if (existingVisit == null)
            {
                // Nuova visita → INSERT
                await _repository.AddAsync(actualVisit);
                result.Inserted++;
            }
            else
            {
                // Visita esistente → risoluzione conflitti
                var resolved = _conflictResolver.Resolve(
                    existingVisit, 
                    actualVisit
                );
                
                if (resolved.HasConflict)
                {
                    result.Conflicts.Add(resolved);
                }
                
                if (resolved.ShouldUpdate)
                {
                    await _repository.UpdateAsync(actualVisit);
                    result.Updated++;
                }
                else
                {
                    result.Skipped++;
                }
            }
        }
        
        // Merge relazioni operatori
        foreach (var visitOp in packet.Entities["actual_visit_operators"])
        {
            var exists = await _repository.VisitOperatorExistsAsync(
                visitOp.ActualVisitId,
                visitOp.OperatorId
            );
            
            if (!exists)
            {
                await _repository.AddVisitOperatorAsync(visitOp);
                result.Inserted++;
            }
        }
        
        // Salva log sincronizzazione
        await _syncLogRepository.AddAsync(new SyncLog
        {
            PacketId = packet.PacketId,
            SourceOperator = packet.SourceOperator,
            ImportedAt = DateTime.UtcNow,
            EntitiesInserted = result.Inserted,
            EntitiesUpdated = result.Updated,
            ConflictsResolved = result.Conflicts.Count
        });
        
        await transaction.CommitAsync();
        
        return result;
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}
```

**Conflict Resolution (Master-Slave):**

```csharp
public ConflictResolution Resolve(
    ActualVisit localVisit, 
    ActualVisit incomingVisit)
{
    // Coordinatore ha sempre priorità
    if (localVisit.Source == "CoordinatorDirect" && 
        incomingVisit.Source == "EducatorImport")
    {
        return new ConflictResolution
        {
            HasConflict = true,
            ShouldUpdate = false,  // Mantieni versione Coordinatore
            Winner = "Coordinator",
            Reason = "Coordinator has master authority"
        };
    }
    
    // Altrimenti usa timestamp
    if (incomingVisit.UpdatedAt > localVisit.UpdatedAt)
    {
        return new ConflictResolution
        {
            HasConflict = false,
            ShouldUpdate = true,
            Winner = "Incoming",
            Reason = "Newer timestamp"
        };
    }
    
    return new ConflictResolution
    {
        HasConflict = false,
        ShouldUpdate = false,
        Winner = "Local",
        Reason = "Local version is newer"
    };
}
```

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

## 🔐 FLUSSO 6: Controllo Accessi (RBAC)

### Scenario: Educatore tenta operazione non autorizzata

#### Step 11: Verifica Permessi

**Azione Utente:** Educatore tenta di modificare dati paziente

**Business Logic:**

```csharp
public class AuthorizationService
{
    public bool CanEditPatient(Operator currentUser, Patient patient)
    {
        // Solo Coordinatori possono modificare anagrafiche
        if (currentUser.Role != OperatorRole.Coordinator)
        {
            _auditLogger.LogUnauthorizedAccess(
                currentUser.Username,
                "EDIT_PATIENT",
                patient.Id,
                "DENIED: Insufficient permissions"
            );
            
            return false;
        }
        
        return true;
    }
    
    public bool CanRegisterVisit(
        Operator currentUser, 
        ScheduledVisit visit)
    {
        // Educatori possono registrare solo visite dei propri progetti
        if (currentUser.Role == OperatorRole.Educator)
        {
            var isAssigned = _projectRepository
                .IsOperatorAssignedToProject(
                    currentUser.Id,
                    visit.ProjectId
                );
            
            if (!isAssigned)
            {
                _auditLogger.LogUnauthorizedAccess(
                    currentUser.Username,
                    "REGISTER_VISIT",
                    visit.Id,
                    "DENIED: Not assigned to project"
                );
                
                return false;
            }
        }
        
        // Coordinatori hanno accesso globale
        return true;
    }
    
    public bool CanViewPatient(Operator currentUser, Patient patient)
    {
        // Coordinatori vedono tutti
        if (currentUser.Role == OperatorRole.Coordinator)
            return true;
        
        // Educatori vedono solo pazienti dei loro progetti
        var hasActiveProject = _projectRepository
            .HasActiveProjectWithOperator(
                patient.Id,
                currentUser.Id
            );
        
        return hasActiveProject;
    }
}
```

**Matrice Permessi:**

| Operazione | Educatore | Coordinatore |
|------------|-----------|---------------|
| **Pazienti** |
| Visualizzare tutti | ❌ | ✅ |
| Visualizzare assegnati | ✅ | ✅ |
| Creare/Modificare | ❌ | ✅ |
| Eliminare | ❌ | ✅ |
| **Progetti** |
| Visualizzare tutti | ❌ | ✅ |
| Visualizzare assegnati | ✅ | ✅ |
| Creare | ❌ | ✅ |
| Assegnare educatori | ❌ | ✅ |
| Modificare stato | ❌ | ✅ |
| **Visite** |
| Visualizzare tutte | ❌ | ✅ |
| Visualizzare proprie | ✅ | ✅ |
| Registrare (propri progetti) | ✅ | ✅ |
| Registrare (altri progetti) | ❌ | ✅ |
| Modificare registrate | ❌ | ✅ |
| **Educatori** |
| Visualizzare lista | ✅ | ✅ |
| Creare/Modificare | ❌ | ✅ |
| **Sincronizzazione** |
| Esportare propri dati | ✅ | ✅ |
| Importare dati altrui | ❌ | ✅ |

**UI Messaggio Errore:**
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

**Query Database Complessa:**

```sql
-- KPI Principali
WITH kpi_stats AS (
    SELECT 
        (SELECT COUNT(*) FROM patients WHERE status = 'Active') as active_patients,
        (SELECT COUNT(*) FROM therapeutic_projects WHERE status = 'In Progress') as active_projects,
        (SELECT COUNT(DISTINCT o.id) 
         FROM operators o 
         JOIN project_operators po ON o.id = po.operator_id 
         WHERE EXISTS (
             SELECT 1 FROM therapeutic_projects tp 
             WHERE tp.id = po.project_id AND tp.status = 'In Progress'
         )
        ) as active_operators
),

-- Statistiche Visite Mese Corrente
visit_stats AS (
    SELECT 
        sv.status,
        COUNT(*) as count
    FROM scheduled_visits sv
    WHERE STRFTIME('%Y-%m', sv.scheduled_date) = STRFTIME('%Y-%m', 'now')
    GROUP BY sv.status
),

-- Top Educatori
top_educators AS (
    SELECT 
        o.first_name || ' ' || o.last_name as educator_name,
        COUNT(DISTINCT avo.actual_visit_id) as visits_count
    FROM actual_visit_operators avo
    JOIN operators o ON avo.operator_id = o.id
    JOIN actual_visits av ON avo.actual_visit_id = av.id
    WHERE STRFTIME('%Y-%m', av.actual_date) = STRFTIME('%Y-%m', 'now')
    GROUP BY o.id
    ORDER BY visits_count DESC
    LIMIT 10
)

SELECT * FROM kpi_stats, visit_stats, top_educators;
```

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
   - **Educatori mostrati via progetto attivo**

3. **ProjectDetailView** 🚧
   - Form creazione progetto
   - Assegnazione educatori (N:N)
   - Generazione automatica 4 visite programmate
   - Validazione unicità progetto attivo

4. **CalendarView** 🔲
   - Calendario mensile visite
   - Lista giornaliera visite programmate
   - Filtri per tipo visita ed educatore

5. **VisitRegistrationView** 🔲
   - Form registrazione visita effettiva
   - Selezione operatori presenti
   - Note cliniche obbligatorie
   - Tracking `source = 'EducatorImport'`

#### FASE 2 - Core Functionality

6. **SyncView** 🔲
   - Tab Esporta: creazione pacchetto .ptrp
   - Tab Importa: verifica e merge dati
   - Visualizzazione conflitti
   - Log sincronizzazioni

7. **AuthorizationLayer** 🔲
   - Implementazione RBAC
   - Controlli accesso su tutte le view
   - Messaggi errore user-friendly
   - Audit log automatico

8. **OperatorManagementView** 🔲
   - Lista educatori
   - Form CRUD educatore
   - Visualizzazione progetti assegnati

#### FASE 3 - Advanced Features

9. **DashboardView** 🔲
   - KPI principali
   - Grafici trend mensili
   - Top educatori
   - Export PDF

10. **ReportingModule** 🔲
    - Report personalizzabili
    - Export Excel/PDF
    - Grafici interattivi

---

## 🎯 PUNTI CHIAVE ARCHITETTURALI

### 1. Relazione Paziente-Educatore

**❌ NON esiste tabella `patient_operators`**

**✅ Relazione implicita via progetto:**
```sql
-- Query educatori di un paziente
SELECT DISTINCT o.id, o.first_name, o.last_name
FROM operators o
JOIN project_operators po ON o.id = po.operator_id
JOIN therapeutic_projects tp ON po.project_id = tp.id
WHERE tp.patient_id = ?
  AND tp.status = 'Active';  -- Solo progetto attivo
```

### 2. Unicità Progetto Attivo

**Constraint Business Logic (NON database):**
```csharp
public async Task<ValidationResult> ValidateNewProjectAsync(
    Guid patientId)
{
    var activeProjects = await _repository
        .GetProjectsByPatientIdAndStatusAsync(
            patientId, 
            ProjectStatus.Active
        );
    
    if (activeProjects.Any())
    {
        return ValidationResult.Failure(
            "Il paziente ha già un progetto attivo. " +
            "Completa o sospendi il progetto corrente."
        );
    }
    
    return ValidationResult.Success();
}
```

### 3. Ruoli Semplificati

**Solo 2 ruoli:**
- `OperatorRole.Coordinator` (Master)
- `OperatorRole.Educator` (Slave)

**NON esiste `Supervisor`**

**Tabella `project_operators` semplificata:**
```sql
CREATE TABLE project_operators (
    id              TEXT PRIMARY KEY,
    project_id      TEXT NOT NULL,
    operator_id     TEXT NOT NULL,
    created_at      DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by      TEXT,
    
    FOREIGN KEY (project_id) REFERENCES therapeutic_projects(id) ON DELETE CASCADE,
    FOREIGN KEY (operator_id) REFERENCES operators(id) ON DELETE RESTRICT,
    
    UNIQUE(project_id, operator_id)
);
```

**Rimosso:** Campo `role_in_project` (non necessario)

### 4. Audit Trail Source

**Discriminazione fonte dati:**
- `source = 'EducatorImport'` → registrato da Educatore
- `source = 'CoordinatorDirect'` → registrato da Coordinatore

**Usato per:**
- Conflict resolution (Coordinatore vince)
- Audit e tracciabilità
- UI badge visivi

---

## 🔗 RIFERIMENTI DOCUMENTAZIONE

- [README.md](../README.md) - Panoramica progetto
- [ARCHITECTURE.md](ARCHITECTURE.md) - Pattern MVVM
- [DATABASE.md](DATABASE.md) - Schema database dettagliato
- [SECURITY.md](SECURITY.md) - RBAC e crittografia
- [SEED.md](SEED.md) - Dati iniziali
- [DEVELOPMENT.md](DEVELOPMENT.md) - Guida sviluppatori

---

**Documento creato:** 30 Gennaio 2026  
**Versione:** 1.0  
**Autore:** Marco Cavallo (@artcava)
