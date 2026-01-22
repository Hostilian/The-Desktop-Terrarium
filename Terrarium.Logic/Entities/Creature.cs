namespace Terrarium.Logic.Entities
{
    using Terrarium.Logic.Interfaces;
    using Terrarium.Logic.Simulation;

    /// <summary>
    /// Base class for all creatures that can move and have hunger.
    /// </summary>
    public abstract class Creature : LivingEntity, IMovable, IClickable
    {
        private double speed;
        private double hunger;
        private double velocityX;
        private double velocityY;

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
        /// Gets or sets movement speed of the creature.
        /// </summary>
        public double Speed
        {
            get => speed;
            protected set => speed = Math.Max(0, value);
        }

        /// <summary>
        /// Gets or sets current hunger level (0-100). Higher values mean more hungry.
        /// </summary>
        public double Hunger
        {
            get => hunger;
            protected set => hunger = Math.Clamp(value, MinHunger, MaxHunger);
        }

        /// <summary>
        /// Gets or sets horizontal velocity component.
        /// </summary>
        public double VelocityX
        {
            get => velocityX;
            set => velocityX = value;
        }

        /// <summary>
        /// Gets or sets vertical velocity component.
        /// </summary>
        public double VelocityY
        {
            get => velocityY;
            set => velocityY = value;
        }

        /// <summary>
        /// Gets or sets faction allegiance of this creature.
        /// </summary>
        public FactionType Faction { get; set; }

        /// <summary>
        /// Gets or sets aggressiveness trait (0-1). Higher values mean more aggressive behavior.
        /// </summary>
        public double Aggressiveness { get; set; }

        /// <summary>
        /// Gets or sets social trait (0-1). Higher values mean more social/grouping behavior.
        /// </summary>
        public double SocialTendency { get; set; }

        /// <summary>
        /// Gets or sets curiosity trait (0-1). Higher values mean more exploration behavior.
        /// </summary>
        public double Curiosity { get; set; }

        /// <summary>
        /// Gets or sets intelligence trait (0-1). Higher values mean more complex decision making.
        /// </summary>
        public double Intelligence { get; set; }

        protected Creature(double x, double y, string type, double speed = DefaultSpeed, FactionType faction = FactionType.VerdantCollective)
            : base(x, y, type)
        {
            this.speed = speed;
            hunger = MinHunger;
            velocityX = 0;
            velocityY = 0;
            Faction = faction;

            // Initialize personality traits with some randomness
            var random = new Random();
            Aggressiveness = random.NextDouble();
            SocialTendency = random.NextDouble();
            Curiosity = random.NextDouble();
            Intelligence = random.NextDouble();
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
            double magnitude = Math.Sqrt((directionX * directionX) + (directionY * directionY));
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
        /// <returns></returns>
        public bool ContainsPoint(double x, double y)
        {
            double dx = x - X;
            double dy = y - Y;
            return ((dx * dx) + (dy * dy)) <= (ClickRadius * ClickRadius);
        }
    }
}
