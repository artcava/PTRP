using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace PTRP.ViewModels.Calendar;

/// <summary>
/// ViewModel per CalendarView - Gestione calendario mensile appuntamenti
/// </summary>
public partial class CalendarViewModel : ViewModelBase
{
    public override string DisplayName => "Calendario";

    #region Properties

    /// <summary>
    /// Mese e anno correntemente visualizzati
    /// </summary>
    [ObservableProperty]
    private DateTime _currentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

    /// <summary>
    /// Display del mese corrente (es: "Febbraio 2026")
    /// </summary>
    public string CurrentMonthDisplay => CurrentMonth.ToString("MMMM yyyy");

    /// <summary>
    /// Giorni del mese visualizzati nella griglia (include giorni mese precedente/successivo per completare settimane)
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<DayViewModel> _days = new();

    /// <summary>
    /// Data selezionata dall'utente
    /// </summary>
    [ObservableProperty]
    private DateTime? _selectedDate;

    /// <summary>
    /// Appuntamenti del giorno selezionato
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<AppointmentSummaryViewModel> _selectedDayAppointments = new();

    /// <summary>
    /// Indica se ci sono appuntamenti nel giorno selezionato
    /// </summary>
    public bool HasSelectedDayAppointments => SelectedDayAppointments.Count > 0;

    /// <summary>
    /// Messaggio quando nessun appuntamento nel giorno selezionato
    /// </summary>
    public string NoAppointmentsMessage => SelectedDate.HasValue
        ? $"Nessun appuntamento il {SelectedDate.Value:dd/MM/yyyy}"
        : "Seleziona un giorno per visualizzare gli appuntamenti";

    #endregion

    #region Filters

    /// <summary>
    /// Filtro per tipo appuntamento - INTAKE
    /// </summary>
    [ObservableProperty]
    private bool _filterIntake = true;

    /// <summary>
    /// Filtro per tipo appuntamento - INTERMEDIATE e FINAL
    /// </summary>
    [ObservableProperty]
    private bool _filterVerifiche = true;

    /// <summary>
    /// Filtro per tipo appuntamento - DISCHARGE
    /// </summary>
    [ObservableProperty]
    private bool _filterDimissioni = true;

    /// <summary>
    /// Filtro per educatore (Guid.Empty = Tutti)
    /// </summary>
    [ObservableProperty]
    private Guid _selectedEducatorFilter = Guid.Empty;

    /// <summary>
    /// Lista educatori disponibili per filtro
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<EducatorFilterItem> _educatorFilterOptions = new()
    {
        new EducatorFilterItem { Id = Guid.Empty, Name = "Tutti" }
    };

    /// <summary>
    /// Filtro per stato progetto ("All", "Active", "Suspended", "Completed", "Deceased")
    /// </summary>
    [ObservableProperty]
    private string _selectedProjectStateFilter = "All";

    /// <summary>
    /// Opzioni filtro stato progetto
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<string> _projectStateFilterOptions = new()
    {
        "Tutti",
        "Active",
        "Suspended",
        "Completed",
        "Deceased"
    };

    #endregion

    #region Loading

    /// <summary>
    /// Indica se i dati sono in caricamento
    /// </summary>
    [ObservableProperty]
    private bool _isLoading;

    #endregion

    #region Commands

    /// <summary>
    /// Naviga al mese precedente
    /// </summary>
    [RelayCommand]
    private async Task GoToPreviousMonthAsync()
    {
        CurrentMonth = CurrentMonth.AddMonths(-1);
        await LoadMonthDataAsync();
    }

    /// <summary>
    /// Naviga al mese successivo
    /// </summary>
    [RelayCommand]
    private async Task GoToNextMonthAsync()
    {
        CurrentMonth = CurrentMonth.AddMonths(1);
        await LoadMonthDataAsync();
    }

    /// <summary>
    /// Naviga al mese corrente (oggi)
    /// </summary>
    [RelayCommand]
    private async Task GoToTodayAsync()
    {
        CurrentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        SelectedDate = DateTime.Today;
        await LoadMonthDataAsync();
    }

    /// <summary>
    /// Seleziona un giorno e carica i suoi appuntamenti
    /// </summary>
    [RelayCommand]
    private async Task SelectDayAsync(DayViewModel day)
    {
        // Deseleziona giorno precedente
        var previousSelected = Days.FirstOrDefault(d => d.IsSelected);
        if (previousSelected != null)
            previousSelected.IsSelected = false;

        // Seleziona nuovo giorno
        day.IsSelected = true;
        SelectedDate = day.Date;

        // Carica appuntamenti del giorno
        await LoadDayAppointmentsAsync(day.Date);
    }

    /// <summary>
    /// Apre form registrazione visita per appuntamento
    /// </summary>
    [RelayCommand]
    private void RegisterVisit(AppointmentSummaryViewModel appointment)
    {
        // TODO: Aprire VisitFormView con dati appuntamento precompilati
        // Implementazione con NavigationService o Dialog Service
    }

    /// <summary>
    /// Riprogramma appuntamento
    /// </summary>
    [RelayCommand]
    private async Task RescheduleAppointmentAsync(AppointmentSummaryViewModel appointment)
    {
        // TODO: Implementare dialog riprogrammazione
        await Task.CompletedTask;
    }

    /// <summary>
    /// Segna appuntamento come mancato
    /// </summary>
    [RelayCommand]
    private async Task MarkAsMissedAsync(AppointmentSummaryViewModel appointment)
    {
        // TODO: Chiamare service per marcare come Missed
        appointment.AppointmentStatus = "Missed";
        await Task.CompletedTask;
    }

    /// <summary>
    /// Riapplica filtri e ricarica dati
    /// </summary>
    [RelayCommand]
    private async Task ApplyFiltersAsync()
    {
        await LoadMonthDataAsync();
        if (SelectedDate.HasValue)
            await LoadDayAppointmentsAsync(SelectedDate.Value);
    }

    #endregion

    #region Data Loading

    /// <summary>
    /// Carica dati del mese corrente
    /// </summary>
    public async Task LoadMonthDataAsync()
    {
        IsLoading = true;
        try
        {            // TODO: Sostituire con chiamata a IScheduledVisitService
            await Task.Delay(300); // Simula API call

            // Genera giorni del mese (con padding per settimane complete)
            GenerateCalendarDays();

            // Carica appuntamenti del mese e assegnali ai giorni
            await LoadMonthAppointmentsAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Genera griglia giorni del calendario
    /// </summary>
    private void GenerateCalendarDays()
    {
        Days.Clear();

        var firstDayOfMonth = CurrentMonth;
        var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

        // Determina primo giorno della prima settimana (Lunedì)
        var firstDayOfWeek = firstDayOfMonth;
        while (firstDayOfWeek.DayOfWeek != DayOfWeek.Monday)
            firstDayOfWeek = firstDayOfWeek.AddDays(-1);

        // Determina ultimo giorno dell'ultima settimana (Domenica)
        var lastDayOfWeek = lastDayOfMonth;
        while (lastDayOfWeek.DayOfWeek != DayOfWeek.Sunday)
            lastDayOfWeek = lastDayOfWeek.AddDays(1);

        // Genera tutti i giorni
        var currentDay = firstDayOfWeek;
        while (currentDay <= lastDayOfWeek)
        {
            var day = new DayViewModel
            {
                Date = currentDay,
                IsCurrentMonth = currentDay.Month == CurrentMonth.Month,
                IsSelected = SelectedDate.HasValue && currentDay.Date == SelectedDate.Value.Date
            };

            Days.Add(day);
            currentDay = currentDay.AddDays(1);
        }
    }

    /// <summary>
    /// Carica appuntamenti del mese e li assegna ai giorni
    /// </summary>
    private async Task LoadMonthAppointmentsAsync()
    {
        // TODO: Sostituire con chiamata reale a service
        await Task.Delay(100);

        // MOCK DATA per testing
        var sampleAppointments = GenerateSampleAppointments();

        // Assegna appuntamenti ai giorni
        foreach (var appointment in sampleAppointments)
        {
            var day = Days.FirstOrDefault(d => d.Date.Date == appointment.ScheduledDateTime.Date);
            if (day != null)
            {
                day.Appointments.Add(appointment);
            }
        }
    }

    /// <summary>
    /// Carica appuntamenti di un giorno specifico
    /// </summary>
    private async Task LoadDayAppointmentsAsync(DateTime date)
    {
        IsLoading = true;
        try
        {
            await Task.Delay(100);

            var day = Days.FirstOrDefault(d => d.Date.Date == date.Date);
            if (day != null)
            {
                SelectedDayAppointments = new ObservableCollection<AppointmentSummaryViewModel>(day.Appointments);
            }
            else
            {
                SelectedDayAppointments.Clear();
            }

            OnPropertyChanged(nameof(HasSelectedDayAppointments));
            OnPropertyChanged(nameof(NoAppointmentsMessage));
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Genera dati di esempio per testing
    /// </summary>
    private List<AppointmentSummaryViewModel> GenerateSampleAppointments()
    {
        var appointments = new List<AppointmentSummaryViewModel>();

        // Appuntamento 1: Prima Apertura - Active
        appointments.Add(new AppointmentSummaryViewModel
        {
            ScheduledVisitId = Guid.NewGuid(),
            VisitType = "INTAKE",
            VisitTypeDisplay = "Prima Apertura",
            PatientName = "Mario Rossi",
            PatientId = Guid.NewGuid(),
            ProjectTitle = "PTRP 2025-2027",
            ProjectId = Guid.NewGuid(),
            ProjectState = "Active",
            ProjectStateDisplay = "Attivo",
            EducatorNames = "Bianchi, Verdi",
            ScheduledDateTime = new DateTime(CurrentMonth.Year, CurrentMonth.Month, 3, 14, 30, 0),
            AppointmentStatus = "Scheduled"
        });

        // Appuntamento 2: Verifica Intermedia - Suspended
        appointments.Add(new AppointmentSummaryViewModel
        {
            ScheduledVisitId = Guid.NewGuid(),
            VisitType = "INTERMEDIATE",
            VisitTypeDisplay = "Verifica Intermedia",
            PatientName = "Luca Bianchi",
            PatientId = Guid.NewGuid(),
            ProjectTitle = "PTRP 2024-2026",
            ProjectId = Guid.NewGuid(),
            ProjectState = "Suspended",
            ProjectStateDisplay = "Sospeso",
            EducatorNames = "Rossi",
            ScheduledDateTime = new DateTime(CurrentMonth.Year, CurrentMonth.Month, 17, 10, 0, 0),
            AppointmentStatus = "Scheduled"
        });

        // Appuntamento 3: Dimissioni - Completed
        appointments.Add(new AppointmentSummaryViewModel
        {
            ScheduledVisitId = Guid.NewGuid(),
            VisitType = "DISCHARGE",
            VisitTypeDisplay = "Dimissioni",
            PatientName = "Anna Verdi",
            PatientId = Guid.NewGuid(),
            ProjectTitle = "PTRP 2023-2025",
            ProjectId = Guid.NewGuid(),
            ProjectState = "Completed",
            ProjectStateDisplay = "Completato",
            EducatorNames = "Verdi, Neri",
            ScheduledDateTime = new DateTime(CurrentMonth.Year, CurrentMonth.Month, 23, 15, 30, 0),
            AppointmentStatus = "Completed"
        });

        return appointments;
    }

    #endregion

    partial void OnCurrentMonthChanged(DateTime value)
    {
        OnPropertyChanged(nameof(CurrentMonthDisplay));
    }

    partial void OnSelectedDateChanged(DateTime? value)
    {
        OnPropertyChanged(nameof(NoAppointmentsMessage));
    }

    partial void OnSelectedDayAppointmentsChanged(ObservableCollection<AppointmentSummaryViewModel> value)
    {
        OnPropertyChanged(nameof(HasSelectedDayAppointments));
        OnPropertyChanged(nameof(NoAppointmentsMessage));
    }
}

/// <summary>
/// Helper per item filtro educatore
/// </summary>
public class EducatorFilterItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
