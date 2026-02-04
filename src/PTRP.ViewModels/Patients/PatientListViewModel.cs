using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PTRP.Models;
using PTRP.Services.Enums;
using PTRP.Services.Interfaces;
using PTRP.ViewModels.Patients;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PTRP.ViewModels.Patients
{
    /// <summary>
    /// ViewModel for PatientListView with Master-Detail layout.
    /// Supports search, filtering by project state, and patient selection.
    /// Integrated with IPatientService for real data access.
    /// </summary>
    public partial class PatientListViewModel : ViewModelBase
    {
        private readonly IPatientService _patientService;
        private CancellationTokenSource? _searchCts;

        #region Properties

        /// <summary>
        /// Search term for real-time filtering on patient names.
        /// </summary>
        [ObservableProperty]
        private string _searchTerm = string.Empty;

        /// <summary>
        /// Selected project state filter (enum-based).
        /// </summary>
        [ObservableProperty]
        private ProjectStateFilter _selectedStateFilter = ProjectStateFilter.All;

        /// <summary>
        /// Available project state filter options.
        /// </summary>
        public ObservableCollection<ProjectStateFilterItem> ProjectStateFilters { get; } = new()
        {
            new ProjectStateFilterItem { Value = ProjectStateFilter.All, Display = "Tutti" },
            new ProjectStateFilterItem { Value = ProjectStateFilter.Active, Display = "Attivi" },
            new ProjectStateFilterItem { Value = ProjectStateFilter.Suspended, Display = "Sospesi" },
            new ProjectStateFilterItem { Value = ProjectStateFilter.Completed, Display = "Completati" },
            new ProjectStateFilterItem { Value = ProjectStateFilter.Deceased, Display = "Deceduti" }
        };

        /// <summary>
        /// Filtered and displayed patients list.
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<PatientViewModel> _patients = new();

        /// <summary>
        /// Currently selected patient for detail panel display.
        /// </summary>
        [ObservableProperty]
        private PatientViewModel? _selectedPatient;

        /// <summary>
        /// Indicates if data is currently loading.
        /// </summary>
        [ObservableProperty]
        private bool _isLoading;

        /// <summary>
        /// Error message to display if search fails.
        /// </summary>
        [ObservableProperty]
        private string? _errorMessage;

        /// <summary>
        /// Display name override for navigation title.
        /// </summary>
        public override string DisplayName => "Pazienti";

        /// <summary>
        /// Determines if a new project can be created for the selected patient.
        /// True only if patient has NO active project.
        /// </summary>
        public bool CanCreateNewProject => SelectedPatient != null 
                                          && SelectedPatient.ProjectState != "Active";

        #endregion

        #region Events

        /// <summary>
        /// Event raised when user requests to create a new project.
        /// Carries patient ID and full name for ProjectFormViewModel initialization.
        /// </summary>
        public event EventHandler<(Guid PatientId, string PatientFullName)>? NewProjectRequested;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor with dependency injection.
        /// </summary>
        /// <param name="patientService">Service for patient data access</param>
        public PatientListViewModel(IPatientService patientService)
        {
            _patientService = patientService ?? throw new ArgumentNullException(nameof(patientService));
        }

        #endregion

        #region Commands

        /// <summary>
        /// Command to load patients with current filters.
        /// </summary>
        [RelayCommand]
        public async Task LoadPatientsAsync()
        {
            await SearchPatientsAsync();
        }

        /// <summary>
        /// Command to clear search and filters.
        /// </summary>
        [RelayCommand]
        private async Task ClearSearchAsync()
        {
            SearchTerm = string.Empty;
            SelectedStateFilter = ProjectStateFilter.All;
            await SearchPatientsAsync();
        }

        /// <summary>
        /// Command to open the project creation form for selected patient.
        /// Only enabled if patient has no active project.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanCreateNewProject))]
        private void NewProject()
        {
            if (SelectedPatient == null)
                return;

            // Raise event to notify view to open ProjectFormView
            NewProjectRequested?.Invoke(this, (SelectedPatient.Id, SelectedPatient.FullName));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Searches patients using IPatientService with current filters.
        /// Supports cancellation for responsive UI during typing.
        /// </summary>
        private async Task SearchPatientsAsync()
        {
            // Cancel previous search if still running
            _searchCts?.Cancel();
            _searchCts = new CancellationTokenSource();
            var ct = _searchCts.Token;

            IsLoading = true;
            ErrorMessage = null;

            try
            {
                // Debounce: wait 300ms before executing search
                await Task.Delay(300, ct);

                // Call IPatientService.SearchAsync with filters
                var searchTerm = string.IsNullOrWhiteSpace(SearchTerm) ? null : SearchTerm;
                var stateFilter = SelectedStateFilter == ProjectStateFilter.All ? null : (ProjectStateFilter?)SelectedStateFilter;

                var patientModels = await _patientService.SearchAsync(searchTerm, stateFilter, ct);

                // Convert to ViewModels
                var patientVMs = patientModels.Select(MapToViewModel).ToList();

                Patients = new ObservableCollection<PatientViewModel>(patientVMs);
            }
            catch (OperationCanceledException)
            {
                // Search was cancelled, ignore
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Errore durante il caricamento dei pazienti: {ex.Message}";
                Patients.Clear();
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Maps PatientModel to PatientViewModel for UI display.
        /// </summary>
        private PatientViewModel MapToViewModel(PatientModel model)
        {
            // Determine project state from model's therapy projects
            var activeProject = model.TherapyProjects?.FirstOrDefault(p => p.Status == "Active");
            var anyProject = model.TherapyProjects?.FirstOrDefault();

            string projectState = "None";
            string projectStateDisplay = "Nessun Progetto";

            if (activeProject != null)
            {
                projectState = activeProject.Status;
                projectStateDisplay = MapStateToDisplay(activeProject.Status);
            }
            else if (anyProject != null)
            {
                projectState = anyProject.Status;
                projectStateDisplay = MapStateToDisplay(anyProject.Status);
            }

            return new PatientViewModel
            {
                Id = model.Id,
                FirstName = model.FirstName,
                LastName = model.LastName,
                CreatedAt = model.CreatedAt,
                ProjectState = projectState,
                ProjectStateDisplay = projectStateDisplay,
                AssignedEducatorsDisplay = GetEducatorsDisplay(activeProject),
                ActiveProject = activeProject != null ? MapActiveProject(activeProject) : null,
                NextAppointment = null // TODO: Implement when VisitService is available
            };
        }

        /// <summary>
        /// Maps project state string to display text.
        /// </summary>
        private string MapStateToDisplay(string state)
        {
            return state switch
            {
                "Active" => "Attivo",
                "Suspended" => "Sospeso",
                "Completed" => "Completato",
                "Deceased" => "Deceduto",
                _ => "Sconosciuto"
            };
        }

        /// <summary>
        /// Gets comma-separated educators display string.
        /// </summary>
        private string GetEducatorsDisplay(TherapyProjectModel? project)
        {
            if (project?.ProfessionalEducators == null || !project.ProfessionalEducators.Any())
                return "-";

            return string.Join(", ", project.ProfessionalEducators.Select(e => e.LastName));
        }

        /// <summary>
        /// Maps TherapyProjectModel to ActiveProjectViewModel.
        /// </summary>
        private ActiveProjectViewModel MapActiveProject(TherapyProjectModel model)
        {
            var startDate = model.StartDate;
            var endDate = model.EndDate;

            // Format period string with nullable endDate handling
            var periodString = endDate.HasValue 
                ? $"{startDate:MMMM yyyy} - {endDate.Value:MMMM yyyy}" 
                : $"{startDate:MMMM yyyy} - In corso";

            return new ActiveProjectViewModel
            {
                Id = model.Id,
                Title = model.Title ?? string.Empty,
                Period = periodString,
                StartDate = startDate,
                EndDate = endDate,
                Educators = model.ProfessionalEducators?.Select(e => new EducatorViewModel
                {
                    Id = e.Id,
                    Name = $"{e.FirstName} {e.LastName}",
                    Initials = $"{e.FirstName[0]}{e.LastName[0]}",
                    Role = "Educatore" // TODO: Get from ProjectOperatorModel.RoleInProject when available
                }).ToList() ?? new List<EducatorViewModel>()
            };
        }

        #endregion

        #region Partial Methods

        /// <summary>
        /// Reapply search when search term changes (debounced).
        /// </summary>
        partial void OnSearchTermChanged(string value)
        {
            _ = SearchPatientsAsync();
        }

        /// <summary>
        /// Reapply search when state filter changes.
        /// </summary>
        partial void OnSelectedStateFilterChanged(ProjectStateFilter value)
        {
            _ = SearchPatientsAsync();
        }

        /// <summary>
        /// Update CanCreateNewProject when selected patient changes.
        /// </summary>
        partial void OnSelectedPatientChanged(PatientViewModel? value)
        {
            NewProjectCommand.NotifyCanExecuteChanged();
        }

        #endregion
    }

    #region Helper Classes

    /// <summary>
    /// Helper class for ComboBox binding with enum values.
    /// </summary>
    public class ProjectStateFilterItem
    {
        public ProjectStateFilter Value { get; set; }
        public string Display { get; set; } = string.Empty;
    }

    #endregion
}
