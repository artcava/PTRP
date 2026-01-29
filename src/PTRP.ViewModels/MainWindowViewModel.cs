using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using PTRP.App.Models;
using PTRP.App.Services.Interfaces;

namespace PTRP.App.ViewModels
{
    /// <summary>
    /// ViewModel per la finestra principale dell'applicazione
    /// Gestisce la lista dei pazienti e i comandi CRUD
    /// </summary>
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly IPatientService _patientService;

        // Backing fields
        private ObservableCollection<PatientModel> _patients;
        private PatientModel _selectedPatient;
        private string _searchTerm;
        private string _statusMessage;
        private bool _isLoading;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Costruttore
        /// </summary>
        public MainWindowViewModel(IPatientService patientService)
        {
            _patientService = patientService ?? throw new ArgumentNullException(nameof(patientService));

            // Inizializza collezioni
            Patients = new ObservableCollection<PatientModel>();

            // Inizializza comandi
            LoadPatientsCommand = new AsyncRelayCommand(async _ => await LoadPatientsAsync());
            SearchPatientsCommand = new AsyncRelayCommand(async _ => await SearchPatientsAsync());
            ClearSearchCommand = new RelayCommand(_ => ClearSearch());
            AddPatientCommand = new AsyncRelayCommand(async _ => await AddPatientAsync());
            UpdatePatientCommand = new AsyncRelayCommand(async _ => await UpdatePatientAsync(), _ => SelectedPatient != null);
            DeletePatientCommand = new AsyncRelayCommand(async _ => await DeletePatientAsync(), _ => SelectedPatient != null);
        }

        #region Properties

        /// <summary>
        /// Collezione di pazienti
        /// </summary>
        public ObservableCollection<PatientModel> Patients
        {
            get => _patients;
            set
            {
                _patients = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Paziente selezionato nella lista
        /// </summary>
        public PatientModel SelectedPatient
        {
            get => _selectedPatient;
            set
            {
                _selectedPatient = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Termine di ricerca
        /// </summary>
        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                _searchTerm = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Messaggio di stato mostrato nella status bar
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Indica se è in corso un'operazione asincrona
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Commands

        public IAsyncCommand LoadPatientsCommand { get; }
        public IAsyncCommand SearchPatientsCommand { get; }
        public ICommand ClearSearchCommand { get; }
        public IAsyncCommand AddPatientCommand { get; }
        public IAsyncCommand UpdatePatientCommand { get; }
        public IAsyncCommand DeletePatientCommand { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Carica tutti i pazienti dal servizio
        /// </summary>
        public async Task LoadPatientsAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Caricamento pazienti...";

                var patients = await _patientService.GetAllAsync();
                
                Patients.Clear();
                foreach (var patient in patients)
                {
                    Patients.Add(patient);
                }

                StatusMessage = $"Caricati {Patients.Count} pazienti";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Errore nel caricamento: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Cerca pazienti per termine di ricerca
        /// </summary>
        private async Task SearchPatientsAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = string.IsNullOrWhiteSpace(SearchTerm) 
                    ? "Caricamento tutti i pazienti..." 
                    : $"Ricerca '{SearchTerm}'...";

                var patients = await _patientService.SearchAsync(SearchTerm ?? string.Empty);
                
                Patients.Clear();
                foreach (var patient in patients)
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
        /// Cancella la ricerca e ricarica tutti i pazienti
        /// </summary>
        private void ClearSearch()
        {
            SearchTerm = string.Empty;
            _ = LoadPatientsAsync();
        }

        /// <summary>
        /// Aggiunge un nuovo paziente (placeholder - da implementare con dialog)
        /// </summary>
        private async Task AddPatientAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Aggiunta paziente...";

                // TODO: Mostrare dialog per input dati paziente
                var newPatient = new PatientModel
                {
                    FirstName = "Nuovo",
                    LastName = "Paziente"
                };

                await _patientService.AddAsync(newPatient);
                await LoadPatientsAsync();

                StatusMessage = "Paziente aggiunto con successo";
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
        /// Aggiorna il paziente selezionato (placeholder - da implementare con dialog)
        /// </summary>
        private async Task UpdatePatientAsync()
        {
            if (SelectedPatient == null) return;

            try
            {
                IsLoading = true;
                StatusMessage = "Aggiornamento paziente...";

                // TODO: Mostrare dialog per modifica dati paziente
                await _patientService.UpdateAsync(SelectedPatient);
                await LoadPatientsAsync();

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
        /// Elimina il paziente selezionato
        /// </summary>
        private async Task DeletePatientAsync()
        {
            if (SelectedPatient == null) return;

            try
            {
                IsLoading = true;
                StatusMessage = "Eliminazione paziente...";

                // TODO: Mostrare dialog di conferma
                await _patientService.DeleteAsync(SelectedPatient.Id);
                await LoadPatientsAsync();

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

        #endregion

        #region INotifyPropertyChanged

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    /// <summary>
    /// Interfaccia per comandi asincroni
    /// </summary>
    public interface IAsyncCommand : ICommand
    {
        Task ExecuteAsync(object parameter);
    }

    /// <summary>
    /// Implementazione di comando asincrono per ViewModel
    /// </summary>
    public class AsyncRelayCommand : IAsyncCommand
    {
        private readonly Func<object, Task> _execute;
        private readonly Func<object, bool> _canExecute;
        private bool _isExecuting;

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public AsyncRelayCommand(Func<object, Task> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return !_isExecuting && (_canExecute == null || _canExecute(parameter));
        }

        public async void Execute(object parameter)
        {
            await ExecuteAsync(parameter);
        }

        public async Task ExecuteAsync(object parameter)
        {
            if (!CanExecute(parameter))
                return;

            try
            {
                _isExecuting = true;
                CommandManager.InvalidateRequerySuggested();
                await _execute(parameter);
            }
            finally
            {
                _isExecuting = false;
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    /// <summary>
    /// Implementazione semplice di ICommand per i comandi del ViewModel
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }
}
