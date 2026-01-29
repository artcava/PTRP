using PTRP.App.ViewModels;
using System.Windows;

namespace PTRP.App
{
    /// <summary>
    /// MainWindow.xaml.cs
    /// Code-behind per la finestra principale dell'applicazione
    /// 
    /// Gestisce:
    /// 1. Collegamento del ViewModel (binding)
    /// 2. EventHandler per lifecycle (caricamento finestra)
    /// 
    /// NOTA: La logica di visibilità della selezione è stata spostata nel XAML
    /// usando binding e converter, rispettando il pattern MVVM.
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
