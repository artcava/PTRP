using PTRP.Models;

namespace PTRP.Data.Repositories.Interfaces;

/// <summary>
/// Interfaccia per il repository dei Progetti Terapeutici
/// Definisce le operazioni CRUD e di ricerca per l'entità TherapyProject
/// 
/// NOTA: Ogni operazione CRUD (Add, Update, Delete) effettua il commit atomico.
/// Non è necessario chiamare un metodo SaveChangesAsync() separato.
/// </summary>
public interface ITherapyProjectRepository
{
    /// <summary>
    /// Recupera tutti i progetti terapeutici dal database
    /// </summary>
    /// <returns>Collezione di tutti i progetti</returns>
    Task<IEnumerable<TherapyProjectModel>> GetAllAsync();

    /// <summary>
    /// Recupera un progetto terapeutico per ID
    /// </summary>
    /// <param name="id">ID univoco del progetto</param>
    /// <returns>Progetto trovato o null</returns>
    Task<TherapyProjectModel?> GetByIdAsync(Guid id);

    /// <summary>
    /// Recupera un progetto con il paziente associato (eager loading)
    /// </summary>
    /// <param name="id">ID univoco del progetto</param>
    /// <returns>Progetto con paziente caricato</returns>
    Task<TherapyProjectModel?> GetByIdWithPatientAsync(Guid id);

    /// <summary>
    /// Recupera un progetto con paziente ed educatori (eager loading completo)
    /// </summary>
    /// <param name="id">ID univoco del progetto</param>
    /// <returns>Progetto con tutte le relazioni caricate</returns>
    Task<TherapyProjectModel?> GetByIdWithRelationsAsync(Guid id);

    /// <summary>
    /// Recupera tutti i progetti di un paziente specifico
    /// </summary>
    /// <param name="patientId">ID del paziente</param>
    /// <returns>Collezione dei progetti del paziente</returns>
    Task<IEnumerable<TherapyProjectModel>> GetByPatientIdAsync(Guid patientId);

    /// <summary>
    /// Recupera tutti i progetti assegnati a un educatore professionale
    /// </summary>
    /// <param name="educatorId">ID dell'educatore</param>
    /// <returns>Collezione dei progetti dell'educatore</returns>
    Task<IEnumerable<TherapyProjectModel>> GetByEducatorIdAsync(Guid educatorId);

    /// <summary>
    /// Recupera progetti per status
    /// </summary>
    /// <param name="status">Status del progetto (es. "In Progress", "Completed")</param>
    /// <returns>Collezione dei progetti con lo status specificato</returns>
    Task<IEnumerable<TherapyProjectModel>> GetByStatusAsync(string status);

    /// <summary>
    /// Cerca progetti per titolo o descrizione
    /// </summary>
    /// <param name="searchTerm">Termine di ricerca</param>
    /// <returns>Collezione di progetti che corrispondono alla ricerca</returns>
    Task<IEnumerable<TherapyProjectModel>> SearchAsync(string searchTerm);

    /// <summary>
    /// Aggiunge un nuovo progetto terapeutico (commit atomico)
    /// </summary>
    /// <param name="therapyProject">Progetto da aggiungere</param>
    /// <returns>Task completato</returns>
    Task AddAsync(TherapyProjectModel therapyProject);

    /// <summary>
    /// Aggiorna un progetto esistente (commit atomico)
    /// </summary>
    /// <param name="therapyProject">Progetto con dati aggiornati</param>
    /// <returns>Task completato</returns>
    Task UpdateAsync(TherapyProjectModel therapyProject);

    /// <summary>
    /// Elimina un progetto per ID (commit atomico)
    /// </summary>
    /// <param name="id">ID del progetto da eliminare</param>
    /// <returns>True se eliminato, False se non trovato</returns>
    Task<bool> DeleteAsync(Guid id);

    /// <summary>
    /// Verifica se esiste un progetto con l'ID specificato
    /// </summary>
    /// <param name="id">ID del progetto</param>
    /// <returns>True se esiste, False altrimenti</returns>
    Task<bool> ExistsAsync(Guid id);

    /// <summary>
    /// Assegna un educatore professionale a un progetto (relazione N-N)
    /// </summary>
    /// <param name="projectId">ID del progetto</param>
    /// <param name="educatorId">ID dell'educatore</param>
    /// <returns>Task completato</returns>
    Task AssignEducatorAsync(Guid projectId, Guid educatorId);

    /// <summary>
    /// Rimuove un educatore professionale da un progetto
    /// </summary>
    /// <param name="projectId">ID del progetto</param>
    /// <param name="educatorId">ID dell'educatore</param>
    /// <returns>Task completato</returns>
    Task RemoveEducatorAsync(Guid projectId, Guid educatorId);
}
