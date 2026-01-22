namespace Terrarium.Desktop.Rendering;

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Terrarium.Logic.Simulation.Achievements;

/// <summary>
/// Manages ecosystem achievements and milestones.
/// </summary>
public class AchievementSystem
{
    private readonly Canvas canvas;
    private readonly HashSet<string> unlockedAchievements;
    private readonly Queue<Achievement> pendingAchievements;
    private Border? achievementBanner;
    private double displayTimer;
    private bool isDisplaying;
    private bool isVisible = true;

    private static readonly SolidColorBrush GoldBrush = CreateFrozenBrush(Color.FromRgb(255, 215, 0));
    private static readonly SolidColorBrush LightTextBrush = CreateFrozenBrush(Color.FromRgb(200, 200, 200));
    private static readonly LinearGradientBrush BannerBackgroundBrush = Freeze(new LinearGradientBrush(
        Color.FromArgb(240, 40, 40, 55),
        Color.FromArgb(240, 30, 30, 42),
        90));

    private static readonly System.Windows.Media.Effects.DropShadowEffect BannerShadowEffect = Freeze(new System.Windows.Media.Effects.DropShadowEffect
    {
        Color = Color.FromRgb(255, 215, 0),
        Direction = 0,
        ShadowDepth = 0,
        BlurRadius = 20,
        Opacity = 0.4
    });

    private const double DisplayDuration = 4.0;
    private const double BannerWidth = 320;
    private const double BannerHeight = 80;

    public event Action<string, string>? OnAchievementUnlocked;

    public bool IsVisible
    {
        get => isVisible;
        set
        {
            isVisible = value;
            if (achievementBanner != null)
            {
                achievementBanner.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }

    public AchievementSystem(Canvas canvas)
    {
        this.canvas = canvas;
        unlockedAchievements = new HashSet<string>();
        pendingAchievements = new Queue<Achievement>();
        displayTimer = 0;
        isDisplaying = false;
    }

    /// <summary>
    /// Checks and unlocks achievements based on current stats.
    /// </summary>
    public void CheckAchievements(int totalBirths, int totalDeaths, int peakPopulation,
                                   int currentPlants, int currentHerbivores, int currentCarnivores,
                                   double simulationTime)
    {
        foreach (var achievement in AchievementEvaluator.Evaluate(
                     totalBirths,
                     totalDeaths,
                     peakPopulation,
                     currentPlants,
                     currentHerbivores,
                     currentCarnivores,
                     simulationTime))
        {
            if (unlockedAchievements.Add(achievement.Id))
            {
                pendingAchievements.Enqueue(new Achievement
                {
                    Id = achievement.Id,
                    Title = achievement.Title,
                    Description = achievement.Description
                });
                OnAchievementUnlocked?.Invoke(achievement.Title, achievement.Description);
            }
        }
    }

    /// <summary>
    /// Updates achievement display.
    /// </summary>
    public void Update(double deltaTime)
    {
        if (!isVisible)
        {
            return;
        }

        if (isDisplaying)
        {
            displayTimer -= deltaTime;
            if (displayTimer <= 0)
            {
                HideBanner();
                isDisplaying = false;
            }
        }

        if (!isDisplaying && pendingAchievements.Count > 0)
        {
            var achievement = pendingAchievements.Dequeue();
            ShowBanner(achievement);
            displayTimer = DisplayDuration;
            isDisplaying = true;
        }
    }

    private void ShowBanner(Achievement achievement)
    {
        if (achievementBanner != null)
        {
            canvas.Children.Remove(achievementBanner);
        }

        var content = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center
        };

        // Trophy icon
        var icon = new TextBlock
        {
            Text = "🏆",
            FontSize = 32,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(15, 0, 15, 0)
        };

        // Text content
        var textPanel = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center
        };

        var titleText = new TextBlock
        {
            Text = achievement.Title,
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            Foreground = GoldBrush // Gold
        };

        var descText = new TextBlock
        {
            Text = achievement.Description,
            FontSize = 11,
            Foreground = LightTextBrush,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 220
        };

        textPanel.Children.Add(titleText);
        textPanel.Children.Add(descText);

        content.Children.Add(icon);
        content.Children.Add(textPanel);

        achievementBanner = new Border
        {
            Width = BannerWidth,
            Height = BannerHeight,
            Background = BannerBackgroundBrush,
            BorderBrush = GoldBrush,
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(12),
            Child = content,
            Effect = BannerShadowEffect
        };

        double x = (canvas.ActualWidth - BannerWidth) / 2;
        Canvas.SetLeft(achievementBanner, x);
        Canvas.SetTop(achievementBanner, -BannerHeight);
        Canvas.SetZIndex(achievementBanner, 950);

        canvas.Children.Add(achievementBanner);

        var slideIn = new DoubleAnimation
        {
            From = -BannerHeight,
            To = 80,
            Duration = TimeSpan.FromMilliseconds(400),
            EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.3 }
        };
        achievementBanner.BeginAnimation(Canvas.TopProperty, slideIn);
    }

    private void HideBanner()
    {
        if (achievementBanner == null)
        {
            return;
        }

        var slideOut = new DoubleAnimation
        {
            To = -BannerHeight - 20,
            Duration = TimeSpan.FromMilliseconds(300),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
        };

        slideOut.Completed += (s, e) =>
        {
            if (achievementBanner != null)
            {
                canvas.Children.Remove(achievementBanner);
                achievementBanner = null;
            }
        };

        achievementBanner.BeginAnimation(Canvas.TopProperty, slideOut);
    }

    /// <summary>
    /// Gets the count of unlocked achievements.
    /// </summary>
    public int UnlockedCount => unlockedAchievements.Count;

    /// <summary>
    /// Gets total available achievements.
    /// </summary>
    public int TotalAchievements => AchievementEvaluator.TotalAchievements;

    private static SolidColorBrush CreateFrozenBrush(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }

    private static T Freeze<T>(T freezable)
        where T : Freezable
    {
        if (freezable.CanFreeze)
        {
            freezable.Freeze();
        }

        return freezable;
    }
}

internal class Achievement
{
    public string Id { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}
