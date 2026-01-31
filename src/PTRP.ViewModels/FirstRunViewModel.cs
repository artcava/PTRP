using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using PTRP.Services.Interfaces;

namespace PTRP.ViewModels;

/// <summary>
/// ViewModel per schermata First Run Setup
/// Gestisce importazione pacchetto di configurazione .ptrp
/// </summary>
public partial class FirstRunViewModel : ViewModelBase
{
    private readonly IConfigurationService _configurationService;
    private readonly INavigationService _navigationService;
    private readonly MainViewModel _mainViewModel;
    
    public override string DisplayName => "Configurazione Iniziale";
    
    /// <summary>
    /// Indica se è in corso l'importazione
    /// </summary>
    [ObservableProperty]
    private bool _isImporting = false;
    
    /// <summary>
    /// Messaggio di stato durante importazione
    /// </summary>
    [ObservableProperty]
    private string _importStatusMessage = string.Empty;
    
    public FirstRunViewModel(
        IConfigurationService configurationService,
        INavigationService navigationService,
        MainViewModel mainViewModel)
    {
        _configurationService = configurationService;
        _navigationService = navigationService;
        _mainViewModel = mainViewModel;
    }
    
    /// <summary>
    /// Comando per importare pacchetto di configurazione
    /// </summary>
    [RelayCommand]
    private async Task ImportConfigurationAsync()
    {
        try
        {
            // Apri dialog per selezionare file .ptrp
            var openFileDialog = new OpenFileDialog
            {
                Title = "Seleziona Pacchetto di Configurazione",
                Filter = "Pacchetti PTRP (*.ptrp)|*.ptrp|Tutti i file (*.*)|*.*",
                CheckFileExists = true,
                Multiselect = false
            };
            
            if (openFileDialog.ShowDialog() != true)
                return;
            
            var filePath = openFileDialog.FileName;
            
            // Attiva loading
            IsImporting = true;
            _mainViewModel.IsLoading = true;
            
            // Step 1: Verifica integrità pacchetto
            ImportStatusMessage = "Verifica integrità pacchetto...";
            _mainViewModel.ShowInfoMessage("Verifica integrità in corso...");
            await Task.Delay(500); // Simula verifica
            
            // Step 2: Verifica firma HMAC
            ImportStatusMessage = "Verifica firma digitale...";
            var isValid = await _configurationService.ValidatePackageAsync(filePath);
            
            if (!isValid)
            {
                _mainViewModel.ShowErrorMessage(
                    "Il pacchetto selezionato non è valido o è stato manomesso.",
                    5000);
                return;
            }
            
            // Step 3: Importa configurazione
            ImportStatusMessage = "Importazione dati...";
            _mainViewModel.ShowInfoMessage("Importazione dati in corso...");
            await Task.Delay(500);
            
            var config = await _configurationService.ImportConfigurationAsync(filePath);
            
            // Step 4: Crea database locale
            ImportStatusMessage = "Creazione database locale...";
            _mainViewModel.ShowInfoMessage("Creazione database...");
            await Task.Delay(500);
            
            await _configurationService.InitializeDatabaseAsync(config);
            
            // Step 5: Configura profilo utente
            ImportStatusMessage = "Configurazione profilo utente...";
            _mainViewModel.ShowInfoMessage("Configurazione profilo...");
            await Task.Delay(500);
            
            await _configurationService.SetupUserProfileAsync(config);
            
            // Step 6: Aggiorna database status
            var dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "PTRP",
                "ptrp.db");
            
            if (File.Exists(dbPath))
            {
                var fileInfo = new FileInfo(dbPath);
                _mainViewModel.UpdateDatabaseStatus(true, fileInfo.Length);
            }
            
            // Step 7: Success - Ricarica MainViewModel con nuovo profilo
            ImportStatusMessage = "Completamento...";
            await Task.Delay(300);
            
            await _mainViewModel.ReloadAfterConfigurationAsync();
            
            _mainViewModel.ShowSuccessMessage(
                "Configurazione completata con successo!");
            
            // Step 8: Naviga a Dashboard
            await Task.Delay(500);
            
            // TODO: Navigare a DashboardViewModel quando sarà implementato (Issue #50)
            // Per ora mostra messaggio
            _mainViewModel.ShowInfoMessage(
                "Configurazione completata! Dashboard in sviluppo (Issue #50)");
            
            // Quando DashboardViewModel sarà pronto:
            // _navigationService.NavigateTo<DashboardViewModel>();
        }
        catch (InvalidOperationException ex)
        {
            _mainViewModel.ShowErrorMessage(
                $"Errore: {ex.Message}",
                5000);
        }
        catch (Exception ex)
        {
            _mainViewModel.ShowErrorMessage(
                $"Errore durante l'importazione: {ex.Message}",
                5000);
            
            // TODO: Log error con ILogger quando sarà integrato
            // _logger.LogError(ex, "Error importing configuration package");
        }
        finally
        {
            IsImporting = false;
            _mainViewModel.IsLoading = false;
            ImportStatusMessage = string.Empty;
        }
    }
}
