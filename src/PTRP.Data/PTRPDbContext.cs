using Microsoft.EntityFrameworkCore;
using PTRP.Models;
using PTRP.Models.Enums;

namespace PTRP.Data;

/// <summary>
/// Database context principale per l'applicazione PTRP
/// Gestisce le entità: Patient, TherapyProject, ProfessionalEducator, ScheduledVisit, ActualVisit
/// </summary>
public class PTRPDbContext : DbContext
{
    public PTRPDbContext(DbContextOptions<PTRPDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// DbSet per i Pazienti
    /// </summary>
    public DbSet<PatientModel> Patients { get; set; }

    /// <summary>
    /// DbSet per i Progetti Terapeutici
    /// </summary>
    public DbSet<TherapyProjectModel> TherapyProjects { get; set; }

    /// <summary>
    /// DbSet per gli Educatori Professionali
    /// </summary>
    public DbSet<ProfessionalEducatorModel> ProfessionalEducators { get; set; }

    /// <summary>
    /// DbSet per gli Appuntamenti Programmati (Visite Schedulate)
    /// </summary>
    public DbSet<ScheduledVisitModel> ScheduledVisits { get; set; }

    /// <summary>
    /// DbSet per le Visite Effettive Registrate
    /// </summary>
    public DbSet<ActualVisitModel> ActualVisits { get; set; }

    /// <summary>
    /// DbSet per la relazione Many-to-Many tra ActualVisit e ProfessionalEducator
    /// </summary>
    public DbSet<VisitOperatorModel> VisitOperators { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurazione PatientModel
        modelBuilder.Entity<PatientModel>(entity =>
        {
            entity.ToTable("Patients");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.Property(e => e.UpdatedAt);

            // Relazione 1-N con TherapyProject
            entity.HasMany(p => p.TherapyProjects)
                .WithOne(tp => tp.Patient)
                .HasForeignKey(tp => tp.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indice per ricerca per nome
            entity.HasIndex(e => new { e.FirstName, e.LastName })
                .HasDatabaseName("IX_Patients_FullName");
        });

        // Configurazione TherapyProjectModel
        modelBuilder.Entity<TherapyProjectModel>(entity =>
        {
            entity.ToTable("TherapyProjects");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.PatientId)
                .IsRequired();

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Description)
                .HasMaxLength(2000);

            entity.Property(e => e.StartDate)
                .IsRequired();

            entity.Property(e => e.EndDate);

            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("In Progress");

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.Property(e => e.UpdatedAt);

            // Relazione N-1 con Patient (già configurata sopra)

            // Relazione 1-N con ScheduledVisit
            entity.HasMany(tp => tp.ScheduledVisits)
                .WithOne(sv => sv.TherapyProject)
                .HasForeignKey(sv => sv.TherapyProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relazione N-N con ProfessionalEducator
            entity.HasMany(tp => tp.ProfessionalEducators)
                .WithMany(pe => pe.AssignedTherapyProjects)
                .UsingEntity<Dictionary<string, object>>(
                    "TherapyProjectEducator",
                    j => j.HasOne<ProfessionalEducatorModel>()
                        .WithMany()
                        .HasForeignKey("ProfessionalEducatorId")
                        .OnDelete(DeleteBehavior.Cascade),
                    j => j.HasOne<TherapyProjectModel>()
                        .WithMany()
                        .HasForeignKey("TherapyProjectId")
                        .OnDelete(DeleteBehavior.Cascade));

            // Indice per ricerca per paziente
            entity.HasIndex(e => e.PatientId)
                .HasDatabaseName("IX_TherapyProjects_PatientId");

            // Indice per ricerca per status
            entity.HasIndex(e => e.Status)
                .HasDatabaseName("IX_TherapyProjects_Status");
        });

        // Configurazione ProfessionalEducatorModel
        modelBuilder.Entity<ProfessionalEducatorModel>(entity =>
        {
            entity.ToTable("ProfessionalEducators");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.PhoneNumber)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(e => e.DateOfBirth)
                .IsRequired();

            entity.Property(e => e.Specialization)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.LicenseNumber)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.HireDate)
                .IsRequired();

            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("Active");

            // Issue #49: First-run configuration support
            entity.Property(e => e.Role)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("Educatore");

            entity.Property(e => e.IsCurrentUser)
                .IsRequired()
                .HasDefaultValue(false);

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.Property(e => e.UpdatedAt);

            // Relazione N-N con TherapyProject (già configurata sopra)

            // Indice per ricerca per email (univoca)
            entity.HasIndex(e => e.Email)
                .IsUnique()
                .HasDatabaseName("IX_ProfessionalEducators_Email");

            // Indice per ricerca per specializzazione
            entity.HasIndex(e => e.Specialization)
                .HasDatabaseName("IX_ProfessionalEducators_Specialization");

            // Indice per ricerca per status
            entity.HasIndex(e => e.Status)
                .HasDatabaseName("IX_ProfessionalEducators_Status");

            // Issue #49: Indice per first-run detection e caricamento profilo locale
            entity.HasIndex(e => e.IsCurrentUser)
                .HasDatabaseName("IX_ProfessionalEducators_IsCurrentUser");

            // Issue #49: Indice per ricerca per ruolo
            entity.HasIndex(e => e.Role)
                .HasDatabaseName("IX_ProfessionalEducators_Role");
        });

        // Configurazione ScheduledVisitModel
        modelBuilder.Entity<ScheduledVisitModel>(entity =>
        {
            entity.ToTable("ScheduledVisits");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.TherapyProjectId)
                .IsRequired();

            entity.Property(e => e.Type)
                .IsRequired()
                .HasConversion<string>(); // Store enum as string

            // IMPORTANTE: HasDefaultValue deve usare valore ENUM, non stringa
            // EF Core fa la conversione automaticamente quando salva nel DB
            entity.Property(e => e.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasDefaultValue(AppointmentStatus.Scheduled); // Valore ENUM, non stringa

            entity.Property(e => e.ScheduledDate)
                .IsRequired();

            entity.Property(e => e.RescheduledDate);

            entity.Property(e => e.Notes)
                .HasMaxLength(1000);

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.Property(e => e.UpdatedAt);

            entity.Property(e => e.Version)
                .IsRequired()
                .HasDefaultValue(1);

            // Relazione N-1 con TherapyProject (già configurata sopra)

            // Relazione 1-1 con ActualVisit (opzionale)
            entity.HasOne(sv => sv.ActualVisit)
                .WithOne(av => av.ScheduledVisit)
                .HasForeignKey<ActualVisitModel>(av => av.ScheduledVisitId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete of actual visit when scheduled visit deleted

            // Indici per query performance
            entity.HasIndex(e => e.TherapyProjectId)
                .HasDatabaseName("IX_ScheduledVisits_TherapyProjectId");

            entity.HasIndex(e => e.ScheduledDate)
                .HasDatabaseName("IX_ScheduledVisits_ScheduledDate");

            entity.HasIndex(e => e.Status)
                .HasDatabaseName("IX_ScheduledVisits_Status");

            entity.HasIndex(e => e.Type)
                .HasDatabaseName("IX_ScheduledVisits_Type");
        });

        // Configurazione ActualVisitModel
        modelBuilder.Entity<ActualVisitModel>(entity =>
        {
            entity.ToTable("ActualVisits");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.ScheduledVisitId)
                .IsRequired();

            entity.Property(e => e.ActualDate)
                .IsRequired();

            entity.Property(e => e.StartTime)
                .IsRequired();

            entity.Property(e => e.EndTime)
                .IsRequired();

            entity.Property(e => e.PatientPresence)
                .IsRequired()
                .HasConversion<string>();

            entity.Property(e => e.ClinicalNotes)
                .IsRequired()
                .HasMaxLength(5000);

            entity.Property(e => e.Outcomes)
                .HasMaxLength(2000);

            // IMPORTANTE: HasDefaultValue deve usare valore ENUM, non stringa
            entity.Property(e => e.Source)
                .IsRequired()
                .HasConversion<string>()
                .HasDefaultValue(VisitSource.EducatorImport); // Valore ENUM, non stringa

            entity.Property(e => e.RegisteredBy)
                .IsRequired();

            entity.Property(e => e.RegisteredByName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.Property(e => e.UpdatedAt);

            entity.Property(e => e.Version)
                .IsRequired()
                .HasDefaultValue(1);

            // Relazione 1-1 con ScheduledVisit (già configurata sopra)

            // Indici per query performance
            entity.HasIndex(e => e.ScheduledVisitId)
                .IsUnique() // 1-1 relationship
                .HasDatabaseName("IX_ActualVisits_ScheduledVisitId");

            entity.HasIndex(e => e.ActualDate)
                .HasDatabaseName("IX_ActualVisits_ActualDate");

            entity.HasIndex(e => e.RegisteredBy)
                .HasDatabaseName("IX_ActualVisits_RegisteredBy");

            entity.HasIndex(e => e.Source)
                .HasDatabaseName("IX_ActualVisits_Source");
        });

        // Configurazione VisitOperatorModel (Join Table Many-to-Many)
        modelBuilder.Entity<VisitOperatorModel>(entity =>
        {
            entity.ToTable("VisitOperators");
            
            // CHIAVE PRIMARIA COMPOSITA: ActualVisitId + EducatorId
            entity.HasKey(vo => new { vo.ActualVisitId, vo.EducatorId });

            entity.Property(vo => vo.ActualVisitId)
                .IsRequired();

            entity.Property(vo => vo.EducatorId)
                .IsRequired();

            entity.Property(vo => vo.IsRegistrant)
                .IsRequired()
                .HasDefaultValue(false);

            entity.Property(vo => vo.AssignedAt)
                .IsRequired();

            // Relazione N-1 con ActualVisit
            entity.HasOne(vo => vo.ActualVisit)
                .WithMany(av => av.OperatorsPresent)
                .HasForeignKey(vo => vo.ActualVisitId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relazione N-1 con ProfessionalEducator
            entity.HasOne(vo => vo.Educator)
                .WithMany() // ProfessionalEducator non ha navigation property verso VisitOperator
                .HasForeignKey(vo => vo.EducatorId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete if educator is deleted

            // Indice per query "tutte le visite di un educatore"
            entity.HasIndex(vo => vo.EducatorId)
                .HasDatabaseName("IX_VisitOperators_EducatorId");

            // Indice per query "chi ha registrato questa visita"
            entity.HasIndex(vo => vo.IsRegistrant)
                .HasDatabaseName("IX_VisitOperators_IsRegistrant");
        });
    }
}
