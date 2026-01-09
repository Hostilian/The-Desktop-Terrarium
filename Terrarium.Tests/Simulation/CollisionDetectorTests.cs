using Microsoft.VisualStudio.TestTools.UnitTesting;
using Terrarium.Logic.Simulation;
using Terrarium.Logic.Entities;

namespace Terrarium.Tests.Simulation
{
    /// <summary>
    /// Unit tests for CollisionDetector.
    /// Tests collision detection logic.
    /// </summary>
    [TestClass]
    public class CollisionDetectorTests
    {
        [TestMethod]
        public void CollisionDetector_AreColliding_DetectsCollision()
        {
            // Arrange
            var detector = new CollisionDetector();
            var creature1 = new Herbivore(100, 100);
            var creature2 = new Herbivore(110, 110); // Very close

            // Act
            bool colliding = detector.AreColliding(creature1, creature2);

            // Assert
            Assert.IsTrue(colliding, "Should detect collision between nearby creatures");
        }

        [TestMethod]
        public void CollisionDetector_AreColliding_NoCollisionWhenFar()
        {
            // Arrange
            var detector = new CollisionDetector();
            var creature1 = new Herbivore(100, 100);
            var creature2 = new Herbivore(300, 300); // Far away

            // Act
            bool colliding = detector.AreColliding(creature1, creature2);

            // Assert
            Assert.IsFalse(colliding, "Should not detect collision when entities are far apart");
        }

        [TestMethod]
        public void CollisionDetector_FindNearbyEntities_FindsCloseOnes()
        {
            // Arrange
            var detector = new CollisionDetector();
            var center = new Herbivore(100, 100);
            var entities = new List<Plant>
            {
                new Plant(110, 110), // Close
                new Plant(300, 300), // Far
                new Plant(120, 100)  // Close
            };

            // Act
            var nearby = detector.FindNearbyEntities(center, entities, radius: 50);

            // Assert
            Assert.HasCount(2, nearby);
        }

        [TestMethod]
        public void CollisionDetector_ResolveCreatureCollision_SeparatesCreatures()
        {
            // Arrange
            var detector = new CollisionDetector();
            var creature1 = new Herbivore(100, 100);
            var creature2 = new Herbivore(100, 100); // Same position
            double initialDistance = creature1.DistanceTo(creature2);

            // Act
            detector.ResolveCreatureCollision(creature1, creature2);

            // Assert
            double finalDistance = creature1.DistanceTo(creature2);
            Assert.IsGreaterThan(initialDistance, finalDistance, "Creatures should be pushed apart");
        }

        [TestMethod]
        public void CollisionDetector_ResolveCreatureCollisions_SeparatesCreaturesAcrossCellBoundary()
        {
            // Arrange
            var detector = new CollisionDetector();
            var creature1 = new Herbivore(39, 100);
            var creature2 = new Herbivore(41, 100); // Very close, likely in adjacent spatial cells
            double initialDistance = creature1.DistanceTo(creature2);

            var creatures = new List<Creature> { creature1, creature2 };

            // Act
            detector.ResolveCreatureCollisions(creatures);

            // Assert
            double finalDistance = creature1.DistanceTo(creature2);
            Assert.IsGreaterThan(initialDistance, finalDistance, "Creatures should be pushed apart even when in adjacent cells");
        }
    }
}
