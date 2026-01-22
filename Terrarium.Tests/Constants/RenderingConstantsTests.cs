using Microsoft.VisualStudio.TestTools.UnitTesting;
using Terrarium.Desktop.Constants;

namespace Terrarium.Tests.Constants;

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
        Assert.AreEqual(60, RenderingConstants.DEFAULT_RENDER_FPS,
            "Standard rendering should be 60 FPS for smooth animation");
    }

    [TestMethod]
    public void DefaultRenderFps_ShouldBePositive()
    {
        Assert.IsGreaterThan(0, RenderingConstants.DEFAULT_RENDER_FPS,
            "FPS must be positive");
    }

    [TestMethod]
    public void MillisecondsPerSecond_ShouldBe1000()
    {
        Assert.AreEqual(1000.0, RenderingConstants.MILLISECONDS_PER_SECOND,
            "There are exactly 1000 milliseconds in a second");
    }

    [TestMethod]
    public void RenderIntervalMs_ShouldMatchFpsCalculation()
    {
        double expected = RenderingConstants.MILLISECONDS_PER_SECOND / RenderingConstants.DEFAULT_RENDER_FPS;
        Assert.AreEqual(expected, RenderingConstants.RENDER_INTERVAL_MS, 0.01,
            "Render interval should equal 1000ms / FPS");
    }

    [TestMethod]
    public void RenderIntervalMs_ShouldBeApproximately16_67()
    {
        // 1000ms / 60fps = 16.67ms per frame
        Assert.AreEqual(16.67, RenderingConstants.RENDER_INTERVAL_MS, 0.1,
            "60 FPS should result in approximately 16.67ms per frame");
    }

    [TestMethod]
    public void SystemMonitorUpdateInterval_ShouldBePositive()
    {
        Assert.IsGreaterThan(0.0, RenderingConstants.SYSTEM_MONITOR_UPDATE_INTERVAL_MS,
            "System monitor interval must be positive");
    }

    [TestMethod]
    public void SystemMonitorUpdateInterval_ShouldBeSlowerThanRenderInterval()
    {
        Assert.IsGreaterThan(RenderingConstants.RENDER_INTERVAL_MS, RenderingConstants.SYSTEM_MONITOR_UPDATE_INTERVAL_MS,
            "System monitor should update slower than rendering to save CPU");
    }
}
#pragma warning restore MSTEST0032
#pragma warning restore MSTEST0017
#pragma warning restore MSTEST0025


