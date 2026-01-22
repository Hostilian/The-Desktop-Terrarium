namespace Terrarium.Logic.Simulation
{
    using System.Collections.Generic;
    using System.Linq;
    using Terrarium.Logic.Entities;

    /// <summary>
    /// Manages factions and their relationships in the god simulator.
    /// </summary>
    public class FactionManager
    {
        private readonly Dictionary<FactionType, Faction> factions = new();

        /// <summary>
        /// Gets all factions in the simulation.
        /// </summary>
        public IReadOnlyDictionary<FactionType, Faction> Factions => factions;

        public FactionManager()
        {
            // Initialize all factions
            foreach (FactionType factionType in System.Enum.GetValues(typeof(FactionType)))
            {
                factions[factionType] = new Faction(factionType);
            }
        }

        /// <summary>
        /// Gets a faction by type.
        /// </summary>
        /// <returns></returns>
        public Faction GetFaction(FactionType type)
        {
            return factions[type];
        }

        /// <summary>
        /// Updates faction populations based on current entities.
        /// </summary>
        public void UpdatePopulations(IEnumerable<Creature> creatures)
        {
            // Reset populations
            foreach (var faction in factions.Values)
            {
                faction.Population = 0;
            }

            // Count current populations
            foreach (var creature in creatures)
            {
                if (factions.ContainsKey(creature.Faction))
                {
                    factions[creature.Faction].Population++;
                }
            }
        }

        /// <summary>
        /// Gets the dominant faction (highest population).
        /// </summary>
        /// <returns></returns>
        public Faction? GetDominantFaction()
        {
            return factions.Values.OrderByDescending(f => f.Population).FirstOrDefault();
        }

        /// <summary>
        /// Gets factions sorted by population.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Faction> GetFactionsByPopulation()
        {
            return factions.Values.OrderByDescending(f => f.Population);
        }

        /// <summary>
        /// Checks if two factions are hostile.
        /// </summary>
        /// <returns></returns>
        public bool AreHostile(FactionType faction1, FactionType faction2)
        {
            if (faction1 == faction2) return false;
            var f1 = GetFaction(faction1);
            var f2 = GetFaction(faction2);
            return f1.Relationships[faction2] < -50 || f2.Relationships[faction1] < -50;
        }

        /// <summary>
        /// Checks if two factions are allied.
        /// </summary>
        /// <returns></returns>
        public bool AreAllied(FactionType faction1, FactionType faction2)
        {
            if (faction1 == faction2) return true;
            var f1 = GetFaction(faction1);
            var f2 = GetFaction(faction2);
            return f1.Relationships[faction2] > 50 && f2.Relationships[faction1] > 50;
        }
    }
}
