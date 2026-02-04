using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PTRP.ViewModels.Projects;

/// <summary>
/// ViewModel per rappresentare un educatore selezionabile nella form del progetto.
/// Permette la selezione multipla degli educatori da assegnare al progetto.
/// </summary>
public class SelectableEducatorViewModel : INotifyPropertyChanged
{
    private bool _isSelected;

    /// <summary>
    /// ID dell'educatore
    /// </summary>
    public Guid EducatorId { get; }

    /// <summary>
    /// Nome completo dell'educatore
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Ruolo dell'educatore (es. "Educatore Professionale", "Coordinatore")
    /// </summary>
    public string Role { get; }

    /// <summary>
    /// Indica se l'educatore è selezionato per il progetto
    /// </summary>
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }
    }

    public SelectableEducatorViewModel(Guid educatorId, string name, string role)
    {
        EducatorId = educatorId;
        Name = name;
        Role = role;
        _isSelected = false;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
