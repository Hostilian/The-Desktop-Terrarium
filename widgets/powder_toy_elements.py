#!/usr/bin/env python3
"""
POWDER TOY ELEMENT DEFINITIONS - EXPANDED
==========================================

Extended element library with more particle types.
"""

from dataclasses import dataclass
from typing import Optional, Tuple, TYPE_CHECKING
import random

if TYPE_CHECKING:
    from powder_toy_engine import PowderToySimulation, Particle

# =============================================================================
# ELEMENT BASE CLASS
# =============================================================================

@dataclass
class Element:
    """
    Base element definition - defines all properties and behaviors.
    Based on TPT's Element class from Element.h
    """
    # Identification
    identifier: str           # Unique ID (e.g., "DUST", "WATR")
    name: str                 # Display name
    color: Tuple[int, int, int]  # RGB color
    
    # Physics properties
    weight: int = 50          # Density (heavier sinks). 0-100, default 50
    gravity: float = 0.0      # Additional gravity force
    loss: float = 0.95        # Velocity dampening (0-1)
    collision: float = 0.0    # Collision coefficient
    diffusion: float = 0.0    # Diffusion rate
    
    # Movement behavior
    falldown: int = 0         # 0=none, 1=powder, 2=liquid, 3=gas
    
    # Thermal properties
    heat_conduct: int = 0     # Heat conductivity (0-255)
    default_temp: float = 295.15  # Default temperature (Kelvin)
    
    # State transitions
    low_temp: float = 0.0     # Freeze/condense below this temp
    low_temp_transition: int = 0   # Element to become when frozen
    high_temp: float = 0.0    # Melt/boil above this temp
    high_temp_transition: int = 0  # Element to become when heated
    
    # Interaction properties
    flammable: int = 0        # Flammability (0-1000)
    explosive: int = 0        # Explosive power (0-100)
    meltable: int = 0         # Can melt (0 or 1)
    hardness: int = 0         # Resistance to destruction (0-100)
    
    # Rendering
    menu_visible: bool = True  # Show in element menu
    menu_section: int = 0     # Category (0=powders, 1=liquids, etc.)
    
    def update(self, sim: 'PowderToySimulation', i: int, x: int, y: int):
        """
        Update function called once per frame for each particle.
        Override in subclasses for custom behavior.
        """
        pass
        
    def graphics(self, sim: 'PowderToySimulation', particle: 'Particle') -> Tuple[int, int, int]:
        """
        Get display color for this particle.
        Returns RGB tuple.
        """
        return self.color

# =============================================================================
# ELEMENT IMPLEMENTATIONS
# =============================================================================

class Element_NONE(Element):
    """Empty space / Air"""
    def __init__(self):
        super().__init__(
            identifier="NONE",
            name="Empty",
            color=(0, 0, 0),
            weight=0,
            falldown=0,
            menu_visible=False
        )

class Element_DUST(Element):
    """Basic powder - falls and piles up"""
    def __init__(self):
        super().__init__(
            identifier="DUST",
            name="Dust",
            color=(255, 224, 178),  # Light brown
            weight=75,              # Heavy powder
            gravity=0.1,
            loss=0.95,
            falldown=1,             # Powder behavior
            heat_conduct=70,
            menu_section=0          # Powders category
        )

class Element_WATR(Element):
    """Water - flows and spreads"""
    def __init__(self):
        super().__init__(
            identifier="WATR",
            name="Water",
            color=(32, 64, 255),    # Blue
            weight=20,              # Light liquid
            gravity=0.1,
            loss=0.98,
            falldown=2,             # Liquid behavior
            heat_conduct=251,       # High heat conductivity
            default_temp=295.15,    # Room temp
            low_temp=273.15,        # Freezes at 0°C
            low_temp_transition=0,  # TODO: PT_ICE
            high_temp=373.15,       # Boils at 100°C
            high_temp_transition=0, # TODO: PT_WTRV (steam)
            menu_section=1          # Liquids category
        )
        
    def update(self, sim, i, x, y):
        """Water-specific behavior: spreads horizontally"""
        p = sim.particles[i]
        if p is None:
            return
            
        # Try to spread left/right
        if random.random() < 0.5:
            direction = 1 if random.random() < 0.5 else -1
            nx = x + direction
            if 0 <= nx < sim.XRES and sim.pmap[y][nx] == 0:
                p.vx += direction * 0.5

class Element_SAND(Element):
    """Heavy sand"""
    def __init__(self):
        super().__init__(
            identifier="SAND",
            name="Sand",
            color=(255, 204, 0),    # Yellow (Powder Toy color!)
            weight=90,              # Very heavy
            gravity=0.15,
            loss=0.90,
            falldown=1,             # Powder behavior
            heat_conduct=70,
            high_temp=1973.15,      # Melts at 1700°C
            high_temp_transition=0, # TODO: PT_LAVA
            menu_section=0
        )

class Element_FIRE(Element):
    """Fire - rises and has limited lifetime"""
    def __init__(self):
        super().__init__(
            identifier="FIRE",
            name="Fire",
            color=(255, 100, 0),    # Orange-red
            weight=-2,              # Negative weight = rises
            gravity=-0.1,           # Upward force
            loss=0.92,
            falldown=3,             # Gas behavior
            heat_conduct=88,
            default_temp=600.0,     # Hot!
            menu_section=2          # Gases category
        )
        
    def update(self, sim, i, x, y):
        """Fire behavior: has lifetime, generates heat"""
        p = sim.particles[i]
        if p is None:
            return
            
        # Decay over time
        p.life += 1
        if p.life > 50:  # Lives for ~50 frames
            sim.delete_particle(x, y)
            return
            
        # Heat nearby particles
        for dy in [-1, 0, 1]:
            for dx in [-1, 0, 1]:
                nx, ny = x + dx, y + dy
                if 0 <= nx < sim.XRES and 0 <= ny < sim.YRES:
                    ni = sim.pmap[ny][nx]
                    if ni > 0:
                        other = sim.particles[ni - 1]
                        if other:
                            other.temp += 10.0  # Heat transfer

class Element_STONE(Element):
    """Solid stone - doesn't move"""
    def __init__(self):
        super().__init__(
            identifier="STONE",
            name="Stone",
            color=(128, 128, 128),  # Grey
            weight=100,             # Very heavy
            falldown=0,             # Solid - doesn't fall
            heat_conduct=200,
            high_temp=1973.15,      # High melting point
            high_temp_transition=0, # TODO: PT_LAVA
            hardness=50,
            menu_section=4          # Solids category
        )

class Element_LAVA(Element):
    """Molten lava - extremely hot liquid"""
    def __init__(self):
        super().__init__(
            identifier="LAVA",
            name="Lava",
            color=(255, 40, 0),     # Bright red-orange
            weight=45,              # Medium-heavy liquid
            gravity=0.1,
            loss=0.95,
            falldown=2,             # Liquid behavior
            heat_conduct=255,       # Maximum heat conductivity
            default_temp=2273.15,   # 2000°C - very hot!
            low_temp=1273.15,       # Solidifies below 1000°C
            low_temp_transition=5,  # PT_STONE
            flammable=0,
            menu_section=1          # Liquids
        )
        
    def update(self, sim, i, x, y):
        """Lava heats and ignites nearby particles"""
        p = sim.particles[i]
        if p is None:
            return
            
        # Heat nearby particles intensely
        for dy in [-1, 0, 1]:
            for dx in [-1, 0, 1]:
                if dx == 0 and dy == 0:
                    continue
                nx, ny = x + dx, y + dy
                if 0 <= nx < sim.XRES and 0 <= ny < sim.YRES:
                    ni = sim.pmap[ny][nx]
                    if ni > 0:
                        other = sim.particles[ni - 1]
                        if other:
                            other.temp += 50.0  # Intense heating

class Element_GUNP(Element):
    """Gunpowder - explodes when ignited"""
    def __init__(self):
        super().__init__(
            identifier="GUNP",
            name="Gunpowder",
            color=(128, 128, 64),   # Dark yellow-gray
            weight=85,
            gravity=0.1,
            loss=0.92,
            falldown=1,             # Powder
            heat_conduct=70,
            high_temp=673.15,       # Ignites at 400°C
            high_temp_transition=4, # PT_FIRE
            flammable=600,
            explosive=1,
            menu_section=3          # Explosives
        )

class Element_SALT(Element):
    """Salt - dissolves in water"""
    def __init__(self):
        super().__init__(
            identifier="SALT",
            name="Salt",
            color=(255, 255, 255),  # White
            weight=95,
            gravity=0.12,
            loss=0.90,
            falldown=1,             # Powder
            heat_conduct=110,
            high_temp=1074.15,      # Melts at 801°C
            menu_section=0          # Powders
        )
        
    def update(self, sim, i, x, y):
        """Salt dissolves in water"""
        p = sim.particles[i]
        if p is None:
            return
            
        # Check for water neighbors
        for dy in [-1, 0, 1]:
            for dx in [-1, 0, 1]:
                if dx == 0 and dy == 0:
                    continue
                nx, ny = x + dx, y + dy
                if 0 <= nx < sim.XRES and 0 <= ny < sim.YRES:
                    ni = sim.pmap[ny][nx]
                    if ni > 0:
                        other = sim.particles[ni - 1]
                        if other and other.type == 2:  # PT_WATR
                            # Dissolve into water
                            if random.random() < 0.05:
                                sim.delete_particle(x, y)
                                return

class Element_OIL(Element):
    """Oil - flammable liquid, floats on water"""
    def __init__(self):
        super().__init__(
            identifier="OIL",
            name="Oil",
            color=(64, 32, 0),      # Dark brown
            weight=10,              # Lighter than water!
            gravity=0.08,
            loss=0.97,
            falldown=2,             # Liquid
            heat_conduct=40,
            flammable=20,
            high_temp=533.15,       # Ignites at 260°C
            high_temp_transition=4, # PT_FIRE
            menu_section=1          # Liquids
        )

class Element_WOOD(Element):
    """Wood - burns slowly"""
    def __init__(self):
        super().__init__(
            identifier="WOOD",
            name="Wood",
            color=(139, 69, 19),    # Brown
            weight=100,
            falldown=0,             # Solid
            heat_conduct=40,
            flammable=5,
            high_temp=573.15,       # Burns at 300°C
            high_temp_transition=4, # PT_FIRE
            menu_section=4          # Solids
        )

# Update the registry function
def get_element_list():
    """Return list of all element definitions indexed by ElementType"""
    from powder_toy_engine import ElementType
    
    elements = [None] * 100
    
    elements[ElementType.PT_NONE] = Element_NONE()
    elements[ElementType.PT_DUST] = Element_DUST()
    elements[ElementType.PT_WATR] = Element_WATR()
    elements[ElementType.PT_SAND] = Element_SAND()
    elements[ElementType.PT_FIRE] = Element_FIRE()
    elements[ElementType.PT_STONE] = Element_STONE()
    elements[ElementType.PT_LAVA] = Element_LAVA()
    elements[ElementType.PT_GUNP] = Element_GUNP()
    elements[ElementType.PT_SALT] = Element_SALT()
    elements[ElementType.PT_OIL] = Element_OIL()
    elements[ElementType.PT_WOOD] = Element_WOOD()
    
    return elements
