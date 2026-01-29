using PTRP.App.Models;
using PTRP.App.Services;
using PTRP.App.Services.Interfaces;

namespace PTRP.Tests.Services
{
    /// <summary>
    /// Test per la classe PatientService
    /// </summary>
    public class PatientServiceTests
    {
        private IPatientService CreatePatientService()
        {
            return new PatientService();
        }

        /// <summary>
        /// Test positivo: aggiunta di un paziente valido
        /// </summary>
        [Fact]
        public async Task AddAsync_WithValidPatient_SuccessfullyAddsPatient()
        {
            // arrange
            var service = CreatePatientService();
            var newPatient = new PatientModel
            {
                FirstName = "Giovanni",
                LastName = "Bianchi"
            };

            // act
            await service.AddAsync(newPatient);

            // assert
            Assert.NotEqual(Guid.Empty, newPatient.Id);
            Assert.NotEqual(default, newPatient.CreatedAt);

            // Verifica che il paziente sia recuperabile
            var retrievedPatient = await service.GetByIdAsync(newPatient.Id);
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
            var service = CreatePatientService();
            var invalidPatient = new PatientModel
            {
                FirstName = "",
                LastName = "Rossi"
            };

            // act & assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => service.AddAsync(invalidPatient)
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
            var service = CreatePatientService();
            var invalidPatient = new PatientModel
            {
                FirstName = "Marco",
                LastName = ""
            };

            // act & assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => service.AddAsync(invalidPatient)
            );
            Assert.Contains("cognome", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Test positivo: ricerca paziente per ID
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_WithExistingId_ReturnsPatient()
        {
            // arrange
            var service = CreatePatientService();
            var newPatient = new PatientModel
            {
                FirstName = "Lucia",
                LastName = "Verdi"
            };
            await service.AddAsync(newPatient);
            var patientId = newPatient.Id;

            // act
            var retrievedPatient = await service.GetByIdAsync(patientId);

            // assert
            Assert.NotNull(retrievedPatient);
            Assert.Equal(patientId, retrievedPatient.Id);
            Assert.Equal("Lucia", retrievedPatient.FirstName);
        }

        /// <summary>
        /// Test di errore: eliminazione paziente non trovato
        /// </summary>
        [Fact]
        public async Task DeleteAsync_WithNonExistentId_ThrowsInvalidOperationException()
        {
            // arrange
            var service = CreatePatientService();
            var nonExistentId = Guid.NewGuid();

            // act & assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.DeleteAsync(nonExistentId)
            );
            Assert.Contains("non trovato", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Test positivo: ricerca pazienti per nome
        /// </summary>
        [Fact]
        public async Task SearchAsync_WithValidTerm_ReturnsMatchingPatients()
        {
            // arrange
            var service = CreatePatientService();
            var newPatient = new PatientModel
            {
                FirstName = "Paolo",
                LastName = "Gallo"
            };
            await service.AddAsync(newPatient);

            // act
            var results = await service.SearchAsync("Gallo");

            // assert
            Assert.NotEmpty(results);
            Assert.Contains(results, p => p.LastName == "Gallo");
        }
    }
}
