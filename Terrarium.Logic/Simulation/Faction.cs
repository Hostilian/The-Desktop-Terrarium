using System;
using System.Collections.Generic;

namespace Terrarium.Logic.Simulation
{
    /// <summary>
    /// Represents the different factions in the god simulator.
    /// Each faction has unique abilities, visuals, and behaviors.
    /// </summary>
    public enum FactionType
    {
        /// <summary>
        /// The Verdant Collective - Plant-based faction focused on growth and defense.
        /// </summary>
        VerdantCollective,

        /// <summary>
        /// The Ashen Legion - Fire-themed aggressive faction.
        /// </summary>
        AshenLegion,

        /// <summary>
        /// The Crystal Choir - Defensive crystalline faction.
        /// </summary>
        CrystalChoir,

        /// <summary>
        /// The Tide Walkers - Aquatic amphibious faction.
        /// </summary>
        TideWalkers,

        /// <summary>
        /// The Scrapborn Swarm - Mechanical adaptive faction.
        /// </summary>
        ScrapbornSwarm,

        /// <summary>
        /// The Nomadic Covenant - Mobile trading faction.
        /// </summary>
        NomadicCovenant
    }

    /// <summary>
    /// Represents a faction with its properties and behaviors.
    /// </summary>
    public class Faction
    {
        /// <summary>
        /// The type of this faction.
        /// </summary>
        public FactionType Type { get; }

        /// <summary>
        /// Display name of the faction.
        /// </summary>
        public string? Name { get; }

        /// <summary>
        /// Color associated with this faction.
        /// </summary>
        public string? Color { get; }

        /// <summary>
        /// Current population count.
        /// </summary>
        public int Population { get; set; }

        /// <summary>
        /// Territory control percentage (0-100).
        /// </summary>
        public double TerritoryControl { get; set; }

        /// <summary>
        /// Resources owned by this faction.
        /// </summary>
        public Dictionary<string, int> Resources { get; } = new();

        /// <summary>
        /// Relationships with other factions (-100 to 100).
        /// </summary>
        public Dictionary<FactionType, int> Relationships { get; } = new();

        /// <summary>
        /// Special abilities of this faction.
        /// </summary>
        public List<string> Abilities { get; } = new();

        /// <summary>
        /// Lore and backstory of the faction.
        /// </summary>
        public string? Lore { get; }

        public Faction(FactionType type)
        {
            Type = type;

            switch (type)
            {
                case FactionType.VerdantCollective:
                    Name = "Verdant Collective";
                    Color = "#22AA22";
                    Abilities.AddRange(new[] { "Photosynthesis", "Root Network", "Defensive Thorns" });
                    Lore = "Ancient forest consciousness split into individual minds, seeking reunification through growth.";
                    break;

                case FactionType.AshenLegion:
                    Name = "Ashen Legion";
                    Color = "#AA2222";
                    Abilities.AddRange(new[] { "Forge Economy", "Aggressive Expansion", "Fire Immunity" });
                    Lore = "Warrior-smiths from volcanic homeland, believing strength proves worth through conquest.";
                    break;

                case FactionType.CrystalChoir:
                    Name = "Crystal Choir";
                    Color = "#AA22AA";
                    Abilities.AddRange(new[] { "Resonance", "Defensive Harmony", "Crystal Growth" });
                    Lore = "Sentient crystals that sing in harmonic frequencies, seeking perfect unity.";
                    break;

                case FactionType.TideWalkers:
                    Name = "Tide Walkers";
                    Color = "#2244AA";
                    Abilities.AddRange(new[] { "Amphibious", "Flood Control", "Water Healing" });
                    Lore = "Former sailors transformed by ancient ocean deity, seeking to claim all coasts.";
                    break;

                case FactionType.ScrapbornSwarm:
                    Name = "Scrapborn Swarm";
                    Color = "#AA8822";
                    Abilities.AddRange(new[] { "Adaptive Evolution", "Salvage", "Swarm Tactics" });
                    Lore = "Artificial life created as servants, now seeking to prove superiority through adaptation.";
                    break;

                case FactionType.NomadicCovenant:
                    Name = "Nomadic Covenant";
                    Color = "#AAAA22";
                    Abilities.AddRange(new[] { "Migration Bonus", "Trade Networks", "Diplomatic Immunity" });
                    Lore = "Prophetic pilgrims seeking the 'Promised Land' marked in ancient visions.";
                    break;
            }

            // Initialize default relationships
            foreach (FactionType factionType in Enum.GetValues(typeof(FactionType)))
            {
                if (factionType != type)
                {
                    Relationships[factionType] = 0; // Neutral by default
                }
            }

            // Set specific relationships
            InitializeRelationships();
        }

        private void InitializeRelationships()
        {
            switch (Type)
            {
                case FactionType.VerdantCollective:
                    Relationships[FactionType.AshenLegion] = -80; // Hostile
                    Relationships[FactionType.CrystalChoir] = 20; // Friendly
                    break;

                case FactionType.AshenLegion:
                    Relationships[FactionType.VerdantCollective] = -80; // Hostile
                    Relationships[FactionType.ScrapbornSwarm] = -30; // Competitive
                    break;

                case FactionType.CrystalChoir:
                    // Generally diplomatic
                    Relationships[FactionType.VerdantCollective] = 20;
                    Relationships[FactionType.TideWalkers] = 15;
                    Relationships[FactionType.NomadicCovenant] = 25;
                    break;

                case FactionType.TideWalkers:
                    Relationships[FactionType.CrystalChoir] = 15;
                    Relationships[FactionType.NomadicCovenant] = 10;
                    break;

                case FactionType.ScrapbornSwarm:
                    // Opportunistic - negative relationships with everyone
                    foreach (var kvp in Relationships.ToList())
                    {
                        Relationships[kvp.Key] = -20;
                    }
                    break;

                case FactionType.NomadicCovenant:
                    // Generally positive trading relationships
                    foreach (var kvp in Relationships.ToList())
                    {
                        Relationships[kvp.Key] = 15;
                    }
                    break;
            }
        }

        /// <summary>
        /// Gets the relationship status with another faction.
        /// </summary>
        public string GetRelationshipStatus(FactionType otherFaction)
        {
            if (!Relationships.TryGetValue(otherFaction, out int value))
                return "Unknown";

            if (value >= 50) return "Allied";
            if (value >= 20) return "Friendly";
            if (value >= -19) return "Neutral";
            if (value >= -49) return "Unfriendly";
            return "Hostile";
        }

        /// <summary>
        /// Adds resources to this faction.
        /// </summary>
        public void AddResource(string resourceType, int amount)
        {
            if (!Resources.ContainsKey(resourceType))
                Resources[resourceType] = 0;
            Resources[resourceType] += amount;
        }

        /// <summary>
        /// Consumes resources from this faction.
        /// </summary>
        public bool ConsumeResource(string resourceType, int amount)
        {
            if (!Resources.ContainsKey(resourceType) || Resources[resourceType] < amount)
                return false;

            Resources[resourceType] -= amount;
            return true;
        }
    }
}
