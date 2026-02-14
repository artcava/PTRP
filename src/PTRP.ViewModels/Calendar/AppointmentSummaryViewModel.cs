using CommunityToolkit.Mvvm.ComponentModel;

namespace PTRP.ViewModels.Calendar;

/// <summary>
/// Rappresenta un appuntamento nella lista giornaliera del calendario
/// </summary>
public partial class AppointmentSummaryViewModel : ObservableObject
{
    /// <summary>
    /// ID dell'appuntamento programmato
    /// </summary>
    [ObservableProperty]
    private Guid _scheduledVisitId;

    /// <summary>
    /// Tipo appuntamento (INTAKE, INTERMEDIATE, FINAL, DISCHARGE)
    /// </summary>
    [ObservableProperty]
    private string _visitType = string.Empty;

    /// <summary>
    /// Display name del tipo appuntamento ("Prima Apertura", "Verifica Intermedia", etc.)
    /// </summary>
    [ObservableProperty]
    private string _visitTypeDisplay = string.Empty;

    /// <summary>
    /// Nome completo paziente
    /// </summary>
    [ObservableProperty]
    private string _patientName = string.Empty;

    /// <summary>
    /// ID paziente (per navigazione)
    /// </summary>
    [ObservableProperty]
    private Guid _patientId;

    /// <summary>
    /// Titolo progetto terapeutico
    /// </summary>
    [ObservableProperty]
    private string _projectTitle = string.Empty;

    /// <summary>
    /// ID progetto terapeutico (per navigazione)
    /// </summary>
    [ObservableProperty]
    private Guid _projectId;

    /// <summary>
    /// Stato progetto (Active, Suspended, Completed, Deceased)
    /// </summary>
    [ObservableProperty]
    private string _projectState = string.Empty;

    /// <summary>
    /// Display name stato progetto ("Attivo", "Sospeso", etc.)
    /// </summary>
    [ObservableProperty]
    private string _projectStateDisplay = string.Empty;

    /// <summary>
    /// Lista nomi educatori assegnati (es: "Bianchi, Verdi")
    /// </summary>
    [ObservableProperty]
    private string _educatorNames = string.Empty;

    /// <summary>
    /// Data e ora programmata appuntamento
    /// </summary>
    [ObservableProperty]
    private DateTime _scheduledDateTime;

    /// <summary>
    /// Data e ora formattata ("14:30")
    /// </summary>
    public string TimeDisplay => ScheduledDateTime.ToString("HH:mm");

    /// <summary>
    /// Stato appuntamento (Scheduled, Completed, Missed, Rescheduled)
    /// </summary>
    [ObservableProperty]
    private string _appointmentStatus = "Scheduled";

    /// <summary>
    /// Indica se l'appuntamento è già stato completato (ha visita registrata)
    /// </summary>
    public bool IsCompleted => AppointmentStatus == "Completed";

    /// <summary>
    /// Indica se l'appuntamento è stato segnato come mancato
    /// </summary>
    public bool IsMissed => AppointmentStatus == "Missed";

    /// <summary>
    /// Indica se l'appuntamento può essere gestito (registra visita, riprogramma, segna mancato)
    /// </summary>
    public bool CanManage => !IsCompleted && !IsMissed;

    /// <summary>
    /// Colore badge basato su stato progetto
    /// </summary>
    public string ProjectStateBadgeColor => ProjectState switch
    {
        "Active" => "#28A745",     // Verde
        "Suspended" => "#FFC107",  // Giallo
        "Completed" => "#6C757D",  // Grigio
        "Deceased" => "#000000",   // Nero
        _ => "#6C757D"
    };

    partial void OnScheduledDateTimeChanged(DateTime value)
    {
        OnPropertyChanged(nameof(TimeDisplay));
    }

    partial void OnAppointmentStatusChanged(string value)
    {
        OnPropertyChanged(nameof(IsCompleted));
        OnPropertyChanged(nameof(IsMissed));
        OnPropertyChanged(nameof(CanManage));
    }
}
