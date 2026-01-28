# SECURITY.md - Architettura di Sicurezza PTRP

## 📋 Panoramica

Questo documento descrive la strategia di sicurezza per l'applicazione PTRP, con particolare attenzione a:
- **Crittografia** dei dati sensibili in transito e a riposo
- **Autenticazione e autorizzazione** nel contesto offline-first
- **Protezione dei pacchetti di scambio** tramite firma digitale (HMAC)
- **Privacy** dei dati clinici sensibili
- **Audit e tracciabilità** delle operazioni
- **Compliance** con normative GDPR e sanitarie

---

## 🔒 Principi di Sicurezza

### Defense in Depth
Il sistema implementa **molteplici livelli di protezione**:
1. **Database a riposo**: Crittografia AES-256 del file SQLite
2. **Dati in transito**: HMAC-SHA256 per integrità + AES per confidenzialità
3. **Accesso applicativo**: Controllo permessi e audit trail
4. **Key management**: Derivazione da password locale, mai hardcoded

### Zero Trust Architecture
Nel contesto offline-first:
- Ogni pacchetto di scambio è **verificato per integrità** prima dell'importazione
- La **risoluzione dei conflitti** ha garanzie deterministiche (Master-Slave logic)
- L'**audit trail completo** permette il ripudio di operazioni non autorizzate

### Privacy by Design
- Minimizzazione dei dati sensibili nel seed
- Pseudoanonimizzazione in ambienti di sviluppo
- GDPR-ready con diritto all'oblio implementabile

---

## 🔐 Crittografia dei Dati

### 1. Database Locale (SQLite Criptato)

#### Standard
- **Algoritmo**: AES-256 in modalità CBC
- **Key derivation**: PBKDF2 con salt casuale (almeno 128 bit)
- **IV (Initialization Vector)**: Generato casualmente per ogni sessione
- **Implementazione**: SQLite native encryption o Entity Framework Core con extension

#### Implementazione Raccomandata

```csharp
public sealed class DatabaseEncryptionService
{
    private readonly byte[] _masterKey;
    private readonly string _databasePath;
    private const int KeySize = 32; // 256 bits
    private const int SaltSize = 16; // 128 bits
    private const int Iterations = 10000; // PBKDF2 iterations

    public DatabaseEncryptionService(string password, string databasePath)
    {
        _databasePath = databasePath;
        _masterKey = DeriveKeyFromPassword(password);
    }

    private byte[] DeriveKeyFromPassword(string password)
    {
        using (var rng = new RNGCryptoServiceProvider())
        {
            byte[] salt = new byte[SaltSize];
            rng.GetBytes(salt);

            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
            {
                return pbkdf2.GetBytes(KeySize);
            }
        }
    }

    public void EncryptDatabase()
    {
        // SQLite native encryption pragmas
        // pragma key = 'password';
        // pragma cipher = 'aes-256-cbc';
    }

    public void DecryptDatabase()
    {
        // Decryption happens automatically at connection time
    }
}
```

#### Security Considerations
- La **password locale** è nota solo all'utente che installa l'app
- Il **salt è casuale** e immagazzinato con il database
- **PBKDF2 iterations**: almeno 10,000 (NIST recommendation)
- **Key stretching**: previene attacchi brute-force

### 2. Pacchetti di Scambio (Sincronizzazione)

#### Struttura del Pacchetto

```json
{
  "packet_id": "<guid-univoco>",
  "source": "Coordinator|Educator",
  "timestamp": "2026-01-28T18:04:00Z",
  "version": "1.0",
  "payload_encrypted": "<base64-encoded-AES-ciphertext>",
  "hmac_signature": "<base64-encoded-HMAC-SHA256>",
  "payload_hash_algorithm": "SHA256"
}
```

#### Implementazione HMAC

```csharp
public sealed class SyncPacketSigningService
{
    private readonly byte[] _hmacKey;
    private const int HmacKeySize = 32; // 256 bits

    public SyncPacketSigningService(byte[] masterKey)
    {
        // Derive HMAC key from master encryption key
        _hmacKey = DeriveHmacKey(masterKey);
    }

    public string SignPacket(string jsonPayload)
    {
        using (var hmac = new HMACSHA256(_hmacKey))
        {
            byte[] payloadBytes = Encoding.UTF8.GetBytes(jsonPayload);
            byte[] signature = hmac.ComputeHash(payloadBytes);
            return Convert.ToBase64String(signature);
        }
    }

    public bool VerifyPacketSignature(string jsonPayload, string signature)
    {
        string computedSignature = SignPacket(jsonPayload);
        return CryptographicOperations.FixedTimeEquals(
            Convert.FromBase64String(signature),
            Convert.FromBase64String(computedSignature)
        );
    }

    private byte[] DeriveHmacKey(byte[] masterKey)
    {
        // HKDF-SHA256 per derivare HMAC key da master key
        using (var hkdf = new HKDFWithSHA256(masterKey, null, "HMAC_KEY".ToUtf8()))
        {
            return hkdf.GetBytes(HmacKeySize);
        }
    }
}
```

#### Protezione da Replay Attacks
- **Timestamp** nel pacchetto
- **GUID univoco** per ogni pacchetto (impedisce duplicati)
- **Versione schema** dichiarata nel pacchetto
- **Reject policy**: pacchetti con timestamp antecedente all'ultima sincronizzazione

### 3. Crittografia in Memoria

```csharp
public sealed class SensitiveDataProtector : IDisposable
{
    private GCHandle _handle;
    private byte[] _buffer;

    public SensitiveDataProtector(string sensitiveData)
    {
        _buffer = Encoding.UTF8.GetBytes(sensitiveData);
        _handle = GCHandle.Alloc(_buffer, GCHandleType.Pinned);
        ProtectMemory(_buffer);
    }

    private void ProtectMemory(byte[] data)
    {
        // Windows: DataProtectionScope.CurrentUser
        ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
    }

    public void Dispose()
    {
        Array.Clear(_buffer, 0, _buffer.Length);
        _handle.Free();
        GC.SuppressFinalize(this);
    }
}
```

Dati sensibili (es. password, chiavi) vanno:
- Memorizzati in array di byte (non string, immutable in .NET)
- Protetti con `ProtectedData` su Windows
- Cancellati dalla memoria subito dopo l'uso
- Mai loggati in chiaro

---

## 🔑 Gestione Delle Chiavi

### Derivazione della Chiave Master

```csharp
public sealed class MasterKeyManager
{
    public static byte[] DeriveApplicationMasterKey(
        string userPassword,
        byte[] salt = null,
        int iterations = 10000)
    {
        salt ??= GenerateRandomSalt(16);

        using (var pbkdf2 = new Rfc2898DeriveBytes(
            userPassword,
            salt,
            iterations,
            HashAlgorithmName.SHA256))
        {
            return pbkdf2.GetBytes(32); // 256-bit key
        }
    }

    private static byte[] GenerateRandomSalt(int size)
    {
        using (var rng = new RNGCryptoServiceProvider())
        {
            byte[] salt = new byte[size];
            rng.GetBytes(salt);
            return salt;
        }
    }
}
```

### Key Rotation Policy

La **rotazione della chiave** è necessaria quando:
1. **Compromissione sospetta** della chiave
2. **Cambio password** dell'utente
3. **Migrazione schema** tra versioni app (opzionale ma consigliato)

**Procedura di rotazione**:
1. Decripta database con vecchia chiave
2. Rideriva nuova chiave da nuova password
3. Ricripta tutto con nuova chiave
4. Cancella vecchia chiave dalla memoria

### Dove NON Memorizzare Chiavi

❌ **MAI**:
- Hardcoded nel codice sorgente
- In file config (JSON, XML) in chiaro
- In registry Windows in chiaro
- In variabili di ambiente (troppo visibili)
- In commit git (usare `.gitignore`)

✅ **SI'**:
- Derivate da password utente (PBKDF2)
- In memoria protetta (ProtectedData)
- In key vault aziendali (per produzione)
- Separate per ambiente (dev/test/prod)

---

## 🛡️ Controllo di Accesso e Autorizzazione

### Role-Based Access Control (RBAC)

```csharp
public enum OperatorRole
{
    Educator,     // Può registrare visite, consultare pazienti assegnati
    Coordinator,  // Master globale, può modificare anagrafiche e assegnazioni
    Supervisor    // Audit e report, accesso read-only su tutti i dati
}

public sealed record ProjectOperator
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; init; }
    public Guid OperatorId { get; init; }
    public string RoleInProject { get; init; } // Primary | Assistant | Supervisor
    public DateTime AssignmentDate { get; init; }
    public DateTime? EndDate { get; init; }
}
```

### Matrice di Permessi

| Operazione | Educator | Coordinator | Supervisor |
|------------|----------|-------------|------------|
| Visualizzare pazienti assegnati | ✅ | ✅ | ✅ |
| Visualizzare tutti i pazienti | ❌ | ✅ | ✅ (read-only) |
| Registrare visita personale | ✅ | ✅ | ❌ |
| Registrare visita per altro educatore | ❌ | ✅ | ❌ |
| Modificare anagrafica paziente | ❌ | ✅ | ❌ |
| Assegnare progetto | ❌ | ✅ | ❌ |
| Generare report | ✅ (propri) | ✅ | ✅ |
| Esportare dati | ❌ | ❌ | ✅ |

### Implementazione del Controllo

```csharp
public sealed class AuthorizationService
{
    public bool CanRegisterVisit(Operator operator, ActualVisit visit)
    {
        if (operator.Role == OperatorRole.Educator)
        {
            // Educatore può registrare solo proprie visite
            return visit.RegisteredBy == operator.Id.ToString();
        }

        if (operator.Role == OperatorRole.Coordinator)
        {
            // Coordinatore può registrare qualunque visita
            return true;
        }

        // Supervisor non può registrare
        return false;
    }

    public bool CanViewPatient(Operator operator, Patient patient)
    {
        if (operator.Role == OperatorRole.Coordinator ||
            operator.Role == OperatorRole.Supervisor)
        {
            return true; // Accesso globale
        }

        // Educatore vede solo pazienti a cui è assegnato
        return IsAssignedToPatient(operator.Id, patient.Id);
    }

    private bool IsAssignedToPatient(Guid operatorId, Guid patientId)
    {
        // Controlla relazione N:N via project_operators
        return _context.ProjectOperators
            .Any(po => po.OperatorId == operatorId &&
                       po.Project.PatientId == patientId &&
                       (po.EndDate == null || po.EndDate > DateTime.UtcNow));
    }
}
```

---

## 📋 Audit e Tracciabilità

### Audit Trail Schema

Ogni entità clinica critica deve avere:

```csharp
public abstract record AuditedEntity
{
    public Guid Id { get; init; } = Guid.NewGuid();
    
    // Chi ha creato/modificato
    public Guid CreatedBy { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    
    public Guid? UpdatedBy { get; init; }
    public DateTime? UpdatedAt { get; init; }
    
    // Versionamento per conflict resolution
    public int Version { get; init; } = 1;
    
    // Tracciamento sincronia
    public Guid? SyncPacketId { get; init; } // Quale pacchetto lo ha importato
}

public sealed record ActualVisit : AuditedEntity
{
    public Guid ScheduledVisitId { get; init; }
    public DateTime ActualDate { get; init; }
    public VisitSource Source { get; init; } = VisitSource.CoordinatorDirect;
    public string ClinicalNotes { get; init; } = string.Empty;
}
```

### Log di Sincronizzazione

```csharp
public sealed record SyncLog
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid PacketId { get; init; }
    public string SourceOperator { get; init; } = string.Empty;
    public DateTime SyncDate { get; init; } = DateTime.UtcNow;
    public SyncStatus Status { get; init; } = SyncStatus.Pending;
    public string Details { get; init; } = string.Empty;  // Conflitti risolti, errori, ecc.
    public int ConflictCount { get; init; }
    public int MergedRecords { get; init; }
}

public enum SyncStatus
{
    Pending,
    Completed,
    Failed,
    PartialConflict
}
```

### Interrogazioni di Audit

```csharp
public sealed class AuditService
{
    // "Chi ha modificato questo paziente e quando?"
    public IQueryable<AuditEvent> GetPatientHistory(Guid patientId)
    {
        return _context.AuditLogs
            .Where(log => log.EntityId == patientId)
            .OrderByDescending(log => log.Timestamp);
    }

    // "Quali modifiche ha fatto questo educatore?"
    public IQueryable<AuditEvent> GetOperatorActions(Guid operatorId, DateTime from, DateTime to)
    {
        return _context.AuditLogs
            .Where(log => log.OperatorId == operatorId &&
                          log.Timestamp >= from &&
                          log.Timestamp <= to)
            .OrderByDescending(log => log.Timestamp);
    }

    // "Quali conflitti si sono verificati durante questa sincronizzazione?"
    public IQueryable<ConflictResolutionLog> GetSyncConflicts(Guid syncPacketId)
    {
        return _context.ConflictLogs
            .Where(c => c.SyncPacketId == syncPacketId)
            .OrderByDescending(c => c.ResolvedAt);
    }
}
```

---

## 🔐 Sicurezza in Sviluppo

### Secrets Management

```bash
# Non committare secrets
git config core.hooksPath .githooks

# Usare user-secrets in development
dotnet user-secrets init
dotnet user-secrets set "Database:EncryptionPassword" "<dev-password>"
```

**`.gitignore` includerà sempre**:
```
secrets.json
*.key
*.pem
appsettings.Production.json
.env
```

### Code Scanning (GitHub Actions)

```yaml
name: Security Scan

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main, develop]

jobs:
  security:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      # SAST: Secret scanning
      - name: Secret scanning
        uses: truffleHog/truffleHog@main
        with:
          path: ./
          base: ${{ github.event.repository.default_branch }}
          head: HEAD
      
      # SAST: Dependency check
      - name: Dependency check
        uses: dependency-check/Dependency-Check_Action@main
        with:
          path: '.'
          format: 'SARIF'
      
      # SAST: CodeQL
      - uses: github/codeql-action/init@v2
        with:
          languages: 'csharp'
      
      - uses: github/codeql-action/analyze@v2
```

### Logging Sicuro

```csharp
public static class SecureLogger
{
    // ❌ WRONG
    // _logger.LogInformation($"Processing patient {patient.Name} with SSN {patient.SocialSecurityNumber}");

    // ✅ CORRECT
    public static string Redact(string sensitiveValue, int visibleChars = 4)
    {
        if (string.IsNullOrEmpty(sensitiveValue)) return "***";
        if (sensitiveValue.Length <= visibleChars) return "***";
        
        return sensitiveValue[..visibleChars] + new string('*', sensitiveValue.Length - visibleChars);
    }

    // Usage:
    // _logger.LogInformation("Processing patient {PatientId}", patient.Id); // Solo ID
    // _logger.LogWarning("SSN pattern: {RedactedSSN}", Redact(ssn)); // Redacted
}
```

---

## ⚖️ Compliance e Privacy

### GDPR Requirements

#### Data Minimization
- ✅ Raccogliere SOLO dati clinicamente rilevanti
- ✅ Non includere SSN, indirizzi completi, numeri telefonici nei seed
- ✅ Anonimizzare i dati in ambienti non-produzione

#### Right to be Forgotten (Art. 17)
```csharp
public sealed class GDPRComplianceService
{
    public async Task DeletePatientDataAsync(Guid patientId)
    {
        // 1. Raccogliere tutti i record associati al paziente
        var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Id == patientId);
        if (patient == null) throw new PatientNotFoundException();

        var projects = await _context.TherapeuticProjects
            .Where(p => p.PatientId == patientId)
            .ToListAsync();

        var visits = await _context.ScheduledVisits
            .Where(sv => projects.Contains(sv.Project))
            .ToListAsync();

        // 2. Cancellare in cascata
        foreach (var project in projects)
        {
            _context.TherapeuticProjects.Remove(project);
        }
        
        foreach (var visit in visits)
        {
            _context.ScheduledVisits.Remove(visit);
        }
        
        _context.Patients.Remove(patient);
        
        // 3. Audit trail: registrare la cancellazione
        _context.AuditLogs.Add(new AuditLog
        {
            Operation = "DELETE_PATIENT",
            EntityId = patientId,
            Reason = "GDPR Right to be Forgotten",
            Timestamp = DateTime.UtcNow
        });
        
        await _context.SaveChangesAsync();
    }
}
```

#### Data Portability (Art. 20)
```csharp
public async Task<string> ExportPatientDataAsJsonAsync(Guid patientId)
{
    var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Id == patientId);
    var projects = await _context.TherapeuticProjects
        .Where(p => p.PatientId == patientId)
        .ToListAsync();
    var visits = await _context.ScheduledVisits
        .Where(sv => projects.Select(p => p.Id).Contains(sv.ProjectId))
        .ToListAsync();

    var export = new
    {
        patient,
        projects,
        visits,
        exportDate = DateTime.UtcNow
    };

    return JsonSerializer.Serialize(export, new JsonSerializerOptions { WriteIndented = true });
}
```

### Normative Sanitarie Italiane

#### DPS (Data Protection by Design)
- ✅ Crittografia end-to-end
- ✅ Audit trail completo
- ✅ Accesso basato su ruoli
- ✅ Isolamento dati sensibili

#### Fascicolo Sanitario Elettronico (FSE)
In futuro, PTRP può integrarsi con FSE mantenendo:
- Conformità a standard HL7/FHIR
- Interoperabilità con gateways regionali
- Compatibilità con sistemi di audit sanitari nazionali

---

## 🔍 Security Checklist

### Pre-Development
- [ ] Tutti gli sviluppatori hanno completato security training
- [ ] OWASP Top 10 è noto al team
- [ ] Threat modeling completato per il sistema

### During Development
- [ ] Nessun secret è committato su git
- [ ] Code review ha focus su sicurezza
- [ ] Unit test per funzioni crittografiche
- [ ] Nessun hardcoding di password/chiavi
- [ ] Logging non espone dati sensibili

### Before Release
- [ ] Security scan GitHub Actions passa
- [ ] Dependency vulnerabilities sono zero
- [ ] Penetration test su pacchetti di scambio
- [ ] GDPR assessment completato
- [ ] Documentazione di sicurezza aggiornata

### Post-Release
- [ ] Incident response plan è in vigore
- [ ] Security updates sono applicate entro 24h
- [ ] Audit log è conservato per audit esterni
- [ ] Alerting per anomalie è configurato

---

## 📚 Riferimenti

- **OWASP**: [Top 10 2021](https://owasp.org/www-project-top-ten/)
- **NIST**: [Cybersecurity Framework](https://www.nist.gov/cyberframework)
- **GDPR**: [Art. 32 - Security of Processing](https://gdpr-info.eu/art-32-gdpr/)
- **.NET Security**: [Microsoft Security Best Practices](https://learn.microsoft.com/en-us/dotnet/standard/security/)
- **Cryptography**: [OWASP Cryptographic Storage Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Cryptographic_Storage_Cheat_Sheet.html)
- **FHIR Security**: [HL7 FHIR Security & Privacy](https://www.hl7.org/fhir/security.html)

---

## 📞 Segnalare Vulnerabilità

Se scopri una vulnerabilità:

1. **NON** aprire issue pubblica
2. Contatta: `cavallo.marco@gmail.com` con oggetto **`[SECURITY]`**
3. Includi:
   - Descrizione della vulnerabilità
   - Passi per riprodurla
   - Impatto stimato
4. Aspetta conferma prima di divulgare pubblicamente

---

*Documento di Sicurezza - Progetto PTRP-Sync v1.0*
*Ultimo aggiornamento: January 28, 2026*