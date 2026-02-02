namespace PTRP.Models.Enums;

/// <summary>
/// Status della presenza del paziente durante una visita effettiva
/// </summary>
public enum PresenceStatus
{
    /// <summary>
    /// Paziente presente alla visita
    /// </summary>
    Attended,

    /// <summary>
    /// Paziente assente
    /// </summary>
    Absent,

    /// <summary>
    /// Presenza parziale (es. arrivato in ritardo, uscito anticipatamente)
    /// </summary>
    PartiallyAttended
}
