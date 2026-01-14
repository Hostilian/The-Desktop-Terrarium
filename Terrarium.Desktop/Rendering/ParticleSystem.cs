using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Terrarium.Desktop.Rendering;

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

    private const double ParticleBaseSizeMin = 4.0;
    private const double ParticleBaseSizeRange = 4.0;
    private const double ParticleDefaultLifetimeMinSeconds = 0.5;
    private const double ParticleDefaultLifetimeRangeSeconds = 0.5;
    private const double ParticleDefaultSpeedMin = 30.0;
    private const double ParticleDefaultSpeedRange = 50.0;
    private const double FullCircleRadians = Math.PI * 2.0;

    private const double HeartLifetimeMinSeconds = 1.0;
    private const double HeartLifetimeRangeSeconds = 0.5;
    private const double HeartSpeedMin = 20.0;
    private const double HeartSpeedRange = 30.0;
    private const double HeartFontSizeMultiplier = 2.0;

    private const double WispLifetimeMinSeconds = 1.5;
    private const double WispLifetimeRangeSeconds = 1.0;
    private const double WispSpeedMin = 10.0;
    private const double WispSpeedRange = 20.0;
    private const double WispBaseAngleRadians = -Math.PI / 2.0;
    private const double WispAngleJitterRadians = Math.PI / 4.0;

    private const double DropletLifetimeMinSeconds = 0.8;
    private const double DropletLifetimeRangeSeconds = 0.4;
    private const double DropletBaseAngleRadians = Math.PI / 2.0;
    private const double DropletAngleJitterRadians = Math.PI / 3.0;
    private const double DropletOpacity = 0.8;

    private const double DefaultGravity = 0.0;
    private const double DropletGravity = 100.0;
    private const double WispGravity = -20.0;

    private const double GradientStopInnerPosition = 0.0;
    private const double GradientStopOuterPosition = 1.0;

    private const double WispWidthMultiplier = 1.5;
    private const double WispHeightMultiplier = 2.0;
    private const double DropletWidthMultiplier = 0.6;

    private const byte TransparentAlpha = 0;
    private const byte WispGradientAlpha = 180;
    private const double RandomCenterOffset = 0.5;

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
        if (!IsEnabled)
        {
            return;
        }

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
        if (_particles.Count >= MaxParticles)
        {
            return;
        }

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
        double size = ParticleBaseSizeMin + _random.NextDouble() * ParticleBaseSizeRange;
        double lifetime = ParticleDefaultLifetimeMinSeconds + _random.NextDouble() * ParticleDefaultLifetimeRangeSeconds;
        double speed = ParticleDefaultSpeedMin + _random.NextDouble() * ParticleDefaultSpeedRange;
        double angle = _random.NextDouble() * FullCircleRadians;

        switch (type)
        {
            case ParticleType.Heart:
                visual = CreateHeartVisual(color, size);
                lifetime = HeartLifetimeMinSeconds + _random.NextDouble() * HeartLifetimeRangeSeconds;
                speed = HeartSpeedMin + _random.NextDouble() * HeartSpeedRange;
                break;

            case ParticleType.Wisp:
                visual = CreateWispVisual(color, size);
                lifetime = WispLifetimeMinSeconds + _random.NextDouble() * WispLifetimeRangeSeconds;
                speed = WispSpeedMin + _random.NextDouble() * WispSpeedRange;
                angle = WispBaseAngleRadians + (_random.NextDouble() - RandomCenterOffset) * WispAngleJitterRadians; // Mostly upward
                break;

            case ParticleType.Droplet:
                visual = CreateDropletVisual(color, size);
                lifetime = DropletLifetimeMinSeconds + _random.NextDouble() * DropletLifetimeRangeSeconds;
                angle = DropletBaseAngleRadians + (_random.NextDouble() - RandomCenterOffset) * DropletAngleJitterRadians; // Mostly downward
                break;

            case ParticleType.Sparkle:
            default:
                visual = CreateSparkleVisual(color, size);
                break;
        }

        double vx = Math.Cos(angle) * speed;
        double vy = Math.Sin(angle) * speed;

        // Adjust for gravity on some types
        double gravity = type == ParticleType.Droplet ? DropletGravity :
                         type == ParticleType.Wisp ? WispGravity : DefaultGravity;

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
        var sparkleBrush = new RadialGradientBrush
        {
            GradientStops = new GradientStopCollection
            {
                new GradientStop(color, GradientStopInnerPosition),
                new GradientStop(Color.FromArgb(TransparentAlpha, color.R, color.G, color.B), GradientStopOuterPosition)
            }
        };
        sparkleBrush.Freeze();
        return new Ellipse
        {
            Width = size,
            Height = size,
            Fill = sparkleBrush
        };
    }

    private UIElement CreateHeartVisual(Color color, double size)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        var textBlock = new TextBlock
        {
            Text = "❤",
            FontSize = size * HeartFontSizeMultiplier,
            Foreground = brush,
            RenderTransformOrigin = new Point(0.5, 0.5),
            RenderTransform = new ScaleTransform(1, 1)
        };
        return textBlock;
    }

    private UIElement CreateWispVisual(Color color, double size)
    {
        var wispBrush = new RadialGradientBrush
        {
            GradientStops = new GradientStopCollection
            {
                new GradientStop(Color.FromArgb(WispGradientAlpha, color.R, color.G, color.B), GradientStopInnerPosition),
                new GradientStop(Color.FromArgb(TransparentAlpha, color.R, color.G, color.B), GradientStopOuterPosition)
            }
        };
        wispBrush.Freeze();
        return new Ellipse
        {
            Width = size * WispWidthMultiplier,
            Height = size * WispHeightMultiplier,
            Fill = wispBrush
        };
    }

    private UIElement CreateDropletVisual(Color color, double size)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return new Ellipse
        {
            Width = size * DropletWidthMultiplier,
            Height = size,
            Fill = brush,
            Opacity = DropletOpacity
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
        VelocityY += Gravity * deltaTime;
        X += VelocityX * deltaTime;
        Y += VelocityY * deltaTime;

        Lifetime -= deltaTime;

        Canvas.SetLeft(Visual, X);
        Canvas.SetTop(Visual, Y);

        double lifeRatio = Math.Max(0, Lifetime / MaxLifetime);
        Visual.Opacity = lifeRatio;

        // Scale down hearts as they fade
        if (Type == ParticleType.Heart && Visual is TextBlock tb)
        {
            if (tb.RenderTransform is not ScaleTransform scaleTransform)
            {
                tb.RenderTransformOrigin = new Point(0.5, 0.5);
                scaleTransform = new ScaleTransform(1.0, 1.0);
                tb.RenderTransform = scaleTransform;
            }

            scaleTransform.ScaleX = lifeRatio;
            scaleTransform.ScaleY = lifeRatio;
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
