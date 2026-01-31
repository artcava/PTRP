using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;

namespace PTRP.ViewModels.Patients
{
    /// <summary>
    /// ViewModel representing a patient with associated project and appointment data.
    /// Used in PatientListView for Master-Detail display.
    /// </summary>
    public partial class PatientViewModel : ObservableObject
    {
        /// <summary>
        /// Unique identifier for the patient.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Patient's first name.
        /// </summary>
        [ObservableProperty]
        private string _firstName = string.Empty;

        /// <summary>
        /// Patient's last name.
        /// </summary>
        [ObservableProperty]
        private string _lastName = string.Empty;

        /// <summary>
        /// Full name combining first and last name.
        /// </summary>
        public string FullName => $"{FirstName} {LastName}";

        /// <summary>
        /// Date when patient record was created.
        /// </summary>
        [ObservableProperty]
        private DateTime _createdAt;

        /// <summary>
        /// Current project state (Active, Suspended, Completed, Deceased).
        /// </summary>
        [ObservableProperty]
        private string _projectState = string.Empty;

        /// <summary>
        /// Display-friendly version of project state.
        /// </summary>
        [ObservableProperty]
        private string _projectStateDisplay = string.Empty;

        /// <summary>
        /// Comma-separated list of assigned educator names for display in DataGrid.
        /// </summary>
        [ObservableProperty]
        private string _assignedEducatorsDisplay = string.Empty;

        /// <summary>
        /// Active project details (null if no active project).
        /// </summary>
        [ObservableProperty]
        private ActiveProjectViewModel? _activeProject;

        /// <summary>
        /// Next scheduled appointment details (null if none scheduled).
        /// </summary>
        [ObservableProperty]
        private NextAppointmentViewModel? _nextAppointment;

        /// <summary>
        /// Notification when first/last name changes to update FullName.
        /// </summary>
        partial void OnFirstNameChanged(string value) => OnPropertyChanged(nameof(FullName));
        
        /// <summary>
        /// Notification when first/last name changes to update FullName.
        /// </summary>
        partial void OnLastNameChanged(string value) => OnPropertyChanged(nameof(FullName));
    }
}