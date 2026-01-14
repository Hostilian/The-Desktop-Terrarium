#!/usr/bin/env python3
"""
POWDER TOY PHYSICS ENGINE
=========================

A Python implementation of The Powder Toy's falling-sand particle simulation,
based on the C++ source code from https://github.com/The-Powder-Toy/The-Powder-Toy

This is a derivative work licensed under GPL-3.0, the same license as The Powder Toy.
Original work Copyright (c) 2008-2024 Stanislaw Skowronek and contributors.
Python implementation Copyright (c) 2026.

License: GPL-3.0
"""

import pygame
import random
import math
from dataclasses import dataclass
from typing import List, Optional, Tuple
from enum import IntEnum

# =============================================================================
# ELEMENT TYPE DEFINITIONS
# =============================================================================

class ElementType(IntEnum):
    """Element IDs - mirrors The Powder Toy's PT_* constants"""
    PT_NONE = 0    # Empty/Air
    PT_DUST = 1    # Basic powder
    PT_WATR = 2    # Water
    PT_SAND = 3    # Heavy sand
    PT_FIRE = 4    # Fire gas
    PT_STONE = 5   # Solid stone
    PT_LAVA = 6    # Molten lava
    PT_GUNP = 7    # Gunpowder
    PT_SALT = 8    # Salt
    PT_OIL = 9     # Oil
    PT_WOOD = 10   # Wood

# =============================================================================
# PARTICLE SYSTEM
# =============================================================================

@dataclass
class Particle:
    """
    Represents a single particle in the simulation.
    Based on TPT's Particle struct from Particle.h
    """
    type: int           # Element type (ElementType enum)
    x: float           # X position (sub-pixel precision)
    y: float           # Y position
    vx: float = 0.0    # X velocity
    vy: float = 0.0    # Y velocity
    temp: float = 295.15  # Temperature in Kelvin (default 22°C)
    life: int = 0      # Lifetime counter / general purpose int
    ctype: int = 0     # Carries type (for colored elements)
    tmp: int = 0       # General purpose storage 1
    tmp2: int = 0      # General purpose storage 2
    flags: int = 0     # Particle flags
    dcolour: int = 0   # Decoration color (ARGB)

# =============================================================================
# SIMULATION CORE
# =============================================================================

class PowderToySimulation:
    """
    Main simulation class - handles particle physics, grid management,
    and the update loop. Based on TPT's Simulation class.
    """
    
    # Simulation dimensions (matching TPT defaults)
    XRES = 400  # Simulation width in pixels (Reduced for widget)
    YRES = 250  # Simulation height in pixels (Reduced for widget)
    CELL = 4    # Cell size for air simulation (XRES/CELL x YRES/CELL grid)
    
    # Grid cell counts
    XCELLS = XRES // CELL  # 153 cells
    YCELLS = YRES // CELL  # 96 cells
    
    # Particle limits
    NPART = 5000  # Maximum particles (TPT uses ~50,000, we start smaller)
    
    def __init__(self):
        """Initialize the simulation"""
        
        # Particle storage
        self.particles: List[Optional[Particle]] = [None] * self.NPART
        self.pfree = 0  # Next free particle index
        self.parts_active = 0  # Count of active particles
        
        # Particle map: pmap[y][x] = particle index (0 = empty)
        # This lets us quickly find which particle is at a given position
        self.pmap = [[0] * self.XRES for _ in range(self.YRES)]
        
        # Photon layer (separate from normal particles, for PHOT element)
        self.photons = [[0] * self.XRES for _ in range(self.YRES)]
        
        # Air simulation grids
        self.vx = [[0.0] * self.XCELLS for _ in range(self.YCELLS)]  # Air velocity X
        self.vy = [[0.0] * self.XCELLS for _ in range(self.YCELLS)]  # Air velocity Y
        self.pv = [[0.0] * self.XCELLS for _ in range(self.YCELLS)]  # Air pressure
        self.hv = [[0.0] * self.XCELLS for _ in range(self.YCELLS)]  # Heat (temperature)
        
        # Elements registry
        self.elements = self._initialize_elements()
        
        # Simulation state
        self.frame_count = 0
        self.paused = False
        
    def _initialize_elements(self):
        """Initialize element definitions"""
        from powder_toy_elements import get_element_list
        return get_element_list()
        
    def create_particle(self, x: int, y: int, element_type: int) -> Optional[int]:
        """
        Create a new particle at the given position.
        Returns the particle index, or None if failed.
        
        Based on TPT's create_part function.
        """
        # Check bounds
        if x < 0 or x >= self.XRES or y < 0 or y >= self.YRES:
            return None
            
        # Check if position is occupied
        if self.pmap[y][x] != 0:
            return None
            
        # Find free particle slot
        if self.pfree >= self.NPART:
            return None  # Too many particles
            
        i = self.pfree
        self.pfree += 1
        self.parts_active += 1
        
        # Create particle
        element = self.elements[element_type]
        p = Particle(
            type=element_type,
            x=float(x),
            y=float(y),
            temp=element.default_temp
        )
        
        self.particles[i] = p
        self.pmap[y][x] = i + 1  # Store index+1 (0 means empty)
        
        return i
        
    def delete_particle(self, x: int, y: int):
        """Delete particle at position"""
        if x < 0 or x >= self.XRES or y < 0 or y >= self.YRES:
            return
            
        i = self.pmap[y][x]
        if i == 0:
            return
            
        # Clear particle
        self.particles[i - 1] = None
        self.pmap[y][x] = 0
        self.parts_active -= 1
        
    def update_particles(self):
        """
        Main particle update loop - called once per frame.
        Based on TPT's UpdateParticles function.
        """
        for i in range(self.NPART):
            p = self.particles[i]
            if p is None or p.type == ElementType.PT_NONE:
                continue
                
            x, y = int(p.x), int(p.y)
            element = self.elements[p.type]
            
            # 1. Element-specific update (custom behaviors)
            element.update(self, i, x, y)
            
            # 2. Physics: Apply gravity and movement
            self._update_particle_physics(i)
            
            # 3. Heat transfer
            self._update_particle_heat(i)
            
        self.frame_count += 1
        
    def _update_particle_physics(self, i: int):
        """Update particle position based on velocity and gravity"""
        p = self.particles[i]
        if p is None:
            return
            
        element = self.elements[p.type]
        
        # Apply gravity
        if element.weight > 0:
            p.vy += element.gravity + 0.1  # Base gravity
            
        # Apply velocity dampening
        p.vx *= element.loss
        p.vy *= element.loss
        
        # Update position
        new_x = p.x + p.vx
        new_y = p.y + p.vy
        
        # Try to move particle
        self._try_move_particle(i, new_x, new_y)
        
    def _try_move_particle(self, i: int, new_x: float, new_y: float):
        """Attempt to move particle to new position"""
        p = self.particles[i]
        if p is None:
            return
            
        old_x, old_y = int(p.x), int(p.y)
        target_x, target_y = int(new_x), int(new_y)
        
        # Check bounds
        if target_x < 0 or target_x >= self.XRES or target_y < 0 or target_y >= self.YRES:
            # Bounce off walls
            if target_x < 0 or target_x >= self.XRES:
                p.vx *= -0.8
            if target_y < 0 or target_y >= self.YRES:
                p.vy *= -0.8
            return
            
        # Check if target position is free
        if self.pmap[target_y][target_x] == 0:
            # Clear old position
            self.pmap[old_y][old_x] = 0
            # Move to new position
            p.x, p.y = new_x, new_y
            self.pmap[target_y][target_x] = i + 1
        else:
            # Position occupied - try to swap based on density
            other_i = self.pmap[target_y][target_x] - 1
            other = self.particles[other_i]
            
            if other and self._should_swap(p, other):
                # Swap particles
                self.pmap[old_y][old_x] = other_i + 1
                self.pmap[target_y][target_x] = i + 1
                
                p.x, p.y = new_x, new_y
                other.x, other.y = float(old_x), float(old_y)
            else:
                # Can't move - stop
                p.vx *= 0.5
                p.vy *= 0.5
                
    def _should_swap(self, p1: Particle, p2: Particle) -> bool:
        """Determine if two particles should swap based on density"""
        e1 = self.elements[p1.type]
        e2 = self.elements[p2.type]
        
        # Heavier sinks below lighter
        return e1.weight > e2.weight
        
    def _update_particle_heat(self, i: int):
        """Update particle temperature and check state transitions"""
        p = self.particles[i]
        if p is None:
            return
            
        element = self.elements[p.type]
        
        # Heat conduction (simplified - full version uses neighbors)
        if element.heat_conduct > 0:
            # Ambient cooling/heating
            p.temp += (295.15 - p.temp) * 0.001
            
        # State transitions
        if element.low_temp > 0 and p.temp < element.low_temp:
            # Freeze
            if element.low_temp_transition != ElementType.PT_NONE:
                p.type = element.low_temp_transition
                
        if element.high_temp > 0 and p.temp > element.high_temp:
            # Melt/boil
            if element.high_temp_transition != ElementType.PT_NONE:
                p.type = element.high_temp_transition
                
    def clear_sim(self):
        """Clear all particles and reset simulation"""
        self.particles = [None] * self.NPART
        self.pmap = [[0] * self.XRES for _ in range(self.YRES)]
        self.photons = [[0] * self.XRES for _ in range(self.YRES)]
        self.pfree = 0
        self.parts_active = 0
        self.frame_count = 0
