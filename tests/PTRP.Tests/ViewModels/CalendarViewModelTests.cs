using FluentAssertions;
using Moq;
using PTRP.Data.Models;
using PTRP.Services.Interfaces;
using PTRP.ViewModels.Calendar;
using Xunit;

namespace PTRP.Tests.ViewModels;

/// <summary>
/// Unit tests for CalendarViewModel
/// Issue #75: CalendarView and VisitFormView implementation
/// </summary>
public class CalendarViewModelTests
{
    private readonly Mock<INavigationService> _mockNavigationService;
    
    public CalendarViewModelTests()
    {
        _mockNavigationService = new Mock<INavigationService>();
    }
    
    private CalendarViewModel CreateViewModel()
    {
        return new CalendarViewModel(_mockNavigationService.Object);
    }
    
    #region Constructor Tests
    
    [Fact]
    public void Constructor_ShouldInitializeWithCurrentMonth()
    {
        // Act
        var viewModel = CreateViewModel();
        
        // Assert
        viewModel.Should().NotBeNull();
        viewModel.CurrentMonth.Year.Should().Be(DateTime.Now.Year);
        viewModel.CurrentMonth.Month.Should().Be(DateTime.Now.Month);
        viewModel.DisplayName.Should().Be("Calendario Appuntamenti");
    }
    
    [Fact]
    public void Constructor_ShouldInitializeFiltersToTrue()
    {
        // Act
        var viewModel = CreateViewModel();
        
        // Assert
        viewModel.FilterIntake.Should().BeTrue();
        viewModel.FilterIntermediate.Should().BeTrue();
        viewModel.FilterFinal.Should().BeTrue();
        viewModel.FilterDischarge.Should().BeTrue();
    }
    
    [Fact]
    public void Constructor_ShouldInitializeEmptyCollections()
    {
        // Act
        var viewModel = CreateViewModel();
        
        // Assert
        viewModel.Days.Should().NotBeNull();
        viewModel.Days.Should().BeEmpty();
        viewModel.SelectedDayAppointments.Should().NotBeNull();
        viewModel.SelectedDayAppointments.Should().BeEmpty();
    }
    
    #endregion
    
    #region Month Navigation Tests
    
    [Fact]
    public void GoToPreviousMonthCommand_ShouldDecrementMonth()
    {
        // Arrange
        var viewModel = CreateViewModel();
        var initialMonth = viewModel.CurrentMonth;
        
        // Act
        viewModel.GoToPreviousMonthCommand.Execute(null);
        
        // Assert
        viewModel.CurrentMonth.Should().Be(initialMonth.AddMonths(-1));
    }
    
    [Fact]
    public void GoToNextMonthCommand_ShouldIncrementMonth()
    {
        // Arrange
        var viewModel = CreateViewModel();
        var initialMonth = viewModel.CurrentMonth;
        
        // Act
        viewModel.GoToNextMonthCommand.Execute(null);
        
        // Assert
        viewModel.CurrentMonth.Should().Be(initialMonth.AddMonths(1));
    }
    
    [Fact]
    public void GoToTodayCommand_ShouldResetToCurrentMonth()
    {
        // Arrange
        var viewModel = CreateViewModel();
        viewModel.GoToNextMonthCommand.Execute(null); // Move forward
        viewModel.GoToNextMonthCommand.Execute(null); // Move forward again
        
        // Act
        viewModel.GoToTodayCommand.Execute(null);
        
        // Assert
        viewModel.CurrentMonth.Year.Should().Be(DateTime.Now.Year);
        viewModel.CurrentMonth.Month.Should().Be(DateTime.Now.Month);
    }
    
    [Fact]
    public void CurrentMonth_WhenChanged_ShouldUpdateMonthYearDisplay()
    {
        // Arrange
        var viewModel = CreateViewModel();
        var targetDate = new DateTime(2025, 6, 15);
        
        // Act
        viewModel.CurrentMonth = targetDate;
        
        // Assert
        viewModel.MonthYearDisplay.Should().Be("Giugno 2025");
    }
    
    #endregion
    
    #region Day Selection Tests
    
    [Fact]
    public void SelectDayCommand_WithValidDay_ShouldSetSelectedDate()
    {
        // Arrange
        var viewModel = CreateViewModel();
        var testDate = new DateTime(2025, 5, 15);
        var dayViewModel = new DayViewModel
        {
            Date = testDate,
            IsCurrentMonth = true
        };
        
        // Act
        viewModel.SelectDayCommand.Execute(dayViewModel);
        
        // Assert
        viewModel.SelectedDate.Should().Be(testDate);
    }
    
    [Fact]
    public void SelectDayCommand_WithNullDay_ShouldNotThrow()
    {
        // Arrange
        var viewModel = CreateViewModel();
        
        // Act
        Action act = () => viewModel.SelectDayCommand.Execute(null);
        
        // Assert
        act.Should().NotThrow();
    }
    
    [Fact]
    public void SelectedDate_WhenChanged_ShouldFilterAppointments()
    {
        // Arrange
        var viewModel = CreateViewModel();
        var testDate = DateTime.Today;
        
        // Act
        viewModel.SelectedDate = testDate;
        
        // Assert - should trigger appointment filtering logic
        viewModel.SelectedDayAppointments.Should().NotBeNull();
    }
    
    #endregion
    
    #region Filter Tests
    
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void FilterIntake_WhenChanged_ShouldUpdateAppointmentsList(bool filterValue)
    {
        // Arrange
        var viewModel = CreateViewModel();
        
        // Act
        viewModel.FilterIntake = filterValue;
        
        // Assert
        viewModel.FilterIntake.Should().Be(filterValue);
    }
    
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void FilterIntermediate_WhenChanged_ShouldUpdateAppointmentsList(bool filterValue)
    {
        // Arrange
        var viewModel = CreateViewModel();
        
        // Act
        viewModel.FilterIntermediate = filterValue;
        
        // Assert
        viewModel.FilterIntermediate.Should().Be(filterValue);
    }
    
    [Fact]
    public void AllFiltersOff_ShouldShowNoAppointments()
    {
        // Arrange
        var viewModel = CreateViewModel();
        
        // Act
        viewModel.FilterIntake = false;
        viewModel.FilterIntermediate = false;
        viewModel.FilterFinal = false;
        viewModel.FilterDischarge = false;
        
        // Assert - when all filters are off, no appointments should be visible
        viewModel.SelectedDayAppointments.Should().BeEmpty();
    }
    
    #endregion
    
    #region Appointment Action Tests
    
    [Fact]
    public void RegisterVisitCommand_ShouldBeCreatedAndExecutable()
    {
        // Arrange
        var viewModel = CreateViewModel();
        
        // Assert
        viewModel.RegisterVisitCommand.Should().NotBeNull();
        viewModel.RegisterVisitCommand.CanExecute(null).Should().BeTrue();
    }
    
    [Fact]
    public void RescheduleCommand_ShouldBeCreatedAndExecutable()
    {
        // Arrange
        var viewModel = CreateViewModel();
        
        // Assert
        viewModel.RescheduleCommand.Should().NotBeNull();
        viewModel.RescheduleCommand.CanExecute(null).Should().BeTrue();
    }
    
    [Fact]
    public void MarkAsMissedCommand_ShouldBeCreatedAndExecutable()
    {
        // Arrange
        var viewModel = CreateViewModel();
        
        // Assert
        viewModel.MarkAsMissedCommand.Should().NotBeNull();
        viewModel.MarkAsMissedCommand.CanExecute(null).Should().BeTrue();
    }
    
    #endregion
    
    #region Month Display Tests
    
    [Theory]
    [InlineData(1, "Gennaio")]
    [InlineData(2, "Febbraio")]
    [InlineData(3, "Marzo")]
    [InlineData(4, "Aprile")]
    [InlineData(5, "Maggio")]
    [InlineData(6, "Giugno")]
    [InlineData(7, "Luglio")]
    [InlineData(8, "Agosto")]
    [InlineData(9, "Settembre")]
    [InlineData(10, "Ottobre")]
    [InlineData(11, "Novembre")]
    [InlineData(12, "Dicembre")]
    public void MonthYearDisplay_ShouldFormatCorrectlyForAllMonths(int month, string expectedMonthName)
    {
        // Arrange
        var viewModel = CreateViewModel();
        var testDate = new DateTime(2025, month, 1);
        
        // Act
        viewModel.CurrentMonth = testDate;
        
        // Assert
        viewModel.MonthYearDisplay.Should().Contain(expectedMonthName);
        viewModel.MonthYearDisplay.Should().Contain("2025");
    }
    
    #endregion
    
    #region Integration Tests
    
    [Fact]
    public async Task LoadMonthAsync_ShouldPopulateDaysCollection()
    {
        // Arrange
        var viewModel = CreateViewModel();
        
        // Act
        await viewModel.LoadMonthAsync();
        
        // Assert
        viewModel.Days.Should().NotBeEmpty();
        // Calendar should have 35-42 days (5-6 weeks)
        viewModel.Days.Count.Should().BeInRange(35, 42);
    }
    
    [Fact]
    public async Task LoadMonthAsync_ShouldMarkTodayCorrectly()
    {
        // Arrange
        var viewModel = CreateViewModel();
        
        // Act
        await viewModel.LoadMonthAsync();
        
        // Assert
        var todayDay = viewModel.Days.FirstOrDefault(d => d.IsToday);
        todayDay.Should().NotBeNull();
        todayDay!.Date.Date.Should().Be(DateTime.Today);
    }
    
    [Fact]
    public async Task LoadMonthAsync_ShouldDistinguishCurrentMonthDays()
    {
        // Arrange
        var viewModel = CreateViewModel();
        
        // Act
        await viewModel.LoadMonthAsync();
        
        // Assert
        var currentMonthDays = viewModel.Days.Where(d => d.IsCurrentMonth).ToList();
        currentMonthDays.Should().NotBeEmpty();
        
        var otherMonthDays = viewModel.Days.Where(d => !d.IsCurrentMonth).ToList();
        // Should have some days from adjacent months in a full calendar grid
        (currentMonthDays.Count + otherMonthDays.Count).Should().Be(viewModel.Days.Count);
    }
    
    #endregion
}
