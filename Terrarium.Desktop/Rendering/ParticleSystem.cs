using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Terrarium.Desktop.Rendering
{
    /// <summary>
    /// Manages particle effects for visual feedback on events like eating, births, and deaths.
    /// </summary>
    public class ParticleSystem
    {
        private readonly Canvas _canvas;
        private readonly List<Particle> _particles;
        private readonly Random _random;

        // Particle limits
        private const int MaxParticles = 200;

        /// <summary>
        /// Gets or sets whether the particle system is enabled.
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        public ParticleSystem(Canvas canvas)
        {
            _canvas = canvas;
            _particles = new List<Particle>();
            _random = new Random();
        }

        /// <summary>
        /// Updates all active particles.
        /// </summary>
        public void Update(double deltaTime)
        {
            if (!IsEnabled) return;

            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                var particle = _particles[i];
                particle.Update(deltaTime);

                if (particle.IsDead)
                {
                    _canvas.Children.Remove(particle.Visual);
                    _particles.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Clears all particles.
        /// </summary>
        public void Clear()
        {
            foreach (var particle in _particles)
            {
                _canvas.Children.Remove(particle.Visual);
            }
            _particles.Clear();
        }

        /// <summary>
        /// Spawns eating particles (green sparkles when herbivore eats plant).
        /// </summary>
        public void SpawnEatEffect(double x, double y, bool isPlantEating = true)
        {
            var color = isPlantEating 
                ? Color.FromRgb(76, 175, 80)  // Green for plant eating
                : Color.FromRgb(200, 80, 80); // Red for carnivore eating

            SpawnBurst(x, y, color, 8, ParticleType.Sparkle);
        }

        /// <summary>
        /// Spawns birth celebration particles (hearts and sparkles).
        /// </summary>
        public void SpawnBirthEffect(double x, double y)
        {
            // Pink hearts
            SpawnBurst(x, y, Color.FromRgb(255, 182, 193), 6, ParticleType.Heart);
            // Yellow sparkles
            SpawnBurst(x, y, Color.FromRgb(255, 215, 0), 10, ParticleType.Sparkle);
        }

        /// <summary>
        /// Spawns death particles (gray fading wisps).
        /// </summary>
        public void SpawnDeathEffect(double x, double y)
        {
            SpawnBurst(x, y, Color.FromRgb(128, 128, 128), 12, ParticleType.Wisp);
        }

        /// <summary>
        /// Spawns water droplet particles (when clicking on plants).
        /// </summary>
        public void SpawnWaterEffect(double x, double y)
        {
            SpawnBurst(x, y, Color.FromRgb(52, 152, 219), 6, ParticleType.Droplet);
        }

        /// <summary>
        /// Spawns feed particles (when clicking on creatures).
        /// </summary>
        public void SpawnFeedEffect(double x, double y)
        {
            SpawnBurst(x, y, Color.FromRgb(255, 193, 7), 8, ParticleType.Sparkle);
        }

        /// <summary>
        /// Spawns a burst of particles at a location.
        /// </summary>
        private void SpawnBurst(double x, double y, Color color, int count, ParticleType type)
        {
            if (_particles.Count >= MaxParticles) return;

            for (int i = 0; i < count && _particles.Count < MaxParticles; i++)
            {
                var particle = CreateParticle(x, y, color, type);
                _particles.Add(particle);
                _canvas.Children.Add(particle.Visual);
            }
        }

        /// <summary>
        /// Creates a single particle.
        /// </summary>
        private Particle CreateParticle(double x, double y, Color color, ParticleType type)
        {
            UIElement visual;
            double size = 4 + _random.NextDouble() * 4;
            double lifetime = 0.5 + _random.NextDouble() * 0.5;
            double speed = 30 + _random.NextDouble() * 50;
            double angle = _random.NextDouble() * Math.PI * 2;

            switch (type)
            {
                case ParticleType.Heart:
                    visual = CreateHeartVisual(color, size);
                    lifetime = 1.0 + _random.NextDouble() * 0.5;
                    speed = 20 + _random.NextDouble() * 30;
                    break;

                case ParticleType.Wisp:
                    visual = CreateWispVisual(color, size);
                    lifetime = 1.5 + _random.NextDouble();
                    speed = 10 + _random.NextDouble() * 20;
                    angle = -Math.PI / 2 + (_random.NextDouble() - 0.5) * Math.PI / 4; // Mostly upward
                    break;

                case ParticleType.Droplet:
                    visual = CreateDropletVisual(color, size);
                    lifetime = 0.8 + _random.NextDouble() * 0.4;
                    angle = Math.PI / 2 + (_random.NextDouble() - 0.5) * Math.PI / 3; // Mostly downward
                    break;

                case ParticleType.Sparkle:
                default:
                    visual = CreateSparkleVisual(color, size);
                    break;
            }

            double vx = Math.Cos(angle) * speed;
            double vy = Math.Sin(angle) * speed;

            // Adjust for gravity on some types
            double gravity = type == ParticleType.Droplet ? 100 : 
                             type == ParticleType.Wisp ? -20 : 0;

            Canvas.SetLeft(visual, x);
            Canvas.SetTop(visual, y);

            return new Particle
            {
                Visual = visual,
                X = x,
                Y = y,
                VelocityX = vx,
                VelocityY = vy,
                Gravity = gravity,
                Lifetime = lifetime,
                MaxLifetime = lifetime,
                Type = type
            };
        }

        private UIElement CreateSparkleVisual(Color color, double size)
        {
            return new Ellipse
            {
                Width = size,
                Height = size,
                Fill = new RadialGradientBrush
                {
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop(color, 0),
                        new GradientStop(Color.FromArgb(0, color.R, color.G, color.B), 1)
                    }
                }
            };
        }

        private UIElement CreateHeartVisual(Color color, double size)
        {
            var textBlock = new TextBlock
            {
                Text = "❤",
                FontSize = size * 2,
                Foreground = new SolidColorBrush(color)
            };
            return textBlock;
        }

        private UIElement CreateWispVisual(Color color, double size)
        {
            return new Ellipse
            {
                Width = size * 1.5,
                Height = size * 2,
                Fill = new RadialGradientBrush
                {
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop(Color.FromArgb(180, color.R, color.G, color.B), 0),
                        new GradientStop(Color.FromArgb(0, color.R, color.G, color.B), 1)
                    }
                }
            };
        }

        private UIElement CreateDropletVisual(Color color, double size)
        {
            return new Ellipse
            {
                Width = size * 0.6,
                Height = size,
                Fill = new SolidColorBrush(color),
                Opacity = 0.8
            };
        }
    }

    /// <summary>
    /// Represents a single particle.
    /// </summary>
    internal class Particle
    {
        public UIElement Visual { get; set; } = null!;
        public double X { get; set; }
        public double Y { get; set; }
        public double VelocityX { get; set; }
        public double VelocityY { get; set; }
        public double Gravity { get; set; }
        public double Lifetime { get; set; }
        public double MaxLifetime { get; set; }
        public ParticleType Type { get; set; }

        public bool IsDead => Lifetime <= 0;

        public void Update(double deltaTime)
        {
            // Update physics
            VelocityY += Gravity * deltaTime;
            X += VelocityX * deltaTime;
            Y += VelocityY * deltaTime;

            // Update lifetime
            Lifetime -= deltaTime;

            // Update visual position
            Canvas.SetLeft(Visual, X);
            Canvas.SetTop(Visual, Y);

            // Fade out based on lifetime
            double lifeRatio = Math.Max(0, Lifetime / MaxLifetime);
            Visual.Opacity = lifeRatio;

            // Scale down hearts as they fade
            if (Type == ParticleType.Heart && Visual is TextBlock tb)
            {
                tb.RenderTransform = new ScaleTransform(lifeRatio, lifeRatio);
            }
        }
    }

    /// <summary>
    /// Types of particles.
    /// </summary>
    internal enum ParticleType
    {
        Sparkle,
        Heart,
        Wisp,
        Droplet
    }
}
