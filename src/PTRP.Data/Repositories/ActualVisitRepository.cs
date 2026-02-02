using Microsoft.EntityFrameworkCore;
using PTRP.Models;
using PTRP.Data.Repositories.Interfaces;

namespace PTRP.Data.Repositories;

/// <summary>
/// Implementazione del repository per l'entità ActualVisitModel (Visite Effettive)
/// Usa Entity Framework Core per le operazioni database
/// Gestisce la relazione 1:1 critica con ScheduledVisitModel tramite UNIQUE constraint su ScheduledVisitId
/// Gestisce relazioni N-N con ProfessionalEducator tramite VisitOperatorModel
/// </summary>
public class ActualVisitRepository : IActualVisitRepository
{
    private readonly PTRPDbContext _context;

    public ActualVisitRepository(PTRPDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ActualVisitModel>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.ActualVisits
            .AsNoTracking()
            .OrderByDescending(av => av.VisitDate)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<ActualVisitModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.ActualVisits
            .AsNoTracking()
            .FirstOrDefaultAsync(av => av.Id == id, ct);
    }

    /// <inheritdoc />
    public async Task<ActualVisitModel?> GetByIdWithRelationsAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.ActualVisits
            .Include(av => av.ScheduledVisit)
                .ThenInclude(sv => sv.TherapyProject)
                    .ThenInclude(tp => tp.Patient)
            .Include(av => av.VisitOperators)
                .ThenInclude(vo => vo.Operator)
            .AsNoTracking()
            .FirstOrDefaultAsync(av => av.Id == id, ct);
    }

    /// <inheritdoc />
    public async Task<ActualVisitModel?> GetByScheduledVisitIdAsync(Guid scheduledVisitId, CancellationToken ct = default)
    {
        return await _context.ActualVisits
            .AsNoTracking()
            .FirstOrDefaultAsync(av => av.ScheduledVisitId == scheduledVisitId, ct);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ActualVisitModel>> GetByEducatorIdInRangeAsync(
        Guid educatorId,
        DateTime fromDate,
        DateTime toDate,
        CancellationToken ct = default)
    {
        return await _context.ActualVisits
            .Include(av => av.ScheduledVisit)
                .ThenInclude(sv => sv.TherapyProject)
                    .ThenInclude(tp => tp.Patient)
            .Include(av => av.VisitOperators)
            .AsNoTracking()
            .Where(av => av.VisitDate >= fromDate &&
                         av.VisitDate <= toDate &&
                         av.VisitOperators.Any(vo => vo.OperatorId == educatorId))
            .OrderByDescending(av => av.VisitDate)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ActualVisitModel>> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default)
    {
        return await _context.ActualVisits
            .Include(av => av.ScheduledVisit)
            .AsNoTracking()
            .Where(av => av.ScheduledVisit.TherapyProjectId == projectId)
            .OrderByDescending(av => av.VisitDate)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ActualVisitModel>> GetByPatientIdAsync(Guid patientId, CancellationToken ct = default)
    {
        return await _context.ActualVisits
            .Include(av => av.ScheduledVisit)
                .ThenInclude(sv => sv.TherapyProject)
            .AsNoTracking()
            .Where(av => av.ScheduledVisit.TherapyProject.PatientId == patientId)
            .OrderByDescending(av => av.VisitDate)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ActualVisitModel>> GetByPatientAttendanceAsync(
        string attendanceStatus,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(attendanceStatus))
        {
            return await GetAllAsync(ct);
        }

        var normalizedStatus = attendanceStatus.Trim();

        return await _context.ActualVisits
            .AsNoTracking()
            .Where(av => av.PatientAttendance == normalizedStatus)
            .OrderByDescending(av => av.VisitDate)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ActualVisitModel>> GetByDateAsync(DateTime date, CancellationToken ct = default)
    {
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1).AddTicks(-1);

        return await _context.ActualVisits
            .AsNoTracking()
            .Where(av => av.VisitDate >= startOfDay && av.VisitDate <= endOfDay)
            .OrderByDescending(av => av.VisitDate)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ActualVisitModel>> GetInRangeAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken ct = default)
    {
        return await _context.ActualVisits
            .AsNoTracking()
            .Where(av => av.VisitDate >= fromDate && av.VisitDate <= toDate)
            .OrderByDescending(av => av.VisitDate)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task AddAsync(ActualVisitModel actualVisit, CancellationToken ct = default)
    {
        if (actualVisit == null)
            throw new ArgumentNullException(nameof(actualVisit));

        // Verifica che l'appuntamento programmato esista
        var scheduledVisit = await _context.ScheduledVisits
            .FirstOrDefaultAsync(sv => sv.Id == actualVisit.ScheduledVisitId, ct);
        
        if (scheduledVisit == null)
        {
            throw new InvalidOperationException(
                $"Cannot create ActualVisit: ScheduledVisit with ID {actualVisit.ScheduledVisitId} does not exist");
        }

        // VINCOLO CRITICO 1:1: Verifica che non esista già una visita effettiva per questo appuntamento
        var existingActualVisit = await _context.ActualVisits
            .AnyAsync(av => av.ScheduledVisitId == actualVisit.ScheduledVisitId, ct);
        
        if (existingActualVisit)
        {
            throw new InvalidOperationException(
                $"Cannot create ActualVisit: A visit already exists for ScheduledVisit with ID {actualVisit.ScheduledVisitId}. " +
                "The 1:1 relationship constraint requires that each ScheduledVisit has at most one ActualVisit.");
        }

        // Assicurati che abbia un ID
        if (actualVisit.Id == Guid.Empty)
        {
            actualVisit.Id = Guid.NewGuid();
        }

        // Imposta CreatedAt se non già impostato
        if (actualVisit.CreatedAt == default)
        {
            actualVisit.CreatedAt = DateTime.Now;
        }

        // Imposta VisitSource se non già impostato
        if (string.IsNullOrEmpty(actualVisit.VisitSource))
        {
            actualVisit.VisitSource = "EducatorImport"; // Default: importazione da educatore
        }

        await _context.ActualVisits.AddAsync(actualVisit, ct);
        await _context.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(ActualVisitModel actualVisit, CancellationToken ct = default)
    {
        if (actualVisit == null)
            throw new ArgumentNullException(nameof(actualVisit));

        var existingVisit = await _context.ActualVisits
            .FindAsync(new object[] { actualVisit.Id }, cancellationToken: ct);
            
        if (existingVisit == null)
        {
            throw new InvalidOperationException(
                $"ActualVisit with ID {actualVisit.Id} not found");
        }

        // Non permettere il cambio dell'appuntamento programmato (relazione 1:1 immutabile)
        if (existingVisit.ScheduledVisitId != actualVisit.ScheduledVisitId)
        {
            throw new InvalidOperationException(
                $"Cannot change ScheduledVisitId of an existing ActualVisit. " +
                "The 1:1 relationship is immutable. Delete and create a new ActualVisit instead.");
        }

        // Aggiorna i campi modificabili
        existingVisit.VisitDate = actualVisit.VisitDate;
        existingVisit.ClinicalNotes = actualVisit.ClinicalNotes;
        existingVisit.PatientAttendance = actualVisit.PatientAttendance;
        existingVisit.VisitSource = actualVisit.VisitSource;
        existingVisit.UpdatedAt = DateTime.Now;

        _context.ActualVisits.Update(existingVisit);
        await _context.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var actualVisit = await _context.ActualVisits
            .Include(av => av.VisitOperators)
            .FirstOrDefaultAsync(av => av.Id == id, ct);
            
        if (actualVisit == null)
        {
            return false;
        }

        // Rimuovi le relazioni N-N con gli operatori prima di eliminare la visita
        actualVisit.VisitOperators.Clear();

        _context.ActualVisits.Remove(actualVisit);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.ActualVisits
            .AsNoTracking()
            .AnyAsync(av => av.Id == id, ct);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByScheduledVisitIdAsync(Guid scheduledVisitId, CancellationToken ct = default)
    {
        return await _context.ActualVisits
            .AsNoTracking()
            .AnyAsync(av => av.ScheduledVisitId == scheduledVisitId, ct);
    }

    /// <inheritdoc />
    public async Task<int> CountByProjectIdAsync(Guid projectId, CancellationToken ct = default)
    {
        return await _context.ActualVisits
            .Include(av => av.ScheduledVisit)
            .AsNoTracking()
            .CountAsync(av => av.ScheduledVisit.TherapyProjectId == projectId, ct);
    }

    /// <inheritdoc />
    public async Task<int> CountByEducatorInRangeAsync(
        Guid educatorId,
        DateTime fromDate,
        DateTime toDate,
        CancellationToken ct = default)
    {
        return await _context.ActualVisits
            .Include(av => av.VisitOperators)
            .AsNoTracking()
            .CountAsync(av => av.VisitDate >= fromDate &&
                              av.VisitDate <= toDate &&
                              av.VisitOperators.Any(vo => vo.OperatorId == educatorId), ct);
    }
}
