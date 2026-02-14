using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace PTRP.ViewModels.Visits;

/// <summary>
/// ViewModel per VisitFormView - Registrazione visita effettiva
/// Eredita da ObservableValidator per supportare data annotations validation
/// </summary>
public partial class VisitFormViewModel : ObservableValidator
{
    // DisplayName implementato come property normale (non da ViewModelBase)
    public string DisplayName => "Registrazione Visita";

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

    #region Editable Fields with Validation

    /// <summary>
    /// Data effettiva visita (non può essere futura)
    /// </summary>
    [ObservableProperty]
    [Required(ErrorMessage = "La data effettiva è obbligatoria")]
    [CustomValidation(typeof(VisitFormViewModel), nameof(ValidateActualDate))]
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
    [CustomValidation(typeof(VisitFormViewModel), nameof(ValidateEndTime))]
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

    #region Custom Validation Methods

    /// <summary>
    /// Valida che la data effettiva non sia futura
    /// </summary>
    public static ValidationResult? ValidateActualDate(DateTime actualDate, ValidationContext context)
    {
        if (actualDate.Date > DateTime.Today)
        {
            return new ValidationResult("La data effettiva non può essere futura");
        }
        return ValidationResult.Success;
    }

    /// <summary>
    /// Valida che EndTime sia successivo a StartTime
    /// </summary>
    public static ValidationResult? ValidateEndTime(TimeSpan endTime, ValidationContext context)
    {
        var instance = (VisitFormViewModel)context.ObjectInstance;
        if (endTime <= instance.StartTime)
        {
            return new ValidationResult("L'ora di fine deve essere successiva all'ora di inizio");
        }
        return ValidationResult.Success;
    }

    #endregion

    #region Additional Validation

    /// <summary>
    /// Valida che almeno un operatore sia selezionato
    /// </summary>
    private bool ValidateOperators()
    {
        return SelectedOperatorsCount > 0;
    }

    /// <summary>
    /// Metodo pubblico per eseguire validazione completa (utile per testing)
    /// </summary>
    public void Validate()
    {
        ValidateAllProperties();
    }

    /// <summary>
    /// Valida l'intero form inclusi requisiti custom
    /// </summary>
    private bool ValidateForm()
    {
        // Valida properties con DataAnnotations
        ValidateAllProperties();

        // Validazione custom: almeno un operatore
        if (!ValidateOperators())
        {
            // Non possiamo aggiungere errori custom direttamente alle properties
            // Usiamo una property separata per gli errori custom
            return false;
        }

        return !HasErrors;
    }

    #endregion

    #region Commands

    /// <summary>
    /// Salva visita
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSaveVisit))]
    private async Task SaveVisitAsync()
    {
        if (!ValidateForm())
        {
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
        catch (Exception)
        {
            // TODO: Gestire errore
        }
    }

    /// <summary>
    /// Determina se il comando Save può essere eseguito
    /// </summary>
    private bool CanSaveVisit()
    {
        // Quick check senza validazione completa
        return !string.IsNullOrWhiteSpace(ClinicalNotes) 
            && ClinicalNotes.Length >= 10
            && SelectedOperatorsCount > 0
            && !string.IsNullOrWhiteSpace(SelectedPresenceStatus);
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

    #region Property Changed Handlers

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
                    SaveVisitCommand.NotifyCanExecuteChanged();
                }
            };
        }
    }

    partial void OnClinicalNotesChanged(string value)
    {
        SaveVisitCommand.NotifyCanExecuteChanged();
    }

    partial void OnSelectedPresenceStatusChanged(string value)
    {
        SaveVisitCommand.NotifyCanExecuteChanged();
    }

    partial void OnStartTimeChanged(TimeSpan value)
    {
        // Rivalidare EndTime quando cambia StartTime
        ValidateProperty(EndTime, nameof(EndTime));
    }

    #endregion
}

/// <summary>
/// Helper per item presenza paziente
/// </summary>
public class PresenceStatusItem
{
    public string Value { get; set; } = string.Empty;
    public string Display { get; set; } = string.Empty;
}
