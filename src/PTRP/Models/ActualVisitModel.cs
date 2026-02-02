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
    /// IMMUTABILE: può essere impostato solo alla creazione (private set).
    /// Garantisce l'integrità della relazione 1:1 a livello di modello.
    /// </summary>
    public Guid ScheduledVisitId { get; private set; }

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

    /// <summary>
    /// Costruttore vuoto per EF Core.
    /// </summary>
    public ActualVisitModel()
    {
    }

    /// <summary>
    /// Costruttore che imposta lo ScheduledVisitId.
    /// Dopo questa chiamata, ScheduledVisitId non può essere modificato.
    /// </summary>
    public ActualVisitModel(Guid scheduledVisitId)
    {
        ScheduledVisitId = scheduledVisitId;
    }

    /// <summary>
    /// Metodo interno per impostare lo ScheduledVisitId.
    /// Utilizzato dal repository durante la creazione.
    /// </summary>
    internal void SetScheduledVisitId(Guid scheduledVisitId)
    {
        if (ScheduledVisitId != Guid.Empty)
        {
            throw new InvalidOperationException(
                "ScheduledVisitId è immutabile e può essere impostato solo una volta.");
        }
        ScheduledVisitId = scheduledVisitId;
    }
}
