namespace PTRP.Models.Enums;

/// <summary>
/// Tipologia di visita programmata secondo il protocollo PTRP.
/// Valori sincronizzati con la tabella visit_types del database.
/// </summary>
public enum VisitType
{
    /// <summary>
    /// Prima apertura - valutazione iniziale (+3 mesi dall'inizio progetto, 90 min)
    /// </summary>
    Intake,

    /// <summary>
    /// Verifica intermedia (+6 mesi dall'Intake, 60 min)
    /// </summary>
    Intermediate,

    /// <summary>
    /// Verifica finale (+6 mesi dalla verifica intermedia, 60 min)
    /// </summary>
    Final,

    /// <summary>
    /// Dimissioni (+1 mese dalla verifica finale, 45 min)
    /// </summary>
    Discharge,

    /// <summary>
    /// Visita aggiuntiva/straordinaria (non canonica)
    /// </summary>
    ExtraVisit
}
