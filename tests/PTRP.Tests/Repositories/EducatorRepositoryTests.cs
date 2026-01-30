using Microsoft.EntityFrameworkCore;
using PTRP.Data;
using PTRP.Data.Repositories;
using PTRP.Models;

namespace PTRP.Tests.Repositories;

public class EducatorRepositoryTests : IDisposable
{
    private readonly PTRPDbContext _context;
    private readonly EducatorRepository _repository;

    public EducatorRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<PTRPDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        _context = new PTRPDbContext(options);
        _repository = new EducatorRepository(_context);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllEducators_OrderedByLastName()
    {
        // Arrange
        var educator1 = CreateValidEducator("Mario", "Rossi");
        var educator2 = CreateValidEducator("Luigi", "Bianchi");
        await _repository.AddAsync(educator1);
        await _repository.AddAsync(educator2);

        // Act
        var result = (await _repository.GetAllAsync()).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Bianchi", result[0].LastName); // Alfabeticamente prima
        Assert.Equal("Rossi", result[1].LastName);
    }

    [Fact]
    public async Task GetByIdAsync_ValidId_ReturnsEducator()
    {
        // Arrange
        var educator = CreateValidEducator("Test", "Educator");
        await _repository.AddAsync(educator);

        // Act
        var result = await _repository.GetByIdAsync(educator.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(educator.Id, result.Id);
        Assert.Equal(educator.Email, result.Email);
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
    public async Task GetByStatusAsync_ReturnsEducatorsWithStatus()
    {
        // Arrange
        var active = CreateValidEducator("Active", "One");
        active.Status = "Active";
        var inactive = CreateValidEducator("Inactive", "Two");
        inactive.Status = "Inactive";
        await _repository.AddAsync(active);
        await _repository.AddAsync(inactive);

        // Act
        var result = await _repository.GetByStatusAsync("Active");

        // Assert
        Assert.Single(result);
        Assert.Equal(active.Id, result.First().Id);
    }

    [Fact]
    public async Task GetBySpecializationAsync_ReturnsEducatorsWithSpecialization()
    {
        // Arrange
        var psychologist = CreateValidEducator("Test", "Psy");
        psychologist.Specialization = "Psicologo";
        var physiotherapist = CreateValidEducator("Test", "Physio");
        physiotherapist.Specialization = "Fisioterapista";
        await _repository.AddAsync(psychologist);
        await _repository.AddAsync(physiotherapist);

        // Act
        var result = await _repository.GetBySpecializationAsync("Psicologo");

        // Assert
        Assert.Single(result);
        Assert.Equal(psychologist.Id, result.First().Id);
    }

    [Fact]
    public async Task SearchAsync_FindsByFirstName()
    {
        // Arrange
        var educator = CreateValidEducator("Giovanni", "Verdi");
        await _repository.AddAsync(educator);

        // Act
        var result = await _repository.SearchAsync("giov");

        // Assert
        Assert.Single(result);
        Assert.Equal(educator.Id, result.First().Id);
    }

    [Fact]
    public async Task SearchAsync_FindsByLastName()
    {
        // Arrange
        var educator = CreateValidEducator("Test", "Ferrari");
        await _repository.AddAsync(educator);

        // Act
        var result = await _repository.SearchAsync("ferr");

        // Assert
        Assert.Single(result);
        Assert.Equal(educator.Id, result.First().Id);
    }

    [Fact]
    public async Task SearchAsync_FindsByEmail()
    {
        // Arrange
        var educator = CreateValidEducator("Test", "User");
        educator.Email = "unique@test.com";
        await _repository.AddAsync(educator);

        // Act
        var result = await _repository.SearchAsync("unique");

        // Assert
        Assert.Single(result);
        Assert.Equal(educator.Id, result.First().Id);
    }

    [Fact]
    public async Task SearchAsync_EmptyTerm_ReturnsEmpty()
    {
        // Act
        var result = await _repository.SearchAsync("");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task AddAsync_AddsEducatorSuccessfully()
    {
        // Arrange
        var educator = CreateValidEducator("New", "Educator");

        // Act
        await _repository.AddAsync(educator);
        var result = await _repository.GetByIdAsync(educator.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New", result.FirstName);
    }

    [Fact]
    public async Task AddAsync_DuplicateEmail_ThrowsException()
    {
        // Arrange
        var educator1 = CreateValidEducator("First", "User");
        educator1.Email = "duplicate@test.com";
        var educator2 = CreateValidEducator("Second", "User");
        educator2.Email = "duplicate@test.com";

        await _repository.AddAsync(educator1);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _repository.AddAsync(educator2));
    }

    [Fact]
    public async Task UpdateAsync_UpdatesEducatorSuccessfully()
    {
        // Arrange
        var educator = CreateValidEducator("Original", "Name");
        await _repository.AddAsync(educator);

        educator.FirstName = "Updated";
        educator.Status = "Inactive";

        // Act
        await _repository.UpdateAsync(educator);
        var result = await _repository.GetByIdAsync(educator.Id);

        // Assert
        Assert.Equal("Updated", result!.FirstName);
        Assert.Equal("Inactive", result.Status);
        Assert.NotNull(result.UpdatedAt);
    }

    [Fact]
    public async Task UpdateAsync_NonExistentEducator_ThrowsException()
    {
        // Arrange
        var educator = CreateValidEducator("Test", "User");
        educator.Id = Guid.NewGuid(); // Non-existent

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _repository.UpdateAsync(educator));
    }

    [Fact]
    public async Task UpdateAsync_DuplicateEmail_ThrowsException()
    {
        // Arrange
        var educator1 = CreateValidEducator("First", "User");
        educator1.Email = "first@test.com";
        var educator2 = CreateValidEducator("Second", "User");
        educator2.Email = "second@test.com";

        await _repository.AddAsync(educator1);
        await _repository.AddAsync(educator2);

        educator2.Email = "first@test.com"; // Duplicate

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _repository.UpdateAsync(educator2));
    }

    [Fact]
    public async Task DeleteAsync_RemovesEducator()
    {
        // Arrange
        var educator = CreateValidEducator("To", "Delete");
        await _repository.AddAsync(educator);

        // Act
        var deleted = await _repository.DeleteAsync(educator.Id);
        var result = await _repository.GetByIdAsync(educator.Id);

        // Assert
        Assert.True(deleted);
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_NonExistentEducator_ReturnsFalse()
    {
        // Act
        var result = await _repository.DeleteAsync(Guid.NewGuid());

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ExistsAsync_ExistingEducator_ReturnsTrue()
    {
        // Arrange
        var educator = CreateValidEducator("Exists", "Test");
        await _repository.AddAsync(educator);

        // Act
        var result = await _repository.ExistsAsync(educator.Id);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ExistsAsync_NonExistingEducator_ReturnsFalse()
    {
        // Act
        var result = await _repository.ExistsAsync(Guid.NewGuid());

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task EmailExistsAsync_ExistingEmail_ReturnsTrue()
    {
        // Arrange
        var educator = CreateValidEducator("Test", "User");
        educator.Email = "exists@test.com";
        await _repository.AddAsync(educator);

        // Act
        var result = await _repository.EmailExistsAsync("exists@test.com");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task EmailExistsAsync_NonExistingEmail_ReturnsFalse()
    {
        // Act
        var result = await _repository.EmailExistsAsync("notexists@test.com");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task EmailExistsAsync_WithExcludeId_ExcludesSpecifiedEducator()
    {
        // Arrange
        var educator = CreateValidEducator("Test", "User");
        educator.Email = "test@example.com";
        await _repository.AddAsync(educator);

        // Act - escludiamo l'educatore con quell'ID
        var result = await _repository.EmailExistsAsync("test@example.com", educator.Id);

        // Assert
        Assert.False(result); // Non dovrebbe trovarlo perché è escluso
    }

    [Fact]
    public async Task GetUniqueSpecializationsAsync_ReturnsDistinctSpecializations()
    {
        // Arrange
        var edu1 = CreateValidEducator("Test1", "User1");
        edu1.Specialization = "Psicologo";
        var edu2 = CreateValidEducator("Test2", "User2");
        edu2.Specialization = "Psicologo";
        var edu3 = CreateValidEducator("Test3", "User3");
        edu3.Specialization = "Fisioterapista";

        await _repository.AddAsync(edu1);
        await _repository.AddAsync(edu2);
        await _repository.AddAsync(edu3);

        // Act
        var result = (await _repository.GetUniqueSpecializationsAsync()).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains("Psicologo", result);
        Assert.Contains("Fisioterapista", result);
    }

    private ProfessionalEducatorModel CreateValidEducator(string firstName, string lastName)
    {
        return new ProfessionalEducatorModel
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Email = $"{firstName.ToLower()}.{lastName.ToLower()}@test.com",
            PhoneNumber = "+39 333 1234567",
            DateOfBirth = new DateTime(1985, 5, 15),
            Specialization = "Educatore Professionale",
            LicenseNumber = $"EP{Guid.NewGuid().ToString()[..8]}",
            HireDate = new DateTime(2020, 1, 1),
            Status = "Active"
        };
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
