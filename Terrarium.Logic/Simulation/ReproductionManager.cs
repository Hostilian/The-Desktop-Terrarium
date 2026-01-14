using Terrarium.Logic.Entities;

namespace Terrarium.Logic.Simulation
{
    /// <summary>
    /// Manages creature reproduction mechanics.
    /// Creatures can reproduce when well-fed, healthy, and near a mate.
    /// </summary>
    public class ReproductionManager
    {
        private readonly World _world;
        private readonly EventSystem _eventSystem;
        private readonly Random _random;
        private readonly CollisionDetector _collisionDetector;

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

        private readonly Dictionary<int, double> _reproductionCooldowns;

        private readonly List<int> _expiredCooldownIdsBuffer = new();
        private readonly List<Herbivore> _herbivoreIterationBuffer = new();
        private readonly List<Carnivore> _carnivoreIterationBuffer = new();

        /// <summary>
        /// Multiplier applied to the base herbivore reproduction chance.
        /// Allows the simulation engine to apply gentle population pressure.
        /// </summary>
        public double HerbivoreReproductionChanceMultiplier { get; set; } = 1.0;

        /// <summary>
        /// Multiplier applied to the base carnivore reproduction chance.
        /// Allows the simulation engine to apply gentle population pressure.
        /// </summary>
        public double CarnivoreReproductionChanceMultiplier { get; set; } = 1.0;

        public ReproductionManager(World world) : this(world, EventSystem.Instance, random: null) { }

        public ReproductionManager(World world, EventSystem eventSystem) : this(world, eventSystem, random: null) { }

        public ReproductionManager(World world, EventSystem eventSystem, Random? random)
        {
            _world = world;
            _eventSystem = eventSystem;
            _random = random ?? new Random();
            _collisionDetector = new CollisionDetector();
            _reproductionCooldowns = new Dictionary<int, double>();
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
            _expiredCooldownIdsBuffer.Clear();
            foreach (var kvp in _reproductionCooldowns)
            {
                _reproductionCooldowns[kvp.Key] = kvp.Value - deltaTime;
                if (_reproductionCooldowns[kvp.Key] <= 0)
                    _expiredCooldownIdsBuffer.Add(kvp.Key);
            }
            foreach (var id in _expiredCooldownIdsBuffer)
                _reproductionCooldowns.Remove(id);
        }

        private void TryReproduceHerbivores()
        {
            if (_world.Herbivores.Count >= MaxHerbivores) return;

            double chance = Math.Clamp(BaseReproductionChance * HerbivoreReproductionChanceMultiplier, 0.0, 1.0);
            _herbivoreIterationBuffer.Clear();
            _herbivoreIterationBuffer.AddRange(_world.Herbivores);

            foreach (var herbivore in _herbivoreIterationBuffer)
            {
                if (!CanReproduce(herbivore)) continue;

                var mate = FindMate(herbivore, _world.Herbivores);
                if (mate != null && _random.NextDouble() < chance)
                {
                    var offspring = CreateOffspring(herbivore, mate);
                    if (offspring != null)
                    {
                        _world.AddHerbivore((Herbivore)offspring);
                        ApplyReproductionCost(herbivore);
                        ApplyReproductionCost(mate);
                        SetCooldown(herbivore);
                        SetCooldown(mate);
                        _eventSystem.OnEntityBorn(offspring);
                        _eventSystem.OnEntityReproduced(herbivore, mate, offspring);
                    }
                }
            }
        }

        private void TryReproduceCarnivores()
        {
            if (_world.Carnivores.Count >= MaxCarnivores) return;

            double chance = Math.Clamp(BaseReproductionChance * CarnivoreReproductionChanceMultiplier, 0.0, 1.0);
            _carnivoreIterationBuffer.Clear();
            _carnivoreIterationBuffer.AddRange(_world.Carnivores);

            foreach (var carnivore in _carnivoreIterationBuffer)
            {
                if (!CanReproduce(carnivore)) continue;

                var mate = FindMate(carnivore, _world.Carnivores);
                if (mate != null && _random.NextDouble() < chance)
                {
                    var offspring = CreateOffspring(carnivore, mate);
                    if (offspring != null)
                    {
                        _world.AddCarnivore((Carnivore)offspring);
                        ApplyReproductionCost(carnivore);
                        ApplyReproductionCost(mate);
                        SetCooldown(carnivore);
                        SetCooldown(mate);
                        _eventSystem.OnEntityBorn(offspring);
                        _eventSystem.OnEntityReproduced(carnivore, mate, offspring);
                    }
                }
            }
        }

        public bool CanReproduce(Creature creature) =>
            creature.IsAlive &&
            creature.Health >= MinHealthForReproduction &&
            creature.Hunger <= MaxHungerForReproduction &&
            creature.Age >= MinAgeForReproduction &&
            !_reproductionCooldowns.ContainsKey(creature.Id);

        private T? FindMate<T>(T creature, IEnumerable<T> potentialMates) where T : Creature
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
            double angle = _random.NextDouble() * Math.PI * 2;
            double offsetX = Math.Cos(angle) * OffspringSpawnRadius * _random.NextDouble();
            double offsetY = Math.Sin(angle) * OffspringSpawnRadius * _random.NextDouble();
            double spawnX = Math.Clamp(midX + offsetX, 0, _world.Width);
            double spawnY = Math.Clamp(midY + offsetY, 0, _world.Height);

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

        private void SetCooldown(Creature creature) => _reproductionCooldowns[creature.Id] = ReproductionCooldown;

        public void ClearCooldown(int creatureId) => _reproductionCooldowns.Remove(creatureId);
    }
}
