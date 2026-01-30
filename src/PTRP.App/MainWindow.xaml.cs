using PTRP.ViewModels;
using System.Windows;

namespace PTRP.App;

/// <summary>
/// MainWindow.xaml.cs
/// Code-behind per la finestra principale dell'applicazione
/// 
/// Gestisce:
/// 1. Collegamento del ViewModel (binding)
/// 2. Nessuna logica UI (delegata al ViewModel)
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// Costruttore - riceve il ViewModel via Dependency Injection
    /// </summary>
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();

        // Imposta il ViewModel come DataContext
        // Questo abilita il binding XAML alle proprietà del ViewModel
        DataContext = viewModel;
    }
}
