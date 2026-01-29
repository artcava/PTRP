using PTRP.Models;

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
        /// <returns>Collezione di PatientModel</returns>
        Task<IEnumerable<PatientModel>> GetAllAsync();

        /// <summary>
        /// Recupera un paziente specifico per ID
        /// </summary>
        /// <param name="id">ID del paziente</param>
        /// <returns>PatientModel se trovato, altrimenti null</returns>
        Task<PatientModel?> GetByIdAsync(Guid id);

        /// <summary>
        /// Aggiunge un nuovo paziente
        /// </summary>
        /// <param name="patient">PatientModel da aggiungere</param>
        /// <exception cref="ArgumentException">Se i dati del paziente non sono validi</exception>
        Task AddAsync(PatientModel patient);

        /// <summary>
        /// Aggiorna un paziente esistente
        /// </summary>
        /// <param name="patient">PatientModel da aggiornare</param>
        Task UpdateAsync(PatientModel patient);

        /// <summary>
        /// Elimina un paziente per ID
        /// </summary>
        /// <param name="id">ID del paziente da eliminare</param>
        Task DeleteAsync(Guid id);

        /// <summary>
        /// Cerca pazienti per nome
        /// </summary>
        /// <param name="searchTerm">Termine di ricerca (FirstName o LastName)</param>
        /// <returns>Collezione di PatientModel che corrispondono alla ricerca</returns>
        Task<IEnumerable<PatientModel>> SearchAsync(string searchTerm);
    }
}
