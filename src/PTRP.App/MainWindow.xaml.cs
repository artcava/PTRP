using System;
using System.ComponentModel;
using System.Windows;
using MaterialDesignThemes.Wpf;
using PTRP.App.Infrastructure;
using PTRP.ViewModels;

namespace PTRP.App;

/// <summary>
/// MainWindow.xaml.cs
/// Code-behind per la finestra principale dell'applicazione
/// 
/// Gestisce:
/// 1. Collegamento del ViewModel (binding)
/// 2. Setup MessageQueue per Snackbar
/// 3. Gestione eventi notifica dal ViewModel
/// 4. Risoluzione View tramite ViewLocator (Issue #94)
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private readonly ViewLocator _viewLocator;
    
    /// <summary>
    /// Costruttore - riceve il ViewModel e ViewLocator via Dependency Injection
    /// </summary>
    public MainWindow(MainViewModel viewModel, ViewLocator viewLocator)
    {
        InitializeComponent();

        _viewModel = viewModel;
        _viewLocator = viewLocator;
        
        // Imposta il ViewModel come DataContext
        DataContext = _viewModel;
        
        // Setup Snackbar MessageQueue
        MainSnackbar.MessageQueue = new SnackbarMessageQueue(TimeSpan.FromSeconds(3));
        
        // Subscribe to notification events
        _viewModel.NotificationRequested += OnNotificationRequested;
        
        // Subscribe to CurrentViewModel changes to resolve Views via ViewLocator
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }
    
    /// <summary>
    /// Intercetta i cambiamenti di CurrentViewModel e risolve la View tramite ViewLocator.
    /// Issue #94: Permette Views con costruttori DI invece di DataTemplate statici.
    /// </summary>
    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.CurrentViewModel))
        {
            // Risolvi la View per il ViewModel corrente tramite ViewLocator
            var view = _viewLocator.CreateViewForViewModel(_viewModel.CurrentViewModel);
            
            // Imposta la View nel ContentControl
            if (view != null)
            {
                ContentArea.Content = view;
            }
            else
            {
                // ViewModel sconosciuto o non implementato - mostra placeholder
                ContentArea.Content = null;
            }
        }
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
                    (obj) => { }, // Action button handler (takes object? parameter)
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
        _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        base.OnClosed(e);
    }
}
