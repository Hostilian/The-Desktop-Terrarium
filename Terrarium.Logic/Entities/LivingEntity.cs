namespace Terrarium.Logic.Entities
{
    /// <summary>
    /// Base class for all living entities that have health and age.
    /// </summary>
    public abstract class LivingEntity : WorldEntity
    {
        private double _health;
        private double _age;
        private bool _isAlive;

        // Named constants instead of magic numbers
        protected const double MaxHealth = 100.0;
        protected const double MinHealth = 0.0;
        protected const double HealthDecayRate = 0.1;

        /// <summary>
        /// Current health of the entity (0-100).
        /// </summary>
        public double Health
        {
            get => _health;
            private set
            {
                _health = Math.Clamp(value, MinHealth, MaxHealth);
                if (_health <= MinHealth)
                {
                    _isAlive = false;
                }
            }
        }

        /// <summary>
        /// Age of the entity in simulation ticks.
        /// </summary>
        public double Age
        {
            get => _age;
            private set => _age = value;
        }

        /// <summary>
        /// Whether the entity is still alive.
        /// </summary>
        public bool IsAlive
        {
            get => _isAlive;
            private set => _isAlive = value;
        }

        protected LivingEntity(double x, double y, double initialHealth = MaxHealth)
            : base(x, y)
        {
            _isAlive = true;
            Health = initialHealth;
            Age = 0;
        }

        public override void Update(double deltaTime)
        {
            if (!IsAlive) return;

            Age += deltaTime;
            UpdateHealth(deltaTime);
        }

        /// <summary>
        /// Restores core life-state for persistence.
        /// </summary>
        internal void RestoreVitalStats(double health, double age)
        {
            _isAlive = true;
            Health = health;
            Age = age;
        }

        /// <summary>
        /// Updates health-related logic for the entity.
        /// </summary>
        protected virtual void UpdateHealth(double deltaTime)
        {
            // Base implementation: health naturally decays over time
            Health -= HealthDecayRate * deltaTime;
        }

        /// <summary>
        /// Damages the entity by the specified amount.
        /// </summary>
        public void TakeDamage(double damage)
        {
            Health -= damage;
        }

        /// <summary>
        /// Heals the entity by the specified amount.
        /// </summary>
        public void Heal(double amount)
        {
            Health += amount;
        }
    }
}
