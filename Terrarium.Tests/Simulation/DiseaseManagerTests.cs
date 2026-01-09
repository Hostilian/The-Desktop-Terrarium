using Microsoft.VisualStudio.TestTools.UnitTesting;
using Terrarium.Logic.Simulation;

namespace Terrarium.Tests.Simulation
{
    [TestClass]
    public class DiseaseManagerTests
    {
        [TestMethod]
        public void DiseaseManager_SeedsInfection_WhenProbabilityHighEnough()
        {
            var world = new World(500, 200);
            world.SpawnRandomHerbivore();
            world.SpawnRandomHerbivore();
            world.SpawnRandomCarnivore();

            var manager = new DiseaseManager(new System.Random(1234));

            // Make this deterministic: guarantee seeding happens on update.
            manager.SeedInfectionChancePerSecond = 1.0;

            manager.Update(world, 1.0);

            Assert.IsGreaterThan(0, manager.InfectedCount);
        }

        [TestMethod]
        public void DiseaseManager_DoesNotCrash_OnEmptyWorld()
        {
            var world = new World(500, 200);
            var manager = new DiseaseManager(new System.Random(0));

            manager.Update(world, 1.0);

            Assert.AreEqual(0, manager.InfectedCount);
        }
    }
}
