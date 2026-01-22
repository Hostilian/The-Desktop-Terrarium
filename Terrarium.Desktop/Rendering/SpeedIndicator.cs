namespace Terrarium.Desktop.Rendering;

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

/// <summary>
/// Shows the current simulation speed with visual feedback.
/// </summary>
public class SpeedIndicator
{
    private readonly Canvas canvas;
    private Border? container;
    private TextBlock? speedText;
    private readonly Rectangle[] speedBars;
    private readonly SolidColorBrush[] speedBarBrushes;
    private double currentSpeed;
    private double displaySpeed;
    private double hideTimer;

    private int lastDisplayedSpeedTenths = int.MinValue;
    private Brush? lastSpeedForeground;

    private readonly SolidColorBrush speedTextSlowBrush = new(Color.FromRgb(100, 150, 255));
    private readonly SolidColorBrush speedTextFastBrush = new(Color.FromRgb(255, 150, 100));
    private readonly SolidColorBrush speedTextNormalBrush = new(Colors.White);

    private static readonly Color InactiveBarColor = Color.FromRgb(60, 60, 60);

    private const double HideDelay = 3.0;
    private const double AnimationSpeed = 8.0;

    public bool IsEnabled { get; set; } = true;

    public SpeedIndicator(Canvas canvas)
    {
        this.canvas = canvas;
        speedBars = new Rectangle[5];
        speedBarBrushes = new SolidColorBrush[5];
        currentSpeed = 1.0;
        displaySpeed = 1.0;

        if (speedTextSlowBrush.CanFreeze)
        {
            speedTextSlowBrush.Freeze();
        }

        if (speedTextFastBrush.CanFreeze)
        {
            speedTextFastBrush.Freeze();
        }

        if (speedTextNormalBrush.CanFreeze)
        {
            speedTextNormalBrush.Freeze();
        }

        CreateUI();
    }

    private void CreateUI()
    {
        container = new Border
        {
            Background = CreateFrozenBrush(Color.FromArgb(200, 20, 30, 40)),
            BorderBrush = CreateFrozenBrush(Color.FromRgb(100, 100, 100)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(12, 8, 12, 8),
            Opacity = 0
        };

        var stack = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center
        };

        // Speed icon
        var icon = new TextBlock
        {
            Text = "⏱️",
            FontSize = 14,
            Margin = new Thickness(0, 0, 8, 0),
            VerticalAlignment = VerticalAlignment.Center
        };
        stack.Children.Add(icon);

        // Speed bars
        var barsPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 8, 0)
        };

        for (int i = 0; i < 5; i++)
        {
            var brush = new SolidColorBrush(InactiveBarColor);
            speedBarBrushes[i] = brush;
            speedBars[i] = new Rectangle
            {
                Width = 4,
                Height = 8 + (i * 3),
                Fill = brush,
                Margin = new Thickness(2, 0, 2, 0),
                RadiusX = 1,
                RadiusY = 1,
                VerticalAlignment = VerticalAlignment.Bottom
            };
            barsPanel.Children.Add(speedBars[i]);
        }
        stack.Children.Add(barsPanel);

        speedText = new TextBlock
        {
            Text = "1.0x",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = Brushes.White,
            VerticalAlignment = VerticalAlignment.Center
        };
        stack.Children.Add(speedText);

        container.Child = stack;

        Canvas.SetZIndex(container, 750);
        canvas.Children.Add(container);

        UpdatePosition();
    }

    private void UpdatePosition()
    {
        if (container == null)
        {
            return;
        }

        double canvasWidth = canvas.ActualWidth > 0 ? canvas.ActualWidth : 800;
        Canvas.SetLeft(container, (canvasWidth - 120) / 2);
        Canvas.SetBottom(container, 20);
        Canvas.SetTop(container, double.NaN);
    }

    /// <summary>
    /// Sets the current simulation speed.
    /// </summary>
    public void SetSpeed(double speed)
    {
        if (Math.Abs(currentSpeed - speed) > 0.01)
        {
            currentSpeed = speed;
            hideTimer = HideDelay;
            this.Show();
        }
    }

    /// <summary>
    /// Updates the indicator visuals.
    /// </summary>
    public void Update(double deltaTime)
    {
        if (!IsEnabled || container == null)
        {
            return;
        }

        displaySpeed += (currentSpeed - displaySpeed) * AnimationSpeed * deltaTime;
        displaySpeed = Math.Clamp(displaySpeed, 0.1, 5.0);

        if (speedText != null)
        {
            int speedTenths = (int)Math.Round(displaySpeed * 10.0);
            if (speedTenths != lastDisplayedSpeedTenths)
            {
                speedText.Text = $"{speedTenths / 10.0:F1}x";
                lastDisplayedSpeedTenths = speedTenths;
            }

            Brush desiredForeground = displaySpeed switch
            {
                < 0.5 => speedTextSlowBrush,
                > 2.0 => speedTextFastBrush,
                _ => speedTextNormalBrush
            };

            if (!ReferenceEquals(lastSpeedForeground, desiredForeground))
            {
                speedText.Foreground = desiredForeground;
                lastSpeedForeground = desiredForeground;
            }
        }

        int activeBars = displaySpeed switch
        {
            < 0.5 => 1,
            < 1.0 => 2,
            < 1.5 => 3,
            < 2.5 => 4,
            _ => 5
        };

        Color activeBarColor = GetSpeedColor(displaySpeed);

        for (int i = 0; i < 5; i++)
        {
            Color barColor = i < activeBars ? activeBarColor : InactiveBarColor;
            if (speedBarBrushes[i].Color != barColor)
            {
                speedBarBrushes[i].Color = barColor;
            }
        }

        // Auto-hide
        hideTimer -= deltaTime;
        if (hideTimer <= 0)
        {
            Hide();
        }

        UpdatePosition();
    }

    private static Color GetSpeedColor(double speed)
    {
        if (speed < 0.8)
        {
            return Color.FromRgb(100, 150, 255); // Blue for slow
        }

        if (speed < 1.2)
        {
            return Color.FromRgb(100, 255, 150); // Green for normal
        }

        if (speed < 2.0)
        {
            return Color.FromRgb(255, 200, 100); // Yellow for fast
        }

        return Color.FromRgb(255, 100, 100); // Red for very fast
    }

    private void Show()
    {
        if (container == null)
        {
            return;
        }

        var animation = new DoubleAnimation
        {
            To = 1.0,
            Duration = TimeSpan.FromMilliseconds(200),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        container.BeginAnimation(UIElement.OpacityProperty, animation);
    }

    private void Hide()
    {
        if (container == null)
        {
            return;
        }

        var animation = new DoubleAnimation
        {
            To = 0.0,
            Duration = TimeSpan.FromMilliseconds(500),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
        };
        container.BeginAnimation(UIElement.OpacityProperty, animation);
    }

    /// <summary>
    /// Forces the indicator to show.
    /// </summary>
    public void ForceShow()
    {
        hideTimer = HideDelay;
        Show();
    }

    private static SolidColorBrush CreateFrozenBrush(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }
}
