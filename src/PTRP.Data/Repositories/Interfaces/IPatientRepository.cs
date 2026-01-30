using PTRP.Models;

namespace PTRP.Data.Repositories.Interfaces;

/// <summary>
/// Interfaccia per il repository dei Pazienti
/// Definisce le operazioni CRUD e di ricerca per l'entità Patient
/// 
/// NOTA: Ogni operazione CRUD (Add, Update, Delete) effettua il commit atomico.
/// Non è necessario chiamare un metodo SaveChangesAsync() separato.
/// </summary>
public interface IPatientRepository
{
    /// <summary>
    /// Recupera tutti i pazienti dal database
    /// </summary>
    /// <returns>Collezione di tutti i pazienti</returns>
    Task<IEnumerable<PatientModel>> GetAllAsync();

    /// <summary>
    /// Recupera un paziente per ID
    /// </summary>
    /// <param name="id">ID univoco del paziente</param>
    /// <returns>Paziente trovato o null</returns>
    Task<PatientModel?> GetByIdAsync(Guid id);

    /// <summary>
    /// Recupera un paziente con i suoi progetti terapeutici
    /// </summary>
    /// <param name="id">ID univoco del paziente</param>
    /// <returns>Paziente con progetti terapeutici caricati</returns>
    Task<PatientModel?> GetByIdWithProjectsAsync(Guid id);

    /// <summary>
    /// Cerca pazienti per nome e cognome
    /// </summary>
    /// <param name="searchTerm">Termine di ricerca (nome o cognome)</param>
    /// <returns>Collezione di pazienti che corrispondono alla ricerca</returns>
    Task<IEnumerable<PatientModel>> SearchAsync(string searchTerm);

    /// <summary>
    /// Aggiunge un nuovo paziente al database (commit atomico)
    /// </summary>
    /// <param name="patient">Paziente da aggiungere</param>
    /// <returns>Task completato</returns>
    Task AddAsync(PatientModel patient);

    /// <summary>
    /// Aggiorna un paziente esistente (commit atomico)
    /// </summary>
    /// <param name="patient">Paziente con dati aggiornati</param>
    /// <returns>Task completato</returns>
    Task UpdateAsync(PatientModel patient);

    /// <summary>
    /// Elimina un paziente per ID (commit atomico)
    /// </summary>
    /// <param name="id">ID del paziente da eliminare</param>
    /// <returns>True se eliminato, False se non trovato</returns>
    Task<bool> DeleteAsync(Guid id);

    /// <summary>
    /// Verifica se esiste un paziente con l'ID specificato
    /// </summary>
    /// <param name="id">ID del paziente</param>
    /// <returns>True se esiste, False altrimenti</returns>
    Task<bool> ExistsAsync(Guid id);
}
