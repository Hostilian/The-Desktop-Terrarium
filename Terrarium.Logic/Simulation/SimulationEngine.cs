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
        private readonly DayNightCycle _dayNightCycle;
        private readonly SeasonCycle _seasonCycle;
        private readonly DiseaseManager _diseaseManager;
        private readonly ReproductionManager _reproductionManager;
        private readonly StatisticsTracker _statisticsTracker;
        private readonly EventSystem _eventSystem;

        private readonly List<Creature> _creatureCollisionBuffer = new();

        // Simulation timing constants
        private const double LogicTickRate = 0.2; // Logic updates 5 times per second

        // Behavior tuning constants
        private const double HerbivoreHungryThreshold = 30.0;
        private const double CarnivoreHungryThreshold = 20.0;
        private const double StormWeatherThreshold = 0.5;
        private const double StormPlantDamageRate = 0.1;
        private const double StormPlantWaterBonus = 5.0;

        // Fleeing behavior constants
        private const double FleeDetectionRange = 100.0;
        private const double FleeSpeedMultiplier = 1.5;

        private double _logicAccumulator;
        private double _previousWeatherIntensity;
        private string _previousDayPhase = "";

        /// <summary>
        /// The simulation world.
        /// </summary>
        public World World => _world;

        /// <summary>
        /// The day/night cycle manager.
        /// </summary>
        public DayNightCycle DayNightCycle => _dayNightCycle;

        /// <summary>
        /// The season cycle manager.
        /// </summary>
        public SeasonCycle SeasonCycle => _seasonCycle;

        /// <summary>
        /// The statistics tracker.
        /// </summary>
        public StatisticsTracker Statistics => _statisticsTracker;

        /// <summary>
        /// The event system for notifications.
        /// </summary>
        public EventSystem EventSystem => _eventSystem;

        /// <summary>
        /// Weather intensity (0.0 = calm, 1.0 = stormy).
        /// </summary>
        public double WeatherIntensity { get; set; }

        /// <summary>
        /// Simulation speed multiplier (default 1.0).
        /// </summary>
        public double SimulationSpeed { get; set; } = 1.0;

        /// <summary>
        /// Whether the simulation is currently paused.
        /// </summary>
        public bool IsPaused { get; private set; }

        private const double MinSimulationSpeed = 0.25;
        private const double MaxSimulationSpeed = 4.0;

        public SimulationEngine(double worldWidth, double worldHeight)
        {
            _world = new World(worldWidth, worldHeight);
            _movementCalculator = new MovementCalculator(_world);
            _collisionDetector = new CollisionDetector();
            _foodManager = new FoodManager(_world);
            _dayNightCycle = new DayNightCycle();
            _seasonCycle = new SeasonCycle();
            _diseaseManager = new DiseaseManager();
            _statisticsTracker = new StatisticsTracker();
            _eventSystem = new EventSystem();
            _reproductionManager = new ReproductionManager(_world, _eventSystem);
            _logicAccumulator = 0;
            WeatherIntensity = 0.0;
            _previousWeatherIntensity = 0.0;
        }

        public SimulationEngine(World world)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _movementCalculator = new MovementCalculator(_world);
            _collisionDetector = new CollisionDetector();
            _foodManager = new FoodManager(_world);
            _dayNightCycle = new DayNightCycle();
            _seasonCycle = new SeasonCycle();
            _diseaseManager = new DiseaseManager();
            _statisticsTracker = new StatisticsTracker();
            _eventSystem = new EventSystem();
            _reproductionManager = new ReproductionManager(_world, _eventSystem);
            _logicAccumulator = 0;
            WeatherIntensity = 0.0;
            _previousWeatherIntensity = 0.0;
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
            if (IsPaused)
                return;

            // Apply simulation speed multiplier
            double scaledDelta = deltaTime * SimulationSpeed;

            // Accumulate time for fixed logic updates
            _logicAccumulator += scaledDelta;

            // Run logic updates at a fixed rate
            while (_logicAccumulator >= LogicTickRate)
            {
                UpdateLogic(LogicTickRate);
                _logicAccumulator -= LogicTickRate;
            }
        }

        /// <summary>
        /// Sets simulation speed with safety clamping.
        /// </summary>
        public void SetSimulationSpeed(double speed)
        {
            SimulationSpeed = Math.Clamp(speed, MinSimulationSpeed, MaxSimulationSpeed);
        }

        /// <summary>
        /// Pauses the simulation.
        /// </summary>
        public void Pause()
        {
            IsPaused = true;
        }

        /// <summary>
        /// Resumes the simulation.
        /// </summary>
        public void Resume()
        {
            IsPaused = false;
        }

        /// <summary>
        /// Toggles pause state.
        /// </summary>
        public void TogglePause()
        {
            IsPaused = !IsPaused;
        }

        /// <summary>
        /// Updates simulation logic at fixed intervals.
        /// </summary>
        private void UpdateLogic(double deltaTime)
        {
            UpdateCyclesAndEvents(deltaTime);
            UpdateManagers(deltaTime);
            UpdateStatistics(deltaTime);
            UpdateAllEntities(deltaTime);
            UpdateBehaviorsAndWorldEffects(deltaTime);
        }

        private void UpdateCyclesAndEvents(double deltaTime)
        {
            _dayNightCycle.Update(deltaTime);

            var oldSeason = _seasonCycle.CurrentSeason;
            _seasonCycle.Update(deltaTime);
            if (oldSeason != _seasonCycle.CurrentSeason)
            {
                // No UI dependency required; season is exposed via SeasonCycle.
            }

            // Check for day phase change
            string currentPhase = GetTimeOfDayString();
            if (currentPhase != _previousDayPhase)
            {
                _eventSystem.RaiseDayPhaseChanged(currentPhase);
                _previousDayPhase = currentPhase;
            }

            // Check for weather changes
            if (Math.Abs(WeatherIntensity - _previousWeatherIntensity) > 0.1)
            {
                _previousWeatherIntensity = WeatherIntensity;
            }
        }

        private void UpdateManagers(double deltaTime)
        {
            _foodManager.PlantSpawnChanceMultiplier = _seasonCycle.PlantSpawnChanceMultiplier;
            _foodManager.Update(deltaTime);

            // Gentle population pressure: encourage reproduction when resources/prey are abundant.
            int plantCount = _world.Plants.Count;
            int herbivoreCount = _world.Herbivores.Count;
            int carnivoreCount = _world.Carnivores.Count;

            double plantsPerHerb = plantCount / Math.Max(1.0, herbivoreCount);
            double herbPerCarn = herbivoreCount / Math.Max(1.0, carnivoreCount);

            _reproductionManager.HerbivoreReproductionChanceMultiplier = Math.Clamp(plantsPerHerb, 0.2, 1.5);
            _reproductionManager.CarnivoreReproductionChanceMultiplier = Math.Clamp(herbPerCarn, 0.2, 1.5);

            _reproductionManager.Update(deltaTime);

            // Disease pressure (low-impact)
            _diseaseManager.Update(_world, deltaTime);
        }

        private void UpdateStatistics(double deltaTime)
        {
            _statisticsTracker.UpdateTime(deltaTime);
            _statisticsTracker.UpdateSnapshot(
                _world.Plants.Count,
                _world.Herbivores.Count,
                _world.Carnivores.Count);
        }

        private void UpdateAllEntities(double deltaTime)
        {
            foreach (var entity in _world.GetAllEntities())
            {
                entity.Update(deltaTime);
            }
        }

        private void UpdateBehaviorsAndWorldEffects(double deltaTime)
        {
            UpdateHerbivores(deltaTime);
            UpdateCarnivores(deltaTime);
            ResolveCreatureCollisions();
            ApplyWeatherEffects(deltaTime);
            _world.RemoveDeadEntities();
        }

        /// <summary>
        /// Resolves collisions between all creatures.
        /// </summary>
        private void ResolveCreatureCollisions()
        {
            _creatureCollisionBuffer.Clear();

            foreach (var herbivore in _world.Herbivores)
            {
                if (herbivore.IsAlive)
                    _creatureCollisionBuffer.Add(herbivore);
            }

            foreach (var carnivore in _world.Carnivores)
            {
                if (carnivore.IsAlive)
                    _creatureCollisionBuffer.Add(carnivore);
            }

            _collisionDetector.ResolveCreatureCollisions(_creatureCollisionBuffer);
        }

        /// <summary>
        /// Updates herbivore AI behavior.
        /// </summary>
        private void UpdateHerbivores(double deltaTime)
        {
            foreach (var herbivore in _world.Herbivores)
            {
                if (!herbivore.IsAlive)
                    continue;

                // Check for nearby predators and flee if necessary
                var nearestPredator = FindNearestPredator(herbivore);
                if (nearestPredator != null)
                {
                    // Flee from predator!
                    FleeFrom(herbivore, nearestPredator);
                    _movementCalculator.EnforceBoundaries(herbivore);
                    continue;
                }

                // Look for nearby plants if hungry (only during daytime)
                if (herbivore.Hunger > HerbivoreHungryThreshold && _dayNightCycle.IsDay)
                {
                    var nearestPlant = herbivore.FindNearestPlant(_world.Plants);
                    if (nearestPlant != null)
                    {
                        herbivore.MoveToward(nearestPlant.X, nearestPlant.Y);
                        if (herbivore.TryEat(nearestPlant))
                        {
                            _eventSystem.OnEntityFed(herbivore, nearestPlant, 30.0);
                            _statisticsTracker.RecordFeeding(herbivore, nearestPlant, 30.0);
                        }
                    }
                    else
                    {
                        _movementCalculator.UpdateWandering(herbivore, deltaTime);
                    }
                }
                else if (_dayNightCycle.IsNight)
                {
                    // Rest at night (slower, random movement)
                    herbivore.Stop();
                }
                else
                {
                    _movementCalculator.UpdateWandering(herbivore, deltaTime);
                }

                _movementCalculator.EnforceBoundaries(herbivore);
            }
        }

        /// <summary>
        /// Finds the nearest predator to a herbivore within detection range.
        /// </summary>
        private Carnivore? FindNearestPredator(Herbivore herbivore)
        {
            Carnivore? nearest = null;
            double minDistance = FleeDetectionRange;

            foreach (var carnivore in _world.Carnivores)
            {
                if (!carnivore.IsAlive)
                    continue;

                double distance = herbivore.DistanceTo(carnivore);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = carnivore;
                }
            }

            return nearest;
        }

        /// <summary>
        /// Makes a herbivore flee from a predator.
        /// </summary>
        private void FleeFrom(Herbivore herbivore, Carnivore predator)
        {
            // Move in the opposite direction from the predator
            double dx = herbivore.X - predator.X;
            double dy = herbivore.Y - predator.Y;

            // Normalize and apply flee speed boost
            double length = Math.Sqrt(dx * dx + dy * dy);
            if (length > 0)
            {
                dx /= length;
                dy /= length;
            }

            // Set direction with enhanced speed
            herbivore.SetDirection(dx * FleeSpeedMultiplier, dy * FleeSpeedMultiplier);
        }

        /// <summary>
        /// Updates carnivore AI behavior.
        /// </summary>
        private void UpdateCarnivores(double deltaTime)
        {
            foreach (var carnivore in _world.Carnivores)
            {
                if (!carnivore.IsAlive)
                    continue;

                // Apply day/night behavior - carnivores are more active at dawn/dusk
                bool isHuntingTime = _dayNightCycle.CurrentPhase == DayPhase.Dawn ||
                                    _dayNightCycle.CurrentPhase == DayPhase.Dusk ||
                                    _dayNightCycle.CurrentPhase == DayPhase.Day;

                // Hunt herbivores if hungry and it's hunting time
                if (carnivore.Hunger > CarnivoreHungryThreshold && isHuntingTime)
                {
                    var nearestPrey = carnivore.FindNearestPrey(_world.Herbivores);
                    if (nearestPrey != null)
                    {
                        carnivore.Hunt(nearestPrey);
                        if (carnivore.TryEat(nearestPrey))
                        {
                            _eventSystem.OnEntityFed(carnivore, nearestPrey, 50.0);
                            _statisticsTracker.RecordFeeding(carnivore, nearestPrey, 50.0);

                            if (!nearestPrey.IsAlive)
                            {
                                _eventSystem.OnEntityDied(nearestPrey, DeathCause.Predation);
                                _statisticsTracker.RecordDeath(nearestPrey, DeathCause.Predation);
                            }
                        }
                    }
                    else
                    {
                        _movementCalculator.UpdateWandering(carnivore, deltaTime);
                    }
                }
                else if (_dayNightCycle.IsNight)
                {
                    // Rest at night
                    carnivore.Stop();
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
            if (WeatherIntensity > StormWeatherThreshold)
            {
                foreach (var plant in _world.Plants)
                {
                    // Storm damages plants but also waters them (rain!)
                    plant.TakeDamage(WeatherIntensity * StormPlantDamageRate * deltaTime);
                    plant.Water(WeatherIntensity * StormPlantWaterBonus * deltaTime);
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

        public bool IsEcosystemBalanced()
        {
            return _foodManager.IsEcosystemBalanced();
        }

        public double GetEcosystemHealth()
        {
            return _foodManager.GetEcosystemHealth();
        }

        /// <summary>
        /// Gets the current time of day as a string.
        /// </summary>
        public string GetTimeOfDayString()
        {
            return _dayNightCycle.CurrentPhase.ToString();
        }

        /// <summary>
        /// Gets the light level (0.0-1.0) for rendering purposes.
        /// </summary>
        public double GetLightLevel()
        {
            return _dayNightCycle.LightLevel;
        }
    }
}
