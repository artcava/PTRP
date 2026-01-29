using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PTRP.Models
{
    /// <summary>
    /// Modello per rappresentare un Progetto Terapeutico
    /// È associato a un Paziente e può avere molteplici Educatori Professionali
    /// </summary>
    public class TherapyProjectModel : IValidatableObject
    {
        /// <summary>
        /// Identificatore univoco del progetto terapeutico
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// ID del paziente a cui è associato il progetto
        /// </summary>
        public Guid PatientId { get; set; }

        /// <summary>
        /// Titolo/Nome del progetto terapeutico
        /// </summary>
        [Required(ErrorMessage = "Il titolo del progetto è obbligatorio")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Il titolo deve contenere tra 3 e 200 caratteri")]
        public string Title { get; set; }

        /// <summary>
        /// Descrizione dettagliata del progetto terapeutico
        /// </summary>
        [StringLength(2000, ErrorMessage = "La descrizione non può superare 2000 caratteri")]
        public string Description { get; set; }

        /// <summary>
        /// Data di inizio del progetto terapeutico
        /// </summary>
        [Required(ErrorMessage = "La data di inizio è obbligatoria")]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Data di fine prevista del progetto terapeutico
        /// Validazione: non può essere prima di StartDate
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Stato del progetto terapeutico (es. "In Progress", "Completed", "Suspended")
        /// </summary>
        [Required(ErrorMessage = "Lo stato del progetto è obbligatorio")]
        [StringLength(50)]
        public string Status { get; set; } = "In Progress";

        /// <summary>
        /// Data di creazione del record
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Data dell'ultimo aggiornamento del record
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Navigazione al paziente associato (relazione inversa)
        /// </summary>
        public PatientModel Patient { get; set; }

        /// <summary>
        /// Collezione di Educatori Professionali assegnati al progetto
        /// </summary>
        public ICollection<ProfessionalEducatorModel> ProfessionalEducators { get; set; } = new List<ProfessionalEducatorModel>();

        /// <summary>
        /// Rappresentazione testuale del progetto terapeutico
        /// </summary>
        public override string ToString() => $"{Title} (Paziente ID: {PatientId:D})";

        /// <summary>
        /// Validazione custom per le regole di business
        /// </summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Validazione: EndDate non può essere nel passato
            if (EndDate.HasValue && EndDate.Value < DateTime.Now)
            {
                yield return new ValidationResult(
                    "La data di fine non può essere nel passato",
                    new[] { nameof(EndDate) });
            }

            // Validazione: EndDate non può essere prima di StartDate
            if (EndDate.HasValue && EndDate.Value < StartDate)
            {
                yield return new ValidationResult(
                    "La data di fine non può essere prima della data di inizio",
                    new[] { nameof(EndDate) });
            }

            // Validazione: StartDate non può essere eccessivamente nel futuro
            if (StartDate > DateTime.Now.AddHours(1))
            {
                yield return new ValidationResult(
                    "La data di inizio non può essere eccessivamente nel futuro (massimo 1 ora da ora)",
                    new[] { nameof(StartDate) });
            }
        }
    }
}
