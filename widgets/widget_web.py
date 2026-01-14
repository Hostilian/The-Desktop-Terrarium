#!/usr/bin/env python3
"""
Web Browser Widget
=================
A frameless webview wrapper to play browser games as widgets.
"""

import webview
import sys
import ctypes
import math

# Default to 2048 main site if no arg provided
DEFAULT_URL = "https://play2048.co"

def set_window_position_bottom_right(width, height):
    try:
        user32 = ctypes.windll.user32
        screensize = user32.GetSystemMetrics(0), user32.GetSystemMetrics(1) 
        x = screensize[0] - width - 20
        y = screensize[1] - height - 60
        # For pywebview, we typically set this via the window creation API, 
        # but exact positioning might depend on the OS window manager.
        return x, y
    except:
        return 100, 100

def main():
    width = 400
    height = 500
    
    url = DEFAULT_URL
    if len(sys.argv) > 1:
        url = sys.argv[1]

    # Calculate position
    x, y = set_window_position_bottom_right(width, height)

    webview.create_window(
        'Web Widget', 
        url, 
        width=width, 
        height=height, 
        frameless=True,
        on_top=True,
        x=x,
        y=y,
        background_color='#141419' # Match our dark theme
    )
    
    webview.start()

if __name__ == "__main__":
    main()
