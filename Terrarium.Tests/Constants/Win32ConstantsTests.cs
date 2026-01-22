using Xunit;
using Terrarium.Desktop.Constants;

namespace Terrarium.Tests.Constants
{
    /// <summary>
    /// Tests for Win32Constants to achieve 100% coverage.
    /// </summary>
    public class Win32ConstantsTests
    {
        [TestMethod]
        public void WM_NC_HITTEST_HasCorrectValue()
        {
            // Assert
            Assert.Equal(0x0084, Win32Constants.WM_NC_HITTEST);
        }

        [TestMethod]
        public void HT_TRANSPARENT_HasCorrectValue()
        {
            // Assert
            Assert.Equal(-1, Win32Constants.HT_TRANSPARENT);
        }
    }
}


