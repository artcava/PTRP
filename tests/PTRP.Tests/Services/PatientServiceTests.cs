using Microsoft.EntityFrameworkCore;
using PTRP.App.Models;
using PTRP.App.Services;
using PTRP.App.Services.Interfaces;
using PTRP.Data;
using PTRP.Data.Repositories;
using PTRP.Data.Repositories.Interfaces;

namespace PTRP.Tests.Services
{
    /// <summary>
    /// Test per la classe PatientService
    /// Usa InMemory database provider per i test
    /// </summary>
    public class PatientServiceTests : IDisposable
    {
        private readonly PTRPDbContext _context;
        private readonly IPatientRepository _repository;
        private readonly IPatientService _service;

        public PatientServiceTests()
        {
            // Crea DbContext InMemory con nome univoco per ogni test
            var options = new DbContextOptionsBuilder<PTRPDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new PTRPDbContext(options);
            _repository = new PatientRepository(_context);
            _service = new PatientService(_repository);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        /// <summary>
        /// Test positivo: aggiunta di un paziente valido
        /// </summary>
        [Fact]
        public async Task AddAsync_WithValidPatient_SuccessfullyAddsPatient()
        {
            // arrange
            var newPatient = new PatientModel
            {
                FirstName = "Giovanni",
                LastName = "Bianchi"
            };

            // act
            await _service.AddAsync(newPatient);

            // assert
            Assert.NotEqual(Guid.Empty, newPatient.Id);
            Assert.NotEqual(default, newPatient.CreatedAt);

            // Verifica che il paziente sia recuperabile
            var retrievedPatient = await _service.GetByIdAsync(newPatient.Id);
            Assert.NotNull(retrievedPatient);
            Assert.Equal("Giovanni", retrievedPatient.FirstName);
            Assert.Equal("Bianchi", retrievedPatient.LastName);
        }

        /// <summary>
        /// Test di errore: aggiunta paziente con FirstName vuoto
        /// </summary>
        [Fact]
        public async Task AddAsync_WithEmptyFirstName_ThrowsArgumentException()
        {
            // arrange
            var invalidPatient = new PatientModel
            {
                FirstName = "",
                LastName = "Rossi"
            };

            // act & assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.AddAsync(invalidPatient)
            );
            Assert.Contains("nome", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Test di errore: aggiunta paziente con LastName vuoto
        /// </summary>
        [Fact]
        public async Task AddAsync_WithEmptyLastName_ThrowsArgumentException()
        {
            // arrange
            var invalidPatient = new PatientModel
            {
                FirstName = "Marco",
                LastName = ""
            };

            // act & assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.AddAsync(invalidPatient)
            );
            Assert.Contains("cognome", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Test validazione: FirstName troppo lungo
        /// </summary>
        [Fact]
        public async Task AddAsync_WithTooLongFirstName_ThrowsArgumentException()
        {
            // arrange
            var invalidPatient = new PatientModel
            {
                FirstName = new string('A', 101), // 101 caratteri
                LastName = "Rossi"
            };

            // act & assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.AddAsync(invalidPatient)
            );
            Assert.Contains("100 caratteri", exception.Message);
        }

        /// <summary>
        /// Test positivo: ricerca paziente per ID
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_WithExistingId_ReturnsPatient()
        {
            // arrange
            var newPatient = new PatientModel
            {
                FirstName = "Lucia",
                LastName = "Verdi"
            };
            await _service.AddAsync(newPatient);
            var patientId = newPatient.Id;

            // act
            var retrievedPatient = await _service.GetByIdAsync(patientId);

            // assert
            Assert.NotNull(retrievedPatient);
            Assert.Equal(patientId, retrievedPatient.Id);
            Assert.Equal("Lucia", retrievedPatient.FirstName);
        }

        /// <summary>
        /// Test di errore: ricerca paziente non esistente
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_WithNonExistentId_ThrowsInvalidOperationException()
        {
            // arrange
            var nonExistentId = Guid.NewGuid();

            // act & assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.GetByIdAsync(nonExistentId)
            );
            Assert.Contains("non trovato", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Test positivo: aggiornamento paziente
        /// </summary>
        [Fact]
        public async Task UpdateAsync_WithValidPatient_SuccessfullyUpdates()
        {
            // arrange
            var patient = new PatientModel
            {
                FirstName = "Mario",
                LastName = "Rossi"
            };
            await _service.AddAsync(patient);

            // act
            patient.FirstName = "Maria";
            await _service.UpdateAsync(patient);

            // assert
            var updated = await _service.GetByIdAsync(patient.Id);
            Assert.Equal("Maria", updated.FirstName);
            Assert.NotNull(updated.UpdatedAt);
        }

        /// <summary>
        /// Test di errore: eliminazione paziente non trovato
        /// </summary>
        [Fact]
        public async Task DeleteAsync_WithNonExistentId_ThrowsInvalidOperationException()
        {
            // arrange
            var nonExistentId = Guid.NewGuid();

            // act & assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.DeleteAsync(nonExistentId)
            );
            Assert.Contains("non trovato", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Test positivo: eliminazione paziente esistente
        /// </summary>
        [Fact]
        public async Task DeleteAsync_WithExistingId_SuccessfullyDeletes()
        {
            // arrange
            var patient = new PatientModel
            {
                FirstName = "Test",
                LastName = "Delete"
            };
            await _service.AddAsync(patient);
            var patientId = patient.Id;

            // act
            await _service.DeleteAsync(patientId);

            // assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.GetByIdAsync(patientId)
            );
        }

        /// <summary>
        /// Test positivo: ricerca pazienti per nome
        /// </summary>
        [Fact]
        public async Task SearchAsync_WithValidTerm_ReturnsMatchingPatients()
        {
            // arrange
            await _service.AddAsync(new PatientModel { FirstName = "Paolo", LastName = "Gallo" });
            await _service.AddAsync(new PatientModel { FirstName = "Maria", LastName = "Rossi" });
            await _service.AddAsync(new PatientModel { FirstName = "Giovanni", LastName = "Gallo" });

            // act
            var results = await _service.SearchAsync("Gallo");

            // assert
            Assert.Equal(2, results.Count());
            Assert.All(results, p => Assert.Equal("Gallo", p.LastName));
        }

        /// <summary>
        /// Test positivo: ricerca case-insensitive
        /// </summary>
        [Fact]
        public async Task SearchAsync_IsCaseInsensitive()
        {
            // arrange
            await _service.AddAsync(new PatientModel { FirstName = "Paolo", LastName = "Gallo" });

            // act
            var resultsLower = await _service.SearchAsync("gallo");
            var resultsUpper = await _service.SearchAsync("GALLO");

            // assert
            Assert.Single(resultsLower);
            Assert.Single(resultsUpper);
        }

        /// <summary>
        /// Test positivo: GetAllAsync ritorna pazienti ordinati
        /// </summary>
        [Fact]
        public async Task GetAllAsync_ReturnsOrderedPatients()
        {
            // arrange
            await _service.AddAsync(new PatientModel { FirstName = "Zara", LastName = "Zeta" });
            await _service.AddAsync(new PatientModel { FirstName = "Anna", LastName = "Alfa" });
            await _service.AddAsync(new PatientModel { FirstName = "Mario", LastName = "Beta" });

            // act
            var patients = (await _service.GetAllAsync()).ToList();

            // assert
            Assert.Equal(3, patients.Count);
            Assert.Equal("Alfa", patients[0].LastName);
            Assert.Equal("Beta", patients[1].LastName);
            Assert.Equal("Zeta", patients[2].LastName);
        }
    }
}
