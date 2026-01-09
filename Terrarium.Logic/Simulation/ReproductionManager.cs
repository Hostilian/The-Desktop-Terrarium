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

        public ReproductionManager(World world)
        {
            _world = world;
            _random = new Random();
            _collisionDetector = new CollisionDetector();
            _reproductionCooldowns = new Dictionary<int, double>();
        }

        /// <summary>
        /// Updates reproduction logic.
        /// </summary>
        public void Update(double deltaTime)
        {
            // Update cooldowns
            UpdateCooldowns(deltaTime);

            // Try reproduction for herbivores
            TryReproduceHerbivores();

            // Try reproduction for carnivores
            TryReproduceCarnivores();
        }

        /// <summary>
        /// Updates reproduction cooldowns.
        /// </summary>
        private void UpdateCooldowns(double deltaTime)
        {
            var expiredCooldowns = new List<int>();

            foreach (var kvp in _reproductionCooldowns)
            {
                _reproductionCooldowns[kvp.Key] = kvp.Value - deltaTime;
                if (_reproductionCooldowns[kvp.Key] <= 0)
                {
                    expiredCooldowns.Add(kvp.Key);
                }
            }

            foreach (var id in expiredCooldowns)
            {
                _reproductionCooldowns.Remove(id);
            }
        }

        /// <summary>
        /// Attempts reproduction for all eligible herbivores.
        /// </summary>
        private void TryReproduceHerbivores()
        {
            if (_world.Herbivores.Count >= MaxHerbivores) return;

            foreach (var herbivore in _world.Herbivores.ToList())
            {
                if (!CanReproduce(herbivore)) continue;

                var mate = FindMate(herbivore, _world.Herbivores);
                if (mate != null && _random.NextDouble() < BaseReproductionChance)
                {
                    var offspring = CreateOffspring(herbivore, mate);
                    if (offspring != null)
                    {
                        _world.AddHerbivore((Herbivore)offspring);
                        ApplyReproductionCost(herbivore);
                        ApplyReproductionCost(mate);
                        SetCooldown(herbivore);
                        SetCooldown(mate);

                        EventSystem.Instance.OnEntityBorn(offspring);
                        EventSystem.Instance.OnEntityReproduced(herbivore, mate, offspring);
                    }
                }
            }
        }

        /// <summary>
        /// Attempts reproduction for all eligible carnivores.
        /// </summary>
        private void TryReproduceCarnivores()
        {
            if (_world.Carnivores.Count >= MaxCarnivores) return;

            foreach (var carnivore in _world.Carnivores.ToList())
            {
                if (!CanReproduce(carnivore)) continue;

                var mate = FindMate(carnivore, _world.Carnivores);
                if (mate != null && _random.NextDouble() < BaseReproductionChance)
                {
                    var offspring = CreateOffspring(carnivore, mate);
                    if (offspring != null)
                    {
                        _world.AddCarnivore((Carnivore)offspring);
                        ApplyReproductionCost(carnivore);
                        ApplyReproductionCost(mate);
                        SetCooldown(carnivore);
                        SetCooldown(mate);

                        EventSystem.Instance.OnEntityBorn(offspring);
                        EventSystem.Instance.OnEntityReproduced(carnivore, mate, offspring);
                    }
                }
            }
        }

        /// <summary>
        /// Checks if a creature can reproduce.
        /// </summary>
        public bool CanReproduce(Creature creature)
        {
            if (!creature.IsAlive) return false;
            if (creature.Health < MinHealthForReproduction) return false;
            if (creature.Hunger > MaxHungerForReproduction) return false;
            if (creature.Age < MinAgeForReproduction) return false;
            if (_reproductionCooldowns.ContainsKey(creature.Id)) return false;

            return true;
        }

        /// <summary>
        /// Finds a suitable mate nearby.
        /// </summary>
        private T? FindMate<T>(T creature, IEnumerable<T> potentialMates) where T : Creature
        {
            foreach (var potential in potentialMates)
            {
                if (potential.Id == creature.Id) continue;
                if (!CanReproduce(potential)) continue;
                if (creature.DistanceTo(potential) > MatingRange) continue;

                return potential;
            }

            return null;
        }

        /// <summary>
        /// Creates offspring from two parents.
        /// </summary>
        private Creature? CreateOffspring(Creature parent1, Creature parent2)
        {
            // Calculate spawn position between parents
            double midX = (parent1.X + parent2.X) / 2;
            double midY = (parent1.Y + parent2.Y) / 2;

            // Add random offset
            double angle = _random.NextDouble() * Math.PI * 2;
            double offsetX = Math.Cos(angle) * OffspringSpawnRadius * _random.NextDouble();
            double offsetY = Math.Sin(angle) * OffspringSpawnRadius * _random.NextDouble();

            double spawnX = Math.Clamp(midX + offsetX, 0, _world.Width);
            double spawnY = Math.Clamp(midY + offsetY, 0, _world.Height);

            // Create offspring of the same type as parents
            if (parent1 is Herbivore h1)
            {
                return new Herbivore(spawnX, spawnY, h1.Type);
            }
            else if (parent1 is Carnivore c1)
            {
                return new Carnivore(spawnX, spawnY, c1.Type);
            }

            return null;
        }

        /// <summary>
        /// Applies the energy cost of reproduction.
        /// </summary>
        private void ApplyReproductionCost(Creature creature)
        {
            creature.TakeDamage(ReproductionHealthCost);
            creature.Feed(-ReproductionHungerCost); // Negative feed = increase hunger
        }

        /// <summary>
        /// Sets reproduction cooldown for a creature.
        /// </summary>
        private void SetCooldown(Creature creature)
        {
            _reproductionCooldowns[creature.Id] = ReproductionCooldown;
        }

        /// <summary>
        /// Clears cooldown for a creature (for testing).
        /// </summary>
        public void ClearCooldown(int creatureId)
        {
            _reproductionCooldowns.Remove(creatureId);
        }
    }
}
