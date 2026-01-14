# Development Guide

## Setup Instructions

### Prerequisites
1. Visual Studio 2022 or later
2. .NET 8.0 SDK
3. Windows 10/11

### First-Time Setup
```bash
# Clone repository
git clone https://github.com/Hostilian/The-Desktop-Terrarium.git
cd The-Desktop-Terrarium

# Open in Visual Studio
start DesktopTerrarium.sln

# Restore packages
dotnet restore

# Build solution
dotnet build

# Run tests
dotnet test
```

## Development Workflow

### Week 1: Logic Layer ✅
- [x] Create entity hierarchy
- [x] Implement interfaces
- [x] Add unit tests
- [x] Test movement and growth

### Week 2: Presentation Layer ✅
- [x] Create WPF window
- [x] Implement renderer
- [x] Add mouse interaction
- [x] Position at screen bottom

### Week 3: Advanced Features ✅
- [x] Add carnivores and hunting
- [x] Implement save/load
- [x] Add system monitoring
- [x] Create animation system

### Week 4: Polish ✅
- [x] Add XML documentation
- [x] Remove dead code
- [x] Format all files
- [x] Write comprehensive README

## Code Organization

### Naming Conventions
```csharp
// Classes, Methods, Properties
public class SimulationEngine { }
public void CalculateMovement() { }
public int Health { get; set; }

// Private fields
private double _health;
private int _entityCount;

// Constants
private const double MaxSpeed = 100.0;
private const int DefaultPopulation = 10;

// Local variables
double currentSpeed = 50.0;
int frameCount = 0;
```

### File Organization
- One class per file
- File name matches class name
- Organize by feature (Entities/, Simulation/, Rendering/)

## Testing Guidelines

### Unit Test Structure
```csharp
[TestMethod]
public void MethodName_Scenario_ExpectedBehavior()
{
    // Arrange
    var entity = new Herbivore(100, 100);
    
    // Act
    entity.Update(1.0);
    
    // Assert
    Assert.IsTrue(entity.Hunger > 0);
}
```

### What to Test
- ✅ Entity behavior (growth, movement, eating)
- ✅ Manager logic (collision, movement, food)
- ✅ Boundary conditions
- ✅ State changes
- ❌ Don't test UI directly
- ❌ Don't test framework features

## Architecture Principles

### Layered Architecture Rules
1. **Logic Layer** NEVER references Presentation
2. **Presentation Layer** depends on Logic
3. **Test Layer** only tests Logic

### Single Responsibility Examples
❌ **Bad - God Object**:
```csharp
class GameManager {
    void Update() { }
    void Render() { }
    void CheckCollisions() { }
    void SpawnFood() { }
    void SaveGame() { }
}
```

✅ **Good - Separated Concerns**:
```csharp
class SimulationEngine { void Update() { } }
class Renderer { void Render() { } }
class CollisionDetector { void CheckCollisions() { } }
class FoodManager { void SpawnFood() { } }
class SaveManager { void SaveGame() { } }
```

## Common Issues

### Issue: Can't click window
**Cause**: Window not focused
**Fix**: Set `Focusable="True"` in XAML

### Issue: Entities disappear
**Cause**: Exceeding world bounds
**Fix**: Check `MovementCalculator.EnforceBoundaries()`

### Issue: Tests fail randomly
**Cause**: Time-dependent logic
**Fix**: Use fixed deltaTime in tests

## Performance Optimization

### Rendering (60 FPS)
- Use object pooling for visuals
- Batch canvas updates
- Implement culling for off-screen entities

### Logic (5 Hz)
- Fixed timestep accumulator
- Spatial partitioning for collision
- Lazy evaluation where possible

## Debugging Tips

### Enable Debug Output
```csharp
#if DEBUG
    Console.WriteLine($"Entity {Id} at ({X}, {Y})");
#endif
```

### Visual Studio Shortcuts
- F5: Start debugging
- F10: Step over
- F11: Step into
- Ctrl+K, Ctrl+D: Format document

## Git Workflow

```bash
# Create feature branch
git checkout -b feature/new-biome

# Make changes and commit
git add .
git commit -m "Add desert biome support"

# Push to remote
git push origin feature/new-biome

# Create pull request on GitHub
```

## Documentation Standards

### XML Documentation Template
```csharp
/// <summary>
/// Brief description of what the method does.
/// </summary>
/// <param name="paramName">Description of parameter</param>
/// <returns>Description of return value</returns>
/// <exception cref="ExceptionType">When this is thrown</exception>
public int MethodName(string paramName)
{
    // Implementation
}
```

### README Sections
- Overview
- Features
- Architecture
- Getting Started
- Controls
- Testing
- Troubleshooting

## Submission Checklist

Before submitting:
- [ ] All tests pass
- [ ] No compiler warnings
- [ ] Code is formatted (Ctrl+K, Ctrl+D)
- [ ] XML documentation on public members
- [ ] README is complete
- [ ] No magic numbers
- [ ] Named constants used
- [ ] Single responsibility followed
- [ ] Layered architecture maintained

## Resources

- [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [WPF Tutorial](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/)
- [MSTest Documentation](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-mstest)
- [SOLID Principles](https://en.wikipedia.org/wiki/SOLID)

---

## 🚀 CI/CD Pipeline

Desktop Terrarium uses GitHub Actions for continuous integration and deployment. The pipeline is defined in `.github/workflows/ci-cd.yml`.

### Pipeline Architecture

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                           GITHUB ACTIONS CI/CD PIPELINE                          │
├─────────────────────────────────────────────────────────────────────────────────┤
│                                                                                  │
│   ┌──────────────────────┐     ┌──────────────────────┐                         │
│   │   build-and-test     │     │   logic-tests-linux  │                         │
│   │   (windows-latest)   │     │   (ubuntu-latest)    │                         │
│   │                      │     │                      │                         │
│   │ • Restore NuGet      │     │ • Cross-platform     │                         │
│   │ • Build Release      │     │   logic verification │                         │
│   │ • Run all tests      │     │ • Coverage report    │                         │
│   │ • Coverage (81%+)    │     │                      │                         │
│   │ • Coverage gate 70%  │     │                      │                         │
│   │ • HTML report        │     │                      │                         │
│   └──────────┬───────────┘     └──────────────────────┘                         │
│              │                                                                   │
│              ▼                                                                   │
│   ┌──────────────────────┐     ┌──────────────────────┐                         │
│   │    code-quality      │     │   docs-validation    │                         │
│   │   (windows-latest)   │     │   (ubuntu-latest)    │                         │
│   │                      │     │                      │                         │
│   │ • Format check       │     │ • Validate links     │                         │
│   │ • Vulnerability scan │     │ • Check assets       │                         │
│   │ • Deprecated pkgs    │     │                      │                         │
│   │ • Outdated packages  │     │                      │                         │
│   └──────────┬───────────┘     └──────────┬───────────┘                         │
│              │                            │                                      │
│              ▼                            ▼                                      │
│   ┌──────────────────────┐     ┌──────────────────────┐                         │
│   │  publish-integrity   │     │    deploy-docs       │                         │
│   │   (windows-latest)   │     │   (ubuntu-latest)    │                         │
│   │                      │     │                      │                         │
│   │ • Publish win-x64    │     │ • Upload docs/       │                         │
│   │ • Publish win-arm64  │     │ • Deploy Pages       │                         │
│   │ • Verify manifests   │     │                      │                         │
│   │ • Upload artifacts   │     │ (main branch only)   │                         │
│   └──────────┬───────────┘     └──────────────────────┘                         │
│              │                                                                   │
│              ▼ (only on version tags: v*)                                       │
│   ┌──────────────────────┐                                                      │
│   │      publish         │                                                      │
│   │   (windows-latest)   │                                                      │
│   │                      │                                                      │
│   │ • Self-contained exe │                                                      │
│   │ • Single file        │                                                      │
│   │ • Create ZIP         │                                                      │
│   │ • GitHub Release     │                                                      │
│   └──────────────────────┘                                                      │
│                                                                                  │
└─────────────────────────────────────────────────────────────────────────────────┘
```

### Pipeline Jobs

| Job | Runs On | Trigger | Purpose |
|-----|---------|---------|---------|
| `build-and-test` | windows-latest | All pushes/PRs | Core build, tests, coverage |
| `code-quality` | windows-latest | After build | Format, vulnerability, deprecation checks |
| `logic-tests-linux` | ubuntu-latest | All pushes/PRs | Verify cross-platform logic layer |
| `docs-validation` | ubuntu-latest | All pushes/PRs | Validate documentation links |
| `publish-integrity` | windows-latest | All pushes/PRs | Verify publish manifests unchanged |
| `publish` | windows-latest | Version tags (v*) | Create GitHub release |
| `deploy-docs` | ubuntu-latest | Main branch | Deploy website to GitHub Pages |

### Key Features

#### Coverage Gate
The pipeline enforces a **minimum 70% line coverage** threshold:
```bash
python scripts/summarize_coverage.py TestResults --min-line 70
```

#### Warnings as Errors
All builds use `-p:TreatWarningsAsErrors=true` to ensure clean code.

#### Format Check
Code formatting is verified with:
```bash
dotnet format --verify-no-changes
```

#### Vulnerability Scanning
NuGet packages are scanned for security issues:
```bash
dotnet list package --vulnerable --include-transitive
```

### Running Locally

You can simulate the CI pipeline locally:

```bash
# Build and test
dotnet build DesktopTerrarium.sln -c Release -p:TreatWarningsAsErrors=true
dotnet test DesktopTerrarium.sln -c Release /p:CollectCoverage=true

# Format check
dotnet format DesktopTerrarium.sln --verify-no-changes

# Vulnerability scan
dotnet list DesktopTerrarium.sln package --vulnerable

# Publish
dotnet publish Terrarium.Desktop/Terrarium.Desktop.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

### Creating a Release

1. Ensure all tests pass on `main` branch
2. Create a version tag:
   ```bash
   git tag v1.0.0
   git push origin v1.0.0
   ```
3. The `publish` job automatically:
   - Builds a self-contained single-file executable
   - Creates a ZIP archive
   - Creates a GitHub Release with the ZIP attached
   - Generates release notes from commit history

### Monitoring Pipeline

- **Build status badge**: Shows in README.md
- **Check GitHub Actions tab**: See all workflow runs
- **Artifacts**: Download test results, coverage reports, and builds

---

Happy Coding! 🚀
