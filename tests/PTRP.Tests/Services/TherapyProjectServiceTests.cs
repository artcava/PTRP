using Moq;
using Xunit;
using PTRP.Models;
using PTRP.Models.Enums;
using PTRP.Services;
using PTRP.Services.Configuration;
using PTRP.Services.Models;
using PTRP.Data.Repositories.Interfaces;

namespace PTRP.Tests.Services;

/// <summary>
/// Unit tests per TherapyProjectService.
/// Copre le regole di business critiche richieste da issue #73.
/// </summary>
public class TherapyProjectServiceTests
{
    private readonly Mock<ITherapyProjectRepository> _projectRepoMock;
    private readonly Mock<IPatientRepository> _patientRepoMock;
    private readonly Mock<IScheduledVisitRepository> _visitRepoMock;
    private readonly Mock<IEducatorRepository> _educatorRepoMock;
    private readonly TherapyProjectService _service;

    public TherapyProjectServiceTests()
    {
        _projectRepoMock = new Mock<ITherapyProjectRepository>();
        _patientRepoMock = new Mock<IPatientRepository>();
        _visitRepoMock = new Mock<IScheduledVisitRepository>();
        _educatorRepoMock = new Mock<IEducatorRepository>();

        _service = new TherapyProjectService(
            _projectRepoMock.Object,
            _patientRepoMock.Object,
            _visitRepoMock.Object,
            _educatorRepoMock.Object);
    }

    [Fact]
    public async Task CreateProjectAsync_WhenActiveProjectExists_ThrowsInvalidOperationException()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var existingProject = new TherapyProjectModel
        {
            Id = Guid.NewGuid(),
            PatientId = patientId,
            Status = nameof(TherapyProjectState.Active),
            Title = "Existing Active Project",
            StartDate = DateTime.Now.AddMonths(-6)
        };

        _patientRepoMock.Setup(r => r.ExistsAsync(patientId)).ReturnsAsync(true);
        _projectRepoMock.Setup(r => r.GetByPatientIdAsync(patientId))
            .ReturnsAsync(new List<TherapyProjectModel> { existingProject });

        var educatorId = Guid.NewGuid();
        _educatorRepoMock.Setup(r => r.ExistsAsync(educatorId)).ReturnsAsync(true);

        var request = new CreateProjectRequest(
            PatientId: patientId,
            Title: "New Project",
            Description: "Description",
            StartDate: DateTime.Now,
            PlannedEndDate: null,
            InitialState: TherapyProjectState.Active,
            EducatorIds: new List<Guid> { educatorId },
            GenerateCanonicalAppointments: false
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateProjectAsync(request));

        Assert.Contains("Active project already exists", exception.Message);
    }

    [Fact]
    public async Task CreateProjectAsync_WithValidData_CreatesProjectSuccessfully()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var educatorId = Guid.NewGuid();

        _patientRepoMock.Setup(r => r.ExistsAsync(patientId)).ReturnsAsync(true);
        _educatorRepoMock.Setup(r => r.ExistsAsync(educatorId)).ReturnsAsync(true);
        _projectRepoMock.Setup(r => r.GetByPatientIdAsync(patientId))
            .ReturnsAsync(new List<TherapyProjectModel>()); // No existing projects
        _projectRepoMock.Setup(r => r.AddAsync(It.IsAny<TherapyProjectModel>()))
            .Returns(Task.CompletedTask);
        _projectRepoMock.Setup(r => r.AssignEducatorAsync(It.IsAny<Guid>(), educatorId))
            .Returns(Task.CompletedTask);

        var request = new CreateProjectRequest(
            PatientId: patientId,
            Title: "Test Project",
            Description: "Test Description",
            StartDate: DateTime.Now,
            PlannedEndDate: DateTime.Now.AddMonths(12),
            InitialState: TherapyProjectState.Active,
            EducatorIds: new List<Guid> { educatorId },
            GenerateCanonicalAppointments: false
        );

        // Act
        var projectId = await _service.CreateProjectAsync(request);

        // Assert
        Assert.NotEqual(Guid.Empty, projectId);
        _projectRepoMock.Verify(r => r.AddAsync(It.Is<TherapyProjectModel>(
            p => p.Title == "Test Project" && p.Status == "Active")), Times.Once);
        _projectRepoMock.Verify(r => r.AssignEducatorAsync(It.IsAny<Guid>(), educatorId), Times.Once);
    }

    [Fact]
    public async Task CreateProjectAsync_WithCanonicalAppointments_Generates4Visits()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var educatorId = Guid.NewGuid();

        _patientRepoMock.Setup(r => r.ExistsAsync(patientId)).ReturnsAsync(true);
        _educatorRepoMock.Setup(r => r.ExistsAsync(educatorId)).ReturnsAsync(true);
        _projectRepoMock.Setup(r => r.GetByPatientIdAsync(patientId))
            .ReturnsAsync(new List<TherapyProjectModel>());
        _projectRepoMock.Setup(r => r.AddAsync(It.IsAny<TherapyProjectModel>()))
            .Returns(Task.CompletedTask);
        _projectRepoMock.Setup(r => r.AssignEducatorAsync(It.IsAny<Guid>(), educatorId))
            .Returns(Task.CompletedTask);

        List<ScheduledVisitModel>? capturedAppointments = null;
        _visitRepoMock.Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<ScheduledVisitModel>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<ScheduledVisitModel>, CancellationToken>((visits, _) => capturedAppointments = visits.ToList())
            .Returns(Task.CompletedTask);

        var startDate = new DateTime(2026, 1, 1);
        var request = new CreateProjectRequest(
            PatientId: patientId,
            Title: "Test Project",
            Description: null,
            StartDate: startDate,
            PlannedEndDate: null,
            InitialState: TherapyProjectState.Active,
            EducatorIds: new List<Guid> { educatorId },
            GenerateCanonicalAppointments: true
        );

        // Act
        await _service.CreateProjectAsync(request);

        // Assert
        Assert.NotNull(capturedAppointments);
        Assert.Equal(4, capturedAppointments.Count);

        // Verifica i 4 tipi canonici
        Assert.Contains(capturedAppointments, v => v.Type == VisitType.INTAKE);
        Assert.Contains(capturedAppointments, v => v.Type == VisitType.INTERMEDIATE);
        Assert.Contains(capturedAppointments, v => v.Type == VisitType.FINAL);
        Assert.Contains(capturedAppointments, v => v.Type == VisitType.DISCHARGE);

        // Verifica le date canoniche
        var intake = capturedAppointments.First(v => v.Type == VisitType.INTAKE);
        Assert.Equal(startDate.AddDays(90), intake.ScheduledDate); // +3 mesi

        var intermediate = capturedAppointments.First(v => v.Type == VisitType.INTERMEDIATE);
        Assert.Equal(intake.ScheduledDate.AddDays(180), intermediate.ScheduledDate); // +6 mesi da INTAKE

        var final = capturedAppointments.First(v => v.Type == VisitType.FINAL);
        Assert.Equal(intermediate.ScheduledDate.AddDays(180), final.ScheduledDate); // +6 mesi da INTERMEDIATE

        var discharge = capturedAppointments.First(v => v.Type == VisitType.DISCHARGE);
        Assert.Equal(final.ScheduledDate.AddDays(30), discharge.ScheduledDate); // +1 mese da FINAL

        // Verifica che tutti hanno Status = Scheduled
        Assert.All(capturedAppointments, v => Assert.Equal(AppointmentStatus.Scheduled, v.Status));
    }

    [Fact]
    public async Task GetActiveForPatientAsync_ReturnsAtMostOne Project()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var activeProject = new TherapyProjectModel
        {
            Id = Guid.NewGuid(),
            PatientId = patientId,
            Status = nameof(TherapyProjectState.Active),
            Title = "Active Project",
            StartDate = DateTime.Now
        };

        _patientRepoMock.Setup(r => r.ExistsAsync(patientId)).ReturnsAsync(true);
        _projectRepoMock.Setup(r => r.GetByPatientIdAsync(patientId))
            .ReturnsAsync(new List<TherapyProjectModel> { activeProject });

        // Act
        var result = await _service.GetActiveForPatientAsync(patientId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(activeProject.Id, result.Id);
        Assert.Equal(nameof(TherapyProjectState.Active), result.Status);
    }

    [Fact]
    public async Task ChangeProjectStateAsync_FromCompletedToActive_ThrowsException()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var project = new TherapyProjectModel
        {
            Id = projectId,
            PatientId = Guid.NewGuid(),
            Status = nameof(TherapyProjectState.Completed),
            Title = "Completed Project",
            StartDate = DateTime.Now.AddMonths(-12)
        };

        _projectRepoMock.Setup(r => r.GetByIdAsync(projectId)).ReturnsAsync(project);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.ChangeProjectStateAsync(projectId, TherapyProjectState.Active));

        Assert.Contains("final state", exception.Message);
    }

    [Fact]
    public async Task CreateProjectAsync_WithoutEducators_ThrowsArgumentException()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        _patientRepoMock.Setup(r => r.ExistsAsync(patientId)).ReturnsAsync(true);

        var request = new CreateProjectRequest(
            PatientId: patientId,
            Title: "Test Project",
            Description: null,
            StartDate: DateTime.Now,
            PlannedEndDate: null,
            InitialState: TherapyProjectState.Active,
            EducatorIds: new List<Guid>(), // Empty list
            GenerateCanonicalAppointments: false
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateProjectAsync(request));

        Assert.Contains("almeno un educatore", exception.Message);
    }

    [Fact]
    public async Task CreateProjectAsync_WithTitleLessThan3Chars_ThrowsArgumentException()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        _patientRepoMock.Setup(r => r.ExistsAsync(patientId)).ReturnsAsync(true);

        var request = new CreateProjectRequest(
            PatientId: patientId,
            Title: "AB", // Only 2 characters
            Description: null,
            StartDate: DateTime.Now,
            PlannedEndDate: null,
            InitialState: TherapyProjectState.Active,
            EducatorIds: new List<Guid> { Guid.NewGuid() },
            GenerateCanonicalAppointments: false
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateProjectAsync(request));

        Assert.Contains("almeno 3 caratteri", exception.Message);
    }

    [Fact]
    public async Task CreateProjectAsync_WithEndDateBeforeStartDate_ThrowsArgumentException()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        _patientRepoMock.Setup(r => r.ExistsAsync(patientId)).ReturnsAsync(true);

        var startDate = new DateTime(2026, 6, 1);
        var endDate = new DateTime(2026, 1, 1); // Before start

        var request = new CreateProjectRequest(
            PatientId: patientId,
            Title: "Test Project",
            Description: null,
            StartDate: startDate,
            PlannedEndDate: endDate,
            InitialState: TherapyProjectState.Active,
            EducatorIds: new List<Guid> { Guid.NewGuid() },
            GenerateCanonicalAppointments: false
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateProjectAsync(request));

        Assert.Contains("precedente alla data di inizio", exception.Message);
    }
}
