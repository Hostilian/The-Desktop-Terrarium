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
        Assert.IsGreaterThan(0, UIConstants.ENTITY_CLICK_TOLERANCE_PIXELS,
            "Click tolerance must be positive");
    }

    [TestMethod]
    public void CreatureClickTolerance_ShouldBePositive()
    {
        Assert.IsGreaterThan(0, UIConstants.CREATURE_CLICK_TOLERANCE_PIXELS,
            "Creature click tolerance must be positive");
    }

    [TestMethod]
    public void CreatureClickTolerance_ShouldBeSmallerThanEntityTolerance()
    {
        Assert.IsLessThan(UIConstants.ENTITY_CLICK_TOLERANCE_PIXELS, UIConstants.CREATURE_CLICK_TOLERANCE_PIXELS,
            "Creature tolerance should be smaller than general entity tolerance for precision");
    }

    [TestMethod]
    public void SimulationSpeedPresets_ShouldNotBeEmpty()
    {
        Assert.IsNotEmpty(UIConstants.SIMULATION_SPEED_PRESETS,
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
            Assert.IsGreaterThan(0.0, speed, $"Speed preset {speed} must be positive");
        }
    }
}
