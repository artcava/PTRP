using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;
using System.Windows;

namespace PTRP.ViewModels;

/// <summary>
/// ViewModel principale per MainWindow
/// Gestisce:
/// - Navigazione tra le viste
/// - Stato utente corrente
/// - Notifiche (Snackbar)
/// </summary>
public partial class MainViewModel : ViewModelBase
{
    private readonly IServiceProvider _serviceProvider;
    
    public override string DisplayName => "PTRP";
    
    /// <summary>
    /// ViewModel corrente visualizzato nell'area contenuto
    /// </summary>
    [ObservableProperty]
    private ViewModelBase? _currentViewModel;
    
    /// <summary>
    /// Titolo della pagina corrente (per TopBar)
    /// </summary>
    [ObservableProperty]
    private string _currentPageTitle = "Dashboard";
    
    /// <summary>
    /// Nome completo utente corrente
    /// </summary>
    [ObservableProperty]
    private string _userFullName = "Coordinatore Principale";
    
    /// <summary>
    /// Ruolo utente corrente
    /// </summary>
    [ObservableProperty]
    private string _userRole = "Coordinatore";
    
    /// <summary>
    /// Iniziali utente (per avatar)
    /// </summary>
    [ObservableProperty]
    private string _userInitials = "CP";
    
    /// <summary>
    /// MessageQueue per Snackbar notifications
    /// </summary>
    public SnackbarMessageQueue MessageQueue { get; } = new(TimeSpan.FromSeconds(3));
    
    public MainViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        
        // TODO: Load user profile from configuration service
        LoadUserProfile();
        
        // Navigate to Dashboard by default
        // TODO: Check if first run, navigate to FirstRunView if needed
        NavigateToDashboard();
    }
    
    /// <summary>
    /// Carica il profilo utente corrente
    /// </summary>
    private void LoadUserProfile()
    {
        // TODO: Implementare con IConfigurationService in Issue #49
        // Per ora usa valori hardcoded
        UserFullName = "Marco Cavallo";
        UserRole = "Coordinatore";
        UserInitials = "MC";
    }
    
    #region Navigation Commands
    
    [RelayCommand]
    private void NavigateToDashboard()
    {
        // TODO: Implementare NavigationService in Issue #46
        // Per ora imposta titolo
        CurrentPageTitle = "Dashboard";
        
        // Placeholder: in Issue #46 sarà:
        // CurrentViewModel = _navigationService.NavigateTo<DashboardViewModel>();
        
        ShowInfo("Dashboard - In sviluppo (Issue #50)");
    }
    
    [RelayCommand]
    private void NavigateToPatients()
    {
        CurrentPageTitle = "Pazienti";
        
        // TODO: Issue #46 + #51
        // CurrentViewModel = _navigationService.NavigateTo<PatientListViewModel>();
        
        ShowInfo("Lista Pazienti - In sviluppo (Issue #51)");
    }
    
    [RelayCommand]
    private void NavigateToProjects()
    {
        CurrentPageTitle = "Progetti";
        ShowInfo("Progetti - In sviluppo (FASE 2)");
    }
    
    [RelayCommand]
    private void NavigateToEducators()
    {
        CurrentPageTitle = "Educatori";
        ShowInfo("Educatori - In sviluppo (FASE 2)");
    }
    
    [RelayCommand]
    private void NavigateToCalendar()
    {
        CurrentPageTitle = "Calendario";
        ShowInfo("Calendario - In sviluppo (FASE 2)");
    }
    
    [RelayCommand]
    private void NavigateToSync()
    {
        CurrentPageTitle = "Sincronizzazione";
        ShowInfo("Sync - In sviluppo (Issue #52)");
    }
    
    [RelayCommand]
    private void NavigateToReports()
    {
        CurrentPageTitle = "Report";
        ShowInfo("Report - In sviluppo (FASE 2)");
    }
    
    [RelayCommand]
    private void NavigateToSettings()
    {
        CurrentPageTitle = "Impostazioni";
        ShowInfo("Impostazioni - In sviluppo (FASE 4)");
    }
    
    #endregion
    
    #region Notification Methods
    
    /// <summary>
    /// Mostra messaggio informativo nel Snackbar
    /// </summary>
    public void ShowInfo(string message)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            MessageQueue.Enqueue(message);
        });
    }
    
    /// <summary>
    /// Mostra messaggio di successo nel Snackbar
    /// </summary>
    public void ShowSuccess(string message)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            MessageQueue.Enqueue(
                message,
                null,
                null,
                null,
                false,
                false,
                TimeSpan.FromSeconds(2));
        });
    }
    
    /// <summary>
    /// Mostra messaggio di errore nel Snackbar
    /// </summary>
    public void ShowError(string message, int durationSeconds = 5)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            MessageQueue.Enqueue(
                message,
                "CHIUDI",
                () => { },
                null,
                false,
                true,
                TimeSpan.FromSeconds(durationSeconds));
        });
    }
    
    #endregion
}
