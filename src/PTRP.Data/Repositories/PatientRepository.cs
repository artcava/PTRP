using Microsoft.EntityFrameworkCore;
using PTRP.Models;
using PTRP.Data.Repositories.Interfaces;

namespace PTRP.Data.Repositories;

/// <summary>
/// Implementazione del repository per l'entità Patient
/// Usa Entity Framework Core per le operazioni database
/// 
/// Nota: Ogni operazione CRUD effettua un commit atomico automatico.
/// </summary>
public class PatientRepository : IPatientRepository
{
    private readonly PTRPDbContext _context;

    public PatientRepository(PTRPDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<PatientModel>> GetAllAsync()
    {
        return await _context.Patients
            .AsNoTracking()
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<PatientModel?> GetByIdAsync(Guid id)
    {
        return await _context.Patients
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    /// <inheritdoc />
    public async Task<PatientModel?> GetByIdWithProjectsAsync(Guid id)
    {
        return await _context.Patients
            .Include(p => p.TherapyProjects)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<PatientModel>> SearchAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return await GetAllAsync();
        }

        var normalizedSearchTerm = searchTerm.Trim().ToLower();

        return await _context.Patients
            .AsNoTracking()
            .Where(p => 
                p.FirstName.ToLower().Contains(normalizedSearchTerm) ||
                p.LastName.ToLower().Contains(normalizedSearchTerm))
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task AddAsync(PatientModel patient)
    {
        if (patient == null)
            throw new ArgumentNullException(nameof(patient));

        // Assicurati che abbia un ID
        if (patient.Id == Guid.Empty)
        {
            patient.Id = Guid.NewGuid();
        }

        // Imposta CreatedAt se non già impostato
        if (patient.CreatedAt == default)
        {
            patient.CreatedAt = DateTime.Now;
        }

        await _context.Patients.AddAsync(patient);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task UpdateAsync(PatientModel patient)
    {
        if (patient == null)
            throw new ArgumentNullException(nameof(patient));

        var existingPatient = await _context.Patients.FindAsync(patient.Id);
        if (existingPatient == null)
        {
            throw new InvalidOperationException($"Patient with ID {patient.Id} not found");
        }

        // Aggiorna solo i campi modificabili
        existingPatient.FirstName = patient.FirstName;
        existingPatient.LastName = patient.LastName;
        existingPatient.UpdatedAt = DateTime.Now;

        _context.Patients.Update(existingPatient);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(Guid id)
    {
        var patient = await _context.Patients.FindAsync(id);
        if (patient == null)
        {
            return false;
        }

        _context.Patients.Remove(patient);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Patients
            .AsNoTracking()
            .AnyAsync(p => p.Id == id);
    }
}
