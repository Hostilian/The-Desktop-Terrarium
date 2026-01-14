using Microsoft.VisualStudio.TestTools.UnitTesting;
using Terrarium.Logic.Entities;
using Terrarium.Logic.Simulation;

namespace Terrarium.Tests.Simulation
{
    /// <summary>
    /// Unit tests for StatisticsTracker.
    /// </summary>
    [TestClass]
    public class StatisticsTrackerTests
    {
        [TestMethod]
        public void StatisticsTracker_InitializesWithZeroValues()
        {
            // Arrange & Act
            var tracker = new StatisticsTracker();

            // Assert
            Assert.AreEqual(0, tracker.TotalBirths);
            Assert.AreEqual(0, tracker.TotalDeaths);
            Assert.AreEqual(0, tracker.SessionBirths);
            Assert.AreEqual(0.0, tracker.SessionTime);
        }

        [TestMethod]
        public void StatisticsTracker_RecordBirth_IncrementsBirthCounters()
        {
            // Arrange
            var tracker = new StatisticsTracker();
            var herbivore = new Herbivore(100, 100);

            // Act
            tracker.RecordBirth(herbivore);

            // Assert
            Assert.AreEqual(1, tracker.TotalBirths);
            Assert.AreEqual(1, tracker.SessionBirths);
        }

        [TestMethod]
        public void StatisticsTracker_RecordBirth_CountsPlants()
        {
            // Arrange
            var tracker = new StatisticsTracker();
            var plant = new Plant(100, 100);

            // Act
            tracker.RecordBirth(plant);

            // Assert
            Assert.AreEqual(1, tracker.TotalPlantsGrown);
        }

        [TestMethod]
        public void StatisticsTracker_RecordDeath_IncrementsDeathCounters()
        {
            // Arrange
            var tracker = new StatisticsTracker();
            var herbivore = new Herbivore(100, 100);

            // Act
            tracker.RecordDeath(herbivore, DeathCause.Starvation);

            // Assert
            Assert.AreEqual(1, tracker.TotalDeaths);
            Assert.AreEqual(1, tracker.SessionDeaths);
        }

        [TestMethod]
        public void StatisticsTracker_RecordDeath_TracksPredation()
        {
            // Arrange
            var tracker = new StatisticsTracker();
            var herbivore = new Herbivore(100, 100);

            // Act
            tracker.RecordDeath(herbivore, DeathCause.Predation);

            // Assert
            Assert.AreEqual(1, tracker.TotalHerbivoresEaten);
        }

        [TestMethod]
        public void StatisticsTracker_RecordFeeding_TracksFoodConsumed()
        {
            // Arrange
            var tracker = new StatisticsTracker();
            var herbivore = new Herbivore(100, 100);
            var plant = new Plant(100, 100);

            // Act
            tracker.RecordFeeding(herbivore, plant, 30.0);

            // Assert
            Assert.AreEqual(30.0, tracker.TotalFoodConsumed);
            Assert.AreEqual(1, tracker.TotalPlantsEaten);
        }

        [TestMethod]
        public void StatisticsTracker_UpdateSnapshot_TracksPeakValues()
        {
            // Arrange
            var tracker = new StatisticsTracker();

            // Act
            tracker.UpdateSnapshot(10, 5, 2);
            tracker.UpdateSnapshot(8, 4, 1);

            // Assert
            Assert.AreEqual(17, tracker.PeakPopulation);
            Assert.AreEqual(10, tracker.PeakPlants);
            Assert.AreEqual(5, tracker.PeakHerbivores);
            Assert.AreEqual(2, tracker.PeakCarnivores);
        }

        [TestMethod]
        public void StatisticsTracker_UpdateTime_AccumulatesSessionTime()
        {
            // Arrange
            var tracker = new StatisticsTracker();

            // Act
            tracker.UpdateTime(1.5);
            tracker.UpdateTime(2.5);

            // Assert
            Assert.AreEqual(4.0, tracker.SessionTime);
        }

        [TestMethod]
        public void StatisticsTracker_CurrentPopulation_ReturnsSum()
        {
            // Arrange
            var tracker = new StatisticsTracker();
            tracker.UpdateSnapshot(5, 3, 1);

            // Act & Assert
            Assert.AreEqual(9, tracker.CurrentPopulation);
        }

        [TestMethod]
        public void StatisticsTracker_Reset_ClearsAllValues()
        {
            // Arrange
            var tracker = new StatisticsTracker();
            tracker.RecordBirth(new Herbivore(100, 100));
            tracker.RecordDeath(new Herbivore(100, 100), DeathCause.Natural);
            tracker.UpdateTime(10.0);

            // Act
            tracker.Reset();

            // Assert
            Assert.AreEqual(0, tracker.TotalBirths);
            Assert.AreEqual(0, tracker.TotalDeaths);
            Assert.AreEqual(0.0, tracker.SessionTime);
        }

        [TestMethod]
        public void StatisticsTracker_ResetSession_OnlyClearsSessionValues()
        {
            // Arrange
            var tracker = new StatisticsTracker();
            tracker.RecordBirth(new Herbivore(100, 100));
            tracker.UpdateTime(10.0);

            // Act
            tracker.ResetSession();

            // Assert
            Assert.AreEqual(1, tracker.TotalBirths); // Total preserved
            Assert.AreEqual(0, tracker.SessionBirths); // Session cleared
            Assert.AreEqual(0.0, tracker.SessionTime); // Session cleared
        }

        [TestMethod]
        public void StatisticsTracker_GetSummary_ReturnsNonEmptyString()
        {
            // Arrange
            var tracker = new StatisticsTracker();
            tracker.UpdateSnapshot(5, 3, 1);

            // Act
            string summary = tracker.GetSummary();

            // Assert
            Assert.IsFalse(string.IsNullOrEmpty(summary));
            Assert.Contains("Population", summary);
        }
    }
}
