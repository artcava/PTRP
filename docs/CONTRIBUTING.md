# Contributing to PTRP

Thank you for your interest in contributing to PTRP (Progetto Terapeutico Ressourcenplanung)!

This guide explains how to contribute code, documentation, and bug reports.

---

## Code of Conduct

- Be respectful and inclusive
- Focus on constructive feedback
- No harassment or discrimination
- Report violations to maintainers

---

## Getting Started

### 1. Fork and Clone
```bash
# Fork repository on GitHub (click Fork button)

# Clone your fork
git clone https://github.com/YOUR-USERNAME/PTRP.git
cd PTRP

# Add upstream remote
git remote add upstream https://github.com/artcava/PTRP.git
```

### 2. Set Up Development Environment
```bash
# Install .NET 10
# Download from https://dotnet.microsoft.com/en-us/download/dotnet/10.0

# Restore dependencies
dotnet restore

# Build
dotnet build

# Run tests
dotnet test

# Run app
dotnet run --project src/PTRP.App/PTRP.App.csproj
```

---

## Workflow: Code Contributions

### Step 1: Create Feature Branch
```bash
# Update main
git checkout main
git pull upstream main

# Create feature branch
git checkout -b feature/my-feature

# Or bugfix
git checkout -b bugfix/my-bugfix
```

### Step 2: Make Changes
```bash
# Edit files
# Follow code style (see below)
# Add tests if needed

# Verify builds
dotnet build

# Run tests
dotnet test

# Check code coverage
dotnet test /p:CollectCoverage=true
```

### Step 3: Commit
```bash
git add .
git commit -m "feat: description of feature"

# If multiple commits, squash them
git rebase -i upstream/main
```

**Commit Message Format:**
```
<type>: <subject>

<body (optional)>

<footer (optional)>
```

**Types:**
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation
- `test`: Tests
- `refactor`: Refactoring
- `perf`: Performance
- `ci`: CI/CD changes
- `chore`: Build, dependencies

**Examples:**
```bash
git commit -m "feat: add patient search by email"
git commit -m "fix: prevent crash when date is invalid"
git commit -m "test: add unit tests for PatientService"
```

### Step 4: Push and Create PR
```bash
# Push to your fork
git push origin feature/my-feature
```

1. Go to GitHub
2. Click "Compare & pull request"
3. Fill PR details:
   - **Title:** Clear, descriptive
   - **Description:** What, why, how
   - **Linked issues:** `Fixes #123`
4. Wait for status checks
5. Respond to feedback
6. Merge when approved

**PR Template:**
```markdown
## Description
Brief description of changes.

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Related Issue
Closes #123

## Changes Made
- Change 1
- Change 2
- Change 3

## Testing
How to test these changes:
1. Step 1
2. Step 2

## Screenshots (if applicable)

## Checklist
- [ ] My code follows the code style
- [ ] I have performed a self-review
- [ ] I have commented complex logic
- [ ] I have added tests
- [ ] New tests pass
- [ ] Existing tests still pass
- [ ] Code coverage maintained (70%+)
```

---

## Workflow: Documentation Contributions

Documentation updates are easier and don't require status checks:

### Step 1: Create Docs Branch
```bash
git checkout main
git pull upstream main
git checkout -b docs/topic-name
```

### Step 2: Edit Documentation
```bash
# Edit .md files in docs/
# Follow Markdown style guide
# Verify links work
```

### Step 3: Commit and Push
```bash
git add .
git commit -m "docs: add database setup guide"
git push origin docs/topic-name
```

### Step 4: Create PR
- No status checks needed!
- Can merge quickly
- Base: `main`

---

## Code Style Guide

### C# Conventions
- **Namespaces:** `PTRP.App.Services`, `PTRP.App.ViewModels`
- **Classes:** PascalCase (e.g., `PatientService`)
- **Methods:** PascalCase (e.g., `GetPatientAsync`)
- **Variables:** camelCase (e.g., `patientList`)
- **Constants:** UPPER_CASE (e.g., `MAX_RETRIES`)
- **Private fields:** `_camelCase` (e.g., `_repository`)

### Style Standards
```csharp
// Good
public class PatientService : IPatientService
{
    private readonly IPatientRepository _repository;
    
    public PatientService(IPatientRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }
    
    public async Task<PatientModel> GetByIdAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("ID must be positive", nameof(id));
        
        return await _repository.GetByIdAsync(id);
    }
}

// Bad
public class patientservice  // wrong casing
{
    IPatientRepository repo;  // no underscore
    
    public patientservice(IPatientRepository r)  // wrong param name
    {
        repo = r;  // no null check
    }
}
```

### XAML Conventions
```xaml
<!-- Good: readable, organized -->
<Button Command="{Binding AddPatientCommand}"
        Content="Add Patient"
        Width="150"
        Height="35"
        Background="#28A745"
        Foreground="White"
        Margin="0,0,5,0"/>

<!-- Bad: unclear -->
<Button Cmd="{B AddPatientCommand}" Cnt="Add" W="150" H="35" Bg="#28A745" Fg="#FFF" M="0,0,5,0"/>
```

### Testing Conventions
```csharp
// Good: Arrange-Act-Assert pattern
[Fact]
public async Task AddAsync_WithValidPatient_ShouldSucceed()
{
    // Arrange
    var patient = new PatientModel { FirstName = "John", LastName = "Doe" };
    var mockRepo = new Mock<IPatientRepository>();
    var service = new PatientService(mockRepo.Object);
    
    // Act
    await service.AddAsync(patient);
    
    // Assert
    mockRepo.Verify(x => x.AddAsync(It.IsAny<PatientModel>()), Times.Once);
}
```

---

## Project Structure

```
PTRP/
├── src/
│   ├── PTRP.App/                 # Main WPF application
│   │   ├── Models/
│   │   ├── Services/
│   │   ├── ViewModels/
│   │   ├── Views/
│   │   └── App.xaml.cs
│   ├── PTRP.Models/              # Shared models (future)
│   └── PTRP.ViewModels/          # Shared ViewModels (future)
├── tests/
│   └── PTRP.App.Tests/           # Unit tests
├── docs/
│   ├── ARCHITECTURE.md           # System design
│   ├── GIT-FLOW.md               # Branching model
│   ├── RELEASE-PROCESS.md        # Release steps
│   └── CONTRIBUTING.md           # This file
├── .github/workflows/            # CI/CD pipelines
├── velopack.json                 # Installer config
└── README.md
```

---

## Testing Requirements

### Unit Test Coverage
- **Minimum:** 70% code coverage
- **Ideal:** 80%+ for critical paths
- **Tools:** xUnit, Moq

### Running Tests Locally
```bash
# Run all tests
dotnet test

# Run specific test
dotnet test --filter "PatientServiceTests"

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverageFormat=cobertura
```

### Writing Tests
```csharp
[Fact]
public async Task Example_Test_Name()
{
    // Arrange: Set up test data
    var input = new PatientModel { /* ... */ };
    var mockService = new Mock<IPatientService>();
    
    // Act: Execute the code
    var result = await mockService.Object.AddAsync(input);
    
    // Assert: Verify result
    Assert.NotNull(result);
    mockService.Verify(x => x.AddAsync(It.IsAny<PatientModel>()), Times.Once);
}
```

---

## PR Review Checklist

Before submitting a PR, verify:

- [ ] Code builds without errors
- [ ] All tests pass (`dotnet test`)
- [ ] Code coverage maintained (70%+)
- [ ] No hard-coded values
- [ ] Error handling implemented
- [ ] Comments added for complex logic
- [ ] No dead code
- [ ] PR description is clear
- [ ] Commit messages follow convention
- [ ] No merge conflicts

---

## Common Issues

### Build Fails Locally
```bash
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build
```

### Tests Fail
```bash
# Run specific test with details
dotnet test --filter "TestName" --logger "console;verbosity=detailed"

# Check dependencies
dotnet nuget locals all --clear
dotnet restore
```

### PR has Merge Conflicts
```bash
# Update from main
git fetch upstream
git rebase upstream/main

# Resolve conflicts in editor
# Then:
git add .
git rebase --continue
git push --force-with-lease origin feature/branch-name
```

---

## Need Help?

- **Documentation:** See [docs/](../docs/)
- **Architecture:** See [ARCHITECTURE.md](./ARCHITECTURE.md)
- **Git Flow:** See [GIT-FLOW.md](./GIT-FLOW.md)
- **GitHub Issues:** Ask in comments or create new issue

---

## Recognition

Contributors will be:
- Added to README.md
- Mentioned in release notes
- Thanked in commit messages

---

**Happy contributing! 🚀**
