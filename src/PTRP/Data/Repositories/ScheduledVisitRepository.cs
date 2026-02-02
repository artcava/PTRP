using Microsoft.EntityFrameworkCore;
using PTRP.Models;

namespace PTRP.Data.Repositories;

/// <summary>
/// Repository per la gestione delle visite programmate (ScheduledVisit).
/// </summary>
public class ScheduledVisitRepository
{
    private readonly PTRPDbContext _context;

    public ScheduledVisitRepository(PTRPDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Recupera una visita programmata per ID.
    /// </summary>
    public async Task<ScheduledVisitModel?> GetByIdAsync(Guid id)
    {
        return await _context.ScheduledVisits
            .Include(sv => sv.TherapyProject)
            .FirstOrDefaultAsync(sv => sv.Id == id);
    }

    /// <summary>
    /// Recupera tutte le visite programmate di un progetto terapeutico.
    /// </summary>
    public async Task<IEnumerable<ScheduledVisitModel>> GetByProjectIdAsync(Guid projectId)
    {
        return await _context.ScheduledVisits
            .Where(sv => sv.TherapyProjectId == projectId)
            .OrderBy(sv => sv.ScheduledDate)
            .ToListAsync();
    }

    /// <summary>
    /// Recupera le visite programmate di un educatore in un range di date.
    /// </summary>
    public async Task<IEnumerable<ScheduledVisitModel>> GetByEducatorIdInRangeAsync(
        Guid educatorId, DateTime fromDate, DateTime toDate)
    {
        return await _context.ScheduledVisits
            .Include(sv => sv.TherapyProject)
                .ThenInclude(tp => tp.ProfessionalEducators)
            .Where(sv => sv.TherapyProject.ProfessionalEducators
                .Any(pe => pe.Id == educatorId) &&
                sv.ScheduledDate.Date >= fromDate.Date &&
                sv.ScheduledDate.Date <= toDate.Date)
            .OrderBy(sv => sv.ScheduledDate)
            .ToListAsync();
    }

    /// <summary>
    /// Recupera le visite programmate filtrate per stato.
    /// </summary>
    public async Task<IEnumerable<ScheduledVisitModel>> GetByStatusAsync(string status)
    {
        return await _context.ScheduledVisits
            .Where(sv => sv.Status.ToString() == status)
            .OrderBy(sv => sv.ScheduledDate)
            .ToListAsync();
    }

    /// <summary>
    /// Recupera le visite programmate per una data specifica.
    /// </summary>
    public async Task<IEnumerable<ScheduledVisitModel>> GetByDateAsync(DateTime date)
    {
        return await _context.ScheduledVisits
            .Where(sv => sv.ScheduledDate.Date == date.Date)
            .OrderBy(sv => sv.ScheduledDate)
            .ToListAsync();
    }

    /// <summary>
    /// Aggiunge una nuova visita programmata.
    /// Verifica che il progetto terapeutico esista.
    /// </summary>
    public async Task AddAsync(ScheduledVisitModel scheduledVisit)
    {
        var projectExists = await _context.TherapyProjects
            .AnyAsync(tp => tp.Id == scheduledVisit.TherapyProjectId);

        if (!projectExists)
        {
            throw new InvalidOperationException(
                $"Il progetto terapeutico con ID {scheduledVisit.TherapyProjectId} non esiste.");
        }

        _context.ScheduledVisits.Add(scheduledVisit);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Aggiunge un range di visite programmate.
    /// </summary>
    public async Task AddRangeAsync(IEnumerable<ScheduledVisitModel> scheduledVisits)
    {
        _context.ScheduledVisits.AddRange(scheduledVisits);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Aggiorna una visita programmata esistente.
    /// </summary>
    public async Task UpdateAsync(ScheduledVisitModel scheduledVisit)
    {
        _context.ScheduledVisits.Update(scheduledVisit);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Elimina una visita programmata.
    /// </summary>
    public async Task<bool> DeleteAsync(Guid id)
    {
        var scheduledVisit = await _context.ScheduledVisits.FindAsync(id);

        if (scheduledVisit == null)
        {
            return false;
        }

        _context.ScheduledVisits.Remove(scheduledVisit);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Verifica se una visita programmata esiste.
    /// </summary>
    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.ScheduledVisits.AnyAsync(sv => sv.Id == id);
    }

    /// <summary>
    /// Conta le visite programmate di un progetto.
    /// </summary>
    public async Task<int> CountByProjectIdAsync(Guid projectId)
    {
        return await _context.ScheduledVisits
            .CountAsync(sv => sv.TherapyProjectId == projectId);
    }
}
