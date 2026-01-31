using System.IO;
using Microsoft.EntityFrameworkCore;
using PTRP.Data;
using PTRP.Models;
using PTRP.Services.Interfaces;

namespace PTRP.Services;

/// <summary>
/// Servizio per gestione configurazione iniziale e profilo utente
/// </summary>
public class ConfigurationService : IConfigurationService
{
    private readonly PTRPDbContext _dbContext;
    private const string ConfigMarkerFile = ".ptrp_configured";
    
    public ConfigurationService(PTRPDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    /// <summary>
    /// Controlla se l'applicazione è già configurata
    /// Verifica:
    /// 1. Database può connettersi
    /// 2. Esiste almeno un operatore con IsCurrentUser = true
    /// </summary>
    public async Task<bool> IsConfiguredAsync()
    {
        try
        {   
            // Controlla connessione database
            var canConnect = await _dbContext.Database.CanConnectAsync();
            if (!canConnect)
                return false;
            
            // Controlla se esiste operatore corrente (profilo locale)
            var hasCurrentUser = await _dbContext.Operators
                .AnyAsync(o => o.IsCurrentUser);
            
            return hasCurrentUser;
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// Valida firma HMAC del pacchetto .ptrp
    /// TODO: Implementare verifica crittografica in issue Security
    /// </summary>
    public async Task<bool> ValidatePackageAsync(string filePath)
    {
        // Verifica esistenza file
        if (!File.Exists(filePath))
            return false;
        
        // Verifica estensione
        if (!filePath.EndsWith(".ptrp", StringComparison.OrdinalIgnoreCase))
            return false;
        
        // TODO: Implementare verifica firma HMAC
        // 1. Leggere firma dal file
        // 2. Ricalcolare HMAC sui dati
        // 3. Confrontare firme
        
        // Per ora simula verifica con delay
        await Task.Delay(300);
        
        return true;
    }
    
    /// <summary>
    /// Importa configurazione da pacchetto .ptrp
    /// TODO: Implementare deserializzazione e decrittografia in issue Security
    /// </summary>
    public async Task<ConfigurationPackage> ImportConfigurationAsync(string filePath)
    {
        // Simula import con delay
        await Task.Delay(300);
        
        var fileName = Path.GetFileName(filePath).ToLowerInvariant();
        
        // Determina tipo pacchetto da nome file
        var isAdmin = fileName.StartsWith("admin");
        var isAppointments = fileName.StartsWith("appointments");
        
        if (!isAdmin && !isAppointments)
        {
            throw new InvalidOperationException(
                "Nome file non valido. Deve iniziare con 'admin' o 'appointments'");
        }
        
        // TODO: Leggere e deserializzare dati reali dal file
        // Per ora ritorna pacchetto di esempio
        return new ConfigurationPackage
        {
            PackageType = isAdmin ? "admin" : "appointments",
            UserRole = isAdmin ? "Coordinatore" : "Educatore",
            UserName = isAdmin ? "Coordinatore Principale" : "Educatore Professionale",
            ExportDate = DateTime.Now,
            Data = Array.Empty<byte>(),
            Signature = Array.Empty<byte>()
        };
    }
    
    /// <summary>
    /// Inizializza database locale con dati del pacchetto
    /// </summary>
    public async Task InitializeDatabaseAsync(ConfigurationPackage config)
    {
        // Crea database se non esiste
        await _dbContext.Database.EnsureCreatedAsync();
        
        // Applica eventuali migrations pending
        var pendingMigrations = await _dbContext.Database.GetPendingMigrationsAsync();
        if (pendingMigrations.Any())
        {
            await _dbContext.Database.MigrateAsync();
        }
        
        // TODO: Popolare database con dati dal pacchetto
        // Per pacchetto admin:
        //   - Lista operatori
        //   - Lista pazienti
        //   - Progetti terapeutici
        // Per pacchetto appointments:
        //   - Appuntamenti assegnati all'educatore
        //   - Pazienti correlati
        
        // Simula popolazione dati
        await Task.Delay(300);
    }
    
    /// <summary>
    /// Configura profilo utente locale
    /// Crea operatore con flag IsCurrentUser = true
    /// </summary>
    public async Task SetupUserProfileAsync(ConfigurationPackage config)
    {
        // Verifica che non esista già un utente corrente
        var existingCurrentUser = await _dbContext.Operators
            .FirstOrDefaultAsync(o => o.IsCurrentUser);
        
        if (existingCurrentUser != null)
        {
            throw new InvalidOperationException(
                "Esiste già un profilo utente configurato");
        }
        
        // Crea nuovo operatore per profilo locale
        var currentUser = new Operator
        {
            Id = Guid.NewGuid(),
            FirstName = config.UserName,
            LastName = string.Empty,
            Role = config.UserRole,
            IsCurrentUser = true,
            Email = string.Empty,
            Phone = string.Empty,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            SyncStatus = SyncStatus.Synced
        };
        
        _dbContext.Operators.Add(currentUser);
        await _dbContext.SaveChangesAsync();
    }
    
    /// <summary>
    /// Ottiene ruolo utente corrente
    /// </summary>
    public async Task<string> GetCurrentUserRoleAsync()
    {
        var currentUser = await _dbContext.Operators
            .FirstOrDefaultAsync(o => o.IsCurrentUser);
        
        return currentUser?.Role ?? "Coordinatore";
    }
    
    /// <summary>
    /// Ottiene nome completo utente corrente
    /// </summary>
    public async Task<string> GetCurrentUserFullNameAsync()
    {
        var currentUser = await _dbContext.Operators
            .FirstOrDefaultAsync(o => o.IsCurrentUser);
        
        if (currentUser == null)
            return "Utente";
        
        var fullName = $"{currentUser.FirstName} {currentUser.LastName}".Trim();
        return string.IsNullOrEmpty(fullName) ? "Utente" : fullName;
    }
}
