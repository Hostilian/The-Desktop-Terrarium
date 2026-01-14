using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Terrarium.Logic.Simulation.Achievements;

namespace Terrarium.Tests.Rendering
{
    [TestClass]
    public class AchievementSystemTests
    {
        [TestMethod]
        public void AchievementSystem_Evaluator_DoesNotUnlockAtZeroStats()
        {
            var unlocked = AchievementEvaluator.Evaluate(
                totalBirths: 0,
                totalDeaths: 0,
                peakPopulation: 0,
                currentPlants: 0,
                currentHerbivores: 0,
                currentCarnivores: 0,
                simulationTime: 0);

            Assert.IsEmpty(unlocked);
        }

        [TestMethod]
        public void AchievementSystem_Evaluator_UnlocksFirstBirthAtOneBirth()
        {
            var unlocked = AchievementEvaluator.Evaluate(
                totalBirths: 1,
                totalDeaths: 0,
                peakPopulation: 1,
                currentPlants: 1,
                currentHerbivores: 1,
                currentCarnivores: 0,
                simulationTime: 0);

            Assert.IsTrue(unlocked.Any(a => a.Id == "first_birth"));
        }

        [TestMethod]
        public void AchievementSystem_Evaluator_BalanceRequiresTenPlusCreatures()
        {
            // Exactly 9 creatures: should not unlock balance even if ratio is in-range.
            var unlockedTooSmall = AchievementEvaluator.Evaluate(
                totalBirths: 0,
                totalDeaths: 0,
                peakPopulation: 9,
                currentPlants: 10,
                currentHerbivores: 6,
                currentCarnivores: 3,
                simulationTime: 0);
            Assert.IsFalse(unlockedTooSmall.Any(a => a.Id == "balance"));

            // 10 creatures with 70% herbivores: should unlock balance.
            var unlockedInRange = AchievementEvaluator.Evaluate(
                totalBirths: 0,
                totalDeaths: 0,
                peakPopulation: 10,
                currentPlants: 10,
                currentHerbivores: 7,
                currentCarnivores: 3,
                simulationTime: 0);
            Assert.IsTrue(unlockedInRange.Any(a => a.Id == "balance"));
        }

        [TestMethod]
        public void AchievementSystem_Evaluator_AllAchievementsCanBeUnlocked()
        {
            var unlocked = AchievementEvaluator.Evaluate(
                totalBirths: 100,
                totalDeaths: 0,
                peakPopulation: 50,
                currentPlants: 40,
                currentHerbivores: 8,
                currentCarnivores: 5,
                simulationTime: 3600);

            int uniqueCount = unlocked.Select(a => a.Id).Distinct().Count();
            Assert.AreEqual(AchievementEvaluator.TotalAchievements, uniqueCount);
        }
    }
}
