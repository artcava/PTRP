using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PTRP.Models
{
    /// <summary>
    /// Modello per rappresentare un Progetto Terapeutico
    /// Un progetto terapeutico è associato a un paziente e può coinvolgere più educatori professionali
    /// </summary>
    public class TherapyProjectModel : IValidatableObject
    {
        /// <summary>
        /// Identificatore univoco del progetto
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Chiave esterna: ID del paziente associato
        /// </summary>
        public Guid PatientId { get; set; }

        /// <summary>
        /// Titolo del progetto terapeutico
        /// </summary>
        [Required(ErrorMessage = "Il titolo del progetto è obbligatorio")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Il titolo deve avere tra 3 e 200 caratteri")]
        public string? Title { get; set; }

        /// <summary>
        /// Descrizione dettagliata del progetto
        /// </summary>
        [StringLength(2000, ErrorMessage = "La descrizione non può superare i 2000 caratteri")]
        public string? Description { get; set; }

        /// <summary>
        /// Data di inizio del progetto
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Data di fine prevista del progetto
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Status del progetto (es. "In Progress", "Completed", "On Hold")
        /// </summary>
        [Required]
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
        /// Paziente associato al progetto (navigazione)
        /// </summary>
        public PatientModel? Patient { get; set; }

        /// <summary>
        /// Educatori professionali assegnati al progetto
        /// </summary>
        public ICollection<ProfessionalEducatorModel> ProfessionalEducators { get; set; } = new List<ProfessionalEducatorModel>();

        /// <summary>
        /// Validazione custom del modello
        /// Verifica che EndDate non sia prima di StartDate e non nel passato
        /// </summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var errors = new List<ValidationResult>();

            // EndDate non può essere prima di StartDate
            if (EndDate.HasValue && EndDate < StartDate)
            {
                errors.Add(new ValidationResult(
                    "La data di fine non può essere prima della data di inizio",
                    new[] { nameof(EndDate) }));
            }

            // EndDate non può essere nel passato (rispetto a StartDate)
            if (EndDate.HasValue && EndDate < DateTime.Now)
            {
                errors.Add(new ValidationResult(
                    "La data di fine non può essere nel passato",
                    new[] { nameof(EndDate) }));
            }

            // StartDate non può essere eccessivamente nel futuro (max 1 anno)
            if (StartDate > DateTime.Now.AddYears(1))
            {
                errors.Add(new ValidationResult(
                    "La data di inizio non può essere più di un anno nel futuro",
                    new[] { nameof(StartDate) }));
            }

            return errors;
        }

        /// <summary>
        /// Rappresentazione testuale del progetto
        /// </summary>
        public override string ToString() => $"{Title} (Paziente: {PatientId}, Status: {Status})";
    }
}
