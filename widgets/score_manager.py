import json
import os

class ScoreManager:
    """
    Manages high scores for all Desktop Terrarium widgets.
    Stores data in 'highscores.json' in the same directory.
    """
    def __init__(self):
        self.filename = "highscores.json"
        self.scores = self._load_scores()

    def _load_scores(self):
        if not os.path.exists(self.filename):
            return {}
        try:
            with open(self.filename, 'r') as f:
                return json.load(f)
        except:
            return {}

    def _save_scores(self):
        try:
            with open(self.filename, 'w') as f:
                json.dump(self.scores, f)
        except Exception as e:
            print(f"Failed to save scores: {e}")

    def get_high_score(self, game_name):
        """Returns the high score for a specific game."""
        return self.scores.get(game_name, 0)

    def save_high_score(self, game_name, score):
        """
        Updates the high score if the new score is higher.
        Returns True if a new high score was set.
        """
        current_high = self.get_high_score(game_name)
        if score > current_high:
            self.scores[game_name] = score
            self._save_scores()
            return True
        return False
