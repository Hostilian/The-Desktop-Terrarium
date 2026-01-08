using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Terrarium.Desktop.Rendering;
using Terrarium.Logic.Simulation;
using Terrarium.Logic.Persistence;
using System.Diagnostics;

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
        private DispatcherTimer? _renderTimer;
        private DispatcherTimer? _systemMonitorTimer;
        private Stopwatch _frameStopwatch;
        private SystemMonitor? _systemMonitor;
        private SaveManager? _saveManager;

        // Timing constants
        private const int RenderFps = 60;
        private const double RenderInterval = 1000.0 / RenderFps; // milliseconds
        private const double SystemMonitorInterval = 2000.0; // milliseconds

        private int _frameCount;
        private double _fpsAccumulator;
        private double _currentFps;

        public MainWindow()
        {
            InitializeComponent();
            _frameStopwatch = new Stopwatch();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeSimulation();
            InitializeRendering();
            InitializeSaveSystem();
            InitializeSystemMonitoring();
            StartSimulation();
            PositionWindowAtBottom();
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
                        // Replace current world with loaded world
                        _simulationEngine = new SimulationEngine(loadedWorld.Width, loadedWorld.Height);
                        // Note: Would need to properly restore the world in SimulationEngine
                    }
                }
                catch
                {
                    // If load fails, continue with new world
                }
            }
        }   _systemMonitorTimer.Tick += SystemMonitorTimer_Tick;
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

            // High CPU usage (>70%) causes stormy weather
            _simulationEngine.WeatherIntensity = cpuUsage > 0.7 ? (cpuUsage - 0.7) / 0.3 : 0.0;
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

            FpsText.Text = $"FPS: {_currentFps:F1}";

            int totalEntities = _simulationEngine.World.Plants.Count +
                              _simulationEngine.World.Herbivores.Count +
                              _simulationEngine.World.Carnivores.Count;

            EntityCountText.Text = $"Entities: {totalEntities} " +
                                 $"(P:{_simulationEngine.World.Plants.Count} " +
                                 $"H:{_simulationEngine.World.Herbivores.Count} " +
                                 $"C:{_simulationEngine.World.Carnivores.Count})";

            WeatherText.Text = $"Weather: {(_simulationEngine.WeatherIntensity > 0.5 ? "Stormy" : "Calm")}";
        }

        /// <summary>
        /// Handles mouse click events.
        /// </summary>
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_simulationEngine == null) return;

            var position = e.GetPosition(RenderCanvas);
            var clickable = _simulationEngine.FindClickableAt(position.X, position.Y);

            Handles keyboard input for save/load and controls.
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
                    StatusPanel.Visibility = StatusPanel.Visibility == Visibility.Visible
                        ? Visibility.Collapsed
                        : Visibility.Visible;
                    e.Handled = true;
                    break;

                case Key.Escape:
                    // ESC: Close application
                    Close();
                    e.Handled = true;
                    break;
            }
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
                _simulationEngine = new SimulationEngine(loadedWorld.Width, loadedWorld.Height);
                // Note: Would need proper world restoration in SimulationEngine

                // Could show a load confirmation visual here
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load: {ex.Message}", "Load Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Cleanup when window closes.
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            // Auto-save on exit
            SaveGame();
               }
        }

        /// <summary>
        /// Handles mouse move events (for hover effects).
        /// </summary>
        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (_simulationEngine == null) return;

            var position = e.GetPosition(RenderCanvas);

            // Check if hovering over a plant
            foreach (var plant in _simulationEngine.World.Plants)
            {
                if (plant.IsAlive && plant.ContainsPoint(position.X, position.Y))
                {
                    plant.Shake();
                    _renderer?.TriggerPlantShake(plant);
                }
            }
        }

        /// <summary>
        /// Cleanup when window closes.
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            _renderTimer?.Stop();
            _systemMonitorTimer?.Stop();
            _systemMonitor?.Dispose();
            base.OnClosed(e);
        }
    }
}
