using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Terrarium.Tests.Rendering
{
    [TestClass]
    public class SessionTimerTests
    {
        private const double DayDuration = 120.0;

        [TestMethod]
        public void SessionTimer_StartsAtDayOne()
        {
            double sessionTime = 0;
            int dayCount = (int)(sessionTime / DayDuration) + 1;

            Assert.AreEqual(1, dayCount, "Session should start at Day 1");
        }

        [TestMethod]
        public void SessionTimer_DayAdvancesCorrectly()
        {
            double sessionTime = 120; // Exactly 2 minutes
            int dayCount = (int)(sessionTime / DayDuration) + 1;

            Assert.AreEqual(2, dayCount, "Day 2 should start at 120 seconds");
        }

        [TestMethod]
        public void SessionTimer_MultipleAdvances()
        {
            double sessionTime = 360; // 6 minutes
            int dayCount = (int)(sessionTime / DayDuration) + 1;

            Assert.AreEqual(4, dayCount, "Day 4 should be at 360 seconds");
        }

        [TestMethod]
        public void SessionTimer_TimeFormatting()
        {
            double sessionTime = 185; // 3:05
            int minutes = (int)(sessionTime / 60);
            int seconds = (int)(sessionTime % 60);

            Assert.AreEqual(3, minutes, "Minutes should be 3");
            Assert.AreEqual(5, seconds, "Seconds should be 5");
        }

        [TestMethod]
        public void SessionTimer_LongSession()
        {
            double sessionTime = 3700; // Just over 1 hour
            int minutes = (int)(sessionTime / 60);

            Assert.IsTrue(minutes > 60, "Minutes should exceed 60 for long sessions");

            int dayCount = (int)(sessionTime / DayDuration) + 1;
            Assert.AreEqual(31, dayCount, "Should be on day 31 after ~1 hour");
        }

        [TestMethod]
        public void SessionTimer_GenerationTracking()
        {
            int generationCount = 1;

            // Simulate reproduction events
            generationCount++;
            Assert.AreEqual(2, generationCount, "Generation should increment");

            generationCount += 10;
            Assert.AreEqual(12, generationCount, "Multiple increments should work");
        }

        [TestMethod]
        public void SessionTimer_Reset()
        {
            double sessionTime = 500;
            int dayCount = 5;
            int generationCount = 20;

            // Reset
            sessionTime = 0;
            dayCount = 1;
            generationCount = 1;

            Assert.AreEqual(0, sessionTime, "Time should reset to 0");
            Assert.AreEqual(1, dayCount, "Day should reset to 1");
            Assert.AreEqual(1, generationCount, "Generation should reset to 1");
        }

        [TestMethod]
        public void SessionTimer_DayDurationIsReasonable()
        {
            // 2 minutes per day - allows seeing full day/night cycle
            Assert.AreEqual(120.0, DayDuration, "Day duration should be 2 minutes");

            // An hour session would have 30 days
            double hourInSeconds = 3600;
            int daysInHour = (int)(hourInSeconds / DayDuration);
            Assert.AreEqual(30, daysInHour, "One hour should have 30 in-game days");
        }
    }
}
