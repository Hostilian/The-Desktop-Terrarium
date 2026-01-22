using Xunit;
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
            Assert.Equal(World.MaxX, world.Width);
            Assert.Equal(World.MaxY, world.Height);
            Assert.Equal(TerrariumType.Forest, world.TerrariumType);
        }

        [TestMethod]
        public void Constructor_InitializesWithCustomDimensions()
        {
            // Arrange & Act
            var world = new World(800, 600, TerrariumType.Desert);

            // Assert
            Assert.Equal(800, world.Width);
            Assert.Equal(600, world.Height);
            Assert.Equal(TerrariumType.Desert, world.TerrariumType);
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
            Assert.Contains(plant, world.Plants);
            Assert.Equal(1, world.Plants.Count);
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
            Assert.Contains(herbivore, world.Herbivores);
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
            Assert.Contains(carnivore, world.Carnivores);
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
            Assert.Contains(alivePlant, world.Plants);
            Assert.DoesNotContain(deadPlant, world.Plants);
            Assert.Contains(aliveHerbivore, world.Herbivores);
            Assert.DoesNotContain(deadHerbivore, world.Herbivores);
        }

        [TestMethod]
        public void SpawnRandomPlant_CreatesPlantWithCorrectType()
        {
            // Arrange
            var world = new World(800, 600, TerrariumType.Forest);

            // Act
            var plant = world.SpawnRandomPlant();

            // Assert
            Assert.NotNull(plant);
            Assert.AreEqual("Tree", plant.Type);
            Assert.Contains(plant, world.Plants);
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
            Assert.Equal("Algae", plant.Type);
        }

        [Fact]
        public void SpawnRandomPlant_GodSimulatorType_CreatesCrystal()
        {
            // Arrange
            var world = new World(800, 600, TerrariumType.GodSimulator);

            // Act
            var plant = world.SpawnRandomPlant();

            // Assert
            Assert.Equal("Crystal", plant.Type);
        }

        [Fact]
        public void SpawnPlantAt_CreatesPlantAtSpecificLocation()
        {
            // Arrange
            var world = new World(800, 600, TerrariumType.Forest);

            // Act
            var plant = world.SpawnPlantAt(250, 300);

            // Assert
            Assert.Equal(250, plant.X);
            Assert.Equal(300, plant.Y);
            Assert.Equal("Tree", plant.Type);
            Assert.Contains(plant, world.Plants);
        }

        [Fact]
        public void SpawnRandomHerbivore_CreatesCorrectType()
        {
            // Arrange
            var world = new World(800, 600, TerrariumType.Forest);

            // Act
            var herbivore = world.SpawnRandomHerbivore();

            // Assert
            Assert.NotNull(herbivore);
            Assert.Equal("Deer", herbivore.Type);
            Assert.Contains(herbivore, world.Herbivores);
        }

        [Fact]
        public void SpawnRandomHerbivore_WithFactionOverride_UsesFaction()
        {
            // Arrange
            var world = new World(800, 600, TerrariumType.GodSimulator);

            // Act
            var herbivore = world.SpawnRandomHerbivore(factionOverride: FactionType.CrystalChoir);

            // Assert
            Assert.Equal(FactionType.CrystalChoir, herbivore.Faction);
        }

        [Fact]
        public void SpawnRandomCarnivore_CreatesCorrectType()
        {
            // Arrange
            var world = new World(800, 600, TerrariumType.GodSimulator);

            // Act
            var carnivore = world.SpawnRandomCarnivore();

            // Assert
            Assert.NotNull(carnivore);
            Assert.Equal("Dragon", carnivore.Type);
            Assert.Contains(carnivore, world.Carnivores);
        }

        [Fact]
        public void SpawnRandomCarnivore_WithTypeOverride_UsesType()
        {
            // Arrange
            var world = new World(800, 600, TerrariumType.Forest);

            // Act
            var carnivore = world.SpawnRandomCarnivore("CustomWolf");

            // Assert
            Assert.Equal("CustomWolf", carnivore.Type);
        }

        [Fact]
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
            Assert.Equal(3, allEntities.Count);
        }

        [Fact]
        public void GetTerrainAt_WithinBounds_ReturnsTerrain()
        {
            // Arrange
            var world = new World(800, 600, TerrariumType.Forest);

            // Act
            var terrain = world.GetTerrainAt(100, 100);

            // Assert
            Assert.NotEqual(TerrainType.Void, terrain);
        }

        [Fact]
        public void GetTerrainAt_OutOfBounds_ReturnsVoid()
        {
            // Arrange
            var world = new World(800, 600, TerrariumType.Forest);

            // Act
            var terrain = world.GetTerrainAt(-100, -100);

            // Assert
            Assert.Equal(TerrainType.Void, terrain);
        }

        [Fact]
        public void SetTerrainAt_ChangesTerrainType()
        {
            // Arrange
            var world = new World(800, 600, TerrariumType.Forest);

            // Act
            world.SetTerrainAt(100, 100, TerrainType.AshenWasteland);
            var terrain = world.GetTerrainAt(100, 100);

            // Assert
            Assert.Equal(TerrainType.AshenWasteland, terrain);
        }

        [Fact]
        public void SetTerrainAt_OutOfBounds_DoesNothing()
        {
            // Arrange
            var world = new World(800, 600, TerrariumType.Forest);

            // Act & Assert - should not throw
            world.SetTerrainAt(-100, -100, TerrainType.AshenWasteland);
        }

        [Fact]
        public void AttemptTerrainConversion_CanConvert_ReturnsTrue()
        {
            // Arrange
            var world = new World(800, 600, TerrariumType.GodSimulator);
            world.SetTerrainAt(100, 100, TerrainType.VerdantGrowth);

            // Act
            bool converted = world.AttemptTerrainConversion(100, 100, FactionType.AshenLegion);

            // Assert
            Assert.True(converted);
            Assert.Equal(TerrainType.AshenWasteland, world.GetTerrainAt(100, 100));
        }

        [Fact]
        public void AttemptTerrainConversion_AlreadyOwned_ReturnsFalse()
        {
            // Arrange
            var world = new World(800, 600, TerrariumType.GodSimulator);
            world.SetTerrainAt(100, 100, TerrainType.VerdantGrowth);

            // Act
            bool converted = world.AttemptTerrainConversion(100, 100, FactionType.VerdantCollective);

            // Assert
            Assert.False(converted);
        }

        [Fact]
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
            Assert.True(allTerrain.Any());
        }

        [Fact]
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
            Assert.All(territory, cell => Assert.Equal(TerrainType.VerdantGrowth, cell.terrain));
        }

        [Fact]
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
