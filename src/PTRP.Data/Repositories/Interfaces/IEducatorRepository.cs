using PTRP.Models;

namespace PTRP.Data.Repositories.Interfaces;

/// <summary>
/// Interfaccia per il repository degli Educatori Professionali.
/// Gestisce la persistenza e le query per ProfessionalEducatorModel.
/// </summary>
public interface IEducatorRepository
{
    /// <summary>
    /// Recupera tutti gli educatori, ordinati per cognome e nome.
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
    /// Recupera un educatore per ID con i progetti terapeutici assegnati.
    /// </summary>
    /// <param name="id">ID dell'educatore</param>
    /// <returns>Educatore con progetti caricati, altrimenti null</returns>
    Task<ProfessionalEducatorModel?> GetByIdWithProjectsAsync(Guid id);

    /// <summary>
    /// Recupera gli educatori per status.
    /// </summary>
    /// <param name="status">Status da filtrare (Active, Inactive, OnLeave)</param>
    /// <returns>Collezione di educatori con lo status specificato</returns>
    Task<IEnumerable<ProfessionalEducatorModel>> GetByStatusAsync(string status);

    /// <summary>
    /// Recupera gli educatori per specializzazione.
    /// </summary>
    /// <param name="specialization">Specializzazione da cercare</param>
    /// <returns>Collezione di educatori con la specializzazione specificata</returns>
    Task<IEnumerable<ProfessionalEducatorModel>> GetBySpecializationAsync(string specialization);

    /// <summary>
    /// Recupera gli educatori assegnati a un progetto terapeutico specifico.
    /// </summary>
    /// <param name="projectId">ID del progetto terapeutico</param>
    /// <returns>Collezione di educatori assegnati al progetto</returns>
    Task<IEnumerable<ProfessionalEducatorModel>> GetByProjectIdAsync(Guid projectId);

    /// <summary>
    /// Cerca educatori per nome, cognome o email.
    /// La ricerca è case-insensitive.
    /// </summary>
    /// <param name="searchTerm">Termine di ricerca</param>
    /// <returns>Collezione di educatori che corrispondono al criterio</returns>
    Task<IEnumerable<ProfessionalEducatorModel>> SearchAsync(string searchTerm);

    /// <summary>
    /// Aggiunge un nuovo educatore.
    /// </summary>
    /// <param name="educator">Educatore da aggiungere</param>
    Task AddAsync(ProfessionalEducatorModel educator);

    /// <summary>
    /// Aggiorna un educatore esistente.
    /// Imposta automaticamente UpdatedAt.
    /// </summary>
    /// <param name="educator">Educatore da aggiornare</param>
    Task UpdateAsync(ProfessionalEducatorModel educator);

    /// <summary>
    /// Elimina un educatore per ID.
    /// Rimuove anche le assegnazioni ai progetti terapeutici.
    /// </summary>
    /// <param name="id">ID dell'educatore da eliminare</param>
    /// <returns>True se eliminato con successo, false se non trovato</returns>
    Task<bool> DeleteAsync(Guid id);

    /// <summary>
    /// Verifica se un educatore esiste.
    /// </summary>
    /// <param name="id">ID dell'educatore</param>
    /// <returns>True se esiste, altrimenti false</returns>
    Task<bool> ExistsAsync(Guid id);

    /// <summary>
    /// Verifica se esiste un educatore con la stessa email.
    /// Utile per validare unicità email.
    /// </summary>
    /// <param name="email">Email da verificare</param>
    /// <param name="excludeId">ID da escludere (per update)</param>
    /// <returns>True se l'email è già in uso, altrimenti false</returns>
    Task<bool> EmailExistsAsync(string email, Guid? excludeId = null);

    /// <summary>
    /// Recupera le specializzazioni uniche presenti nel database.
    /// Utile per dropdown/filtri.
    /// </summary>
    /// <returns>Lista di specializzazioni uniche</returns>
    Task<IEnumerable<string>> GetUniqueSpecializationsAsync();
}
