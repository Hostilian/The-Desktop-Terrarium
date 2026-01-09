using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Terrarium.Tests.Rendering
{
    [TestClass]
    public class PredatorWarningTests
    {
        private const double WarningRadius = 100.0;

        [TestMethod]
        public void PredatorNearby_WithinRadius_ReturnsTrue()
        {
            double herbivoreX = 100, herbivoreY = 100;
            double carnivoreX = 150, carnivoreY = 100;

            double distance = CalculateDistance(herbivoreX, herbivoreY, carnivoreX, carnivoreY);
            bool inDanger = distance < WarningRadius;

            Assert.IsTrue(inDanger, "Herbivore 50 units from carnivore should be in danger");
        }

        [TestMethod]
        public void PredatorNearby_OutsideRadius_ReturnsFalse()
        {
            double herbivoreX = 100, herbivoreY = 100;
            double carnivoreX = 300, carnivoreY = 100;

            double distance = CalculateDistance(herbivoreX, herbivoreY, carnivoreX, carnivoreY);
            bool inDanger = distance < WarningRadius;

            Assert.IsFalse(inDanger, "Herbivore 200 units from carnivore should be safe");
        }

        [TestMethod]
        public void PredatorNearby_ExactRadius_ReturnsFalse()
        {
            double herbivoreX = 0, herbivoreY = 0;
            double carnivoreX = WarningRadius, carnivoreY = 0;

            double distance = CalculateDistance(herbivoreX, herbivoreY, carnivoreX, carnivoreY);
            bool inDanger = distance < WarningRadius;

            Assert.IsFalse(inDanger, "Exactly at radius boundary should be safe");
        }

        [TestMethod]
        public void PredatorNearby_DiagonalDistance()
        {
            // At (0,0), carnivore at (70, 70)
            double herbivoreX = 0, herbivoreY = 0;
            double carnivoreX = 70, carnivoreY = 70;

            double distance = CalculateDistance(herbivoreX, herbivoreY, carnivoreX, carnivoreY);
            // sqrt(70^2 + 70^2) = sqrt(9800) ≈ 98.99

            Assert.IsLessThan(WarningRadius, distance,
                $"Diagonal distance {distance:F2} should be within {WarningRadius}");
        }

        [TestMethod]
        public void NearestPredator_SelectsClosest()
        {
            double herbivoreX = 0, herbivoreY = 0;

            var carnivores = new[]
            {
                (x: 200.0, y: 0.0),   // 200 units away
                (x: 50.0, y: 0.0),    // 50 units away (nearest)
                (x: 150.0, y: 0.0),   // 150 units away
            };

            double nearestDistance = double.MaxValue;
            foreach (var (x, y) in carnivores)
            {
                double dist = CalculateDistance(herbivoreX, herbivoreY, x, y);
                nearestDistance = Math.Min(nearestDistance, dist);
            }

            Assert.AreEqual(50, nearestDistance, 0.01,
                "Nearest predator should be 50 units away");
        }

        /// <summary>
        /// Mirrors the PredatorWarningSystem's distance calculation.
        /// </summary>
        private double CalculateDistance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
        }
    }
}
