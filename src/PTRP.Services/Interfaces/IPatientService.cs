using PTRP.Models;
using PTRP.Services.Enums;

namespace PTRP.Services.Interfaces
{
    /// <summary>
    /// Interfaccia per il servizio di gestione dei Pazienti
    /// Definisce i contratti per le operazioni CRUD e logica di business
    /// </summary>
    public interface IPatientService
    {
        /// <summary>
        /// Recupera tutti i pazienti
        /// </summary>
        /// <param name="ct">CancellationToken</param>
        /// <returns>Collezione di PatientModel</returns>
        Task<IReadOnlyList<PatientModel>> GetAllAsync(CancellationToken ct = default);

        /// <summary>
        /// Recupera un paziente specifico per ID
        /// </summary>
        /// <param name="id">ID del paziente</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>PatientModel se trovato, altrimenti null</returns>
        Task<PatientModel?> GetByIdAsync(Guid id, CancellationToken ct = default);

        /// <summary>
        /// Recupera un paziente con i suoi progetti terapeutici caricati (eager loading).
        /// Utile per visualizzare lo storico progetti e il progetto attivo.
        /// </summary>
        /// <param name="id">ID del paziente</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>PatientModel con progetti caricati, o null se non trovato</returns>
        Task<PatientModel?> GetByIdWithProjectsAsync(Guid id, CancellationToken ct = default);

        /// <summary>
        /// Cerca pazienti per nome/cognome con filtro opzionale sullo stato del progetto.
        /// Se searchTerm è null o vuoto, restituisce tutti i pazienti applicando solo il filtro stato.
        /// </summary>
        /// <param name="searchTerm">Termine di ricerca (FirstName o LastName) - opzionale</param>
        /// <param name="stateFilter">Filtro per stato progetto (All, Active, Suspended, Completed, Deceased) - opzionale</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>Collezione di PatientModel che corrispondono ai criteri</returns>
        Task<IReadOnlyList<PatientModel>> SearchAsync(
            string? searchTerm = null, 
            ProjectStateFilter? stateFilter = null, 
            CancellationToken ct = default);

        /// <summary>
        /// Aggiunge un nuovo paziente
        /// </summary>
        /// <param name="patient">PatientModel da aggiungere</param>
        /// <param name="ct">CancellationToken</param>
        /// <exception cref="ArgumentException">Se i dati del paziente non sono validi</exception>
        Task CreateAsync(PatientModel patient, CancellationToken ct = default);

        /// <summary>
        /// Aggiorna un paziente esistente
        /// </summary>
        /// <param name="patient">PatientModel da aggiornare</param>
        /// <param name="ct">CancellationToken</param>
        Task UpdateAsync(PatientModel patient, CancellationToken ct = default);

        /// <summary>
        /// Elimina un paziente per ID
        /// </summary>
        /// <param name="id">ID del paziente da eliminare</param>
        /// <param name="ct">CancellationToken</param>
        Task DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
