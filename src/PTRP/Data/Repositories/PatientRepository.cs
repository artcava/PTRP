using Microsoft.EntityFrameworkCore;
using PTRP.Models;

namespace PTRP.Data.Repositories;

/// <summary>
/// Repository per la gestione dei pazienti (Patient).
/// </summary>
public class PatientRepository
{
    private readonly PTRPDbContext _context;

    public PatientRepository(PTRPDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Recupera un paziente per ID.
    /// </summary>
    public async Task<PatientModel?> GetByIdAsync(Guid id)
    {
        return await _context.Patients
            .Include(p => p.TherapyProjects)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    /// <summary>
    /// Recupera tutti i pazienti.
    /// </summary>
    public async Task<IEnumerable<PatientModel>> GetAllAsync()
    {
        return await _context.Patients
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .ToListAsync();
    }

    /// <summary>
    /// Cerca pazienti per nome o cognome.
    /// </summary>
    public async Task<IEnumerable<PatientModel>> SearchAsync(string searchTerm)
    {
        return await _context.Patients
            .Where(p => p.FirstName.Contains(searchTerm) || 
                       p.LastName.Contains(searchTerm))
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .ToListAsync();
    }

    /// <summary>
    /// Aggiunge un nuovo paziente.
    /// </summary>
    public async Task AddAsync(PatientModel patient)
    {
        _context.Patients.Add(patient);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Aggiorna un paziente esistente.
    /// </summary>
    public async Task UpdateAsync(PatientModel patient)
    {
        _context.Patients.Update(patient);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Elimina un paziente.
    /// </summary>
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

    /// <summary>
    /// Verifica se un paziente esiste.
    /// </summary>
    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Patients.AnyAsync(p => p.Id == id);
    }

    /// <summary>
    /// Conta tutti i pazienti.
    /// </summary>
    public async Task<int> CountAsync()
    {
        return await _context.Patients.CountAsync();
    }
}
