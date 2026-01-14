#!/usr/bin/env python3
"""
Desktop Pet Widget
=================
A transparent, walking desktop pet using Tkinter.
"""

import tkinter as tk
import time
import random
import os

class Pet:
    def __init__(self):
        self.window = tk.Tk()
        
        # Window configuration
        self.window.config(highlightbackground='black')
        self.window.overrideredirect(True) # Frameless
        self.window.attributes('-topmost', True) # Always on top
        self.window.attributes('-transparentcolor', 'black') # Transparency key
        
        # Load sprites (using placeholders or creating simple shapes)
        # Ideally, we load GIFs here. Since we don't have assets, we'll draw a canvas shape
        self.canvas = tk.Canvas(self.window, width=100, height=100, bg='black', highlightthickness=0)
        self.canvas.pack()
        
        # Draw a simple "Pet" (a white blob with eyes)
        self.body = self.canvas.create_oval(20, 20, 80, 80, fill='white', outline='white')
        self.eye_l = self.canvas.create_oval(35, 40, 45, 50, fill='black')
        self.eye_r = self.canvas.create_oval(55, 40, 65, 50, fill='black')
        
        # Position
        self.screen_width = self.window.winfo_screenwidth()
        self.screen_height = self.window.winfo_screenheight()
        self.x = self.screen_width - 150
        self.y = self.screen_height - 150
        self.window.geometry(f'100x100+{self.x}+{self.y}')
        
        # Animation state
        self.state = 0 # 0: start, 1: moving left, 2: moving right, 3: idle
        self.timestamp = time.time()
        
        # Events
        self.canvas.bind('<Button-1>', self.on_click)
        self.canvas.bind('<B1-Motion>', self.on_drag)
        self.window.bind('<Escape>', lambda e: self.window.destroy())
        
        self.update()
        self.window.mainloop()

    def on_click(self, event):
        self.drag_start_x = event.x
        self.drag_start_y = event.y

    def on_drag(self, event):
        x = self.window.winfo_x() - self.drag_start_x + event.x
        y = self.window.winfo_y() - self.drag_start_y + event.y
        self.x = x
        self.y = y
        self.window.geometry(f'+{x}+{y}')

    def update(self):
        curr_time = time.time()
        
        # Target: Mouse position
        mouse_x = self.window.winfo_pointerx()
        mouse_y = self.window.winfo_pointery()
        
        # Destination
        dest_x = mouse_x - 50 # Center offset
        dest_y = mouse_y - 50
        
        # Ease towards mouse (10% of distance per frame)
        dx = dest_x - self.x
        dy = dest_y - self.y
        
        # Distance check (stop if close)
        dist = math.hypot(dx, dy)
        
        if dist > 50:
            self.x += dx * 0.05
            self.y += dy * 0.05
        
        # Bounce/Wiggle animation
        offset = int(math.sin(curr_time * 8) * 3)
        self.canvas.coords(self.body, 20, 20 + offset, 80, 80 + offset)
        
        # Look at mouse
        eye_offset_x = 2 if dx > 0 else -2
        self.canvas.coords(self.eye_l, 35 + eye_offset_x, 40 + offset, 45 + eye_offset_x, 50 + offset)
        self.canvas.coords(self.eye_r, 55 + eye_offset_x, 40 + offset, 65 + eye_offset_x, 50 + offset)

        self.window.geometry(f'+{int(self.x)}+{int(self.y)}')
        self.window.after(20, self.update)

import math

if __name__ == '__main__':
    Pet()
