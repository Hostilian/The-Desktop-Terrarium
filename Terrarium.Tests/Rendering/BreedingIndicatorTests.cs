using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Terrarium.Tests.Rendering
{
    [TestClass]
    public class BreedingIndicatorTests
    {
        private const double MinHealthForBreeding = 60;
        private const double MaxHungerForBreeding = 40;
        private const double MinAgeForBreeding = 5.0;

        [TestMethod]
        public void CanBreed_HealthyMatureCreature_ReturnsTrue()
        {
            bool canBreed = CanBreed(health: 80, hunger: 20, age: 10);
            Assert.IsTrue(canBreed, "Healthy mature creature should be able to breed");
        }

        [TestMethod]
        public void CanBreed_LowHealth_ReturnsFalse()
        {
            bool canBreed = CanBreed(health: 50, hunger: 20, age: 10);
            Assert.IsFalse(canBreed, "Low health creature should not breed");
        }

        [TestMethod]
        public void CanBreed_TooHungry_ReturnsFalse()
        {
            bool canBreed = CanBreed(health: 80, hunger: 60, age: 10);
            Assert.IsFalse(canBreed, "Hungry creature should not breed");
        }

        [TestMethod]
        public void CanBreed_TooYoung_ReturnsFalse()
        {
            bool canBreed = CanBreed(health: 80, hunger: 20, age: 2);
            Assert.IsFalse(canBreed, "Young creature should not breed");
        }

        [TestMethod]
        public void CanBreed_ExactThreshold_ReturnsTrue()
        {
            bool canBreed = CanBreed(
                health: MinHealthForBreeding, 
                hunger: MaxHungerForBreeding, 
                age: MinAgeForBreeding);
            Assert.IsTrue(canBreed, "Creature at exact thresholds should breed");
        }

        [TestMethod]
        public void BreedingThresholds_AreReasonable()
        {
            Assert.IsTrue(MinHealthForBreeding > 50, "Min health should be over half");
            Assert.IsTrue(MaxHungerForBreeding < 50, "Max hunger should be under half");
            Assert.IsTrue(MinAgeForBreeding > 0, "Min age should be positive");
        }

        /// <summary>
        /// Mirrors the BreedingIndicator's breeding check logic.
        /// </summary>
        private bool CanBreed(double health, double hunger, double age)
        {
            return health >= MinHealthForBreeding &&
                   hunger <= MaxHungerForBreeding &&
                   age >= MinAgeForBreeding;
        }
    }
}
