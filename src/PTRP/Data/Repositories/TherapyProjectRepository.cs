using Microsoft.EntityFrameworkCore;
using PTRP.Models;

namespace PTRP.Data.Repositories;

/// <summary>
/// Repository per la gestione dei progetti terapeutici (TherapyProject).
/// </summary>
public class TherapyProjectRepository
{
    private readonly PTRPDbContext _context;

    public TherapyProjectRepository(PTRPDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Recupera un progetto terapeutico per ID.
    /// </summary>
    public async Task<TherapyProjectModel?> GetByIdAsync(Guid id)
    {
        return await _context.TherapyProjects
            .Include(tp => tp.Patient)
            .Include(tp => tp.ProfessionalEducators)
            .Include(tp => tp.ScheduledVisits)
            .FirstOrDefaultAsync(tp => tp.Id == id);
    }

    /// <summary>
    /// Recupera tutti i progetti terapeutici di un paziente.
    /// </summary>
    public async Task<IEnumerable<TherapyProjectModel>> GetByPatientIdAsync(Guid patientId)
    {
        return await _context.TherapyProjects
            .Include(tp => tp.ProfessionalEducators)
            .Where(tp => tp.PatientId == patientId)
            .OrderByDescending(tp => tp.StartDate)
            .ToListAsync();
    }

    /// <summary>
    /// Recupera tutti i progetti terapeutici.
    /// </summary>
    public async Task<IEnumerable<TherapyProjectModel>> GetAllAsync()
    {
        return await _context.TherapyProjects
            .Include(tp => tp.Patient)
            .Include(tp => tp.ProfessionalEducators)
            .OrderByDescending(tp => tp.StartDate)
            .ToListAsync();
    }

    /// <summary>
    /// Recupera i progetti terapeutici per stato.
    /// </summary>
    public async Task<IEnumerable<TherapyProjectModel>> GetByStatusAsync(string status)
    {
        return await _context.TherapyProjects
            .Include(tp => tp.Patient)
            .Where(tp => tp.Status == status)
            .OrderByDescending(tp => tp.StartDate)
            .ToListAsync();
    }

    /// <summary>
    /// Recupera i progetti terapeutici di un educatore.
    /// </summary>
    public async Task<IEnumerable<TherapyProjectModel>> GetByEducatorIdAsync(Guid educatorId)
    {
        return await _context.TherapyProjects
            .Include(tp => tp.Patient)
            .Include(tp => tp.ProfessionalEducators)
            .Where(tp => tp.ProfessionalEducators.Any(pe => pe.Id == educatorId))
            .OrderByDescending(tp => tp.StartDate)
            .ToListAsync();
    }

    /// <summary>
    /// Aggiunge un nuovo progetto terapeutico.
    /// Verifica che il paziente esista.
    /// </summary>
    public async Task AddAsync(TherapyProjectModel project)
    {
        var patientExists = await _context.Patients
            .AnyAsync(p => p.Id == project.PatientId);

        if (!patientExists)
        {
            throw new InvalidOperationException(
                $"Il paziente con ID {project.PatientId} non esiste.");
        }

        _context.TherapyProjects.Add(project);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Aggiorna un progetto terapeutico esistente.
    /// </summary>
    public async Task UpdateAsync(TherapyProjectModel project)
    {
        _context.TherapyProjects.Update(project);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Elimina un progetto terapeutico.
    /// </summary>
    public async Task<bool> DeleteAsync(Guid id)
    {
        var project = await _context.TherapyProjects.FindAsync(id);

        if (project == null)
        {
            return false;
        }

        _context.TherapyProjects.Remove(project);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Verifica se un progetto terapeutico esiste.
    /// </summary>
    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.TherapyProjects.AnyAsync(tp => tp.Id == id);
    }

    /// <summary>
    /// Conta i progetti terapeutici di un paziente.
    /// </summary>
    public async Task<int> CountByPatientIdAsync(Guid patientId)
    {
        return await _context.TherapyProjects
            .CountAsync(tp => tp.PatientId == patientId);
    }
}
