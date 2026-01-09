using Terrarium.Logic.Entities;

namespace Terrarium.Logic.Simulation
{
    /// <summary>
    /// Tracks and applies a lightweight disease system to creatures.
    /// Intended to add occasional ecosystem pressure without dominating gameplay.
    /// </summary>
    public class DiseaseManager
    {
        private readonly Random _random;

        private readonly Dictionary<int, InfectionState> _infections = new();

        // Tuning (settable for balancing and tests)
        public double SeedInfectionChancePerSecond { get; set; } = 0.003; // ~0.3% per second when no infections
        public double SpreadChancePerSecond { get; set; } = 0.08;
        public double SpreadRadius { get; set; } = 40.0;
        public double InfectionDurationSeconds { get; set; } = 45.0;

        public double HungerIncreasePerSecond { get; set; } = 1.2;
        public double DamagePerSecond { get; set; } = 0.35;

        public int InfectedCount => _infections.Count;

        public DiseaseManager(Random? random = null)
        {
            _random = random ?? new Random();
        }

        public bool IsInfected(int creatureId)
        {
            return _infections.ContainsKey(creatureId);
        }

        public void Update(World world, double deltaTime)
        {
            if (world == null)
                throw new ArgumentNullException(nameof(world));

            if (deltaTime <= 0)
                return;

            // Gather alive creatures once.
            var creatures = new List<Creature>(world.Herbivores.Count + world.Carnivores.Count);
            foreach (var h in world.Herbivores)
            {
                if (h.IsAlive)
                    creatures.Add(h);
            }
            foreach (var c in world.Carnivores)
            {
                if (c.IsAlive)
                    creatures.Add(c);
            }

            if (creatures.Count == 0)
            {
                _infections.Clear();
                return;
            }

            CleanupDeadOrMissing(creatures);

            if (_infections.Count == 0)
            {
                TrySeedInfection(creatures, deltaTime);
            }

            ApplyEffectsAndProgress(creatures, deltaTime);
            TrySpread(creatures, deltaTime);
        }

        private void CleanupDeadOrMissing(List<Creature> aliveCreatures)
        {
            if (_infections.Count == 0)
                return;

            var aliveIds = new HashSet<int>();
            foreach (var c in aliveCreatures)
                aliveIds.Add(c.Id);

            var toRemove = new List<int>();
            foreach (var id in _infections.Keys)
            {
                if (!aliveIds.Contains(id))
                    toRemove.Add(id);
            }

            foreach (var id in toRemove)
                _infections.Remove(id);
        }

        private void TrySeedInfection(List<Creature> creatures, double deltaTime)
        {
            if (creatures.Count < 3)
                return;

            double probability = 1.0 - Math.Pow(1.0 - Math.Clamp(SeedInfectionChancePerSecond, 0.0, 1.0), deltaTime);
            if (_random.NextDouble() < probability)
            {
                var chosen = creatures[_random.Next(creatures.Count)];
                Infect(chosen.Id);
            }
        }

        private void ApplyEffectsAndProgress(List<Creature> creatures, double deltaTime)
        {
            if (_infections.Count == 0)
                return;

            var toRecover = new List<int>();

            foreach (var creature in creatures)
            {
                if (!_infections.TryGetValue(creature.Id, out var state))
                    continue;

                state.ElapsedSeconds += deltaTime;

                // Effects
                creature.TakeDamage(DamagePerSecond * deltaTime);
                creature.Feed(-HungerIncreasePerSecond * deltaTime);

                if (state.ElapsedSeconds >= InfectionDurationSeconds)
                {
                    toRecover.Add(creature.Id);
                }
                else
                {
                    _infections[creature.Id] = state;
                }
            }

            foreach (var id in toRecover)
                _infections.Remove(id);
        }

        private void TrySpread(List<Creature> creatures, double deltaTime)
        {
            if (_infections.Count == 0)
                return;

            double probability = 1.0 - Math.Pow(1.0 - Math.Clamp(SpreadChancePerSecond, 0.0, 1.0), deltaTime);
            if (probability <= 0)
                return;

            // For each infected creature, attempt to infect one nearby creature per tick.
            foreach (var infected in creatures)
            {
                if (!_infections.ContainsKey(infected.Id))
                    continue;

                if (_random.NextDouble() >= probability)
                    continue;

                Creature? closest = null;
                double closestDistance = double.MaxValue;

                foreach (var candidate in creatures)
                {
                    if (candidate.Id == infected.Id)
                        continue;
                    if (_infections.ContainsKey(candidate.Id))
                        continue;

                    double d = infected.DistanceTo(candidate);
                    if (d <= SpreadRadius && d < closestDistance)
                    {
                        closestDistance = d;
                        closest = candidate;
                    }
                }

                if (closest != null)
                {
                    Infect(closest.Id);
                }
            }
        }

        private void Infect(int creatureId)
        {
            if (_infections.ContainsKey(creatureId))
                return;

            _infections[creatureId] = new InfectionState { ElapsedSeconds = 0 };
        }

        private struct InfectionState
        {
            public double ElapsedSeconds;
        }
    }
}
