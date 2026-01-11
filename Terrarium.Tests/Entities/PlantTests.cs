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
            Assert.IsTrue(initialSize < plant.Size);
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
            Assert.IsTrue(initialWaterLevel > plant.WaterLevel);
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
            Assert.IsTrue(waterBefore < plant.WaterLevel);
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
            Assert.IsFalse(plant.IsAlive);
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
            Assert.IsTrue(waterBefore < plant.WaterLevel);
        }

        [TestMethod]
        public void Plant_ContainsPoint_DetectsClick()
        {
            // Arrange
            var plant = new Plant(100, 100, initialSize: 20);

            // Act & Assert
            Assert.IsTrue(plant.ContainsPoint(100, 100));
            Assert.IsTrue(plant.ContainsPoint(110, 110));
            Assert.IsFalse(plant.ContainsPoint(200, 200));
        }
    }
}
