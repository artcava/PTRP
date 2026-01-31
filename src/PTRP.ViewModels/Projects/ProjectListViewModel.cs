using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PTRP.ViewModels.Projects;

/// <summary>
/// ViewModel principale per la vista lista progetti.
/// Gestisce il caricamento, la ricerca e la selezione dei progetti.
/// </summary>
public partial class ProjectListViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _searchTerm = string.Empty;

    [ObservableProperty]
    private string _selectedStateFilter = "Tutti";

    [ObservableProperty]
    private ObservableCollection<string> _stateFilters = new()
    {
        "Tutti",
        "Active",
        "Suspended",
        "Completed",
        "Deceased"
    };

    [ObservableProperty]
    private ObservableCollection<ProjectViewModel> _projects = new();

    private List<ProjectViewModel> _allProjects = new();

    [ObservableProperty]
    private ProjectViewModel? _selectedProject;

    [ObservableProperty]
    private bool _isLoading;

    public override string DisplayName => "Progetti";

    /// <summary>
    /// Carica la lista di progetti.
    /// TODO: Sostituire con IProjectService.GetAllAsync() quando disponibile.
    /// </summary>
    public async Task LoadProjectsAsync()
    {
        IsLoading = true;
        try
        {
            await Task.Delay(500); // Simula chiamata API
            _allProjects = GenerateSampleProjects();
            ApplyFilters();
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Applica i filtri di ricerca e stato alla lista progetti.
    /// </summary>
    private void ApplyFilters()
    {
        var query = _allProjects.AsEnumerable();

        // Filtro ricerca (Title, Patient name)
        if (!string.IsNullOrWhiteSpace(SearchTerm))
        {
            query = query.Where(p =>
                p.Title.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                p.Patient.FullName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase));
        }

        // Filtro stato
        if (SelectedStateFilter != "Tutti")
        {
            query = query.Where(p => p.State == SelectedStateFilter);
        }

        Projects = new ObservableCollection<ProjectViewModel>(query);
    }

    /// <summary>
    /// Genera progetti di test con dati realistici.
    /// </summary>
    private List<ProjectViewModel> GenerateSampleProjects()
    {
        return new List<ProjectViewModel>
        {
            new ProjectViewModel
            {
                Id = Guid.NewGuid(),
                Title = "PTRP 2025-2027",
                Description = "Progetto triennale per supporto autonomia e integrazione sociale",
                StartDate = new DateTime(2025, 1, 15),
                EndDate = new DateTime(2027, 12, 31),
                State = "Active",
                CreatedAt = new DateTime(2025, 1, 10),
                Patient = new ProjectPatientViewModel
                {
                    PatientId = Guid.NewGuid(),
                    FirstName = "Mario",
                    LastName = "Rossi",
                    BirthDate = new DateTime(1990, 5, 20)
                },
                Educators = new List<ProjectEducatorViewModel>
                {
                    new ProjectEducatorViewModel
                    {
                        EducatorId = Guid.NewGuid(),
                        FirstName = "Mario",
                        LastName = "Bianchi",
                        Initials = "MB"
                    },
                    new ProjectEducatorViewModel
                    {
                        EducatorId = Guid.NewGuid(),
                        FirstName = "Laura",
                        LastName = "Verdi",
                        Initials = "LV"
                    }
                },
                Appointments = new List<ProjectAppointmentViewModel>
                {
                    new ProjectAppointmentViewModel
                    {
                        AppointmentId = Guid.NewGuid(),
                        Type = "PrimaApertura",
                        TypeDisplay = "Prima Apertura",
                        ScheduledDate = new DateTime(2025, 2, 5, 14, 30, 0),
                        IsCompleted = true,
                        CompletedDate = new DateTime(2025, 2, 5, 15, 45, 0),
                        Location = "Centro Diurno"
                    },
                    new ProjectAppointmentViewModel
                    {
                        AppointmentId = Guid.NewGuid(),
                        Type = "Intermedia",
                        TypeDisplay = "Visita Intermedia",
                        ScheduledDate = new DateTime(2025, 3, 15, 10, 0, 0),
                        IsCompleted = true,
                        CompletedDate = new DateTime(2025, 3, 15, 11, 30, 0),
                        Location = "Ambulatorio"
                    },
                    new ProjectAppointmentViewModel
                    {
                        AppointmentId = Guid.NewGuid(),
                        Type = "Intermedia",
                        TypeDisplay = "Visita Intermedia",
                        ScheduledDate = new DateTime(2025, 4, 2, 14, 30, 0),
                        IsCompleted = false,
                        Location = "Centro Diurno"
                    },
                    new ProjectAppointmentViewModel
                    {
                        AppointmentId = Guid.NewGuid(),
                        Type = "Intermedia",
                        TypeDisplay = "Visita Intermedia",
                        ScheduledDate = new DateTime(2025, 6, 10, 9, 0, 0),
                        IsCompleted = false,
                        Location = "Centro Diurno"
                    },
                    new ProjectAppointmentViewModel
                    {
                        AppointmentId = Guid.NewGuid(),
                        Type = "Finale",
                        TypeDisplay = "Visita Finale",
                        ScheduledDate = new DateTime(2027, 12, 20, 15, 0, 0),
                        IsCompleted = false,
                        Location = "Centro Diurno"
                    }
                }
            },
            new ProjectViewModel
            {
                Id = Guid.NewGuid(),
                Title = "PTRP 2024-2026",
                Description = "Progetto biennale per sviluppo abilità lavorative",
                StartDate = new DateTime(2024, 9, 1),
                EndDate = new DateTime(2026, 8, 31),
                State = "Active",
                CreatedAt = new DateTime(2024, 8, 20),
                Patient = new ProjectPatientViewModel
                {
                    PatientId = Guid.NewGuid(),
                    FirstName = "Luca",
                    LastName = "Bianchi",
                    BirthDate = new DateTime(1985, 11, 10)
                },
                Educators = new List<ProjectEducatorViewModel>
                {
                    new ProjectEducatorViewModel
                    {
                        EducatorId = Guid.NewGuid(),
                        FirstName = "Giovanni",
                        LastName = "Rossi",
                        Initials = "GR"
                    }
                },
                Appointments = new List<ProjectAppointmentViewModel>
                {
                    new ProjectAppointmentViewModel
                    {
                        AppointmentId = Guid.NewGuid(),
                        Type = "PrimaApertura",
                        TypeDisplay = "Prima Apertura",
                        ScheduledDate = new DateTime(2024, 9, 10, 14, 0, 0),
                        IsCompleted = true,
                        CompletedDate = new DateTime(2024, 9, 10, 15, 30, 0),
                        Location = "Centro Diurno"
                    },
                    new ProjectAppointmentViewModel
                    {
                        AppointmentId = Guid.NewGuid(),
                        Type = "Intermedia",
                        TypeDisplay = "Visita Intermedia",
                        ScheduledDate = new DateTime(2024, 12, 5, 10, 30, 0),
                        IsCompleted = true,
                        CompletedDate = new DateTime(2024, 12, 5, 11, 45, 0),
                        Location = "Ambulatorio"
                    },
                    new ProjectAppointmentViewModel
                    {
                        AppointmentId = Guid.NewGuid(),
                        Type = "Intermedia",
                        TypeDisplay = "Visita Intermedia",
                        ScheduledDate = new DateTime(2025, 3, 20, 9, 0, 0),
                        IsCompleted = true,
                        CompletedDate = new DateTime(2025, 3, 20, 10, 15, 0),
                        Location = "Centro Diurno"
                    },
                    new ProjectAppointmentViewModel
                    {
                        AppointmentId = Guid.NewGuid(),
                        Type = "Finale",
                        TypeDisplay = "Visita Finale",
                        ScheduledDate = new DateTime(2025, 5, 15, 10, 0, 0),
                        IsCompleted = false,
                        Location = "Ambulatorio"
                    }
                }
            },
            new ProjectViewModel
            {
                Id = Guid.NewGuid(),
                Title = "PTRP 2023-2025",
                Description = "Progetto per supporto abitativo e autonomia personale",
                StartDate = new DateTime(2023, 3, 1),
                EndDate = new DateTime(2025, 2, 28),
                State = "Suspended",
                CreatedAt = new DateTime(2023, 2, 15),
                SuspendedAt = new DateTime(2025, 1, 15),
                SuspensionReason = "Ricovero ospedaliero del paziente",
                Patient = new ProjectPatientViewModel
                {
                    PatientId = Guid.NewGuid(),
                    FirstName = "Anna",
                    LastName = "Verdi",
                    BirthDate = new DateTime(1992, 7, 3)
                },
                Educators = new List<ProjectEducatorViewModel>
                {
                    new ProjectEducatorViewModel
                    {
                        EducatorId = Guid.NewGuid(),
                        FirstName = "Laura",
                        LastName = "Verdi",
                        Initials = "LV"
                    }
                },
                Appointments = new List<ProjectAppointmentViewModel>
                {
                    new ProjectAppointmentViewModel
                    {
                        AppointmentId = Guid.NewGuid(),
                        Type = "PrimaApertura",
                        TypeDisplay = "Prima Apertura",
                        ScheduledDate = new DateTime(2023, 3, 10, 14, 30, 0),
                        IsCompleted = true,
                        CompletedDate = new DateTime(2023, 3, 10, 16, 0, 0),
                        Location = "Centro Diurno"
                    },
                    new ProjectAppointmentViewModel
                    {
                        AppointmentId = Guid.NewGuid(),
                        Type = "Intermedia",
                        TypeDisplay = "Visita Intermedia",
                        ScheduledDate = new DateTime(2023, 9, 20, 11, 0, 0),
                        IsCompleted = true,
                        CompletedDate = new DateTime(2023, 9, 20, 12, 15, 0),
                        Location = "Ambulatorio"
                    },
                    new ProjectAppointmentViewModel
                    {
                        AppointmentId = Guid.NewGuid(),
                        Type = "Intermedia",
                        TypeDisplay = "Visita Intermedia",
                        ScheduledDate = new DateTime(2024, 6, 5, 15, 30, 0),
                        IsCompleted = false,
                        Location = "Centro Diurno"
                    }
                }
            },
            new ProjectViewModel
            {
                Id = Guid.NewGuid(),
                Title = "PTRP 2022-2024",
                Description = "Progetto completato con successo - integrazione lavorativa",
                StartDate = new DateTime(2022, 1, 10),
                EndDate = new DateTime(2024, 12, 31),
                State = "Completed",
                CreatedAt = new DateTime(2022, 1, 5),
                CompletedAt = new DateTime(2024, 12, 20),
                Patient = new ProjectPatientViewModel
                {
                    PatientId = Guid.NewGuid(),
                    FirstName = "Giuseppe",
                    LastName = "Neri",
                    BirthDate = new DateTime(1988, 4, 15)
                },
                Educators = new List<ProjectEducatorViewModel>
                {
                    new ProjectEducatorViewModel
                    {
                        EducatorId = Guid.NewGuid(),
                        FirstName = "Giovanni",
                        LastName = "Rossi",
                        Initials = "GR"
                    }
                },
                Appointments = new List<ProjectAppointmentViewModel>
                {
                    new ProjectAppointmentViewModel
                    {
                        AppointmentId = Guid.NewGuid(),
                        Type = "PrimaApertura",
                        TypeDisplay = "Prima Apertura",
                        ScheduledDate = new DateTime(2022, 1, 20, 14, 0, 0),
                        IsCompleted = true,
                        CompletedDate = new DateTime(2022, 1, 20, 15, 30, 0),
                        Location = "Centro Diurno"
                    },
                    new ProjectAppointmentViewModel
                    {
                        AppointmentId = Guid.NewGuid(),
                        Type = "Intermedia",
                        TypeDisplay = "Visita Intermedia",
                        ScheduledDate = new DateTime(2022, 6, 10, 10, 0, 0),
                        IsCompleted = true,
                        CompletedDate = new DateTime(2022, 6, 10, 11, 15, 0),
                        Location = "Ambulatorio"
                    },
                    new ProjectAppointmentViewModel
                    {
                        AppointmentId = Guid.NewGuid(),
                        Type = "Intermedia",
                        TypeDisplay = "Visita Intermedia",
                        ScheduledDate = new DateTime(2023, 1, 15, 15, 0, 0),
                        IsCompleted = true,
                        CompletedDate = new DateTime(2023, 1, 15, 16, 30, 0),
                        Location = "Centro Diurno"
                    },
                    new ProjectAppointmentViewModel
                    {
                        AppointmentId = Guid.NewGuid(),
                        Type = "Intermedia",
                        TypeDisplay = "Visita Intermedia",
                        ScheduledDate = new DateTime(2023, 9, 5, 11, 30, 0),
                        IsCompleted = true,
                        CompletedDate = new DateTime(2023, 9, 5, 12, 45, 0),
                        Location = "Ambulatorio"
                    },
                    new ProjectAppointmentViewModel
                    {
                        AppointmentId = Guid.NewGuid(),
                        Type = "Intermedia",
                        TypeDisplay = "Visita Intermedia",
                        ScheduledDate = new DateTime(2024, 6, 20, 14, 0, 0),
                        IsCompleted = true,
                        CompletedDate = new DateTime(2024, 6, 20, 15, 30, 0),
                        Location = "Centro Diurno"
                    },
                    new ProjectAppointmentViewModel
                    {
                        AppointmentId = Guid.NewGuid(),
                        Type = "Finale",
                        TypeDisplay = "Visita Finale",
                        ScheduledDate = new DateTime(2024, 12, 15, 10, 0, 0),
                        IsCompleted = true,
                        CompletedDate = new DateTime(2024, 12, 15, 11, 45, 0),
                        Location = "Ambulatorio"
                    }
                }
            },
            new ProjectViewModel
            {
                Id = Guid.NewGuid(),
                Title = "PTRP 2025-2028",
                Description = "Progetto triennale per sviluppo relazioni interpersonali",
                StartDate = new DateTime(2025, 1, 5),
                EndDate = new DateTime(2028, 1, 5),
                State = "Active",
                CreatedAt = new DateTime(2024, 12, 20),
                Patient = new ProjectPatientViewModel
                {
                    PatientId = Guid.NewGuid(),
                    FirstName = "Francesca",
                    LastName = "Blu",
                    BirthDate = new DateTime(1995, 9, 12)
                },
                Educators = new List<ProjectEducatorViewModel>
                {
                    new ProjectEducatorViewModel
                    {
                        EducatorId = Guid.NewGuid(),
                        FirstName = "Mario",
                        LastName = "Bianchi",
                        Initials = "MB"
                    },
                    new ProjectEducatorViewModel
                    {
                        EducatorId = Guid.NewGuid(),
                        FirstName = "Laura",
                        LastName = "Verdi",
                        Initials = "LV"
                    },
                    new ProjectEducatorViewModel
                    {
                        EducatorId = Guid.NewGuid(),
                        FirstName = "Giovanni",
                        LastName = "Rossi",
                        Initials = "GR"
                    }
                },
                Appointments = new List<ProjectAppointmentViewModel>
                {
                    new ProjectAppointmentViewModel
                    {
                        AppointmentId = Guid.NewGuid(),
                        Type = "PrimaApertura",
                        TypeDisplay = "Prima Apertura",
                        ScheduledDate = new DateTime(2025, 1, 20, 14, 30, 0),
                        IsCompleted = true,
                        CompletedDate = new DateTime(2025, 1, 20, 16, 0, 0),
                        Location = "Centro Diurno"
                    },
                    new ProjectAppointmentViewModel
                    {
                        AppointmentId = Guid.NewGuid(),
                        Type = "Intermedia",
                        TypeDisplay = "Visita Intermedia",
                        ScheduledDate = new DateTime(2025, 3, 10, 15, 0, 0),
                        IsCompleted = false,
                        Location = "Centro Diurno"
                    },
                    new ProjectAppointmentViewModel
                    {
                        AppointmentId = Guid.NewGuid(),
                        Type = "Intermedia",
                        TypeDisplay = "Visita Intermedia",
                        ScheduledDate = new DateTime(2025, 6, 5, 10, 30, 0),
                        IsCompleted = false,
                        Location = "Ambulatorio"
                    },
                    new ProjectAppointmentViewModel
                    {
                        AppointmentId = Guid.NewGuid(),
                        Type = "Intermedia",
                        TypeDisplay = "Visita Intermedia",
                        ScheduledDate = new DateTime(2025, 9, 15, 14, 0, 0),
                        IsCompleted = false,
                        Location = "Centro Diurno"
                    },
                    new ProjectAppointmentViewModel
                    {
                        AppointmentId = Guid.NewGuid(),
                        Type = "Intermedia",
                        TypeDisplay = "Visita Intermedia",
                        ScheduledDate = new DateTime(2026, 1, 10, 11, 0, 0),
                        IsCompleted = false,
                        Location = "Ambulatorio"
                    },
                    new ProjectAppointmentViewModel
                    {
                        AppointmentId = Guid.NewGuid(),
                        Type = "Intermedia",
                        TypeDisplay = "Visita Intermedia",
                        ScheduledDate = new DateTime(2027, 6, 20, 15, 30, 0),
                        IsCompleted = false,
                        Location = "Centro Diurno"
                    },
                    new ProjectAppointmentViewModel
                    {
                        AppointmentId = Guid.NewGuid(),
                        Type = "Finale",
                        TypeDisplay = "Visita Finale",
                        ScheduledDate = new DateTime(2027, 12, 30, 14, 0, 0),
                        IsCompleted = false,
                        Location = "Centro Diurno"
                    }
                }
            },
            new ProjectViewModel
            {
                Id = Guid.NewGuid(),
                Title = "PTRP 2023-2025",
                Description = "Progetto interrotto per decesso del paziente",
                StartDate = new DateTime(2023, 6, 1),
                EndDate = new DateTime(2025, 5, 31),
                State = "Deceased",
                CreatedAt = new DateTime(2023, 5, 20),
                CompletedAt = new DateTime(2025, 2, 5),
                Patient = new ProjectPatientViewModel
                {
                    PatientId = Guid.NewGuid(),
                    FirstName = "Paolo",
                    LastName = "Grigi",
                    BirthDate = new DateTime(1980, 12, 8)
                },
                Educators = new List<ProjectEducatorViewModel>
                {
                    new ProjectEducatorViewModel
                    {
                        EducatorId = Guid.NewGuid(),
                        FirstName = "Laura",
                        LastName = "Verdi",
                        Initials = "LV"
                    }
                },
                Appointments = new List<ProjectAppointmentViewModel>
                {
                    new ProjectAppointmentViewModel
                    {
                        AppointmentId = Guid.NewGuid(),
                        Type = "PrimaApertura",
                        TypeDisplay = "Prima Apertura",
                        ScheduledDate = new DateTime(2023, 6, 15, 14, 30, 0),
                        IsCompleted = true,
                        CompletedDate = new DateTime(2023, 6, 15, 16, 0, 0),
                        Location = "Centro Diurno"
                    },
                    new ProjectAppointmentViewModel
                    {
                        AppointmentId = Guid.NewGuid(),
                        Type = "Intermedia",
                        TypeDisplay = "Visita Intermedia",
                        ScheduledDate = new DateTime(2023, 12, 10, 10, 0, 0),
                        IsCompleted = true,
                        CompletedDate = new DateTime(2023, 12, 10, 11, 30, 0),
                        Location = "Ambulatorio"
                    },
                    new ProjectAppointmentViewModel
                    {
                        AppointmentId = Guid.NewGuid(),
                        Type = "Intermedia",
                        TypeDisplay = "Visita Intermedia",
                        ScheduledDate = new DateTime(2024, 6, 20, 14, 0, 0),
                        IsCompleted = true,
                        CompletedDate = new DateTime(2024, 6, 20, 15, 15, 0),
                        Location = "Centro Diurno"
                    },
                    new ProjectAppointmentViewModel
                    {
                        AppointmentId = Guid.NewGuid(),
                        Type = "Finale",
                        TypeDisplay = "Visita Finale",
                        ScheduledDate = new DateTime(2025, 5, 20, 10, 0, 0),
                        IsCompleted = false,
                        Location = "Ambulatorio"
                    }
                }
            }
        };
    }

    // Property changed handlers per aggiornamento automatico filtri
    partial void OnSearchTermChanged(string value)
    {
        ApplyFilters();
    }

    partial void OnSelectedStateFilterChanged(string value)
    {
        ApplyFilters();
    }
}
