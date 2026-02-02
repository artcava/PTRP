using PTRP.Models;

namespace PTRP.Data.Repositories.Interfaces;

/// <summary>
/// Interfaccia per il repository delle Visite Effettive (Actual Visits)
/// Definisce le operazioni CRUD e di ricerca per l'entità ActualVisitModel
/// 
/// NOTA: Ogni operazione CRUD (Add, Update, Delete) effettua il commit atomico.
/// Non è necessario chiamare un metodo SaveChangesAsync() separato.
/// 
/// VINCOLO CRITICO: Relazione 1:1 con ScheduledVisitModel tramite ScheduledVisitId (UNIQUE constraint)
/// Una visita effettiva può essere associata SOLO a un appuntamento programmato specifico.
/// </summary>
public interface IActualVisitRepository
{
    /// <summary>
    /// Recupera tutte le visite effettive dal database
    /// </summary>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Collezione di tutte le visite effettive</returns>
    Task<IEnumerable<ActualVisitModel>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Recupera una visita effettiva per ID
    /// </summary>
    /// <param name="id">ID univoco della visita effettiva</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Visita trovata o null</returns>
    Task<ActualVisitModel?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Recupera una visita effettiva con tutte le relazioni caricate (appuntamento, operatori, paziente, progetto)
    /// </summary>
    /// <param name="id">ID univoco della visita effettiva</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Visita con tutte le relazioni caricate</returns>
    Task<ActualVisitModel?> GetByIdWithRelationsAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Recupera la visita effettiva associata a un appuntamento programmato (relazione 1:1)
    /// VINCOLO: Restituisce al massimo una visita per appuntamento
    /// </summary>
    /// <param name="scheduledVisitId">ID dell'appuntamento programmato</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Visita effettiva se esiste, null altrimenti</returns>
    Task<ActualVisitModel?> GetByScheduledVisitIdAsync(Guid scheduledVisitId, CancellationToken ct = default);

    /// <summary>
    /// Recupera tutte le visite effettive registrate da un educatore in un intervallo temporale
    /// Utilizzato per l'export di visite verso il Coordinatore (sincronizzazione)
    /// </summary>
    /// <param name="educatorId">ID dell'educatore professionale</param>
    /// <param name="fromDate">Data inizio intervallo</param>
    /// <param name="toDate">Data fine intervallo</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Collezione delle visite registrate dall'educatore nel periodo</returns>
    Task<IEnumerable<ActualVisitModel>> GetByEducatorIdInRangeAsync(
        Guid educatorId, 
        DateTime fromDate, 
        DateTime toDate, 
        CancellationToken ct = default);

    /// <summary>
    /// Recupera tutte le visite effettive registrate per un progetto terapeutico
    /// </summary>
    /// <param name="projectId">ID del progetto terapeutico</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Collezione delle visite effettive del progetto</returns>
    Task<IEnumerable<ActualVisitModel>> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default);

    /// <summary>
    /// Recupera tutte le visite effettive registrate per un paziente
    /// </summary>
    /// <param name="patientId">ID del paziente</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Collezione delle visite effettive del paziente</returns>
    Task<IEnumerable<ActualVisitModel>> GetByPatientIdAsync(Guid patientId, CancellationToken ct = default);

    /// <summary>
    /// Recupera visite effettive per stato di presenza paziente (Attended, Absent, PartiallyAttended)
    /// </summary>
    /// <param name="attendanceStatus">Stato di presenza del paziente (PatientAttendance enum)</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Collezione delle visite con lo stato di presenza specificato</returns>
    Task<IEnumerable<ActualVisitModel>> GetByPatientAttendanceAsync(string attendanceStatus, CancellationToken ct = default);

    /// <summary>
    /// Recupera visite effettive in una data specifica
    /// </summary>
    /// <param name="date">Data delle visite</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Collezione delle visite della data</returns>
    Task<IEnumerable<ActualVisitModel>> GetByDateAsync(DateTime date, CancellationToken ct = default);

    /// <summary>
    /// Recupera visite effettive in un intervallo temporale
    /// </summary>
    /// <param name="fromDate">Data inizio intervallo</param>
    /// <param name="toDate">Data fine intervallo</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Collezione delle visite nel periodo</returns>
    Task<IEnumerable<ActualVisitModel>> GetInRangeAsync(
        DateTime fromDate, 
        DateTime toDate, 
        CancellationToken ct = default);

    /// <summary>
    /// Aggiunge una nuova visita effettiva (commit atomico)
    /// Precondizione: L'appuntamento programmato associato deve esistere
    /// </summary>
    /// <param name="actualVisit">Visita effettiva da aggiungere</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Task completato</returns>
    /// <exception cref="InvalidOperationException">Se l'appuntamento non esiste o già ha una visita associata</exception>
    Task AddAsync(ActualVisitModel actualVisit, CancellationToken ct = default);

    /// <summary>
    /// Aggiorna una visita effettiva esistente (commit atomico)
    /// </summary>
    /// <param name="actualVisit">Visita con dati aggiornati</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Task completato</returns>
    Task UpdateAsync(ActualVisitModel actualVisit, CancellationToken ct = default);

    /// <summary>
    /// Elimina una visita effettiva per ID (commit atomico)
    /// </summary>
    /// <param name="id">ID della visita da eliminare</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>True se eliminata, False se non trovata</returns>
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Verifica se esiste una visita effettiva con l'ID specificato
    /// </summary>
    /// <param name="id">ID della visita</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>True se esiste, False altrimenti</returns>
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Verifica se esiste una visita effettiva per un appuntamento programmato specifico
    /// Utilizzato per enforcer il vincolo 1:1 ScheduledVisit-ActualVisit
    /// </summary>
    /// <param name="scheduledVisitId">ID dell'appuntamento programmato</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>True se esiste una visita effettiva, False altrimenti</returns>
    Task<bool> ExistsByScheduledVisitIdAsync(Guid scheduledVisitId, CancellationToken ct = default);

    /// <summary>
    /// Conta il numero di visite effettive registrate per un progetto
    /// </summary>
    /// <param name="projectId">ID del progetto</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Numero di visite registrate</returns>
    Task<int> CountByProjectIdAsync(Guid projectId, CancellationToken ct = default);

    /// <summary>
    /// Conta il numero di visite effettive registrate da un educatore in un periodo
    /// Utilizzato per statistiche e reportistica
    /// </summary>
    /// <param name="educatorId">ID dell'educatore</param>
    /// <param name="fromDate">Data inizio periodo</param>
    /// <param name="toDate">Data fine periodo</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Numero di visite registrate</returns>
    Task<int> CountByEducatorInRangeAsync(
        Guid educatorId, 
        DateTime fromDate, 
        DateTime toDate, 
        CancellationToken ct = default);
}
