using Microsoft.VisualStudio.TestTools.UnitTesting;
using Terrarium.Logic.Entities;

namespace Terrarium.Tests.Entities
{
    /// <summary>
    /// Unit tests for Herbivore eating behavior.
    /// Tests the interaction between herbivores and plants.
    /// </summary>
    [TestClass]
    public class HerbivoreTests
    {
        [TestMethod]
        public void Herbivore_TryEat_EatsNearbyPlant()
        {
            // Arrange
            var sheep = new Herbivore(100, 100);
            var plant = new Plant(105, 105); // Close to sheep
            sheep.Update(5.0); // Make hungry

            // Act
            bool ateSuccessfully = sheep.TryEat(plant);

            // Assert
            Assert.IsTrue(ateSuccessfully, "Herbivore should eat nearby plant");
            Assert.IsLessThan(50, sheep.Hunger, "Hunger should decrease after eating");
        }

        [TestMethod]
        public void Herbivore_TryEat_FailsWhenPlantTooFar()
        {
            // Arrange
            var sheep = new Herbivore(100, 100);
            var plant = new Plant(200, 200); // Far from sheep

            // Act
            bool ateSuccessfully = sheep.TryEat(plant);

            // Assert
            Assert.IsFalse(ateSuccessfully, "Herbivore should not eat plant that's too far");
        }

        [TestMethod]
        public void Herbivore_Eating_DamagesPlant()
        {
            // Arrange
            var sheep = new Herbivore(100, 100);
            var plant = new Plant(105, 105);
            double plantHealthBefore = plant.Health;

            // Act
            sheep.TryEat(plant);

            // Assert
            Assert.IsLessThan(plantHealthBefore, plant.Health, "Eating should damage the plant");
        }

        [TestMethod]
        public void Herbivore_FindNearestPlant_FindsClosest()
        {
            // Arrange
            var sheep = new Herbivore(100, 100);
            var closePlant = new Plant(120, 110);
            var plants = new List<Plant>
            {
                new Plant(200, 200), // Far
                closePlant,           // Close
                new Plant(300, 300)   // Very far
            };

            // Act
            var nearestPlant = sheep.FindNearestPlant(plants);

            // Assert
            Assert.IsNotNull(nearestPlant, "Should find a nearby plant");
            Assert.AreSame(closePlant, nearestPlant, "Should return the exact closest plant instance");
        }

        [TestMethod]
        public void Herbivore_MoveToward_ApproachesTarget()
        {
            // Arrange
            var sheep = new Herbivore(100, 100);
            double targetX = 200;
            double targetY = 100;

            // Act
            sheep.MoveToward(targetX, targetY);
            sheep.Update(1.0);

            // Assert
            Assert.IsGreaterThan(100, sheep.X, "Herbivore should move toward target");
        }
    }
}
