using Microsoft.VisualStudio.TestTools.UnitTesting;
using Terrarium.Desktop.ViewModels;
using Terrarium.Logic.Simulation;

using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace Terrarium.Tests.ViewModels;

/// <summary>
/// Tests for MainWindowViewModel to ensure MVVM pattern works correctly.
/// </summary>
[TestClass]
public class MainWindowViewModelTests
{
    private SimulationEngine? _engine;
    private MainWindowViewModel? _viewModel;

    [TestInitialize]
    public void Setup()
    {
        _engine = new SimulationEngine(800, 600, TerrariumType.GodSimulator);
        _engine.Initialize();
        _viewModel = new MainWindowViewModel(_engine);
    }

    [TestMethod]
    public void Constructor_InitializesWithDefaultValues()
    {
        Assert.IsFalse(_viewModel!.IsPaused);
        Assert.AreEqual(1.0, _viewModel.SimulationSpeed);
        Assert.AreEqual("Ready", _viewModel.StatusMessage);
    }

    [TestMethod]
    public void PlayPauseCommand_TogglesPausedState()
    {
        bool initialState = _viewModel!.IsPaused;

        _viewModel.PlayPauseCommand.Execute(null);

        Assert.AreNotEqual(initialState, _viewModel.IsPaused);
    }

    [TestMethod]
    public void SpeedUpCommand_CyclesThroughSpeeds()
    {
        Assert.AreEqual(1.0, _viewModel!.SimulationSpeed);

        _viewModel.SpeedUpCommand.Execute(null);
        Assert.AreEqual(2.0, _viewModel.SimulationSpeed);

        _viewModel.SpeedUpCommand.Execute(null);
        Assert.AreEqual(5.0, _viewModel.SimulationSpeed);
    }

    [TestMethod]
    public void SpawnPlantCommand_UpdatesStatusMessage()
    {
        _viewModel!.SpawnPlantCommand.Execute(null);

        Assert.IsTrue(_viewModel.StatusMessage.Contains("Plant spawned"));
    }

    [TestMethod]
    public void LightningStrikeCommand_UpdatesStatusMessage()
    {
        _viewModel!.LightningStrikeCommand.Execute(null);

        Assert.IsTrue(_viewModel.StatusMessage.Contains("Lightning"));
    }

    [TestMethod]
    public void PropertyChanged_RaisesEvent()
    {
        bool eventRaised = false;
        _viewModel!.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(_viewModel.IsPaused))
                eventRaised = true;
        };

        _viewModel.IsPaused = true;

        Assert.IsTrue(eventRaised);
    }
}





