using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Terrarium.Desktop.Rendering;
using Terrarium.Logic.Persistence;
using Terrarium.Logic.Simulation;

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
        private SessionTimer? _sessionTimer;
        private DispatcherTimer? _renderTimer;
        private DispatcherTimer? _systemMonitorTimer;
        private Stopwatch _frameStopwatch;
        private SystemMonitor? _systemMonitor;
        private SaveManager? _saveManager;
        private bool _isFollowingEntity;

        private Brush? _healthGoodBrush;
        private Brush? _healthWarnBrush;
        private Brush? _healthBadBrush;

        private Color _orbDawnCenter;
        private Color _orbDawnEdge;
        private Color _orbDayCenter;
        private Color _orbDayEdge;
        private Color _orbDuskCenter;
        private Color _orbDuskEdge;
        private Color _orbNightCenter;
        private Color _orbNightEdge;
        private Color _orbNightGlow;

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
            InitializeUiResources();

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

        private void InitializeUiResources()
        {
            _healthGoodBrush = TryFindResource("HealthGoodBrush") as Brush;
            _healthWarnBrush = TryFindResource("HealthWarnBrush") as Brush;
            _healthBadBrush = TryFindResource("HealthBadBrush") as Brush;

            _orbDawnCenter = TryGetColorResource("OrbDawnCenter", Color.FromRgb(255, 182, 193));
            _orbDawnEdge = TryGetColorResource("OrbDawnEdge", Color.FromRgb(255, 140, 105));
            _orbDayCenter = TryGetColorResource("OrbDayCenter", Color.FromRgb(255, 215, 0));
            _orbDayEdge = TryGetColorResource("OrbDayEdge", Color.FromRgb(255, 165, 0));
            _orbDuskCenter = TryGetColorResource("OrbDuskCenter", Color.FromRgb(255, 99, 71));
            _orbDuskEdge = TryGetColorResource("OrbDuskEdge", Color.FromRgb(148, 0, 211));
            _orbNightCenter = TryGetColorResource("OrbNightCenter", Color.FromRgb(70, 130, 180));
            _orbNightEdge = TryGetColorResource("OrbNightEdge", Color.FromRgb(25, 25, 112));
            _orbNightGlow = TryGetColorResource("OrbNightGlow", Color.FromRgb(135, 206, 250));
        }

        private Color TryGetColorResource(string key, Color fallback)
        {
            object resource = TryFindResource(key);
            return resource is Color color ? color : fallback;
        }
    }
}
