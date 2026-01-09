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
                heart.Visual.RenderTransform = new ScaleTransform(scale, scale, 6, 6);
            }

            if (_updateTimer < UpdateInterval) return;
            _updateTimer = 0;

            var existingCreatures = new HashSet<Creature>();

            // Check herbivores
            foreach (var herbivore in herbivores)
            {
                if (!herbivore.IsAlive) continue;
                existingCreatures.Add(herbivore);
                
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
                if (!carnivore.IsAlive) continue;
                existingCreatures.Add(carnivore);
                
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
            var toRemove = new List<Creature>();
            foreach (var creature in _hearts.Keys)
            {
                if (!existingCreatures.Contains(creature))
                {
                    _canvas.Children.Remove(_hearts[creature].Visual);
                    toRemove.Add(creature);
                }
                else
                {
                    // Update position
                    var heart = _hearts[creature];
                    Canvas.SetLeft(heart.Visual, creature.X - 6);
                    heart.BaseY = creature.Y - 28;
                }
            }

            foreach (var creature in toRemove)
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
            if (_hearts.ContainsKey(creature)) return;

            var heart = new TextBlock
            {
                Text = "💕",
                FontSize = 12,
                RenderTransformOrigin = new Point(0.5, 0.5)
            };

            Canvas.SetLeft(heart, creature.X - 6);
            Canvas.SetTop(heart, creature.Y - 28);
            Canvas.SetZIndex(heart, 550);

            _canvas.Children.Add(heart);

            _hearts[creature] = new HeartVisual
            {
                Visual = heart,
                BaseY = creature.Y - 28,
                AnimOffset = new Random().NextDouble() * Math.PI * 2
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
    }
}
