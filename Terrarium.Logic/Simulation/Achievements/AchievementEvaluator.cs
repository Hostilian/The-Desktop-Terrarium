namespace Terrarium.Logic.Simulation.Achievements
{
    using System.Collections.Generic;

    public static class AchievementEvaluator
    {
        public const int TotalAchievements = 15;

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
            if (totalBirths >= 1)
                results.Add(new AchievementInfo("first_birth", "🎉 First Birth", "Welcome the first creature to the world!"));
            if (peakPopulation >= 10)
                results.Add(new AchievementInfo("population_10", "👨‍👩‍👧‍👦 Growing Family", "Reach 10 total creatures"));
            if (peakPopulation >= 25)
                results.Add(new AchievementInfo("population_25", "🏘️ Village", "Reach 25 total creatures"));
            if (peakPopulation >= 50)
                results.Add(new AchievementInfo("population_50", "🌆 City", "Reach 50 total creatures"));

            // Birth achievements
            if (totalBirths >= 10)
                results.Add(new AchievementInfo("births_10", "👶 Nursery", "Witness 10 births"));
            if (totalBirths >= 50)
                results.Add(new AchievementInfo("births_50", "🏥 Maternity Ward", "Witness 50 births"));
            if (totalBirths >= 100)
                results.Add(new AchievementInfo("births_100", "🎊 Baby Boom", "Witness 100 births"));

            // Survival achievements
            if (currentPlants > 0 && currentHerbivores > 0 && currentCarnivores > 0)
                results.Add(new AchievementInfo("survivor", "💪 Survivor", "Have at least one of each species alive"));

            // Time achievements
            if (simulationTime >= 300)
                results.Add(new AchievementInfo("time_5min", "⏰ Getting Started", "Run simulation for 5 minutes"));
            if (simulationTime >= 1800)
                results.Add(new AchievementInfo("time_30min", "🕐 Dedicated Observer", "Run simulation for 30 minutes"));
            if (simulationTime >= 3600)
                results.Add(new AchievementInfo("time_1hour", "🏆 Ecosystem Master", "Run simulation for 1 hour"));

            // Plant achievements
            if (currentPlants >= 20)
                results.Add(new AchievementInfo("plants_20", "🌳 Forest", "Grow 20 plants"));
            if (currentPlants >= 40)
                results.Add(new AchievementInfo("plants_40", "🌲 Jungle", "Grow 40 plants"));

            // Balance achievements
            int totalCreatures = currentHerbivores + currentCarnivores;
            if (totalCreatures >= 10)
            {
                double herbivoreRatio = (double)currentHerbivores / totalCreatures;
                if (herbivoreRatio >= 0.6 && herbivoreRatio <= 0.8)
                {
                    results.Add(new AchievementInfo(
                        "balance",
                        "⚖️ Perfect Balance",
                        "Maintain 60-80% herbivore ratio with 10+ creatures"));
                }
            }

            // Predator achievements
            if (currentCarnivores >= 5)
                results.Add(new AchievementInfo("apex_predator", "🦁 Apex Predators", "Have 5+ carnivores alive"));

            return results;
        }
    }
}
