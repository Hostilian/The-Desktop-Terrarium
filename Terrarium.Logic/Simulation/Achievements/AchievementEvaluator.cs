using System.Collections.Generic;

namespace Terrarium.Logic.Simulation.Achievements
{
    public static class AchievementEvaluator
    {
        public const int TotalAchievements = 15;

        // Population thresholds
        private const int FirstBirthThreshold = 1;
        private const int PopulationSmallThreshold = 10;
        private const int PopulationMediumThreshold = 25;
        private const int PopulationLargeThreshold = 50;

        // Birth count thresholds
        private const int BirthsLowThreshold = 10;
        private const int BirthsMediumThreshold = 50;
        private const int BirthsHighThreshold = 100;

        // Time thresholds (in seconds)
        private const double TimeShortMinutes = 300.0;   // 5 minutes
        private const double TimeMediumMinutes = 1800.0; // 30 minutes
        private const double TimeLongMinutes = 3600.0;   // 1 hour

        // Plant count thresholds
        private const int PlantsForestThreshold = 20;
        private const int PlantsJungleThreshold = 40;

        // Balance thresholds
        private const int MinCreaturesForBalance = 10;
        private const double HerbivoreRatioMin = 0.6;
        private const double HerbivoreRatioMax = 0.8;

        // Predator thresholds
        private const int ApexPredatorThreshold = 5;

        // Species survival
        private const int MinSpeciesCount = 0;

        public static IReadOnlyList<AchievementInfo> Evaluate(
            int totalBirths,
            int totalDeaths,
            int peakPopulation,
            int currentPlants,
            int currentHerbivores,
            int currentCarnivores,
            double simulationTime)
        {
            var results = new List<AchievementInfo>(capacity: 8);

            // Population achievements
            if (totalBirths >= FirstBirthThreshold)
                results.Add(new AchievementInfo("first_birth", "🎉 First Birth", "Welcome the first creature to the world!"));
            if (peakPopulation >= PopulationSmallThreshold)
                results.Add(new AchievementInfo("population_10", "👨‍👩‍👧‍👦 Growing Family", "Reach 10 total creatures"));
            if (peakPopulation >= PopulationMediumThreshold)
                results.Add(new AchievementInfo("population_25", "🏘️ Village", "Reach 25 total creatures"));
            if (peakPopulation >= PopulationLargeThreshold)
                results.Add(new AchievementInfo("population_50", "🌆 City", "Reach 50 total creatures"));

            // Birth achievements
            if (totalBirths >= BirthsLowThreshold)
                results.Add(new AchievementInfo("births_10", "👶 Nursery", "Witness 10 births"));
            if (totalBirths >= BirthsMediumThreshold)
                results.Add(new AchievementInfo("births_50", "🏥 Maternity Ward", "Witness 50 births"));
            if (totalBirths >= BirthsHighThreshold)
                results.Add(new AchievementInfo("births_100", "🎊 Baby Boom", "Witness 100 births"));

            // Survival achievements
            if (currentPlants > MinSpeciesCount && currentHerbivores > MinSpeciesCount && currentCarnivores > MinSpeciesCount)
                results.Add(new AchievementInfo("survivor", "💪 Survivor", "Have at least one of each species alive"));

            // Time achievements
            if (simulationTime >= TimeShortMinutes)
                results.Add(new AchievementInfo("time_5min", "⏰ Getting Started", "Run simulation for 5 minutes"));
            if (simulationTime >= TimeMediumMinutes)
                results.Add(new AchievementInfo("time_30min", "🕐 Dedicated Observer", "Run simulation for 30 minutes"));
            if (simulationTime >= TimeLongMinutes)
                results.Add(new AchievementInfo("time_1hour", "🏆 Ecosystem Master", "Run simulation for 1 hour"));

            // Plant achievements
            if (currentPlants >= PlantsForestThreshold)
                results.Add(new AchievementInfo("plants_20", "🌳 Forest", "Grow 20 plants"));
            if (currentPlants >= PlantsJungleThreshold)
                results.Add(new AchievementInfo("plants_40", "🌲 Jungle", "Grow 40 plants"));

            // Balance achievements
            int totalCreatures = currentHerbivores + currentCarnivores;
            if (totalCreatures >= MinCreaturesForBalance)
            {
                double herbivoreRatio = (double)currentHerbivores / totalCreatures;
                if (herbivoreRatio >= HerbivoreRatioMin && herbivoreRatio <= HerbivoreRatioMax)
                {
                    results.Add(new AchievementInfo(
                        "balance",
                        "⚖️ Perfect Balance",
                        "Maintain 60-80% herbivore ratio with 10+ creatures"));
                }
            }

            // Predator achievements
            if (currentCarnivores >= ApexPredatorThreshold)
                results.Add(new AchievementInfo("apex_predator", "🦁 Apex Predators", "Have 5+ carnivores alive"));

            return results;
        }
    }
}
