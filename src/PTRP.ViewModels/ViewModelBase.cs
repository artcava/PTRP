using CommunityToolkit.Mvvm.ComponentModel;

namespace PTRP.ViewModels;

/// <summary>
/// Classe base per tutti i ViewModel
/// Fornisce funzionalità comuni:
/// - Implementazione INotifyPropertyChanged (CommunityToolkit)
/// - DisplayName per titolo pagina
/// </summary>
public abstract class ViewModelBase : ObservableObject
{
    /// <summary>
    /// Nome visualizzato del ViewModel (usato per titolo pagina)
    /// </summary>
    public virtual string DisplayName => GetType().Name.Replace("ViewModel", "");
}
