using Terrarium.Logic.Interfaces;

namespace Terrarium.Logic.Entities
{
    /// <summary>
    /// Base class for all creatures that can move and have hunger.
    /// </summary>
    public abstract class Creature : LivingEntity, IMovable, IClickable
    {
        private double _speed;
        private double _hunger;
        private double _velocityX;
        private double _velocityY;

        // Creature-specific constants
        protected const double MaxHunger = 100.0;
        protected const double MinHunger = 0.0;
        protected const double HungerIncreaseRate = 0.3;
        protected const double DefaultSpeed = 50.0;
        protected const double StarvationThreshold = 20.0;
        private const double ClickFeedNutritionValue = 20.0;
        private const double StarvationDamageRate = 0.5;
        private const double FeedHealMultiplier = 0.5;
        private const double ClickRadius = 25.0;

        /// <summary>
        /// Movement speed of the creature.
        /// </summary>
        public double Speed
        {
            get => _speed;
            protected set => _speed = Math.Max(0, value);
        }

        /// <summary>
        /// Current hunger level (0-100). Higher values mean more hungry.
        /// </summary>
        public double Hunger
        {
            get => _hunger;
            protected set => _hunger = Math.Clamp(value, MinHunger, MaxHunger);
        }

        /// <summary>
        /// Horizontal velocity component.
        /// </summary>
        public double VelocityX
        {
            get => _velocityX;
            set => _velocityX = value;
        }

        /// <summary>
        /// Vertical velocity component.
        /// </summary>
        public double VelocityY
        {
            get => _velocityY;
            set => _velocityY = value;
        }

        protected Creature(double x, double y, double speed = DefaultSpeed)
            : base(x, y)
        {
            _speed = speed;
            _hunger = MinHunger;
            _velocityX = 0;
            _velocityY = 0;
        }

        public override void Update(double deltaTime)
        {
            base.Update(deltaTime);
            if (!IsAlive) return;

            Hunger += HungerIncreaseRate * deltaTime;
            if (Hunger > MaxHunger - StarvationThreshold)
                TakeDamage(StarvationDamageRate * deltaTime);

            X += VelocityX * deltaTime;
            Y += VelocityY * deltaTime;
        }

        /// <summary>
        /// Feeds the creature, reducing hunger and restoring health.
        /// </summary>
        public virtual void Feed(double nutritionValue)
        {
            Hunger = Math.Max(MinHunger, Hunger - nutritionValue);

            if (nutritionValue > 0)
            {
                Heal(nutritionValue * FeedHealMultiplier);
            }
        }

        public void SetDirection(double directionX, double directionY)
        {
            double magnitude = Math.Sqrt(directionX * directionX + directionY * directionY);
            if (magnitude > 0)
            {
                VelocityX = (directionX / magnitude) * Speed;
                VelocityY = (directionY / magnitude) * Speed;
            }
        }

        public void Stop()
        {
            VelocityX = 0;
            VelocityY = 0;
        }

        public void Move(double deltaTime)
        {
            X += VelocityX * deltaTime;
            Y += VelocityY * deltaTime;
        }

        public virtual void OnClick() => Feed(ClickFeedNutritionValue);

        /// <summary>
        /// Restores creature-specific state for persistence.
        /// </summary>
        internal void RestoreCreatureState(double hunger, double velocityX, double velocityY)
        {
            Hunger = hunger;
            VelocityX = velocityX;
            VelocityY = velocityY;
        }

        /// <summary>
        /// Checks if a point is within the creature's clickable area.
        /// </summary>
        public bool ContainsPoint(double x, double y)
        {
            double dx = x - X;
            double dy = y - Y;
            return (dx * dx + dy * dy) <= (ClickRadius * ClickRadius);
        }
    }
}
