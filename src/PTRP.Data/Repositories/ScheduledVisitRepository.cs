using Microsoft.EntityFrameworkCore;
using PTRP.Models;
using PTRP.Models.Enums;
using PTRP.Data.Repositories.Interfaces;

namespace PTRP.Data.Repositories;

/// <summary>
/// Implementazione del repository per l'entità ScheduledVisitModel (Appuntamenti Programmati)
/// Usa Entity Framework Core per le operazioni database
/// Gestisce relazioni 1-N con TherapyProject e 1-1 con ActualVisit
/// </summary>
public class ScheduledVisitRepository : IScheduledVisitRepository
{
    private readonly PTRPDbContext _context;

    public ScheduledVisitRepository(PTRPDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ScheduledVisitModel>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.ScheduledVisits
            .AsNoTracking()
            .OrderBy(sv => sv.ScheduledDate)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<ScheduledVisitModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.ScheduledVisits
            .AsNoTracking()
            .FirstOrDefaultAsync(sv => sv.Id == id, ct);
    }

    /// <inheritdoc />
    public async Task<ScheduledVisitModel?> GetByIdWithRelationsAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.ScheduledVisits
            .Include(sv => sv.TherapyProject)
                .ThenInclude(tp => tp.Patient)
            .AsNoTracking()
            .FirstOrDefaultAsync(sv => sv.Id == id, ct);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ScheduledVisitModel>> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default)
    {
        return await _context.ScheduledVisits
            .AsNoTracking()
            .Where(sv => sv.TherapyProjectId == projectId)
            .OrderBy(sv => sv.ScheduledDate)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ScheduledVisitModel>> GetByEducatorIdInRangeAsync(
        Guid educatorId,
        DateTime fromDate,
        DateTime toDate,
        CancellationToken ct = default)
    {
        return await _context.ScheduledVisits
            .Include(sv => sv.TherapyProject)
                .ThenInclude(tp => tp.Patient)
            .AsNoTracking()
            .Where(sv => sv.ScheduledDate >= fromDate &&
                         sv.ScheduledDate <= toDate &&
                         sv.TherapyProject.ProfessionalEducators.Any(pe => pe.Id == educatorId))
            .OrderBy(sv => sv.ScheduledDate)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ScheduledVisitModel>> GetByVisitTypeIdAsync(Guid visitTypeId, CancellationToken ct = default)
    {
        // Nota: Type è un enum VisitType, non una FK verso una tabella VisitTypes
        // Questo metodo assume che visitTypeId rappresenti il valore numerico dell'enum
        // In un contesto reale, potresti voler convertire o rimuovere questo metodo
        return await _context.ScheduledVisits
            .AsNoTracking()
            .OrderBy(sv => sv.ScheduledDate)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ScheduledVisitModel>> GetByStatusAsync(string status, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return await GetAllAsync(ct);
        }

        // Tenta di convertire la stringa in enum AppointmentStatus
        if (Enum.TryParse<AppointmentStatus>(status, ignoreCase: true, out var statusEnum))
        {
            return await _context.ScheduledVisits
                .AsNoTracking()
                .Where(sv => sv.Status == statusEnum)
                .OrderBy(sv => sv.ScheduledDate)
                .ToListAsync(ct);
        }

        // Se la conversione fallisce, restituisci lista vuota
        return new List<ScheduledVisitModel>();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ScheduledVisitModel>> GetByDateAsync(DateTime date, CancellationToken ct = default)
    {
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1).AddTicks(-1);

        return await _context.ScheduledVisits
            .AsNoTracking()
            .Where(sv => sv.ScheduledDate >= startOfDay && sv.ScheduledDate <= endOfDay)
            .OrderBy(sv => sv.ScheduledDate)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task AddAsync(ScheduledVisitModel scheduledVisit, CancellationToken ct = default)
    {
        if (scheduledVisit == null)
            throw new ArgumentNullException(nameof(scheduledVisit));

        // Assicurati che abbia un ID
        if (scheduledVisit.Id == Guid.Empty)
        {
            scheduledVisit.Id = Guid.NewGuid();
        }

        // Imposta CreatedAt se non già impostato
        if (scheduledVisit.CreatedAt == default)
        {
            scheduledVisit.CreatedAt = DateTime.Now;
        }

        // Verifica che il progetto esista
        var projectExists = await _context.TherapyProjects
            .AnyAsync(tp => tp.Id == scheduledVisit.TherapyProjectId, ct);
        
        if (!projectExists)
        {
            throw new InvalidOperationException(
                $"Cannot create ScheduledVisit: TherapyProject with ID {scheduledVisit.TherapyProjectId} does not exist");
        }

        await _context.ScheduledVisits.AddAsync(scheduledVisit, ct);
        await _context.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public async Task AddRangeAsync(IEnumerable<ScheduledVisitModel> scheduledVisits, CancellationToken ct = default)
    {
        if (scheduledVisits == null)
            throw new ArgumentNullException(nameof(scheduledVisits));

        var visitsList = scheduledVisits.ToList();

        if (!visitsList.Any())
            return;

        // Prepara e valida tutti gli appuntamenti
        foreach (var visit in visitsList)
        {
            if (visit.Id == Guid.Empty)
            {
                visit.Id = Guid.NewGuid();
            }

            if (visit.CreatedAt == default)
            {
                visit.CreatedAt = DateTime.Now;
            }

            // Verifica che il progetto esista
            var projectExists = await _context.TherapyProjects
                .AnyAsync(tp => tp.Id == visit.TherapyProjectId, ct);
            
            if (!projectExists)
            {
                throw new InvalidOperationException(
                    $"Cannot create ScheduledVisit: TherapyProject with ID {visit.TherapyProjectId} does not exist");
            }
        }

        // Aggiungi tutti gli appuntamenti in una singola operazione
        await _context.ScheduledVisits.AddRangeAsync(visitsList, ct);
        await _context.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(ScheduledVisitModel scheduledVisit, CancellationToken ct = default)
    {
        if (scheduledVisit == null)
            throw new ArgumentNullException(nameof(scheduledVisit));

        var existingVisit = await _context.ScheduledVisits.FindAsync(
            new object[] { scheduledVisit.Id }, 
            cancellationToken: ct);
            
        if (existingVisit == null)
        {
            throw new InvalidOperationException(
                $"ScheduledVisit with ID {scheduledVisit.Id} not found");
        }

        // Aggiorna i campi modificabili
        existingVisit.ScheduledDate = scheduledVisit.ScheduledDate;
        existingVisit.RescheduledDate = scheduledVisit.RescheduledDate;
        existingVisit.Status = scheduledVisit.Status;
        existingVisit.Notes = scheduledVisit.Notes;
        existingVisit.UpdatedAt = DateTime.Now;
        existingVisit.UpdatedBy = scheduledVisit.UpdatedBy;
        existingVisit.Version++;

        _context.ScheduledVisits.Update(existingVisit);
        await _context.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var scheduledVisit = await _context.ScheduledVisits.FindAsync(
            new object[] { id }, 
            cancellationToken: ct);
            
        if (scheduledVisit == null)
        {
            return false;
        }

        _context.ScheduledVisits.Remove(scheduledVisit);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.ScheduledVisits
            .AsNoTracking()
            .AnyAsync(sv => sv.Id == id, ct);
    }

    /// <inheritdoc />
    public async Task<int> CountByProjectIdAsync(Guid projectId, CancellationToken ct = default)
    {
        return await _context.ScheduledVisits
            .AsNoTracking()
            .CountAsync(sv => sv.TherapyProjectId == projectId, ct);
    }
}
