# Desktop Terrarium - Architecture Overview

## System Architecture

### **Layered Architecture**
```
┌─────────────────┐
│  Terrarium.Desktop  │ ← WPF UI Layer
│  (Presentation)     │
├─────────────────┤
│  Terrarium.Logic     │ ← Business Logic Layer
│  (Domain Model)      │
├─────────────────┤
│  Terrarium.Tests     │ ← Testing Layer
│  (Quality Assurance) │
└─────────────────┘
```

### **Core Components**

#### **SimulationEngine** (Central Coordinator)
- **Location**: `Terrarium.Logic/Simulation/SimulationEngine.cs`
- **Purpose**: Main orchestration hub for all game logic
- **Responsibilities**:
  - Coordinates all subsystem updates
  - Manages simulation timing (5 logic ticks/second)
  - Handles weather, seasons, and environmental effects
  - Processes entity behaviors and interactions
- **Key Managers**:
  - `MovementCalculator` - Entity movement and pathfinding
  - `CollisionDetector` - Spatial collision detection
  - `FoodManager` - Resource consumption and growth
  - `FactionManager` - Inter-faction relationships
  - `LoreManager` - Procedural narrative generation

#### **World** (Game State Container)
- **Location**: `Terrarium.Logic/Simulation/World.cs`
- **Purpose**: Central repository for all game entities and terrain
- **Contains**:
  - Entity collections (Plants, Herbivores, Carnivores)
  - Terrain grid (9 terrain types with faction ownership)
  - Spatial partitioning for performance
  - World boundaries and dimensions

#### **Entity System** (Game Objects)
- **Base Classes**:
  - `WorldEntity` - Position and basic properties
  - `LivingEntity` - Health, age, reproduction
  - `Creature` - Movement, hunger, behavior
  - `Plant` - Growth, resource production
- **Specialized Types**:
  - `Herbivore` - Plant-eating creatures
  - `Carnivore` - Meat-eating predators
- **Interfaces**:
  - `IMovable` - Movement capabilities
  - `IClickable` - User interaction

#### **Faction System** (Political Simulation)
- **Location**: `Terrarium.Logic/Simulation/Faction.cs`
- **Purpose**: Multi-faction warfare and diplomacy
- **Features**:
  - 6 opposing factions with unique abilities
  - Relationship matrices (-100 to +100)
  - Territory control and expansion
  - Procedural faction naming and lore

#### **Terrain System** (Cellular Automata)
- **Location**: `Terrarium.Logic/Simulation/TerrainType.cs`
- **Purpose**: Dynamic landscape with faction conquest
- **Features**:
  - 9 terrain types (Void, Soil, Stone, Water + faction variants)
  - Conquest mechanics with elemental counters
  - God painting tools for direct manipulation
  - Terrain-based movement modifiers

## Data Flow

### **Main Game Loop**
```
User Input → MainWindow → SimulationEngine → World Updates → Renderer → UI Display
```

### **Entity Behavior Flow**
```
SimulationEngine.Tick() → MovementCalculator → CollisionDetector → Entity.Update() → World.Update()
```

### **Rendering Pipeline**
```
SimulationEngine → World → Renderer → Canvas Updates → WPF Display
```

### **God Power Flow**
```
UI Button → MainWindow → SimulationEngine → World Modification → Visual Feedback
```

## State Management

### **Centralized State**
- **Single Source of Truth**: `World` class contains all entities and terrain
- **Immutable Updates**: State changes create new entity states
- **Event-Driven**: `EventSystem` broadcasts changes to interested components

### **UI State**
- **Reactive Updates**: UI reflects simulation state changes
- **Performance Optimized**: Only update changed elements
- **Thread Safety**: All UI updates on dispatcher thread

### **Persistence**
- **SaveManager**: JSON-based save/load system
- **Automatic Saves**: World state serialization
- **Load Validation**: Integrity checks on loaded data

## Key Design Patterns

### **Observer Pattern**
- **EventSystem**: Decoupled communication between components
- **INotifyPropertyChanged**: UI data binding
- **Event Handlers**: Loose coupling between layers

### **Factory Pattern**
- **Entity Creation**: Centralized factory methods
- **Terrain Generation**: Procedural content creation
- **Faction Initialization**: Dynamic faction setup

### **Strategy Pattern**
- **God Powers**: Different behavior implementations
- **Entity Behaviors**: Pluggable AI strategies
- **Rendering Modes**: Different visual styles

### **Singleton Pattern**
- **Manager Classes**: Single instances for global state
- **Resource Management**: Centralized asset handling

## Performance Optimizations

### **Spatial Partitioning**
- **Grid-Based**: Entities organized by location
- **Collision Detection**: O(n) complexity reduction
- **Rendering Culling**: Only render visible entities

### **Update Batching**
- **Logic Ticks**: 5 updates/second for heavy calculations
- **Render Loop**: 60 FPS for smooth visuals
- **Background Processing**: Non-critical updates

### **Memory Management**
- **Object Pooling**: Reuse common objects
- **Garbage Collection**: Minimize allocations
- **Efficient Collections**: Appropriate data structures

## Error Handling

### **Graceful Degradation**
- **Exception Boundaries**: Isolate failures
- **Fallback Behaviors**: Continue operation when possible
- **User Feedback**: Clear error messages

### **Validation**
- **Input Validation**: Prevent invalid states
- **Boundary Checks**: Prevent out-of-bounds access
- **Integrity Checks**: Verify data consistency

## Extensibility Points

### **Plugin Architecture**
- **Manager Interfaces**: Swappable implementations
- **Entity Extensions**: New creature types
- **God Power Additions**: New divine abilities

### **Configuration**
- **Settings System**: Runtime configuration
- **Theme Support**: Multiple visual styles
- **Difficulty Scaling**: Adjustable parameters</content>
<parameter name="filePath">c:\Users\Hostilian\The-Desktop-Terrarium\.planning\ARCHITECTURE.md
