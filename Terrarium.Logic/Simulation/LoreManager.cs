using System;
using System.Collections.Generic;
using System.Linq;
using Terrarium.Logic.Entities;
using Terrarium.Logic.Simulation;

namespace Terrarium.Logic.Simulation
{
    /// <summary>
    /// Manages procedural lore generation and chronicle system.
    /// Inspired by Caves of Qud's narrative depth.
    /// </summary>
    public class LoreManager
    {
        private readonly Random _random;
        private readonly List<LoreEvent> _chronicle = new();
        private readonly Dictionary<int, NamedCharacter> _namedCharacters = new();

        // Ancient events that explain the world state
        private readonly List<string> _ancientEvents = new()
        {
            "The Sundering - When the First Root split into many minds, creating the Verdant Collective",
            "The Forge Pact - Treaty signed in cooling lava, now broken by the Ashen Legion",
            "The Crystal Convergence - When harmonic frequencies first united the Crystal Choir",
            "The Great Tide - Oceanic deity that transformed the Tide Walkers",
            "The Scrap Awakening - Artificial servants who gained sentience as the Scrapborn Swarm",
            "The Prophetic Visions - Dreams that guide the Nomadic Covenant in their endless pilgrimage"
        };

        // Event templates for procedural generation
        private readonly List<string> _eventTemplates = new()
        {
            "{0} warriors of the {1} clashed with {2} forces of the {3} at the {4}",
            "A great famine struck the lands, weakening the {1} but strengthening the {3}",
            "The {1} discovered ancient ruins containing {5} technology",
            "{0} heroes emerged from the {1}, leading their people to victory",
            "Diplomatic talks between the {1} and {3} resulted in an uneasy {6}",
            "Natural disaster devastated the {1} territories, causing mass migration",
            "Technological breakthrough by the {1} changed the balance of power",
            "Betrayal within the {3} led to civil war and division"
        };

        private readonly List<string> _battleNames = new()
        {
            "Battle of Crying Stones", "Siege of the Crystal Spire", "Clash at Forge's Heart",
            "Tidal Massacre", "Swarm Incursion", "Prophet's Stand", "Root War", "Ash Storm"
        };

        private readonly List<string> _technologyTypes = new()
        {
            "crystalline", "volcanic", "aquatic", "mechanical", "organic", "mystical"
        };

        private readonly List<string> _diplomacyResults = new()
        {
            "alliance", "truce", "trade agreement", "non-aggression pact", "hostile standoff"
        };

        public LoreManager(Random? random = null)
        {
            _random = random ?? new Random();
            GenerateAncientHistory();
        }

        /// <summary>
        /// The chronicle of events that have occurred.
        /// </summary>
        public IReadOnlyList<LoreEvent> Chronicle => _chronicle.AsReadOnly();

        /// <summary>
        /// Named characters in the world.
        /// </summary>
        public IReadOnlyDictionary<int, NamedCharacter> NamedCharacters => _namedCharacters;

        /// <summary>
        /// Generates the ancient history that explains the current world state.
        /// </summary>
        private void GenerateAncientHistory()
        {
            // Select 3-5 ancient events
            int numEvents = _random.Next(3, 6);
            var selectedEvents = _ancientEvents.OrderBy(x => _random.Next()).Take(numEvents);

            foreach (var ancientEvent in selectedEvents)
            {
                _chronicle.Add(new LoreEvent
                {
                    Timestamp = -_random.Next(100, 1000), // Years ago
                    Description = ancientEvent,
                    Type = LoreEventType.AncientHistory,
                    Importance = LoreImportance.Major
                });
            }
        }

        /// <summary>
        /// Records a significant event in the chronicle.
        /// </summary>
        public void RecordEvent(string description, LoreEventType type, LoreImportance importance = LoreImportance.Minor)
        {
            _chronicle.Add(new LoreEvent
            {
                Timestamp = DateTime.Now.Ticks,
                Description = description,
                Type = type,
                Importance = importance
            });

            // Keep only the most recent 100 events
            if (_chronicle.Count > 100)
            {
                _chronicle.RemoveAt(0);
            }
        }

        /// <summary>
        /// Generates procedural event descriptions.
        /// </summary>
        public string GenerateEventDescription(FactionManager factionManager)
        {
            var factions = factionManager.GetFactionsByPopulation().ToList();
            if (factions.Count < 2) return "Peace reigns as a single faction dominates the land.";

            var faction1 = factions[_random.Next(factions.Count)];
            var faction2 = factions.Where(f => f.Type != faction1.Type).ToList()[_random.Next(Math.Min(2, factions.Count - 1))];

            string template = _eventTemplates[_random.Next(_eventTemplates.Count)];
            string battleName = _battleNames[_random.Next(_battleNames.Count)];
            string technology = _technologyTypes[_random.Next(_technologyTypes.Count)];
            string diplomacy = _diplomacyResults[_random.Next(_diplomacyResults.Count)];

            return string.Format(template,
                _random.Next(10, 100), // number
                faction1.Name,
                _random.Next(5, 50), // number
                faction2.Name,
                battleName,
                technology,
                diplomacy);
        }

        /// <summary>
        /// Potentially creates a named character from a creature.
        /// </summary>
        public void TryCreateNamedCharacter(Creature creature)
        {
            // 1 in 50 chance for a creature to become "named"
            if (_random.Next(50) != 0) return;

            string name = GenerateName(creature.Faction);
            var namedChar = new NamedCharacter
            {
                Id = creature.Id,
                Name = name,
                Faction = creature.Faction,
                Titles = new List<string>(),
                Biography = $"{name} emerged from the {GetFactionAdjective(creature.Faction)} ranks.",
                BirthTime = DateTime.Now,
                IsAlive = true
            };

            _namedCharacters[creature.Id] = namedChar;
            RecordEvent($"{name} has risen to prominence among the {GetFactionName(creature.Faction)}", LoreEventType.CharacterBirth, LoreImportance.Medium);
        }

        /// <summary>
        /// Records the death of a named character.
        /// </summary>
        public void RecordCharacterDeath(int creatureId)
        {
            if (_namedCharacters.TryGetValue(creatureId, out var character))
            {
                character.IsAlive = false;
                character.DeathTime = DateTime.Now;
                RecordEvent($"{character.Name} has fallen in battle", LoreEventType.CharacterDeath, LoreImportance.Major);
            }
        }

        /// <summary>
        /// Gets lore for a specific entity.
        /// </summary>
        public string GetEntityLore(Creature creature)
        {
            if (_namedCharacters.TryGetValue(creature.Id, out var character))
            {
                return $"{character.Name} - {character.Biography}";
            }

            // Generate procedural lore for unnamed creatures
            return GenerateProceduralLore(creature);
        }

        private string GenerateProceduralLore(Creature creature)
        {
            var templates = new[]
            {
                $"A {GetCreatureDescription(creature)} of the {GetFactionName(creature.Faction)}, born in {GetRecentEventContext()}",
                $"Warrior of the {GetFactionName(creature.Faction)}, survivor of the {GetRandomBattleName()}",
                $"Elder {GetCreatureDescription(creature)}, keeper of {GetFactionAdjective(creature.Faction)} traditions",
                $"Scout from the {GetFactionName(creature.Faction)} territories, exploring new lands",
                $"Guardian of the {GetFactionName(creature.Faction)}, protector of their people's future"
            };

            return templates[_random.Next(templates.Length)];
        }

        private string GenerateName(FactionType faction)
        {
            var nameParts = faction switch
            {
                FactionType.VerdantCollective => new[] { "Oak", "Willow", "Root", "Leaf", "Bark", "Thorn" },
                FactionType.AshenLegion => new[] { "Forge", "Ash", "Flame", "Iron", "Steel", "Ember" },
                FactionType.CrystalChoir => new[] { "Crystal", "Harmony", "Resonance", "Echo", "Prism", "Facet" },
                FactionType.TideWalkers => new[] { "Wave", "Tide", "Current", "Storm", "Depth", "Surf" },
                FactionType.ScrapbornSwarm => new[] { "Gear", "Bolt", "Circuit", "Wire", "Scrap", "Rust" },
                FactionType.NomadicCovenant => new[] { "Wander", "Prophet", "Pilgrim", "Seeker", "Vision", "Path" },
                _ => new[] { "Unknown" }
            };

            var titles = new[] { "the Brave", "Bloodied", "Wise", "Fierce", "Ancient", "Young" };

            string baseName = nameParts[_random.Next(nameParts.Length)];
            string title = _random.Next(3) == 0 ? " " + titles[_random.Next(titles.Length)] : "";

            return baseName + title;
        }

        private string GetFactionName(FactionType faction) => faction switch
        {
            FactionType.VerdantCollective => "Verdant Collective",
            FactionType.AshenLegion => "Ashen Legion",
            FactionType.CrystalChoir => "Crystal Choir",
            FactionType.TideWalkers => "Tide Walkers",
            FactionType.ScrapbornSwarm => "Scrapborn Swarm",
            FactionType.NomadicCovenant => "Nomadic Covenant",
            _ => "Unknown Faction"
        };

        private string GetFactionAdjective(FactionType faction) => faction switch
        {
            FactionType.VerdantCollective => "verdant",
            FactionType.AshenLegion => "ashen",
            FactionType.CrystalChoir => "crystalline",
            FactionType.TideWalkers => "tidal",
            FactionType.ScrapbornSwarm => "scrap",
            FactionType.NomadicCovenant => "nomadic",
            _ => "unknown"
        };

        private string GetCreatureDescription(Creature creature)
        {
            return creature switch
            {
                Herbivore => "herbivore",
                Carnivore => "carnivore",
                _ => "creature"
            };
        }

        private string GetRecentEventContext()
        {
            if (_chronicle.Count == 0) return "recent times";
            var recentEvent = _chronicle.Last();
            return recentEvent.Description.Length > 20
                ? recentEvent.Description.Substring(0, 20) + "..."
                : recentEvent.Description;
        }

        private string GetRandomBattleName() => _battleNames[_random.Next(_battleNames.Count)];
    }

    /// <summary>
    /// Represents a lore event in the chronicle.
    /// </summary>
    public class LoreEvent
    {
        public long Timestamp { get; set; }
        public string Description { get; set; } = "";
        public LoreEventType Type { get; set; }
        public LoreImportance Importance { get; set; }
    }

    /// <summary>
    /// Types of lore events.
    /// </summary>
    public enum LoreEventType
    {
        AncientHistory,
        Battle,
        Diplomacy,
        Technology,
        Disaster,
        CharacterBirth,
        CharacterDeath,
        FactionEvent
    }

    /// <summary>
    /// Importance levels for lore events.
    /// </summary>
    public enum LoreImportance
    {
        Minor,
        Medium,
        Major
    }

    /// <summary>
    /// Represents a named character with biography.
    /// </summary>
    public class NamedCharacter
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public FactionType Faction { get; set; }
        public List<string> Titles { get; set; } = new();
        public string Biography { get; set; } = "";
        public DateTime BirthTime { get; set; }
        public DateTime? DeathTime { get; set; }
        public bool IsAlive { get; set; }
    }
}