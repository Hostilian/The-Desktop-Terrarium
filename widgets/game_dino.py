#!/usr/bin/env python3
"""
T-Rex Runner - Corner Widget Edition
===================================
A clone of the famous Chrome offline game, styled for the corner widget.
"""

import pygame
import random
import os
from score_manager import ScoreManager
from arcade_sound import ArcadeSound

# =============================================================================
# CONFIGURATION
# =============================================================================

WINDOW_WIDTH = 600
WINDOW_HEIGHT = 250
GROUND_HEIGHT = 200

# Colors
COLORS = {
    'background': (20, 20, 25),
    'ground': (255, 255, 255),
    'dino': (255, 255, 255),
    'cactus': (200, 200, 200),
    'text': (255, 255, 255),
    'accent': (255, 50, 50) # Game Over color
}

class Dino:
    def __init__(self):
        self.width = 40
        self.height = 40
        self.x = 50
        self.y = GROUND_HEIGHT - self.height
        self.vel_y = 0
        self.jumping = False
        self.ducking = False
        self.jump_speed = -15
        self.gravity = 0.8
        
    def jump(self):
        if not self.jumping:
            self.jumping = True
            self.vel_y = self.jump_speed
            ArcadeSound.play('JUMP')
            
    def update(self):
        if self.jumping:
            self.vel_y += self.gravity
            self.y += self.vel_y
            
            if self.y >= GROUND_HEIGHT - self.height:
                self.y = GROUND_HEIGHT - self.height
                self.jumping = False
                self.vel_y = 0

    def draw(self, screen):
        # Body
        if self.ducking:
             pygame.draw.rect(screen, COLORS['dino'], (self.x, self.y + 20, 50, 20))
        else:
             pygame.draw.rect(screen, COLORS['dino'], (self.x, self.y, self.width, self.height))

class Cactus:
    def __init__(self, x):
        self.x = x
        self.y = GROUND_HEIGHT - 40
        self.width = 20
        self.height = 40
        self.passed = False
        
    def update(self, speed):
        self.x -= speed
        
    def draw(self, screen):
        pygame.draw.rect(screen, COLORS['cactus'], (self.x, self.y, self.width, self.height))
        # Add arms
        pygame.draw.rect(screen, COLORS['cactus'], (self.x - 5, self.y + 10, 5, 10))
        pygame.draw.rect(screen, COLORS['cactus'], (self.x + 20, self.y + 5, 5, 15))

# =============================================================================
# MAIN GAME
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
    pygame.display.set_caption("Dino Widget")
    clock = pygame.time.Clock()
    font = pygame.font.Font(None, 30)
    
    score_manager = ScoreManager()
    high_score = score_manager.get_high_score('Dino')
    
    # Game State
    dino = Dino()
    cacti = []
    speed = 6
    score = 0
    game_over = False
    
    spawn_timer = 0
    
    running = True
    while running:
        clock.tick(60)
        
        # Input
        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                running = False
            elif event.type == pygame.KEYDOWN:
                if event.key == pygame.K_ESCAPE:
                    running = False
                elif event.key == pygame.K_SPACE or event.key == pygame.K_UP:
                    if not game_over:
                        dino.jump()
                    else:
                        # Reset
                        dino = Dino()
                        cacti = []
                        speed = 6
                        score = 0
                        game_over = False
                elif event.key == pygame.K_DOWN:
                    dino.ducking = True
            elif event.type == pygame.KEYUP:
                if event.key == pygame.K_DOWN:
                    dino.ducking = False

        if not game_over:
            # Update
            dino.update()
            
            # Spawn logic
            spawn_timer += 1
            if spawn_timer > random.randint(60, 100):
                cacti.append(Cactus(WINDOW_WIDTH + random.randint(0, 50)))
                spawn_timer = 0
                
            # Cacti Update
            for cactus in cacti:
                cactus.update(speed)
                if cactus.x < -20:
                    cacti.remove(cactus)
                    
                # Collision
                dino_rect = pygame.Rect(dino.x, dino.y, dino.width, dino.height)
                if dino.ducking:
                     dino_rect = pygame.Rect(dino.x, dino.y + 20, 50, 20)
                     
                cactus_rect = pygame.Rect(cactus.x, cactus.y, cactus.width, cactus.height)
                
                if dino_rect.colliderect(cactus_rect):
                    game_over = True
                    ArcadeSound.play('CRASH')
            
            # Score and Difficulty
            score += 0.1
            if score > high_score:
                high_score = score
                score_manager.save_high_score('Dino', high_score)
            
            if int(score) % 100 == 0:
                speed += 0.05
                ArcadeSound.play('SCORE')

        # Render
        screen.fill(COLORS['background'])
        
        # Ground
        pygame.draw.line(screen, COLORS['ground'], (0, GROUND_HEIGHT), (WINDOW_WIDTH, GROUND_HEIGHT), 2)
        
        # Draw Entities
        dino.draw(screen)
        for cactus in cacti:
            cactus.draw(screen)
            
        # Score
        score_text = font.render(f"{int(score):05}", True, COLORS['text'])
        screen.blit(score_text, (WINDOW_WIDTH - 80, 20))
        
        hi_text = font.render(f"HI {int(high_score):05}", True, (200, 200, 200))
        screen.blit(hi_text, (WINDOW_WIDTH - 180, 20))
        
        if game_over:
            go_text = font.render("GAME OVER", True, COLORS['accent'])
            restart_text = font.render("Press SPACE to Restart", True, COLORS['text'])
            screen.blit(go_text, (WINDOW_WIDTH//2 - go_text.get_width()//2, WINDOW_HEIGHT//2 - 20))
            screen.blit(restart_text, (WINDOW_WIDTH//2 - restart_text.get_width()//2, WINDOW_HEIGHT//2 + 10))

        # Border
        pygame.draw.rect(screen, (50, 50, 60), (0, 0, WINDOW_WIDTH, WINDOW_HEIGHT), 1)

        pygame.display.flip()

    pygame.quit()

if __name__ == "__main__":
    main()
