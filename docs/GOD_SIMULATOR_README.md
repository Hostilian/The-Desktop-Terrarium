# 🌍 World Box - God Simulator

A complete god simulator inspired by **WorldBox: God Simulator** and **Caves of Qud**, featuring deep procedural lore, territorial conquest, and complex faction warfare.

## 🎮 Features

### Core Gameplay
- **Grid-Based World**: 120x80 tile world with diverse terrain types
- **Opposing Factions**: 5 distinct factions battling for territorial dominance
- **Multiplication Mechanics**: Entities automatically multiply and expand
- **Territorial Conquest**: Factions conquer and assimilate opposing territories
- **God Powers**: 5 divine abilities (Spawn, Smite, Mutate, Heal, Freeze)
- **Theme Selection**: Choose Forest, Desert, or Aquatic worlds at startup

### Deep Lore System (Caves of Qud Inspired)
- **Procedural Faction Names**: Generated names like "The Chromatic Salt Sultanate"
- **Historical Backstories**: Each faction has a rich, procedurally generated history
- **Entity Personalities**: Named entities with faction-specific lore
- **Ancient Artifacts**: Hidden relics that grant special powers when discovered

### Visual Design (WorldBox Inspired)
- **Pixel-Art Aesthetics**: Clean, colorful 8x8 pixel tiles
- **Faction Color Coding**: Each faction has distinct visual identity
- **Particle Effects**: Dynamic visual feedback for all god powers
- **Dynamic Effects**: Glow effects, particles, and smooth animations
- **Minimalist UI**: Clean interface that stays out of the way

## 🚀 Installation & Running

### Prerequisites
- Python 3.8+
- pip package manager

### Setup
```bash
# Install dependencies
pip install -r requirements.txt

# Run the simulator
python god_simulator.py
```

Or use the Windows batch file:
```cmd
run_simulator.bat
```

## 🎯 Controls

### Startup
- **↑↓**: Select theme (Forest/Desert/Aquatic)
- **ENTER**: Start simulation
- **ESC**: Cancel/Quit

### God Powers
- **1**: Spawn Power - Create entities of selected faction
- **2**: Smite Power - Destroy entities with fiery wrath
- **3**: Mutate Power - Change entity faction allegiance
- **4**: Heal Power - Restore entity health
- **5**: Freeze Power - Temporarily stun entities

### Interaction
- **Left Click**: Use selected god power
- **Right Click**: Cancel power selection
- **ESC**: Cancel power selection

### Factions
- **Forest Alliance**: Green faction, nature-focused
- **Desert Empire**: Orange faction, expansionist
- **Aquatic Dominion**: Blue faction, water-based
- **Mountain Clans**: Grey faction, defensive
- **Neutral**: White faction, unaligned
- **E**: Aquatic Dominion (Blue)
- **R**: Mountain Clans (Gray)

### Mouse Controls
- **Left Click**: Use selected god power
- **Right Click**: Cancel power selection

### General
- **ESC**: Cancel current power selection

## 🏗️ Architecture

### Core Systems
- **World System**: Grid-based terrain with faction control
- **Entity System**: AI-driven entities with complex behaviors
- **Lore Generator**: Procedural content creation
- **God Powers**: Interactive player abilities
- **Rendering Engine**: Efficient tile-based graphics

### Factions
1. **Forest Alliance**: Nature-attuned entities, excel in wooded areas
2. **Desert Empire**: Harsh survivors, thrive in arid environments
3. **Aquatic Dominion**: Water-based entities, masters of the seas
4. **Mountain Clans**: Rugged dwellers, conquer mountainous terrain
5. **Void Horde**: Chaotic entities that can survive anywhere

### Entity Types
- **Workers**: Gather resources and multiply
- **Warriors**: Conquer territory and defend
- **Scouts**: Explore and expand influence
- **Artifacts**: Special entities with unique powers

## 🔬 Simulation Mechanics

### Multiplication
Entities automatically attempt to reproduce when they have sufficient energy, creating offspring that expand their faction's territory.

### Conquest
Entities seek out unconquered or weakly defended territory, attempting to assimilate it into their faction's control.

### Assimilation
When territory is conquered, all entities on that tile are converted to the conquering faction's allegiance.

### Resource Management
Entities require energy to survive and multiply. Different terrain types provide varying levels of sustenance.

## 🎨 Visual Style

The simulator uses a pixel-art aesthetic similar to WorldBox:
- 8x8 pixel tiles for terrain
- Distinct colors for each faction
- Visual effects for actions and status
- Clean UI that doesn't obstruct the world view

## 📚 Lore Examples

**Faction Names:**
- The Eternal Oak Sultanate
- The Chromatic Salt Empire
- The Primal Coral Dominion
- The Jagged Iron Clans

**Entity Names:**
- Guardian of the Forest Alliance
- Scout of the Desert Empire
- Worker of the Aquatic Dominion

**Historical Events:**
- "The Chromatic Sultanate of the Salt Dunes emerged from the depths of forgotten ruins."
- "The Primal Coral Dominion was forged in the fires of eternal conflict."

## 🛠️ Technical Details

- **Language**: Python 3.8+
- **Graphics**: Pygame
- **Architecture**: Object-oriented with clean separation of concerns
- **Performance**: Optimized for thousands of simultaneous entities
- **Extensibility**: Modular design for easy addition of new features

## 🎯 Design Philosophy

This simulator combines:
- **WorldBox's** accessible god gameplay and visual style
- **Caves of Qud's** deep procedural lore and logical consistency
- **Emergent Complexity**: Simple rules create complex faction dynamics
- **Player Agency**: God powers allow intervention without breaking simulation

## 🚀 Future Enhancements

- Advanced AI behaviors
- More god powers
- Dynamic world events
- Multiplayer support
- Modding API
- Advanced artifact systems

---

*Created by Senior Game Engine Architect - January 12, 2026*
