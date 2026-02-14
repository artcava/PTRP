using FluentAssertions;
using Moq;
using PTRP.Data.Models;
using PTRP.Data.Models.Enums;
using PTRP.Services.Interfaces;
using PTRP.ViewModels.Visits;
using Xunit;

namespace PTRP.Tests.ViewModels;

/// <summary>
/// Unit tests for VisitFormViewModel
/// Issue #75: CalendarView and VisitFormView implementation
/// </summary>
public class VisitFormViewModelTests
{
    private readonly Mock<INavigationService> _mockNavigationService;
    
    public VisitFormViewModelTests()
    {
        _mockNavigationService = new Mock<INavigationService>();
    }
    
    private ScheduledVisitModel CreateTestScheduledVisit()
    {
        return new ScheduledVisitModel
        {
            Id = Guid.NewGuid(),
            TherapyProjectId = Guid.NewGuid(),
            VisitType = VisitType.INTAKE,
            ScheduledDate = DateTime.Today.AddDays(1),
            Status = AppointmentStatus.Scheduled,
            TherapyProject = new TherapyProjectModel
            {
                Id = Guid.NewGuid(),
                PatientId = Guid.NewGuid(),
                StartDate = DateTime.Today.AddMonths(-1),
                EndDate = DateTime.Today.AddMonths(11),
                State = TherapyProjectState.Active,
                Patient = new PatientModel
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Mario",
                    LastName = "Rossi",
                    FiscalCode = "RSSMRA80A01H501U",
                    DateOfBirth = new DateTime(1980, 1, 1)
                }
            }
        };
    }
    
    private VisitFormViewModel CreateViewModel(ScheduledVisitModel? scheduledVisit = null)
    {
        scheduledVisit ??= CreateTestScheduledVisit();
        return new VisitFormViewModel(
            _mockNavigationService.Object,
            scheduledVisit
        );
    }
    
    #region Constructor Tests
    
    [Fact]
    public void Constructor_WithValidScheduledVisit_ShouldInitializeProperties()
    {
        // Arrange
        var scheduledVisit = CreateTestScheduledVisit();
        
        // Act
        var viewModel = CreateViewModel(scheduledVisit);
        
        // Assert
        viewModel.Should().NotBeNull();
        viewModel.PatientName.Should().Be("Rossi Mario");
        viewModel.AppointmentTypeDisplay.Should().Contain("Prima Apertura");
        viewModel.ScheduledDate.Should().Be(scheduledVisit.ScheduledDate);
        viewModel.DisplayName.Should().Be("Registrazione Visita");
    }
    
    [Fact]
    public void Constructor_ShouldInitializeActualDateToToday()
    {
        // Act
        var viewModel = CreateViewModel();
        
        // Assert
        viewModel.ActualDate.Should().Be(DateTime.Today);
    }
    
    [Fact]
    public void Constructor_ShouldInitializeOperatorsList()
    {
        // Act
        var viewModel = CreateViewModel();
        
        // Assert
        viewModel.Operators.Should().NotBeNull();
        viewModel.Operators.Should().BeEmpty(); // Empty until LoadOperatorsAsync is called
    }
    
    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenScheduledVisitIsNull()
    {
        // Act
        Action act = () => new VisitFormViewModel(_mockNavigationService.Object, null!);
        
        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
    
    #endregion
    
    #region Validation Tests - ActualDate
    
    [Fact]
    public void ActualDate_WhenNull_ShouldHaveValidationError()
    {
        // Arrange
        var viewModel = CreateViewModel();
        
        // Act
        viewModel.ActualDate = null;
        
        // Assert
        viewModel.HasErrors.Should().BeTrue();
        viewModel.GetErrors(nameof(viewModel.ActualDate)).Should().NotBeEmpty();
    }
    
    [Fact]
    public void ActualDate_WhenFutureDate_ShouldHaveValidationError()
    {
        // Arrange
        var viewModel = CreateViewModel();
        
        // Act
        viewModel.ActualDate = DateTime.Today.AddDays(1);
        
        // Assert
        viewModel.HasErrors.Should().BeTrue();
        var errors = viewModel.GetErrors(nameof(viewModel.ActualDate)).Cast<string>().ToList();
        errors.Should().Contain(e => e.Contains("non può essere futura"));
    }
    
    [Fact]
    public void ActualDate_WhenTodayOrPast_ShouldNotHaveValidationError()
    {
        // Arrange
        var viewModel = CreateViewModel();
        
        // Act
        viewModel.ActualDate = DateTime.Today;
        
        // Assert
        var errors = viewModel.GetErrors(nameof(viewModel.ActualDate)).Cast<string>().ToList();
        errors.Should().BeEmpty();
    }
    
    #endregion
    
    #region Validation Tests - Time
    
    [Fact]
    public void StartTime_WhenNull_ShouldHaveValidationError()
    {
        // Arrange
        var viewModel = CreateViewModel();
        
        // Act
        viewModel.StartTime = null;
        
        // Assert
        viewModel.HasErrors.Should().BeTrue();
        viewModel.GetErrors(nameof(viewModel.StartTime)).Should().NotBeEmpty();
    }
    
    [Fact]
    public void EndTime_WhenNull_ShouldHaveValidationError()
    {
        // Arrange
        var viewModel = CreateViewModel();
        
        // Act
        viewModel.EndTime = null;
        
        // Assert
        viewModel.HasErrors.Should().BeTrue();
        viewModel.GetErrors(nameof(viewModel.EndTime)).Should().NotBeEmpty();
    }
    
    [Fact]
    public void EndTime_WhenBeforeStartTime_ShouldHaveValidationError()
    {
        // Arrange
        var viewModel = CreateViewModel();
        
        // Act
        viewModel.StartTime = TimeSpan.FromHours(10);
        viewModel.EndTime = TimeSpan.FromHours(9); // End before start
        
        // Assert
        viewModel.HasErrors.Should().BeTrue();
        var errors = viewModel.GetErrors(nameof(viewModel.EndTime)).Cast<string>().ToList();
        errors.Should().Contain(e => e.Contains("deve essere successiva"));
    }
    
    [Fact]
    public void EndTime_WhenAfterStartTime_ShouldNotHaveValidationError()
    {
        // Arrange
        var viewModel = CreateViewModel();
        
        // Act
        viewModel.StartTime = TimeSpan.FromHours(10);
        viewModel.EndTime = TimeSpan.FromHours(11);
        
        // Assert
        var errors = viewModel.GetErrors(nameof(viewModel.EndTime)).Cast<string>().ToList();
        errors.Should().NotContain(e => e.Contains("deve essere successiva"));
    }
    
    #endregion
    
    #region Validation Tests - ClinicalNotes
    
    [Fact]
    public void ClinicalNotes_WhenNullOrEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var viewModel = CreateViewModel();
        
        // Act & Assert - Null
        viewModel.ClinicalNotes = null;
        viewModel.HasErrors.Should().BeTrue();
        viewModel.GetErrors(nameof(viewModel.ClinicalNotes)).Should().NotBeEmpty();
        
        // Act & Assert - Empty
        viewModel.ClinicalNotes = "";
        viewModel.HasErrors.Should().BeTrue();
        viewModel.GetErrors(nameof(viewModel.ClinicalNotes)).Should().NotBeEmpty();
        
        // Act & Assert - Whitespace
        viewModel.ClinicalNotes = "   ";
        viewModel.HasErrors.Should().BeTrue();
        viewModel.GetErrors(nameof(viewModel.ClinicalNotes)).Should().NotBeEmpty();
    }
    
    [Fact]
    public void ClinicalNotes_WhenTooShort_ShouldHaveValidationError()
    {
        // Arrange
        var viewModel = CreateViewModel();
        
        // Act
        viewModel.ClinicalNotes = "Short"; // Less than 10 characters
        
        // Assert
        viewModel.HasErrors.Should().BeTrue();
        var errors = viewModel.GetErrors(nameof(viewModel.ClinicalNotes)).Cast<string>().ToList();
        errors.Should().Contain(e => e.Contains("almeno 10 caratteri"));
    }
    
    [Fact]
    public void ClinicalNotes_WhenValidLength_ShouldNotHaveValidationError()
    {
        // Arrange
        var viewModel = CreateViewModel();
        
        // Act
        viewModel.ClinicalNotes = "Questa è una nota clinica valida di lunghezza sufficiente.";
        
        // Assert
        var errors = viewModel.GetErrors(nameof(viewModel.ClinicalNotes)).Cast<string>().ToList();
        errors.Should().BeEmpty();
    }
    
    #endregion
    
    #region Validation Tests - Operators
    
    [Fact]
    public void Operators_WhenNoneSelected_ShouldPreventSave()
    {
        // Arrange
        var viewModel = CreateViewModel();
        viewModel.ActualDate = DateTime.Today;
        viewModel.StartTime = TimeSpan.FromHours(10);
        viewModel.EndTime = TimeSpan.FromHours(11);
        viewModel.ClinicalNotes = "Note cliniche valide e sufficientemente lunghe";
        viewModel.SelectedPresenceStatus = PresenceStatus.PresentCollaborative;
        
        // No operators added or selected
        
        // Act
        var canSave = viewModel.SaveCommand.CanExecute(null);
        
        // Assert
        canSave.Should().BeFalse();
    }
    
    [Fact]
    public void Operators_WhenAtLeastOneSelected_ShouldAllowSave()
    {
        // Arrange
        var viewModel = CreateViewModel();
        viewModel.ActualDate = DateTime.Today;
        viewModel.StartTime = TimeSpan.FromHours(10);
        viewModel.EndTime = TimeSpan.FromHours(11);
        viewModel.ClinicalNotes = "Note cliniche valide e sufficientemente lunghe";
        viewModel.SelectedPresenceStatus = PresenceStatus.PresentCollaborative;
        
        // Add and select an operator
        var operatorVm = new OperatorCheckboxViewModel
        {
            OperatorId = Guid.NewGuid(),
            FullName = "Bianchi Giovanni",
            IsSelected = true
        };
        viewModel.Operators.Add(operatorVm);
        
        // Act
        var canSave = viewModel.SaveCommand.CanExecute(null);
        
        // Assert
        canSave.Should().BeTrue();
    }
    
    #endregion
    
    #region Presence Status Tests
    
    [Fact]
    public void SelectedPresenceStatus_ShouldInitializeToDefault()
    {
        // Act
        var viewModel = CreateViewModel();
        
        // Assert
        viewModel.SelectedPresenceStatus.Should().Be(PresenceStatus.PresentCollaborative);
    }
    
    [Theory]
    [InlineData(PresenceStatus.PresentCollaborative)]
    [InlineData(PresenceStatus.PresentNonCollaborative)]
    [InlineData(PresenceStatus.AbsentJustified)]
    [InlineData(PresenceStatus.AbsentNotJustified)]
    public void SelectedPresenceStatus_ShouldAcceptAllValidValues(PresenceStatus status)
    {
        // Arrange
        var viewModel = CreateViewModel();
        
        // Act
        viewModel.SelectedPresenceStatus = status;
        
        // Assert
        viewModel.SelectedPresenceStatus.Should().Be(status);
    }
    
    #endregion
    
    #region Command Tests
    
    [Fact]
    public void SaveCommand_WhenValid_ShouldBeExecutable()
    {
        // Arrange
        var viewModel = CreateViewModel();
        viewModel.ActualDate = DateTime.Today;
        viewModel.StartTime = TimeSpan.FromHours(10);
        viewModel.EndTime = TimeSpan.FromHours(11);
        viewModel.ClinicalNotes = "Note cliniche valide e sufficientemente lunghe per il test";
        viewModel.SelectedPresenceStatus = PresenceStatus.PresentCollaborative;
        
        var operatorVm = new OperatorCheckboxViewModel
        {
            OperatorId = Guid.NewGuid(),
            FullName = "Bianchi Giovanni",
            IsSelected = true
        };
        viewModel.Operators.Add(operatorVm);
        
        // Act
        var canExecute = viewModel.SaveCommand.CanExecute(null);
        
        // Assert
        canExecute.Should().BeTrue();
    }
    
    [Fact]
    public void SaveCommand_WhenInvalid_ShouldNotBeExecutable()
    {
        // Arrange
        var viewModel = CreateViewModel();
        // Leave all fields empty/invalid
        
        // Act
        var canExecute = viewModel.SaveCommand.CanExecute(null);
        
        // Assert
        canExecute.Should().BeFalse();
    }
    
    [Fact]
    public void CancelCommand_ShouldAlwaysBeExecutable()
    {
        // Arrange
        var viewModel = CreateViewModel();
        
        // Act
        var canExecute = viewModel.CancelCommand.CanExecute(null);
        
        // Assert
        canExecute.Should().BeTrue();
    }
    
    #endregion
    
    #region Integration Tests
    
    [Fact]
    public void CompleteValidForm_ShouldHaveNoErrors()
    {
        // Arrange
        var viewModel = CreateViewModel();
        
        // Act - Fill all required fields correctly
        viewModel.ActualDate = DateTime.Today;
        viewModel.StartTime = TimeSpan.FromHours(10);
        viewModel.EndTime = TimeSpan.FromHours(11);
        viewModel.ClinicalNotes = "Il paziente ha mostrato collaborazione durante la visita. Obiettivi raggiunti.";
        viewModel.Outcomes = "Obiettivi: mantenimento autonomie ADL.";
        viewModel.SelectedPresenceStatus = PresenceStatus.PresentCollaborative;
        
        var operatorVm = new OperatorCheckboxViewModel
        {
            OperatorId = Guid.NewGuid(),
            FullName = "Bianchi Giovanni",
            IsSelected = true
        };
        viewModel.Operators.Add(operatorVm);
        
        // Assert
        viewModel.HasErrors.Should().BeFalse();
        viewModel.SaveCommand.CanExecute(null).Should().BeTrue();
    }
    
    [Fact]
    public void DisplayProperties_ShouldFormatCorrectly()
    {
        // Arrange
        var scheduledVisit = CreateTestScheduledVisit();
        scheduledVisit.VisitType = VisitType.INTERMEDIATE;
        
        // Act
        var viewModel = CreateViewModel(scheduledVisit);
        
        // Assert
        viewModel.PatientName.Should().Contain("Rossi");
        viewModel.PatientName.Should().Contain("Mario");
        viewModel.AppointmentTypeDisplay.Should().Contain("Verifica");
    }
    
    #endregion
}
