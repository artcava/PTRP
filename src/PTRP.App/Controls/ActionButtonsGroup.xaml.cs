using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PTRP.App.Controls;

/// <summary>
/// Gruppo standard di pulsanti per form (Save/Cancel o Edit/Delete).
/// </summary>
/// <remarks>
/// Garantisce consistenza visiva in tutti i form dell'applicazione.
/// Due modalità:
/// - Save/Cancel: per form di creazione/modifica
/// - Edit/Delete: per detail view con azioni su entità esistente
/// </remarks>
/// <example>
/// <code>
/// &lt;!-- Form Mode --&gt;
/// &lt;controls:ActionButtonsGroup 
///     ShowSaveCancel="True"
///     SaveCommand="{Binding SaveCommand}"
///     CancelCommand="{Binding CancelCommand}" /&gt;
/// 
/// &lt;!-- Detail Mode --&gt;
/// &lt;controls:ActionButtonsGroup 
///     ShowEditDelete="True"
///     EditCommand="{Binding EditCommand}"
///     DeleteCommand="{Binding DeleteCommand}" /&gt;
/// </code>
/// </example>
public partial class ActionButtonsGroup : UserControl
{
    public static readonly DependencyProperty ShowSaveCancelProperty =
        DependencyProperty.Register(
            nameof(ShowSaveCancel),
            typeof(bool),
            typeof(ActionButtonsGroup),
            new PropertyMetadata(false));

    public static readonly DependencyProperty ShowEditDeleteProperty =
        DependencyProperty.Register(
            nameof(ShowEditDelete),
            typeof(bool),
            typeof(ActionButtonsGroup),
            new PropertyMetadata(false));

    public static readonly DependencyProperty SaveCommandProperty =
        DependencyProperty.Register(
            nameof(SaveCommand),
            typeof(ICommand),
            typeof(ActionButtonsGroup),
            new PropertyMetadata(null));

    public static readonly DependencyProperty CancelCommandProperty =
        DependencyProperty.Register(
            nameof(CancelCommand),
            typeof(ICommand),
            typeof(ActionButtonsGroup),
            new PropertyMetadata(null));

    public static readonly DependencyProperty EditCommandProperty =
        DependencyProperty.Register(
            nameof(EditCommand),
            typeof(ICommand),
            typeof(ActionButtonsGroup),
            new PropertyMetadata(null));

    public static readonly DependencyProperty DeleteCommandProperty =
        DependencyProperty.Register(
            nameof(DeleteCommand),
            typeof(ICommand),
            typeof(ActionButtonsGroup),
            new PropertyMetadata(null));

    public static readonly DependencyProperty SaveTextProperty =
        DependencyProperty.Register(
            nameof(SaveText),
            typeof(string),
            typeof(ActionButtonsGroup),
            new PropertyMetadata("Salva"));

    public static readonly DependencyProperty CancelTextProperty =
        DependencyProperty.Register(
            nameof(CancelText),
            typeof(string),
            typeof(ActionButtonsGroup),
            new PropertyMetadata("Annulla"));

    public static readonly DependencyProperty EditTextProperty =
        DependencyProperty.Register(
            nameof(EditText),
            typeof(string),
            typeof(ActionButtonsGroup),
            new PropertyMetadata("Modifica"));

    public static readonly DependencyProperty DeleteTextProperty =
        DependencyProperty.Register(
            nameof(DeleteText),
            typeof(string),
            typeof(ActionButtonsGroup),
            new PropertyMetadata("Elimina"));

    public static readonly DependencyProperty ButtonAlignmentProperty =
        DependencyProperty.Register(
            nameof(ButtonAlignment),
            typeof(HorizontalAlignment),
            typeof(ActionButtonsGroup),
            new PropertyMetadata(HorizontalAlignment.Right));

    /// <summary>
    /// Mostra i pulsanti Save/Cancel (modalità form).
    /// </summary>
    public bool ShowSaveCancel
    {
        get => (bool)GetValue(ShowSaveCancelProperty);
        set => SetValue(ShowSaveCancelProperty, value);
    }

    /// <summary>
    /// Mostra i pulsanti Edit/Delete (modalità detail).
    /// </summary>
    public bool ShowEditDelete
    {
        get => (bool)GetValue(ShowEditDeleteProperty);
        set => SetValue(ShowEditDeleteProperty, value);
    }

    /// <summary>
    /// Comando per il pulsante Save.
    /// </summary>
    public ICommand? SaveCommand
    {
        get => (ICommand?)GetValue(SaveCommandProperty);
        set => SetValue(SaveCommandProperty, value);
    }

    /// <summary>
    /// Comando per il pulsante Cancel.
    /// </summary>
    public ICommand? CancelCommand
    {
        get => (ICommand?)GetValue(CancelCommandProperty);
        set => SetValue(CancelCommandProperty, value);
    }

    /// <summary>
    /// Comando per il pulsante Edit.
    /// </summary>
    public ICommand? EditCommand
    {
        get => (ICommand?)GetValue(EditCommandProperty);
        set => SetValue(EditCommandProperty, value);
    }

    /// <summary>
    /// Comando per il pulsante Delete.
    /// </summary>
    public ICommand? DeleteCommand
    {
        get => (ICommand?)GetValue(DeleteCommandProperty);
        set => SetValue(DeleteCommandProperty, value);
    }

    /// <summary>
    /// Testo del pulsante Save (default: "Salva").
    /// </summary>
    public string SaveText
    {
        get => (string)GetValue(SaveTextProperty);
        set => SetValue(SaveTextProperty, value);
    }

    /// <summary>
    /// Testo del pulsante Cancel (default: "Annulla").
    /// </summary>
    public string CancelText
    {
        get => (string)GetValue(CancelTextProperty);
        set => SetValue(CancelTextProperty, value);
    }

    /// <summary>
    /// Testo del pulsante Edit (default: "Modifica").
    /// </summary>
    public string EditText
    {
        get => (string)GetValue(EditTextProperty);
        set => SetValue(EditTextProperty, value);
    }

    /// <summary>
    /// Testo del pulsante Delete (default: "Elimina").
    /// </summary>
    public string DeleteText
    {
        get => (string)GetValue(DeleteTextProperty);
        set => SetValue(DeleteTextProperty, value);
    }

    /// <summary>
    /// Allineamento orizzontale del gruppo pulsanti (default: Right).
    /// </summary>
    public HorizontalAlignment ButtonAlignment
    {
        get => (HorizontalAlignment)GetValue(ButtonAlignmentProperty);
        set => SetValue(ButtonAlignmentProperty, value);
    }

    public ActionButtonsGroup()
    {
        InitializeComponent();
    }
}
