namespace PTRP.Services.Enums;

/// <summary>
/// Filtro per stato del progetto terapeutico nella ricerca pazienti.
/// Consente di filtrare i pazienti in base allo stato del loro progetto attivo/più recente.
/// </summary>
public enum ProjectStateFilter
{
    /// <summary>
    /// Nessun filtro - restituisce tutti i pazienti
    /// </summary>
    All,

    /// <summary>
    /// Solo pazienti con progetto attivo (Active)
    /// </summary>
    Active,

    /// <summary>
    /// Solo pazienti con progetto sospeso (Suspended)
    /// </summary>
    Suspended,

    /// <summary>
    /// Solo pazienti con progetto completato (Completed)
    /// </summary>
    Completed,

    /// <summary>
    /// Solo pazienti deceduti (progetto con stato Deceased)
    /// </summary>
    Deceased
}
