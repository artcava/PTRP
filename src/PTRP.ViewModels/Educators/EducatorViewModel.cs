using System;
using System.Collections.Generic;

namespace PTRP.ViewModels.Educators;

/// <summary>
/// ViewModel per rappresentare un educatore professionale.
/// Utilizzato sia nella lista master che nel detail panel.
/// </summary>
public class EducatorViewModel
{
    /// <summary>
    /// ID univoco dell'educatore.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Nome dell'educatore.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Cognome dell'educatore.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Nome completo (Cognome + Nome).
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";

    /// <summary>
    /// Iniziali dell'educatore (es: "MB" per Mario Bianchi).
    /// </summary>
    public string Initials { get; set; } = string.Empty;

    /// <summary>
    /// Ruolo dell'educatore (Coordinatore, Educatore, Tirocinante).
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Ruolo formattato per visualizzazione nel badge.
    /// </summary>
    public string RoleDisplay => Role switch
    {
        "Coordinatore" => "Coordinatore",
        "Educatore" => "Educatore",
        "Tirocinante" => "Tirocinante",
        "Inattivo" => "Inattivo",
        _ => "Non specificato"
    };

    /// <summary>
    /// Email di contatto.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Numero di telefono.
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Data di assunzione.
    /// </summary>
    public DateTime HireDate { get; set; }

    /// <summary>
    /// Indica se l'educatore è attivo.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Numero di pazienti attualmente assegnati.
    /// </summary>
    public int AssignedPatientsCount { get; set; }

    /// <summary>
    /// Lista dei pazienti assegnati con dettagli.
    /// </summary>
    public List<AssignedPatientViewModel> AssignedPatients { get; set; } = new();

    /// <summary>
    /// Numero totale di visite registrate nel mese corrente.
    /// (Placeholder - sarà popolato quando integrato con database)
    /// </summary>
    public int TotalVisitsThisMonth { get; set; } = 0;

    /// <summary>
    /// Numero di appuntamenti programmati futuri.
    /// (Placeholder - sarà popolato quando integrato con database)
    /// </summary>
    public int ScheduledAppointments { get; set; } = 0;
}
