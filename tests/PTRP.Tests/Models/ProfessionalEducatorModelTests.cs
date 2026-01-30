using PTRP.Models;

namespace PTRP.Tests.Models;

public class ProfessionalEducatorModelTests
{
    [Fact]
    public void Constructor_SetsPropertiesCorrectly()
    {
        // Arrange & Act
        var educator = new ProfessionalEducatorModel
        {
            Id = Guid.NewGuid(),
            FirstName = "Mario",
            LastName = "Rossi",
            Email = "mario.rossi@example.com",
            PhoneNumber = "+39 333 1234567",
            DateOfBirth = new DateTime(1985, 5, 15),
            Specialization = "Psicologia",
            LicenseNumber = "PSY12345",
            HireDate = DateTime.Now,
            Status = "Active"
        };

        // Assert
        Assert.NotEqual(Guid.Empty, educator.Id);
        Assert.Equal("Mario", educator.FirstName);
        Assert.Equal("Rossi", educator.LastName);
        Assert.Equal("mario.rossi@example.com", educator.Email);
        Assert.Equal("+39 333 1234567", educator.PhoneNumber);
        Assert.Equal(new DateTime(1985, 5, 15), educator.DateOfBirth);
        Assert.Equal("Psicologia", educator.Specialization);
        Assert.Equal("PSY12345", educator.LicenseNumber);
        Assert.NotEqual(default(DateTime), educator.HireDate);
        Assert.Equal("Active", educator.Status);
    }

    [Fact]
    public void ToString_ReturnsCorrectFormat()
    {
        // Arrange
        var educator = new ProfessionalEducatorModel
        {
            FirstName = "Luigi",
            LastName = "Bianchi",
            Specialization = "Pedagogia"
        };

        // Act
        var result = educator.ToString();

        // Assert
        Assert.Equal("Luigi Bianchi (Pedagogia)", result);
    }

    [Fact]
    public void Status_DefaultsToActive()
    {
        // Arrange & Act
        var educator = new ProfessionalEducatorModel
        {
            FirstName = "Test",
            LastName = "User"
        };

        // Assert - default value should be "Active"
        Assert.Equal("Active", educator.Status);
    }

    [Fact]
    public void CreatedAt_IsSetAutomatically()
    {
        // Arrange
        var beforeCreation = DateTime.Now.AddSeconds(-1);

        // Act
        var educator = new ProfessionalEducatorModel
        {
            FirstName = "Test",
            LastName = "User"
        };

        // Assert
        Assert.True(educator.CreatedAt >= beforeCreation);
        Assert.Null(educator.UpdatedAt);
    }

    [Fact]
    public void AssignedTherapyProjects_InitializesAsEmptyCollection()
    {
        // Arrange & Act
        var educator = new ProfessionalEducatorModel
        {
            FirstName = "Test",
            LastName = "User"
        };

        // Assert
        Assert.NotNull(educator.AssignedTherapyProjects);
        Assert.Empty(educator.AssignedTherapyProjects);
    }
}
