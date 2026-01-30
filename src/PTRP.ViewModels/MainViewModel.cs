using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PTRP.Services.Interfaces;

namespace PTRP.ViewModels;

/// <summary>
/// ViewModel principale per MainWindow
/// Gestisce:
/// - Navigazione tra le viste (tramite INavigationService)
/// - Stato utente corrente
/// - Eventi di notifica (delegati alla View)
/// </summary>
public partial class MainViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    
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
    /// Evento per notifiche da mostrare nella View
    /// </summary>
    public event EventHandler<NotificationEventArgs>? NotificationRequested;
    
    public MainViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
        
        // Subscribe to navigation changes
        _navigationService.CurrentViewModelChanged += OnCurrentViewModelChanged;
        
        // TODO: Load user profile from configuration service (Issue #49)
        LoadUserProfile();
        
        // Navigate to Dashboard by default
        // TODO: Check if first run, navigate to FirstRunView if needed (Issue #49)
        NavigateToDashboard();
    }
    
    /// <summary>
    /// Gestisce il cambio di ViewModel corrente dal NavigationService
    /// </summary>
    private void OnCurrentViewModelChanged(object? sender, ViewModelBase? viewModel)
    {
        CurrentViewModel = viewModel;
        CurrentPageTitle = viewModel?.DisplayName ?? "PTRP";
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
        // TODO: Implementare DashboardViewModel in Issue #50
        // Per ora mostra messaggio
        ShowInfo("Dashboard - In sviluppo (Issue #50)");
        CurrentPageTitle = "Dashboard";
        
        // Quando DashboardViewModel sarà implementato:
        // _navigationService.NavigateTo<DashboardViewModel>();
    }
    
    [RelayCommand]
    private void NavigateToPatients()
    {
        // TODO: Implementare PatientListViewModel in Issue #51
        // Per ora mostra messaggio
        ShowInfo("Lista Pazienti - In sviluppo (Issue #51)");
        CurrentPageTitle = "Pazienti";
        
        // Quando PatientListViewModel sarà implementato:
        // _navigationService.NavigateTo<PatientListViewModel>();
    }
    
    [RelayCommand]
    private void NavigateToProjects()
    {
        ShowInfo("Progetti - In sviluppo (FASE 2)");
        CurrentPageTitle = "Progetti";
    }
    
    [RelayCommand]
    private void NavigateToEducators()
    {
        ShowInfo("Educatori - In sviluppo (FASE 2)");
        CurrentPageTitle = "Educatori";
    }
    
    [RelayCommand]
    private void NavigateToCalendar()
    {
        ShowInfo("Calendario - In sviluppo (FASE 2)");
        CurrentPageTitle = "Calendario";
    }
    
    [RelayCommand]
    private void NavigateToSync()
    {
        // TODO: Implementare SyncViewModel in Issue #52
        ShowInfo("Sync - In sviluppo (Issue #52)");
        CurrentPageTitle = "Sincronizzazione";
        
        // Quando SyncViewModel sarà implementato:
        // _navigationService.NavigateTo<SyncViewModel>();
    }
    
    [RelayCommand]
    private void NavigateToReports()
    {
        ShowInfo("Report - In sviluppo (FASE 2)");
        CurrentPageTitle = "Report";
    }
    
    [RelayCommand]
    private void NavigateToSettings()
    {
        ShowInfo("Impostazioni - In sviluppo (FASE 4)");
        CurrentPageTitle = "Impostazioni";
    }
    
    #endregion
    
    #region Notification Methods
    
    /// <summary>
    /// Mostra messaggio informativo
    /// </summary>
    public void ShowInfo(string message)
    {
        OnNotificationRequested(new NotificationEventArgs
        {
            Message = message,
            Type = NotificationType.Info
        });
    }
    
    /// <summary>
    /// Mostra messaggio di successo
    /// </summary>
    public void ShowSuccess(string message)
    {
        OnNotificationRequested(new NotificationEventArgs
        {
            Message = message,
            Type = NotificationType.Success,
            DurationSeconds = 2
        });
    }
    
    /// <summary>
    /// Mostra messaggio di errore
    /// </summary>
    public void ShowError(string message, int durationSeconds = 5)
    {
        OnNotificationRequested(new NotificationEventArgs
        {
            Message = message,
            Type = NotificationType.Error,
            DurationSeconds = durationSeconds,
            ShowActionButton = true,
            ActionButtonText = "CHIUDI"
        });
    }
    
    /// <summary>
    /// Solleva l'evento NotificationRequested
    /// </summary>
    protected virtual void OnNotificationRequested(NotificationEventArgs e)
    {
        NotificationRequested?.Invoke(this, e);
    }
    
    #endregion
}

/// <summary>
/// Tipo di notifica
/// </summary>
public enum NotificationType
{
    Info,
    Success,
    Error,
    Warning
}

/// <summary>
/// Argomenti per evento di notifica
/// </summary>
public class NotificationEventArgs : EventArgs
{
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public int DurationSeconds { get; set; } = 3;
    public bool ShowActionButton { get; set; }
    public string ActionButtonText { get; set; } = string.Empty;
}
