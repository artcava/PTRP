namespace PTRP.Models.Enums
{
    /// <summary>
    /// Origine/fonte della registrazione di una visita effettiva.
    /// Utile per tracciabilità e audit trail.
    /// </summary>
    public enum VisitSource
    {
        /// <summary>
        /// Visita importata dall'app mobile/portatile dell'educatore
        /// (flusso tipico: educatore registra su dispositivo, poi sincronizza con coordinatore)
        /// </summary>
        EducatorImport,

        /// <summary>
        /// Visita inserita direttamente dal coordinatore nell'app desktop
        /// (caso eccezionale, es. recupero dati mancanti)
        /// </summary>
        CoordinatorDirect
    }
}
