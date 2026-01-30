using System.ComponentModel.DataAnnotations;
using PTRP.Data.Repositories.Interfaces;
using PTRP.Models;
using PTRP.Services.Interfaces;

namespace PTRP.Services;

/// <summary>
/// Implementazione del servizio per la gestione degli Educatori Professionali.
/// Gestisce validazioni e business logic.
/// </summary>
public class EducatorService : IEducatorService
{
    private readonly IEducatorRepository _educatorRepository;
    private static readonly string[] ValidStatuses = { "Active", "Inactive", "OnLeave" };

    /// <summary>
    /// Inizializza una nuova istanza del servizio.
    /// </summary>
    /// <param name="educatorRepository">Repository degli educatori</param>
    public EducatorService(IEducatorRepository educatorRepository)
    {
        _educatorRepository = educatorRepository ?? throw new ArgumentNullException(nameof(educatorRepository));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ProfessionalEducatorModel>> GetAllAsync()
    {
        return await _educatorRepository.GetAllAsync();
    }

    /// <inheritdoc />
    public async Task<ProfessionalEducatorModel?> GetByIdAsync(Guid id)
    {
        return await _educatorRepository.GetByIdAsync(id);
    }

    /// <inheritdoc />
    public async Task<ProfessionalEducatorModel?> GetByIdWithProjectsAsync(Guid id)
    {
        return await _educatorRepository.GetByIdWithProjectsAsync(id);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ProfessionalEducatorModel>> GetByStatusAsync(string status)
    {
        if (!ValidStatuses.Contains(status))
            throw new ArgumentException($"Status non valido. Valori ammessi: {string.Join(", ", ValidStatuses)}", nameof(status));

        return await _educatorRepository.GetByStatusAsync(status);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ProfessionalEducatorModel>> GetActiveEducatorsAsync()
    {
        return await _educatorRepository.GetByStatusAsync("Active");
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ProfessionalEducatorModel>> GetBySpecializationAsync(string specialization)
    {
        if (string.IsNullOrWhiteSpace(specialization))
            throw new ArgumentException("La specializzazione non può essere vuota.", nameof(specialization));

        return await _educatorRepository.GetBySpecializationAsync(specialization);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ProfessionalEducatorModel>> GetByProjectIdAsync(Guid projectId)
    {
        return await _educatorRepository.GetByProjectIdAsync(projectId);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ProfessionalEducatorModel>> SearchAsync(string searchTerm)
    {
        return await _educatorRepository.SearchAsync(searchTerm);
    }

    /// <inheritdoc />
    public async Task AddAsync(ProfessionalEducatorModel educator)
    {
        if (educator == null)
            throw new ArgumentNullException(nameof(educator));

        await ValidateAsync(educator);

        // Validazione email duplicata
        if (!string.IsNullOrWhiteSpace(educator.Email))
        {
            var emailExists = await _educatorRepository.EmailExistsAsync(educator.Email);
            if (emailExists)
                throw new InvalidOperationException($"L'email '{educator.Email}' è già in uso.");
        }

        await _educatorRepository.AddAsync(educator);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(ProfessionalEducatorModel educator)
    {
        if (educator == null)
            throw new ArgumentNullException(nameof(educator));

        var exists = await _educatorRepository.ExistsAsync(educator.Id);
        if (!exists)
            throw new InvalidOperationException($"Educatore con ID {educator.Id} non trovato.");

        await ValidateAsync(educator);

        // Validazione email duplicata (escludendo l'educatore corrente)
        if (!string.IsNullOrWhiteSpace(educator.Email))
        {
            var emailExists = await _educatorRepository.EmailExistsAsync(educator.Email, educator.Id);
            if (emailExists)
                throw new InvalidOperationException($"Un altro educatore con email '{educator.Email}' esiste già.");
        }

        await _educatorRepository.UpdateAsync(educator);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid id)
    {
        var deleted = await _educatorRepository.DeleteAsync(id);
        if (!deleted)
            throw new InvalidOperationException($"Educatore con ID {id} non trovato.");
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _educatorRepository.ExistsAsync(id);
    }

    /// <inheritdoc />
    public async Task<bool> ValidateAsync(ProfessionalEducatorModel educator)
    {
        if (educator == null)
            throw new ArgumentNullException(nameof(educator));

        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(educator);

        // Validazioni campi obbligatori
        if (string.IsNullOrWhiteSpace(educator.FirstName))
            validationResults.Add(new ValidationResult("Il nome è obbligatorio.", new[] { nameof(educator.FirstName) }));

        if (string.IsNullOrWhiteSpace(educator.LastName))
            validationResults.Add(new ValidationResult("Il cognome è obbligatorio.", new[] { nameof(educator.LastName) }));

        if (string.IsNullOrWhiteSpace(educator.Email))
            validationResults.Add(new ValidationResult("L'email è obbligatoria.", new[] { nameof(educator.Email) }));
        else if (!IsValidEmail(educator.Email))
            validationResults.Add(new ValidationResult("Formato email non valido.", new[] { nameof(educator.Email) }));

        if (string.IsNullOrWhiteSpace(educator.PhoneNumber))
            validationResults.Add(new ValidationResult("Il numero di telefono è obbligatorio.", new[] { nameof(educator.PhoneNumber) }));

        if (string.IsNullOrWhiteSpace(educator.Specialization))
            validationResults.Add(new ValidationResult("La specializzazione è obbligatoria.", new[] { nameof(educator.Specialization) }));

        if (string.IsNullOrWhiteSpace(educator.LicenseNumber))
            validationResults.Add(new ValidationResult("Il numero di licenza è obbligatorio.", new[] { nameof(educator.LicenseNumber) }));

        // Validazioni date
        if (educator.DateOfBirth >= DateTime.Today)
            validationResults.Add(new ValidationResult("La data di nascita deve essere nel passato.", new[] { nameof(educator.DateOfBirth) }));

        var age = DateTime.Today.Year - educator.DateOfBirth.Year;
        if (educator.DateOfBirth.Date > DateTime.Today.AddYears(-age)) age--;
        if (age < 18)
            validationResults.Add(new ValidationResult("L'educatore deve avere almeno 18 anni.", new[] { nameof(educator.DateOfBirth) }));
        if (age > 100)
            validationResults.Add(new ValidationResult("La data di nascita non è realistica.", new[] { nameof(educator.DateOfBirth) }));

        if (educator.HireDate > DateTime.Today.AddYears(1))
            validationResults.Add(new ValidationResult("La data di assunzione non può essere oltre un anno nel futuro.", new[] { nameof(educator.HireDate) }));

        // Validazione status
        if (!ValidStatuses.Contains(educator.Status))
            validationResults.Add(new ValidationResult($"Status non valido. Valori ammessi: {string.Join(", ", ValidStatuses)}", new[] { nameof(educator.Status) }));

        if (validationResults.Any())
        {
            var errors = string.Join("; ", validationResults.Select(v => v.ErrorMessage));
            throw new ArgumentException($"Validazione fallita: {errors}");
        }

        return await Task.FromResult(true);
    }

    /// <inheritdoc />
    public async Task DeactivateAsync(Guid id)
    {
        var educator = await _educatorRepository.GetByIdAsync(id);
        if (educator == null)
            throw new InvalidOperationException($"Educatore con ID {id} non trovato.");

        if (educator.Status == "Inactive")
            throw new InvalidOperationException("L'educatore è già inattivo.");

        educator.Status = "Inactive";
        await _educatorRepository.UpdateAsync(educator);
    }

    /// <inheritdoc />
    public async Task ActivateAsync(Guid id)
    {
        var educator = await _educatorRepository.GetByIdAsync(id);
        if (educator == null)
            throw new InvalidOperationException($"Educatore con ID {id} non trovato.");

        if (educator.Status == "Active")
            throw new InvalidOperationException("L'educatore è già attivo.");

        educator.Status = "Active";
        await _educatorRepository.UpdateAsync(educator);
    }

    /// <inheritdoc />
    public async Task SetOnLeaveAsync(Guid id)
    {
        var educator = await _educatorRepository.GetByIdAsync(id);
        if (educator == null)
            throw new InvalidOperationException($"Educatore con ID {id} non trovato.");

        if (educator.Status == "OnLeave")
            throw new InvalidOperationException("L'educatore è già in permesso.");

        educator.Status = "OnLeave";
        await _educatorRepository.UpdateAsync(educator);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>> GetAvailableSpecializationsAsync()
    {
        return await _educatorRepository.GetUniqueSpecializationsAsync();
    }

    /// <summary>
    /// Valida formato email con regex semplice.
    /// </summary>
    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
