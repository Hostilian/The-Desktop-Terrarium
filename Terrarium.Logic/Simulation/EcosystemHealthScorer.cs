using System;

namespace Terrarium.Logic.Simulation
{
    public static class EcosystemHealthScorer
    {
        private const double ScoreMin = 0;
        private const double ScoreMax = 100;

        public static double CalculateHealth01(int plants, int herbivores, int carnivores)
        {
            return CalculateHealthPercent(plants, herbivores, carnivores) / 100.0;
        }

        public static double CalculateHealthPercent(int plants, int herbivores, int carnivores)
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
                score = Math.Min(ScoreMax, score * 1.1);

            // Population size bonus
            if (total >= 20 && total <= 100)
                score = Math.Min(ScoreMax, score * 1.05);

            return Math.Clamp(score, ScoreMin, ScoreMax);
        }
    }
}
