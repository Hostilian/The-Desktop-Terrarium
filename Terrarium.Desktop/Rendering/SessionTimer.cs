namespace Terrarium.Desktop.Rendering;

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

/// <summary>
/// Displays session statistics including runtime and day count.
/// </summary>
public class SessionTimer
{
    private readonly Canvas canvas;
    private Border? container;
    private TextBlock? timeText;
    private TextBlock? dayText;
    private TextBlock? generationText;

    private static readonly Brush ContainerBackgroundBrush = CreateFrozenBrush(Color.FromArgb(180, 20, 30, 40));
    private static readonly Brush ContainerBorderBrush = CreateFrozenBrush(Color.FromRgb(70, 70, 70));
    private static readonly Brush DayTextBrush = CreateFrozenBrush(Color.FromRgb(200, 200, 200));
    private static readonly Brush GenerationBaseBrush = CreateFrozenBrush(Color.FromRgb(180, 220, 255));

    private static readonly SolidColorBrush TimeGreenBrush = CreateFrozenBrush(Color.FromRgb(100, 255, 150));
    private static readonly SolidColorBrush MilestoneGoldBrush = CreateFrozenBrush(Color.FromRgb(255, 215, 0));
    private static readonly SolidColorBrush MilestoneOrangeBrush = CreateFrozenBrush(Color.FromRgb(255, 180, 100));
    private static readonly SolidColorBrush MilestoneGreenBrush = CreateFrozenBrush(Color.FromRgb(100, 255, 180));

    private double sessionTime;
    private int dayCount;
    private int generationCount;

    private int lastDisplayedSessionSecond = -1;

    private const double DayDuration = 120.0; // 2 minutes per in-game day

    public bool IsEnabled { get; set; } = true;

    public bool IsVisible { get; set; } = true;

    public SessionTimer(Canvas canvas)
    {
        this.canvas = canvas;
        sessionTime = 0;
        dayCount = 1;
        generationCount = 1;
        CreateUI();
    }

    private void CreateUI()
    {
        container = new Border
        {
            Background = ContainerBackgroundBrush,
            BorderBrush = ContainerBorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(8, 4, 8, 4)
        };

        var stack = new StackPanel
        {
            Orientation = Orientation.Horizontal
        };

        // Clock icon and time
        var clockIcon = new TextBlock
        {
            Text = "⏱️",
            FontSize = 11,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 4, 0)
        };
        stack.Children.Add(clockIcon);

        timeText = new TextBlock
        {
            Text = "00:00",
            FontSize = 11,
            FontWeight = FontWeights.SemiBold,
            Foreground = Brushes.White,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 10, 0)
        };
        stack.Children.Add(timeText);

        // Day counter
        var dayIcon = new TextBlock
        {
            Text = "📅",
            FontSize = 11,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 4, 0)
        };
        stack.Children.Add(dayIcon);

        dayText = new TextBlock
        {
            Text = "Day 1",
            FontSize = 11,
            Foreground = DayTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 10, 0)
        };
        stack.Children.Add(dayText);

        // Generation counter
        var genIcon = new TextBlock
        {
            Text = "🧬",
            FontSize = 11,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 4, 0)
        };
        stack.Children.Add(genIcon);

        generationText = new TextBlock
        {
            Text = "Gen 1",
            FontSize = 11,
            Foreground = GenerationBaseBrush,
            VerticalAlignment = VerticalAlignment.Center
        };
        stack.Children.Add(generationText);

        container.Child = stack;

        Canvas.SetZIndex(container, 700);
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
        Canvas.SetLeft(container, (canvasWidth - 200) / 2);
        Canvas.SetTop(container, 15);
    }

    /// <summary>
    /// Updates the session timer display.
    /// </summary>
    public void Update(double deltaTime)
    {
        if (!IsEnabled || container == null)
        {
            if (container != null)
            {
                container.Visibility = Visibility.Collapsed;
            }

            return;
        }

        container.Visibility = Visibility.Visible;
        sessionTime += deltaTime;

        int newDayCount = (int)(sessionTime / DayDuration) + 1;
        if (newDayCount != dayCount)
        {
            dayCount = newDayCount;
            if (dayText != null)
            {
                dayText.Text = $"Day {dayCount}";
            }
        }

        // Update time display (MM:SS format)
        if (timeText != null)
        {
            int totalSeconds = (int)sessionTime;
            if (totalSeconds != lastDisplayedSessionSecond)
            {
                lastDisplayedSessionSecond = totalSeconds;
                int minutes = totalSeconds / 60;
                int seconds = totalSeconds % 60;
                timeText.Text = $"{minutes:D2}:{seconds:D2}";
            }

            Brush desiredBrush = Brushes.White;
            if (sessionTime > 3600) // Over 1 hour
            {
                desiredBrush = MilestoneGoldBrush;
            }
            else if (sessionTime > 1800) // Over 30 minutes
            {
                desiredBrush = TimeGreenBrush;
            }

            if (!ReferenceEquals(timeText.Foreground, desiredBrush))
            {
                timeText.Foreground = desiredBrush;
            }
        }

        UpdatePosition();
    }

    /// <summary>
    /// Increments the generation counter (called when creatures reproduce).
    /// </summary>
    public void IncrementGeneration()
    {
        generationCount++;
        if (generationText != null)
        {
            generationText.Text = $"Gen {generationCount}";

            // Milestone colors
            Brush? desiredBrush = null;
            if (generationCount >= 100)
            {
                desiredBrush = MilestoneGoldBrush;
            }
            else if (generationCount >= 50)
            {
                desiredBrush = MilestoneOrangeBrush;
            }
            else if (generationCount >= 20)
            {
                desiredBrush = MilestoneGreenBrush;
            }

            if (desiredBrush != null && !ReferenceEquals(generationText.Foreground, desiredBrush))
            {
                generationText.Foreground = desiredBrush;
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
    /// Gets the current session time in seconds.
    /// </summary>
    /// <returns></returns>
    public double GetSessionTime() => sessionTime;

    /// <summary>
    /// Gets the current day count.
    /// </summary>
    /// <returns></returns>
    public int GetDayCount() => dayCount;

    /// <summary>
    /// Sets the session time (for loading saved games).
    /// </summary>
    public void SetSessionTime(double time)
    {
        sessionTime = time;
        dayCount = (int)(time / DayDuration) + 1;
    }

    /// <summary>
    /// Sets the generation count (for loading saved games).
    /// </summary>
    public void SetGenerationCount(int count)
    {
        generationCount = count;
        if (generationText != null)
        {
            generationText.Text = $"Gen {generationCount}";
        }
    }

    /// <summary>
    /// Resets the timer for a new session.
    /// </summary>
    public void Reset()
    {
        sessionTime = 0;
        dayCount = 1;
        generationCount = 1;
        if (timeText != null)
        {
            timeText.Text = "00:00";
        }

        if (dayText != null)
        {
            dayText.Text = "Day 1";
        }

        if (generationText != null)
        {
            generationText.Text = "Gen 1";
        }
    }
}
