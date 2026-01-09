using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Terrarium.Desktop.Rendering;
using Terrarium.Logic.Persistence;
using Terrarium.Logic.Simulation;

namespace Terrarium.Desktop
{
    public partial class MainWindow
    {
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
            _sessionTimer = new SessionTimer(RenderCanvas);

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
                catch (Exception ex)
                {
                    // Avoid error hiding: log and continue with a new world.
                    Debug.WriteLine($"Save file exists but failed to load: {ex}");
                    _notificationManager?.Notify("⚠️ Save file failed to load. Starting new world.", NotificationType.Warning);
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
    }
}
