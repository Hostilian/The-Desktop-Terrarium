# Desktop Terrarium - Technical Specification

## 📋 Table of Contents

1. [Overview](#overview)
2. [Architecture](#architecture)
3. [Data Model](#data-model)
4. [Simulation Rules](#simulation-rules)
5. [User Interface](#user-interface)
6. [Persistence](#persistence)
7. [Performance Requirements](#performance-requirements)
8. [Extensibility Points](#extensibility-points)

---

## 🌱 Overview

### Project Purpose

Desktop Terrarium is a desktop ecosystem simulator that runs as a transparent overlay at the bottom of the user's screen. It simulates a living ecosystem with plants, herbivores, and carnivores that interact with each other and respond to user input.

### Core Requirements

- **Platform**: Windows (x64, ARM64)
- **Framework**: .NET 8.0, WPF
- **Architecture**: Layered (Presentation → Logic → Data)
- **Testing**: Unit tests with 70%+ coverage
- **Performance**: 60 FPS rendering, 5 Hz logic updates

---

## 🏛️ Architecture

### Layered Architecture

```
┌─────────────────────────────────────┐
│  Presentation Layer (WPF)          │
│  Terrarium.Desktop                  │
│  - MainWindow                       │
│  - Renderer                         │
│  - UI Components                    │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│  Logic Layer (.NET Standard)       │
│  Terrarium.Logic                     │
│  - SimulationEngine                 │
│  - Entities                         │
│  - Managers                         │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│  Data Layer                         │
│  - Entity State                     │
│  - Save/Load (JSON)                │
└─────────────────────────────────────┘
```

### Key Principles

1. **Separation of Concerns**: Logic layer has no UI dependencies
2. **Single Responsibility**: Each class has one clear purpose
3. **Dependency Injection**: Managers injected into SimulationEngine
4. **Testability**: Logic layer fully unit-testable

---

## 📊 Data Model

### Entity Hierarchy

```
WorldEntity (base)
├── X, Y (position)
├── Id (unique identifier)
└── DistanceTo() (utility method)

LivingEntity (inherits WorldEntity)
├── Health (0-100)
├── Age (seconds)
├── IsAlive (boolean)
└── TakeDamage() (behavior)

Plant (inherits LivingEntity)
├── Size (5-30 pixels)
├── GrowthRate (pixels/second)
├── WaterLevel (0-100)
└── Grow() (behavior)

Creature (inherits LivingEntity)
├── Speed (pixels/second)
├── Hunger (0-100)
├── VelocityX, VelocityY
└── Move(), Feed() (behavior)

Herbivore (inherits Creature)
├── Type ("Sheep", "Rabbit", etc.)
└── TryEatPlant() (behavior)

Carnivore (inherits Creature)
├── Type ("Wolf", "Fox", etc.)
└── TryEat() (hunt behavior)
```

### World State

```csharp
World
├── Width, Height (dimensions)
├── Plants (List<Plant>)
├── Herbivores (List<Herbivore>)
├── Carnivores (List<Carnivore>)
└── Add/Remove methods
```

### Simulation State

```csharp
SimulationEngine
├── World (World instance)
├── DayNightCycle (time management)
├── SeasonCycle (season management)
├── WeatherIntensity (0.0-1.0)
├── SimulationSpeed (0.25-4.0)
└── StatisticsTracker (metrics)
```

---

## ⚙️ Simulation Rules

### Update Loop

1. **Render Loop**: 60 FPS (16.67ms per frame)
   - Visual rendering only
   - UI updates
   - Animation interpolation

2. **Logic Loop**: 5 Hz (200ms per tick)
   - Entity updates
   - Collision detection
   - Feeding interactions
   - Reproduction checks
   - Disease spread

### Entity Behavior

#### Plants

- **Growth**: Grows at `GrowthRate * deltaTime * (WaterLevel / 100)`
- **Water Decay**: `WaterLevel -= WaterDecayRate * deltaTime`
- **Death**: Dies when `WaterLevel <= 0` or `Health <= 0`
- **Max Size**: 30 pixels radius

#### Herbivores

- **Movement**: Moves at `Speed` pixels/second
- **Hunger**: Increases by `HungerIncreaseRate * deltaTime`
- **Feeding**: Eats plants when within `EatingRange` (30 pixels)
- **Fleeing**: Flees from carnivores within `FleeDetectionRange` (100 pixels)
- **Death**: Dies when `Hunger > StarvationThreshold` (20.0) or `Health <= 0`

#### Carnivores

- **Movement**: Moves at `Speed` pixels/second (faster than herbivores)
- **Hunger**: Increases by `HungerIncreaseRate * deltaTime`
- **Hunting**: Attacks herbivores within `EatingRange` (30 pixels)
- **Death**: Dies when `Hunger > StarvationThreshold` (20.0) or `Health <= 0`

### Food Chain

```
Plants → Herbivores → Carnivores
```

- **Herbivores eat Plants**: Reduces plant health, increases herbivore hunger satisfaction
- **Carnivores eat Herbivores**: Reduces herbivore health, increases carnivore hunger satisfaction
- **Balance**: Ecosystem maintains balance through population dynamics

### Day/Night Cycle

- **Day Phase**: 0.0-0.5 (normal behavior)
- **Night Phase**: 0.5-1.0 (reduced activity)
- **Duration**: 60 seconds per full cycle

### Weather System

- **Intensity**: 0.0 (calm) to 1.0 (stormy)
- **Source**: CPU usage (system integration)
- **Effects**:
  - Storms damage plants (`StormPlantDamageRate`)
  - Storms provide water bonus (`StormPlantWaterBonus`)
  - Affects creature behavior (reduced movement)

### Reproduction

- **Conditions**:
  - Two entities of same type nearby
  - Health > 50%
  - Age > reproduction threshold
- **Result**: New entity spawned near parents
- **Cooldown**: Prevents rapid population explosion

### Disease System

- **Spread**: Diseases spread between nearby entities
- **Effects**: Reduces health over time
- **Recovery**: Entities can recover if health improves

---

## 🖥️ User Interface

### Window Properties

- **Position**: Bottom of screen
- **Size**: Full width, 200px height
- **Transparency**: Transparent background
- **Always on Top**: Optional (default: true)
- **Hit-Test Passthrough**: Enabled for empty areas

### Interaction

#### Mouse

- **Hover (Plant)**: Plant shakes gently
- **Hover (Creature)**: Tooltip shows stats
- **Click (Plant)**: Waters plant (+30 water)
- **Click (Creature)**: Feeds creature (+20 nutrition)
- **Click (Empty)**: Passes through to desktop

#### Keyboard

- **P**: Spawn plant
- **H**: Spawn herbivore
- **C**: Spawn carnivore
- **W**: Water all plants
- **M**: Toggle mini-map
- **G**: Toggle population graph
- **F1**: Toggle status bar
- **F2**: Toggle settings panel
- **Ctrl+S**: Manual save
- **Ctrl+L**: Manual load

### UI Components

- **Status Bar**: FPS, entity counts, ecosystem health
- **Mini-Map**: Bird's eye view of ecosystem
- **Population Graph**: Real-time population trends
- **Settings Panel**: Simulation speed, spawn rates, visual effects
- **Tooltips**: Entity information on hover
- **Notifications**: Toast-style achievement notifications

---

## 💾 Persistence

### Save Format

**File**: JSON (`terrarium_save.json`)

**Schema Version**: 1

**Structure**:
```json
{
  "SchemaVersion": 1,
  "Width": 1920.0,
  "Height": 200.0,
  "Plants": [
    {
      "Id": 1,
      "X": 100.0,
      "Y": 150.0,
      "Health": 100.0,
      "Age": 45.2,
      "Size": 15.0,
      "WaterLevel": 80.0
    }
  ],
  "Herbivores": [...],
  "Carnivores": [...],
  "SaveDate": "2026-01-11T12:00:00Z"
}
```

### Save/Load Behavior

- **Auto-Save**: On application exit
- **Manual Save**: Ctrl+S
- **Manual Load**: Ctrl+L
- **Backup**: Creates `.bak` file before overwriting
- **Atomic Write**: Uses temp file + move for safety

### Async Support

- **Async Methods**: `SaveWorldAsync()`, `LoadWorldAsync()`
- **Sync Wrappers**: `SaveWorld()`, `LoadWorld()` (for compatibility)
- **Cancellation**: Supports `CancellationToken`

---

## ⚡ Performance Requirements

### Rendering

- **Target FPS**: 60 FPS
- **Frame Time**: < 16.67ms per frame
- **Rendering**: Canvas-based (WPF)
- **Optimization**: Entity culling, efficient drawing

### Logic

- **Update Rate**: 5 Hz (200ms per tick)
- **Entity Limit**: ~100-200 entities (performance degrades beyond)
- **Memory**: < 100MB typical usage

### File I/O

- **Save Time**: < 100ms for typical world
- **Load Time**: < 200ms for typical world
- **File Size**: < 1MB for typical world

---

## 🔌 Extensibility Points

### Adding New Entity Types

1. **Create Entity Class**:
   ```csharp
   public class NewEntity : LivingEntity, IClickable
   {
       // Properties and behavior
   }
   ```

2. **Add to World**:
   ```csharp
   public List<NewEntity> NewEntities { get; } = new();
   ```

3. **Update SimulationEngine**:
   - Add update logic
   - Add to save/load system

### Adding New Managers

1. **Create Manager Class**:
   ```csharp
   public class NewManager
   {
       public void Update(World world, double deltaTime) { }
   }
   ```

2. **Inject into SimulationEngine**:
   ```csharp
   private readonly NewManager _newManager;
   ```

3. **Call in Update Logic**:
   ```csharp
   _newManager.Update(_world, deltaTime);
   ```

### Adding New UI Components

1. **Create Component Class**:
   ```csharp
   public class NewUIComponent
   {
       public void Update(double deltaTime) { }
       public void Render(Canvas canvas) { }
   }
   ```

2. **Add to MainWindow**:
   ```csharp
   private NewUIComponent? _newUIComponent;
   ```

3. **Update in Render Loop**:
   ```csharp
   _newUIComponent?.Update(deltaTime);
   _newUIComponent?.Render(RenderCanvas);
   ```

### Custom Rendering

- **Renderer Class**: `Terrarium.Desktop/Rendering/Renderer.cs`
- **Entity Rendering**: Override `RenderEntity()` methods
- **Particle Effects**: Use `ParticleSystem` class

### Custom Simulation Rules

- **Modify Constants**: Update constants in entity classes
- **Add Behaviors**: Extend entity `Update()` methods
- **New Interactions**: Add to `FoodManager` or create new manager

---

## 📐 Design Patterns Used

### 1. Layered Architecture
- **Purpose**: Separation of concerns
- **Implementation**: Three distinct layers (Presentation, Logic, Data)

### 2. Dependency Injection
- **Purpose**: Loose coupling, testability
- **Implementation**: Managers injected into SimulationEngine

### 3. Strategy Pattern
- **Purpose**: Interchangeable algorithms
- **Implementation**: Different movement strategies, feeding strategies

### 4. Observer Pattern
- **Purpose**: Event-driven updates
- **Implementation**: EventSystem for notifications

### 5. Factory Pattern
- **Purpose**: Object creation
- **Implementation**: World.SpawnRandom*() methods

### 6. Singleton Pattern (Implicit)
- **Purpose**: Single instance
- **Implementation**: SimulationEngine instance per application

---

## 🔒 Constraints & Limitations

### Platform Constraints

- **WPF**: Windows-only (UI layer)
- **Logic Layer**: Cross-platform (.NET Standard)

### Performance Constraints

- **Entity Limit**: ~200 entities before performance degrades
- **World Size**: Fixed to screen width (typically 1920px)

### Feature Limitations

- **No Network**: Single-player only
- **No Audio**: Sound system is placeholder
- **No Custom Sprites**: Uses simple shapes (extensible)

---

## 📝 Version History

### Schema Versions

- **v1**: Initial schema (current)

### Future Schema Changes

When adding new fields:
1. Increment `CurrentSchemaVersion`
2. Add migration logic in `SaveManager`
3. Handle backward compatibility

---

*Last updated: January 2026*  
*Maintained by: Project maintainers*

