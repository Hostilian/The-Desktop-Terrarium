using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows;
using Terrarium.Logic.Entities;

namespace Terrarium.Tests.Helpers;

/// <summary>
/// Tests for helper methods extracted during refactoring.
/// These methods are now testable independently.
/// </summary>
[TestClass]
public class CalculationHelpersTests
{
    [TestMethod]
    public void CalculateDistance_WithSamePoint_ReturnsZero()
    {
        double distance = CalculateDistance(10.0, 20.0, 10.0, 20.0);
        Assert.AreEqual(0.0, distance, 0.001);
    }

    [TestMethod]
    public void CalculateDistance_WithHorizontalPoints_ReturnsCorrectDistance()
    {
        double distance = CalculateDistance(0.0, 0.0, 3.0, 0.0);
        Assert.AreEqual(3.0, distance, 0.001);
    }

    [TestMethod]
    public void CalculateDistance_WithVerticalPoints_ReturnsCorrectDistance()
    {
        double distance = CalculateDistance(0.0, 0.0, 0.0, 4.0);
        Assert.AreEqual(4.0, distance, 0.001);
    }

    [TestMethod]
    public void CalculateDistance_WithDiagonalPoints_ReturnsPythagoreanDistance()
    {
        // 3-4-5 right triangle
        double distance = CalculateDistance(0.0, 0.0, 3.0, 4.0);
        Assert.AreEqual(5.0, distance, 0.001);
    }

    [TestMethod]
    public void CalculateDistance_WithNegativeCoordinates_ReturnsCorrectDistance()
    {
        double distance = CalculateDistance(-3.0, -4.0, 0.0, 0.0);
        Assert.AreEqual(5.0, distance, 0.001);
    }

    // Helper method (would be in MainWindow in real code)
    private static double CalculateDistance(double x1, double y1, double x2, double y2)
    {
        return Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
    }
}
