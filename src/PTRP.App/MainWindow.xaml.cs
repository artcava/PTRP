using MaterialDesignThemes.Wpf;
using PTRP.ViewModels;
using System.Windows;

namespace PTRP.App;

/// <summary>
/// MainWindow.xaml.cs
/// Code-behind per la finestra principale dell'applicazione
/// 
/// Gestisce:
/// 1. Collegamento del ViewModel (binding)
/// 2. Setup MessageQueue per Snackbar
/// 3. Gestione eventi notifica dal ViewModel
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    
    /// <summary>
    /// Costruttore - riceve il ViewModel via Dependency Injection
    /// </summary>
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        
        // Imposta il ViewModel come DataContext
        DataContext = _viewModel;
        
        // Setup Snackbar MessageQueue
        MainSnackbar.MessageQueue = new SnackbarMessageQueue(TimeSpan.FromSeconds(3));
        
        // Subscribe to notification events
        _viewModel.NotificationRequested += OnNotificationRequested;
    }
    
    /// <summary>
    /// Gestisce le richieste di notifica dal ViewModel
    /// </summary>
    private void OnNotificationRequested(object? sender, NotificationEventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            if (MainSnackbar.MessageQueue == null)
                return;
                
            if (e.ShowActionButton)
            {
                MainSnackbar.MessageQueue.Enqueue(
                    e.Message,
                    e.ActionButtonText,
                    () => { },
                    null,
                    false,
                    e.Type == NotificationType.Error,
                    TimeSpan.FromSeconds(e.DurationSeconds));
            }
            else
            {
                MainSnackbar.MessageQueue.Enqueue(
                    e.Message,
                    null,
                    null,
                    null,
                    false,
                    false,
                    TimeSpan.FromSeconds(e.DurationSeconds));
            }
        });
    }
    
    /// <summary>
    /// Cleanup quando la finestra viene chiusa
    /// </summary>
    protected override void OnClosed(EventArgs e)
    {
        _viewModel.NotificationRequested -= OnNotificationRequested;
        base.OnClosed(e);
    }
}
