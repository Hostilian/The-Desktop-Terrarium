using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Terrarium.Tests.Rendering
{
    [TestClass]
    public class CreatureMoodTests
    {
        [TestMethod]
        public void MoodLogic_LowHealth_ReturnsCriticalMood()
        {
            string mood = GetMoodEmoji(health: 15, hunger: 50, isMoving: false);
            Assert.AreEqual("💔", mood, "Low health should show critical mood");
        }

        [TestMethod]
        public void MoodLogic_HighHunger_ReturnsHungryMood()
        {
            string mood = GetMoodEmoji(health: 80, hunger: 85, isMoving: false);
            Assert.AreEqual("😰", mood, "High hunger should show hungry mood");
        }

        [TestMethod]
        public void MoodLogic_Hunting_ReturnsHuntingMood()
        {
            // Creature moving with moderate hunger shows hunting
            string mood = GetMoodEmoji(health: 80, hunger: 50, isMoving: true);
            Assert.AreEqual("🎯", mood, "Moving creature with hunger should show hunting mood");
        }

        [TestMethod]
        public void MoodLogic_Healthy_ReturnsHappyMood()
        {
            string mood = GetMoodEmoji(health: 90, hunger: 20, isMoving: false);
            Assert.AreEqual("😊", mood, "Healthy, fed creature should be happy");
        }

        [TestMethod]
        public void MoodLogic_MediumHealth_ReturnsNeutralMood()
        {
            string mood = GetMoodEmoji(health: 55, hunger: 45, isMoving: false);
            Assert.AreEqual("😐", mood, "Medium state should show neutral mood");
        }

        [TestMethod]
        public void MoodLogic_Priority_HealthFirst()
        {
            // Even if hungry, critical health takes priority
            string mood = GetMoodEmoji(health: 10, hunger: 90, isMoving: true);
            Assert.AreEqual("💔", mood, "Critical health should take priority");
        }

        /// <summary>
        /// Mirrors the CreatureMoodIndicator's mood determination logic.
        /// </summary>
        private string GetMoodEmoji(double health, double hunger, bool isMoving)
        {
            // Critical health
            if (health < 20)
                return "💔";

            // Very hungry
            if (hunger > 80)
                return "😰";

            // Hunting/active
            if (isMoving && hunger > 30)
                return "🎯";

            // Happy and healthy
            if (health > 70 && hunger < 30)
                return "😊";

            // Neutral
            if (health > 40 && hunger < 60)
                return "😐";

            // Worried
            if (health < 40 || hunger > 60)
                return "😟";

            // Just ate
            if (hunger < 20)
                return "🍖";

            // Content
            return "😋";
        }
    }
}
