using Terrarium.Logic.Interfaces;

namespace Terrarium.Logic.Entities
{
    /// <summary>
    /// Represents a plant entity that grows over time.
    /// </summary>
    public class Plant : LivingEntity, IClickable
    {
        private double _size;
        private double _growthRate;

        // Plant-specific constants
        private const double MinSize = 1.0;
        private const double MaxSize = 50.0;
        private const double DefaultGrowthRate = 0.5;
        private const double WaterLevel = 100.0;
        private const double WaterDecayRate = 0.2;

        private double _waterLevel;

        /// <summary>
        /// Current size of the plant.
        /// </summary>
        public double Size
        {
            get => _size;
            private set => _size = Math.Clamp(value, MinSize, MaxSize);
        }

        /// <summary>
        /// Rate at which the plant grows per tick.
        /// </summary>
        public double GrowthRate
        {
            get => _growthRate;
            set => _growthRate = Math.Max(0, value);
        }

        /// <summary>
        /// Current water level of the plant (0-100).
        /// </summary>
        public double WaterLevel
        {
            get => _waterLevel;
            private set => _waterLevel = Math.Clamp(value, 0, WaterLevel);
        }

        public Plant(double x, double y, double initialSize = MinSize)
            : base(x, y)
        {
            _size = initialSize;
            _growthRate = DefaultGrowthRate;
            _waterLevel = WaterLevel;
        }

        public override void Update(double deltaTime)
        {
            base.Update(deltaTime);

            if (!IsAlive) return;

            // Plants need water to survive
            WaterLevel -= WaterDecayRate * deltaTime;

            if (WaterLevel > 20)
            {
                Grow(deltaTime);
            }
            else
            {
                // Plant starts dying without water
                TakeDamage(1.0 * deltaTime);
            }
        }

        /// <summary>
        /// Makes the plant grow larger.
        /// </summary>
        public void Grow(double deltaTime)
        {
            if (Size < MaxSize && IsAlive)
            {
                Size += GrowthRate * deltaTime;
                // Growing consumes health
                Health = Math.Min(MaxHealth, Health + 0.1 * deltaTime);
            }
        }

        /// <summary>
        /// Waters the plant, restoring its water level.
        /// </summary>
        public void Water(double amount)
        {
            WaterLevel += amount;
        }

        /// <summary>
        /// Makes the plant shake (visual effect trigger).
        /// </summary>
        public void Shake()
        {
            // This method is called by UI when mouse hovers
            // The visual effect is handled in the presentation layer
        }

        /// <summary>
        /// Called when the plant is clicked (waters it).
        /// </summary>
        public void OnClick()
        {
            Water(30.0); // Clicking waters the plant
        }

        /// <summary>
        /// Checks if a point is within the plant's clickable area.
        /// </summary>
        public bool ContainsPoint(double x, double y)
        {
            double clickRadius = Size + 10.0;
            double dx = x - X;
            double dy = Y - y; // Plants grow upward
            return (dx * dx + dy * dy) <= (clickRadius * clickRadius);
        }
    }
}
