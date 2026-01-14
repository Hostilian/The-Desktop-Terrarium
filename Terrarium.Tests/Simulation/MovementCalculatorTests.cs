using Microsoft.VisualStudio.TestTools.UnitTesting;
using Terrarium.Logic.Simulation;
using Terrarium.Logic.Entities;

namespace Terrarium.Tests.Simulation
{
    /// <summary>
    /// Unit tests for MovementCalculator.
    /// Tests movement logic separately from entity classes.
    /// </summary>
    [TestClass]
    public class MovementCalculatorTests
    {
        [TestMethod]
        public void MovementCalculator_EnforceBoundaries_KeepsEntityInBounds()
        {
            // Arrange
            var world = new World(500, 500);
            var calculator = new MovementCalculator(world);
            var creature = new Herbivore(-10, -10); // Out of bounds

            // Act
            calculator.EnforceBoundaries(creature);

            // Assert
            Assert.IsGreaterThanOrEqualTo(0, creature.X, "X should be kept within bounds");
            Assert.IsGreaterThanOrEqualTo(0, creature.Y, "Y should be kept within bounds");
        }

        [TestMethod]
        public void MovementCalculator_EnforceBoundaries_BouncesCreature()
        {
            // Arrange
            var world = new World(500, 500);
            var calculator = new MovementCalculator(world);
            var creature = new Herbivore(10, 250);
            creature.SetDirection(-1, 0); // Moving left toward boundary
            double velocityXBefore = creature.VelocityX;

            // Act
            creature.X = -10; // Force out of bounds
            calculator.EnforceBoundaries(creature);

            // Assert
            Assert.IsGreaterThan(0, creature.VelocityX, "Velocity should reverse when hitting boundary");
        }

        [TestMethod]
        public void MovementCalculator_RandomizeDirection_SetsVelocity()
        {
            // Arrange
            var world = new World(500, 500);
            var calculator = new MovementCalculator(world);
            var creature = new Herbivore(250, 250);
            creature.Stop();

            // Act
            calculator.RandomizeDirection(creature);

            // Assert
            bool hasVelocity = creature.VelocityX != 0 || creature.VelocityY != 0;
            Assert.IsTrue(hasVelocity, "Creature should have velocity after randomization");
        }

        [TestMethod]
        public void MovementCalculator_MoveToward_ApproachesTarget()
        {
            // Arrange
            var world = new World(500, 500);
            var calculator = new MovementCalculator(world);
            var creature = new Herbivore(100, 100);

            // Act
            calculator.MoveToward(creature, 200, 100);

            // Assert
            Assert.IsGreaterThan(0, creature.VelocityX, "Should move toward target on X axis");
        }
    }
}
