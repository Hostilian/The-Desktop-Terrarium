using Microsoft.VisualStudio.TestTools.UnitTesting;
using Terrarium.Logic.Simulation;

namespace Terrarium.Tests.Simulation
{
    /// <summary>
    /// Unit tests for FoodManager spawning behavior.
    /// Tests plant spawning and ecosystem balance logic.
    /// </summary>
    [TestClass]
    public class FoodManagerTests
    {
        [TestMethod]
        public void FoodManager_InitializeStartingFood_SpawnsPlants()
        {
            // Arrange
            var world = new World(800, 600);
            var foodManager = new FoodManager(world);

            // Act
            foodManager.InitializeStartingFood();

            // Assert
            Assert.IsGreaterThanOrEqualTo(5, world.Plants.Count, "Should spawn at least minimum number of plants");
        }

        [TestMethod]
        public void FoodManager_Update_MaintainsMinimumPlants()
        {
            // Arrange
            var world = new World(800, 600);
            var foodManager = new FoodManager(world);
            // Start with no plants

            // Act - simulate several spawn intervals
            for (int i = 0; i < 20; i++)
            {
                foodManager.Update(5.0); // Each 5s triggers spawn check
            }

            // Assert
            Assert.IsGreaterThanOrEqualTo(5, world.Plants.Count, "FoodManager should maintain minimum plant count");
        }

        [TestMethod]
        public void FoodManager_IsEcosystemBalanced_TrueWhenBalanced()
        {
            // Arrange
            var world = new World(800, 600);
            var foodManager = new FoodManager(world);

            // Create balanced ecosystem: plants > herbivores > carnivores
            for (int i = 0; i < 10; i++)
                world.SpawnRandomPlant();
            for (int i = 0; i < 5; i++)
                world.SpawnRandomHerbivore();
            for (int i = 0; i < 2; i++)
                world.SpawnRandomCarnivore();

            // Act
            bool isBalanced = foodManager.IsEcosystemBalanced();

            // Assert
            Assert.IsTrue(isBalanced);
        }

        [TestMethod]
        public void FoodManager_IsEcosystemBalanced_FalseWhenUnbalanced()
        {
            // Arrange
            var world = new World(800, 600);
            var foodManager = new FoodManager(world);

            // Create unbalanced ecosystem: carnivores > herbivores
            for (int i = 0; i < 2; i++)
                world.SpawnRandomPlant();
            for (int i = 0; i < 2; i++)
                world.SpawnRandomHerbivore();
            for (int i = 0; i < 5; i++)
                world.SpawnRandomCarnivore();

            // Act
            bool isBalanced = foodManager.IsEcosystemBalanced();

            // Assert
            Assert.IsFalse(isBalanced);
        }

        [TestMethod]
        public void FoodManager_GetEcosystemHealth_ReturnsValueBetween0And1()
        {
            // Arrange
            var world = new World(800, 600);
            var foodManager = new FoodManager(world);
            foodManager.InitializeStartingFood();

            // Act
            double health = foodManager.GetEcosystemHealth();

            // Assert
            Assert.IsTrue(health >= 0.0 && health <= 1.0);
        }

        [TestMethod]
        public void FoodManager_PlantSpawnChanceMultiplier_AffectsSpawning()
        {
            // Arrange
            var world = new World(800, 600);
            var foodManager = new FoodManager(world);

            // Act - Set high multiplier and run many updates
            foodManager.PlantSpawnChanceMultiplier = 2.0;
            int initialCount = world.Plants.Count;
            for (int i = 0; i < 50; i++)
            {
                foodManager.Update(5.0);
            }

            // Assert
            Assert.IsGreaterThan(initialCount, world.Plants.Count, "Higher spawn multiplier should result in more plants");
        }

        [TestMethod]
        public void FoodManager_PlantSpawnChanceMultiplier_DefaultIsOne()
        {
            // Arrange
            var world = new World(800, 600);
            var foodManager = new FoodManager(world);

            // Assert
            Assert.AreEqual(1.0, foodManager.PlantSpawnChanceMultiplier, 0.001);
        }

        [TestMethod]
        public void FoodManager_Update_DoesNotSpawnBeyondMax()
        {
            // Arrange
            var world = new World(800, 600);
            var foodManager = new FoodManager(world);

            // Manually spawn many plants
            for (int i = 0; i < 30; i++)
            {
                world.SpawnRandomPlant();
            }
            int initialCount = world.Plants.Count;

            // Act - run several updates
            for (int i = 0; i < 20; i++)
            {
                foodManager.Update(5.0);
            }

            // Assert - should not grow much beyond max (20)
            Assert.IsLessThanOrEqualTo(initialCount + 5, world.Plants.Count, "Should not spawn indefinitely beyond max");
        }
    }
}
