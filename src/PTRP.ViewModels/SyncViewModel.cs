using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using PTRP.Services.Interfaces;
using System.Collections.ObjectModel;
using System.IO;

namespace PTRP.ViewModels;

/// <summary>
/// ViewModel per la gestione della sincronizzazione (Import/Export pacchetti .ptrp)
/// Supporta sia ruolo Coordinatore che Educatore con UI dinamica
/// </summary>
public partial class SyncViewModel : ViewModelBase
{
    private readonly IConfigurationService _configService;
    private readonly IEducatorService _educatorService;
    // TODO: Aggiungere ISyncService quando implementato

    public override string DisplayName => "Sincronizzazione";

    #region Observable Properties

    /// <summary>
    /// Ruolo corrente utente (Coordinator/Educator)
    /// </summary>
    [ObservableProperty]
    private string _currentRole = "Coordinator";

    /// <summary>
    /// Tab corrente selezionato (0=Esporta, 1=Importa, 2=Storico)
    /// </summary>
    [ObservableProperty]
    private int _selectedTabIndex;

    #endregion

    #region Export Properties

    /// <summary>
    /// Lista educatori disponibili per export (solo Coordinatore)
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<EducatorItemViewModel> _availableEducators = new();

    /// <summary>
    /// Educatore selezionato per export
    /// </summary>
    [ObservableProperty]
    private EducatorItemViewModel? _selectedEducator;

    /// <summary>
    /// Data inizio periodo export
    /// </summary>
    [ObservableProperty]
    private DateTime _exportStartDate = DateTime.Now;

    /// <summary>
    /// Data fine periodo export
    /// </summary>
    [ObservableProperty]
    private DateTime _exportEndDate = DateTime.Now.AddMonths(1);

    /// <summary>
    /// Numero appuntamenti da esportare
    /// </summary>
    [ObservableProperty]
    private int _appointmentsToExportCount;

    /// <summary>
    /// Preview dati export
    /// </summary>
    [ObservableProperty]
    private string _exportPreview = "Seleziona educatore e periodo per visualizzare anteprima";

    /// <summary>
    /// Indica se export è in corso
    /// </summary>
    [ObservableProperty]
    private bool _isExporting;

    /// <summary>
    /// Messaggio status export
    /// </summary>
    [ObservableProperty]
    private string _exportStatusMessage = string.Empty;

    #endregion

    #region Import Properties

    /// <summary>
    /// Path file selezionato per import
    /// </summary>
    [ObservableProperty]
    private string _selectedImportFilePath = string.Empty;

    /// <summary>
    /// Indica se file è stato validato
    /// </summary>
    [ObservableProperty]
    private bool _isFileValidated;

    /// <summary>
    /// Indica se HMAC è valido
    /// </summary>
    [ObservableProperty]
    private bool _isHmacValid;

    /// <summary>
    /// Indica se versione schema è compatibile
    /// </summary>
    [ObservableProperty]
    private bool _isSchemaCompatible;

    /// <summary>
    /// Indica se data pacchetto è obsoleta
    /// </summary>
    [ObservableProperty]
    private bool _isPackageDateObsolete;

    /// <summary>
    /// Data creazione pacchetto
    /// </summary>
    [ObservableProperty]
    private DateTime? _packageDate;

    /// <summary>
    /// Numero record nel pacchetto
    /// </summary>
    [ObservableProperty]
    private int _packageRecordsCount;

    /// <summary>
    /// Preview contenuto pacchetto
    /// </summary>
    [ObservableProperty]
    private string _importPreview = "Seleziona file .ptrp per visualizzare contenuto";

    /// <summary>
    /// Indica se import è in corso
    /// </summary>
    [ObservableProperty]
    private bool _isImporting;

    /// <summary>
    /// Messaggio status import
    /// </summary>
    [ObservableProperty]
    private string _importStatusMessage = string.Empty;

    /// <summary>
    /// Messaggi di validazione
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<ValidationMessageViewModel> _validationMessages = new();

    #endregion

    #region History Properties

    /// <summary>
    /// Storico sincronizzazioni
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<SyncHistoryItemViewModel> _syncHistory = new();

    #endregion

    #region Constructor

    public SyncViewModel(
        IConfigurationService configService,
        IEducatorService educatorService)
    {
        _configService = configService;
        _educatorService = educatorService;

        // TODO: Carica ruolo da configurazione
        CurrentRole = "Coordinator"; // Hardcoded per ora

        // Carica educatori se Coordinatore
        if (CurrentRole == "Coordinator")
        {
            LoadEducatorsAsync();
        }

        LoadSyncHistory();
    }

    #endregion

    #region Commands

    /// <summary>
    /// Carica anteprima dati da esportare
    /// </summary>
    [RelayCommand]
    private async Task LoadExportPreviewAsync()
    {
        if (SelectedEducator == null)
        {
            ExportPreview = "Seleziona un educatore";
            AppointmentsToExportCount = 0;
            return;
        }

        // TODO: Implementare con SyncService
        // var preview = await _syncService.GetExportPreviewAsync(
        //     SelectedEducator.Id, ExportStartDate, ExportEndDate);

        // Mock per ora
        await Task.Delay(500);
        AppointmentsToExportCount = 15;
        ExportPreview = $"Appuntamenti per {SelectedEducator.FullName}:\n" +
                       $"- Periodo: {ExportStartDate:dd/MM/yyyy} - {ExportEndDate:dd/MM/yyyy}\n" +
                       $"- Totale appuntamenti: {AppointmentsToExportCount}\n" +
                       $"- Pazienti coinvolti: 8";
    }

    /// <summary>
    /// Esporta pacchetto dati
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExport))]
    private async Task ExportPackageAsync()
    {
        if (SelectedEducator == null) return;

        // Apri SaveFileDialog
        var dialog = new SaveFileDialog
        {
            Filter = "PTRP Package|*.ptrp",
            FileName = $"export_{SelectedEducator.LastName}_{DateTime.Now:yyyyMMdd}.ptrp",
            DefaultExt = "ptrp"
        };

        if (dialog.ShowDialog() != true) return;

        IsExporting = true;
        ExportStatusMessage = "Esportazione in corso...";

        try
        {
            // TODO: Implementare con SyncService
            // await _syncService.ExportPackageAsync(
            //     SelectedEducator.Id, ExportStartDate, ExportEndDate, dialog.FileName);

            await Task.Delay(2000); // Mock

            ExportStatusMessage = $"✓ Pacchetto esportato: {Path.GetFileName(dialog.FileName)}";

            // Aggiungi a storico
            SyncHistory.Insert(0, new SyncHistoryItemViewModel
            {
                Date = DateTime.Now,
                Type = "Export",
                FileName = Path.GetFileName(dialog.FileName),
                RecordsCount = AppointmentsToExportCount,
                Status = "Success"
            });
        }
        catch (Exception ex)
        {
            ExportStatusMessage = $"✗ Errore: {ex.Message}";
        }
        finally
        {
            IsExporting = false;
        }
    }

    private bool CanExport() => SelectedEducator != null && AppointmentsToExportCount > 0 && !IsExporting;

    /// <summary>
    /// Seleziona file per import
    /// </summary>
    [RelayCommand]
    private async Task SelectImportFileAsync()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "PTRP Package|*.ptrp",
            DefaultExt = "ptrp"
        };

        if (dialog.ShowDialog() != true) return;

        SelectedImportFilePath = dialog.FileName;
        await ValidateImportFileAsync();
    }

    /// <summary>
    /// Valida file import
    /// </summary>
    private async Task ValidateImportFileAsync()
    {
        ValidationMessages.Clear();
        IsFileValidated = false;

        if (string.IsNullOrEmpty(SelectedImportFilePath))
            return;

        ImportPreview = "Validazione in corso...";

        try
        {
            // TODO: Implementare con SyncService
            // var validation = await _syncService.ValidatePackageAsync(SelectedImportFilePath);

            // Mock per ora
            await Task.Delay(1000);

            IsHmacValid = true;
            IsSchemaCompatible = true;
            PackageDate = DateTime.Now.AddDays(-2);
            IsPackageDateObsolete = false;
            PackageRecordsCount = 23;

            ValidationMessages.Add(new ValidationMessageViewModel
            {
                IsValid = IsHmacValid,
                Message = "Firma HMAC valida"
            });
            ValidationMessages.Add(new ValidationMessageViewModel
            {
                IsValid = IsSchemaCompatible,
                Message = "Schema compatibile"
            });
            ValidationMessages.Add(new ValidationMessageViewModel
            {
                IsValid = !IsPackageDateObsolete,
                Message = PackageDate.HasValue
                    ? $"Data pacchetto: {PackageDate.Value:dd/MM/yyyy HH:mm}"
                    : "Data non disponibile"
            });

            IsFileValidated = IsHmacValid && IsSchemaCompatible;

            ImportPreview = $"Contenuto pacchetto:\n" +
                           $"- Record totali: {PackageRecordsCount}\n" +
                           $"- Visite registrate: {PackageRecordsCount}\n" +
                           $"- Data creazione: {PackageDate:dd/MM/yyyy HH:mm}";
        }
        catch (Exception ex)
        {
            ValidationMessages.Add(new ValidationMessageViewModel
            {
                IsValid = false,
                Message = $"Errore validazione: {ex.Message}"
            });
            ImportPreview = "File non valido";
        }
    }

    /// <summary>
    /// Importa pacchetto dati
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanImport))]
    private async Task ImportPackageAsync()
    {
        IsImporting = true;
        ImportStatusMessage = "Importazione in corso...";

        try
        {
            // TODO: Implementare con SyncService
            // var result = await _syncService.ImportPackageAsync(SelectedImportFilePath);

            await Task.Delay(2000); // Mock

            ImportStatusMessage = $"✓ Importazione completata: {PackageRecordsCount} record";

            // Aggiungi a storico
            SyncHistory.Insert(0, new SyncHistoryItemViewModel
            {
                Date = DateTime.Now,
                Type = "Import",
                FileName = Path.GetFileName(SelectedImportFilePath),
                RecordsCount = PackageRecordsCount,
                Status = "Success"
            });

            // Reset form
            SelectedImportFilePath = string.Empty;
            IsFileValidated = false;
            ValidationMessages.Clear();
            ImportPreview = "Seleziona file .ptrp per visualizzare contenuto";
        }
        catch (Exception ex)
        {
            ImportStatusMessage = $"✗ Errore: {ex.Message}";
        }
        finally
        {
            IsImporting = false;
        }
    }

    private bool CanImport() => IsFileValidated && !IsImporting;

    #endregion

    #region Private Methods

    private async void LoadEducatorsAsync()
    {
        try
        {
            // FIX: Cambiato da GetAllEducatorsAsync() a GetAllAsync()
            var educators = await _educatorService.GetAllAsync();
            AvailableEducators = new ObservableCollection<EducatorItemViewModel>(
                educators.Select(e => new EducatorItemViewModel
                {
                    Id = e.Id,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    FullName = $"{e.FirstName} {e.LastName}"
                }));
        }
        catch
        {
            // TODO: Gestione errori
        }
    }

    private void LoadSyncHistory()
    {
        // TODO: Caricare da database
        // Mock per ora
        SyncHistory = new ObservableCollection<SyncHistoryItemViewModel>
        {
            new SyncHistoryItemViewModel
            {
                Date = DateTime.Now.AddDays(-1),
                Type = "Export",
                FileName = "export_rossi_20260130.ptrp",
                RecordsCount = 12,
                Status = "Success"
            },
            new SyncHistoryItemViewModel
            {
                Date = DateTime.Now.AddDays(-3),
                Type = "Import",
                FileName = "visite_bianchi_20260127.ptrp",
                RecordsCount = 8,
                Status = "Success"
            }
        };
    }

    #endregion
}

#region Helper ViewModels

public class EducatorItemViewModel
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}

public class ValidationMessageViewModel
{
    public bool IsValid { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class SyncHistoryItemViewModel
{
    public DateTime Date { get; set; }
    public string Type { get; set; } = string.Empty; // "Export" or "Import"
    public string FileName { get; set; } = string.Empty;
    public int RecordsCount { get; set; }
    public string Status { get; set; } = string.Empty; // "Success", "Failed", "Warning"

    public string StatusIcon => Status switch
    {
        "Success" => "✓",
        "Failed" => "✗",
        "Warning" => "⚠",
        _ => "·"
    };

    public string StatusColor => Status switch
    {
        "Success" => "#4CAF50",
        "Failed" => "#F44336",
        "Warning" => "#FF9800",
        _ => "#9E9E9E"
    };
}

#endregion
