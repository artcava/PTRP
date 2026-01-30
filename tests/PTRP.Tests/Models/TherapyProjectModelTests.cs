using PTRP.Models;

namespace PTRP.Tests.Models;

public class TherapyProjectModelTests
{
    [Fact]
    public void Constructor_SetsPropertiesCorrectly()
    {
        // Arrange & Act
        var patientId = Guid.NewGuid();
        var project = new TherapyProjectModel
        {
            Id = Guid.NewGuid(),
            PatientId = patientId,
            Title = "Progetto Terapeutico 2024",
            Description = "Intervento educativo individualizzato",
            StartDate = new DateTime(2024, 1, 15),
            EndDate = new DateTime(2024, 12, 31),
            Status = "In Progress"
        };

        // Assert
        Assert.NotEqual(Guid.Empty, project.Id);
        Assert.Equal(patientId, project.PatientId);
        Assert.Equal("Progetto Terapeutico 2024", project.Title);
        Assert.Equal("Intervento educativo individualizzato", project.Description);
        Assert.Equal(new DateTime(2024, 1, 15), project.StartDate);
        Assert.Equal(new DateTime(2024, 12, 31), project.EndDate);
        Assert.Equal("In Progress", project.Status);
    }

    [Fact]
    public void ToString_ReturnsCorrectFormat()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var project = new TherapyProjectModel
        {
            PatientId = patientId,
            Title = "Test Project",
            Status = "In Progress"
        };

        // Act
        var result = project.ToString();

        // Assert - Format: "Title (Paziente: ID, Status: Status)"
        Assert.Equal($"Test Project (Paziente: {patientId}, Status: In Progress)", result);
    }

    [Fact]
    public void Status_HasDefaultValue()
    {
        // Arrange & Act
        var project = new TherapyProjectModel
        {
            PatientId = Guid.NewGuid(),
            Title = "Test"
        };

        // Assert - Default status should be "In Progress"
        Assert.Equal("In Progress", project.Status);
    }

    [Fact]
    public void CreatedAt_IsSetAutomatically()
    {
        // Arrange
        var beforeCreation = DateTime.Now.AddSeconds(-1);

        // Act
        var project = new TherapyProjectModel
        {
            PatientId = Guid.NewGuid(),
            Title = "Test Project"
        };

        // Assert
        Assert.True(project.CreatedAt >= beforeCreation);
        Assert.Null(project.UpdatedAt);
    }

    [Fact]
    public void Patient_CanBeNull()
    {
        // Arrange & Act
        var project = new TherapyProjectModel
        {
            PatientId = Guid.NewGuid(),
            Title = "Test"
        };

        // Assert - Patient navigation property can be null
        Assert.Null(project.Patient);
    }

    [Fact]
    public void EndDate_CanBeNull()
    {
        // Arrange & Act
        var project = new TherapyProjectModel
        {
            PatientId = Guid.NewGuid(),
            Title = "Ongoing Project",
            StartDate = DateTime.Now
        };

        // Assert
        Assert.Null(project.EndDate);
    }

    [Fact]
    public void UpdatedAt_CanBeNull()
    {
        // Arrange & Act
        var project = new TherapyProjectModel
        {
            PatientId = Guid.NewGuid(),
            Title = "New Project"
        };

        // Assert
        Assert.Null(project.UpdatedAt);
    }

    [Fact]
    public void ProfessionalEducators_InitializesAsEmptyCollection()
    {
        // Arrange & Act
        var project = new TherapyProjectModel
        {
            PatientId = Guid.NewGuid(),
            Title = "Test Project"
        };

        // Assert
        Assert.NotNull(project.ProfessionalEducators);
        Assert.Empty(project.ProfessionalEducators);
    }
}
