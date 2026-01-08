using Microsoft.VisualStudio.TestTools.UnitTesting;
using Terrarium.Logic.Entities;

namespace Terrarium.Tests.Entities
{
    /// <summary>
    /// Unit tests for Plant entity growth and behavior.
    /// Tests the Logic layer independently from the UI.
    /// </summary>
    [TestClass]
    public class PlantTests
    {
        [TestMethod]
        public void Plant_Grow_IncreasesSize()
        {
            // Arrange
            var plant = new Plant(100, 100, initialSize: 10);
            double initialSize = plant.Size;

            // Act
            plant.Grow(deltaTime: 1.0);

            // Assert
            Assert.IsTrue(plant.Size > initialSize, "Plant size should increase after growing");
        }

        [TestMethod]
        public void Plant_WaterLevel_DecreasesOverTime()
        {
            // Arrange
            var plant = new Plant(100, 100);
            double initialWaterLevel = plant.WaterLevel;

            // Act
            plant.Update(deltaTime: 5.0);

            // Assert
            Assert.IsTrue(plant.WaterLevel < initialWaterLevel, "Water level should decrease over time");
        }

        [TestMethod]
        public void Plant_Water_IncreasesWaterLevel()
        {
            // Arrange
            var plant = new Plant(100, 100);
            plant.Update(5.0); // Deplete some water
            double waterBefore = plant.WaterLevel;

            // Act
            plant.Water(30);

            // Assert
            Assert.IsTrue(plant.WaterLevel > waterBefore, "Watering should increase water level");
        }

        [TestMethod]
        public void Plant_DiesWithoutWater()
        {
            // Arrange
            var plant = new Plant(100, 100);

            // Act - Update for long time without water
            for (int i = 0; i < 100; i++)
            {
                plant.Update(deltaTime: 1.0);
            }

            // Assert
            Assert.IsFalse(plant.IsAlive, "Plant should die without water");
        }

        [TestMethod]
        public void Plant_OnClick_WatersPlant()
        {
            // Arrange
            var plant = new Plant(100, 100);
            plant.Update(5.0); // Deplete water
            double waterBefore = plant.WaterLevel;

            // Act
            plant.OnClick();

            // Assert
            Assert.IsTrue(plant.WaterLevel > waterBefore, "Clicking should water the plant");
        }

        [TestMethod]
        public void Plant_ContainsPoint_DetectsClick()
        {
            // Arrange
            var plant = new Plant(100, 100, initialSize: 20);

            // Act & Assert
            Assert.IsTrue(plant.ContainsPoint(100, 100), "Should detect click at plant center");
            Assert.IsTrue(plant.ContainsPoint(110, 110), "Should detect click near plant");
            Assert.IsFalse(plant.ContainsPoint(200, 200), "Should not detect click far away");
        }
    }
}
