using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PTRP.Models.Enums;
using PTRP.Services.Interfaces;
using PTRP.Services.Models;
using System.Collections.ObjectModel;
using System.Windows;

namespace PTRP.ViewModels.Projects;

/// <summary>
/// ViewModel per la form di creazione/modifica di un Progetto Terapeutico.
/// Gestisce la logica di creazione progetto, validazione input e selezione educatori.
/// </summary>
public partial class ProjectFormViewModel : ObservableObject
{
    private readonly ITherapyProjectService _projectService;
    private readonly IEducatorService _educatorService;
    private readonly Guid _patientId;
    private readonly string _patientFullName;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private DateTime _startDate = DateTime.Today;

    [ObservableProperty]
    private DateTime? _plannedEndDate;

    [ObservableProperty]
    private TherapyProjectState _selectedProjectState = TherapyProjectState.Active;

    [ObservableProperty]
    private bool _generateCanonicalAppointments = true;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isSaving;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string? _successMessage;

    /// <summary>
    /// Lista degli educatori disponibili per la selezione
    /// </summary>
    public ObservableCollection<SelectableEducatorViewModel> AvailableEducators { get; } = new();

    /// <summary>
    /// Stati disponibili per il progetto
    /// </summary>
    public IReadOnlyList<TherapyProjectState> AvailableStates { get; } = new[]
    {
        TherapyProjectState.Active,
        TherapyProjectState.Suspended
    };

    /// <summary>
    /// Nome completo del paziente (per display nel titolo)
    /// </summary>
    public string PatientFullName => _patientFullName;

    /// <summary>
    /// Indica se la form è valida e può essere salvata
    /// </summary>
    public bool CanSave => !string.IsNullOrWhiteSpace(Title) 
                           && Title.Length >= 3 
                           && StartDate != default 
                           && AvailableEducators.Any(e => e.IsSelected)
                           && !IsSaving;

    public ProjectFormViewModel(
        ITherapyProjectService projectService,
        IEducatorService educatorService,
        Guid patientId,
        string patientFullName)
    {
        _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
        _educatorService = educatorService ?? throw new ArgumentNullException(nameof(educatorService));
        _patientId = patientId;
        _patientFullName = patientFullName;
    }

    /// <summary>
    /// Carica la lista degli educatori disponibili
    /// </summary>
    public async Task LoadEducatorsAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var educators = await _educatorService.GetAllAsync();
            
            AvailableEducators.Clear();
            foreach (var educator in educators.OrderBy(e => e.LastName).ThenBy(e => e.FirstName))
            {
                AvailableEducators.Add(new SelectableEducatorViewModel(
                    educator.Id,
                    $"{educator.LastName} {educator.FirstName}",
                    educator.Role ?? "Educatore Professionale"
                ));
            }

            // Subscribe to IsSelected changes to trigger CanSave update
            foreach (var educator in AvailableEducators)
            {
                educator.PropertyChanged += (_, _) => SaveCommand.NotifyCanExecuteChanged();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Errore nel caricamento degli educatori: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Comando per salvare il progetto
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        IsSaving = true;
        ErrorMessage = null;
        SuccessMessage = null;

        try
        {
            // Validazione input
            if (!ValidateInput(out var validationError))
            {
                ErrorMessage = validationError;
                return;
            }

            // Verifica se esiste già un progetto attivo per il paziente
            if (SelectedProjectState == TherapyProjectState.Active)
            {
                var existingActiveProject = await _projectService.GetActiveForPatientAsync(_patientId);
                if (existingActiveProject != null)
                {
                    ErrorMessage = "ATTENZIONE: Esiste già un progetto attivo per questo paziente. " +
                                   "Un paziente può avere un solo progetto attivo alla volta. " +
                                   "Completare o sospendere il progetto esistente prima di crearne uno nuovo.";
                    return;
                }
            }

            // Prepara la richiesta
            var request = new CreateProjectRequest(
                PatientId: _patientId,
                Title: Title.Trim(),
                Description: string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
                StartDate: StartDate,
                PlannedEndDate: PlannedEndDate,
                InitialState: SelectedProjectState,
                EducatorIds: AvailableEducators.Where(e => e.IsSelected).Select(e => e.EducatorId).ToList(),
                GenerateCanonicalAppointments: GenerateCanonicalAppointments
            );

            // Crea il progetto
            var projectId = await _projectService.CreateProjectAsync(request);

            SuccessMessage = "Progetto creato con successo!";
            
            // Notifica il completamento (gestito dalla view con evento o callback)
            OnProjectCreated?.Invoke(projectId);
        }
        catch (InvalidOperationException ex)
        {
            ErrorMessage = $"Operazione non valida: {ex.Message}";
        }
        catch (ArgumentException ex)
        {
            ErrorMessage = $"Dati non validi: {ex.Message}";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Errore nella creazione del progetto: {ex.Message}";
        }
        finally
        {
            IsSaving = false;
        }
    }

    /// <summary>
    /// Comando per annullare e chiudere la form
    /// </summary>
    [RelayCommand]
    private void Cancel()
    {
        OnCancelled?.Invoke();
    }

    /// <summary>
    /// Validazione dell'input del form
    /// </summary>
    private bool ValidateInput(out string? error)
    {
        error = null;

        if (string.IsNullOrWhiteSpace(Title))
        {
            error = "Il titolo del progetto è obbligatorio.";
            return false;
        }

        if (Title.Length < 3)
        {
            error = "Il titolo deve contenere almeno 3 caratteri.";
            return false;
        }

        if (StartDate == default)
        {
            error = "La data di inizio è obbligatoria.";
            return false;
        }

        if (PlannedEndDate.HasValue && PlannedEndDate.Value < StartDate)
        {
            error = "La data di fine prevista non può essere precedente alla data di inizio.";
            return false;
        }

        if (!AvailableEducators.Any(e => e.IsSelected))
        {
            error = "Selezionare almeno un educatore per il progetto.";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Aggiorna le proprietà dipendenti quando cambiano title o educatori
    /// </summary>
    partial void OnTitleChanged(string value)
    {
        SaveCommand.NotifyCanExecuteChanged();
    }

    partial void OnStartDateChanged(DateTime value)
    {
        SaveCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// Evento scatenato quando il progetto viene creato con successo
    /// </summary>
    public event Action<Guid>? OnProjectCreated;

    /// <summary>
    /// Evento scatenato quando l'utente annulla l'operazione
    /// </summary>
    public event Action? OnCancelled;
}
