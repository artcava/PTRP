using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PTRP.App.Models;
using PTRP.App.Services.Interfaces;
using System.Collections.ObjectModel;

namespace PTRP.App.ViewModels
{
    /// <summary>
    /// ViewModel principale per MainWindow
    /// Gestisce la logica di presentazione e lo stato della finestra principale
    /// 
    /// Utilizza MVVM Toolkit:
    /// - ObservableObject: implementa INotifyPropertyChanged automaticamente
    /// - RelayCommand: implementa ICommand automaticamente
    /// - SetProperty: notifica binding sui cambiamenti di proprietà
    /// </summary>
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly IPatientService _patientService;

        /// <summary>
        /// Collezione osservabile di pazienti (si aggiorna automaticamente nel binding)
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<PatientModel> patients;

        /// <summary>
        /// Paziente attualmente selezionato nella lista
        /// </summary>
        [ObservableProperty]
        private PatientModel selectedPatient;

        /// <summary>
        /// Termine di ricerca per filtrare i pazienti
        /// </summary>
        [ObservableProperty]
        private string searchTerm;

        /// <summary>
        /// Flag per indicare se il caricamento dati è in corso
        /// </summary>
        [ObservableProperty]
        private bool isLoading;

        /// <summary>
        /// Messaggio di stato (errori, successi, info)
        /// </summary>
        [ObservableProperty]
        private string statusMessage;

        /// <summary>
        /// Costruttore con dependency injection del servizio
        /// </summary>
        /// <param name="patientService">Servizio per la gestione dei pazienti</param>
        public MainWindowViewModel(IPatientService patientService)
        {
            _patientService = patientService ?? throw new ArgumentNullException(nameof(patientService));
            Patients = new ObservableCollection<PatientModel>();
            SearchTerm = string.Empty;
        }

        /// <summary>
        /// Comando per caricare tutti i pazienti
        /// Viene eseguito al caricamento della finestra
        /// </summary>
        [RelayCommand]
        private async Task LoadPatients()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Caricamento pazienti...";

                var patientsList = await _patientService.GetAllAsync();

                // Pulisce e ripopola la collezione (il binding aggiornerà automaticamente l'UI)
                Patients.Clear();
                foreach (var patient in patientsList)
                {
                    Patients.Add(patient);
                }

                StatusMessage = $"Caricati {Patients.Count} pazienti";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Errore nel caricamento: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Exception: {ex}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Comando per cercare pazienti in base al termine di ricerca
        /// </summary>
        [RelayCommand]
        private async Task SearchPatients()
        {
            try
            {
                IsLoading = true;

                if (string.IsNullOrWhiteSpace(SearchTerm))
                {
                    // Se il termine è vuoto, carica tutti
                    await LoadPatientsCommand.ExecuteAsync(null);
                    return;
                }

                StatusMessage = $"Ricerca in corso per: {SearchTerm}";
                var results = await _patientService.SearchAsync(SearchTerm);

                Patients.Clear();
                foreach (var patient in results)
                {
                    Patients.Add(patient);
                }

                StatusMessage = $"Trovati {Patients.Count} pazienti";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Errore nella ricerca: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Comando per aggiungere un nuovo paziente
        /// </summary>
        [RelayCommand]
        private async Task AddPatient()
        {
            try
            {
                // Stub: crea un paziente di esempio
                var newPatient = new PatientModel
                {
                    FirstName = "Nuovo",
                    LastName = "Paziente",
                    Email = "nuovo@example.com",
                    DateOfBirth = new DateTime(1995, 1, 1),
                    PhoneNumber = "+39 000 000 0000"
                };

                IsLoading = true;
                StatusMessage = "Aggiunta paziente...";

                await _patientService.AddAsync(newPatient);
                Patients.Add(newPatient);

                StatusMessage = $"Paziente {newPatient} aggiunto con successo";
            }
            catch (ArgumentException ex)
            {
                StatusMessage = $"Validazione fallita: {ex.Message}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Errore nell'aggiunta: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Comando per eliminare il paziente selezionato
        /// </summary>
        [RelayCommand]
        private async Task DeletePatient()
        {
            if (SelectedPatient == null)
            {
                StatusMessage = "Selezionare un paziente da eliminare";
                return;
            }

            try
            {
                IsLoading = true;
                StatusMessage = $"Eliminazione di {SelectedPatient}...";

                await _patientService.DeleteAsync(SelectedPatient.Id);
                Patients.Remove(SelectedPatient);
                SelectedPatient = null;

                StatusMessage = "Paziente eliminato con successo";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Errore nell'eliminazione: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Comando per aggiornare il paziente selezionato
        /// </summary>
        [RelayCommand]
        private async Task UpdatePatient()
        {
            if (SelectedPatient == null)
            {
                StatusMessage = "Selezionare un paziente da modificare";
                return;
            }

            try
            {
                IsLoading = true;
                StatusMessage = $"Aggiornamento di {SelectedPatient}...";

                await _patientService.UpdateAsync(SelectedPatient);

                // Notifica il binding che la proprietà è cambiata
                OnPropertyChanged(nameof(SelectedPatient));

                StatusMessage = "Paziente aggiornato con successo";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Errore nell'aggiornamento: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Comando per pulire il termine di ricerca e ricaricare tutti i pazienti
        /// </summary>
        [RelayCommand]
        private async Task ClearSearch()
        {
            SearchTerm = string.Empty;
            await LoadPatientsCommand.ExecuteAsync(null);
        }
    }
}
