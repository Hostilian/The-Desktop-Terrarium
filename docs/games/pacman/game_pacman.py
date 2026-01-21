#!/usr/bin/env python3
"""
Pacman Widget
=============
A simplified Pacman clone for the corner.
"""

import pygame
import random
import os
import math
from score_manager import ScoreManager
from arcade_sound import ArcadeSound

# =============================================================================
# CONFIGURATION
# =============================================================================

WINDOW_WIDTH = 400
WINDOW_HEIGHT = 420
BLOCK_SIZE = 20
GRID_WIDTH = WINDOW_WIDTH // BLOCK_SIZE
GRID_HEIGHT = WINDOW_HEIGHT // BLOCK_SIZE

# Colors
COLORS = {
    'background': (0, 0, 0),
    'wall': (30, 30, 150),
    'dot': (255, 180, 180),
    'pacman': (255, 255, 0),
    'ghost': (255, 0, 0),
    'text': (255, 255, 255)
}

# Simple map (1 = wall, 0 = dot)
# 20x21 grid
MAP_LAYOUT = [
    "11111111111111111111",
    "10000000010000000001",
    "10111111010111111001",
    "10100000000000001001",
    "10101110111111101011",
    "10000000000000000001",
    "11111011100111011111",
    "10000010000001000001",
    "10111010111101011101",
    "10000000000000000001",
    "10111110111111101111",
    "10000000000000000001",
    "11111011111111011111",
    "10000000010000000001",
    "10111111010111111001",
    "10000000000000000001",
    "11101111111111110111",
    "10000000010000000001",
    "10111111010111111001",
    "10000000000000000001",
    "11111111111111111111",
]

# =============================================================================
# GAME LOGIC
# =============================================================================

class PacmanGame:
    def __init__(self):
        self.score_manager = ScoreManager()
        self.high_score = self.score_manager.get_high_score('Pacman')
        self.reset()
        
    def reset(self):
        self.walls = []
        self.dots = []
        self.score = 0
        self.game_over = False
        
        # Parse map
        for y, row in enumerate(MAP_LAYOUT):
            for x, char in enumerate(row):
                if char == '1':
                    self.walls.append(pygame.Rect(x*BLOCK_SIZE, y*BLOCK_SIZE, BLOCK_SIZE, BLOCK_SIZE))
                else:
                    self.dots.append(pygame.Rect(x*BLOCK_SIZE + BLOCK_SIZE//2 - 2, y*BLOCK_SIZE + BLOCK_SIZE//2 - 2, 4, 4))
        
        self.pacman_pos = [10 * BLOCK_SIZE, 15 * BLOCK_SIZE]
        self.pacman_dir = (0, 0)
        self.pacman_next_dir = (0, 0)
        
        self.ghost_pos = [10 * BLOCK_SIZE, 10 * BLOCK_SIZE]
        self.ghost_dir = (1, 0)
        
    def can_move(self, pos, direction):
        test_rect = pygame.Rect(pos[0] + direction[0]*BLOCK_SIZE, pos[1] + direction[1]*BLOCK_SIZE, BLOCK_SIZE, BLOCK_SIZE)
        # Relaxed collision for smoother movement (check center point against walls)
        center = (test_rect.centerx, test_rect.centery)
        for wall in self.walls:
            if wall.collidepoint(center):
                return False
        return True

    def update(self):
        if self.game_over: return

        # Pacman Movement
        # Align to grid
        grid_x = round(self.pacman_pos[0] / BLOCK_SIZE) * BLOCK_SIZE
        grid_y = round(self.pacman_pos[1] / BLOCK_SIZE) * BLOCK_SIZE
        
        # If aligned, can change direction
        dist = math.hypot(self.pacman_pos[0] - grid_x, self.pacman_pos[1] - grid_y)
        
        if dist < 2:
            self.pacman_pos = [grid_x, grid_y]
            if self.pacman_next_dir != (0,0) and self.can_move(self.pacman_pos, self.pacman_next_dir):
                self.pacman_dir = self.pacman_next_dir
            elif not self.can_move(self.pacman_pos, self.pacman_dir):
                self.pacman_dir = (0, 0)

        self.pacman_pos[0] += self.pacman_dir[0] * 3 # Speed
        self.pacman_pos[1] += self.pacman_dir[1] * 3
        
        # Screen Wrap
        if self.pacman_pos[0] < -BLOCK_SIZE: self.pacman_pos[0] = WINDOW_WIDTH
        if self.pacman_pos[0] > WINDOW_WIDTH: self.pacman_pos[0] = -BLOCK_SIZE

        # Eat Dots
        pac_rect = pygame.Rect(self.pacman_pos[0], self.pacman_pos[1], BLOCK_SIZE, BLOCK_SIZE)
        for dot in self.dots[:]:
            if pac_rect.colliderect(dot):
                self.dots.remove(dot)
                self.score += 10
                ArcadeSound.play('EAT')
                if self.score > self.high_score:
                    self.high_score = self.score
                    self.score_manager.save_high_score('Pacman', self.high_score)
        
        if not self.dots:
            self.game_over = True # Win condition basically

        # Ghost AI (Very dumb: Random movement when hitting walls)
        g_grid_x = round(self.ghost_pos[0] / BLOCK_SIZE) * BLOCK_SIZE
        g_grid_y = round(self.ghost_pos[1] / BLOCK_SIZE) * BLOCK_SIZE
        g_dist = math.hypot(self.ghost_pos[0] - g_grid_x, self.ghost_pos[1] - g_grid_y)
        
        if g_dist < 2:
            self.ghost_pos = [g_grid_x, g_grid_y]
            possible_dirs = []
            for dx, dy in [(1,0), (-1,0), (0,1), (0,-1)]:
                if self.can_move(self.ghost_pos, (dx, dy)):
                    possible_dirs.append((dx, dy))
            
            # 50% chance to track player, 50% random
            if random.random() < 0.5:
                # Track player
                best_dir = (0,0)
                min_dist = 99999
                for d in possible_dirs:
                    tx = self.ghost_pos[0] + d[0] * BLOCK_SIZE
                    ty = self.ghost_pos[1] + d[1] * BLOCK_SIZE
                    dist_to_pac = math.hypot(tx - self.pacman_pos[0], ty - self.pacman_pos[1])
                    if dist_to_pac < min_dist:
                        min_dist = dist_to_pac
                        best_dir = d
                if best_dir != (0,0):
                     self.ghost_dir = best_dir
                else: 
                     self.ghost_dir = random.choice(possible_dirs) if possible_dirs else (0,0)
            else:
                 self.ghost_dir = random.choice(possible_dirs) if possible_dirs else (0,0)

        self.ghost_pos[0] += self.ghost_dir[0] * 2
        self.ghost_pos[1] += self.ghost_dir[1] * 2

        # Collision with Ghost
        ghost_rect = pygame.Rect(self.ghost_pos[0], self.ghost_pos[1], BLOCK_SIZE, BLOCK_SIZE)
        if pac_rect.colliderect(ghost_rect):
            self.game_over = True
            ArcadeSound.play('GAME_OVER')

# =============================================================================
# UI
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

def main():
    set_window_position_bottom_right()
    pygame.init()
    
    screen = pygame.display.set_mode((WINDOW_WIDTH, WINDOW_HEIGHT), pygame.NOFRAME)
    pygame.display.set_caption("Pacman Widget")
    clock = pygame.time.Clock()
    font = pygame.font.Font(None, 36)
    
    game = PacmanGame()
    running = True

    while running:
        clock.tick(60)
        
        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                running = False
            elif event.type == pygame.KEYDOWN:
                if event.key == pygame.K_ESCAPE:
                    running = False
                elif event.key == pygame.K_UP:
                    game.pacman_next_dir = (0, -1)
                elif event.key == pygame.K_DOWN:
                    game.pacman_next_dir = (0, 1)
                elif event.key == pygame.K_LEFT:
                    game.pacman_next_dir = (-1, 0)
                elif event.key == pygame.K_RIGHT:
                    game.pacman_next_dir = (1, 0)
                elif event.key == pygame.K_r:
                    game.reset()

        game.update()

        # Render
        screen.fill(COLORS['background'])
        
        # Walls
        for wall in game.walls:
            pygame.draw.rect(screen, COLORS['wall'], wall, border_radius=4)
            
        # Dots
        for dot in game.dots:
            pygame.draw.rect(screen, COLORS['dot'], dot)
            
        # Pacman
        px, py = int(game.pacman_pos[0] + BLOCK_SIZE/2), int(game.pacman_pos[1] + BLOCK_SIZE/2)
        pygame.draw.circle(screen, COLORS['pacman'], (px, py), BLOCK_SIZE//2 - 2)
        
        # Ghost
        gx, gy = int(game.ghost_pos[0] + BLOCK_SIZE/2), int(game.ghost_pos[1] + BLOCK_SIZE/2)
        pygame.draw.circle(screen, COLORS['ghost'], (gx, gy), BLOCK_SIZE//2 - 2)
        
        # Score
        score_surface = font.render(f"SCORE: {game.score}", True, COLORS['text'])
        screen.blit(score_surface, (10, WINDOW_HEIGHT - 30))
        
        hi_surface = font.render(f"BEST: {game.high_score}", True, (255, 255, 0))
        screen.blit(hi_surface, (WINDOW_WIDTH - 150, WINDOW_HEIGHT - 30))

        if game.game_over:
            overlay = pygame.Surface((WINDOW_WIDTH, WINDOW_HEIGHT))
            overlay.set_alpha(200)
            overlay.fill((0, 0, 0))
            screen.blit(overlay, (0, 0))
            
            msg = "GAME OVER" if game.dots else "YOU WIN!"
            col = (255, 0, 0) if game.dots else (0, 255, 0)
            
            txt = font.render(msg, True, col)
            screen.blit(txt, (WINDOW_WIDTH//2 - txt.get_width()//2, WINDOW_HEIGHT//2))
            
            rst = font.render("Press R to Restart", True, (255, 255, 255))
            screen.blit(rst, (WINDOW_WIDTH//2 - rst.get_width()//2, WINDOW_HEIGHT//2 + 40))

        pygame.draw.rect(screen, (50, 50, 60), (0, 0, WINDOW_WIDTH, WINDOW_HEIGHT), 1)
        pygame.display.flip()

    pygame.quit()

if __name__ == "__main__":
    main()
