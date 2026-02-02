using PTRP.Models;
using PTRP.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace PTRP.Tests.Models;

public class ActualVisitModelTests
{
    [Fact]
    public void Constructor_SetsPropertiesCorrectly()
    {
        // Arrange & Act
        var scheduledVisitId = Guid.NewGuid();
        var registrantId = Guid.NewGuid();
        var actualDate = DateTime.UtcNow.Date;
        var visit = new ActualVisitModel
        {
            Id = Guid.NewGuid(),
            ScheduledVisitId = scheduledVisitId,
            ActualDate = actualDate,
            StartTime = new TimeSpan(10, 0, 0),
            EndTime = new TimeSpan(11, 30, 0),
            PatientPresence = PresenceStatus.PresentCollaborative,
            ClinicalNotes = "Paziente collaborativo, obiettivi raggiunti secondo piano.",
            Outcomes = "Progressi evidenti nelle attività sociali",
            Source = VisitSource.EducatorImport,
            RegisteredBy = registrantId,
            RegisteredByName = "Bianchi Marco"
        };

        // Assert
        Assert.NotEqual(Guid.Empty, visit.Id);
        Assert.Equal(scheduledVisitId, visit.ScheduledVisitId);
        Assert.Equal(actualDate, visit.ActualDate);
        Assert.Equal(new TimeSpan(10, 0, 0), visit.StartTime);
        Assert.Equal(new TimeSpan(11, 30, 0), visit.EndTime);
        Assert.Equal(PresenceStatus.PresentCollaborative, visit.PatientPresence);
        Assert.Equal("Paziente collaborativo, obiettivi raggiunti secondo piano.", visit.ClinicalNotes);
        Assert.Equal("Progressi evidenti nelle attività sociali", visit.Outcomes);
        Assert.Equal(VisitSource.EducatorImport, visit.Source);
        Assert.Equal(registrantId, visit.RegisteredBy);
        Assert.Equal("Bianchi Marco", visit.RegisteredByName);
    }

    [Fact]
    public void Source_HasDefaultValue_EducatorImport()
    {
        // Arrange & Act
        var visit = new ActualVisitModel
        {
            ScheduledVisitId = Guid.NewGuid(),
            ActualDate = DateTime.UtcNow,
            StartTime = TimeSpan.FromHours(10),
            EndTime = TimeSpan.FromHours(11),
            PatientPresence = PresenceStatus.PresentCollaborative,
            ClinicalNotes = "Note cliniche obbligatorie",
            RegisteredBy = Guid.NewGuid(),
            RegisteredByName = "Test User"
        };

        // Assert
        Assert.Equal(VisitSource.EducatorImport, visit.Source);
    }

    [Fact]
    public void CreatedAt_IsSetAutomatically()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow.AddSeconds(-1);

        // Act
        var visit = new ActualVisitModel
        {
            ScheduledVisitId = Guid.NewGuid(),
            ActualDate = DateTime.UtcNow,
            StartTime = TimeSpan.FromHours(9),
            EndTime = TimeSpan.FromHours(10),
            PatientPresence = PresenceStatus.PresentCollaborative,
            ClinicalNotes = "Note cliniche minime richieste",
            RegisteredBy = Guid.NewGuid(),
            RegisteredByName = "Tester"
        };

        // Assert
        Assert.True(visit.CreatedAt >= beforeCreation);
        Assert.Null(visit.UpdatedAt);
    }

    [Fact]
    public void Version_StartsAtOne()
    {
        // Arrange & Act
        var visit = new ActualVisitModel
        {
            ScheduledVisitId = Guid.NewGuid(),
            ActualDate = DateTime.UtcNow,
            StartTime = TimeSpan.FromHours(14),
            EndTime = TimeSpan.FromHours(15),
            PatientPresence = PresenceStatus.PresentNonCollaborative,
            ClinicalNotes = "Paziente non collaborativo, intervento modificato.",
            RegisteredBy = Guid.NewGuid(),
            RegisteredByName = "Rossi Sara"
        };

        // Assert
        Assert.Equal(1, visit.Version);
    }

    [Fact]
    public void ToString_ReturnsCorrectFormat()
    {
        // Arrange
        var actualDate = new DateTime(2026, 2, 15);
        var visit = new ActualVisitModel
        {
            ScheduledVisitId = Guid.NewGuid(),
            ActualDate = actualDate,
            StartTime = new TimeSpan(10, 30, 0),
            EndTime = new TimeSpan(12, 0, 0),
            PatientPresence = PresenceStatus.PresentCollaborative,
            ClinicalNotes = "Note cliniche complete",
            RegisteredBy = Guid.NewGuid(),
            RegisteredByName = "Verdi Luca"
        };

        // Act
        var result = visit.ToString();

        // Assert - Format: "Visita del dd/MM/yyyy - HH:mm-HH:mm (registrata da Name)"
        Assert.Equal("Visita del 15/02/2026 - 10:30-12:00 (registrata da Verdi Luca)", result);
    }

    [Fact]
    public void OperatorsPresent_InitializesAsEmptyCollection()
    {
        // Arrange & Act
        var visit = new ActualVisitModel
        {
            ScheduledVisitId = Guid.NewGuid(),
            ActualDate = DateTime.UtcNow,
            StartTime = TimeSpan.FromHours(9),
            EndTime = TimeSpan.FromHours(10),
            PatientPresence = PresenceStatus.PresentCollaborative,
            ClinicalNotes = "Test visit",
            RegisteredBy = Guid.NewGuid(),
            RegisteredByName = "Test"
        };

        // Assert
        Assert.NotNull(visit.OperatorsPresent);
        Assert.Empty(visit.OperatorsPresent);
    }

    [Fact]
    public void AllPresenceStatuses_CanBeUsed()
    {
        // Test that all PresenceStatus enum values can be assigned
        var scheduledVisitId = Guid.NewGuid();
        var baseDate = DateTime.UtcNow;
        var registrantId = Guid.NewGuid();

        var presentCollaborative = new ActualVisitModel
        {
            ScheduledVisitId = scheduledVisitId,
            ActualDate = baseDate,
            StartTime = TimeSpan.FromHours(9),
            EndTime = TimeSpan.FromHours(10),
            PatientPresence = PresenceStatus.PresentCollaborative,
            ClinicalNotes = "Test",
            RegisteredBy = registrantId,
            RegisteredByName = "Test"
        };

        var presentNonCollaborative = new ActualVisitModel
        {
            ScheduledVisitId = scheduledVisitId,
            ActualDate = baseDate,
            StartTime = TimeSpan.FromHours(9),
            EndTime = TimeSpan.FromHours(10),
            PatientPresence = PresenceStatus.PresentNonCollaborative,
            ClinicalNotes = "Test",
            RegisteredBy = registrantId,
            RegisteredByName = "Test"
        };

        var absentJustified = new ActualVisitModel
        {
            ScheduledVisitId = scheduledVisitId,
            ActualDate = baseDate,
            StartTime = TimeSpan.FromHours(9),
            EndTime = TimeSpan.FromHours(10),
            PatientPresence = PresenceStatus.AbsentJustified,
            ClinicalNotes = "Paziente assente per motivi di salute documentati",
            RegisteredBy = registrantId,
            RegisteredByName = "Test"
        };

        var absentNotJustified = new ActualVisitModel
        {
            ScheduledVisitId = scheduledVisitId,
            ActualDate = baseDate,
            StartTime = TimeSpan.FromHours(9),
            EndTime = TimeSpan.FromHours(10),
            PatientPresence = PresenceStatus.AbsentNotJustified,
            ClinicalNotes = "Paziente assente senza preavviso",
            RegisteredBy = registrantId,
            RegisteredByName = "Test"
        };

        Assert.Equal(PresenceStatus.PresentCollaborative, presentCollaborative.PatientPresence);
        Assert.Equal(PresenceStatus.PresentNonCollaborative, presentNonCollaborative.PatientPresence);
        Assert.Equal(PresenceStatus.AbsentJustified, absentJustified.PatientPresence);
        Assert.Equal(PresenceStatus.AbsentNotJustified, absentNotJustified.PatientPresence);
    }

    [Fact]
    public void AllVisitSources_CanBeUsed()
    {
        // Test that all VisitSource enum values can be assigned
        var scheduledVisitId = Guid.NewGuid();
        var baseDate = DateTime.UtcNow;
        var registrantId = Guid.NewGuid();

        var educatorImport = new ActualVisitModel
        {
            ScheduledVisitId = scheduledVisitId,
            ActualDate = baseDate,
            StartTime = TimeSpan.FromHours(9),
            EndTime = TimeSpan.FromHours(10),
            PatientPresence = PresenceStatus.PresentCollaborative,
            ClinicalNotes = "Importata da app educatore",
            Source = VisitSource.EducatorImport,
            RegisteredBy = registrantId,
            RegisteredByName = "Educatore"
        };

        var coordinatorDirect = new ActualVisitModel
        {
            ScheduledVisitId = scheduledVisitId,
            ActualDate = baseDate,
            StartTime = TimeSpan.FromHours(10),
            EndTime = TimeSpan.FromHours(11),
            PatientPresence = PresenceStatus.PresentCollaborative,
            ClinicalNotes = "Inserita direttamente dal coordinatore",
            Source = VisitSource.CoordinatorDirect,
            RegisteredBy = registrantId,
            RegisteredByName = "Coordinatore"
        };

        Assert.Equal(VisitSource.EducatorImport, educatorImport.Source);
        Assert.Equal(VisitSource.CoordinatorDirect, coordinatorDirect.Source);
    }

    [Fact]
    public void ClinicalNotes_Validation_RequiresMinimum10Characters()
    {
        // Arrange
        var visit = new ActualVisitModel
        {
            ScheduledVisitId = Guid.NewGuid(),
            ActualDate = DateTime.UtcNow,
            StartTime = TimeSpan.FromHours(9),
            EndTime = TimeSpan.FromHours(10),
            PatientPresence = PresenceStatus.PresentCollaborative,
            ClinicalNotes = "Breve", // Solo 5 caratteri - dovrebbe fallire validazione
            RegisteredBy = Guid.NewGuid(),
            RegisteredByName = "Test"
        };

        // Act
        var context = new ValidationContext(visit) { MemberName = nameof(visit.ClinicalNotes) };
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateProperty(visit.ClinicalNotes, context, results);

        // Assert
        Assert.False(isValid);
        Assert.Contains(results, r => r.ErrorMessage != null && r.ErrorMessage.Contains("almeno 10 caratteri"));
    }

    [Fact]
    public void ClinicalNotes_Validation_AcceptsValidLength()
    {
        // Arrange
        var visit = new ActualVisitModel
        {
            ScheduledVisitId = Guid.NewGuid(),
            ActualDate = DateTime.UtcNow,
            StartTime = TimeSpan.FromHours(9),
            EndTime = TimeSpan.FromHours(10),
            PatientPresence = PresenceStatus.PresentCollaborative,
            ClinicalNotes = "Questo è un testo di note cliniche valido con più di 10 caratteri.",
            RegisteredBy = Guid.NewGuid(),
            RegisteredByName = "Test"
        };

        // Act
        var context = new ValidationContext(visit) { MemberName = nameof(visit.ClinicalNotes) };
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateProperty(visit.ClinicalNotes, context, results);

        // Assert
        Assert.True(isValid);
        Assert.Empty(results);
    }

    [Fact]
    public void Outcomes_CanBeNull()
    {
        // Arrange & Act
        var visit = new ActualVisitModel
        {
            ScheduledVisitId = Guid.NewGuid(),
            ActualDate = DateTime.UtcNow,
            StartTime = TimeSpan.FromHours(9),
            EndTime = TimeSpan.FromHours(10),
            PatientPresence = PresenceStatus.PresentCollaborative,
            ClinicalNotes = "Note cliniche complete e dettagliate",
            RegisteredBy = Guid.NewGuid(),
            RegisteredByName = "Test"
        };

        // Assert
        Assert.Null(visit.Outcomes);
    }

    [Fact]
    public void AuditFields_CanBeSetAndRetrieved()
    {
        // Arrange
        var registrantId = Guid.NewGuid();
        var updaterId = Guid.NewGuid();
        var syncPacketId = Guid.NewGuid();

        // Act
        var visit = new ActualVisitModel
        {
            ScheduledVisitId = Guid.NewGuid(),
            ActualDate = DateTime.UtcNow,
            StartTime = TimeSpan.FromHours(10),
            EndTime = TimeSpan.FromHours(11),
            PatientPresence = PresenceStatus.PresentCollaborative,
            ClinicalNotes = "Visita con audit trail completo",
            RegisteredBy = registrantId,
            RegisteredByName = "Bianchi Marco",
            UpdatedBy = updaterId,
            Version = 3,
            SyncPacketId = syncPacketId
        };

        // Assert
        Assert.Equal(registrantId, visit.RegisteredBy);
        Assert.Equal("Bianchi Marco", visit.RegisteredByName);
        Assert.Equal(updaterId, visit.UpdatedBy);
        Assert.Equal(3, visit.Version);
        Assert.Equal(syncPacketId, visit.SyncPacketId);
    }
}
