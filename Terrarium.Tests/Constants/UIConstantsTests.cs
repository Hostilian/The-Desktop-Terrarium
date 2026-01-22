namespace Terrarium.Tests.Constants;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Terrarium.Desktop.Constants;

/// <summary>
/// Unit tests for UIConstants to verify interaction values are valid.
/// </summary>
[TestClass]
public class UIConstantsTests
{
    [TestMethod]
    public void EntityClickTolerance_ShouldBePositive()
    {
        Assert.IsGreaterThan(
            0,
            UIConstants.ENTITYCLICKTOLERANCEPIXELS,
            "Click tolerance must be positive");
    }

    [TestMethod]
    public void CreatureClickTolerance_ShouldBePositive()
    {
        Assert.IsGreaterThan(
            0,
            UIConstants.CREATURECLICKTOLERANCEPIXELS,
            "Creature click tolerance must be positive");
    }

    [TestMethod]
    public void CreatureClickTolerance_ShouldBeSmallerThanEntityTolerance()
    {
        Assert.IsLessThan(
            UIConstants.ENTITYCLICKTOLERANCEPIXELS,
            UIConstants.CREATURECLICKTOLERANCEPIXELS,
            "Creature tolerance should be smaller than general entity tolerance for precision");
    }

    [TestMethod]
    public void SimulationSpeedPresets_ShouldNotBeEmpty()
    {
        Assert.IsNotEmpty(UIConstants.SIMULATIONSPEEDPRESETS,
            "Must have at least one speed preset");
    }

    [TestMethod]
    public void SimulationSpeedPresets_ShouldStartWithNormalSpeed()
    {
        Assert.AreEqual(
            1.0,
            UIConstants.SIMULATIONSPEEDPRESETS[0],
            "First speed preset should be 1.0 (normal speed)");
    }

    [TestMethod]
    public void SimulationSpeedPresets_ShouldBeInAscendingOrder()
    {
#pragma warning disable MSTEST0037 // Assert.IsTrue is appropriate for complex loop conditions
        for (int i = 1; i < UIConstants.SIMULATIONSPEEDPRESETS.Length; i++)
        {
            Assert.IsTrue(UIConstants.SIMULATIONSPEEDPRESETS[i] > UIConstants.SIMULATIONSPEEDPRESETS[i - 1],
                $"Speed presets must be in ascending order. Found {UIConstants.SIMULATIONSPEEDPRESETS[i]} after {UIConstants.SIMULATIONSPEEDPRESETS[i - 1]}");
        }
#pragma warning restore MSTEST0037
    }

    [TestMethod]
    public void SimulationSpeedPresets_AllValuesShouldBePositive()
    {
        foreach (var speed in UIConstants.SIMULATIONSPEEDPRESETS)
        {
            Assert.IsGreaterThan(0.0, speed, $"Speed preset {speed} must be positive");
        }
    }
}
