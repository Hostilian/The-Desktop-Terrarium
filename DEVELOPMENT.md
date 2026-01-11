# Development Guide

## Setup Instructions

### Prerequisites
1. Visual Studio 2022 or later
2. .NET 10.0 SDK
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

Happy Coding! 🚀
