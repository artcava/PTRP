using PTRP.App.ViewModels;
using System.Windows;

namespace PTRP.App
{
    /// <summary>
    /// MainWindow.xaml.cs
    /// Code-behind minimalista - solo per il collegamento del ViewModel
    /// 
    /// IMPORTANTE: La logica rimane nel ViewModel (MainWindowViewModel)
    /// Questo file contiene solo:
    /// 1. Collegamento del DataContext (binding)
    /// 2. EventHandler per lifecycle (caricamento finestra)
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Costruttore - riceve il ViewModel via Dependency Injection
        /// </summary>
        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();

            // Imposta il ViewModel come DataContext
            // Questo abilita il binding XAML alle proprietà del ViewModel
            DataContext = viewModel;
        }

        /// <summary>
        /// Event handler per il caricamento della finestra
        /// Carica i dati iniziali (pazienti)
        /// </summary>
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Accedi al ViewModel tramite DataContext
            if (DataContext is MainWindowViewModel viewModel)
            {
                // Esegui il comando per caricare i pazienti
                await viewModel.LoadPatientsCommand.ExecuteAsync(null);
            }
        }
    }
}
