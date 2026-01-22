namespace Terrarium.Desktop.Rendering;

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

/// <summary>
/// Handles weather visual effects like rain particles.
/// </summary>
public class WeatherEffects
{
    private readonly Canvas canvas;
    private readonly List<RainDrop> rainDrops;
    private readonly Random random;

    // Rain configuration
    private const int MaxRainDrops = 100;
    private const double RainDropMinSpeed = 200.0;
    private const double RainDropMaxSpeed = 400.0;
    private const double RainDropMinLength = 10.0;
    private const double RainDropMaxLength = 25.0;
    private const double RainDropWidth = 2.0;
    private const double RainAngle = 15.0; // Degrees from vertical
    private const double DegreesToRadians = Math.PI / 180.0;
    private const double RainAngleRadians = RainAngle * DegreesToRadians;

    private const double RainIntensityThreshold = 0.5;
    private const double RainSpawnStartYOffset = -20.0;
    private const double RainDriftSpeedMultiplier = 0.3;
    private const double RainOffscreenBottomPadding = 30.0;

    private const double LightningIntensityThreshold = 0.8;
    private const double InitialLightningIntervalMinSeconds = 5.0;
    private const double InitialLightningIntervalRangeSeconds = 10.0;
    private const double LightningIntervalMinSeconds = 2.0;
    private const double LightningIntervalRangeSeconds = 8.0;
    private const double LightningFlashInitialOpacity = 0.6;
    private const double LightningFlashFadeRate = 5.0;
    private const int LightningFlashZIndex = 1000;

    // Visual properties
    private static readonly Brush RainColor = CreateFrozenBrush(Color.FromArgb(180, 150, 180, 220));
    private static readonly Brush LightningColor = CreateFrozenBrush(Color.FromArgb(200, 255, 255, 200));

    private double currentIntensity;
    private bool isRaining;
    private double lightningTimer;
    private double nextLightningTime;
    private Rectangle? lightningFlash;

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether weather effects are enabled.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    public WeatherEffects(Canvas canvas)
    {
        this.canvas = canvas;
        rainDrops = new List<RainDrop>();
        random = new Random();
        currentIntensity = 0;
        isRaining = false;
        lightningTimer = 0;
        nextLightningTime = (random.NextDouble() * InitialLightningIntervalRangeSeconds) + InitialLightningIntervalMinSeconds; // 5-15 seconds
    }

    /// <summary>
    /// Updates weather effects based on intensity.
    /// </summary>
    public void Update(double deltaTime, double weatherIntensity)
    {
        if (!IsEnabled)
        {
            if (isRaining)
            {
                StopRain();
            }

            return;
        }

        currentIntensity = weatherIntensity;
        bool shouldRain = weatherIntensity > RainIntensityThreshold;

        if (shouldRain && !isRaining)
        {
            StartRain();
        }
        else if (!shouldRain && isRaining)
        {
            StopRain();
        }

        if (isRaining)
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
        isRaining = true;
        SpawnInitialRainDrops();
    }

    /// <summary>
    /// Stops the rain effect.
    /// </summary>
    private void StopRain()
    {
        isRaining = false;
        ClearAllRainDrops();
    }

    /// <summary>
    /// Spawns initial rain drops.
    /// </summary>
    private void SpawnInitialRainDrops()
    {
        int dropCount = (int)(MaxRainDrops * currentIntensity);
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
        double x = random.NextDouble() * canvas.ActualWidth;
        double y = randomizeY ? random.NextDouble() * canvas.ActualHeight : RainSpawnStartYOffset;
        double speed = RainDropMinSpeed + (random.NextDouble() * (RainDropMaxSpeed - RainDropMinSpeed));
        double length = RainDropMinLength + (random.NextDouble() * (RainDropMaxLength - RainDropMinLength));

        var line = new Line
        {
            X1 = 0,
            Y1 = 0,
            X2 = Math.Sin(RainAngleRadians) * length,
            Y2 = length,
            Stroke = RainColor,
            StrokeThickness = RainDropWidth,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeEndLineCap = PenLineCap.Round
        };

        Canvas.SetLeft(line, x);
        Canvas.SetTop(line, y);
        canvas.Children.Add(line);

        rainDrops.Add(new RainDrop
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
        int targetDropCount = (int)(MaxRainDrops * currentIntensity);

        // Spawn more drops if needed
        while (rainDrops.Count < targetDropCount)
        {
            SpawnRainDrop();
        }

        // Update existing drops (iterate backwards so we can remove in-place)
        double driftStep = Math.Sin(RainAngleRadians) * RainDriftSpeedMultiplier;
        double offscreenY = canvas.ActualHeight + RainOffscreenBottomPadding;

        for (int i = rainDrops.Count - 1; i >= 0; i--)
        {
            var drop = rainDrops[i];

            // Move the drop
            drop.Y += drop.Speed * deltaTime;
            drop.X += driftStep * drop.Speed * deltaTime;

            Canvas.SetTop(drop.Visual, drop.Y);
            Canvas.SetLeft(drop.Visual, drop.X);

            // Remove off-screen drops
            if (drop.Y > offscreenY)
            {
                canvas.Children.Remove(drop.Visual);
                rainDrops.RemoveAt(i);
            }
        }
    }

    private static SolidColorBrush CreateFrozenBrush(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }

    /// <summary>
    /// Updates lightning effects during severe storms.
    /// </summary>
    private void UpdateLightning(double deltaTime, double weatherIntensity)
    {
        // Only show lightning during intense storms
        if (weatherIntensity < LightningIntensityThreshold)
        {
            return;
        }

        lightningTimer += deltaTime;

        // Flash lightning
        if (lightningTimer >= nextLightningTime)
        {
            ShowLightningFlash();
            lightningTimer = 0;
            nextLightningTime = (random.NextDouble() * LightningIntervalRangeSeconds) + LightningIntervalMinSeconds; // 2-10 seconds
        }

        if (lightningFlash != null && lightningFlash.Opacity > 0)
        {
            lightningFlash.Opacity -= deltaTime * LightningFlashFadeRate; // Fast fade
            if (lightningFlash.Opacity <= 0)
            {
                canvas.Children.Remove(lightningFlash);
                lightningFlash = null;
            }
        }
    }

    /// <summary>
    /// Shows a lightning flash effect.
    /// </summary>
    private void ShowLightningFlash()
    {
        if (lightningFlash != null)
        {
            canvas.Children.Remove(lightningFlash);
        }

        lightningFlash = new Rectangle
        {
            Width = canvas.ActualWidth,
            Height = canvas.ActualHeight,
            Fill = LightningColor,
            Opacity = LightningFlashInitialOpacity
        };

        Canvas.SetLeft(lightningFlash, 0);
        Canvas.SetTop(lightningFlash, 0);
        Panel.SetZIndex(lightningFlash, LightningFlashZIndex); // On top of everything

        canvas.Children.Add(lightningFlash);
    }

    /// <summary>
    /// Clears all rain drops from the canvas.
    /// </summary>
    private void ClearAllRainDrops()
    {
        foreach (var drop in rainDrops)
        {
            canvas.Children.Remove(drop.Visual);
        }
        rainDrops.Clear();

        if (lightningFlash != null)
        {
            canvas.Children.Remove(lightningFlash);
            lightningFlash = null;
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
