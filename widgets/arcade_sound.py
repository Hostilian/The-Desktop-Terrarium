
import sys

# Try import winsound (Windows only)
try:
    import winsound
except ImportError:
    winsound = None

class ArcadeSound:
    """
    Simple retro sound effects using Windows Beep.
    Non-blocking where possible (though Beep is blocking, short durations are fine).
    """
    
    @staticmethod
    def play(effect):
        """
        Plays a predefined sound effect.
        effect: 'JUMP', 'EAT', 'CRASH', 'SCORE', 'CLEAR', 'MERGE'
        """
        if not winsound: return

        try:
            # Using very short durations to minimize blocking the game loop
            if effect == 'JUMP':
                winsound.Beep(600, 40)
            elif effect == 'EAT':
                # Very short high pitch for eating
                winsound.Beep(1200, 30)
            elif effect == 'CRASH':
                # Lower pitch, longer
                winsound.Beep(150, 400)
            elif effect == 'SCORE':
                # Victory soundish
                winsound.Beep(1500, 100)
            elif effect == 'CLEAR':
                # Tetris line clear
                winsound.Beep(1200, 80)
            elif effect == 'MERGE':
                # 2048 merge
                winsound.Beep(800, 40)
            elif effect == 'GAME_OVER':
                winsound.Beep(100, 500)
                
        except Exception:
            pass
