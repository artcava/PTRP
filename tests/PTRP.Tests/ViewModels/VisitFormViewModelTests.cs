using FluentAssertions;
using Moq;
using PTRP.ViewModels.Visits;
using Xunit;

namespace PTRP.Tests.ViewModels;

/// <summary>
/// Unit tests for VisitFormViewModel
/// Issue #75: CalendarView and VisitFormView implementation
/// </summary>
public class VisitFormViewModelTests
{
    private VisitFormViewModel CreateViewModel()
    {
        return new VisitFormViewModel();
    }
    
    #region Constructor Tests
    
    [Fact]
    public void Constructor_ShouldInitializeWithDefaults()
    {
        // Act
        var viewModel = CreateViewModel();
        
        // Assert
        viewModel.Should().NotBeNull();
        viewModel.DisplayName.Should().Be("Registrazione Visita");
        viewModel.ActualDate.Should().Be(DateTime.Today);
        viewModel.SelectedPresenceStatus.Should().Be("PresentCollaborative");
    }
    
    [Fact]
    public void Constructor_ShouldInitializeOperatorsList()
    {
        // Act
        var viewModel = CreateViewModel();
        
        // Assert
        viewModel.AvailableOperators.Should().NotBeNull();
        viewModel.AvailableOperators.Should().BeEmpty(); // Empty until InitializeFromAppointment is called
    }
    
    #endregion
    
    #region Validation Tests - ActualDate
    
    [Fact]
    public void ActualDate_WhenFutureDate_ShouldHaveValidationError()
    {
        // Arrange
        var viewModel = CreateViewModel();
        
        // Act
        viewModel.ActualDate = DateTime.Today.AddDays(1);
        viewModel.ValidateAllProperties();
        
        // Assert
        viewModel.HasErrors.Should().BeTrue();
        var errors = viewModel.GetErrors(nameof(viewModel.ActualDate));
        errors.Should().NotBeNull();
    }
    
    [Fact]
    public void ActualDate_WhenTodayOrPast_ShouldNotHaveValidationError()
    {
        // Arrange
        var viewModel = CreateViewModel();
        
        // Act
        viewModel.ActualDate = DateTime.Today;
        viewModel.ValidateAllProperties();
        
        // Assert
        var errors = viewModel.GetErrors(nameof(viewModel.ActualDate));
        if (errors != null)
        {
            var errorList = errors.Cast<string>().ToList();
            errorList.Should().BeEmpty();
        }
    }
    
    [Fact]
    public void ActualDate_WhenPastDate_ShouldNotHaveValidationError()
    {
        // Arrange
        var viewModel = CreateViewModel();
        
        // Act
        viewModel.ActualDate = DateTime.Today.AddDays(-1);
        viewModel.ValidateAllProperties();
        
        // Assert
        var errors = viewModel.GetErrors(nameof(viewModel.ActualDate));
        if (errors != null)
        {
            var errorList = errors.Cast<string>().ToList();
            errorList.Should().BeEmpty();
        }
    }
    
    #endregion
    
    #region Validation Tests - Time
    
    [Fact]
    public void EndTime_WhenBeforeStartTime_ShouldHaveValidationError()
    {
        // Arrange
        var viewModel = CreateViewModel();
        
        // Act
        viewModel.StartTime = TimeSpan.FromHours(10);
        viewModel.EndTime = TimeSpan.FromHours(9); // End before start
        viewModel.ValidateAllProperties();
        
        // Assert
        viewModel.HasErrors.Should().BeTrue();
        var errors = viewModel.GetErrors(nameof(viewModel.EndTime));
        errors.Should().NotBeNull();
    }
    
    [Fact]
    public void EndTime_WhenAfterStartTime_ShouldNotHaveValidationError()
    {
        // Arrange
        var viewModel = CreateViewModel();
        
        // Act
        viewModel.StartTime = TimeSpan.FromHours(10);
        viewModel.EndTime = TimeSpan.FromHours(11);
        viewModel.ClinicalNotes = "Note valide di almeno 10 caratteri"; // To avoid other validation errors
        viewModel.ValidateAllProperties();
        
        // Assert
        var errors = viewModel.GetErrors(nameof(viewModel.EndTime));
        if (errors != null)
        {
            var errorList = errors.Cast<string>().ToList();
            errorList.Where(e => e.Contains("deve essere successiva")).Should().BeEmpty();
        }
    }
    
    [Fact]
    public void EndTime_WhenEqualToStartTime_ShouldHaveValidationError()
    {
        // Arrange
        var viewModel = CreateViewModel();
        
        // Act
        viewModel.StartTime = TimeSpan.FromHours(10);
        viewModel.EndTime = TimeSpan.FromHours(10); // Equal
        viewModel.ValidateAllProperties();
        
        // Assert
        viewModel.HasErrors.Should().BeTrue();
    }
    
    #endregion
    
    #region Validation Tests - ClinicalNotes
    
    [Fact]
    public void ClinicalNotes_WhenEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var viewModel = CreateViewModel();
        
        // Act
        viewModel.ClinicalNotes = "";
        viewModel.ValidateAllProperties();
        
        // Assert
        viewModel.HasErrors.Should().BeTrue();
        var errors = viewModel.GetErrors(nameof(viewModel.ClinicalNotes));
        errors.Should().NotBeNull();
    }
    
    [Fact]
    public void ClinicalNotes_WhenTooShort_ShouldHaveValidationError()
    {
        // Arrange
        var viewModel = CreateViewModel();
        
        // Act
        viewModel.ClinicalNotes = "Short"; // Less than 10 characters
        viewModel.ValidateAllProperties();
        
        // Assert
        viewModel.HasErrors.Should().BeTrue();
        var errors = viewModel.GetErrors(nameof(viewModel.ClinicalNotes));
        errors.Should().NotBeNull();
    }
    
    [Fact]
    public void ClinicalNotes_WhenValidLength_ShouldNotHaveValidationError()
    {
        // Arrange
        var viewModel = CreateViewModel();
        
        // Act
        viewModel.ClinicalNotes = "È una nota clinica valida di lunghezza sufficiente.";
        viewModel.StartTime = TimeSpan.FromHours(10);
        viewModel.EndTime = TimeSpan.FromHours(11);
        viewModel.ValidateAllProperties();
        
        // Assert
        var errors = viewModel.GetErrors(nameof(viewModel.ClinicalNotes));
        if (errors != null)
        {
            var errorList = errors.Cast<string>().ToList();
            errorList.Should().BeEmpty();
        }
    }
    
    #endregion
    
    #region Validation Tests - Operators
    
    [Fact]
    public void SaveCommand_WhenNoOperatorsSelected_ShouldBeDisabled()
    {
        // Arrange
        var viewModel = CreateViewModel();
        viewModel.ActualDate = DateTime.Today;
        viewModel.StartTime = TimeSpan.FromHours(10);
        viewModel.EndTime = TimeSpan.FromHours(11);
        viewModel.ClinicalNotes = "Note cliniche valide e sufficientemente lunghe";
        viewModel.SelectedPresenceStatus = "PresentCollaborative";
        
        // No operators added or selected
        
        // Act
        var canSave = viewModel.SaveVisitCommand.CanExecute(null);
        
        // Assert
        canSave.Should().BeFalse();
    }
    
    [Fact]
    public void SaveCommand_WhenAtLeastOneOperatorSelected_ShouldBeEnabled()
    {
        // Arrange
        var viewModel = CreateViewModel();
        viewModel.ActualDate = DateTime.Today;
        viewModel.StartTime = TimeSpan.FromHours(10);
        viewModel.EndTime = TimeSpan.FromHours(11);
        viewModel.ClinicalNotes = "Note cliniche valide e sufficientemente lunghe";
        viewModel.SelectedPresenceStatus = "PresentCollaborative";
        
        // Add and select an operator
        var operatorVm = new OperatorCheckboxViewModel
        {
            EducatorId = Guid.NewGuid(),
            FullName = "Bianchi Giovanni",
            IsSelected = true
        };
        viewModel.AvailableOperators.Add(operatorVm);
        
        // Act
        var canSave = viewModel.SaveVisitCommand.CanExecute(null);
        
        // Assert
        canSave.Should().BeTrue();
    }
    
    [Fact]
    public void SelectedOperatorsCount_ShouldReflectSelectedOperators()
    {
        // Arrange
        var viewModel = CreateViewModel();
        viewModel.AvailableOperators.Add(new OperatorCheckboxViewModel 
        { 
            EducatorId = Guid.NewGuid(), 
            FullName = "Op1", 
            IsSelected = true 
        });
        viewModel.AvailableOperators.Add(new OperatorCheckboxViewModel 
        { 
            EducatorId = Guid.NewGuid(), 
            FullName = "Op2", 
            IsSelected = false 
        });
        viewModel.AvailableOperators.Add(new OperatorCheckboxViewModel 
        { 
            EducatorId = Guid.NewGuid(), 
            FullName = "Op3", 
            IsSelected = true 
        });
        
        // Act & Assert
        viewModel.SelectedOperatorsCount.Should().Be(2);
    }
    
    #endregion
    
    #region Presence Status Tests
    
    [Fact]
    public void SelectedPresenceStatus_ShouldInitializeToDefault()
    {
        // Act
        var viewModel = CreateViewModel();
        
        // Assert
        viewModel.SelectedPresenceStatus.Should().Be("PresentCollaborative");
    }
    
    [Theory]
    [InlineData("PresentCollaborative")]
    [InlineData("PresentNonCollaborative")]
    [InlineData("AbsentJustified")]
    [InlineData("AbsentNotJustified")]
    public void SelectedPresenceStatus_ShouldAcceptAllValidValues(string status)
    {
        // Arrange
        var viewModel = CreateViewModel();
        
        // Act
        viewModel.SelectedPresenceStatus = status;
        
        // Assert
        viewModel.SelectedPresenceStatus.Should().Be(status);
    }
    
    [Fact]
    public void PresenceStatusOptions_ShouldContainAllFourOptions()
    {
        // Arrange
        var viewModel = CreateViewModel();
        
        // Assert
        viewModel.PresenceStatusOptions.Should().HaveCount(4);
        viewModel.PresenceStatusOptions.Select(o => o.Value).Should().Contain(new[]
        {
            "PresentCollaborative",
            "PresentNonCollaborative",
            "AbsentJustified",
            "AbsentNotJustified"
        });
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
        viewModel.SelectedPresenceStatus = "PresentCollaborative";
        
        var operatorVm = new OperatorCheckboxViewModel
        {
            EducatorId = Guid.NewGuid(),
            FullName = "Bianchi Giovanni",
            IsSelected = true
        };
        viewModel.AvailableOperators.Add(operatorVm);
        
        // Act
        var canExecute = viewModel.SaveVisitCommand.CanExecute(null);
        
        // Assert
        canExecute.Should().BeTrue();
    }
    
    [Fact]
    public void SaveCommand_WhenInvalid_ShouldNotBeExecutable()
    {
        // Arrange
        var viewModel = CreateViewModel();
        viewModel.ClinicalNotes = ""; // Invalid
        viewModel.ValidateAllProperties();
        
        // Act
        var canExecute = viewModel.SaveVisitCommand.CanExecute(null);
        
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
    
    #region Initialization Tests
    
    [Fact]
    public void InitializeFromAppointment_ShouldPopulateReadOnlyFields()
    {
        // Arrange
        var viewModel = CreateViewModel();
        var scheduledVisitId = Guid.NewGuid();
        var scheduledDate = new DateTime(2026, 2, 15, 14, 30, 0);
        
        // Act
        viewModel.InitializeFromAppointment(
            scheduledVisitId,
            "Rossi Mario",
            "Verifica Intermedia",
            scheduledDate,
            new List<Guid> { Guid.NewGuid() }
        );
        
        // Assert
        viewModel.ScheduledVisitId.Should().Be(scheduledVisitId);
        viewModel.PatientName.Should().Be("Rossi Mario");
        viewModel.AppointmentTypeDisplay.Should().Be("Verifica Intermedia");
        viewModel.ScheduledDate.Should().Be(scheduledDate);
    }
    
    [Fact]
    public void InitializeFromAppointment_ShouldSetDefaultActualDateAndTimes()
    {
        // Arrange
        var viewModel = CreateViewModel();
        var scheduledDate = new DateTime(2026, 2, 15, 14, 30, 0);
        
        // Act
        viewModel.InitializeFromAppointment(
            Guid.NewGuid(),
            "Rossi Mario",
            "Verifica Intermedia",
            scheduledDate,
            new List<Guid>()
        );
        
        // Assert
        viewModel.ActualDate.Should().Be(scheduledDate.Date);
        viewModel.StartTime.Should().Be(new TimeSpan(14, 30, 0));
        viewModel.EndTime.Should().Be(new TimeSpan(15, 30, 0)); // +1 ora
    }
    
    [Fact]
    public void InitializeFromAppointment_ShouldLoadOperators()
    {
        // Arrange
        var viewModel = CreateViewModel();
        
        // Act
        viewModel.InitializeFromAppointment(
            Guid.NewGuid(),
            "Rossi Mario",
            "Verifica Intermedia",
            DateTime.Today,
            new List<Guid> { Guid.NewGuid(), Guid.NewGuid() }
        );
        
        // Assert
        viewModel.AvailableOperators.Should().NotBeEmpty();
        viewModel.AvailableOperators.Any(o => o.IsCurrentUser).Should().BeTrue();
    }
    
    #endregion
    
    #region Integration Tests
    
    [Fact]
    public void CompleteValidForm_ShouldAllowSave()
    {
        // Arrange
        var viewModel = CreateViewModel();
        
        // Act - Fill all required fields correctly
        viewModel.ActualDate = DateTime.Today;
        viewModel.StartTime = TimeSpan.FromHours(10);
        viewModel.EndTime = TimeSpan.FromHours(11);
        viewModel.ClinicalNotes = "Il paziente ha mostrato collaborazione durante la visita. Obiettivi raggiunti.";
        viewModel.Outcomes = "Obiettivi: mantenimento autonomie ADL.";
        viewModel.SelectedPresenceStatus = "PresentCollaborative";
        
        var operatorVm = new OperatorCheckboxViewModel
        {
            EducatorId = Guid.NewGuid(),
            FullName = "Bianchi Giovanni",
            IsSelected = true
        };
        viewModel.AvailableOperators.Add(operatorVm);
        
        // Assert
        viewModel.SaveVisitCommand.CanExecute(null).Should().BeTrue();
    }
    
    [Fact]
    public void ScheduledDateDisplay_ShouldFormatCorrectly()
    {
        // Arrange
        var viewModel = CreateViewModel();
        var testDate = new DateTime(2026, 2, 15, 14, 30, 0);
        
        // Act
        viewModel.ScheduledDate = testDate;
        
        // Assert
        viewModel.ScheduledDateDisplay.Should().Be("15/02/2026 14:30");
    }
    
    #endregion
}
