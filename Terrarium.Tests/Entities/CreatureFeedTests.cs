using Microsoft.VisualStudio.TestTools.UnitTesting;
using Terrarium.Logic.Entities;

namespace Terrarium.Tests.Entities
{
    [TestClass]
    public class CreatureFeedTests
    {
        [TestMethod]
        public void Creature_Feed_NegativeNutrition_IncreasesHungerButDoesNotHealOrHurt()
        {
            var creature = new Herbivore(0, 0, "Sheep");
            double startHealth = creature.Health;
            double startHunger = creature.Hunger;

            creature.Feed(-10.0);

            Assert.AreEqual(startHealth, creature.Health);
            Assert.IsGreaterThan(creature.Hunger, startHunger);
        }
    }
}
