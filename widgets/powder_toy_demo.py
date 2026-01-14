#!/usr/bin/env python3
"""
THE POWDER TOY
==============

Full implementation of The Powder Toy particle physics simulation.
Python port of the original C++ version.

Based on The Powder Toy by Stanislaw Skowronek and contributors (GPL-3.0)
https://github.com/The-Powder-Toy/The-Powder-Toy

Controls:
- Left Click/Drag: Draw selected element
- Right Click/Drag: Erase particles
- Middle Click: Sample element at cursor
- Mouse Wheel: Change brush size
- Number Keys 1-9, 0: Select element shortcuts
- SPACE: Pause/Resume simulation
- R: Reset/Clear simulation
- D: Toggle debug display
- H: Toggle help overlay
- F: Toggle FPS counter
- +/- : Increase/Decrease simulation speed
- ESC: Exit
"""

import pygame
import sys
from powder_toy_engine import PowderToySimulation, ElementType
from powder_toy_elements import Element

class PowderToy:
    """The Powder Toy - Full Implementation"""
    
    # Color scheme matching TPT
    COLOR_BG = (0, 0, 0)  # Pure black
    COLOR_UI_DARK = (26, 26, 26)
    COLOR_UI_BORDER = (60, 60, 60)
    COLOR_UI_TEXT = (255, 255, 255)
    COLOR_UI_TEXT_DIM = (150, 150, 150)
    COLOR_HIGHLIGHT = (100, 150, 255)
    
    def __init__(self):
        pygame.init()
        
        # Window setup
        self.screen_width = 650
        self.screen_height = 400
        
        import os
        import ctypes
        try:
            user32 = ctypes.windll.user32
            screensize = user32.GetSystemMetrics(0), user32.GetSystemMetrics(1) 
            x = screensize[0] - self.screen_width - 20
            y = screensize[1] - self.screen_height - 60
            os.environ['SDL_VIDEO_WINDOW_POS'] = "%d,%d" % (x,y)
        except:
            pass

        self.screen = pygame.display.set_mode((self.screen_width, self.screen_height), pygame.NOFRAME)
        pygame.display.set_caption("The Powder Toy - Corner Widget")
        
        # Simulation
        self.sim = PowderToySimulation()
        
        # Rendering settings
        self.sim_scale = 1
        self.offset_x = 10
        self.offset_y = 70
        
        # UI State
        self.selected_element = ElementType.PT_SAND
        self.brush_size = 3
        self.brush_shape = 'circle'  # circle, square, line
        self.paused = False
        self.show_help = False
        self.show_debug = False
        self.show_fps = True
        self.simulation_speed = 1
        
        # Element categories for UI
        self.categories = self._build_categories()
        self.selected_category = 0
        
        # Input state
        self.mouse_down = [False, False, False]
        self.last_mouse_pos = (0, 0)
        
        # Fonts
        self.font_small = pygame.font.Font(None, 16)
        self.font_medium = pygame.font.Font(None, 20)
        self.font_large = pygame.font.Font(None, 28)
        self.font_title = pygame.font.Font(None, 36)
        
        # Performance tracking
        self.clock = pygame.time.Clock()
        self.fps = 60
        self.frame_times = []
        
        self.running = True
        
    def _build_categories(self):
        """Build element category structure"""
        return {
            'Powders': [
                (ElementType.PT_DUST, "Dust", "1"),
                (ElementType.PT_SAND, "Sand", "2"),
                (ElementType.PT_SALT, "Salt", "3"),
                (ElementType.PT_GUNP, "Gunpowder", "4"),
            ],
            'Liquids': [
                (ElementType.PT_WATR, "Water", "5"),
                (ElementType.PT_OIL, "Oil", "6"),
                (ElementType.PT_LAVA, "Lava", "7"),
            ],
            'Gases': [
                (ElementType.PT_FIRE, "Fire", "8"),
            ],
            'Solids': [
                (ElementType.PT_STONE, "Stone", "9"),
                (ElementType.PT_WOOD, "Wood", "0"),
            ]
        }
        
    def handle_events(self):
        """Handle all user input"""
        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                self.running = False
                
            elif event.type == pygame.KEYDOWN:
                self.handle_keypress(event.key)
                
            elif event.type == pygame.MOUSEBUTTONDOWN:
                # Check if clicking UI elements first
                if self.handle_ui_click(event.pos, event.button):
                    continue  # UI element was clicked, don't draw
                    
                self.mouse_down[event.button - 1] = True
                if event.button == 2:  # Middle click - sample
                    self.sample_element()
                elif event.button == 4:  # Scroll up
                    self.brush_size = min(self.brush_size + 1, 20)
                elif event.button == 5:  # Scroll down
                    self.brush_size = max(self.brush_size - 1, 1)
                    
            elif event.type == pygame.MOUSEBUTTONUP:
                if event.button <= 3:
                    self.mouse_down[event.button - 1] = False
                    
        # Handle continuous drawing
        if self.mouse_down[0] or self.mouse_down[2]:
            self.handle_drawing()
            
    def handle_ui_click(self, pos, button) -> bool:
        """
        Handle clicks on UI elements (buttons, element panel, etc.)
        Returns True if a UI element was clicked
        """
        mouse_x, mouse_y = pos
        
        # Only handle left clicks for UI
        if button != 1:
            return False
            
        # Help button (top right corner)
        help_button_rect = pygame.Rect(self.screen_width - 100, 15, 80, 30)
        if help_button_rect.collidepoint(mouse_x, mouse_y):
            self.show_help = not self.show_help
            return True
            
        # Element selection in side panel
        panel_x = self.screen_width - 250
        if mouse_x >= panel_x and mouse_y >= 80:
            return self.handle_element_panel_click(mouse_x, mouse_y, panel_x)
            
        return False
        
    def handle_element_panel_click(self, mouse_x, mouse_y, panel_x) -> bool:
        """Handle clicks on element panel to select elements"""
        y = 80
        
        for cat_name, elements in self.categories.items():
            y += 25  # Category header
            
            for elem_id, elem_name, shortcut in elements:
                # Check if this element was clicked
                elem_rect = pygame.Rect(panel_x + 5, y - 2, 240, 22)
                if elem_rect.collidepoint(mouse_x, mouse_y):
                    self.selected_element = elem_id
                    return True
                y += 22
                
            y += 10  # Category spacing
            
        return False
            
    def handle_keypress(self, key):
        """Handle keyboard shortcuts"""
        # System controls
        if key == pygame.K_ESCAPE:
            self.running = False
        elif key == pygame.K_SPACE:
            self.paused = not self.paused
        elif key == pygame.K_r:
            self.sim.clear_sim()
        elif key == pygame.K_h:
            self.show_help = not self.show_help
        elif key == pygame.K_d:
            self.show_debug = not self.show_debug
        elif key == pygame.K_f:
            self.show_fps = not self.show_fps
        elif key == pygame.K_PLUS or key == pygame.K_EQUALS:
            self.simulation_speed = min(self.simulation_speed + 1, 10)
        elif key == pygame.K_MINUS:
            self.simulation_speed = max(self.simulation_speed - 1, 1)
            
        # Element selection shortcuts
        elif key == pygame.K_1:
            self.selected_element = ElementType.PT_DUST
        elif key == pygame.K_2:
            self.selected_element = ElementType.PT_SAND
        elif key == pygame.K_3:
            self.selected_element = ElementType.PT_SALT
        elif key == pygame.K_4:
            self.selected_element = ElementType.PT_GUNP
        elif key == pygame.K_5:
            self.selected_element = ElementType.PT_WATR
        elif key == pygame.K_6:
            self.selected_element = ElementType.PT_OIL
        elif key == pygame.K_7:
            self.selected_element = ElementType.PT_LAVA
        elif key == pygame.K_8:
            self.selected_element = ElementType.PT_FIRE
        elif key == pygame.K_9:
            self.selected_element = ElementType.PT_STONE
        elif key == pygame.K_0:
            self.selected_element = ElementType.PT_WOOD
            
    def sample_element(self):
        """Sample element under mouse cursor"""
        mouse_x, mouse_y = pygame.mouse.get_pos()
        sim_x = int((mouse_x - self.offset_x) / self.sim_scale)
        sim_y = int((mouse_y - self.offset_y) / self.sim_scale)
        
        if 0 <= sim_x < self.sim.XRES and 0 <= sim_y < self.sim.YRES:
            particle_i = self.sim.pmap[sim_y][sim_x]
            if particle_i > 0:
                p = self.sim.particles[particle_i - 1]
                if p:
                    self.selected_element = p.type
                    
    def handle_drawing(self):
        """Handle mouse drawing"""
        mouse_x, mouse_y = pygame.mouse.get_pos()
        sim_x = int((mouse_x - self.offset_x) / self.sim_scale)
        sim_y = int((mouse_y - self.offset_y) / self.sim_scale)
        
        # Draw line from last position (for smooth drawing)
        last_x, last_y = self.last_mouse_pos
        self.draw_line(last_x, last_y, sim_x, sim_y)
        self.last_mouse_pos = (sim_x, sim_y)
        
    def draw_line(self, x0, y0, x1, y1):
        """Draw particles along a line (Bresenham's algorithm)"""
        dx = abs(x1 - x0)
        dy = abs(y1 - y0)
        sx = 1 if x0 < x1 else -1
        sy = 1 if y0 < y1 else -1
        err = dx - dy
        
        while True:
            self.draw_brush(x0, y0)
            
            if x0 == x1 and y0 == y1:
                break
                
            e2 = 2 * err
            if e2 > -dy:
                err -= dy
                x0 += sx
            if e2 < dx:
                err += dx
                y0 += sy
                
    def draw_brush(self, cx, cy):
        """Draw particles with current brush"""
        element = ElementType.PT_NONE if self.mouse_down[2] else self.selected_element
        
        if self.brush_shape == 'circle':
            for dy in range(-self.brush_size, self.brush_size + 1):
                for dx in range(-self.brush_size, self.brush_size + 1):
                    if dx*dx + dy*dy <= self.brush_size*self.brush_size:
                        x, y = cx + dx, cy + dy
                        self.place_particle(x, y, element)
        elif self.brush_shape == 'square':
            for dy in range(-self.brush_size, self.brush_size + 1):
                for dx in range(-self.brush_size, self.brush_size + 1):
                    x, y = cx + dx, cy + dy
                    self.place_particle(x, y, element)
                    
    def place_particle(self, x, y, element):
        """Place or remove a particle"""
        if element == ElementType.PT_NONE:
            self.sim.delete_particle(x, y)
        else:
            self.sim.create_particle(x, y, element)
            
    def update(self):
        """Update simulation"""
        if not self.paused:
            for _ in range(self.simulation_speed):
                self.sim.update_particles()
                
    def render(self):
        """Render everything"""
        # Clear screen
        self.screen.fill(self.COLOR_BG)
        
        # Draw simulation area
        self.render_simulation()
        
        # Draw UI overlays
        self.render_top_bar()
        self.render_side_panel()
        self.render_bottom_bar()
        
        if self.show_help:
            self.render_help_overlay()
        if self.show_debug:
            self.render_debug_info()
            
        pygame.display.flip()
        
    def render_simulation(self):
        """Render the particle simulation"""
        # Background
        sim_rect = pygame.Rect(self.offset_x, self.offset_y,
                              self.sim.XRES, self.sim.YRES)
        pygame.draw.rect(self.screen, self.COLOR_BG, sim_rect)
        
        # Particles
        for y in range(self.sim.YRES):
            for x in range(self.sim.XRES):
                particle_i = self.sim.pmap[y][x]
                if particle_i == 0:
                    continue
                    
                p = self.sim.particles[particle_i - 1]
                if p is None:
                    continue
                    
                element = self.sim.elements[p.type]
                color = element.graphics(self.sim, p)
                
                screen_x = self.offset_x + x
                screen_y = self.offset_y + y
                pygame.draw.rect(self.screen, color, (screen_x, screen_y, 1, 1))
                
        # Border
        pygame.draw.rect(self.screen, self.COLOR_UI_BORDER, sim_rect, 2)
        
        # Cursor preview (brush outline)
        mouse_x, mouse_y = pygame.mouse.get_pos()
        if (self.offset_x <= mouse_x < self.offset_x + self.sim.XRES and
            self.offset_y <= mouse_y < self.offset_y + self.sim.YRES):
            pygame.draw.circle(self.screen, self.COLOR_HIGHLIGHT,
                             (mouse_x, mouse_y), self.brush_size, 1)
                             
    def render_top_bar(self):
        """Render top menu bar"""
        bar_rect = pygame.Rect(0, 0, self.screen_width, 60)
        pygame.draw.rect(self.screen, self.COLOR_UI_DARK, bar_rect)
        pygame.draw.line(self.screen, self.COLOR_UI_BORDER,
                        (0, 60), (self.screen_width, 60), 1)
        
        # Title
        title = self.font_title.render("The Powder Toy", True, self.COLOR_UI_TEXT)
        self.screen.blit(title, (20, 15))
        
        # Status indicators
        x = 300
        if self.paused:
            pause_text = self.font_medium.render("⏸ PAUSED", True, (255, 200, 0))
            self.screen.blit(pause_text, (x, 20))
            x += 120
            
        # FPS
        if self.show_fps:
            fps_text = self.font_medium.render(f"FPS: {int(self.fps)}", True, self.COLOR_UI_TEXT_DIM)
            self.screen.blit(fps_text, (x, 20))
            x += 100
            
        # Particle count
        count_text = self.font_medium.render(
            f"Particles: {self.sim.parts_active}/{self.sim.NPART}",
            True, self.COLOR_UI_TEXT_DIM
        )
        self.screen.blit(count_text, (x, 20))
        
        # Speed
        speed_text = self.font_medium.render(f"Speed: {self.simulation_speed}x", True, self.COLOR_UI_TEXT_DIM)
        self.screen.blit(speed_text, (self.screen_width - 270, 20))
        
        # Help Button (clickable!)
        help_button_rect = pygame.Rect(self.screen_width - 100, 15, 80, 30)
        button_color = self.COLOR_HIGHLIGHT if self.show_help else self.COLOR_UI_BORDER
        pygame.draw.rect(self.screen, button_color, help_button_rect, 2, border_radius=5)
        help_text = self.font_medium.render("? Help", True, self.COLOR_UI_TEXT)
        self.screen.blit(help_text, (self.screen_width - 90, 20))
        
    def render_side_panel(self):
        """Render right-side element panel"""
        panel_x = self.screen_width - 250
        panel_rect = pygame.Rect(panel_x, 60, 250, self.screen_height - 60)
        pygame.draw.rect(self.screen, self.COLOR_UI_DARK, panel_rect)
        pygame.draw.line(self.screen, self.COLOR_UI_BORDER,
                        (panel_x, 60), (panel_x, self.screen_height), 1)
        
        y = 80
        
        # Category tabs
        for cat_name, elements in self.categories.items():
            # Category header
            cat_text = self.font_medium.render(cat_name, True, self.COLOR_UI_TEXT)
            self.screen.blit(cat_text, (panel_x + 10, y))
            y += 25
            
            # Elements in category
            for elem_id, elem_name, shortcut in elements:
                element = self.sim.elements[elem_id]
                
                # Highlight if selected
                if self.selected_element == elem_id:
                    highlight_rect = pygame.Rect(panel_x + 5, y - 2, 240, 24)
                    pygame.draw.rect(self.screen, self.COLOR_HIGHLIGHT, highlight_rect)
                
                # Element color swatch
                color_rect = pygame.Rect(panel_x + 15, y + 2, 20, 16)
                pygame.draw.rect(self.screen, element.color, color_rect)
                pygame.draw.rect(self.screen, self.COLOR_UI_BORDER, color_rect, 1)
                
                # Element name
                name_color = self.COLOR_BG if self.selected_element == elem_id else self.COLOR_UI_TEXT
                name_text = self.font_medium.render(elem_name, True, name_color)
                self.screen.blit(name_text, (panel_x + 45, y))
                
                # Shortcut key
                key_text = self.font_small.render(f"[{shortcut}]", True, self.COLOR_UI_TEXT_DIM)
                self.screen.blit(key_text, (panel_x + 200, y + 2))
                
                y += 22
            y += 10  # Space between categories
            
    def render_bottom_bar(self):
        """Render bottom control bar"""
        bar_y = self.screen_height - 40
        bar_rect = pygame.Rect(0, bar_y, self.screen_width, 40)
        pygame.draw.rect(self.screen, self.COLOR_UI_DARK, bar_rect)
        pygame.draw.line(self.screen, self.COLOR_UI_BORDER,
                        (0, bar_y), (self.screen_width, bar_y), 1)
        
        # Selected element info
        element = self.sim.elements[self.selected_element]
        info_text = self.font_medium.render(
            f"Selected: {element.name} | Brush: {self.brush_size} | "
            f"Click '? Help' button or press [H] for controls | [SPACE] Pause | [R] Reset | [ESC] Exit",
            True, self.COLOR_UI_TEXT
        )
        self.screen.blit(info_text, (20, bar_y + 10))
        
    def render_help_overlay(self):
        """Render help overlay"""
        # Semi-transparent background
        overlay = pygame.Surface((self.screen_width, self.screen_height))
        overlay.set_alpha(200)
        overlay.fill(self.COLOR_BG)
        self.screen.blit(overlay, (0, 0))
        
        # Help box
        box_width = 700
        box_height = 600
        box_x = (self.screen_width - box_width) // 2
        box_y = (self.screen_height - box_height) // 2
        
        pygame.draw.rect(self.screen, self.COLOR_UI_DARK,
                        (box_x, box_y, box_width, box_height), border_radius=10)
        pygame.draw.rect(self.screen, self.COLOR_HIGHLIGHT,
                        (box_x, box_y, box_width, box_height), 3, border_radius=10)
        
        # Title
        title = self.font_title.render("⚛️ The Powder Toy - Help & Tips", True, self.COLOR_HIGHLIGHT)
        self.screen.blit(title, (box_x + 20, box_y + 15))
        
        # Controls list
        controls = [
            "",
            "DRAWING:",
            "  • Left Click/Drag - Draw selected element",
            "  • Right Click/Drag - Erase particles",
            "  • Middle Click - Sample element under cursor",
            "  • Mouse Wheel - Change brush size",
            "  • Click elements in right panel to select them!",
            "",
            "KEYBOARD SHORTCUTS:",
            "  • Keys 1-9, 0 - Quick select elements",
            "  • SPACE - Pause/Resume simulation",
            "  • R - Reset/Clear everything",
            "  • +/- - Increase/Decrease simulation speed",
            "  • H - Toggle this help (or click ? Help button)",
            "  • D - Toggle debug info",
            "",
            "EXPERIMENT IDEAS:",
            "  🔥 Draw GUNPOWDER, then ignite it with FIRE!",
            "  💧 Pour WATER, then add OIL - watch it float!",
            "  🌋 Draw LAVA and see it heat nearby particles",
            "  💨 Watch FIRE rise (it has negative weight!)",
            "  🏖️ Build walls with STONE, fill with WATER",
            "  🌲 Stack WOOD, then burn it slowly with FIRE",
            "  ⚗️ Mix SALT and WATER - salt dissolves!",
            "",
            "PRO TIPS:",
            "  • Pause (SPACE) to place particles precisely",
            "  • Use larger brushes (scroll up) for faster building",
            "  • Right click to erase mistakes quickly",
            "  • All particles react to temperature!"
        ]
        
        y = box_y + 60
        for line in controls:
            if line.startswith("  •") or line.startswith("  🔥") or line.startswith("  💧") :
                color = self.COLOR_UI_TEXT
            elif line and not line.startswith(" "):
                color = self.COLOR_HIGHLIGHT
            else:
                color = self.COLOR_UI_TEXT_DIM
                
            text = self.font_small.render(line, True, color)
            self.screen.blit(text, (box_x + 30, y))
            y += 18
            
        # Close instruction
        close_text = self.font_medium.render("Click ? Help button or press H to close", True, self.COLOR_HIGHLIGHT)
        self.screen.blit(close_text, (box_x + box_width//2 - 180, box_y + box_height - 30))
        
    def render_debug_info(self):
        """Render debug information"""
        debug_lines = [
            f"Frame: {self.sim.frame_count}",
            f"FPS: {self.fps:.1f}",
            f"Particles: {self.sim.parts_active}/{self.sim.NPART}",
            f"Grid: {self.sim.XRES}x{self.sim.YRES}",
            f"Brush: {self.brush_size} ({self.brush_shape})",
            f"Speed: {self.simulation_speed}x",
        ]
        
        y = 70
        for line in debug_lines:
            text = self.font_small.render(line, True, (0, 255, 0))
            self.screen.blit(text, (10, y))
            y += 18
            
    def run(self):
        """Main game loop"""
        while self.running:
            # Timing
            dt = self.clock.tick(60) / 1000.0
            self.fps = self.clock.get_fps()
            
            # Process
            self.handle_events()
            self.update()
            self.render()
            
        pygame.quit()
        sys.exit()

if __name__ == "__main__":
    print("╔═══════════════════════════════════════════╗")
    print("║       THE POWDER TOY - Python Port       ║")
    print("║  Falling Sand Particle Physics Engine    ║")
    print("╚═══════════════════════════════════════════╝")
    print()
    print("Based on The Powder Toy (GPL-3.0)")
    print("Original: https://github.com/The-Powder-Toy/The-Powder-Toy")
    print()
    print("Starting simulation...")
    print()
    
    app = PowderToy()
    app.run()
