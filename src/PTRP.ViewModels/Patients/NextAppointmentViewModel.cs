using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace PTRP.ViewModels.Patients
{
    /// <summary>
    /// ViewModel representing the next scheduled appointment for a patient.
    /// Displayed in the detail panel of PatientListView.
    /// </summary>
    public partial class NextAppointmentViewModel : ObservableObject
    {
        /// <summary>
        /// Unique identifier for the appointment.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Type of appointment (e.g., "Prima Apertura", "Visita Intermedia", "Chiusura").
        /// </summary>
        [ObservableProperty]
        private string _type = string.Empty;

        /// <summary>
        /// Display-friendly version of appointment type.
        /// </summary>
        [ObservableProperty]
        private string _typeDisplay = string.Empty;

        /// <summary>
        /// Scheduled date and time of the appointment.
        /// </summary>
        [ObservableProperty]
        private DateTime _scheduledDate;

        /// <summary>
        /// Location/venue of the appointment (optional).
        /// </summary>
        [ObservableProperty]
        private string _location = string.Empty;

        /// <summary>
        /// Formatted date string for display (e.g., "02/04/2025 - 14:30").
        /// </summary>
        public string FormattedDate => ScheduledDate.ToString("dd/MM/yyyy - HH:mm");

        /// <summary>
        /// Notification when ScheduledDate changes to update FormattedDate.
        /// </summary>
        partial void OnScheduledDateChanged(DateTime value) => OnPropertyChanged(nameof(FormattedDate));
    }
}