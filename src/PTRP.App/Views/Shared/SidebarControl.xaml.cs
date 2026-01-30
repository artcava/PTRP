using System.Windows.Controls;

namespace PTRP.App.Views.Shared;

/// <summary>
/// Sidebar con menu di navigazione dinamico
/// DataContext: MainViewModel (ereditato da MainWindow)
/// </summary>
public partial class SidebarControl : UserControl
{
    public SidebarControl()
    {
        InitializeComponent();
    }
}
