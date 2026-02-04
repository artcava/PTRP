using System;
using System.Windows;
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
/// Used by ViewLocatorDataTemplateSelector to resolve Views from DI container.
/// Issue #94: Enables DataTemplate pattern with DI-based View constructors.
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
    /// </summary>
    /// <param name="viewModel">The ViewModel instance</param>
    /// <returns>A UserControl instance with DataContext set to the ViewModel</returns>
    public UserControl? CreateViewForViewModel(object? viewModel)
    {
        if (viewModel == null)
            return null;

        UserControl? view = viewModel switch
        {
            // First Run / Setup
            FirstRunViewModel => new FirstRunView(),

            // Patients Module
            PatientListViewModel => _serviceProvider.GetRequiredService<PatientListView>(),

            // Educators Module
            EducatorListViewModel => _serviceProvider.GetRequiredService<EducatorListView>(),

            // Projects Module
            ProjectListViewModel => _serviceProvider.GetRequiredService<ProjectListView>(),
            ProjectFormViewModel => new ProjectFormView(), // ProjectFormView is created manually with specific patient context

            // Sync Module
            SyncViewModel => _serviceProvider.GetRequiredService<SyncView>(),

            // Unknown ViewModel
            _ => null
        };

        if (view != null)
        {
            view.DataContext = viewModel;
        }

        return view;
    }
}

/// <summary>
/// DataTemplateSelector that uses ViewLocator to resolve Views from DI container.
/// Replaces static DataTemplates in App.xaml for DI-based View instantiation.
/// </summary>
public class ViewLocatorDataTemplateSelector : DataTemplateSelector
{
    private static ViewLocator? _viewLocator;

    /// <summary>
    /// Sets the ViewLocator instance (called from App.xaml.cs on startup)
    /// </summary>
    public static void Initialize(IServiceProvider serviceProvider)
    {
        _viewLocator = new ViewLocator(serviceProvider);
    }

    /// <summary>
    /// Selects a DataTemplate by creating a View for the ViewModel via DI.
    /// </summary>
    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        if (_viewLocator == null)
        {
            throw new InvalidOperationException(
                "ViewLocatorDataTemplateSelector not initialized. Call Initialize() in App.xaml.cs.");
        }

        // Create a DataTemplate that instantiates the View via ViewLocator
        var dataTemplate = new DataTemplate
        {
            VisualTree = new FrameworkElementFactory(typeof(ContentPresenter))
        };

        // We can't use FrameworkElementFactory with DI, so we use a different approach:
        // Return a template that creates a ContentControl and set its Content in code-behind
        // Actually, we need to create the view directly here
        
        var view = _viewLocator.CreateViewForViewModel(item);
        
        if (view != null)
        {
            // Create a DataTemplate with the view as content
            var factory = new FrameworkElementFactory(view.GetType());
            dataTemplate.VisualTree = factory;
        }

        return dataTemplate;
    }
}
