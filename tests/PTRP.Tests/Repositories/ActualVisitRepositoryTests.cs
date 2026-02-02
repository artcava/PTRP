using Microsoft.EntityFrameworkCore;
using PTRP.Data;
using PTRP.Data.Repositories;
using PTRP.Models;

namespace PTRP.Tests.Repositories;

public class ActualVisitRepositoryTests : IDisposable
{
    private readonly PTRPDbContext _context;
    private readonly ActualVisitRepository _repository;
    private readonly ScheduledVisitRepository _scheduledVisitRepository;
    private readonly PatientRepository _patientRepository;
    private readonly TherapyProjectRepository _projectRepository;

    public ActualVisitRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<PTRPDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        _context = new PTRPDbContext(options);
        _repository = new ActualVisitRepository(_context);
        _scheduledVisitRepository = new ScheduledVisitRepository(_context);
        _patientRepository = new PatientRepository(_context);
        _projectRepository = new TherapyProjectRepository(_context);
    }

    [Fact]
    public async Task GetByIdAsync_ValidId_ReturnsActualVisit()
    {
        // Arrange
        var actualVisit = await CreateValidActualVisit();
        await _repository.AddAsync(actualVisit);

        // Act
        var result = await _repository.GetByIdAsync(actualVisit.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(actualVisit.Id, result.Id);
    }

    [Fact]
    public async Task GetByScheduledVisitIdAsync_ReturnsActualVisitForScheduledVisit()
    {
        // Arrange
        var scheduledVisit = await CreateValidScheduledVisit();
        var actualVisit = await CreateValidActualVisit(scheduledVisit);
        await _repository.AddAsync(actualVisit);

        // Act
        var result = await _repository.GetByScheduledVisitIdAsync(scheduledVisit.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(actualVisit.Id, result.Id);
        Assert.Equal(scheduledVisit.Id, result.ScheduledVisitId);
    }

    [Fact]
    public async Task GetByScheduledVisitIdAsync_NoActualVisit_ReturnsNull()
    {
        // Arrange
        var scheduledVisit = await CreateValidScheduledVisit();

        // Act
        var result = await _repository.GetByScheduledVisitIdAsync(scheduledVisit.Id);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByEducatorIdInRangeAsync_ReturnsVisitsInRange()
    {
        // Arrange
        var educator = new ProfessionalEducatorModel
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "Educator"
        };
        _context.ProfessionalEducators.Add(educator);
        await _context.SaveChangesAsync();

        var (scheduledVisit, _) = await CreateScheduledAndActualVisit(educator);
        var actualVisit = await _repository.GetByScheduledVisitIdAsync(scheduledVisit.Id);

        var fromDate = actualVisit!.VisitDate.AddDays(-1);
        var toDate = actualVisit.VisitDate.AddDays(1);

        // Act
        var result = (await _repository.GetByEducatorIdInRangeAsync(educator.Id, fromDate, toDate)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(actualVisit.Id, result[0].Id);
    }

    [Fact]
    public async Task GetByProjectIdAsync_ReturnsAllVisitsForProject()
    {
        // Arrange
        var project = await CreateValidProject();
        var educator = CreateEducator();
        _context.ProfessionalEducators.Add(educator);
        await _context.SaveChangesAsync();

        var scheduledVisit1 = await CreateScheduledVisit(project);
        var scheduledVisit2 = await CreateScheduledVisit(project);

        var actualVisit1 = new ActualVisitModel
        {
            ScheduledVisitId = scheduledVisit1.Id,
            VisitDate = DateTime.Now,
            ClinicalNotes = "Test notes",
            PatientAttendance = "Attended",
            VisitSource = "EducatorImport"
        };

        var actualVisit2 = new ActualVisitModel
        {
            ScheduledVisitId = scheduledVisit2.Id,
            VisitDate = DateTime.Now.AddDays(1),
            ClinicalNotes = "Test notes 2",
            PatientAttendance = "Attended",
            VisitSource = "EducatorImport"
        };

        var visitOperator = new VisitOperatorModel
        {
            ActualVisitId = actualVisit1.Id,
            OperatorId = educator.Id,
            Role = "Lead"
        };
        actualVisit1.VisitOperators.Add(visitOperator);

        await _repository.AddAsync(actualVisit1);
        await _repository.AddAsync(actualVisit2);

        // Act
        var result = (await _repository.GetByProjectIdAsync(project.Id)).ToList();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetByPatientIdAsync_ReturnsAllVisitsForPatient()
    {
        // Arrange
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

        var scheduledVisit = await CreateScheduledVisit(project);
        var actualVisit = new ActualVisitModel
        {
            ScheduledVisitId = scheduledVisit.Id,
            VisitDate = DateTime.Now,
            ClinicalNotes = "Test",
            PatientAttendance = "Attended",
            VisitSource = "EducatorImport"
        };
        await _repository.AddAsync(actualVisit);

        // Act
        var result = (await _repository.GetByPatientIdAsync(patient.Id)).ToList();

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public async Task GetByPatientAttendanceAsync_FiltersCorrectly()
    {
        // Arrange
        var actualVisit = await CreateValidActualVisit();
        actualVisit.PatientAttendance = "Attended";
        await _repository.AddAsync(actualVisit);

        // Act
        var result = (await _repository.GetByPatientAttendanceAsync("Attended")).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("Attended", result[0].PatientAttendance);
    }

    [Fact]
    public async Task AddAsync_ValidActualVisit_SavesSuccessfully()
    {
        // Arrange
        var actualVisit = await CreateValidActualVisit();

        // Act
        await _repository.AddAsync(actualVisit);
        var result = await _repository.GetByIdAsync(actualVisit.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(actualVisit.ScheduledVisitId, result.ScheduledVisitId);
    }

    [Fact]
    public async Task AddAsync_NonExistentScheduledVisit_ThrowsException()
    {
        // Arrange
        var actualVisit = new ActualVisitModel
        {
            ScheduledVisitId = Guid.NewGuid(), // Non-existent
            VisitDate = DateTime.Now,
            ClinicalNotes = "Test",
            PatientAttendance = "Attended",
            VisitSource = "EducatorImport"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _repository.AddAsync(actualVisit));
    }

    [Fact]
    public async Task AddAsync_ScheduledVisitAlreadyHasActualVisit_ThrowsException()
    {
        // Arrange - Create first actual visit
        var actualVisit1 = await CreateValidActualVisit();
        await _repository.AddAsync(actualVisit1);

        var scheduledVisitId = actualVisit1.ScheduledVisitId;

        // Arrange - Try to create second actual visit for same scheduled visit
        var actualVisit2 = new ActualVisitModel
        {
            ScheduledVisitId = scheduledVisitId,
            VisitDate = DateTime.Now.AddDays(1),
            ClinicalNotes = "Second visit",
            PatientAttendance = "Attended",
            VisitSource = "EducatorImport"
        };

        // Act & Assert - Should throw due to 1:1 constraint violation
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _repository.AddAsync(actualVisit2));
    }

    [Fact]
    public async Task UpdateAsync_ModifiesVisitSuccessfully()
    {
        // Arrange
        var actualVisit = await CreateValidActualVisit();
        await _repository.AddAsync(actualVisit);

        actualVisit.ClinicalNotes = "Updated notes";

        // Act
        await _repository.UpdateAsync(actualVisit);
        var result = await _repository.GetByIdAsync(actualVisit.Id);

        // Assert
        Assert.Equal("Updated notes", result!.ClinicalNotes);
    }

    [Fact]
    public async Task UpdateAsync_CannotChangeScheduledVisit_ThrowsException()
    {
        // Arrange
        var actualVisit = await CreateValidActualVisit();
        await _repository.AddAsync(actualVisit);

        var newScheduledVisit = await CreateValidScheduledVisit();
        actualVisit.ScheduledVisitId = newScheduledVisit.Id; // Try to change

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _repository.UpdateAsync(actualVisit));
    }

    [Fact]
    public async Task DeleteAsync_RemovesVisit()
    {
        // Arrange
        var actualVisit = await CreateValidActualVisit();
        await _repository.AddAsync(actualVisit);

        // Act
        var deleted = await _repository.DeleteAsync(actualVisit.Id);
        var result = await _repository.GetByIdAsync(actualVisit.Id);

        // Assert
        Assert.True(deleted);
        Assert.Null(result);
    }

    [Fact]
    public async Task ExistsAsync_ExistingVisit_ReturnsTrue()
    {
        // Arrange
        var actualVisit = await CreateValidActualVisit();
        await _repository.AddAsync(actualVisit);

        // Act
        var result = await _repository.ExistsAsync(actualVisit.Id);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ExistsByScheduledVisitIdAsync_ReturnsTrue_WhenActualVisitExists()
    {
        // Arrange
        var actualVisit = await CreateValidActualVisit();
        await _repository.AddAsync(actualVisit);

        // Act
        var result = await _repository.ExistsByScheduledVisitIdAsync(actualVisit.ScheduledVisitId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ExistsByScheduledVisitIdAsync_ReturnsFalse_WhenNoActualVisit()
    {
        // Arrange
        var scheduledVisit = await CreateValidScheduledVisit();

        // Act
        var result = await _repository.ExistsByScheduledVisitIdAsync(scheduledVisit.Id);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CountByProjectIdAsync_ReturnsCorrectCount()
    {
        // Arrange
        var project = await CreateValidProject();
        var scheduledVisit1 = await CreateScheduledVisit(project);
        var scheduledVisit2 = await CreateScheduledVisit(project);

        var actualVisit1 = new ActualVisitModel
        {
            ScheduledVisitId = scheduledVisit1.Id,
            VisitDate = DateTime.Now,
            ClinicalNotes = "Test",
            PatientAttendance = "Attended",
            VisitSource = "EducatorImport"
        };

        var actualVisit2 = new ActualVisitModel
        {
            ScheduledVisitId = scheduledVisit2.Id,
            VisitDate = DateTime.Now,
            ClinicalNotes = "Test",
            PatientAttendance = "Attended",
            VisitSource = "EducatorImport"
        };

        await _repository.AddAsync(actualVisit1);
        await _repository.AddAsync(actualVisit2);

        // Act
        var count = await _repository.CountByProjectIdAsync(project.Id);

        // Assert
        Assert.Equal(2, count);
    }

    private async Task<ActualVisitModel> CreateValidActualVisit(
        ScheduledVisitModel? scheduledVisit = null)
    {
        scheduledVisit ??= await CreateValidScheduledVisit();

        return new ActualVisitModel
        {
            ScheduledVisitId = scheduledVisit.Id,
            VisitDate = DateTime.Now,
            ClinicalNotes = "Test clinical notes",
            PatientAttendance = "Attended",
            VisitSource = "EducatorImport"
        };
    }

    private async Task<ScheduledVisitModel> CreateValidScheduledVisit()
    {
        var project = await CreateValidProject();
        return await CreateScheduledVisit(project);
    }

    private async Task<ScheduledVisitModel> CreateScheduledVisit(TherapyProjectModel project)
    {
        var visitType = new VisitTypeModel { Name = "INTAKE" };
        _context.VisitTypes.Add(visitType);
        await _context.SaveChangesAsync();

        var visit = new ScheduledVisitModel
        {
            TherapyProjectId = project.Id,
            VisitTypeId = visitType.Id,
            ScheduledDate = DateTime.Now.AddMonths(3),
            AppointmentStatus = "Scheduled"
        };
        await _scheduledVisitRepository.AddAsync(visit);

        return visit;
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

    private async Task<(ScheduledVisitModel, ActualVisitModel)> CreateScheduledAndActualVisit(
        ProfessionalEducatorModel educator)
    {
        var project = await CreateValidProject();
        project.ProfessionalEducators.Add(educator);
        _context.TherapyProjects.Update(project);
        await _context.SaveChangesAsync();

        var scheduledVisit = await CreateScheduledVisit(project);

        var actualVisit = new ActualVisitModel
        {
            ScheduledVisitId = scheduledVisit.Id,
            VisitDate = DateTime.Now,
            ClinicalNotes = "Test",
            PatientAttendance = "Attended",
            VisitSource = "EducatorImport"
        };

        var visitOperator = new VisitOperatorModel
        {
            OperatorId = educator.Id,
            Role = "Lead"
        };
        actualVisit.VisitOperators.Add(visitOperator);

        await _repository.AddAsync(actualVisit);

        return (scheduledVisit, actualVisit);
    }

    private ProfessionalEducatorModel CreateEducator()
    {
        return new ProfessionalEducatorModel
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe"
        };
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
