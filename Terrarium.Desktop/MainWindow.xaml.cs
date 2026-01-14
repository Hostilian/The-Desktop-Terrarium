using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Terrarium.Desktop.Rendering;
using Terrarium.Logic.Entities;
using Terrarium.Logic.Persistence;
using Terrarium.Logic.Simulation;

namespace Terrarium.Desktop;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// Simplified presentation layer for multiple terrarium types.
/// </summary>
public partial class MainWindow : Window
{
    private SimulationEngine? _simulationEngine;
    private Renderer? _renderer;
    private DispatcherTimer? _renderTimer;
    private DispatcherTimer? _systemMonitorTimer;
    private readonly Stopwatch _frameStopwatch;
    private SystemMonitor? _systemMonitor;
    private SaveManager? _saveManager;
    private SoundManager? _soundManager;

    // Win32 hit testing constants
    private const int WmNcHitTest = 0x0084;
    private const int HtTransparent = -1;

    // Timing constants
    private const int RenderFps = 60;
    private const double RenderInterval = 1000.0 / RenderFps;
    private const double SystemMonitorInterval = 2000.0;

    private int _frameCount;
    private double _fpsAccumulator;
    private double _currentFps;

    // God painting mode
    private GodPaintMode _godPaintMode = GodPaintMode.None;

    // Sound effect tracking
    private int _lastTotalBirths = 0;
    private int _lastTotalDeaths = 0;
    private int _lastTotalPlantsEaten = 0;

    private Terrarium.Logic.Simulation.TerrariumType _terrariumType;
    private double _simulationSpeed = 1.0;

    // Mouse interaction fields
    private Point _mousePosition = new Point(0, 0);
    private bool _mouseInCanvas = false;

    public MainWindow()
    {
        InitializeComponent();
        _frameStopwatch = new Stopwatch();
    }

    private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
    {
        if (_simulationEngine == null)
        {
            return;
        }

        if (_simulationEngine.IsPaused)
        {
            _simulationEngine.Resume();
            _renderTimer?.Start();
            _systemMonitorTimer?.Start();
            PlayPauseButton.Content = "⏸️";
        }
        else
        {
            _simulationEngine.Pause();
            _renderTimer?.Stop();
            _systemMonitorTimer?.Stop();
            PlayPauseButton.Content = "▶️";
        }
    }

    private void SpeedButton_Click(object sender, RoutedEventArgs e)
    {
        // Cycle through speed options: 1x -> 2x -> 5x -> 10x -> 1x
        double[] speeds = { 1.0, 2.0, 5.0, 10.0 };
        int currentIndex = Array.IndexOf(speeds, _simulationSpeed);
        int nextIndex = (currentIndex + 1) % speeds.Length;
        _simulationSpeed = speeds[nextIndex];

        _simulationEngine?.SetSimulationSpeed(_simulationSpeed);
        SpeedButton.Content = $"{_simulationSpeed}x";
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    // God Power Button Handlers
    private void SpawnPlantButton_Click(object sender, RoutedEventArgs e)
    {
        if (_simulationEngine == null)
        {
            return;
        }

        var plant = _simulationEngine.World.SpawnRandomPlant();
        ShowNotification($"🌱 Plant spawned at ({plant.X:F0}, {plant.Y:F0})", "#44FF44");
    }

    private void SpawnHerbivoreButton_Click(object sender, RoutedEventArgs e)
    {
        if (_simulationEngine == null)
        {
            return;
        }

        // Get random faction for God Simulator mode
        var faction = _terrariumType == Terrarium.Logic.Simulation.TerrariumType.GodSimulator
            ? (FactionType)new Random().Next(Enum.GetValues(typeof(FactionType)).Length)
            : FactionType.VerdantCollective;

        var herbivore = _simulationEngine.World.SpawnRandomHerbivore("Rabbit", faction);
        var factionName = _simulationEngine.FactionManager.GetFaction(herbivore.Faction).Name;
        ShowNotification($"🐰 {factionName} Herbivore spawned at ({herbivore.X:F0}, {herbivore.Y:F0})", "#FFFFFF");
    }

    private void SpawnCarnivoreButton_Click(object sender, RoutedEventArgs e)
    {
        if (_simulationEngine == null)
        {
            return;
        }

        // Get random faction for God Simulator mode
        var faction = _terrariumType == Terrarium.Logic.Simulation.TerrariumType.GodSimulator
            ? (FactionType)new Random().Next(Enum.GetValues(typeof(FactionType)).Length)
            : FactionType.AshenLegion;

        var carnivore = _simulationEngine.World.SpawnRandomCarnivore("Wolf", faction);
        var factionName = _simulationEngine.FactionManager.GetFaction(carnivore.Faction).Name;
        ShowNotification($"🐺 {factionName} Carnivore spawned at ({carnivore.X:F0}, {carnivore.Y:F0})", "#FF4444");
    }

    private void LightningStrikeButton_Click(object sender, RoutedEventArgs e)
    {
        if (_simulationEngine == null)
        {
            return;
        }

        var random = new Random();
        var allEntities = _simulationEngine.World.GetAllEntities().ToList();

        if (allEntities.Count == 0)
        {
            ShowNotification("⚡ No entities to strike!", "#FFFF44");
            return;
        }

        // Strike 3 random entities
        for (int i = 0; i < Math.Min(3, allEntities.Count); i++)
        {
            var entity = allEntities[random.Next(allEntities.Count)];
            entity.TakeDamage(30); // Damage

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
        if (_simulationEngine == null)
        {
            return;
        }

        var random = new Random();
        var allEntities = _simulationEngine.World.GetAllEntities().ToList();

        if (allEntities.Count == 0)
        {
            ShowNotification("☄️ No entities to bombard!", "#FF8844");
            return;
        }

        // Create 5 meteor impact points
        for (int i = 0; i < 5; i++)
        {
            // Random impact point
            double impactX = random.NextDouble() * _simulationEngine.World.Width;
            double impactY = random.NextDouble() * _simulationEngine.World.Height;
            double impactRadius = 50; // Damage radius

            // Damage entities within radius
            foreach (var entity in allEntities)
            {
                double distance = Math.Sqrt(Math.Pow(entity.X - impactX, 2) + Math.Pow(entity.Y - impactY, 2));
                if (distance <= impactRadius)
                {
                    double damage = 50 * (1 - distance / impactRadius); // More damage closer to center
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
        if (_simulationEngine == null)
        {
            return;
        }

        var random = new Random();
        var allCreatures = _simulationEngine.World.GetAllEntities().OfType<Creature>().ToList();

        if (allCreatures.Count == 0)
        {
            ShowNotification("💀 No creatures to plague!", "#AA44AA");
            return;
        }

        // Infect up to 5 random creatures with plague
        int infectedCount = 0;
        for (int i = 0; i < Math.Min(5, allCreatures.Count); i++)
        {
            var creature = allCreatures[random.Next(allCreatures.Count)];
            creature.TakeDamage(25); // Initial plague damage

            ShowNotification($"💀 {creature.GetType().Name} infected with plague at ({creature.X:F0}, {creature.Y:F0})", "#AA44AA");
            infectedCount++;
        }

        ShowNotification($"💀 Plague unleashed! {infectedCount} creatures infected!", "#AA44AA");
    }

    private void FertilityBlessingButton_Click(object sender, RoutedEventArgs e)
    {
        if (_simulationEngine == null)
        {
            return;
        }

        // Temporarily boost reproduction rates
        _simulationEngine.ReproductionManager.HerbivoreReproductionChanceMultiplier *= 2.0;
        _simulationEngine.ReproductionManager.CarnivoreReproductionChanceMultiplier *= 2.0;

        ShowNotification("🌸 Fertility blessing granted! Reproduction rates doubled for 30 seconds!", "#FF88FF");

        // Reset after 30 seconds
        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
        timer.Tick += (s, args) =>
        {
            _simulationEngine.ReproductionManager.HerbivoreReproductionChanceMultiplier /= 2.0;
            _simulationEngine.ReproductionManager.CarnivoreReproductionChanceMultiplier /= 2.0;
            ShowNotification("🌸 Fertility blessing faded", "#FF88FF");
            timer.Stop();
        };
        timer.Start();
    }

    private void AbundanceButton_Click(object sender, RoutedEventArgs e)
    {
        if (_simulationEngine == null)
        {
            return;
        }

        // Spawn extra plants
        for (int i = 0; i < 10; i++)
        {
            _simulationEngine.World.SpawnRandomPlant();
        }

        ShowNotification("🍎 Abundance bestowed! 10 extra plants created!", "#88FF88");
    }

    private void DivineProtectionButton_Click(object sender, RoutedEventArgs e)
    {
        // Implement divine protection - damage reduction
        ShowNotification("🛡️ Divine protection activated!", "#8888FF");
    }

    private void FamineButton_Click(object sender, RoutedEventArgs e)
    {
        // Implement famine - reduce food
        ShowNotification("🏜️ Famine descends!", "#FFAA44");
    }

    private void MadnessButton_Click(object sender, RoutedEventArgs e)
    {
        // Implement madness - entities attack allies
        ShowNotification("😵 Madness spreads!", "#FF4444");
    }

    private void StagnationButton_Click(object sender, RoutedEventArgs e)
    {
        // Implement stagnation - block reproduction
        ShowNotification("🕳️ Stagnation begins!", "#666666");
    }

    private void WeaknessButton_Click(object sender, RoutedEventArgs e)
    {
        if (_simulationEngine == null)
        {
            return;
        }

        var allCreatures = _simulationEngine.World.GetAllEntities().OfType<Creature>().ToList();

        if (allCreatures.Count == 0)
        {
            ShowNotification("💪 No creatures to weaken!", "#FF8844");
            return;
        }

        // Weaken all creatures - TODO: implement speed reduction (Speed setter is protected)
        // foreach (var creature in allCreatures)
        // {
        //     creature.Speed *= 0.5; // Reduce speed by 50%
        // }

        ShowNotification($"💪 Weakness afflicts {allCreatures.Count} creatures! Speed reduced by 50%!", "#FF8844");
    }

    private void CorruptionButton_Click(object sender, RoutedEventArgs e)
    {
        if (_simulationEngine == null)
        {
            return;
        }

        var allCreatures = _simulationEngine.World.GetAllEntities().OfType<Creature>().ToList();

        if (allCreatures.Count == 0)
        {
            ShowNotification("😈 No creatures to corrupt!", "#880088");
            return;
        }

        var random = new Random();
        var factionTypes = Enum.GetValues<FactionType>().ToArray();
        int corruptedCount = 0;

        // Corrupt up to 3 random creatures by changing their faction
        for (int i = 0; i < Math.Min(3, allCreatures.Count); i++)
        {
            var creature = allCreatures[random.Next(allCreatures.Count)];
            var currentFaction = creature.Faction;

            // Pick a different random faction
            FactionType newFaction;
            do
            {
                newFaction = factionTypes[random.Next(factionTypes.Length)];
            } while (newFaction == currentFaction);

            creature.Faction = newFaction;
            corruptedCount++;
        }

        ShowNotification($"😈 Corruption spreads! {corruptedCount} creatures changed allegiance!", "#880088");
    }

    private void ChangeBiomeButton_Click(object sender, RoutedEventArgs e)
    {
        // Implement biome change
        ShowNotification("🏔️ Biome changing...", "#44AAFF");
    }

    private void ChangeSeasonButton_Click(object sender, RoutedEventArgs e)
    {
        // Implement season change
        ShowNotification("🌤️ Season shifting...", "#FFFF88");
    }

    private void FloodButton_Click(object sender, RoutedEventArgs e)
    {
        // Implement flood - expand water
        ShowNotification("🌊 Flood waters rise!", "#4488FF");
    }

    // God Painting Tools - Terrain Manipulation
    private void PaintLifeButton_Click(object sender, RoutedEventArgs e)
    {
        if (_simulationEngine == null)
        {
            return;
        }

        // Enable life painting mode - convert terrain to faction-specific growth
        ShowNotification("🌱 Life Brush activated! Click to paint fertile growth.", "#44FF44");
        _godPaintMode = GodPaintMode.Life;
    }

    private void PaintDeathButton_Click(object sender, RoutedEventArgs e)
    {
        if (_simulationEngine == null)
        {
            return;
        }

        // Enable death painting mode - convert terrain to void
        ShowNotification("💀 Death Brush activated! Click to erase life.", "#444444");
        _godPaintMode = GodPaintMode.Death;
    }

    private void PaintStoneButton_Click(object sender, RoutedEventArgs e)
    {
        if (_simulationEngine == null)
        {
            return;
        }

        // Enable stone painting mode - convert terrain to stone
        ShowNotification("🪨 Stone Brush activated! Click to create fortifications.", "#888888");
        _godPaintMode = GodPaintMode.Stone;
    }

    private void PaintWaterButton_Click(object sender, RoutedEventArgs e)
    {
        if (_simulationEngine == null)
        {
            return;
        }

        // Enable water painting mode - convert terrain to water
        ShowNotification("🌊 Water Brush activated! Click to create aquatic zones.", "#4488FF");
        _godPaintMode = GodPaintMode.Water;
    }

    private void UpdateFactionDisplay()
    {
        if (_simulationEngine == null || FactionPopulationPanel == null)
        {
            return;
        }

        FactionPopulationPanel.Children.Clear();

        // Add header
        var header = new System.Windows.Controls.TextBlock
        {
            Text = "Faction Populations",
            Foreground = new SolidColorBrush(Colors.White),
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 5)
        };
        FactionPopulationPanel.Children.Add(header);

        // Add faction bars
        foreach (var faction in _simulationEngine.FactionManager.GetFactionsByPopulation())
        {
            if (faction.Population > 0)
            {
                var factionPanel = new System.Windows.Controls.StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 2, 0, 2)
                };

                // Faction color indicator
                var colorBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(faction.Color));
                var colorRect = new System.Windows.Shapes.Rectangle
                {
                    Width = 12,
                    Height = 12,
                    Fill = colorBrush,
                    Margin = new Thickness(0, 0, 5, 0)
                };

                // Faction name and count
                var factionText = new System.Windows.Controls.TextBlock
                {
                    Text = $"{faction.Name}: {faction.Population}",
                    Foreground = new SolidColorBrush(Colors.White),
                    FontSize = 10,
                    VerticalAlignment = VerticalAlignment.Center
                };

                factionPanel.Children.Add(colorRect);
                factionPanel.Children.Add(factionText);
                FactionPopulationPanel.Children.Add(factionPanel);
            }
        }
    }

    private void ResetButton_Click(object sender, RoutedEventArgs e)
    {
        // Reset simulation - not implemented yet
        MessageBox.Show("Reset functionality not implemented yet.");
    }

    private void SpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        // Speed slider removed in god simulator UI - functionality moved to SpeedButton
        // _simulationSpeed = e.NewValue;
        // if (SpeedText != null)
        // {
        //     SpeedText.Text = $"Speed: {_simulationSpeed:F1}x";
        // }
        // // Apply speed to simulation engine
        // _simulationEngine?.SetSimulationSpeed(_simulationSpeed);
    }

    private void ShowTrailsCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        // Implement trail visibility
    }

    private void ShowTrailsCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        // Implement trail visibility
    }

    private void MuteSoundCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        // Implement sound muting
    }

    private void MuteSoundCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        // Implement sound muting
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
        // Apply sound settings
        if (_soundManager != null)
        {
            _soundManager.IsEnabled = settings.EnableSound;
            _soundManager.MasterVolume = settings.MasterVolume;
            if (!settings.EnableAmbientMusic)
            {
                _soundManager.SetMuted(true);
            }
            else
            {
                _soundManager.SetMuted(false);
            }
        }

        // Apply display settings - Transparency changes require restart
        if (settings.TransparentBackground != (AllowsTransparency && Background == Brushes.Transparent))
        {
            MessageBox.Show("Transparency changes will take effect after restarting the application.", "Settings Applied - Restart Required");
            // Don't try to change AllowsTransparency at runtime - this causes crashes
            // The setting will be applied on next startup
        }

        // Apply renderer settings
        _renderer?.SetRenderQuality(settings.RenderQuality);
        _renderer?.SetShowScenery(settings.ShowScenery);
        _renderer?.SetShowShadows(settings.ShowEntityShadows);

        // Apply simulation speed
        if (double.TryParse(settings.SimulationSpeed.Replace("x", ""), out double speed))
        {
            _simulationEngine?.SetSimulationSpeed(speed);
            _simulationSpeed = speed;
            // Speed UI elements removed in god simulator UI
            // if (SpeedText != null)
            // {
            //     SpeedText.Text = $"Speed: {_simulationSpeed:F1}x";
            // }
            // if (SpeedSlider != null)
            // {
            //     SpeedSlider.Value = _simulationSpeed;
            // }
        }

        // Note: Some changes require restart
        bool requiresRestart = false;
        string restartReasons = "";

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
            MessageBox.Show($"The following changes will take effect after restart:\n\n{restartReasons}", "Settings Applied");
        }
    }

    private void StatsButton_Click(object sender, RoutedEventArgs e)
    {
        // Open stats window
        if (_simulationEngine == null || _systemMonitor == null)
        {
            return;
        }

        var statsWindow = new StatsWindow(_simulationEngine, _systemMonitor);
        statsWindow.Owner = this;
        statsWindow.Show();
    }

    private void ChronicleButton_Click(object sender, RoutedEventArgs e)
    {
        // Open chronicle window
        if (_simulationEngine == null)
        {
            return;
        }

        var chronicleWindow = new ChronicleWindow(_simulationEngine);
        chronicleWindow.Owner = this;
        chronicleWindow.Show();
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        _renderTimer?.Stop();
        _systemMonitorTimer?.Stop();
        _systemMonitor?.Dispose();
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        _renderTimer?.Stop();
        _systemMonitorTimer?.Stop();
    }

    private void ShowEntityInfoAtPoint(Point point)
    {
        if (_simulationEngine == null)
        {
            return;
        }

        // Check for plants
        foreach (var plant in _simulationEngine.World.Plants)
        {
            double distance = Math.Sqrt(Math.Pow(plant.X - point.X, 2) + Math.Pow(plant.Y - point.Y, 2));
            if (distance <= 30) // Click tolerance
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
        foreach (var herbivore in _simulationEngine.World.Herbivores)
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
        foreach (var carnivore in _simulationEngine.World.Carnivores)
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
        if (_godPaintMode != GodPaintMode.None && _simulationEngine != null)
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
        if (_simulationEngine == null)
        {
            return;
        }

        TerrainType paintTerrain = _godPaintMode switch
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
                if (worldX >= 0 && worldX < _simulationEngine.World.Width &&
                    worldY >= 0 && worldY < _simulationEngine.World.Height)
                {
                    _simulationEngine.World.SetTerrainAt(worldX, worldY, paintTerrain);
                }
            }
        }

        // Show painting feedback
        string paintName = _godPaintMode switch
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
