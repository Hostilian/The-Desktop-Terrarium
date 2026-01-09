using Microsoft.VisualStudio.TestTools.UnitTesting;
using Terrarium.Logic.Simulation;

namespace Terrarium.Tests.Rendering
{
    [TestClass]
    public class EcosystemHealthTests
    {
        [TestMethod]
        public void HealthScore_TotalExtinction_ReturnsZero()
        {
            double score = EcosystemHealthScorer.CalculateHealthPercent(0, 0, 0);
            Assert.AreEqual(0, score, "Total extinction should return 0 health");
        }

        [TestMethod]
        public void HealthScore_AllSpeciesPresent_HigherThanPartial()
        {
            double fullScore = EcosystemHealthScorer.CalculateHealthPercent(10, 7, 3);
            double partialScore = EcosystemHealthScorer.CalculateHealthPercent(10, 0, 0);

            Assert.IsGreaterThan(partialScore, fullScore,
                "Full ecosystem should have higher score than plants only");
        }

        [TestMethod]
        public void HealthScore_BalancedEcosystem_HighScore()
        {
            // Ideal ratio: ~50% plants, ~35% herbivores, ~15% carnivores
            double score = EcosystemHealthScorer.CalculateHealthPercent(50, 35, 15);

            Assert.IsGreaterThanOrEqualTo(80, score, $"Balanced ecosystem should have high score, got {score}");
        }

        [TestMethod]
        public void HealthScore_ImbalancedEcosystem_LowerScore()
        {
            // All carnivores, no balance
            double score = EcosystemHealthScorer.CalculateHealthPercent(0, 0, 50);

            Assert.IsLessThan(50, score,
                $"Carnivores-only ecosystem should have low score, got {score}");
        }

        [TestMethod]
        public void HealthScore_WithinValidRange()
        {
            // Test various scenarios
            var scenarios = new (int plants, int herbs, int carns)[]
            {
                (10, 5, 2),
                (50, 20, 5),
                (0, 10, 5),
                (20, 0, 5),
                (20, 10, 0),
            };

            foreach (var (plants, herbs, carns) in scenarios)
            {
                double score = EcosystemHealthScorer.CalculateHealthPercent(plants, herbs, carns);
                Assert.IsGreaterThanOrEqualTo(0, score,
                    $"Score {score} should be >= 0 for ({plants}, {herbs}, {carns})");
                Assert.IsLessThanOrEqualTo(100, score,
                    $"Score {score} should be <= 100 for ({plants}, {herbs}, {carns})");
            }
        }
    }
}
