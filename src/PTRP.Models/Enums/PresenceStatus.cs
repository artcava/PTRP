namespace PTRP.Models.Enums
{
    /// <summary>
    /// Stato di presenza del paziente durante una visita effettiva (ActualVisit)
    /// </summary>
    public enum PresenceStatus
    {
        /// <summary>
        /// Paziente presente e collaborativo durante la visita
        /// </summary>
        PresentCollaborative,

        /// <summary>
        /// Paziente presente ma non collaborativo
        /// </summary>
        PresentNonCollaborative,

        /// <summary>
        /// Paziente assente con giustificazione valida
        /// </summary>
        AbsentJustified,

        /// <summary>
        /// Paziente assente senza giustificazione
        /// </summary>
        AbsentNotJustified
    }
}
