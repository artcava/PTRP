using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using MaterialDesignThemes.Wpf;

namespace PTRP.App.Models;

/// <summary>
/// Rappresenta un elemento del menu di navigazione laterale
/// Supporta:
/// - Navigazione type-safe tramite ViewModelType
/// - Badge con contatori dinamici
/// - Comandi custom per azioni speciali
/// </summary>
public partial class MenuItemViewModel : ObservableObject
{
    /// <summary>
    /// Titolo visualizzato nel menu
    /// </summary>
    [ObservableProperty]
    private string _title = string.Empty;
    
    /// <summary>
    /// Icona Material Design
    /// </summary>
    [ObservableProperty]
    private PackIconKind _iconKind;
    
    /// <summary>
    /// Numero da visualizzare nel badge
    /// </summary>
    [ObservableProperty]
    private int _badgeCount = 0;
    
    /// <summary>
    /// Indica se mostrare il badge
    /// </summary>
    [ObservableProperty]
    private bool _hasBadge = false;
    
    /// <summary>
    /// Tipo del ViewModel da navigare quando questo menu item viene selezionato
    /// Null se usa CustomCommand
    /// </summary>
    public Type? ViewModelType { get; set; }
    
    /// <summary>
    /// Comando custom se non usa navigazione standard
    /// Esempio: Logout, Settings dialog, ecc.
    /// </summary>
    public ICommand? CustomCommand { get; set; }
}
