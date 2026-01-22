namespace Terrarium.Logic.Simulation
{
    using Terrarium.Logic.Entities;

    /// <summary>
    /// Main simulation engine that orchestrates all game logic.
    /// Coordinates updates without becoming a "God Object" by delegating to specialized managers.
    /// </summary>
    public class SimulationEngine
    {
        private readonly World world;
        private readonly MovementCalculator movementCalculator;
        private readonly CollisionDetector collisionDetector;
        private readonly FoodManager foodManager;
        private readonly DayNightCycle dayNightCycle;
        private readonly SeasonCycle seasonCycle;
        private readonly DiseaseManager diseaseManager;
        private readonly ReproductionManager reproductionManager;
        private readonly StatisticsTracker statisticsTracker;
        private readonly EventSystem eventSystem;
        private readonly FactionManager factionManager;
        private readonly LoreManager loreManager;

        private readonly List<Creature> creatureCollisionBuffer = new();

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

        private double logicAccumulator;
        private double previousWeatherIntensity;
        private string previousDayPhase = string.Empty;

        /// <summary>
        /// Gets the simulation world.
        /// </summary>
        public World World => world;

        /// <summary>
        /// Gets the food manager.
        /// </summary>
        public FoodManager FoodManager => foodManager;

        /// <summary>
        /// Gets the day/night cycle manager.
        /// </summary>
        public DayNightCycle DayNightCycle => dayNightCycle;

        /// <summary>
        /// Gets the season cycle manager.
        /// </summary>
        public SeasonCycle SeasonCycle => seasonCycle;

        /// <summary>
        /// Gets the statistics tracker.
        /// </summary>
        public StatisticsTracker Statistics => statisticsTracker;

        /// <summary>
        /// Gets the faction manager.
        /// </summary>
        public FactionManager FactionManager => factionManager;

        /// <summary>
        /// Gets the lore manager.
        /// </summary>
        public LoreManager LoreManager => loreManager;

        /// <summary>
        /// Gets the reproduction manager.
        /// </summary>
        public ReproductionManager ReproductionManager => reproductionManager;

        /// <summary>
        /// Gets or sets weather intensity (0.0 = calm, 1.0 = stormy).
        /// </summary>
        public double WeatherIntensity { get; set; }

        /// <summary>
        /// Gets or sets simulation speed multiplier (default 1.0).
        /// </summary>
        public double SimulationSpeed { get; set; } = 1.0;

        /// <summary>
        /// Gets a value indicating whether whether the simulation is currently paused.
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
            this.world = world ?? throw new ArgumentNullException(nameof(world));
            movementCalculator = new MovementCalculator(this.world);
            collisionDetector = new CollisionDetector();
            foodManager = new FoodManager(this.world);
            dayNightCycle = new DayNightCycle();
            seasonCycle = new SeasonCycle();
            diseaseManager = new DiseaseManager();
            statisticsTracker = new StatisticsTracker();
            eventSystem = new EventSystem();
            factionManager = new FactionManager();
            loreManager = new LoreManager();
            reproductionManager = new ReproductionManager(this.world, eventSystem);

            // Hook into events for lore generation
            eventSystem.EntityDied += OnEntityDied;
            eventSystem.OnCreatureBorn += OnCreatureBorn;
        }

        /// <summary>
        /// Initializes the simulation with starting entities.
        /// </summary>
        public void Initialize()
        {
            foodManager.InitializeStartingFood();

            // Spawn starting creatures
            world.SpawnRandomHerbivore("Sheep");
            world.SpawnRandomHerbivore("Rabbit");
            world.SpawnRandomCarnivore("Wolf");
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
            logicAccumulator += scaledDelta;

            // Run logic updates at a fixed rate
            while (logicAccumulator >= LogicTickRate)
            {
                UpdateLogic(LogicTickRate);
                logicAccumulator -= LogicTickRate;
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
            dayNightCycle.Update(deltaTime);
            seasonCycle.Update(deltaTime);

            string currentPhase = GetTimeOfDayString();
            if (currentPhase != previousDayPhase)
            {
                eventSystem.RaiseDayPhaseChanged(currentPhase);
                previousDayPhase = currentPhase;
            }

            if (Math.Abs(WeatherIntensity - previousWeatherIntensity) > 0.1)
                previousWeatherIntensity = WeatherIntensity;
        }

        private void UpdateManagers(double deltaTime)
        {
            foodManager.PlantSpawnChanceMultiplier = seasonCycle.PlantSpawnChanceMultiplier;
            foodManager.Update(deltaTime);

            int plantCount = world.Plants.Count;
            int herbivoreCount = world.Herbivores.Count;
            int carnivoreCount = world.Carnivores.Count;

            reproductionManager.HerbivoreReproductionChanceMultiplier = Math.Clamp(plantCount / Math.Max(1.0, herbivoreCount), 0.2, 1.5);
            reproductionManager.CarnivoreReproductionChanceMultiplier = Math.Clamp(herbivoreCount / Math.Max(1.0, carnivoreCount), 0.2, 1.5);
            reproductionManager.Update(deltaTime);
            diseaseManager.Update(world, deltaTime);

            // Generate lore events periodically
            if (statisticsTracker.SessionTime % 30 < deltaTime) // Every 30 seconds
            {
                string eventDescription = loreManager.GenerateEventDescription(factionManager);
                loreManager.RecordEvent(eventDescription, LoreEventType.FactionEvent, LoreImportance.Medium);
            }
        }

        private void UpdateStatistics(double deltaTime)
        {
            statisticsTracker.UpdateTime(deltaTime);
            statisticsTracker.UpdateSnapshot(
                world.Plants.Count,
                world.Herbivores.Count,
                world.Carnivores.Count);

            // Update faction populations
            var allCreatures = world.Herbivores.Cast<Creature>().Concat(world.Carnivores.Cast<Creature>());
            factionManager.UpdatePopulations(allCreatures);
        }

        private void UpdateAllEntities(double deltaTime)
        {
            foreach (var entity in world.GetAllEntities())
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
            world.RemoveDeadEntities();
        }

        /// <summary>
        /// Updates terrain conquest mechanics for faction warfare.
        /// </summary>
        private void UpdateTerrainConquest(double deltaTime)
        {
            // Process cellular automata terrain conquest
            world.ProcessTerrainConquest();

            // Generate lore events for significant territory changes
            var territoryControl = world.GetTerritoryControl();
            foreach (var kvp in territoryControl)
            {
                if (kvp.Value > 50.0) // Major faction controls more than half the territory
                {
                    var faction = factionManager.GetFaction(kvp.Key);
                    if (faction != null)
                    {
                        string eventDescription = $"{faction.Name} has claimed dominion over {kvp.Value:F1}% of the land, their territory expanding relentlessly.";
                        loreManager.RecordEvent(eventDescription, LoreEventType.FactionEvent, LoreImportance.Major);
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
            creatureCollisionBuffer.Clear();

            foreach (var herbivore in world.Herbivores)
            {
                if (herbivore.IsAlive)
                    creatureCollisionBuffer.Add(herbivore);
            }

            foreach (var carnivore in world.Carnivores)
            {
                if (carnivore.IsAlive)
                    creatureCollisionBuffer.Add(carnivore);
            }

            collisionDetector.ResolveCreatureCollisions(creatureCollisionBuffer);
        }

        private void UpdateHerbivores(double deltaTime)
        {
            foreach (var herbivore in world.Herbivores)
            {
                if (!herbivore.IsAlive) continue;

                HandlePredatorAvoidance(herbivore);
                HandleSocialBehavior(herbivore);
                HandleHungerBehavior(herbivore, deltaTime);
                HandleLeisureBehavior(herbivore, deltaTime);

                movementCalculator.EnforceBoundaries(herbivore);
            }
        }

        private void HandlePredatorAvoidance(Herbivore herbivore)
        {
            var nearestPredator = FindNearestPredator(herbivore);
            if (nearestPredator != null)
            {
                FleeFrom(herbivore, nearestPredator);
            }
        }

        private void HandleSocialBehavior(Herbivore herbivore)
        {
            if (herbivore.SocialTendency > 0.5)
            {
                var nearestFriend = FindNearestHerbivore(herbivore);
                if (nearestFriend != null && herbivore.DistanceTo(nearestFriend) > 50)
                {
                    herbivore.MoveToward(nearestFriend.X, nearestFriend.Y);
                }
            }
        }

        private void HandleHungerBehavior(Herbivore herbivore, double deltaTime)
        {
            if (herbivore.Hunger > HerbivoreHungryThreshold && dayNightCycle.IsDay)
            {
                var nearestPlant = herbivore.FindNearestPlant(world.Plants);
                if (nearestPlant != null)
                {
                    MoveTowardFood(herbivore, nearestPlant);
                    if (herbivore.TryEat(nearestPlant))
                    {
                        eventSystem.OnEntityFed(herbivore, nearestPlant, 30.0);
                        statisticsTracker.RecordFeeding(herbivore, nearestPlant, 30.0);
                    }
                }
                else
                {
                    HandleExplorationWhenHungry(herbivore, deltaTime);
                }
            }
        }

        private void MoveTowardFood(Herbivore herbivore, Plant nearestPlant)
        {
            if (herbivore.Intelligence > 0.7)
            {
                herbivore.MoveToward(nearestPlant.X, nearestPlant.Y);
            }
            else
            {
                var directionX = nearestPlant.X - herbivore.X;
                var directionY = nearestPlant.Y - herbivore.Y;
                herbivore.SetDirection(directionX, directionY);
            }
        }

        private void HandleExplorationWhenHungry(Herbivore herbivore, double deltaTime)
        {
            if (herbivore.Curiosity > 0.6)
            {
                movementCalculator.UpdateExploration(herbivore, deltaTime);
            }
            else
            {
                movementCalculator.UpdateWandering(herbivore, deltaTime);
            }
        }

        private void HandleLeisureBehavior(Herbivore herbivore, double deltaTime)
        {
            if (dayNightCycle.IsNight)
            {
                if (herbivore.Curiosity > 0.8)
                {
                    movementCalculator.UpdateWandering(herbivore, deltaTime * 0.5);
                }
                else
                {
                    herbivore.Stop();
                }
            }
            else
            {
                if (herbivore.Curiosity > 0.7)
                {
                    movementCalculator.UpdateExploration(herbivore, deltaTime);
                }
                else if (herbivore.SocialTendency > 0.6)
                {
                    var nearestFriend = FindNearestHerbivore(herbivore);
                    if (nearestFriend != null && herbivore.DistanceTo(nearestFriend) > 30)
                    {
                        herbivore.MoveToward(nearestFriend.X, nearestFriend.Y);
                    }
                    else
                    {
                        movementCalculator.UpdateWandering(herbivore, deltaTime);
                    }
                }
                else
                {
                    movementCalculator.UpdateWandering(herbivore, deltaTime);
                }
            }
        }

        private Carnivore? FindNearestCarnivore(Carnivore source)
        {
            Carnivore? nearest = null;
            double minDistance = double.MaxValue;

            foreach (var carnivore in world.Carnivores)
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

            foreach (var carnivore in world.Carnivores)
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

            foreach (var herbivore in world.Herbivores)
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
            double length = Math.Sqrt((dx * dx) + (dy * dy));

            if (length > 0)
                herbivore.SetDirection(dx / length * FleeSpeedMultiplier, dy / length * FleeSpeedMultiplier);
        }

        private void UpdateCarnivores(double deltaTime)
        {
            foreach (var carnivore in world.Carnivores)
            {
                if (!carnivore.IsAlive) continue;

                HandlePackBehavior(carnivore);
                HandleHuntingBehavior(carnivore, deltaTime);
                HandleCarnivoreLeisureBehavior(carnivore, deltaTime);

                movementCalculator.EnforceBoundaries(carnivore);
            }
        }

        private void HandlePackBehavior(Carnivore carnivore)
        {
            if (carnivore.SocialTendency > 0.6)
            {
                var nearestPackMate = FindNearestCarnivore(carnivore);
                if (nearestPackMate != null && carnivore.DistanceTo(nearestPackMate) > 40)
                {
                    movementCalculator.MoveToward(carnivore, nearestPackMate.X, nearestPackMate.Y);
                }
            }
        }

        private void HandleHuntingBehavior(Carnivore carnivore, double deltaTime)
        {
            bool isHuntingTime = dayNightCycle.CurrentPhase is DayPhase.Dawn or DayPhase.Dusk or DayPhase.Day;

            if (carnivore.Hunger > CarnivoreHungryThreshold && isHuntingTime)
            {
                var nearestPrey = carnivore.FindNearestPrey(world.Herbivores);
                if (nearestPrey != null)
                {
                    PerformHunt(carnivore, nearestPrey);
                }
                else
                {
                    HandleExplorationWhenHunting(carnivore, deltaTime);
                }
            }
        }

        private void PerformHunt(Carnivore carnivore, Herbivore nearestPrey)
        {
            if (carnivore.Aggressiveness > 0.7)
            {
                carnivore.Hunt(nearestPrey);
            }
            else
            {
                double stalkDistance = 80 + (carnivore.Intelligence * 40);
                if (carnivore.DistanceTo(nearestPrey) > stalkDistance)
                {
                    movementCalculator.MoveToward(carnivore, nearestPrey.X, nearestPrey.Y);
                }
                else
                {
                    carnivore.Hunt(nearestPrey);
                }
            }

            if (carnivore.TryEat(nearestPrey))
            {
                eventSystem.OnEntityFed(carnivore, nearestPrey, 50.0);
                statisticsTracker.RecordFeeding(carnivore, nearestPrey, 50.0);

                if (!nearestPrey.IsAlive)
                {
                    eventSystem.OnEntityDied(nearestPrey, DeathCause.Predation);
                    statisticsTracker.RecordDeath(nearestPrey, DeathCause.Predation);
                }
            }
        }

        private void HandleExplorationWhenHunting(Carnivore carnivore, double deltaTime)
        {
            if (carnivore.Curiosity > 0.6)
            {
                movementCalculator.UpdateExploration(carnivore, deltaTime);
            }
            else
            {
                movementCalculator.UpdateWandering(carnivore, deltaTime);
            }
        }

        private void HandleCarnivoreLeisureBehavior(Carnivore carnivore, double deltaTime)
        {
            if (dayNightCycle.IsNight)
            {
                if (carnivore.Curiosity > 0.8 || carnivore.Hunger > CarnivoreHungryThreshold * 0.8)
                {
                    movementCalculator.UpdateWandering(carnivore, deltaTime * 0.7);
                }
                else
                {
                    carnivore.Stop();
                }
            }
            else
            {
                if (carnivore.Curiosity > 0.7)
                {
                    movementCalculator.UpdateExploration(carnivore, deltaTime);
                }
                else if (carnivore.SocialTendency > 0.5)
                {
                    var nearestPackMate = FindNearestCarnivore(carnivore);
                    if (nearestPackMate != null && carnivore.DistanceTo(nearestPackMate) > 60)
                    {
                        movementCalculator.MoveToward(carnivore, nearestPackMate.X, nearestPackMate.Y);
                    }
                    else
                    {
                        movementCalculator.UpdateWandering(carnivore, deltaTime);
                    }
                }
                else
                {
                    movementCalculator.UpdateWandering(carnivore, deltaTime);
                }
            }
        }

        private void ApplyWeatherEffects(double deltaTime)
        {
            if (WeatherIntensity <= StormWeatherThreshold) return;

            foreach (var plant in world.Plants)
            {
                plant.TakeDamage(WeatherIntensity * StormPlantDamageRate * deltaTime);
                plant.Water(WeatherIntensity * StormPlantWaterBonus * deltaTime);
            }
        }

        public Interfaces.IClickable? FindClickableAt(double x, double y)
        {
            foreach (var creature in world.GetAllEntities().OfType<Creature>())
            {
                if (creature.IsAlive && creature.ContainsPoint(x, y))
                    return creature;
            }

            foreach (var plant in world.Plants)
            {
                if (plant.IsAlive && plant.ContainsPoint(x, y))
                    return plant;
            }

            return null;
        }

        public bool IsEcosystemBalanced()
        {
            return foodManager.IsEcosystemBalanced();
        }

        public double GetEcosystemHealth()
        {
            return foodManager.GetEcosystemHealth();
        }

        public string GetTimeOfDayString() => dayNightCycle.CurrentPhase.ToString();

        public double LightLevel => dayNightCycle.LightLevel;

        /// <summary>
        /// Handles entity death events for lore generation.
        /// </summary>
        private void OnEntityDied(object? sender, EntityDeathEventArgs e)
        {
            if (e.Entity is Creature creature)
            {
                // Check if this was a named character
                loreManager.RecordCharacterDeath(creature.Id);

                // Potentially create a named character from this death (for dramatic effect)
                // This gives dead creatures a chance to become legendary
                loreManager.TryCreateNamedCharacter(creature);
            }
        }

        /// <summary>
        /// Handles creature birth events for lore generation.
        /// </summary>
        private void OnCreatureBorn(Creature creature, Creature? parent1, Creature? parent2)
        {
            // Potentially create a named character from newborn creatures
            loreManager.TryCreateNamedCharacter(creature);
        }
    }
}
