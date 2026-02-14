using CommunityToolkit.Mvvm.ComponentModel;

namespace PTRP.ViewModels.Visits;

/// <summary>
/// Rappresenta un operatore nella checkbox list del form registrazione visita
/// </summary>
public partial class OperatorCheckboxViewModel : ObservableObject
{
    /// <summary>
    /// ID dell'educatore
    /// </summary>
    [ObservableProperty]
    private Guid _educatorId;

    /// <summary>
    /// Nome completo educatore
    /// </summary>
    [ObservableProperty]
    private string _fullName = string.Empty;

    /// <summary>
    /// Indica se questo è l'operatore che sta registrando la visita (io)
    /// </summary>
    [ObservableProperty]
    private bool _isCurrentUser;

    /// <summary>
    /// Indica se l'operatore era presente durante la visita
    /// </summary>
    [ObservableProperty]
    private bool _isSelected;

    /// <summary>
    /// Display name con indicatore (io) se è l'utente corrente
    /// </summary>
    public string DisplayName => IsCurrentUser ? $"{FullName} (io)" : FullName;

    partial void OnIsCurrentUserChanged(bool value)
    {
        OnPropertyChanged(nameof(DisplayName));
    }

    partial void OnFullNameChanged(string value)
    {
        OnPropertyChanged(nameof(DisplayName));
    }
}
