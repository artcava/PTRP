using System;

namespace PTRP.ViewModels.Educators;

/// <summary>
/// ViewModel per rappresentare un paziente assegnato a un educatore.
/// Utilizzato nella lista pazienti nel detail panel di EducatorListView.
/// </summary>
public class AssignedPatientViewModel
{
    /// <summary>
    /// ID univoco del paziente.
    /// </summary>
    public Guid PatientId { get; set; }

    /// <summary>
    /// Nome completo del paziente.
    /// </summary>
    public string PatientFullName { get; set; } = string.Empty;

    /// <summary>
    /// Titolo del progetto terapeutico associato.
    /// </summary>
    public string ProjectTitle { get; set; } = string.Empty;

    /// <summary>
    /// Stato del progetto (Active, Suspended, Completed, Deceased).
    /// </summary>
    public string ProjectState { get; set; } = string.Empty;

    /// <summary>
    /// Data del prossimo appuntamento programmato.
    /// </summary>
    public DateTime? NextAppointmentDate { get; set; }

    /// <summary>
    /// Rappresentazione formattata del prossimo appuntamento.
    /// Esempio: "02/04/2025 14:30"
    /// </summary>
    public string NextAppointmentDisplay => NextAppointmentDate?.ToString("dd/MM/yyyy HH:mm") ?? "Nessun appuntamento";
}
