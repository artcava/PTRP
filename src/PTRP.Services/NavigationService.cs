using Microsoft.Extensions.DependencyInjection;
using PTRP.Services.Interfaces;
using PTRP.ViewModels;

namespace PTRP.Services;

/// <summary>
/// Implementazione del servizio di navigazione
/// Gestisce la navigazione tra ViewModels utilizzando Dependency Injection
/// </summary>
public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Stack<ViewModelBase> _navigationHistory = new();
    private readonly object _lock = new();
    
    private ViewModelBase? _currentViewModel;
    
    /// <summary>
    /// ViewModel corrente visualizzato
    /// </summary>
    public ViewModelBase? CurrentViewModel
    {
        get => _currentViewModel;
        private set
        {
            if (_currentViewModel != value)
            {
                _currentViewModel = value;
                CurrentViewModelChanged?.Invoke(this, _currentViewModel);
            }
        }
    }
    
    /// <summary>
    /// Indica se è possibile navigare indietro
    /// </summary>
    public bool CanNavigateBack
    {
        get
        {
            lock (_lock)
            {
                return _navigationHistory.Count > 0;
            }
        }
    }
    
    /// <summary>
    /// Evento sollevato quando il ViewModel corrente cambia
    /// </summary>
    public event EventHandler<ViewModelBase?>? CurrentViewModelChanged;
    
    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    /// <summary>
    /// Naviga a un ViewModel specifico
    /// Risolve il ViewModel tramite DI e lo imposta come corrente
    /// </summary>
    public TViewModel NavigateTo<TViewModel>() where TViewModel : ViewModelBase
    {
        lock (_lock)
        {
            // Salva il ViewModel corrente nella history (se esiste)
            if (CurrentViewModel != null)
            {
                _navigationHistory.Push(CurrentViewModel);
            }
            
            // Risolvi il nuovo ViewModel dal DI container
            var viewModel = _serviceProvider.GetRequiredService<TViewModel>();
            
            // Imposta come corrente
            CurrentViewModel = viewModel;
            
            return viewModel;
        }
    }
    
    /// <summary>
    /// Naviga al ViewModel precedente nella history
    /// </summary>
    public void NavigateBack()
    {
        lock (_lock)
        {
            if (_navigationHistory.Count > 0)
            {
                var previousViewModel = _navigationHistory.Pop();
                CurrentViewModel = previousViewModel;
            }
        }
    }
    
    /// <summary>
    /// Pulisce la history di navigazione
    /// Utile per reset o logout
    /// </summary>
    public void ClearHistory()
    {
        lock (_lock)
        {
            _navigationHistory.Clear();
        }
    }
}
