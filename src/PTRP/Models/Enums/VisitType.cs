namespace PTRP.Models.Enums;

/// <summary>
/// Tipologia di visita programmata nel piano terapeutico
/// </summary>
public enum VisitType
{
    /// <summary>
    /// Visita di intake (iniziale) - +3 mesi dall'inizio progetto
    /// </summary>
    Intake,

    /// <summary>
    /// Visita intermedia - +6 mesi dall'inizio progetto
    /// </summary>
    Intermediate,

    /// <summary>
    /// Visita finale - +9 mesi dall'inizio progetto
    /// </summary>
    Final,

    /// <summary>
    /// Visita di dimissione - +12 mesi dall'inizio progetto
    /// </summary>
    Discharge,

    /// <summary>
    /// Visita di follow-up aggiuntiva
    /// </summary>
    Followup
}
