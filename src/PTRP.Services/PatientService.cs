using PTRP.App.Models;
using PTRP.App.Services.Interfaces;

namespace PTRP.App.Services
{
    /// <summary>
    /// Implementazione del servizio PatientService
    /// Contiene la logica di business per la gestione dei pazienti
    /// 
    /// NOTA: Attualmente implementazione STUB (mock in-memory)
    /// Quando il database sarà disponibile, questo servizio
    /// delegherà a IPatientRepository per la persistenza
    /// </summary>
    public class PatientService : IPatientService
    {
        // Stub in-memory per ora - sarà sostituito con repository quando il DB è pronto
        private static List<PatientModel> _patients = new()
        {
            new PatientModel
            {
                Id = Guid.Parse("f1234567-89ab-cdef-0123-456789abcdef"),
                FirstName = "Marco",
                LastName = "Cavallo",
                CreatedAt = DateTime.Now
            },
            new PatientModel
            {
                Id = Guid.Parse("a0987654-3210-fedc-ba98-765432100abc"),
                FirstName = "Anna",
                LastName = "Rossi",
                CreatedAt = DateTime.Now
            }
        };

        /// <summary>
        /// Costruttore del servizio
        /// 
        /// QUANDO IL DATABASE SARÀ PRONTO:
        /// public PatientService(IPatientRepository repository)
        /// {
        ///     _repository = repository;
        /// }
        /// </summary>
        public PatientService()
        {
            // Attualmente stub - nessuna dependency
        }

        /// <summary>
        /// Recupera tutti i pazienti
        /// </summary>
        public async Task<IEnumerable<PatientModel>> GetAllAsync()
        {
            // Simula latenza asincrona
            await Task.Delay(100);

            // STUB: restituisce dati in-memory
            return _patients.OrderBy(p => p.LastName).ToList();

            // IMPLEMENTAZIONE FINALE:
            // return await _repository.GetAllAsync();
        }

        /// <summary>
        /// Recupera un paziente per ID
        /// </summary>
        public async Task<PatientModel> GetByIdAsync(Guid id)
        {
            await Task.Delay(50);

            var patient = _patients.FirstOrDefault(p => p.Id == id);
            return patient;

            // IMPLEMENTAZIONE FINALE:
            // return await _repository.GetByIdAsync(id);
        }

        /// <summary>
        /// Aggiunge un nuovo paziente con validazione
        /// </summary>
        public async Task AddAsync(PatientModel patient)
        {
            // Validazioni (business logic)
            if (string.IsNullOrWhiteSpace(patient.FirstName))
                throw new ArgumentException("Il nome è obbligatorio", nameof(patient.FirstName));

            if (string.IsNullOrWhiteSpace(patient.LastName))
                throw new ArgumentException("Il cognome è obbligatorio", nameof(patient.LastName));

            // Se l'ID non è stato impostato, genera un nuovo Guid
            if (patient.Id == Guid.Empty)
                patient.Id = Guid.NewGuid();

            // Logica di business: setta CreatedAt se non già impostato
            if (patient.CreatedAt == default)
                patient.CreatedAt = DateTime.Now;

            await Task.Delay(100);

            // STUB: aggiunge in-memory
            _patients.Add(patient);

            // IMPLEMENTAZIONE FINALE:
            // await _repository.AddAsync(patient);
        }

        /// <summary>
        /// Aggiorna un paziente esistente
        /// </summary>
        public async Task UpdateAsync(PatientModel patient)
        {
            if (patient.Id == Guid.Empty)
                throw new ArgumentException("ID paziente non valido", nameof(patient.Id));

            var existing = _patients.FirstOrDefault(p => p.Id == patient.Id);
            if (existing == null)
                throw new InvalidOperationException($"Paziente con ID {patient.Id} non trovato");

            // Logica: aggiorna campi e timestamp
            existing.FirstName = patient.FirstName;
            existing.LastName = patient.LastName;
            existing.UpdatedAt = DateTime.Now;

            await Task.Delay(100);

            // IMPLEMENTAZIONE FINALE:
            // await _repository.UpdateAsync(patient);
        }

        /// <summary>
        /// Elimina un paziente
        /// </summary>
        public async Task DeleteAsync(Guid id)
        {
            var patient = _patients.FirstOrDefault(p => p.Id == id);
            if (patient == null)
                throw new InvalidOperationException($"Paziente con ID {id} non trovato");

            await Task.Delay(100);

            _patients.Remove(patient);

            // IMPLEMENTAZIONE FINALE:
            // await _repository.DeleteAsync(id);
        }

        /// <summary>
        /// Ricerca pazienti per nome (FirstName o LastName contiene il termine)
        /// </summary>
        public async Task<IEnumerable<PatientModel>> SearchAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync();

            await Task.Delay(100);

            var results = _patients
                .Where(p => p.FirstName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                         || p.LastName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .OrderBy(p => p.LastName)
                .ToList();

            return results;

            // IMPLEMENTAZIONE FINALE:
            // return await _repository.SearchAsync(searchTerm);
        }
    }
}
