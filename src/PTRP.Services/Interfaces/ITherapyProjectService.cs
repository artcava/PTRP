using PTRP.Models;

namespace PTRP.Services.Interfaces;

/// <summary>
/// Interfaccia per il servizio di gestione dei Progetti Terapeutici
/// Definisce i contratti per le operazioni CRUD e logica di business
/// Gestisce validazioni e regole di business per TherapyProject
/// </summary>
public interface ITherapyProjectService
{
    /// <summary>
    /// Recupera tutti i progetti terapeutici
    /// </summary>
    /// <returns>Collezione di TherapyProjectModel</returns>
    Task<IEnumerable<TherapyProjectModel>> GetAllAsync();

    /// <summary>
    /// Recupera un progetto specifico per ID
    /// </summary>
    /// <param name="id">ID del progetto</param>
    /// <returns>TherapyProjectModel se trovato, altrimenti null</returns>
    Task<TherapyProjectModel?> GetByIdAsync(Guid id);

    /// <summary>
    /// Recupera un progetto con il paziente associato
    /// </summary>
    /// <param name="id">ID del progetto</param>
    /// <returns>TherapyProjectModel con paziente caricato</returns>
    Task<TherapyProjectModel?> GetByIdWithPatientAsync(Guid id);

    /// <summary>
    /// Recupera un progetto con tutte le relazioni (paziente ed educatori)
    /// </summary>
    /// <param name="id">ID del progetto</param>
    /// <returns>TherapyProjectModel con relazioni complete</returns>
    Task<TherapyProjectModel?> GetByIdWithRelationsAsync(Guid id);

    /// <summary>
    /// Recupera tutti i progetti di un paziente
    /// </summary>
    /// <param name="patientId">ID del paziente</param>
    /// <returns>Collezione di progetti del paziente</returns>
    Task<IEnumerable<TherapyProjectModel>> GetByPatientIdAsync(Guid patientId);

    /// <summary>
    /// Recupera tutti i progetti assegnati a un educatore
    /// </summary>
    /// <param name="educatorId">ID dell'educatore</param>
    /// <returns>Collezione di progetti dell'educatore</returns>
    Task<IEnumerable<TherapyProjectModel>> GetByEducatorIdAsync(Guid educatorId);

    /// <summary>
    /// Recupera progetti per status
    /// </summary>
    /// <param name="status">Status del progetto</param>
    /// <returns>Collezione di progetti con lo status specificato</returns>
    Task<IEnumerable<TherapyProjectModel>> GetByStatusAsync(string status);

    /// <summary>
    /// Cerca progetti per titolo o descrizione
    /// </summary>
    /// <param name="searchTerm">Termine di ricerca</param>
    /// <returns>Collezione di progetti che corrispondono alla ricerca</returns>
    Task<IEnumerable<TherapyProjectModel>> SearchAsync(string searchTerm);

    /// <summary>
    /// Aggiunge un nuovo progetto terapeutico con validazione business
    /// </summary>
    /// <param name="therapyProject">TherapyProjectModel da aggiungere</param>
    /// <exception cref="ArgumentException">Se i dati del progetto non sono validi</exception>
    /// <exception cref="InvalidOperationException">Se il paziente associato non esiste</exception>
    Task AddAsync(TherapyProjectModel therapyProject);

    /// <summary>
    /// Aggiorna un progetto esistente con validazione business
    /// </summary>
    /// <param name="therapyProject">TherapyProjectModel da aggiornare</param>
    /// <exception cref="ArgumentException">Se i dati del progetto non sono validi</exception>
    /// <exception cref="InvalidOperationException">Se il progetto non esiste</exception>
    Task UpdateAsync(TherapyProjectModel therapyProject);

    /// <summary>
    /// Elimina un progetto per ID
    /// Rimuove automaticamente le relazioni con gli educatori
    /// </summary>
    /// <param name="id">ID del progetto da eliminare</param>
    /// <exception cref="InvalidOperationException">Se il progetto non esiste</exception>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Assegna un educatore professionale a un progetto
    /// </summary>
    /// <param name="projectId">ID del progetto</param>
    /// <param name="educatorId">ID dell'educatore</param>
    /// <exception cref="InvalidOperationException">Se progetto o educatore non esistono</exception>
    Task AssignEducatorAsync(Guid projectId, Guid educatorId);

    /// <summary>
    /// Rimuove un educatore da un progetto
    /// </summary>
    /// <param name="projectId">ID del progetto</param>
    /// <param name="educatorId">ID dell'educatore</param>
    Task RemoveEducatorAsync(Guid projectId, Guid educatorId);

    /// <summary>
    /// Valida un progetto terapeutico secondo le regole di business
    /// </summary>
    /// <param name="therapyProject">Progetto da validare</param>
    /// <returns>True se valido, False altrimenti</returns>
    /// <exception cref="ArgumentException">Se la validazione fallisce, con dettagli dell'errore</exception>
    Task<bool> ValidateAsync(TherapyProjectModel therapyProject);

    /// <summary>
    /// Completa un progetto cambiando lo status a "Completed"
    /// Valida che la EndDate sia impostata
    /// </summary>
    /// <param name="projectId">ID del progetto</param>
    /// <exception cref="InvalidOperationException">Se il progetto non può essere completato</exception>
    Task CompleteProjectAsync(Guid projectId);

    /// <summary>
    /// Mette in pausa un progetto cambiando lo status a "On Hold"
    /// </summary>
    /// <param name="projectId">ID del progetto</param>
    Task PutOnHoldAsync(Guid projectId);

    /// <summary>
    /// Riattiva un progetto in pausa riportandolo a "In Progress"
    /// </summary>
    /// <param name="projectId">ID del progetto</param>
    Task ResumeProjectAsync(Guid projectId);
}
