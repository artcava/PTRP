namespace PTRP.App.Models
{
    /// <summary>
    /// Modello per rappresentare un Paziente nell'applicazione
    /// </summary>
    public class PatientModel
    {
        /// <summary>
        /// Identificatore univoco del paziente
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nome del paziente
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Cognome del paziente
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Data di nascita del paziente
        /// </summary>
        public DateTime DateOfBirth { get; set; }

        /// <summary>
        /// Email di contatto del paziente
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Numero di telefono del paziente
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Data di creazione del record
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Data dell'ultimo aggiornamento
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Proprietà di navigazione per i progetti terapeutici associati
        /// </summary>
        public ICollection<TherapyProjectModel> TherapyProjects { get; set; } = new List<TherapyProjectModel>();

        /// <summary>
        /// Override di ToString per visualizzazione rapida
        /// </summary>
        public override string ToString()
        {
            return $"{FirstName} {LastName} (ID: {Id})";
        }
    }

    /// <summary>
    /// Modello per rappresentare un Progetto Terapeutico (stub per ora)
    /// </summary>
    public class TherapyProjectModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int PatientId { get; set; }
        public PatientModel Patient { get; set; }
    }
}
