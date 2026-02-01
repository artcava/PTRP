using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace PTRP.ViewModels;

/// <summary>
/// ViewModel per la gestione della risoluzione conflitti durante importazione pacchetti .ptrp
/// </summary>
public partial class ConflictResolutionViewModel : ViewModelBase
{
    public override string DisplayName => "Risoluzione Conflitti";

    #region Observable Properties

    /// <summary>
    /// Lista conflitti da risolvere
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<ConflictViewModel> _conflicts = new();

    /// <summary>
    /// Numero totale conflitti
    /// </summary>
    [ObservableProperty]
    private int _totalConflictsCount;

    /// <summary>
    /// Numero conflitti risolti
    /// </summary>
    [ObservableProperty]
    private int _resolvedConflictsCount;

    /// <summary>
    /// Percentuale progresso risoluzione (0-100)
    /// </summary>
    [ObservableProperty]
    private double _resolutionProgress;

    /// <summary>
    /// Indica se tutti i conflitti sono stati risolti
    /// </summary>
    [ObservableProperty]
    private bool _allConflictsResolved;

    /// <summary>
    /// Indica se l'applicazione delle risoluzioni è in corso
    /// </summary>
    [ObservableProperty]
    private bool _isApplying;

    /// <summary>
    /// Messaggio di status operazione
    /// </summary>
    [ObservableProperty]
    private string _statusMessage = string.Empty;

    #endregion

    #region Constructor

    /// <summary>
    /// Costruttore con mock data per testing UI
    /// </summary>
    public ConflictResolutionViewModel()
    {
        // Mock data per testing
        LoadMockConflicts();
    }

    /// <summary>
    /// Costruttore con conflitti reali (da usare in produzione)
    /// </summary>
    /// <param name="conflicts">Lista conflitti rilevati</param>
    public ConflictResolutionViewModel(List<ConflictData> conflicts)
    {
        Conflicts = new ObservableCollection<ConflictViewModel>(
            conflicts.Select(c => new ConflictViewModel(c, this)));

        TotalConflictsCount = Conflicts.Count;
        UpdateProgress();
    }

    #endregion

    #region Commands

    /// <summary>
    /// Applica tutte le risoluzioni e completa l'importazione
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanApplyResolutions))]
    private async Task ApplyResolutionsAsync()
    {
        IsApplying = true;
        StatusMessage = "Applicazione risoluzioni in corso...";

        try
        {
            // TODO: Implementare con SyncService
            // var resolutions = Conflicts.Select(c => c.GetResolution()).ToList();
            // await _syncService.ApplyConflictResolutionsAsync(resolutions);

            // Mock per ora
            await Task.Delay(2000);

            StatusMessage = "✓ Tutte le risoluzioni sono state applicate con successo!";

            // TODO: Chiudere dialog e completare importazione
        }
        catch (Exception ex)
        {
            StatusMessage = $"✗ Errore durante l'applicazione: {ex.Message}";
        }
        finally
        {
            IsApplying = false;
        }
    }

    private bool CanApplyResolutions() => AllConflictsResolved && !IsApplying;

    /// <summary>
    /// Annulla l'importazione
    /// </summary>
    [RelayCommand]
    private void Cancel()
    {
        // TODO: Rollback transazione e chiudere dialog
        StatusMessage = "Importazione annullata";
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Aggiorna il progresso di risoluzione
    /// Chiamato quando un conflitto viene risolto
    /// </summary>
    public void UpdateProgress()
    {
        ResolvedConflictsCount = Conflicts.Count(c => c.IsResolved);
        ResolutionProgress = TotalConflictsCount > 0
            ? (double)ResolvedConflictsCount / TotalConflictsCount * 100
            : 0;
        AllConflictsResolved = ResolvedConflictsCount == TotalConflictsCount;

        // Ricalcola CanExecute del comando Apply
        ApplyResolutionsCommand.NotifyCanExecuteChanged();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Carica conflitti mock per testing UI
    /// </summary>
    private void LoadMockConflicts()
    {
        var mockConflicts = new List<ConflictData>
        {
            new ConflictData
            {
                Id = Guid.NewGuid(),
                Type = ConflictType.DuplicateVisit,
                LocalVersion = new ConflictVersionData
                {
                    EntityName = "Visita - Mario Rossi",
                    Timestamp = DateTime.Now.AddDays(-1),
                    Details = new Dictionary<string, string>
                    {
                        { "Data", "31/01/2026 14:30" },
                        { "Tipo", "Visita Intermedia" },
                        { "Luogo", "Centro Diurno" },
                        { "Note", "Paziente presente e collaborativo. Discussi obiettivi terapeutici." }
                    }
                },
                RemoteVersion = new ConflictVersionData
                {
                    EntityName = "Visita - Mario Rossi",
                    Timestamp = DateTime.Now.AddHours(-2),
                    Details = new Dictionary<string, string>
                    {
                        { "Data", "31/01/2026 14:30" },
                        { "Tipo", "Visita Intermedia" },
                        { "Luogo", "Ambulatorio" },
                        { "Note", "Visita svolta in ambulatorio causa maltempo. Paziente sereno." }
                    }
                }
            },
            new ConflictData
            {
                Id = Guid.NewGuid(),
                Type = ConflictType.ModifiedAppointment,
                LocalVersion = new ConflictVersionData
                {
                    EntityName = "Appuntamento - Laura Bianchi",
                    Timestamp = DateTime.Now.AddHours(-5),
                    Details = new Dictionary<string, string>
                    {
                        { "Data", "05/02/2026 10:00" },
                        { "Tipo", "Prima Apertura" },
                        { "Educatore", "Dr. Verdi" },
                        { "Luogo", "Sede Principale" }
                    }
                },
                RemoteVersion = new ConflictVersionData
                {
                    EntityName = "Appuntamento - Laura Bianchi",
                    Timestamp = DateTime.Now.AddDays(-2),
                    Details = new Dictionary<string, string>
                    {
                        { "Data", "03/02/2026 14:00" },
                        { "Tipo", "Prima Apertura" },
                        { "Educatore", "Dr. Verdi" },
                        { "Luogo", "Sede Principale" }
                    }
                }
            },
            new ConflictData
            {
                Id = Guid.NewGuid(),
                Type = ConflictType.DeletedEntity,
                LocalVersion = new ConflictVersionData
                {
                    EntityName = "Paziente - Giuseppe Neri",
                    Timestamp = DateTime.Now.AddDays(-3),
                    Details = new Dictionary<string, string>
                    {
                        { "Stato", "Eliminato" },
                        { "Motivo", "Progetto terminato" },
                        { "Data Eliminazione", "29/01/2026" }
                    }
                },
                RemoteVersion = new ConflictVersionData
                {
                    EntityName = "Visita - Giuseppe Neri",
                    Timestamp = DateTime.Now.AddHours(-10),
                    Details = new Dictionary<string, string>
                    {
                        { "Data", "30/01/2026 11:00" },
                        { "Tipo", "Verifica Intermedia" },
                        { "Luogo", "Domicilio" },
                        { "Note", "Visita domiciliare. Paziente stabile." }
                    }
                }
            }
        };

        Conflicts = new ObservableCollection<ConflictViewModel>(
            mockConflicts.Select(c => new ConflictViewModel(c, this)));

        TotalConflictsCount = Conflicts.Count;
        UpdateProgress();
    }

    #endregion
}

#region Helper Classes

/// <summary>
/// ViewModel per singolo conflitto
/// </summary>
public partial class ConflictViewModel : ObservableObject
{
    private readonly ConflictResolutionViewModel _parent;

    public Guid Id { get; }
    public ConflictType Type { get; }
    public ConflictVersionData LocalVersion { get; }
    public ConflictVersionData RemoteVersion { get; }

    /// <summary>
    /// Indica se il conflitto è stato risolto
    /// </summary>
    [ObservableProperty]
    private bool _isResolved;

    /// <summary>
    /// Strategia di risoluzione selezionata
    /// </summary>
    [ObservableProperty]
    private ResolutionStrategy _selectedStrategy;

    public ConflictViewModel(ConflictData data, ConflictResolutionViewModel parent)
    {
        _parent = parent;
        Id = data.Id;
        Type = data.Type;
        LocalVersion = data.LocalVersion;
        RemoteVersion = data.RemoteVersion;
    }

    /// <summary>
    /// Tipo conflitto in formato leggibile
    /// </summary>
    public string TypeDisplay => Type switch
    {
        ConflictType.DuplicateVisit => "Visita Duplicata",
        ConflictType.ModifiedAppointment => "Appuntamento Modificato",
        ConflictType.DeletedEntity => "Entità Eliminata",
        _ => "Sconosciuto"
    };

    /// <summary>
    /// Colore badge tipo conflitto
    /// </summary>
    public string TypeColor => Type switch
    {
        ConflictType.DuplicateVisit => "#FF9800",     // Arancione
        ConflictType.ModifiedAppointment => "#2196F3", // Blu
        ConflictType.DeletedEntity => "#F44336",      // Rosso
        _ => "#9E9E9E"                                 // Grigio
    };

    /// <summary>
    /// Icona MaterialDesign per tipo conflitto
    /// </summary>
    public string TypeIcon => Type switch
    {
        ConflictType.DuplicateVisit => "ContentDuplicate",
        ConflictType.ModifiedAppointment => "CalendarEdit",
        ConflictType.DeletedEntity => "Delete",
        _ => "Alert"
    };

    /// <summary>
    /// Seleziona versione locale
    /// </summary>
    [RelayCommand]
    private void SelectLocal()
    {
        SelectedStrategy = ResolutionStrategy.KeepLocal;
        IsResolved = true;
        _parent.UpdateProgress();
    }

    /// <summary>
    /// Seleziona versione remota
    /// </summary>
    [RelayCommand]
    private void SelectRemote()
    {
        SelectedStrategy = ResolutionStrategy.KeepRemote;
        IsResolved = true;
        _parent.UpdateProgress();
    }

    /// <summary>
    /// Seleziona merge (solo per DuplicateVisit)
    /// </summary>
    [RelayCommand]
    private void SelectMerge()
    {
        SelectedStrategy = ResolutionStrategy.Merge;
        IsResolved = true;
        _parent.UpdateProgress();
    }

    /// <summary>
    /// Ottiene la risoluzione selezionata
    /// </summary>
    public ConflictResolution GetResolution()
    {
        return new ConflictResolution
        {
            ConflictId = Id,
            Strategy = SelectedStrategy,
            Timestamp = DateTime.Now
        };
    }
}

/// <summary>
/// Dati di un conflitto
/// </summary>
public class ConflictData
{
    public Guid Id { get; set; }
    public ConflictType Type { get; set; }
    public ConflictVersionData LocalVersion { get; set; } = new();
    public ConflictVersionData RemoteVersion { get; set; } = new();
}

/// <summary>
/// Dati di una versione (locale o remota)
/// </summary>
public class ConflictVersionData
{
    public string EntityName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Dictionary<string, string> Details { get; set; } = new();
}

/// <summary>
/// Risoluzione applicata a un conflitto
/// </summary>
public class ConflictResolution
{
    public Guid ConflictId { get; set; }
    public ResolutionStrategy Strategy { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Tipo di conflitto
/// </summary>
public enum ConflictType
{
    DuplicateVisit,
    ModifiedAppointment,
    DeletedEntity
}

/// <summary>
/// Strategia di risoluzione
/// </summary>
public enum ResolutionStrategy
{
    None,
    KeepLocal,
    KeepRemote,
    Merge
}

#endregion
