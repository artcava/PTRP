using System;

namespace PTRP.ViewModels.Projects;

/// <summary>
/// ViewModel per rappresentare un appuntamento/visita collegato a un progetto.
/// Utilizzato nella timeline appuntamenti nel detail panel progetto.
/// </summary>
public class ProjectAppointmentViewModel
{
    /// <summary>
    /// ID univoco dell'appuntamento.
    /// </summary>
    public Guid AppointmentId { get; set; }

    /// <summary>
    /// Tipo di appuntamento.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Descrizione del tipo per visualizzazione.
    /// </summary>
    public string TypeDisplay { get; set; } = string.Empty;

    /// <summary>
    /// Data e ora programmata dell'appuntamento.
    /// </summary>
    public DateTime ScheduledDate { get; set; }

    /// <summary>
    /// Data formattata per visualizzazione.
    /// </summary>
    public string FormattedDate => ScheduledDate.ToString("dd/MM/yyyy HH:mm");

    /// <summary>
    /// Indica se l'appuntamento è stato completato.
    /// </summary>
    public bool IsCompleted { get; set; }

    /// <summary>
    /// Data di completamento (se completato).
    /// </summary>
    public DateTime? CompletedDate { get; set; }

    /// <summary>
    /// Luogo dell'appuntamento.
    /// </summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Stato formattato per visualizzazione.
    /// </summary>
    public string StatusDisplay => IsCompleted ? "Completata" : "Programmata";
}
