using Microsoft.EntityFrameworkCore;
using PTRP.Data;
using PTRP.Data.Repositories;
using PTRP.Models;

namespace PTRP.Tests.Repositories;

public class ScheduledVisitRepositoryTests : IDisposable
{
    private readonly PTRPDbContext _context;
    private readonly ScheduledVisitRepository _repository;
    private readonly PatientRepository _patientRepository;
    private readonly TherapyProjectRepository _projectRepository;

    public ScheduledVisitRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<PTRPDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        _context = new PTRPDbContext(options);
        _repository = new ScheduledVisitRepository(_context);
        _patientRepository = new PatientRepository(_context);
        _projectRepository = new TherapyProjectRepository(_context);
    }

    [Fact]
    public async Task GetByIdAsync_ValidId_ReturnsScheduledVisit()
    {
        // Arrange
        var visit = await CreateValidScheduledVisit();
        await _repository.AddAsync(visit);

        // Act
        var result = await _repository.GetByIdAsync(visit.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(visit.Id, result.Id);
    }

    [Fact]
    public async Task GetByProjectIdAsync_ReturnsAllVisitsForProject()
    {
        // Arrange
        var project = await CreateProjectWithVisits(3);

        // Act
        var result = (await _repository.GetByProjectIdAsync(project.Id)).ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.All(result, v => Assert.Equal(project.Id, v.TherapyProjectId));
    }

    [Fact]
    public async Task GetByEducatorIdInRangeAsync_ReturnsVisitsInDateRange()
    {
        // Arrange
        var educator = new ProfessionalEducatorModel
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "Educator"
        };
        _context.ProfessionalEducators.Add(educator);

        var patient = new PatientModel { FirstName = "Test", LastName = "Patient" };
        await _patientRepository.AddAsync(patient);

        var project = new TherapyProjectModel
        {
            PatientId = patient.Id,
            Title = "Test Project",
            StartDate = DateTime.Now,
            Status = "Active"
        };
        await _projectRepository.AddAsync(project);
        project.ProfessionalEducators.Add(educator);
        _context.TherapyProjects.Update(project);
        await _context.SaveChangesAsync();

        var visitType = new VisitTypeModel { Name = "INTAKE" };
        _context.VisitTypes.Add(visitType);
        await _context.SaveChangesAsync();

        var now = DateTime.Now;
        var visit1 = new ScheduledVisitModel
        {
            TherapyProjectId = project.Id,
            VisitTypeId = visitType.Id,
            ScheduledDate = now.AddDays(-1),
            AppointmentStatus = "Scheduled"
        };
        var visit2 = new ScheduledVisitModel
        {
            TherapyProjectId = project.Id,
            VisitTypeId = visitType.Id,
            ScheduledDate = now.AddDays(5),
            AppointmentStatus = "Scheduled"
        };

        await _repository.AddAsync(visit1);
        await _repository.AddAsync(visit2);

        // Act
        var fromDate = now.AddDays(-2);
        var toDate = now.AddDays(2);
        var result = (await _repository.GetByEducatorIdInRangeAsync(educator.Id, fromDate, toDate)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(visit1.Id, result[0].Id);
    }

    [Fact]
    public async Task GetByStatusAsync_ReturnsVisitsWithStatus()
    {
        // Arrange
        var project = await CreateProjectWithVisits(2);
        var visits = (await _repository.GetByProjectIdAsync(project.Id)).ToList();

        visits[0].AppointmentStatus = "Completed";
        visits[1].AppointmentStatus = "Scheduled";

        await _repository.UpdateAsync(visits[0]);
        await _repository.UpdateAsync(visits[1]);

        // Act
        var result = (await _repository.GetByStatusAsync("Completed")).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("Completed", result[0].AppointmentStatus);
    }

    [Fact]
    public async Task GetByDateAsync_ReturnsVisitsForSpecificDate()
    {
        // Arrange
        var targetDate = new DateTime(2025, 4, 2, 10, 30, 0);
        var visit = await CreateValidScheduledVisit();
        visit.ScheduledDate = targetDate;
        await _repository.AddAsync(visit);

        // Act
        var result = (await _repository.GetByDateAsync(targetDate.Date)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(visit.Id, result[0].Id);
    }

    [Fact]
    public async Task AddAsync_ValidVisit_SavesSuccessfully()
    {
        // Arrange
        var visit = await CreateValidScheduledVisit();

        // Act
        await _repository.AddAsync(visit);
        var result = await _repository.GetByIdAsync(visit.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(visit.TherapyProjectId, result.TherapyProjectId);
    }

    [Fact]
    public async Task AddAsync_NonExistentProject_ThrowsException()
    {
        // Arrange
        var visitType = new VisitTypeModel { Name = "INTAKE" };
        _context.VisitTypes.Add(visitType);
        await _context.SaveChangesAsync();

        var visit = new ScheduledVisitModel
        {
            TherapyProjectId = Guid.NewGuid(), // Non-existent
            VisitTypeId = visitType.Id,
            ScheduledDate = DateTime.Now,
            AppointmentStatus = "Scheduled"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _repository.AddAsync(visit));
    }

    [Fact]
    public async Task AddRangeAsync_AddsMultipleVisits()
    {
        // Arrange
        var project = await CreateValidProject();
        var visitType = new VisitTypeModel { Name = "INTAKE" };
        _context.VisitTypes.Add(visitType);
        await _context.SaveChangesAsync();

        var visits = new[]
        {
            new ScheduledVisitModel
            {
                TherapyProjectId = project.Id,
                VisitTypeId = visitType.Id,
                ScheduledDate = DateTime.Now.AddMonths(3),
                AppointmentStatus = "Scheduled"
            },
            new ScheduledVisitModel
            {
                TherapyProjectId = project.Id,
                VisitTypeId = visitType.Id,
                ScheduledDate = DateTime.Now.AddMonths(9),
                AppointmentStatus = "Scheduled"
            }
        };

        // Act
        await _repository.AddRangeAsync(visits);
        var results = (await _repository.GetByProjectIdAsync(project.Id)).ToList();

        // Assert
        Assert.Equal(2, results.Count);
    }

    [Fact]
    public async Task UpdateAsync_ModifiesVisitSuccessfully()
    {
        // Arrange
        var visit = await CreateValidScheduledVisit();
        await _repository.AddAsync(visit);

        visit.AppointmentStatus = "Completed";

        // Act
        await _repository.UpdateAsync(visit);
        var result = await _repository.GetByIdAsync(visit.Id);

        // Assert
        Assert.Equal("Completed", result!.AppointmentStatus);
    }

    [Fact]
    public async Task DeleteAsync_RemovesVisit()
    {
        // Arrange
        var visit = await CreateValidScheduledVisit();
        await _repository.AddAsync(visit);

        // Act
        var deleted = await _repository.DeleteAsync(visit.Id);
        var result = await _repository.GetByIdAsync(visit.Id);

        // Assert
        Assert.True(deleted);
        Assert.Null(result);
    }

    [Fact]
    public async Task ExistsAsync_ExistingVisit_ReturnsTrue()
    {
        // Arrange
        var visit = await CreateValidScheduledVisit();
        await _repository.AddAsync(visit);

        // Act
        var result = await _repository.ExistsAsync(visit.Id);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ExistsAsync_NonExistingVisit_ReturnsFalse()
    {
        // Act
        var result = await _repository.ExistsAsync(Guid.NewGuid());

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CountByProjectIdAsync_ReturnsCorrectCount()
    {
        // Arrange
        var project = await CreateProjectWithVisits(3);

        // Act
        var count = await _repository.CountByProjectIdAsync(project.Id);

        // Assert
        Assert.Equal(3, count);
    }

    private async Task<TherapyProjectModel> CreateProjectWithVisits(int visitCount)
    {
        var project = await CreateValidProject();
        var visitType = new VisitTypeModel { Name = "INTAKE" };
        _context.VisitTypes.Add(visitType);
        await _context.SaveChangesAsync();

        for (int i = 0; i < visitCount; i++)
        {
            var visit = new ScheduledVisitModel
            {
                TherapyProjectId = project.Id,
                VisitTypeId = visitType.Id,
                ScheduledDate = DateTime.Now.AddDays(i),
                AppointmentStatus = "Scheduled"
            };
            await _repository.AddAsync(visit);
        }

        return project;
    }

    private async Task<TherapyProjectModel> CreateValidProject()
    {
        var patient = new PatientModel { FirstName = "Test", LastName = "Patient" };
        await _patientRepository.AddAsync(patient);

        var project = new TherapyProjectModel
        {
            PatientId = patient.Id,
            Title = "Test Project",
            StartDate = DateTime.Now,
            Status = "Active"
        };
        await _projectRepository.AddAsync(project);

        return project;
    }

    private async Task<ScheduledVisitModel> CreateValidScheduledVisit()
    {
        var project = await CreateValidProject();
        var visitType = new VisitTypeModel { Name = "INTAKE" };
        _context.VisitTypes.Add(visitType);
        await _context.SaveChangesAsync();

        return new ScheduledVisitModel
        {
            TherapyProjectId = project.Id,
            VisitTypeId = visitType.Id,
            ScheduledDate = DateTime.Now.AddMonths(3),
            AppointmentStatus = "Scheduled"
        };
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
