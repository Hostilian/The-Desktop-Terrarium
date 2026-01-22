namespace Terrarium.Desktop;

using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Terrarium.Desktop.Constants;
using Terrarium.Desktop.Rendering;
using Terrarium.Desktop.Services;
using Terrarium.Logic.Entities;
using Terrarium.Logic.Persistence;
using Terrarium.Logic.Simulation;

/// <summary>
/// Interaction logic for MainWindow.xaml.
/// Main presentation layer window for the Desktop Terrarium application.
/// Handles user input, rendering, and communication with the simulation engine.
/// </summary>
/// <remarks>
/// This window supports multiple terrarium types including forest ecosystems,
/// god simulator mode with divine powers, and particle sandbox simulation.
/// Follows MVVM principles with logic delegated to services (future refactoring).
/// </remarks>
public partial class MainWindow : Window, IDisposable
{
    private SimulationEngine? simulationEngine;
    private Renderer? renderer;
    private DispatcherTimer? renderTimer;
    private DispatcherTimer? systemMonitorTimer;
    private readonly Stopwatch frameStopwatch;
    private SystemMonitor? systemMonitor;
    private SaveManager? saveManager;
    private SoundManager? soundManager;
#pragma warning disable CS0169 // _godPowerService is reserved for future god powers functionality
    private GodPowerService? godPowerService;
#pragma warning restore CS0169

    // Win32 hit testing constants
    private const int WmNcHitTest = Win32Constants.WMNCHITTEST;
    private const int HtTransparent = Win32Constants.HTTRANSPARENT;

    // Timing constants
    private const int RenderFps = RenderingConstants.DEFAULTRENDERFPS;
    private const double RenderInterval = RenderingConstants.RENDERINTERVALMS;
    private const double SystemMonitorInterval = RenderingConstants.SYSTEMMONITORUPDATEINTERVALMS;

    private int frameCount;
    private double fpsAccumulator;
    private double currentFps;

    /// <summary>
    /// Gets the current FPS for display.
    /// </summary>
    public double CurrentFps => currentFps;

    // God painting mode
    private GodPaintMode godPaintMode = GodPaintMode.None;

    // Sound effect tracking
    private int lastTotalBirths = 0;
    private int lastTotalDeaths = 0;
    private int lastTotalPlantsEaten = 0;

    private Terrarium.Logic.Simulation.TerrariumType terrariumType;
    private double simulationSpeed = 1.0;

    // Mouse interaction fields
    private Point mousePosition = new Point(0, 0);
    private bool mouseInCanvas = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// Sets up the frame stopwatch for FPS tracking.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
        frameStopwatch = new Stopwatch();
    }

    private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
    {
        if (simulationEngine == null)
        {
            return;
        }

        if (simulationEngine.IsPaused)
        {
            simulationEngine.Resume();
            renderTimer?.Start();
            systemMonitorTimer?.Start();
            PlayPauseButton.Content = "⏸️";
        }
        else
        {
            simulationEngine.Pause();
            renderTimer?.Stop();
            systemMonitorTimer?.Stop();
            PlayPauseButton.Content = "▶️";
        }
    }

    /// <summary>
    /// Handles the speed button click event.
    /// Cycles through predefined simulation speed multipliers (1x, 2x, 5x, 10x).
    /// </summary>
    private void SpeedButton_Click(object sender, RoutedEventArgs e)
    {
        // Cycle through speed options
        int currentIndex = Array.IndexOf(UIConstants.SIMULATIONSPEEDPRESETS, simulationSpeed);
        int nextIndex = (currentIndex + 1) % UIConstants.SIMULATIONSPEEDPRESETS.Length;
        simulationSpeed = UIConstants.SIMULATIONSPEEDPRESETS[nextIndex];

        simulationEngine?.SetSimulationSpeed(simulationSpeed);
        SpeedButton.Content = $"{simulationSpeed}x";
    }

    /// <summary>
    /// Closes the main window and terminates the application.
    /// </summary>
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    // God Power Button Handlers
    private void SpawnPlantButton_Click(object sender, RoutedEventArgs e)
    {
        if (simulationEngine == null)
        {
            return;
        }

        var plant = simulationEngine.World.SpawnRandomPlant();
        ShowNotification($"🌱 Plant spawned at ({plant.X:F0}, {plant.Y:F0})", "#44FF44");
    }

    private void SpawnHerbivoreButton_Click(object sender, RoutedEventArgs e)
    {
        if (simulationEngine == null)
        {
            return;
        }

        // Get random faction for God Simulator mode
        var faction = terrariumType == Terrarium.Logic.Simulation.TerrariumType.GodSimulator
            ? (FactionType)new Random().Next(Enum.GetValues(typeof(FactionType)).Length)
            : FactionType.VerdantCollective;

        var herbivore = simulationEngine.World.SpawnRandomHerbivore("Rabbit", faction);
        var factionName = simulationEngine.FactionManager.GetFaction(herbivore.Faction).Name;
        ShowNotification($"🐰 {factionName} Herbivore spawned at ({herbivore.X:F0}, {herbivore.Y:F0})", "#FFFFFF");
    }

    private void SpawnCarnivoreButton_Click(object sender, RoutedEventArgs e)
    {
        if (simulationEngine == null)
        {
            return;
        }

        // Get random faction for God Simulator mode
        var faction = terrariumType == Terrarium.Logic.Simulation.TerrariumType.GodSimulator
            ? (FactionType)new Random().Next(Enum.GetValues(typeof(FactionType)).Length)
            : FactionType.AshenLegion;

        var carnivore = simulationEngine.World.SpawnRandomCarnivore("Wolf", faction);
        var factionName = simulationEngine.FactionManager.GetFaction(carnivore.Faction).Name;
        ShowNotification($"🐺 {factionName} Carnivore spawned at ({carnivore.X:F0}, {carnivore.Y:F0})", "#FF4444");
    }

    private void LightningStrikeButton_Click(object sender, RoutedEventArgs e)
    {
        if (simulationEngine == null)
        {
            return;
        }

        var random = new Random();
        var allEntities = simulationEngine.World.GetAllEntities().ToList();

        if (allEntities.Count == 0)
        {
            ShowNotification("⚡ No entities to strike!", "#FFFF44");
            return;
        }

        for (int i = 0; i < Math.Min(GodPowerConstants.LIGHTNINGSTRIKETARGETCOUNT, allEntities.Count); i++)
        {
            var entity = allEntities[random.Next(allEntities.Count)];
            entity.TakeDamage(GodPowerConstants.LIGHTNINGSTRIKEDAMAGE);

            if (entity is Creature creature)
            {
                ShowNotification($"⚡ {creature.GetType().Name} struck at ({creature.X:F0}, {creature.Y:F0})", "#FFFF44");
            }
            else if (entity is Plant plant)
            {
                ShowNotification($"⚡ Plant struck at ({plant.X:F0}, {plant.Y:F0})", "#FFFF44");
            }
        }
    }

    private void MeteorShowerButton_Click(object sender, RoutedEventArgs e)
    {
        if (simulationEngine == null)
        {
            return;
        }

        var random = new Random();
        var allEntities = simulationEngine.World.GetAllEntities().ToList();

        if (allEntities.Count == 0)
        {
            ShowNotification("☄️ No entities to bombard!", "#FF8844");
            return;
        }

        for (int i = 0; i < GodPowerConstants.METEORSHOWERCOUNT; i++)
        {
            // Random impact point
            double impactX = random.NextDouble() * simulationEngine.World.Width;
            double impactY = random.NextDouble() * simulationEngine.World.Height;
            double impactRadius = GodPowerConstants.METEORIMPACTRADIUSPIXELS;

            // Damage entities within radius
            foreach (var entity in allEntities)
            {
                double distance = Math.Sqrt(Math.Pow(entity.X - impactX, 2) + Math.Pow(entity.Y - impactY, 2));
                if (distance <= impactRadius)
                {
                    double damage = GodPowerConstants.METEORBASEDAMAGE * (1 - (distance / impactRadius));
                    entity.TakeDamage(damage);

                    if (entity is Creature creature)
                    {
                        ShowNotification($"☄️ Meteor strikes {creature.GetType().Name} at ({creature.X:F0}, {creature.Y:F0})", "#FF8844");
                    }
                    else if (entity is Plant plant)
                    {
                        ShowNotification($"☄️ Meteor strikes plant at ({plant.X:F0}, {plant.Y:F0})", "#FF8844");
                    }
                }
            }
        }

        ShowNotification("☄️ Meteor shower rains destruction!", "#FF8844");
    }

    private void PlagueButton_Click(object sender, RoutedEventArgs e)
    {
        if (simulationEngine == null)
        {
            return;
        }

        var random = new Random();
        var allCreatures = simulationEngine.World.GetAllEntities().OfType<Creature>().ToList();
        int infectedCount = 0;

        if (allCreatures.Count == 0)
        {
            ShowNotification("💀 No creatures to plague!", "#AA44AA");
            return;
        }

        for (int i = 0; i < Math.Min(GodPowerConstants.PLAGUEINFECTIONCOUNT, allCreatures.Count); i++)
        {
            var creature = allCreatures[random.Next(allCreatures.Count)];
            creature.TakeDamage(GodPowerConstants.PLAGUEINITIALDAMAGE);

            ShowNotification($"💀 {creature.GetType().Name} infected with plague at ({creature.X:F0}, {creature.Y:F0})", "#AA44AA");
            infectedCount++;
        }

        ShowNotification($"💀 Plague unleashed! {infectedCount} creatures infected!", "#AA44AA");
    }

    private void FertilityBlessingButton_Click(object sender, RoutedEventArgs e)
    {
        if (simulationEngine == null)
        {
            return;
        }

        simulationEngine.ReproductionManager.HerbivoreReproductionChanceMultiplier *= GodPowerConstants.FERTILITYBLESSINGMULTIPLIER;
        simulationEngine.ReproductionManager.CarnivoreReproductionChanceMultiplier *= GodPowerConstants.FERTILITYBLESSINGMULTIPLIER;

        ShowNotification("🌸 Fertility blessing granted! Reproduction rates doubled for 30 seconds!", "#FF88FF");

        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(GodPowerConstants.FERTILITYBLESSINGDURATIONSECONDS) };
        timer.Tick += (s, args) =>
        {
            simulationEngine.ReproductionManager.HerbivoreReproductionChanceMultiplier /= GodPowerConstants.FERTILITYBLESSINGMULTIPLIER;
            simulationEngine.ReproductionManager.CarnivoreReproductionChanceMultiplier /= GodPowerConstants.FERTILITYBLESSINGMULTIPLIER;
            ShowNotification("🌸 Fertility blessing faded", "#FF88FF");
            timer.Stop();
        };
        timer.Start();
    }

    private void AbundanceButton_Click(object sender, RoutedEventArgs e)
    {
        if (simulationEngine == null)
        {
            return;
        }

        for (int i = 0; i < GodPowerConstants.ABUNDANCEPLANTCOUNT; i++)
        {
            simulationEngine.World.SpawnRandomPlant();
        }

        ShowNotification("🍎 Abundance bestowed! 10 extra plants created!", "#88FF88");
    }

    private void DivineProtectionButton_Click(object sender, RoutedEventArgs e)
    {
        if (simulationEngine == null)
        {
            return;
        }

        var allCreatures = simulationEngine.World.GetAllEntities().OfType<Creature>().ToList();

        if (allCreatures.Count == 0)
        {
            ShowNotification("🛡️ No creatures to protect!", "#8888FF");
            return;
        }

        // Grant temporary invulnerability
        foreach (var creature in allCreatures)
        {
            creature.Heal(100.0 - creature.Health); // Full heal
            // Note: In a full implementation, you'd add an invulnerability flag or damage reduction
        }

        ShowNotification($"🛡️ Divine protection granted to {allCreatures.Count} creatures!", "#8888FF");

        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(GodPowerConstants.DIVINEPROTECTIONDURATIONSECONDS) };
        timer.Tick += (s, args) =>
        {
            ShowNotification("🛡️ Divine protection faded", "#8888FF");
            timer.Stop();
        };
        timer.Start();
    }

    private void FamineButton_Click(object sender, RoutedEventArgs e)
    {
        if (simulationEngine == null)
        {
            return;
        }

        var allPlants = simulationEngine.World.Plants.ToList();

        if (allPlants.Count == 0)
        {
            ShowNotification("🏜️ No plants to wither!", "#FFAA44");
            return;
        }

        foreach (var plant in allPlants)
        {
            plant.TakeDamage(GodPowerConstants.FAMINEDAMAGE);
        }

        ShowNotification($"🏜️ Famine strikes! {allPlants.Count} plants withered!", "#FFAA44");
    }

    private void MadnessButton_Click(object sender, RoutedEventArgs e)
    {
        if (simulationEngine == null)
        {
            return;
        }

        var allCreatures = simulationEngine.World.GetAllEntities().OfType<Creature>().ToList();

        if (allCreatures.Count == 0)
        {
            ShowNotification("😵 No creatures to madden!", "#FF44FF");
            return;
        }

        var random = new Random();
        int maddenedCount = 0;

        for (int i = 0; i < Math.Min(GodPowerConstants.MADNESSTARGETCOUNT, allCreatures.Count); i++)
        {
            var creature = allCreatures[random.Next(allCreatures.Count)];
            // In a full implementation, this would alter behavior (e.g., random movement, attack allies)
            creature.TakeDamage(GodPowerConstants.MADNESSDAMAGE);
            maddenedCount++;
        }

        ShowNotification($"😵 Madness unleashed! {maddenedCount} creatures driven insane!", "#FF44FF");
    }

    private void StagnationButton_Click(object sender, RoutedEventArgs e)
    {
        if (simulationEngine == null)
        {
            return;
        }

        // Halt reproduction and growth temporarily
        simulationEngine.ReproductionManager.HerbivoreReproductionChanceMultiplier = 0;
        simulationEngine.ReproductionManager.CarnivoreReproductionChanceMultiplier = 0;
        simulationEngine.FoodManager.PlantSpawnChanceMultiplier = 0;

        ShowNotification("🕳️ Stagnation curse! No growth or reproduction for 60 seconds!", "#666666");

        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(GodPowerConstants.STAGNATIONDURATIONSECONDS) };
        timer.Tick += (s, args) =>
        {
            simulationEngine.ReproductionManager.HerbivoreReproductionChanceMultiplier = 1.0;
            simulationEngine.ReproductionManager.CarnivoreReproductionChanceMultiplier = 1.0;
            simulationEngine.FoodManager.PlantSpawnChanceMultiplier = 1.0;
            ShowNotification("🕳️ Stagnation lifted", "#666666");
            timer.Stop();
        };
        timer.Start();
    }

    private void ChangeBiomeButton_Click(object sender, RoutedEventArgs e)
    {
        if (simulationEngine == null)
        {
            return;
        }

        // Cycle through biomes (simplified - in full game, this would change terrain generation)
        var random = new Random();
        var biomes = new[] { "Forest", "Desert", "Tundra", "Jungle" };
        var newBiome = biomes[random.Next(biomes.Length)];

        ShowNotification($"🏔️ Biome changed to {newBiome}! Terrain regeneration begins.", "#88AAFF");
        // In full implementation, regenerate terrain based on biome
    }

    private void ChangeSeasonButton_Click(object sender, RoutedEventArgs e)
    {
        if (simulationEngine == null)
        {
            return;
        }

        // Force season change
        simulationEngine.SeasonCycle.SetSeason((Season)((int)(simulationEngine.SeasonCycle.CurrentSeason + 1) % 4));

        ShowNotification($"🌤️ Season forcibly changed to {simulationEngine.SeasonCycle.CurrentSeason}!", "#FFFF88");
    }

    private void FloodButton_Click(object sender, RoutedEventArgs e)
    {
        if (simulationEngine == null)
        {
            return;
        }

        // Convert random areas to water
        var random = new Random();
        for (int i = 0; i < GodPowerConstants.FLOODAREACOUNT; i++)
        {
            double x = random.NextDouble() * simulationEngine.World.Width;
            double y = random.NextDouble() * simulationEngine.World.Height;
            simulationEngine.World.SetTerrainAt(x, y, TerrainType.Water);
        }

        ShowNotification("🌊 Flood waters rise! Areas converted to aquatic terrain.", "#4488FF");
    }

    private void WeaknessButton_Click(object sender, RoutedEventArgs e)
    {
        if (simulationEngine == null)
        {
            return;
        }

        var allCreatures = simulationEngine.World.GetAllEntities().OfType<Creature>().ToList();

        if (allCreatures.Count == 0)
        {
            ShowNotification("💪 No creatures to weaken!", "#FF8844");
            return;
        }

        foreach (var creature in allCreatures)
        {
            creature.TakeDamage(creature.Health * (1 - GodPowerConstants.WEAKNESSHEALTHMULTIPLIER));
        }

        ShowNotification($"💪 Weakness curse affects {allCreatures.Count} creatures!", "#FF8844");
    }

    private void CorruptionButton_Click(object sender, RoutedEventArgs e)
    {
        if (simulationEngine == null)
        {
            return;
        }

        var allCreatures = simulationEngine.World.GetAllEntities().OfType<Creature>().ToList();

        if (allCreatures.Count == 0)
        {
            ShowNotification("😈 No creatures to corrupt!", "#880088");
            return;
        }

        var random = new Random();
        var factionTypes = Enum.GetValues<FactionType>().ToArray();
        int corruptedCount = 0;

        for (int i = 0; i < Math.Min(GodPowerConstants.CORRUPTIONTARGETCOUNT, allCreatures.Count); i++)
        {
            var creature = allCreatures[random.Next(allCreatures.Count)];
            var currentFaction = creature.Faction;

            // Pick a different random faction
            FactionType newFaction;
            do
            {
                newFaction = factionTypes[random.Next(factionTypes.Length)];
            }
            while (newFaction == currentFaction);

            creature.Faction = newFaction;
            corruptedCount++;
        }

        ShowNotification($"😈 Corruption spreads! {corruptedCount} creatures changed allegiance!", "#880088");
    }

    // God Painting Tools - Terrain Manipulation
    private void PaintLifeButton_Click(object sender, RoutedEventArgs e)
    {
        if (simulationEngine == null)
        {
            return;
        }

        // Enable life painting mode - convert terrain to faction-specific growth
        ShowNotification("🌱 Life Brush activated! Click to paint fertile growth.", "#44FF44");
        godPaintMode = GodPaintMode.Life;
    }

    private void PaintDeathButton_Click(object sender, RoutedEventArgs e)
    {
        if (simulationEngine == null)
        {
            return;
        }

        // Enable death painting mode - convert terrain to void
        ShowNotification("💀 Death Brush activated! Click to erase life.", "#444444");
        godPaintMode = GodPaintMode.Death;
    }

    private void PaintStoneButton_Click(object sender, RoutedEventArgs e)
    {
        if (simulationEngine == null)
        {
            return;
        }

        // Enable stone painting mode - convert terrain to stone
        ShowNotification("🪨 Stone Brush activated! Click to create fortifications.", "#888888");
        godPaintMode = GodPaintMode.Stone;
    }

    private void PaintWaterButton_Click(object sender, RoutedEventArgs e)
    {
        if (simulationEngine == null)
        {
            return;
        }

        // Enable water painting mode - convert terrain to water
        ShowNotification("🌊 Water Brush activated! Click to create aquatic zones.", "#4488FF");
        godPaintMode = GodPaintMode.Water;
    }

    private void UpdateFactionDisplay()
    {
        if (simulationEngine == null || FactionPopulationPanel == null) return;

        FactionPopulationPanel.Children.Clear();
        FactionPopulationPanel.Children.Add(CreateFactionHeader());

        foreach (var faction in simulationEngine.FactionManager.GetFactionsByPopulation())
        {
            if (faction.Population > 0)
            {
                FactionPopulationPanel.Children.Add(CreateFactionBar(faction));
            }
        }
    }

    private static TextBlock CreateFactionHeader()
    {
        return new TextBlock
        {
            Text = "Faction Populations",
            Foreground = new SolidColorBrush(Colors.White),
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 5)
        };
    }

    private static StackPanel CreateFactionBar(Faction faction)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 2, 0, 2)
        };

#pragma warning disable CS8604 // faction.Color and faction.Name are guaranteed to be non-null from faction initialization
        panel.Children.Add(CreateFactionColorIndicator(faction.Color));
        panel.Children.Add(CreateFactionText(faction.Name, faction.Population));
#pragma warning restore CS8604

        return panel;
    }

    private static System.Windows.Shapes.Rectangle CreateFactionColorIndicator(string color)
    {
        return new System.Windows.Shapes.Rectangle
        {
            Width = 12,
            Height = 12,
            Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color)),
            Margin = new Thickness(0, 0, 5, 0)
        };
    }

    private static TextBlock CreateFactionText(string name, int population)
    {
        return new TextBlock
        {
            Text = $"{name}: {population}",
            Foreground = new SolidColorBrush(Colors.White),
            FontSize = 10,
            VerticalAlignment = VerticalAlignment.Center
        };
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        // Open settings dialog
        var settingsDialog = new SettingsDialog();
        if (settingsDialog.ShowDialog() == true)
        {
            // Apply settings
            ApplySettings(settingsDialog);
        }
    }

    private void ApplySettings(SettingsDialog settings)
    {
        ApplySoundSettings(settings);
        ApplyDisplaySettings(settings);
        ApplyRendererSettings(settings);
        ApplySimulationSpeedSettings(settings);
        CheckAndNotifyRestartRequirements(settings);
    }

    private void ApplySoundSettings(SettingsDialog settings)
    {
        if (soundManager == null) return;

        soundManager.IsEnabled = settings.EnableSound;
        soundManager.MasterVolume = settings.MasterVolume;
        soundManager.SetMuted(!settings.EnableAmbientMusic);
    }

    private void ApplyDisplaySettings(SettingsDialog settings)
    {
        bool transparencyChanged = settings.TransparentBackground != (AllowsTransparency && Background == Brushes.Transparent);

        if (transparencyChanged)
        {
            MessageBox.Show(
                "Transparency changes will take effect after restarting the application.",
                "Settings Applied - Restart Required");
        }
    }

    private void ApplyRendererSettings(SettingsDialog settings)
    {
        renderer?.SetRenderQuality(settings.RenderQuality);
        renderer?.SetShowScenery(settings.ShowScenery);
        renderer?.SetShowShadows(settings.ShowEntityShadows);
    }

    private void ApplySimulationSpeedSettings(SettingsDialog settings)
    {
        if (!double.TryParse(settings.SimulationSpeed.Replace("x", string.Empty), out double speed))
        {
            return;
        }

        simulationEngine?.SetSimulationSpeed(speed);
        simulationSpeed = speed;
    }

    private void CheckAndNotifyRestartRequirements(SettingsDialog settings)
    {
        bool requiresRestart = false;
        string restartReasons = string.Empty;

        if (settings.WorldWidth != 800 || settings.WorldHeight != 600)
        {
            requiresRestart = true;
            restartReasons += "• World size changes\n";
        }

        if (settings.InitialPlants != 50 || settings.InitialHerbivores != 20 || settings.InitialCarnivores != 5)
        {
            requiresRestart = true;
            restartReasons += "• Initial entity counts\n";
        }

        if (requiresRestart)
        {
            MessageBox.Show(
                $"The following changes will take effect after restart:\n\n{restartReasons}",
                "Settings Applied");
        }
    }

    private void StatsButton_Click(object sender, RoutedEventArgs e)
    {
        // Open stats window
        if (simulationEngine == null || systemMonitor == null)
        {
            return;
        }

        var statsWindow = new StatsWindow(simulationEngine, systemMonitor);
        statsWindow.Owner = this;
        statsWindow.Show();
    }

    private void ChronicleButton_Click(object sender, RoutedEventArgs e)
    {
        // Open chronicle window
        if (simulationEngine == null)
        {
            return;
        }

        var chronicleWindow = new ChronicleWindow(simulationEngine);
        chronicleWindow.Owner = this;
        chronicleWindow.Show();
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        renderTimer?.Stop();
        systemMonitorTimer?.Stop();
        systemMonitor?.Dispose();
    }

    /// <summary>
    /// Disposes of managed resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes of managed resources.
    /// </summary>
    /// <param name="disposing">True if called from Dispose(), false if called from finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            renderTimer?.Stop();
            systemMonitorTimer?.Stop();
            systemMonitor?.Dispose();
            soundManager?.Dispose(); // Assuming SoundManager implements IDisposable
        }
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        renderTimer?.Stop();
        systemMonitorTimer?.Stop();
    }

    private void ShowEntityInfoAtPoint(Point point)
    {
        if (simulationEngine == null)
        {
            return;
        }

        // Check for plants
        foreach (var plant in simulationEngine.World.Plants)
        {
            double distance = Math.Sqrt(Math.Pow(plant.X - point.X, 2) + Math.Pow(plant.Y - point.Y, 2));
            if (distance <= UIConstants.ENTITYCLICKTOLERANCEPIXELS)
            {
                ShowEntityInfoDialog($"🌿 Plant #{plant.Id}",
                    $"Health: {plant.Health:F1}%\n" +
                    $"Size: {plant.Size:F0}\n" +
                    $"Age: {plant.Age:F1}s\n" +
                    $"Position: ({plant.X:F0}, {plant.Y:F0})");
                return;
            }
        }

        // Check for herbivores
        foreach (var herbivore in simulationEngine.World.Herbivores)
        {
            double distance = Math.Sqrt(Math.Pow(herbivore.X - point.X, 2) + Math.Pow(herbivore.Y - point.Y, 2));
            if (distance <= 25) // Click tolerance
            {
                ShowEntityInfoDialog($"🐰 Herbivore #{herbivore.Id}",
                    $"Health: {herbivore.Health:F1}%\n" +
                    $"Hunger: {herbivore.Hunger:F1}\n" +
                    $"Age: {herbivore.Age:F1}s\n" +
                    $"Speed: {herbivore.Speed:F2}\n" +
                    $"Position: ({herbivore.X:F0}, {herbivore.Y:F0})");
                return;
            }
        }

        // Check for carnivores
        foreach (var carnivore in simulationEngine.World.Carnivores)
        {
            double distance = Math.Sqrt(Math.Pow(carnivore.X - point.X, 2) + Math.Pow(carnivore.Y - point.Y, 2));
            if (distance <= 25) // Click tolerance
            {
                ShowEntityInfoDialog($"🐺 Carnivore #{carnivore.Id}",
                    $"Health: {carnivore.Health:F1}%\n" +
                    $"Hunger: {carnivore.Hunger:F1}\n" +
                    $"Age: {carnivore.Age:F1}s\n" +
                    $"Speed: {carnivore.Speed:F2}\n" +
                    $"Position: ({carnivore.X:F0}, {carnivore.Y:F0})");
                return;
            }
        }
    }

    private void ShowEntityInfoDialog(string title, string info)
    {
        var dialog = new Window
        {
            Title = title,
            Width = 300,
            Height = 200,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = this,
            ResizeMode = ResizeMode.NoResize
        };

        var textBlock = new System.Windows.Controls.TextBlock
        {
            Text = info,
            Margin = new Thickness(20),
            FontSize = 14,
            TextWrapping = TextWrapping.Wrap
        };

        var closeButton = new System.Windows.Controls.Button
        {
            Content = "Close",
            Width = 80,
            Height = 30,
            Margin = new Thickness(10),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom
        };

        closeButton.Click += (s, e) => dialog.Close();

        var grid = new System.Windows.Controls.Grid();
        grid.Children.Add(textBlock);
        grid.Children.Add(closeButton);

        dialog.Content = grid;
        dialog.ShowDialog();
    }

    private void RenderCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        Point clickPoint = e.GetPosition(RenderCanvas);

        // Handle god painting first
        if (godPaintMode != GodPaintMode.None && simulationEngine != null)
        {
            HandleGodPainting(clickPoint);
            return;
        }

        // Handle entity clicks for info/stats
        ShowEntityInfoAtPoint(clickPoint);
    }

    /// <summary>
    /// Handles god painting terrain manipulation.
    /// </summary>
    private void HandleGodPainting(Point clickPoint)
    {
        if (simulationEngine == null)
        {
            return;
        }

        TerrainType paintTerrain = godPaintMode switch
        {
            GodPaintMode.Life => TerrainType.VerdantGrowth, // Default to Verdant for life
            GodPaintMode.Death => TerrainType.Void,
            GodPaintMode.Stone => TerrainType.Stone,
            GodPaintMode.Water => TerrainType.Water,
            _ => TerrainType.Soil
        };

        // Paint in a small radius around the click point
        const int paintRadius = 3; // 3x3 grid cells
        int centerGridX = (int)(clickPoint.X / 20); // Assuming 20px cell size
        int centerGridY = (int)(clickPoint.Y / 20);

        for (int dx = -paintRadius; dx <= paintRadius; dx++)
        {
            for (int dy = -paintRadius; dy <= paintRadius; dy++)
            {
                double worldX = (centerGridX + dx) * 20;
                double worldY = (centerGridY + dy) * 20;

                // Only paint within world bounds
                if (worldX >= 0 && worldX < simulationEngine.World.Width &&
                    worldY >= 0 && worldY < simulationEngine.World.Height)
                {
                    simulationEngine.World.SetTerrainAt(worldX, worldY, paintTerrain);
                }
            }
        }

        // Show painting feedback
        string paintName = godPaintMode switch
        {
            GodPaintMode.Life => "Life",
            GodPaintMode.Death => "Death",
            GodPaintMode.Stone => "Stone",
            GodPaintMode.Water => "Water",
            _ => "Terrain"
        };

        ShowNotification($"🎨 Painted {paintName} at ({clickPoint.X:F0}, {clickPoint.Y:F0})", "#FFFFFF");
    }

    private void ShowNotification(string message, string colorHex = "#FFFFFF")
    {
        if (NotificationPanel == null)
        {
            return;
        }

        var textBlock = new TextBlock
        {
            Text = message,
            Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString(colorHex)!,
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(0, 0, 0, 5)
        };

        NotificationPanel.Children.Add(textBlock);

        // Auto-remove after 3 seconds
        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
        timer.Tick += (s, e) =>
        {
            NotificationPanel.Children.Remove(textBlock);
            timer.Stop();
        };
        timer.Start();
    }
}

/// <summary>
/// God painting modes for terrain manipulation.
/// </summary>
public enum GodPaintMode
{
    None,
    Life,
    Death,
    Stone,
    Water
}
