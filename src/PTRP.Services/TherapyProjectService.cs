using System.ComponentModel.DataAnnotations;
using PTRP.Models;
using PTRP.Data.Repositories.Interfaces;
using PTRP.Services.Interfaces;

namespace PTRP.Services;

/// <summary>
/// Implementazione del servizio per la gestione dei Progetti Terapeutici
/// Gestisce la logica di business e le validazioni
/// </summary>
public class TherapyProjectService : ITherapyProjectService
{
    private readonly ITherapyProjectRepository _therapyProjectRepository;
    private readonly IPatientRepository _patientRepository;

    public TherapyProjectService(
        ITherapyProjectRepository therapyProjectRepository,
        IPatientRepository patientRepository)
    {
        _therapyProjectRepository = therapyProjectRepository 
            ?? throw new ArgumentNullException(nameof(therapyProjectRepository));
        _patientRepository = patientRepository 
            ?? throw new ArgumentNullException(nameof(patientRepository));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TherapyProjectModel>> GetAllAsync()
    {
        return await _therapyProjectRepository.GetAllAsync();
    }

    /// <inheritdoc />
    public async Task<TherapyProjectModel?> GetByIdAsync(Guid id)
    {
        return await _therapyProjectRepository.GetByIdAsync(id);
    }

    /// <inheritdoc />
    public async Task<TherapyProjectModel?> GetByIdWithPatientAsync(Guid id)
    {
        return await _therapyProjectRepository.GetByIdWithPatientAsync(id);
    }

    /// <inheritdoc />
    public async Task<TherapyProjectModel?> GetByIdWithRelationsAsync(Guid id)
    {
        return await _therapyProjectRepository.GetByIdWithRelationsAsync(id);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TherapyProjectModel>> GetByPatientIdAsync(Guid patientId)
    {
        // Verifica che il paziente esista
        var patientExists = await _patientRepository.ExistsAsync(patientId);
        if (!patientExists)
        {
            throw new InvalidOperationException(
                $"Patient with ID {patientId} does not exist");
        }

        return await _therapyProjectRepository.GetByPatientIdAsync(patientId);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TherapyProjectModel>> GetByEducatorIdAsync(Guid educatorId)
    {
        return await _therapyProjectRepository.GetByEducatorIdAsync(educatorId);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TherapyProjectModel>> GetByStatusAsync(string status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            throw new ArgumentException("Status cannot be null or empty", nameof(status));
        }

        // Valida che lo status sia uno dei valori consentiti
        var validStatuses = new[] { "In Progress", "Completed", "On Hold", "Cancelled" };
        if (!validStatuses.Contains(status))
        {
            throw new ArgumentException(
                $"Invalid status '{status}'. Valid values are: {string.Join(", ", validStatuses)}",
                nameof(status));
        }

        return await _therapyProjectRepository.GetByStatusAsync(status);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TherapyProjectModel>> SearchAsync(string searchTerm)
    {
        return await _therapyProjectRepository.SearchAsync(searchTerm);
    }

    /// <inheritdoc />
    public async Task AddAsync(TherapyProjectModel therapyProject)
    {
        if (therapyProject == null)
            throw new ArgumentNullException(nameof(therapyProject));

        // Validazione business
        await ValidateAsync(therapyProject);

        // Verifica che il paziente esista
        var patientExists = await _patientRepository.ExistsAsync(therapyProject.PatientId);
        if (!patientExists)
        {
            throw new InvalidOperationException(
                $"Cannot create TherapyProject: Patient with ID {therapyProject.PatientId} does not exist");
        }

        await _therapyProjectRepository.AddAsync(therapyProject);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(TherapyProjectModel therapyProject)
    {
        if (therapyProject == null)
            throw new ArgumentNullException(nameof(therapyProject));

        // Verifica che il progetto esista
        var exists = await _therapyProjectRepository.ExistsAsync(therapyProject.Id);
        if (!exists)
        {
            throw new InvalidOperationException(
                $"TherapyProject with ID {therapyProject.Id} does not exist");
        }

        // Validazione business
        await ValidateAsync(therapyProject);

        // Verifica che il paziente esista
        var patientExists = await _patientRepository.ExistsAsync(therapyProject.PatientId);
        if (!patientExists)
        {
            throw new InvalidOperationException(
                $"Cannot update TherapyProject: Patient with ID {therapyProject.PatientId} does not exist");
        }

        await _therapyProjectRepository.UpdateAsync(therapyProject);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid id)
    {
        var deleted = await _therapyProjectRepository.DeleteAsync(id);
        if (!deleted)
        {
            throw new InvalidOperationException(
                $"TherapyProject with ID {id} does not exist");
        }
    }

    /// <inheritdoc />
    public async Task AssignEducatorAsync(Guid projectId, Guid educatorId)
    {
        // Verifica che il progetto esista
        var projectExists = await _therapyProjectRepository.ExistsAsync(projectId);
        if (!projectExists)
        {
            throw new InvalidOperationException(
                $"TherapyProject with ID {projectId} does not exist");
        }

        await _therapyProjectRepository.AssignEducatorAsync(projectId, educatorId);
    }

    /// <inheritdoc />
    public async Task RemoveEducatorAsync(Guid projectId, Guid educatorId)
    {
        // Verifica che il progetto esista
        var projectExists = await _therapyProjectRepository.ExistsAsync(projectId);
        if (!projectExists)
        {
            throw new InvalidOperationException(
                $"TherapyProject with ID {projectId} does not exist");
        }

        await _therapyProjectRepository.RemoveEducatorAsync(projectId, educatorId);
    }

    /// <inheritdoc />
    public async Task<bool> ValidateAsync(TherapyProjectModel therapyProject)
    {
        if (therapyProject == null)
            throw new ArgumentNullException(nameof(therapyProject));

        var validationContext = new ValidationContext(therapyProject);
        var validationResults = new List<ValidationResult>();

        // Validazione con DataAnnotations
        var isValid = Validator.TryValidateObject(
            therapyProject, 
            validationContext, 
            validationResults, 
            validateAllProperties: true);

        if (!isValid)
        {
            var errors = string.Join("; ", validationResults.Select(vr => vr.ErrorMessage));
            throw new ArgumentException($"TherapyProject validation failed: {errors}");
        }

        // Validazioni custom implementate in IValidatableObject
        var customValidationResults = therapyProject.Validate(validationContext);
        if (customValidationResults.Any())
        {
            var errors = string.Join("; ", customValidationResults.Select(vr => vr.ErrorMessage));
            throw new ArgumentException($"TherapyProject business validation failed: {errors}");
        }

        // Validazioni business aggiuntive
        if (therapyProject.EndDate.HasValue && therapyProject.EndDate.Value < therapyProject.StartDate)
        {
            throw new ArgumentException(
                "End date cannot be before start date",
                nameof(therapyProject.EndDate));
        }

        // Verifica che il paziente esista (solo per Add/Update, non per validazione generica)
        // Questa verifica è fatta nei metodi Add/Update per evitare dipendenza ciclica

        return await Task.FromResult(true);
    }

    /// <inheritdoc />
    public async Task CompleteProjectAsync(Guid projectId)
    {
        var project = await _therapyProjectRepository.GetByIdAsync(projectId);
        if (project == null)
        {
            throw new InvalidOperationException(
                $"TherapyProject with ID {projectId} does not exist");
        }

        // Regola business: per completare un progetto deve avere EndDate
        if (!project.EndDate.HasValue)
        {
            throw new InvalidOperationException(
                "Cannot complete project: EndDate must be set");
        }

        // Regola business: non può completare un progetto già completato
        if (project.Status == "Completed")
        {
            throw new InvalidOperationException(
                "Project is already completed");
        }

        project.Status = "Completed";
        project.UpdatedAt = DateTime.Now;

        await _therapyProjectRepository.UpdateAsync(project);
    }

    /// <inheritdoc />
    public async Task PutOnHoldAsync(Guid projectId)
    {
        var project = await _therapyProjectRepository.GetByIdAsync(projectId);
        if (project == null)
        {
            throw new InvalidOperationException(
                $"TherapyProject with ID {projectId} does not exist");
        }

        // Regola business: non può mettere in pausa un progetto completato
        if (project.Status == "Completed")
        {
            throw new InvalidOperationException(
                "Cannot put a completed project on hold");
        }

        project.Status = "On Hold";
        project.UpdatedAt = DateTime.Now;

        await _therapyProjectRepository.UpdateAsync(project);
    }

    /// <inheritdoc />
    public async Task ResumeProjectAsync(Guid projectId)
    {
        var project = await _therapyProjectRepository.GetByIdAsync(projectId);
        if (project == null)
        {
            throw new InvalidOperationException(
                $"TherapyProject with ID {projectId} does not exist");
        }

        // Regola business: si può riprendere solo un progetto in pausa
        if (project.Status != "On Hold")
        {
            throw new InvalidOperationException(
                $"Cannot resume project: current status is '{project.Status}', expected 'On Hold'");
        }

        project.Status = "In Progress";
        project.UpdatedAt = DateTime.Now;

        await _therapyProjectRepository.UpdateAsync(project);
    }
}
