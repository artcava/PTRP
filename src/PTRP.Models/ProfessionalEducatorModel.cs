using System;
using System.Collections.Generic;

namespace PTRP.Models
{
    /// <summary>
    /// Modello per rappresentare un Educatore Professionale
    /// Può essere assegnato a molteplici Progetti Terapeutici
    /// </summary>
    public class ProfessionalEducatorModel
    {
        /// <summary>
        /// Identificatore univoco dell'educatore
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Nome dell'educatore
        /// </summary>
        public string? FirstName { get; set; }

        /// <summary>
        /// Cognome dell'educatore
        /// </summary>
        public string? LastName { get; set; }

        /// <summary>
        /// Email di contatto
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Numero di telefono
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Data di nascita
        /// </summary>
        public DateTime DateOfBirth { get; set; }

        /// <summary>
        /// Specializzazione/titolo professionale
        /// (es. "Psicologo", "Fisioterapista", "Logopedista")
        /// </summary>
        public string? Specialization { get; set; }

        /// <summary>
        /// Numero di licenza/albo professionale
        /// </summary>
        public string? LicenseNumber { get; set; }

        /// <summary>
        /// Data di inizio dell'impiego/collaborazione
        /// </summary>
        public DateTime HireDate { get; set; }

        /// <summary>
        /// Stato dell'educatore (es. "Active", "Inactive", "OnLeave")
        /// </summary>
        public string Status { get; set; } = "Active";

        /// <summary>
        /// Ruolo operativo ("Coordinatore" o "Educatore")
        /// Determina i permessi e le funzionalità disponibili
        /// </summary>
        public string Role { get; set; } = "Educatore";

        /// <summary>
        /// Flag per identificare il profilo dell'utente locale
        /// Solo un educatore per istanza può avere questo flag a true
        /// Usato per first-run detection e caricamento profilo
        /// </summary>
        public bool IsCurrentUser { get; set; } = false;

        /// <summary>
        /// Data di creazione del record
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Data dell'ultimo aggiornamento del record
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Collezione di Progetti Terapeutici a cui è assegnato
        /// </summary>
        public ICollection<TherapyProjectModel> AssignedTherapyProjects { get; set; } = new List<TherapyProjectModel>();

        /// <summary>
        /// Rappresentazione testuale dell'educatore
        /// </summary>
        public override string ToString() => $"{FirstName} {LastName} ({Specialization})";
    }
}
