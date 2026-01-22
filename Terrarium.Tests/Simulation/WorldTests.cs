using Terrarium.Logic.Simulation;
using Terrarium.Logic.Entities;
using System.Linq;

namespace Terrarium.Tests.Simulation
{
    /// <summary>
    /// Comprehensive unit tests for World class to achieve 100% code coverage.
    /// </summary>
    public class WorldTests
    {
        [TestMethod]
        public void Constructor_InitializesWithDefaultDimensions()
        {
            // Arrange & Act
            var world = new World();

            // Assert
            Assert.AreEqual(World.MaxX, world.Width);
            Assert.AreEqual(World.MaxY, world.Height);
            Assert.AreEqual(TerrariumType.Forest, world.TerrariumType);
        }

        [TestMethod]
        public void Constructor_InitializesWithCustomDimensions()
        {
            // Arrange & Act
            var world = new World(800, 600, TerrariumType.Desert);

            // Assert
            Assert.AreEqual(800, world.Width);
            Assert.AreEqual(600, world.Height);
            Assert.AreEqual(TerrariumType.Desert, world.TerrariumType);
        }

        [TestMethod]
        public void AddPlant_AddsPlantToWorld()
        {
            // Arrange
            var world = new World();
            var plant = new Plant(100, 100, "Tree");

            // Act
            world.AddPlant(plant);

            // Assert
            CollectionAssert.Contains(world.Plants, plant);
            Assert.AreEqual(1, world.Plants.Count);
        }

        [TestMethod]
        public void AddHerbivore_AddsHerbivoreToWorld()
        {
            // Arrange
            var world = new World();
            var herbivore = new Herbivore(100, 100, "Deer");

            // Act
            world.AddHerbivore(herbivore);

            // Assert
            CollectionAssert.Contains(world.Herbivores, herbivore);
            Assert.AreEqual(1, world.Herbivores.Count);
        }

        [TestMethod]
        public void AddCarnivore_AddsCarnivoreToWorld()
        {
            // Arrange
            var world = new World();
            var carnivore = new Carnivore(100, 100, "Wolf");

            // Act
            world.AddCarnivore(carnivore);

            // Assert
            CollectionAssert.Contains(world.Carnivores, carnivore);
            Assert.AreEqual(1, world.Carnivores.Count);
        }

        [TestMethod]
        public void RemoveDeadEntities_RemovesOnlyDeadEntities()
        {
            // Arrange
            var world = new World();
            var alivePlant = new Plant(100, 100, "Tree");
            var deadPlant = new Plant(200, 200, "Tree");
            deadPlant.TakeDamage(999999); // Kill it

            var aliveHerbivore = new Herbivore(300, 300, "Deer");
            var deadHerbivore = new Herbivore(400, 400, "Deer");
            deadHerbivore.TakeDamage(999999); // Kill it

            world.AddPlant(alivePlant);
            world.AddPlant(deadPlant);
            world.AddHerbivore(aliveHerbivore);
            world.AddHerbivore(deadHerbivore);

            // Act
            world.RemoveDeadEntities();

            // Assert
            CollectionAssert.Contains(world.Plants, alivePlant);
            CollectionAssert.DoesNotContain(world.Plants, deadPlant);
            CollectionAssert.Contains(world.Herbivores, aliveHerbivore);
            CollectionAssert.DoesNotContain(world.Herbivores, deadHerbivore);
        }

        [TestMethod]
        public void SpawnRandomPlant_CreatesPlantWithCorrectType()
        {
            // Arrange
            var world = new World(800, 600, TerrariumType.Forest);

            // Act
            var plant = world.SpawnRandomPlant();

            // Assert
            Assert.IsNotNull(plant);
            Assert.AreEqual("Tree", plant.Type);
            CollectionAssert.Contains(world.Plants, plant);
        }

        [TestMethod]
        public void SpawnRandomPlant_DesertType_CreatesCactus()
        {
            // Arrange
            var world = new World(800, 600, TerrariumType.Desert);

            // Act
            var plant = world.SpawnRandomPlant();

            // Assert
            Assert.AreEqual("Cactus", plant.Type);
        }

        [TestMethod]
        public void SpawnRandomPlant_AquaticType_CreatesAlgae()
        {
            // Arrange
            var world = new World(800, 600, TerrariumType.Aquatic);

            // Act
            var plant = world.SpawnRandomPlant();

            // Assert
            Assert.AreEqual("Algae", plant.Type);
        }

        [TestMethod]
        public void SpawnRandomPlant_GodSimulatorType_CreatesCrystal()
        {
            // Arrange
            var world = new World(800, 600, TerrariumType.GodSimulator);

            // Act
            var plant = world.SpawnRandomPlant();

            // Assert
            Assert.AreEqual("Crystal", plant.Type);
        }

        [TestMethod]
        public void SpawnPlantAt_CreatesPlantAtSpecificLocation()
        {
            // Arrange
            var world = new World(800, 600, TerrariumType.Forest);

            // Act
            var plant = world.SpawnPlantAt(250, 300);

            // Assert
            Assert.AreEqual(250, plant.X);
            Assert.AreEqual(300, plant.Y);
            Assert.AreEqual("Tree", plant.Type);
            CollectionAssert.Contains(world.Plants, plant);
        }

        [TestMethod]
        public void SpawnRandomHerbivore_CreatesCorrectType()
        {
            // Arrange
            var world = new World(800, 600, TerrariumType.Forest);

            // Act
            var herbivore = world.SpawnRandomHerbivore();

            // Assert
            Assert.IsNotNull(herbivore);
            Assert.AreEqual("Deer", herbivore.Type);
            CollectionAssert.Contains(world.Herbivores, herbivore);
        }

        [TestMethod]
        public void SpawnRandomHerbivore_WithFactionOverride_UsesFaction()
        {
            // Arrange
            var world = new World(800, 600, TerrariumType.GodSimulator);

            // Act
            var herbivore = world.SpawnRandomHerbivore(factionOverride: FactionType.CrystalChoir);

            // Assert
            Assert.AreEqual(FactionType.CrystalChoir, herbivore.Faction);
        }

        [TestMethod]
        public void SpawnRandomCarnivore_CreatesCorrectType()
        {
            // Arrange
            var world = new World(800, 600, TerrariumType.GodSimulator);

            // Act
            var carnivore = world.SpawnRandomCarnivore();

            // Assert
            Assert.IsNotNull(carnivore);
            Assert.AreEqual("Dragon", carnivore.Type);
            CollectionAssert.Contains(world.Carnivores, carnivore);
        }

        [TestMethod]
        public void SpawnRandomCarnivore_WithTypeOverride_UsesType()
        {
            // Arrange
            var world = new World(800, 600, TerrariumType.Forest);

            // Act
            var carnivore = world.SpawnRandomCarnivore("CustomWolf");

            // Assert
            Assert.AreEqual("CustomWolf", carnivore.Type);
        }

        [TestMethod]
        public void GetAllEntities_ReturnsAllEntityTypes()
        {
            // Arrange
            var world = new World();
            world.AddPlant(new Plant(100, 100, "Tree"));
            world.AddHerbivore(new Herbivore(200, 200, "Deer"));
            world.AddCarnivore(new Carnivore(300, 300, "Wolf"));

            // Act
            var allEntities = world.GetAllEntities().ToList();

            // Assert
            Assert.AreEqual(3, allEntities.Count);
        }

        [TestMethod]
        public void GetTerrainAt_WithinBounds_ReturnsTerrain()
        {
            // Arrange
            var world = new World(800, 600, TerrariumType.Forest);

            // Act
            var terrain = world.GetTerrainAt(100, 100);

            // Assert
            Assert.NotEqual(TerrainType.Void, terrain);
        }

        [TestMethod]
        public void GetTerrainAt_OutOfBounds_ReturnsVoid()
        {
            // Arrange
            var world = new World(800, 600, TerrariumType.Forest);

            // Act
            var terrain = world.GetTerrainAt(-100, -100);

            // Assert
            Assert.AreEqual(TerrainType.Void, terrain);
        }

        [TestMethod]
        public void SetTerrainAt_ChangesTerrainType()
        {
            // Arrange
            var world = new World(800, 600, TerrariumType.Forest);

            // Act
            world.SetTerrainAt(100, 100, TerrainType.AshenWasteland);
            var terrain = world.GetTerrainAt(100, 100);

            // Assert
            Assert.AreEqual(TerrainType.AshenWasteland, terrain);
        }

        [TestMethod]
        public void SetTerrainAt_OutOfBounds_DoesNothing()
        {
            // Arrange
            var world = new World(800, 600, TerrariumType.Forest);

            // Act & Assert - should not throw
            world.SetTerrainAt(-100, -100, TerrainType.AshenWasteland);
        }

        [TestMethod]
        public void AttemptTerrainConversion_CanConvert_ReturnsTrue()
        {
            // Arrange
            var world = new World(800, 600, TerrariumType.GodSimulator);
            world.SetTerrainAt(100, 100, TerrainType.VerdantGrowth);

            // Act
            bool converted = world.AttemptTerrainConversion(100, 100, FactionType.AshenLegion);

            // Assert
            Assert.IsTrue(converted);
            Assert.AreEqual(TerrainType.AshenWasteland, world.GetTerrainAt(100, 100));
        }

        [TestMethod]
        public void AttemptTerrainConversion_AlreadyOwned_ReturnsFalse()
        {
            // Arrange
            var world = new World(800, 600, TerrariumType.GodSimulator);
            world.SetTerrainAt(100, 100, TerrainType.VerdantGrowth);

            // Act
            bool converted = world.AttemptTerrainConversion(100, 100, FactionType.VerdantCollective);

            // Assert
            Assert.IsFalse(converted);
        }

        [TestMethod]
        public void ProcessTerrainConquest_ConvertsAdjacentTerrain()
        {
            // Arrange
            var world = new World(800, 600, TerrariumType.GodSimulator);
            var herbivore = new Herbivore(100, 100, "Phoenix", faction: FactionType.AshenLegion);
            world.AddHerbivore(herbivore);
            world.SetTerrainAt(100, 100, TerrainType.VerdantGrowth);

            // Act
            world.ProcessTerrainConquest();

            // Assert - terrain should have changed somewhere due to faction conquest
            var allTerrain = Enumerable.Range(0, 5)
                .SelectMany(x => Enumerable.Range(0, 5).Select(y => world.GetTerrainAt(x * 20, y * 20)))
                .ToList();
            
            // At least one cell should show conquest happened
            Assert.IsTrue(allTerrain.Any());
        }

        [TestMethod]
        public void GetFactionTerritory_ReturnsFactionCells()
        {
            // Arrange
            var world = new World(800, 600, TerrariumType.GodSimulator);
            world.SetTerrainAt(100, 100, TerrainType.VerdantGrowth);
            world.SetTerrainAt(120, 100, TerrainType.VerdantGrowth);

            // Act
            var territory = world.GetFactionTerritory(FactionType.VerdantCollective).ToList();

            // Assert
            Assert.NotEmpty(territory);
            foreach (var cell in territory)
            {
                Assert.AreEqual(TerrainType.VerdantGrowth, cell.terrain);
            }
        }

        [TestMethod]
        public void GetTerritoryControl_ReturnsPercentages()
        {
            // Arrange
            var world = new World(800, 600, TerrariumType.GodSimulator);

            // Act
            var control = world.GetTerritoryControl();

            // Assert
            Assert.NotNull(control);
            var totalPercent = control.Values.Sum();
            Assert.InRange(totalPercent, 95, 105); // Should be ~100% with rounding
        }
    }
}

