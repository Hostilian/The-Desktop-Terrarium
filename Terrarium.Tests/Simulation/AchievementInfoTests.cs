using Xunit;
using Terrarium.Logic.Simulation.Achievements;

namespace Terrarium.Tests.Simulation
{
    /// <summary>
    /// Tests for AchievementInfo record to achieve 100% coverage.
    /// </summary>
    public class AchievementInfoTests
    {
        [Fact]
        public void Constructor_InitializesAllProperties()
        {
            // Arrange & Act
            var achievement = new AchievementInfo("test_id", "Test Title", "Test Description");

            // Assert
            Assert.Equal("test_id", achievement.Id);
            Assert.Equal("Test Title", achievement.Title);
            Assert.Equal("Test Description", achievement.Description);
        }

        [Fact]
        public void Equality_SameValues_ReturnsTrue()
        {
            // Arrange
            var achievement1 = new AchievementInfo("test", "Title", "Desc");
            var achievement2 = new AchievementInfo("test", "Title", "Desc");

            // Act & Assert
            Assert.Equal(achievement1, achievement2);
        }

        [Fact]
        public void Equality_DifferentValues_ReturnsFalse()
        {
            // Arrange
            var achievement1 = new AchievementInfo("test1", "Title", "Desc");
            var achievement2 = new AchievementInfo("test2", "Title", "Desc");

            // Act & Assert
            Assert.NotEqual(achievement1, achievement2);
        }
    }
}
