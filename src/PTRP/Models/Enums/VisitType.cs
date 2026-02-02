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
    INTAKE,

    /// <summary>
    /// Verifica intermedia (+6 mesi dall'INTAKE, 60 min)
    /// </summary>
    INTERMEDIATE,

    /// <summary>
    /// Verifica finale (+6 mesi dalla verifica intermedia, 60 min)
    /// </summary>
    FINAL,

    /// <summary>
    /// Dimissioni (+1 mese dalla verifica finale, 45 min)
    /// </summary>
    DISCHARGE,

    /// <summary>
    /// Visita aggiuntiva/straordinaria follow-up (non canonica)
    /// </summary>
    FOLLOW_UP
}
