using Microsoft.EntityFrameworkCore;
using PTRP.Data;
using PTRP.Data.Repositories;
using PTRP.Models;

namespace PTRP.Tests.Repositories;

public class TherapyProjectRepositoryTests : IDisposable
{
    private readonly PTRPDbContext _context;
    private readonly TherapyProjectRepository _repository;
    private readonly PatientModel _testPatient;
    private readonly ProfessionalEducatorModel _testEducator;

    public TherapyProjectRepositoryTests()
    {
        // Setup InMemory database
        var options = new DbContextOptionsBuilder<PTRPDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        _context = new PTRPDbContext(options);
        _repository = new TherapyProjectRepository(_context);

        // Seed test data
        _testPatient = new PatientModel
        {
            Id = Guid.NewGuid(),
            FirstName = "Mario",
            LastName = "Rossi"
        };

        _testEducator = new ProfessionalEducatorModel
        {
            Id = Guid.NewGuid(),
            FirstName = "Luca",
            LastName = "Bianchi",
            Email = "luca.bianchi@example.com",
            PhoneNumber = "+39 333 1234567",
            LicenseNumber = "EP12345",
            Specialization = "Riabilitazione Psicosociale"
        };

        _context.Patients.Add(_testPatient);
        _context.ProfessionalEducators.Add(_testEducator);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllProjects_OrderedByStartDate()
    {
        // Arrange
        var project1 = CreateTestProject("Project 1", DateTime.Now.AddDays(-10));
        var project2 = CreateTestProject("Project 2", DateTime.Now.AddDays(-5));
        await _repository.AddAsync(project1);
        await _repository.AddAsync(project2);

        // Act
        var result = (await _repository.GetAllAsync()).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(project2.Id, result[0].Id); // Più recente prima
        Assert.Equal(project1.Id, result[1].Id);
    }

    [Fact]
    public async Task GetByIdAsync_ValidId_ReturnsProject()
    {
        // Arrange
        var project = CreateTestProject("Test Project");
        await _repository.AddAsync(project);

        // Act
        var result = await _repository.GetByIdAsync(project.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(project.Id, result.Id);
        Assert.Equal(project.Title, result.Title);
    }

    [Fact]
    public async Task GetByIdAsync_InvalidId_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdWithPatientAsync_LoadsPatient()
    {
        // Arrange
        var project = CreateTestProject("Test Project");
        await _repository.AddAsync(project);

        // Act
        var result = await _repository.GetByIdWithPatientAsync(project.Id);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Patient);
        Assert.Equal(_testPatient.Id, result.Patient.Id);
    }

    [Fact]
    public async Task GetByIdWithRelationsAsync_LoadsPatientAndEducators()
    {
        // Arrange
        var project = CreateTestProject("Test Project");
        await _repository.AddAsync(project);
        await _repository.AssignEducatorAsync(project.Id, _testEducator.Id);

        // Act
        var result = await _repository.GetByIdWithRelationsAsync(project.Id);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Patient);
        Assert.Single(result.ProfessionalEducators);
        Assert.Equal(_testEducator.Id, result.ProfessionalEducators.First().Id);
    }

    [Fact]
    public async Task GetByPatientIdAsync_ReturnsPatientProjects()
    {
        // Arrange
        var project1 = CreateTestProject("Project 1");
        var project2 = CreateTestProject("Project 2");
        await _repository.AddAsync(project1);
        await _repository.AddAsync(project2);

        // Act
        var result = await _repository.GetByPatientIdAsync(_testPatient.Id);

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetByEducatorIdAsync_ReturnsEducatorProjects()
    {
        // Arrange
        var project = CreateTestProject("Test Project");
        await _repository.AddAsync(project);
        await _repository.AssignEducatorAsync(project.Id, _testEducator.Id);

        // Act
        var result = await _repository.GetByEducatorIdAsync(_testEducator.Id);

        // Assert
        Assert.Single(result);
        Assert.Equal(project.Id, result.First().Id);
    }

    [Fact]
    public async Task GetByStatusAsync_ReturnsProjectsWithStatus()
    {
        // Arrange
        var project1 = CreateTestProject("Project 1");
        project1.Status = "In Progress";
        var project2 = CreateTestProject("Project 2");
        project2.Status = "Completed";
        await _repository.AddAsync(project1);
        await _repository.AddAsync(project2);

        // Act
        var result = await _repository.GetByStatusAsync("In Progress");

        // Assert
        Assert.Single(result);
        Assert.Equal(project1.Id, result.First().Id);
    }

    [Fact]
    public async Task SearchAsync_FindsByTitleOrDescription()
    {
        // Arrange
        var project1 = CreateTestProject("Cognitive Therapy");
        project1.Description = "Focus on cognitive skills";
        var project2 = CreateTestProject("Physical Therapy");
        project2.Description = "Motor rehabilitation";
        await _repository.AddAsync(project1);
        await _repository.AddAsync(project2);

        // Act
        var result = await _repository.SearchAsync("cognitive");

        // Assert
        Assert.Single(result);
        Assert.Equal(project1.Id, result.First().Id);
    }

    [Fact]
    public async Task AddAsync_AddsProjectSuccessfully()
    {
        // Arrange
        var project = CreateTestProject("New Project");

        // Act
        await _repository.AddAsync(project);
        var result = await _repository.GetByIdAsync(project.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Project", result.Title);
    }

    [Fact]
    public async Task AddAsync_WithNonExistentPatient_ThrowsException()
    {
        // Arrange
        var project = new TherapyProjectModel
        {
            Id = Guid.NewGuid(),
            PatientId = Guid.NewGuid(), // Non-existent
            Title = "Invalid Project",
            StartDate = DateTime.Now
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _repository.AddAsync(project));
    }

    [Fact]
    public async Task UpdateAsync_UpdatesProjectSuccessfully()
    {
        // Arrange
        var project = CreateTestProject("Original Title");
        await _repository.AddAsync(project);

        project.Title = "Updated Title";
        project.Status = "Completed";

        // Act
        await _repository.UpdateAsync(project);
        var result = await _repository.GetByIdAsync(project.Id);

        // Assert
        Assert.Equal("Updated Title", result!.Title);
        Assert.Equal("Completed", result.Status);
        Assert.NotNull(result.UpdatedAt);
    }

    [Fact]
    public async Task DeleteAsync_RemovesProject()
    {
        // Arrange
        var project = CreateTestProject("To Delete");
        await _repository.AddAsync(project);

        // Act
        var deleted = await _repository.DeleteAsync(project.Id);
        var result = await _repository.GetByIdAsync(project.Id);

        // Assert
        Assert.True(deleted);
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_NonExistentProject_ReturnsFalse()
    {
        // Act
        var result = await _repository.DeleteAsync(Guid.NewGuid());

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ExistsAsync_ExistingProject_ReturnsTrue()
    {
        // Arrange
        var project = CreateTestProject("Existing");
        await _repository.AddAsync(project);

        // Act
        var result = await _repository.ExistsAsync(project.Id);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ExistsAsync_NonExistingProject_ReturnsFalse()
    {
        // Act
        var result = await _repository.ExistsAsync(Guid.NewGuid());

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task AssignEducatorAsync_AssignsSuccessfully()
    {
        // Arrange
        var project = CreateTestProject("Test Project");
        await _repository.AddAsync(project);

        // Act
        await _repository.AssignEducatorAsync(project.Id, _testEducator.Id);
        var result = await _repository.GetByIdWithRelationsAsync(project.Id);

        // Assert
        Assert.Single(result!.ProfessionalEducators);
        Assert.Equal(_testEducator.Id, result.ProfessionalEducators.First().Id);
    }

    [Fact]
    public async Task AssignEducatorAsync_DuplicateAssignment_DoesNotDuplicate()
    {
        // Arrange
        var project = CreateTestProject("Test Project");
        await _repository.AddAsync(project);

        // Act
        await _repository.AssignEducatorAsync(project.Id, _testEducator.Id);
        await _repository.AssignEducatorAsync(project.Id, _testEducator.Id); // Duplicate
        var result = await _repository.GetByIdWithRelationsAsync(project.Id);

        // Assert
        Assert.Single(result!.ProfessionalEducators);
    }

    [Fact]
    public async Task RemoveEducatorAsync_RemovesSuccessfully()
    {
        // Arrange
        var project = CreateTestProject("Test Project");
        await _repository.AddAsync(project);
        await _repository.AssignEducatorAsync(project.Id, _testEducator.Id);

        // Act
        await _repository.RemoveEducatorAsync(project.Id, _testEducator.Id);
        var result = await _repository.GetByIdWithRelationsAsync(project.Id);

        // Assert
        Assert.Empty(result!.ProfessionalEducators);
    }

    private TherapyProjectModel CreateTestProject(string title, DateTime? startDate = null)
    {
        return new TherapyProjectModel
        {
            Id = Guid.NewGuid(),
            PatientId = _testPatient.Id,
            Title = title,
            Description = $"Description for {title}",
            StartDate = startDate ?? DateTime.Now,
            Status = "In Progress"
        };
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
