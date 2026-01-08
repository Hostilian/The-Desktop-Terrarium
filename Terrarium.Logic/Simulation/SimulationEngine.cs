using Terrarium.Logic.Entities;

namespace Terrarium.Logic.Simulation
{
    /// <summary>
    /// Main simulation engine that orchestrates all game logic.
    /// Coordinates updates without becoming a "God Object" by delegating to specialized managers.
    /// </summary>
    public class SimulationEngine
    {
        private readonly World _world;
        private readonly MovementCalculator _movementCalculator;
        private readonly CollisionDetector _collisionDetector;
        private readonly FoodManager _foodManager;

        // Simulation timing constants
        private const double LogicTickRate = 0.2; // Logic updates 5 times per second
        private double _logicAccumulator;

        /// <summary>
        /// The simulation world.
        /// </summary>
        public World World => _world;

        /// <summary>
        /// Weather intensity (0.0 = calm, 1.0 = stormy).
        /// </summary>
        public double WeatherIntensity { get; set; }

        public SimulationEngine(double worldWidth, double worldHeight)
        {
            _world = new World(worldWidth, worldHeight);
            _movementCalculator = new MovementCalculator(_world);
            _collisionDetector = new CollisionDetector();
            _foodManager = new FoodManager(_world);
            _logicAccumulator = 0;
            WeatherIntensity = 0.0;
        }

        /// <summary>
        /// Initializes the simulation with starting entities.
        /// </summary>
        public void Initialize()
        {
            _foodManager.InitializeStartingFood();
            
            // Spawn starting creatures
            _world.SpawnRandomHerbivore("Sheep");
            _world.SpawnRandomHerbivore("Rabbit");
            _world.SpawnRandomCarnivore("Wolf");
        }

        /// <summary>
        /// Updates the simulation (called every frame).
        /// </summary>
        public void Update(double deltaTime)
        {
            // Accumulate time for fixed logic updates
            _logicAccumulator += deltaTime;

            // Run logic updates at a fixed rate
            while (_logicAccumulator >= LogicTickRate)
            {
                UpdateLogic(LogicTickRate);
                _logicAccumulator -= LogicTickRate;
            }
        }

        /// <summary>
        /// Updates simulation logic at fixed intervals.
        /// </summary>
        private void UpdateLogic(double deltaTime)
        {
            // Update managers
            _foodManager.Update(deltaTime);

            // Update all entities
            foreach (var entity in _world.GetAllEntities())
            {
                entity.Update(deltaTime);
            }

            // Update creature behaviors
            UpdateHerbivores(deltaTime);
            UpdateCarnivores(deltaTime);

            // Apply weather effects
            ApplyWeatherEffects(deltaTime);

            // Clean up dead entities
            _world.RemoveDeadEntities();
        }

        /// <summary>
        /// Updates herbivore AI behavior.
        /// </summary>
        private void UpdateHerbivores(double deltaTime)
        {
            foreach (var herbivore in _world.Herbivores)
            {
                if (!herbivore.IsAlive) continue;

                // Look for nearby plants if hungry
                if (herbivore.Hunger > 30)
                {
                    var nearestPlant = herbivore.FindNearestPlant(_world.Plants);
                    if (nearestPlant != null)
                    {
                        herbivore.MoveToward(nearestPlant.X, nearestPlant.Y);
                        herbivore.TryEat(nearestPlant);
                    }
                    else
                    {
                        _movementCalculator.UpdateWandering(herbivore, deltaTime);
                    }
                }
                else
                {
                    _movementCalculator.UpdateWandering(herbivore, deltaTime);
                }

                _movementCalculator.EnforceBoundaries(herbivore);
            }
        }

        /// <summary>
        /// Updates carnivore AI behavior.
        /// </summary>
        private void UpdateCarnivores(double deltaTime)
        {
            foreach (var carnivore in _world.Carnivores)
            {
                if (!carnivore.IsAlive) continue;

                // Hunt herbivores if hungry
                if (carnivore.Hunger > 20)
                {
                    var nearestPrey = carnivore.FindNearestPrey(_world.Herbivores);
                    if (nearestPrey != null)
                    {
                        carnivore.Hunt(nearestPrey);
                        carnivore.TryEat(nearestPrey);
                    }
                    else
                    {
                        _movementCalculator.UpdateWandering(carnivore, deltaTime);
                    }
                }
                else
                {
                    _movementCalculator.UpdateWandering(carnivore, deltaTime);
                }

                _movementCalculator.EnforceBoundaries(carnivore);
            }
        }

        /// <summary>
        /// Applies weather effects to entities.
        /// </summary>
        private void ApplyWeatherEffects(double deltaTime)
        {
            if (WeatherIntensity > 0.5)
            {
                // Storm damages plants
                foreach (var plant in _world.Plants)
                {
                    plant.TakeDamage(WeatherIntensity * 0.1 * deltaTime);
                }
            }
        }

        /// <summary>
        /// Finds a clickable entity at the specified position.
        /// </summary>
        public Interfaces.IClickable? FindClickableAt(double x, double y)
        {
            // Check creatures first (they're on top)
            foreach (var creature in _world.GetAllEntities().OfType<Creature>())
            {
                if (creature.IsAlive && creature.ContainsPoint(x, y))
                    return creature;
            }

            // Then check plants
            foreach (var plant in _world.Plants)
            {
                if (plant.IsAlive && plant.ContainsPoint(x, y))
                    return plant;
            }

            return null;
        }
    }
}
