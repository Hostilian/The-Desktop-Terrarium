namespace Terrarium.Tests.Simulation
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Terrarium.Logic.Simulation.Achievements;

    [TestClass]
    /// <summary>
    /// Tests for AchievementInfo record to achieve 100% coverage.
    /// </summary>
    public class AchievementInfoTests
    {
        [TestMethod]
        public void Constructor_InitializesAllProperties()
        {
            // Arrange & Act
            var achievement = new AchievementInfo("test_id", "Test Title", "Test Description");

            // Assert
            Assert.AreEqual("test_id", achievement.Id);
            Assert.AreEqual("Test Title", achievement.Title);
            Assert.AreEqual("Test Description", achievement.Description);
        }

        [TestMethod]
        public void Equality_SameValues_ReturnsTrue()
        {
            // Arrange
            var achievement1 = new AchievementInfo("test", "Title", "Desc");
            var achievement2 = new AchievementInfo("test", "Title", "Desc");

            // Act & Assert
            Assert.AreEqual(achievement1, achievement2);
        }

        [TestMethod]
        public void Equality_DifferentValues_ReturnsFalse()
        {
            // Arrange
            var achievement1 = new AchievementInfo("test1", "Title", "Desc");
            var achievement2 = new AchievementInfo("test2", "Title", "Desc");

            // Act & Assert
            Assert.AreNotEqual(achievement1, achievement2);
        }
    }
}
