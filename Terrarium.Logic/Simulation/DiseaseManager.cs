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

        private readonly List<Creature> _creaturesBuffer = new();
        private readonly HashSet<int> _aliveCreatureIdsBuffer = new();
        private readonly List<int> _infectionIdsToRemoveBuffer = new();
        private readonly List<int> _infectionIdsToRecoverBuffer = new();

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
            _creaturesBuffer.Clear();
            foreach (var h in world.Herbivores)
            {
                if (h.IsAlive)
                    _creaturesBuffer.Add(h);
            }
            foreach (var c in world.Carnivores)
            {
                if (c.IsAlive)
                    _creaturesBuffer.Add(c);
            }

            if (_creaturesBuffer.Count == 0)
            {
                _infections.Clear();
                return;
            }

            CleanupDeadOrMissing(_creaturesBuffer);

            if (_infections.Count == 0)
            {
                TrySeedInfection(_creaturesBuffer, deltaTime);
            }

            ApplyEffectsAndProgress(_creaturesBuffer, deltaTime);
            TrySpread(_creaturesBuffer, deltaTime);
        }

        private void CleanupDeadOrMissing(List<Creature> aliveCreatures)
        {
            if (_infections.Count == 0)
                return;

            _aliveCreatureIdsBuffer.Clear();
            foreach (var c in aliveCreatures)
                _aliveCreatureIdsBuffer.Add(c.Id);

            _infectionIdsToRemoveBuffer.Clear();
            foreach (var id in _infections.Keys)
            {
                if (!_aliveCreatureIdsBuffer.Contains(id))
                    _infectionIdsToRemoveBuffer.Add(id);
            }

            foreach (var id in _infectionIdsToRemoveBuffer)
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

            _infectionIdsToRecoverBuffer.Clear();

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
                    _infectionIdsToRecoverBuffer.Add(creature.Id);
                }
                else
                {
                    _infections[creature.Id] = state;
                }
            }

            foreach (var id in _infectionIdsToRecoverBuffer)
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
