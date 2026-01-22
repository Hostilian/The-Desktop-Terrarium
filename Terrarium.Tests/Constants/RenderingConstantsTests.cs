namespace Terrarium.Tests.Constants;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Terrarium.Desktop.Constants;

/// <summary>
/// Unit tests for RenderingConstants to verify rendering configuration values.
/// </summary>
[TestClass]
public class RenderingConstantsTests
{
#pragma warning disable MSTEST0032 // Meaningful assertions for constant validation
#pragma warning disable MSTEST0017 // Analyzer doesn't understand constant expressions
#pragma warning disable MSTEST0025 // Analyzer doesn't understand tolerance in assertions
    [TestMethod]
    public void DefaultRenderFps_ShouldBe60()
    {
        Assert.AreEqual(60, RenderingConstants.DEFAULTRENDERFPS,
            "Standard rendering should be 60 FPS for smooth animation");
    }

    [TestMethod]
    public void DefaultRenderFps_ShouldBePositive()
    {
        Assert.IsGreaterThan(0, RenderingConstants.DEFAULTRENDERFPS,
            "FPS must be positive");
    }

    [TestMethod]
    public void MillisecondsPerSecond_ShouldBe1000()
    {
        Assert.AreEqual(1000.0, RenderingConstants.MILLISECONDSPERSECOND,
            "There are exactly 1000 milliseconds in a second");
    }

    [TestMethod]
    public void RenderIntervalMs_ShouldMatchFpsCalculation()
    {
        double expected = RenderingConstants.MILLISECONDSPERSECOND / RenderingConstants.DEFAULTRENDERFPS;
        Assert.AreEqual(expected, RenderingConstants.RENDERINTERVALMS, 0.01,
            "Render interval should equal 1000ms / FPS");
    }

    [TestMethod]
    public void RenderIntervalMs_ShouldBeApproximately16_67()
    {
        // 1000ms / 60fps = 16.67ms per frame
        Assert.AreEqual(16.67, RenderingConstants.RENDERINTERVALMS, 0.1,
            "60 FPS should result in approximately 16.67ms per frame");
    }

    [TestMethod]
    public void SystemMonitorUpdateInterval_ShouldBePositive()
    {
        Assert.IsGreaterThan(0.0, RenderingConstants.SYSTEMMONITORUPDATEINTERVALMS,
            "System monitor interval must be positive");
    }

    [TestMethod]
    public void SystemMonitorUpdateInterval_ShouldBeSlowerThanRenderInterval()
    {
        Assert.IsGreaterThan(RenderingConstants.RENDERINTERVALMS, RenderingConstants.SYSTEMMONITORUPDATEINTERVALMS,
            "System monitor should update slower than rendering to save CPU");
    }
}
#pragma warning restore MSTEST0032
#pragma warning restore MSTEST0017
#pragma warning restore MSTEST0025

