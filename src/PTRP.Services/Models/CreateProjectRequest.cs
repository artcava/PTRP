using PTRP.Models.Enums;

namespace PTRP.Services.Models;

/// <summary>
/// Record per la richiesta di creazione di un nuovo Progetto Terapeutico.
/// Incapsula tutti i parametri necessari per creare un progetto con le relative regole di business.
/// </summary>
/// <param name="PatientId">ID del paziente per cui creare il progetto</param>
/// <param name="Title">Titolo del progetto (obbligatorio, min 3 caratteri)</param>
/// <param name="Description">Descrizione dettagliata del progetto (opzionale)</param>
/// <param name="StartDate">Data di inizio del progetto (obbligatoria)</param>
/// <param name="PlannedEndDate">Data di fine prevista del progetto (opzionale)</param>
/// <param name="InitialState">Stato iniziale del progetto (default: Active)</param>
/// <param name="EducatorIds">Lista di ID degli educatori da assegnare al progetto (almeno 1 richiesto)</param>
/// <param name="GenerateCanonicalAppointments">Se true, genera automaticamente i 4 appuntamenti canonici (INTAKE, Intermedia, Finale, Dimissioni)</param>
public record CreateProjectRequest(
    Guid PatientId,
    string Title,
    string? Description,
    DateTime StartDate,
    DateTime? PlannedEndDate,
    TherapyProjectState InitialState,
    IReadOnlyList<Guid> EducatorIds,
    bool GenerateCanonicalAppointments
);
