using Xunit;
using Terrarium.Desktop;

namespace Terrarium.Tests.Desktop
{
    /// <summary>
    /// Tests for TerrariumTypeOption to achieve 100% coverage.
    /// </summary>
    public class TerrariumTypeOptionTests
    {
        [Fact]
        public void Properties_CanBeSetAndRetrieved()
        {
            // Arrange
            var option = new TerrariumTypeOption
            {
                DisplayName = "Test Display",
                Description = "Test Description",
                Type = global::Terrarium.Logic.Simulation.TerrariumType.Forest
            };

            // Assert
            Assert.Equal("Test Display", option.DisplayName);
            Assert.Equal("Test Description", option.Description);
            Assert.Equal(global::Terrarium.Logic.Simulation.TerrariumType.Forest, option.Type);
        }
    }
}
