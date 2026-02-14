using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace PTRP.ViewModels.Calendar;

/// <summary>
/// Rappresenta un singolo giorno nel calendario mensile
/// </summary>
public partial class DayViewModel : ObservableObject
{
    /// <summary>
    /// Data del giorno
    /// </summary>
    [ObservableProperty]
    private DateTime _date;

    /// <summary>
    /// Numero del giorno (1-31)
    /// </summary>
    public int DayNumber => Date.Day;

    /// <summary>
    /// Indica se il giorno appartiene al mese corrente visualizzato
    /// </summary>
    [ObservableProperty]
    private bool _isCurrentMonth;

    /// <summary>
    /// Indica se il giorno è oggi
    /// </summary>
    public bool IsToday => Date.Date == DateTime.Today;

    /// <summary>
    /// Indica se il giorno ha almeno un appuntamento
    /// </summary>
    public bool HasAppointments => Appointments.Count > 0;

    /// <summary>
    /// Numero totale di appuntamenti nel giorno
    /// </summary>
    public int AppointmentCount => Appointments.Count;

    /// <summary>
    /// Appuntamenti del giorno (per determinare badge colore)
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<AppointmentSummaryViewModel> _appointments = new();

    /// <summary>
    /// Indica se il giorno è selezionato
    /// </summary>
    [ObservableProperty]
    private bool _isSelected;

    /// <summary>
    /// Colore badge principale basato sullo stato del primo progetto Active
    /// (per visualizzazione nella cella calendario)
    /// </summary>
    public string BadgeColor
    {
        get
        {
            if (!HasAppointments) return "Transparent";

            // Trova primo appuntamento con progetto Active
            var activeAppt = Appointments.FirstOrDefault(a => a.ProjectState == "Active");
            if (activeAppt != null) return "#28A745"; // Verde

            var suspendedAppt = Appointments.FirstOrDefault(a => a.ProjectState == "Suspended");
            if (suspendedAppt != null) return "#FFC107"; // Giallo

            var completedAppt = Appointments.FirstOrDefault(a => a.ProjectState == "Completed");
            if (completedAppt != null) return "#6C757D"; // Grigio

            var deceasedAppt = Appointments.FirstOrDefault(a => a.ProjectState == "Deceased");
            if (deceasedAppt != null) return "#000000"; // Nero

            return "Transparent";
        }
    }

    partial void OnAppointmentsChanged(ObservableCollection<AppointmentSummaryViewModel> value)
    {
        OnPropertyChanged(nameof(HasAppointments));
        OnPropertyChanged(nameof(AppointmentCount));
        OnPropertyChanged(nameof(BadgeColor));
    }
}
