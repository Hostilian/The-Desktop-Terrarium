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
    /// Shows visual indicators when a predator is near prey.
    /// </summary>
    public class PredatorWarningSystem
    {
        private readonly Canvas _canvas;
        private readonly Dictionary<Herbivore, WarningVisual> _warnings;
        private double _updateTimer;

        private const double UpdateInterval = 0.2;
        private const double WarningRadius = 100.0;
        private const double PulseSpeed = 3.0;

        public bool IsEnabled { get; set; } = true;

        public PredatorWarningSystem(Canvas canvas)
        {
            _canvas = canvas;
            _warnings = new Dictionary<Herbivore, WarningVisual>();
            _updateTimer = 0;
        }

        /// <summary>
        /// Updates warning indicators.
        /// </summary>
        public void Update(double deltaTime, IEnumerable<Herbivore> herbivores, IEnumerable<Carnivore> carnivores)
        {
            if (!IsEnabled)
            {
                Clear();
                return;
            }

            _updateTimer += deltaTime;

            // Update pulse animation
            foreach (var warning in _warnings.Values)
            {
                warning.PulsePhase += PulseSpeed * deltaTime;
                double scale = 1.0 + 0.2 * Math.Sin(warning.PulsePhase);
                warning.Visual.RenderTransform = new ScaleTransform(scale, scale, 10, 10);
                warning.Visual.Opacity = 0.5 + 0.3 * Math.Sin(warning.PulsePhase);
            }

            if (_updateTimer < UpdateInterval) return;
            _updateTimer = 0;

            var carnivoreList = new List<Carnivore>(carnivores);
            var existingHerbivores = new HashSet<Herbivore>();

            foreach (var herbivore in herbivores)
            {
                if (!herbivore.IsAlive) continue;

                existingHerbivores.Add(herbivore);
                
                // Check distance to nearest carnivore
                double nearestDistance = double.MaxValue;
                foreach (var carnivore in carnivoreList)
                {
                    if (!carnivore.IsAlive) continue;
                    double dist = Math.Sqrt(
                        Math.Pow(carnivore.X - herbivore.X, 2) +
                        Math.Pow(carnivore.Y - herbivore.Y, 2));
                    nearestDistance = Math.Min(nearestDistance, dist);
                }

                bool inDanger = nearestDistance < WarningRadius;

                if (inDanger)
                {
                    ShowWarning(herbivore, nearestDistance);
                }
                else
                {
                    HideWarning(herbivore);
                }
            }

            // Remove warnings for dead/removed herbivores
            var toRemove = new List<Herbivore>();
            foreach (var kvp in _warnings)
            {
                if (!existingHerbivores.Contains(kvp.Key))
                {
                    _canvas.Children.Remove(kvp.Value.Visual);
                    toRemove.Add(kvp.Key);
                }
                else
                {
                    // Update position
                    Canvas.SetLeft(kvp.Value.Visual, kvp.Key.X - 10);
                    Canvas.SetTop(kvp.Value.Visual, kvp.Key.Y - 30);
                }
            }

            foreach (var herbivore in toRemove)
            {
                _warnings.Remove(herbivore);
            }
        }

        private void ShowWarning(Herbivore herbivore, double distance)
        {
            if (_warnings.ContainsKey(herbivore))
            {
                // Update intensity based on distance
                double intensity = 1.0 - (distance / WarningRadius);
                var warning = _warnings[herbivore];
                
                // Closer = more red/urgent
                byte red = (byte)(255 * intensity);
                byte green = (byte)(100 * (1 - intensity));
                warning.Visual.Foreground = new SolidColorBrush(Color.FromRgb(red, green, 0));
                return;
            }

            // Create new warning
            var textBlock = new TextBlock
            {
                Text = "⚠️",
                FontSize = 16,
                RenderTransformOrigin = new Point(0.5, 0.5),
                Foreground = new SolidColorBrush(Color.FromRgb(255, 100, 0))
            };

            Canvas.SetLeft(textBlock, herbivore.X - 10);
            Canvas.SetTop(textBlock, herbivore.Y - 30);
            Canvas.SetZIndex(textBlock, 600);

            _canvas.Children.Add(textBlock);
            _warnings[herbivore] = new WarningVisual { Visual = textBlock, PulsePhase = 0 };
        }

        private void HideWarning(Herbivore herbivore)
        {
            if (_warnings.TryGetValue(herbivore, out var warning))
            {
                _canvas.Children.Remove(warning.Visual);
                _warnings.Remove(herbivore);
            }
        }

        /// <summary>
        /// Clears all warning indicators.
        /// </summary>
        public void Clear()
        {
            foreach (var warning in _warnings.Values)
            {
                _canvas.Children.Remove(warning.Visual);
            }
            _warnings.Clear();
        }
    }

    internal class WarningVisual
    {
        public TextBlock Visual { get; set; } = null!;
        public double PulsePhase { get; set; }
    }
}
