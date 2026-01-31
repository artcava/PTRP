using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PTRP.ViewModels;

/// <summary>
/// ViewModel per Dashboard Coordinatore
/// Visualizza KPI, statistiche e top educatori
/// </summary>
public partial class DashboardViewModel : ViewModelBase
{
    public override string DisplayName => "Dashboard";

    #region KPI Properties

    /// <summary>
    /// Totale pazienti attivi
    /// </summary>
    [ObservableProperty]
    private int _totalPatientsCount;

    /// <summary>
    /// Totale progetti terapeutici attivi
    /// </summary>
    [ObservableProperty]
    private int _activeProjectsCount;

    /// <summary>
    /// Totale educatori operativi
    /// </summary>
    [ObservableProperty]
    private int _operationalEducatorsCount;

    /// <summary>
    /// Visite completate nel mese corrente
    /// </summary>
    [ObservableProperty]
    private int _completedVisitsCount;

    /// <summary>
    /// Totale visite programmate nel mese corrente
    /// </summary>
    [ObservableProperty]
    private int _totalScheduledVisitsCount;

    /// <summary>
    /// Percentuale completamento visite (0-100)
    /// </summary>
    [ObservableProperty]
    private double _visitsCompletionPercentage;

    /// <summary>
    /// Display del mese corrente (es. "Gennaio 2026")
    /// </summary>
    [ObservableProperty]
    private string _currentMonthDisplay = DateTime.Now.ToString("MMMM yyyy");

    #endregion

    #region Collections

    /// <summary>
    /// Top 5 educatori per numero visite
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<TopEducatorViewModel> _topEducators = new();

    #endregion

    #region Loading State

    /// <summary>
    /// Indica se i dati stanno caricando
    /// </summary>
    [ObservableProperty]
    private bool _isLoading;

    #endregion

    /// <summary>
    /// Constructor per DI
    /// </summary>
    public DashboardViewModel()
    {
        // Per ora senza dipendenze repository
        // Verranno aggiunte quando i repository saranno implementati
    }

    /// <summary>
    /// Carica dati dashboard in modo asincrono
    /// </summary>
    public async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;

            // TODO: Implementare caricamento dati reali quando repository saranno disponibili
            // Per ora uso dati di esempio

            await Task.Delay(500); // Simula caricamento

            // Dati di esempio
            TotalPatientsCount = 42;
            ActiveProjectsCount = 18;
            OperationalEducatorsCount = 7;
            CompletedVisitsCount = 156;
            TotalScheduledVisitsCount = 200;
            VisitsCompletionPercentage = TotalScheduledVisitsCount > 0
                ? (double)CompletedVisitsCount / TotalScheduledVisitsCount * 100
                : 0;

            // Top educatori di esempio
            TopEducators = new ObservableCollection<TopEducatorViewModel>
            {
                new() { Rank = 1, Name = "Marco Rossi", VisitsCount = 45 },
                new() { Rank = 2, Name = "Laura Bianchi", VisitsCount = 38 },
                new() { Rank = 3, Name = "Giuseppe Verdi", VisitsCount = 32 },
                new() { Rank = 4, Name = "Anna Neri", VisitsCount = 28 },
                new() { Rank = 5, Name = "Luca Gialli", VisitsCount = 13 }
            };

            /* CODICE FUTURO quando repository saranno disponibili:
            
            // Carica KPI
            TotalPatientsCount = await _patientRepository.GetTotalCountAsync();
            ActiveProjectsCount = await _projectRepository.GetActiveCountAsync();
            OperationalEducatorsCount = await _operatorRepository.GetOperationalCountAsync();

            // Visite mese corrente
            var currentMonth = DateTime.Now;
            var visits = await _visitRepository.GetVisitsByMonthAsync(
                currentMonth.Year,
                currentMonth.Month);

            CompletedVisitsCount = visits.Count(v => v.IsCompleted);
            TotalScheduledVisitsCount = visits.Count;
            VisitsCompletionPercentage = TotalScheduledVisitsCount > 0
                ? (double)CompletedVisitsCount / TotalScheduledVisitsCount * 100
                : 0;

            // Top educatori
            var topEducatorsData = await _operatorRepository.GetTopEducatorsByVisitsAsync(5);
            TopEducators = new ObservableCollection<TopEducatorViewModel>(
                topEducatorsData.Select((e, index) => new TopEducatorViewModel
                {
                    Rank = index + 1,
                    Name = $"{e.FirstName} {e.LastName}",
                    VisitsCount = e.VisitsCount
                }));
            */
        }
        catch (Exception ex)
        {
            // TODO: Log error quando logging sarà implementato
            // _logger.LogError(ex, "Error loading dashboard data");
            
            // Reset a valori default in caso di errore
            TotalPatientsCount = 0;
            ActiveProjectsCount = 0;
            OperationalEducatorsCount = 0;
            CompletedVisitsCount = 0;
            TotalScheduledVisitsCount = 0;
            VisitsCompletionPercentage = 0;
            TopEducators.Clear();
        }
        finally
        {
            IsLoading = false;
        }
    }
}

/// <summary>
/// ViewModel per rappresentare un educatore nella classifica
/// </summary>
public class TopEducatorViewModel
{
    /// <summary>
    /// Posizione in classifica (1-5)
    /// </summary>
    public int Rank { get; set; }

    /// <summary>
    /// Nome completo educatore
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Numero visite effettuate nel mese
    /// </summary>
    public int VisitsCount { get; set; }
}
