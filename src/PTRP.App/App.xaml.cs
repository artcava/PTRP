using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PTRP.App.Services;
using PTRP.App.Services.Interfaces;
using PTRP.App.ViewModels;
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
        private ServiceProvider _serviceProvider;

        /// <summary>
        /// Configurazione e costruzione del DI container
        /// Viene eseguito prima del caricamento della finestra principale
        /// </summary>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Configura i servizi
            var services = new ServiceCollection();
            ConfigureServices(services);

            // Costruisce il service provider
            _serviceProvider = services.BuildServiceProvider();

            // Assicura che il database sia creato e migrato
            EnsureDatabaseCreated();

            // Risolve MainWindow con il suo ViewModel iniettato
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
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
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
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

            // Registra i ViewModels
            services.AddScoped<MainWindowViewModel>();

            // Registra le Views
            services.AddScoped<MainWindow>();
        }

        /// <summary>
        /// Assicura che il database sia creato e le migrations applicate
        /// </summary>
        private void EnsureDatabaseCreated()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<PTRPDbContext>();
            
            // Crea il database se non esiste e applica le migrations
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
