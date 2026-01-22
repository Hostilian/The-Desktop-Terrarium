namespace Terrarium.Tests.Desktop
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Terrarium.Desktop;

    [TestClass]
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
                Title = "Test Title",
                Subtitle = "Test Subtitle",
                Id = "test_id"
            };

            // Assert
            Assert.AreEqual("Test Title", option.Title);
            Assert.AreEqual("Test Subtitle", option.Subtitle);
            Assert.AreEqual("test_id", option.Id);
        }
    }
}
