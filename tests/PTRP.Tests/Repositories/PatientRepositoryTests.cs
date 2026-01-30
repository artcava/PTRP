using Microsoft.EntityFrameworkCore;
using PTRP.Data;
using PTRP.Data.Repositories;
using PTRP.Models;

namespace PTRP.Tests.Repositories;

public class PatientRepositoryTests : IDisposable
{
    private readonly PTRPDbContext _context;
    private readonly PatientRepository _repository;

    public PatientRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<PTRPDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        _context = new PTRPDbContext(options);
        _repository = new PatientRepository(_context);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllPatients_OrderedByLastName()
    {
        // Arrange
        var patient1 = CreateValidPatient("Mario", "Rossi");
        var patient2 = CreateValidPatient("Luigi", "Bianchi");
        await _repository.AddAsync(patient1);
        await _repository.AddAsync(patient2);

        // Act
        var result = (await _repository.GetAllAsync()).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Bianchi", result[0].LastName); // Alfabeticamente prima
        Assert.Equal("Rossi", result[1].LastName);
    }

    [Fact]
    public async Task GetByIdAsync_ValidId_ReturnsPatient()
    {
        // Arrange
        var patient = CreateValidPatient("Test", "Patient");
        await _repository.AddAsync(patient);

        // Act
        var result = await _repository.GetByIdAsync(patient.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(patient.Id, result.Id);
        Assert.Equal(patient.FirstName, result.FirstName);
        Assert.Equal(patient.LastName, result.LastName);
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
    public async Task GetByIdWithProjectsAsync_LoadsTherapyProjects()
    {
        // Arrange
        var patient = CreateValidPatient("Test", "Patient");
        await _repository.AddAsync(patient);

        // Aggiungi un progetto terapeutico
        var project = new TherapyProjectModel
        {
            PatientId = patient.Id,
            Title = "Test Project",
            StartDate = DateTime.Now,
            Status = "In Progress"
        };
        _context.TherapyProjects.Add(project);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdWithProjectsAsync(patient.Id);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.TherapyProjects);
        Assert.Single(result.TherapyProjects);
        Assert.Equal("Test Project", result.TherapyProjects.First().Title);
    }

    [Fact]
    public async Task SearchAsync_FindsByFirstName()
    {
        // Arrange
        var patient = CreateValidPatient("Giovanni", "Verdi");
        await _repository.AddAsync(patient);

        // Act
        var result = await _repository.SearchAsync("giov");

        // Assert
        Assert.Single(result);
        Assert.Equal(patient.Id, result.First().Id);
    }

    [Fact]
    public async Task SearchAsync_FindsByLastName()
    {
        // Arrange
        var patient = CreateValidPatient("Test", "Ferrari");
        await _repository.AddAsync(patient);

        // Act
        var result = await _repository.SearchAsync("ferr");

        // Assert
        Assert.Single(result);
        Assert.Equal(patient.Id, result.First().Id);
    }

    [Fact]
    public async Task SearchAsync_IsCaseInsensitive()
    {
        // Arrange
        var patient = CreateValidPatient("Anna", "Bianchi");
        await _repository.AddAsync(patient);

        // Act
        var resultUpper = await _repository.SearchAsync("ANNA");
        var resultLower = await _repository.SearchAsync("anna");
        var resultMixed = await _repository.SearchAsync("AnNa");

        // Assert
        Assert.Single(resultUpper);
        Assert.Single(resultLower);
        Assert.Single(resultMixed);
    }

    [Fact]
    public async Task SearchAsync_EmptyTerm_ReturnsEmpty()
    {
        // Arrange
        var patient = CreateValidPatient("Test", "Patient");
        await _repository.AddAsync(patient);

        // Act
        var result = await _repository.SearchAsync("");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task SearchAsync_NoMatch_ReturnsEmpty()
    {
        // Arrange
        var patient = CreateValidPatient("Mario", "Rossi");
        await _repository.AddAsync(patient);

        // Act
        var result = await _repository.SearchAsync("nonexistent");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task AddAsync_AddsPatientSuccessfully()
    {
        // Arrange
        var patient = CreateValidPatient("New", "Patient");

        // Act
        await _repository.AddAsync(patient);
        var result = await _repository.GetByIdAsync(patient.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New", result.FirstName);
        Assert.Equal("Patient", result.LastName);
    }

    [Fact]
    public async Task AddAsync_SetsCreatedAt()
    {
        // Arrange
        var patient = CreateValidPatient("Test", "User");
        var beforeAdd = DateTime.Now.AddSeconds(-1);

        // Act
        await _repository.AddAsync(patient);
        var result = await _repository.GetByIdAsync(patient.Id);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.CreatedAt >= beforeAdd);
        Assert.Null(result.UpdatedAt);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesPatientSuccessfully()
    {
        // Arrange
        var patient = CreateValidPatient("Original", "Name");
        await _repository.AddAsync(patient);

        patient.FirstName = "Updated";
        patient.LastName = "Changed";

        // Act
        await _repository.UpdateAsync(patient);
        var result = await _repository.GetByIdAsync(patient.Id);

        // Assert
        Assert.Equal("Updated", result!.FirstName);
        Assert.Equal("Changed", result.LastName);
        Assert.NotNull(result.UpdatedAt);
    }

    [Fact]
    public async Task UpdateAsync_SetsUpdatedAt()
    {
        // Arrange
        var patient = CreateValidPatient("Test", "User");
        await _repository.AddAsync(patient);
        var beforeUpdate = DateTime.Now.AddSeconds(-1);

        patient.FirstName = "Modified";

        // Act
        await _repository.UpdateAsync(patient);
        var result = await _repository.GetByIdAsync(patient.Id);

        // Assert
        Assert.NotNull(result!.UpdatedAt);
        Assert.True(result.UpdatedAt >= beforeUpdate);
    }

    [Fact]
    public async Task UpdateAsync_NonExistentPatient_ThrowsException()
    {
        // Arrange
        var patient = CreateValidPatient("Test", "User");
        patient.Id = Guid.NewGuid(); // Non-existent

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _repository.UpdateAsync(patient));
    }

    [Fact]
    public async Task DeleteAsync_RemovesPatient()
    {
        // Arrange
        var patient = CreateValidPatient("To", "Delete");
        await _repository.AddAsync(patient);

        // Act
        var deleted = await _repository.DeleteAsync(patient.Id);
        var result = await _repository.GetByIdAsync(patient.Id);

        // Assert
        Assert.True(deleted);
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_NonExistentPatient_ReturnsFalse()
    {
        // Act
        var result = await _repository.DeleteAsync(Guid.NewGuid());

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAsync_CascadeDeletesTherapyProjects()
    {
        // Arrange
        var patient = CreateValidPatient("Test", "Patient");
        await _repository.AddAsync(patient);

        var project = new TherapyProjectModel
        {
            PatientId = patient.Id,
            Title = "Project to be deleted",
            StartDate = DateTime.Now,
            Status = "In Progress"
        };
        _context.TherapyProjects.Add(project);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(patient.Id);
        var projectResult = await _context.TherapyProjects.FindAsync(project.Id);

        // Assert
        Assert.Null(projectResult); // Progetto eliminato per cascade
    }

    [Fact]
    public async Task ExistsAsync_ExistingPatient_ReturnsTrue()
    {
        // Arrange
        var patient = CreateValidPatient("Exists", "Test");
        await _repository.AddAsync(patient);

        // Act
        var result = await _repository.ExistsAsync(patient.Id);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ExistsAsync_NonExistingPatient_ReturnsFalse()
    {
        // Act
        var result = await _repository.ExistsAsync(Guid.NewGuid());

        // Assert
        Assert.False(result);
    }

    private PatientModel CreateValidPatient(string firstName, string lastName)
    {
        return new PatientModel
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName
        };
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
