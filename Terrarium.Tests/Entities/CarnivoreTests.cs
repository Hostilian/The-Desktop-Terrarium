using Microsoft.VisualStudio.TestTools.UnitTesting;
using Terrarium.Logic.Entities;

namespace Terrarium.Tests.Entities
{
    /// <summary>
    /// Unit tests for Carnivore hunting behavior.
    /// Tests the predator-prey interaction.
    /// </summary>
    [TestClass]
    public class CarnivoreTests
    {
        [TestMethod]
        public void Carnivore_TryEat_AttacksPrey()
        {
            // Arrange
            var wolf = new Carnivore(100, 100);
            var sheep = new Herbivore(105, 105);
            double sheepHealthBefore = sheep.Health;

            // Act
            wolf.TryEat(sheep);

            // Assert
            Assert.IsLessThan(sheepHealthBefore, sheep.Health, "Wolf should damage prey");
        }

        [TestMethod]
        public void Carnivore_TryEat_KillsAndEatsPrey()
        {
            // Arrange
            var wolf = new Carnivore(100, 100);
            wolf.Update(10.0); // Make wolf hungry
            var sheep = new Herbivore(105, 105);
            double wolfHungerBefore = wolf.Hunger;

            // Act - Attack until sheep dies
            while (sheep.IsAlive)
            {
                wolf.TryEat(sheep);
            }

            // Assert
            Assert.IsFalse(sheep.IsAlive, "Sheep should be dead after wolf attack");
            Assert.IsLessThan(wolfHungerBefore, wolf.Hunger, "Wolf should be less hungry after eating");
        }

        [TestMethod]
        public void Carnivore_FindNearestPrey_FindsClosest()
        {
            // Arrange
            var wolf = new Carnivore(100, 100);
            var herbivores = new List<Herbivore>
            {
                new Herbivore(300, 300, "Rabbit"), // Far
                new Herbivore(120, 110, "Sheep"),  // Close
                new Herbivore(400, 400, "Deer")    // Very far
            };

            // Act
            var nearestPrey = wolf.FindNearestPrey(herbivores);

            // Assert
            Assert.IsNotNull(nearestPrey, "Should find nearby prey");
            Assert.AreEqual(120, nearestPrey.X, "Should find the closest prey");
        }

        [TestMethod]
        public void Carnivore_Hunt_MovesTowardTarget()
        {
            // Arrange
            var wolf = new Carnivore(100, 100);
            var sheep = new Herbivore(200, 100);
            double initialDistance = wolf.DistanceTo(sheep);

            // Act
            wolf.Hunt(sheep);
            wolf.Update(1.0);

            // Assert
            double finalDistance = wolf.DistanceTo(sheep);
            Assert.IsLessThan(initialDistance, finalDistance, "Wolf should get closer to prey");
        }

        [TestMethod]
        public void Carnivore_FasterThanHerbivore()
        {
            // Arrange
            var wolf = new Carnivore(100, 100);
            var sheep = new Herbivore(100, 100);

            // Assert
            Assert.IsGreaterThan(sheep.Speed, wolf.Speed, "Carnivores should be faster than herbivores");
        }
    }
}
