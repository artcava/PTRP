using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PTRP.ViewModels.Educators;

/// <summary>
/// ViewModel principale per la vista lista educatori.
/// Gestisce il caricamento, la ricerca e la selezione degli educatori.
/// </summary>
public partial class EducatorListViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _searchTerm = string.Empty;

    [ObservableProperty]
    private ObservableCollection<EducatorViewModel> _educators = new();

    private List<EducatorViewModel> _allEducators = new();

    [ObservableProperty]
    private EducatorViewModel? _selectedEducator;

    [ObservableProperty]
    private bool _isLoading;

    public override string DisplayName => "Educatori";

    /// <summary>
    /// Carica la lista di educatori.
    /// TODO: Sostituire con IEducatorService.GetAllAsync() quando disponibile.
    /// </summary>
    public async Task LoadEducatorsAsync()
    {
        IsLoading = true;
        try
        {
            await Task.Delay(500); // Simula chiamata API
            _allEducators = GenerateSampleEducators();
            ApplyFilters();
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Applica il filtro di ricerca alla lista educatori.
    /// </summary>
    private void ApplyFilters()
    {
        var query = _allEducators.AsEnumerable();

        // Filtro ricerca (FirstName, LastName)
        if (!string.IsNullOrWhiteSpace(SearchTerm))
        {
            query = query.Where(e =>
                e.FirstName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                e.LastName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase));
        }

        Educators = new ObservableCollection<EducatorViewModel>(query);
    }

    /// <summary>
    /// Genera educatori di test con dati realistici.
    /// </summary>
    private List<EducatorViewModel> GenerateSampleEducators()
    {
        return new List<EducatorViewModel>
        {
            new EducatorViewModel
            {
                Id = Guid.NewGuid(),
                FirstName = "Mario",
                LastName = "Bianchi",
                Initials = "MB",
                Email = "mario.bianchi@ptrp.it",
                Phone = "+39 011 123 4567",
                HireDate = new DateTime(2018, 3, 15),
                IsActive = true,
                AssignedPatientsCount = 8,
                TotalVisitsThisMonth = 15,
                ScheduledAppointments = 6,
                AssignedPatients = new List<AssignedPatientViewModel>
                {
                    new AssignedPatientViewModel
                    {
                        PatientId = Guid.NewGuid(),
                        PatientFullName = "Mario Rossi",
                        ProjectTitle = "PTRP 2025-2027",
                        ProjectState = "Active",
                        NextAppointmentDate = new DateTime(2025, 4, 2, 14, 30, 0)
                    },
                    new AssignedPatientViewModel
                    {
                        PatientId = Guid.NewGuid(),
                        PatientFullName = "Luca Bianchi",
                        ProjectTitle = "PTRP 2024-2026",
                        ProjectState = "Active",
                        NextAppointmentDate = new DateTime(2025, 5, 15, 10, 0, 0)
                    },
                    new AssignedPatientViewModel
                    {
                        PatientId = Guid.NewGuid(),
                        PatientFullName = "Anna Verdi",
                        ProjectTitle = "PTRP 2023-2025",
                        ProjectState = "Suspended",
                        NextAppointmentDate = null
                    }
                }
            },
            new EducatorViewModel
            {
                Id = Guid.NewGuid(),
                FirstName = "Laura",
                LastName = "Verdi",
                Initials = "LV",
                Email = "laura.verdi@ptrp.it",
                Phone = "+39 011 234 5678",
                HireDate = new DateTime(2019, 9, 1),
                IsActive = true,
                AssignedPatientsCount = 12,
                TotalVisitsThisMonth = 24,
                ScheduledAppointments = 8,
                AssignedPatients = new List<AssignedPatientViewModel>
                {
                    new AssignedPatientViewModel
                    {
                        PatientId = Guid.NewGuid(),
                        PatientFullName = "Giuseppe Neri",
                        ProjectTitle = "PTRP 2024-2026",
                        ProjectState = "Active",
                        NextAppointmentDate = new DateTime(2025, 3, 20, 9, 0, 0)
                    },
                    new AssignedPatientViewModel
                    {
                        PatientId = Guid.NewGuid(),
                        PatientFullName = "Francesca Blu",
                        ProjectTitle = "PTRP 2025-2028",
                        ProjectState = "Active",
                        NextAppointmentDate = new DateTime(2025, 3, 10, 15, 0, 0)
                    }
                }
            },
            new EducatorViewModel
            {
                Id = Guid.NewGuid(),
                FirstName = "Giovanni",
                LastName = "Rossi",
                Initials = "GR",
                Email = "giovanni.rossi@ptrp.it",
                Phone = "+39 011 345 6789",
                HireDate = new DateTime(2020, 1, 10),
                IsActive = true,
                AssignedPatientsCount = 10,
                TotalVisitsThisMonth = 18,
                ScheduledAppointments = 5,
                AssignedPatients = new List<AssignedPatientViewModel>
                {
                    new AssignedPatientViewModel
                    {
                        PatientId = Guid.NewGuid(),
                        PatientFullName = "Paolo Grigi",
                        ProjectTitle = "PTRP 2023-2025",
                        ProjectState = "Completed",
                        NextAppointmentDate = null
                    }
                }
            },
            new EducatorViewModel
            {
                Id = Guid.NewGuid(),
                FirstName = "Sara",
                LastName = "Neri",
                Initials = "SN",
                Email = "sara.neri@ptrp.it",
                Phone = "+39 011 456 7890",
                HireDate = new DateTime(2024, 10, 1),
                IsActive = true,
                AssignedPatientsCount = 3,
                TotalVisitsThisMonth = 6,
                ScheduledAppointments = 2,
                AssignedPatients = new List<AssignedPatientViewModel>
                {
                    new AssignedPatientViewModel
                    {
                        PatientId = Guid.NewGuid(),
                        PatientFullName = "Elena Bianchi",
                        ProjectTitle = "PTRP 2024-2026",
                        ProjectState = "Active",
                        NextAppointmentDate = new DateTime(2025, 2, 28, 11, 30, 0)
                    }
                }
            },
            new EducatorViewModel
            {
                Id = Guid.NewGuid(),
                FirstName = "Paolo",
                LastName = "Gialli",
                Initials = "PG",
                Email = "paolo.gialli@ptrp.it",
                Phone = "+39 011 567 8901",
                HireDate = new DateTime(2017, 5, 20),
                IsActive = false,
                AssignedPatientsCount = 0,
                TotalVisitsThisMonth = 0,
                ScheduledAppointments = 0,
                AssignedPatients = new List<AssignedPatientViewModel>()
            }
        };
    }

    // Property changed handler per aggiornamento automatico filtri
    partial void OnSearchTermChanged(string value)
    {
        ApplyFilters();
    }
}
