using PTRP.App.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace PTRP.App
{
    /// <summary>
    /// MainWindow.xaml.cs
    /// Code-behind per la finestra principale dell'applicazione
    /// 
    /// Gestisce:
    /// 1. Collegamento del ViewModel (binding)
    /// 2. EventHandler per lifecycle (caricamento finestra)
    /// 3. Logica della visibilita dell'indicatore chevron nella selezione righe
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
            // Questo abilita il binding XAML alle proprieta del ViewModel
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

        /// <summary>
        /// Event handler per la selezione di celle nel DataGrid
        /// Gestisce la visibilita dell'indicatore chevron nella prima colonna
        /// </summary>
        private void PatientsDataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (sender is not DataGrid dataGrid)
                return;

            // Nascondi tutti gli indicatori (frecce) nelle righe
            HideAllSelectionIndicators();

            // Mostra l'indicatore solo nella riga selezionata
            if (dataGrid.SelectedItem != null)
            {
                ShowSelectionIndicatorForRow(dataGrid.SelectedItem);
            }
        }

        /// <summary>
        /// Nasconde tutti gli indicatori di selezione (frecce) nel DataGrid
        /// </summary>
        private void HideAllSelectionIndicators()
        {
            foreach (var item in PatientsDataGrid.Items)
            {
                var row = PatientsDataGrid.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
                if (row != null)
                {
                    HideSelectionIndicatorForRow(row);
                }
            }
        }

        /// <summary>
        /// Nascondi l'indicatore di selezione (freccia) per una specifica riga
        /// </summary>
        private void HideSelectionIndicatorForRow(DataGridRow row)
        {
            if (row != null)
            {
                // Ricerca il TextBlock nel visual tree della riga
                var textBlock = FindTextBlockInVisualTree(row);
                if (textBlock != null)
                {
                    textBlock.Visibility = Visibility.Collapsed;
                }
            }
        }

        /// <summary>
        /// Mostra l'indicatore di selezione (freccia) per la riga selezionata
        /// </summary>
        private void ShowSelectionIndicatorForRow(object item)
        {
            var row = PatientsDataGrid.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
            if (row != null)
            {
                // Ricerca il TextBlock nel visual tree della riga
                var textBlock = FindTextBlockInVisualTree(row);
                if (textBlock != null)
                {
                    textBlock.Visibility = Visibility.Visible;
                }
            }
        }

        /// <summary>
        /// Ricerca il primo TextBlock nel visual tree a partire da un elemento
        /// </summary>
        private TextBlock? FindTextBlockInVisualTree(DependencyObject parent)
        {
            if (parent == null)
                return null;

            int childrenCount = System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);

                // Se e un TextBlock, lo ritorna
                if (child is TextBlock textBlock)
                {
                    return textBlock;
                }

                // Ricerca ricorsiva
                var result = FindTextBlockInVisualTree(child);
                if (result != null)
                    return result;
            }

            return null;
        }
    }
}
