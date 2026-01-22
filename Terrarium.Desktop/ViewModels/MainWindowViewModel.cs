namespace Terrarium.Desktop.ViewModels;

using System;
using System.Windows.Input;
using Terrarium.Desktop.Commands;
using Terrarium.Desktop.Services;
using Terrarium.Desktop.ViewModels.Base;
using Terrarium.Logic.Simulation;

/// <summary>
/// ViewModel for MainWindow - handles all UI logic and state without business logic.
/// Follows MVVM pattern by delegating all operations to services.
/// </summary>
public class MainWindowViewModel : ViewModelBase
{
    private readonly GodPowerService godPowerService;
    private bool isPaused;
    private double simulationSpeed = 1.0;
    private string statusMessage = "Ready";

    public MainWindowViewModel(SimulationEngine simulationEngine)
    {
        godPowerService = new GodPowerService(simulationEngine);

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

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether the simulation is paused.
    /// </summary>
    public bool IsPaused
    {
        get => isPaused;
        set => SetProperty(ref isPaused, value);
    }

    /// <summary>
    /// Gets or sets the current simulation speed multiplier.
    /// </summary>
    public double SimulationSpeed
    {
        get => simulationSpeed;
        set => SetProperty(ref simulationSpeed, value);
    }

    /// <summary>
    /// Gets or sets the status message displayed to the user.
    /// </summary>
    public string StatusMessage
    {
        get => statusMessage;
        set => SetProperty(ref statusMessage, value);
    }

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
        var plant = godPowerService.SpawnPlant();
        StatusMessage = $"🌱 Plant spawned at ({plant.X:F0}, {plant.Y:F0})";
    }

    private void ExecuteSpawnHerbivore()
    {
        var herbivore = godPowerService.SpawnHerbivore();
        StatusMessage = $"🐰 Herbivore spawned at ({herbivore.X:F0}, {herbivore.Y:F0})";
    }

    private void ExecuteSpawnCarnivore()
    {
        var carnivore = godPowerService.SpawnCarnivore();
        StatusMessage = $"🐺 Carnivore spawned at ({carnivore.X:F0}, {carnivore.Y:F0})";
    }

    private void ExecuteLightningStrike()
    {
        int struck = godPowerService.ExecuteLightningStrike();
        StatusMessage = $"⚡ Lightning struck {struck} entities";
    }

    private void ExecuteMeteorShower()
    {
        int damaged = godPowerService.ExecuteMeteorShower();
        StatusMessage = $"☄️ Meteor shower damaged {damaged} entities";
    }

    private void ExecutePlague()
    {
        int infected = godPowerService.ExecutePlague();
        StatusMessage = $"💀 Plague infected {infected} creatures";
    }

    private void ExecuteFertilityBlessing()
    {
        double duration = godPowerService.ApplyFertilityBlessing();
        StatusMessage = $"🌸 Fertility blessing for {duration} seconds";
    }

    private void ExecuteAbundance()
    {
        int plants = godPowerService.CreateAbundance();
        StatusMessage = $"🍎 Created {plants} plants";
    }

    private void ExecuteCorruption()
    {
        int corrupted = godPowerService.ExecuteCorruption();
        StatusMessage = $"😈 Corrupted {corrupted} creatures";
    }
}
