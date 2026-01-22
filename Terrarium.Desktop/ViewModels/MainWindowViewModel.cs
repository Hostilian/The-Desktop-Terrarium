using System;
using System.Windows.Input;
using Terrarium.Desktop.Commands;
using Terrarium.Desktop.Services;
using Terrarium.Desktop.ViewModels.Base;
using Terrarium.Logic.Simulation;

namespace Terrarium.Desktop.ViewModels;

/// <summary>
/// ViewModel for MainWindow - handles all UI logic and state without business logic.
/// Follows MVVM pattern by delegating all operations to services.
/// </summary>
public class MainWindowViewModel : ViewModelBase
{
    private readonly GodPowerService _godPowerService;
    private bool _isPaused;
    private double _simulationSpeed = 1.0;
    private string _statusMessage = "Ready";

    public MainWindowViewModel(SimulationEngine simulationEngine)
    {
        _godPowerService = new GodPowerService(simulationEngine);

        // Initialize commands
        PlayPauseCommand = new RelayCommand(ExecutePlayPause);
        SpeedUpCommand = new RelayCommand(ExecuteSpeedUp);
        SpawnPlantCommand = new RelayCommand(ExecuteSpawnPlant);
        SpawnHerbivoreCommand = new RelayCommand(ExecuteSpawnHerbivore);
        SpawnCarnivoreCommand = new RelayCommand(ExecuteSpawnCarnivore);
        LightningStrikeCommand = new RelayCommand(ExecuteLightningStrike);
        MeteorShowerCommand = new RelayCommand(ExecuteMeteorShower);
        PlagueCommand = new RelayCommand(ExecutePlague);
        FertilityBlessingCommand = new RelayCommand(ExecuteFertilityBlessing);
        AbundanceCommand = new RelayCommand(ExecuteAbundance);
        CorruptionCommand = new RelayCommand(ExecuteCorruption);
    }

    #region Properties

    /// <summary>
    /// Gets or sets whether the simulation is paused.
    /// </summary>
    public bool IsPaused
    {
        get => _isPaused;
        set => SetProperty(ref _isPaused, value);
    }

    /// <summary>
    /// Gets or sets the current simulation speed multiplier.
    /// </summary>
    public double SimulationSpeed
    {
        get => _simulationSpeed;
        set => SetProperty(ref _simulationSpeed, value);
    }

    /// <summary>
    /// Gets or sets the status message displayed to the user.
    /// </summary>
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    #endregion

    #region Commands

    public ICommand PlayPauseCommand { get; }
    public ICommand SpeedUpCommand { get; }
    public ICommand SpawnPlantCommand { get; }
    public ICommand SpawnHerbivoreCommand { get; }
    public ICommand SpawnCarnivoreCommand { get; }
    public ICommand LightningStrikeCommand { get; }
    public ICommand MeteorShowerCommand { get; }
    public ICommand PlagueCommand { get; }
    public ICommand FertilityBlessingCommand { get; }
    public ICommand AbundanceCommand { get; }
    public ICommand CorruptionCommand { get; }

    #endregion

    #region Command Implementations

    private void ExecutePlayPause()
    {
        IsPaused = !IsPaused;
        StatusMessage = IsPaused ? "Paused" : "Running";
    }

    private void ExecuteSpeedUp()
    {
        double[] speeds = { 1.0, 2.0, 5.0, 10.0 };
        int currentIndex = Array.IndexOf(speeds, SimulationSpeed);
        int nextIndex = (currentIndex + 1) % speeds.Length;
        SimulationSpeed = speeds[nextIndex];
        StatusMessage = $"Speed: {SimulationSpeed}x";
    }

    private void ExecuteSpawnPlant()
    {
        var plant = _godPowerService.SpawnPlant();
        StatusMessage = $"🌱 Plant spawned at ({plant.X:F0}, {plant.Y:F0})";
    }

    private void ExecuteSpawnHerbivore()
    {
        var herbivore = _godPowerService.SpawnHerbivore();
        StatusMessage = $"🐰 Herbivore spawned at ({herbivore.X:F0}, {herbivore.Y:F0})";
    }

    private void ExecuteSpawnCarnivore()
    {
        var carnivore = _godPowerService.SpawnCarnivore();
        StatusMessage = $"🐺 Carnivore spawned at ({carnivore.X:F0}, {carnivore.Y:F0})";
    }

    private void ExecuteLightningStrike()
    {
        int struck = _godPowerService.ExecuteLightningStrike();
        StatusMessage = $"⚡ Lightning struck {struck} entities";
    }

    private void ExecuteMeteorShower()
    {
        int damaged = _godPowerService.ExecuteMeteorShower();
        StatusMessage = $"☄️ Meteor shower damaged {damaged} entities";
    }

    private void ExecutePlague()
    {
        int infected = _godPowerService.ExecutePlague();
        StatusMessage = $"💀 Plague infected {infected} creatures";
    }

    private void ExecuteFertilityBlessing()
    {
        double duration = _godPowerService.ApplyFertilityBlessing();
        StatusMessage = $"🌸 Fertility blessing for {duration} seconds";
    }

    private void ExecuteAbundance()
    {
        int plants = _godPowerService.CreateAbundance();
        StatusMessage = $"🍎 Created {plants} plants";
    }

    private void ExecuteCorruption()
    {
        int corrupted = _godPowerService.ExecuteCorruption();
        StatusMessage = $"😈 Corrupted {corrupted} creatures";
    }

    #endregion
}
