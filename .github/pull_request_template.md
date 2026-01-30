## Descrizione

<!-- Descrivi brevemente le modifiche apportate in questa PR -->

## Tipo di Modifica

<!-- Seleziona il tipo di modifica (metti una 'x' nelle parentesi) -->

- [ ] 🐛 Bug fix (modifica non breaking che risolve un problema)
- [ ] ✨ Nuova feature (modifica non breaking che aggiunge funzionalità)
- [ ] 💥 Breaking change (modifica che causa incompatibilità con versioni precedenti)
- [ ] 📝 Documentazione (aggiornamento solo documentazione)
- [ ] 🔧 Refactoring (ristrutturazione codice senza cambiare comportamento)
- [ ] ⚡ Miglioramento performance
- [ ] 🧪 Test (aggiunta o modifica test)
- [ ] 🔨 Configurazione/Build (modifiche a CI/CD, dipendenze, ecc.)

## Modifiche Apportate

<!-- Lista dettagliata delle modifiche -->

- 
- 
- 

## Issue Correlate

<!-- Riferimenti a issue risolte -->

Closes #
Related to #

---

## ✅ Checklist Standard (per tutte le PR)

### Codice
- [ ] Il codice segue le convenzioni del progetto
- [ ] Ho effettuato una self-review del codice
- [ ] Ho commentato le parti di codice complesse
- [ ] Ho rimosso codice commentato o di debug non necessario
- [ ] Non ci sono warning del compilatore

### Test
- [ ] Ho aggiunto/aggiornato i test per coprire le modifiche
- [ ] Tutti i test esistenti passano localmente
- [ ] La coverage non è diminuita (o è giustificato)

### Documentazione
- [ ] Ho aggiornato la documentazione (README, docs/, commenti)
- [ ] Ho aggiornato i commenti XML per le API pubbliche

### Dipendenze
- [ ] Non ho aggiunto dipendenze non necessarie
- [ ] Le nuove dipendenze sono giustificate e documentate

---

## 🚀 Checklist Release (SOLO per merge su `main`)

<!-- ⚠️ OBBLIGATORIA per PR da develop → main -->

### Pre-Release
- [ ] **Versione aggiornata**: `velopack.json` contiene la nuova versione
- [ ] **CHANGELOG aggiornato**: `CHANGELOG.md` include tutte le modifiche di questa release
- [ ] **Tag preparato**: Ho verificato il formato del tag (`v*.*.*`)
- [ ] **Branch protection**: Tutti gli status check sono verdi (Build + Test & Coverage)

### Qualità
- [ ] **Test completi**: Tutti i test unitari e di integrazione passano
- [ ] **Test manuali**: Ho testato manualmente le funzionalità principali
- [ ] **Performance**: Nessun degrado di performance rilevato
- [ ] **Memory leaks**: Nessun memory leak introdotto

### Documentazione Release
- [ ] **Release notes**: Ho preparato le note di rilascio
- [ ] **Breaking changes**: Eventuali breaking changes sono documentati
- [ ] **Migration guide**: Ho preparato guida migrazione (se necessario)

### Database/Configurazione
- [ ] **Migrazioni database**: Migrations testate e verificate
- [ ] **Seed data**: Dati di seed aggiornati se necessario
- [ ] **Configurazioni**: File di configurazione aggiornati

### Build & Deploy
- [ ] **Build Release**: Build in configurazione Release testata
- [ ] **Installer**: Velopack installer generato e testato
- [ ] **Dimensione**: Dimensione installer è ragionevole
- [ ] **Auto-update**: Meccanismo di auto-update verificato

### Sicurezza
- [ ] **Vulnerabilità**: Nessuna vulnerabilità nota nelle dipendenze
- [ ] **Credenziali**: Nessuna credenziale o secret hardcoded
- [ ] **Dati sensibili**: Nessun dato sensibile nei log o nel codice

### Post-Merge Plan
- [ ] **Tag Git**: Creerò tag `vX.Y.Z` subito dopo il merge
- [ ] **Release GitHub**: Creerò release su GitHub con assets
- [ ] **Comunicazione**: Ho un piano per comunicare la release

---

## 📸 Screenshot (se applicabile)

<!-- Aggiungi screenshot per modifiche UI -->

## 🧪 Testing Effettuato

<!-- Descrivi i test manuali effettuati -->

### Ambiente
- OS: 
- .NET Runtime: 
- Database: 

### Test Cases
1. 
2. 
3. 

## 📊 Performance Impact

<!-- Solo se rilevante -->

- Tempo di startup: 
- Utilizzo memoria: 
- Tempo di risposta UI: 

## ⚠️ Breaking Changes

<!-- Lista eventuali breaking changes e come mitigarli -->

Nessuno / Descrizione:

## 🔄 Rollback Plan

<!-- SOLO per merge su main: come fare rollback in caso di problemi -->

1. 
2. 

## 📝 Note Aggiuntive

<!-- Altre informazioni utili per i reviewer -->

---

## 👀 Reviewer Notes

<!-- Area per i commenti del reviewer (se auto-review, usa per note a te stesso) -->

**Focus areas for review:**
- 
- 

**Known issues/limitations:**
- 
