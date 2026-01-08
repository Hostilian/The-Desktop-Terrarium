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
        private DispatcherTimer? _renderTimer;
        private DispatcherTimer? _systemMonitorTimer;
        private Stopwatch _frameStopwatch;
        private SystemMonitor? _systemMonitor;
        private SaveManager? _saveManager;

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
                        StatusPanel.Visibility = StatusPanel.Visibility == Visibility.Visible
                            ? Visibility.Collapsed
                            : Visibility.Visible;
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

            FpsText.Text = $"FPS: {_currentFps:F1}";

            int totalEntities = _simulationEngine.World.Plants.Count +
                              _simulationEngine.World.Herbivores.Count +
                              _simulationEngine.World.Carnivores.Count;

            EntityCountText.Text = $"Entities: {totalEntities} " +
                                 $"(P:{_simulationEngine.World.Plants.Count} " +
                                 $"H:{_simulationEngine.World.Herbivores.Count} " +
                                 $"C:{_simulationEngine.World.Carnivores.Count})";

            EcosystemHealthText.Text = $"Ecosystem: {_simulationEngine.GetEcosystemHealth():P0} " +
                                      $"({(_simulationEngine.IsEcosystemBalanced() ? "Balanced" : "Unbalanced")})";

            WeatherText.Text = $"Weather: {(_simulationEngine.WeatherIntensity > StormyWeatherThreshold ? "Stormy" : "Calm")}";

            // Darken slightly as storms intensify.
            BackgroundRect.Opacity = Math.Clamp(
                CalmBackgroundOpacity + (_simulationEngine.WeatherIntensity * (StormMaxBackgroundOpacity - CalmBackgroundOpacity)),
                CalmBackgroundOpacity,
                StormMaxBackgroundOpacity);
        }

        /// <summary>
        /// Handles mouse click events.
        /// </summary>
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_simulationEngine == null) return;

            var position = e.GetPosition(RenderCanvas);
            var clickable = _simulationEngine.FindClickableAt(position.X, position.Y);

            if (clickable != null)
            {
                clickable.OnClick();
            }
        }

        /// <summary>
        /// Handles mouse move events (for hover effects).
        /// </summary>
        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (_simulationEngine == null) return;

            var position = e.GetPosition(RenderCanvas);

            var hoveredPlant = _simulationEngine.World.Plants
                .FirstOrDefault(p => p.IsAlive && p.ContainsPoint(position.X, position.Y));

            if (hoveredPlant != null)
            {
                hoveredPlant.Shake();
                _renderer?.TriggerPlantShake(hoveredPlant);
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
            base.OnClosed(e);
        }
    }
}
