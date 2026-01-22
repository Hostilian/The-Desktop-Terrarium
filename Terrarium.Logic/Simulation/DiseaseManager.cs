namespace Terrarium.Logic.Simulation
{
    using Terrarium.Logic.Entities;

    /// <summary>
    /// Tracks and applies a lightweight disease system to creatures.
    /// Intended to add occasional ecosystem pressure without dominating gameplay.
    /// </summary>
    public class DiseaseManager
    {
        private readonly Random random;

        private readonly Dictionary<int, InfectionState> infections = new();

        private readonly List<Creature> creaturesBuffer = new();
        private readonly HashSet<int> aliveCreatureIdsBuffer = new();
        private readonly List<int> infectionIdsToRemoveBuffer = new();
        private readonly List<int> infectionIdsToRecoverBuffer = new();

        // Tuning (settable for balancing and tests)
        public double SeedInfectionChancePerSecond { get; set; } = 0.003; // ~0.3% per second when no infections

        public double SpreadChancePerSecond { get; set; } = 0.08;

        public double SpreadRadius { get; set; } = 40.0;

        public double InfectionDurationSeconds { get; set; } = 45.0;

        public double HungerIncreasePerSecond { get; set; } = 1.2;

        public double DamagePerSecond { get; set; } = 0.35;

        public int InfectedCount => infections.Count;

        public DiseaseManager(Random? random = null)
        {
            this.random = random ?? new Random();
        }

        public bool IsInfected(int creatureId)
        {
            return infections.ContainsKey(creatureId);
        }

        public void Update(World world, double deltaTime)
        {
            if (world == null)
                throw new ArgumentNullException(nameof(world));

            if (deltaTime <= 0)
                return;

            // Gather alive creatures once.
            creaturesBuffer.Clear();
            foreach (var h in world.Herbivores)
            {
                if (h.IsAlive)
                    creaturesBuffer.Add(h);
            }
            foreach (var c in world.Carnivores)
            {
                if (c.IsAlive)
                    creaturesBuffer.Add(c);
            }

            if (creaturesBuffer.Count == 0)
            {
                infections.Clear();
                return;
            }

            CleanupDeadOrMissing(creaturesBuffer);

            if (infections.Count == 0)
            {
                TrySeedInfection(creaturesBuffer, deltaTime);
            }

            ApplyEffectsAndProgress(creaturesBuffer, deltaTime);
            TrySpread(creaturesBuffer, deltaTime);
        }

        private void CleanupDeadOrMissing(List<Creature> aliveCreatures)
        {
            if (infections.Count == 0)
                return;

            aliveCreatureIdsBuffer.Clear();
            foreach (var c in aliveCreatures)
                aliveCreatureIdsBuffer.Add(c.Id);

            infectionIdsToRemoveBuffer.Clear();
            foreach (var id in infections.Keys)
            {
                if (!aliveCreatureIdsBuffer.Contains(id))
                    infectionIdsToRemoveBuffer.Add(id);
            }

            foreach (var id in infectionIdsToRemoveBuffer)
                infections.Remove(id);
        }

        private void TrySeedInfection(List<Creature> creatures, double deltaTime)
        {
            if (creatures.Count < 3)
                return;

            double probability = 1.0 - Math.Pow(1.0 - Math.Clamp(SeedInfectionChancePerSecond, 0.0, 1.0), deltaTime);
            if (random.NextDouble() < probability)
            {
                var chosen = creatures[random.Next(creatures.Count)];
                Infect(chosen.Id);
            }
        }

        private void ApplyEffectsAndProgress(List<Creature> creatures, double deltaTime)
        {
            if (infections.Count == 0)
                return;

            infectionIdsToRecoverBuffer.Clear();

            foreach (var creature in creatures)
            {
                if (!infections.TryGetValue(creature.Id, out var state))
                    continue;

                state.ElapsedSeconds += deltaTime;

                // Effects
                creature.TakeDamage(DamagePerSecond * deltaTime);
                creature.Feed(-HungerIncreasePerSecond * deltaTime);

                if (state.ElapsedSeconds >= InfectionDurationSeconds)
                {
                    infectionIdsToRecoverBuffer.Add(creature.Id);
                }
                else
                {
                    infections[creature.Id] = state;
                }
            }

            foreach (var id in infectionIdsToRecoverBuffer)
                infections.Remove(id);
        }

        private void TrySpread(List<Creature> creatures, double deltaTime)
        {
            if (infections.Count == 0)
                return;

            double probability = 1.0 - Math.Pow(1.0 - Math.Clamp(SpreadChancePerSecond, 0.0, 1.0), deltaTime);
            if (probability <= 0)
                return;

            // For each infected creature, attempt to infect one nearby creature per tick.
            foreach (var infected in creatures)
            {
                if (!infections.ContainsKey(infected.Id))
                    continue;

                if (random.NextDouble() >= probability)
                    continue;

                Creature? closest = null;
                double closestDistance = double.MaxValue;

                foreach (var candidate in creatures)
                {
                    if (candidate.Id == infected.Id)
                        continue;
                    if (infections.ContainsKey(candidate.Id))
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
            if (infections.ContainsKey(creatureId))
                return;

            infections[creatureId] = new InfectionState { ElapsedSeconds = 0 };
        }

        private struct InfectionState
        {
            public double ElapsedSeconds;
        }
    }
}
