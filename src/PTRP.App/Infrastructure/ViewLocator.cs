using System;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using PTRP.App.Views.Educators;
using PTRP.App.Views.Patients;
using PTRP.App.Views.Projects;
using PTRP.App.Views.Setup;
using PTRP.App.Views.Sync;
using PTRP.ViewModels;
using PTRP.ViewModels.Educators;
using PTRP.ViewModels.Patients;
using PTRP.ViewModels.Projects;

namespace PTRP.App.Infrastructure;

/// <summary>
/// Locates and instantiates Views for ViewModels using Dependency Injection.
/// Replaces DataTemplate approach when Views require constructor parameters.
/// Issue #94: Enables MVVM navigation with DI-based View constructors.
/// </summary>
public class ViewLocator
{
    private readonly IServiceProvider _serviceProvider;

    public ViewLocator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <summary>
    /// Creates a View instance for the given ViewModel, resolving dependencies via DI.
    /// Sets the ViewModel as the View's DataContext.
    /// </summary>
    /// <param name="viewModel">The ViewModel instance</param>
    /// <returns>A UserControl instance with DataContext set to the ViewModel, or null if no matching View found</returns>
    public UserControl? CreateViewForViewModel(object? viewModel)
    {
        if (viewModel == null)
            return null;

        UserControl? view = viewModel switch
        {
            // First Run / Setup
            FirstRunViewModel => new FirstRunView(),

            // Patients Module (requires IServiceProvider in constructor)
            PatientListViewModel => _serviceProvider.GetRequiredService<PatientListView>(),

            // Educators Module (requires IServiceProvider in constructor)
            EducatorListViewModel => _serviceProvider.GetRequiredService<EducatorListView>(),

            // Projects Module (requires IServiceProvider in constructor)
            ProjectListViewModel => _serviceProvider.GetRequiredService<ProjectListView>(),
            
            // ProjectFormView is created manually with specific patient context,
            // so it's not navigated to directly - it's opened in dialogs
            ProjectFormViewModel => new ProjectFormView(),

            // Sync Module (requires IServiceProvider in constructor)
            SyncViewModel => _serviceProvider.GetRequiredService<SyncView>(),

            // Unknown ViewModel - return null
            _ => null
        };

        if (view != null)
        {
            view.DataContext = viewModel;
        }

        return view;
    }
}
