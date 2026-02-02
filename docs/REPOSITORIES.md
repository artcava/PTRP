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
6. **Domain Constraints**: Use C# language features (e.g., `init` accessor) to enforce immutability at compile-time

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
- `GetByIdAsync(Guid id)` - Load single visit with operators
- `GetByScheduledVisitIdAsync(Guid scheduledVisitId)` - **1:1 query** - single visit or null
- `GetByEducatorIdInRangeAsync(Guid educatorId, DateTime from, DateTime to)` - Visits registered by educator in period (used for export/sync)
- `GetByProjectIdAsync(Guid projectId)` - All registered visits for project
- `GetByPatientIdAsync(Guid patientId)` - All visits for patient
- `GetByPatientAttendanceAsync(string status)` - Filter by attendance (Attended, Absent, PartiallyAttended)
- `ExistsByScheduledVisitIdAsync(Guid scheduledVisitId)` - Check if visit already registered (1:1 constraint)
- `CountByProjectIdAsync(Guid projectId)` - Statistics
- `AddAsync()`, `UpdateAsync()`, `DeleteAsync()` - CRUD operations

**CRITICAL: 1:1 Constraint Enforcement**

The relationship between `ScheduledVisitModel` and `ActualVisitModel` is strictly **1:1 and immutable**.

#### Design: Immutability with `init` Accessor

The `ScheduledVisitId` property uses C# 9's `init` accessor to guarantee immutability:

```csharp
public class ActualVisitModel
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// IMMUTABILE: Può essere impostato solo durante l'inizializzazione (object initializer).
    /// Una volta creato, questo vincolo 1:1 non può mai essere violato.
    /// </summary>
    public Guid ScheduledVisitId { get; init; }
    
    public DateTime ActualDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string ClinicalNotes { get; set; } = string.Empty;
    public PresenceStatus PatientPresence { get; set; }
    public VisitSource Source { get; set; }
    public Guid RegisteredBy { get; set; }
    public string RegisteredByName { get; set; } = string.Empty;
    
    // Navigation
    public ScheduledVisitModel ScheduledVisit { get; set; } = null!;
    public ICollection<VisitOperatorModel> OperatorsPresent { get; set; } = new List<VisitOperatorModel>();
}
```

#### Why `init`?

- **Compile-time guarantee**: Attempting `actualVisit.ScheduledVisitId = newId;` fails with `CS8852`
- **Zero runtime overhead**: No validation methods or property wrappers needed
- **Standard C# idiom**: Idiomatic approach for immutable properties in C# 9+
- **EF Core compatible**: Works seamlessly with Entity Framework Core

#### Correct Usage

```csharp
// ✅ CORRECT: Set during initialization
var actualVisit = new ActualVisitModel
{
    ScheduledVisitId = appointmentId,  // OK - only during init
    ActualDate = DateTime.Now,
    StartTime = new TimeSpan(9, 0, 0),
    EndTime = new TimeSpan(10, 0, 0),
    ClinicalNotes = "Patient showed good progress...",
    PatientPresence = PresenceStatus.PresentCollaborative,
    Source = VisitSource.EducatorImport,
    RegisteredBy = educatorId,
    RegisteredByName = $"{educator.FirstName} {educator.LastName}"
};

await _actualVisitRepository.AddAsync(actualVisit);

// ❌ INCORRECT: Attempt to modify after creation
actualVisit.ScheduledVisitId = anotherAppointmentId;
// Error CS8852: Init-only property can only be assigned in an object initializer
```

#### Repository-Level Validation

Additionally, `AddAsync` enforces business rules at runtime:

```csharp
public async Task AddAsync(ActualVisitModel actualVisit)
{
    // Verify ScheduledVisit exists
    if (!await _context.ScheduledVisits.AnyAsync(sv => sv.Id == actualVisit.ScheduledVisitId))
    {
        throw new InvalidOperationException(
            $"La visita programmata con ID {actualVisit.ScheduledVisitId} non esiste.");
    }

    // Enforce 1:1 constraint - no second visit for same appointment
    if (await ExistsByScheduledVisitIdAsync(actualVisit.ScheduledVisitId))
    {
        throw new InvalidOperationException(
            $"La visita programmata con ID {actualVisit.ScheduledVisitId} ha già una visita effettiva associata.");
    }

    _context.ActualVisits.Add(actualVisit);
    await _context.SaveChangesAsync();
}
```

#### UpdateAsync: No Changes to ScheduledVisitId

`UpdateAsync` is simplified because `ScheduledVisitId` cannot be modified:

```csharp
public async Task UpdateAsync(ActualVisitModel actualVisit)
{
    // Verify visit exists
    if (!await ExistsAsync(actualVisit.Id))
    {
        throw new InvalidOperationException(
            $"La visita effettiva con ID {actualVisit.Id} non esiste.");
    }

    _context.ActualVisits.Update(actualVisit);
    await _context.SaveChangesAsync();
}
```

The `init` accessor guarantees that `ScheduledVisitId` was set at creation and cannot be changed. No runtime checks needed.

**Example Usage**:
```csharp
var actualVisit = await _actualVisitRepository.GetByIdAsync(visitId);
actualVisit.ClinicalNotes = "Updated notes"; // OK
actualVisit.PatientPresence = PresenceStatus.PresentCollaborative; // OK
await _actualVisitRepository.UpdateAsync(actualVisit);

// actualVisit.ScheduledVisitId = otherId; // Compile-time error!
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
  - **1:1 constraint violation** (ActualVisit already exists for ScheduledVisit)
  - *(No longer thrown for changing ScheduledVisitId - prevented at compile-time)*

- **`ArgumentNullException`**: Null parameter passed to method

### Example Error Handling

```csharp
try
{
    var actualVisit = new ActualVisitModel 
    { 
        ScheduledVisitId = appointmentId,
        // ... other properties
    };
    await _actualVisitRepository.AddAsync(actualVisit);
}
catch (InvalidOperationException ex) when (ex.Message.Contains("1:1"))
{
    // Handle duplicate actual visit for same appointment
    logger.LogWarning($"Visit already registered for appointment {appointmentId}");
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
- **Immutability at Compile-Time**: `ScheduledVisitId` cannot be reassigned (language feature test)
- **UpdateAsync Validation**: Verify existing visits can be updated without modifying `ScheduledVisitId`
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

## Domain Constraints Best Practices

### Use Language Features to Enforce Constraints

For immutable relationships (like 1:1 FK), use `init` instead of runtime checks:

```csharp
// ✅ GOOD: Compile-time guarantee
public Guid ScheduledVisitId { get; init; }

// ❌ AVOID: Runtime-only (can be bypassed)
public Guid ScheduledVisitId { get; private set; }
```

### Combine Language Features with Runtime Validation

Even with `init`, repositories should validate:
- Foreign key existence
- 1:1 constraint (no duplicate visits for same appointment)
- Other business rules

### Document Constraints Clearly

Use XML documentation to explain immutability and constraints:

```csharp
/// <summary>
/// IMMUTABILE: Può essere impostato solo durante l'inizializzazione.
/// Una volta creato, questo vincolo non può mai essere violato.
/// </summary>
public Guid ScheduledVisitId { get; init; }
```

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
- [C# init Accessor](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/init) (C# 9+)
- PTRP Database Schema: `docs/DATABASE.md`
- PTRP Architecture: `docs/ARCHITECTURE.md`
