using System;
using System.Collections.Generic;

namespace Terrarium.Logic.Simulation
{
    /// <summary>
    /// Represents different terrain types in the cellular automata world.
    /// Each terrain type has properties that affect gameplay and faction interactions.
    /// </summary>
    public enum TerrainType
    {
        // Neutral/Base terrains
        Void = 0,       // Empty space, cannot be occupied
        Soil = 1,       // Basic fertile ground, allows all life
        Stone = 2,      // Rocky terrain, hard to terraform but provides defense
        Water = 3,      // Liquid terrain, allows aquatic life but hinders land creatures

        // Faction-controlled terrains (converted from base types)
        VerdantGrowth = 10,    // Verdant Collective - fertile, spreading plant life
        AshenWasteland = 11,   // Ashen Legion - burned, toxic land
        AquaticDomain = 12,    // Aquatic Domain - flooded, aquatic zones
        StoneWardens = 13,     // Stone Wardens - rocky fortifications
        CelestialOrder = 14,   // Celestial Order - purified holy ground
        NetherCult = 15        // Nether Cult - corrupted, twisted terrain
    }

    /// <summary>
    /// Properties and behaviors for each terrain type.
    /// </summary>
    public static class TerrainProperties
    {
        private static readonly Dictionary<TerrainType, TerrainData> _properties = new()
        {
            [TerrainType.Void] = new TerrainData {
                Name = "Void", Color = "#000000", Walkable = false, Fertility = 0,
                Description = "Empty nothingness, the absence of creation itself."
            },
            [TerrainType.Soil] = new TerrainData {
                Name = "Soil", Color = "#8B4513", Walkable = true, Fertility = 100,
                Description = "Rich, fertile earth teeming with potential."
            },
            [TerrainType.Stone] = new TerrainData {
                Name = "Stone", Color = "#696969", Walkable = true, Fertility = 10,
                Description = "Ancient rock, unyielding and eternal."
            },
            [TerrainType.Water] = new TerrainData {
                Name = "Water", Color = "#4169E1", Walkable = false, Fertility = 30,
                Description = "Life-giving fluid, both nurturing and destructive."
            },
            [TerrainType.VerdantGrowth] = new TerrainData {
                Name = "Verdant Growth", Color = "#228B22", Walkable = true, Fertility = 150,
                Description = "Living vegetation that spreads relentlessly, claiming territory for the Verdant Collective."
            },
            [TerrainType.AshenWasteland] = new TerrainData {
                Name = "Ashen Wasteland", Color = "#2F2F2F", Walkable = true, Fertility = 5,
                Description = "Burned earth poisoned by the Ashen Legion's eternal flames."
            },
            [TerrainType.AquaticDomain] = new TerrainData {
                Name = "Aquatic Domain", Color = "#00CED1", Walkable = false, Fertility = 80,
                Description = "Flooded territories claimed by the Aquatic Domain's endless waters."
            },
            [TerrainType.StoneWardens] = new TerrainData {
                Name = "Stone Wardens", Color = "#696969", Walkable = true, Fertility = 15,
                Description = "Fortified stone formations raised by the Stone Wardens."
            },
            [TerrainType.CelestialOrder] = new TerrainData {
                Name = "Celestial Order", Color = "#FFD700", Walkable = true, Fertility = 60,
                Description = "Purified ground blessed by the Celestial Order's divine light."
            },
            [TerrainType.NetherCult] = new TerrainData {
                Name = "Nether Cult", Color = "#8B008B", Walkable = true, Fertility = 25,
                Description = "Twisted, corrupted terrain warped by the Nether Cult's dark rituals."
            }
        };

        public static TerrainData GetProperties(TerrainType type) => _properties[type];

        /// <summary>
        /// Gets the faction that controls this terrain type, or null if neutral.
        /// </summary>
        public static FactionType? GetControllingFaction(TerrainType terrain)
        {
            return terrain switch
            {
                TerrainType.VerdantGrowth => FactionType.VerdantCollective,
                TerrainType.AshenWasteland => FactionType.AshenLegion,
                TerrainType.AquaticDomain => FactionType.TideWalkers,
                TerrainType.StoneWardens => FactionType.CrystalChoir,
                TerrainType.CelestialOrder => FactionType.NomadicCovenant,
                TerrainType.NetherCult => FactionType.ScrapbornSwarm,
                _ => null
            };
        }

        /// <summary>
        /// Gets the terrain type associated with a specific faction.
        /// </summary>
        public static TerrainType GetFactionTerrain(FactionType faction)
        {
            return faction switch
            {
                FactionType.VerdantCollective => TerrainType.VerdantGrowth,
                FactionType.AshenLegion => TerrainType.AshenWasteland,
                FactionType.TideWalkers => TerrainType.AquaticDomain,
                FactionType.CrystalChoir => TerrainType.StoneWardens,
                FactionType.NomadicCovenant => TerrainType.CelestialOrder,
                FactionType.ScrapbornSwarm => TerrainType.NetherCult,
                _ => TerrainType.Soil
            };
        }

        /// <summary>
        /// Determines if one terrain type can convert another.
        /// </summary>
        public static bool CanConvert(TerrainType from, TerrainType to, FactionType faction)
        {
            var controllingFaction = GetControllingFaction(to);
            if (controllingFaction.HasValue && controllingFaction.Value != faction)
                return false; // Cannot convert to enemy faction's terrain

            // Specific conversion rules
            return (from, to) switch
            {
                (TerrainType.Soil, TerrainType.VerdantGrowth) when faction == FactionType.VerdantCollective => true,
                (TerrainType.Soil, TerrainType.AshenWasteland) when faction == FactionType.AshenLegion => true,
                (TerrainType.Soil, TerrainType.StoneWardens) when faction == FactionType.CrystalChoir => true,
                (TerrainType.Water, TerrainType.AquaticDomain) when faction == FactionType.TideWalkers => true,
                (TerrainType.Stone, TerrainType.NetherCult) when faction == FactionType.ScrapbornSwarm => true,
                (TerrainType.Soil, TerrainType.CelestialOrder) when faction == FactionType.NomadicCovenant => true,
                _ => false
            };
        }
    }

    /// <summary>
    /// Data structure for terrain properties.
    /// </summary>
    public class TerrainData
    {
        public string Name { get; set; } = "";
        public string Color { get; set; } = "#FFFFFF";
        public bool Walkable { get; set; }
        public int Fertility { get; set; }
        public string Description { get; set; } = "";
    }
}
