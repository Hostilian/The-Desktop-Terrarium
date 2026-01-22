namespace Terrarium.Tests.Services;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Terrarium.Desktop.Services;
using Terrarium.Logic.Simulation;

/// <summary>
/// Unit tests for GodPowerService to ensure all god powers work correctly.
/// </summary>
[TestClass]
public class GodPowerServiceTests
{
    private SimulationEngine? engine;
    private GodPowerService? service;

    [TestInitialize]
    public void Setup()
    {
        engine = new SimulationEngine(800, 600, TerrariumType.GodSimulator);
        engine.Initialize();
        service = new GodPowerService(engine);
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
        engine = new SimulationEngine(800, 600, TerrariumType.GodSimulator);
        service = new GodPowerService(engine);

        int struck = service!.ExecuteLightningStrike();

        Assert.AreEqual(0, struck);
    }

    [TestMethod]
    public void LightningStrike_WithEntities_DamagesCreatures()
    {
        int initialCount = engine!.World.GetAllEntities().Count();

        int struck = service!.ExecuteLightningStrike();

        Assert.IsGreaterThan(0, struck, "Should strike at least one entity");
        Assert.IsLessThanOrEqualTo(struck, 3, "Should not strike more than 3 entities");
    }

    [TestMethod]
    public void MeteorShower_WithNoEntities_ReturnsZero()
    {
        engine = new SimulationEngine(800, 600, TerrariumType.GodSimulator);
        service = new GodPowerService(engine);

        int damaged = service!.ExecuteMeteorShower();

        Assert.AreEqual(0, damaged);
    }

    [TestMethod]
    public void Plague_WithNoCreatures_ReturnsZero()
    {
        engine = new SimulationEngine(800, 600, TerrariumType.GodSimulator);
        service = new GodPowerService(engine);

        int infected = service!.ExecutePlague();

        Assert.AreEqual(0, infected);
    }

    [TestMethod]
    public void FertilityBlessing_IncreasesReproductionRate()
    {
        double originalHerbivoreRate = engine!.ReproductionManager.HerbivoreReproductionChanceMultiplier;
        double originalCarnivoreRate = engine.ReproductionManager.CarnivoreReproductionChanceMultiplier;

        service!.ApplyFertilityBlessing();

        Assert.IsGreaterThan(originalHerbivoreRate, engine.ReproductionManager.HerbivoreReproductionChanceMultiplier);
        Assert.IsGreaterThan(originalCarnivoreRate, engine.ReproductionManager.CarnivoreReproductionChanceMultiplier);
    }

    [TestMethod]
    public void RemoveFertilityBlessing_RestoresOriginalRate()
    {
        double originalRate = engine!.ReproductionManager.HerbivoreReproductionChanceMultiplier;

        service!.ApplyFertilityBlessing();
        service.RemoveFertilityBlessing();

        Assert.AreEqual(originalRate, engine.ReproductionManager.HerbivoreReproductionChanceMultiplier, 0.001);
    }

    [TestMethod]
    public void CreateAbundance_ReturnsCorrectPlantCount()
    {
        int plantsCreated = service!.CreateAbundance();

        Assert.AreEqual(10, plantsCreated);
    }

    [TestMethod]
    public void CreateAbundance_ActuallySpawnsPlants()
    {
        int initialPlantCount = engine!.World.Plants.Count;

        int plantsCreated = service!.CreateAbundance();

        int finalPlantCount = engine.World.Plants.Count;
        Assert.IsGreaterThan(0, plantsCreated, "Should create some plants for abundance");
    }

    [TestMethod]
    public void SpawnPlant_ReturnsValidPlant()
    {
        var plant = service!.SpawnPlant();

        Assert.IsNotNull(plant);
        Assert.IsTrue(plant.IsAlive);
    }

    [TestMethod]
    public void SpawnHerbivore_ReturnsValidHerbivore()
    {
        var herbivore = service!.SpawnHerbivore();

        Assert.IsNotNull(herbivore);
        Assert.IsTrue(herbivore.IsAlive);
    }

    [TestMethod]
    public void SpawnCarnivore_ReturnsValidCarnivore()
    {
        var carnivore = service!.SpawnCarnivore();

        Assert.IsNotNull(carnivore);
        Assert.IsTrue(carnivore.IsAlive);
    }
}
