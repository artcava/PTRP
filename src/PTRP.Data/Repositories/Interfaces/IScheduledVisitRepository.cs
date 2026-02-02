using PTRP.Models;

namespace PTRP.Data.Repositories.Interfaces;

/// <summary>
/// Interfaccia per il repository degli Appuntamenti Programmati (Scheduled Visits)
/// Definisce le operazioni CRUD e di ricerca per l'entità ScheduledVisitModel
/// 
/// NOTA: Ogni operazione CRUD (Add, Update, Delete) effettua il commit atomico.
/// Non è necessario chiamare un metodo SaveChangesAsync() separato.
/// </summary>
public interface IScheduledVisitRepository
{
    /// <summary>
    /// Recupera tutti gli appuntamenti programmati dal database
    /// </summary>
    /// <returns>Collezione di tutti gli appuntamenti</returns>
    Task<IEnumerable<ScheduledVisitModel>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Recupera un appuntamento programmato per ID
    /// </summary>
    /// <param name="id">ID univoco dell'appuntamento</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Appuntamento trovato o null</returns>
    Task<ScheduledVisitModel?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Recupera un appuntamento con tutte le relazioni caricate (progetto, tipo visita)
    /// </summary>
    /// <param name="id">ID univoco dell'appuntamento</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Appuntamento con relazioni</returns>
    Task<ScheduledVisitModel?> GetByIdWithRelationsAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Recupera tutti gli appuntamenti programmati per un progetto terapeutico
    /// </summary>
    /// <param name="projectId">ID del progetto terapeutico</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Collezione degli appuntamenti del progetto</returns>
    Task<IEnumerable<ScheduledVisitModel>> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default);

    /// <summary>
    /// Recupera appuntamenti programmati per un educatore in un intervallo temporale
    /// Utilizzato per popolare il calendario dell'educatore
    /// </summary>
    /// <param name="educatorId">ID dell'educatore professionale</param>
    /// <param name="fromDate">Data inizio intervallo</param>
    /// <param name="toDate">Data fine intervallo</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Collezione degli appuntamenti dell'educatore nel periodo</returns>
    Task<IEnumerable<ScheduledVisitModel>> GetByEducatorIdInRangeAsync(
        Guid educatorId, 
        DateTime fromDate, 
        DateTime toDate, 
        CancellationToken ct = default);

    /// <summary>
    /// Recupera appuntamenti per tipo visita (es. INTAKE, INTERMEDIATE, FINAL, DISCHARGE)
    /// </summary>
    /// <param name="visitTypeId">ID del tipo visita</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Collezione degli appuntamenti del tipo specificato</returns>
    Task<IEnumerable<ScheduledVisitModel>> GetByVisitTypeIdAsync(Guid visitTypeId, CancellationToken ct = default);

    /// <summary>
    /// Recupera appuntamenti per stato (es. Scheduled, Completed, Missed, Rescheduled)
    /// </summary>
    /// <param name="status">Status dell'appuntamento (AppointmentStatus)</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Collezione degli appuntamenti con lo status specificato</returns>
    Task<IEnumerable<ScheduledVisitModel>> GetByStatusAsync(string status, CancellationToken ct = default);

    /// <summary>
    /// Recupera appuntamenti programmati in una data specifica
    /// </summary>
    /// <param name="date">Data degli appuntamenti</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Collezione degli appuntamenti della data</returns>
    Task<IEnumerable<ScheduledVisitModel>> GetByDateAsync(DateTime date, CancellationToken ct = default);

    /// <summary>
    /// Aggiunge un nuovo appuntamento programmato (commit atomico)
    /// </summary>
    /// <param name="scheduledVisit">Appuntamento da aggiungere</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Task completato</returns>
    Task AddAsync(ScheduledVisitModel scheduledVisit, CancellationToken ct = default);

    /// <summary>
    /// Aggiunge molteplici appuntamenti programmati in una singola operazione (commit atomico)
    /// Utilizzato per la creazione automatica dei 4 appuntamenti canonici
    /// </summary>
    /// <param name="scheduledVisits">Collezione di appuntamenti da aggiungere</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Task completato</returns>
    Task AddRangeAsync(IEnumerable<ScheduledVisitModel> scheduledVisits, CancellationToken ct = default);

    /// <summary>
    /// Aggiorna un appuntamento esistente (commit atomico)
    /// </summary>
    /// <param name="scheduledVisit">Appuntamento con dati aggiornati</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Task completato</returns>
    Task UpdateAsync(ScheduledVisitModel scheduledVisit, CancellationToken ct = default);

    /// <summary>
    /// Elimina un appuntamento per ID (commit atomico)
    /// </summary>
    /// <param name="id">ID dell'appuntamento da eliminare</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>True se eliminato, False se non trovato</returns>
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Verifica se esiste un appuntamento con l'ID specificato
    /// </summary>
    /// <param name="id">ID dell'appuntamento</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>True se esiste, False altrimenti</returns>
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Conta gli appuntamenti programmati per un progetto
    /// </summary>
    /// <param name="projectId">ID del progetto</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Numero di appuntamenti</returns>
    Task<int> CountByProjectIdAsync(Guid projectId, CancellationToken ct = default);
}
