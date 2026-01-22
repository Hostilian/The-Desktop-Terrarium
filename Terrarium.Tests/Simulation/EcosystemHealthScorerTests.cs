
using Terrarium.Logic.Simulation;
using Terrarium.Logic.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Terrarium.Tests.Simulation
{
    /// <summary>
    /// Comprehensive tests for EcosystemHealthScorer to achieve 100% coverage.
    /// </summary>
    public class EcosystemHealthScorerTests
    {
        [TestMethod]
        public void CalculateHealth01_ReturnsValueBetweenZeroAndOne()
        {
            // Arrange & Act
            double health = EcosystemHealthScorer.CalculateHealth01(50, 35, 15);

            // Assert
            Assert.IsTrue(health >= 0.0 && health <= 1.0);
        }

        [TestMethod]
        public void CalculateHealthPercent_IdealBalance_ReturnsHighScore()
        {
            // Arrange - ideal ratios: 50% plants, 35% herbivores, 15% carnivores
            int plants = 50;
            int herbivores = 35;
            int carnivores = 15;

            // Act
            double health = EcosystemHealthScorer.CalculateHealthPercent(plants, herbivores, carnivores);

            // Assert - should be close to 100 with ideal balance
            Assert.IsTrue(health >= 95 && health <= 100);
        }

        [TestMethod]
        public void CalculateHealthPercent_PlantsExtinct_AppliesPenalty()
        {
            // Arrange
            int plants = 0;
            int herbivores = 10;
            int carnivores = 5;

            // Act
            double health = EcosystemHealthScorer.CalculateHealthPercent(plants, herbivores, carnivores);

            // Assert - should have significant penalty
            Assert.IsTrue(health >= 0 && health <= 60);
        }

        [TestMethod]
        public void CalculateHealthPercent_HerbivoresExtinct_AppliesPenalty()
        {
            // Arrange
            int plants = 10;
            int herbivores = 0;
            int carnivores = 5;

            // Act
            double health = EcosystemHealthScorer.CalculateHealthPercent(plants, herbivores, carnivores);

            // Assert - should have penalty
            Assert.IsTrue(health >= 0 && health <= 70);
        }

        [TestMethod]
        public void CalculateHealthPercent_CarnivoresExtinct_AppliesPenalty()
        {
            // Arrange
            int plants = 10;
            int herbivores = 10;
            int carnivores = 0;

            // Act
            double health = EcosystemHealthScorer.CalculateHealthPercent(plants, herbivores, carnivores);

            // Assert - should have penalty
            Assert.IsTrue(health >= 0 && health <= 80);
        }

        [TestMethod]
        public void CalculateHealthPercent_TotalExtinction_ReturnsZero()
        {
            // Arrange
            int plants = 0;
            int herbivores = 0;
            int carnivores = 0;

            // Act
            double health = EcosystemHealthScorer.CalculateHealthPercent(plants, herbivores, carnivores);

            // Assert
            Assert.AreEqual(0, health);
        }

        [TestMethod]
        public void CalculateHealthPercent_AllSpeciesPresent_GetsDiversityBonus()
        {
            // Arrange
            int plants = 10;
            int herbivores = 10;
            int carnivores = 10;

            // Act
            double health = EcosystemHealthScorer.CalculateHealthPercent(plants, herbivores, carnivores);

            // Assert - should get diversity bonus
            Assert.IsTrue(health > 50);
        }

        [TestMethod]
        public void CalculateHealthPercent_PopulationInIdealRange_GetsBonus()
        {
            // Arrange - total between 20 and 100
            int plants = 30;
            int herbivores = 20;
            int carnivores = 10;

            // Act
            double health = EcosystemHealthScorer.CalculateHealthPercent(plants, herbivores, carnivores);

            // Assert - should get population bonus
            Assert.IsTrue(health >= 70 && health <= 100);
        }

        [TestMethod]
        public void CalculateHealthPercent_VeryUnbalanced_LowScore()
        {
            // Arrange - almost all carnivores (very unbalanced)
            int plants = 2;
            int herbivores = 2;
            int carnivores = 96;

            // Act
            double health = EcosystemHealthScorer.CalculateHealthPercent(plants, herbivores, carnivores);

            // Assert - should be low due to imbalance
            Assert.IsTrue(health >= 0 && health <= 50);
        }

        [TestMethod]
        public void CalculateHealth01_MatchesPercentDividedBy100()
        {
            // Arrange
            int plants = 50;
            int herbivores = 35;
            int carnivores = 15;

            // Act
            double health01 = EcosystemHealthScorer.CalculateHealth01(plants, herbivores, carnivores);
            double healthPercent = EcosystemHealthScorer.CalculateHealthPercent(plants, herbivores, carnivores);

            // Assert
            Assert.AreEqual(healthPercent / 100.0, health01, 0.00001);
        }

        [TestMethod]
        public void CalculateHealthPercent_NeverExceedsMaximum()
        {
            // Arrange - perfect conditions
            int plants = 50;
            int herbivores = 35;
            int carnivores = 15;

            // Act
            double health = EcosystemHealthScorer.CalculateHealthPercent(plants, herbivores, carnivores);

            // Assert
            Assert.IsTrue(health <= 100);
        }

        [TestMethod]
        public void CalculateHealthPercent_NeverGoesNegative()
        {
            // Arrange - worst conditions
            int plants = 100;
            int herbivores = 0;
            int carnivores = 0;

            // Act
            double health = EcosystemHealthScorer.CalculateHealthPercent(plants, herbivores, carnivores);

            // Assert
            Assert.IsTrue(health >= 0);
        }
    }
}





