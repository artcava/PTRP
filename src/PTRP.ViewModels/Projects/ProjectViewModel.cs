using System;
using System.Collections.Generic;
using System.Linq;

namespace PTRP.ViewModels.Projects;

/// <summary>
/// ViewModel per rappresentare un Progetto Terapeutico Riabilitativo Personalizzato.
/// Utilizzato sia nella lista master che nel detail panel.
/// </summary>
public class ProjectViewModel
{
    /// <summary>
    /// ID univoco del progetto.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Titolo del progetto (es: "PTRP 2025-2027").
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Descrizione dettagliata del progetto.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Data inizio progetto.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Data fine progetto.
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Periodo formattato (es: "gennaio 2025 - dicembre 2027").
    /// </summary>
    public string Period => $"{StartDate:MMMM yyyy} - {EndDate:MMMM yyyy}";

    /// <summary>
    /// Periodo formattato breve (es: "01/2025 - 12/2027").
    /// </summary>
    public string PeriodShort => $"{StartDate:MM/yyyy} - {EndDate:MM/yyyy}";

    /// <summary>
    /// Stato del progetto: "Active", "Suspended", "Completed", "Deceased".
    /// </summary>
    public string State { get; set; } = "Active";

    /// <summary>
    /// Stato formattato per visualizzazione.
    /// </summary>
    public string StateDisplay => State switch
    {
        "Active" => "Attivo",
        "Suspended" => "Sospeso",
        "Completed" => "Completato",
        "Deceased" => "Deceduto",
        _ => "Sconosciuto"
    };

    /// <summary>
    /// Data di creazione del progetto.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Data di sospensione (se presente).
    /// </summary>
    public DateTime? SuspendedAt { get; set; }

    /// <summary>
    /// Data di completamento (se presente).
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Motivazione della sospensione (se presente).
    /// </summary>
    public string? SuspensionReason { get; set; }

    // === Relazioni ===

    /// <summary>
    /// Paziente collegato al progetto.
    /// </summary>
    public ProjectPatientViewModel Patient { get; set; } = new();

    /// <summary>
    /// Lista educatori assegnati al progetto.
    /// </summary>
    public List<ProjectEducatorViewModel> Educators { get; set; } = new();

    /// <summary>
    /// Numero di educatori assegnati.
    /// </summary>
    public int EducatorCount => Educators.Count;

    /// <summary>
    /// Nomi educatori separati da virgola (solo cognomi).
    /// </summary>
    public string EducatorNamesDisplay => string.Join(", ", Educators.Select(e => e.LastName));

    /// <summary>
    /// Lista appuntamenti/visite collegati al progetto.
    /// </summary>
    public List<ProjectAppointmentViewModel> Appointments { get; set; } = new();

    /// <summary>
    /// Numero totale di appuntamenti.
    /// </summary>
    public int TotalAppointments => Appointments.Count;

    /// <summary>
    /// Numero di appuntamenti completati.
    /// </summary>
    public int CompletedAppointments => Appointments.Count(a => a.IsCompleted);

    /// <summary>
    /// Prossimo appuntamento programmato (se presente).
    /// </summary>
    public ProjectAppointmentViewModel? NextAppointment => 
        Appointments
            .Where(a => !a.IsCompleted && a.ScheduledDate > DateTime.Now)
            .OrderBy(a => a.ScheduledDate)
            .FirstOrDefault();

    // === UI Helpers ===

    /// <summary>
    /// Progresso visite formattato (es: "2/5 visite").
    /// </summary>
    public string ProgressDisplay => $"{CompletedAppointments}/{TotalAppointments} visite";

    /// <summary>
    /// Percentuale progresso visite (0-100).
    /// </summary>
    public double ProgressPercentage => TotalAppointments > 0 
        ? (double)CompletedAppointments / TotalAppointments * 100 
        : 0;
}
