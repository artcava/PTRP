namespace PTRP.Models.Enums;

/// <summary>
/// Stati possibili per un Progetto Terapeutico.
/// Lo stato clinico del paziente è una proprietà del progetto, non del paziente stesso.
/// </summary>
public enum TherapyProjectState
{
    /// <summary>
    /// Progetto attivo e in corso
    /// Un paziente può avere UN SOLO progetto Active contemporaneamente
    /// </summary>
    Active,

    /// <summary>
    /// Progetto temporaneamente sospeso
    /// </summary>
    Suspended,

    /// <summary>
    /// Progetto completato con successo
    /// </summary>
    Completed,

    /// <summary>
    /// Paziente deceduto - progetto terminato
    /// </summary>
    Deceased
}
