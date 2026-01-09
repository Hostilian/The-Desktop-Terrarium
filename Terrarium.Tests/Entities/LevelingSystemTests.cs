using Microsoft.VisualStudio.TestTools.UnitTesting;
using Terrarium.Logic.Entities;

namespace Terrarium.Tests.Entities
{
    /// <summary>
    /// Unit tests for the leveling and experience system.
    /// </summary>
    [TestClass]
    public class LevelingSystemTests
    {
        [TestMethod]
        public void Entity_StartsAtLevelOne()
        {
            // Arrange & Act
            var plant = new Plant(100, 100);

            // Assert
            Assert.AreEqual(1, plant.Level, "Entity should start at level 1");
        }

        [TestMethod]
        public void Entity_StartsWithZeroExperience()
        {
            // Arrange & Act
            var herbivore = new Herbivore(100, 100);

            // Assert
            Assert.AreEqual(0, herbivore.Experience, "Entity should start with 0 experience");
        }

        [TestMethod]
        public void Plant_GainsExperienceFromGrowing()
        {
            // Arrange
            var plant = new Plant(100, 100);
            double initialExperience = plant.Experience;

            // Act
            plant.Grow(deltaTime: 5.0);

            // Assert
            Assert.IsTrue(plant.Experience > initialExperience, "Plant should gain experience from growing");
        }

        [TestMethod]
        public void Plant_GainsExperienceFromWatering()
        {
            // Arrange
            var plant = new Plant(100, 100);
            double initialExperience = plant.Experience;

            // Act
            plant.Water(30.0);

            // Assert
            Assert.IsTrue(plant.Experience > initialExperience, "Plant should gain experience from being watered");
        }

        [TestMethod]
        public void Creature_GainsExperienceFromEating()
        {
            // Arrange
            var herbivore = new Herbivore(100, 100);
            double initialExperience = herbivore.Experience;

            // Act
            herbivore.Feed(30.0);

            // Assert
            Assert.IsTrue(herbivore.Experience > initialExperience, "Creature should gain experience from eating");
        }

        [TestMethod]
        public void Entity_LevelsUpAfterGainingEnoughExperience()
        {
            // Arrange
            var plant = new Plant(100, 100);
            int initialLevel = plant.Level;

            // Act - gain 100 XP to level up (ExperiencePerLevel = 100)
            plant.GainExperience(100.0);

            // Assert
            Assert.AreEqual(initialLevel + 1, plant.Level, "Entity should level up after gaining enough experience");
        }

        [TestMethod]
        public void Entity_ExperienceOverflowsIntoNextLevel()
        {
            // Arrange
            var herbivore = new Herbivore(100, 100);

            // Act - gain 150 XP (should level up and have 50 XP remaining)
            herbivore.GainExperience(150.0);

            // Assert
            Assert.AreEqual(2, herbivore.Level, "Entity should be at level 2");
            Assert.AreEqual(50.0, herbivore.Experience, "Overflow experience should carry to next level");
        }

        [TestMethod]
        public void Entity_CanLevelUpMultipleTimes()
        {
            // Arrange
            var carnivore = new Carnivore(100, 100);

            // Act - gain 350 XP (should reach level 4 with 50 XP remaining)
            carnivore.GainExperience(350.0);

            // Assert
            Assert.AreEqual(4, carnivore.Level, "Entity should reach level 4");
            Assert.AreEqual(50.0, carnivore.Experience, "Overflow experience should be 50");
        }

        [TestMethod]
        public void Plant_IncreasesGrowthRateWithLevel()
        {
            // Arrange
            var plant1 = new Plant(100, 100, initialSize: 10);
            var plant2 = new Plant(200, 200, initialSize: 10);
            plant2.GainExperience(100.0); // Level up to level 2

            double initialSize1 = plant1.Size;
            double initialSize2 = plant2.Size;

            // Act - grow for same amount of time
            plant1.Grow(deltaTime: 1.0);
            plant2.Grow(deltaTime: 1.0);

            // Assert
            double growth1 = plant1.Size - initialSize1;
            double growth2 = plant2.Size - initialSize2;
            Assert.IsTrue(growth2 > growth1, "Higher level plant should grow faster");
        }

        [TestMethod]
        public void Plant_IncreasesMaxSizeWithLevel()
        {
            // Arrange
            var plant = new Plant(100, 100, initialSize: 48);
            
            // Act - level up to increase max size
            plant.GainExperience(100.0);
            
            // Grow beyond original max size (50)
            for (int i = 0; i < 20; i++)
            {
                plant.Grow(deltaTime: 1.0);
            }

            // Assert - should be able to grow past 50 (original max)
            Assert.IsTrue(plant.Size > 50.0, "Higher level plant should have higher max size");
        }

        [TestMethod]
        public void Creature_IncreasesSpeedWithLevel()
        {
            // Arrange
            var herbivore = new Herbivore(100, 100);
            double initialSpeed = herbivore.GetEffectiveSpeed();

            // Act
            herbivore.GainExperience(100.0); // Level up

            // Assert
            double newSpeed = herbivore.GetEffectiveSpeed();
            Assert.IsTrue(newSpeed > initialSpeed, "Higher level creature should be faster");
        }

        [TestMethod]
        public void Herbivore_IncreasesDetectionRangeWithLevel()
        {
            // Arrange - place plant at edge of level 1 detection range
            var plant = new Plant(305, 100); // Just beyond default 200 range (distance ~205)
            var herbivore1 = new Herbivore(100, 100);
            var herbivore2 = new Herbivore(100, 100);
            herbivore2.GainExperience(100.0); // Level up to level 2, gets +10 detection range

            var plants = new[] { plant };

            // Act
            var found1 = herbivore1.FindNearestPlant(plants);
            var found2 = herbivore2.FindNearestPlant(plants);

            // Assert - level 1 shouldn't find it, but level 2 with bonus should
            Assert.IsNull(found1, "Level 1 herbivore should not detect plant at this distance");
            Assert.IsNotNull(found2, "Higher level herbivore should have better detection");
        }

        [TestMethod]
        public void Carnivore_IncreasesAttackDamageWithLevel()
        {
            // Arrange
            var prey = new Herbivore(100, 100);
            var carnivore = new Carnivore(100, 100);
            double preyInitialHealth = prey.Health;

            // Level up the carnivore
            carnivore.GainExperience(100.0);

            // Act
            carnivore.TryEat(prey);

            // Assert - prey should take more damage from a higher level carnivore
            double damageTaken = preyInitialHealth - prey.Health;
            Assert.IsTrue(damageTaken > 80.0, "Higher level carnivore should deal more damage");
        }

        [TestMethod]
        public void Carnivore_IncreasesDetectionRangeWithLevel()
        {
            // Arrange - place prey at edge of level 1 detection range
            var prey = new Herbivore(405, 100); // Just beyond default 300 range (distance ~305)
            var carnivore1 = new Carnivore(100, 100);
            var carnivore2 = new Carnivore(100, 100);
            carnivore2.GainExperience(100.0); // Level up to level 2, gets +15 detection range

            var herbivores = new[] { prey };

            // Act
            var found1 = carnivore1.FindNearestPrey(herbivores);
            var found2 = carnivore2.FindNearestPrey(herbivores);

            // Assert - level 1 shouldn't find it, but level 2 with bonus should
            Assert.IsNull(found1, "Level 1 carnivore should not detect prey at this distance");
            Assert.IsNotNull(found2, "Higher level carnivore should have better detection");
        }

        [TestMethod]
        public void Entity_LevelUpRestoresHealth()
        {
            // Arrange
            var plant = new Plant(100, 100);
            plant.TakeDamage(30.0); // Reduce health
            double healthBeforeLevelUp = plant.Health;

            // Act
            plant.GainExperience(100.0); // Level up

            // Assert
            Assert.IsTrue(plant.Health > healthBeforeLevelUp, "Leveling up should restore some health");
        }

        [TestMethod]
        public void DeadEntity_DoesNotGainExperience()
        {
            // Arrange
            var plant = new Plant(100, 100);
            plant.TakeDamage(100.0); // Kill the plant

            // Act
            plant.GainExperience(50.0);

            // Assert
            Assert.AreEqual(0, plant.Experience, "Dead entity should not gain experience");
            Assert.AreEqual(1, plant.Level, "Dead entity should not level up");
        }
    }
}
