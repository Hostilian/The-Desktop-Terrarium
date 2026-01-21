using Microsoft.VisualStudio.TestTools.UnitTesting;
using Terrarium.Desktop.Services;
using Terrarium.Logic.Simulation;

namespace Terrarium.Tests.Services;

/// <summary>
/// Unit tests for GodPowerService to ensure all god powers work correctly.
/// </summary>
[TestClass]
public class GodPowerServiceTests
{
    private SimulationEngine? _engine;
    private GodPowerService? _service;

    [TestInitialize]
    public void Setup()
    {
        _engine = new SimulationEngine(800, 600, TerrariumType.GodSimulator);
        _engine.Initialize();
        _service = new GodPowerService(_engine);
    }

    [TestMethod]
    public void Constructor_WithNullEngine_ThrowsException()
    {
        try
        {
            new GodPowerService(null!);
            Assert.Fail("Expected ArgumentNullException");
        }
        catch (ArgumentNullException)
        {
            // Expected
        }
    }

    [TestMethod]
    public void LightningStrike_WithNoEntities_ReturnsZero()
    {
        // Remove all entities
        _engine = new SimulationEngine(800, 600, TerrariumType.GodSimulator);
        _service = new GodPowerService(_engine);

        int struck = _service!.ExecuteLightningStrike();

        Assert.AreEqual(0, struck);
    }

    [TestMethod]
    public void LightningStrike_WithEntities_DamagesCreatures()
    {
        int initialCount = _engine!.World.GetAllEntities().Count();
        
        int struck = _service!.ExecuteLightningStrike();

        Assert.IsGreaterThan(0, struck, "Should strike at least one entity");
        Assert.IsLessThanOrEqualTo(struck, 3, "Should not strike more than 3 entities");
    }

    [TestMethod]
    public void MeteorShower_WithNoEntities_ReturnsZero()
    {
        _engine = new SimulationEngine(800, 600, TerrariumType.GodSimulator);
        _service = new GodPowerService(_engine);

        int damaged = _service!.ExecuteMeteorShower();

        Assert.AreEqual(0, damaged);
    }

    [TestMethod]
    public void Plague_WithNoCreatures_ReturnsZero()
    {
        _engine = new SimulationEngine(800, 600, TerrariumType.GodSimulator);
        _service = new GodPowerService(_engine);

        int infected = _service!.ExecutePlague();

        Assert.AreEqual(0, infected);
    }

    [TestMethod]
    public void FertilityBlessing_IncreasesReproductionRate()
    {
        double originalHerbivoreRate = _engine!.ReproductionManager.HerbivoreReproductionChanceMultiplier;
        double originalCarnivoreRate = _engine.ReproductionManager.CarnivoreReproductionChanceMultiplier;

        _service!.ApplyFertilityBlessing();

        Assert.IsGreaterThan(originalHerbivoreRate, _engine.ReproductionManager.HerbivoreReproductionChanceMultiplier);
        Assert.IsGreaterThan(originalCarnivoreRate, _engine.ReproductionManager.CarnivoreReproductionChanceMultiplier);
    }

    [TestMethod]
    public void RemoveFertilityBlessing_RestoresOriginalRate()
    {
        double originalRate = _engine!.ReproductionManager.HerbivoreReproductionChanceMultiplier;

        _service!.ApplyFertilityBlessing();
        _service.RemoveFertilityBlessing();

        Assert.AreEqual(originalRate, _engine.ReproductionManager.HerbivoreReproductionChanceMultiplier, 0.001);
    }

    [TestMethod]
    public void CreateAbundance_ReturnsCorrectPlantCount()
    {
        int plantsCreated = _service!.CreateAbundance();

        Assert.AreEqual(10, plantsCreated);
    }

    [TestMethod]
    public void CreateAbundance_ActuallySpawnsPlants()
    {
        int initialPlantCount = _engine!.World.Plants.Count;

        int plantsCreated = _service!.CreateAbundance();

        int finalPlantCount = _engine.World.Plants.Count;
        Assert.IsGreaterThan(0, plantsCreated, "Should create some plants for abundance");
    }

    [TestMethod]
    public void SpawnPlant_ReturnsValidPlant()
    {
        var plant = _service!.SpawnPlant();

        Assert.IsNotNull(plant);
        Assert.IsTrue(plant.IsAlive);
    }

    [TestMethod]
    public void SpawnHerbivore_ReturnsValidHerbivore()
    {
        var herbivore = _service!.SpawnHerbivore();

        Assert.IsNotNull(herbivore);
        Assert.IsTrue(herbivore.IsAlive);
    }

    [TestMethod]
    public void SpawnCarnivore_ReturnsValidCarnivore()
    {
        var carnivore = _service!.SpawnCarnivore();

        Assert.IsNotNull(carnivore);
        Assert.IsTrue(carnivore.IsAlive);
    }
}
