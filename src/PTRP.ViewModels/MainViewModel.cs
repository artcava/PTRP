using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
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
/// - StatusBar (messaggi, loading, database status, sync time)
/// </summary>
public partial class MainViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    
    public override string DisplayName => "PTRP";
    
    #region Current Page Properties
    
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
    
    #endregion
    
    #region User Properties
    
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
    
    #endregion
    
    #region Navigation Menu Properties
    
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
    
    #endregion
    
    #region StatusBar Properties
    
    /// <summary>
    /// Messaggio di stato temporaneo (success/error/info)
    /// </summary>
    [ObservableProperty]
    private string _statusMessage = "Pronto";
    
    /// <summary>
    /// Indica se il messaggio di stato è visibile
    /// </summary>
    [ObservableProperty]
    private bool _hasStatusMessage = false;
    
    /// <summary>
    /// Icona del messaggio di stato
    /// </summary>
    [ObservableProperty]
    private PackIconKind _statusIcon = PackIconKind.Information;
    
    /// <summary>
    /// Colore dell'icona di stato
    /// </summary>
    [ObservableProperty]
    private Brush _statusIconColor = Brushes.Gray;
    
    /// <summary>
    /// Indica se è in corso un'operazione (mostra progress bar)
    /// </summary>
    [ObservableProperty]
    private bool _isLoading = false;
    
    /// <summary>
    /// Stato connessione database
    /// </summary>
    [ObservableProperty]
    private string _databaseStatus = "Connesso";
    
    /// <summary>
    /// Tooltip per database status
    /// </summary>
    [ObservableProperty]
    private string _databaseStatusTooltip = "Database SQLite locale - Crittografato";
    
    /// <summary>
    /// Colore indicatore database
    /// </summary>
    [ObservableProperty]
    private Brush _databaseStatusColor = Brushes.Green;
    
    /// <summary>
    /// Display ultima sincronizzazione (formato relativo)
    /// </summary>
    [ObservableProperty]
    private string _lastSyncTimeDisplay = "Mai sincronizzato";
    
    #endregion
    
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
        
        // Initialize database status (placeholder)
        UpdateDatabaseStatus(true, 2_097_152); // 2 MB placeholder
        
        // Initialize last sync (placeholder)
        UpdateLastSyncTime(null);
        
        // Navigate to Dashboard by default
        // TODO: Check if first run, navigate to FirstRunView if needed (Issue #49)
        NavigateToDashboard();
    }
    
    #region Navigation Menu Methods
    
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
            catch (TargetInvocationException)
            {
                // Se il ViewModel non è ancora implementato, mostra placeholder
                ShowInfoMessage($"{value.Title} - In sviluppo");
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
            ShowInfoMessage($"{value.Title} - In sviluppo");
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
    
    #endregion
    
    #region User Profile Methods
    
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
    
    #endregion
    
    #region StatusBar Methods
    
    /// <summary>
    /// Mostra messaggio di successo nella status bar con auto-hide
    /// </summary>
    public void ShowSuccessMessage(string message, int durationMs = 3000)
    {
        StatusMessage = message;
        StatusIcon = PackIconKind.CheckCircle;
        StatusIconColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#28A745")!);
        HasStatusMessage = true;
        
        // Auto-hide dopo durata specificata
        Task.Delay(durationMs).ContinueWith(_ => 
        {
            Application.Current.Dispatcher.Invoke(() => 
            {
                HasStatusMessage = false;
                StatusMessage = "Pronto";
            });
        });
    }
    
    /// <summary>
    /// Mostra messaggio di errore nella status bar con auto-hide
    /// </summary>
    public void ShowErrorMessage(string message, int durationMs = 5000)
    {
        StatusMessage = message;
        StatusIcon = PackIconKind.AlertCircle;
        StatusIconColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DC3545")!);
        HasStatusMessage = true;
        
        Task.Delay(durationMs).ContinueWith(_ => 
        {
            Application.Current.Dispatcher.Invoke(() => 
            {
                HasStatusMessage = false;
                StatusMessage = "Pronto";
            });
        });
    }
    
    /// <summary>
    /// Mostra messaggio informativo nella status bar con auto-hide
    /// </summary>
    public void ShowInfoMessage(string message, int durationMs = 3000)
    {
        StatusMessage = message;
        StatusIcon = PackIconKind.Information;
        StatusIconColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#007ACC")!);
        HasStatusMessage = true;
        
        Task.Delay(durationMs).ContinueWith(_ => 
        {
            Application.Current.Dispatcher.Invoke(() => 
            {
                HasStatusMessage = false;
                StatusMessage = "Pronto";
            });
        });
    }
    
    /// <summary>
    /// Aggiorna lo stato della connessione al database
    /// </summary>
    public void UpdateDatabaseStatus(bool isConnected, long sizeBytes)
    {
        if (isConnected)
        {
            var sizeMb = sizeBytes / (1024.0 * 1024.0);
            DatabaseStatus = $"Connesso ({sizeMb:F2} MB)";
            DatabaseStatusColor = Brushes.Green;
            DatabaseStatusTooltip = $"Database SQLite locale - Crittografato - Dimensione: {sizeMb:F2} MB";
        }
        else
        {
            DatabaseStatus = "Disconnesso";
            DatabaseStatusColor = Brushes.Red;
            DatabaseStatusTooltip = "Database non disponibile";
        }
    }
    
    /// <summary>
    /// Aggiorna il display del timestamp dell'ultima sincronizzazione
    /// </summary>
    public void UpdateLastSyncTime(DateTime? lastSyncTime)
    {
        if (lastSyncTime.HasValue)
        {
            var elapsed = DateTime.Now - lastSyncTime.Value;
            
            LastSyncTimeDisplay = elapsed.TotalMinutes < 1 
                ? "Pochi secondi fa"
                : elapsed.TotalHours < 1 
                    ? $"{(int)elapsed.TotalMinutes} minuti fa"
                    : elapsed.TotalDays < 1 
                        ? $"{(int)elapsed.TotalHours} ore fa"
                        : $"{lastSyncTime.Value:dd/MM/yyyy HH:mm}";
        }
        else
        {
            LastSyncTimeDisplay = "Mai sincronizzato";
        }
    }
    
    #endregion
    
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
    
    #region Notification Methods (Snackbar - mantenuto per compatibilità)
    
    /// <summary>
    /// Mostra messaggio informativo tramite Snackbar
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
    /// Mostra messaggio di successo tramite Snackbar
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
    /// Mostra messaggio di errore tramite Snackbar
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
