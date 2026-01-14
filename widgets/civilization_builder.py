# ... (continuation of civilization_builder.py from previous part)

# =============================================================================
# UI AND RENDERING
# =============================================================================

class CivUI:
    """User interface for Civilization Builder"""
    
    # Color scheme
    COLOR_BG = (20, 20, 30)
    COLOR_PANEL = (40, 40, 50)
    COLOR_BORDER = (80, 80, 100)
    COLOR_TEXT = (255, 255, 255)
    COLOR_TEXT_DIM = (150, 150, 150)
    COLOR_HIGHLIGHT = (100, 150, 255)
    COLOR_BUTTON = (60, 60, 80)
    COLOR_BUTTON_HOVER = (80, 80, 120)
    
    def __init__(self, screen: pygame.Surface, game: CivilizationGame):
        self.screen = screen
        self.game = game
        self.screen_width, self.screen_height = screen.get_size()
        
        # Camera
        self.camera_x = 400
        self.camera_y = 300
        
        # Selection
        self.selected_hex: Optional[HexCoord] = None
        self.selected_unit: Optional[Unit] = None
        self.selected_city: Optional[City] = None
        
        # UI state
        self.show_help = False
        
        # Fonts
        self.font_small = pygame.font.Font(None, 16)
        self.font_medium = pygame.font.Font(None, 20)
        self.font_large = pygame.font.Font(None, 28)
        self.font_title = pygame.font.Font(None, 36)
        
    def render(self):
        """Main render function"""
        self.screen.fill(self.COLOR_BG)
        
        # Render map
        self.render_map()
        
        # Render UI panels
        self.render_top_bar()
        self.render_side_panel()
        self.render_bottom_bar()
        
        if self.show_help:
            self.render_help()
            
    def render_map(self):
        """Render the hexagonal map"""
        for coord, tile in self.game.map.items():
            pixel_x, pixel_y = hex_to_pixel(coord, self.camera_x, self.camera_y)
            
            # Skip if off-screen
            if pixel_x < -100 or pixel_x > self.screen_width + 100:
                continue
            if pixel_y < -100 or pixel_y > self.screen_height + 100:
                continue
            
            # Draw hex
            color = tile.get_color()
            draw_hexagon(self.screen, (pixel_x, pixel_y), HEX_SIZE, color)
            
            # Draw border
            border_color = (0, 0, 0, 50)
            draw_hexagon(self.screen, (pixel_x, pixel_y), HEX_SIZE, border_color, 1)
            
            # Highlight if selected
            if self.selected_hex == coord:
                draw_hexagon(self.screen, (pixel_x, pixel_y), HEX_SIZE, COLOR_SELECTED, 3)
            
            # Draw resource icon
            if tile.resource != ResourceType.NONE:
                self.draw_resource_icon(pixel_x, pixel_y, tile.resource)
        
        # Draw cities
        for civ in self.game.civilizations:
            for city in civ.cities:
                pixel_x, pixel_y = hex_to_pixel(city.position, self.camera_x, self.camera_y)
                pygame.draw.circle(self.screen, civ.color, (pixel_x, pixel_y), 15)
                pygame.draw.circle(self.screen, (255, 255, 255), (pixel_x, pixel_y), 15, 2)
                
                # City name
                name_surf = self.font_small.render(city.name, True, self.COLOR_TEXT)
                self.screen.blit(name_surf, (pixel_x - name_surf.get_width()//2, pixel_y - 30))
        
        # Draw units
        for civ in self.game.civilizations:
            for unit in civ.units:
                pixel_x, pixel_y = hex_to_pixel(unit.position, self.camera_x, self.camera_y)
                
                # Unit icon (simple square for now)
                rect = pygame.Rect(pixel_x - 8, pixel_y - 8, 16, 16)
                pygame.draw.rect(self.screen, civ.color, rect)
                pygame.draw.rect(self.screen, (255, 255, 255), rect, 1)
                
                # Highlight selected unit
                if self.selected_unit == unit:
                    pygame.draw.rect(self.screen, COLOR_SELECTED, rect, 2)
    
    def draw_resource_icon(self, x: int, y: int, resource: ResourceType):
        """Draw small icon for resource"""
        icon_colors = {
            ResourceType.IRON: (128, 128, 128),
            ResourceType.HORSES: (139, 69, 19),
            ResourceType.WHEAT: (255, 215, 0),
            ResourceType.GOLD: (255, 215, 0),
            ResourceType.GEMS: (147, 112, 219),
        }
        color = icon_colors.get(resource, (255, 255, 255))
        pygame.draw.circle(self.screen, color, (x, y), 5)
    
    def render_top_bar(self):
        """Render top status bar"""
        bar_rect = pygame.Rect(0, 0, self.screen_width, 50)
        pygame.draw.rect(self.screen, self.COLOR_PANEL, bar_rect)
        pygame.draw.line(self.screen, self.COLOR_BORDER, (0, 50), (self.screen_width, 50), 2)
        
        civ = self.game.get_current_civ()
        
        # Title
        title = self.font_title.render("Civilization Builder", True, self.COLOR_TEXT)
        self.screen.blit(title, (20, 10))
        
        # Current civ & turn
        info_text = self.font_medium.render(
            f"{civ.name} | Turn {self.game.turn_number} | Gold: {civ.gold} | Science: {civ.science}",
            True, self.COLOR_TEXT_DIM
        )
        self.screen.blit(info_text, (300, 15))
        
        # Help button
        help_rect = pygame.Rect(self.screen_width - 100, 10, 80, 30)
        pygame.draw.rect(self.screen, self.COLOR_BUTTON, help_rect, border_radius=5)
        pygame.draw.rect(self.screen, self.COLOR_BORDER, help_rect, 2, border_radius=5)
        help_text = self.font_medium.render("? Help", True, self.COLOR_TEXT)
        self.screen.blit(help_text, (self.screen_width - 85, 15))
    
    def render_side_panel(self):
        """Render right side information panel"""
        panel_x = self.screen_width - 300
        panel_rect = pygame.Rect(panel_x, 50, 300, self.screen_height - 100)
        pygame.draw.rect(self.screen, self.COLOR_PANEL, panel_rect)
        pygame.draw.line(self.screen, self.COLOR_BORDER, (panel_x, 50), (panel_x, self.screen_height - 50), 2)
        
        y = 70
        
        # Selected tile info
        if self.selected_hex and self.selected_hex in self.game.map:
            tile = self.game.map[self.selected_hex]
            
            title = self.font_large.render("Selected Tile", True, self.COLOR_TEXT)
            self.screen.blit(title, (panel_x + 10, y))
            y += 30
            
            # Terrain type
            terrain_names = {
                TerrainType.OCEAN: "Ocean",
                TerrainType.GRASSLAND: "Grassland",
                TerrainType.PLAINS: "Plains",
                TerrainType.DESERT: "Desert",
                TerrainType.TUNDRA: "Tundra",
                TerrainType.MOUNTAIN: "Mountain",
            }
            terrain_text = self.font_medium.render(
                f"Terrain: {terrain_names.get(tile.terrain, 'Unknown')}",
                True, self.COLOR_TEXT_DIM
            )
            self.screen.blit(terrain_text, (panel_x + 10, y))
            y += 25
        
        # Selected unit info
        if self.selected_unit:
            y += 20
            title = self.font_large.render("Selected Unit", True, self.COLOR_TEXT)
            self.screen.blit(title, (panel_x + 10, y))
            y += 30
            
            unit_text = self.font_medium.render(
                f"{self.selected_unit.get_name()}",
                True, self.COLOR_TEXT
            )
            self.screen.blit(unit_text, (panel_x + 10, y))
            y += 25
            
            health_text = self.font_small.render(
                f"Health: {self.selected_unit.health}/100",
                True, self.COLOR_TEXT_DIM
            )
            self.screen.blit(health_text, (panel_x + 10, y))
            y += 20
            
            movement_text = self.font_small.render(
                f"Movement: {self.selected_unit.movement_left}/{self.selected_unit.movement}",
                True, self.COLOR_TEXT_DIM
            )
            self.screen.blit(movement_text, (panel_x + 10, y))
    
    def render_bottom_bar(self):
        """Render bottom control bar"""
        bar_y = self.screen_height - 50
        bar_rect = pygame.Rect(0, bar_y, self.screen_width, 50)
        pygame.draw.rect(self.screen, self.COLOR_PANEL, bar_rect)
        pygame.draw.line(self.screen, self.COLOR_BORDER, (0, bar_y), (self.screen_width, bar_y), 2)
        
        # End Turn button
        button_rect = pygame.Rect(self.screen_width - 150, bar_y + 10, 130, 30)
        pygame.draw.rect(self.screen, self.COLOR_HIGHLIGHT, button_rect, border_radius=5)
        button_text = self.font_medium.render("End Turn", True, self.COLOR_TEXT)
        self.screen.blit(button_text, (self.screen_width - 135, bar_y + 15))
        
        # Controls hint
        controls_text = self.font_small.render(
            "Click tiles to select | WASD to move camera | [H] Help | [ESC] Exit",
            True, self.COLOR_TEXT_DIM
        )
        self.screen.blit(controls_text, (20, bar_y + 15))
    
    def render_help(self):
        """Render help overlay"""
        overlay = pygame.Surface((self.screen_width, self.screen_height))
        overlay.set_alpha(220)
        overlay.fill(self.COLOR_BG)
        self.screen.blit(overlay, (0, 0))
        
        box_width = 600
        box_height = 500
        box_x = (self.screen_width - box_width) // 2
        box_y = (self.screen_height - box_height) // 2
        
        pygame.draw.rect(self.screen, self.COLOR_PANEL,
                        (box_x, box_y, box_width, box_height), border_radius=10)
        pygame.draw.rect(self.screen, self.COLOR_HIGHLIGHT,
                        (box_x, box_y, box_width, box_height), 3, border_radius=10)
        
        title = self.font_title.render("⚔️ Civilization Builder - Help", True, self.COLOR_HIGHLIGHT)
        self.screen.blit(title, (box_x + 20, box_y + 15))
        
        help_lines = [
            "",
            "CONTROLS:",
            "  • Click tiles to select them",
            "  • Click units to select them",
            "  • WASD or Arrow Keys - Move camera",
            "  • H - Toggle this help",
            "  • ESC - Exit game",
            "",
            "GAMEPLAY:",
            "  • Build cities to grow your civilization",
            "  • Research technologies to advance",
            "  • Train units to defend and conquer",
            "  • Manage resources and production",
            "  • End your turn to let other civilizations play",
            "",
            "GOALS:",
            "  • Expand your empire across the map",
            "  • Research all technologies (Science Victory)",
            "  • Conquer all other civilizations (Domination)",
            "",
            "Press H to close"
        ]
        
        y = box_y + 60
        for line in help_lines:
            if line.startswith("  •"):
                color = self.COLOR_TEXT
            elif line and not line.startswith(" "):
                color = self.COLOR_HIGHLIGHT
            else:
                color = self.COLOR_TEXT_DIM
                
            text = self.font_small.render(line, True, color)
            self.screen.blit(text, (box_x + 30, y))
            y += 22

# =============================================================================
# MAIN APPLICATION
# =============================================================================

class CivilizationBuilder:
    """Main application class"""
    
    def __init__(self):
        pygame.init()
        
        self.screen_width = 700
        self.screen_height = 500
        
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
        pygame.display.set_caption("Civilization Builder - Widget")
        
        self.game = CivilizationGame()
        self.ui = CivUI(self.screen, self.game)
        
        self.clock = pygame.time.Clock()
        self.running = True
        
    def handle_events(self):
        """Handle user input"""
        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                self.running = False
                
            elif event.type == pygame.KEYDOWN:
                if event.key == pygame.K_ESCAPE:
                    self.running = False
                elif event.key == pygame.K_h:
                    self.ui.show_help = not self.ui.show_help
                    
                # Camera movement
                elif event.key == pygame.K_w or event.key == pygame.K_UP:
                    self.ui.camera_y += 50
                elif event.key == pygame.K_s or event.key == pygame.K_DOWN:
                    self.ui.camera_y -= 50
                elif event.key == pygame.K_a or event.key == pygame.K_LEFT:
                    self.ui.camera_x += 50
                elif event.key == pygame.K_d or event.key == pygame.K_RIGHT:
                    self.ui.camera_x -= 50
                    
            elif event.type == pygame.MOUSEBUTTONDOWN:
                if event.button == 1:  # Left click
                    self.handle_click(event.pos)
                    
    def handle_click(self, pos):
        """Handle mouse click"""
        mouse_x, mouse_y = pos
        
        # Check End Turn button
        if mouse_y > self.screen_height - 50:
            if mouse_x > self.screen_width - 150:
                self.game.end_turn()
                return
        
        # Check help button
        if mouse_y < 50 and mouse_x > self.screen_width - 100:
            self.ui.show_help = not self.ui.show_help
            return
        
        # Convert to hex coordinate
        hex_coord = pixel_to_hex(mouse_x, mouse_y, self.ui.camera_x, self.ui.camera_y)
        
        if hex_coord in self.game.map:
            self.ui.selected_hex = hex_coord
            
            # Check if unit is on this tile
            civ = self.game.get_current_civ()
            for unit in civ.units:
                if unit.position == hex_coord:
                    self.ui.selected_unit = unit
                    return
            
            self.ui.selected_unit = None
    
    def update(self):
        """Update game state"""
        # Game logic updates would go here
        pass
    
    def render(self):
        """Render everything"""
        self.ui.render()
        pygame.display.flip()
    
    def run(self):
        """Main game loop"""
        while self.running:
            self.handle_events()
            self.update()
            self.render()
            self.clock.tick(30)  # 30 FPS
            
        pygame.quit()

if __name__ == "__main__":
    print("╔════════════════════════════════════════╗")
    print("║    CIVILIZATION BUILDER - 4X Game     ║")
    print("║   Build • Research • Conquer • Win!   ║")
    print("╚════════════════════════════════════════╝")
    print()
    print("Starting game...")
    
    app = CivilizationBuilder()
    app.run()
