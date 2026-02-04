using Bogus;
using PTRP.Models;
using PTRP.Models.Enums;

namespace PTRP.Data;

/// <summary>
/// Inizializza il database con dati di esempio per l'ambiente di sviluppo.
/// Issue #13: Data seeding per testing e demo.
/// </summary>
public static class DbInitializer
{
    /// <summary>
    /// Popola il database con dati di esempio se vuoto.
    /// Idempotente: non duplica dati se già presenti.
    /// </summary>
    public static void Initialize(PTRPDbContext context)
    {
        // Se ci sono già pazienti, assume che il DB sia già popolato
        if (context.Patients.Any())
        {
            return; // DB già seeded
        }

        // Configura Bogus per italiano
        Randomizer.Seed = new Random(12345); // Seed fisso per dati riproducibili

        // 1. Crea Educatori Professionali (7)
        var educators = CreateEducators();
        context.ProfessionalEducators.AddRange(educators);
        context.SaveChanges(); // IMPORTANTE: Salva prima di creare relazioni

        // 2. Crea Pazienti (12)
        var patients = CreatePatients();
        context.Patients.AddRange(patients);
        context.SaveChanges(); // IMPORTANTE: Salva prima di creare relazioni

        // 3. Crea Progetti Terapeutici con relazioni (20-25)
        var projects = CreateTherapyProjects(patients, educators);
        context.TherapyProjects.AddRange(projects);
        context.SaveChanges(); // EF gestisce automaticamente la join table

        // 4. Crea Appuntamenti per progetti attivi
        var appointments = CreateScheduledVisits(projects);
        context.ScheduledVisits.AddRange(appointments);
        context.SaveChanges();
    }

    private static List<ProfessionalEducatorModel> CreateEducators()
    {
        var educatorFaker = new Faker<ProfessionalEducatorModel>("it")
            .RuleFor(e => e.Id, f => Guid.NewGuid())
            .RuleFor(e => e.FirstName, f => f.Name.FirstName())
            .RuleFor(e => e.LastName, f => f.Name.LastName())
            .RuleFor(e => e.Email, (f, e) => f.Internet.Email(e.FirstName, e.LastName))
            .RuleFor(e => e.PhoneNumber, f => f.Phone.PhoneNumber("+39 ### ### ####"))
            .RuleFor(e => e.DateOfBirth, f => f.Date.Past(40, DateTime.Now.AddYears(-25)))
            .RuleFor(e => e.Specialization, f => f.PickRandom(
                "Disturbi dell'umore",
                "Dipendenze",
                "Disturbi d'ansia",
                "Psicosi",
                "Disabilità cognitiva",
                "Disturbi della personalità"
            ))
            .RuleFor(e => e.LicenseNumber, f => f.Random.Replace("EP-####-##"))
            .RuleFor(e => e.HireDate, f => f.Date.Past(5))
            .RuleFor(e => e.Status, f => f.Random.Bool(0.9f) ? "Active" : "Inactive")
            .RuleFor(e => e.Role, f => f.PickRandom("Coordinatore", "Educatore", "Educatore")) // 1/3 coordinatore
            .RuleFor(e => e.IsCurrentUser, f => false)
            .RuleFor(e => e.CreatedAt, f => f.Date.Past(2))
            .RuleFor(e => e.UpdatedAt, f => f.Date.Recent());

        var educators = educatorFaker.Generate(7);
        // Imposta il primo come utente corrente
        educators[0].IsCurrentUser = true;
        educators[0].Role = "Coordinatore";
        
        return educators;
    }

    private static List<PatientModel> CreatePatients()
    {
        var patientFaker = new Faker<PatientModel>("it")
            .RuleFor(p => p.Id, f => Guid.NewGuid())
            .RuleFor(p => p.FirstName, f => f.Name.FirstName())
            .RuleFor(p => p.LastName, f => f.Name.LastName())
            .RuleFor(p => p.CreatedAt, f => f.Date.Past(3))
            .RuleFor(p => p.UpdatedAt, f => f.Date.Recent());

        return patientFaker.Generate(12);
    }

    private static List<TherapyProjectModel> CreateTherapyProjects(
        List<PatientModel> patients,
        List<ProfessionalEducatorModel> educators)
    {
        var projects = new List<TherapyProjectModel>();
        var random = new Random(12345);
        var faker = new Faker("it");

        foreach (var patient in patients)
        {
            // Ogni paziente ha 1-3 progetti (storico + eventualmente attivo)
            var projectCount = random.Next(1, 4);

            for (int i = 0; i < projectCount; i++)
            {
                var isActive = (i == projectCount - 1) && random.Next(0, 100) < 70; // 70% ha progetto attivo
                var status = isActive
                    ? "In Progress"
                    : faker.PickRandom("Completed", "On Hold", "Cancelled");

                var startDate = DateTime.UtcNow.AddMonths(-random.Next(3, 24));
                var endDate = isActive ? null : (DateTime?)startDate.AddMonths(random.Next(6, 18));

                var project = new TherapyProjectModel
                {
                    Id = Guid.NewGuid(),
                    PatientId = patient.Id,
                    Title = $"PT {patient.LastName} {startDate.Year}",
                    Description = faker.Lorem.Paragraph(),
                    Status = status,
                    StartDate = startDate,
                    EndDate = endDate,
                    CreatedAt = startDate.AddDays(-7),
                    UpdatedAt = DateTime.UtcNow
                };

                // Assegna 1-3 educatori al progetto
                // EF gestisce automaticamente la join table TherapyProjectEducator
                var assignedEducators = educators
                    .OrderBy(x => random.Next())
                    .Take(random.Next(1, 4))
                    .ToList();

                foreach (var educator in assignedEducators)
                {
                    project.ProfessionalEducators.Add(educator);
                }

                projects.Add(project);
            }
        }

        return projects;
    }

    private static List<ScheduledVisitModel> CreateScheduledVisits(
        List<TherapyProjectModel> projects)
    {
        var appointments = new List<ScheduledVisitModel>();
        var random = new Random(12345);
        var faker = new Faker("it");

        // Crea appuntamenti solo per progetti attivi
        var activeProjects = projects.Where(p => p.Status == "In Progress").ToList();

        foreach (var project in activeProjects)
        {
            // 4 appuntamenti canonici per progetto attivo
            // IMPORTANTE: Enum values sono UPPERCASE_SNAKE_CASE
            var visitTypes = new[] 
            { 
                VisitType.INTAKE, 
                VisitType.INTERMEDIATE, 
                VisitType.INTERMEDIATE, 
                VisitType.FINAL 
            };
            
            var startDate = project.StartDate;

            for (int i = 0; i < visitTypes.Length; i++)
            {
                var scheduledDate = startDate.AddMonths(i * 3).AddDays(random.Next(-7, 7));
                var isPast = scheduledDate < DateTime.UtcNow;

                var appointment = new ScheduledVisitModel
                {
                    Id = Guid.NewGuid(),
                    TherapyProjectId = project.Id,
                    Type = visitTypes[i],
                    ScheduledDate = scheduledDate,
                    Status = isPast
                        ? faker.PickRandom(AppointmentStatus.Completed, AppointmentStatus.Missed)
                        : AppointmentStatus.Scheduled,
                    Notes = faker.Lorem.Sentence(),
                    CreatedAt = scheduledDate.AddDays(-14),
                    UpdatedAt = DateTime.UtcNow,
                    Version = 1
                };

                appointments.Add(appointment);
            }

            // Aggiungi 0-2 appuntamenti extra casuali
            var extraCount = random.Next(0, 3);
            for (int i = 0; i < extraCount; i++)
            {
                var extraDate = DateTime.UtcNow.AddDays(random.Next(-30, 60));

                appointments.Add(new ScheduledVisitModel
                {
                    Id = Guid.NewGuid(),
                    TherapyProjectId = project.Id,
                    Type = VisitType.FOLLOW_UP, // UPPERCASE_SNAKE_CASE
                    ScheduledDate = extraDate,
                    Status = extraDate < DateTime.UtcNow
                        ? AppointmentStatus.Completed
                        : AppointmentStatus.Scheduled,
                    Notes = "Visita straordinaria",
                    CreatedAt = extraDate.AddDays(-7),
                    UpdatedAt = DateTime.UtcNow,
                    Version = 1
                });
            }
        }

        return appointments;
    }
}
