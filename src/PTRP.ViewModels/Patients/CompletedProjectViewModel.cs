using System;

namespace PTRP.ViewModels.Patients
{
    /// <summary>
    /// ViewModel representing a completed project in patient history.
    /// Used to display summary information about past projects.
    /// </summary>
    public class CompletedProjectViewModel
    {
        /// <summary>
        /// Unique identifier for the project.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Project title.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Final state of the project (Completed, Deceased, etc.).
        /// </summary>
        public string FinalState { get; set; } = string.Empty;

        /// <summary>
        /// Display-friendly version of final state.
        /// </summary>
        public string FinalStateDisplay { get; set; } = string.Empty;

        /// <summary>
        /// Project start date.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Project end date (when it was closed).
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Formatted period string for display (e.g., "Gen 2025 - Dic 2025").
        /// </summary>
        public string Period { get; set; } = string.Empty;

        /// <summary>
        /// Comma-separated list of educators who worked on this project.
        /// </summary>
        public string EducatorsDisplay { get; set; } = string.Empty;

        /// <summary>
        /// Total number of appointments/visits conducted during the project.
        /// </summary>
        public int TotalVisits { get; set; }
    }
}
