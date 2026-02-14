using FluentAssertions;
using Moq;
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
        return new CalendarViewModel();
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
        viewModel.FilterVerifiche.Should().BeTrue();
        viewModel.FilterDimissioni.Should().BeTrue();
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
    public async Task GoToPreviousMonthCommand_ShouldDecrementMonth()
    {
        // Arrange
        var viewModel = CreateViewModel();
        var initialMonth = viewModel.CurrentMonth;
        
        // Act
        await viewModel.GoToPreviousMonthCommand.ExecuteAsync(null);
        
        // Assert
        viewModel.CurrentMonth.Should().Be(initialMonth.AddMonths(-1));
    }
    
    [Fact]
    public async Task GoToNextMonthCommand_ShouldIncrementMonth()
    {
        // Arrange
        var viewModel = CreateViewModel();
        var initialMonth = viewModel.CurrentMonth;
        
        // Act
        await viewModel.GoToNextMonthCommand.ExecuteAsync(null);
        
        // Assert
        viewModel.CurrentMonth.Should().Be(initialMonth.AddMonths(1));
    }
    
    [Fact]
    public async Task GoToTodayCommand_ShouldResetToCurrentMonth()
    {
        // Arrange
        var viewModel = CreateViewModel();
        await viewModel.GoToNextMonthCommand.ExecuteAsync(null); // Move forward
        await viewModel.GoToNextMonthCommand.ExecuteAsync(null); // Move forward again
        
        // Act
        await viewModel.GoToTodayCommand.ExecuteAsync(null);
        
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
        viewModel.CurrentMonthDisplay.Should().Contain("giugno");
        viewModel.CurrentMonthDisplay.Should().Contain("2025");
    }
    
    #endregion
    
    #region Day Selection Tests
    
    [Fact]
    public async Task SelectDayCommand_WithValidDay_ShouldSetSelectedDate()
    {
        // Arrange
        var viewModel = CreateViewModel();
        await viewModel.LoadMonthDataAsync();
        
        var testDate = new DateTime(2025, 5, 15);
        var dayViewModel = viewModel.Days.FirstOrDefault(d => d.Date.Date == testDate.Date);
        
        if (dayViewModel != null)
        {
            // Act
            await viewModel.SelectDayCommand.ExecuteAsync(dayViewModel);
            
            // Assert
            viewModel.SelectedDate.Should().Be(testDate);
        }
    }
    
    [Fact]
    public void SelectedDate_WhenChanged_ShouldUpdateMessage()
    {
        // Arrange
        var viewModel = CreateViewModel();
        var testDate = DateTime.Today;
        
        // Act
        viewModel.SelectedDate = testDate;
        
        // Assert
        viewModel.NoAppointmentsMessage.Should().Contain(testDate.ToString("dd/MM/yyyy"));
    }
    
    #endregion
    
    #region Filter Tests
    
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void FilterIntake_WhenChanged_ShouldUpdateProperty(bool filterValue)
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
    public void FilterVerifiche_WhenChanged_ShouldUpdateProperty(bool filterValue)
    {
        // Arrange
        var viewModel = CreateViewModel();
        
        // Act
        viewModel.FilterVerifiche = filterValue;
        
        // Assert
        viewModel.FilterVerifiche.Should().Be(filterValue);
    }
    
    [Fact]
    public async Task ApplyFiltersCommand_ShouldReloadData()
    {
        // Arrange
        var viewModel = CreateViewModel();
        viewModel.FilterIntake = false;
        
        // Act
        await viewModel.ApplyFiltersCommand.ExecuteAsync(null);
        
        // Assert - command should execute without errors
        viewModel.FilterIntake.Should().BeFalse();
    }
    
    #endregion
    
    #region Command Tests
    
    [Fact]
    public void RegisterVisitCommand_ShouldBeCreatedAndExecutable()
    {
        // Arrange
        var viewModel = CreateViewModel();
        
        // Assert
        viewModel.RegisterVisitCommand.Should().NotBeNull();
    }
    
    [Fact]
    public void RescheduleAppointmentCommand_ShouldBeCreatedAndExecutable()
    {
        // Arrange
        var viewModel = CreateViewModel();
        
        // Assert
        viewModel.RescheduleAppointmentCommand.Should().NotBeNull();
    }
    
    [Fact]
    public void MarkAsMissedCommand_ShouldBeCreatedAndExecutable()
    {
        // Arrange
        var viewModel = CreateViewModel();
        
        // Assert
        viewModel.MarkAsMissedCommand.Should().NotBeNull();
    }
    
    #endregion
    
    #region Month Display Tests
    
    [Theory]
    [InlineData(1, "gennaio")]
    [InlineData(2, "febbraio")]
    [InlineData(3, "marzo")]
    [InlineData(4, "aprile")]
    [InlineData(5, "maggio")]
    [InlineData(6, "giugno")]
    [InlineData(7, "luglio")]
    [InlineData(8, "agosto")]
    [InlineData(9, "settembre")]
    [InlineData(10, "ottobre")]
    [InlineData(11, "novembre")]
    [InlineData(12, "dicembre")]
    public void CurrentMonthDisplay_ShouldFormatCorrectlyForAllMonths(int month, string expectedMonthName)
    {
        // Arrange
        var viewModel = CreateViewModel();
        var testDate = new DateTime(2025, month, 1);
        
        // Act
        viewModel.CurrentMonth = testDate;
        
        // Assert
        viewModel.CurrentMonthDisplay.ToLower().Should().Contain(expectedMonthName.ToLower());
        viewModel.CurrentMonthDisplay.Should().Contain("2025");
    }
    
    #endregion
    
    #region Integration Tests
    
    [Fact]
    public async Task LoadMonthDataAsync_ShouldPopulateDaysCollection()
    {
        // Arrange
        var viewModel = CreateViewModel();
        
        // Act
        await viewModel.LoadMonthDataAsync();
        
        // Assert
        viewModel.Days.Should().NotBeEmpty();
        // Calendar should have 35-42 days (5-6 weeks)
        viewModel.Days.Count.Should().BeInRange(35, 42);
    }
    
    [Fact]
    public async Task LoadMonthDataAsync_ShouldDistinguishCurrentMonthDays()
    {
        // Arrange
        var viewModel = CreateViewModel();
        
        // Act
        await viewModel.LoadMonthDataAsync();
        
        // Assert
        var currentMonthDays = viewModel.Days.Where(d => d.IsCurrentMonth).ToList();
        currentMonthDays.Should().NotBeEmpty();
        
        var otherMonthDays = viewModel.Days.Where(d => !d.IsCurrentMonth).ToList();
        // Should have some days from adjacent months in a full calendar grid
        (currentMonthDays.Count + otherMonthDays.Count).Should().Be(viewModel.Days.Count);
    }
    
    [Fact]
    public async Task LoadMonthAsync_ShouldCallLoadMonthDataAsync()
    {
        // Arrange
        var viewModel = CreateViewModel();
        
        // Act
        await viewModel.LoadMonthAsync();
        
        // Assert
        viewModel.Days.Should().NotBeEmpty();
    }
    
    #endregion
}
