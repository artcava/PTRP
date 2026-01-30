using PTRP.Models;

namespace PTRP.Services.Interfaces;

/// <summary>
/// Interfaccia per il servizio di gestione degli Educatori Professionali.
/// Contiene la business logic per le operazioni sugli educatori.
/// </summary>
public interface IEducatorService
{
    /// <summary>
    /// Recupera tutti gli educatori.
    /// </summary>
    /// <returns>Collezione di tutti gli educatori</returns>
    Task<IEnumerable<ProfessionalEducatorModel>> GetAllAsync();

    /// <summary>
    /// Recupera un educatore per ID.
    /// </summary>
    /// <param name="id">ID dell'educatore</param>
    /// <returns>Educatore se trovato, altrimenti null</returns>
    Task<ProfessionalEducatorModel?> GetByIdAsync(Guid id);

    /// <summary>
    /// Recupera un educatore con i progetti assegnati.
    /// </summary>
    /// <param name="id">ID dell'educatore</param>
    /// <returns>Educatore con progetti, altrimenti null</returns>
    Task<ProfessionalEducatorModel?> GetByIdWithProjectsAsync(Guid id);

    /// <summary>
    /// Recupera educatori per status.
    /// </summary>
    /// <param name="status">Status da filtrare</param>
    /// <returns>Collezione di educatori con lo status specificato</returns>
    Task<IEnumerable<ProfessionalEducatorModel>> GetByStatusAsync(string status);

    /// <summary>
    /// Recupera educatori attivi.
    /// Shortcut per GetByStatusAsync("Active").
    /// </summary>
    /// <returns>Collezione di educatori attivi</returns>
    Task<IEnumerable<ProfessionalEducatorModel>> GetActiveEducatorsAsync();

    /// <summary>
    /// Recupera educatori per specializzazione.
    /// </summary>
    /// <param name="specialization">Specializzazione da cercare</param>
    /// <returns>Collezione di educatori con la specializzazione specificata</returns>
    Task<IEnumerable<ProfessionalEducatorModel>> GetBySpecializationAsync(string specialization);

    /// <summary>
    /// Recupera educatori assegnati a un progetto.
    /// </summary>
    /// <param name="projectId">ID del progetto</param>
    /// <returns>Collezione di educatori assegnati</returns>
    Task<IEnumerable<ProfessionalEducatorModel>> GetByProjectIdAsync(Guid projectId);

    /// <summary>
    /// Cerca educatori per termine di ricerca.
    /// </summary>
    /// <param name="searchTerm">Termine di ricerca</param>
    /// <returns>Collezione di educatori che corrispondono</returns>
    Task<IEnumerable<ProfessionalEducatorModel>> SearchAsync(string searchTerm);

    /// <summary>
    /// Aggiunge un nuovo educatore con validazione.
    /// </summary>
    /// <param name="educator">Educatore da aggiungere</param>
    /// <exception cref="ArgumentException">Se la validazione fallisce</exception>
    /// <exception cref="InvalidOperationException">Se l'email è già in uso</exception>
    Task AddAsync(ProfessionalEducatorModel educator);

    /// <summary>
    /// Aggiorna un educatore esistente con validazione.
    /// </summary>
    /// <param name="educator">Educatore da aggiornare</param>
    /// <exception cref="ArgumentException">Se la validazione fallisce</exception>
    /// <exception cref="InvalidOperationException">Se l'educatore non esiste o l'email è duplicata</exception>
    Task UpdateAsync(ProfessionalEducatorModel educator);

    /// <summary>
    /// Elimina un educatore.
    /// </summary>
    /// <param name="id">ID dell'educatore da eliminare</param>
    /// <exception cref="InvalidOperationException">Se l'educatore non esiste</exception>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Verifica se un educatore esiste.
    /// </summary>
    /// <param name="id">ID dell'educatore</param>
    /// <returns>True se esiste, altrimenti false</returns>
    Task<bool> ExistsAsync(Guid id);

    /// <summary>
    /// Valida un educatore secondo le regole di business.
    /// </summary>
    /// <param name="educator">Educatore da validare</param>
    /// <returns>True se valido</returns>
    /// <exception cref="ArgumentException">Se la validazione fallisce con dettagli dell'errore</exception>
    Task<bool> ValidateAsync(ProfessionalEducatorModel educator);

    /// <summary>
    /// Cambia lo status di un educatore a "Inactive".
    /// </summary>
    /// <param name="id">ID dell'educatore</param>
    /// <exception cref="InvalidOperationException">Se l'educatore non esiste o è già inattivo</exception>
    Task DeactivateAsync(Guid id);

    /// <summary>
    /// Cambia lo status di un educatore a "Active".
    /// </summary>
    /// <param name="id">ID dell'educatore</param>
    /// <exception cref="InvalidOperationException">Se l'educatore non esiste o è già attivo</exception>
    Task ActivateAsync(Guid id);

    /// <summary>
    /// Cambia lo status di un educatore a "OnLeave".
    /// </summary>
    /// <param name="id">ID dell'educatore</param>
    /// <exception cref="InvalidOperationException">Se l'educatore non esiste o è già in permesso</exception>
    Task SetOnLeaveAsync(Guid id);

    /// <summary>
    /// Recupera tutte le specializzazioni uniche.
    /// </summary>
    /// <returns>Lista di specializzazioni</returns>
    Task<IEnumerable<string>> GetAvailableSpecializationsAsync();
}
