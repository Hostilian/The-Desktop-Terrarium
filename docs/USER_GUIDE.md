# 📚 Desktop Terrarium - User Guide

**Welcome to Desktop Terrarium!** This guide will help you get started with creating and managing your own digital ecosystems.

---

![Desktop Terrarium Main View](screenshots/terrarium-main.png)


## 📥 Installation

### System Requirements
- **OS:** Windows 10 (1809+) or Windows 11
- **Runtime:** .NET 8.0 Runtime (auto-installed with app)
- **RAM:** 4GB minimum (8GB recommended)
- **GPU:** DirectX 11 compatible
- **Storage:** 500MB free space
- **Display:** 1280x720 minimum resolution

### Installation Steps
1. Download `DesktopTerrariumSetup.msi` from [Releases](https://github.com/Hostilian/The-Desktop-Terrarium/releases)
2. Double-click to run installer
3. Follow installation wizard
4. Launch from Start Menu or Desktop shortcut

### Portable Version
Extract `DesktopTerrariumPortable.zip` and run `Terrarium.Desktop.exe` - no installation needed!

---

## 🚀 Quick Start (First 5 Minutes)

### 1. Launch the Application
On first launch, you'll see the **Terrarium Selection** dialog.

### 2. Choose Your Terrarium Type
Pick from 6 ecosystem types:
- 🌲 **Forest** - Lush greenery, balanced ecosystem
- 🏜️ **Desert** - Hardy plants, scarce resources  
- 🌊 **Aquatic** - Water-based life forms
- ❄️ **Tundra** - Cold-adapted species
- 🌋 **Volcanic** - Extreme environment
- 🏙️ **Urban** - Human-influenced ecosystem

### 3. Watch Your Ecosystem Evolve
Your terrarium will automatically populate with:
- 🌱 **Plants** (green) - Primary producers
- 🦌 **Herbivores** (blue) - Plant eaters
- 🐺 **Carnivores** (red) - Predators

### 4. Use God Powers (Optional)
Click the buttons on the right to interact:
- ☀️ **Spawn Sun** - Boost plant growth
- 🌧️ **Rain** - Increase fertility temporarily
- ⚡ **Lightning** - Random dramatic events
- And more!

---

## 🎮 Core Features

### Main Simulation View
The central canvas shows your living ecosystem with:
- Real-time entity movement and interactions
- Day/night cycle with lighting effects
- Faction territories (colored terrain)
- Particle effects (births, deaths, feeding)

### Entity Information
**Hover over any creature** to see:
- Name and species
- Faction affiliation
- Health and hunger levels
- Age and personality traits

**Right-click** for extended lore (if available)

---

## ⌨️ Keyboard Controls

### Essential Controls
| Key | Action |
|-----|--------|
| **Space** | Pause/Resume simulation |
| **+/-** | Increase/Decrease speed |
| **R** | Reset/Restart terrarium |
| **Esc** | Exit application |

### Window Controls  
| Key | Action |
|-----|--------|
| **F2** | Open Settings |
| **F3** | Toggle Statistics |
| **F4** | Open Chronicle (history) |
| **M** | Toggle Mini-map |
| **G** | Toggle Population Graph |

### Advanced Controls
| Key | Action |
|-----|--------|
| **F** | Toggle fullscreen |
| **P** | Toggle performance overlay |
| **L** | Toggle lore tooltips |
| **1-6** | Quick spawn entities |

---

## 🎯 God Powers Explained

### Growth & Life
- **☀️ Spawn Sun** - Creates sunlight boost zone
- **🌱 Spawn Plant** - Place plants manually
- **🐣 Spawn Creature** - Add herbivores/carnivores

### Weather & Environment
- **🌧️ Make it Rain** - Fertility bonus for 30 seconds
- **⚡ Lightning Strike** - Dramatic random events
- **❄️ Frost** - Slow down activity temporarily

### Cosmic Powers
- **🌙 Time Control** - Speed up/slow down time
- **🌊 Flood** - Water-based events
- **🔥 Fireball** - Targeted destruction (use wisely!)

**Pro Tip:** God powers have cooldowns to maintain balance!

---

## 📊 Understanding Statistics (F3)

![Statistics Overlay](screenshots/terrarium-stats.png)


### Population Panel
- **Total Population** - All living creatures
- **Plants** - Green producers
- **Herbivores** - Blue consumers  
- **Carnivores** - Red predators

### Ecosystem Health
- **Health Score** - 0-100% ecosystem stability
  - 🟢 **80-100%** - Thriving!
  - 🟡 **50-79%** - Stable
  - 🔴 **0-49%** - Struggling

### Session Statistics
- **Birth Rate** - New creatures per minute
- **Death Rate** - Losses per minute
- **Average Lifespan** - How long creatures survive
- **Food Consumption** - Resource usage rate

---

## 🏴 Faction System

### The Six Factions
Each faction controls territory and has unique traits:

1. **🌿 Verdant Collective** (Green)
   - Territory: Forests and plains
   - Strength: Plant growth
   - Weakness: Fire vulnerable

2. **🔥 Ashen Legion** (Red)
   - Territory: Volcanic areas
   - Strength: Heat resistance
   - Weakness: Water damage

3. **🌊 Tide Walkers** (Blue)
   - Territory: Aquatic zones
   - Strength: Swimming
   - Weakness: Drought

4. **⚡ Crystal Choir** (Yellow)
   - Territory: Mountain peaks
   - Strength: Defense
   - Weakness: Slow movement

5. **🌙 Nomadic Covenant** (Purple)
   - Territory: Wandering
   - Strength: Adaptability
   - Weakness: No permanent base

6. **⚙️ Scrapborn Swarm** (Gray)
   - Territory: Urban ruins
   - Strength: Scavenging
   - Weakness: Pollution dependency

### Faction Relationships
- **Allied** - Share resources, protect each other
- **Neutral** - Coexist peacefully
- **Hostile** - Compete for territory and food

**Watch territories change** colors as factions expand/contract!

---

## 📜 Chronicle Window (F4)

The Chronicle records major events:
- **🎂 Births** - New creatures spawned
- **☠️ Deaths** - Losses (with causes)
- **🍖 Hunts** - Predation events
- **👑 Named Characters** - Legendary creatures
- **🏆 Milestones** - Population records

**Named Characters** are rare! They have:
- Unique names
- Extended biographies
- Special abilities
- Multi-generational lore

---

## ⚙️ Settings (F2)

![Settings Dialog](screenshots/terrarium-settings.png)


### Graphics Options
- **Visual Quality** - Low/Medium/High
- **Particle Density** - 0-100%
- **Enable Shadows** - Toggle shadows
- **Enable Weather Effects** - Visual weather
- **Screen Resolution** - Window size

### Gameplay Options
- **Simulation Speed** - 0.5x to 10x
- **Entity Limit** - Max creatures (performance)
- **Auto-Save Interval** - 1-60 minutes
- **God Mode** - Enable all powers

### Audio Options
- **Master Volume** - Overall sound level
- **Music Volume** - Background music
- **SFX Volume** - Sound effects
- **Ambient Sounds** - Environmental audio

---

## 💾 Saving & Loading

### Auto-Save
- Enabled by default
- Saves every 5 minutes
- Located in `%AppData%\DesktopTerrarium\Saves\`

### Manual Save
1. Press **Ctrl+S** or click File → Save
2. Enter save name
3. Confirm

### Loading Saved Terrarium
1. Click File → Load
2. Select save file from list
3. Confirm (current progress will be replaced)

### Export/Import
- **Export** - Share terrariums with friends (`.terrarium` file)
- **Import** - Load shared terrariums

---

## 🏆 Achievements

Unlock 15+ achievements by playing:

### Population Achievements
- **🌱 First Sprout** - Grow 10 plants
- **🏘️ Village** - Reach 100 population
- **🏙️ Metropolis** - Reach 1,000 population
- **🌍 Megacity** - Reach 10,000 population

### Survival Achievements
- **📅 Week One** - Survive 7 in-game days
- **🎂 Elder** - Creature lives 100+ days
- **🦖 Ancient** - Creature lives 365+ days

### Balance Achievements
- **⚖️ Perfect Harmony** - 30% herbivores, 10% carnivores, 60% plants
- **🌿 Green Paradise** - 90%+ plants
- **🦁 Apex Predator** - Carnivores dominate

### Special Achievements
- **👑 Dynasty** - Same lineage survives 10 generations
- **🌈 Biodiversity** - All 6 factions present
- **⏰ Time Lord** - Run simulation for 24 real hours

**Check:** Menu → Achievements to see progress!

---

## 🔧 Troubleshooting

### Performance Issues
**Symptom:** Low FPS, stuttering

**Solutions:**
1. Lower graphics quality (F2 → Settings)
2. Reduce particle density
3. Set entity limit to 5,000 or lower
4. Close other applications
5. Update GPU drivers

### Crashes on Startup
**Symptom:** App won't launch

**Solutions:**
1. Install .NET 8.0 Runtime
2. Update Windows to latest version
3. Run as Administrator
4. Check `error.log` in app folder

### Entities Not Spawning
**Symptom:** Empty terrarium

**Solutions:**
1. Wait 30 seconds for initial population
2. Use "Spawn Creature" god power
3. Reset terrarium (R key)
4. Check that simulation isn't paused

### Save File Won't Load
**Symptom:** Error loading save

**Solutions:**
1. Verify file isn't corrupted
2. Check file permissions
3. Try backup save (auto-saves kept)
4. Start new terrarium

---

## 💡 Tips & Tricks

### For Beginners
- **Start with Forest** - Most balanced ecosystem
- **Don't overuse god powers** - Let nature find balance
- **Watch the health score** - Aim for 60%+
- **Save often** - Before experiments

### For Advanced Users
- **Speed up boring parts** - Use 5x-10x speed for growth
- **Create challenges** - Try carnivore-only terrarium
- **Experiment with factions** - See which dominates
- **Hunt for named characters** - Rare but rewarding lore

### Optimal Ecosystem Ratios
- **Plants:** 60-70%
- **Herbivores:** 20-30%
- **Carnivores:** 5-15%

### Best God Power Combos
1. **Rain** → **Spawn Plant** → **Spawn Sun** = Rapid growth
2. **Lightning** → **Fireball** = Dramatic reset
3. **Frost** → Observe slowly = Study behavior

---

## 🎨 Customization (Advanced)

### Mod Support (Coming Soon)
- Custom creature sprites
- New faction definitions
- Terrain type additions
- Behavior AI tweaks

### Configuration Files
Edit `terrarium_settings.json` for:
- Custom entity attributes
- Spawn rate adjustments
- Color scheme changes
- Physics parameters

**⚠️ Warning:** Edit at your own risk! Backup first.

---

## 🆘 Getting Help

### Resources
- **📖 Documentation:** [GitHub Wiki](https://github.com/Hostilian/The-Desktop-Terrarium/wiki)
- **💬 Community:** [Discord Server](#) (Coming Soon)
- **🐛 Bug Reports:** [GitHub Issues](https://github.com/Hostilian/The-Desktop-Terrarium/issues)
- **📧 Email:** support@example.com

### Frequently Asked Questions

**Q: Can I play offline?**  
A: Yes! Desktop Terrarium is 100% offline after installation.

**Q: Is multiplayer planned?**  
A: Yes, in v2.0! Shared terrariums and competitive modes.

**Q: How many creatures can I have?**  
A: Default limit is 10,000. Adjust in Settings based on PC performance.

**Q: Can I delete a faction?**  
A: Not directly, but you can eliminate all creatures of that faction.

**Q: What do the colors mean?**  
A: Green = Plants, Blue = Herbivores, Red = Carnivores, Territory colors = Faction control.

---

## 🎯 Next Steps

Now that you know the basics:

1. **🎮 Play for 30 minutes** - Get comfortable with controls
2. **🏆 Unlock your first achievement** - Try "First Sprout"
3. **📊 Check statistics** - Learn to read ecosystem health
4. **🌍 Try all terrarium types** - See which you prefer
5. **🎨 Experiment** - Use god powers creatively!

---

## 🙏 Thank You!

Thank you for choosing Desktop Terrarium! If you enjoy the experience:
- ⭐ **Star the repository** on GitHub
- 💬 **Share** with friends
- 🐛 **Report bugs** to help improve
- 💡 **Suggest features** for future versions

**Happy simulating!** 🌿✨

---

*Last Updated: 2026-01-22 | Version 1.0*
