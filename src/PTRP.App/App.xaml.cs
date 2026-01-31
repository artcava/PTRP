using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PTRP.Services;
using PTRP.Services.Interfaces;
using PTRP.ViewModels;
using PTRP.Data;
using PTRP.Data.Repositories;
using PTRP.Data.Repositories.Interfaces;
using System.IO;
using System.Windows;

namespace PTRP.App
{
    /// <summary>
    /// App.xaml.cs
    /// Configurazione dell'applicazione WPF e Dependency Injection
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Service provider per la risoluzione delle dipendenze
        /// </summary>
        private ServiceProvider? _serviceProvider;

        /// <summary>
        /// Configurazione e costruzione del DI container
        /// Viene eseguito prima del caricamento della finestra principale
        /// </summary>
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Configura i servizi
            var services = new ServiceCollection();
            ConfigureServices(services);

            // Costruisce il service provider
            _serviceProvider = services.BuildServiceProvider();

            // Assicura che il database sia creato (senza dati se primo avvio)
            EnsureDatabaseCreated();

            // Risolve MainWindow e MainViewModel
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            var mainViewModel = _serviceProvider.GetRequiredService<MainViewModel>();
            mainWindow.DataContext = mainViewModel;

            // Controlla se è primo avvio (Issue #49: First Run Detection)
            var configService = _serviceProvider.GetRequiredService<IConfigurationService>();
            var isConfigured = await configService.IsConfiguredAsync();

            if (!isConfigured)
            {
                // Mostra schermata primo avvio
                var firstRunViewModel = _serviceProvider.GetRequiredService<FirstRunViewModel>();
                mainViewModel.CurrentViewModel = firstRunViewModel;
                mainViewModel.ShowInfoMessage("Importa un pacchetto di configurazione per iniziare");
            }
            else
            {
                // Applicazione già configurata - carica dashboard normale
                // TODO: Navigare a DashboardViewModel quando sarà implementato (Issue #50)
                mainViewModel.ShowInfoMessage("Benvenuto! Dashboard in sviluppo (Issue #50)");
            }

            mainWindow.Show();
        }

        /// <summary>
        /// Registrazione di tutti i servizi nel DI container
        /// 
        /// Pattern:
        /// - AddScoped: una nuova istanza per ogni "scope" (es. per finestra)
        /// - AddSingleton: una sola istanza per l'intera applicazione
        /// - AddTransient: una nuova istanza ogni volta che viene richiesta
        /// </summary>
        private void ConfigureServices(ServiceCollection services)
        {
            // Configura il database SQLite
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "PTRP"
            );
            Directory.CreateDirectory(appDataPath);
            var dbPath = Path.Combine(appDataPath, "ptrp.db");

            services.AddDbContext<PTRPDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));

            // Registra i Repositories
            services.AddScoped<IPatientRepository, PatientRepository>();

            // Registra i Services
            services.AddScoped<IPatientService, PatientService>();
            services.AddSingleton<INavigationService, NavigationService>();  // Issue #46: Navigation Service
            services.AddScoped<IConfigurationService, ConfigurationService>(); // Issue #49: Configuration Service

            // Registra i ViewModels
            services.AddSingleton<MainViewModel>();  // Singleton per condividere stato app
            services.AddTransient<FirstRunViewModel>();  // Issue #49: First Run ViewModel
            // TODO: Registrare qui i ViewModels delle pagine quando verranno creati
            // services.AddTransient<DashboardViewModel>();  // Issue #50
            // services.AddTransient<PatientListViewModel>(); // Issue #51
            // services.AddTransient<SyncViewModel>();        // Issue #52

            // Registra le Views
            services.AddScoped<MainWindow>();
        }

        /// <summary>
        /// Assicura che il database sia creato
        /// Le migrations verranno applicate durante ConfigurationService.InitializeDatabaseAsync
        /// </summary>
        private void EnsureDatabaseCreated()
        {
            if (_serviceProvider == null) return;

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<PTRPDbContext>();
            
            // Crea il database se non esiste (senza dati)
            // I dati verranno popolati dal pacchetto di configurazione
            context.Database.EnsureCreated();
        }

        /// <summary>
        /// Pulizia delle risorse all'uscita dell'app
        /// </summary>
        protected override void OnExit(ExitEventArgs e)
        {
            _serviceProvider?.Dispose();
            base.OnExit(e);
        }
    }
}
