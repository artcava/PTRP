using Microsoft.EntityFrameworkCore;
using PTRP.Models;

namespace PTRP.Data.Repositories;

/// <summary>
/// Repository per la gestione delle visite effettive (ActualVisit).
/// Implementa il vincolo 1:1 con ScheduledVisit.
/// L'immutabilità dello ScheduledVisitId è garantita dal modello (private setter).
/// </summary>
public class ActualVisitRepository
{
    private readonly PTRPDbContext _context;

    public ActualVisitRepository(PTRPDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Recupera una visita effettiva per ID.
    /// </summary>
    public async Task<ActualVisitModel?> GetByIdAsync(Guid id)
    {
        return await _context.ActualVisits
            .Include(av => av.OperatorsPresent)
            .FirstOrDefaultAsync(av => av.Id == id);
    }

    /// <summary>
    /// Recupera la visita effettiva associata ad una visita programmata.
    /// Restituisce null se la visita programmata non ha ancora una visita effettiva.
    /// </summary>
    public async Task<ActualVisitModel?> GetByScheduledVisitIdAsync(Guid scheduledVisitId)
    {
        return await _context.ActualVisits
            .Include(av => av.OperatorsPresent)
            .FirstOrDefaultAsync(av => av.ScheduledVisitId == scheduledVisitId);
    }

    /// <summary>
    /// Recupera le visite effettive di un educatore in un range di date.
    /// </summary>
    public async Task<IEnumerable<ActualVisitModel>> GetByEducatorIdInRangeAsync(
        Guid educatorId, DateTime fromDate, DateTime toDate)
    {
        return await _context.ActualVisits
            .Include(av => av.ScheduledVisit)
                .ThenInclude(sv => sv.TherapyProject)
                    .ThenInclude(tp => tp.ProfessionalEducators)
            .Include(av => av.OperatorsPresent)
            .Where(av => av.ScheduledVisit.TherapyProject.ProfessionalEducators
                .Any(pe => pe.Id == educatorId) &&
                av.ActualDate >= fromDate.Date &&
                av.ActualDate <= toDate.Date)
            .OrderBy(av => av.ActualDate)
            .ToListAsync();
    }

    /// <summary>
    /// Recupera tutte le visite effettive di un progetto terapeutico.
    /// </summary>
    public async Task<IEnumerable<ActualVisitModel>> GetByProjectIdAsync(Guid projectId)
    {
        return await _context.ActualVisits
            .Include(av => av.ScheduledVisit)
            .Include(av => av.OperatorsPresent)
            .Where(av => av.ScheduledVisit.TherapyProjectId == projectId)
            .OrderBy(av => av.ActualDate)
            .ToListAsync();
    }

    /// <summary>
    /// Recupera tutte le visite effettive di un paziente.
    /// </summary>
    public async Task<IEnumerable<ActualVisitModel>> GetByPatientIdAsync(Guid patientId)
    {
        return await _context.ActualVisits
            .Include(av => av.ScheduledVisit)
                .ThenInclude(sv => sv.TherapyProject)
            .Include(av => av.OperatorsPresent)
            .Where(av => av.ScheduledVisit.TherapyProject.PatientId == patientId)
            .OrderBy(av => av.ActualDate)
            .ToListAsync();
    }

    /// <summary>
    /// Recupera le visite effettive filtrate per stato di presenza del paziente.
    /// </summary>
    public async Task<IEnumerable<ActualVisitModel>> GetByPatientAttendanceAsync(string attendanceStatus)
    {
        return await _context.ActualVisits
            .Include(av => av.OperatorsPresent)
            .Where(av => av.PatientPresence.ToString() == attendanceStatus)
            .OrderByDescending(av => av.ActualDate)
            .ToListAsync();
    }

    /// <summary>
    /// Aggiunge una nuova visita effettiva.
    /// Verifica i vincoli:
    /// - La visita programmata deve esistere
    /// - La visita programmata non deve avere già una visita effettiva (relazione 1:1)
    /// </summary>
    public async Task AddAsync(ActualVisitModel actualVisit)
    {
        // Verifica che la ScheduledVisit esista
        var scheduledVisitExists = await _context.ScheduledVisits
            .AnyAsync(sv => sv.Id == actualVisit.ScheduledVisitId);

        if (!scheduledVisitExists)
        {
            throw new InvalidOperationException(
                $"La visita programmata con ID {actualVisit.ScheduledVisitId} non esiste.");
        }

        // Verifica vincolo 1:1 - la ScheduledVisit non deve avere già una ActualVisit
        var alreadyHasActualVisit = await ExistsByScheduledVisitIdAsync(actualVisit.ScheduledVisitId);

        if (alreadyHasActualVisit)
        {
            throw new InvalidOperationException(
                $"La visita programmata con ID {actualVisit.ScheduledVisitId} ha già una visita effettiva associata. " +
                "Vincolo 1:1 violato.");
        }

        _context.ActualVisits.Add(actualVisit);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Aggiorna una visita effettiva esistente.
    /// NOTA: Lo ScheduledVisitId è immutabile per design (private setter nel modello),
    /// quindi non è necessario verificarlo qui.
    /// </summary>
    public async Task UpdateAsync(ActualVisitModel actualVisit)
    {
        var exists = await ExistsAsync(actualVisit.Id);
        if (!exists)
        {
            throw new InvalidOperationException(
                $"La visita effettiva con ID {actualVisit.Id} non esiste.");
        }

        _context.ActualVisits.Update(actualVisit);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Elimina una visita effettiva.
    /// </summary>
    public async Task<bool> DeleteAsync(Guid id)
    {
        var actualVisit = await _context.ActualVisits.FindAsync(id);

        if (actualVisit == null)
        {
            return false;
        }

        _context.ActualVisits.Remove(actualVisit);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Verifica se una visita effettiva esiste.
    /// </summary>
    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.ActualVisits.AnyAsync(av => av.Id == id);
    }

    /// <summary>
    /// Verifica se una visita programmata ha già una visita effettiva associata.
    /// Utilizzato per garantire il vincolo 1:1.
    /// </summary>
    public async Task<bool> ExistsByScheduledVisitIdAsync(Guid scheduledVisitId)
    {
        return await _context.ActualVisits
            .AnyAsync(av => av.ScheduledVisitId == scheduledVisitId);
    }

    /// <summary>
    /// Conta le visite effettive di un progetto.
    /// </summary>
    public async Task<int> CountByProjectIdAsync(Guid projectId)
    {
        return await _context.ActualVisits
            .Include(av => av.ScheduledVisit)
            .CountAsync(av => av.ScheduledVisit.TherapyProjectId == projectId);
    }
}
