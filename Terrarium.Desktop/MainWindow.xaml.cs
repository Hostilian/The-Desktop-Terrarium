using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Terrarium.Desktop.Rendering;
using Terrarium.Logic.Simulation;
using Terrarium.Logic.Persistence;
using System.Diagnostics;
using System.Collections.Generic;
using System.Windows.Interop;

namespace Terrarium.Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// Main presentation layer - handles UI and delegates logic to SimulationEngine.
    /// </summary>
    public partial class MainWindow : Window
    {
        private SimulationEngine? _simulationEngine;
        private Renderer? _renderer;
        private WeatherEffects? _weatherEffects;
        private SoundManager? _soundManager;
        private ParticleSystem? _particleSystem;
        private NotificationManager? _notificationManager;
        private TooltipManager? _tooltipManager;
        private SettingsPanel? _settingsPanel;
        private MiniMap? _miniMap;
        private AchievementSystem? _achievementSystem;
        private CreatureMoodIndicator? _moodIndicator;
        private PopulationGraph? _populationGraph;
        private PredatorWarningSystem? _predatorWarning;
        private EntitySelector? _entitySelector;
        private SpeedIndicator? _speedIndicator;
        private BreedingIndicator? _breedingIndicator;
        private EcosystemHealthBar? _ecosystemHealthBar;
        private DispatcherTimer? _renderTimer;
        private DispatcherTimer? _systemMonitorTimer;
        private Stopwatch _frameStopwatch;
        private SystemMonitor? _systemMonitor;
        private SaveManager? _saveManager;
        private bool _isFollowingEntity;

        // Weather from CPU constants
        private const double CpuStormStartThreshold = 0.70;
        private const double CpuStormMaxThreshold = 1.00;
        private const double StormyWeatherThreshold = 0.50;

        // Win32 hit testing constants
        private const int WmNcHitTest = 0x0084;
        private const int HtTransparent = -1;

        // Win32 hotkey constants
        private const int WmHotKey = 0x0312;
        private const uint ModAlt = 0x0001;
        private const uint ModControl = 0x0002;
        private const uint ModNoRepeat = 0x4000;

        private const int HotkeySave = 1;
        private const int HotkeyLoad = 2;
        private const int HotkeyToggleStatus = 3;

        // Win32 window styles
        private const int GwlExStyle = -20;
        private const int WsExToolWindow = 0x00000080;
        private const int WsExNoActivate = 0x08000000;

        // Timing constants
        private const int RenderFps = 60;
        private const double RenderInterval = 1000.0 / RenderFps; // milliseconds
        private const double SystemMonitorInterval = 2000.0; // milliseconds

        // Visual tuning constants (uses existing BackgroundRect)
        private const double CalmBackgroundOpacity = 0.0;
        private const double StormMaxBackgroundOpacity = 1.0;
        private const double NightBackgroundOpacity = 0.6;

        private int _frameCount;
        private double _fpsAccumulator;
        private double _currentFps;

        public MainWindow()
        {
            InitializeComponent();
            _frameStopwatch = new Stopwatch();

            SourceInitialized += (_, _) =>
            {
                if (PresentationSource.FromVisual(this) is HwndSource hwndSource)
                {
                    ApplyNoActivateStyles(hwndSource.Handle);
                    RegisterGlobalHotkeys(hwndSource.Handle);
                    hwndSource.AddHook(WndProc);
                }
            };
        }

        private void RegisterGlobalHotkeys(IntPtr hwnd)
        {
            RegisterHotKey(hwnd, HotkeySave, ModControl | ModAlt | ModNoRepeat,
                (uint)KeyInterop.VirtualKeyFromKey(Key.S));

            RegisterHotKey(hwnd, HotkeyLoad, ModControl | ModAlt | ModNoRepeat,
                (uint)KeyInterop.VirtualKeyFromKey(Key.L));

            RegisterHotKey(hwnd, HotkeyToggleStatus, ModControl | ModAlt | ModNoRepeat,
                (uint)KeyInterop.VirtualKeyFromKey(Key.F1));
        }

        private void UnregisterGlobalHotkeys(IntPtr hwnd)
        {
            UnregisterHotKey(hwnd, HotkeySave);
            UnregisterHotKey(hwnd, HotkeyLoad);
            UnregisterHotKey(hwnd, HotkeyToggleStatus);
        }

        private static void ApplyNoActivateStyles(IntPtr hwnd)
        {
            IntPtr exStylePtr = GetWindowLongPtr(hwnd, GwlExStyle);
            long exStyle = exStylePtr.ToInt64();

            exStyle |= WsExNoActivate;
            exStyle |= WsExToolWindow;

            SetWindowLongPtr(hwnd, GwlExStyle, new IntPtr(exStyle));
        }

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WmHotKey)
            {
                int hotkeyId = wParam.ToInt32();
                switch (hotkeyId)
                {
                    case HotkeySave:
                        SaveGame();
                        handled = true;
                        return IntPtr.Zero;

                    case HotkeyLoad:
                        LoadGame();
                        handled = true;
                        return IntPtr.Zero;

                    case HotkeyToggleStatus:
                        ToggleUIVisibility();
                        handled = true;
                        return IntPtr.Zero;
                }
            }

            if (msg != WmNcHitTest)
            {
                return IntPtr.Zero;
            }

            // If we haven't started, don't block input.
            if (_simulationEngine == null)
            {
                handled = true;
                return new IntPtr(HtTransparent);
            }

            // Screen coords packed into lParam.
            int x = GetSignedLowWord(lParam);
            int y = GetSignedHighWord(lParam);

            Point screenPoint = new Point(x, y);
            Point windowPoint = PointFromScreen(screenPoint);
            Point canvasPoint = TranslateToCanvasPoint(windowPoint);

            var clickable = _simulationEngine.FindClickableAt(canvasPoint.X, canvasPoint.Y);
            if (clickable == null)
            {
                handled = true;
                return new IntPtr(HtTransparent);
            }

            return IntPtr.Zero;
        }

        private static int GetSignedLowWord(IntPtr ptr)
        {
            int value = unchecked((short)((long)ptr & 0xFFFF));
            return value;
        }

        private static int GetSignedHighWord(IntPtr ptr)
        {
            int value = unchecked((short)(((long)ptr >> 16) & 0xFFFF));
            return value;
        }

        private Point TranslateToCanvasPoint(Point windowPoint)
        {
            // RenderCanvas is inside the window; translate the point.
            return RenderCanvas.TransformToAncestor(this).Inverse.Transform(windowPoint);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PositionWindowAtBottom();
            InitializeSimulation();
            InitializeRendering();
            InitializeSaveSystem();
            InitializeSystemMonitoring();
            StartSimulation();
        }

        /// <summary>
        /// Positions the window at the bottom of the screen.
        /// </summary>
        private void PositionWindowAtBottom()
        {
            var screenHeight = SystemParameters.PrimaryScreenHeight;
            var screenWidth = SystemParameters.PrimaryScreenWidth;

            Width = screenWidth;
            Left = 0;
            Top = screenHeight - Height;
        }

        /// <summary>
        /// Initializes the simulation engine.
        /// </summary>
        private void InitializeSimulation()
        {
            _simulationEngine = new SimulationEngine(Width, Height);
            _simulationEngine.Initialize();
        }

        /// <summary>
        /// Initializes the rendering system.
        /// </summary>
        private void InitializeRendering()
        {
            _renderer = new Renderer(RenderCanvas);
            _weatherEffects = new WeatherEffects(RenderCanvas);
            _soundManager = new SoundManager();
            _particleSystem = new ParticleSystem(RenderCanvas);
            _notificationManager = new NotificationManager(RenderCanvas);
            _tooltipManager = new TooltipManager(RenderCanvas);
            _settingsPanel = new SettingsPanel(RenderCanvas);
            _miniMap = new MiniMap(RenderCanvas);
            _achievementSystem = new AchievementSystem(RenderCanvas);
            _moodIndicator = new CreatureMoodIndicator(RenderCanvas);
            _populationGraph = new PopulationGraph(RenderCanvas);
            _predatorWarning = new PredatorWarningSystem(RenderCanvas);
            _entitySelector = new EntitySelector(RenderCanvas);
            _speedIndicator = new SpeedIndicator(RenderCanvas);
            _breedingIndicator = new BreedingIndicator(RenderCanvas);
            _ecosystemHealthBar = new EcosystemHealthBar(RenderCanvas);

            // Connect achievement system events
            _achievementSystem.OnAchievementUnlocked += (id, name) =>
            {
                _notificationManager?.Notify($"🏆 Achievement: {name}!", NotificationType.Achievement);
            };

            // Connect entity selector events
            _entitySelector.OnEntitySelected += (sender, entity) =>
            {
                _notificationManager?.Notify($"Selected: {GetEntityName(entity)}", NotificationType.Info);
            };

            // Connect settings panel events
            _settingsPanel.SimulationSpeedChanged += speed =>
            {
                if (_simulationEngine != null)
                    _simulationEngine.SimulationSpeed = speed;
            };

            _settingsPanel.ParticlesToggled += enabled =>
            {
                if (_particleSystem != null)
                    _particleSystem.IsEnabled = enabled;
            };

            _settingsPanel.NotificationsToggled += enabled =>
            {
                if (_notificationManager != null)
                    _notificationManager.IsEnabled = enabled;
            };

            _settingsPanel.WeatherEffectsToggled += enabled =>
            {
                if (_weatherEffects != null)
                    _weatherEffects.IsEnabled = enabled;
            };

            _settingsPanel.SoundToggled += enabled =>
            {
                if (_soundManager != null)
                    _soundManager.IsEnabled = enabled;
            };

            // Subscribe to simulation events for particles and notifications
            if (_simulationEngine != null)
            {
                _simulationEngine.EventSystem.OnCreatureBorn += (creature, parent1, parent2) =>
                {
                    _particleSystem?.SpawnBirthEffect(creature.X, creature.Y);
                    string creatureType = creature is Terrarium.Logic.Entities.Herbivore h ? h.Type :
                                         creature is Terrarium.Logic.Entities.Carnivore c ? c.Type : "Creature";
                    _notificationManager?.NotifyBirth(creatureType);
                };

                _simulationEngine.EventSystem.OnCreatureDied += (creature, cause) =>
                {
                    _particleSystem?.SpawnDeathEffect(creature.X, creature.Y);
                    string creatureType = creature is Terrarium.Logic.Entities.Herbivore h ? h.Type :
                                         creature is Terrarium.Logic.Entities.Carnivore c ? c.Type : "Creature";
                    _notificationManager?.NotifyDeath(creatureType, cause);
                };

                _simulationEngine.EventSystem.OnPlantEaten += (plant, creature) =>
                {
                    _particleSystem?.SpawnEatEffect(plant.X, plant.Y);
                };

                _simulationEngine.EventSystem.OnDayPhaseChanged += phase =>
                {
                    _notificationManager?.NotifyDayPhaseChange(phase);
                };

                _simulationEngine.EventSystem.OnMilestoneReached += (name, value) =>
                {
                    _notificationManager?.NotifyMilestone(name, value);
                };
            }

            // Setup render timer (60 FPS)
            _renderTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(RenderInterval)
            };
            _renderTimer.Tick += RenderTimer_Tick;
        }

        /// <summary>
        /// Initializes system monitoring for weather effects.
        /// </summary>
        private void InitializeSystemMonitoring()
        {
            _systemMonitor = new SystemMonitor();

            _systemMonitorTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(SystemMonitorInterval)
            };
            _systemMonitorTimer.Tick += SystemMonitorTimer_Tick;
        }

        /// <summary>
        /// Initializes save/load system.
        /// </summary>
        private void InitializeSaveSystem()
        {
            _saveManager = new SaveManager();

            // Try to load existing save file
            if (_saveManager.SaveFileExists())
            {
                try
                {
                    var loadedWorld = _saveManager.LoadWorld();
                    if (_simulationEngine != null)
                    {
                        _simulationEngine = new SimulationEngine(loadedWorld);
                    }
                }
                catch
                {
                    // If load fails, continue with new world
                }
            }
        }

        /// <summary>
        /// Starts the simulation and rendering.
        /// </summary>
        private void StartSimulation()
        {
            _frameStopwatch.Start();
            _renderTimer?.Start();
            _systemMonitorTimer?.Start();
        }

        /// <summary>
        /// Main render loop (called 60 times per second).
        /// </summary>
        private void RenderTimer_Tick(object? sender, EventArgs e)
        {
            if (_simulationEngine == null || _renderer == null) return;

            // Calculate delta time
            double deltaTime = _frameStopwatch.Elapsed.TotalSeconds;
            _frameStopwatch.Restart();

            // Update simulation
            _simulationEngine.Update(deltaTime);

            // Update weather effects
            _weatherEffects?.Update(deltaTime, _simulationEngine.WeatherIntensity);

            // Update particle system
            _particleSystem?.Update(deltaTime);

            // Update notifications
            _notificationManager?.Update(deltaTime);

            // Update tooltips (live data refresh)
            _tooltipManager?.Update();

            // Update sound manager
            _soundManager?.Update(deltaTime, _simulationEngine.DayNightCycle.IsDay, _simulationEngine.WeatherIntensity);

            // Update mini-map
            _miniMap?.Update(_simulationEngine.World, RenderCanvas.ActualWidth, RenderCanvas.ActualHeight);

            // Update new visual systems
            var herbivores = _simulationEngine.World.Herbivores;
            var carnivores = _simulationEngine.World.Carnivores;
            var plants = _simulationEngine.World.Plants;

            _moodIndicator?.Update(deltaTime, herbivores, carnivores);
            _populationGraph?.Update(deltaTime, plants.Count, herbivores.Count, carnivores.Count);
            _predatorWarning?.Update(deltaTime, herbivores, carnivores);
            _entitySelector?.Update(deltaTime);
            _speedIndicator?.SetSpeed(_simulationEngine.SimulationSpeed);
            _speedIndicator?.Update(deltaTime);
            _breedingIndicator?.Update(deltaTime, herbivores, carnivores);
            _ecosystemHealthBar?.Update(deltaTime, plants.Count, herbivores.Count, carnivores.Count);

            // Check achievements
            var stats = _simulationEngine.Statistics;
            _achievementSystem?.CheckAchievements(
                stats.TotalBirths,
                stats.TotalDeaths,
                stats.PeakPopulation,
                stats.CurrentPlants,
                stats.CurrentHerbivores,
                stats.CurrentCarnivores,
                stats.SessionTime);

            // Render
            _renderer.Clear();
            _renderer.RenderWorld(_simulationEngine.World, _simulationEngine.WeatherIntensity);

            // Update FPS counter
            UpdateFpsCounter(deltaTime);
            UpdateStatusDisplay();
        }

        /// <summary>
        /// System monitoring timer (updates weather based on CPU usage).
        /// </summary>
        private void SystemMonitorTimer_Tick(object? sender, EventArgs e)
        {
            if (_simulationEngine == null || _systemMonitor == null) return;

            double cpuUsage = _systemMonitor.GetCpuUsage();

            // High CPU usage causes stormy weather
            if (cpuUsage <= CpuStormStartThreshold)
            {
                _simulationEngine.WeatherIntensity = 0.0;
                return;
            }

            double denom = CpuStormMaxThreshold - CpuStormStartThreshold;
            _simulationEngine.WeatherIntensity = denom <= 0
                ? 0.0
                : Math.Clamp((cpuUsage - CpuStormStartThreshold) / denom, 0.0, 1.0);
        }

        /// <summary>
        /// Updates FPS counter.
        /// </summary>
        private void UpdateFpsCounter(double deltaTime)
        {
            _frameCount++;
            _fpsAccumulator += deltaTime;

            if (_fpsAccumulator >= 1.0)
            {
                _currentFps = _frameCount / _fpsAccumulator;
                _frameCount = 0;
                _fpsAccumulator = 0;
            }
        }

        /// <summary>
        /// Updates status display.
        /// </summary>
        private void UpdateStatusDisplay()
        {
            if (_simulationEngine == null) return;

            // FPS Display
            FpsText.Text = $"{_currentFps:F0}";

            // Entity counts with new separate fields
            PlantCountText.Text = _simulationEngine.World.Plants.Count.ToString();
            HerbivoreCountText.Text = _simulationEngine.World.Herbivores.Count.ToString();
            CarnivoreCountText.Text = _simulationEngine.World.Carnivores.Count.ToString();

            // Health bar with visual
            double healthPercent = _simulationEngine.GetEcosystemHealth();
            EcosystemHealthText.Text = $"{healthPercent:P0}";
            HealthBar.Width = Math.Max(0, Math.Min(100, healthPercent * 100));
            
            // Color health bar based on value
            if (healthPercent >= 0.7)
                HealthBar.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(46, 204, 113)); // Green
            else if (healthPercent >= 0.4)
                HealthBar.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(241, 196, 15)); // Yellow
            else
                HealthBar.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(231, 76, 60)); // Red

            // Weather display with icons
            bool isStormy = _simulationEngine.WeatherIntensity > StormyWeatherThreshold;
            WeatherIcon.Text = isStormy ? "⛈️" : "☀️";
            WeatherText.Text = isStormy ? "Stormy" : "Calm";

            // Time of day display
            string timeOfDay = _simulationEngine.GetTimeOfDayString();
            string timeIcon = GetTimeIcon(timeOfDay);
            TimeOfDayText.Text = $"{timeIcon} {timeOfDay}";

            // Update day/night orb
            UpdateDayNightOrb(timeOfDay);

            // Statistics panel
            var stats = _simulationEngine.Statistics;
            StatsText.Text = $"Births: {stats.TotalBirths} | Deaths: {stats.TotalDeaths} | Peak Pop: {stats.PeakPopulation}";

            // Background opacity based on time and weather
            double weatherOpacity = _simulationEngine.WeatherIntensity * (StormMaxBackgroundOpacity - CalmBackgroundOpacity);
            double nightOpacity = (1.0 - _simulationEngine.GetLightLevel()) * NightBackgroundOpacity;
            double totalOpacity = Math.Min(weatherOpacity + nightOpacity, StormMaxBackgroundOpacity);

            BackgroundRect.Opacity = Math.Clamp(
                CalmBackgroundOpacity + totalOpacity,
                CalmBackgroundOpacity,
                StormMaxBackgroundOpacity);
        }

        /// <summary>
        /// Gets the appropriate icon for the time of day.
        /// </summary>
        private static string GetTimeIcon(string timeOfDay)
        {
            return timeOfDay.ToLower() switch
            {
                "dawn" => "🌅",
                "day" => "☀️",
                "dusk" => "🌇",
                "night" => "🌙",
                _ => "🌍"
            };
        }

        /// <summary>
        /// Updates the day/night indicator orb.
        /// </summary>
        private void UpdateDayNightOrb(string timeOfDay)
        {
            switch (timeOfDay.ToLower())
            {
                case "dawn":
                    OrbCenterColor.Color = System.Windows.Media.Color.FromRgb(255, 182, 193); // Light pink
                    OrbEdgeColor.Color = System.Windows.Media.Color.FromRgb(255, 140, 105); // Coral
                    OrbGlow.Color = System.Windows.Media.Color.FromRgb(255, 140, 105);
                    DayNightIcon.Text = "🌅";
                    break;
                case "day":
                    OrbCenterColor.Color = System.Windows.Media.Color.FromRgb(255, 215, 0); // Gold
                    OrbEdgeColor.Color = System.Windows.Media.Color.FromRgb(255, 165, 0); // Orange
                    OrbGlow.Color = System.Windows.Media.Color.FromRgb(255, 215, 0);
                    DayNightIcon.Text = "☀️";
                    break;
                case "dusk":
                    OrbCenterColor.Color = System.Windows.Media.Color.FromRgb(255, 99, 71); // Tomato
                    OrbEdgeColor.Color = System.Windows.Media.Color.FromRgb(148, 0, 211); // Purple
                    OrbGlow.Color = System.Windows.Media.Color.FromRgb(255, 99, 71);
                    DayNightIcon.Text = "🌇";
                    break;
                case "night":
                    OrbCenterColor.Color = System.Windows.Media.Color.FromRgb(70, 130, 180); // Steel blue
                    OrbEdgeColor.Color = System.Windows.Media.Color.FromRgb(25, 25, 112); // Midnight blue
                    OrbGlow.Color = System.Windows.Media.Color.FromRgb(135, 206, 250);
                    DayNightIcon.Text = "🌙";
                    break;
            }
        }

        /// <summary>
        /// Handles mouse click events.
        /// </summary>
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_simulationEngine == null) return;

            var position = e.GetPosition(RenderCanvas);

            // Try entity selection first (left click)
            if (e.ChangedButton == MouseButton.Left && _entitySelector != null)
            {
                bool selected = _entitySelector.TrySelect(
                    position.X, position.Y,
                    _simulationEngine.World.Plants,
                    _simulationEngine.World.Herbivores,
                    _simulationEngine.World.Carnivores);

                if (selected) return;
            }

            var clickable = _simulationEngine.FindClickableAt(position.X, position.Y);

            if (clickable != null)
            {
                clickable.OnClick();
            }
        }

        /// <summary>
        /// Handles mouse move events (for hover effects and tooltips).
        /// </summary>
        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (_simulationEngine == null) return;

            var position = e.GetPosition(RenderCanvas);

            // Check for entity under cursor for tooltip
            Terrarium.Logic.Entities.WorldEntity? hoveredEntity = null;

            // Check plants first
            hoveredEntity = _simulationEngine.World.Plants
                .FirstOrDefault(p => p.IsAlive && p.ContainsPoint(position.X, position.Y));

            // Check herbivores
            if (hoveredEntity == null)
            {
                hoveredEntity = _simulationEngine.World.Herbivores
                    .FirstOrDefault(h => h.IsAlive && h.ContainsPoint(position.X, position.Y));
            }

            // Check carnivores
            if (hoveredEntity == null)
            {
                hoveredEntity = _simulationEngine.World.Carnivores
                    .FirstOrDefault(c => c.IsAlive && c.ContainsPoint(position.X, position.Y));
            }

            if (hoveredEntity != null)
            {
                _tooltipManager?.ShowTooltip(hoveredEntity, position.X, position.Y);

                // Plant shake effect
                if (hoveredEntity is Terrarium.Logic.Entities.Plant plant)
                {
                    plant.Shake();
                    _renderer?.TriggerPlantShake(plant);
                }
            }
            else
            {
                _tooltipManager?.HideTooltip();
            }
        }

        /// <summary>
        /// Handles keyboard input for save/load and controls.
        /// </summary>
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (_simulationEngine == null) return;

            switch (e.Key)
            {
                case Key.S when Keyboard.Modifiers == ModifierKeys.Control:
                    // Ctrl+S: Save game
                    SaveGame();
                    e.Handled = true;
                    break;

                case Key.L when Keyboard.Modifiers == ModifierKeys.Control:
                    // Ctrl+L: Load game
                    LoadGame();
                    e.Handled = true;
                    break;

                case Key.P:
                    // P: Spawn plant
                    _simulationEngine.World.SpawnRandomPlant();
                    _particleSystem?.SpawnFeedEffect(
                        _simulationEngine.World.Plants.LastOrDefault()?.X ?? 400,
                        _simulationEngine.World.Plants.LastOrDefault()?.Y ?? 100);
                    e.Handled = true;
                    break;

                case Key.H:
                    // H: Spawn herbivore
                    _simulationEngine.World.SpawnRandomHerbivore();
                    e.Handled = true;
                    break;

                case Key.C:
                    // C: Spawn carnivore
                    _simulationEngine.World.SpawnRandomCarnivore();
                    e.Handled = true;
                    break;

                case Key.F1:
                    // F1: Toggle status panel
                    ToggleUIVisibility();
                    e.Handled = true;
                    break;

                case Key.F2:
                    // F2: Toggle settings panel
                    _settingsPanel?.Toggle();
                    e.Handled = true;
                    break;

                case Key.M:
                    // M: Toggle mini-map
                    _miniMap?.Toggle();
                    e.Handled = true;
                    break;

                case Key.G:
                    // G: Toggle population graph
                    if (_populationGraph != null)
                    {
                        _populationGraph.IsVisible = !_populationGraph.IsVisible;
                        _notificationManager?.Notify(
                            _populationGraph.IsVisible ? "📊 Population Graph ON" : "📊 Population Graph OFF",
                            NotificationType.Info);
                    }
                    e.Handled = true;
                    break;

                case Key.F:
                    // F: Toggle follow mode for selected entity
                    if (_entitySelector?.SelectedEntity != null)
                    {
                        _isFollowingEntity = !_isFollowingEntity;
                        _notificationManager?.Notify(
                            _isFollowingEntity ? "👁️ Following entity" : "👁️ Stopped following",
                            NotificationType.Info);
                    }
                    e.Handled = true;
                    break;

                case Key.W:
                    // W: Water plants (spawn rain particles)
                    WaterAllPlants();
                    e.Handled = true;
                    break;

                case Key.Escape:
                    // ESC: Close settings, deselect entity, or close application
                    if (_settingsPanel?.IsVisible == true)
                    {
                        _settingsPanel.Hide();
                    }
                    else if (_entitySelector?.SelectedEntity != null)
                    {
                        _entitySelector.Deselect();
                        _isFollowingEntity = false;
                    }
                    else
                    {
                        Close();
                    }
                    e.Handled = true;
                    break;
            }
        }

        /// <summary>
        /// Waters all plants and shows particle effects.
        /// </summary>
        private void WaterAllPlants()
        {
            if (_simulationEngine == null) return;

            foreach (var plant in _simulationEngine.World.Plants.Where(p => p.IsAlive).Take(10))
            {
                plant.Water(20);
                _particleSystem?.SpawnWaterEffect(plant.X, plant.Y);
            }

            _notificationManager?.Notify("💧 Watered plants!", NotificationType.Info);
        }

        /// <summary>
        /// Saves the current game state.
        /// </summary>
        private void SaveGame()
        {
            if (_saveManager == null || _simulationEngine == null) return;

            try
            {
                _saveManager.SaveWorld(_simulationEngine.World);
                // Could show a save confirmation visual here
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save: {ex.Message}", "Save Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Loads a saved game state.
        /// </summary>
        private void LoadGame()
        {
            if (_saveManager == null) return;

            try
            {
                var loadedWorld = _saveManager.LoadWorld();
                _simulationEngine = new SimulationEngine(loadedWorld);

                // Could show a load confirmation visual here
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load: {ex.Message}", "Load Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Toggles the visibility of all UI panels.
        /// </summary>
        private void ToggleUIVisibility()
        {
            var newVisibility = StatusPanel.Visibility == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;

            StatusPanel.Visibility = newVisibility;
            StatsPanel.Visibility = newVisibility;
            HotkeyPanel.Visibility = newVisibility;
            DayNightOrb.Visibility = newVisibility;

            // Toggle minimap visibility
            if (_miniMap != null)
            {
                _miniMap.IsVisible = newVisibility == Visibility.Visible;
            }
        }

        /// <summary>
        /// Cleanup when window closes.
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            // Auto-save on exit
            SaveGame();

            if (PresentationSource.FromVisual(this) is HwndSource hwndSource)
            {
                UnregisterGlobalHotkeys(hwndSource.Handle);
            }

            _renderTimer?.Stop();
            _systemMonitorTimer?.Stop();
            _systemMonitor?.Dispose();
            _soundManager?.Dispose();
            _weatherEffects?.Clear();
            _particleSystem?.Clear();
            _notificationManager?.ClearAll();
            _moodIndicator?.Clear();
            _predatorWarning?.Clear();
            _breedingIndicator?.Clear();
            _entitySelector?.Clear();
            base.OnClosed(e);
        }

        /// <summary>
        /// Gets a display name for an entity.
        /// </summary>
        private static string GetEntityName(Terrarium.Logic.Entities.LivingEntity entity)
        {
            return entity switch
            {
                Terrarium.Logic.Entities.Carnivore c => $"🔴 {c.Type}",
                Terrarium.Logic.Entities.Herbivore h => $"🟢 {h.Type}",
                Terrarium.Logic.Entities.Plant _ => "🌿 Plant",
                _ => "Entity"
            };
        }
    }
}
