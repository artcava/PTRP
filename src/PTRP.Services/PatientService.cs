using Microsoft.EntityFrameworkCore;
using PTRP.Models;
using PTRP.Models.Enums;
using PTRP.Services.Enums;
using PTRP.Services.Interfaces;
using PTRP.Data.Repositories.Interfaces;

namespace PTRP.Services
{
    /// <summary>
    /// Implementazione del servizio PatientService
    /// Contiene la logica di business per la gestione dei pazienti
    /// Delega le operazioni di persistenza al repository
    /// </summary>
    public class PatientService : IPatientService
    {
        private readonly IPatientRepository _patientRepository;
        private readonly ITherapyProjectRepository _therapyProjectRepository;

        /// <summary>
        /// Costruttore con dependency injection
        /// </summary>
        /// <param name="patientRepository">Repository per operazioni database pazienti</param>
        /// <param name="therapyProjectRepository">Repository per progetti (per filtro stato)</param>
        public PatientService(
            IPatientRepository patientRepository,
            ITherapyProjectRepository therapyProjectRepository)
        {
            _patientRepository = patientRepository ?? throw new ArgumentNullException(nameof(patientRepository));
            _therapyProjectRepository = therapyProjectRepository ?? throw new ArgumentNullException(nameof(therapyProjectRepository));
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<PatientModel>> GetAllAsync(CancellationToken ct = default)
        {
            var patients = await _patientRepository.GetAllAsync();
            return patients.ToList();
        }

        /// <inheritdoc />
        public async Task<PatientModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var patient = await _patientRepository.GetByIdAsync(id);
            if (patient == null)
            {
                throw new InvalidOperationException($"Paziente con ID {id} non trovato");
            }
            return patient;
        }

        /// <inheritdoc />
        public async Task<PatientModel?> GetByIdWithProjectsAsync(Guid id, CancellationToken ct = default)
        {
            return await _patientRepository.GetByIdWithProjectsAsync(id);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<PatientModel>> SearchAsync(
            string? searchTerm = null,
            ProjectStateFilter? stateFilter = null,
            CancellationToken ct = default)
        {
            // Se non c'è filtro di stato, usa ricerca semplice esistente
            if (!stateFilter.HasValue || stateFilter == ProjectStateFilter.All)
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return await GetAllAsync(ct);
                }
                var patients = await _patientRepository.SearchAsync(searchTerm);
                return patients.ToList();
            }

            // Logica con filtro stato progetto
            // Recupera tutti i pazienti con progetti
            IEnumerable<PatientModel> allPatients;
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                // Nessun searchTerm: tutti i pazienti
                allPatients = await _patientRepository.GetAllAsync();
            }
            else
            {
                // Con searchTerm: filtra per nome/cognome
                allPatients = await _patientRepository.SearchAsync(searchTerm);
            }

            // Carica progetti per ciascun paziente e filtra per stato
            var filteredPatients = new List<PatientModel>();

            foreach (var patient in allPatients)
            {
                var projects = await _therapyProjectRepository.GetByPatientIdAsync(patient.Id);
                
                // Filtra progetti per stato
                bool shouldInclude = stateFilter.Value switch
                {
                    ProjectStateFilter.Active => projects.Any(p => p.Status == nameof(TherapyProjectState.Active)),
                    ProjectStateFilter.Suspended => projects.Any(p => p.Status == nameof(TherapyProjectState.Suspended)),
                    ProjectStateFilter.Completed => projects.Any(p => p.Status == nameof(TherapyProjectState.Completed)),
                    ProjectStateFilter.Deceased => projects.Any(p => p.Status == nameof(TherapyProjectState.Deceased)),
                    _ => true
                };

                if (shouldInclude)
                {
                    filteredPatients.Add(patient);
                }
            }

            return filteredPatients;
        }

        /// <inheritdoc />
        public async Task CreateAsync(PatientModel patient, CancellationToken ct = default)
        {
            // Validazioni business logic
            if (string.IsNullOrWhiteSpace(patient.FirstName))
                throw new ArgumentException("Il nome è obbligatorio", nameof(patient.FirstName));

            if (string.IsNullOrWhiteSpace(patient.LastName))
                throw new ArgumentException("Il cognome è obbligatorio", nameof(patient.LastName));

            if ((patient.FirstName?.Length ?? 0) > 100)
                throw new ArgumentException("Il nome non può superare i 100 caratteri", nameof(patient.FirstName));

            if ((patient.LastName?.Length ?? 0) > 100)
                throw new ArgumentException("Il cognome non può superare i 100 caratteri", nameof(patient.LastName));

            // Il repository gestirà la generazione dell'ID e CreatedAt
            await _patientRepository.AddAsync(patient);
        }

        /// <inheritdoc />
        public async Task UpdateAsync(PatientModel patient, CancellationToken ct = default)
        {
            if (patient.Id == Guid.Empty)
                throw new ArgumentException("ID paziente non valido", nameof(patient.Id));

            // Verifica esistenza
            if (!await _patientRepository.ExistsAsync(patient.Id))
            {
                throw new InvalidOperationException($"Paziente con ID {patient.Id} non trovato");
            }

            // Validazioni business logic
            if (string.IsNullOrWhiteSpace(patient.FirstName))
                throw new ArgumentException("Il nome è obbligatorio", nameof(patient.FirstName));

            if (string.IsNullOrWhiteSpace(patient.LastName))
                throw new ArgumentException("Il cognome è obbligatorio", nameof(patient.LastName));

            if ((patient.FirstName?.Length ?? 0) > 100)
                throw new ArgumentException("Il nome non può superare i 100 caratteri", nameof(patient.FirstName));

            if ((patient.LastName?.Length ?? 0) > 100)
                throw new ArgumentException("Il cognome non può superare i 100 caratteri", nameof(patient.LastName));

            await _patientRepository.UpdateAsync(patient);
        }

        /// <inheritdoc />
        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            bool deleted = await _patientRepository.DeleteAsync(id);
            if (!deleted)
            {
                throw new InvalidOperationException($"Paziente con ID {id} non trovato");
            }
        }
    }
}
