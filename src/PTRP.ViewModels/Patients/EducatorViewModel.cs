using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace PTRP.ViewModels.Patients
{
    /// <summary>
    /// ViewModel representing an educator (professional educator).
    /// Used in ActiveProjectViewModel to display assigned team members.
    /// </summary>
    public partial class EducatorViewModel : ObservableObject
    {
        /// <summary>
        /// Unique identifier for the educator.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Educator's full name.
        /// </summary>
        [ObservableProperty]
        private string _name = string.Empty;

        /// <summary>
        /// Optional initials for compact display (e.g., "MB" for Mario Bianchi).
        /// </summary>
        [ObservableProperty]
        private string _initials = string.Empty;

        /// <summary>
        /// Optional role/title (e.g., "Coordinatore", "Educatore").
        /// </summary>
        [ObservableProperty]
        private string _role = string.Empty;
    }
}