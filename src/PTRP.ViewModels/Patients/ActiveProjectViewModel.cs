using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;

namespace PTRP.ViewModels.Patients
{
    /// <summary>
    /// ViewModel representing an active therapeutic project for a patient.
    /// Displayed in the detail panel of PatientListView.
    /// </summary>
    public partial class ActiveProjectViewModel : ObservableObject
    {
        /// <summary>
        /// Unique identifier for the project.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Project title/name.
        /// </summary>
        [ObservableProperty]
        private string _title = string.Empty;

        /// <summary>
        /// Project validity period (e.g., "2025-2027" or "Gennaio 2025 - Dicembre 2027").
        /// </summary>
        [ObservableProperty]
        private string _period = string.Empty;

        /// <summary>
        /// Start date of the project.
        /// </summary>
        [ObservableProperty]
        private DateTime _startDate;

        /// <summary>
        /// End date of the project.
        /// </summary>
        [ObservableProperty]
        private DateTime _endDate;

        /// <summary>
        /// List of educators assigned to this project.
        /// </summary>
        [ObservableProperty]
        private List<EducatorViewModel> _educators = new();
    }
}