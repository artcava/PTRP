using PTRP.Models;
using PTRP.Models.Enums;
using PTRP.Services.Models;

namespace PTRP.Services.Interfaces;

/// <summary>
/// Interfaccia per il servizio di gestione dei Progetti Terapeutici
/// Definisce i contratti per le operazioni CRUD e logica di business
/// Gestisce validazioni e regole di business per TherapyProject
/// </summary>
public interface ITherapyProjectService
{
    /// <summary>
    /// Recupera tutti i progetti terapeutici
    /// </summary>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Collezione di TherapyProjectModel</returns>
    Task<IEnumerable<TherapyProjectModel>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Recupera un progetto specifico per ID
    /// </summary>
    /// <param name="id">ID del progetto</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>TherapyProjectModel se trovato, altrimenti null</returns>
    Task<TherapyProjectModel?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Recupera un progetto con il paziente associato
    /// </summary>
    /// <param name="id">ID del progetto</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>TherapyProjectModel con paziente caricato</returns>
    Task<TherapyProjectModel?> GetByIdWithPatientAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Recupera un progetto con tutte le relazioni (paziente ed educatori)
    /// </summary>
    /// <param name="id">ID del progetto</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>TherapyProjectModel con relazioni complete</returns>
    Task<TherapyProjectModel?> GetByIdWithRelationsAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Recupera il progetto ATTIVO (Active) per un paziente specifico.
    /// REGOLA DI BUSINESS: un paziente può avere UN SOLO progetto Active contemporaneamente.
    /// </summary>
    /// <param name="patientId">ID del paziente</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>TherapyProjectModel Active del paziente, o null se non esiste</returns>
    Task<TherapyProjectModel?> GetActiveForPatientAsync(Guid patientId, CancellationToken ct = default);

    /// <summary>
    /// Recupera tutti i progetti completati (Completed) per un paziente.
    /// Utile per visualizzare lo storico dei progetti terminati.
    /// </summary>
    /// <param name="patientId">ID del paziente</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Collezione di progetti completati del paziente</returns>
    Task<IReadOnlyList<TherapyProjectModel>> GetCompletedForPatientAsync(Guid patientId, CancellationToken ct = default);

    /// <summary>
    /// Recupera tutti i progetti di un paziente (qualsiasi stato)
    /// </summary>
    /// <param name="patientId">ID del paziente</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Collezione di progetti del paziente</returns>
    Task<IEnumerable<TherapyProjectModel>> GetByPatientIdAsync(Guid patientId, CancellationToken ct = default);

    /// <summary>
    /// Recupera tutti i progetti assegnati a un educatore
    /// </summary>
    /// <param name="educatorId">ID dell'educatore</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Collezione di progetti dell'educatore</returns>
    Task<IEnumerable<TherapyProjectModel>> GetByEducatorIdAsync(Guid educatorId, CancellationToken ct = default);

    /// <summary>
    /// Recupera progetti per status
    /// </summary>
    /// <param name="status">Status del progetto</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Collezione di progetti con lo status specificato</returns>
    Task<IEnumerable<TherapyProjectModel>> GetByStatusAsync(string status, CancellationToken ct = default);

    /// <summary>
    /// Cerca progetti per titolo o descrizione
    /// </summary>
    /// <param name="searchTerm">Termine di ricerca</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Collezione di progetti che corrispondono alla ricerca</returns>
    Task<IEnumerable<TherapyProjectModel>> SearchAsync(string searchTerm, CancellationToken ct = default);

    /// <summary>
    /// Crea un nuovo progetto terapeutico con validazione completa delle regole di business.
    /// 
    /// REGOLE DI BUSINESS:
    /// - Verifica che il paziente esista
    /// - Blocca se esiste già un progetto Active per il paziente
    /// - Valida titolo (min 3 caratteri), date, educatori (almeno 1)
    /// - Se GenerateCanonicalAppointments=true, crea 4 appuntamenti (INTAKE, Intermedia, Finale, Dimissioni)
    /// </summary>
    /// <param name="request">Richiesta di creazione progetto con tutti i parametri</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>ID del progetto creato</returns>
    /// <exception cref="ArgumentException">Se i dati della richiesta non sono validi</exception>
    /// <exception cref="InvalidOperationException">Se esiste già un progetto Active per il paziente</exception>
    Task<Guid> CreateProjectAsync(CreateProjectRequest request, CancellationToken ct = default);

    /// <summary>
    /// Cambia lo stato di un progetto terapeutico (Active, Suspended, Completed, Deceased).
    /// 
    /// TRANSIZIONI VALIDE:
    /// - Active -> Suspended, Completed, Deceased
    /// - Suspended -> Active, Completed, Deceased
    /// - Completed/Deceased -> NESSUNA TRANSIZIONE (stati finali)
    /// </summary>
    /// <param name="projectId">ID del progetto</param>
    /// <param name="newState">Nuovo stato del progetto</param>
    /// <param name="ct">CancellationToken</param>
    /// <exception cref="InvalidOperationException">Se la transizione di stato non è valida</exception>
    Task ChangeProjectStateAsync(Guid projectId, TherapyProjectState newState, CancellationToken ct = default);

    /// <summary>
    /// Aggiunge un nuovo progetto terapeutico con validazione business
    /// </summary>
    /// <param name="therapyProject">TherapyProjectModel da aggiungere</param>
    /// <param name="ct">CancellationToken</param>
    /// <exception cref="ArgumentException">Se i dati del progetto non sono validi</exception>
    /// <exception cref="InvalidOperationException">Se il paziente associato non esiste</exception>
    Task AddAsync(TherapyProjectModel therapyProject, CancellationToken ct = default);

    /// <summary>
    /// Aggiorna un progetto esistente con validazione business
    /// </summary>
    /// <param name="therapyProject">TherapyProjectModel da aggiornare</param>
    /// <param name="ct">CancellationToken</param>
    /// <exception cref="ArgumentException">Se i dati del progetto non sono validi</exception>
    /// <exception cref="InvalidOperationException">Se il progetto non esiste</exception>
    Task UpdateAsync(TherapyProjectModel therapyProject, CancellationToken ct = default);

    /// <summary>
    /// Elimina un progetto per ID
    /// Rimuove automaticamente le relazioni con gli educatori
    /// </summary>
    /// <param name="id">ID del progetto da eliminare</param>
    /// <param name="ct">CancellationToken</param>
    /// <exception cref="InvalidOperationException">Se il progetto non esiste</exception>
    Task DeleteAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Assegna un educatore professionale a un progetto
    /// </summary>
    /// <param name="projectId">ID del progetto</param>
    /// <param name="educatorId">ID dell'educatore</param>
    /// <param name="ct">CancellationToken</param>
    /// <exception cref="InvalidOperationException">Se progetto o educatore non esistono</exception>
    Task AssignEducatorAsync(Guid projectId, Guid educatorId, CancellationToken ct = default);

    /// <summary>
    /// Rimuove un educatore da un progetto
    /// </summary>
    /// <param name="projectId">ID del progetto</param>
    /// <param name="educatorId">ID dell'educatore</param>
    /// <param name="ct">CancellationToken</param>
    Task RemoveEducatorAsync(Guid projectId, Guid educatorId, CancellationToken ct = default);

    /// <summary>
    /// Valida un progetto terapeutico secondo le regole di business
    /// </summary>
    /// <param name="therapyProject">Progetto da validare</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>True se valido, False altrimenti</returns>
    /// <exception cref="ArgumentException">Se la validazione fallisce, con dettagli dell'errore</exception>
    Task<bool> ValidateAsync(TherapyProjectModel therapyProject, CancellationToken ct = default);

    /// <summary>
    /// Completa un progetto cambiando lo status a "Completed"
    /// Valida che la EndDate sia impostata
    /// </summary>
    /// <param name="projectId">ID del progetto</param>
    /// <param name="ct">CancellationToken</param>
    /// <exception cref="InvalidOperationException">Se il progetto non può essere completato</exception>
    Task CompleteProjectAsync(Guid projectId, CancellationToken ct = default);

    /// <summary>
    /// Mette in pausa un progetto cambiando lo status a "On Hold"
    /// </summary>
    /// <param name="projectId">ID del progetto</param>
    /// <param name="ct">CancellationToken</param>
    Task PutOnHoldAsync(Guid projectId, CancellationToken ct = default);

    /// <summary>
    /// Riattiva un progetto in pausa riportandolo a "In Progress"
    /// </summary>
    /// <param name="projectId">ID del progetto</param>
    /// <param name="ct">CancellationToken</param>
    Task ResumeProjectAsync(Guid projectId, CancellationToken ct = default);
}
