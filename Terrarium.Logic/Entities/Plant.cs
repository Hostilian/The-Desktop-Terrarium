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
        private const double MaxWaterLevel = 100.0;
        private const double WaterDecayRate = 2.0; // Water depletes at 2 units per second
        private const double DehydrationDamage = 5.0; // Damage per second without water
        private const double ClickWaterAmount = 30.0;
        private const double WaterSufficientThreshold = 20.0;
        private const double GrowthHealRate = 0.1;
        private const double ClickRadiusPadding = 10.0;
        private const double GrowthExperienceRate = 0.5; // XP per unit of growth
        private const double WaterExperienceAmount = 5.0; // XP gained when watered
        private const double GrowthRateLevelBonus = 0.1; // Growth rate increase per level
        private const double MaxSizeLevelBonus = 2.0; // Max size increase per level

        private double _waterLevel;

        /// <summary>
        /// Current size of the plant.
        /// </summary>
        public double Size
        {
            get => _size;
            private set => _size = Math.Clamp(value, MinSize, GetMaxSize());
        }

        /// <summary>
        /// Gets the maximum size for the plant based on its level.
        /// </summary>
        private double GetMaxSize()
        {
            return MaxSize + ((Level - 1) * MaxSizeLevelBonus);
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
            private set => _waterLevel = Math.Clamp(value, 0, MaxWaterLevel);
        }

        public Plant(double x, double y, double initialSize = MinSize)
            : base(x, y)
        {
            _size = initialSize;
            _growthRate = DefaultGrowthRate;
            _waterLevel = MaxWaterLevel;
        }

        public override void Update(double deltaTime)
        {
            base.Update(deltaTime);

            if (!IsAlive)
                return;

            // Plants need water to survive
            WaterLevel -= WaterDecayRate * deltaTime;

            if (WaterLevel > WaterSufficientThreshold)
            {
                Grow(deltaTime);
            }
            else
            {
                // Plant starts dying without water
                TakeDamage(DehydrationDamage * deltaTime);
            }
        }

        /// <summary>
        /// Makes the plant grow larger.
        /// </summary>
        public void Grow(double deltaTime)
        {
            double maxSize = GetMaxSize();
            if (Size < maxSize && IsAlive)
            {
                double oldSize = Size;
                double effectiveGrowthRate = GrowthRate + ((Level - 1) * GrowthRateLevelBonus);
                Size += effectiveGrowthRate * deltaTime;
                // Growing consumes health
                Heal(GrowthHealRate * deltaTime);
                
                // Gain experience from growing
                double growthAmount = Size - oldSize;
                GainExperience(growthAmount * GrowthExperienceRate);
            }
        }

        /// <summary>
        /// Waters the plant, restoring its water level.
        /// </summary>
        public void Water(double amount)
        {
            WaterLevel += amount;
            // Gain experience from being watered
            GainExperience(WaterExperienceAmount);
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
            Water(ClickWaterAmount); // Clicking waters the plant
        }

        /// <summary>
        /// Checks if a point is within the plant's clickable area.
        /// </summary>
        public bool ContainsPoint(double x, double y)
        {
            double clickRadius = Size + ClickRadiusPadding;
            double dx = x - X;
            double dy = Y - y; // Plants grow upward
            return (dx * dx + dy * dy) <= (clickRadius * clickRadius);
        }
    }
}
