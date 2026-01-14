#!/usr/bin/env python3
"""
WORLD BOX - GOD SIMULATOR HYBRID
===============================

A complete god simulator inspired by WorldBox and Caves of Qud.
Features: Grid-based world, opposing factions, territorial conquest,
procedural lore, god powers, and deep simulation mechanics.

Author: Senior Game Engine Architect
Date: January 12, 2026
"""

import pygame
import random
import math
import time
from enum import Enum
from typing import List, Dict, Tuple, Optional, Set
from collections import defaultdict, deque
import json
import os

class StartupDialog:
    """Startup dialog for selecting app type and theme."""

    def __init__(self, screen: pygame.Surface):
        self.screen = screen
        self.font_large = pygame.font.Font(None, 48)
        self.font_medium = pygame.font.Font(None, 32)
        self.font_small = pygame.font.Font(None, 24)

        self.app_types = [
            {
                'name': 'Old Style Terrarium',
                'description': 'Classic desktop ecosystem with plants and creatures',
                'color': (34, 139, 34),
                'preview': '🌿 Living plants, herbivores, carnivores'
            },
            {
                'name': 'God Simulator',
                'description': 'World Box-inspired god simulator with factions',
                'color': (70, 130, 180),
                'preview': '🌍 Control factions, use divine powers'
            }
        ]

        self.god_themes = [
            {
                'name': 'Forest',
                'description': 'Trees, Deer, Wolves',
                'color': COLORS['FOREST_FACTION'],
                'preview': '🌲 Dense forests with wildlife'
            },
            {
                'name': 'Desert',
                'description': 'Cacti, Lizards, Snakes',
                'color': COLORS['DESERT_FACTION'],
                'preview': '🏜️ Arid sands with desert creatures'
            },
            {
                'name': 'Aquatic',
                'description': 'Algae, Fish, Sharks',
                'color': COLORS['AQUATIC_FACTION'],
                'preview': '🌊 Underwater world with marine life'
            },
            {
                'name': 'Powder Toy Sandbox',
                'description': 'Particles, Sand, Physics',
                'color': (255, 204, 0),  # Bright yellow (Powder Toy sand color)
                'preview': '⚛️ Falling sand particle simulation'
            }
        ]

        self.current_stage = 'app_type'  # 'app_type' or 'god_theme'
        self.selected_index = 0
        self.done = False
        self.selected_app = None
        self.selected_theme = None

    def render(self):
        """Render the startup dialog."""
        # Semi-transparent overlay
        overlay = pygame.Surface((self.screen.get_width(), self.screen.get_height()))
        overlay.fill((0, 0, 0))
        overlay.set_alpha(128)
        self.screen.blit(overlay, (0, 0))

        # Dialog background
        dialog_width = 600
        dialog_height = 500
        dialog_x = (self.screen.get_width() - dialog_width) // 2
        dialog_y = (self.screen.get_height() - dialog_height) // 2

        pygame.draw.rect(self.screen, COLORS['UI_PANEL'],
                        (dialog_x, dialog_y, dialog_width, dialog_height), border_radius=10)
        pygame.draw.rect(self.screen, COLORS['UI_HIGHLIGHT'],
                        (dialog_x, dialog_y, dialog_width, dialog_height), 3, border_radius=10)

        # Title
        if self.current_stage == 'app_type':
            title = self.font_large.render("Choose Your Experience", True, COLORS['UI_TEXT'])
        else:
            title = self.font_large.render("Choose Your World", True, COLORS['UI_TEXT'])
        title_x = dialog_x + (dialog_width - title.get_width()) // 2
        self.screen.blit(title, (title_x, dialog_y + 30))

        # Options
        y_offset = dialog_y + 100
        if self.current_stage == 'app_type':
            options = self.app_types
        else:
            options = self.god_themes

        for i, option in enumerate(options):
            # Selection highlight
            if i == self.selected_index:
                pygame.draw.rect(self.screen, COLORS['UI_HIGHLIGHT'],
                               (dialog_x + 20, y_offset - 5, dialog_width - 40, 80), border_radius=5)

            # Option name
            name_color = option['color'] if i == self.selected_index else COLORS['UI_TEXT']
            name_text = self.font_medium.render(option['name'], True, name_color)
            self.screen.blit(name_text, (dialog_x + 40, y_offset))

            # Description
            desc_text = self.font_small.render(option['description'], True, COLORS['UI_TEXT'])
            self.screen.blit(desc_text, (dialog_x + 40, y_offset + 35))

            # Preview
            preview_text = self.font_small.render(option['preview'], True, (150, 150, 150))
            self.screen.blit(preview_text, (dialog_x + 300, y_offset + 35))

            y_offset += 90

        # Instructions
        if self.current_stage == 'app_type':
            instructions = self.font_small.render("↑↓ to select, ENTER to choose, ESC to quit", True, COLORS['UI_TEXT'])
        else:
            instructions = self.font_small.render("↑↓ to select, ENTER to start, BACKSPACE for app type", True, COLORS['UI_TEXT'])
        inst_x = dialog_x + (dialog_width - instructions.get_width()) // 2
        self.screen.blit(instructions, (inst_x, dialog_y + dialog_height - 40))

    def handle_input(self, event):
        """Handle input for the dialog."""
        if event.type == pygame.KEYDOWN:
            if event.key == pygame.K_UP:
                self.selected_index = (self.selected_index - 1) % len(self.get_current_options())
            elif event.key == pygame.K_DOWN:
                self.selected_index = (self.selected_index + 1) % len(self.get_current_options())
            elif event.key == pygame.K_RETURN:
                self.handle_selection()
            elif event.key == pygame.K_BACKSPACE and self.current_stage == 'god_theme':
                self.current_stage = 'app_type'
                self.selected_index = 0
            elif event.key == pygame.K_ESCAPE:
                self.done = True
                self.selected_app = None
                self.selected_theme = None

    def get_current_options(self):
        """Get current options based on stage."""
        if self.current_stage == 'app_type':
            return self.app_types
        else:
            return self.god_themes

    def handle_selection(self):
        """Handle selection based on current stage."""
        if self.current_stage == 'app_type':
            selected = self.app_types[self.selected_index]
            if selected['name'] == 'Old Style Terrarium':
                self.selected_app = 'old'
                self.done = True
            else:  # God Simulator
                self.current_stage = 'god_theme'
                self.selected_index = 0
        else:  # god_theme
            self.selected_app = 'god'
            self.selected_theme = self.god_themes[self.selected_index]['name']
            self.done = True

    def run(self):
        """Run the dialog and return selected options."""
        while not self.done:
            for event in pygame.event.get():
                if event.type == pygame.QUIT:
                    return None, None
                self.handle_input(event)

            self.render()
            pygame.display.flip()

        return self.selected_app, self.selected_theme

class Particle:
    """Visual particle effects."""

    def __init__(self, x: float, y: float, vx: float, vy: float, color: Tuple[int, int, int], lifetime: float):
        self.x = x
        self.y = y
        self.vx = vx
        self.vy = vy
        self.color = color
        self.lifetime = lifetime
        self.max_lifetime = lifetime
        self.size = random.uniform(1, 4)

    def update(self, delta_time: float) -> bool:
        """Update particle. Returns False if dead."""
        self.x += self.vx * delta_time
        self.y += self.vy * delta_time
        self.lifetime -= delta_time
        self.vy += 50 * delta_time  # Gravity
        return self.lifetime > 0

    def render(self, renderer: 'Renderer'):
        """Render the particle."""
        screen_x, screen_y = renderer.world_to_screen(int(self.x), int(self.y))
        alpha = int(255 * (self.lifetime / self.max_lifetime))
        color = (*self.color, alpha)

        # Create surface with alpha
        surface = pygame.Surface((self.size * 2, self.size * 2), pygame.SRCALPHA)
        pygame.draw.circle(surface, color, (self.size, self.size), self.size)
        renderer.screen.blit(surface, (screen_x - self.size, screen_y - self.size))

class ParticleSystem:
    """Manages particle effects."""

    def __init__(self):
        self.particles: List[Particle] = []

    def add_particles(self, x: float, y: float, count: int, color: Tuple[int, int, int], speed: float = 50):
        """Add particles at a location."""
        for _ in range(count):
            angle = random.uniform(0, 2 * math.pi)
            velocity = random.uniform(speed * 0.5, speed)
            vx = math.cos(angle) * velocity
            vy = math.sin(angle) * velocity - random.uniform(0, speed * 0.5)  # Some upward bias
            lifetime = random.uniform(0.5, 1.5)
            self.particles.append(Particle(x, y, vx, vy, color, lifetime))

    def update(self, delta_time: float):
        """Update all particles."""
        self.particles = [p for p in self.particles if p.update(delta_time)]

    def render(self, renderer: 'Renderer'):
        """Render all particles."""
        for particle in self.particles:
            particle.render(renderer)

# World Dimensions
WORLD_WIDTH = 120
WORLD_HEIGHT = 80
TILE_SIZE = 8  # Pixel size for each tile

# Colors (WorldBox-inspired palette)
COLORS = {
    'DEEP_OCEAN': (0, 20, 40),
    'OCEAN': (0, 40, 80),
    'SHALLOW_WATER': (0, 80, 160),
    'SAND': (240, 220, 120),
    'SOIL': (120, 80, 40),
    'GRASS': (40, 120, 40),
    'FOREST': (20, 80, 20),
    'MOUNTAIN': (120, 120, 120),
    'SNOW': (240, 240, 240),

    # Faction Colors
    'FOREST_FACTION': (34, 139, 34),
    'DESERT_FACTION': (210, 180, 140),
    'AQUATIC_FACTION': (70, 130, 180),
    'MOUNTAIN_FACTION': (120, 120, 120),
    'VOID_FACTION': (40, 40, 40),

    # UI Colors
    'UI_BG': (20, 20, 30),
    'UI_PANEL': (40, 40, 50),
    'UI_TEXT': (200, 200, 220),
    'UI_HIGHLIGHT': (100, 150, 255),

    # Powder Toy Theme Colors
    'POWDER_TOY_BG': (0, 0, 0),  # Pure black background
    'POWDER_TOY_SAND': (255, 204, 0),  # Bright yellow sand
    'POWDER_TOY_DUST': (139, 90, 43),  # Brown dust/soil
    'POWDER_TOY_SALT': (255, 255, 255),  # White salt/snow
    'POWDER_TOY_WATER': (32, 64, 255),  # Bright blue water
    'POWDER_TOY_STONE': (128, 128, 128),  # Grey stone/mountain
    'POWDER_TOY_GRASS': (0, 255, 0),  # Bright green grass
    'POWDER_TOY_FOREST': (0, 128, 0),  # Dark green forest
    'POWDER_TOY_UI_DARK': (26, 26, 26),  # Dark UI panels
    'POWDER_TOY_UI_BORDER': (60, 60, 60),  # UI borders
    'POWDER_TOY_FACTION': (255, 100, 0),  # Orange faction color
}

# Simulation Parameters
SIMULATION_SPEED = 1.0
MAX_ENTITIES = 5000
MULTIPLICATION_RATE = 0.02  # Chance per tick for entity to multiply
CONQUEST_RANGE = 2  # Tiles entities can expand to
GOD_POWER_COOLDOWN = 5.0  # Seconds between god powers

# Entity Types
class EntityType(Enum):
    WORKER = "worker"
    WARRIOR = "warrior"
    SCOUT = "scout"
    ARTIFACT = "artifact"
    REMNANT = "remnant"

# Factions
class Faction(Enum):
    FOREST_ALLIANCE = "Forest Alliance"
    DESERT_EMPIRE = "Desert Empire"
    AQUATIC_DOMINION = "Aquatic Dominion"
    MOUNTAIN_CLANS = "Mountain Clans"
    VOID_HORDE = "Void Horde"
    NEUTRAL = "Neutral"

# Terrain Types
class TerrainType(Enum):
    DEEP_OCEAN = "deep_ocean"
    OCEAN = "ocean"
    SHALLOW_WATER = "shallow_water"
    SAND = "sand"
    SOIL = "soil"
    GRASS = "grass"
    FOREST = "forest"
    MOUNTAIN = "mountain"
    SNOW = "snow"

class Season(Enum):
    SPRING = "Spring"
    SUMMER = "Summer"
    AUTUMN = "Autumn"
    WINTER = "Winter"

class SeasonManager:
    def __init__(self):
        self.current_season = Season.SPRING
        self.timer = 0
        self.season_length = 30.0 # seconds per season
        
    def update(self, delta_time):
        self.timer += delta_time
        if self.timer >= self.season_length:
            self.timer = 0
            self.advance_season()
            
    def advance_season(self):
        seasons = list(Season)
        idx = seasons.index(self.current_season)
        self.current_season = seasons[(idx + 1) % len(seasons)]

# =============================================================================
# PROCEDURAL LORE GENERATION (Caves of Qud Style)
# =============================================================================

class LoreGenerator:
    """Generates procedural history and names for factions and entities."""

    def __init__(self):
        self.name_prefixes = [
            "Ancient", "Chromatic", "Eternal", "Forgotten", "Glorious",
            "Hidden", "Immortal", "Jagged", "Kaleidoscopic", "Lost",
            "Mysterious", "Noble", "Obsidian", "Primal", "Quantum",
            "Radiant", "Spectral", "Temporal", "Ultimate", "Vortex",
            "Whispering", "Xenon", "Yielding", "Zenith"
        ]

        self.name_suffixes = [
            "Sultanate", "Empire", "Dominion", "Alliance", "Horde",
            "Clans", "Kingdom", "Republic", "Confederation", "League",
            "Order", "Cult", "Brotherhood", "Sisterhood", "Enclave"
        ]

        self.descriptors = [
            "Crystal", "Sand", "Salt", "Coral", "Iron", "Gold", "Silver",
            "Blood", "Shadow", "Light", "Storm", "Flame", "Frost", "Void",
            "Star", "Moon", "Sun", "Earth", "Wind", "Water", "Fire", "Ice"
        ]

        self.events = [
            "rose from the ashes of a great cataclysm",
            "emerged from the depths of forgotten ruins",
            "was forged in the fires of eternal conflict",
            "descended from the stars in a shower of light",
            "awoke from millennia of slumber",
            "was born from the union of ancient powers",
            "survived the great cleansing",
            "endured the long night of darkness",
            "triumphed over the void invaders",
            "mastered the secrets of creation"
        ]

    def generate_faction_name(self, faction: Faction) -> str:
        """Generate a unique faction name."""
        prefix = random.choice(self.name_prefixes)
        descriptor = random.choice(self.descriptors)
        suffix = random.choice(self.name_suffixes)

        # Make it faction-appropriate
        if faction == Faction.FOREST_ALLIANCE:
            descriptor = random.choice(["Oak", "Pine", "Birch", "Willow", "Ancient"])
        elif faction == Faction.DESERT_EMPIRE:
            descriptor = random.choice(["Sand", "Salt", "Dune", "Scorpion", "Oasis"])
        elif faction == Faction.AQUATIC_DOMINION:
            descriptor = random.choice(["Coral", "Pearl", "Wave", "Tide", "Abyss"])
        elif faction == Faction.MOUNTAIN_CLANS:
            descriptor = random.choice(["Stone", "Iron", "Peak", "Crag", "Forge"])

        return f"The {prefix} {descriptor} {suffix}"

    def generate_faction_history(self, faction: Faction, name: str) -> str:
        """Generate a faction's history."""
        event = random.choice(self.events)
        return f"The {name} {event}. They control the {faction.value.lower()} territories and seek to expand their dominion across the world."

    def generate_entity_name(self, entity_type: EntityType, faction: Faction) -> str:
        """Generate an entity name."""
        if entity_type == EntityType.WORKER:
            names = ["Laborer", "Builder", "Gatherer", "Tender", "Caretaker"]
        elif entity_type == EntityType.WARRIOR:
            names = ["Guardian", "Defender", "Warrior", "Sentinel", "Protector"]
        elif entity_type == EntityType.SCOUT:
            names = ["Explorer", "Pathfinder", "Scout", "Ranger", "Watcher"]
        elif entity_type == EntityType.ARTIFACT:
            names = ["Relic", "Artifact", "Remnant", "Echo", "Vestige"]
        else:
            names = ["Entity", "Being", "Form", "Presence", "Manifestation"]

        return f"{random.choice(names)} of {faction.value}"

# =============================================================================
# WORLD & TERRAIN SYSTEM
# =============================================================================

class Tile:
    """Represents a single tile in the world grid."""

    def __init__(self, x: int, y: int, terrain: TerrainType):
        self.x = x
        self.y = y
        self.terrain = terrain
        self.faction_control = Faction.NEUTRAL
        self.control_strength = 0.0  # 0.0 to 1.0
        self.entities: List['Entity'] = []
        self.resources = 0
        self.artifact: Optional['Artifact'] = None

    def get_color(self, powder_toy_mode: bool = False, season: Optional[Season] = None) -> Tuple[int, int, int]:
        """Get the display color for this tile."""
        # Check if Powder Toy theme is active
        """Get the display color for this tile."""
        # Check if Powder Toy theme is active
        if powder_toy_mode:
            # Powder Toy vibrant color scheme
            if self.terrain == TerrainType.DEEP_OCEAN:
                color = COLORS['POWDER_TOY_WATER']
            elif self.terrain == TerrainType.OCEAN:
                color = COLORS['POWDER_TOY_WATER']
            elif self.terrain == TerrainType.SHALLOW_WATER:
                color = COLORS['POWDER_TOY_WATER']
            elif self.terrain == TerrainType.SAND:
                color = COLORS['POWDER_TOY_SAND']
            elif self.terrain == TerrainType.SOIL:
                color = COLORS['POWDER_TOY_DUST']
            elif self.terrain == TerrainType.GRASS:
                color = COLORS['POWDER_TOY_GRASS']
            elif self.terrain == TerrainType.FOREST:
                color = COLORS['POWDER_TOY_FOREST']
            elif self.terrain == TerrainType.MOUNTAIN:
                color = COLORS['POWDER_TOY_STONE']
            else:  # SNOW
                color = COLORS['POWDER_TOY_SALT']
        else:
            # Base terrain color (default theme)
            if self.terrain == TerrainType.DEEP_OCEAN:
                color = COLORS['DEEP_OCEAN']
            elif self.terrain == TerrainType.OCEAN:
                color = COLORS['OCEAN']
            elif self.terrain == TerrainType.SHALLOW_WATER:
                color = COLORS['SHALLOW_WATER']
            elif self.terrain == TerrainType.SAND:
                color = COLORS['SAND']
            elif self.terrain == TerrainType.SOIL:
                color = COLORS['SOIL']
            elif self.terrain == TerrainType.GRASS:
                color = COLORS['GRASS']
            elif self.terrain == TerrainType.FOREST:
                color = COLORS['FOREST']
            elif self.terrain == TerrainType.MOUNTAIN:
                color = COLORS['MOUNTAIN']
            else:
                color = COLORS['SNOW']

            # Apply Season Tint (Organic tiles only)
            if season and not powder_toy_mode:
                if self.terrain in [TerrainType.GRASS, TerrainType.FOREST]:
                    r, g, b = color
                    if season == Season.AUTUMN:
                        # Orange/Brown tint
                        color = (min(255, r + 60), max(0, g - 20), max(0, b - 20))
                    elif season == Season.WINTER:
                        # White/Blue tint (Snow coverage)
                        avg = (r + g + b) // 3
                        color = (min(255, avg + 60), min(255, avg + 60), min(255, avg + 80))
                    elif season == Season.SUMMER:
                        # Brighter, warmer
                        color = (min(255, r + 20), min(255, g + 20), b)
                elif self.terrain == TerrainType.MOUNTAIN and season == Season.WINTER:
                     # Snowy mountains
                     r, g, b = color
                     color = (min(255, r + 80), min(255, g + 80), min(255, b + 90))

        # Apply faction tint
        if self.faction_control != Faction.NEUTRAL and self.control_strength > 0.1:
            faction_color = self.get_faction_color()
            # Blend colors based on control strength
            blend_factor = min(self.control_strength, 0.7)
            color = (
                int(color[0] * (1 - blend_factor) + faction_color[0] * blend_factor),
                int(color[1] * (1 - blend_factor) + faction_color[1] * blend_factor),
                int(color[2] * (1 - blend_factor) + faction_color[2] * blend_factor)
            )

        return color

    def get_faction_color(self) -> Tuple[int, int, int]:
        """Get the color for the controlling faction."""
        if self.faction_control == Faction.FOREST_ALLIANCE:
            return COLORS['FOREST_FACTION']
        elif self.faction_control == Faction.DESERT_EMPIRE:
            return COLORS['DESERT_FACTION']
        elif self.faction_control == Faction.AQUATIC_DOMINION:
            return COLORS['AQUATIC_FACTION']
        elif self.faction_control == Faction.MOUNTAIN_CLANS:
            return COLORS['MOUNTAIN_FACTION']
        elif self.faction_control == Faction.VOID_HORDE:
            return COLORS['VOID_FACTION']
        else:
            return (128, 128, 128)

    def can_support_entity(self, entity: 'Entity') -> bool:
        """Check if this tile can support the given entity type."""
        if entity.faction == Faction.AQUATIC_DOMINION:
            return self.terrain in [TerrainType.DEEP_OCEAN, TerrainType.OCEAN, TerrainType.SHALLOW_WATER]
        elif entity.faction == Faction.DESERT_EMPIRE:
            return self.terrain in [TerrainType.SAND, TerrainType.SOIL]
        elif entity.faction == Faction.FOREST_ALLIANCE:
            return self.terrain in [TerrainType.GRASS, TerrainType.FOREST, TerrainType.SOIL]
        elif entity.faction == Faction.MOUNTAIN_CLANS:
            return self.terrain in [TerrainType.MOUNTAIN, TerrainType.SOIL]
        else:
            return True  # Void can go anywhere

    def get_movement_cost(self) -> float:
        """Get movement cost for this terrain."""
        costs = {
            TerrainType.DEEP_OCEAN: 2.0,
            TerrainType.OCEAN: 1.5,
            TerrainType.SHALLOW_WATER: 1.2,
            TerrainType.SAND: 1.3,
            TerrainType.SOIL: 1.0,
            TerrainType.GRASS: 1.0,
            TerrainType.FOREST: 1.4,
            TerrainType.MOUNTAIN: 2.5,
            TerrainType.SNOW: 1.8,
        }
        return costs.get(self.terrain, 1.0)

class World:
    """The game world containing all tiles and entities."""

    def __init__(self, theme: str = "Default"):
        self.width = WORLD_WIDTH
        self.height = WORLD_HEIGHT
        self.tiles: List[List[Tile]] = []
        self.entities: List['Entity'] = []
        self.factions: Dict[Faction, Dict] = {}
        self.lore_generator = LoreGenerator()
        self.theme = theme  # Store theme for rendering

        # Default terrain weights
        self.terrain_weights = {
            TerrainType.FOREST: 0.25,
            TerrainType.GRASS: 0.25,
            TerrainType.MOUNTAIN: 0.2,
            TerrainType.SHALLOW_WATER: 0.15,
            TerrainType.SAND: 0.1,
            TerrainType.DEEP_OCEAN: 0.05,
        }

        self.initialize_world()
        self.initialize_factions()

    def initialize_world(self):
        """Generate the initial world terrain."""
        self.tiles = []
        for y in range(self.height):
            row = []
            for x in range(self.width):
                # Generate terrain based on position and noise
                terrain = self.generate_terrain(x, y)
                tile = Tile(x, y, terrain)
                row.append(tile)
            self.tiles.append(row)

        # Add some random artifacts
        for _ in range(10):
            x, y = random.randint(0, self.width-1), random.randint(0, self.height-1)
            if random.random() < 0.3:  # 30% chance for artifact
                self.tiles[y][x].artifact = Artifact(x, y, self.lore_generator)

    def generate_terrain(self, x: int, y: int) -> TerrainType:
        """Generate terrain type for a given position."""
        # Use weighted random selection based on theme
        rand = random.random()
        cumulative = 0.0

        for terrain_type, weight in self.terrain_weights.items():
            cumulative += weight
            if rand <= cumulative:
                return terrain_type

        return TerrainType.GRASS  # fallback

    def initialize_factions(self):
        """Initialize faction data."""
        for faction in Faction:
            if faction != Faction.NEUTRAL:
                name = self.lore_generator.generate_faction_name(faction)
                history = self.lore_generator.generate_faction_history(faction, name)
                self.factions[faction] = {
                    'name': name,
                    'history': history,
                    'entity_count': 0,
                    'territory_count': 0,
                    'color': self.get_faction_color(faction)
                }

    def get_faction_color(self, faction: Faction) -> Tuple[int, int, int]:
        """Get faction color."""
        colors = {
            Faction.FOREST_ALLIANCE: COLORS['FOREST_FACTION'],
            Faction.DESERT_EMPIRE: COLORS['DESERT_FACTION'],
            Faction.AQUATIC_DOMINION: COLORS['AQUATIC_FACTION'],
            Faction.MOUNTAIN_CLANS: COLORS['MOUNTAIN_FACTION'],
            Faction.VOID_HORDE: COLORS['VOID_FACTION']
        }
        return colors.get(faction, (128, 128, 128))

    def get_tile(self, x: int, y: int) -> Optional[Tile]:
        """Get tile at position."""
        if 0 <= x < self.width and 0 <= y < self.height:
            return self.tiles[y][x]
        return None

    def get_neighbors(self, x: int, y: int, radius: int = 1) -> List[Tile]:
        """Get neighboring tiles within radius."""
        neighbors = []
        for dy in range(-radius, radius + 1):
            for dx in range(-radius, radius + 1):
                if dx == 0 and dy == 0:
                    continue
                nx, ny = x + dx, y + dy
                tile = self.get_tile(nx, ny)
                if tile:
                    neighbors.append(tile)
        return neighbors

    def update_faction_territory(self):
        """Update faction territory counts."""
        for faction in self.factions:
            self.factions[faction]['territory_count'] = 0

        for row in self.tiles:
            for tile in row:
                if tile.faction_control != Faction.NEUTRAL:
                    self.factions[tile.faction_control]['territory_count'] += 1

    def spawn_entity(self, x: int, y: int, entity_type: EntityType, faction: Faction):
        """Spawn a new entity."""
        if len(self.entities) >= MAX_ENTITIES:
            return None

        tile = self.get_tile(x, y)
        if not tile:
            return None

        entity = Entity(x, y, entity_type, faction, self.lore_generator)
        tile.entities.append(entity)
        self.entities.append(entity)
        self.factions[faction]['entity_count'] += 1

        return entity

# =============================================================================
# ENTITY SYSTEM
# =============================================================================

class Entity:
    """Base class for all entities in the world."""

    def __init__(self, x: int, y: int, entity_type: EntityType, faction: Faction, lore_generator: LoreGenerator):
        self.x = x
        self.y = y
        self.entity_type = entity_type
        self.faction = faction
        self.id = id(self)  # Unique ID
        self.age = 0
        self.energy = 100.0
        self.health = 100.0
        self.name = lore_generator.generate_entity_name(entity_type, faction)
        self.target_x = x
        self.target_y = y
        self.state = "idle"  # idle, moving, fighting, multiplying
        self.last_action = 0

    def update(self, world: World, delta_time: float):
        """Update entity behavior."""
        self.age += delta_time
        self.energy = max(0, self.energy - delta_time * 2)  # Energy drain

        # Basic AI behavior
        if self.energy < 20:
            self.state = "seeking_food"
            self.seek_food(world)
        elif random.random() < MULTIPLICATION_RATE and self.energy > 50:
            self.attempt_multiplication(world)
        else:
            self.explore_and_conquer(world)

        # Move towards target
        self.move_towards_target(world, delta_time)

    def seek_food(self, world: World):
        """Seek food/energy sources."""
        tile = world.get_tile(self.x, self.y)
        if tile and tile.resources > 0:
            tile.resources -= 1
            self.energy = min(100, self.energy + 20)
            self.state = "idle"
        else:
            # Move to adjacent tile with resources
            neighbors = world.get_neighbors(self.x, self.y)
            resource_tiles = [t for t in neighbors if t.resources > 0]
            if resource_tiles:
                target = random.choice(resource_tiles)
                self.target_x, self.target_y = target.x, target.y

    def attempt_multiplication(self, world: World):
        """Attempt to multiply/create offspring."""
        self.state = "multiplying"

        # Find suitable adjacent tile
        neighbors = world.get_neighbors(self.x, self.y)
        suitable_tiles = [t for t in neighbors if t.can_support_entity(self) and len(t.entities) < 3]

        if suitable_tiles:
            target_tile = random.choice(suitable_tiles)
            # Create offspring
            offspring = world.spawn_entity(target_tile.x, target_tile.y, self.entity_type, self.faction)
            if offspring:
                self.energy -= 30  # Cost of reproduction
                self.state = "idle"

    def explore_and_conquer(self, world: World):
        """Explore and attempt to conquer new territory."""
        self.state = "exploring"

        # Look for unconquered or weakly held territory
        neighbors = world.get_neighbors(self.x, self.y, CONQUEST_RANGE)
        target_tiles = []

        for tile in neighbors:
            if tile.faction_control != self.faction:
                if tile.faction_control == Faction.NEUTRAL:
                    target_tiles.append((tile, 1.0))  # Easy conquest
                else:
                    # Calculate conquest difficulty
                    defender_strength = tile.control_strength
                    attacker_advantage = 1.0
                    if self.entity_type == EntityType.WARRIOR:
                        attacker_advantage = 1.5
                    difficulty = defender_strength / attacker_advantage
                    if difficulty < 0.8:  # Worth attempting
                        target_tiles.append((tile, difficulty))

        if target_tiles:
            # Choose best target (lowest difficulty)
            target_tiles.sort(key=lambda x: x[1])
            target_tile, _ = target_tiles[0]
            self.target_x, self.target_y = target_tile.x, target_tile.y

            # Attempt conquest when arrived
            if self.x == target_tile.x and self.y == target_tile.y:
                self.conquer_tile(world, target_tile)

    def conquer_tile(self, world: World, tile: Tile):
        """Attempt to conquer a tile."""
        if tile.faction_control == Faction.NEUTRAL:
            tile.faction_control = self.faction
            tile.control_strength = 0.3
        elif tile.faction_control != self.faction:
            # Combat resolution
            defender_strength = tile.control_strength
            attacker_strength = 0.4  # Base conquest strength

            if self.entity_type == EntityType.WARRIOR:
                attacker_strength *= 1.5

            if random.random() * attacker_strength > defender_strength:
                # Successful conquest
                old_faction = tile.faction_control
                tile.faction_control = self.faction
                tile.control_strength = attacker_strength

                # Assimilate entities on the tile
                for entity in tile.entities[:]:
                    if entity.faction == old_faction:
                        entity.faction = self.faction
                        entity.name = world.lore_generator.generate_entity_name(entity.entity_type, self.faction)

                self.energy -= 10  # Cost of conquest
            else:
                # Failed conquest
                self.health -= 20
                if self.health <= 0:
                    self.die(world)

    def move_towards_target(self, world: World, delta_time: float):
        """Move towards target position."""
        if self.x == self.target_x and self.y == self.target_y:
            return

        dx = self.target_x - self.x
        dy = self.target_y - self.y
        distance = math.sqrt(dx*dx + dy*dy)

        if distance > 0:
            # Normalize direction
            dx /= distance
            dy /= distance

            # Calculate movement speed (affected by terrain)
            tile = world.get_tile(self.x, self.y)
            speed = 20 * delta_time / (tile.get_movement_cost() if tile else 1.0)

            # Move
            new_x = self.x + dx * speed
            new_y = self.y + dy * speed

            # Check bounds
            new_x = max(0, min(world.width - 1, new_x))
            new_y = max(0, min(world.height - 1, new_y))

            # Update position
            self.x = int(new_x)
            self.y = int(new_y)

    def die(self, world: World):
        """Handle entity death."""
        tile = world.get_tile(self.x, self.y)
        if tile:
            tile.entities.remove(self)
        world.entities.remove(self)
        world.factions[self.faction]['entity_count'] -= 1

        # Add death particles
        # This would be called from the main game class

    def get_color(self, world) -> Tuple[int, int, int]:
        """Get display color for this entity."""
        base_color = world.get_faction_color(self.faction)

        # Modify based on type
        if self.entity_type == EntityType.WARRIOR:
            # Darker for warriors
            return (max(0, base_color[0] - 40), max(0, base_color[1] - 40), max(0, base_color[2] - 40))
        elif self.entity_type == EntityType.SCOUT:
            # Lighter for scouts
            return (min(255, base_color[0] + 40), min(255, base_color[1] + 40), min(255, base_color[2] + 40))
        else:
            return base_color

class Artifact:
    """Special artifacts that can be discovered."""

    def __init__(self, x: int, y: int, lore_generator: LoreGenerator):
        self.x = x
        self.y = y
        self.discovered = False
        self.power_type = random.choice(["spawn_booster", "conquest_booster", "healing_aura", "mutation"])
        self.name = f"Ancient {random.choice(['Crystal', 'Relic', 'Artifact', 'Remnant', 'Vestige'])}"
        self.description = f"A mysterious {self.power_type.replace('_', ' ')} that grants special abilities."

    def activate(self, world: World, activator_faction: Faction):
        """Activate the artifact's power."""
        if self.power_type == "spawn_booster":
            # Boost reproduction rate for faction
            pass  # Would implement faction-wide effects
        elif self.power_type == "conquest_booster":
            # Boost conquest strength
            pass
        elif self.power_type == "healing_aura":
            # Heal nearby entities
            for entity in world.entities:
                if entity.faction == activator_faction:
                    distance = math.sqrt((entity.x - self.x)**2 + (entity.y - self.y)**2)
                    if distance < 5:
                        entity.health = min(100, entity.health + 20)
        elif self.power_type == "mutation":
            # Mutate nearby entities
            for entity in world.entities:
                if entity.faction == activator_faction:
                    distance = math.sqrt((entity.x - self.x)**2 + (entity.y - self.y)**2)
                    if distance < 3:
                        entity.entity_type = random.choice(list(EntityType))

# =============================================================================
# GOD POWERS SYSTEM
# =============================================================================

class GodPower:
    """Represents a god power that the player can use."""

    def __init__(self, name: str, description: str, cooldown: float, cost: int = 0):
        self.name = name
        self.description = description
        self.cooldown = cooldown
        self.last_used = 0
        self.cost = cost

    def can_use(self) -> bool:
        """Check if power can be used."""
        return time.time() - self.last_used >= self.cooldown

    def use(self, world: World, x: int, y: int, faction: Optional[Faction] = None):
        """Use the power at the given location."""
        if not self.can_use():
            return False

        self.last_used = time.time()
        self._execute(world, x, y, faction)
        return True

    def _execute(self, world: World, x: int, y: int, faction: Optional[Faction]):
        """Execute the power's effect."""
        pass

class SpawnPower(GodPower):
    """Power to spawn entities."""

    def __init__(self):
        super().__init__("Spawn", "Create entities of a chosen faction", 2.0)

    def _execute(self, world: World, x: int, y: int, faction: Optional[Faction]):
        if faction:
            entity_type = random.choice([EntityType.WORKER, EntityType.WARRIOR, EntityType.SCOUT])
            world.spawn_entity(x, y, entity_type, faction)

class SmitePower(GodPower):
    """Power to destroy entities in an area."""

    def __init__(self):
        super().__init__("Smite", "Destroy all entities in target area", 5.0)

    def _execute(self, world: World, x: int, y: int, faction: Optional[Faction]):
        # Destroy entities in 3x3 area
        for dy in range(-1, 2):
            for dx in range(-1, 2):
                tile = world.get_tile(x + dx, y + dy)
                if tile:
                    # Kill entities
                    for entity in tile.entities[:]:
                        entity.die(world)
                    # Damage terrain control
                    tile.control_strength *= 0.5

class MutatePower(GodPower):
    """Power to mutate entities."""

    def __init__(self):
        super().__init__("Mutate", "Transform entities in target area", 8.0)

    def _execute(self, world: World, x: int, y: int, faction: Optional[Faction]):
        # Mutate entities in area
        for dy in range(-2, 3):
            for dx in range(-2, 3):
                tile = world.get_tile(x + dx, y + dy)
                if tile:
                    for entity in tile.entities:
                        if random.random() < 0.7:  # 70% chance to mutate
                            entity.entity_type = random.choice(list(EntityType))
                            entity.name = world.lore_generator.generate_entity_name(entity.entity_type, entity.faction)

class HealPower(GodPower):
    """Power to heal entities."""

    def __init__(self):
        super().__init__("Heal", "Restore health to entities in area", 6.0)

    def _execute(self, world: World, x: int, y: int, faction: Optional[Faction]):
        # Heal entities in area
        healed_count = 0
        for dy in range(-3, 4):
            for dx in range(-3, 4):
                tile = world.get_tile(x + dx, y + dy)
                if tile:
                    for entity in tile.entities:
                        if entity.health < 100:
                            entity.health = min(100, entity.health + 30)
                            healed_count += 1

        # Visual feedback
        if healed_count > 0:
            # Add healing particles
            pass  # Will be handled by particle system

class FreezePower(GodPower):
    """Power to freeze entities in place."""

    def __init__(self):
        super().__init__("Freeze", "Temporarily immobilize entities", 10.0)

    def _execute(self, world: World, x: int, y: int, faction: Optional[Faction]):
        # Freeze entities in area (would need to add freeze state to entities)
        # For now, just damage them
        for dy in range(-2, 3):
            for dx in range(-2, 3):
                tile = world.get_tile(x + dx, y + dy)
                if tile:
                    for entity in tile.entities:
                        entity.health = max(0, entity.health - 25)

# =============================================================================
# RENDERING & UI SYSTEM
# =============================================================================

class Renderer:
    """Handles all rendering operations."""

    def __init__(self, screen: pygame.Surface):
        self.screen = screen
        self.font = pygame.font.Font(None, 24)
        self.small_font = pygame.font.Font(None, 16)
        self.camera_x = 0
        self.camera_y = 0
        self.zoom = 1.0

    def render_world(self, world: World, season: Optional[Season] = None):
        """Render the entire world."""
        """Render the entire world."""
        # Clear screen with theme-appropriate background
        if world.theme == "Powder Toy Sandbox":
            self.screen.fill(COLORS['POWDER_TOY_BG'])  # Pure black
        else:
            self.screen.fill(COLORS['DEEP_OCEAN'])

        # Calculate visible area (accounting for UI at bottom)
        world_pixel_width = WORLD_WIDTH * TILE_SIZE
        world_pixel_height = WORLD_HEIGHT * TILE_SIZE
        ui_height = 200
        available_height = self.screen.get_height() - ui_height

        # Center the world horizontally and vertically in available space
        offset_x = (self.screen.get_width() - world_pixel_width) // 2
        offset_y = (available_height - world_pixel_height) // 2

        # Check if Powder Toy mode is active
        powder_toy_mode = (world.theme == "Powder Toy Sandbox")

        # Render tiles
        for y in range(world.height):
            for x in range(world.width):
                tile = world.tiles[y][x]
                color = tile.get_color(powder_toy_mode, season)

                # Convert world coordinates to screen coordinates with centering
                screen_x = x * TILE_SIZE + offset_x
                screen_y = y * TILE_SIZE + offset_y
                screen_size = TILE_SIZE

                # Only render if visible
                if (screen_x + screen_size > 0 and screen_x < self.screen.get_width() and
                    screen_y + screen_size > 0 and screen_y < available_height):
                    rect = pygame.Rect(screen_x, screen_y, screen_size, screen_size)
                    pygame.draw.rect(self.screen, color, rect)

                    # Render artifacts
                    if tile.artifact and not tile.artifact.discovered:
                        pygame.draw.circle(self.screen, (255, 255, 0),
                                         (screen_x + screen_size/2, screen_y + screen_size/2), 2)

        # Render entities
        for entity in world.entities:
            screen_x = entity.x * TILE_SIZE + offset_x
            screen_y = entity.y * TILE_SIZE + offset_y
            screen_size = TILE_SIZE

            if (screen_x + screen_size > 0 and screen_x < self.screen.get_width() and
                screen_y + screen_size > 0 and screen_y < available_height):
                color = entity.get_color(world)
                rect = pygame.Rect(screen_x, screen_y, screen_size, screen_size)
                pygame.draw.rect(self.screen, color, rect)

                # Health bar for damaged entities
                if entity.health < 100:
                    bar_width = screen_size
                    bar_height = 2
                    health_ratio = entity.health / 100
                    pygame.draw.rect(self.screen, (255, 0, 0),
                                   (screen_x, screen_y - 3, bar_width, bar_height))
                    pygame.draw.rect(self.screen, (0, 255, 0),
                                   (screen_x, screen_y - 3, bar_width * health_ratio, bar_height))

    def render_ui(self, world: World, selected_power: Optional[GodPower], mouse_world_pos: Tuple[int, int], god_powers: List[GodPower], season: Optional[Season] = None):
        """Render the user interface."""
        # UI Background
        ui_height = 200
        ui_rect = pygame.Rect(0, self.screen.get_height() - ui_height,
                            self.screen.get_width(), ui_height)
        pygame.draw.rect(self.screen, COLORS['UI_BG'], ui_rect)

        # Faction info (left side)
        y_offset = self.screen.get_height() - ui_height + 10
        for faction, data in world.factions.items():
            if data['entity_count'] > 0:  # Only show factions with entities
                color = data['color']
                text = f"{data['name']}: {data['entity_count']} entities, {data['territory_count']} tiles"
                text_surface = self.small_font.render(text, True, color)
                self.screen.blit(text_surface, (10, y_offset))
                y_offset += 20

        # God powers (right side, better positioned)
        power_x = self.screen.get_width() - 350  # Moved left to prevent cutoff
        power_y = self.screen.get_height() - ui_height + 10

        for i, power in enumerate(god_powers):
            color = COLORS['UI_HIGHLIGHT'] if selected_power and selected_power.name == power.name else COLORS['UI_TEXT']
            key_text = f"{i+1}:"
            power_text = f"{power.name}"

            # Render key
            key_surface = self.font.render(key_text, True, COLORS['UI_HIGHLIGHT'])
            self.screen.blit(key_surface, (power_x, power_y))

            # Render power name
            name_surface = self.font.render(power_text, True, color)
            self.screen.blit(name_surface, (power_x + 30, power_y))

            # Render description on next line
            desc_surface = self.small_font.render(power.description, True, COLORS['UI_TEXT'])
            self.screen.blit(desc_surface, (power_x + 30, power_y + 25))

            power_y += 50  # More spacing between powers

        # Mouse position info (bottom left)
        tile = world.get_tile(mouse_world_pos[0], mouse_world_pos[1])
        if tile:
            info_text = f"Tile: {tile.terrain.value}, Faction: {tile.faction_control.value}, Entities: {len(tile.entities)}"
            if tile.artifact:
                info_text += f", Artifact: {tile.artifact.name}"
            text_surface = self.small_font.render(info_text, True, COLORS['UI_TEXT'])
            text_surface = self.small_font.render(info_text, True, COLORS['UI_TEXT'])
            self.screen.blit(text_surface, (10, self.screen.get_height() - 30))

        # Render Season
        if season:
            season_text = f"Season: {season.value}"
            season_surf = self.font.render(season_text, True, COLORS['UI_TEXT'])
            # Top right, keeping space
            self.screen.blit(season_surf, (self.screen.get_width() - 150, 10))

    def world_to_screen(self, world_x: int, world_y: int) -> Tuple[int, int]:
        """Convert world coordinates to screen coordinates."""
        world_pixel_width = WORLD_WIDTH * TILE_SIZE
        world_pixel_height = WORLD_HEIGHT * TILE_SIZE
        ui_height = 200
        available_height = self.screen.get_height() - ui_height

        # Center the world
        offset_x = (self.screen.get_width() - world_pixel_width) // 2
        offset_y = (available_height - world_pixel_height) // 2

        screen_x = world_x * TILE_SIZE + offset_x
        screen_y = world_y * TILE_SIZE + offset_y
        return screen_x, screen_y

    def screen_to_world(self, screen_x: int, screen_y: int) -> Tuple[int, int]:
        """Convert screen coordinates to world coordinates."""
        world_pixel_width = WORLD_WIDTH * TILE_SIZE
        world_pixel_height = WORLD_HEIGHT * TILE_SIZE
        ui_height = 200
        available_height = self.screen.get_height() - ui_height

        # Center the world
        offset_x = (self.screen.get_width() - world_pixel_width) // 2
        offset_y = (available_height - world_pixel_height) // 2

        # Convert screen to world
        world_x = (screen_x - offset_x) // TILE_SIZE
        world_y = (screen_y - offset_y) // TILE_SIZE

        # Clamp to world bounds
        world_x = max(0, min(world_x, WORLD_WIDTH - 1))
        world_y = max(0, min(world_y, WORLD_HEIGHT - 1))

        return world_x, world_y

# =============================================================================
# MAIN GAME CLASS
# =============================================================================

class GodSimulator:
    """Main game class."""

    def __init__(self):
        pygame.init()
        # Window Setup
        self.screen_width = 500
        self.screen_height = 400
        
        # Position in bottom-right corner
        import os
        import ctypes
        try:
            user32 = ctypes.windll.user32
            screensize = user32.GetSystemMetrics(0), user32.GetSystemMetrics(1) 
            x = screensize[0] - self.screen_width - 20
            y = screensize[1] - self.screen_height - 60
            os.environ['SDL_VIDEO_WINDOW_POS'] = "%d,%d" % (x,y)
        except:
            os.environ['SDL_VIDEO_WINDOW_POS'] = "100,100"

        self.screen = pygame.display.set_mode((self.screen_width, self.screen_height), pygame.NOFRAME)
        pygame.display.set_caption("World Box - God Simulator")

        # Show startup dialog
        dialog = StartupDialog(self.screen)
        selected_app, selected_theme = dialog.run()

        if selected_app is None:
            pygame.quit()
            exit(0)
        elif selected_app == 'old':
            # Launch old style terrarium
            pygame.quit()
            self.launch_old_terrarium()
            exit(0)

        self.selected_theme = selected_theme
        print(f"🌍 Starting {selected_theme} God Simulator...")

        self.world = World(theme=selected_theme)
        self.renderer = Renderer(self.screen)
        self.particles = ParticleSystem()
        self.season_manager = SeasonManager()

        self.clock = pygame.time.Clock()
        self.running = True

        # Game state
        self.selected_power: Optional[GodPower] = None
        self.selected_faction = Faction.FOREST_ALLIANCE
        self.god_powers = [SpawnPower(), SmitePower(), MutatePower(), HealPower(), FreezePower()]
        
        # Theme flags
        self.powder_toy_mode = False

        # Apply theme-specific settings
        self.apply_theme_settings()

        # Initial spawn
        self.spawn_initial_entities()

    def launch_old_terrarium(self):
        """Launch the old style C# terrarium."""
        import subprocess
        import sys
        import os

        try:
            # Try to run the C# app
            csproj_path = os.path.join(os.path.dirname(__file__), 'Terrarium.Desktop', 'Terrarium.Desktop.csproj')
            if os.path.exists(csproj_path):
                print("Launching old style terrarium...")
                # Try dotnet run
                subprocess.run(['dotnet', 'run', '--project', csproj_path], check=True)
            else:
                print("Old terrarium project not found. Please ensure Terrarium.Desktop.csproj exists.")
        except subprocess.CalledProcessError as e:
            print(f"Failed to launch old terrarium: {e}")
        except FileNotFoundError:
            print("dotnet command not found. Please install .NET SDK.")

    def apply_theme_settings(self):
        """Apply settings based on selected theme."""
        if self.selected_theme == "Forest":
            # Forest theme: More trees, forest creatures
            self.world.terrain_weights = {
                TerrainType.FOREST: 0.4,
                TerrainType.GRASS: 0.3,
                TerrainType.MOUNTAIN: 0.2,
                TerrainType.SHALLOW_WATER: 0.1,
            }
            self.initial_factions = [Faction.FOREST_ALLIANCE, Faction.MOUNTAIN_CLANS]
        elif self.selected_theme == "Desert":
            # Desert theme: Sandy terrain, desert creatures
            self.world.terrain_weights = {
                TerrainType.SAND: 0.5,
                TerrainType.MOUNTAIN: 0.3,
                TerrainType.GRASS: 0.2,
            }
            self.initial_factions = [Faction.DESERT_EMPIRE, Faction.MOUNTAIN_CLANS]
        elif self.selected_theme == "Aquatic":
            # Aquatic theme: Lots of water, aquatic creatures
            self.world.terrain_weights = {
                TerrainType.SHALLOW_WATER: 0.4,
                TerrainType.DEEP_OCEAN: 0.3,
                TerrainType.GRASS: 0.2,
                TerrainType.MOUNTAIN: 0.1,
            }
            self.initial_factions = [Faction.AQUATIC_DOMINION, Faction.FOREST_ALLIANCE]
        elif self.selected_theme == "Powder Toy Sandbox":
            # Powder Toy theme: Particle simulation with granular materials
            self.world.terrain_weights = {
                TerrainType.SAND: 0.40,      # Heavy sand emphasis (yellow particles)
                TerrainType.SOIL: 0.20,       # Dust/powder (brown particles)
                TerrainType.SHALLOW_WATER: 0.15,  # Liquid particles (blue)
                TerrainType.MOUNTAIN: 0.10,   # Solid barriers (grey stone)
                TerrainType.GRASS: 0.10,      # Mixed particles (green)
                TerrainType.SNOW: 0.05,       # White particles (salt)
            }
            self.initial_factions = [Faction.DESERT_EMPIRE, Faction.VOID_HORDE]
            # Apply Powder Toy color scheme to UI
            self.powder_toy_mode = True
        else:
            # Default
            self.initial_factions = [Faction.FOREST_ALLIANCE, Faction.DESERT_EMPIRE, Faction.AQUATIC_DOMINION, Faction.MOUNTAIN_CLANS]
            self.powder_toy_mode = False

    def spawn_initial_entities(self):
        """Spawn initial entities for each faction."""
        # Use theme-specific factions if available
        factions_to_spawn = getattr(self, 'initial_factions', [
            Faction.FOREST_ALLIANCE, Faction.DESERT_EMPIRE,
            Faction.AQUATIC_DOMINION, Faction.MOUNTAIN_CLANS
        ])

        spawn_points = {
            Faction.FOREST_ALLIANCE: [(20, 20), (25, 25), (30, 20)],
            Faction.DESERT_EMPIRE: [(80, 20), (85, 25), (90, 20)],
            Faction.AQUATIC_DOMINION: [(20, 60), (25, 65), (30, 60)],
            Faction.MOUNTAIN_CLANS: [(80, 60), (85, 65), (90, 60)],
        }

        for faction in factions_to_spawn:
            if faction in spawn_points:
                for x, y in spawn_points[faction]:
                    entity_type = random.choice([EntityType.WORKER, EntityType.WARRIOR, EntityType.SCOUT])
                    self.world.spawn_entity(x, y, entity_type, faction)

    def handle_events(self):
        """Handle user input events."""
        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                self.running = False
            elif event.type == pygame.MOUSEBUTTONDOWN:
                self.handle_mouse_click(event)
            elif event.type == pygame.KEYDOWN:
                self.handle_key_press(event)

    def handle_mouse_click(self, event):
        """Handle mouse click events."""
        mouse_x, mouse_y = event.pos
        world_x, world_y = self.renderer.screen_to_world(mouse_x, mouse_y)

        if event.button == 1:  # Left click
            if self.selected_power:
                # Use god power
                if self.selected_power.use(self.world, world_x, world_y, self.selected_faction):
                    # Add particle effects based on power type
                    if isinstance(self.selected_power, SpawnPower):
                        self.particles.add_particles(world_x * TILE_SIZE + TILE_SIZE/2,
                                                   world_y * TILE_SIZE + TILE_SIZE/2,
                                                   10, (0, 255, 0), 30)
                    elif isinstance(self.selected_power, SmitePower):
                        self.particles.add_particles(world_x * TILE_SIZE + TILE_SIZE/2,
                                                   world_y * TILE_SIZE + TILE_SIZE/2,
                                                   20, (255, 100, 0), 50)
                    elif isinstance(self.selected_power, MutatePower):
                        self.particles.add_particles(world_x * TILE_SIZE + TILE_SIZE/2,
                                                   world_y * TILE_SIZE + TILE_SIZE/2,
                                                   15, (255, 0, 255), 25)
                    elif isinstance(self.selected_power, HealPower):
                        self.particles.add_particles(world_x * TILE_SIZE + TILE_SIZE/2,
                                                   world_y * TILE_SIZE + TILE_SIZE/2,
                                                   12, (0, 255, 255), 20)
                    elif isinstance(self.selected_power, FreezePower):
                        self.particles.add_particles(world_x * TILE_SIZE + TILE_SIZE/2,
                                                   world_y * TILE_SIZE + TILE_SIZE/2,
                                                   8, (100, 100, 255), 15)

                    self.selected_power = None
            else:
                # Select faction or inspect
                tile = self.world.get_tile(world_x, world_y)
                if tile and tile.entities:
                    # Could implement entity selection here
                    pass

        elif event.button == 3:  # Right click
            # Cancel power selection
            self.selected_power = None

    def handle_key_press(self, event):
        """Handle keyboard input."""
        if event.key == pygame.K_1:
            self.selected_power = self.god_powers[0]  # Spawn
        elif event.key == pygame.K_2:
            self.selected_power = self.god_powers[1]  # Smite
        elif event.key == pygame.K_3:
            self.selected_power = self.god_powers[2]  # Mutate
        elif event.key == pygame.K_4:
            self.selected_power = self.god_powers[3]  # Heal
        elif event.key == pygame.K_5:
            self.selected_power = self.god_powers[4]  # Freeze
        elif event.key == pygame.K_q:
            self.selected_faction = Faction.FOREST_ALLIANCE
        elif event.key == pygame.K_w:
            self.selected_faction = Faction.DESERT_EMPIRE
        elif event.key == pygame.K_e:
            self.selected_faction = Faction.AQUATIC_DOMINION
        elif event.key == pygame.K_r:
            self.selected_faction = Faction.MOUNTAIN_CLANS
        elif event.key == pygame.K_ESCAPE:
            self.selected_power = None

    def update(self, delta_time: float):
        """Update game state."""
        # Update entities
        dead_entities = []
        for entity in self.world.entities[:]:  # Copy list to avoid modification during iteration
            entity.update(self.world, delta_time)
            if entity.health <= 0:
                dead_entities.append(entity)

        # Handle dead entities with particle effects
        for entity in dead_entities:
            # Add death particles
            self.particles.add_particles(entity.x * TILE_SIZE + TILE_SIZE/2,
                                       entity.y * TILE_SIZE + TILE_SIZE/2,
                                       8, (255, 50, 50), 40)
            entity.die(self.world)

        # Update faction statistics
        self.world.update_faction_territory()

        # Update particles
        self.particles.update(delta_time)
        
        # Update Season
        self.season_manager.update(delta_time)

    def render(self):
        """Render the game."""
        mouse_x, mouse_y = pygame.mouse.get_pos()
        mouse_world_pos = self.renderer.screen_to_world(mouse_x, mouse_y)

        self.renderer.render_world(self.world, self.season_manager.current_season)
        self.particles.render(self.renderer)
        self.renderer.render_ui(self.world, self.selected_power, mouse_world_pos, self.god_powers, self.season_manager.current_season)

        pygame.display.flip()

    def run(self):
        """Main game loop."""
        last_time = time.time()

        while self.running:
            current_time = time.time()
            delta_time = (current_time - last_time) * SIMULATION_SPEED
            last_time = current_time

            self.handle_events()
            self.update(delta_time)
            self.render()

            self.clock.tick(60)

        pygame.quit()
        sys.exit(0)

# =============================================================================
# MAIN ENTRY POINT
# =============================================================================

if __name__ == "__main__":
    print("🌍 World Box - God Simulator Starting...")
    print("Choose your terrarium theme and become a god!")
    print()

    simulator = GodSimulator()

    if hasattr(simulator, 'selected_theme') and simulator.selected_theme:
        print(f"🏞️  {simulator.selected_theme} Terrarium initialized!")
        print("Watch as factions multiply, conquer territory, and wage eternal war!")
        print("Use your god powers to influence the balance of power...")
        print()

    simulator.run()
