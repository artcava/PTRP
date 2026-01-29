using System;
using System.Collections.Generic;
using PTRP.App.Models;

namespace PTRP.App.Models
{
    /// <summary>
    /// Modello per rappresentare un Paziente
    /// </summary>
    public class PatientModel
    {
        /// <summary>
        /// Identificatore univoco del paziente
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Nome del paziente
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Cognome del paziente
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Data di creazione del record
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Data dell'ultimo aggiornamento del record
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Collezione di Progetti Terapeutici associati al paziente
        /// </summary>
        public ICollection<TherapyProjectModel> TherapyProjects { get; set; } = new List<TherapyProjectModel>();

        /// <summary>
        /// Rappresentazione testuale del paziente (Nome Cognome)
        /// </summary>
        public override string ToString() => $"{FirstName} {LastName}";
    }
}
