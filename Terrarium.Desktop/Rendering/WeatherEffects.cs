using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Terrarium.Desktop.Rendering
{
    /// <summary>
    /// Handles weather visual effects like rain particles.
    /// </summary>
    public class WeatherEffects
    {
        private readonly Canvas _canvas;
        private readonly List<RainDrop> _rainDrops;
        private readonly Random _random;

        // Rain configuration
        private const int MaxRainDrops = 100;
        private const double RainDropMinSpeed = 200.0;
        private const double RainDropMaxSpeed = 400.0;
        private const double RainDropMinLength = 10.0;
        private const double RainDropMaxLength = 25.0;
        private const double RainDropWidth = 2.0;
        private const double RainAngle = 15.0; // Degrees from vertical

        // Visual properties
        private static readonly Brush RainColor = new SolidColorBrush(Color.FromArgb(180, 150, 180, 220));
        private static readonly Brush LightningColor = new SolidColorBrush(Color.FromArgb(200, 255, 255, 200));

        private double _currentIntensity;
        private bool _isRaining;
        private double _lightningTimer;
        private double _nextLightningTime;
        private Rectangle? _lightningFlash;

        /// <summary>
        /// Gets or sets whether weather effects are enabled.
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        public WeatherEffects(Canvas canvas)
        {
            _canvas = canvas;
            _rainDrops = new List<RainDrop>();
            _random = new Random();
            _currentIntensity = 0;
            _isRaining = false;
            _lightningTimer = 0;
            _nextLightningTime = _random.NextDouble() * 10 + 5; // 5-15 seconds
        }

        /// <summary>
        /// Updates weather effects based on intensity.
        /// </summary>
        public void Update(double deltaTime, double weatherIntensity)
        {
            if (!IsEnabled)
            {
                if (_isRaining) StopRain();
                return;
            }

            _currentIntensity = weatherIntensity;
            bool shouldRain = weatherIntensity > 0.5;

            if (shouldRain && !_isRaining)
            {
                StartRain();
            }
            else if (!shouldRain && _isRaining)
            {
                StopRain();
            }

            if (_isRaining)
            {
                UpdateRain(deltaTime);
                UpdateLightning(deltaTime, weatherIntensity);
            }
        }

        /// <summary>
        /// Starts the rain effect.
        /// </summary>
        private void StartRain()
        {
            _isRaining = true;
            SpawnInitialRainDrops();
        }

        /// <summary>
        /// Stops the rain effect.
        /// </summary>
        private void StopRain()
        {
            _isRaining = false;
            ClearAllRainDrops();
        }

        /// <summary>
        /// Spawns initial rain drops.
        /// </summary>
        private void SpawnInitialRainDrops()
        {
            int dropCount = (int)(MaxRainDrops * _currentIntensity);
            for (int i = 0; i < dropCount; i++)
            {
                SpawnRainDrop(randomizeY: true);
            }
        }

        /// <summary>
        /// Spawns a single rain drop.
        /// </summary>
        private void SpawnRainDrop(bool randomizeY = false)
        {
            double x = _random.NextDouble() * _canvas.ActualWidth;
            double y = randomizeY ? _random.NextDouble() * _canvas.ActualHeight : -20;
            double speed = RainDropMinSpeed + _random.NextDouble() * (RainDropMaxSpeed - RainDropMinSpeed);
            double length = RainDropMinLength + _random.NextDouble() * (RainDropMaxLength - RainDropMinLength);

            var line = new Line
            {
                X1 = 0,
                Y1 = 0,
                X2 = Math.Sin(RainAngle * Math.PI / 180) * length,
                Y2 = length,
                Stroke = RainColor,
                StrokeThickness = RainDropWidth,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round
            };

            Canvas.SetLeft(line, x);
            Canvas.SetTop(line, y);
            _canvas.Children.Add(line);

            _rainDrops.Add(new RainDrop
            {
                Visual = line,
                Speed = speed,
                X = x,
                Y = y
            });
        }

        /// <summary>
        /// Updates all rain drops.
        /// </summary>
        private void UpdateRain(double deltaTime)
        {
            // Adjust number of drops based on intensity
            int targetDropCount = (int)(MaxRainDrops * _currentIntensity);

            // Spawn more drops if needed
            while (_rainDrops.Count < targetDropCount)
            {
                SpawnRainDrop();
            }

            // Update existing drops
            var dropsToRemove = new List<RainDrop>();

            foreach (var drop in _rainDrops)
            {
                // Move the drop
                drop.Y += drop.Speed * deltaTime;
                drop.X += Math.Sin(RainAngle * Math.PI / 180) * drop.Speed * deltaTime * 0.3;

                Canvas.SetTop(drop.Visual, drop.Y);
                Canvas.SetLeft(drop.Visual, drop.X);

                // Check if drop is off screen
                if (drop.Y > _canvas.ActualHeight + 30)
                {
                    dropsToRemove.Add(drop);
                }
            }

            // Remove off-screen drops
            foreach (var drop in dropsToRemove)
            {
                _canvas.Children.Remove(drop.Visual);
                _rainDrops.Remove(drop);
            }
        }

        /// <summary>
        /// Updates lightning effects during severe storms.
        /// </summary>
        private void UpdateLightning(double deltaTime, double weatherIntensity)
        {
            // Only show lightning during intense storms
            if (weatherIntensity < 0.8) return;

            _lightningTimer += deltaTime;

            // Flash lightning
            if (_lightningTimer >= _nextLightningTime)
            {
                ShowLightningFlash();
                _lightningTimer = 0;
                _nextLightningTime = _random.NextDouble() * 8 + 2; // 2-10 seconds
            }

            // Fade out flash
            if (_lightningFlash != null && _lightningFlash.Opacity > 0)
            {
                _lightningFlash.Opacity -= deltaTime * 5; // Fast fade
                if (_lightningFlash.Opacity <= 0)
                {
                    _canvas.Children.Remove(_lightningFlash);
                    _lightningFlash = null;
                }
            }
        }

        /// <summary>
        /// Shows a lightning flash effect.
        /// </summary>
        private void ShowLightningFlash()
        {
            if (_lightningFlash != null)
            {
                _canvas.Children.Remove(_lightningFlash);
            }

            _lightningFlash = new Rectangle
            {
                Width = _canvas.ActualWidth,
                Height = _canvas.ActualHeight,
                Fill = LightningColor,
                Opacity = 0.6
            };

            Canvas.SetLeft(_lightningFlash, 0);
            Canvas.SetTop(_lightningFlash, 0);
            Panel.SetZIndex(_lightningFlash, 1000); // On top of everything

            _canvas.Children.Add(_lightningFlash);
        }

        /// <summary>
        /// Clears all rain drops from the canvas.
        /// </summary>
        private void ClearAllRainDrops()
        {
            foreach (var drop in _rainDrops)
            {
                _canvas.Children.Remove(drop.Visual);
            }
            _rainDrops.Clear();

            if (_lightningFlash != null)
            {
                _canvas.Children.Remove(_lightningFlash);
                _lightningFlash = null;
            }
        }

        /// <summary>
        /// Cleans up all weather effects.
        /// </summary>
        public void Clear()
        {
            ClearAllRainDrops();
        }

        /// <summary>
        /// Internal class to track rain drop data.
        /// </summary>
        private class RainDrop
        {
            public Line Visual { get; set; } = null!;
            public double Speed { get; set; }
            public double X { get; set; }
            public double Y { get; set; }
        }
    }
}
