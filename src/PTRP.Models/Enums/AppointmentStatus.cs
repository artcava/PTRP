namespace PTRP.Models.Enums
{
    /// <summary>
    /// Stato di un appuntamento programmato (ScheduledVisit)
    /// </summary>
    public enum AppointmentStatus
    {
        /// <summary>
        /// Appuntamento programmato, in attesa di essere svolto
        /// </summary>
        Scheduled,

        /// <summary>
        /// Appuntamento completato con visita effettiva registrata
        /// </summary>
        Completed,

        /// <summary>
        /// Appuntamento mancato (paziente assente o visita non svolta)
        /// </summary>
        Missed,

        /// <summary>
        /// Appuntamento riprogrammato a nuova data
        /// </summary>
        Rescheduled,

        /// <summary>
        /// Appuntamento cancellato definitivamente
        /// </summary>
        Cancelled
    }
}
