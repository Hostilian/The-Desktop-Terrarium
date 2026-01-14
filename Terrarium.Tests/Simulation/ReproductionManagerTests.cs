using Microsoft.VisualStudio.TestTools.UnitTesting;
using Terrarium.Logic.Entities;
using Terrarium.Logic.Simulation;

namespace Terrarium.Tests.Simulation
{
    /// <summary>
    /// Unit tests for ReproductionManager.
    /// </summary>
    [TestClass]
    public class ReproductionManagerTests
    {
        [TestMethod]
        public void ReproductionManager_CanReproduce_ReturnsFalse_WhenLowHealth()
        {
            // Arrange
            var world = new World(500, 500);
            var manager = new ReproductionManager(world);
            var herbivore = new Herbivore(100, 100);
            herbivore.TakeDamage(50); // Reduce health below threshold

            // Act
            bool canReproduce = manager.CanReproduce(herbivore);

            // Assert
            Assert.IsFalse(canReproduce);
        }

        [TestMethod]
        public void ReproductionManager_CanReproduce_ReturnsFalse_WhenTooHungry()
        {
            // Arrange
            var world = new World(500, 500);
            var manager = new ReproductionManager(world);
            var herbivore = new Herbivore(100, 100);

            // Make the creature hungry by updating many times
            for (int i = 0; i < 200; i++)
            {
                herbivore.Update(1.0);
            }

            // Act
            bool canReproduce = manager.CanReproduce(herbivore);

            // Assert
            Assert.IsFalse(canReproduce);
        }

        [TestMethod]
        public void ReproductionManager_CanReproduce_ReturnsFalse_WhenTooYoung()
        {
            // Arrange
            var world = new World(500, 500);
            var manager = new ReproductionManager(world);
            var herbivore = new Herbivore(100, 100);
            herbivore.Feed(50); // Well fed

            // Act - creature is brand new, age = 0
            bool canReproduce = manager.CanReproduce(herbivore);

            // Assert
            Assert.IsFalse(canReproduce);
        }

        [TestMethod]
        public void ReproductionManager_CanReproduce_ReturnsTrue_WhenConditionsMet()
        {
            // Arrange
            var world = new World(500, 500);
            var manager = new ReproductionManager(world);
            var herbivore = new Herbivore(100, 100);

            // Feed well and age the creature
            herbivore.Feed(50);
            for (int i = 0; i < 15; i++)
            {
                herbivore.Update(1.0);
                herbivore.Feed(10); // Keep feeding to prevent hunger
            }

            // Act
            bool canReproduce = manager.CanReproduce(herbivore);

            // Assert
            Assert.IsTrue(canReproduce);
        }

        [TestMethod]
        public void ReproductionManager_CanReproduce_ReturnsFalse_WhenDead()
        {
            // Arrange
            var world = new World(500, 500);
            var manager = new ReproductionManager(world);
            var herbivore = new Herbivore(100, 100);
            herbivore.TakeDamage(150); // Kill the creature

            // Act
            bool canReproduce = manager.CanReproduce(herbivore);

            // Assert
            Assert.IsFalse(canReproduce);
        }

        [TestMethod]
        public void ReproductionManager_Update_DoesNotExceedMaxPopulation()
        {
            // Arrange
            var world = new World(500, 500);
            var manager = new ReproductionManager(world);

            // Add maximum herbivores
            for (int i = 0; i < 15; i++)
            {
                world.AddHerbivore(new Herbivore(i * 30, 100));
            }

            int initialCount = world.Herbivores.Count;

            // Act
            manager.Update(1.0);

            // Assert - Should not exceed max
            Assert.IsLessThanOrEqualTo(world.Herbivores.Count, 15);
        }

        [TestMethod]
        public void ReproductionManager_ClearCooldown_AllowsReproduction()
        {
            // Arrange
            var world = new World(500, 500);
            var manager = new ReproductionManager(world);
            var herbivore = new Herbivore(100, 100);

            // Set up and clear cooldown
            manager.ClearCooldown(herbivore.Id);

            // Feed well and age the creature
            herbivore.Feed(50);
            for (int i = 0; i < 15; i++)
            {
                herbivore.Update(1.0);
                herbivore.Feed(10);
            }

            // Act
            bool canReproduce = manager.CanReproduce(herbivore);

            // Assert
            Assert.IsTrue(canReproduce);
        }
    }
}
