using Microsoft.VisualStudio.TestTools.UnitTesting;
using Terrarium.Logic.Simulation;

namespace Terrarium.Tests.Simulation
{
    /// <summary>
    /// Unit tests for DayNightCycle.
    /// </summary>
    [TestClass]
    public class DayNightCycleTests
    {
        [TestMethod]
        public void DayNightCycle_StartsAtDawn()
        {
            // Arrange & Act
            var cycle = new DayNightCycle();

            // Assert
            Assert.AreEqual(DayPhase.Dawn, cycle.CurrentPhase);
            Assert.IsTrue(cycle.IsDay);
            Assert.IsFalse(cycle.IsNight);
        }

        [TestMethod]
        public void DayNightCycle_Update_AdvancesTime()
        {
            // Arrange
            var cycle = new DayNightCycle();
            double initialTime = cycle.CurrentTime;

            // Act
            cycle.Update(10.0);

            // Assert
            Assert.IsGreaterThan(initialTime, cycle.CurrentTime);
        }

        [TestMethod]
        public void DayNightCycle_TransitionsToDayAfterDawn()
        {
            // Arrange
            var cycle = new DayNightCycle();

            // Act - Advance past dawn duration (5 seconds)
            cycle.Update(6.0);

            // Assert
            Assert.AreEqual(DayPhase.Day, cycle.CurrentPhase);
            Assert.IsTrue(cycle.IsDay);
        }

        [TestMethod]
        public void DayNightCycle_TransitionsToNight()
        {
            // Arrange
            var cycle = new DayNightCycle();

            // Act - Advance to night (past 60 seconds day duration)
            cycle.Update(65.0);

            // Assert
            Assert.AreEqual(DayPhase.Night, cycle.CurrentPhase);
            Assert.IsTrue(cycle.IsNight);
        }

        [TestMethod]
        public void DayNightCycle_WrapsAround()
        {
            // Arrange
            var cycle = new DayNightCycle();

            // Act - Advance past full cycle (90 seconds total)
            // 90 seconds = full cycle, 92 seconds puts us at 2 seconds into new Dawn
            cycle.Update(92.0);

            // Assert - Should be back to dawn (CurrentTime = 2.0 which is < 5.0)
            Assert.AreEqual(DayPhase.Dawn, cycle.CurrentPhase);
        }

        [TestMethod]
        public void DayNightCycle_SpeedMultiplier_LowerAtNight()
        {
            // Arrange
            var dayCycle = new DayNightCycle();
            var nightCycle = new DayNightCycle();

            // Act
            dayCycle.SetPhase(DayPhase.Day);
            nightCycle.SetPhase(DayPhase.Night);

            // Assert
            Assert.AreEqual(1.0, dayCycle.SpeedMultiplier);
            Assert.IsLessThan(1.0, nightCycle.SpeedMultiplier);
        }

        [TestMethod]
        public void DayNightCycle_LightLevel_HighestAtDay()
        {
            // Arrange
            var cycle = new DayNightCycle();

            // Act
            cycle.SetPhase(DayPhase.Day);

            // Assert
            Assert.AreEqual(1.0, cycle.LightLevel);
        }

        [TestMethod]
        public void DayNightCycle_LightLevel_LowestAtNight()
        {
            // Arrange
            var cycle = new DayNightCycle();

            // Act
            cycle.SetPhase(DayPhase.Night);

            // Assert
            Assert.IsLessThan(0.5, cycle.LightLevel);
        }

        [TestMethod]
        public void DayNightCycle_SetTime_WorksCorrectly()
        {
            // Arrange
            var cycle = new DayNightCycle();

            // Act
            cycle.SetTime(30.0);

            // Assert
            Assert.AreEqual(30.0, cycle.CurrentTime);
        }

        [TestMethod]
        public void DayNightCycle_HungerRateMultiplier_LowerAtNight()
        {
            // Arrange
            var dayCycle = new DayNightCycle();
            var nightCycle = new DayNightCycle();

            // Act
            dayCycle.SetPhase(DayPhase.Day);
            nightCycle.SetPhase(DayPhase.Night);

            // Assert
            Assert.AreEqual(1.0, dayCycle.HungerRateMultiplier);
            Assert.IsLessThan(1.0, nightCycle.HungerRateMultiplier);
        }
    }
}
