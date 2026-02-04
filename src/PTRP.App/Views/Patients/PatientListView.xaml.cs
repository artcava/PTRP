using System;
using System.Windows;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.DependencyInjection;
using PTRP.App.Views.Projects;
using PTRP.Services.Interfaces;
using PTRP.ViewModels.Patients;
using PTRP.ViewModels.Projects;

namespace PTRP.App.Views.Patients
{
    /// <summary>
    /// Interaction logic for PatientListView.xaml
    /// Master-Detail view for patient management with search and state filtering.
    /// </summary>
    public partial class PatientListView : UserControl
    {
        private readonly IServiceProvider _serviceProvider;

        public PatientListView(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            InitializeComponent();
        }

        /// <summary>
        /// Loads patients when the view is loaded and subscribes to events.
        /// </summary>
        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is PatientListViewModel viewModel)
            {
                // Subscribe to NewProjectRequested event
                viewModel.NewProjectRequested += OnNewProjectRequested;
                
                // Load patients
                await viewModel.LoadPatientsCommand.ExecuteAsync(null);
            }
        }

        /// <summary>
        /// Unsubscribe from events when unloaded.
        /// </summary>
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is PatientListViewModel viewModel)
            {
                viewModel.NewProjectRequested -= OnNewProjectRequested;
            }
        }

        /// <summary>
        /// Handles the NewProjectRequested event by opening ProjectFormView in a dialog.
        /// </summary>
        private async void OnNewProjectRequested(object? sender, (Guid PatientId, string PatientFullName) args)
        {
            try
            {
                // Create ProjectFormViewModel with dependencies
                var projectService = _serviceProvider.GetRequiredService<ITherapyProjectService>();
                var educatorService = _serviceProvider.GetRequiredService<IEducatorService>();
                
                var formViewModel = new ProjectFormViewModel(
                    projectService,
                    educatorService,
                    args.PatientId,
                    args.PatientFullName
                );

                // Create the view and set DataContext
                var formView = new ProjectFormView
                {
                    DataContext = formViewModel
                };

                // Subscribe to completion events
                bool projectCreated = false;
                formViewModel.OnProjectCreated += (projectId) =>
                {
                    projectCreated = true;
                    DialogHost.CloseDialogCommand.Execute(null, null);
                };

                formViewModel.OnCancelled += () =>
                {
                    DialogHost.CloseDialogCommand.Execute(null, null);
                };

                // Show dialog
                await DialogHost.Show(formView, "RootDialog");

                // Refresh patient list if project was created
                if (projectCreated && DataContext is PatientListViewModel viewModel)
                {
                    await viewModel.LoadPatientsCommand.ExecuteAsync(null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Errore nell'apertura del form progetto: {ex.Message}",
                    "Errore",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
    }
}
