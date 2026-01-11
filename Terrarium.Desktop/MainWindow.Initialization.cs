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
        private EventSystem? _wiredEventSystem;
        private Action<Terrarium.Logic.Entities.Creature, Terrarium.Logic.Entities.Creature?, Terrarium.Logic.Entities.Creature?>? _onCreatureBornHandler;
        private Action<Terrarium.Logic.Entities.Creature, string>? _onCreatureDiedHandler;
        private Action<Terrarium.Logic.Entities.Plant, Terrarium.Logic.Entities.Creature>? _onPlantEatenHandler;
        private Action<string>? _onDayPhaseChangedHandler;
        private Action<string, int>? _onMilestoneReachedHandler;

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

        private void InitializeSimulation()
        {
            _simulationEngine = new SimulationEngine(Width, Height);
            _simulationEngine.Initialize();
        }

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

            _settingsPanel.NotificationsToggled += enabled => _notificationManager.IsEnabled = enabled;
            _settingsPanel.WeatherEffectsToggled += enabled => _weatherEffects.IsEnabled = enabled;
            _settingsPanel.SoundToggled += enabled => _soundManager.IsEnabled = enabled;

            WireSimulationEvents();

            _renderTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(RenderInterval) };
            _renderTimer.Tick += RenderTimer_Tick;
        }

        private void WireSimulationEvents()
        {
            if (_simulationEngine == null)
                return;

            var eventSystem = _simulationEngine.EventSystem;

            if (ReferenceEquals(_wiredEventSystem, eventSystem))
                return;

            if (_wiredEventSystem != null)
            {
                _wiredEventSystem.OnCreatureBorn -= _onCreatureBornHandler;
                _wiredEventSystem.OnCreatureDied -= _onCreatureDiedHandler;
                _wiredEventSystem.OnPlantEaten -= _onPlantEatenHandler;
                _wiredEventSystem.OnDayPhaseChanged -= _onDayPhaseChangedHandler;
                _wiredEventSystem.OnMilestoneReached -= _onMilestoneReachedHandler;
            }

            _onCreatureBornHandler ??= (creature, parent1, parent2) =>
            {
                _particleSystem?.SpawnBirthEffect(creature.X, creature.Y);
                string creatureType = creature is Terrarium.Logic.Entities.Herbivore h ? h.Type :
                                     creature is Terrarium.Logic.Entities.Carnivore c ? c.Type : "Creature";
                _notificationManager?.NotifyBirth(creatureType);
            };

            _onCreatureDiedHandler ??= (creature, cause) =>
            {
                _particleSystem?.SpawnDeathEffect(creature.X, creature.Y);
                string creatureType = creature is Terrarium.Logic.Entities.Herbivore h ? h.Type :
                                     creature is Terrarium.Logic.Entities.Carnivore c ? c.Type : "Creature";
                _notificationManager?.NotifyDeath(creatureType, cause);
            };

            _onPlantEatenHandler ??= (plant, creature) =>
            {
                _particleSystem?.SpawnEatEffect(plant.X, plant.Y);
            };

            _onDayPhaseChangedHandler ??= phase =>
            {
                _notificationManager?.NotifyDayPhaseChange(phase);
            };

            _onMilestoneReachedHandler ??= (name, value) =>
            {
                _notificationManager?.NotifyMilestone(name, value);
            };

            eventSystem.OnCreatureBorn += _onCreatureBornHandler;
            eventSystem.OnCreatureDied += _onCreatureDiedHandler;
            eventSystem.OnPlantEaten += _onPlantEatenHandler;
            eventSystem.OnDayPhaseChanged += _onDayPhaseChangedHandler;
            eventSystem.OnMilestoneReached += _onMilestoneReachedHandler;

            _wiredEventSystem = eventSystem;
        }

        private void InitializeSystemMonitoring()
        {
            _systemMonitor = new SystemMonitor();
            _systemMonitorTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(SystemMonitorInterval) };
            _systemMonitorTimer.Tick += SystemMonitorTimer_Tick;
        }

        private void InitializeSaveSystem()
        {
            _saveManager = new SaveManager();
            if (!_saveManager.SaveFileExists()) return;

            try
            {
                var loadedWorld = _saveManager.LoadWorld();
                if (_simulationEngine != null)
                {
                    _simulationEngine = new SimulationEngine(loadedWorld);
                    WireSimulationEvents();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Save file exists but failed to load: {ex}");
                _notificationManager?.Notify("⚠️ Save file failed to load. Starting new world.", NotificationType.Warning);
            }
        }

        private void StartSimulation()
        {
            _frameStopwatch.Start();
            _renderTimer?.Start();
            _systemMonitorTimer?.Start();
        }
    }
}
