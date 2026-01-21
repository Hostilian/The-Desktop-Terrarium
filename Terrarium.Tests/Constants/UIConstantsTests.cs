using Microsoft.VisualStudio.TestTools.UnitTesting;
using Terrarium.Desktop.Constants;

namespace Terrarium.Tests.Constants;

/// <summary>
/// Unit tests for UIConstants to verify interaction values are valid.
/// </summary>
[TestClass]
public class UIConstantsTests
{
    [TestMethod]
    public void EntityClickTolerance_ShouldBePositive()
    {
        Assert.IsTrue(UIConstants.ENTITY_CLICK_TOLERANCE_PIXELS > 0,
            "Click tolerance must be positive");
    }

    [TestMethod]
    public void CreatureClickTolerance_ShouldBePositive()
    {
        Assert.IsTrue(UIConstants.CREATURE_CLICK_TOLERANCE_PIXELS > 0,
            "Creature click tolerance must be positive");
    }

    [TestMethod]
    public void CreatureClickTolerance_ShouldBeSmallerThanEntityTolerance()
    {
        Assert.IsTrue(UIConstants.CREATURE_CLICK_TOLERANCE_PIXELS < UIConstants.ENTITY_CLICK_TOLERANCE_PIXELS,
            "Creature tolerance should be smaller than general entity tolerance for precision");
    }

    [TestMethod]
    public void SimulationSpeedPresets_ShouldNotBeEmpty()
    {
        Assert.IsTrue(UIConstants.SIMULATION_SPEED_PRESETS.Length > 0,
            "Must have at least one speed preset");
    }

    [TestMethod]
    public void SimulationSpeedPresets_ShouldStartWithNormalSpeed()
    {
        Assert.AreEqual(1.0, UIConstants.SIMULATION_SPEED_PRESETS[0],
            "First speed preset should be 1.0 (normal speed)");
    }

    [TestMethod]
    public void SimulationSpeedPresets_ShouldBeInAscendingOrder()
    {
        for (int i = 1; i < UIConstants.SIMULATION_SPEED_PRESETS.Length; i++)
        {
            Assert.IsTrue(UIConstants.SIMULATION_SPEED_PRESETS[i] > UIConstants.SIMULATION_SPEED_PRESETS[i - 1],
                $"Speed presets must be in ascending order. Found {UIConstants.SIMULATION_SPEED_PRESETS[i]} after {UIConstants.SIMULATION_SPEED_PRESETS[i - 1]}");
        }
    }

    [TestMethod]
    public void SimulationSpeedPresets_AllValuesShouldBePositive()
    {
        foreach (var speed in UIConstants.SIMULATION_SPEED_PRESETS)
        {
            Assert.IsGreaterThan(speed, 0.0, $"Speed preset {speed} must be positive");
        }
    }
}
