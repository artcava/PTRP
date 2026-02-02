using PTRP.Models.Enums;

namespace PTRP.Models;

/// <summary>
/// Rappresenta una visita effettivamente svolta.
/// Ha una relazione 1:1 con ScheduledVisitModel.
/// Lo ScheduledVisitId è immutabile dopo la creazione.
/// </summary>
public class ActualVisitModel
{
    public Guid Id { get; set; }

    /// <summary>
    /// ID della visita programmata associata.
    /// IMMUTABILE: può essere impostato solo durante l'inizializzazione dell'oggetto.
    /// Garantisce l'integrità della relazione 1:1 a livello di modello.
    /// </summary>
    public Guid ScheduledVisitId { get; init; }

    public DateTime ActualDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string ClinicalNotes { get; set; } = string.Empty;
    public PresenceStatus PatientPresence { get; set; }
    public VisitSource Source { get; set; }
    public Guid RegisteredBy { get; set; }
    public string RegisteredByName { get; set; } = string.Empty;

    // Navigation properties
    public ScheduledVisitModel ScheduledVisit { get; set; } = null!;
    public ICollection<VisitOperatorModel> OperatorsPresent { get; set; } = new List<VisitOperatorModel>();
}
