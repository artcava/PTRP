using System;

namespace PTRP.ViewModels.Projects;

/// <summary>
/// ViewModel per rappresentare un paziente collegato a un progetto.
/// Utilizzato nella sezione paziente del detail panel progetto.
/// </summary>
public class ProjectPatientViewModel
{
    /// <summary>
    /// ID univoco del paziente.
    /// </summary>
    public Guid PatientId { get; set; }

    /// <summary>
    /// Nome del paziente.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Cognome del paziente.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Nome completo (Nome + Cognome).
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";

    /// <summary>
    /// Data di nascita del paziente.
    /// </summary>
    public DateTime BirthDate { get; set; }

    /// <summary>
    /// Età calcolata del paziente.
    /// </summary>
    public int Age => DateTime.Now.Year - BirthDate.Year;
}
