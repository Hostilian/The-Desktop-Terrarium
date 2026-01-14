using Terrarium.Logic.Entities;

namespace Terrarium.Logic.Simulation
{
    /// <summary>
    /// Event system for broadcasting entity lifecycle and interaction events.
    /// Follows the Observer pattern for loose coupling.
    /// </summary>
    public class EventSystem
    {
        // Singleton instance for global access
        private static EventSystem? _instance;
        public static EventSystem Instance => _instance ??= new EventSystem();

        /// <summary>
        /// Raised when an entity is born/spawned.
        /// </summary>
        public event EventHandler<EntityEventArgs>? EntityBorn;

        /// <summary>
        /// Raised when an entity dies.
        /// </summary>
        public event EventHandler<EntityDeathEventArgs>? EntityDied;

        /// <summary>
        /// Raised when an entity eats food.
        /// </summary>
        public event EventHandler<EntityFeedEventArgs>? EntityFed;

        /// <summary>
        /// Raised when two entities reproduce.
        /// </summary>
        public event EventHandler<ReproductionEventArgs>? EntityReproduced;

        /// <summary>
        /// Raised when day/night phase changes.
        /// </summary>
        public event EventHandler<DayPhaseEventArgs>? DayPhaseChanged;

        /// <summary>
        /// Raised when weather changes significantly.
        /// </summary>
        public event EventHandler<WeatherEventArgs>? WeatherChanged;

        // Simple delegate-based events for UI integration
        /// <summary>
        /// Simple event for creature births: (creature, parent1, parent2)
        /// </summary>
        public event Action<Creature, Creature?, Creature?>? OnCreatureBorn;

        /// <summary>
        /// Simple event for creature deaths: (creature, cause)
        /// </summary>
        public event Action<Creature, string>? OnCreatureDied;

        /// <summary>
        /// Simple event for plant eaten: (plant, eater)
        /// </summary>
        public event Action<Plant, Creature>? OnPlantEaten;

        /// <summary>
        /// Simple event for day phase change: (phase)
        /// </summary>
        public event Action<string>? OnDayPhaseChanged;

        /// <summary>
        /// Simple event for milestone reached: (name, value)
        /// </summary>
        public event Action<string, int>? OnMilestoneReached;

        /// <summary>
        /// Triggers the EntityBorn event.
        /// </summary>
        public void OnEntityBorn(WorldEntity entity)
        {
            EntityBorn?.Invoke(this, new EntityEventArgs(entity));
        }

        /// <summary>
        /// Triggers the EntityDied event.
        /// </summary>
        public void OnEntityDied(WorldEntity entity, DeathCause cause)
        {
            EntityDied?.Invoke(this, new EntityDeathEventArgs(entity, cause));

            // Also trigger simple event for UI
            if (entity is Creature creature)
            {
                OnCreatureDied?.Invoke(creature, cause.ToString());
            }
        }

        /// <summary>
        /// Triggers the EntityFed event.
        /// </summary>
        public void OnEntityFed(Creature eater, WorldEntity food, double nutritionValue)
        {
            EntityFed?.Invoke(this, new EntityFeedEventArgs(eater, food, nutritionValue));

            // Also trigger simple event for UI
            if (food is Plant plant)
            {
                OnPlantEaten?.Invoke(plant, eater);
            }
        }

        /// <summary>
        /// Triggers the EntityReproduced event.
        /// </summary>
        public void OnEntityReproduced(Creature parent1, Creature? parent2, Creature offspring)
        {
            EntityReproduced?.Invoke(this, new ReproductionEventArgs(parent1, parent2, offspring));

            // Also trigger simple event for UI
            OnCreatureBorn?.Invoke(offspring, parent1, parent2);
        }

        /// <summary>
        /// Raises a day phase changed event.
        /// </summary>
        public void RaiseDayPhaseChanged(string phase)
        {
            OnDayPhaseChanged?.Invoke(phase);
        }

        /// <summary>
        /// Raises a milestone reached event.
        /// </summary>
        public void RaiseMilestoneReached(string name, int value)
        {
            OnMilestoneReached?.Invoke(name, value);
        }

        /// <summary>
        /// Triggers the DayPhaseChanged event.
        /// </summary>
        public void RaiseDayPhaseChanged(DayPhase newPhase, DayPhase oldPhase)
        {
            DayPhaseChanged?.Invoke(this, new DayPhaseEventArgs(newPhase, oldPhase));
            OnDayPhaseChanged?.Invoke(newPhase.ToString());
        }

        /// <summary>
        /// Triggers the WeatherChanged event.
        /// </summary>
        public void OnWeatherChanged(double newIntensity, double oldIntensity)
        {
            WeatherChanged?.Invoke(this, new WeatherEventArgs(newIntensity, oldIntensity));
        }

        /// <summary>
        /// Resets the singleton (for testing purposes).
        /// </summary>
        public static void Reset()
        {
            _instance = new EventSystem();
        }
    }

    /// <summary>
    /// Base event args for entity events.
    /// </summary>
    public class EntityEventArgs : EventArgs
    {
        public WorldEntity Entity { get; }

        public EntityEventArgs(WorldEntity entity)
        {
            Entity = entity;
        }
    }

    /// <summary>
    /// Event args for entity death.
    /// </summary>
    public class EntityDeathEventArgs : EntityEventArgs
    {
        public DeathCause Cause { get; }

        public EntityDeathEventArgs(WorldEntity entity, DeathCause cause) : base(entity)
        {
            Cause = cause;
        }
    }

    /// <summary>
    /// Event args for entity feeding.
    /// </summary>
    public class EntityFeedEventArgs : EventArgs
    {
        public Creature Eater { get; }
        public WorldEntity Food { get; }
        public double NutritionValue { get; }

        public EntityFeedEventArgs(Creature eater, WorldEntity food, double nutritionValue)
        {
            Eater = eater;
            Food = food;
            NutritionValue = nutritionValue;
        }
    }

    /// <summary>
    /// Event args for reproduction.
    /// </summary>
    public class ReproductionEventArgs : EventArgs
    {
        public Creature Parent1 { get; }
        public Creature? Parent2 { get; }
        public Creature Offspring { get; }

        public ReproductionEventArgs(Creature parent1, Creature? parent2, Creature offspring)
        {
            Parent1 = parent1;
            Parent2 = parent2;
            Offspring = offspring;
        }
    }

    /// <summary>
    /// Event args for day phase changes.
    /// </summary>
    public class DayPhaseEventArgs : EventArgs
    {
        public DayPhase NewPhase { get; }
        public DayPhase OldPhase { get; }

        public DayPhaseEventArgs(DayPhase newPhase, DayPhase oldPhase)
        {
            NewPhase = newPhase;
            OldPhase = oldPhase;
        }
    }

    /// <summary>
    /// Event args for weather changes.
    /// </summary>
    public class WeatherEventArgs : EventArgs
    {
        public double NewIntensity { get; }
        public double OldIntensity { get; }
        public bool IsStormy => NewIntensity > 0.5;

        public WeatherEventArgs(double newIntensity, double oldIntensity)
        {
            NewIntensity = newIntensity;
            OldIntensity = oldIntensity;
        }
    }

    /// <summary>
    /// Causes of entity death.
    /// </summary>
    public enum DeathCause
    {
        Natural,
        Starvation,
        Predation,
        Dehydration,
        Weather,
        OldAge
    }
}
