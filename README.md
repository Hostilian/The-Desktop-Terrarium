# The Desktop Terrarium 🌱

A desktop ecosystem simulator that runs transparently at the bottom of your screen, featuring interactive plants and creatures that respond to your mouse and system activity.

![License](https://img.shields.io/badge/license-MIT-blue.svg)
![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)
![WPF](https://img.shields.io/badge/WPF-Windows-blue.svg)

## 📋 Project Overview

**The Desktop Terrarium** combines an ecosystem simulator with a desktop virtual pet. It's a transparent WPF application that sits at the bottom of your screen, where plants grow and creatures interact with each other and your mouse while you work.

### Key Features

- 🌿 **Living Ecosystem**: Plants grow, herbivores graze, carnivores hunt
- 🖱️ **Mouse Interaction**: Click to feed creatures or water plants; hover to make plants shake
- 🌦️ **System Integration**: High CPU usage causes stormy weather that affects the ecosystem
- 💾 **Save/Load System**: Automatically saves on exit; manual save with Ctrl+S
- 🎨 **Dual Rendering**: Supports both simple shapes and custom sprite graphics
- 📊 **Real-time Statistics**: Monitor FPS, entity count, and ecosystem health

## 🏗️ Architecture

This project demonstrates **Layered Architecture** with proper separation of concerns:

### Layer 1: Application Logic (`Terrarium.Logic`)
- **No GUI dependencies** - Pure C# logic
- Contains all simulation math, entity behavior, and game rules
- Fully unit-testable
- **Key Classes**:
  - `WorldEntity` → `LivingEntity` → `Plant` / `Creature` → `Herbivore` / `Carnivore`
  - `SimulationEngine` - Main simulation orchestrator
  - `MovementCalculator` - Handles entity movement
  - `CollisionDetector` - Manages entity interactions
  - `FoodManager` - Controls ecosystem balance

### Layer 2: Presentation (`Terrarium.Desktop`)
- WPF application with transparent window
- Renders entities and handles user input
- Delegates all logic to the Logic layer
- **Key Classes**:
  - `MainWindow` - Main application window
  - `Renderer` - Draws entities to canvas
  - `SystemMonitor` - Monitors CPU for weather effects
  - `AnimationController` - Manages sprite animations

### Layer 3: Testing (`Terrarium.Tests`)
- MSTest unit tests for the Logic layer
- Tests growth, eating, movement, collisions
- Proves logic-UI separation works

## 🎯 OOP Principles Demonstrated

### ✅ Inheritance (IS-A Relationship)
```
WorldEntity (base)
    ├─ LivingEntity (has Health, Age)
          ├─ Plant (grows, needs water)
          └─ Creature (moves, has Hunger)
                ├─ Herbivore (eats plants)
                └─ Carnivore (hunts herbivores)
```

### ✅ Interfaces (CAN-DO Relationship)
- `IClickable` - For entities that respond to clicks
- `IMovable` - For entities that can move

### ✅ Encapsulation
- All fields are `private` (e.g., `_health`, `_hunger`)
- Public properties with controlled access
- Most values are named constants; remaining rendering constants are tracked in `VERIFICATION_CHECKLIST.md`

### ✅ Single Responsibility Principle
- `Creature` doesn't draw itself - `Renderer` does
- `SimulationEngine` doesn't calculate movement - `MovementCalculator` does
- Specialized managers prevent "God Object" anti-pattern

## 🚀 Getting Started

### Prerequisites
- Visual Studio 2022 or later
- .NET 10.0 SDK
- Windows OS (for WPF support)

### Building the Project

1. **Clone the repository**
   ```bash
   git clone https://github.com/Hostilian/The-Desktop-Terrarium.git
   cd The-Desktop-Terrarium
   ```

2. **Open in Visual Studio**
   ```
   Open DesktopTerrarium.sln
   ```

3. **Restore NuGet packages**
   ```
   Right-click solution → Restore NuGet Packages
   ```

4. **Build solution**
   ```
   Build → Build Solution (Ctrl+Shift+B)
   ```

5. **Run tests**
   ```
   Test → Run All Tests
   ```

6. **Run application**
   ```
   Set Terrarium.Desktop as startup project
   Press F5 to run
   ```

## 🎮 Controls

| Key | Action |
|-----|--------|
| **Left Click** | Select entity / Feed creature / Water plant |
| **Mouse Hover** | Make plants shake / Show tooltip |
| **P** | Spawn random plant |
| **H** | Spawn random herbivore |
| **C** | Spawn random carnivore |
| **W** | Water all plants |
| **M** | Toggle mini-map |
| **G** | Toggle population graph |
| **F** | Follow selected entity |
| **Space** | Pause / Resume simulation |
| **+ / -** | Increase / Decrease simulation speed |
| **F1** | Toggle status display |
| **F2** | Open settings panel |
| **ESC** | Deselect entity / Close settings / Exit |
| **Ctrl+S** | Save game |
| **Ctrl+L** | Load game |
| **Ctrl+Alt+S** | Save game (global hotkey) |
| **Ctrl+Alt+L** | Load game (global hotkey) |
| **Ctrl+Alt+F1** | Toggle UI panels (global hotkey) |

### Visual Systems
- 🏆 **Achievement System**: Unlock achievements as you play
- 😊 **Mood Indicators**: Emoji show creature emotions
- ⚠️ **Predator Warnings**: Alerts when herbivores are in danger
- 💕 **Breeding Indicators**: Hearts show when creatures can breed
- 📊 **Population Graph**: Real-time population history
- 🌍 **Ecosystem Health Bar**: Shows overall ecosystem balance
- 🗺️ **Mini-Map**: Bird's eye view of the terrarium

## 🧪 Running Unit Tests

The project includes comprehensive unit tests for the Logic layer:

```bash
# In Visual Studio Test Explorer
Test → Run All Tests

# Or via command line
dotnet test Terrarium.Tests/Terrarium.Tests.csproj
```

**Test Coverage:**
- ✅ Plant growth mechanics
- ✅ Creature movement and hunger
- ✅ Herbivore eating behavior
- ✅ Carnivore hunting behavior
- ✅ Collision detection
- ✅ Boundary enforcement
- ✅ Simulation engine integration

## 📁 Project Structure

```
The-Desktop-Terrarium/
├── Terrarium.Logic/              # Application Logic Layer
│   ├── Entities/
│   │   ├── WorldEntity.cs        # Base entity class
│   │   ├── LivingEntity.cs       # Living entity with health/age
│   │   ├── Plant.cs              # Plant entity
│   │   ├── Creature.cs           # Base creature class
│   │   ├── Herbivore.cs          # Herbivore (sheep, rabbit)
│   │   └── Carnivore.cs          # Carnivore (wolf, fox)
│   ├── Interfaces/
│   │   ├── IClickable.cs         # Clickable behavior
│   │   └── IMovable.cs           # Movable behavior
│   ├── Simulation/
│   │   ├── World.cs              # World container
│   │   ├── SimulationEngine.cs   # Main simulation logic
│   │   ├── MovementCalculator.cs # Movement logic
│   │   ├── CollisionDetector.cs  # Collision handling
│   │   └── FoodManager.cs        # Ecosystem balance
│   └── Persistence/
│       └── SaveManager.cs        # Save/load functionality
│
├── Terrarium.Desktop/            # Presentation Layer
│   ├── MainWindow.xaml           # Main window UI
│   ├── MainWindow.xaml.cs        # Window code-behind
│   ├── App.xaml                  # Application definition
│   ├── App.xaml.cs               # Application startup
│   └── Rendering/
│       ├── Renderer.cs           # Entity rendering
│       ├── SystemMonitor.cs      # CPU monitoring
│       ├── AnimationController.cs # Animation system
│       ├── WeatherEffects.cs     # Weather visuals
│       ├── ParticleSystem.cs     # Particle effects
│       ├── NotificationManager.cs # Toast notifications
│       ├── TooltipManager.cs     # Entity tooltips
│       ├── SettingsPanel.cs      # Settings UI
│       ├── MiniMap.cs            # Mini-map display
│       ├── AchievementSystem.cs  # Achievement tracking
│       ├── CreatureMoodIndicator.cs # Mood emojis
│       ├── PopulationGraph.cs    # Population history
│       ├── PredatorWarningSystem.cs # Danger alerts
│       ├── EntitySelector.cs     # Entity selection
│       ├── SpeedIndicator.cs     # Speed display
│       ├── BreedingIndicator.cs  # Breeding status
│       └── EcosystemHealthBar.cs # Health display

├── Terrarium.Tests/              # Unit Tests
│   ├── Entities/
│   │   ├── PlantTests.cs
│   │   ├── CreatureTests.cs
│   │   ├── HerbivoreTests.cs
│   │   └── CarnivoreTests.cs
│   ├── Simulation/
│   │   ├── MovementCalculatorTests.cs
│   │   ├── CollisionDetectorTests.cs
│   │   └── SimulationEngineTests.cs
│   └── Rendering/
│       ├── AchievementSystemTests.cs
│       ├── EcosystemHealthTests.cs
│       ├── CreatureMoodTests.cs
│       ├── BreedingIndicatorTests.cs
│       └── PredatorWarningTests.cs
│
└── DesktopTerrarium.sln          # Visual Studio solution
```

## 🎨 Customization

### Adding Custom Sprites

1. Create a folder: `Terrarium.Desktop/Assets/`
2. Add PNG images:
   - `plant.png` - Plant sprite
   - `herbivore.png` - Herbivore sprite
   - `carnivore.png` - Carnivore sprite
3. Set Build Action to `Resource`
4. In `Renderer.cs`, set `_useSpriteMode = true`

### Adjusting Game Balance

Edit constants in entity classes:
```csharp
// In Creature.cs
protected const double HungerIncreaseRate = 0.3;  // How fast creatures get hungry
protected const double StarvationThreshold = 20.0; // When creatures start losing health

// In Plant.cs
private const double DefaultGrowthRate = 0.5;     // How fast plants grow
private const double WaterDecayRate = 0.2;        // How fast plants use water
```

## 🐛 Troubleshooting

### Issue: Window not transparent
**Solution**: Ensure Windows Aero/transparency effects are enabled in Windows settings.

### Issue: High CPU usage
**Solution**: Reduce `RenderFps` constant in `MainWindow.xaml.cs` (default: 60).

### Issue: Tests failing
**Solution**: Rebuild solution and ensure .NET 10.0 SDK is installed.

### Issue: Performance counter errors
**Solution**: SystemMonitor gracefully handles missing performance counters. Run as administrator if needed.

## 📚 Learning Objectives

This project demonstrates:

1. **Layered Architecture** - Separation of logic and presentation
2. **OOP Principles** - Inheritance, interfaces, encapsulation
3. **SOLID Principles** - Single responsibility, dependency injection
4. **Unit Testing** - Testing logic independently from UI
5. **WPF Development** - Transparent windows, animations, data binding
6. **Game Loop** - Fixed timestep simulation with variable rendering
7. **State Management** - Save/load system with JSON serialization

## 📝 Coding Standards

- ✅ **PascalCase** for classes, methods, properties: `CalculateGrowth()`
- ✅ **camelCase** for local variables, parameters: `currentSpeed`
- ✅ **Named constants** instead of magic numbers: `MaxHunger = 100`
- ✅ **XML documentation** on all public members
- ✅ **Private fields** with underscore prefix: `_health`
- ✅ **Single Responsibility** - Each class has one clear purpose

## 🤝 Contributing

Contributions are welcome! Please follow these guidelines:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Follow existing code style and naming conventions
4. Add unit tests for new features
5. Commit changes (`git commit -m 'Add AmazingFeature'`)
6. Push to branch (`git push origin feature/AmazingFeature`)
7. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🎓 Academic Use

This project was created as an educational demonstration of:
- Layered software architecture
- Object-oriented programming principles
- Unit testing practices
- WPF desktop application development

Feel free to use this project as a reference or learning resource!

## ✨ Future Enhancements

Potential features for future development:
- [ ] Multiple biome types (desert, forest, ocean)
- [ ] Weather particle effects (rain, snow)
- [ ] Creature reproduction system
- [ ] Food chain statistics graph
- [ ] Custom sprite editor
- [ ] Network multiplayer ecosystems
- [ ] Achievement system
- [ ] Day/night cycle

## 📞 Contact

Project Link: [https://github.com/Hostilian/The-Desktop-Terrarium](https://github.com/Hostilian/The-Desktop-Terrarium)

---

Made with ❤️ and C# | January 2026
