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

        private readonly List<Carnivore> _carnivoreBuffer = new();
        private readonly HashSet<Herbivore> _existingHerbivoresBuffer = new();
        private readonly List<Herbivore> _toRemoveBuffer = new();

        private const double UpdateInterval = 0.2;
        private const double WarningRadius = 100.0;
        private const double WarningRadiusSquared = WarningRadius * WarningRadius;
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
                warning.ScaleTransform.ScaleX = scale;
                warning.ScaleTransform.ScaleY = scale;
                warning.Visual.Opacity = 0.5 + 0.3 * Math.Sin(warning.PulsePhase);
            }

            if (_updateTimer < UpdateInterval)
                return;
            _updateTimer = 0;

            _carnivoreBuffer.Clear();
            foreach (var carnivore in carnivores)
            {
                _carnivoreBuffer.Add(carnivore);
            }

            _existingHerbivoresBuffer.Clear();

            foreach (var herbivore in herbivores)
            {
                if (!herbivore.IsAlive)
                    continue;

                _existingHerbivoresBuffer.Add(herbivore);

                // Check distance to nearest carnivore
                double nearestDistanceSquared = double.MaxValue;
                foreach (var carnivore in _carnivoreBuffer)
                {
                    if (!carnivore.IsAlive)
                        continue;

                    var dx = carnivore.X - herbivore.X;
                    var dy = carnivore.Y - herbivore.Y;
                    var distSquared = (dx * dx) + (dy * dy);
                    if (distSquared < nearestDistanceSquared)
                    {
                        nearestDistanceSquared = distSquared;
                    }
                }

                bool inDanger = nearestDistanceSquared < WarningRadiusSquared;

                if (inDanger)
                {
                    ShowWarning(herbivore, Math.Sqrt(nearestDistanceSquared));
                }
                else
                {
                    HideWarning(herbivore);
                }
            }

            // Remove warnings for dead/removed herbivores
            _toRemoveBuffer.Clear();
            foreach (var kvp in _warnings)
            {
                if (!_existingHerbivoresBuffer.Contains(kvp.Key))
                {
                    _canvas.Children.Remove(kvp.Value.Visual);
                    _toRemoveBuffer.Add(kvp.Key);
                }
                else
                {
                    // Update position
                    Canvas.SetLeft(kvp.Value.Visual, kvp.Key.X - 10);
                    Canvas.SetTop(kvp.Value.Visual, kvp.Key.Y - 30);
                }
            }

            foreach (var herbivore in _toRemoveBuffer)
            {
                _warnings.Remove(herbivore);
            }
        }

        private void ShowWarning(Herbivore herbivore, double distance)
        {
            if (_warnings.TryGetValue(herbivore, out var warning))
            {
                // Update intensity based on distance
                double intensity = 1.0 - (distance / WarningRadius);

                // Closer = more red/urgent
                byte red = (byte)(255 * intensity);
                byte green = (byte)(100 * (1 - intensity));
                warning.ForegroundBrush.Color = Color.FromRgb(red, green, 0);
                return;
            }

            // Create new warning
            var brush = new SolidColorBrush(Color.FromRgb(255, 100, 0));
            var scaleTransform = new ScaleTransform(1.0, 1.0, 10, 10);
            var textBlock = new TextBlock
            {
                Text = "⚠️",
                FontSize = 16,
                RenderTransformOrigin = new Point(0.5, 0.5),
                Foreground = brush,
                RenderTransform = scaleTransform
            };

            Canvas.SetLeft(textBlock, herbivore.X - 10);
            Canvas.SetTop(textBlock, herbivore.Y - 30);
            Canvas.SetZIndex(textBlock, 600);

            _canvas.Children.Add(textBlock);
            _warnings[herbivore] = new WarningVisual
            {
                Visual = textBlock,
                PulsePhase = 0,
                ForegroundBrush = brush,
                ScaleTransform = scaleTransform
            };
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
        public SolidColorBrush ForegroundBrush { get; set; } = null!;
        public ScaleTransform ScaleTransform { get; set; } = null!;
    }
}
