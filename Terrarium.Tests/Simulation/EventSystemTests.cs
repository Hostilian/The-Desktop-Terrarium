using Microsoft.VisualStudio.TestTools.UnitTesting;
using Terrarium.Logic.Entities;
using Terrarium.Logic.Simulation;

namespace Terrarium.Tests.Simulation
{
    /// <summary>
    /// Unit tests for EventSystem.
    /// </summary>
    [TestClass]
    public class EventSystemTests
    {
        [TestInitialize]
        public void Setup()
        {
            // Reset event system before each test
            EventSystem.Reset();
        }

        [TestMethod]
        public void EventSystem_Instance_IsSingleton()
        {
            // Arrange & Act
            var instance1 = EventSystem.Instance;
            var instance2 = EventSystem.Instance;

            // Assert
            Assert.AreSame(instance1, instance2);
        }

        [TestMethod]
        public void EventSystem_OnEntityBorn_RaisesEvent()
        {
            // Arrange
            bool eventRaised = false;
            WorldEntity? capturedEntity = null;

            EventSystem.Instance.EntityBorn += (sender, args) =>
            {
                eventRaised = true;
                capturedEntity = args.Entity;
            };

            var herbivore = new Herbivore(100, 100);

            // Act
            EventSystem.Instance.OnEntityBorn(herbivore);

            // Assert
            Assert.IsTrue(eventRaised);
            Assert.AreSame(herbivore, capturedEntity);
        }

        [TestMethod]
        public void EventSystem_OnEntityDied_RaisesEventWithCause()
        {
            // Arrange
            DeathCause capturedCause = DeathCause.Natural;

            EventSystem.Instance.EntityDied += (sender, args) =>
            {
                capturedCause = args.Cause;
            };

            var herbivore = new Herbivore(100, 100);

            // Act
            EventSystem.Instance.OnEntityDied(herbivore, DeathCause.Starvation);

            // Assert
            Assert.AreEqual(DeathCause.Starvation, capturedCause);
        }

        [TestMethod]
        public void EventSystem_OnEntityFed_RaisesEventWithNutritionValue()
        {
            // Arrange
            double capturedNutrition = 0;

            EventSystem.Instance.EntityFed += (sender, args) =>
            {
                capturedNutrition = args.NutritionValue;
            };

            var herbivore = new Herbivore(100, 100);
            var plant = new Plant(100, 100);

            // Act
            EventSystem.Instance.OnEntityFed(herbivore, plant, 30.0);

            // Assert
            Assert.AreEqual(30.0, capturedNutrition);
        }

        [TestMethod]
        public void EventSystem_OnDayPhaseChanged_RaisesEvent()
        {
            // Arrange
            DayPhase capturedNewPhase = DayPhase.Dawn;
            DayPhase capturedOldPhase = DayPhase.Dawn;

            EventSystem.Instance.DayPhaseChanged += (sender, args) =>
            {
                capturedNewPhase = args.NewPhase;
                capturedOldPhase = args.OldPhase;
            };

            // Act
            EventSystem.Instance.RaiseDayPhaseChanged(DayPhase.Night, DayPhase.Dusk);

            // Assert
            Assert.AreEqual(DayPhase.Night, capturedNewPhase);
            Assert.AreEqual(DayPhase.Dusk, capturedOldPhase);
        }

        [TestMethod]
        public void EventSystem_OnWeatherChanged_RaisesEvent()
        {
            // Arrange
            double capturedNewIntensity = 0;
            bool capturedIsStormy = false;

            EventSystem.Instance.WeatherChanged += (sender, args) =>
            {
                capturedNewIntensity = args.NewIntensity;
                capturedIsStormy = args.IsStormy;
            };

            // Act
            EventSystem.Instance.OnWeatherChanged(0.8, 0.2);

            // Assert
            Assert.AreEqual(0.8, capturedNewIntensity);
            Assert.IsTrue(capturedIsStormy);
        }

        [TestMethod]
        public void EventSystem_OnEntityReproduced_RaisesEvent()
        {
            // Arrange
            Creature? capturedOffspring = null;

            EventSystem.Instance.EntityReproduced += (sender, args) =>
            {
                capturedOffspring = args.Offspring;
            };

            var parent1 = new Herbivore(100, 100);
            var parent2 = new Herbivore(150, 100);
            var offspring = new Herbivore(125, 100);

            // Act
            EventSystem.Instance.OnEntityReproduced(parent1, parent2, offspring);

            // Assert
            Assert.AreSame(offspring, capturedOffspring);
        }

        [TestMethod]
        public void EventSystem_Reset_CreatesNewInstance()
        {
            // Arrange
            var oldInstance = EventSystem.Instance;

            // Act
            EventSystem.Reset();
            var newInstance = EventSystem.Instance;

            // Assert
            Assert.AreNotSame(oldInstance, newInstance);
        }

        [TestMethod]
        public void WeatherEventArgs_IsStormy_ReturnsTrueAboveThreshold()
        {
            // Arrange
            var args = new WeatherEventArgs(0.6, 0.1);

            // Assert
            Assert.IsTrue(args.IsStormy);
        }

        [TestMethod]
        public void WeatherEventArgs_IsStormy_ReturnsFalseBelowThreshold()
        {
            // Arrange
            var args = new WeatherEventArgs(0.3, 0.1);

            // Assert
            Assert.IsFalse(args.IsStormy);
        }
    }
}
