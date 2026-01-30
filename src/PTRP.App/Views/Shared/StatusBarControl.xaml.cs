using System.Windows.Controls;

namespace PTRP.App.Views.Shared;

/// <summary>
/// Status bar (bottom) con:
/// - Messaggi di stato temporanei (success/error/info)
/// - Database status indicator
/// - Last sync timestamp
/// - Loading progress bar
/// DataContext: MainViewModel (ereditato da MainWindow)
/// </summary>
public partial class StatusBarControl : UserControl
{
    public StatusBarControl()
    {
        InitializeComponent();
    }
}
