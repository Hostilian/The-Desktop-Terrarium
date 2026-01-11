using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Terrarium.Logic.Entities;

namespace Terrarium.Desktop.Rendering
{
    /// <summary>
    /// Shows visual indicators for breeding-ready creatures.
    /// </summary>
    public class BreedingIndicator
    {
        private readonly Canvas _canvas;
        private readonly Dictionary<Creature, HeartVisual> _hearts;
        private double _updateTimer;
        private double _animationTime;

        private readonly HashSet<Creature> _existingCreaturesBuffer = new();
        private readonly List<Creature> _toRemoveBuffer = new();

        private const double UpdateInterval = 0.3;
        private const double MinHealthForBreeding = 60;
        private const double MaxHungerForBreeding = 40;
        private const double MinAgeForBreeding = 5.0;

        public bool IsEnabled { get; set; } = true;

        public BreedingIndicator(Canvas canvas)
        {
            _canvas = canvas;
            _hearts = new Dictionary<Creature, HeartVisual>();
            _updateTimer = 0;
            _animationTime = 0;
        }

        /// <summary>
        /// Updates breeding indicators.
        /// </summary>
        public void Update(double deltaTime, IEnumerable<Herbivore> herbivores, IEnumerable<Carnivore> carnivores)
        {
            if (!IsEnabled)
            {
                Clear();
                return;
            }

            _animationTime += deltaTime;
            _updateTimer += deltaTime;

            // Update existing heart animations
            foreach (var heart in _hearts.Values)
            {
                double bounce = Math.Sin(_animationTime * 4 + heart.AnimOffset) * 3;
                Canvas.SetTop(heart.Visual, heart.BaseY + bounce);

                // Pulse effect
                double scale = 1.0 + 0.15 * Math.Sin(_animationTime * 3 + heart.AnimOffset);
                heart.ScaleTransform.ScaleX = scale;
                heart.ScaleTransform.ScaleY = scale;
            }

            if (_updateTimer < UpdateInterval)
                return;
            _updateTimer = 0;

            _existingCreaturesBuffer.Clear();

            // Check herbivores
            foreach (var herbivore in herbivores)
            {
                if (!herbivore.IsAlive)
                    continue;
                _existingCreaturesBuffer.Add(herbivore);

                if (CanBreed(herbivore))
                {
                    ShowHeart(herbivore);
                }
                else
                {
                    HideHeart(herbivore);
                }
            }

            // Check carnivores
            foreach (var carnivore in carnivores)
            {
                if (!carnivore.IsAlive)
                    continue;
                _existingCreaturesBuffer.Add(carnivore);

                if (CanBreed(carnivore))
                {
                    ShowHeart(carnivore);
                }
                else
                {
                    HideHeart(carnivore);
                }
            }

            // Clean up removed creatures
            _toRemoveBuffer.Clear();
            foreach (var kvp in _hearts)
            {
                var creature = kvp.Key;
                var heart = kvp.Value;
                if (!_existingCreaturesBuffer.Contains(creature))
                {
                    _canvas.Children.Remove(heart.Visual);
                    _toRemoveBuffer.Add(creature);
                }
                else
                {
                    Canvas.SetLeft(heart.Visual, creature.X - 6);
                    heart.BaseY = creature.Y - 28;
                }
            }

            foreach (var creature in _toRemoveBuffer)
            {
                _hearts.Remove(creature);
            }
        }

        private bool CanBreed(Creature creature)
        {
            return creature.IsAlive &&
                   creature.Health >= MinHealthForBreeding &&
                   creature.Hunger <= MaxHungerForBreeding &&
                   creature.Age >= MinAgeForBreeding;
        }

        private void ShowHeart(Creature creature)
        {
            if (_hearts.ContainsKey(creature))
                return;

            var scaleTransform = new ScaleTransform(1.0, 1.0, 6, 6);
            var heart = new TextBlock
            {
                Text = "💕",
                FontSize = 12,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = scaleTransform
            };

            Canvas.SetLeft(heart, creature.X - 6);
            Canvas.SetTop(heart, creature.Y - 28);
            Canvas.SetZIndex(heart, 550);

            _canvas.Children.Add(heart);

            _hearts[creature] = new HeartVisual
            {
                Visual = heart,
                BaseY = creature.Y - 28,
                AnimOffset = Random.Shared.NextDouble() * Math.PI * 2,
                ScaleTransform = scaleTransform
            };
        }

        private void HideHeart(Creature creature)
        {
            if (_hearts.TryGetValue(creature, out var heart))
            {
                _canvas.Children.Remove(heart.Visual);
                _hearts.Remove(creature);
            }
        }

        /// <summary>
        /// Clears all breeding indicators.
        /// </summary>
        public void Clear()
        {
            foreach (var heart in _hearts.Values)
            {
                _canvas.Children.Remove(heart.Visual);
            }
            _hearts.Clear();
        }
    }

    internal class HeartVisual
    {
        public TextBlock Visual { get; set; } = null!;
        public double BaseY { get; set; }
        public double AnimOffset { get; set; }
        public ScaleTransform ScaleTransform { get; set; } = null!;
    }
}
