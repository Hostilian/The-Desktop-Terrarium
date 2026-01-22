namespace Terrarium.Tests.ViewModels;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Terrarium.Desktop.ViewModels;
using Terrarium.Logic.Simulation;

/// <summary>
/// Tests for MainWindowViewModel to ensure MVVM pattern works correctly.
/// </summary>
[TestClass]
public class MainWindowViewModelTests
{
    private SimulationEngine? engine;
    private MainWindowViewModel? viewModel;

    [TestInitialize]
    public void Setup()
    {
        engine = new SimulationEngine(800, 600, TerrariumType.GodSimulator);
        engine.Initialize();
        viewModel = new MainWindowViewModel(engine);
    }

    [TestMethod]
    public void Constructor_InitializesWithDefaultValues()
    {
        Assert.IsFalse(viewModel!.IsPaused);
        Assert.AreEqual(1.0, viewModel.SimulationSpeed);
        Assert.AreEqual("Ready", viewModel.StatusMessage);
    }

    [TestMethod]
    public void PlayPauseCommand_TogglesPausedState()
    {
        bool initialState = viewModel!.IsPaused;

        viewModel.PlayPauseCommand.Execute(null);

        Assert.AreNotEqual(initialState, viewModel.IsPaused);
    }

    [TestMethod]
    public void SpeedUpCommand_CyclesThroughSpeeds()
    {
        Assert.AreEqual(1.0, viewModel!.SimulationSpeed);

        viewModel.SpeedUpCommand.Execute(null);
        Assert.AreEqual(2.0, viewModel.SimulationSpeed);

        viewModel.SpeedUpCommand.Execute(null);
        Assert.AreEqual(5.0, viewModel.SimulationSpeed);
    }

    [TestMethod]
    public void SpawnPlantCommand_UpdatesStatusMessage()
    {
        viewModel!.SpawnPlantCommand.Execute(null);

        Assert.Contains("Plant spawned", viewModel.StatusMessage);
    }

    [TestMethod]
    public void LightningStrikeCommand_UpdatesStatusMessage()
    {
        viewModel!.LightningStrikeCommand.Execute(null);

        Assert.Contains("Lightning", viewModel.StatusMessage);
    }

    [TestMethod]
    public void PropertyChanged_RaisesEvent()
    {
        bool eventRaised = false;
        viewModel!.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(viewModel.IsPaused))
                eventRaised = true;
        };

        viewModel.IsPaused = true;

        Assert.IsTrue(eventRaised);
    }
}
