#!/usr/bin/env python3
"""
2048 - Corner Widget Edition
===========================
A clean, neon-styled implementation of 2048 designed to sit in the corner of your screen.
"""

import pygame
import random
import os
import sys
from score_manager import ScoreManager
from arcade_sound import ArcadeSound

# =============================================================================
# CONFIGURATION
# =============================================================================

WINDOW_WIDTH = 400
WINDOW_HEIGHT = 500  # Extra height for score/header
GRID_SIZE = 4
TILE_SIZE = 80
GRID_PADDING = 10
ANIMATION_SPEED = 0.2

# Colors (Neon/Dark Theme)
COLORS = {
    'background': (20, 20, 25),
    'grid_bg': (40, 40, 50),
    'text': (255, 255, 255),
    'score_label': (150, 150, 150),
    'score_bg': (60, 60, 70),
    
    # Tile Colors (Value: (Bg Color, Text Color))
    0: ((50, 50, 60), None),
    2: ((238, 228, 218), (119, 110, 101)),
    4: ((237, 224, 200), (119, 110, 101)),
    8: ((242, 177, 121), (249, 246, 242)),
    16: ((245, 149, 99), (249, 246, 242)),
    32: ((246, 124, 95), (249, 246, 242)),
    64: ((246, 94, 59), (249, 246, 242)),
    128: ((237, 207, 114), (249, 246, 242)),
    256: ((237, 204, 97), (249, 246, 242)),
    512: ((237, 200, 80), (249, 246, 242)),
    1024: ((237, 197, 63), (249, 246, 242)),
    2048: ((237, 194, 46), (249, 246, 242)),
}

# =============================================================================
# GAME LOGIC
# =============================================================================

class Game2048:
    def __init__(self):
        self.grid = [[0] * GRID_SIZE for _ in range(GRID_SIZE)]
        self.score = 0
        self.won = False
        self.game_over = False
        
        # High Score
        self.score_manager = ScoreManager()
        self.high_score = self.score_manager.get_high_score('2048')

        # Add initial tiles
        self.add_tile()
        self.add_tile()

    def add_tile(self):
        empty_cells = [(r, c) for r in range(GRID_SIZE) for c in range(GRID_SIZE) if self.grid[r][c] == 0]
        if not empty_cells:
            return
        r, c = random.choice(empty_cells)
        self.grid[r][c] = 2 if random.random() < 0.9 else 4

    def compress(self, grid):
        """Slide all tiles to the left, removing zeros."""
        new_grid = [[0] * GRID_SIZE for _ in range(GRID_SIZE)]
        for r in range(GRID_SIZE):
            pos = 0
            for c in range(GRID_SIZE):
                if grid[r][c] != 0:
                    new_grid[r][pos] = grid[r][c]
                    pos += 1
        return new_grid

    def merge(self, grid):
        """Merge adjacent tiles of same value."""
        for r in range(GRID_SIZE):
            for c in range(GRID_SIZE - 1):
                if grid[r][c] != 0 and grid[r][c] == grid[r][c+1]:
                    grid[r][c] *= 2
                    grid[r][c+1] = 0
                    self.score += grid[r][c]
                    ArcadeSound.play('MERGE')
                    if self.score > self.high_score:
                        self.high_score = self.score
                        self.score_manager.save_high_score('2048', self.high_score)
                    if grid[r][c] == 2048 and not self.won:
                        self.won = True
        return grid

    def reverse(self, grid):
        return [row[::-1] for row in grid]

    def transpose(self, grid):
        return [list(row) for row in zip(*grid)]

    def move(self, direction):
        if self.game_over: return

        # Create a copy to check if anything changed
        original_grid = [row[:] for row in self.grid]
        
        if direction == 'LEFT':
            self.grid = self.compress(self.grid)
            self.grid = self.merge(self.grid)
            self.grid = self.compress(self.grid)
        elif direction == 'RIGHT':
            self.grid = self.reverse(self.grid)
            self.grid = self.compress(self.grid)
            self.grid = self.merge(self.grid)
            self.grid = self.compress(self.grid)
            self.grid = self.reverse(self.grid)
        elif direction == 'UP':
            self.grid = self.transpose(self.grid)
            self.grid = self.compress(self.grid)
            self.grid = self.merge(self.grid)
            self.grid = self.compress(self.grid)
            self.grid = self.transpose(self.grid)
        elif direction == 'DOWN':
            self.grid = self.transpose(self.grid)
            self.grid = self.reverse(self.grid)
            self.grid = self.compress(self.grid)
            self.grid = self.merge(self.grid)
            self.grid = self.compress(self.grid)
            self.grid = self.reverse(self.grid)
            self.grid = self.transpose(self.grid)

        if self.grid != original_grid:
            self.add_tile()
            if not self.can_move():
                self.game_over = True
                ArcadeSound.play('GAME_OVER')

    def can_move(self):
        for r in range(GRID_SIZE):
            for c in range(GRID_SIZE):
                if self.grid[r][c] == 0: return True
                if c < GRID_SIZE - 1 and self.grid[r][c] == self.grid[r][c+1]: return True
                if r < GRID_SIZE - 1 and self.grid[r][c] == self.grid[r+1][c]: return True
        return False

# =============================================================================
# UI & RENDERING
# =============================================================================

def set_window_position_bottom_right():
    """Positions the window in the bottom-right corner."""
    try:
        import ctypes
        user32 = ctypes.windll.user32
        screensize = user32.GetSystemMetrics(0), user32.GetSystemMetrics(1) 
        x = screensize[0] - WINDOW_WIDTH - 20
        y = screensize[1] - WINDOW_HEIGHT - 60 # Account for taskbar approx
        os.environ['SDL_VIDEO_WINDOW_POS'] = "%d,%d" % (x,y)
    except:
        # Fallback if ctypes fails or not windows
        os.environ['SDL_VIDEO_WINDOW_POS'] = "100,100"

def main():
    set_window_position_bottom_right()
    pygame.init()
    
    # Setup window
    screen = pygame.display.set_mode((WINDOW_WIDTH, WINDOW_HEIGHT), pygame.NOFRAME)
    pygame.display.set_caption("2048 Widget")
    clock = pygame.time.Clock()
    
    # Fonts
    font_large = pygame.font.Font(None, 48)
    font_medium = pygame.font.Font(None, 36)
    font_small = pygame.font.Font(None, 24)
    
    game = Game2048()
    running = True

    while running:
        # Event Handling
        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                running = False
            elif event.type == pygame.KEYDOWN:
                if event.key == pygame.K_ESCAPE:
                    running = False
                elif event.key == pygame.K_LEFT:
                    game.move('LEFT')
                elif event.key == pygame.K_RIGHT:
                    game.move('RIGHT')
                elif event.key == pygame.K_UP:
                    game.move('UP')
                elif event.key == pygame.K_DOWN:
                    game.move('DOWN')
                elif event.key == pygame.K_r: # Reset
                    game = Game2048()

        # Rendering
        screen.fill(COLORS['background'])

        # Header
        title = font_large.render("2048", True, COLORS['text'])
        screen.blit(title, (20, 20))
        
        sub = font_small.render("Corner Widget", True, COLORS['score_label'])
        screen.blit(sub, (22, 55))

        # Score Box
        score_bg_rect = pygame.Rect(WINDOW_WIDTH - 110, 20, 90, 50)
        pygame.draw.rect(screen, COLORS['score_bg'], score_bg_rect, border_radius=5)
        
        score_label = font_small.render("SCORE", True, COLORS['score_label'])
        score_val = font_medium.render(str(game.score), True, COLORS['text'])
        screen.blit(score_label, (score_bg_rect.centerx - score_label.get_width()//2, score_bg_rect.y + 5))
        screen.blit(score_val, (score_bg_rect.centerx - score_val.get_width()//2, score_bg_rect.y + 25))

        # Best Score Box
        best_bg_rect = pygame.Rect(WINDOW_WIDTH - 210, 20, 90, 50)
        pygame.draw.rect(screen, COLORS['score_bg'], best_bg_rect, border_radius=5)
        
        best_label = font_small.render("BEST", True, COLORS['score_label'])
        best_val = font_medium.render(str(game.high_score), True, COLORS['text'])
        screen.blit(best_label, (best_bg_rect.centerx - best_label.get_width()//2, best_bg_rect.y + 5))
        screen.blit(best_val, (best_bg_rect.centerx - best_val.get_width()//2, best_bg_rect.y + 25))

        # Grid Background
        grid_area_size = WINDOW_WIDTH - 20
        grid_start_y = 100
        pygame.draw.rect(screen, COLORS['grid_bg'], (10, grid_start_y, grid_area_size, grid_area_size), border_radius=10)

        # Draw Tiles
        cell_size = (grid_area_size - (GRID_SIZE + 1) * GRID_PADDING) // GRID_SIZE
        
        for r in range(GRID_SIZE):
            for c in range(GRID_SIZE):
                val = game.grid[r][c]
                x = 10 + GRID_PADDING + c * (cell_size + GRID_PADDING)
                y = grid_start_y + GRID_PADDING + r * (cell_size + GRID_PADDING)
                
                rect = pygame.Rect(x, y, cell_size, cell_size)
                
                # Determine colors
                bg_color = COLORS.get(val, COLORS[2048])[0] if val > 0 else COLORS[0][0]
                text_color = COLORS.get(val, COLORS[2048])[1] if val > 0 else None
                
                pygame.draw.rect(screen, bg_color, rect, border_radius=5)
                
                if val > 0:
                    text_surf = font_medium.render(str(val), True, text_color)
                    text_rect = text_surf.get_rect(center=rect.center)
                    screen.blit(text_surf, text_rect)

        # Game Over / Win Overlay
        if game.game_over:
            overlay = pygame.Surface((WINDOW_WIDTH, WINDOW_HEIGHT))
            overlay.set_alpha(200)
            overlay.fill(COLORS['background'])
            screen.blit(overlay, (0, 0))
            
            msg = font_large.render("Game Over!", True, COLORS['text'])
            screen.blit(msg, (WINDOW_WIDTH//2 - msg.get_width()//2, WINDOW_HEIGHT//2 - 20))
            
            sub = font_small.render("Press R to restart", True, COLORS['text'])
            screen.blit(sub, (WINDOW_WIDTH//2 - sub.get_width()//2, WINDOW_HEIGHT//2 + 30))

        # Footer / Controls Hint
        hint = font_small.render("Arrows to Move • R to Reset • Esc to Quit", True, COLORS['score_label'])
        screen.blit(hint, (WINDOW_WIDTH//2 - hint.get_width()//2, WINDOW_HEIGHT - 30))

        # Draw border
        pygame.draw.rect(screen, (50, 50, 60), (0, 0, WINDOW_WIDTH, WINDOW_HEIGHT), 2)

        pygame.display.flip()
        clock.tick(60)

    pygame.quit()

if __name__ == "__main__":
    main()
