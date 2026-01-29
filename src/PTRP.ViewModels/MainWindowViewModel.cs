using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using PTRP.Models;
using PTRP.Services.Interfaces;

namespace PTRP.ViewModels
{
    /// <summary>
    /// ViewModel per la finestra principale dell'applicazione
    /// Gestisce la lista dei pazienti e i comandi CRUD
    /// </summary>
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly IPatientService _patientService;
        private CancellationTokenSource _searchCancellationTokenSource;
        private const int SearchDelayMs = 400;

        // Backing fields
        private ObservableCollection<PatientModel>? _patients;
        private PatientModel? _selectedPatient;
        private string? _searchTerm;
        private string? _statusMessage;
        private bool _isLoading;
        private bool _hasSearchText;

        public event PropertyChangedEventHandler? PropertyChanged;

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
            ClearSearchCommand = new RelayCommand(_ => ClearSearch());
            AddNewPatientCommand = new RelayCommand(_ => AddNewPatient());
            EditPatientCommand = new AsyncRelayCommand(async param => await EditPatientAsync(param as PatientModel));
            DeletePatientCommand = new AsyncRelayCommand(async param => await DeletePatientAsync(param as PatientModel));
            SavePatientCommand = new AsyncRelayCommand(async param => await SavePatientAsync(param as PatientModel));
            CancelEditCommand = new RelayCommand(param => CancelEdit(param as PatientModel));
        }

        #region Properties

        /// <summary>
        /// Collezione di pazienti
        /// </summary>
        public ObservableCollection<PatientModel>? Patients
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
        public PatientModel? SelectedPatient
        {
            get => _selectedPatient;
            set
            {
                _selectedPatient = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Termine di ricerca con debounce automatico
        /// </summary>
        public string? SearchTerm
        {
            get => _searchTerm;
            set
            {
                _searchTerm = value;
                OnPropertyChanged();
                HasSearchText = !string.IsNullOrWhiteSpace(value);
                _ = PerformDebouncedSearchAsync();
            }
        }

        /// <summary>
        /// Indica se c'è testo nel campo di ricerca (per mostrare bottone X)
        /// </summary>
        public bool HasSearchText
        {
            get => _hasSearchText;
            set
            {
                _hasSearchText = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Messaggio di stato mostrato nella status bar
        /// </summary>
        public string? StatusMessage
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
        public ICommand ClearSearchCommand { get; }
        public ICommand AddNewPatientCommand { get; }
        public IAsyncCommand EditPatientCommand { get; }
        public IAsyncCommand DeletePatientCommand { get; }
        public IAsyncCommand SavePatientCommand { get; }
        public ICommand CancelEditCommand { get; }

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
                
                if (Patients != null)
                {
                    Patients.Clear();
                    foreach (var patient in patients)
                    {
                        Patients.Add(patient);
                    }
                    StatusMessage = $"Caricati {Patients.Count} pazienti";
                }
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
        /// Esegue ricerca con debounce automatico
        /// </summary>
        private async Task PerformDebouncedSearchAsync()
        {
            // Cancella la ricerca precedente
            _searchCancellationTokenSource?.Cancel();
            _searchCancellationTokenSource = new CancellationTokenSource();
            var token = _searchCancellationTokenSource.Token;

            try
            {
                // Attendi il delay (debounce)
                await Task.Delay(SearchDelayMs, token);

                // Se non è stato cancellato, esegui la ricerca
                if (!token.IsCancellationRequested)
                {
                    await SearchPatientsAsync();
                }
            }
            catch (TaskCanceledException)
            {
                // Ricerca cancellata, ignora
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
                
                if (Patients != null)
                {
                    Patients.Clear();
                    foreach (var patient in patients)
                    {
                        Patients.Add(patient);
                    }
                    StatusMessage = $"Trovati {Patients.Count} pazienti";
                }
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
        }

        /// <summary>
        /// Aggiunge una nuova riga vuota per inserimento paziente
        /// </summary>
        private void AddNewPatient()
        {
            var newPatient = new PatientModel
            {
                FirstName = "",
                LastName = "",
                IsEditing = true,
                IsNew = true
            };

            Patients?.Insert(0, newPatient);
            StatusMessage = "Inserisci i dati del nuovo paziente e clicca Salva";
        }

        /// <summary>
        /// Abilita la modifica per un paziente esistente
        /// </summary>
        private async Task EditPatientAsync(PatientModel? patient)
        {
            if (patient == null) return;

            // Salva i valori originali per eventuale annullamento
            patient.OriginalFirstName = patient.FirstName;
            patient.OriginalLastName = patient.LastName;
            patient.IsEditing = true;

            StatusMessage = $"Modifica {patient.FirstName} {patient.LastName}";
            await Task.CompletedTask;
        }

        /// <summary>
        /// Elimina un paziente con conferma
        /// </summary>
        private async Task DeletePatientAsync(PatientModel? patient)
        {
            if (patient == null) return;

            // Mostra dialog di conferma
            var result = MessageBox.Show(
                $"Sei sicuro di voler eliminare il paziente?\n\nNome: {patient.FirstName}\nCognome: {patient.LastName}",
                "Conferma Eliminazione",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning,
                MessageBoxResult.No
            );

            if (result != MessageBoxResult.Yes)
            {
                StatusMessage = "Eliminazione annullata";
                return;
            }

            try
            {
                IsLoading = true;
                StatusMessage = "Eliminazione paziente...";

                await _patientService.DeleteAsync(patient.Id);
                Patients?.Remove(patient);

                StatusMessage = $"Paziente {patient.FirstName} {patient.LastName} eliminato con successo";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Errore nell'eliminazione: {ex.Message}";
                MessageBox.Show($"Impossibile eliminare il paziente:\n{ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Salva un paziente (nuovo o modificato)
        /// </summary>
        private async Task SavePatientAsync(PatientModel? patient)
        {
            if (patient == null) return;

            try
            {
                IsLoading = true;
                StatusMessage = patient.IsNew ? "Aggiunta paziente..." : "Aggiornamento paziente...";

                if (patient.IsNew)
                {
                    await _patientService.AddAsync(patient);
                    patient.IsNew = false;
                    StatusMessage = $"Paziente {patient.FirstName} {patient.LastName} aggiunto con successo";
                }
                else
                {
                    await _patientService.UpdateAsync(patient);
                    StatusMessage = $"Paziente {patient.FirstName} {patient.LastName} aggiornato con successo";
                }

                patient.IsEditing = false;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Errore nel salvataggio: {ex.Message}";
                MessageBox.Show($"Impossibile salvare il paziente:\n{ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Annulla la modifica di un paziente
        /// </summary>
        private void CancelEdit(PatientModel? patient)
        {
            if (patient == null) return;

            if (patient.IsNew)
            {
                // Se è nuovo, rimuovilo dalla lista
                Patients?.Remove(patient);
                StatusMessage = "Inserimento annullato";
            }
            else
            {
                // Ripristina i valori originali
                patient.FirstName = patient.OriginalFirstName;
                patient.LastName = patient.OriginalLastName;
                patient.IsEditing = false;
                StatusMessage = "Modifiche annullate";
            }
        }

        #endregion

        #region INotifyPropertyChanged

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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
        Task ExecuteAsync(object? parameter);
    }

    /// <summary>
    /// Implementazione di comando asincrono per ViewModel
    /// </summary>
    public class AsyncRelayCommand : IAsyncCommand
    {
        private readonly Func<object?, Task> _execute;
        private readonly Func<object?, bool>? _canExecute;
        private bool _isExecuting;

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public AsyncRelayCommand(Func<object?, Task> execute, Func<object?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            return !_isExecuting && (_canExecute == null || _canExecute(parameter));
        }

        public async void Execute(object? parameter)
        {
            await ExecuteAsync(parameter);
        }

        public async Task ExecuteAsync(object? parameter)
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
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object? parameter)
        {
            _execute(parameter);
        }
    }
}
