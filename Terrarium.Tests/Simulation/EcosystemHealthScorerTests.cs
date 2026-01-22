using Xunit;
using Terrarium.Logic.Simulation;

namespace Terrarium.Tests.Simulation
{
    /// <summary>
    /// Comprehensive tests for EcosystemHealthScorer to achieve 100% coverage.
    /// </summary>
    public class EcosystemHealthScorerTests
    {
        [Fact]
        public void CalculateHealth01_ReturnsValueBetweenZeroAndOne()
        {
            // Arrange & Act
            double health = EcosystemHealthScorer.CalculateHealth01(50, 35, 15);

            // Assert
            Assert.InRange(health, 0.0, 1.0);
        }

        [Fact]
        public void CalculateHealthPercent_IdealBalance_ReturnsHighScore()
        {
            // Arrange - ideal ratios: 50% plants, 35% herbivores, 15% carnivores
            int plants = 50;
            int herbivores = 35;
            int carnivores = 15;

            // Act
            double health = EcosystemHealthScorer.CalculateHealthPercent(plants, herbivores, carnivores);

            // Assert - should be close to 100 with ideal balance
            Assert.InRange(health, 95, 100);
        }

        [Fact]
        public void CalculateHealthPercent_PlantsExtinct_AppliesPenalty()
        {
            // Arrange
            int plants = 0;
            int herbivores = 10;
            int carnivores = 5;

            // Act
            double health = EcosystemHealthScorer.CalculateHealthPercent(plants, herbivores, carnivores);

            // Assert - should have significant penalty
            Assert.InRange(health, 0, 60);
        }

        [Fact]
        public void CalculateHealthPercent_HerbivoresExtinct_AppliesPenalty()
        {
            // Arrange
            int plants = 10;
            int herbivores = 0;
            int carnivores = 5;

            // Act
            double health = EcosystemHealthScorer.CalculateHealthPercent(plants, herbivores, carnivores);

            // Assert - should have penalty
            Assert.InRange(health, 0, 70);
        }

        [Fact]
        public void CalculateHealthPercent_CarnivoresExtinct_AppliesPenalty()
        {
            // Arrange
            int plants = 10;
            int herbivores = 10;
            int carnivores = 0;

            // Act
            double health = EcosystemHealthScorer.CalculateHealthPercent(plants, herbivores, carnivores);

            // Assert - should have penalty
            Assert.InRange(health, 0, 80);
        }

        [Fact]
        public void CalculateHealthPercent_TotalExtinction_ReturnsZero()
        {
            // Arrange
            int plants = 0;
            int herbivores = 0;
            int carnivores = 0;

            // Act
            double health = EcosystemHealthScorer.CalculateHealthPercent(plants, herbivores, carnivores);

            // Assert
            Assert.Equal(0, health);
        }

        [Fact]
        public void CalculateHealthPercent_AllSpeciesPresent_GetsDiversityBonus()
        {
            // Arrange
            int plants = 10;
            int herbivores = 10;
            int carnivores = 10;

            // Act
            double health = EcosystemHealthScorer.CalculateHealthPercent(plants, herbivores, carnivores);

            // Assert - should get diversity bonus
            Assert.True(health > 50);
        }

        [Fact]
        public void CalculateHealthPercent_PopulationInIdealRange_GetsBonus()
        {
            // Arrange - total between 20 and 100
            int plants = 30;
            int herbivores = 20;
            int carnivores = 10;

            // Act
            double health = EcosystemHealthScorer.CalculateHealthPercent(plants, herbivores, carnivores);

            // Assert - should get population bonus
            Assert.InRange(health, 70, 100);
        }

        [Fact]
        public void CalculateHealthPercent_VeryUnbalanced_LowScore()
        {
            // Arrange - almost all carnivores (very unbalanced)
            int plants = 2;
            int herbivores = 2;
            int carnivores = 96;

            // Act
            double health = EcosystemHealthScorer.CalculateHealthPercent(plants, herbivores, carnivores);

            // Assert - should be low due to imbalance
            Assert.InRange(health, 0, 50);
        }

        [Fact]
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
            Assert.Equal(healthPercent / 100.0, health01, precision: 5);
        }

        [Fact]
        public void CalculateHealthPercent_NeverExceedsMaximum()
        {
            // Arrange - perfect conditions
            int plants = 50;
            int herbivores = 35;
            int carnivores = 15;

            // Act
            double health = EcosystemHealthScorer.CalculateHealthPercent(plants, herbivores, carnivores);

            // Assert
            Assert.True(health <= 100);
        }

        [Fact]
        public void CalculateHealthPercent_NeverGoesNegative()
        {
            // Arrange - worst conditions
            int plants = 100;
            int herbivores = 0;
            int carnivores = 0;

            // Act
            double health = EcosystemHealthScorer.CalculateHealthPercent(plants, herbivores, carnivores);

            // Assert
            Assert.True(health >= 0);
        }
    }
}
