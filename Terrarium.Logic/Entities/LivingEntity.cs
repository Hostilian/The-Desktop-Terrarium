namespace Terrarium.Logic.Entities
{
    /// <summary>
    /// Base class for all living entities that have health and age.
    /// </summary>
    public abstract class LivingEntity : WorldEntity
    {
        private double health;
        private double age;
        private bool isAlive;

        // Named constants instead of magic numbers
        protected const double MaxHealth = 100.0;
        protected const double MinHealth = 0.0;
        protected const double HealthDecayRate = 0.1;

        /// <summary>
        /// Gets current health of the entity (0-100).
        /// </summary>
        public double Health
        {
            get => health;
            private set
            {
                health = Math.Clamp(value, MinHealth, MaxHealth);
                if (health <= MinHealth)
                {
                    isAlive = false;
                }
            }
        }

        /// <summary>
        /// Gets age of the entity in simulation ticks.
        /// </summary>
        public double Age
        {
            get => age;
            private set => age = value;
        }

        /// <summary>
        /// Gets a value indicating whether whether the entity is still alive.
        /// </summary>
        public bool IsAlive
        {
            get => isAlive;
            private set => isAlive = value;
        }

        protected LivingEntity(double x, double y, string type, double initialHealth = MaxHealth)
            : base(x, y, type)
        {
            isAlive = true;
            Health = initialHealth;
            Age = 0;
        }

        public override void Update(double deltaTime)
        {
            if (!IsAlive) return;
            Age += deltaTime;
            UpdateHealth(deltaTime);
        }

        internal void RestoreVitalStats(double health, double age)
        {
            isAlive = true;
            Health = health;
            Age = age;
        }

        protected virtual void UpdateHealth(double deltaTime) => Health -= HealthDecayRate * deltaTime;

        public void TakeDamage(double damage) => Health -= damage;

        public void Heal(double amount) => Health += amount;
    }
}
