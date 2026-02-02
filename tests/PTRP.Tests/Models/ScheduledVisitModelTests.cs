using PTRP.Models;
using PTRP.Models.Enums;

namespace PTRP.Tests.Models;

public class ScheduledVisitModelTests
{
    [Fact]
    public void Constructor_SetsPropertiesCorrectly()
    {
        // Arrange & Act
        var projectId = Guid.NewGuid();
        var scheduledDate = DateTime.UtcNow.AddDays(7);
        var visit = new ScheduledVisitModel
        {
            Id = Guid.NewGuid(),
            TherapyProjectId = projectId,
            Type = VisitType.INTAKE,
            Status = AppointmentStatus.Scheduled,
            ScheduledDate = scheduledDate,
            Notes = "Richiedere documenti identità"
        };

        // Assert
        Assert.NotEqual(Guid.Empty, visit.Id);
        Assert.Equal(projectId, visit.TherapyProjectId);
        Assert.Equal(VisitType.INTAKE, visit.Type);
        Assert.Equal(AppointmentStatus.Scheduled, visit.Status);
        Assert.Equal(scheduledDate, visit.ScheduledDate);
        Assert.Equal("Richiedere documenti identità", visit.Notes);
    }

    [Fact]
    public void Status_HasDefaultValue_Scheduled()
    {
        // Arrange & Act
        var visit = new ScheduledVisitModel
        {
            TherapyProjectId = Guid.NewGuid(),
            Type = VisitType.INTERMEDIATE,
            ScheduledDate = DateTime.UtcNow.AddDays(30)
        };

        // Assert
        Assert.Equal(AppointmentStatus.Scheduled, visit.Status);
    }

    [Fact]
    public void CreatedAt_IsSetAutomatically()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow.AddSeconds(-1);

        // Act
        var visit = new ScheduledVisitModel
        {
            TherapyProjectId = Guid.NewGuid(),
            Type = VisitType.FINAL,
            ScheduledDate = DateTime.UtcNow.AddDays(60)
        };

        // Assert
        Assert.True(visit.CreatedAt >= beforeCreation);
        Assert.Null(visit.UpdatedAt);
    }

    [Fact]
    public void Version_StartsAtOne()
    {
        // Arrange & Act
        var visit = new ScheduledVisitModel
        {
            TherapyProjectId = Guid.NewGuid(),
            Type = VisitType.DISCHARGE,
            ScheduledDate = DateTime.UtcNow.AddDays(90)
        };

        // Assert
        Assert.Equal(1, visit.Version);
    }

    [Fact]
    public void ToString_ReturnsCorrectFormat()
    {
        // Arrange
        var scheduledDate = new DateTime(2026, 3, 15, 10, 30, 0);
        var visit = new ScheduledVisitModel
        {
            TherapyProjectId = Guid.NewGuid(),
            Type = VisitType.INTAKE,
            Status = AppointmentStatus.Scheduled,
            ScheduledDate = scheduledDate
        };

        // Act
        var result = visit.ToString();

        // Assert - Format: "Type - dd/MM/yyyy HH:mm [Status]"
        Assert.Equal("INTAKE - 15/03/2026 10:30 [Scheduled]", result);
    }

    [Fact]
    public void RescheduledDate_CanBeNull()
    {
        // Arrange & Act
        var visit = new ScheduledVisitModel
        {
            TherapyProjectId = Guid.NewGuid(),
            Type = VisitType.INTERMEDIATE,
            ScheduledDate = DateTime.UtcNow.AddDays(30)
        };

        // Assert
        Assert.Null(visit.RescheduledDate);
    }

    [Fact]
    public void RescheduledDate_CanBeSet_WhenStatusIsRescheduled()
    {
        // Arrange & Act
        var originalDate = DateTime.UtcNow.AddDays(7);
        var newDate = DateTime.UtcNow.AddDays(14);
        var visit = new ScheduledVisitModel
        {
            TherapyProjectId = Guid.NewGuid(),
            Type = VisitType.INTAKE,
            ScheduledDate = originalDate,
            Status = AppointmentStatus.Rescheduled,
            RescheduledDate = newDate
        };

        // Assert
        Assert.Equal(AppointmentStatus.Rescheduled, visit.Status);
        Assert.Equal(originalDate, visit.ScheduledDate);
        Assert.Equal(newDate, visit.RescheduledDate);
    }

    [Fact]
    public void ActualVisit_CanBeNull_WhenNotYetPerformed()
    {
        // Arrange & Act
        var visit = new ScheduledVisitModel
        {
            TherapyProjectId = Guid.NewGuid(),
            Type = VisitType.INTAKE,
            ScheduledDate = DateTime.UtcNow.AddDays(7)
        };

        // Assert - ActualVisit navigation property should be null for future appointments
        Assert.Null(visit.ActualVisit);
    }

    [Fact]
    public void TherapyProject_CanBeNull_InitiallyBeforeNavigation()
    {
        // Arrange & Act
        var visit = new ScheduledVisitModel
        {
            TherapyProjectId = Guid.NewGuid(),
            Type = VisitType.INTERMEDIATE,
            ScheduledDate = DateTime.UtcNow.AddDays(30)
        };

        // Assert - TherapyProject navigation property can be null before loading
        Assert.Null(visit.TherapyProject);
    }

    [Fact]
    public void AllVisitTypes_CanBeUsed()
    {
        // Test that all VisitType enum values can be assigned
        var projectId = Guid.NewGuid();
        var baseDate = DateTime.UtcNow;

        var intake = new ScheduledVisitModel { TherapyProjectId = projectId, Type = VisitType.INTAKE, ScheduledDate = baseDate };
        var intermediate = new ScheduledVisitModel { TherapyProjectId = projectId, Type = VisitType.INTERMEDIATE, ScheduledDate = baseDate };
        var final = new ScheduledVisitModel { TherapyProjectId = projectId, Type = VisitType.FINAL, ScheduledDate = baseDate };
        var discharge = new ScheduledVisitModel { TherapyProjectId = projectId, Type = VisitType.DISCHARGE, ScheduledDate = baseDate };
        var homeVisit = new ScheduledVisitModel { TherapyProjectId = projectId, Type = VisitType.HOME_VISIT, ScheduledDate = baseDate };
        var followUp = new ScheduledVisitModel { TherapyProjectId = projectId, Type = VisitType.FOLLOW_UP, ScheduledDate = baseDate };
        var other = new ScheduledVisitModel { TherapyProjectId = projectId, Type = VisitType.OTHER, ScheduledDate = baseDate };

        Assert.Equal(VisitType.INTAKE, intake.Type);
        Assert.Equal(VisitType.INTERMEDIATE, intermediate.Type);
        Assert.Equal(VisitType.FINAL, final.Type);
        Assert.Equal(VisitType.DISCHARGE, discharge.Type);
        Assert.Equal(VisitType.HOME_VISIT, homeVisit.Type);
        Assert.Equal(VisitType.FOLLOW_UP, followUp.Type);
        Assert.Equal(VisitType.OTHER, other.Type);
    }

    [Fact]
    public void AllAppointmentStatuses_CanBeUsed()
    {
        // Test that all AppointmentStatus enum values can be assigned
        var projectId = Guid.NewGuid();
        var scheduledDate = DateTime.UtcNow.AddDays(7);

        var scheduled = new ScheduledVisitModel { TherapyProjectId = projectId, Type = VisitType.INTAKE, ScheduledDate = scheduledDate, Status = AppointmentStatus.Scheduled };
        var completed = new ScheduledVisitModel { TherapyProjectId = projectId, Type = VisitType.INTAKE, ScheduledDate = scheduledDate, Status = AppointmentStatus.Completed };
        var missed = new ScheduledVisitModel { TherapyProjectId = projectId, Type = VisitType.INTAKE, ScheduledDate = scheduledDate, Status = AppointmentStatus.Missed };
        var rescheduled = new ScheduledVisitModel { TherapyProjectId = projectId, Type = VisitType.INTAKE, ScheduledDate = scheduledDate, Status = AppointmentStatus.Rescheduled };
        var cancelled = new ScheduledVisitModel { TherapyProjectId = projectId, Type = VisitType.INTAKE, ScheduledDate = scheduledDate, Status = AppointmentStatus.Cancelled };

        Assert.Equal(AppointmentStatus.Scheduled, scheduled.Status);
        Assert.Equal(AppointmentStatus.Completed, completed.Status);
        Assert.Equal(AppointmentStatus.Missed, missed.Status);
        Assert.Equal(AppointmentStatus.Rescheduled, rescheduled.Status);
        Assert.Equal(AppointmentStatus.Cancelled, cancelled.Status);
    }

    [Fact]
    public void AuditFields_CanBeSetAndRetrieved()
    {
        // Arrange
        var operatorId = Guid.NewGuid();
        var syncPacketId = Guid.NewGuid();

        // Act
        var visit = new ScheduledVisitModel
        {
            TherapyProjectId = Guid.NewGuid(),
            Type = VisitType.INTAKE,
            ScheduledDate = DateTime.UtcNow.AddDays(7),
            CreatedBy = operatorId,
            UpdatedBy = operatorId,
            Version = 2,
            SyncPacketId = syncPacketId
        };

        // Assert
        Assert.Equal(operatorId, visit.CreatedBy);
        Assert.Equal(operatorId, visit.UpdatedBy);
        Assert.Equal(2, visit.Version);
        Assert.Equal(syncPacketId, visit.SyncPacketId);
    }
}
