using System.Collections.ObjectModel;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;
using PTRP.Services.Interfaces;

namespace PTRP.ViewModels;

/// <summary>
/// ViewModel principale per MainWindow
/// Gestisce:
/// - Navigazione tra le viste (tramite INavigationService)
/// - Menu dinamico basato su ruolo utente
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
    /// Elementi del menu di navigazione laterale
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<MenuItemViewModel> _navigationMenuItems = new();
    
    /// <summary>
    /// Menu item attualmente selezionato
    /// </summary>
    [ObservableProperty]
    private MenuItemViewModel? _selectedMenuItem;
    
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
        
        // Initialize navigation menu based on user role
        InitializeNavigationMenu();
        
        // Navigate to Dashboard by default
        // TODO: Check if first run, navigate to FirstRunView if needed (Issue #49)
        NavigateToDashboard();
    }
    
    /// <summary>
    /// Inizializza il menu di navigazione in base al ruolo utente
    /// </summary>
    private void InitializeNavigationMenu()
    {
        // TODO: Leggere ruolo da IConfigurationService in Issue #49
        var userRole = GetCurrentUserRole();
        
        NavigationMenuItems = userRole == "Coordinatore" 
            ? CreateCoordinatorMenu() 
            : CreateEducatorMenu();
    }
    
    /// <summary>
    /// Crea il menu per utente Coordinatore
    /// </summary>
    private ObservableCollection<MenuItemViewModel> CreateCoordinatorMenu()
    {
        return new ObservableCollection<MenuItemViewModel>
        {
            new MenuItemViewModel
            {
                Title = "Dashboard",
                IconKind = PackIconKind.ViewDashboard,
                // ViewModelType = typeof(DashboardViewModel) // TODO: Issue #50
            },
            new MenuItemViewModel
            {
                Title = "Pazienti",
                IconKind = PackIconKind.AccountGroup,
                // ViewModelType = typeof(PatientListViewModel) // TODO: Issue #51
            },
            new MenuItemViewModel
            {
                Title = "Educatori",
                IconKind = PackIconKind.AccountTie,
                // ViewModelType = typeof(OperatorListViewModel) // TODO: FASE 2
            },
            new MenuItemViewModel
            {
                Title = "Progetti",
                IconKind = PackIconKind.FolderMultiple,
                // ViewModelType = typeof(ProjectListViewModel) // TODO: FASE 2
            },
            new MenuItemViewModel
            {
                Title = "Calendario",
                IconKind = PackIconKind.Calendar,
                BadgeCount = 0, // TODO: Aggiornato dinamicamente
                // ViewModelType = typeof(CalendarViewModel) // TODO: FASE 2
            },
            new MenuItemViewModel
            {
                Title = "Sincronizzazione",
                IconKind = PackIconKind.Sync,
                // ViewModelType = typeof(SyncViewModel) // TODO: Issue #52
            },
            new MenuItemViewModel
            {
                Title = "Report",
                IconKind = PackIconKind.ChartBar,
                // ViewModelType = typeof(ReportViewModel) // TODO: FASE 2
            }
        };
    }
    
    /// <summary>
    /// Crea il menu per utente Educatore
    /// </summary>
    private ObservableCollection<MenuItemViewModel> CreateEducatorMenu()
    {
        return new ObservableCollection<MenuItemViewModel>
        {
            new MenuItemViewModel
            {
                Title = "I Miei Appuntamenti",
                IconKind = PackIconKind.CalendarCheck,
                BadgeCount = 0, // TODO: Aggiornato dinamicamente
                // ViewModelType = typeof(MyAppointmentsViewModel) // TODO: FASE 2
            },
            new MenuItemViewModel
            {
                Title = "I Miei Pazienti",
                IconKind = PackIconKind.AccountMultiple,
                // ViewModelType = typeof(MyPatientsViewModel) // TODO: FASE 2
            },
            new MenuItemViewModel
            {
                Title = "Visite Registrate",
                IconKind = PackIconKind.FileDocument,
                // ViewModelType = typeof(MyVisitsViewModel) // TODO: FASE 2
            },
            new MenuItemViewModel
            {
                Title = "Sincronizzazione",
                IconKind = PackIconKind.CloudSync,
                // ViewModelType = typeof(SyncViewModel) // TODO: Issue #52
            }
        };
    }
    
    /// <summary>
    /// Gestisce il cambio di ViewModel corrente dal NavigationService
    /// </summary>
    private void OnCurrentViewModelChanged(object? sender, object? viewModel)
    {
        // Cast to ViewModelBase (NavigationService returns object? to avoid circular dependency)
        CurrentViewModel = viewModel as ViewModelBase;
        CurrentPageTitle = CurrentViewModel?.DisplayName ?? "PTRP";
    }
    
    /// <summary>
    /// Gestisce la selezione di un menu item
    /// </summary>
    partial void OnSelectedMenuItemChanged(MenuItemViewModel? value)
    {
        if (value?.ViewModelType != null)
        {
            // Naviga al ViewModel associato usando reflection per chiamare il metodo generico
            try
            {
                var navigateToMethod = _navigationService.GetType()
                    .GetMethod(nameof(INavigationService.NavigateTo))
                    ?.MakeGenericMethod(value.ViewModelType);
                    
                navigateToMethod?.Invoke(_navigationService, null);
            }
            catch (TargetInvocationException ex)
            {
                // Se il ViewModel non è ancora implementato, mostra placeholder
                ShowInfo($"{value.Title} - In sviluppo");
                CurrentPageTitle = value.Title;
            }
        }
        else if (value?.CustomCommand != null)
        {
            // Esegui comando custom se presente
            if (value.CustomCommand.CanExecute(null))
            {
                value.CustomCommand.Execute(null);
            }
        }
        else if (value != null)
        {
            // Menu item senza ViewModelType (ancora da implementare)
            ShowInfo($"{value.Title} - In sviluppo");
            CurrentPageTitle = value.Title;
        }
    }
    
    /// <summary>
    /// Aggiorna il badge su un menu item specifico
    /// </summary>
    /// <param name="title">Titolo del menu item</param>
    /// <param name="count">Nuovo valore del badge</param>
    public void UpdateMenuItemBadge(string title, int count)
    {
        var menuItem = NavigationMenuItems.FirstOrDefault(x => x.Title == title);
        if (menuItem != null)
        {
            menuItem.BadgeCount = count;
            menuItem.HasBadge = count > 0;
        }
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
    
    /// <summary>
    /// Ottiene il ruolo dell'utente corrente
    /// </summary>
    private string GetCurrentUserRole()
    {
        // TODO: Leggere da IConfigurationService in Issue #49
        // Per ora ritorna "Coordinatore" come default
        return "Coordinatore";
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
/// Rappresenta un elemento del menu di navigazione
/// (Definito qui per evitare dipendenze circolari con PTRP.App)
/// </summary>
public partial class MenuItemViewModel : ObservableObject
{
    [ObservableProperty]
    private string _title = string.Empty;
    
    [ObservableProperty]
    private PackIconKind _iconKind;
    
    [ObservableProperty]
    private int _badgeCount = 0;
    
    [ObservableProperty]
    private bool _hasBadge = false;
    
    public Type? ViewModelType { get; set; }
    public System.Windows.Input.ICommand? CustomCommand { get; set; }
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
