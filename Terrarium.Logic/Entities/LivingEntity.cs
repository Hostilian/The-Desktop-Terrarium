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
        private int _level;
        private double _experience;

        // Named constants instead of magic numbers
        protected const double MaxHealth = 100.0;
        protected const double MinHealth = 0.0;
        protected const double HealthDecayRate = 0.1;
        protected const int MinLevel = 1;
        protected const double ExperiencePerLevel = 100.0;
        protected const double LevelUpHealthBonus = 10.0;

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

        /// <summary>
        /// Current level of the entity (starts at 1).
        /// </summary>
        public int Level
        {
            get => _level;
            private set => _level = Math.Max(MinLevel, value);
        }

        /// <summary>
        /// Current experience points. When reaching ExperiencePerLevel, entity levels up.
        /// </summary>
        public double Experience
        {
            get => _experience;
            private set => _experience = Math.Max(0, value);
        }

        protected LivingEntity(double x, double y, double initialHealth = MaxHealth)
            : base(x, y)
        {
            _isAlive = true;
            Health = initialHealth;
            Age = 0;
            _level = MinLevel;
            _experience = 0;
        }

        public override void Update(double deltaTime)
        {
            if (!IsAlive)
                return;

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
        /// Restores level and experience for persistence.
        /// </summary>
        internal void RestoreLevelStats(int level, double experience)
        {
            _level = level;
            _experience = experience;
        }

        /// <summary>
        /// Adds experience to the entity and handles level-ups.
        /// </summary>
        public void GainExperience(double amount)
        {
            if (!IsAlive)
                return;

            Experience += amount;

            // Check for level up
            while (Experience >= ExperiencePerLevel)
            {
                Experience -= ExperiencePerLevel;
                LevelUp();
            }
        }

        /// <summary>
        /// Called when the entity levels up. Can be overridden for entity-specific bonuses.
        /// </summary>
        protected virtual void LevelUp()
        {
            Level++;
            // Base bonus: restore some health on level up
            Heal(LevelUpHealthBonus);
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
