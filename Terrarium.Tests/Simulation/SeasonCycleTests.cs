using Microsoft.VisualStudio.TestTools.UnitTesting;
using Terrarium.Logic.Simulation;

namespace Terrarium.Tests.Simulation
{
    [TestClass]
    public class SeasonCycleTests
    {
        [TestMethod]
        public void SeasonCycle_StartsAtSpring()
        {
            var cycle = new SeasonCycle();

            Assert.AreEqual(Season.Spring, cycle.CurrentSeason);
            Assert.IsGreaterThanOrEqualTo(0.0, cycle.SeasonProgress);
            Assert.IsLessThanOrEqualTo(1.0, cycle.SeasonProgress);
        }

        [TestMethod]
        public void SeasonCycle_Update_TransitionsAcrossSeasons()
        {
            var cycle = new SeasonCycle();

            cycle.SetSeason(Season.Spring);
            cycle.Update(130.0);

            Assert.AreEqual(Season.Summer, cycle.CurrentSeason);
        }

        [TestMethod]
        public void SeasonCycle_PlantSpawnChanceMultiplier_IsLowestInWinter()
        {
            var cycle = new SeasonCycle();

            cycle.SetSeason(Season.Winter);
            double winter = cycle.PlantSpawnChanceMultiplier;

            cycle.SetSeason(Season.Spring);
            double spring = cycle.PlantSpawnChanceMultiplier;

            Assert.IsLessThan(spring, winter);
        }
    }
}
