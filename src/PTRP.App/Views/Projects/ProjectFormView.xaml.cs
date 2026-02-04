using PTRP.ViewModels.Projects;
using System.Windows.Controls;

namespace PTRP.App.Views.Projects;

/// <summary>
/// Interaction logic for ProjectFormView.xaml
/// </summary>
public partial class ProjectFormView : UserControl
{
    public ProjectFormView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is ProjectFormViewModel viewModel)
        {
            await viewModel.LoadEducatorsAsync();
        }
    }
}
