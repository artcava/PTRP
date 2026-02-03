using System.ComponentModel.DataAnnotations;
using PTRP.Models;
using PTRP.Models.Enums;
using PTRP.Data.Repositories.Interfaces;
using PTRP.Services.Configuration;
using PTRP.Services.Interfaces;
using PTRP.Services.Models;

namespace PTRP.Services;

/// <summary>
/// Implementazione del servizio per la gestione dei Progetti Terapeutici
/// Gestisce la logica di business e le validazioni
/// </summary>
public class TherapyProjectService : ITherapyProjectService
{
    private readonly ITherapyProjectRepository _therapyProjectRepository;
    private readonly IPatientRepository _patientRepository;
    private readonly IScheduledVisitRepository _scheduledVisitRepository;
    private readonly IEducatorRepository _educatorRepository;
    private readonly CanonicalAppointmentsConfiguration _appointmentsConfig;

    public TherapyProjectService(
        ITherapyProjectRepository therapyProjectRepository,
        IPatientRepository patientRepository,
        IScheduledVisitRepository scheduledVisitRepository,
        IEducatorRepository educatorRepository,
        CanonicalAppointmentsConfiguration? appointmentsConfig = null)
    {
        _therapyProjectRepository = therapyProjectRepository 
            ?? throw new ArgumentNullException(nameof(therapyProjectRepository));
        _patientRepository = patientRepository 
            ?? throw new ArgumentNullException(nameof(patientRepository));
        _scheduledVisitRepository = scheduledVisitRepository
            ?? throw new ArgumentNullException(nameof(scheduledVisitRepository));
        _educatorRepository = educatorRepository
            ?? throw new ArgumentNullException(nameof(educatorRepository));
        _appointmentsConfig = appointmentsConfig ?? CanonicalAppointmentsConfiguration.Default;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TherapyProjectModel>> GetAllAsync(CancellationToken ct = default)
    {
        return await _therapyProjectRepository.GetAllAsync();
    }

    /// <inheritdoc />
    public async Task<TherapyProjectModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _therapyProjectRepository.GetByIdAsync(id);
    }

    /// <inheritdoc />
    public async Task<TherapyProjectModel?> GetByIdWithPatientAsync(Guid id, CancellationToken ct = default)
    {
        return await _therapyProjectRepository.GetByIdWithPatientAsync(id);
    }

    /// <inheritdoc />
    public async Task<TherapyProjectModel?> GetByIdWithRelationsAsync(Guid id, CancellationToken ct = default)
    {
        return await _therapyProjectRepository.GetByIdWithRelationsAsync(id);
    }

    /// <inheritdoc />
    public async Task<TherapyProjectModel?> GetActiveForPatientAsync(Guid patientId, CancellationToken ct = default)
    {
        // Verifica che il paziente esista
        var patientExists = await _patientRepository.ExistsAsync(patientId);
        if (!patientExists)
        {
            throw new InvalidOperationException(
                $"Patient with ID {patientId} does not exist");
        }

        var projects = await _therapyProjectRepository.GetByPatientIdAsync(patientId);
        var activeProjects = projects.Where(p => p.Status == nameof(TherapyProjectState.Active)).ToList();

        // REGOLA DI BUSINESS: max 1 progetto Active per paziente
        if (activeProjects.Count > 1)
        {
            throw new InvalidOperationException(
                $"Data integrity error: Patient {patientId} has {activeProjects.Count} Active projects, expected at most 1");
        }

        return activeProjects.FirstOrDefault();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TherapyProjectModel>> GetCompletedForPatientAsync(Guid patientId, CancellationToken ct = default)
    {
        // Verifica che il paziente esista
        var patientExists = await _patientRepository.ExistsAsync(patientId);
        if (!patientExists)
        {
            throw new InvalidOperationException(
                $"Patient with ID {patientId} does not exist");
        }

        var projects = await _therapyProjectRepository.GetByPatientIdAsync(patientId);
        return projects
            .Where(p => p.Status == nameof(TherapyProjectState.Completed))
            .ToList();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TherapyProjectModel>> GetByPatientIdAsync(Guid patientId, CancellationToken ct = default)
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
    public async Task<IEnumerable<TherapyProjectModel>> GetByEducatorIdAsync(Guid educatorId, CancellationToken ct = default)
    {
        return await _therapyProjectRepository.GetByEducatorIdAsync(educatorId);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TherapyProjectModel>> GetByStatusAsync(string status, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            throw new ArgumentException("Status cannot be null or empty", nameof(status));
        }

        // Valida che lo status sia uno dei valori consentiti
        var validStatuses = Enum.GetNames(typeof(TherapyProjectState));
        if (!validStatuses.Contains(status))
        {
            throw new ArgumentException(
                $"Invalid status '{status}'. Valid values are: {string.Join(", ", validStatuses)}",
                nameof(status));
        }

        return await _therapyProjectRepository.GetByStatusAsync(status);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TherapyProjectModel>> SearchAsync(string searchTerm, CancellationToken ct = default)
    {
        return await _therapyProjectRepository.SearchAsync(searchTerm);
    }

    /// <inheritdoc />
    public async Task<Guid> CreateProjectAsync(CreateProjectRequest request, CancellationToken ct = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        // === VALIDAZIONI ===

        // Titolo obbligatorio, min 3 caratteri
        if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Length < 3)
            throw new ArgumentException(
                "Il titolo del progetto è obbligatorio e deve avere almeno 3 caratteri",
                nameof(request.Title));

        // StartDate obbligatoria
        if (request.StartDate == default)
            throw new ArgumentException(
                "La data di inizio del progetto è obbligatoria",
                nameof(request.StartDate));

        // PlannedEndDate >= StartDate (se specificata)
        if (request.PlannedEndDate.HasValue && request.PlannedEndDate.Value < request.StartDate)
            throw new ArgumentException(
                "La data di fine prevista non può essere precedente alla data di inizio",
                nameof(request.PlannedEndDate));

        // Almeno 1 educatore
        if (request.EducatorIds == null || request.EducatorIds.Count == 0)
            throw new ArgumentException(
                "È necessario assegnare almeno un educatore professionale al progetto",
                nameof(request.EducatorIds));

        // Verifica che il paziente esista
        var patientExists = await _patientRepository.ExistsAsync(request.PatientId);
        if (!patientExists)
        {
            throw new InvalidOperationException(
                $"Cannot create TherapyProject: Patient with ID {request.PatientId} does not exist");
        }

        // Verifica che tutti gli educatori esistano
        foreach (var educatorId in request.EducatorIds)
        {
            var educatorExists = await _educatorRepository.ExistsAsync(educatorId);
            if (!educatorExists)
            {
                throw new InvalidOperationException(
                    $"Cannot create TherapyProject: Educator with ID {educatorId} does not exist");
            }
        }

        // REGOLA DI BUSINESS: Blocca se esiste già un progetto Active per il paziente
        if (request.InitialState == TherapyProjectState.Active)
        {
            var existingActiveProject = await GetActiveForPatientAsync(request.PatientId, ct);
            if (existingActiveProject != null)
            {
                throw new InvalidOperationException(
                    $"Cannot create Active project for Patient {request.PatientId}: " +
                    $"an Active project already exists (ID: {existingActiveProject.Id}). " +
                    "A patient can have only ONE Active project at a time.");
            }
        }

        // === CREAZIONE PROGETTO ===

        var project = new TherapyProjectModel
        {
            Id = Guid.NewGuid(),
            PatientId = request.PatientId,
            Title = request.Title,
            Description = request.Description,
            StartDate = request.StartDate,
            EndDate = request.PlannedEndDate,
            Status = request.InitialState.ToString(), // Mappa enum -> string
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };

        await _therapyProjectRepository.AddAsync(project);

        // Assegna educatori
        foreach (var educatorId in request.EducatorIds)
        {
            await _therapyProjectRepository.AssignEducatorAsync(project.Id, educatorId);
        }

        // === GENERAZIONE APPUNTAMENTI CANONICI ===

        if (request.GenerateCanonicalAppointments)
        {
            var appointments = GenerateCanonicalAppointments(project.Id, request.StartDate);
            await _scheduledVisitRepository.AddRangeAsync(appointments, ct);
        }

        return project.Id;
    }

    /// <summary>
    /// Genera i 4 appuntamenti canonici (INTAKE, INTERMEDIATE, FINAL, DISCHARGE)
    /// utilizzando la configurazione CanonicalAppointmentsConfiguration.
    /// </summary>
    private List<ScheduledVisitModel> GenerateCanonicalAppointments(Guid projectId, DateTime startDate)
    {
        var appointments = new List<ScheduledVisitModel>();
        DateTime? previousDate = null;

        foreach (var definition in _appointmentsConfig.Appointments)
        {
            var scheduledDate = definition.CalculateScheduledDate(startDate, previousDate);

            var appointment = new ScheduledVisitModel
            {
                Id = Guid.NewGuid(),
                TherapyProjectId = projectId,
                Type = definition.Type,
                Status = AppointmentStatus.Scheduled,
                ScheduledDate = scheduledDate,
                Notes = definition.Description,
                CreatedAt = DateTime.UtcNow,
                Version = 1
            };

            appointments.Add(appointment);
            previousDate = scheduledDate;
        }

        return appointments;
    }

    /// <inheritdoc />
    public async Task ChangeProjectStateAsync(Guid projectId, TherapyProjectState newState, CancellationToken ct = default)
    {
        var project = await _therapyProjectRepository.GetByIdAsync(projectId);
        if (project == null)
        {
            throw new InvalidOperationException(
                $"TherapyProject with ID {projectId} does not exist");
        }

        var currentState = Enum.Parse<TherapyProjectState>(project.Status);

        // VALIDAZIONE TRANSIZIONI
        // Completed e Deceased sono stati finali: non si può tornare indietro
        if (currentState == TherapyProjectState.Completed || currentState == TherapyProjectState.Deceased)
        {
            throw new InvalidOperationException(
                $"Cannot change state from {currentState} to {newState}: " +
                "Completed and Deceased are final states.");
        }

        // Se il nuovo stato è Active, verifica unicità
        if (newState == TherapyProjectState.Active)
        {
            var existingActiveProject = await GetActiveForPatientAsync(project.PatientId, ct);
            if (existingActiveProject != null && existingActiveProject.Id != projectId)
            {
                throw new InvalidOperationException(
                    $"Cannot change project {projectId} to Active: " +
                    $"Patient {project.PatientId} already has an Active project (ID: {existingActiveProject.Id})");
            }
        }

        // Applica cambio stato
        project.Status = newState.ToString();
        project.UpdatedAt = DateTime.UtcNow;

        await _therapyProjectRepository.UpdateAsync(project);
    }

    /// <inheritdoc />
    public async Task AddAsync(TherapyProjectModel therapyProject, CancellationToken ct = default)
    {
        if (therapyProject == null)
            throw new ArgumentNullException(nameof(therapyProject));

        // Validazione business
        await ValidateAsync(therapyProject, ct);

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
    public async Task UpdateAsync(TherapyProjectModel therapyProject, CancellationToken ct = default)
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
        await ValidateAsync(therapyProject, ct);

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
    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var deleted = await _therapyProjectRepository.DeleteAsync(id);
        if (!deleted)
        {
            throw new InvalidOperationException(
                $"TherapyProject with ID {id} does not exist");
        }
    }

    /// <inheritdoc />
    public async Task AssignEducatorAsync(Guid projectId, Guid educatorId, CancellationToken ct = default)
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
    public async Task RemoveEducatorAsync(Guid projectId, Guid educatorId, CancellationToken ct = default)
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
    public async Task<bool> ValidateAsync(TherapyProjectModel therapyProject, CancellationToken ct = default)
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

        return await Task.FromResult(true);
    }

    /// <inheritdoc />
    public async Task CompleteProjectAsync(Guid projectId, CancellationToken ct = default)
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
        if (project.Status == nameof(TherapyProjectState.Completed))
        {
            throw new InvalidOperationException(
                "Project is already completed");
        }

        project.Status = nameof(TherapyProjectState.Completed);
        project.UpdatedAt = DateTime.UtcNow;

        await _therapyProjectRepository.UpdateAsync(project);
    }

    /// <inheritdoc />
    public async Task PutOnHoldAsync(Guid projectId, CancellationToken ct = default)
    {
        var project = await _therapyProjectRepository.GetByIdAsync(projectId);
        if (project == null)
        {
            throw new InvalidOperationException(
                $"TherapyProject with ID {projectId} does not exist");
        }

        // Regola business: non può mettere in pausa un progetto completato o deceduto
        if (project.Status == nameof(TherapyProjectState.Completed) || 
            project.Status == nameof(TherapyProjectState.Deceased))
        {
            throw new InvalidOperationException(
                $"Cannot put project on hold: current status is {project.Status} (final state)");
        }

        project.Status = nameof(TherapyProjectState.Suspended);
        project.UpdatedAt = DateTime.UtcNow;

        await _therapyProjectRepository.UpdateAsync(project);
    }

    /// <inheritdoc />
    public async Task ResumeProjectAsync(Guid projectId, CancellationToken ct = default)
    {
        var project = await _therapyProjectRepository.GetByIdAsync(projectId);
        if (project == null)
        {
            throw new InvalidOperationException(
                $"TherapyProject with ID {projectId} does not exist");
        }

        // Regola business: si può riprendere solo un progetto sospeso
        if (project.Status != nameof(TherapyProjectState.Suspended))
        {
            throw new InvalidOperationException(
                $"Cannot resume project: current status is '{project.Status}', expected 'Suspended'");
        }

        // Verifica unicità progetto Active
        var existingActiveProject = await GetActiveForPatientAsync(project.PatientId, ct);
        if (existingActiveProject != null)
        {
            throw new InvalidOperationException(
                $"Cannot resume project {projectId}: " +
                $"Patient {project.PatientId} already has an Active project (ID: {existingActiveProject.Id})");
        }

        project.Status = nameof(TherapyProjectState.Active);
        project.UpdatedAt = DateTime.UtcNow;

        await _therapyProjectRepository.UpdateAsync(project);
    }
}
