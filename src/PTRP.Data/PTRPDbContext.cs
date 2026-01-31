using Microsoft.EntityFrameworkCore;
using PTRP.Models;

namespace PTRP.Data;

/// <summary>
/// Database context principale per l'applicazione PTRP
/// Gestisce le entità: Patient, TherapyProject, ProfessionalEducator
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
    }
}
