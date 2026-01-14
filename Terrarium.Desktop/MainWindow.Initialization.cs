using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Terrarium.Desktop.Rendering;
using Terrarium.Logic.Persistence;
using Terrarium.Logic.Simulation;

namespace Terrarium.Desktop;

public partial class MainWindow
{

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        PositionWindowAtBottom();
        InitializeSimulation();
        // Set theme based on terrarium type
        SetTheme();
        UpdateGodPowersVisibility(); // Control which god powers are visible based on mode
        try
        {
            InitializeRendering();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error in InitializeRendering: {ex.Message}", "Error");
            Environment.Exit(1);
            return;
        }
        InitializeSaveSystem();
        InitializeSystemMonitoring();
        InitializeSoundSystem();
        StartSimulation();
    }

    /// <summary>
    /// Positions the window normally.
    /// </summary>
    /// <summary>
    /// Positions the window in the bottom-right corner.
    /// </summary>
    private void PositionWindowAtBottom()
    {
        var desktopWorkingArea = SystemParameters.WorkArea;
        this.Left = desktopWorkingArea.Right - this.Width - 20;
        this.Top = desktopWorkingArea.Bottom - this.Height - 20;
    }

    private void InitializeSimulation()
    {
        try
        {
            if (Application.Current == null)
            {
                 MessageBox.Show("Application.Current is null", "Critical Error");
                 return;
            }

            var app = (App)Application.Current;
            if (!Enum.TryParse<TerrariumType>(app.SelectedTerrariumType, ignoreCase: true, out var type))
            {
                MessageBox.Show($"Invalid terrarium type selected: '{app.SelectedTerrariumType}'.\nSimulation cannot start.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Console.WriteLine($"[MainWindow] Failed to parse terrarium type: '{app.SelectedTerrariumType}'");
                Environment.Exit(1);
                return;
            }
            _simulationEngine = new SimulationEngine(800, 600, type);
            _simulationEngine.Initialize();
            _terrariumType = type;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error initializing simulation: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Console.WriteLine($"[MainWindow] Exception in InitializeSimulation: {ex}");
            Application.Current.Shutdown();
        }
    }

    private void InitializeRendering()
    {
        _renderer = new Renderer(RenderCanvas, _terrariumType);

        _renderTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(RenderInterval) };
        _renderTimer.Tick += RenderTimer_Tick;
    }

    private void SetTheme()
    {
        switch (_terrariumType)
        {
            case Terrarium.Logic.Simulation.TerrariumType.Forest:
                RenderCanvas.Background = new SolidColorBrush(Color.FromRgb(34, 139, 34)); // Forest green
                Title = "Desktop Terrarium - Forest";
                break;
            case Terrarium.Logic.Simulation.TerrariumType.Desert:
                RenderCanvas.Background = new SolidColorBrush(Color.FromRgb(210, 180, 140)); // Tan
                Title = "Desktop Terrarium - Desert";
                break;
            case Terrarium.Logic.Simulation.TerrariumType.Aquatic:
                RenderCanvas.Background = new SolidColorBrush(Color.FromRgb(70, 130, 180)); // Steel blue
                Title = "Desktop Terrarium - Aquatic";
                break;
            case Terrarium.Logic.Simulation.TerrariumType.GodSimulator:
                RenderCanvas.Background = Brushes.Transparent;
                Title = "Desktop Terrarium - God Simulator";
                break;
            case Terrarium.Logic.Simulation.TerrariumType.LiveSandbox:
                LaunchLiveSandbox();
                Application.Current.Shutdown();
                return;
            case Terrarium.Logic.Simulation.TerrariumType.CivilizationBuilder:
                LaunchCivilizationBuilder();
                Application.Current.Shutdown();
                return;
            case Terrarium.Logic.Simulation.TerrariumType.Game2048:
                Launch2048();
                Application.Current.Shutdown();
                return;
            case Terrarium.Logic.Simulation.TerrariumType.GameTetris:
                LaunchTetris();
                Application.Current.Shutdown();
                return;
            case Terrarium.Logic.Simulation.TerrariumType.GameDino:
                LaunchDino();
                Application.Current.Shutdown();
                return;
            case Terrarium.Logic.Simulation.TerrariumType.GameArcade:
                LaunchArcade();
                Application.Current.Shutdown();
                return;
            case Terrarium.Logic.Simulation.TerrariumType.WidgetWeb:
                LaunchWeb();
                Application.Current.Shutdown();
                return;
            case Terrarium.Logic.Simulation.TerrariumType.WidgetPet:
                LaunchPet();
                Application.Current.Shutdown();
                return;
            case Terrarium.Logic.Simulation.TerrariumType.GamePacman:
                LaunchPacman();
                Application.Current.Shutdown();
                return;
        }
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
        if (!_saveManager.SaveFileExists())
        {
            return;
        }

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
            Debug.WriteLine($"Save file exists but failed to load: {ex}");
            // Simplified: no notification for failed load
        }
    }

    private void InitializeSoundSystem()
    {
        _soundManager = new SoundManager();
        // Start playing theme music based on terrarium type
        PlayThemeMusic();
    }

    private void PlayThemeMusic()
    {
        if (_soundManager == null)
        {
            return;
        }

        // Set theme for sound manager
        _soundManager.SetTheme(_terrariumType);

        // Update visual theme
        UpdateThemeVisuals();
    }

    private void UpdateThemeVisuals()
    {
        string? imagePath = null;

        switch (_terrariumType)
        {
            case TerrariumType.Forest:
                imagePath = "Assets/Images/forest_background.jpg";
                break;
            case TerrariumType.Desert:
                imagePath = "Assets/Images/desert_background.jpg";
                break;
            case TerrariumType.Aquatic:
                imagePath = "Assets/Images/aquatic_background.jpg";
                break;
            case TerrariumType.GodSimulator:
                imagePath = "Assets/Images/godsimulator_background.jpg";
                break;
            case TerrariumType.PowderToy:
                // Powder Toy uses its own window - no background needed here
                break;
        }

        if (!string.IsNullOrEmpty(imagePath) && System.IO.File.Exists(imagePath))
        {
            var brush = new ImageBrush(new BitmapImage(new Uri(imagePath, UriKind.Relative)));
            RenderCanvas.Background = brush;
        }
        else
        {
            // Fallback to solid color if image not found
            var solidBrush = new SolidColorBrush();
            switch (_terrariumType)
            {
                case TerrariumType.Forest:
                    solidBrush.Color = Color.FromRgb(34, 139, 34);
                    DrawSimpleScenery(Brushes.DarkGreen);
                    break;
                case TerrariumType.Desert:
                    solidBrush.Color = Color.FromRgb(210, 180, 140);
                    break;
                case TerrariumType.Aquatic:
                    solidBrush.Color = Color.FromRgb(70, 130, 180);
                    break;
               case TerrariumType.GodSimulator:
                    solidBrush.Color = Color.FromRgb(20, 20, 30); // Darker Indigo
                    break;
                case TerrariumType.PowderToy:
                    solidBrush.Color = Colors.Black;
                    break;
            }
            RenderCanvas.Background = solidBrush;
        }
    }

    private void DrawSimpleScenery(Brush brush)
    {
        // Optional: Add simple shapes for scenery if image missing
    }

    private void StartSimulation()
    {
        _frameStopwatch.Start();
        _renderTimer?.Start();
        _systemMonitorTimer?.Start();
    }
}
