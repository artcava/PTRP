using System;
using System.ComponentModel.DataAnnotations;

namespace PTRP.Models
{
    /// <summary>
    /// Entità di join Many-to-Many tra ActualVisit e ProfessionalEducator.
    /// Rappresenta la relazione "operatori presenti" per una visita effettiva.
    /// Permette di tracciare chi ha registrato la visita vs chi era semplicemente presente.
    /// </summary>
    public class VisitOperatorModel
    {
        /// <summary>
        /// Chiave esterna: ID della visita effettiva
        /// </summary>
        [Required]
        public Guid ActualVisitId { get; set; }

        /// <summary>
        /// Navigazione verso la visita effettiva
        /// </summary>
        public ActualVisitModel? ActualVisit { get; set; }

        /// <summary>
        /// Chiave esterna: ID dell'educatore professionale
        /// </summary>
        [Required]
        public Guid EducatorId { get; set; }

        /// <summary>
        /// Navigazione verso l'educatore professionale
        /// </summary>
        public ProfessionalEducatorModel? Educator { get; set; }

        /// <summary>
        /// Flag per distinguere chi ha registrato la visita (true) vs chi era solo presente (false).
        /// Esattamente UN operatore per visita deve avere IsRegistrant = true.
        /// </summary>
        public bool IsRegistrant { get; set; } = false;

        /// <summary>
        /// Data in cui l'operatore è stato associato alla visita
        /// </summary>
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    }
}
