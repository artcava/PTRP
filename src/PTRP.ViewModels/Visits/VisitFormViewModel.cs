using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace PTRP.ViewModels.Visits;

/// <summary>
/// ViewModel per VisitFormView - Registrazione visita effettiva
/// </summary>
public partial class VisitFormViewModel : ViewModelBase
{
    public override string DisplayName => "Registrazione Visita";

    #region Read-Only Info (da appuntamento)

    /// <summary>
    /// ID dell'appuntamento programmato (ScheduledVisit)
    /// </summary>
    [ObservableProperty]
    private Guid _scheduledVisitId;

    /// <summary>
    /// Nome completo paziente
    /// </summary>
    [ObservableProperty]
    private string _patientName = string.Empty;

    /// <summary>
    /// Tipo appuntamento display ("Prima Apertura", "Verifica Intermedia", etc.)
    /// </summary>
    [ObservableProperty]
    private string _appointmentTypeDisplay = string.Empty;

    /// <summary>
    /// Data programmata appuntamento
    /// </summary>
    [ObservableProperty]
    private DateTime _scheduledDate;

    /// <summary>
    /// Data programmata formattata
    /// </summary>
    public string ScheduledDateDisplay => ScheduledDate.ToString("dd/MM/yyyy HH:mm");

    #endregion

    #region Editable Fields

    /// <summary>
    /// Data effettiva visita (non può essere futura)
    /// </summary>
    [ObservableProperty]
    [Required(ErrorMessage = "La data effettiva è obbligatoria")]
    private DateTime _actualDate = DateTime.Today;

    /// <summary>
    /// Ora inizio visita
    /// </summary>
    [ObservableProperty]
    [Required(ErrorMessage = "L'ora di inizio è obbligatoria")]
    private TimeSpan _startTime = new TimeSpan(9, 0, 0);

    /// <summary>
    /// Ora fine visita (deve essere > ora inizio)
    /// </summary>
    [ObservableProperty]
    [Required(ErrorMessage = "L'ora di fine è obbligatoria")]
    private TimeSpan _endTime = new TimeSpan(10, 0, 0);

    /// <summary>
    /// Note cliniche (obbligatorio)
    /// </summary>
    [ObservableProperty]
    [Required(ErrorMessage = "Le note cliniche sono obbligatorie")]
    [MinLength(10, ErrorMessage = "Le note devono contenere almeno 10 caratteri")]
    private string _clinicalNotes = string.Empty;

    /// <summary>
    /// Esiti e obiettivi (opzionale)
    /// </summary>
    [ObservableProperty]
    private string _outcomes = string.Empty;

    /// <summary>
    /// Stato presenza paziente
    /// </summary>
    [ObservableProperty]
    [Required(ErrorMessage = "Selezionare lo stato di presenza del paziente")]
    private string _selectedPresenceStatus = "PresentCollaborative";

    /// <summary>
    /// Opzioni presenza paziente
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<PresenceStatusItem> _presenceStatusOptions = new()
    {
        new PresenceStatusItem { Value = "PresentCollaborative", Display = "Presente e Collaborativo" },
        new PresenceStatusItem { Value = "PresentNonCollaborative", Display = "Presente ma Non Collaborativo" },
        new PresenceStatusItem { Value = "AbsentJustified", Display = "Assente Giustificato" },
        new PresenceStatusItem { Value = "AbsentNotJustified", Display = "Assente Non Giustificato" }
    };

    /// <summary>
    /// Lista operatori disponibili (checkbox)
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<OperatorCheckboxViewModel> _availableOperators = new();

    /// <summary>
    /// Operatori selezionati (presenti durante visita)
    /// </summary>
    public IEnumerable<OperatorCheckboxViewModel> SelectedOperators => 
        AvailableOperators.Where(o => o.IsSelected);

    /// <summary>
    /// Numero operatori selezionati
    /// </summary>
    public int SelectedOperatorsCount => SelectedOperators.Count();

    #endregion

    #region Validation

    /// <summary>
    /// Errori di validazione
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<string> _validationErrors = new();

    /// <summary>
    /// Indica se ci sono errori di validazione
    /// </summary>
    public bool HasValidationErrors => ValidationErrors.Count > 0;

    /// <summary>
    /// Valida tutti i campi del form
    /// </summary>
    private bool ValidateForm()
    {
        ValidationErrors.Clear();

        // Data effettiva non futura
        if (ActualDate.Date > DateTime.Today)
        {
            ValidationErrors.Add("La data effettiva non può essere futura");
        }

        // Ora fine > ora inizio
        if (EndTime <= StartTime)
        {
            ValidationErrors.Add("L'ora di fine deve essere successiva all'ora di inizio");
        }

        // Almeno un operatore selezionato
        if (SelectedOperatorsCount == 0)
        {
            ValidationErrors.Add("Selezionare almeno un operatore presente");
        }

        // Note cliniche obbligatorie e lunghezza minima
        if (string.IsNullOrWhiteSpace(ClinicalNotes))
        {
            ValidationErrors.Add("Le note cliniche sono obbligatorie");
        }
        else if (ClinicalNotes.Length < 10)
        {
            ValidationErrors.Add("Le note cliniche devono contenere almeno 10 caratteri");
        }

        // Presenza paziente selezionata
        if (string.IsNullOrWhiteSpace(SelectedPresenceStatus))
        {
            ValidationErrors.Add("Selezionare lo stato di presenza del paziente");
        }

        OnPropertyChanged(nameof(HasValidationErrors));
        return ValidationErrors.Count == 0;
    }

    #endregion

    #region Commands

    /// <summary>
    /// Salva visita
    /// </summary>
    [RelayCommand]
    private async Task SaveVisitAsync()
    {
        if (!ValidateForm())
        {
            // Mostra errori (già popolati in ValidationErrors)
            return;
        }

        try
        {
            // TODO: Chiamare IActualVisitService.RegisterVisitAsync(...)
            // Creare ActualVisitModel con dati dal form
            // Collegare a ScheduledVisitId
            // Salvare operatori presenti

            await Task.Delay(500); // Simula save

            // Chiudi form e torna al calendario
            // TODO: NavigationService.GoBack() o chiudi dialog
        }
        catch (Exception ex)
        {
            ValidationErrors.Add($"Errore durante il salvataggio: {ex.Message}");
        }
    }

    /// <summary>
    /// Annulla registrazione e torna indietro
    /// </summary>
    [RelayCommand]
    private void Cancel()
    {
        // TODO: NavigationService.GoBack() o chiudi dialog
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Inizializza form con dati da appuntamento
    /// </summary>
    public void InitializeFromAppointment(Guid scheduledVisitId, string patientName, 
        string appointmentType, DateTime scheduledDate, List<Guid> assignedEducatorIds)
    {
        ScheduledVisitId = scheduledVisitId;
        PatientName = patientName;
        AppointmentTypeDisplay = appointmentType;
        ScheduledDate = scheduledDate;

        // Imposta data effettiva = data programmata (modificabile)
        ActualDate = scheduledDate.Date;
        StartTime = scheduledDate.TimeOfDay;
        EndTime = scheduledDate.TimeOfDay.Add(new TimeSpan(1, 0, 0)); // +1 ora default

        // Carica operatori assegnati al progetto
        LoadAvailableOperators(assignedEducatorIds);
    }

    /// <summary>
    /// Carica lista operatori disponibili
    /// </summary>
    private void LoadAvailableOperators(List<Guid> assignedEducatorIds)
    {
        AvailableOperators.Clear();

        // TODO: Caricare da service reale
        // MOCK per testing
        var currentUserId = Guid.NewGuid(); // TODO: Ottenere da context

        AvailableOperators.Add(new OperatorCheckboxViewModel
        {
            EducatorId = currentUserId,
            FullName = "Mario Bianchi",
            IsCurrentUser = true,
            IsSelected = true // Pre-selezionato
        });

        AvailableOperators.Add(new OperatorCheckboxViewModel
        {
            EducatorId = Guid.NewGuid(),
            FullName = "Laura Verdi",
            IsCurrentUser = false,
            IsSelected = false
        });

        AvailableOperators.Add(new OperatorCheckboxViewModel
        {
            EducatorId = Guid.NewGuid(),
            FullName = "Giovanni Rossi",
            IsCurrentUser = false,
            IsSelected = false
        });
    }

    #endregion

    partial void OnScheduledDateChanged(DateTime value)
    {
        OnPropertyChanged(nameof(ScheduledDateDisplay));
    }

    partial void OnAvailableOperatorsChanged(ObservableCollection<OperatorCheckboxViewModel> value)
    {
        // Subscribe to changes in IsSelected
        foreach (var op in value)
        {
            op.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(OperatorCheckboxViewModel.IsSelected))
                {
                    OnPropertyChanged(nameof(SelectedOperators));
                    OnPropertyChanged(nameof(SelectedOperatorsCount));
                }
            };
        }
    }

    partial void OnValidationErrorsChanged(ObservableCollection<string> value)
    {
        OnPropertyChanged(nameof(HasValidationErrors));
    }
}

/// <summary>
/// Helper per item presenza paziente
/// </summary>
public class PresenceStatusItem
{
    public string Value { get; set; } = string.Empty;
    public string Display { get; set; } = string.Empty;
}
