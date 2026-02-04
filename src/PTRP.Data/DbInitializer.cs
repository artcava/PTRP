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

        // 1. Crea Educatori Professionali (5-8)
        var educators = CreateEducators();
        context.ProfessionalEducators.AddRange(educators);
        context.SaveChanges();

        // 2. Crea Pazienti (10-15)
        var patients = CreatePatients();
        context.Patients.AddRange(patients);
        context.SaveChanges();

        // 3. Crea Progetti Terapeutici con relazioni (20-25)
        var projects = CreateTherapyProjects(patients, educators);
        context.TherapyProjects.AddRange(projects);
        context.SaveChanges();

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
            .RuleFor(e => e.IsActive, f => f.Random.Bool(0.9f)) // 90% attivi
            .RuleFor(e => e.Specialization, f => f.PickRandom(
                "Disturbi dell'umore",
                "Dipendenze",
                "Disturbi d'ansia",
                "Psicosi",
                "Disabilità cognitiva",
                "Disturbi della personalità"
            ))
            .RuleFor(e => e.Notes, f => f.Lorem.Sentence())
            .RuleFor(e => e.CreatedAt, f => f.Date.PastOffset(2).UtcDateTime)
            .RuleFor(e => e.UpdatedAt, f => f.Date.RecentOffset().UtcDateTime);

        return educatorFaker.Generate(7);
    }

    private static List<PatientModel> CreatePatients()
    {
        var patientFaker = new Faker<PatientModel>("it")
            .RuleFor(p => p.Id, f => Guid.NewGuid())
            .RuleFor(p => p.FirstName, f => f.Name.FirstName())
            .RuleFor(p => p.LastName, f => f.Name.LastName())
            .RuleFor(p => p.DateOfBirth, f => f.Date.PastOffset(65, DateTime.Now.AddYears(-18)).UtcDateTime)
            .RuleFor(p => p.FiscalCode, f => GenerateFakeFiscalCode())
            .RuleFor(p => p.Address, f => f.Address.FullAddress())
            .RuleFor(p => p.PhoneNumber, f => f.Phone.PhoneNumber("+39 ### ### ####"))
            .RuleFor(p => p.EmergencyContact, f => f.Name.FullName())
            .RuleFor(p => p.EmergencyPhone, f => f.Phone.PhoneNumber("+39 ### ### ####"))
            .RuleFor(p => p.Notes, f => f.Lorem.Sentence())
            .RuleFor(p => p.CreatedAt, f => f.Date.PastOffset(3).UtcDateTime)
            .RuleFor(p => p.UpdatedAt, f => f.Date.RecentOffset().UtcDateTime);

        return patientFaker.Generate(12);
    }

    private static List<TherapyProjectModel> CreateTherapyProjects(
        List<PatientModel> patients,
        List<ProfessionalEducatorModel> educators)
    {
        var projects = new List<TherapyProjectModel>();
        var random = new Random(12345);

        foreach (var patient in patients)
        {
            // Ogni paziente ha 1-3 progetti (storico + eventualmente attivo)
            var projectCount = random.Next(1, 4);

            for (int i = 0; i < projectCount; i++)
            {
                var isActive = (i == projectCount - 1) && random.Next(0, 100) < 70; // 70% ha progetto attivo
                var state = isActive
                    ? TherapyProjectState.Active
                    : new Faker().PickRandom(
                        TherapyProjectState.Completed,
                        TherapyProjectState.Suspended,
                        TherapyProjectState.Deceased
                    );

                var startDate = DateTime.UtcNow.AddMonths(-random.Next(3, 24));
                var endDate = isActive ? null : (DateTime?)startDate.AddMonths(random.Next(6, 18));

                var project = new TherapyProjectModel
                {
                    Id = Guid.NewGuid(),
                    PatientId = patient.Id,
                    Title = $"PT {patient.LastName} {startDate.Year}",
                    Description = new Faker("it").Lorem.Paragraph(),
                    State = state,
                    StartDate = startDate,
                    EndDate = endDate,
                    SuspendedAt = state == TherapyProjectState.Suspended ? endDate : null,
                    SuspensionReason = state == TherapyProjectState.Suspended
                        ? new Faker("it").PickRandom(
                            "Ricovero ospedaliero",
                            "Richiesta del paziente",
                            "Trasferimento ad altra struttura"
                        )
                        : null,
                    CompletedAt = state == TherapyProjectState.Completed ? endDate : null,
                    Goals = new Faker("it").Lorem.Sentences(3),
                    CreatedAt = startDate.AddDays(-7),
                    UpdatedAt = DateTime.UtcNow
                };

                // Assegna 1-3 educatori al progetto
                var assignedEducators = educators
                    .OrderBy(x => random.Next())
                    .Take(random.Next(1, 4))
                    .ToList();

                project.ProjectOperators = assignedEducators.Select((e, idx) => new ProjectOperatorModel
                {
                    Id = Guid.NewGuid(),
                    ProjectId = project.Id,
                    OperatorId = e.Id,
                    Role = idx == 0 ? "Coordinator" : "Assistant",
                    AssignedAt = startDate,
                    RemovedAt = null
                }).ToList();

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
        var activeProjects = projects.Where(p => p.State == TherapyProjectState.Active).ToList();

        foreach (var project in activeProjects)
        {
            // 4 appuntamenti canonici per progetto attivo
            var visitTypes = new[] { "Intake", "Intermediate", "Intermediate", "Final" };
            var startDate = project.StartDate;

            for (int i = 0; i < visitTypes.Length; i++)
            {
                var scheduledDate = startDate.AddMonths(i * 3).AddDays(random.Next(-7, 7));
                var isPast = scheduledDate < DateTime.UtcNow;

                var appointment = new ScheduledVisitModel
                {
                    Id = Guid.NewGuid(),
                    ProjectId = project.Id,
                    VisitTypeId = Guid.NewGuid(), // TODO: Link to real VisitTypeModel when available
                    ScheduledDate = scheduledDate,
                    Duration = 60,
                    Location = faker.PickRandom("Sede Centrale", "Comunità Residenziale", "Domicilio", "Teleconferenza"),
                    Status = isPast
                        ? faker.PickRandom(AppointmentStatus.Completed, AppointmentStatus.Missed)
                        : AppointmentStatus.Scheduled,
                    Notes = faker.Lorem.Sentence(),
                    CreatedAt = scheduledDate.AddDays(-14),
                    UpdatedAt = DateTime.UtcNow
                };

                appointments.Add(appointment);
            }

            // Aggiungi 1-2 appuntamenti extra casuali
            var extraCount = random.Next(0, 3);
            for (int i = 0; i < extraCount; i++)
            {
                var extraDate = DateTime.UtcNow.AddDays(random.Next(-30, 60));

                appointments.Add(new ScheduledVisitModel
                {
                    Id = Guid.NewGuid(),
                    ProjectId = project.Id,
                    VisitTypeId = Guid.NewGuid(), // ExtraVisit
                    ScheduledDate = extraDate,
                    Duration = 45,
                    Location = faker.PickRandom("Sede Centrale", "Domicilio"),
                    Status = extraDate < DateTime.UtcNow
                        ? AppointmentStatus.Completed
                        : AppointmentStatus.Scheduled,
                    Notes = "Visita straordinaria",
                    CreatedAt = extraDate.AddDays(-7),
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }

        return appointments;
    }

    /// <summary>
    /// Genera un codice fiscale fake realistico (non valido ma con formato corretto)
    /// </summary>
    private static string GenerateFakeFiscalCode()
    {
        var faker = new Faker();
        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var digits = "0123456789";

        return $"{chars[faker.Random.Int(0, 25)]}{chars[faker.Random.Int(0, 25)]}{chars[faker.Random.Int(0, 25)]}" +
               $"{chars[faker.Random.Int(0, 25)]}{chars[faker.Random.Int(0, 25)]}{chars[faker.Random.Int(0, 25)]}" +
               $"{digits[faker.Random.Int(0, 9)]}{digits[faker.Random.Int(0, 9)]}" +
               $"{chars[faker.Random.Int(0, 25)]}{digits[faker.Random.Int(0, 9)]}{digits[faker.Random.Int(0, 9)]}" +
               $"{chars[faker.Random.Int(0, 25)]}{digits[faker.Random.Int(0, 9)]}{digits[faker.Random.Int(0, 9)]}{digits[faker.Random.Int(0, 9)]}" +
               $"{chars[faker.Random.Int(0, 25)]}";
    }
}
