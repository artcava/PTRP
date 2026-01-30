using Microsoft.EntityFrameworkCore;
using PTRP.Models;
using PTRP.Data.Repositories.Interfaces;

namespace PTRP.Data.Repositories;

/// <summary>
/// Implementazione del repository per l'entità TherapyProject
/// Usa Entity Framework Core per le operazioni database
/// Gestisce relazioni 1-N con Patient e N-N con ProfessionalEducator
/// </summary>
public class TherapyProjectRepository : ITherapyProjectRepository
{
    private readonly PTRPDbContext _context;

    public TherapyProjectRepository(PTRPDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TherapyProjectModel>> GetAllAsync()
    {
        return await _context.TherapyProjects
            .AsNoTracking()
            .OrderByDescending(tp => tp.StartDate)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<TherapyProjectModel?> GetByIdAsync(Guid id)
    {
        return await _context.TherapyProjects
            .AsNoTracking()
            .FirstOrDefaultAsync(tp => tp.Id == id);
    }

    /// <inheritdoc />
    public async Task<TherapyProjectModel?> GetByIdWithPatientAsync(Guid id)
    {
        return await _context.TherapyProjects
            .Include(tp => tp.Patient)
            .AsNoTracking()
            .FirstOrDefaultAsync(tp => tp.Id == id);
    }

    /// <inheritdoc />
    public async Task<TherapyProjectModel?> GetByIdWithRelationsAsync(Guid id)
    {
        return await _context.TherapyProjects
            .Include(tp => tp.Patient)
            .Include(tp => tp.ProfessionalEducators)
            .AsNoTracking()
            .FirstOrDefaultAsync(tp => tp.Id == id);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TherapyProjectModel>> GetByPatientIdAsync(Guid patientId)
    {
        return await _context.TherapyProjects
            .AsNoTracking()
            .Where(tp => tp.PatientId == patientId)
            .OrderByDescending(tp => tp.StartDate)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TherapyProjectModel>> GetByEducatorIdAsync(Guid educatorId)
    {
        return await _context.TherapyProjects
            .Include(tp => tp.ProfessionalEducators)
            .AsNoTracking()
            .Where(tp => tp.ProfessionalEducators.Any(pe => pe.Id == educatorId))
            .OrderByDescending(tp => tp.StartDate)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TherapyProjectModel>> GetByStatusAsync(string status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return await GetAllAsync();
        }

        var normalizedStatus = status.Trim();

        return await _context.TherapyProjects
            .AsNoTracking()
            .Where(tp => tp.Status == normalizedStatus)
            .OrderByDescending(tp => tp.StartDate)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TherapyProjectModel>> SearchAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return await GetAllAsync();
        }

        var normalizedSearchTerm = searchTerm.Trim().ToLower();

        return await _context.TherapyProjects
            .AsNoTracking()
            .Where(tp => 
                (tp.Title ?? "").ToLower().Contains(normalizedSearchTerm) ||
                (tp.Description ?? "").ToLower().Contains(normalizedSearchTerm))
            .OrderByDescending(tp => tp.StartDate)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task AddAsync(TherapyProjectModel therapyProject)
    {
        if (therapyProject == null)
            throw new ArgumentNullException(nameof(therapyProject));

        // Assicurati che abbia un ID
        if (therapyProject.Id == Guid.Empty)
        {
            therapyProject.Id = Guid.NewGuid();
        }

        // Imposta CreatedAt se non già impostato
        if (therapyProject.CreatedAt == default)
        {
            therapyProject.CreatedAt = DateTime.Now;
        }

        // Verifica che il paziente esista
        var patientExists = await _context.Patients
            .AnyAsync(p => p.Id == therapyProject.PatientId);
        
        if (!patientExists)
        {
            throw new InvalidOperationException(
                $"Cannot create TherapyProject: Patient with ID {therapyProject.PatientId} does not exist");
        }

        await _context.TherapyProjects.AddAsync(therapyProject);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task UpdateAsync(TherapyProjectModel therapyProject)
    {
        if (therapyProject == null)
            throw new ArgumentNullException(nameof(therapyProject));

        var existingProject = await _context.TherapyProjects.FindAsync(therapyProject.Id);
        if (existingProject == null)
        {
            throw new InvalidOperationException(
                $"TherapyProject with ID {therapyProject.Id} not found");
        }

        // Verifica che il paziente esista se è stato modificato
        if (existingProject.PatientId != therapyProject.PatientId)
        {
            var patientExists = await _context.Patients
                .AnyAsync(p => p.Id == therapyProject.PatientId);
            
            if (!patientExists)
            {
                throw new InvalidOperationException(
                    $"Cannot update TherapyProject: Patient with ID {therapyProject.PatientId} does not exist");
            }
        }

        // Aggiorna i campi modificabili
        existingProject.PatientId = therapyProject.PatientId;
        existingProject.Title = therapyProject.Title;
        existingProject.Description = therapyProject.Description;
        existingProject.StartDate = therapyProject.StartDate;
        existingProject.EndDate = therapyProject.EndDate;
        existingProject.Status = therapyProject.Status;
        existingProject.UpdatedAt = DateTime.Now;

        _context.TherapyProjects.Update(existingProject);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(Guid id)
    {
        var therapyProject = await _context.TherapyProjects
            .Include(tp => tp.ProfessionalEducators)
            .FirstOrDefaultAsync(tp => tp.Id == id);
            
        if (therapyProject == null)
        {
            return false;
        }

        // Rimuovi le relazioni N-N con gli educatori prima di eliminare il progetto
        therapyProject.ProfessionalEducators.Clear();

        _context.TherapyProjects.Remove(therapyProject);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.TherapyProjects
            .AsNoTracking()
            .AnyAsync(tp => tp.Id == id);
    }

    /// <inheritdoc />
    public async Task AssignEducatorAsync(Guid projectId, Guid educatorId)
    {
        var project = await _context.TherapyProjects
            .Include(tp => tp.ProfessionalEducators)
            .FirstOrDefaultAsync(tp => tp.Id == projectId);

        if (project == null)
        {
            throw new InvalidOperationException(
                $"TherapyProject with ID {projectId} not found");
        }

        var educator = await _context.ProfessionalEducators
            .FindAsync(educatorId);

        if (educator == null)
        {
            throw new InvalidOperationException(
                $"ProfessionalEducator with ID {educatorId} not found");
        }

        // Verifica che l'educatore non sia già assegnato
        if (project.ProfessionalEducators.Any(pe => pe.Id == educatorId))
        {
            return; // Già assegnato, nessuna azione necessaria
        }

        project.ProfessionalEducators.Add(educator);
        project.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task RemoveEducatorAsync(Guid projectId, Guid educatorId)
    {
        var project = await _context.TherapyProjects
            .Include(tp => tp.ProfessionalEducators)
            .FirstOrDefaultAsync(tp => tp.Id == projectId);

        if (project == null)
        {
            throw new InvalidOperationException(
                $"TherapyProject with ID {projectId} not found");
        }

        var educator = project.ProfessionalEducators
            .FirstOrDefault(pe => pe.Id == educatorId);

        if (educator == null)
        {
            return; // Non assegnato, nessuna azione necessaria
        }

        project.ProfessionalEducators.Remove(educator);
        project.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();
    }
}
