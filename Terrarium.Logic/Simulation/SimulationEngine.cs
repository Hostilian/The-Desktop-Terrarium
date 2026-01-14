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
        private readonly FactionManager _factionManager;
        private readonly LoreManager _loreManager;

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
        /// The faction manager.
        /// </summary>
        public FactionManager FactionManager => _factionManager;

        /// <summary>
        /// The lore manager.
        /// </summary>
        public LoreManager LoreManager => _loreManager;

        /// <summary>
        /// The reproduction manager.
        /// </summary>
        public ReproductionManager ReproductionManager => _reproductionManager;

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

        public SimulationEngine(double worldWidth, double worldHeight, TerrariumType terrariumType = TerrariumType.Forest)
            : this(new World(worldWidth, worldHeight, terrariumType))
        {
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
            _factionManager = new FactionManager();
            _loreManager = new LoreManager();
            _reproductionManager = new ReproductionManager(_world, _eventSystem);

            // Hook into events for lore generation
            _eventSystem.EntityDied += OnEntityDied;
            _eventSystem.OnCreatureBorn += OnCreatureBorn;
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

        public void SetSimulationSpeed(double speed) => SimulationSpeed = Math.Clamp(speed, MinSimulationSpeed, MaxSimulationSpeed);

        public void Pause() => IsPaused = true;

        public void Resume() => IsPaused = false;

        public void TogglePause() => IsPaused = !IsPaused;

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
            UpdateTerrainConquest(deltaTime);
        }

        private void UpdateCyclesAndEvents(double deltaTime)
        {
            _dayNightCycle.Update(deltaTime);
            _seasonCycle.Update(deltaTime);

            string currentPhase = GetTimeOfDayString();
            if (currentPhase != _previousDayPhase)
            {
                _eventSystem.RaiseDayPhaseChanged(currentPhase);
                _previousDayPhase = currentPhase;
            }

            if (Math.Abs(WeatherIntensity - _previousWeatherIntensity) > 0.1)
                _previousWeatherIntensity = WeatherIntensity;
        }

        private void UpdateManagers(double deltaTime)
        {
            _foodManager.PlantSpawnChanceMultiplier = _seasonCycle.PlantSpawnChanceMultiplier;
            _foodManager.Update(deltaTime);

            int plantCount = _world.Plants.Count;
            int herbivoreCount = _world.Herbivores.Count;
            int carnivoreCount = _world.Carnivores.Count;

            _reproductionManager.HerbivoreReproductionChanceMultiplier = Math.Clamp(plantCount / Math.Max(1.0, herbivoreCount), 0.2, 1.5);
            _reproductionManager.CarnivoreReproductionChanceMultiplier = Math.Clamp(herbivoreCount / Math.Max(1.0, carnivoreCount), 0.2, 1.5);
            _reproductionManager.Update(deltaTime);
            _diseaseManager.Update(_world, deltaTime);

            // Generate lore events periodically
            if (_statisticsTracker.SessionTime % 30 < deltaTime) // Every 30 seconds
            {
                string eventDescription = _loreManager.GenerateEventDescription(_factionManager);
                _loreManager.RecordEvent(eventDescription, LoreEventType.FactionEvent, LoreImportance.Medium);
            }
        }

        private void UpdateStatistics(double deltaTime)
        {
            _statisticsTracker.UpdateTime(deltaTime);
            _statisticsTracker.UpdateSnapshot(
                _world.Plants.Count,
                _world.Herbivores.Count,
                _world.Carnivores.Count);

            // Update faction populations
            var allCreatures = _world.Herbivores.Cast<Creature>().Concat(_world.Carnivores.Cast<Creature>());
            _factionManager.UpdatePopulations(allCreatures);
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
        /// Updates terrain conquest mechanics for faction warfare.
        /// </summary>
        private void UpdateTerrainConquest(double deltaTime)
        {
            // Process cellular automata terrain conquest
            _world.ProcessTerrainConquest();

            // Generate lore events for significant territory changes
            var territoryControl = _world.GetTerritoryControl();
            foreach (var kvp in territoryControl)
            {
                if (kvp.Value > 50.0) // Major faction controls more than half the territory
                {
                    var faction = _factionManager.GetFaction(kvp.Key);
                    if (faction != null)
                    {
                        string eventDescription = $"{faction.Name} has claimed dominion over {kvp.Value:F1}% of the land, their territory expanding relentlessly.";
                        _loreManager.RecordEvent(eventDescription, LoreEventType.FactionEvent, LoreImportance.Major);
                        break; // Only record one major event per update
                    }
                }
            }
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

        private void UpdateHerbivores(double deltaTime)
        {
            foreach (var herbivore in _world.Herbivores)
            {
                if (!herbivore.IsAlive) continue;

                // Check for predators first (survival instinct)
                var nearestPredator = FindNearestPredator(herbivore);
                if (nearestPredator != null)
                {
                    FleeFrom(herbivore, nearestPredator);
                    _movementCalculator.EnforceBoundaries(herbivore);
                    continue;
                }

                // Social behavior - group with similar herbivores
                if (herbivore.SocialTendency > 0.5)
                {
                    var nearestFriend = FindNearestHerbivore(herbivore);
                    if (nearestFriend != null && herbivore.DistanceTo(nearestFriend) > 50)
                    {
                        herbivore.MoveToward(nearestFriend.X, nearestFriend.Y);
                        _movementCalculator.EnforceBoundaries(herbivore);
                        continue;
                    }
                }

                // Hunger-driven behavior
                if (herbivore.Hunger > HerbivoreHungryThreshold && _dayNightCycle.IsDay)
                {
                    var nearestPlant = herbivore.FindNearestPlant(_world.Plants);
                    if (nearestPlant != null)
                    {
                        // Intelligent creatures plan their path better
                        if (herbivore.Intelligence > 0.7)
                        {
                            herbivore.MoveToward(nearestPlant.X, nearestPlant.Y);
                        }
                        else
                        {
                            // Less intelligent creatures wander toward food
                            var directionX = nearestPlant.X - herbivore.X;
                            var directionY = nearestPlant.Y - herbivore.Y;
                            herbivore.SetDirection(directionX, directionY);
                        }

                        if (herbivore.TryEat(nearestPlant))
                        {
                            _eventSystem.OnEntityFed(herbivore, nearestPlant, 30.0);
                            _statisticsTracker.RecordFeeding(herbivore, nearestPlant, 30.0);
                        }
                    }
                    else
                    {
                        // Curious creatures explore more when hungry
                        if (herbivore.Curiosity > 0.6)
                        {
                            _movementCalculator.UpdateExploration(herbivore, deltaTime);
                        }
                        else
                        {
                            _movementCalculator.UpdateWandering(herbivore, deltaTime);
                        }
                    }
                }
                else if (_dayNightCycle.IsNight)
                {
                    // Some creatures are nocturnal
                    if (herbivore.Curiosity > 0.8)
                    {
                        _movementCalculator.UpdateWandering(herbivore, deltaTime * 0.5); // Slower at night
                    }
                    else
                    {
                        herbivore.Stop();
                    }
                }
                else
                {
                    // Leisure behavior based on personality
                    if (herbivore.Curiosity > 0.7)
                    {
                        _movementCalculator.UpdateExploration(herbivore, deltaTime);
                    }
                    else if (herbivore.SocialTendency > 0.6)
                    {
                        // Social creatures seek company
                        var nearestFriend = FindNearestHerbivore(herbivore);
                        if (nearestFriend != null && herbivore.DistanceTo(nearestFriend) > 30)
                        {
                            herbivore.MoveToward(nearestFriend.X, nearestFriend.Y);
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
                }

                _movementCalculator.EnforceBoundaries(herbivore);
            }
        }

        private Carnivore? FindNearestCarnivore(Carnivore source)
        {
            Carnivore? nearest = null;
            double minDistance = double.MaxValue;

            foreach (var carnivore in _world.Carnivores)
            {
                if (!carnivore.IsAlive || carnivore == source) continue;

                double distance = source.DistanceTo(carnivore);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = carnivore;
                }
            }

            return nearest;
        }

        private Carnivore? FindNearestPredator(Herbivore herbivore)
        {
            Carnivore? nearest = null;
            double minDistance = FleeDetectionRange;

            foreach (var carnivore in _world.Carnivores)
            {
                if (!carnivore.IsAlive) continue;

                double distance = herbivore.DistanceTo(carnivore);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = carnivore;
                }
            }

            return nearest;
        }

        private Herbivore? FindNearestHerbivore(Herbivore source)
        {
            Herbivore? nearest = null;
            double minDistance = double.MaxValue;

            foreach (var herbivore in _world.Herbivores)
            {
                if (!herbivore.IsAlive || herbivore == source) continue;

                double distance = source.DistanceTo(herbivore);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = herbivore;
                }
            }

            return nearest;
        }

        private void FleeFrom(Herbivore herbivore, Carnivore predator)
        {
            double dx = herbivore.X - predator.X;
            double dy = herbivore.Y - predator.Y;
            double length = Math.Sqrt(dx * dx + dy * dy);

            if (length > 0)
                herbivore.SetDirection(dx / length * FleeSpeedMultiplier, dy / length * FleeSpeedMultiplier);
        }

        private void UpdateCarnivores(double deltaTime)
        {
            foreach (var carnivore in _world.Carnivores)
            {
                if (!carnivore.IsAlive) continue;

                bool isHuntingTime = _dayNightCycle.CurrentPhase is DayPhase.Dawn or DayPhase.Dusk or DayPhase.Day;

                // Pack behavior for social carnivores
                if (carnivore.SocialTendency > 0.6)
                {
                    var nearestPackMate = FindNearestCarnivore(carnivore);
                    if (nearestPackMate != null && carnivore.DistanceTo(nearestPackMate) > 40)
                    {
                        _movementCalculator.MoveToward(carnivore, nearestPackMate.X, nearestPackMate.Y);
                        _movementCalculator.EnforceBoundaries(carnivore);
                        continue;
                    }
                }

                // Hunting behavior
                if (carnivore.Hunger > CarnivoreHungryThreshold && isHuntingTime)
                {
                    var nearestPrey = carnivore.FindNearestPrey(_world.Herbivores);
                    if (nearestPrey != null)
                    {
                        // Aggressive carnivores attack immediately
                        if (carnivore.Aggressiveness > 0.7)
                        {
                            carnivore.Hunt(nearestPrey);
                        }
                        else
                        {
                            // Cautious carnivores stalk from distance
                            double stalkDistance = 80 + (carnivore.Intelligence * 40); // Smarter = better stalking
                            if (carnivore.DistanceTo(nearestPrey) > stalkDistance)
                            {
                                _movementCalculator.MoveToward(carnivore, nearestPrey.X, nearestPrey.Y);
                            }
                            else
                            {
                                carnivore.Hunt(nearestPrey);
                            }
                        }

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
                        // Curious carnivores explore for prey
                        if (carnivore.Curiosity > 0.6)
                        {
                            _movementCalculator.UpdateExploration(carnivore, deltaTime);
                        }
                        else
                        {
                            _movementCalculator.UpdateWandering(carnivore, deltaTime);
                        }
                    }
                }
                else if (_dayNightCycle.IsNight)
                {
                    // Nocturnal predators
                    if (carnivore.Curiosity > 0.8 || carnivore.Hunger > CarnivoreHungryThreshold * 0.8)
                    {
                        _movementCalculator.UpdateWandering(carnivore, deltaTime * 0.7); // Slightly active at night
                    }
                    else
                    {
                        carnivore.Stop();
                    }
                }
                else
                {
                    // Leisure behavior
                    if (carnivore.Curiosity > 0.7)
                    {
                        _movementCalculator.UpdateExploration(carnivore, deltaTime);
                    }
                    else if (carnivore.SocialTendency > 0.5)
                    {
                        // Social carnivores maintain pack cohesion
                        var nearestPackMate = FindNearestCarnivore(carnivore);
                        if (nearestPackMate != null && carnivore.DistanceTo(nearestPackMate) > 60)
                        {
                            _movementCalculator.MoveToward(carnivore, nearestPackMate.X, nearestPackMate.Y);
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
                }

                _movementCalculator.EnforceBoundaries(carnivore);
            }
        }

        private void ApplyWeatherEffects(double deltaTime)
        {
            if (WeatherIntensity <= StormWeatherThreshold) return;

            foreach (var plant in _world.Plants)
            {
                plant.TakeDamage(WeatherIntensity * StormPlantDamageRate * deltaTime);
                plant.Water(WeatherIntensity * StormPlantWaterBonus * deltaTime);
            }
        }

        public Interfaces.IClickable? FindClickableAt(double x, double y)
        {
            foreach (var creature in _world.GetAllEntities().OfType<Creature>())
            {
                if (creature.IsAlive && creature.ContainsPoint(x, y))
                    return creature;
            }

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

        public string GetTimeOfDayString() => _dayNightCycle.CurrentPhase.ToString();

        public double GetLightLevel() => _dayNightCycle.LightLevel;

        /// <summary>
        /// Handles entity death events for lore generation.
        /// </summary>
        private void OnEntityDied(object? sender, EntityDeathEventArgs e)
        {
            if (e.Entity is Creature creature)
            {
                // Check if this was a named character
                _loreManager.RecordCharacterDeath(creature.Id);

                // Potentially create a named character from this death (for dramatic effect)
                // This gives dead creatures a chance to become legendary
                _loreManager.TryCreateNamedCharacter(creature);
            }
        }

        /// <summary>
        /// Handles creature birth events for lore generation.
        /// </summary>
        private void OnCreatureBorn(Creature creature, Creature? parent1, Creature? parent2)
        {
            // Potentially create a named character from newborn creatures
            _loreManager.TryCreateNamedCharacter(creature);
        }
    }
}
