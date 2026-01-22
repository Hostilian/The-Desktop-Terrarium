namespace Terrarium.Tests.Constants
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Terrarium.Desktop.Constants;

    [TestClass]
    /// <summary>
    /// Tests for Win32Constants to achieve 100% coverage.
    /// </summary>
    public class Win32ConstantsTests
    {
        [TestMethod]
        public void WM_NC_HITTEST_HasCorrectValue()
        {
            // Assert
            Assert.AreEqual(0x0084, Win32Constants.WMNCHITTEST);
        }

        [TestMethod]
        public void HT_TRANSPARENT_HasCorrectValue()
        {
            // Assert
            Assert.AreEqual(-1, Win32Constants.HTTRANSPARENT);
        }
    }
}
