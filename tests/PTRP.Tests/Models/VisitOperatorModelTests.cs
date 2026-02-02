using PTRP.Models;
using PTRP.Models.Enums;

namespace PTRP.Tests.Models;

public class VisitOperatorModelTests
{
    [Fact]
    public void Constructor_SetsPropertiesCorrectly()
    {
        // Arrange & Act
        var actualVisitId = Guid.NewGuid();
        var educatorId = Guid.NewGuid();
        var visitOperator = new VisitOperatorModel
        {
            ActualVisitId = actualVisitId,
            EducatorId = educatorId,
            IsRegistrant = true
        };

        // Assert
        Assert.Equal(actualVisitId, visitOperator.ActualVisitId);
        Assert.Equal(educatorId, visitOperator.EducatorId);
        Assert.True(visitOperator.IsRegistrant);
    }

    [Fact]
    public void IsRegistrant_HasDefaultValue_False()
    {
        // Arrange & Act
        var visitOperator = new VisitOperatorModel
        {
            ActualVisitId = Guid.NewGuid(),
            EducatorId = Guid.NewGuid()
        };

        // Assert - Default should be false (operator present but not the registrant)
        Assert.False(visitOperator.IsRegistrant);
    }

    [Fact]
    public void AssignedAt_IsSetAutomatically()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow.AddSeconds(-1);

        // Act
        var visitOperator = new VisitOperatorModel
        {
            ActualVisitId = Guid.NewGuid(),
            EducatorId = Guid.NewGuid(),
            IsRegistrant = false
        };

        // Assert
        Assert.True(visitOperator.AssignedAt >= beforeCreation);
        Assert.True(visitOperator.AssignedAt <= DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public void NavigationProperties_CanBeNull_InitiallyBeforeLoading()
    {
        // Arrange & Act
        var visitOperator = new VisitOperatorModel
        {
            ActualVisitId = Guid.NewGuid(),
            EducatorId = Guid.NewGuid()
        };

        // Assert - Navigation properties can be null before EF Core loads them
        Assert.Null(visitOperator.ActualVisit);
        Assert.Null(visitOperator.Educator);
    }

    [Fact]
    public void IsRegistrant_CanBeTrue_ForRegistrantOperator()
    {
        // Arrange & Act - Scenario: educatore che ha compilato la visita
        var visitOperator = new VisitOperatorModel
        {
            ActualVisitId = Guid.NewGuid(),
            EducatorId = Guid.NewGuid(),
            IsRegistrant = true
        };

        // Assert
        Assert.True(visitOperator.IsRegistrant);
    }

    [Fact]
    public void IsRegistrant_CanBeFalse_ForPresentButNotRegistrantOperator()
    {
        // Arrange & Act - Scenario: educatore presente ma non ha compilato
        var visitOperator = new VisitOperatorModel
        {
            ActualVisitId = Guid.NewGuid(),
            EducatorId = Guid.NewGuid(),
            IsRegistrant = false
        };

        // Assert
        Assert.False(visitOperator.IsRegistrant);
    }

    [Fact]
    public void ManyToMany_CanHaveMultipleOperatorsForSameVisit()
    {
        // Arrange - Scenario: visita con 3 educatori (1 registrant + 2 presenti)
        var actualVisitId = Guid.NewGuid();
        var educator1Id = Guid.NewGuid();
        var educator2Id = Guid.NewGuid();
        var educator3Id = Guid.NewGuid();

        // Act
        var registrant = new VisitOperatorModel
        {
            ActualVisitId = actualVisitId,
            EducatorId = educator1Id,
            IsRegistrant = true
        };

        var present1 = new VisitOperatorModel
        {
            ActualVisitId = actualVisitId,
            EducatorId = educator2Id,
            IsRegistrant = false
        };

        var present2 = new VisitOperatorModel
        {
            ActualVisitId = actualVisitId,
            EducatorId = educator3Id,
            IsRegistrant = false
        };

        // Assert - All share the same ActualVisitId but different EducatorIds
        Assert.Equal(actualVisitId, registrant.ActualVisitId);
        Assert.Equal(actualVisitId, present1.ActualVisitId);
        Assert.Equal(actualVisitId, present2.ActualVisitId);

        Assert.NotEqual(registrant.EducatorId, present1.EducatorId);
        Assert.NotEqual(registrant.EducatorId, present2.EducatorId);
        Assert.NotEqual(present1.EducatorId, present2.EducatorId);

        // Only one should be registrant
        Assert.True(registrant.IsRegistrant);
        Assert.False(present1.IsRegistrant);
        Assert.False(present2.IsRegistrant);
    }

    [Fact]
    public void ManyToMany_CanHaveSingleEducatorForMultipleVisits()
    {
        // Arrange - Scenario: stesso educatore presente in 3 visite diverse
        var educatorId = Guid.NewGuid();
        var visit1Id = Guid.NewGuid();
        var visit2Id = Guid.NewGuid();
        var visit3Id = Guid.NewGuid();

        // Act
        var visit1Operator = new VisitOperatorModel
        {
            ActualVisitId = visit1Id,
            EducatorId = educatorId,
            IsRegistrant = true
        };

        var visit2Operator = new VisitOperatorModel
        {
            ActualVisitId = visit2Id,
            EducatorId = educatorId,
            IsRegistrant = false // Presente ma non registrant
        };

        var visit3Operator = new VisitOperatorModel
        {
            ActualVisitId = visit3Id,
            EducatorId = educatorId,
            IsRegistrant = true
        };

        // Assert - All share the same EducatorId but different ActualVisitIds
        Assert.Equal(educatorId, visit1Operator.EducatorId);
        Assert.Equal(educatorId, visit2Operator.EducatorId);
        Assert.Equal(educatorId, visit3Operator.EducatorId);

        Assert.NotEqual(visit1Operator.ActualVisitId, visit2Operator.ActualVisitId);
        Assert.NotEqual(visit1Operator.ActualVisitId, visit3Operator.ActualVisitId);
        Assert.NotEqual(visit2Operator.ActualVisitId, visit3Operator.ActualVisitId);

        // Registrant status can vary per visit
        Assert.True(visit1Operator.IsRegistrant);
        Assert.False(visit2Operator.IsRegistrant);
        Assert.True(visit3Operator.IsRegistrant);
    }

    [Fact]
    public void AssignedAt_CanBeSetExplicitly_ForHistoricalData()
    {
        // Arrange
        var historicalDate = new DateTime(2025, 6, 15, 14, 30, 0);

        // Act
        var visitOperator = new VisitOperatorModel
        {
            ActualVisitId = Guid.NewGuid(),
            EducatorId = Guid.NewGuid(),
            IsRegistrant = false,
            AssignedAt = historicalDate
        };

        // Assert
        Assert.Equal(historicalDate, visitOperator.AssignedAt);
    }
}
