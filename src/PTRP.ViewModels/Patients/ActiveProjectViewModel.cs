using System;
using System.Collections.Generic;

namespace PTRP.ViewModels.Patients
{
    /// <summary>
    /// ViewModel per rappresentare un progetto terapeutico attivo nella UI.
    /// Usato in PatientListView per visualizzare dettagli progetto nel pannello laterale.
    /// </summary>
    public class ActiveProjectViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Period { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<EducatorViewModel> Educators { get; set; } = new();
    }
}
