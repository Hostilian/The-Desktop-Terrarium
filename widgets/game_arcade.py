#!/usr/bin/env python3
"""
Arcade Widget (Snake)
====================
A clean, dark-themed implementation of all-time classic Snake.
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
WINDOW_HEIGHT = 400
GRID_SIZE = 20
GRID_WIDTH = WINDOW_WIDTH // GRID_SIZE
GRID_HEIGHT = WINDOW_HEIGHT // GRID_SIZE

# Colors
COLORS = {
    'background': (20, 20, 25),
    'snake': (0, 255, 100),
    'food': (255, 50, 50),
    'text': (255, 255, 255),
    'border': (50, 50, 60),
    'grid_line': (30, 30, 35)
}

# =============================================================================
# GAME LOGIC
# =============================================================================

class SnakeGame:
    def __init__(self):
        self.score_manager = ScoreManager()
        self.high_score = self.score_manager.get_high_score('Snake')
        self.reset()
        
    def reset(self):
        self.snake = [(GRID_WIDTH // 2, GRID_HEIGHT // 2)]
        self.direction = (1, 0)
        self.food = self.spawn_food()
        self.score = 0
        self.game_over = False
        self.paused = False
        
    def spawn_food(self):
        while True:
            pos = (random.randint(0, GRID_WIDTH - 1), random.randint(0, GRID_HEIGHT - 1))
            if pos not in self.snake:
                return pos

    def update(self):
        if self.game_over or self.paused:
            return

        head_x, head_y = self.snake[0]
        dx, dy = self.direction
        new_head = (head_x + dx, head_y + dy)

        # Check collision with walls
        if (new_head[0] < 0 or new_head[0] >= GRID_WIDTH or 
            new_head[1] < 0 or new_head[1] >= GRID_HEIGHT):
            self.game_over = True
            ArcadeSound.play('CRASH')
            return

        # Check collision with self
        if new_head in self.snake:
            self.game_over = True
            ArcadeSound.play('CRASH')
            return

        self.snake.insert(0, new_head)

        # Check food
        if new_head == self.food:
            self.score += 10
            ArcadeSound.play('EAT')
            if self.score > self.high_score:
                self.high_score = self.score
                self.score_manager.save_high_score('Snake', self.high_score)
            self.food = self.spawn_food()
        else:
            self.snake.pop()

    def change_direction(self, new_dir):
        # Prevent 180 degree turns
        if (new_dir[0] * -1 != self.direction[0] or 
            new_dir[1] * -1 != self.direction[1]):
            self.direction = new_dir

# =============================================================================
# MAIN
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
    pygame.display.set_caption("Arcade Widget - Snake")
    clock = pygame.time.Clock()
    font = pygame.font.Font(None, 36)
    small_font = pygame.font.Font(None, 24)
    
    game = SnakeGame()
    running = True

    while running:
        clock.tick(15)  # Snake speed
        
        # Input
        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                running = False
            elif event.type == pygame.KEYDOWN:
                if event.key == pygame.K_ESCAPE:
                    running = False
                elif event.key == pygame.K_UP:
                    game.change_direction((0, -1))
                elif event.key == pygame.K_DOWN:
                    game.change_direction((0, 1))
                elif event.key == pygame.K_LEFT:
                    game.change_direction((-1, 0))
                elif event.key == pygame.K_RIGHT:
                    game.change_direction((1, 0))
                elif event.key == pygame.K_r:
                    game.reset()
                elif event.key == pygame.K_p:
                    game.paused = not game.paused

        game.update()

        # Render
        screen.fill(COLORS['background'])
        
        # Draw Grid (Optional, faint)
        # for x in range(0, WINDOW_WIDTH, GRID_SIZE):
        #     pygame.draw.line(screen, COLORS['grid_line'], (x, 0), (x, WINDOW_HEIGHT))
        # for y in range(0, WINDOW_HEIGHT, GRID_SIZE):
        #     pygame.draw.line(screen, COLORS['grid_line'], (0, y), (WINDOW_WIDTH, y))

        # Draw Food
        fx, fy = game.food
        pygame.draw.rect(screen, COLORS['food'], (fx*GRID_SIZE, fy*GRID_SIZE, GRID_SIZE-1, GRID_SIZE-1), border_radius=4)

        # Draw Snake
        for i, segment in enumerate(game.snake):
            sx, sy = segment
            color = COLORS['snake']
            # Head is slightly different color?
            if i == 0:
                color = (50, 255, 150)
            
            pygame.draw.rect(screen, color, (sx*GRID_SIZE, sy*GRID_SIZE, GRID_SIZE-1, GRID_SIZE-1), border_radius=4)

        # Draw Score
        score_text = font.render(str(game.score), True, COLORS['text'])
        screen.blit(score_text, (10, 10))
        
        hi_text = font.render(f"HI {game.high_score}", True, (200, 200, 200))
        screen.blit(hi_text, (WINDOW_WIDTH - 100, 10))

        # Game Over Overlay
        if game.game_over:
            overlay = pygame.Surface((WINDOW_WIDTH, WINDOW_HEIGHT))
            overlay.set_alpha(200)
            overlay.fill((0, 0, 0))
            screen.blit(overlay, (0, 0))
            
            go_text = font.render("GAME OVER", True, (255, 50, 50))
            restart_text = small_font.render("Press R to Restart", True, COLORS['text'])
            
            screen.blit(go_text, (WINDOW_WIDTH//2 - go_text.get_width()//2, WINDOW_HEIGHT//2 - 20))
            screen.blit(restart_text, (WINDOW_WIDTH//2 - restart_text.get_width()//2, WINDOW_HEIGHT//2 + 20))

        # Border
        pygame.draw.rect(screen, COLORS['border'], (0, 0, WINDOW_WIDTH, WINDOW_HEIGHT), 1)

        pygame.display.flip()

    pygame.quit()

if __name__ == "__main__":
    main()
