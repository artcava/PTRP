using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using PTRP.App.Models;

namespace PTRP.Models
{
    /// <summary>
    /// Modello per rappresentare un Paziente
    /// </summary>
    public class PatientModel : INotifyPropertyChanged
    {
        private string _firstName;
        private string _lastName;
        private bool _isEditing;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Identificatore univoco del paziente
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Nome del paziente
        /// </summary>
        public string FirstName
        {
            get => _firstName;
            set
            {
                _firstName = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Cognome del paziente
        /// </summary>
        public string LastName
        {
            get => _lastName;
            set
            {
                _lastName = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Data di creazione del record (read-only in UI)
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Data dell'ultimo aggiornamento del record (read-only in UI)
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Collezione di Progetti Terapeutici associati al paziente
        /// </summary>
        public ICollection<TherapyProjectModel> TherapyProjects { get; set; } = new List<TherapyProjectModel>();

        #region UI-Only Properties (Not Mapped to Database)

        /// <summary>
        /// Indica se il paziente è in modalità editing (mostra pulsanti Salva/Annulla)
        /// Solo per UI, non persistito nel database
        /// </summary>
        [NotMapped]
        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                _isEditing = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNotEditing));
            }
        }

        /// <summary>
        /// Inverso di IsEditing (per binding visibilità pulsanti)
        /// Solo per UI, non persistito nel database
        /// </summary>
        [NotMapped]
        public bool IsNotEditing => !IsEditing;

        /// <summary>
        /// Indica se è un nuovo paziente non ancora salvato
        /// Solo per UI, non persistito nel database
        /// </summary>
        [NotMapped]
        public bool IsNew { get; set; }

        /// <summary>
        /// Valore originale del FirstName (per annullamento modifiche)
        /// Solo per UI, non persistito nel database
        /// </summary>
        [NotMapped]
        public string OriginalFirstName { get; set; }

        /// <summary>
        /// Valore originale del LastName (per annullamento modifiche)
        /// Solo per UI, non persistito nel database
        /// </summary>
        [NotMapped]
        public string OriginalLastName { get; set; }

        #endregion

        /// <summary>
        /// Rappresentazione testuale del paziente (Nome Cognome)
        /// </summary>
        public override string ToString() => $"{FirstName} {LastName}";

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
