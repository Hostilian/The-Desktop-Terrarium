#!/usr/bin/env python3
"""
Tetris - Corner Widget Edition
=============================
A clean, minimal implementation of Tetris designed to sit in the corner.
"""

import pygame
import random
import os
from score_manager import ScoreManager
from arcade_sound import ArcadeSound

# =============================================================================
# CONFIGURATION
# =============================================================================

WINDOW_WIDTH = 300
WINDOW_HEIGHT = 500
BLOCK_SIZE = 25
GRID_WIDTH = 10
GRID_HEIGHT = 18  # Fits nicely in 500px height
SIDE_PANEL = WINDOW_WIDTH - (GRID_WIDTH * BLOCK_SIZE)

# Colors
COLORS = {
    'background': (20, 20, 25),
    'grid_bg': (30, 30, 40),
    'text': (255, 255, 255),
    'border': (50, 50, 60),
    # Tetromino colors
    'I': (0, 240, 240),
    'J': (0, 0, 240),
    'L': (240, 160, 0),
    'O': (240, 240, 0),
    'S': (0, 240, 0),
    'T': (160, 0, 240),
    'Z': (240, 0, 0)
}

# Shapes
SHAPES = {
    'I': [[1, 1, 1, 1]],
    'J': [[1, 0, 0], [1, 1, 1]],
    'L': [[0, 0, 1], [1, 1, 1]],
    'O': [[1, 1], [1, 1]],
    'S': [[0, 1, 1], [1, 1, 0]],
    'T': [[0, 1, 0], [1, 1, 1]],
    'Z': [[1, 1, 0], [0, 1, 1]]
}

# =============================================================================
# GAME LOGIC
# =============================================================================

class TetrisGame:
    def __init__(self):
        self.grid = [[0 for _ in range(GRID_WIDTH)] for _ in range(GRID_HEIGHT)]
        self.score = 0
        self.score_manager = ScoreManager()
        self.high_score = self.score_manager.get_high_score('Tetris')
        self.game_over = False
        self.paused = False
        
        self.current_piece = self.new_piece()
        self.next_piece = self.new_piece()
        
        self.piece_x = GRID_WIDTH // 2 - len(self.current_piece['shape'][0]) // 2
        self.piece_y = 0

    def new_piece(self):
        shape_name = random.choice(list(SHAPES.keys()))
        return {
            'shape': SHAPES[shape_name],
            'color': COLORS[shape_name], 
            'name': shape_name
        }

    def check_collision(self, shape, offset_x, offset_y):
        for y, row in enumerate(shape):
            for x, cell in enumerate(row):
                if cell:
                    try:
                        if (offset_x + x < 0 or 
                            offset_x + x >= GRID_WIDTH or 
                            offset_y + y >= GRID_HEIGHT or 
                            self.grid[offset_y + y][offset_x + x]):
                            return True
                    except IndexError:
                        return True
        return False

    def rotate_shape(self, shape):
        return [list(row) for row in zip(*shape[::-1])]

    def merge_piece(self):
        for y, row in enumerate(self.current_piece['shape']):
            for x, cell in enumerate(row):
                if cell:
                    self.grid[self.piece_y + y][self.piece_x + x] = self.current_piece['color']
        
        self.clear_lines()
        self.current_piece = self.next_piece
        self.next_piece = self.new_piece()
        self.piece_x = GRID_WIDTH // 2 - len(self.current_piece['shape'][0]) // 2
        self.piece_y = 0
        
        if self.check_collision(self.current_piece['shape'], self.piece_x, self.piece_y):
            self.game_over = True
            ArcadeSound.play('GAME_OVER')

    def clear_lines(self):
        lines_cleared = 0
        for y in range(GRID_HEIGHT - 1, -1, -1):
            if 0 not in self.grid[y]:
                del self.grid[y]
                self.grid.insert(0, [0 for _ in range(GRID_WIDTH)])
                lines_cleared += 1
                ArcadeSound.play('CLEAR')
        self.score += [0, 100, 300, 500, 800][lines_cleared]
        if self.score > self.high_score:
            self.high_score = self.score
            self.score_manager.save_high_score('Tetris', self.high_score)

    def move(self, dx, dy):
        if self.game_over or self.paused: return
        if not self.check_collision(self.current_piece['shape'], self.piece_x + dx, self.piece_y + dy):
            self.piece_x += dx
            self.piece_y += dy
            return True
        elif dy > 0: # Hit bottom or other piece
            self.merge_piece()
            return False
        return False

    def rotate(self):
        if self.game_over or self.paused: return
        rotated = self.rotate_shape(self.current_piece['shape'])
        if not self.check_collision(rotated, self.piece_x, self.piece_y):
            self.current_piece['shape'] = rotated

# =============================================================================
# UI & RENDERING
# =============================================================================

def set_window_position_bottom_right():
    try:
        import ctypes
        user32 = ctypes.windll.user32
        screensize = user32.GetSystemMetrics(0), user32.GetSystemMetrics(1) 
        x = screensize[0] - WINDOW_WIDTH - 20
        y = screensize[1] - WINDOW_HEIGHT - 60
        os.environ['SDL_VIDEO_WINDOW_POS'] = "%d,%d" % (x,y)
    except:
        os.environ['SDL_VIDEO_WINDOW_POS'] = "100,100"

def draw_block(screen, x, y, color):
    rect = pygame.Rect(x, y, BLOCK_SIZE - 1, BLOCK_SIZE - 1)
    pygame.draw.rect(screen, color, rect)

def main():
    set_window_position_bottom_right()
    pygame.init()
    
    screen = pygame.display.set_mode((WINDOW_WIDTH, WINDOW_HEIGHT), pygame.NOFRAME)
    pygame.display.set_caption("Tetris Widget")
    clock = pygame.time.Clock()
    
    font = pygame.font.Font(None, 30)
    font_small = pygame.font.Font(None, 20)
    
    game = TetrisGame()
    running = True
    fall_time = 0
    fall_speed = 0.5

    while running:
        dt = clock.get_rawtime()
        fall_time += dt / 1000
        clock.tick(60)

        # Input
        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                running = False
            elif event.type == pygame.KEYDOWN:
                if event.key == pygame.K_ESCAPE:
                    running = False
                elif event.key == pygame.K_LEFT:
                    game.move(-1, 0)
                elif event.key == pygame.K_RIGHT:
                    game.move(1, 0)
                elif event.key == pygame.K_DOWN:
                    game.move(0, 1)
                elif event.key == pygame.K_UP:
                    game.rotate()
                elif event.key == pygame.K_SPACE: # Drop
                    while game.move(0, 1): pass
                elif event.key == pygame.K_p:
                    game.paused = not game.paused
                elif event.key == pygame.K_r:
                    game = TetrisGame()

        # Update
        if fall_time >= fall_speed:
            game.move(0, 1)
            fall_time = 0

        # Render
        screen.fill(COLORS['background'])
        
        # Draw Grid
        pygame.draw.rect(screen, COLORS['grid_bg'], (0, 0, GRID_WIDTH * BLOCK_SIZE, GRID_HEIGHT * BLOCK_SIZE))
        
        # Draw Board
        for y, row in enumerate(game.grid):
            for x, color in enumerate(row):
                if color:
                    draw_block(screen, x * BLOCK_SIZE, y * BLOCK_SIZE, color)
                    
        # Draw Current Piece
        if not game.game_over:
            for y, row in enumerate(game.current_piece['shape']):
                for x, cell in enumerate(row):
                    if cell:
                        draw_block(screen, (game.piece_x + x) * BLOCK_SIZE, (game.piece_y + y) * BLOCK_SIZE, game.current_piece['color'])

        # Draw UI (Side Panel)
        panel_x = GRID_WIDTH * BLOCK_SIZE + 10
        
        score_text = font.render(f"Score", True, COLORS['text'])
        score_val = font.render(f"{game.score}", True, COLORS['text'])
        screen.blit(score_text, (panel_x, 20))
        screen.blit(score_val, (panel_x, 50))
        
        best_text = font.render(f"Best", True, COLORS['text'])
        best_val = font.render(f"{game.high_score}", True, COLORS['text'])
        screen.blit(best_text, (panel_x, 90))
        screen.blit(best_val, (panel_x, 120))

        next_text = font_small.render("Next:", True, COLORS['text'])
        screen.blit(next_text, (panel_x, 160))
        
        # Next Piece Preview
        for y, row in enumerate(game.next_piece['shape']):
            for x, cell in enumerate(row):
                if cell:
                    draw_block(screen, panel_x + x * 15, 190 + y * 15, game.next_piece['color'])

        # Game Over Overlay
        if game.game_over:
            overlay = pygame.Surface((WINDOW_WIDTH, WINDOW_HEIGHT))
            overlay.set_alpha(200)
            overlay.fill((0, 0, 0))
            screen.blit(overlay, (0, 0))
            
            go_text = font.render("GAME OVER", True, (255, 0, 0))
            final_score = font_small.render(f"Final Score: {game.score}", True, (255, 255, 255))
            restart = font_small.render("Press R to Restart", True, (255, 255, 255))
            
            screen.blit(go_text, (WINDOW_WIDTH//2 - go_text.get_width()//2, WINDOW_HEIGHT//2 - 40))
            screen.blit(final_score, (WINDOW_WIDTH//2 - final_score.get_width()//2, WINDOW_HEIGHT//2))
            screen.blit(restart, (WINDOW_WIDTH//2 - restart.get_width()//2, WINDOW_HEIGHT//2 + 30))

        pygame.draw.rect(screen, COLORS['border'], (0, 0, WINDOW_WIDTH, WINDOW_HEIGHT), 1)
        pygame.display.flip()

    pygame.quit()

if __name__ == "__main__":
    main()
