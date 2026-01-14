# Enhanced Visual Design - 90% WorldBox Inspired

## Overview

This document outlines the visual design enhancements that make the god simulator 90% similar to WorldBox while incorporating unique elements inspired by Caves of Qud. The design prioritizes clean pixel art aesthetics, intuitive visual feedback, and atmospheric elements that enhance immersion.

## Core Visual Philosophy

### WorldBox Fidelity (90%)
- **Pixel Art Foundation**: Maintain 8x8 pixel tiles as the core visual unit
- **Color Palette**: Use WorldBox-inspired colors with high saturation and contrast
- **UI Layout**: Mirror WorldBox's toolbar and information display structure
- **Interaction Feedback**: Immediate visual responses to player actions
- **Clean Aesthetics**: Minimalist design that doesn't clutter the simulation

### Caves of Qud Enhancements (10%)
- **Atmospheric Effects**: Dynamic lighting, particles, and environmental moods
- **Lore Visualization**: Visual hints of faction history and ancient mysteries
- **Artifact Glows**: Magical auras and special effects for powerful items
- **Environmental Storytelling**: Visual elements that tell faction stories

## Enhanced Color Palette

### Base Terrain Colors (WorldBox Inspired)
```python
# Enhanced color definitions with atmospheric variants
COLORS = {
    # Core WorldBox colors
    'DEEP_OCEAN': (0, 20, 40),
    'OCEAN': (0, 40, 80),
    'SHALLOW_WATER': (0, 80, 160),
    'SAND': (240, 220, 120),
    'SOIL': (120, 80, 40),
    'GRASS': (40, 120, 40),
    'FOREST': (20, 80, 20),
    'MOUNTAIN': (120, 120, 120),
    'SNOW': (240, 240, 240),

    # Enhanced faction colors with variants
    'FOREST_FACTION': (34, 139, 34),
    'FOREST_FACTION_LIGHT': (100, 180, 100),
    'FOREST_FACTION_DARK': (20, 80, 20),
    'DESERT_FACTION': (210, 180, 140),
    'DESERT_FACTION_LIGHT': (240, 210, 170),
    'DESERT_FACTION_DARK': (180, 140, 100),
    'AQUATIC_FACTION': (70, 130, 180),
    'AQUATIC_FACTION_LIGHT': (120, 180, 220),
    'AQUATIC_FACTION_DARK': (40, 90, 140),
    'MOUNTAIN_FACTION': (120, 120, 120),
    'MOUNTAIN_FACTION_LIGHT': (160, 160, 160),
    'MOUNTAIN_FACTION_DARK': (80, 80, 80),
    'VOID_FACTION': (40, 40, 40),
    'VOID_FACTION_LIGHT': (80, 80, 80),
    'VOID_FACTION_DARK': (20, 20, 20),

    # Atmospheric colors
    'AMBIENT_LIGHT': (255, 255, 220),
    'MOONLIGHT': (200, 200, 255),
    'SUNSET': (255, 180, 100),
    'STORM': (100, 100, 120),
    'MYSTICAL_GLOW': (150, 100, 255),
    'ANCIENT_RUNE': (255, 150, 50),

    # UI colors (WorldBox style)
    'UI_BG': (20, 20, 30),
    'UI_PANEL': (40, 40, 50),
    'UI_TEXT': (200, 200, 220),
    'UI_HIGHLIGHT': (100, 150, 255),
    'UI_ACCENT': (255, 200, 100),
    'UI_SUCCESS': (100, 255, 100),
    'UI_WARNING': (255, 255, 100),
    'UI_ERROR': (255, 100, 100),
}
```

## Enhanced Sprite System

### Multi-Resolution Sprites
```python
class EnhancedSprite:
    """Enhanced sprite with multiple detail levels and animations."""

    def __init__(self, base_sprite: pygame.Surface, faction: Faction):
        self.base_sprite = base_sprite
        self.faction = faction
        self.animation_frames = self.generate_animation_frames()
        self.glow_effect = self.generate_glow_effect()
        self.current_frame = 0
        self.animation_timer = 0

    def generate_animation_frames(self) -> List[pygame.Surface]:
        """Generate animation frames for the sprite."""
        frames = [self.base_sprite]

        # Add subtle animation variations
        for i in range(3):
            frame = self.base_sprite.copy()
            # Apply subtle pixel shifts for animation
            frames.append(frame)

        return frames

    def generate_glow_effect(self) -> pygame.Surface:
        """Generate a glow effect for special entities."""
        glow = pygame.Surface((self.base_sprite.get_width() + 4,
                              self.base_sprite.get_height() + 4), pygame.SRCALPHA)

        # Create soft glow around the sprite
        faction_color = self.get_faction_glow_color()
        for x in range(glow.get_width()):
            for y in range(glow.get_height()):
                distance = math.sqrt((x - glow.get_width()/2)**2 + (y - glow.get_height()/2)**2)
                if distance < 6:
                    alpha = int(100 * (1 - distance/6))
                    pygame.draw.circle(glow, (*faction_color, alpha), (x, y), 1)

        return glow

    def get_faction_glow_color(self) -> Tuple[int, int, int]:
        """Get the glow color for this faction."""
        glow_colors = {
            Faction.FOREST_ALLIANCE: (100, 255, 100),
            Faction.DESERT_EMPIRE: (255, 200, 100),
            Faction.AQUATIC_DOMINION: (100, 200, 255),
            Faction.MOUNTAIN_CLANS: (200, 200, 200),
            Faction.VOID_HORDE: (150, 100, 255)
        }
        return glow_colors.get(self.faction, (255, 255, 255))

    def update_animation(self, delta_time: float):
        """Update sprite animation."""
        self.animation_timer += delta_time
        if self.animation_timer > 0.2:  # 5 FPS animation
            self.animation_timer = 0
            self.current_frame = (self.current_frame + 1) % len(self.animation_frames)

    def render(self, screen: pygame.Surface, x: int, y: int, special_effect: bool = False):
        """Render the sprite with optional effects."""
        current_sprite = self.animation_frames[self.current_frame]

        # Render glow for special entities
        if special_effect:
            glow_x = x - 2
            glow_y = y - 2
            screen.blit(self.glow_effect, (glow_x, glow_y))

        # Render main sprite
        screen.blit(current_sprite, (x, y))
```

## Atmospheric Effects System

### Dynamic Lighting
```python
class LightingSystem:
    """Dynamic lighting system for atmospheric effects."""

    def __init__(self, world_width: int, world_height: int):
        self.world_width = world_width
        self.world_height = world_height
        self.light_map = self.initialize_light_map()
        self.time_of_day = 0.0  # 0.0 to 1.0 (dawn to dusk)
        self.weather_intensity = 0.0

    def initialize_light_map(self) -> List[List[float]]:
        """Initialize the lighting grid."""
        return [[1.0 for _ in range(self.world_width)] for _ in range(self.world_height)]

    def update_time_of_day(self, delta_time: float):
        """Update time-based lighting."""
        self.time_of_day = (self.time_of_day + delta_time * 0.001) % 1.0

        # Calculate base light level
        if self.time_of_day < 0.25:  # Dawn
            base_light = 0.3 + (self.time_of_day / 0.25) * 0.7
        elif self.time_of_day < 0.75:  # Day
            base_light = 1.0
        else:  # Dusk
            base_light = 1.0 - ((self.time_of_day - 0.75) / 0.25) * 0.7

        # Apply weather effects
        weather_darkness = self.weather_intensity * 0.5
        final_light = max(0.1, base_light - weather_darkness)

        # Update light map
        for y in range(self.world_height):
            for x in range(self.world_width):
                self.light_map[y][x] = final_light

    def add_local_light(self, x: int, y: int, radius: int, intensity: float):
        """Add a local light source."""
        for dy in range(-radius, radius + 1):
            for dx in range(-radius, radius + 1):
                distance = math.sqrt(dx*dx + dy*dy)
                if distance <= radius:
                    light_x = x + dx
                    light_y = y + dy

                    if 0 <= light_x < self.world_width and 0 <= light_y < self.world_height:
                        falloff = 1.0 - (distance / radius)
                        self.light_map[light_y][light_x] = min(1.0,
                            self.light_map[light_y][light_x] + intensity * falloff)

    def apply_lighting(self, color: Tuple[int, int, int]) -> Tuple[int, int, int]:
        """Apply lighting to a color."""
        # Simplified lighting application
        light_level = 0.8  # Would use actual light map in full implementation
        r = int(color[0] * light_level)
        g = int(color[1] * light_level)
        b = int(color[2] * light_level)
        return (r, g, b)
```

### Particle Effects System
```python
class EnhancedParticleSystem(ParticleSystem):
    """Enhanced particle system with atmospheric effects."""

    def __init__(self):
        super().__init__()
        self.atmospheric_particles = []
        self.weather_particles = []

    def add_atmospheric_effect(self, effect_type: str, x: int, y: int, intensity: float):
        """Add atmospheric particle effects."""
        if effect_type == "mystic_glow":
            self.add_particles(x, y, int(10 * intensity), COLORS['MYSTICAL_GLOW'], 20)
        elif effect_type == "ancient_runes":
            self.add_particles(x, y, int(5 * intensity), COLORS['ANCIENT_RUNE'], 10)
        elif effect_type == "void_energy":
            self.add_particles(x, y, int(15 * intensity), COLORS['VOID_FACTION_LIGHT'], 30)

    def add_weather_effect(self, weather_type: str, world_width: int, world_height: int):
        """Add weather-based particle effects across the world."""
        if weather_type == "rain":
            for _ in range(50):
                x = random.randint(0, world_width * TILE_SIZE)
                y = random.randint(0, world_height * TILE_SIZE)
                self.add_particles(x, y, 3, (150, 150, 200), 100)
        elif weather_type == "snow":
            for _ in range(30):
                x = random.randint(0, world_width * TILE_SIZE)
                y = random.randint(0, world_height * TILE_SIZE)
                self.add_particles(x, y, 2, (220, 220, 255), 50)

    def update_atmospheric_effects(self, delta_time: float):
        """Update atmospheric particle effects."""
        # Add new atmospheric particles based on world state
        # This would be called from the main game loop
        pass
```

## Enhanced UI Design

### WorldBox-Style Interface
```
┌─────────────────────────────────────────────────────────────┐
│ [FACTION: Forest Alliance] [POP: 1,247] [TERR: 892] [MORALE: ▓▓▓▓▓░░░] │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│                    WORLD SIMULATION                         │
│                    (120x80 pixel grid)                      │
│                                                             │
├─────────────────────────────────────────────────────────────┤
│ GOD POWERS:                                                 │
│ [1]SPAWN [2]SMITE [3]MUTATE [4]HEAL [5]FREEZE [6]BLESS      │
│ [7]CURSE [8]TERRAFORM [9]WEATHER [0]ARTIFACT                │
│                                                             │
│ CURRENT: Spawn Power | TARGET: Workers | ENERGY: ███████░░░│
│                                                             │
│ LORE EVENTS:                                                │
│ • Ancient artifact discovered in mountain ruins             │
│ • Forest Alliance forms alliance with Aquatic Dominion      │
│ • Void Horde begins ideological spread                      │
├─────────────────────────────────────────────────────────────┤
│ CONTROLS: Left-Click to use power | Right-Click to cancel  │
│          Mouse wheel to zoom | ESC for menu                 │
└─────────────────────────────────────────────────────────────┘
```

### Enhanced UI Components
```python
class EnhancedUI:
    """Enhanced UI system with WorldBox styling and Caves of Qud elements."""

    def __init__(self, screen: pygame.Surface):
        self.screen = screen
        self.fonts = self.load_fonts()
        self.animations = {}
        self.notifications = []

    def load_fonts(self) -> Dict[str, pygame.font.Font]:
        """Load pixel-perfect fonts for WorldBox aesthetic."""
        return {
            'small': pygame.font.Font(None, 16),
            'medium': pygame.font.Font(None, 24),
            'large': pygame.font.Font(None, 36),
            'title': pygame.font.Font(None, 48)
        }

    def render_faction_status(self, faction: 'EnhancedFaction', x: int, y: int):
        """Render faction status bar with WorldBox styling."""
        # Background panel
        panel_rect = pygame.Rect(x, y, 300, 40)
        pygame.draw.rect(self.screen, COLORS['UI_PANEL'], panel_rect, border_radius=5)
        pygame.draw.rect(self.screen, COLORS['UI_HIGHLIGHT'], panel_rect, 2, border_radius=5)

        # Faction name and color indicator
        faction_color = self.get_faction_ui_color(faction.enum)
        name_text = self.fonts['medium'].render(faction.name, True, faction_color)
        self.screen.blit(name_text, (x + 50, y + 5))

        # Population, territory, morale bars
        self.render_status_bar(x + 10, y + 25, "POP", faction.population, 2000, COLORS['UI_SUCCESS'])
        self.render_status_bar(x + 110, y + 25, "TERR", faction.territory_count, 2000, COLORS['UI_ACCENT'])

    def render_status_bar(self, x: int, y: int, label: str, value: int, max_value: int, color: Tuple[int, int, int]):
        """Render a WorldBox-style status bar."""
        # Label
        label_text = self.fonts['small'].render(label, True, COLORS['UI_TEXT'])
        self.screen.blit(label_text, (x, y))

        # Bar background
        bar_width = 60
        bar_height = 8
        bar_rect = pygame.Rect(x + 30, y, bar_width, bar_height)
        pygame.draw.rect(self.screen, COLORS['UI_BG'], bar_rect)

        # Bar fill
        fill_width = int(bar_width * min(value / max_value, 1.0))
        fill_rect = pygame.Rect(x + 30, y, fill_width, bar_height)
        pygame.draw.rect(self.screen, color, fill_rect)

        # Value text
        value_text = self.fonts['small'].render(str(value), True, COLORS['UI_TEXT'])
        self.screen.blit(value_text, (x + 30 + bar_width + 5, y))

    def render_lore_notifications(self, notifications: List[str]):
        """Render Caves of Qud-style lore notifications."""
        y_offset = 50
        for notification in notifications[-5:]:  # Show last 5
            # Background with ancient styling
            notif_rect = pygame.Rect(10, y_offset, 400, 25)
            pygame.draw.rect(self.screen, (*COLORS['UI_PANEL'][:3], 200), notif_rect, border_radius=3)

            # Ancient rune accent
            pygame.draw.rect(self.screen, COLORS['ANCIENT_RUNE'], notif_rect, 1, border_radius=3)

            # Text with mystical styling
            text = self.fonts['small'].render(f"📜 {notification}", True, COLORS['MYSTICAL_GLOW'])
            self.screen.blit(text, (15, y_offset + 3))

            y_offset += 30

    def render_god_power_cooldowns(self, powers: List['GodPower']):
        """Render god power buttons with WorldBox styling."""
        button_width = 80
        button_height = 40
        x_start = 50

        for i, power in enumerate(powers):
            x = x_start + i * (button_width + 10)
            y = self.screen.get_height() - 60

            # Button background
            button_rect = pygame.Rect(x, y, button_width, button_height)
            color = COLORS['UI_HIGHLIGHT'] if power.can_use() else COLORS['UI_BG']
            pygame.draw.rect(self.screen, color, button_rect, border_radius=5)
            pygame.draw.rect(self.screen, COLORS['UI_TEXT'], button_rect, 1, border_radius=5)

            # Power number
            number_text = self.fonts['large'].render(str(i + 1), True, COLORS['UI_TEXT'])
            self.screen.blit(number_text, (x + 5, y + 5))

            # Power name
            name_text = self.fonts['small'].render(power.name[:6], True, COLORS['UI_TEXT'])
            self.screen.blit(name_text, (x + 25, y + 8))

            # Cooldown overlay
            if not power.can_use():
                cooldown_progress = (time.time() - power.last_used) / power.cooldown
                overlay_height = int(button_height * (1 - cooldown_progress))
                overlay_rect = pygame.Rect(x, y + button_height - overlay_height,
                                         button_width, overlay_height)
                pygame.draw.rect(self.screen, (*COLORS['UI_BG'][:3], 150), overlay_rect)

    def get_faction_ui_color(self, faction: Faction) -> Tuple[int, int, int]:
        """Get UI color for faction display."""
        colors = {
            Faction.FOREST_ALLIANCE: COLORS['FOREST_FACTION'],
            Faction.DESERT_EMPIRE: COLORS['DESERT_FACTION'],
            Faction.AQUATIC_DOMINION: COLORS['AQUATIC_FACTION'],
            Faction.MOUNTAIN_CLANS: COLORS['MOUNTAIN_FACTION'],
            Faction.VOID_HORDE: COLORS['VOID_FACTION']
        }
        return colors.get(faction, COLORS['UI_TEXT'])
```

## Enhanced Rendering Pipeline

### Multi-Layer Rendering
```python
class EnhancedRenderer(Renderer):
    """Enhanced renderer with atmospheric effects and multi-layer rendering."""

    def __init__(self, screen: pygame.Surface):
        super().__init__(screen)
        self.lighting_system = LightingSystem(WORLD_WIDTH, WORLD_HEIGHT)
        self.atmospheric_effects = EnhancedParticleSystem()
        self.ui_system = EnhancedUI(screen)

    def render_frame(self, world: 'EnhancedWorld', delta_time: float):
        """Render a complete frame with all visual layers."""
        # Update dynamic systems
        self.lighting_system.update_time_of_day(delta_time)
        self.atmospheric_effects.update(delta_time)

        # Clear screen with atmospheric background
        self.render_atmospheric_background()

        # Render world layers (back to front)
        self.render_terrain_layer(world)
        self.render_faction_control_layer(world)
        self.render_entity_layer(world)
        self.render_atmospheric_effects_layer(world)
        self.render_lighting_overlay()

        # Render UI layers
        self.render_ui_layer(world)

    def render_atmospheric_background(self):
        """Render atmospheric background effects."""
        # Time-of-day gradient
        time_factor = self.lighting_system.time_of_day

        if time_factor < 0.25:  # Dawn
            top_color = COLORS['MOONLIGHT']
            bottom_color = COLORS['SUNSET']
        elif time_factor < 0.75:  # Day
            top_color = COLORS['AMBIENT_LIGHT']
            bottom_color = COLORS['AMBIENT_LIGHT']
        else:  # Dusk
            top_color = COLORS['SUNSET']
            bottom_color = COLORS['STORM']

        # Simple gradient (would be more sophisticated in full implementation)
        self.screen.fill(bottom_color)

    def render_terrain_layer(self, world: 'EnhancedWorld'):
        """Render the base terrain layer."""
        for y in range(world.height):
            for x in range(world.width):
                tile = world.tiles[y][x]
                color = self.lighting_system.apply_lighting(tile.get_color())

                screen_x = x * TILE_SIZE
                screen_y = y * TILE_SIZE
                rect = pygame.Rect(screen_x, screen_y, TILE_SIZE, TILE_SIZE)
                pygame.draw.rect(self.screen, color, rect)

    def render_faction_control_layer(self, world: 'EnhancedWorld'):
        """Render faction control overlays."""
        for y in range(world.height):
            for x in range(world.width):
                tile = world.tiles[y][x]
                if tile.faction_control != Faction.NEUTRAL and tile.control_strength > 0.1:
                    # Subtle faction tint overlay
                    faction_color = tile.get_faction_color()
                    alpha = int(50 * tile.control_strength)

                    overlay = pygame.Surface((TILE_SIZE, TILE_SIZE), pygame.SRCALPHA)
                    overlay.fill((*faction_color, alpha))

                    screen_x = x * TILE_SIZE
                    screen_y = y * TILE_SIZE
                    self.screen.blit(overlay, (screen_x, screen_y))

    def render_entity_layer(self, world: 'EnhancedWorld'):
        """Render entities with enhanced sprites."""
        for entity in world.entities:
            screen_x, screen_y = self.world_to_screen(entity.x, entity.y)

            # Get enhanced sprite
            sprite = self.get_enhanced_sprite(entity)
            if sprite:
                # Check if entity should have special effects
                special_effect = self.should_render_special_effect(entity)
                sprite.render(self.screen, screen_x, screen_y, special_effect)

    def render_atmospheric_effects_layer(self, world: 'EnhancedWorld'):
        """Render atmospheric particle effects."""
        self.atmospheric_effects.render(self)

    def render_lighting_overlay(self):
        """Apply lighting overlay effects."""
        # Simplified lighting overlay (full implementation would use shaders)
        pass

    def render_ui_layer(self, world: 'EnhancedWorld'):
        """Render UI with enhanced styling."""
        # Render faction status bars
        y_offset = 10
        for faction in world.enhanced_factions.values():
            self.ui_system.render_faction_status(faction, 10, y_offset)
            y_offset += 50

        # Render god power buttons
        self.ui_system.render_god_power_cooldowns(world.god_powers)

        # Render lore notifications
        lore_events = self.get_recent_lore_events(world)
        self.ui_system.render_lore_notifications(lore_events)

    def get_enhanced_sprite(self, entity) -> Optional[EnhancedSprite]:
        """Get enhanced sprite for entity."""
        # This would cache and return enhanced sprites
        # Implementation would depend on entity type and faction
        return None  # Placeholder

    def should_render_special_effect(self, entity) -> bool:
        """Determine if entity should have special visual effects."""
        # Check for artifacts, special entities, etc.
        return False  # Placeholder

    def get_recent_lore_events(self, world: 'EnhancedWorld') -> List[str]:
        """Get recent lore events for display."""
        # Return last few lore events from world history
        return []  # Placeholder
```

This enhanced visual design system maintains 90% WorldBox fidelity while adding atmospheric depth and lore visualization. The pixel art foundation ensures clean aesthetics, while the enhanced effects and UI create a more immersive and informative experience.</content>
<parameter name="filePath">c:\Users\Hostilian\The-Desktop-Terrarium\.planning\ENHANCED_VISUAL_DESIGN.md
