# Design Patterns in Desktop Terrarium

This document describes the design patterns used throughout the Desktop Terrarium project and their implementation.

---

## 🏛️ Architectural Patterns

### 1. Layered Architecture

**Purpose**: Separation of concerns, maintainability, testability

**Implementation**:
```
Presentation Layer (Terrarium.Desktop)
    ↓ (references)
Logic Layer (Terrarium.Logic)
    ↓ (contains)
Data Layer (Entity state, persistence)
```

**Benefits**:
- Logic layer has no UI dependencies (fully testable)
- Clear boundaries between layers
- Easy to swap presentation layer (e.g., console, web)

**Files**:
- `Terrarium.Desktop/` - Presentation
- `Terrarium.Logic/` - Logic
- `Terrarium.Logic/Persistence/` - Data

---

## 🎯 Creational Patterns

### 2. Factory Pattern

**Purpose**: Centralized object creation with consistent initialization

**Implementation**:
```csharp
// World.cs
public void SpawnRandomHerbivore(string type)
{
    var herbivore = new Herbivore(
        x: Random.NextDouble() * Width,
        y: Random.NextDouble() * Height,
        type: type
    );
    AddHerbivore(herbivore);
}
```

**Benefits**:
- Consistent entity creation
- Encapsulates creation logic
- Easy to extend with new entity types

**Files**:
- `Terrarium.Logic/Simulation/World.cs` - `SpawnRandom*()` methods

---

### 3. Builder Pattern (Implicit)

**Purpose**: Step-by-step object construction

**Implementation**:
```csharp
// SaveManager.cs - Building save data
var saveData = new WorldSaveData
{
    SchemaVersion = WorldSaveData.CurrentSchemaVersion,
    Width = world.Width,
    Height = world.Height,
    Plants = world.Plants.Select(p => EntityToData(p)).ToList(),
    // ... more properties
};
```

**Benefits**:
- Clear object construction
- Optional parameters via defaults
- Fluent interface potential

---

## 🔄 Behavioral Patterns

### 4. Strategy Pattern

**Purpose**: Interchangeable algorithms for different behaviors

**Implementation**:
```csharp
// Different movement strategies
public abstract class Creature : LivingEntity
{
    public virtual void Move(double deltaTime) { }
}

public class Herbivore : Creature
{
    // Herbivore-specific movement (wandering)
    public override void Move(double deltaTime) { }
}

public class Carnivore : Creature
{
    // Carnivore-specific movement (hunting)
    public override void Move(double deltaTime) { }
}
```

**Benefits**:
- Different behaviors for different entity types
- Easy to add new behaviors
- Polymorphism enables runtime behavior selection

**Files**:
- `Terrarium.Logic/Entities/Creature.cs` - Base movement
- `Terrarium.Logic/Entities/Herbivore.cs` - Wandering strategy
- `Terrarium.Logic/Entities/Carnivore.cs` - Hunting strategy

---

### 5. Observer Pattern

**Purpose**: Event-driven notifications and updates

**Implementation**:
```csharp
// EventSystem.cs
public class EventSystem
{
    public event EventHandler<EntityBornEventArgs>? EntityBorn;
    public event EventHandler<EntityDiedEventArgs>? EntityDied;
    
    public void NotifyEntityBorn(LivingEntity entity)
    {
        EntityBorn?.Invoke(this, new EntityBornEventArgs(entity));
    }
}

// Usage in SimulationEngine
_reproductionManager.OnEntityBorn += (sender, args) =>
{
    _eventSystem.NotifyEntityBorn(args.Entity);
};
```

**Benefits**:
- Loose coupling between components
- Multiple observers can react to events
- Easy to add new event types

**Files**:
- `Terrarium.Logic/Simulation/EventSystem.cs`
- `Terrarium.Logic/Simulation/ReproductionManager.cs`

---

### 6. Template Method Pattern

**Purpose**: Define algorithm skeleton, let subclasses override steps

**Implementation**:
```csharp
// LivingEntity.cs
public virtual void Update(double deltaTime)
{
    Age += deltaTime;  // Common step
    
    // Subclasses override for specific behavior
    UpdateSpecific(deltaTime);
    
    if (Health <= 0)  // Common step
    {
        IsAlive = false;
    }
}

// Plant.cs
public override void Update(double deltaTime)
{
    base.Update(deltaTime);  // Call template
    Grow(deltaTime);  // Plant-specific step
}
```

**Benefits**:
- Code reuse
- Consistent update flow
- Flexible extension points

**Files**:
- `Terrarium.Logic/Entities/LivingEntity.cs` - Template
- `Terrarium.Logic/Entities/Plant.cs` - Concrete implementation
- `Terrarium.Logic/Entities/Creature.cs` - Concrete implementation

---

## 🏗️ Structural Patterns

### 7. Composition Pattern

**Purpose**: Build complex objects from simpler parts

**Implementation**:
```csharp
// SimulationEngine HAS-A World, not IS-A World
public class SimulationEngine
{
    private readonly World _world;  // Composition
    private readonly FoodManager _foodManager;  // Composition
    private readonly MovementCalculator _movementCalculator;  // Composition
    // ... more managers
}
```

**Benefits**:
- Avoids deep inheritance hierarchies
- Flexible component arrangement
- Easy to test individual components

**Files**:
- `Terrarium.Logic/Simulation/SimulationEngine.cs` - Composes managers

---

### 8. Facade Pattern

**Purpose**: Simplified interface to complex subsystem

**Implementation**:
```csharp
// SimulationEngine provides simple interface
public class SimulationEngine
{
    public void Update(double deltaTime)  // Simple interface
    {
        // Complex orchestration hidden inside
        _dayNightCycle.Update(deltaTime);
        _foodManager.ProcessFeeding(_world);
        _movementCalculator.UpdateMovement(_world, deltaTime);
        // ... more complex operations
    }
}
```

**Benefits**:
- Simple API for complex operations
- Hides implementation details
- Easy to use from presentation layer

**Files**:
- `Terrarium.Logic/Simulation/SimulationEngine.cs` - Facade for simulation

---

## 🔌 Integration Patterns

### 9. Dependency Injection (Manual)

**Purpose**: Loose coupling, testability

**Implementation**:
```csharp
// SimulationEngine constructor injects dependencies
public SimulationEngine(double worldWidth, double worldHeight)
{
    _world = new World(worldWidth, worldHeight);
    _movementCalculator = new MovementCalculator(_world);
    _collisionDetector = new CollisionDetector();
    _foodManager = new FoodManager(_world);
    // ... more dependencies
}
```

**Benefits**:
- Easy to mock for testing
- Clear dependencies
- Flexible component replacement

**Files**:
- `Terrarium.Logic/Simulation/SimulationEngine.cs` - Constructor injection

---

### 10. Repository Pattern (Implicit)

**Purpose**: Abstraction of data access

**Implementation**:
```csharp
// World acts as repository for entities
public class World
{
    public List<Plant> Plants { get; } = new();
    public List<Herbivore> Herbivores { get; } = new();
    public List<Carnivore> Carnivores { get; } = new();
    
    public void AddPlant(Plant plant) { }
    public void RemovePlant(Plant plant) { }
    // ... more repository methods
}
```

**Benefits**:
- Centralized entity management
- Easy to swap storage mechanism
- Clear data access interface

**Files**:
- `Terrarium.Logic/Simulation/World.cs` - Entity repository

---

## 🎨 UI Patterns

### 11. Model-View Separation

**Purpose**: Separate UI from business logic

**Implementation**:
```
View (MainWindow.xaml) → ViewModel (MainWindow.xaml.cs) → Model (SimulationEngine)
```

**Benefits**:
- UI changes don't affect logic
- Logic changes don't affect UI
- Easy to test logic independently

**Files**:
- `Terrarium.Desktop/MainWindow.xaml` - View
- `Terrarium.Desktop/MainWindow.xaml.cs` - ViewModel
- `Terrarium.Logic/Simulation/SimulationEngine.cs` - Model

---

### 12. Component Pattern

**Purpose**: Modular UI components

**Implementation**:
```csharp
// Rendering components
public class Renderer { }
public class ParticleSystem { }
public class NotificationManager { }
public class MiniMap { }
// ... more components
```

**Benefits**:
- Reusable UI components
- Easy to add/remove features
- Clear component boundaries

**Files**:
- `Terrarium.Desktop/Rendering/` - UI components

---

## 🔄 State Management Patterns

### 13. State Pattern (Implicit)

**Purpose**: Entity behavior changes based on state

**Implementation**:
```csharp
// Creature behavior based on hunger state
public void Update(double deltaTime)
{
    if (Hunger > StarvationThreshold)
    {
        // Starving state behavior
        TakeDamage(StarvationDamageRate * deltaTime);
    }
    else if (Hunger < WellFedThreshold)
    {
        // Well-fed state behavior
        // ... different movement, etc.
    }
}
```

**Benefits**:
- Behavior changes with state
- Clear state transitions
- Easy to add new states

---

## 📊 Pattern Summary

| Pattern | Purpose | Implementation |
|---------|---------|----------------|
| **Layered Architecture** | Separation of concerns | Three distinct layers |
| **Factory** | Object creation | `World.SpawnRandom*()` |
| **Strategy** | Interchangeable algorithms | Different creature behaviors |
| **Observer** | Event notifications | `EventSystem` |
| **Template Method** | Algorithm skeleton | `LivingEntity.Update()` |
| **Composition** | Build from parts | `SimulationEngine` composes managers |
| **Facade** | Simple interface | `SimulationEngine.Update()` |
| **Dependency Injection** | Loose coupling | Constructor injection |
| **Repository** | Data access | `World` as entity repository |
| **Model-View** | UI separation | WPF MVVM-like structure |
| **Component** | Modular UI | Rendering components |

---

## 🎯 When to Use Patterns

### ✅ Good Pattern Usage

- **Layered Architecture**: Always (project requirement)
- **Factory**: When creating entities with complex initialization
- **Strategy**: When different entity types need different behaviors
- **Observer**: When components need to react to events
- **Composition**: When building complex objects from simpler parts

### ⚠️ Pattern Considerations

- **Don't over-engineer**: Simple code is better than complex patterns
- **Use patterns where they add value**: Not every problem needs a pattern
- **Keep it maintainable**: Patterns should make code easier to understand

---

## 📚 References

- **Gang of Four**: Design Patterns book
- **.NET Best Practices**: Microsoft documentation
- **Clean Architecture**: Robert C. Martin principles

---

*Last updated: January 2026*  
*Maintained by: Project maintainers*

