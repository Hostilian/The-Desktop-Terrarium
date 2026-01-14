# Enhanced Faction System - Implementation Guide

## Overview

This document outlines the implementation of an enhanced faction system that adds depth, strategy, and emergent gameplay to the god simulator. Each faction will have unique traits, abilities, diplomatic relations, and evolutionary potential.

## Faction Architecture

### Core Faction Class

```python
@dataclass
class FactionTraits:
    """Enhanced faction characteristics and abilities."""
    name: str
    base_color: Tuple[int, int, int]
    preferred_terrain: List[TerrainType]
    abilities: List[str]
    weaknesses: List[str]
    technologies: List[str]
    culture_type: str
    religion: str
    diplomatic_bias: Dict[Faction, int]  # Base diplomatic tendencies
    evolution_stage: int = 0
    reputation: Dict[Faction, int] = field(default_factory=dict)
    discovered_artifacts: List[str] = field(default_factory=list)
    historical_events: List[str] = field(default_factory=list)

class EnhancedFaction:
    """Enhanced faction with deep mechanics."""

    def __init__(self, faction_enum: Faction, traits: FactionTraits):
        self.enum = faction_enum
        self.traits = traits
        self.population = 0
        self.territory_count = 0
        self.military_strength = 0
        self.economic_power = 0
        self.cultural_influence = 0
        self.technological_level = 1
        self.morale = 50  # 0-100
        self.loyalty = 80  # 0-100, affects defection chance

    def calculate_power_score(self) -> float:
        """Calculate overall faction power for diplomatic calculations."""
        return (self.population * 0.3 +
                self.territory_count * 0.2 +
                self.military_strength * 0.25 +
                self.economic_power * 0.15 +
                self.cultural_influence * 0.1)

    def get_diplomatic_stance(self, other_faction: 'EnhancedFaction') -> int:
        """Get current diplomatic stance toward another faction."""
        base_bias = self.traits.diplomatic_bias.get(other_faction.enum, 0)
        reputation = self.traits.reputation.get(other_faction.enum, 0)
        power_ratio = self.calculate_power_score() / max(other_faction.calculate_power_score(), 1)

        # Power dynamics affect diplomacy
        if power_ratio > 2.0:
            power_modifier = -20  # Weaker factions fear stronger ones
        elif power_ratio < 0.5:
            power_modifier = 30   # Stronger factions bully weaker ones
        else:
            power_modifier = 0

        return base_bias + reputation + power_modifier

    def evolve(self, world_events: List[str], challenges_faced: List[str]):
        """Evolve faction based on history and challenges."""
        self.traits.evolution_stage += 1

        # Gain abilities based on challenges overcome
        for challenge in challenges_faced:
            if "radiation" in challenge and "radiation_resistance" not in self.traits.abilities:
                self.traits.abilities.append("radiation_resistance")
            elif "famine" in challenge and "resource_efficiency" not in self.traits.abilities:
                self.traits.abilities.append("resource_efficiency")
            # Add more evolution logic...

    def generate_lore(self, lore_generator: 'EnhancedLoreGenerator') -> Dict[str, str]:
        """Generate comprehensive faction lore."""
        return lore_generator.generate_faction_lore(self.enum, self.traits)
```

## Faction Definitions

### Forest Alliance
```python
FOREST_ALLIANCE_TRAITS = FactionTraits(
    name="Forest Alliance",
    base_color=(34, 139, 34),
    preferred_terrain=[TerrainType.GRASS, TerrainType.FOREST, TerrainType.SOIL],
    abilities=[
        "plant_growth_acceleration",
        "animal_summoning",
        "nature_magic",
        "regeneration",
        "camouflage"
    ],
    weaknesses=[
        "fire_vulnerability",
        "urban_weakness",
        "industrial_sensitivity"
    ],
    technologies=[
        "druidic_rituals",
        "herbal_medicine",
        "beast_training",
        "nature_weaving"
    ],
    culture_type="Nature Harmony",
    religion="The Green Circle",
    diplomatic_bias={
        Faction.DESERT_EMPIRE: -30,  # Desert destroys forests
        Faction.AQUATIC_DOMINION: 20,  # Water supports life
        Faction.MOUNTAIN_CLANS: 10,   # Mountains provide resources
        Faction.VOID_HORDE: -50       # Void corrupts nature
    }
)
```

### Desert Empire
```python
DESERT_EMPIRE_TRAITS = FactionTraits(
    name="Desert Empire",
    base_color=(210, 180, 140),
    preferred_terrain=[TerrainType.SAND, TerrainType.SOIL],
    abilities=[
        "heat_manipulation",
        "sand_control",
        "resource_conservation",
        "endurance",
        "mirage_creation"
    ],
    weaknesses=[
        "cold_vulnerability",
        "water_dependency",
        "overheating_risk"
    ],
    technologies=[
        "sand_weaving",
        "water_purification",
        "heat_forging",
        "desert_navigation"
    ],
    culture_type="Imperial Expansion",
    religion="The Burning Sands",
    diplomatic_bias={
        Faction.FOREST_ALLIANCE: -20,  # Forests compete for water
        Faction.AQUATIC_DOMINION: -40, # Water is scarce resource
        Faction.MOUNTAIN_CLANS: 15,   # Mountains provide minerals
        Faction.VOID_HORDE: -10       # Void is mysterious
    }
)
```

### Aquatic Dominion
```python
AQUATIC_DOMINION_TRAITS = FactionTraits(
    name="Aquatic Dominion",
    base_color=(70, 130, 180),
    preferred_terrain=[TerrainType.DEEP_OCEAN, TerrainType.OCEAN, TerrainType.SHALLOW_WATER],
    abilities=[
        "water_manipulation",
        "underwater_breathing",
        "pressure_resistance",
        "tidal_control",
        "healing_springs"
    ],
    weaknesses=[
        "dehydration",
        "land_movement_penalty",
        "electrical_vulnerability"
    ],
    technologies=[
        "coral_farming",
        "current_riding",
        "pearl_crafting",
        "abyssal_diving"
    ],
    culture_type="Oceanic Harmony",
    religion="The Eternal Tide",
    diplomatic_bias={
        Faction.FOREST_ALLIANCE: 25,  # Forests provide runoff
        Faction.DESERT_EMPIRE: -35,  # Desert threatens water supply
        Faction.MOUNTAIN_CLANS: 5,   # Neutral relationship
        Faction.VOID_HORDE: -25      # Void contaminates waters
    }
)
```

### Mountain Clans
```python
MOUNTAIN_CLANS_TRAITS = FactionTraits(
    name="Mountain Clans",
    base_color=(120, 120, 120),
    preferred_terrain=[TerrainType.MOUNTAIN, TerrainType.SOIL],
    abilities=[
        "stone_shaping",
        "defensive_bonuses",
        "mining_efficiency",
        "avalanche_control",
        "earthquake_resistance"
    ],
    weaknesses=[
        "mobility_penalty",
        "siege_vulnerability",
        "isolation_risk"
    ],
    technologies=[
        "stone_forging",
        "tunnel_networks",
        "avalanche_engineering",
        "mineral_refining"
    ],
    culture_type="Clan Loyalty",
    religion="The Stone Fathers",
    diplomatic_bias={
        Faction.FOREST_ALLIANCE: 10,  # Forests provide wood
        Faction.DESERT_EMPIRE: 20,   # Trade minerals for water
        Faction.AQUATIC_DOMINION: 0, # Neutral seaborne trade
        Faction.VOID_HORDE: -15     # Void threatens mines
    }
)
```

### Void Horde
```python
VOID_HORDE_TRAITS = FactionTraits(
    name="Void Horde",
    base_color=(40, 40, 40),
    preferred_terrain=[],  # Can survive anywhere
    abilities=[
        "reality_warping",
        "corruption_spread",
        "environmental_adaptation",
        "fear_induction",
        "void_summoning"
    ],
    weaknesses=[
        "light_vulnerability",
        "stability_issues",
        "population_control"
    ],
    technologies=[
        "void_weaving",
        "corruption_rituals",
        "dimensional_rifts",
        "entropy_manipulation"
    ],
    culture_type="Chaotic Dominion",
    religion="The Endless Void",
    diplomatic_bias={
        Faction.FOREST_ALLIANCE: -60, # Nature opposes void
        Faction.DESERT_EMPIRE: -20,  # Desert is harsh but survivable
        Faction.AQUATIC_DOMINION: -45, # Water dilutes corruption
        Faction.MOUNTAIN_CLANS: -30   # Mountains resist corruption
    }
)
```

## Diplomatic Relations System

### Alliance Types
```python
class AllianceType(Enum):
    WAR = -100        # Active military conflict
    HOSTILE = -60     # Border skirmishes, trade embargoes
    UNFRIENDLY = -20  # No cooperation, occasional raids
    NEUTRAL = 0       # Peaceful coexistence, limited trade
    FRIENDLY = 30     # Trade agreements, non-aggression pacts
    ALLIED = 60       # Military alliances, resource sharing
    UNITED = 100      # Full integration, shared governance
```

### Diplomatic Actions
```python
class DiplomaticAction:
    """Represents diplomatic interactions between factions."""

    def __init__(self, initiator: EnhancedFaction, target: EnhancedFaction,
                 action_type: str, success_chance: float):
        self.initiator = initiator
        self.target = target
        self.action_type = action_type  # "trade", "alliance", "war", "gift"
        self.success_chance = success_chance
        self.consequences = self.calculate_consequences()

    def calculate_consequences(self) -> Dict[str, int]:
        """Calculate diplomatic consequences of this action."""
        consequences = {}

        if self.action_type == "trade_agreement":
            consequences["economic_bonus"] = 20
            consequences["relation_change"] = 15
        elif self.action_type == "military_alliance":
            consequences["military_support"] = 30
            consequences["relation_change"] = 25
        elif self.action_type == "declare_war":
            consequences["casualty_risk"] = 40
            consequences["relation_change"] = -80

        return consequences
```

## Evolutionary Adaptation System

### Evolution Triggers
```python
class EvolutionTrigger:
    """Triggers faction evolution based on conditions."""

    def __init__(self, condition: str, threshold: int, reward_ability: str):
        self.condition = condition  # "population", "territory", "survival_time"
        self.threshold = threshold
        self.reward_ability = reward_ability
        self.activated = False

    def check_trigger(self, faction: EnhancedFaction) -> bool:
        """Check if evolution trigger conditions are met."""
        if self.activated:
            return False

        if self.condition == "population" and faction.population >= self.threshold:
            return True
        elif self.condition == "territory" and faction.territory_count >= self.threshold:
            return True
        elif self.condition == "survival_time":
            # Check how long faction has existed
            return True

        return False
```

### Evolution Paths
```python
EVOLUTION_PATHS = {
    "Forest Alliance": [
        EvolutionTrigger("population", 1000, "mass_regeneration"),
        EvolutionTrigger("territory", 2000, "forest_expansion"),
        EvolutionTrigger("survival_time", 10000, "ancient_wisdom")
    ],
    "Desert Empire": [
        EvolutionTrigger("population", 800, "heat_wave_summoning"),
        EvolutionTrigger("territory", 1800, "oasis_creation"),
        EvolutionTrigger("survival_time", 12000, "imperial_ambition")
    ],
    # ... more evolution paths
}
```

## Cultural Spread Mechanics

### Ideological Conversion
```python
class CulturalSpread:
    """Handles ideological and cultural conversion of territories."""

    def __init__(self, world: 'World'):
        self.world = world
        self.conversion_rates = {}  # (x, y) -> conversion progress

    def spread_culture(self, faction: EnhancedFaction, center_x: int, center_y: int,
                      radius: int, intensity: float):
        """Spread faction culture from a center point."""
        for dy in range(-radius, radius + 1):
            for dx in range(-radius, radius + 1):
                distance = math.sqrt(dx*dx + dy*dy)
                if distance <= radius:
                    x, y = center_x + dx, center_y + dy
                    if 0 <= x < self.world.width and 0 <= y < self.world.height:
                        tile = self.world.tiles[y][x]

                        # Calculate conversion strength
                        strength = intensity * (1.0 - distance/radius)
                        strength *= faction.cultural_influence / 100.0

                        # Apply conversion
                        key = (x, y)
                        current_progress = self.conversion_rates.get(key, 0)
                        self.conversion_rates[key] = min(100, current_progress + strength)

                        # Check for conversion completion
                        if self.conversion_rates[key] >= 100:
                            if tile.faction_control != faction.enum:
                                old_faction = tile.faction_control
                                tile.faction_control = faction.enum
                                tile.control_strength = 0.1
                                self.on_territory_converted(x, y, old_faction, faction.enum)

    def on_territory_converted(self, x: int, y: int, old_faction: Faction, new_faction: Faction):
        """Handle territory conversion events."""
        # Update faction statistics
        # Trigger diplomatic consequences
        # Generate historical events
        # Update world lore
        pass
```

## Integration with Existing Systems

### Enhanced World Class
```python
class EnhancedWorld(World):
    """World with enhanced faction and diplomatic systems."""

    def __init__(self):
        super().__init__()
        self.enhanced_factions: Dict[Faction, EnhancedFaction] = {}
        self.diplomatic_relations: Dict[Tuple[Faction, Faction], int] = {}
        self.cultural_spread = CulturalSpread(self)
        self.active_events: List['WorldEvent'] = []

        # Initialize enhanced factions
        self.initialize_enhanced_factions()

    def initialize_enhanced_factions(self):
        """Initialize enhanced faction objects."""
        faction_traits = {
            Faction.FOREST_ALLIANCE: FOREST_ALLIANCE_TRAITS,
            Faction.DESERT_EMPIRE: DESERT_EMPIRE_TRAITS,
            Faction.AQUATIC_DOMINION: AQUATIC_DOMINION_TRAITS,
            Faction.MOUNTAIN_CLANS: MOUNTAIN_CLANS_TRAITS,
            Faction.VOID_HORDE: VOID_HORDE_TRAITS
        }

        for faction_enum, traits in faction_traits.items():
            self.enhanced_factions[faction_enum] = EnhancedFaction(faction_enum, traits)

    def update_diplomacy(self):
        """Update diplomatic relations between factions."""
        for faction_a in self.enhanced_factions.values():
            for faction_b in self.enhanced_factions.values():
                if faction_a != faction_b:
                    relation = faction_a.get_diplomatic_stance(faction_b)
                    key = (faction_a.enum, faction_b.enum)
                    self.diplomatic_relations[key] = relation

    def process_cultural_spread(self):
        """Process cultural conversion across the world."""
        for faction in self.enhanced_factions.values():
            # Spread from faction strongholds
            strongholds = self.get_faction_strongholds(faction.enum)
            for x, y in strongholds:
                self.cultural_spread.spread_culture(faction, x, y, 5, 2.0)
```

This enhanced faction system adds significant depth to the god simulator, creating a living world where factions evolve, form alliances, wage wars, and spread their influence through both military and cultural means. The system integrates seamlessly with the existing codebase while providing a foundation for complex emergent gameplay.</content>
<parameter name="filePath">c:\Users\Hostilian\The-Desktop-Terrarium\.planning\ENHANCED_FACTION_SYSTEM.md
