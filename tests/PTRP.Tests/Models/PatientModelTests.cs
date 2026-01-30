using PTRP.Models;

namespace PTRP.Tests.Models
{
    /// <summary>
    /// Test per la classe PatientModel
    /// </summary>
    public class PatientModelTests
    {
        [Fact]
        public void Ctor_CreatesPatientWithValidValues()
        {
            // arrange
            var firstName = "Marco";
            var lastName = "Cavallo";
            var now = DateTime.Now;

            // act
            var patient = new PatientModel
            {
                FirstName = firstName,
                LastName = lastName,
                CreatedAt = now
            };

            // assert
            Assert.NotEqual(Guid.Empty, patient.Id); // Guid generato di default
            Assert.Equal(firstName, patient.FirstName);
            Assert.Equal(lastName, patient.LastName);
            Assert.Equal(now, patient.CreatedAt);
            Assert.Null(patient.UpdatedAt);
            Assert.NotNull(patient.TherapyProjects);
            Assert.Empty(patient.TherapyProjects);
        }

        [Fact]
        public void ToString_ReturnsFirstNameAndLastName()
        {
            // arrange
            var patient = new PatientModel
            {
                FirstName = "Anna",
                LastName = "Rossi"
            };

            var expectedString = "Anna Rossi";

            // act
            var result = patient.ToString();

            // assert
            Assert.Equal(expectedString, result);
        }

        [Fact]
        public void DefaultValues_AreSetCorrectly()
        {
            // arrange & act
            var patient = new PatientModel();

            // assert
            Assert.NotEqual(Guid.Empty, patient.Id);
            Assert.NotEqual(DateTime.MinValue, patient.CreatedAt);
            Assert.Null(patient.UpdatedAt);
            Assert.Empty(patient.TherapyProjects);
        }
    }
}
