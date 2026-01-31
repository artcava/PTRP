using System;

namespace PTRP.ViewModels.Projects;

/// <summary>
/// ViewModel per rappresentare un educatore assegnato a un progetto.
/// Utilizzato nella lista educatori nel detail panel del progetto.
/// </summary>
public class ProjectEducatorViewModel
{
    /// <summary>
    /// ID univoco dell'educatore.
    /// </summary>
    public Guid EducatorId { get; set; }

    /// <summary>
    /// Nome dell'educatore.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Cognome dell'educatore.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Nome completo (Nome + Cognome).
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";

    /// <summary>
    /// Iniziali dell'educatore (es: "MB" per Mario Bianchi).
    /// </summary>
    public string Initials { get; set; } = string.Empty;
}
