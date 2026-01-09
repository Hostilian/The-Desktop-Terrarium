using Microsoft.VisualStudio.TestTools.UnitTesting;
using Terrarium.Logic.Entities;

namespace Terrarium.Tests.Entities
{
    /// <summary>
    /// Unit tests for Creature movement and hunger mechanics.
    /// </summary>
    [TestClass]
    public class CreatureTests
    {
        [TestMethod]
        public void Creature_Hunger_IncreasesOverTime()
        {
            // Arrange
            var sheep = new Herbivore(100, 100);
            double initialHunger = sheep.Hunger;

            // Act
            sheep.Update(deltaTime: 5.0);

            // Assert
            Assert.IsGreaterThan(initialHunger, sheep.Hunger, "Hunger should increase over time");
        }

        [TestMethod]
        public void Creature_Feed_ReducesHunger()
        {
            // Arrange
            var sheep = new Herbivore(100, 100);
            sheep.Update(5.0); // Make it hungry
            double hungerBefore = sheep.Hunger;

            // Act
            sheep.Feed(nutritionValue: 30);

            // Assert
            Assert.IsLessThan(hungerBefore, sheep.Hunger, "Feeding should reduce hunger");
        }

        [TestMethod]
        public void Creature_Movement_ChangesPosition()
        {
            // Arrange
            var sheep = new Herbivore(100, 100);
            double initialX = sheep.X;
            double initialY = sheep.Y;

            // Act
            sheep.SetDirection(1, 0); // Move right
            sheep.Update(deltaTime: 1.0);

            // Assert
            Assert.AreNotEqual(initialX, sheep.X, "X position should change after movement");
        }

        [TestMethod]
        public void Creature_SetDirection_SetsVelocity()
        {
            // Arrange
            var sheep = new Herbivore(100, 100);

            // Act
            sheep.SetDirection(1, 0); // Move right

            // Assert
            Assert.IsGreaterThan(0, sheep.VelocityX, "VelocityX should be positive when moving right");
            Assert.AreEqual(0, sheep.VelocityY, 0.01, "VelocityY should be zero when moving horizontally");
        }

        [TestMethod]
        public void Creature_Stop_ZeroesVelocity()
        {
            // Arrange
            var sheep = new Herbivore(100, 100);
            sheep.SetDirection(1, 1);

            // Act
            sheep.Stop();

            // Assert
            Assert.AreEqual(0, sheep.VelocityX, "VelocityX should be zero after stopping");
            Assert.AreEqual(0, sheep.VelocityY, "VelocityY should be zero after stopping");
        }

        [TestMethod]
        public void Creature_OnClick_FeedsCreature()
        {
            // Arrange
            var sheep = new Herbivore(100, 100);
            sheep.Update(5.0);
            double hungerBefore = sheep.Hunger;

            // Act
            sheep.OnClick();

            // Assert
            Assert.IsLessThan(hungerBefore, sheep.Hunger, "Clicking should feed the creature");
        }

        [TestMethod]
        public void Creature_Starvation_CausesHealthLoss()
        {
            // Arrange
            var sheep = new Herbivore(100, 100);

            // Act - Make very hungry by updating without food
            for (int i = 0; i < 100; i++)
            {
                sheep.Update(deltaTime: 1.0);
            }

            // Assert
            Assert.IsLessThan(100, sheep.Health, "High hunger should cause health loss");
        }
    }
}
