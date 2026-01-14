using Microsoft.VisualStudio.TestTools.UnitTesting;
using Terrarium.Logic.Entities;

namespace Terrarium.Tests.Entities
{
    /// <summary>
    /// Unit tests for LivingEntity health and aging mechanics.
    /// Tests the fundamental life properties shared by all living entities.
    /// </summary>
    [TestClass]
    public class LivingEntityTests
    {
        [TestMethod]
        public void LivingEntity_Constructor_StartsAlive()
        {
            // Arrange & Act
            var plant = new Plant(0, 0);

            // Assert
            Assert.IsTrue(plant.IsAlive);
        }

        [TestMethod]
        public void LivingEntity_Constructor_StartsWithFullHealth()
        {
            // Arrange & Act
            var plant = new Plant(0, 0);

            // Assert
            Assert.AreEqual(100.0, plant.Health, 0.01);
        }

        [TestMethod]
        public void LivingEntity_Constructor_StartsWithZeroAge()
        {
            // Arrange & Act
            var herbivore = new Herbivore(0, 0);

            // Assert
            Assert.AreEqual(0.0, herbivore.Age, 0.01);
        }

        [TestMethod]
        public void LivingEntity_Update_IncreasesAge()
        {
            // Arrange
            var creature = new Herbivore(100, 100);
            double initialAge = creature.Age;

            // Act
            creature.Update(1.0);

            // Assert
            Assert.IsGreaterThan(initialAge, creature.Age, "Age should increase after update");
        }

        [TestMethod]
        public void LivingEntity_TakeDamage_ReducesHealth()
        {
            // Arrange
            var creature = new Herbivore(100, 100);
            double initialHealth = creature.Health;

            // Act
            creature.TakeDamage(25);

            // Assert
            Assert.AreEqual(initialHealth - 25, creature.Health, 0.01);
        }

        [TestMethod]
        public void LivingEntity_TakeDamage_DiesWhenHealthReachesZero()
        {
            // Arrange
            var creature = new Herbivore(100, 100);

            // Act
            creature.TakeDamage(100);

            // Assert
            Assert.IsFalse(creature.IsAlive);
        }

        [TestMethod]
        public void LivingEntity_Heal_IncreasesHealth()
        {
            // Arrange
            var creature = new Herbivore(100, 100);
            creature.TakeDamage(50); // Reduce health to 50
            double healthAfterDamage = creature.Health;

            // Act
            creature.Heal(25);

            // Assert
            Assert.AreEqual(healthAfterDamage + 25, creature.Health, 0.01);
        }

        [TestMethod]
        public void LivingEntity_Health_ClampedToMax()
        {
            // Arrange
            var creature = new Herbivore(100, 100);

            // Act
            creature.Heal(1000); // Try to heal way over max

            // Assert
            Assert.AreEqual(100.0, creature.Health, 0.01);
        }

        [TestMethod]
        public void LivingEntity_Health_ClampedToMin()
        {
            // Arrange
            var creature = new Herbivore(100, 100);

            // Act
            creature.TakeDamage(1000); // Try to damage way below zero

            // Assert
            Assert.AreEqual(0.0, creature.Health, 0.01);
        }

        [TestMethod]
        public void LivingEntity_DeadEntity_DoesNotUpdate()
        {
            // Arrange
            var creature = new Herbivore(100, 100);
            creature.TakeDamage(100); // Kill it
            double ageAtDeath = creature.Age;

            // Act
            creature.Update(10.0);

            // Assert
            Assert.AreEqual(ageAtDeath, creature.Age, 0.01);
        }

        [TestMethod]
        public void LivingEntity_MultipleUpdates_AccumulatesAge()
        {
            // Arrange
            var creature = new Herbivore(100, 100);

            // Act
            creature.Update(1.0);
            creature.Update(2.0);
            creature.Update(0.5);

            // Assert
            Assert.AreEqual(3.5, creature.Age, 0.01);
        }
    }
}
