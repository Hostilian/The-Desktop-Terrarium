namespace Terrarium.Tests.Simulation
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Terrarium.Logic.Simulation;
    using Terrarium.Logic.Simulation.Achievements;

    [TestClass]
    /// <summary>
    /// Comprehensive tests for AchievementEvaluator to achieve 100% coverage.
    /// </summary>
    public class AchievementEvaluatorTests
    {
        [TestMethod]
        public void Evaluate_FirstBirth_ReturnsAchievement()
        {
            // Arrange & Act
            var achievements = AchievementEvaluator.Evaluate(
                totalBirths: 1,
                totalDeaths: 0,
                peakPopulation: 1,
                currentPlants: 5,
                currentHerbivores: 1,
                currentCarnivores: 0,
                simulationTime: 0);

            // Assert
            Assert.IsTrue(achievements.Any(a => a.Id == "first_birth"));
        }

        [TestMethod]
        public void Evaluate_Population10_ReturnsAchievement()
        {
            // Arrange & Act
            var achievements = AchievementEvaluator.Evaluate(
                totalBirths: 5,
                totalDeaths: 0,
                peakPopulation: 10,
                currentPlants: 5,
                currentHerbivores: 5,
                currentCarnivores: 0,
                simulationTime: 0);

            // Assert
            Assert.IsTrue(achievements.Any(a => a.Id == "population_10"));
        }

        [TestMethod]
        public void Evaluate_Population25_ReturnsAchievement()
        {
            // Arrange & Act
            var achievements = AchievementEvaluator.Evaluate(
                totalBirths: 10,
                totalDeaths: 0,
                peakPopulation: 25,
                currentPlants: 10,
                currentHerbivores: 10,
                currentCarnivores: 5,
                simulationTime: 0);

            // Assert
            Assert.IsTrue(achievements.Any(a => a.Id == "population_25"));
        }

        [TestMethod]
        public void Evaluate_Population50_ReturnsAchievement()
        {
            // Arrange & Act
            var achievements = AchievementEvaluator.Evaluate(
                totalBirths: 20,
                totalDeaths: 0,
                peakPopulation: 50,
                currentPlants: 20,
                currentHerbivores: 20,
                currentCarnivores: 10,
                simulationTime: 0);

            // Assert
            Assert.IsTrue(achievements.Any(a => a.Id == "population_50"));
        }

        [TestMethod]
        public void Evaluate_Births10_ReturnsAchievement()
        {
            // Arrange & Act
            var achievements = AchievementEvaluator.Evaluate(
                totalBirths: 10,
                totalDeaths: 0,
                peakPopulation: 5,
                currentPlants: 5,
                currentHerbivores: 5,
                currentCarnivores: 0,
                simulationTime: 0);

            // Assert
            Assert.IsTrue(achievements.Any(a => a.Id == "births_10"));
        }

        [TestMethod]
        public void Evaluate_Births50_ReturnsAchievement()
        {
            // Arrange & Act
            var achievements = AchievementEvaluator.Evaluate(
                totalBirths: 50,
                totalDeaths: 0,
                peakPopulation: 10,
                currentPlants: 5,
                currentHerbivores: 5,
                currentCarnivores: 0,
                simulationTime: 0);

            // Assert
            Assert.IsTrue(achievements.Any(a => a.Id == "births_50"));
        }

        [TestMethod]
        public void Evaluate_Births100_ReturnsAchievement()
        {
            // Arrange & Act
            var achievements = AchievementEvaluator.Evaluate(
                totalBirths: 100,
                totalDeaths: 10,
                peakPopulation: 20,
                currentPlants: 10,
                currentHerbivores: 10,
                currentCarnivores: 5,
                simulationTime: 0);

            // Assert
            Assert.IsTrue(achievements.Any(a => a.Id == "births_100"));
        }

        [TestMethod]
        public void Evaluate_Survivor_AllSpeciesAlive_ReturnsAchievement()
        {
            // Arrange & Act
            var achievements = AchievementEvaluator.Evaluate(
                totalBirths: 10,
                totalDeaths: 0,
                peakPopulation: 10,
                currentPlants: 5,
                currentHerbivores: 3,
                currentCarnivores: 2,
                simulationTime: 0);

            // Assert
            Assert.IsTrue(achievements.Any(a => a.Id == "survivor"));
        }

        [TestMethod]
        public void Evaluate_Survivor_PlantsExtinct_DoesNotReturnAchievement()
        {
            // Arrange & Act
            var achievements = AchievementEvaluator.Evaluate(
                totalBirths: 10,
                totalDeaths: 0,
                peakPopulation: 10,
                currentPlants: 0,
                currentHerbivores: 5,
                currentCarnivores: 5,
                simulationTime: 0);

            // Assert
            Assert.IsFalse(achievements.Any(a => a.Id == "survivor"));
        }

        [TestMethod]
        public void Evaluate_Time5Minutes_ReturnsAchievement()
        {
            // Arrange & Act
            var achievements = AchievementEvaluator.Evaluate(
                totalBirths: 5,
                totalDeaths: 0,
                peakPopulation: 5,
                currentPlants: 5,
                currentHerbivores: 2,
                currentCarnivores: 0,
                simulationTime: 300);

            // Assert
            Assert.IsTrue(achievements.Any(a => a.Id == "time_5min"));
        }

        [TestMethod]
        public void Evaluate_Time30Minutes_ReturnsAchievement()
        {
            // Arrange & Act
            var achievements = AchievementEvaluator.Evaluate(
                totalBirths: 20,
                totalDeaths: 5,
                peakPopulation: 15,
                currentPlants: 10,
                currentHerbivores: 5,
                currentCarnivores: 2,
                simulationTime: 1800);

            // Assert
            Assert.IsTrue(achievements.Any(a => a.Id == "time_30min"));
        }

        [TestMethod]
        public void Evaluate_Time1Hour_ReturnsAchievement()
        {
            // Arrange & Act
            var achievements = AchievementEvaluator.Evaluate(
                totalBirths: 50,
                totalDeaths: 10,
                peakPopulation: 30,
                currentPlants: 15,
                currentHerbivores: 10,
                currentCarnivores: 5,
                simulationTime: 3600);

            // Assert
            Assert.IsTrue(achievements.Any(a => a.Id == "time_1hour"));
        }

        [TestMethod]
        public void Evaluate_Plants20_ReturnsAchievement()
        {
            // Arrange & Act
            var achievements = AchievementEvaluator.Evaluate(
                totalBirths: 10,
                totalDeaths: 0,
                peakPopulation: 30,
                currentPlants: 20,
                currentHerbivores: 8,
                currentCarnivores: 2,
                simulationTime: 100);

            // Assert
            Assert.IsTrue(achievements.Any(a => a.Id == "plants_20"));
        }

        [TestMethod]
        public void Evaluate_Plants40_ReturnsAchievement()
        {
            // Arrange & Act
            var achievements = AchievementEvaluator.Evaluate(
                totalBirths: 20,
                totalDeaths: 0,
                peakPopulation: 50,
                currentPlants: 40,
                currentHerbivores: 8,
                currentCarnivores: 2,
                simulationTime: 200);

            // Assert
            Assert.IsTrue(achievements.Any(a => a.Id == "plants_40"));
        }

        [TestMethod]
        public void Evaluate_PerfectBalance_ReturnsAchievement()
        {
            // Arrange - 70% herbiv ratio (between 60-80%)
            var achievements = AchievementEvaluator.Evaluate(
                totalBirths: 10,
                totalDeaths: 0,
                peakPopulation: 20,
                currentPlants: 10,
                currentHerbivores: 7,
                currentCarnivores: 3,
                simulationTime: 100);

            // Assert
            Assert.IsTrue(achievements.Any(a => a.Id == "balance"));
        }

        [TestMethod]
        public void Evaluate_PerfectBalance_TooFewCreatures_DoesNotReturnAchievement()
        {
            // Arrange - only 9 creatures, need 10+
            var achievements = AchievementEvaluator.Evaluate(
                totalBirths: 5,
                totalDeaths: 0,
                peakPopulation: 9,
                currentPlants: 10,
                currentHerbivores: 6,
                currentCarnivores: 3,
                simulationTime: 50);

            // Assert
            Assert.IsFalse(achievements.Any(a => a.Id == "balance"));
        }

        [TestMethod]
        public void Evaluate_PerfectBalance_RatioTooLow_DoesNotReturnAchievement()
        {
            // Arrange - only 50% herbivores (below 60%)
            var achievements = AchievementEvaluator.Evaluate(
                totalBirths: 10,
                totalDeaths: 0,
                peakPopulation: 20,
                currentPlants: 10,
                currentHerbivores: 5,
                currentCarnivores: 5,
                simulationTime: 100);

            // Assert
            Assert.IsFalse(achievements.Any(a => a.Id == "balance"));
        }

        [TestMethod]
        public void Evaluate_ApexPredator_ReturnsAchievement()
        {
            // Arrange
            var achievements = AchievementEvaluator.Evaluate(
                totalBirths: 15,
                totalDeaths: 0,
                peakPopulation: 20,
                currentPlants: 10,
                currentHerbivores: 5,
                currentCarnivores: 5,
                simulationTime: 200);

            // Assert
            Assert.IsTrue(achievements.Any(a => a.Id == "apex_predator"));
        }

        [TestMethod]
        public void Evaluate_NoAchievements_ReturnsEmptyList()
        {
            // Arrange
            var achievements = AchievementEvaluator.Evaluate(
                totalBirths: 0,
                totalDeaths: 0,
                peakPopulation: 0,
                currentPlants: 0,
                currentHerbivores: 0,
                currentCarnivores: 0,
                simulationTime: 0);

            // Assert
            Assert.IsEmpty(achievements);
        }

        [TestMethod]
        public void Evaluate_AllAchievements_ReturnsMultiple()
        {
            // Arrange - values that trigger many achievements
            var achievements = AchievementEvaluator.Evaluate(
                totalBirths: 100,
                totalDeaths: 10,
                peakPopulation: 50,
                currentPlants: 40,
                currentHerbivores: 7,
                currentCarnivores: 5,
                simulationTime: 3600);

            // Assert - should have many achievements
            Assert.IsGreaterThanOrEqualTo(10, achievements.Count);
        }
    }
}
