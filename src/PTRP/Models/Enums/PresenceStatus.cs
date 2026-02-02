namespace PTRP.Models.Enums;

/// <summary>
/// Status della presenza del paziente durante una visita effettiva.
/// Corrisponde a PatientAttendance nel DATABASE.md
/// </summary>
public enum PresenceStatus
{
    /// <summary>
    /// Paziente presente e collaborativo
    /// </summary>
    PresentCollaborative,

    /// <summary>
    /// Paziente presente ma non collaborativo
    /// </summary>
    PresentNonCollaborative,

    /// <summary>
    /// Paziente assente con giustificazione
    /// </summary>
    AbsentJustified,

    /// <summary>
    /// Paziente assente senza giustificazione
    /// </summary>
    AbsentUnjustified
}
