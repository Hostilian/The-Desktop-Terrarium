using Microsoft.VisualStudio.TestTools.UnitTesting;
using Terrarium.Logic.Simulation;

namespace Terrarium.Tests.Simulation
{
    /// <summary>
    /// Unit tests for SimulationEngine.
    /// Tests the main simulation orchestration.
    /// </summary>
    [TestClass]
    public class SimulationEngineTests
    {
        [TestMethod]
        public void SimulationEngine_Initialize_CreatesEntities()
        {
            // Arrange
            var engine = new SimulationEngine(800, 600);

            // Act
            engine.Initialize();

            // Assert
            Assert.IsNotEmpty(engine.World.Plants, "Should create initial plants");
            Assert.IsNotEmpty(engine.World.Herbivores, "Should create initial herbivores");
            Assert.IsNotEmpty(engine.World.Carnivores, "Should create initial carnivores");
        }

        [TestMethod]
        public void SimulationEngine_Update_UpdatesEntities()
        {
            // Arrange
            var engine = new SimulationEngine(800, 600);
            engine.Initialize();
            var firstPlant = engine.World.Plants.First();
            double initialAge = firstPlant.Age;

            // Act
            engine.Update(deltaTime: 1.0);

            // Assert
            Assert.IsGreaterThan(initialAge, firstPlant.Age, "Entities should age after update");
        }

        [TestMethod]
        public void SimulationEngine_Update_UsesFixedLogicTickAccumulator()
        {
            // Arrange
            var engine = new SimulationEngine(800, 600);
            engine.Initialize();
            var firstPlant = engine.World.Plants.First();
            double initialAge = firstPlant.Age;

            // Act - below the 0.2s logic tick threshold
            engine.Update(deltaTime: 0.19);

            // Assert - no logic tick should have occurred yet
            Assert.AreEqual(initialAge, firstPlant.Age, 1e-9, "Entities should not update until the logic tick threshold is reached");

            // Act - cross the threshold (0.19 + 0.01 = 0.20)
            engine.Update(deltaTime: 0.01);

            // Assert - exactly one logic tick should have occurred
            Assert.IsGreaterThan(initialAge, firstPlant.Age, "Entities should update once the logic tick threshold is reached");
        }

        [TestMethod]
        public void SimulationEngine_FindClickableAt_FindsEntity()
        {
            // Arrange
            var engine = new SimulationEngine(800, 600);
            var plant = new Logic.Entities.Plant(100, 100);
            engine.World.AddPlant(plant);

            // Act
            var clickable = engine.FindClickableAt(100, 100);

            // Assert
            Assert.IsNotNull(clickable, "Should find clickable entity at position");
            Assert.AreSame(plant, clickable, "Should return the exact entity instance that contains the point");
        }

        [TestMethod]
        public void SimulationEngine_Weather_AffectsPlants()
        {
            // Arrange
            var engine = new SimulationEngine(800, 600);
            var plant = new Logic.Entities.Plant(100, 100);
            engine.World.AddPlant(plant);
            engine.WeatherIntensity = 1.0; // Maximum storm
            double healthBefore = plant.Health;

            // Act
            for (int i = 0; i < 10; i++)
            {
                engine.Update(0.2);
            }

            // Assert
            Assert.IsLessThan(healthBefore, plant.Health, "Stormy weather should damage plants");
            Assert.AreEqual(100.0, plant.WaterLevel, 1e-9, "Stormy weather should keep plants watered (clamped at max)");
        }

        [TestMethod]
        public void SimulationEngine_Pause_StopsEntityUpdates()
        {
            // Arrange
            var engine = new SimulationEngine(800, 600);
            engine.Initialize();
            var firstPlant = engine.World.Plants.First();
            double initialAge = firstPlant.Age;

            // Act
            engine.Pause();
            engine.Update(deltaTime: 1.0);

            // Assert
            Assert.AreEqual(initialAge, firstPlant.Age, 1e-9, "Entities should not update while paused");
        }

        [TestMethod]
        public void SimulationEngine_SetSimulationSpeed_ClampsToValidRange()
        {
            // Arrange
            var engine = new SimulationEngine(800, 600);

            // Act
            engine.SetSimulationSpeed(1000.0);
            double high = engine.SimulationSpeed;
            engine.SetSimulationSpeed(0.0001);
            double low = engine.SimulationSpeed;

            // Assert
            Assert.IsLessThanOrEqualTo(high, 4.0, "Speed should clamp to max");
            Assert.IsGreaterThanOrEqualTo(low, 0.25, "Speed should clamp to min");
        }
    }
}
