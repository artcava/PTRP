using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PTRP.ViewModels.Patients;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace PTRP.ViewModels
{
    /// <summary>
    /// ViewModel for PatientListView with Master-Detail layout.
    /// Supports search, filtering, and patient selection.
    /// </summary>
    public partial class PatientListViewModel : ViewModelBase
    {
        #region Properties

        /// <summary>
        /// Search term for real-time filtering on patient names.
        /// </summary>
        [ObservableProperty]
        private string _searchTerm = string.Empty;

        /// <summary>
        /// Selected project state filter.
        /// </summary>
        [ObservableProperty]
        private string _selectedStateFilter = "Tutti";

        /// <summary>
        /// Available project state filter options.
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<string> _projectStateFilters = new()
        {
            "Tutti",
            "Active",
            "Suspended",
            "Completed",
            "Deceased"
        };

        /// <summary>
        /// Filtered and displayed patients list.
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<PatientViewModel> _patients = new();

        /// <summary>
        /// All patients before filtering (source collection).
        /// </summary>
        private List<PatientViewModel> _allPatients = new();

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

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of PatientListViewModel.
        /// </summary>
        public PatientListViewModel()
        {
            DisplayName = "Pazienti";
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads all patients from data source (simulated for now).
        /// In production, this will call IPatientService.
        /// </summary>
        public async Task LoadPatientsAsync()
        {
            IsLoading = true;
            
            try
            {
                // Simulate API call delay
                await Task.Delay(500);

                // TODO: Replace with actual IPatientService.GetAllAsync()
                _allPatients = GenerateSamplePatients();
                
                ApplyFilters();
            }
            catch (Exception ex)
            {
                // TODO: Show error notification
                Console.WriteLine($"Error loading patients: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Applies search and state filters to the patient list.
        /// </summary>
        private void ApplyFilters()
        {
            var query = _allPatients.AsEnumerable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                query = query.Where(p =>
                    p.FirstName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    p.LastName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase));
            }

            // Apply state filter
            if (SelectedStateFilter != "Tutti")
            {
                query = query.Where(p => p.ProjectState == SelectedStateFilter);
            }

            Patients = new ObservableCollection<PatientViewModel>(query);
        }

        /// <summary>
        /// Generates sample patient data for testing.
        /// TODO: Remove when IPatientService is integrated.
        /// </summary>
        private List<PatientViewModel> GenerateSamplePatients()
        {
            return new List<PatientViewModel>
            {
                new PatientViewModel
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Mario",
                    LastName = "Rossi",
                    CreatedAt = DateTime.Now.AddMonths(-6),
                    ProjectState = "Active",
                    ProjectStateDisplay = "Attivo",
                    AssignedEducatorsDisplay = "Bianchi, Verdi",
                    ActiveProject = new ActiveProjectViewModel
                    {
                        Id = Guid.NewGuid(),
                        Title = "PTRP 2025-2027",
                        Period = "Gennaio 2025 - Dicembre 2027",
                        StartDate = new DateTime(2025, 1, 1),
                        EndDate = new DateTime(2027, 12, 31),
                        Educators = new List<EducatorViewModel>
                        {
                            new EducatorViewModel { Id = Guid.NewGuid(), Name = "Mario Bianchi", Initials = "MB", Role = "Coordinatore" },
                            new EducatorViewModel { Id = Guid.NewGuid(), Name = "Laura Verdi", Initials = "LV", Role = "Educatore" }
                        }
                    },
                    NextAppointment = new NextAppointmentViewModel
                    {
                        Id = Guid.NewGuid(),
                        Type = "Prima Apertura",
                        TypeDisplay = "Prima Apertura",
                        ScheduledDate = new DateTime(2025, 4, 2, 14, 30, 0),
                        Location = "Centro Diurno"
                    }
                },
                new PatientViewModel
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Luca",
                    LastName = "Bianchi",
                    CreatedAt = DateTime.Now.AddMonths(-12),
                    ProjectState = "Active",
                    ProjectStateDisplay = "Attivo",
                    AssignedEducatorsDisplay = "Gialli",
                    ActiveProject = new ActiveProjectViewModel
                    {
                        Id = Guid.NewGuid(),
                        Title = "PTRP 2024-2026",
                        Period = "Gennaio 2024 - Dicembre 2026",
                        StartDate = new DateTime(2024, 1, 1),
                        EndDate = new DateTime(2026, 12, 31),
                        Educators = new List<EducatorViewModel>
                        {
                            new EducatorViewModel { Id = Guid.NewGuid(), Name = "Giovanni Gialli", Initials = "GG", Role = "Educatore" }
                        }
                    },
                    NextAppointment = new NextAppointmentViewModel
                    {
                        Id = Guid.NewGuid(),
                        Type = "Visita Intermedia",
                        TypeDisplay = "Visita Intermedia",
                        ScheduledDate = new DateTime(2025, 5, 15, 10, 0, 0),
                        Location = "Ambulatorio"
                    }
                },
                new PatientViewModel
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Anna",
                    LastName = "Verdi",
                    CreatedAt = DateTime.Now.AddMonths(-18),
                    ProjectState = "Suspended",
                    ProjectStateDisplay = "Sospeso",
                    AssignedEducatorsDisplay = "Rossi",
                    ActiveProject = new ActiveProjectViewModel
                    {
                        Id = Guid.NewGuid(),
                        Title = "PTRP 2023-2025",
                        Period = "Gennaio 2023 - Dicembre 2025",
                        StartDate = new DateTime(2023, 1, 1),
                        EndDate = new DateTime(2025, 12, 31),
                        Educators = new List<EducatorViewModel>
                        {
                            new EducatorViewModel { Id = Guid.NewGuid(), Name = "Paolo Rossi", Initials = "PR", Role = "Educatore" }
                        }
                    },
                    NextAppointment = null // No upcoming appointment
                },
                new PatientViewModel
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Giuseppe",
                    LastName = "Neri",
                    CreatedAt = DateTime.Now.AddMonths(-24),
                    ProjectState = "Completed",
                    ProjectStateDisplay = "Completato",
                    AssignedEducatorsDisplay = "-",
                    ActiveProject = null, // No active project
                    NextAppointment = null
                }
            };
        }

        #endregion

        #region Partial Methods

        /// <summary>
        /// Reapply filters when search term changes.
        /// </summary>
        partial void OnSearchTermChanged(string value)
        {
            ApplyFilters();
        }

        /// <summary>
        /// Reapply filters when state filter changes.
        /// </summary>
        partial void OnSelectedStateFilterChanged(string value)
        {
            ApplyFilters();
        }

        #endregion
    }
}
