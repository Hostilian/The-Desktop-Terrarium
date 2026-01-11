using Microsoft.VisualStudio.TestTools.UnitTesting;
using Terrarium.Logic.Entities;

namespace Terrarium.Tests.Entities
{
    /// <summary>
    /// Unit tests for WorldEntity base class functionality.
    /// Tests distance calculations and positioning.
    /// </summary>
    [TestClass]
    public class WorldEntityTests
    {
        [TestMethod]
        public void WorldEntity_Constructor_SetsPosition()
        {
            // Arrange & Act
            var plant = new Plant(100, 200);

            // Assert
            Assert.AreEqual(100, plant.X, "X position should be set correctly");
            Assert.AreEqual(200, plant.Y, "Y position should be set correctly");
        }

        [TestMethod]
        public void WorldEntity_Id_IsUnique()
        {
            // Arrange & Act
            var entity1 = new Plant(0, 0);
            var entity2 = new Plant(0, 0);
            var entity3 = new Herbivore(0, 0);

            // Assert
            Assert.AreNotEqual(entity1.Id, entity2.Id, "Each entity should have a unique ID");
            Assert.AreNotEqual(entity2.Id, entity3.Id, "Each entity should have a unique ID");
        }

        [TestMethod]
        public void WorldEntity_DistanceTo_CalculatesCorrectly()
        {
            // Arrange
            var entity1 = new Plant(0, 0);
            var entity2 = new Plant(3, 4);

            // Act
            double distance = entity1.DistanceTo(entity2);

            // Assert (3-4-5 triangle)
            Assert.AreEqual(5.0, distance, 0.001, "Distance should be calculated using Pythagorean theorem");
        }

        [TestMethod]
        public void WorldEntity_DistanceTo_ZeroWhenSamePosition()
        {
            // Arrange
            var entity1 = new Plant(50, 50);
            var entity2 = new Plant(50, 50);

            // Act
            double distance = entity1.DistanceTo(entity2);

            // Assert
            Assert.AreEqual(0.0, distance, 0.001, "Distance should be zero when entities are at same position");
        }

        [TestMethod]
        public void WorldEntity_DistanceTo_SymmetricResult()
        {
            // Arrange
            var entity1 = new Plant(10, 20);
            var entity2 = new Plant(40, 60);

            // Act
            double distance1To2 = entity1.DistanceTo(entity2);
            double distance2To1 = entity2.DistanceTo(entity1);

            // Assert
            Assert.AreEqual(distance1To2, distance2To1, 0.001, "Distance should be symmetric");
        }

        [TestMethod]
        public void WorldEntity_Position_CanBeModified()
        {
            // Arrange
            var entity = new Plant(0, 0);

            // Act
            entity.X = 100;
            entity.Y = 200;

            // Assert
            Assert.AreEqual(100, entity.X, "X position should be modifiable");
            Assert.AreEqual(200, entity.Y, "Y position should be modifiable");
        }

        [TestMethod]
        public void WorldEntity_DistanceTo_LargeDistanceCalculation()
        {
            // Arrange
            var entity1 = new Plant(0, 0);
            var entity2 = new Plant(1000, 1000);

            // Act
            double distance = entity1.DistanceTo(entity2);

            // Assert
            double expected = Math.Sqrt(1000 * 1000 + 1000 * 1000);
            Assert.AreEqual(expected, distance, 0.001, "Large distances should calculate correctly");
        }
    }
}
