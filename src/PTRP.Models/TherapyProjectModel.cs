using System;
using System.Collections.Generic;

namespace PTRP.App.Models
{
    /// <summary>
    /// Modello per rappresentare un Progetto Terapeutico
    /// È associato a un Paziente e può avere molteplici Educatori Professionali
    /// </summary>
    public class TherapyProjectModel
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
        public string Title { get; set; }

        /// <summary>
        /// Descrizione dettagliata del progetto terapeutico
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Data di inizio del progetto terapeutico
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Data di fine prevista del progetto terapeutico
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Stato del progetto terapeutico (es. "In Progress", "Completed", "Suspended")
        /// </summary>
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
    }
}
