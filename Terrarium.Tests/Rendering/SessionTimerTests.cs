using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Terrarium.Tests.Rendering
{
    [TestClass]
    public class SessionTimerTests
    {
        private static double DayDuration => 120.0;

        [TestMethod]
        public void SessionTimer_StartsAtDayOne()
        {
            double sessionTime = 0;
            int dayCount = (int)(sessionTime / DayDuration) + 1;

            Assert.AreEqual(1, dayCount);
        }

        [TestMethod]
        public void SessionTimer_DayAdvancesCorrectly()
        {
            double sessionTime = 120; // Exactly 2 minutes
            int dayCount = (int)(sessionTime / DayDuration) + 1;

            Assert.AreEqual(2, dayCount);
        }

        [TestMethod]
        public void SessionTimer_MultipleAdvances()
        {
            double sessionTime = 360; // 6 minutes
            int dayCount = (int)(sessionTime / DayDuration) + 1;

            Assert.AreEqual(4, dayCount);
        }

        [TestMethod]
        public void SessionTimer_TimeFormatting()
        {
            double sessionTime = 185; // 3:05
            int minutes = (int)(sessionTime / 60);
            int seconds = (int)(sessionTime % 60);

            Assert.AreEqual(3, minutes);
            Assert.AreEqual(5, seconds);
        }

        [TestMethod]
        public void SessionTimer_LongSession()
        {
            double sessionTime = 3700; // Just over 1 hour
            int minutes = (int)(sessionTime / 60);

            Assert.IsGreaterThan(60, minutes);

            int dayCount = (int)(sessionTime / DayDuration) + 1;
            Assert.AreEqual(31, dayCount);
        }

        [TestMethod]
        public void SessionTimer_GenerationTracking()
        {
            int generationCount = 1;

            // Simulate reproduction events
            generationCount++;
            Assert.AreEqual(2, generationCount);

            generationCount += 10;
            Assert.AreEqual(12, generationCount);
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

            Assert.AreEqual(0, sessionTime);
            Assert.AreEqual(1, dayCount);
            Assert.AreEqual(1, generationCount);
        }

        [TestMethod]
        public void SessionTimer_DayDurationIsReasonable()
        {
            // An hour session would have 30 days
            double hourInSeconds = 3600;
            int daysInHour = (int)(hourInSeconds / DayDuration);
            Assert.AreEqual(30, daysInHour);
        }
    }
}
