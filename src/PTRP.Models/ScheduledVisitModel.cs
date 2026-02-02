using System;
using System.ComponentModel.DataAnnotations;
using PTRP.Models.Enums;

namespace PTRP.Models
{
    /// <summary>
    /// Modello per rappresentare un appuntamento programmato (visita schedulata).
    /// Un appuntamento è sempre legato a un progetto terapeutico e può essere
    /// uno dei 4 appuntamenti canonici (INTAKE, INTERMEDIATE, FINAL, DISCHARGE)
    /// o un appuntamento aggiuntivo.
    /// </summary>
    public class ScheduledVisitModel
    {
        /// <summary>
        /// Identificatore univoco dell'appuntamento
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Chiave esterna: ID del progetto terapeutico associato
        /// </summary>
        [Required]
        public Guid TherapyProjectId { get; set; }

        /// <summary>
        /// Navigazione verso il progetto terapeutico
        /// </summary>
        public TherapyProjectModel? TherapyProject { get; set; }

        /// <summary>
        /// Tipo di visita (INTAKE, INTERMEDIATE, FINAL, DISCHARGE, etc.)
        /// </summary>
        [Required]
        public VisitType Type { get; set; }

        /// <summary>
        /// Stato dell'appuntamento (Scheduled, Completed, Missed, Rescheduled, Cancelled)
        /// </summary>
        [Required]
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;

        /// <summary>
        /// Data e ora programmata per l'appuntamento
        /// </summary>
        [Required]
        public DateTime ScheduledDate { get; set; }

        /// <summary>
        /// Data e ora riprogrammata (se Status = Rescheduled)
        /// </summary>
        public DateTime? RescheduledDate { get; set; }

        /// <summary>
        /// Note relative all'appuntamento (es. "richiedere documenti", "preparare materiale educativo")
        /// </summary>
        [StringLength(1000)]
        public string? Notes { get; set; }

        /// <summary>
        /// Relazione 1:1 con la visita effettiva (ActualVisit).
        /// Null se l'appuntamento non è ancora stato svolto.
        /// </summary>
        public ActualVisitModel? ActualVisit { get; set; }

        // Campi di audit

        /// <summary>
        /// Data di creazione del record
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Data dell'ultimo aggiornamento
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// ID dell'operatore che ha creato l'appuntamento
        /// </summary>
        public Guid? CreatedBy { get; set; }

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
        /// Rappresentazione testuale dell'appuntamento
        /// </summary>
        public override string ToString() =>
            $"{Type} - {ScheduledDate:dd/MM/yyyy HH:mm} [{Status}]";
    }
}
