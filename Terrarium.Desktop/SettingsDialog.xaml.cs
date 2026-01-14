using System;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Text.Json;
using System.Linq;

namespace Terrarium.Desktop;

public partial class SettingsDialog : Window
{
    private const string SettingsFile = "terrarium_settings.json";

    public int WorldWidth { get; private set; } = 800;
    public int WorldHeight { get; private set; } = 600;
    public int InitialPlants { get; private set; } = 50;
    public int InitialHerbivores { get; private set; } = 20;
    public int InitialCarnivores { get; private set; } = 5;
    public bool ShowDetailedStats { get; private set; } = true;
    public bool EnableAnimations { get; private set; } = true;
    public bool TransparentBackground { get; private set; } = false;
    public bool ShowScenery { get; private set; } = true;
    public bool ShowEntityShadows { get; private set; } = true;
    public bool EnableSound { get; private set; } = true;
    public bool EnableAmbientMusic { get; private set; } = true;
    public double MasterVolume { get; private set; } = 0.7;
    public bool EnableVSync { get; private set; } = true;
    public bool LimitFrameRate { get; private set; } = true;
    public string RenderQuality { get; private set; } = "Medium";
    public bool EnableHunger { get; private set; } = true;
    public bool EnableAging { get; private set; } = true;
    public bool EnableDisasters { get; private set; } = true;
    public bool EnableDiplomacy { get; private set; } = true;
    public bool EnableRebellion { get; private set; } = true;
    public bool EnableExpansion { get; private set; } = true;
    public bool AutoExplore { get; private set; } = false;
    public bool AutoPickup { get; private set; } = false;
    public bool AutoAttack { get; private set; } = false;
    public bool AutoSave { get; private set; } = true;
    public bool PauseOnDeath { get; private set; } = true;
    public bool ShowMinimap { get; private set; } = false;
    public bool ShowHealthBars { get; private set; } = true;
    public bool ShowPopulationGraph { get; private set; } = true;
    public double EffectsVolume { get; private set; } = 0.8;
    public string SimulationSpeed { get; private set; } = "1x";
    public bool DisablePermadeath { get; private set; } = false;
    public bool EnableGodMode { get; private set; } = false;
    public bool ShowDebugInfo { get; private set; } = false;

    public SettingsDialog()
    {
        InitializeComponent();
        LoadSettings();
        WorldWidthSlider.ValueChanged += (s, e) => WorldWidthText.Text = WorldWidthSlider.Value.ToString();
        WorldHeightSlider.ValueChanged += (s, e) => WorldHeightText.Text = WorldHeightSlider.Value.ToString();
        InitialPlantsSlider.ValueChanged += (s, e) => InitialPlantsText.Text = InitialPlantsSlider.Value.ToString();
        InitialHerbivoresSlider.ValueChanged += (s, e) => InitialHerbivoresText.Text = InitialHerbivoresSlider.Value.ToString();
        InitialCarnivoresSlider.ValueChanged += (s, e) => InitialCarnivoresText.Text = InitialCarnivoresSlider.Value.ToString();
        MasterVolumeSlider.ValueChanged += (s, e) => MasterVolumeText.Text = $"{(int)(MasterVolumeSlider.Value * 100)}%";
        EffectsVolumeSlider.ValueChanged += (s, e) => EffectsVolumeText.Text = $"{(int)(EffectsVolumeSlider.Value * 100)}%";
    }

    private void LoadSettings()
    {
        try
        {
            if (File.Exists(SettingsFile))
            {
                var json = File.ReadAllText(SettingsFile);
                var settings = JsonSerializer.Deserialize<SettingsData>(json);

                if (settings != null)
                {
                    WorldWidth = settings.WorldWidth;
                    WorldHeight = settings.WorldHeight;
                    InitialPlants = settings.InitialPlants;
                    InitialHerbivores = settings.InitialHerbivores;
                    InitialCarnivores = settings.InitialCarnivores;
                    ShowDetailedStats = settings.ShowDetailedStats;
                    EnableAnimations = settings.EnableAnimations;
                    TransparentBackground = settings.TransparentBackground;
                    ShowScenery = settings.ShowScenery;
                    ShowEntityShadows = settings.ShowEntityShadows;
                    EnableSound = settings.EnableSound;
                    EnableAmbientMusic = settings.EnableAmbientMusic;
                    MasterVolume = settings.MasterVolume;
                    EnableVSync = settings.EnableVSync;
                    LimitFrameRate = settings.LimitFrameRate;
                    RenderQuality = settings.RenderQuality;
                    EnableHunger = settings.EnableHunger;
                    EnableAging = settings.EnableAging;
                    ShowPopulationGraph = settings.ShowPopulationGraph;
                    EffectsVolume = settings.EffectsVolume;
                    SimulationSpeed = settings.SimulationSpeed;
                    DisablePermadeath = settings.DisablePermadeath;
                    EnableGodMode = settings.EnableGodMode;
                    ShowDebugInfo = settings.ShowDebugInfo;

                    // Update UI
                    WorldWidthSlider.Value = WorldWidth;
                    WorldHeightSlider.Value = WorldHeight;
                    InitialPlantsSlider.Value = InitialPlants;
                    InitialHerbivoresSlider.Value = InitialHerbivores;
                    InitialCarnivoresSlider.Value = InitialCarnivores;
                    ShowDetailedStatsCheckBox.IsChecked = ShowDetailedStats;
                    EnableAnimationsCheckBox.IsChecked = EnableAnimations;
                    TransparentBackgroundCheckBox.IsChecked = TransparentBackground;
                    ShowSceneryCheckBox.IsChecked = ShowScenery;
                    ShowEntityShadowsCheckBox.IsChecked = ShowEntityShadows;
                    EnableSoundCheckBox.IsChecked = EnableSound;
                    EnableAmbientMusicCheckBox.IsChecked = EnableAmbientMusic;
                    MasterVolumeSlider.Value = MasterVolume;
                    EnableVSyncCheckBox.IsChecked = EnableVSync;
                    LimitFrameRateCheckBox.IsChecked = LimitFrameRate;
                    RenderQualityComboBox.SelectedItem = RenderQualityComboBox.Items
                        .Cast<ComboBoxItem>()
                        .FirstOrDefault(item => item.Content.ToString() == RenderQuality);
                    EnableHungerCheckBox.IsChecked = EnableHunger;
                    EnableAgingCheckBox.IsChecked = EnableAging;
                    ShowPopulationGraphCheckBox.IsChecked = ShowPopulationGraph;
                    EffectsVolumeSlider.Value = EffectsVolume;
                    SimulationSpeedComboBox.SelectedItem = SimulationSpeedComboBox.Items
                        .Cast<ComboBoxItem>()
                        .FirstOrDefault(item => item.Content.ToString() == SimulationSpeed);
                    DisablePermadeathCheckBox.IsChecked = DisablePermadeath;
                    EnableGodModeCheckBox.IsChecked = EnableGodMode;
                    ShowDebugInfoCheckBox.IsChecked = ShowDebugInfo;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load settings: {ex.Message}");
        }
    }

    private void SaveSettings()
    {
        try
        {
            var settings = new SettingsData
            {
                WorldWidth = WorldWidth,
                WorldHeight = WorldHeight,
                InitialPlants = InitialPlants,
                InitialHerbivores = InitialHerbivores,
                InitialCarnivores = InitialCarnivores,
                ShowDetailedStats = ShowDetailedStats,
                EnableAnimations = EnableAnimations,
                TransparentBackground = TransparentBackground,
                ShowScenery = ShowScenery,
                ShowEntityShadows = ShowEntityShadows,
                EnableSound = EnableSound,
                EnableAmbientMusic = EnableAmbientMusic,
                MasterVolume = MasterVolume,
                EnableVSync = EnableVSync,
                LimitFrameRate = LimitFrameRate,
                RenderQuality = RenderQuality,
                EnableHunger = EnableHunger,
                EnableAging = EnableAging,
                EnableDisasters = EnableDisasters,
                EnableDiplomacy = EnableDiplomacy,
                EnableRebellion = EnableRebellion,
                EnableExpansion = EnableExpansion,
                AutoExplore = AutoExplore,
                AutoPickup = AutoPickup,
                AutoAttack = AutoAttack,
                AutoSave = AutoSave,
                PauseOnDeath = PauseOnDeath,
                ShowMinimap = ShowMinimap,
                ShowHealthBars = ShowHealthBars,
                ShowPopulationGraph = ShowPopulationGraph,
                EffectsVolume = EffectsVolume,
                SimulationSpeed = SimulationSpeed,
                DisablePermadeath = DisablePermadeath,
                EnableGodMode = EnableGodMode,
                ShowDebugInfo = ShowDebugInfo
            };

            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsFile, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to save settings: {ex.Message}");
        }
    }

    private void OKButton_Click(object sender, RoutedEventArgs e)
    {
        WorldWidth = (int)WorldWidthSlider.Value;
        WorldHeight = (int)WorldHeightSlider.Value;
        InitialPlants = (int)InitialPlantsSlider.Value;
        InitialHerbivores = (int)InitialHerbivoresSlider.Value;
        InitialCarnivores = (int)InitialCarnivoresSlider.Value;
        ShowDetailedStats = ShowDetailedStatsCheckBox.IsChecked ?? true;
        EnableAnimations = EnableAnimationsCheckBox.IsChecked ?? true;
        TransparentBackground = TransparentBackgroundCheckBox.IsChecked ?? false;
        ShowScenery = ShowSceneryCheckBox.IsChecked ?? true;
        ShowEntityShadows = ShowEntityShadowsCheckBox.IsChecked ?? true;
        EnableSound = EnableSoundCheckBox.IsChecked ?? true;
        EnableAmbientMusic = EnableAmbientMusicCheckBox.IsChecked ?? true;
        MasterVolume = MasterVolumeSlider.Value;
        EnableVSync = EnableVSyncCheckBox.IsChecked ?? true;
        LimitFrameRate = LimitFrameRateCheckBox.IsChecked ?? true;
        RenderQuality = (RenderQualityComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Medium";

        // New settings
        EnableHunger = EnableHungerCheckBox.IsChecked ?? true;
        EnableAging = EnableAgingCheckBox.IsChecked ?? true;
        EnableDisasters = EnableDisastersCheckBox.IsChecked ?? true;
        EnableDiplomacy = EnableDiplomacyCheckBox.IsChecked ?? true;
        EnableRebellion = EnableRebellionCheckBox.IsChecked ?? true;
        EnableExpansion = EnableExpansionCheckBox.IsChecked ?? true;
        AutoExplore = AutoExploreCheckBox.IsChecked ?? false;
        AutoPickup = AutoPickupCheckBox.IsChecked ?? false;
        AutoAttack = AutoAttackCheckBox.IsChecked ?? false;
        AutoSave = AutoSaveCheckBox.IsChecked ?? true;
        PauseOnDeath = PauseOnDeathCheckBox.IsChecked ?? true;
        ShowMinimap = ShowMinimapCheckBox.IsChecked ?? false;
        ShowHealthBars = ShowHealthBarsCheckBox.IsChecked ?? true;
        ShowPopulationGraph = ShowPopulationGraphCheckBox.IsChecked ?? true;
        EffectsVolume = EffectsVolumeSlider.Value;
        SimulationSpeed = (SimulationSpeedComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "1x";
        DisablePermadeath = DisablePermadeathCheckBox.IsChecked ?? false;
        EnableGodMode = EnableGodModeCheckBox.IsChecked ?? false;
        ShowDebugInfo = ShowDebugInfoCheckBox.IsChecked ?? false;

        SaveSettings();
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }

    private void SpawnExtraEntitiesButton_Click(object sender, RoutedEventArgs e)
    {
        // This would trigger spawning extra entities in the main window
        MessageBox.Show("Extra entities spawned! (Feature not yet implemented in main simulation)", "Debug Feature");
    }

    private void ResetToDefaultsButton_Click(object sender, RoutedEventArgs e)
    {
        if (MessageBox.Show("Reset all settings to defaults?", "Confirm Reset", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
        {
            // Reset all controls to default values
            WorldWidthSlider.Value = 800;
            WorldHeightSlider.Value = 600;
            InitialPlantsSlider.Value = 50;
            InitialHerbivoresSlider.Value = 20;
            InitialCarnivoresSlider.Value = 5;
            ShowDetailedStatsCheckBox.IsChecked = true;
            EnableAnimationsCheckBox.IsChecked = true;
            TransparentBackgroundCheckBox.IsChecked = false;
            ShowSceneryCheckBox.IsChecked = true;
            ShowEntityShadowsCheckBox.IsChecked = true;
            EnableSoundCheckBox.IsChecked = true;
            EnableAmbientMusicCheckBox.IsChecked = true;
            MasterVolumeSlider.Value = 0.7;
            EnableVSyncCheckBox.IsChecked = true;
            LimitFrameRateCheckBox.IsChecked = true;
            RenderQualityComboBox.SelectedIndex = 1;

            // New settings defaults
            EnableHungerCheckBox.IsChecked = true;
            EnableAgingCheckBox.IsChecked = true;
            EnableDisastersCheckBox.IsChecked = true;
            EnableDiplomacyCheckBox.IsChecked = true;
            EnableRebellionCheckBox.IsChecked = true;
            EnableExpansionCheckBox.IsChecked = true;
            AutoExploreCheckBox.IsChecked = false;
            AutoPickupCheckBox.IsChecked = false;
            AutoAttackCheckBox.IsChecked = false;
            AutoSaveCheckBox.IsChecked = true;
            PauseOnDeathCheckBox.IsChecked = true;
            ShowMinimapCheckBox.IsChecked = false;
            ShowHealthBarsCheckBox.IsChecked = true;
            ShowPopulationGraphCheckBox.IsChecked = true;
            EffectsVolumeSlider.Value = 0.8;
            SimulationSpeedComboBox.SelectedIndex = 2;
            DisablePermadeathCheckBox.IsChecked = false;
            EnableGodModeCheckBox.IsChecked = false;
            ShowDebugInfoCheckBox.IsChecked = false;
        }
    }

    private class SettingsData
    {
        public int WorldWidth { get; set; } = 800;
        public int WorldHeight { get; set; } = 600;
        public int InitialPlants { get; set; } = 50;
        public int InitialHerbivores { get; set; } = 20;
        public int InitialCarnivores { get; set; } = 5;
        public bool ShowDetailedStats { get; set; } = true;
        public bool EnableAnimations { get; set; } = true;
        public bool TransparentBackground { get; set; } = false;
        public bool ShowScenery { get; set; } = true;
        public bool ShowEntityShadows { get; set; } = true;
        public bool EnableSound { get; set; } = true;
        public bool EnableAmbientMusic { get; set; } = true;
        public double MasterVolume { get; set; } = 0.7;
        public bool EnableVSync { get; set; } = true;
        public bool LimitFrameRate { get; set; } = true;
        public string RenderQuality { get; set; } = "Medium";
        public bool EnableHunger { get; set; } = true;
        public bool EnableAging { get; set; } = true;
        public bool EnableDisasters { get; set; } = true;
        public bool EnableDiplomacy { get; set; } = true;
        public bool EnableRebellion { get; set; } = true;
        public bool EnableExpansion { get; set; } = true;
        public bool AutoExplore { get; set; } = false;
        public bool AutoPickup { get; set; } = false;
        public bool AutoAttack { get; set; } = false;
        public bool AutoSave { get; set; } = true;
        public bool PauseOnDeath { get; set; } = true;
        public bool ShowMinimap { get; set; } = false;
        public bool ShowHealthBars { get; set; } = true;
        public bool ShowPopulationGraph { get; set; } = true;
        public double EffectsVolume { get; set; } = 0.8;
        public string SimulationSpeed { get; set; } = "1x";
        public bool DisablePermadeath { get; set; } = false;
        public bool EnableGodMode { get; set; } = false;
        public bool ShowDebugInfo { get; set; } = false;
    }
}
