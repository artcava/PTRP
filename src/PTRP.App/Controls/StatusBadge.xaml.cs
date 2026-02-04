using System.Windows;
using System.Windows.Controls;

namespace PTRP.App.Controls;

/// <summary>
/// Badge colorato per visualizzare lo stato di un'entità (Progetto, Appuntamento, ecc.).
/// Auto-styling basato sul valore dello stato.
/// </summary>
/// <remarks>
/// Supporta TherapyProjectState, AppointmentStatus, PresenceStatus.
/// Il colore viene determinato automaticamente tramite ProjectStateToColorConverter.
/// </remarks>
/// <example>
/// <code>
/// &lt;controls:StatusBadge Status="Active" StatusDisplay="Attivo" /&gt;
/// &lt;controls:StatusBadge Status="Completed" StatusDisplay="Completato" /&gt;
/// </code>
/// </example>
public partial class StatusBadge : UserControl
{
    /// <summary>
    /// Dependency property per lo stato (enum convertito a string).
    /// </summary>
    public static readonly DependencyProperty StatusProperty =
        DependencyProperty.Register(
            nameof(Status),
            typeof(string),
            typeof(StatusBadge),
            new PropertyMetadata(string.Empty));

    /// <summary>
    /// Dependency property per il testo visualizzato.
    /// </summary>
    public static readonly DependencyProperty StatusDisplayProperty =
        DependencyProperty.Register(
            nameof(StatusDisplay),
            typeof(string),
            typeof(StatusBadge),
            new PropertyMetadata(string.Empty));

    /// <summary>
    /// Valore dello stato (es: "Active", "Completed", "Scheduled").
    /// </summary>
    public string Status
    {
        get => (string)GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    /// <summary>
    /// Testo da visualizzare nel badge (es: "Attivo", "Completato", "Programmato").
    /// </summary>
    public string StatusDisplay
    {
        get => (string)GetValue(StatusDisplayProperty);
        set => SetValue(StatusDisplayProperty, value);
    }

    public StatusBadge()
    {
        InitializeComponent();
    }
}
