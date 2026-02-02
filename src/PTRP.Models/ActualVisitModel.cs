using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using PTRP.Models.Enums;

namespace PTRP.Models
{
    /// <summary>
    /// Modello per rappresentare una visita effettiva registrata.
    /// VINCOLO CRITICO: Relazione 1:1 OBBLIGATORIA con ScheduledVisit.
    /// Una visita effettiva può esistere SOLO se esiste un appuntamento programmato.
    /// </summary>
    public class ActualVisitModel
    {
        /// <summary>
        /// Identificatore univoco della visita effettiva
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Chiave esterna OBBLIGATORIA: ID dell'appuntamento programmato
        /// Relazione 1:1 con ScheduledVisit
        /// </summary>
        [Required]
        public Guid ScheduledVisitId { get; set; }

        /// <summary>
        /// Navigazione verso l'appuntamento programmato
        /// </summary>
        [Required]
        public ScheduledVisitModel? ScheduledVisit { get; set; }

        // Data e Ora Effettiva

        /// <summary>
        /// Data effettiva della visita (NON può essere futura)
        /// </summary>
        [Required]
        public DateTime ActualDate { get; set; }

        /// <summary>
        /// Ora di inizio della visita
        /// </summary>
        [Required]
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// Ora di fine della visita (deve essere > StartTime)
        /// </summary>
        [Required]
        public TimeSpan EndTime { get; set; }

        // Presenza Paziente

        /// <summary>
        /// Stato di presenza del paziente durante la visita
        /// </summary>
        [Required]
        public PresenceStatus PatientPresence { get; set; }

        // Contenuto Clinico (OBBLIGATORI)

        /// <summary>
        /// Note cliniche della visita (campo OBBLIGATORIO)
        /// Contiene osservazioni, interventi, valutazioni cliniche
        /// </summary>
        [Required(ErrorMessage = "Le note cliniche sono obbligatorie")]
        [MinLength(10, ErrorMessage = "Le note cliniche devono contenere almeno 10 caratteri")]
        [MaxLength(5000, ErrorMessage = "Le note cliniche non possono superare i 5000 caratteri")]
        public string ClinicalNotes { get; set; } = string.Empty;

        /// <summary>
        /// Esiti e obiettivi raggiunti (opzionale ma consigliato)
        /// </summary>
        [MaxLength(2000)]
        public string? Outcomes { get; set; }

        // Operatori Presenti (Many-to-Many)

        /// <summary>
        /// Relazione Many-to-Many con educatori professionali presenti alla visita.
        /// Usata join entity esplicita (VisitOperatorModel) per tracciare chi ha registrato vs chi era presente.
        /// </summary>
        public ICollection<VisitOperatorModel> OperatorsPresent { get; set; } =
            new List<VisitOperatorModel>();

        // Tracciabilità Fonte

        /// <summary>
        /// Fonte della registrazione (EducatorImport o CoordinatorDirect)
        /// </summary>
        [Required]
        public VisitSource Source { get; set; } = VisitSource.EducatorImport;

        // Campi di Audit

        /// <summary>
        /// Data di creazione del record
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Data dell'ultimo aggiornamento
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// ID dell'operatore che ha registrato la visita (tipicamente l'educatore)
        /// </summary>
        [Required]
        public Guid RegisteredBy { get; set; }

        /// <summary>
        /// Nome completo dell'educatore che ha registrato (per display UI)
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string RegisteredByName { get; set; } = string.Empty;

        /// <summary>
        /// ID dell'operatore che ha effettuato l'ultimo aggiornamento
        /// </summary>
        public Guid? UpdatedBy { get; set; }

        /// <summary>
        /// Versione del record (per gestione conflitti in sincronizzazione)
        /// </summary>
        public int Version { get; set; } = 1;

        /// <summary>
        /// ID del pacchetto di sincronizzazione che ha creato/modificato questo record (opzionale)
        /// </summary>
        public Guid? SyncPacketId { get; set; }

        /// <summary>
        /// Rappresentazione testuale della visita
        /// </summary>
        public override string ToString() =>
            $"Visita del {ActualDate:dd/MM/yyyy} - {StartTime:hh\\:mm}-{EndTime:hh\\:mm} (registrata da {RegisteredByName})";
    }
}
