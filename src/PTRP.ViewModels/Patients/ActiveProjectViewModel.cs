using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PTRP.ViewModels.Patients
{
    /// <summary>
    /// ViewModel per rappresentare un progetto terapeutico attivo nella UI.
    /// Usato in PatientListView per visualizzare dettagli progetto nel pannello laterale.
    /// </summary>
    public partial class ActiveProjectViewModel : ObservableObject
    {
        [ObservableProperty]
        private Guid _id;

        [ObservableProperty]
        private string _title = string.Empty;

        [ObservableProperty]
        private string _period = string.Empty;

        [ObservableProperty]
        private DateTime _startDate;

        [ObservableProperty]
        private DateTime? _endDate; // Nullable for ongoing projects

        [ObservableProperty]
        private List<EducatorViewModel> _educators = new();

        /// <summary>
        /// Formatted period string for display.
        /// </summary>
        public string FormattedPeriod => EndDate.HasValue 
            ? $"{StartDate:dd/MM/yyyy} - {EndDate.Value:dd/MM/yyyy}"
            : $"{StartDate:dd/MM/yyyy} - In corso";
    }
}
