namespace PTRP.Services.Interfaces;

/// <summary>
/// Servizio per gestione configurazione iniziale e profilo utente
/// </summary>
public interface IConfigurationService
{
    /// <summary>
    /// Controlla se l'applicazione è già configurata
    /// Verifica esistenza database e presenza operatori
    /// </summary>
    Task<bool> IsConfiguredAsync();
    
    /// <summary>
    /// Valida integrità e firma HMAC del pacchetto .ptrp
    /// </summary>
    /// <param name="filePath">Percorso file pacchetto</param>
    /// <returns>True se valido, False se corrotto o manomesso</returns>
    Task<bool> ValidatePackageAsync(string filePath);
    
    /// <summary>
    /// Importa configurazione da pacchetto .ptrp
    /// Deserializza e decrittografa dati
    /// </summary>
    /// <param name="filePath">Percorso file pacchetto</param>
    /// <returns>ConfigurationPackage con dati importati</returns>
    Task<ConfigurationPackage> ImportConfigurationAsync(string filePath);
    
    /// <summary>
    /// Inizializza database locale con dati del pacchetto
    /// Crea schema e popola tabelle iniziali
    /// </summary>
    /// <param name="config">Pacchetto configurazione</param>
    Task InitializeDatabaseAsync(ConfigurationPackage config);
    
    /// <summary>
    /// Configura profilo utente locale
    /// Crea operatore con ruolo Coordinatore o Educatore
    /// </summary>
    /// <param name="config">Pacchetto configurazione</param>
    Task SetupUserProfileAsync(ConfigurationPackage config);
    
    /// <summary>
    /// Ottiene ruolo utente corrente
    /// </summary>
    Task<string> GetCurrentUserRoleAsync();
    
    /// <summary>
    /// Ottiene nome completo utente corrente
    /// </summary>
    Task<string> GetCurrentUserFullNameAsync();
}

/// <summary>
/// Rappresenta un pacchetto di configurazione .ptrp
/// </summary>
public class ConfigurationPackage
{
    /// <summary>
    /// Tipo pacchetto: "admin" o "appointments"
    /// </summary>
    public string PackageType { get; set; } = string.Empty;
    
    /// <summary>
    /// Ruolo utente: "Coordinatore" o "Educatore"
    /// </summary>
    public string UserRole { get; set; } = string.Empty;
    
    /// <summary>
    /// Nome completo utente
    /// </summary>
    public string UserName { get; set; } = string.Empty;
    
    /// <summary>
    /// Data esportazione pacchetto
    /// </summary>
    public DateTime ExportDate { get; set; }
    
    /// <summary>
    /// Dati serializzati (pazienti, progetti, operatori, ecc.)
    /// </summary>
    public byte[] Data { get; set; } = Array.Empty<byte>();
    
    /// <summary>
    /// Firma HMAC per validazione integrità
    /// </summary>
    public byte[] Signature { get; set; } = Array.Empty<byte>();
}
