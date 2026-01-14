using System.Collections.Generic;
using System.Linq;
using Terrarium.Logic.Entities;

namespace Terrarium.Logic.Simulation
{
    /// <summary>
    /// Manages factions and their relationships in the god simulator.
    /// </summary>
    public class FactionManager
    {
        private readonly Dictionary<FactionType, Faction> _factions = new();

        /// <summary>
        /// All factions in the simulation.
        /// </summary>
        public IReadOnlyDictionary<FactionType, Faction> Factions => _factions;

        public FactionManager()
        {
            // Initialize all factions
            foreach (FactionType factionType in System.Enum.GetValues(typeof(FactionType)))
            {
                _factions[factionType] = new Faction(factionType);
            }
        }

        /// <summary>
        /// Gets a faction by type.
        /// </summary>
        public Faction GetFaction(FactionType type)
        {
            return _factions[type];
        }

        /// <summary>
        /// Updates faction populations based on current entities.
        /// </summary>
        public void UpdatePopulations(IEnumerable<Creature> creatures)
        {
            // Reset populations
            foreach (var faction in _factions.Values)
            {
                faction.Population = 0;
            }

            // Count current populations
            foreach (var creature in creatures)
            {
                if (_factions.ContainsKey(creature.Faction))
                {
                    _factions[creature.Faction].Population++;
                }
            }
        }

        /// <summary>
        /// Gets the dominant faction (highest population).
        /// </summary>
        public Faction? GetDominantFaction()
        {
            return _factions.Values.OrderByDescending(f => f.Population).FirstOrDefault();
        }

        /// <summary>
        /// Gets factions sorted by population.
        /// </summary>
        public IEnumerable<Faction> GetFactionsByPopulation()
        {
            return _factions.Values.OrderByDescending(f => f.Population);
        }

        /// <summary>
        /// Checks if two factions are hostile.
        /// </summary>
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
        public bool AreAllied(FactionType faction1, FactionType faction2)
        {
            if (faction1 == faction2) return true;
            var f1 = GetFaction(faction1);
            var f2 = GetFaction(faction2);
            return f1.Relationships[faction2] > 50 && f2.Relationships[faction1] > 50;
        }
    }
}
