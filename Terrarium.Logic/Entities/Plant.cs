namespace Terrarium.Logic.Entities
{
    using Terrarium.Logic.Interfaces;

    /// <summary>
    /// Represents a plant entity that grows over time.
    /// </summary>
    public class Plant : LivingEntity, IClickable
    {
        private double size;
        private double growthRate;

        // Plant-specific constants
        private const double MinSize = 1.0;
        private const double MaxSize = 50.0;
        private const double DefaultGrowthRate = 0.5;
        private const double MaxWaterLevel = 100.0;
        private const double WaterDecayRate = 2.0; // Water depletes at 2 units per second
        private const double DehydrationDamage = 5.0; // Damage per second without water
        private const double ClickWaterAmount = 30.0;
        private const double WaterSufficientThreshold = 20.0;
        private const double GrowthHealRate = 0.1;
        private const double ClickRadiusPadding = 10.0;

        private double waterLevel;

        /// <summary>
        /// Gets current size of the plant.
        /// </summary>
        public double Size
        {
            get => size;
            private set => size = Math.Clamp(value, MinSize, MaxSize);
        }

        /// <summary>
        /// Gets or sets rate at which the plant grows per tick.
        /// </summary>
        public double GrowthRate
        {
            get => growthRate;
            set => growthRate = Math.Max(0, value);
        }

        /// <summary>
        /// Gets current water level of the plant (0-100).
        /// </summary>
        public double WaterLevel
        {
            get => waterLevel;
            private set => waterLevel = Math.Clamp(value, 0, MaxWaterLevel);
        }

        public Plant(double x, double y, string type = "Plant", double initialSize = MinSize)
            : base(x, y, type)
        {
            size = initialSize;
            growthRate = DefaultGrowthRate;
            waterLevel = MaxWaterLevel;
        }

        public override void Update(double deltaTime)
        {
            base.Update(deltaTime);
            if (!IsAlive) return;

            WaterLevel -= WaterDecayRate * deltaTime;
            if (WaterLevel > WaterSufficientThreshold)
                Grow(deltaTime);
            else
                TakeDamage(DehydrationDamage * deltaTime);
        }

        public void Grow(double deltaTime)
        {
            if (Size < MaxSize && IsAlive)
            {
                Size += GrowthRate * deltaTime;
                Heal(GrowthHealRate * deltaTime);
            }
        }

        public void Water(double amount) => WaterLevel += amount;

        public void OnClick() => Water(ClickWaterAmount);

        public bool ContainsPoint(double x, double y)
        {
            double clickRadius = Size + ClickRadiusPadding;
            double dx = x - X;
            double dy = Y - y;
            return ((dx * dx) + (dy * dy)) <= (clickRadius * clickRadius);
        }
    }
}
