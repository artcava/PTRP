namespace PTRP.Services.Interfaces;

/// <summary>
/// Servizio per la navigazione tra viste nell'applicazione
/// Gestisce:
/// - Navigazione tra ViewModels
/// - History stack per back navigation
/// - Risoluzione ViewModels via DI
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// ViewModel corrente visualizzato
    /// </summary>
    object? CurrentViewModel { get; }
    
    /// <summary>
    /// Indica se è possibile navigare indietro
    /// </summary>
    bool CanNavigateBack { get; }
    
    /// <summary>
    /// Evento sollevato quando il ViewModel corrente cambia
    /// </summary>
    event EventHandler<object?>? CurrentViewModelChanged;
    
    /// <summary>
    /// Naviga a un ViewModel specifico
    /// </summary>
    /// <typeparam name="TViewModel">Tipo del ViewModel di destinazione</typeparam>
    /// <returns>Il ViewModel creato</returns>
    TViewModel NavigateTo<TViewModel>() where TViewModel : class;
    
    /// <summary>
    /// Naviga al ViewModel precedente nella history
    /// </summary>
    void NavigateBack();
    
    /// <summary>
    /// Pulisce la history di navigazione
    /// </summary>
    void ClearHistory();
}
