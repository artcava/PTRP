using Moq;
using PTRP.Data.Repositories.Interfaces;
using PTRP.Models;
using PTRP.Services;

namespace PTRP.Tests.Services;

public class EducatorServiceTests
{
    private readonly Mock<IEducatorRepository> _mockRepo;
    private readonly EducatorService _service;

    public EducatorServiceTests()
    {
        _mockRepo = new Mock<IEducatorRepository>();
        _service = new EducatorService(_mockRepo.Object);
    }

    [Fact]
    public async Task AddAsync_ValidEducator_CallsRepository()
    {
        // Arrange
        var educator = CreateValidEducator();
        _mockRepo.Setup(r => r.EmailExistsAsync(It.IsAny<string>(), null)).ReturnsAsync(false);
        _mockRepo.Setup(r => r.AddAsync(It.IsAny<ProfessionalEducatorModel>())).Returns(Task.CompletedTask);

        // Act
        await _service.AddAsync(educator);

        // Assert
        _mockRepo.Verify(r => r.AddAsync(educator), Times.Once);
    }

    [Fact]
    public async Task AddAsync_DuplicateEmail_ThrowsException()
    {
        // Arrange
        var educator = CreateValidEducator();
        _mockRepo.Setup(r => r.EmailExistsAsync(educator.Email!, null)).ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.AddAsync(educator));
    }

    [Fact]
    public async Task AddAsync_InvalidEducator_ThrowsArgumentException()
    {
        // Arrange - educatore senza nome
        var educator = CreateValidEducator();
        educator.FirstName = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _service.AddAsync(educator));
    }

    [Fact]
    public async Task UpdateAsync_ValidEducator_CallsRepository()
    {
        // Arrange
        var educator = CreateValidEducator();
        _mockRepo.Setup(r => r.ExistsAsync(educator.Id)).ReturnsAsync(true);
        _mockRepo.Setup(r => r.EmailExistsAsync(educator.Email!, educator.Id)).ReturnsAsync(false);
        _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<ProfessionalEducatorModel>())).Returns(Task.CompletedTask);

        // Act
        await _service.UpdateAsync(educator);

        // Assert
        _mockRepo.Verify(r => r.UpdateAsync(educator), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NonExistentEducator_ThrowsException()
    {
        // Arrange
        var educator = CreateValidEducator();
        _mockRepo.Setup(r => r.ExistsAsync(educator.Id)).ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.UpdateAsync(educator));
    }

    [Fact]
    public async Task DeleteAsync_ExistingEducator_CallsRepository()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepo.Setup(r => r.DeleteAsync(id)).ReturnsAsync(true);

        // Act
        await _service.DeleteAsync(id);

        // Assert
        _mockRepo.Verify(r => r.DeleteAsync(id), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonExistentEducator_ThrowsException()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepo.Setup(r => r.DeleteAsync(id)).ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.DeleteAsync(id));
    }

    [Fact]
    public async Task GetByStatusAsync_ValidStatus_CallsRepository()
    {
        // Arrange
        var educators = new List<ProfessionalEducatorModel> { CreateValidEducator() };
        _mockRepo.Setup(r => r.GetByStatusAsync("Active")).ReturnsAsync(educators);

        // Act
        var result = await _service.GetByStatusAsync("Active");

        // Assert
        Assert.Single(result);
        _mockRepo.Verify(r => r.GetByStatusAsync("Active"), Times.Once);
    }

    [Fact]
    public async Task GetByStatusAsync_InvalidStatus_ThrowsException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _service.GetByStatusAsync("InvalidStatus"));
    }

    [Fact]
    public async Task GetActiveEducatorsAsync_CallsRepositoryWithActiveStatus()
    {
        // Arrange
        var educators = new List<ProfessionalEducatorModel> { CreateValidEducator() };
        _mockRepo.Setup(r => r.GetByStatusAsync("Active")).ReturnsAsync(educators);

        // Act
        var result = await _service.GetActiveEducatorsAsync();

        // Assert
        Assert.Single(result);
        _mockRepo.Verify(r => r.GetByStatusAsync("Active"), Times.Once);
    }

    [Fact]
    public async Task ValidateAsync_ValidEducator_ReturnsTrue()
    {
        // Arrange
        var educator = CreateValidEducator();

        // Act
        var result = await _service.ValidateAsync(educator);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ValidateAsync_MissingFirstName_ThrowsException()
    {
        // Arrange
        var educator = CreateValidEducator();
        educator.FirstName = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _service.ValidateAsync(educator));
    }

    [Fact]
    public async Task ValidateAsync_InvalidEmail_ThrowsException()
    {
        // Arrange
        var educator = CreateValidEducator();
        educator.Email = "invalid-email";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _service.ValidateAsync(educator));
    }

    [Fact]
    public async Task ValidateAsync_DateOfBirthInFuture_ThrowsException()
    {
        // Arrange
        var educator = CreateValidEducator();
        educator.DateOfBirth = DateTime.Today.AddDays(1);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _service.ValidateAsync(educator));
    }

    [Fact]
    public async Task ValidateAsync_AgeLessThan18_ThrowsException()
    {
        // Arrange
        var educator = CreateValidEducator();
        educator.DateOfBirth = DateTime.Today.AddYears(-17);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _service.ValidateAsync(educator));
    }

    [Fact]
    public async Task DeactivateAsync_ActiveEducator_UpdatesStatus()
    {
        // Arrange
        var id = Guid.NewGuid();
        var educator = CreateValidEducator();
        educator.Id = id;
        educator.Status = "Active";

        _mockRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(educator);
        _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<ProfessionalEducatorModel>())).Returns(Task.CompletedTask);

        // Act
        await _service.DeactivateAsync(id);

        // Assert
        _mockRepo.Verify(
            r => r.UpdateAsync(It.Is<ProfessionalEducatorModel>(e => e.Status == "Inactive")),
            Times.Once);
    }

    [Fact]
    public async Task DeactivateAsync_AlreadyInactive_ThrowsException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var educator = CreateValidEducator();
        educator.Id = id;
        educator.Status = "Inactive";

        _mockRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(educator);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.DeactivateAsync(id));
    }

    [Fact]
    public async Task ActivateAsync_InactiveEducator_UpdatesStatus()
    {
        // Arrange
        var id = Guid.NewGuid();
        var educator = CreateValidEducator();
        educator.Id = id;
        educator.Status = "Inactive";

        _mockRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(educator);
        _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<ProfessionalEducatorModel>())).Returns(Task.CompletedTask);

        // Act
        await _service.ActivateAsync(id);

        // Assert
        _mockRepo.Verify(
            r => r.UpdateAsync(It.Is<ProfessionalEducatorModel>(e => e.Status == "Active")),
            Times.Once);
    }

    [Fact]
    public async Task ActivateAsync_AlreadyActive_ThrowsException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var educator = CreateValidEducator();
        educator.Id = id;
        educator.Status = "Active";

        _mockRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(educator);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.ActivateAsync(id));
    }

    [Fact]
    public async Task SetOnLeaveAsync_ActiveEducator_UpdatesStatus()
    {
        // Arrange
        var id = Guid.NewGuid();
        var educator = CreateValidEducator();
        educator.Id = id;
        educator.Status = "Active";

        _mockRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(educator);
        _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<ProfessionalEducatorModel>())).Returns(Task.CompletedTask);

        // Act
        await _service.SetOnLeaveAsync(id);

        // Assert
        _mockRepo.Verify(
            r => r.UpdateAsync(It.Is<ProfessionalEducatorModel>(e => e.Status == "OnLeave")),
            Times.Once);
    }

    [Fact]
    public async Task SetOnLeaveAsync_AlreadyOnLeave_ThrowsException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var educator = CreateValidEducator();
        educator.Id = id;
        educator.Status = "OnLeave";

        _mockRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(educator);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.SetOnLeaveAsync(id));
    }

    [Fact]
    public async Task GetAvailableSpecializationsAsync_CallsRepository()
    {
        // Arrange
        var specializations = new List<string> { "Psicologo", "Fisioterapista" };
        _mockRepo.Setup(r => r.GetUniqueSpecializationsAsync()).ReturnsAsync(specializations);

        // Act
        var result = await _service.GetAvailableSpecializationsAsync();

        // Assert
        Assert.Equal(2, result.Count());
        _mockRepo.Verify(r => r.GetUniqueSpecializationsAsync(), Times.Once);
    }

    private ProfessionalEducatorModel CreateValidEducator()
    {
        return new ProfessionalEducatorModel
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "Educator",
            Email = "test@example.com",
            PhoneNumber = "+39 333 1234567",
            DateOfBirth = new DateTime(1985, 5, 15),
            Specialization = "Educatore Professionale",
            LicenseNumber = "EP12345",
            HireDate = new DateTime(2020, 1, 1),
            Status = "Active"
        };
    }
}
