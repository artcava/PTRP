# Repository Implementation Guide

## Overview

This document describes the repository implementations in PTRP, providing a comprehensive guide to the data access layer.

## Architecture

### Pattern: Repository Pattern with EF Core

Each repository implements:
- **Interface** (`PTRP.Data/Repositories/Interfaces/I*.cs`)
- **Implementation** (`PTRP.Data/Repositories/*.cs`)
- **DbContext**: `PTRPDbContext`

### Key Principles

1. **Atomic Operations**: Each CRUD operation performs automatic `SaveChangesAsync()` - no need to call it separately
2. **Async/Await**: All methods are async for non-blocking I/O
3. **CancellationToken Support**: All async methods support cancellation
4. **Eager Loading**: Use `Include()` / `ThenInclude()` to load related entities only when needed
5. **Query Optimization**: Use `AsNoTracking()` for read-only operations to improve performance

## Repository Implementations

### 1. PatientRepository

**Location**: `src/PTRP.Data/Repositories/PatientRepository.cs`

**Responsibilities**:
- CRUD operations for Patient entities
- Searching patients by first/last name
- Loading related therapy projects

**Key Methods**:
- `GetAllAsync()` - Retrieve all patients
- `GetByIdAsync(Guid id)` - Get single patient
- `GetByIdWithProjectsAsync(Guid id)` - Get patient with related projects (eager load)
- `SearchAsync(string searchTerm)` - Case-insensitive search
- `AddAsync()`, `UpdateAsync()`, `DeleteAsync()` - CRUD operations

**Example Usage**:
```csharp
var patient = await _patientRepository.GetByIdWithProjectsAsync(patientId);
var projects = patient.TherapyProjects; // Already loaded
```

---

### 2. TherapyProjectRepository

**Location**: `src/PTRP.Data/Repositories/TherapyProjectRepository.cs`

**Responsibilities**:
- CRUD operations for TherapyProject entities
- Managing N:N relationships with ProfessionalEducators via `ProjectOperatorModel`
- Querying by patient, educator, or status
- Enforcing business rules (e.g., cannot exceed FK constraints)

**Key Methods**:
- `GetByIdWithRelationsAsync(Guid id)` - Loads project with patient and educators
- `GetByPatientIdAsync(Guid patientId)` - All projects for a patient
- `GetByEducatorIdAsync(Guid educatorId)` - All projects assigned to educator
- `GetByStatusAsync(string status)` - Filter by status (Active, Suspended, etc.)
- `AssignEducatorAsync()`, `RemoveEducatorAsync()` - Manage N:N relationships
- `SearchAsync(string searchTerm)` - Search by title/description

**Business Rule Validation**:
- Verifies patient exists before creating project
- Prevents assignment of non-existent educators

**Example Usage**:
```csharp
// Create project with educators
var project = new TherapyProjectModel { /* ... */ };
await _projectRepository.AddAsync(project);

// Assign educators (N:N)
await _projectRepository.AssignEducatorAsync(projectId, educatorId1);
await _projectRepository.AssignEducatorAsync(projectId, educatorId2);
```

---

### 3. ScheduledVisitRepository

**Location**: `src/PTRP.Data/Repositories/ScheduledVisitRepository.cs`

**Responsibilities**:
- CRUD operations for ScheduledVisitModel (appointment scheduling)
- Querying appointments by date, project, educator, or type
- Bulk creation of appointments (`AddRangeAsync` for 4 canonical appointments)
- Supporting coordinator calendar views and educator schedules

**Key Methods**:
- `GetByIdWithRelationsAsync(Guid id)` - Load with project, patient, and visit type
- `GetByProjectIdAsync(Guid projectId)` - All appointments for a project
- `GetByEducatorIdInRangeAsync(Guid educatorId, DateTime from, DateTime to)` - Educator schedule in date range
- `GetByVisitTypeIdAsync(Guid visitTypeId)` - Filter by visit type (INTAKE, INTERMEDIATE, etc.)
- `GetByStatusAsync(string status)` - Filter by appointment status (Scheduled, Completed, Missed)
- `GetByDateAsync(DateTime date)` - All appointments for a specific day
- `AddRangeAsync(IEnumerable<ScheduledVisitModel> visits)` - Bulk add (e.g., 4 canonical visits)
- `CountByProjectIdAsync(Guid projectId)` - Count appointments for project

**Critical Validations**:
- Verifies TherapyProject exists before creating appointment
- Verifies VisitType exists before creating appointment

**Example Usage**:
```csharp
// Create 4 canonical appointments for new project
var canonicalVisits = new[]
{
    new ScheduledVisitModel { /* INTAKE +3 months */ },
    new ScheduledVisitModel { /* INTERMEDIATE +6 months */ },
    // ... FINAL and DISCHARGE
};
await _scheduledVisitRepository.AddRangeAsync(canonicalVisits);

// Get educator's calendar for month
var visitsThisMonth = await _scheduledVisitRepository.GetByEducatorIdInRangeAsync(
    educatorId,
    DateTime.Now.AddMonths(-0),
    DateTime.Now.AddMonths(1));
```

---

### 4. ActualVisitRepository

**Location**: `src/PTRP.Data/Repositories/ActualVisitRepository.cs`

**Responsibilities**:
- CRUD operations for ActualVisitModel (registered visits)
- Managing N:N relationships with educators via `VisitOperatorModel`
- **CRITICAL: Enforcing 1:1 constraint with ScheduledVisitModel**
- Supporting educator visit registration and export for sync

**Key Methods**:
- `GetByIdWithRelationsAsync(Guid id)` - Load with appointment, patient, project, and operators
- `GetByScheduledVisitIdAsync(Guid scheduledVisitId)` - **1:1 query** - single visit or null
- `GetByEducatorIdInRangeAsync(Guid educatorId, DateTime from, DateTime to)` - Visits registered by educator in period (used for export/sync)
- `GetByProjectIdAsync(Guid projectId)` - All registered visits for project
- `GetByPatientIdAsync(Guid patientId)` - All visits for patient
- `GetByPatientAttendanceAsync(string status)` - Filter by attendance (Attended, Absent, PartiallyAttended)
- `GetByDateAsync(DateTime date)` - Visits on specific date
- `GetInRangeAsync(DateTime from, DateTime to)` - Visits in date range
- `ExistsByScheduledVisitIdAsync(Guid scheduledVisitId)` - Check if visit already registered (1:1 constraint)
- `CountByProjectIdAsync(Guid projectId)` - Statistics
- `CountByEducatorInRangeAsync(Guid educatorId, DateTime from, DateTime to)` - Statistics for reporting

**CRITICAL: 1:1 Constraint Enforcement**

The relationship between `ScheduledVisitModel` and `ActualVisitModel` is strictly 1:1:
- Each `ScheduledVisit` can have **at most ONE** `ActualVisit`
- **UNIQUE constraint** on `ScheduledVisitId` enforced at DB level
- Repository enforces at application level:

```csharp
// AddAsync throws if actual visit already exists for this appointment
if (await this.ExistsByScheduledVisitIdAsync(actualVisit.ScheduledVisitId))
    throw new InvalidOperationException("Relationship already exists");
```

- **UpdateAsync prevents changing the ScheduledVisitId** (immutable relationship)

**Example Usage**:
```csharp
// Register a visit (educator workflow)
var actualVisit = new ActualVisitModel
{
    ScheduledVisitId = appointmentId, // Links to appointment
    VisitDate = DateTime.Now,
    ClinicalNotes = "Patient showed good progress...",
    PatientAttendance = "Attended",
    VisitSource = "EducatorImport"
};

// Add operator to visit (N:N)
var visitOperator = new VisitOperatorModel
{
    OperatorId = educatorId,
    Role = "Lead"
};
actualVisit.VisitOperators.Add(visitOperator);

await _actualVisitRepository.AddAsync(actualVisit);

// Later: query for export/sync
var visitsSinceLastSync = await _actualVisitRepository.GetByEducatorIdInRangeAsync(
    educatorId,
    lastSyncDate,
    DateTime.Now);
```

---

### 5. EducatorRepository

**Location**: `src/PTRP.Data/Repositories/EducatorRepository.cs`

**Responsibilities**:
- CRUD operations for ProfessionalEducatorModel
- Searching educators
- Loading related projects and visits

**Key Methods**:
- `GetAllAsync()` - All educators
- `GetByIdAsync(Guid id)` - Single educator
- `GetByIdWithProjectsAsync(Guid id)` - Educator with assigned projects (eager load)
- `SearchAsync(string searchTerm)` - Case-insensitive search
- `GetBySpecialtyAsync(string specialty)` - Filter by specialty/competence

---

## Usage in Services

### Dependency Injection

Repositories are registered in the DI container:

```csharp
// Program.cs or Startup.cs
services.AddScoped<IPatientRepository, PatientRepository>();
services.AddScoped<ITherapyProjectRepository, TherapyProjectRepository>();
services.AddScoped<IScheduledVisitRepository, ScheduledVisitRepository>();
services.AddScoped<IActualVisitRepository, ActualVisitRepository>();
services.AddScoped<IEducatorRepository, EducatorRepository>();
```

### Service Injection Pattern

```csharp
public class PatientService
{
    private readonly IPatientRepository _patientRepo;
    private readonly ITherapyProjectRepository _projectRepo;

    public PatientService(
        IPatientRepository patientRepo,
        ITherapyProjectRepository projectRepo)
    {
        _patientRepo = patientRepo;
        _projectRepo = projectRepo;
    }

    public async Task CreatePatientWithProjectAsync(PatientModel patient, TherapyProjectModel project)
    {
        // Repositories handle their own SaveChanges
        await _patientRepo.AddAsync(patient);
        project.PatientId = patient.Id;
        await _projectRepo.AddAsync(project);
    }
}
```

---

## Query Optimization

### Eager Loading Example

```csharp
// GOOD: Single query with Include - loaded into memory
var project = await _projectRepository.GetByIdWithRelationsAsync(projectId);
var patient = project.Patient; // Already loaded
var educators = project.ProfessionalEducators; // Already loaded

// AVOID: Multiple queries (N+1 problem)
var project = await _projectRepository.GetByIdAsync(projectId);
var patient = await _patientRepository.GetByIdAsync(project.PatientId); // Extra query
```

### AsNoTracking Example

```csharp
// GOOD: AsNoTracking for read-only - improves performance
var allVisits = await _actualVisitRepository.GetAllAsync();

// When modifying:
var visitToModify = await _actualVisitRepository.GetByIdAsync(visitId); // Tracked
await _actualVisitRepository.UpdateAsync(visitToModify); // Detects changes
```

---

## Error Handling

### Common Exceptions

- **`InvalidOperationException`**: 
  - Entity not found during update/delete
  - Referenced entity doesn't exist (FK constraint)
  - **1:1 constraint violation** (ActualVisit already exists)
  - Attempting to change immutable relationship (ScheduledVisitId)

- **`ArgumentNullException`**: Null parameter passed to method

### Example Error Handling

```csharp
try
{
    var actualVisit = new ActualVisitModel { /* ... */ };
    await _actualVisitRepository.AddAsync(actualVisit);
}
catch (InvalidOperationException ex) when (ex.Message.Contains("1:1"))
{
    // Handle duplicate actual visit for same appointment
    logger.LogWarning($"Visit already registered for appointment {actualVisit.ScheduledVisitId}");
    // Show user-friendly error
}
catch (InvalidOperationException ex)
{
    // Generic FK constraint or not found
    logger.LogError(ex, "Database operation failed");
}
```

---

## Testing

### Repository Tests Location

`tests/PTRP.Tests/Repositories/`

### Test Pattern

```csharp
public class RepositoryTests : IDisposable
{
    private readonly PTRPDbContext _context;
    private readonly IRepository _repository;

    public RepositoryTests()
    {
        // In-memory DB for testing
        var options = new DbContextOptionsBuilder<PTRPDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
        _context = new PTRPDbContext(options);
        _repository = new Repository(_context);
    }

    [Fact]
    public async Task GetByIdAsync_ValidId_ReturnsEntity()
    {
        // Arrange
        var entity = /* create */;
        await _repository.AddAsync(entity);

        // Act
        var result = await _repository.GetByIdAsync(entity.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entity.Id, result.Id);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
```

### Critical Tests for ActualVisitRepository

- **1:1 Constraint**: Verify second `ActualVisit` for same `ScheduledVisit` throws exception
- **Immutable Relationship**: Verify `UpdateAsync` prevents changing `ScheduledVisitId`
- **Cascade Delete**: Verify removing visit cleans up `VisitOperatorModel` entries

---

## Performance Considerations

### Database Indexes

Ensure these indexes are created:

```sql
-- ScheduledVisit queries
CREATE INDEX IX_ScheduledVisit_TherapyProjectId ON ScheduledVisits(TherapyProjectId);
CREATE INDEX IX_ScheduledVisit_VisitTypeId ON ScheduledVisits(VisitTypeId);
CREATE INDEX IX_ScheduledVisit_ScheduledDate ON ScheduledVisits(ScheduledDate);

-- ActualVisit queries (CRITICAL for 1:1 enforcement)
CREATE UNIQUE INDEX UX_ActualVisit_ScheduledVisitId ON ActualVisits(ScheduledVisitId);
CREATE INDEX IX_ActualVisit_VisitDate ON ActualVisits(VisitDate);
```

### Query Performance

- **Avoid loading large collections**: Use pagination for lists > 100 items
- **Lazy load when appropriate**: Don't eagerly load data you won't use
- **Batch operations**: Use `AddRangeAsync` for bulk inserts

---

## Future Enhancements

1. **Caching**: Add in-memory cache for frequently-accessed lookup data (VisitTypes, Educators)
2. **Pagination**: Add `Skip()` / `Take()` methods for large result sets
3. **Specifications Pattern**: Consider for complex query combinations
4. **Unit of Work Pattern**: Coordinate multiple repositories in single transaction

---

## References

- [EF Core Best Practices](https://docs.microsoft.com/en-us/ef/core/performance/)
- [Repository Pattern](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design)
- PTRP Database Schema: `docs/DATABASE.md`
- PTRP Architecture: `docs/ARCHITECTURE.md`
