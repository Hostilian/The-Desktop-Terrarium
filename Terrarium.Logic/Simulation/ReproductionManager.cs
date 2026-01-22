namespace Terrarium.Logic.Simulation
{
    using Terrarium.Logic.Entities;

    /// <summary>
    /// Manages creature reproduction mechanics.
    /// Creatures can reproduce when well-fed, healthy, and near a mate.
    /// </summary>
    public class ReproductionManager
    {
        private readonly World world;
        private readonly EventSystem eventSystem;
        private readonly Random random;

        // Reproduction requirements
        private const double MinHealthForReproduction = 70.0;
        private const double MaxHungerForReproduction = 30.0;
        private const double MinAgeForReproduction = 10.0;
        private const double MatingRange = 50.0;

        // Reproduction cooldowns
        private const double ReproductionCooldown = 30.0;
        private const double BaseReproductionChance = 0.1;

        // Population limits
        private const int MaxHerbivores = 15;
        private const int MaxCarnivores = 5;

        // Energy costs
        private const double ReproductionHealthCost = 20.0;
        private const double ReproductionHungerCost = 30.0;

        // Offspring position offset
        private const double OffspringSpawnRadius = 30.0;

        private readonly Dictionary<int, double> reproductionCooldowns;

        private readonly List<int> expiredCooldownIdsBuffer = new();
        private readonly List<Herbivore> herbivoreIterationBuffer = new();
        private readonly List<Carnivore> carnivoreIterationBuffer = new();

        /// <summary>
        /// Gets or sets multiplier applied to the base herbivore reproduction chance.
        /// Allows the simulation engine to apply gentle population pressure.
        /// </summary>
        public double HerbivoreReproductionChanceMultiplier { get; set; } = 1.0;

        /// <summary>
        /// Gets or sets multiplier applied to the base carnivore reproduction chance.
        /// Allows the simulation engine to apply gentle population pressure.
        /// </summary>
        public double CarnivoreReproductionChanceMultiplier { get; set; } = 1.0;

        public ReproductionManager(World world)
            : this(world, EventSystem.Instance, random: null)
        {
        }

        public ReproductionManager(World world, EventSystem eventSystem)
            : this(world, eventSystem, random: null)
        {
        }

        public ReproductionManager(World world, EventSystem eventSystem, Random? random)
        {
            this.world = world;
            this.eventSystem = eventSystem;
            this.random = random ?? new Random();
            reproductionCooldowns = new Dictionary<int, double>();
        }

        /// <summary>
        /// Updates reproduction logic.
        /// </summary>
        public void Update(double deltaTime)
        {
            UpdateCooldowns(deltaTime);

            TryReproduceHerbivores();

            TryReproduceCarnivores();
        }

        private void UpdateCooldowns(double deltaTime)
        {
            expiredCooldownIdsBuffer.Clear();
            foreach (var kvp in reproductionCooldowns)
            {
                reproductionCooldowns[kvp.Key] = kvp.Value - deltaTime;
                if (reproductionCooldowns[kvp.Key] <= 0)
                    expiredCooldownIdsBuffer.Add(kvp.Key);
            }
            foreach (var id in expiredCooldownIdsBuffer)
                reproductionCooldowns.Remove(id);
        }

        private void TryReproduceHerbivores()
        {
            if (world.Herbivores.Count >= MaxHerbivores) return;

            double chance = Math.Clamp(BaseReproductionChance * HerbivoreReproductionChanceMultiplier, 0.0, 1.0);
            herbivoreIterationBuffer.Clear();
            herbivoreIterationBuffer.AddRange(world.Herbivores);

            foreach (var herbivore in herbivoreIterationBuffer)
            {
                if (!CanReproduce(herbivore)) continue;

                var mate = FindMate(herbivore, world.Herbivores);
                if (mate != null && random.NextDouble() < chance)
                {
                    var offspring = CreateOffspring(herbivore, mate);
                    if (offspring != null)
                    {
                        world.AddHerbivore((Herbivore)offspring);
                        ApplyReproductionCost(herbivore);
                        ApplyReproductionCost(mate);
                        SetCooldown(herbivore);
                        SetCooldown(mate);
                        eventSystem.OnEntityBorn(offspring);
                        eventSystem.OnEntityReproduced(herbivore, mate, offspring);
                    }
                }
            }
        }

        private void TryReproduceCarnivores()
        {
            if (world.Carnivores.Count >= MaxCarnivores) return;

            double chance = Math.Clamp(BaseReproductionChance * CarnivoreReproductionChanceMultiplier, 0.0, 1.0);
            carnivoreIterationBuffer.Clear();
            carnivoreIterationBuffer.AddRange(world.Carnivores);

            foreach (var carnivore in carnivoreIterationBuffer)
            {
                if (!CanReproduce(carnivore)) continue;

                var mate = FindMate(carnivore, world.Carnivores);
                if (mate != null && random.NextDouble() < chance)
                {
                    var offspring = CreateOffspring(carnivore, mate);
                    if (offspring != null)
                    {
                        world.AddCarnivore((Carnivore)offspring);
                        ApplyReproductionCost(carnivore);
                        ApplyReproductionCost(mate);
                        SetCooldown(carnivore);
                        SetCooldown(mate);
                        eventSystem.OnEntityBorn(offspring);
                        eventSystem.OnEntityReproduced(carnivore, mate, offspring);
                    }
                }
            }
        }

        public bool CanReproduce(Creature creature)
        {
            if (creature is null)
            {
                throw new ArgumentNullException(nameof(creature));
            }

            return creature.IsAlive &&
                   creature.Health >= MinHealthForReproduction &&
                   creature.Hunger <= MaxHungerForReproduction &&
                   creature.Age >= MinAgeForReproduction &&
                   !reproductionCooldowns.ContainsKey(creature.Id);
        }

        private T? FindMate<T>(T creature, IEnumerable<T> potentialMates)
            where T : Creature
        {
            foreach (var potential in potentialMates)
            {
                if (potential.Id == creature.Id || !CanReproduce(potential) || creature.DistanceTo(potential) > MatingRange)
                    continue;
                return potential;
            }
            return null;
        }

        private Creature? CreateOffspring(Creature parent1, Creature parent2)
        {
            double midX = (parent1.X + parent2.X) / 2;
            double midY = (parent1.Y + parent2.Y) / 2;
            double angle = random.NextDouble() * Math.PI * 2;
            double offsetX = Math.Cos(angle) * OffspringSpawnRadius * random.NextDouble();
            double offsetY = Math.Sin(angle) * OffspringSpawnRadius * random.NextDouble();
            double spawnX = Math.Clamp(midX + offsetX, 0, world.Width);
            double spawnY = Math.Clamp(midY + offsetY, 0, world.Height);

            return parent1 switch
            {
                Herbivore h1 => new Herbivore(spawnX, spawnY, h1.Type),
                Carnivore c1 => new Carnivore(spawnX, spawnY, c1.Type),
                _ => null
            };
        }

        private void ApplyReproductionCost(Creature creature)
        {
            creature.TakeDamage(ReproductionHealthCost);
            creature.Feed(-ReproductionHungerCost);
        }

        private void SetCooldown(Creature creature) => reproductionCooldowns[creature.Id] = ReproductionCooldown;

        public void ClearCooldown(int creatureId) => reproductionCooldowns.Remove(creatureId);
    }
}
