namespace Terrarium.Desktop.Rendering;

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Terrarium.Logic.Simulation;

/// <summary>
/// Shows a visual indicator of ecosystem health and balance.
/// </summary>
public class EcosystemHealthBar
{
    private readonly Canvas canvas;
    private Border? container;
    private Rectangle? healthBar;
    private TextBlock? statusText;
    private TextBlock? scoreText;

    private LinearGradientBrush? healthGradient;
    private GradientStop? healthGradientLightStop;
    private GradientStop? healthGradientDarkStop;
    private SolidColorBrush? scoreForegroundBrush;
    private SolidColorBrush? statusForegroundBrush;

    private const double BarMaxWidth = 180;

    private double currentHealth;
    private double displayHealth;
    private double pulsePhase;

    private const double AnimationSpeed = 3.0;

    public bool IsEnabled { get; set; } = true;

    public bool IsVisible { get; set; } = true;

    public EcosystemHealthBar(Canvas canvas)
    {
        this.canvas = canvas;
        currentHealth = 100;
        displayHealth = 100;
        CreateUI();
    }

    private void CreateUI()
    {
        container = new Border
        {
            Width = 200,
            Background = CreateFrozenBrush(Color.FromArgb(200, 20, 30, 40)),
            BorderBrush = CreateFrozenBrush(Color.FromRgb(80, 80, 80)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(10)
        };

        var stack = new StackPanel();

        var titleRow = new Grid();
        titleRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        titleRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var title = new TextBlock
        {
            Text = "🌍 Ecosystem Health",
            FontSize = 11,
            FontWeight = FontWeights.Bold,
            Foreground = Brushes.White
        };
        Grid.SetColumn(title, 0);
        titleRow.Children.Add(title);

        scoreForegroundBrush = new SolidColorBrush(Color.FromRgb(100, 255, 150));
        scoreText = new TextBlock
        {
            Text = "100%",
            FontSize = 11,
            FontWeight = FontWeights.Bold,
            Foreground = scoreForegroundBrush
        };
        Grid.SetColumn(scoreText, 1);
        titleRow.Children.Add(scoreText);

        stack.Children.Add(titleRow);

        var barBg = new Border
        {
            Height = 12,
            Background = CreateFrozenBrush(Color.FromRgb(40, 40, 40)),
            CornerRadius = new CornerRadius(6),
            Margin = new Thickness(0, 6, 0, 6)
        };

        healthGradientLightStop = new GradientStop(Color.FromRgb(140, 255, 190), 0);
        healthGradientDarkStop = new GradientStop(Color.FromRgb(100, 255, 150), 1);
        healthGradient = new LinearGradientBrush
        {
            StartPoint = new Point(0, 0),
            EndPoint = new Point(0, 1),
            GradientStops = new GradientStopCollection
            {
                healthGradientLightStop,
                healthGradientDarkStop
            }
        };

        healthBar = new Rectangle
        {
            Height = 12,
            RadiusX = 6,
            RadiusY = 6,
            Fill = healthGradient
        };

        var barContainer = new Grid();
        barContainer.Children.Add(barBg);
        barContainer.Children.Add(healthBar);
        stack.Children.Add(barContainer);

        // Status text
        statusForegroundBrush = new SolidColorBrush(Color.FromRgb(100, 255, 150));
        statusText = new TextBlock
        {
            Text = "✨ Thriving",
            FontSize = 10,
            Foreground = statusForegroundBrush,
            TextAlignment = TextAlignment.Center
        };
        stack.Children.Add(statusText);

        container.Child = stack;

        Canvas.SetLeft(container, 10);
        Canvas.SetBottom(container, 80);
        Canvas.SetTop(container, double.NaN);
        Canvas.SetZIndex(container, 800);

        canvas.Children.Add(container);
    }

    /// <summary>
    /// Calculates and updates the ecosystem health score.
    /// </summary>
    public void Update(double deltaTime, int plantCount, int herbivoreCount, int carnivoreCount)
    {
        if (!IsEnabled)
        {
            if (container != null)
            {
                container.Visibility = Visibility.Collapsed;
            }

            return;
        }

        if (container != null)
        {
            container.Visibility = Visibility.Visible;
        }

        // Calculate health score based on ecosystem balance
        currentHealth = EcosystemHealthScorer.CalculateHealthPercent(plantCount, herbivoreCount, carnivoreCount);

        displayHealth += (currentHealth - displayHealth) * AnimationSpeed * deltaTime;
        displayHealth = Math.Clamp(displayHealth, 0, 100);

        pulsePhase += deltaTime * 2;

        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (healthBar == null || scoreText == null || statusText == null ||
            healthGradientLightStop == null || healthGradientDarkStop == null ||
            scoreForegroundBrush == null || statusForegroundBrush == null)
        {
            return;
        }

        healthBar.Width = (displayHealth / 100) * BarMaxWidth;

        var color = GetHealthColor(displayHealth);
        var lighterColor = Color.FromRgb(
            (byte)Math.Min(255, color.R + 40),
            (byte)Math.Min(255, color.G + 40),
            (byte)Math.Min(255, color.B + 40));
        healthGradientLightStop.Color = lighterColor;
        healthGradientDarkStop.Color = color;

        scoreText.Text = $"{displayHealth:F0}%";
        scoreForegroundBrush.Color = color;

        var (status, statusColor) = GetStatus(displayHealth);
        statusText.Text = status;
        statusForegroundBrush.Color = statusColor;

        // Pulse effect for critical health
        if (displayHealth < 25 && container != null)
        {
            double pulse = 0.7 + (0.3 * Math.Sin(pulsePhase * 3));
            container.Opacity = pulse;
        }
        else if (container != null)
        {
            container.Opacity = 1.0;
        }
    }

    private Color GetHealthColor(double health)
    {
        if (health >= 75)
        {
            return Color.FromRgb(100, 255, 150); // Green
        }

        if (health >= 50)
        {
            return Color.FromRgb(255, 220, 100); // Yellow
        }

        if (health >= 25)
        {
            return Color.FromRgb(255, 150, 100); // Orange
        }

        return Color.FromRgb(255, 80, 80); // Red
    }

    private (string status, Color color) GetStatus(double health)
    {
        if (health >= 80)
        {
            return ("✨ Thriving", Color.FromRgb(100, 255, 150));
        }

        if (health >= 60)
        {
            return ("🌱 Healthy", Color.FromRgb(180, 255, 100));
        }

        if (health >= 40)
        {
            return ("⚖️ Balanced", Color.FromRgb(255, 220, 100));
        }

        if (health >= 25)
        {
            return ("⚠️ Struggling", Color.FromRgb(255, 150, 100));
        }

        return ("🆘 Critical", Color.FromRgb(255, 80, 80));
    }

    private static SolidColorBrush CreateFrozenBrush(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }
}
