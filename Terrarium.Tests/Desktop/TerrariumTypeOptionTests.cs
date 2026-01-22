
using Terrarium.Desktop;
namespace Terrarium.Tests.Desktop
{
    /// <summary>
    /// Tests for TerrariumTypeOption to achieve 100% coverage.
    /// </summary>
    public class TerrariumTypeOptionTests
    {
        [TestMethod]
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
            Assert.AreEqual("Test Display", option.DisplayName);
            Assert.AreEqual("Test Description", option.Description);
            Assert.AreEqual(global::Terrarium.Logic.Simulation.TerrariumType.Forest, option.Type);
        }
    }
}





