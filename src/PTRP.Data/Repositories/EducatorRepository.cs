using Microsoft.EntityFrameworkCore;
using PTRP.Data.Repositories.Interfaces;
using PTRP.Models;

namespace PTRP.Data.Repositories;

/// <summary>
/// Implementazione del repository per la gestione degli Educatori Professionali.
/// Utilizza Entity Framework Core per l'accesso ai dati.
/// </summary>
public class EducatorRepository : IEducatorRepository
{
    private readonly PTRPDbContext _context;

    /// <summary>
    /// Inizializza una nuova istanza del repository.
    /// </summary>
    /// <param name="context">Contesto del database</param>
    public EducatorRepository(PTRPDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ProfessionalEducatorModel>> GetAllAsync()
    {
        return await _context.ProfessionalEducators
            .AsNoTracking()
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<ProfessionalEducatorModel?> GetByIdAsync(Guid id)
    {
        return await _context.ProfessionalEducators
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    /// <inheritdoc />
    public async Task<ProfessionalEducatorModel?> GetByIdWithProjectsAsync(Guid id)
    {
        return await _context.ProfessionalEducators
            .Include(e => e.AssignedTherapyProjects)
                .ThenInclude(p => p.Patient)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ProfessionalEducatorModel>> GetByStatusAsync(string status)
    {
        return await _context.ProfessionalEducators
            .AsNoTracking()
            .Where(e => e.Status == status)
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ProfessionalEducatorModel>> GetBySpecializationAsync(string specialization)
    {
        return await _context.ProfessionalEducators
            .AsNoTracking()
            .Where(e => e.Specialization == specialization)
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ProfessionalEducatorModel>> GetByProjectIdAsync(Guid projectId)
    {
        return await _context.ProfessionalEducators
            .AsNoTracking()
            .Where(e => e.AssignedTherapyProjects.Any(p => p.Id == projectId))
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ProfessionalEducatorModel>> SearchAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return Enumerable.Empty<ProfessionalEducatorModel>();

        var normalizedTerm = searchTerm.ToLower().Trim();

        return await _context.ProfessionalEducators
            .AsNoTracking()
            .Where(e => 
                (e.FirstName != null && e.FirstName.ToLower().Contains(normalizedTerm)) ||
                (e.LastName != null && e.LastName.ToLower().Contains(normalizedTerm)) ||
                (e.Email != null && e.Email.ToLower().Contains(normalizedTerm)))
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task AddAsync(ProfessionalEducatorModel educator)
    {
        if (educator == null)
            throw new ArgumentNullException(nameof(educator));

        // Verifica che l'email non sia già in uso
        if (!string.IsNullOrWhiteSpace(educator.Email))
        {
            var emailExists = await EmailExistsAsync(educator.Email);
            if (emailExists)
                throw new InvalidOperationException($"Un educatore con email '{educator.Email}' esiste già.");
        }

        educator.CreatedAt = DateTime.Now;
        educator.UpdatedAt = null;

        await _context.ProfessionalEducators.AddAsync(educator);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task UpdateAsync(ProfessionalEducatorModel educator)
    {
        if (educator == null)
            throw new ArgumentNullException(nameof(educator));

        var existing = await _context.ProfessionalEducators
            .FirstOrDefaultAsync(e => e.Id == educator.Id);

        if (existing == null)
            throw new InvalidOperationException($"Educatore con ID {educator.Id} non trovato.");

        // Verifica unicità email (escludendo l'educatore corrente)
        if (!string.IsNullOrWhiteSpace(educator.Email))
        {
            var emailExists = await EmailExistsAsync(educator.Email, educator.Id);
            if (emailExists)
                throw new InvalidOperationException($"Un altro educatore con email '{educator.Email}' esiste già.");
        }

        // Aggiorna i campi
        existing.FirstName = educator.FirstName;
        existing.LastName = educator.LastName;
        existing.Email = educator.Email;
        existing.PhoneNumber = educator.PhoneNumber;
        existing.DateOfBirth = educator.DateOfBirth;
        existing.Specialization = educator.Specialization;
        existing.LicenseNumber = educator.LicenseNumber;
        existing.HireDate = educator.HireDate;
        existing.Status = educator.Status;
        existing.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(Guid id)
    {
        var educator = await _context.ProfessionalEducators
            .Include(e => e.AssignedTherapyProjects)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (educator == null)
            return false;

        // Rimuovi le assegnazioni ai progetti (N-N)
        educator.AssignedTherapyProjects.Clear();

        _context.ProfessionalEducators.Remove(educator);
        await _context.SaveChangesAsync();

        return true;
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.ProfessionalEducators
            .AsNoTracking()
            .AnyAsync(e => e.Id == id);
    }

    /// <inheritdoc />
    public async Task<bool> EmailExistsAsync(string email, Guid? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var query = _context.ProfessionalEducators
            .AsNoTracking()
            .Where(e => e.Email == email);

        if (excludeId.HasValue)
            query = query.Where(e => e.Id != excludeId.Value);

        return await query.AnyAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>> GetUniqueSpecializationsAsync()
    {
        return await _context.ProfessionalEducators
            .AsNoTracking()
            .Where(e => !string.IsNullOrWhiteSpace(e.Specialization))
            .Select(e => e.Specialization!)
            .Distinct()
            .OrderBy(s => s)
            .ToListAsync();
    }
}
