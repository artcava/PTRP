using System.Windows;
using System.Windows.Controls;
using PTRP.ViewModels;

namespace PTRP.App.Views.Patients
{
    /// <summary>
    /// Interaction logic for PatientListView.xaml
    /// Master-Detail view for patient management with search and state filtering.
    /// </summary>
    public partial class PatientListView : UserControl
    {
        public PatientListView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Loads patients when the view is loaded.
        /// </summary>
        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is PatientListViewModel viewModel)
            {
                await viewModel.LoadPatientsCommand.ExecuteAsync(null);
            }
        }
    }
}
