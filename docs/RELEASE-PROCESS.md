# Release Process - PTRP

## Overview

This document describes the process for creating and publishing releases of PTRP.

**Frequency:** When features are stable and ready for testing/production  
**Versioning:** Semantic Versioning (e.g., 0.1.0, 0.2.0, 1.0.0)  
**Channels:** Stable releases + Pre-release (alpha/beta)  

---

## Release Types

### 1. Feature Release (Minor Version)
**When:** Multiple features completed and tested  
**Example:** 0.1.0 → 0.2.0  
**Timeline:** ~1-2 weeks of development

```
0.2.0 includes:
  ✓ Patient database integration
  ✓ CRUD operations
  ✓ Search functionality
  ✓ Basic validation
```

---

### 2. Bugfix Release (Patch Version)
**When:** Critical bug fixes need immediate release  
**Example:** 0.1.0 → 0.1.1  
**Timeline:** Same day or next day

```
0.1.1 includes:
  ✓ Fixed patient search crash
  ✓ Fixed database connection issue
```

---

### 3. Major Release
**When:** Significant changes, API changes, or production milestone  
**Example:** 0.X.X → 1.0.0  
**Requirement:** Team discussion + planning

---

## Pre-Release Process (Before Versioning)

### Step 1: Ensure Main/Develop are Clean
```bash
# Switch to main
git checkout main
git pull origin main

# Should have no uncommitted changes
git status  # Should show "nothing to commit, working tree clean"
```

### Step 2: Create Release Branch
```bash
# Create release branch from main (for patch) or develop (for feature)
git checkout develop
git pull origin develop

# Create release branch
git checkout -b release/0.2.0
```

### Step 3: Update Version in Code
Edit `src/PTRP.App/PTRP.App.csproj`:

```xml
<PropertyGroup>
    <Version>0.2.0</Version>
    <AssemblyVersion>0.2.0</AssemblyVersion>
    <FileVersion>0.2.0</FileVersion>
    <InformationalVersion>0.2.0</InformationalVersion>
</PropertyGroup>
```

### Step 4: Update CHANGELOG
Create/update `CHANGELOG.md`:

```markdown
## [0.2.0] - 2026-01-29

### Added
- Patient database integration with SQLite
- Patient CRUD operations (Create, Read, Update, Delete)
- Patient search functionality
- Input validation and error handling
- Unit tests for PatientService (70% coverage)

### Fixed
- Patient search performance issue

### Changed
- Updated to .NET 10 (from WinUI 3 WPF)

### Dependencies
- Added EF Core 9.0
- Added MVVM Toolkit 8.2
```

### Step 5: Commit Changes
```bash
git add .
git commit -m "chore: release v0.2.0

- Update version in CSPROJ
- Update CHANGELOG.md
- Ready for release"

git push -u origin release/0.2.0
```

---

## GitHub Release Process (Maintainer Only)

### Step 1: Create Pull Request to Main
1. Go to GitHub
2. Create Pull Request:
   - **Base:** `main`
   - **Compare:** `release/0.2.0`
   - **Title:** `Release v0.2.0`
   - **Description:** Copy from CHANGELOG

3. Wait for status checks (build + test) to pass
4. Get approval if required
5. **Merge** Pull Request (Squash or Create merge commit)

### Step 2: Create Pull Request Back to Develop
1. Create another Pull Request:
   - **Base:** `develop`
   - **Compare:** `release/0.2.0`
   - **Title:** `Merge release/0.2.0 back to develop`
   - **Description:** Ensures develop gets version bump

2. Merge to develop

### Step 3: Tag Release on Main
```bash
# Switch to main and pull latest
git checkout main
git pull origin main

# Create annotated tag
git tag -a v0.2.0 -m "Release v0.2.0

See CHANGELOG.md for details.

Features:
- Patient database integration
- CRUD operations
- Search functionality

Breaking changes: None"

# Push tag to GitHub
git push origin v0.2.0

# Verify tag
git tag -l
git show v0.2.0
```

### Step 4: GitHub Actions Release Workflow
When you push the tag `v0.2.0`:

1. **GitHub Actions Triggered:** `release.yml` workflow starts
2. **Build & Publish:** 
   - Builds release configuration
   - Publishes to self-contained executable
   - Creates Windows installer via Velopack
3. **Create GitHub Release:**
   - Automatically creates release on GitHub
   - Attaches `.msi` and `.nupkg` files
   - Uses tag message as release notes
4. **Publish to GitHub Releases Page:**
   - Available at: https://github.com/artcava/PTRP/releases

---

## Post-Release Verification

### 1. Check GitHub Release
- Go to [Releases](https://github.com/artcava/PTRP/releases)
- Verify v0.2.0 is listed
- Verify `.msi` and `.nupkg` files are attached
- Verify release notes are correct

### 2. Test Installation
```bash
# Download .msi from release
# Run installer on clean Windows machine
# Verify app launches
# Verify version shows 0.2.0
```

### 3. Test Auto-Update
- Velopack automatically enables auto-update
- Next version release (0.2.1) will auto-update existing installations

### 4. Announce Release
- Post to team/stakeholders
- Update documentation if needed
- Link to release notes

---

## Command Reference

### Create Release Branch
```bash
git checkout develop
git pull origin develop
git checkout -b release/0.2.0
```

### Push Release Branch
```bash
git add .
git commit -m "chore: release v0.2.0"
git push -u origin release/0.2.0
```

### Create Tag
```bash
git tag -a v0.2.0 -m "Release v0.2.0"
git push origin v0.2.0
```

### List Tags
```bash
git tag -l
git show v0.2.0  # Show tag details
```

### Delete Tag (if needed)
```bash
# Delete locally
git tag -d v0.2.0

# Delete remote
git push origin --delete v0.2.0
```

---

## Troubleshooting

### Release workflow failed to create installer
**Symptom:** Tag pushed but no release created  
**Solution:**
1. Check GitHub Actions tab for error logs
2. Verify `velopack.json` is valid
3. Verify `.msi` file was generated locally:
   ```bash
   dotnet tool install -g Velopack.Cmd
   vpk pack --packId PTRP --packVersion 0.2.0 --packDir ./publish
   ```
4. Check `Releases/` directory for outputs

### Status checks failing on PR
**Symptom:** Can't merge release PR due to failed build/test  
**Solution:**
1. Fix failing tests locally
2. Commit and push to release branch
3. PR automatically updates
4. Status checks re-run

### Accidentally pushed to wrong branch
**Solution:**
```bash
# Find the commit SHA
git log --oneline

# Revert on wrong branch
git revert <commit-sha>
git push origin branch-name

# Cherry-pick on correct branch
git checkout correct-branch
git cherry-pick <commit-sha>
git push
```

---

## Release Checklist

- [ ] Features merged to `develop`
- [ ] All tests passing locally
- [ ] Code reviewed and approved
- [ ] Create `release/X.X.X` branch
- [ ] Update version in CSPROJ
- [ ] Update CHANGELOG.md
- [ ] Commit and push release branch
- [ ] Create PR to `main`
- [ ] Status checks pass
- [ ] Merge PR to `main`
- [ ] Create PR back to `develop`
- [ ] Merge back to `develop`
- [ ] Create and push tag `vX.X.X`
- [ ] Verify GitHub Release created
- [ ] Download and test .msi installer
- [ ] Announce release

---

## Schedule

**Target Release Cadence:**
- Feature releases: Every 2 weeks (if features ready)
- Bugfix releases: As needed (ASAP)
- Major releases: Quarterly or when milestone reached

**Current Schedule:**
- v0.1.0: Initial MVP (Dec 2025)
- v0.2.0: Database + CRUD (Jan 2026)
- v0.3.0: Project management (Feb 2026)
- v1.0.0: Production ready (Mar 2026)

---

For questions, see [GIT-FLOW.md](./GIT-FLOW.md) or [CONTRIBUTING.md](./CONTRIBUTING.md).
