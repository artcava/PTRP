using Moq;
using Xunit;
using PTRP.Models;
using PTRP.Models.Enums;
using PTRP.Services.Interfaces;
using PTRP.Services.Models;
using PTRP.ViewModels.Projects;

namespace PTRP.Tests.ViewModels.Projects;

/// <summary>
/// Unit tests per ProjectFormViewModel.
/// Copre i requisiti della issue #74: validazione, creazione progetto, e gestione educatori.
/// </summary>
public class ProjectFormViewModelTests
{
    private readonly Mock<ITherapyProjectService> _projectServiceMock;
    private readonly Mock<IEducatorService> _educatorServiceMock;
    private readonly Guid _patientId;
    private readonly string _patientFullName;

    public ProjectFormViewModelTests()
    {
        _projectServiceMock = new Mock<ITherapyProjectService>();
        _educatorServiceMock = new Mock<IEducatorService>();
        _patientId = Guid.NewGuid();
        _patientFullName = "Rossi Mario";
    }

    private ProjectFormViewModel CreateViewModel()
    {
        return new ProjectFormViewModel(
            _projectServiceMock.Object,
            _educatorServiceMock.Object,
            _patientId,
            _patientFullName);
    }

    #region Initialization Tests

    [Fact]
    public void Constructor_InitializesPropertiesCorrectly()
    {
        // Act
        var viewModel = CreateViewModel();

        // Assert
        Assert.Equal(_patientFullName, viewModel.PatientFullName);
        Assert.Equal(string.Empty, viewModel.Title);
        Assert.Equal(string.Empty, viewModel.Description);
        Assert.Equal(DateTime.Today, viewModel.StartDate);
        Assert.Null(viewModel.PlannedEndDate);
        Assert.Equal(TherapyProjectState.Active, viewModel.SelectedProjectState);
        Assert.True(viewModel.GenerateCanonicalAppointments);
        Assert.False(viewModel.IsLoading);
        Assert.False(viewModel.IsSaving);
        Assert.Null(viewModel.ErrorMessage);
        Assert.Null(viewModel.SuccessMessage);
    }

    [Fact]
    public void Constructor_WithNullServices_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new ProjectFormViewModel(null!, _educatorServiceMock.Object, _patientId, _patientFullName));
        
        Assert.Throws<ArgumentNullException>(() => 
            new ProjectFormViewModel(_projectServiceMock.Object, null!, _patientId, _patientFullName));
    }

    #endregion

    #region LoadEducators Tests

    [Fact]
    public async Task LoadEducatorsAsync_LoadsAndSortsEducators()
    {
        // Arrange
        var educators = new List<EducatorModel>
        {
            new EducatorModel 
            { 
                Id = Guid.NewGuid(), 
                FirstName = "Marco", 
                LastName = "Bianchi",
                Role = "Educatore Professionale"
            },
            new EducatorModel 
            { 
                Id = Guid.NewGuid(), 
                FirstName = "Luca", 
                LastName = "Verdi",
                Role = "Educatore Senior"
            },
            new EducatorModel 
            { 
                Id = Guid.NewGuid(), 
                FirstName = "Sara", 
                LastName = "Neri",
                Role = null // Test null role
            }
        };

        _educatorServiceMock.Setup(s => s.GetAllAsync())
            .ReturnsAsync(educators);

        var viewModel = CreateViewModel();

        // Act
        await viewModel.LoadEducatorsAsync();

        // Assert
        Assert.False(viewModel.IsLoading);
        Assert.Null(viewModel.ErrorMessage);
        Assert.Equal(3, viewModel.AvailableEducators.Count);
        
        // Verifica ordinamento per cognome
        Assert.Equal("Bianchi Marco", viewModel.AvailableEducators[0].Name);
        Assert.Equal("Neri Sara", viewModel.AvailableEducators[1].Name);
        Assert.Equal("Verdi Luca", viewModel.AvailableEducators[2].Name);
        
        // Verifica role di default per null
        Assert.Equal("Educatore Professionale", viewModel.AvailableEducators[2].Role);
    }

    [Fact]
    public async Task LoadEducatorsAsync_OnException_SetsErrorMessage()
    {
        // Arrange
        _educatorServiceMock.Setup(s => s.GetAllAsync())
            .ThrowsAsync(new Exception("Database error"));

        var viewModel = CreateViewModel();

        // Act
        await viewModel.LoadEducatorsAsync();

        // Assert
        Assert.False(viewModel.IsLoading);
        Assert.NotNull(viewModel.ErrorMessage);
        Assert.Contains("Errore nel caricamento degli educatori", viewModel.ErrorMessage);
        Assert.Contains("Database error", viewModel.ErrorMessage);
    }

    #endregion

    #region CanSave Validation Tests

    [Fact]
    public void CanSave_WithEmptyTitle_ReturnsFalse()
    {
        // Arrange
        var viewModel = CreateViewModel();
        viewModel.AvailableEducators.Add(new SelectableEducatorViewModel(Guid.NewGuid(), "Test", "Role") { IsSelected = true });

        // Act & Assert
        Assert.False(viewModel.CanSave);
    }

    [Fact]
    public void CanSave_WithTitleLessThan3Chars_ReturnsFalse()
    {
        // Arrange
        var viewModel = CreateViewModel();
        viewModel.Title = "AB";
        viewModel.AvailableEducators.Add(new SelectableEducatorViewModel(Guid.NewGuid(), "Test", "Role") { IsSelected = true });

        // Act & Assert
        Assert.False(viewModel.CanSave);
    }

    [Fact]
    public void CanSave_WithNoEducatorSelected_ReturnsFalse()
    {
        // Arrange
        var viewModel = CreateViewModel();
        viewModel.Title = "Valid Title";
        viewModel.AvailableEducators.Add(new SelectableEducatorViewModel(Guid.NewGuid(), "Test", "Role") { IsSelected = false });

        // Act & Assert
        Assert.False(viewModel.CanSave);
    }

    [Fact]
    public void CanSave_WithValidData_ReturnsTrue()
    {
        // Arrange
        var viewModel = CreateViewModel();
        viewModel.Title = "Valid Title";
        viewModel.AvailableEducators.Add(new SelectableEducatorViewModel(Guid.NewGuid(), "Test", "Role") { IsSelected = true });

        // Act & Assert
        Assert.True(viewModel.CanSave);
    }

    [Fact]
    public void CanSave_WhileSaving_ReturnsFalse()
    {
        // Arrange
        var viewModel = CreateViewModel();
        viewModel.Title = "Valid Title";
        viewModel.AvailableEducators.Add(new SelectableEducatorViewModel(Guid.NewGuid(), "Test", "Role") { IsSelected = true });
        
        // Use reflection to set IsSaving (since it's private set)
        var isSavingProperty = typeof(ProjectFormViewModel).GetProperty("IsSaving");
        isSavingProperty?.SetValue(viewModel, true);

        // Act & Assert
        Assert.False(viewModel.CanSave);
    }

    #endregion

    #region SaveAsync Tests

    [Fact]
    public async Task SaveAsync_WithValidData_CreatesProjectSuccessfully()
    {
        // Arrange
        var educatorId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        
        _projectServiceMock.Setup(s => s.GetActiveForPatientAsync(_patientId))
            .ReturnsAsync((TherapyProjectModel?)null);
        
        _projectServiceMock.Setup(s => s.CreateProjectAsync(It.IsAny<CreateProjectRequest>()))
            .ReturnsAsync(projectId);

        var viewModel = CreateViewModel();
        viewModel.Title = "Test Project";
        viewModel.Description = "Test Description";
        viewModel.StartDate = new DateTime(2026, 1, 1);
        viewModel.PlannedEndDate = new DateTime(2026, 12, 31);
        viewModel.GenerateCanonicalAppointments = true;
        viewModel.AvailableEducators.Add(new SelectableEducatorViewModel(educatorId, "Educator", "Role") { IsSelected = true });

        Guid? capturedProjectId = null;
        viewModel.OnProjectCreated += (id) => capturedProjectId = id;

        // Act
        await viewModel.SaveCommand.ExecuteAsync(null);

        // Assert
        Assert.Null(viewModel.ErrorMessage);
        Assert.NotNull(viewModel.SuccessMessage);
        Assert.Contains("successo", viewModel.SuccessMessage);
        Assert.Equal(projectId, capturedProjectId);
        
        _projectServiceMock.Verify(s => s.CreateProjectAsync(It.Is<CreateProjectRequest>(
            r => r.PatientId == _patientId &&
                 r.Title == "Test Project" &&
                 r.Description == "Test Description" &&
                 r.StartDate == new DateTime(2026, 1, 1) &&
                 r.PlannedEndDate == new DateTime(2026, 12, 31) &&
                 r.InitialState == TherapyProjectState.Active &&
                 r.EducatorIds.Contains(educatorId) &&
                 r.GenerateCanonicalAppointments == true
        )), Times.Once);
    }

    [Fact]
    public async Task SaveAsync_WhenActiveProjectExists_ShowsErrorMessage()
    {
        // Arrange
        var existingProject = new TherapyProjectModel
        {
            Id = Guid.NewGuid(),
            PatientId = _patientId,
            Status = nameof(TherapyProjectState.Active),
            Title = "Existing Project",
            StartDate = DateTime.Now.AddMonths(-6)
        };

        _projectServiceMock.Setup(s => s.GetActiveForPatientAsync(_patientId))
            .ReturnsAsync(existingProject);

        var viewModel = CreateViewModel();
        viewModel.Title = "New Project";
        viewModel.AvailableEducators.Add(new SelectableEducatorViewModel(Guid.NewGuid(), "Test", "Role") { IsSelected = true });

        // Act
        await viewModel.SaveCommand.ExecuteAsync(null);

        // Assert
        Assert.NotNull(viewModel.ErrorMessage);
        Assert.Contains("ATTENZIONE", viewModel.ErrorMessage);
        Assert.Contains("progetto attivo", viewModel.ErrorMessage);
        Assert.Null(viewModel.SuccessMessage);
        
        // Verifica che NON sia stato chiamato CreateProjectAsync
        _projectServiceMock.Verify(s => s.CreateProjectAsync(It.IsAny<CreateProjectRequest>()), Times.Never);
    }

    [Fact]
    public async Task SaveAsync_WithEmptyTitle_ShowsValidationError()
    {
        // Arrange
        var viewModel = CreateViewModel();
        viewModel.Title = "";
        viewModel.AvailableEducators.Add(new SelectableEducatorViewModel(Guid.NewGuid(), "Test", "Role") { IsSelected = true });

        // Act
        await viewModel.SaveCommand.ExecuteAsync(null);

        // Assert
        Assert.NotNull(viewModel.ErrorMessage);
        Assert.Contains("obbligatorio", viewModel.ErrorMessage);
        _projectServiceMock.Verify(s => s.CreateProjectAsync(It.IsAny<CreateProjectRequest>()), Times.Never);
    }

    [Fact]
    public async Task SaveAsync_WithTitleLessThan3Chars_ShowsValidationError()
    {
        // Arrange
        var viewModel = CreateViewModel();
        viewModel.Title = "AB";
        viewModel.AvailableEducators.Add(new SelectableEducatorViewModel(Guid.NewGuid(), "Test", "Role") { IsSelected = true });

        // Act
        await viewModel.SaveCommand.ExecuteAsync(null);

        // Assert
        Assert.NotNull(viewModel.ErrorMessage);
        Assert.Contains("almeno 3 caratteri", viewModel.ErrorMessage);
    }

    [Fact]
    public async Task SaveAsync_WithEndDateBeforeStartDate_ShowsValidationError()
    {
        // Arrange
        var viewModel = CreateViewModel();
        viewModel.Title = "Valid Title";
        viewModel.StartDate = new DateTime(2026, 6, 1);
        viewModel.PlannedEndDate = new DateTime(2026, 1, 1);
        viewModel.AvailableEducators.Add(new SelectableEducatorViewModel(Guid.NewGuid(), "Test", "Role") { IsSelected = true });

        // Act
        await viewModel.SaveCommand.ExecuteAsync(null);

        // Assert
        Assert.NotNull(viewModel.ErrorMessage);
        Assert.Contains("precedente alla data di inizio", viewModel.ErrorMessage);
    }

    [Fact]
    public async Task SaveAsync_WithNoEducatorsSelected_ShowsValidationError()
    {
        // Arrange
        var viewModel = CreateViewModel();
        viewModel.Title = "Valid Title";
        viewModel.AvailableEducators.Add(new SelectableEducatorViewModel(Guid.NewGuid(), "Test", "Role") { IsSelected = false });

        // Act
        await viewModel.SaveCommand.ExecuteAsync(null);

        // Assert
        Assert.NotNull(viewModel.ErrorMessage);
        Assert.Contains("almeno un educatore", viewModel.ErrorMessage);
    }

    [Fact]
    public async Task SaveAsync_OnServiceException_ShowsErrorMessage()
    {
        // Arrange
        _projectServiceMock.Setup(s => s.GetActiveForPatientAsync(_patientId))
            .ReturnsAsync((TherapyProjectModel?)null);
        
        _projectServiceMock.Setup(s => s.CreateProjectAsync(It.IsAny<CreateProjectRequest>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        var viewModel = CreateViewModel();
        viewModel.Title = "Test Project";
        viewModel.AvailableEducators.Add(new SelectableEducatorViewModel(Guid.NewGuid(), "Test", "Role") { IsSelected = true });

        // Act
        await viewModel.SaveCommand.ExecuteAsync(null);

        // Assert
        Assert.NotNull(viewModel.ErrorMessage);
        Assert.Contains("Errore nella creazione del progetto", viewModel.ErrorMessage);
        Assert.Contains("Database connection failed", viewModel.ErrorMessage);
    }

    [Fact]
    public async Task SaveAsync_WithInvalidOperationException_ShowsSpecificError()
    {
        // Arrange
        _projectServiceMock.Setup(s => s.GetActiveForPatientAsync(_patientId))
            .ReturnsAsync((TherapyProjectModel?)null);
        
        _projectServiceMock.Setup(s => s.CreateProjectAsync(It.IsAny<CreateProjectRequest>()))
            .ThrowsAsync(new InvalidOperationException("Invalid operation"));

        var viewModel = CreateViewModel();
        viewModel.Title = "Test Project";
        viewModel.AvailableEducators.Add(new SelectableEducatorViewModel(Guid.NewGuid(), "Test", "Role") { IsSelected = true });

        // Act
        await viewModel.SaveCommand.ExecuteAsync(null);

        // Assert
        Assert.NotNull(viewModel.ErrorMessage);
        Assert.Contains("Operazione non valida", viewModel.ErrorMessage);
    }

    [Fact]
    public async Task SaveAsync_WithNullDescription_CreatesProjectWithNullDescription()
    {
        // Arrange
        var educatorId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        
        _projectServiceMock.Setup(s => s.GetActiveForPatientAsync(_patientId))
            .ReturnsAsync((TherapyProjectModel?)null);
        
        _projectServiceMock.Setup(s => s.CreateProjectAsync(It.IsAny<CreateProjectRequest>()))
            .ReturnsAsync(projectId);

        var viewModel = CreateViewModel();
        viewModel.Title = "Test Project";
        viewModel.Description = "   "; // Whitespace only
        viewModel.AvailableEducators.Add(new SelectableEducatorViewModel(educatorId, "Test", "Role") { IsSelected = true });

        // Act
        await viewModel.SaveCommand.ExecuteAsync(null);

        // Assert
        _projectServiceMock.Verify(s => s.CreateProjectAsync(It.Is<CreateProjectRequest>(
            r => r.Description == null
        )), Times.Once);
    }

    #endregion

    #region CancelCommand Tests

    [Fact]
    public void CancelCommand_RaisesOnCancelledEvent()
    {
        // Arrange
        var viewModel = CreateViewModel();
        bool eventRaised = false;
        viewModel.OnCancelled += () => eventRaised = true;

        // Act
        viewModel.CancelCommand.Execute(null);

        // Assert
        Assert.True(eventRaised);
    }

    #endregion

    #region Property Change Tests

    [Fact]
    public void OnTitleChanged_NotifiesSaveCommandCanExecuteChanged()
    {
        // Arrange
        var viewModel = CreateViewModel();
        viewModel.AvailableEducators.Add(new SelectableEducatorViewModel(Guid.NewGuid(), "Test", "Role") { IsSelected = true });
        
        bool canExecuteBefore = viewModel.SaveCommand.CanExecute(null);
        
        // Act
        viewModel.Title = "Valid Title";
        bool canExecuteAfter = viewModel.SaveCommand.CanExecute(null);

        // Assert
        Assert.False(canExecuteBefore);
        Assert.True(canExecuteAfter);
    }

    [Fact]
    public void OnStartDateChanged_NotifiesSaveCommandCanExecuteChanged()
    {
        // Arrange
        var viewModel = CreateViewModel();
        viewModel.Title = "Valid Title";
        viewModel.AvailableEducators.Add(new SelectableEducatorViewModel(Guid.NewGuid(), "Test", "Role") { IsSelected = true });

        // Act
        viewModel.StartDate = DateTime.Now.AddDays(1);

        // Assert - Should still be able to save
        Assert.True(viewModel.SaveCommand.CanExecute(null));
    }

    #endregion

    #region AvailableStates Tests

    [Fact]
    public void AvailableStates_ContainsOnlyActiveAndSuspended()
    {
        // Arrange & Act
        var viewModel = CreateViewModel();

        // Assert
        Assert.Equal(2, viewModel.AvailableStates.Count);
        Assert.Contains(TherapyProjectState.Active, viewModel.AvailableStates);
        Assert.Contains(TherapyProjectState.Suspended, viewModel.AvailableStates);
        Assert.DoesNotContain(TherapyProjectState.Completed, viewModel.AvailableStates);
        Assert.DoesNotContain(TherapyProjectState.Deceased, viewModel.AvailableStates);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task FullWorkflow_LoadEducators_SetData_Save_Success()
    {
        // Arrange
        var educators = new List<EducatorModel>
        {
            new EducatorModel { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe", Role = "Educator" },
            new EducatorModel { Id = Guid.NewGuid(), FirstName = "Jane", LastName = "Smith", Role = "Senior" }
        };

        _educatorServiceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(educators);
        _projectServiceMock.Setup(s => s.GetActiveForPatientAsync(_patientId))
            .ReturnsAsync((TherapyProjectModel?)null);
        _projectServiceMock.Setup(s => s.CreateProjectAsync(It.IsAny<CreateProjectRequest>()))
            .ReturnsAsync(Guid.NewGuid());

        var viewModel = CreateViewModel();

        // Act
        await viewModel.LoadEducatorsAsync();
        viewModel.Title = "Complete Project";
        viewModel.Description = "Full integration test";
        viewModel.AvailableEducators[0].IsSelected = true;
        await viewModel.SaveCommand.ExecuteAsync(null);

        // Assert
        Assert.Equal(2, viewModel.AvailableEducators.Count);
        Assert.NotNull(viewModel.SuccessMessage);
        Assert.Null(viewModel.ErrorMessage);
        _projectServiceMock.Verify(s => s.CreateProjectAsync(It.IsAny<CreateProjectRequest>()), Times.Once);
    }

    #endregion
}
