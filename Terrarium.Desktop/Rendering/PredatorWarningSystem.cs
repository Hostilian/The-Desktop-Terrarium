namespace Terrarium.Desktop.Rendering;

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Terrarium.Logic.Entities;

/// <summary>
/// Shows visual indicators when a predator is near prey.
/// </summary>
public class PredatorWarningSystem
{
    private readonly Canvas canvas;
    private readonly Dictionary<Herbivore, WarningVisual> warnings;
    private double updateTimer;

    private readonly List<Carnivore> carnivoreBuffer = new();
    private readonly HashSet<Herbivore> existingHerbivoresBuffer = new();
    private readonly List<Herbivore> toRemoveBuffer = new();

    private const double updateInterval = 0.2;
    private const double warningRadius = 100.0;
    private const double warningRadiusSquared = warningRadius * warningRadius;
    private const double pulseSpeed = 3.0;

    public bool IsEnabled { get; set; } = true;

    public bool IsVisible { get; set; } = true;

    public PredatorWarningSystem(Canvas canvas)
    {
        this.canvas = canvas;
        warnings = new Dictionary<Herbivore, WarningVisual>();
        updateTimer = 0;
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

        updateTimer += deltaTime;

        // Update pulse animation
        foreach (var warning in warnings.Values)
        {
            warning.PulsePhase += pulseSpeed * deltaTime;
            double scale = 1.0 + (0.2 * Math.Sin(warning.PulsePhase));
            warning.ScaleTransform.ScaleX = scale;
            warning.ScaleTransform.ScaleY = scale;
            warning.Visual.Opacity = 0.5 + (0.3 * Math.Sin(warning.PulsePhase));
        }

        if (updateTimer < updateInterval)
        {
            return;
        }
        updateTimer = 0;

        carnivoreBuffer.Clear();
        foreach (var carnivore in carnivores)
        {
            carnivoreBuffer.Add(carnivore);
        }

        existingHerbivoresBuffer.Clear();

        foreach (var herbivore in herbivores)
        {
            if (!herbivore.IsAlive)
            {
                continue;
            }

            existingHerbivoresBuffer.Add(herbivore);

            // Check distance to nearest carnivore
            double nearestDistanceSquared = double.MaxValue;
            foreach (var carnivore in carnivoreBuffer)
            {
                if (!carnivore.IsAlive)
                {
                    continue;
                }

                var dx = carnivore.X - herbivore.X;
                var dy = carnivore.Y - herbivore.Y;
                var distSquared = (dx * dx) + (dy * dy);
                if (distSquared < nearestDistanceSquared)
                {
                    nearestDistanceSquared = distSquared;
                }
            }

            bool inDanger = nearestDistanceSquared < warningRadiusSquared;

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
        toRemoveBuffer.Clear();
        foreach (var kvp in warnings)
        {
            if (!existingHerbivoresBuffer.Contains(kvp.Key))
            {
                canvas.Children.Remove(kvp.Value.Visual);
                toRemoveBuffer.Add(kvp.Key);
            }
            else
            {
                Canvas.SetLeft(kvp.Value.Visual, kvp.Key.X - 10);
                Canvas.SetTop(kvp.Value.Visual, kvp.Key.Y - 30);
            }
        }

        foreach (var herbivore in toRemoveBuffer)
        {
            warnings.Remove(herbivore);
        }
    }

    private void ShowWarning(Herbivore herbivore, double distance)
    {
        if (warnings.TryGetValue(herbivore, out var warning))
        {
            // Update intensity based on distance
            double intensity = 1.0 - (distance / warningRadius);

            // Closer = more red/urgent
            byte red = (byte)(255 * intensity);
            byte green = (byte)(100 * (1 - intensity));
            warning.ForegroundBrush.Color = Color.FromRgb(red, green, 0);
            return;
        }

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

        canvas.Children.Add(textBlock);
        warnings[herbivore] = new WarningVisual
        {
            Visual = textBlock,
            PulsePhase = 0,
            ForegroundBrush = brush,
            ScaleTransform = scaleTransform
        };
    }

    private void HideWarning(Herbivore herbivore)
    {
        if (warnings.TryGetValue(herbivore, out var warning))
        {
            canvas.Children.Remove(warning.Visual);
            warnings.Remove(herbivore);
        }
    }

    /// <summary>
    /// Clears all warning indicators.
    /// </summary>
    public void Clear()
    {
        foreach (var warning in warnings.Values)
        {
            canvas.Children.Remove(warning.Visual);
        }
        warnings.Clear();
    }
}

internal class WarningVisual
{
    public TextBlock Visual { get; set; } = null!;

    public double PulsePhase { get; set; }

    public SolidColorBrush ForegroundBrush { get; set; } = null!;

    public ScaleTransform ScaleTransform { get; set; } = null!;
}
