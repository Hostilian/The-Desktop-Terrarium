using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Terrarium.Tests.Rendering
{
    [TestClass]
    public class EcosystemHealthTests
    {
        [TestMethod]
        public void HealthScore_TotalExtinction_ReturnsZero()
        {
            double score = CalculateHealthScore(0, 0, 0);
            Assert.AreEqual(0, score, "Total extinction should return 0 health");
        }

        [TestMethod]
        public void HealthScore_AllSpeciesPresent_HigherThanPartial()
        {
            double fullScore = CalculateHealthScore(10, 7, 3);
            double partialScore = CalculateHealthScore(10, 0, 0);

            Assert.IsGreaterThan(partialScore, fullScore,
                "Full ecosystem should have higher score than plants only");
        }

        [TestMethod]
        public void HealthScore_BalancedEcosystem_HighScore()
        {
            // Ideal ratio: ~50% plants, ~35% herbivores, ~15% carnivores
            double score = CalculateHealthScore(50, 35, 15);

            Assert.IsGreaterThanOrEqualTo(80, score, $"Balanced ecosystem should have high score, got {score}");
        }

        [TestMethod]
        public void HealthScore_ImbalancedEcosystem_LowerScore()
        {
            // All carnivores, no balance
            double score = CalculateHealthScore(0, 0, 50);

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
                double score = CalculateHealthScore(plants, herbs, carns);
                Assert.IsGreaterThanOrEqualTo(0, score,
                    $"Score {score} should be >= 0 for ({plants}, {herbs}, {carns})");
                Assert.IsLessThanOrEqualTo(100, score,
                    $"Score {score} should be <= 100 for ({plants}, {herbs}, {carns})");
            }
        }

        /// <summary>
        /// Mirrors the EcosystemHealthBar calculation logic.
        /// </summary>
        private double CalculateHealthScore(int plants, int herbivores, int carnivores)
        {
            double score = 100;

            // Penalty for extinction
            if (plants == 0)
                score -= 40;
            if (herbivores == 0)
                score -= 30;
            if (carnivores == 0)
                score -= 20;

            // Check for imbalance
            int total = plants + herbivores + carnivores;
            if (total > 0)
            {
                double plantRatio = (double)plants / total;
                double herbRatio = (double)herbivores / total;
                double carnRatio = (double)carnivores / total;

                // Ideal ratios: ~50% plants, ~35% herbivores, ~15% carnivores
                double plantBalance = 1 - Math.Abs(plantRatio - 0.5) * 1.5;
                double herbBalance = 1 - Math.Abs(herbRatio - 0.35) * 2.0;
                double carnBalance = 1 - Math.Abs(carnRatio - 0.15) * 3.0;

                double balanceScore = (plantBalance + herbBalance + carnBalance) / 3;
                score *= Math.Max(0.3, balanceScore);
            }
            else
            {
                score = 0; // Total extinction
            }

            // Bonus for diversity
            int speciesCount = (plants > 0 ? 1 : 0) + (herbivores > 0 ? 1 : 0) + (carnivores > 0 ? 1 : 0);
            if (speciesCount == 3)
                score = Math.Min(100, score * 1.1);

            // Population size bonus
            if (total >= 20 && total <= 100)
                score = Math.Min(100, score * 1.05);

            return Math.Clamp(score, 0, 100);
        }
    }
}
