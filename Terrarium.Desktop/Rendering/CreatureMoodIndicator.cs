using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Terrarium.Logic.Entities;

namespace Terrarium.Desktop.Rendering
{
    /// <summary>
    /// Displays emoji indicators above creatures showing their current state/mood.
    /// </summary>
    public class CreatureMoodIndicator
    {
        private readonly Canvas _canvas;
        private readonly Dictionary<Creature, MoodVisual> _moodVisuals;
        private double _updateTimer;

        private const double UpdateInterval = 0.5; // Update moods every 0.5 seconds
        private const double IndicatorOffsetY = -25;
        private const double FadeInDuration = 0.2;

        public bool IsEnabled { get; set; } = true;

        public CreatureMoodIndicator(Canvas canvas)
        {
            _canvas = canvas;
            _moodVisuals = new Dictionary<Creature, MoodVisual>();
            _updateTimer = 0;
        }

        /// <summary>
        /// Updates mood indicators for all creatures.
        /// </summary>
        public void Update(double deltaTime, IEnumerable<Herbivore> herbivores, IEnumerable<Carnivore> carnivores)
        {
            if (!IsEnabled) return;

            _updateTimer += deltaTime;
            if (_updateTimer < UpdateInterval) return;
            _updateTimer = 0;

            // Track which creatures still exist
            var existingCreatures = new HashSet<Creature>();

            foreach (var herbivore in herbivores)
            {
                if (!herbivore.IsAlive) continue;

                existingCreatures.Add(herbivore);
                string mood = GetMoodEmoji(herbivore);
                UpdateOrCreateMoodVisual(herbivore, mood);
            }

            foreach (var carnivore in carnivores)
            {
                if (!carnivore.IsAlive) continue;

                existingCreatures.Add(carnivore);
                string mood = GetMoodEmoji(carnivore);
                UpdateOrCreateMoodVisual(carnivore, mood);
            }

            // Remove visuals for dead/removed creatures
            var toRemove = new List<Creature>();
            foreach (var kvp in _moodVisuals)
            {
                if (!existingCreatures.Contains(kvp.Key))
                {
                    _canvas.Children.Remove(kvp.Value.Visual);
                    toRemove.Add(kvp.Key);
                }
                else
                {
                    // Update position
                    Canvas.SetLeft(kvp.Value.Visual, kvp.Key.X - 8);
                    Canvas.SetTop(kvp.Value.Visual, kvp.Key.Y + IndicatorOffsetY);
                }
            }

            foreach (var creature in toRemove)
            {
                _moodVisuals.Remove(creature);
            }
        }

        private string GetMoodEmoji(Creature creature)
        {
            // Priority-based mood selection
            if (creature.Health < 20)
                return "💔"; // Very low health

            if (creature.Hunger > 80)
                return "🍖"; // Starving, needs food

            if (creature.Hunger > 50)
                return "😋"; // Hungry

            // Check if being chased (for herbivores)
            if (creature is Herbivore && Math.Abs(creature.VelocityX) > 30 || Math.Abs(creature.VelocityY) > 30)
                return "😰"; // Fleeing

            // Check if hunting (for carnivores)
            if (creature is Carnivore && (Math.Abs(creature.VelocityX) > 20 || Math.Abs(creature.VelocityY) > 20))
                return "🎯"; // Hunting

            if (creature.Health > 80 && creature.Hunger < 30)
                return "😊"; // Happy and well-fed

            if (creature.Health > 50)
                return "😐"; // Neutral

            return "😟"; // Worried (low-ish health)
        }

        private void UpdateOrCreateMoodVisual(Creature creature, string mood)
        {
            if (_moodVisuals.TryGetValue(creature, out var existing))
            {
                // Update existing
                if (existing.CurrentMood != mood)
                {
                    existing.Visual.Text = mood;
                    existing.CurrentMood = mood;
                }
            }
            else
            {
                // Create new
                var textBlock = new TextBlock
                {
                    Text = mood,
                    FontSize = 14,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                Canvas.SetLeft(textBlock, creature.X - 8);
                Canvas.SetTop(textBlock, creature.Y + IndicatorOffsetY);
                Canvas.SetZIndex(textBlock, 500);

                _canvas.Children.Add(textBlock);
                _moodVisuals[creature] = new MoodVisual { Visual = textBlock, CurrentMood = mood };
            }
        }

        /// <summary>
        /// Clears all mood indicators.
        /// </summary>
        public void Clear()
        {
            foreach (var kvp in _moodVisuals)
            {
                _canvas.Children.Remove(kvp.Value.Visual);
            }
            _moodVisuals.Clear();
        }
    }

    internal class MoodVisual
    {
        public TextBlock Visual { get; set; } = null!;
        public string CurrentMood { get; set; } = string.Empty;
    }
}
