namespace PTRP.Models.Enums
{
    /// <summary>
    /// Tipologia di visita nel contesto del progetto terapeutico.
    /// Le prime quattro sono i "4 appuntamenti canonici" generati automaticamente.
    /// </summary>
    public enum VisitType
    {
        /// <summary>
        /// Prima Apertura (INTAKE) - generata automaticamente a StartDate + 3 mesi
        /// </summary>
        INTAKE,

        /// <summary>
        /// Verifica Intermedia - generata automaticamente a INTAKE + 6 mesi
        /// </summary>
        INTERMEDIATE,

        /// <summary>
        /// Verifica Finale - generata automaticamente a INTERMEDIATE + 6 mesi
        /// </summary>
        FINAL,

        /// <summary>
        /// Dimissioni - generata automaticamente a FINAL + 1 mese
        /// </summary>
        DISCHARGE,

        /// <summary>
        /// Visita domiciliare non programmata
        /// </summary>
        HOME_VISIT,

        /// <summary>
        /// Follow-up straordinario
        /// </summary>
        FOLLOW_UP,

        /// <summary>
        /// Altro tipo di visita
        /// </summary>
        OTHER
    }
}
