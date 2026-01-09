# Cloud Agent Task List - Desktop Terrarium Exam Compliance

## Context
This is a Windows WPF application that requires Windows/.NET environment for building and testing. The codebase is already well-structured, but needs final verification and documentation updates for exam submission.

## Environment Requirements
- Windows OS
- .NET 8.0 SDK
- Visual Studio 2022 (recommended) or Visual Studio Code with C# extensions

## Primary Tasks

### Task 1: Build Verification ✅
**Priority: HIGH**

Run these commands and document the results:
```bash
cd /path/to/The-Desktop-Terrarium
dotnet clean
dotnet build -c Release
```

**Expected Result**: 
- Build succeeds
- 0 errors
- 0 warnings

**Action**: Document results in VERIFICATION_CHECKLIST.md section 1

---

### Task 2: Test Verification ✅
**Priority: HIGH**

Run these commands:
```bash
dotnet test -c Release --verbosity normal
```

**Expected Result**: 
- All 111 tests pass
- 0 failures
- Test execution completes in < 2 seconds

**Action**: Update VERIFICATION_CHECKLIST.md section 2 with actual test count and results

---

### Task 3: Application Runtime Verification ✅
**Priority: HIGH**

**Manual Steps**:
1. Build the application: `dotnet build -c Release`
2. Run the application: `dotnet run --project Terrarium.Desktop -c Release`
3. Verify the window appears at bottom of screen
4. Test mouse interaction:
   - Hover over plants (should shake)
   - Click on creatures (should get fed)
5. Test keyboard shortcuts:
   - Press 'P' to spawn plant
   - Press 'H' to spawn herbivore
   - Press 'C' to spawn carnivore
   - Press 'Space' to pause/resume
6. Let application run for 30 seconds
7. Press Ctrl+S to save
8. Close and reopen application
9. Press Ctrl+L to load (entities should reappear)

**Action**: Check boxes in VERIFICATION_CHECKLIST.md sections for:
- Mouse Interaction
- System Integration
- Save/Load
- Application runs correctly

---

### Task 4: Code Quality Review 🔍
**Priority: MEDIUM**

Review the following for exam compliance:

#### 4.1 Naming Conventions
```bash
# Check that naming follows .NET conventions
# Manual review of key files
```

Files to review:
- `Terrarium.Logic/Entities/*.cs` (PascalCase for classes/methods)
- `Terrarium.Logic/Simulation/*.cs` (camelCase for variables)
- All files should use `_camelCase` for private fields

**Action**: Mark checked in VERIFICATION_CHECKLIST.md section "Naming Conventions"

#### 4.2 No Magic Numbers
```bash
# Search for potential magic numbers
grep -n "if.*[0-9]\{2,\}" Terrarium.Logic/**/*.cs
grep -n "return.*[0-9]\{2,\}" Terrarium.Logic/**/*.cs
```

**Known status**: Most constants already extracted. Check:
- `Terrarium.Desktop/Rendering/Renderer.cs` (already has extensive constants)
- Other rendering files

**Action**: If any magic numbers found, extract to named constants

#### 4.3 Method Length
Review methods for length (should be < 50 lines typically):
- Most methods already concise
- `MainWindow.xaml.cs` was split into partial classes to address this
- Check any remaining long methods

**Action**: Mark checked in VERIFICATION_CHECKLIST.md

#### 4.4 No Dead Code
```bash
# Check for commented-out code
grep -rn "^[[:space:]]*//.*{" Terrarium.Logic/**/*.cs
grep -rn "^[[:space:]]*//" Terrarium.Desktop/**/*.cs | grep -i "old\|unused\|todo\|fixme"
```

**Action**: Remove any found dead code or mark checked if none found

---

### Task 5: Architecture Verification ✅
**Priority: MEDIUM**

Verify layered architecture:

```bash
# Verify Terrarium.Logic has NO WPF references
grep -i "presentationframework\|presentationcore\|windowsbase" Terrarium.Logic/Terrarium.Logic.csproj
# Expected: No matches

# Verify Terrarium.Desktop DOES reference Logic
grep "Terrarium.Logic" Terrarium.Desktop/Terrarium.Desktop.csproj
# Expected: ProjectReference found
```

**Action**: Mark architectural items as checked in VERIFICATION_CHECKLIST.md

---

### Task 6: Documentation Updates 📝
**Priority: MEDIUM**

Update documentation to reflect current state:

1. **Update test counts** in:
   - `README.md` (currently says 111 tests)
   - `PROJECT_SUMMARY.md` (verify all statistics)
   - `EXAM_COMPLIANCE.md` (update test count if changed)

2. **Update VERIFICATION_CHECKLIST.md**:
   - Mark all completed items with `[x]`
   - Update "Current Status" section at bottom
   - Update "Last Verified" date

3. **Verify commands work**:
   ```bash
   # From VERIFICATION_CHECKLIST.md
   dotnet build -c Release
   dotnet test -c Release
   dotnet publish "Terrarium.Desktop\Terrarium.Desktop.csproj" -c Release -r win-x64 --self-contained false -o "publish-integrity\win-x64"
   ```

**Action**: Update all documentation files with accurate information

---

### Task 7: Final Submission Checklist ✅
**Priority: HIGH**

Before marking as complete:

```bash
# Clean and rebuild from scratch
dotnet clean
dotnet build --configuration Release

# Run all tests
dotnet test --configuration Release

# Check git status
git status

# Verify .gitignore is working
ls bin/ obj/  # Should not exist or should be empty

# Create final commit
git add .
git commit -m "Final exam compliance verification complete"
```

**Checklist**:
- [ ] All code compiles with zero warnings
- [ ] All tests pass
- [ ] Application runs correctly
- [ ] Documentation is accurate
- [ ] VERIFICATION_CHECKLIST.md is complete
- [ ] No uncommitted changes
- [ ] .gitignore excludes build artifacts

---

### Task 8: Extract Remaining Magic Numbers (If Any) 🎯
**Priority: LOW** (Most already done)

If any magic numbers are found in Task 4.2, extract them using this pattern:

```csharp
// Before:
if (value > 100)
{
    // ...
}

// After:
private const double MaxValue = 100.0;

if (value > MaxValue)
{
    // ...
}
```

**Files most likely to need work**:
- Rendering-related files in `Terrarium.Desktop/Rendering/`
- Animation/visual effect files

**Action**: Create minimal changes to extract constants, maintain visual appearance

---

## Success Criteria

### Must Have ✅
- [x] Project builds with 0 errors, 0 warnings
- [x] All tests pass (currently 111 tests)
- [ ] Application runs without crashes
- [ ] VERIFICATION_CHECKLIST.md is 90%+ complete
- [ ] Documentation matches reality

### Should Have ✅
- [ ] No magic numbers in logic layer
- [ ] All checklist items verified
- [ ] Manual testing completed
- [ ] Publish artifacts generated

### Nice to Have ⭐
- [ ] Screenshots of running application
- [ ] Performance metrics documented
- [ ] Code coverage report generated

---

## Deliverables

1. **Updated VERIFICATION_CHECKLIST.md** with all verified items marked
2. **Build/Test Output** showing success
3. **Any code changes** needed for magic number extraction
4. **Updated documentation** reflecting current state
5. **Git commit** with all changes

---

## Notes for Cloud Agent

- This codebase is already high-quality and well-structured
- Most work is **verification and documentation**, not coding
- The goal is **exam compliance**, not new features
- Maintain existing visual appearance (don't change constants' values)
- Focus on checking boxes that represent already-complete work
- Be thorough but recognize what's already done

---

## Estimated Time

- Build & Test Verification: 5 minutes
- Manual Application Testing: 10 minutes
- Code Quality Review: 15 minutes
- Documentation Updates: 15 minutes
- Final Verification: 5 minutes

**Total: ~50 minutes**

---

Generated: 2026-01-09
Status: READY FOR CLOUD AGENT PROCESSING
