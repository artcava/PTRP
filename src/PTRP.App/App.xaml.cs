using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PTRP.Services;
using PTRP.Services.Interfaces;
using PTRP.ViewModels;
using PTRP.ViewModels.Educators;
using PTRP.ViewModels.Patients;
using PTRP.ViewModels.Projects;
using PTRP.Data;
using PTRP.Data.Repositories;
using PTRP.Data.Repositories.Interfaces;
using PTRP.App.Infrastructure;
using PTRP.App.Views.Patients;
using PTRP.App.Views.Educators;
using PTRP.App.Views.Projects;
using PTRP.App.Views.Sync;
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

            // Assicura che il database sia creato e popolato con dati di esempio (Issue #13)
            InitializeDatabase();

            // Risolve MainWindow e MainViewModel
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            var mainViewModel = _serviceProvider.GetRequiredService<MainViewModel>();
            mainWindow.DataContext = mainViewModel;

            // TODO: Issue #49 & #50 - Uncomment when FirstRunViewModel and DashboardViewModel are implemented
            // Controlla se è primo avvio (Issue #49: First Run Detection)
            // var configService = _serviceProvider.GetRequiredService<IConfigurationService>();
            // var isConfigured = await configService.IsConfiguredAsync();

            // if (!isConfigured)
            // {
            //     // Mostra schermata primo avvio
            //     var firstRunViewModel = _serviceProvider.GetRequiredService<FirstRunViewModel>();
            //     mainViewModel.CurrentViewModel = firstRunViewModel;
            //     mainViewModel.ShowInfoMessage("Importa un pacchetto di configurazione per iniziare");
            // }
            // else
            // {
            //     // Applicazione già configurata - carica dashboard (Issue #50)
            //     var dashboardViewModel = _serviceProvider.GetRequiredService<DashboardViewModel>();
            //     await dashboardViewModel.LoadDataAsync();
            //     mainViewModel.CurrentViewModel = dashboardViewModel;
            //     mainViewModel.ShowInfoMessage("Benvenuto nella Dashboard!");
            // }

            // TEMPORARY: Navigate to PatientListView until Issues #49 and #50 are implemented
            // MainViewModel constructor already handles this, so just show the window
            mainViewModel.ShowSuccessMessage("Benvenuto in PTRP - Lista Pazienti");

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
            services.AddScoped<IEducatorRepository, EducatorRepository>();  // Issue #63
            services.AddScoped<ITherapyProjectRepository, TherapyProjectRepository>();  // Issue #64

            // Registra i Services
            services.AddScoped<IPatientService, PatientService>();
            services.AddScoped<IEducatorService, EducatorService>();        // Issue #63
            services.AddScoped<ITherapyProjectService, TherapyProjectService>();  // Issue #64
            services.AddSingleton<INavigationService, NavigationService>();  // Issue #46: Navigation Service
            services.AddScoped<IConfigurationService, ConfigurationService>(); // Issue #49: Configuration Service

            // Registra ViewLocator (Issue #94: DI-based View resolution)
            services.AddSingleton<ViewLocator>();

            // Registra i ViewModels
            services.AddSingleton<MainViewModel>();  // Singleton per condividere stato app
            // TODO: Issue #49 - Uncomment when implemented
            // services.AddTransient<FirstRunViewModel>();  // Issue #49: First Run ViewModel
            // TODO: Issue #50 - Uncomment when implemented
            // services.AddTransient<DashboardViewModel>();  // Issue #50: Dashboard ViewModel
            services.AddTransient<PatientListViewModel>(); // Issue #51/#74: Patient List ViewModel
            services.AddTransient<EducatorListViewModel>(); // Issue #63: Educator List ViewModel
            services.AddTransient<ProjectListViewModel>(); // Issue #64: Project List ViewModel
            services.AddTransient<ProjectFormViewModel>(); // Issue #74: Project Form ViewModel
            services.AddTransient<SyncViewModel>();        // Issue #52: Sync ViewModel
            services.AddTransient<ConflictResolutionViewModel>(); // Issue #52: Conflict Resolution ViewModel

            // Registra le Views (Issue #94: Views with DI-based constructors)
            services.AddScoped<MainWindow>();
            services.AddScoped<PatientListView>();  // Issue #51/#74: Patient List View
            services.AddScoped<EducatorListView>();  // Issue #63: Educator List View
            services.AddScoped<ProjectListView>();  // Issue #64: Project List View
            services.AddScoped<SyncView>();         // Issue #52: Sync View
        }

        /// <summary>
        /// Inizializza il database creandolo se necessario e popolandolo con dati di esempio.
        /// Issue #13: Data seeding per sviluppo e testing.
        /// 
        /// IMPORTANTE: In sviluppo, ricrea il database se schema è incompleto/obsoleto.
        /// In produzione, questa logica sarà sostituita da migrations.
        /// </summary>
        private void InitializeDatabase()
        {
            if (_serviceProvider == null) return;

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<PTRPDbContext>();
            
            try
            {
                // Verifica se il database esiste e ha lo schema corretto
                // Tentativo di query su tabella critica
                _ = context.ScheduledVisits.Any();
                
                // Se arriviamo qui, il DB esiste e ha lo schema corretto
                // Popola con dati di esempio solo se vuoto (idempotente)
                DbInitializer.Initialize(context);
            }
            catch (Microsoft.Data.Sqlite.SqliteException ex) when (ex.Message.Contains("no such table"))
            {
                // Schema incompleto o obsoleto - ricrea database
                System.Diagnostics.Debug.WriteLine("Database schema incomplete. Recreating...");
                
                // Elimina e ricrea il database con schema aggiornato
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
                
                // Popola con dati di esempio
                DbInitializer.Initialize(context);
            }
            catch (Exception)
            {
                // Altri errori - prova comunque a creare/aggiornare
                context.Database.EnsureCreated();
                DbInitializer.Initialize(context);
            }
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
