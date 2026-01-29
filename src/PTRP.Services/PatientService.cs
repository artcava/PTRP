using PTRP.App.Models;
using PTRP.App.Services.Interfaces;
using PTRP.Data.Repositories.Interfaces;

namespace PTRP.App.Services
{
    /// <summary>
    /// Implementazione del servizio PatientService
    /// Contiene la logica di business per la gestione dei pazienti
    /// Delega le operazioni di persistenza al repository
    /// </summary>
    public class PatientService : IPatientService
    {
        private readonly IPatientRepository _repository;

        /// <summary>
        /// Costruttore con dependency injection
        /// </summary>
        /// <param name="repository">Repository per operazioni database</param>
        public PatientService(IPatientRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        /// <summary>
        /// Recupera tutti i pazienti
        /// </summary>
        public async Task<IEnumerable<PatientModel>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        /// <summary>
        /// Recupera un paziente per ID
        /// </summary>
        public async Task<PatientModel> GetByIdAsync(Guid id)
        {
            var patient = await _repository.GetByIdAsync(id);
            if (patient == null)
            {
                throw new InvalidOperationException($"Paziente con ID {id} non trovato");
            }
            return patient;
        }

        /// <summary>
        /// Aggiunge un nuovo paziente con validazione business
        /// </summary>
        public async Task AddAsync(PatientModel patient)
        {
            // Validazioni business logic
            if (string.IsNullOrWhiteSpace(patient.FirstName))
                throw new ArgumentException("Il nome è obbligatorio", nameof(patient.FirstName));

            if (string.IsNullOrWhiteSpace(patient.LastName))
                throw new ArgumentException("Il cognome è obbligatorio", nameof(patient.LastName));

            if (patient.FirstName.Length > 100)
                throw new ArgumentException("Il nome non può superare i 100 caratteri", nameof(patient.FirstName));

            if (patient.LastName.Length > 100)
                throw new ArgumentException("Il cognome non può superare i 100 caratteri", nameof(patient.LastName));

            // Il repository gestirà la generazione dell'ID e CreatedAt
            await _repository.AddAsync(patient);
        }

        /// <summary>
        /// Aggiorna un paziente esistente con validazione
        /// </summary>
        public async Task UpdateAsync(PatientModel patient)
        {
            if (patient.Id == Guid.Empty)
                throw new ArgumentException("ID paziente non valido", nameof(patient.Id));

            // Verifica esistenza
            if (!await _repository.ExistsAsync(patient.Id))
            {
                throw new InvalidOperationException($"Paziente con ID {patient.Id} non trovato");
            }

            // Validazioni business logic
            if (string.IsNullOrWhiteSpace(patient.FirstName))
                throw new ArgumentException("Il nome è obbligatorio", nameof(patient.FirstName));

            if (string.IsNullOrWhiteSpace(patient.LastName))
                throw new ArgumentException("Il cognome è obbligatorio", nameof(patient.LastName));

            if (patient.FirstName.Length > 100)
                throw new ArgumentException("Il nome non può superare i 100 caratteri", nameof(patient.FirstName));

            if (patient.LastName.Length > 100)
                throw new ArgumentException("Il cognome non può superare i 100 caratteri", nameof(patient.LastName));

            await _repository.UpdateAsync(patient);
        }

        /// <summary>
        /// Elimina un paziente
        /// </summary>
        public async Task DeleteAsync(Guid id)
        {
            bool deleted = await _repository.DeleteAsync(id);
            if (!deleted)
            {
                throw new InvalidOperationException($"Paziente con ID {id} non trovato");
            }
        }

        /// <summary>
        /// Ricerca pazienti per nome o cognome
        /// </summary>
        public async Task<IEnumerable<PatientModel>> SearchAsync(string searchTerm)
        {
            return await _repository.SearchAsync(searchTerm);
        }
    }
}
