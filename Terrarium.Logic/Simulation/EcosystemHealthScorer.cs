using System;

namespace Terrarium.Logic.Simulation
{
    public static class EcosystemHealthScorer
    {
        private const double ScoreMin = 0;
        private const double ScoreMax = 100;
        private const double PercentToRatio = 100.0;

        // Extinction penalties
        private const double PlantExtinctionPenalty = 40.0;
        private const double HerbivoreExtinctionPenalty = 30.0;
        private const double CarnivoreExtinctionPenalty = 20.0;

        // Ideal ecosystem ratios
        private const double IdealPlantRatio = 0.5;
        private const double IdealHerbivoreRatio = 0.35;
        private const double IdealCarnivoreRatio = 0.15;

        // Balance multipliers (how sensitive we are to ratio deviations)
        private const double PlantBalanceMultiplier = 1.5;
        private const double HerbivoreBalanceMultiplier = 2.0;
        private const double CarnivoreBalanceMultiplier = 3.0;

        // Score adjustments
        private const double MinBalanceScore = 0.3;
        private const double DiversityBonus = 1.1;
        private const double PopulationSizeBonus = 1.05;

        // Population thresholds
        private const int MinHealthyPopulation = 20;
        private const int MaxHealthyPopulation = 100;
        private const int FullSpeciesCount = 3;

        public static double CalculateHealth01(int plants, int herbivores, int carnivores)
        {
            return CalculateHealthPercent(plants, herbivores, carnivores) / PercentToRatio;
        }

        public static double CalculateHealthPercent(int plants, int herbivores, int carnivores)
        {
            double score = ScoreMax;

            // Penalty for extinction
            if (plants == 0)
                score -= PlantExtinctionPenalty;
            if (herbivores == 0)
                score -= HerbivoreExtinctionPenalty;
            if (carnivores == 0)
                score -= CarnivoreExtinctionPenalty;

            // Check for imbalance
            int total = plants + herbivores + carnivores;
            if (total > 0)
            {
                double plantRatio = (double)plants / total;
                double herbRatio = (double)herbivores / total;
                double carnRatio = (double)carnivores / total;

                // Ideal ratios: ~50% plants, ~35% herbivores, ~15% carnivores
                double plantBalance = 1 - Math.Abs(plantRatio - IdealPlantRatio) * PlantBalanceMultiplier;
                double herbBalance = 1 - Math.Abs(herbRatio - IdealHerbivoreRatio) * HerbivoreBalanceMultiplier;
                double carnBalance = 1 - Math.Abs(carnRatio - IdealCarnivoreRatio) * CarnivoreBalanceMultiplier;

                double balanceScore = (plantBalance + herbBalance + carnBalance) / FullSpeciesCount;
                score *= Math.Max(MinBalanceScore, balanceScore);
            }
            else
            {
                score = ScoreMin; // Total extinction
            }

            // Bonus for diversity
            int speciesCount = (plants > 0 ? 1 : 0) + (herbivores > 0 ? 1 : 0) + (carnivores > 0 ? 1 : 0);
            if (speciesCount == FullSpeciesCount)
                score = Math.Min(ScoreMax, score * DiversityBonus);

            // Population size bonus
            if (total >= MinHealthyPopulation && total <= MaxHealthyPopulation)
                score = Math.Min(ScoreMax, score * PopulationSizeBonus);

            return Math.Clamp(score, ScoreMin, ScoreMax);
        }
    }
}
