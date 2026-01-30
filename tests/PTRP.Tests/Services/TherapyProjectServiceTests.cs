using Moq;
using PTRP.Data.Repositories.Interfaces;
using PTRP.Models;
using PTRP.Services;

namespace PTRP.Tests.Services;

public class TherapyProjectServiceTests
{
    private readonly Mock<ITherapyProjectRepository> _mockTherapyProjectRepo;
    private readonly Mock<IPatientRepository> _mockPatientRepo;
    private readonly TherapyProjectService _service;

    public TherapyProjectServiceTests()
    {
        _mockTherapyProjectRepo = new Mock<ITherapyProjectRepository>();
        _mockPatientRepo = new Mock<IPatientRepository>();
        _service = new TherapyProjectService(_mockTherapyProjectRepo.Object, _mockPatientRepo.Object);
    }

    [Fact]
    public async Task AddAsync_ValidProject_CallsRepository()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var project = CreateValidProject(patientId);
        
        _mockPatientRepo.Setup(r => r.ExistsAsync(patientId)).ReturnsAsync(true);
        _mockTherapyProjectRepo.Setup(r => r.AddAsync(It.IsAny<TherapyProjectModel>())).Returns(Task.CompletedTask);

        // Act
        await _service.AddAsync(project);

        // Assert
        _mockTherapyProjectRepo.Verify(r => r.AddAsync(project), Times.Once);
    }

    [Fact]
    public async Task AddAsync_NonExistentPatient_ThrowsException()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var project = CreateValidProject(patientId);
        
        _mockPatientRepo.Setup(r => r.ExistsAsync(patientId)).ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.AddAsync(project));
    }

    [Fact]
    public async Task AddAsync_InvalidProject_ThrowsArgumentException()
    {
        // Arrange
        var project = new TherapyProjectModel
        {
            PatientId = Guid.NewGuid(),
            // Title mancante - validazione fallirà
            StartDate = DateTime.Now
        };
        
        _mockPatientRepo.Setup(r => r.ExistsAsync(It.IsAny<Guid>())).ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _service.AddAsync(project));
    }

    [Fact]
    public async Task UpdateAsync_ValidProject_CallsRepository()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var project = CreateValidProject(patientId);
        
        _mockTherapyProjectRepo.Setup(r => r.ExistsAsync(project.Id)).ReturnsAsync(true);
        _mockPatientRepo.Setup(r => r.ExistsAsync(patientId)).ReturnsAsync(true);
        _mockTherapyProjectRepo.Setup(r => r.UpdateAsync(It.IsAny<TherapyProjectModel>())).Returns(Task.CompletedTask);

        // Act
        await _service.UpdateAsync(project);

        // Assert
        _mockTherapyProjectRepo.Verify(r => r.UpdateAsync(project), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NonExistentProject_ThrowsException()
    {
        // Arrange
        var project = CreateValidProject(Guid.NewGuid());
        
        _mockTherapyProjectRepo.Setup(r => r.ExistsAsync(project.Id)).ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.UpdateAsync(project));
    }

    [Fact]
    public async Task DeleteAsync_ExistingProject_CallsRepository()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        _mockTherapyProjectRepo.Setup(r => r.DeleteAsync(projectId)).ReturnsAsync(true);

        // Act
        await _service.DeleteAsync(projectId);

        // Assert
        _mockTherapyProjectRepo.Verify(r => r.DeleteAsync(projectId), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonExistentProject_ThrowsException()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        _mockTherapyProjectRepo.Setup(r => r.DeleteAsync(projectId)).ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.DeleteAsync(projectId));
    }

    [Fact]
    public async Task GetByStatusAsync_ValidStatus_CallsRepository()
    {
        // Arrange
        var status = "In Progress";
        var projects = new List<TherapyProjectModel> { CreateValidProject(Guid.NewGuid()) };
        _mockTherapyProjectRepo.Setup(r => r.GetByStatusAsync(status)).ReturnsAsync(projects);

        // Act
        var result = await _service.GetByStatusAsync(status);

        // Assert
        Assert.Single(result);
        _mockTherapyProjectRepo.Verify(r => r.GetByStatusAsync(status), Times.Once);
    }

    [Fact]
    public async Task GetByStatusAsync_InvalidStatus_ThrowsException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _service.GetByStatusAsync("InvalidStatus"));
    }

    [Fact]
    public async Task GetByPatientIdAsync_ExistingPatient_ReturnsProjects()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var projects = new List<TherapyProjectModel> { CreateValidProject(patientId) };
        
        _mockPatientRepo.Setup(r => r.ExistsAsync(patientId)).ReturnsAsync(true);
        _mockTherapyProjectRepo.Setup(r => r.GetByPatientIdAsync(patientId)).ReturnsAsync(projects);

        // Act
        var result = await _service.GetByPatientIdAsync(patientId);

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public async Task GetByPatientIdAsync_NonExistentPatient_ThrowsException()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        _mockPatientRepo.Setup(r => r.ExistsAsync(patientId)).ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.GetByPatientIdAsync(patientId));
    }

    [Fact]
    public async Task ValidateAsync_ValidProject_ReturnsTrue()
    {
        // Arrange
        var project = CreateValidProject(Guid.NewGuid());

        // Act
        var result = await _service.ValidateAsync(project);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ValidateAsync_EndDateBeforeStartDate_ThrowsException()
    {
        // Arrange
        var project = CreateValidProject(Guid.NewGuid());
        project.EndDate = project.StartDate.AddDays(-1); // EndDate prima di StartDate

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _service.ValidateAsync(project));
    }

    [Fact]
    public async Task CompleteProjectAsync_WithEndDate_CompletesSuccessfully()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var project = CreateValidProject(Guid.NewGuid());
        project.Id = projectId;
        project.EndDate = DateTime.Now.AddDays(1);
        project.Status = "In Progress";
        
        _mockTherapyProjectRepo.Setup(r => r.GetByIdAsync(projectId)).ReturnsAsync(project);
        _mockTherapyProjectRepo.Setup(r => r.UpdateAsync(It.IsAny<TherapyProjectModel>())).Returns(Task.CompletedTask);

        // Act
        await _service.CompleteProjectAsync(projectId);

        // Assert
        _mockTherapyProjectRepo.Verify(
            r => r.UpdateAsync(It.Is<TherapyProjectModel>(p => p.Status == "Completed")), 
            Times.Once);
    }

    [Fact]
    public async Task CompleteProjectAsync_WithoutEndDate_ThrowsException()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var project = CreateValidProject(Guid.NewGuid());
        project.Id = projectId;
        project.EndDate = null; // Nessuna EndDate
        
        _mockTherapyProjectRepo.Setup(r => r.GetByIdAsync(projectId)).ReturnsAsync(project);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.CompleteProjectAsync(projectId));
    }

    [Fact]
    public async Task CompleteProjectAsync_AlreadyCompleted_ThrowsException()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var project = CreateValidProject(Guid.NewGuid());
        project.Id = projectId;
        project.EndDate = DateTime.Now;
        project.Status = "Completed"; // Già completato
        
        _mockTherapyProjectRepo.Setup(r => r.GetByIdAsync(projectId)).ReturnsAsync(project);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.CompleteProjectAsync(projectId));
    }

    [Fact]
    public async Task PutOnHoldAsync_InProgressProject_UpdatesStatus()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var project = CreateValidProject(Guid.NewGuid());
        project.Id = projectId;
        project.Status = "In Progress";
        
        _mockTherapyProjectRepo.Setup(r => r.GetByIdAsync(projectId)).ReturnsAsync(project);
        _mockTherapyProjectRepo.Setup(r => r.UpdateAsync(It.IsAny<TherapyProjectModel>())).Returns(Task.CompletedTask);

        // Act
        await _service.PutOnHoldAsync(projectId);

        // Assert
        _mockTherapyProjectRepo.Verify(
            r => r.UpdateAsync(It.Is<TherapyProjectModel>(p => p.Status == "On Hold")), 
            Times.Once);
    }

    [Fact]
    public async Task PutOnHoldAsync_CompletedProject_ThrowsException()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var project = CreateValidProject(Guid.NewGuid());
        project.Id = projectId;
        project.Status = "Completed";
        
        _mockTherapyProjectRepo.Setup(r => r.GetByIdAsync(projectId)).ReturnsAsync(project);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.PutOnHoldAsync(projectId));
    }

    [Fact]
    public async Task ResumeProjectAsync_OnHoldProject_UpdatesStatus()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var project = CreateValidProject(Guid.NewGuid());
        project.Id = projectId;
        project.Status = "On Hold";
        
        _mockTherapyProjectRepo.Setup(r => r.GetByIdAsync(projectId)).ReturnsAsync(project);
        _mockTherapyProjectRepo.Setup(r => r.UpdateAsync(It.IsAny<TherapyProjectModel>())).Returns(Task.CompletedTask);

        // Act
        await _service.ResumeProjectAsync(projectId);

        // Assert
        _mockTherapyProjectRepo.Verify(
            r => r.UpdateAsync(It.Is<TherapyProjectModel>(p => p.Status == "In Progress")), 
            Times.Once);
    }

    [Fact]
    public async Task ResumeProjectAsync_NotOnHoldProject_ThrowsException()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var project = CreateValidProject(Guid.NewGuid());
        project.Id = projectId;
        project.Status = "In Progress";
        
        _mockTherapyProjectRepo.Setup(r => r.GetByIdAsync(projectId)).ReturnsAsync(project);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.ResumeProjectAsync(projectId));
    }

    [Fact]
    public async Task AssignEducatorAsync_ValidIds_CallsRepository()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var educatorId = Guid.NewGuid();
        
        _mockTherapyProjectRepo.Setup(r => r.ExistsAsync(projectId)).ReturnsAsync(true);
        _mockTherapyProjectRepo.Setup(r => r.AssignEducatorAsync(projectId, educatorId)).Returns(Task.CompletedTask);

        // Act
        await _service.AssignEducatorAsync(projectId, educatorId);

        // Assert
        _mockTherapyProjectRepo.Verify(r => r.AssignEducatorAsync(projectId, educatorId), Times.Once);
    }

    [Fact]
    public async Task RemoveEducatorAsync_ValidIds_CallsRepository()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var educatorId = Guid.NewGuid();
        
        _mockTherapyProjectRepo.Setup(r => r.ExistsAsync(projectId)).ReturnsAsync(true);
        _mockTherapyProjectRepo.Setup(r => r.RemoveEducatorAsync(projectId, educatorId)).Returns(Task.CompletedTask);

        // Act
        await _service.RemoveEducatorAsync(projectId, educatorId);

        // Assert
        _mockTherapyProjectRepo.Verify(r => r.RemoveEducatorAsync(projectId, educatorId), Times.Once);
    }

    private TherapyProjectModel CreateValidProject(Guid patientId)
    {
        return new TherapyProjectModel
        {
            Id = Guid.NewGuid(),
            PatientId = patientId,
            Title = "Test Therapy Project",
            Description = "Test description",
            StartDate = DateTime.Now,
            Status = "In Progress"
        };
    }
}
