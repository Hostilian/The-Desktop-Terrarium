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

        private readonly HashSet<Creature> _existingCreaturesBuffer = new();
        private readonly List<Creature> _toRemoveBuffer = new();

        private const double UpdateInterval = 0.5; // Update moods every 0.5 seconds
        private const double IndicatorOffsetY = -25;
        private const double FadeInDuration = 0.2;

        private const double IndicatorOffsetX = -8;
        private const double MoodFontSize = 14;
        private const int MoodZIndex = 500;

        private const double CriticalHealthThreshold = 20.0;
        private const double StarvingHungerThreshold = 80.0;
        private const double HungryHungerThreshold = 50.0;

        private const double HerbivoreFleeSpeedThreshold = 30.0;
        private const double CarnivoreHuntSpeedThreshold = 20.0;

        private const double HappyHealthThreshold = 80.0;
        private const double HappyHungerThreshold = 30.0;
        private const double NeutralHealthThreshold = 50.0;

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
            if (!IsEnabled)
                return;

            _updateTimer += deltaTime;
            if (_updateTimer < UpdateInterval)
                return;
            _updateTimer = 0;

            // Track which creatures still exist
            _existingCreaturesBuffer.Clear();

            foreach (var herbivore in herbivores)
            {
                if (!herbivore.IsAlive)
                    continue;

                _existingCreaturesBuffer.Add(herbivore);
                string mood = GetMoodEmoji(herbivore);
                UpdateOrCreateMoodVisual(herbivore, mood);
            }

            foreach (var carnivore in carnivores)
            {
                if (!carnivore.IsAlive)
                    continue;

                _existingCreaturesBuffer.Add(carnivore);
                string mood = GetMoodEmoji(carnivore);
                UpdateOrCreateMoodVisual(carnivore, mood);
            }

            // Remove visuals for dead/removed creatures
            _toRemoveBuffer.Clear();
            foreach (var kvp in _moodVisuals)
            {
                if (!_existingCreaturesBuffer.Contains(kvp.Key))
                {
                    _canvas.Children.Remove(kvp.Value.Visual);
                    _toRemoveBuffer.Add(kvp.Key);
                }
                else
                {
                    Canvas.SetLeft(kvp.Value.Visual, kvp.Key.X + IndicatorOffsetX);
                    Canvas.SetTop(kvp.Value.Visual, kvp.Key.Y + IndicatorOffsetY);
                }
            }

            foreach (var creature in _toRemoveBuffer)
            {
                _moodVisuals.Remove(creature);
            }
        }

        private string GetMoodEmoji(Creature creature)
        {
            // Priority-based mood selection
            if (creature.Health < CriticalHealthThreshold)
                return "💔"; // Very low health

            if (creature.Hunger > StarvingHungerThreshold)
                return "🍖"; // Starving, needs food

            if (creature.Hunger > HungryHungerThreshold)
                return "😋"; // Hungry

            // Check if being chased (for herbivores)
            if (creature is Herbivore && (Math.Abs(creature.VelocityX) > HerbivoreFleeSpeedThreshold || Math.Abs(creature.VelocityY) > HerbivoreFleeSpeedThreshold))
                return "😰"; // Fleeing

            // Check if hunting (for carnivores)
            if (creature is Carnivore && (Math.Abs(creature.VelocityX) > CarnivoreHuntSpeedThreshold || Math.Abs(creature.VelocityY) > CarnivoreHuntSpeedThreshold))
                return "🎯"; // Hunting

            if (creature.Health > HappyHealthThreshold && creature.Hunger < HappyHungerThreshold)
                return "😊"; // Happy and well-fed

            if (creature.Health > NeutralHealthThreshold)
                return "😐"; // Neutral

            return "😟"; // Worried (low-ish health)
        }

        private void UpdateOrCreateMoodVisual(Creature creature, string mood)
        {
            if (_moodVisuals.TryGetValue(creature, out var existing))
            {
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
                    FontSize = MoodFontSize,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                Canvas.SetLeft(textBlock, creature.X + IndicatorOffsetX);
                Canvas.SetTop(textBlock, creature.Y + IndicatorOffsetY);
                Canvas.SetZIndex(textBlock, MoodZIndex);

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
