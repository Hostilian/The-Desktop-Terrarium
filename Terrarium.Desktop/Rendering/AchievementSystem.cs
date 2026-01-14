using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Terrarium.Logic.Simulation.Achievements;

namespace Terrarium.Desktop.Rendering;

/// <summary>
/// Manages ecosystem achievements and milestones.
/// </summary>
public class AchievementSystem
{
    private readonly Canvas _canvas;
    private readonly HashSet<string> _unlockedAchievements;
    private readonly Queue<Achievement> _pendingAchievements;
    private Border? _achievementBanner;
    private double _displayTimer;
    private bool _isDisplaying;
    private bool _isVisible = true;

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
        get => _isVisible;
        set
        {
            _isVisible = value;
            if (_achievementBanner != null)
            {
                _achievementBanner.Visibility = _isVisible ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }

    public AchievementSystem(Canvas canvas)
    {
        _canvas = canvas;
        _unlockedAchievements = new HashSet<string>();
        _pendingAchievements = new Queue<Achievement>();
        _displayTimer = 0;
        _isDisplaying = false;
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
            if (_unlockedAchievements.Add(achievement.Id))
            {
                _pendingAchievements.Enqueue(new Achievement
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
        if (!_isVisible)
        {
            return;
        }

        if (_isDisplaying)
        {
            _displayTimer -= deltaTime;
            if (_displayTimer <= 0)
            {
                HideBanner();
                _isDisplaying = false;
            }
        }

        if (!_isDisplaying && _pendingAchievements.Count > 0)
        {
            var achievement = _pendingAchievements.Dequeue();
            ShowBanner(achievement);
            _displayTimer = DisplayDuration;
            _isDisplaying = true;
        }
    }

    private void ShowBanner(Achievement achievement)
    {
        if (_achievementBanner != null)
        {
            _canvas.Children.Remove(_achievementBanner);
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

        _achievementBanner = new Border
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

        double x = (_canvas.ActualWidth - BannerWidth) / 2;
        Canvas.SetLeft(_achievementBanner, x);
        Canvas.SetTop(_achievementBanner, -BannerHeight);
        Canvas.SetZIndex(_achievementBanner, 950);

        _canvas.Children.Add(_achievementBanner);

        var slideIn = new DoubleAnimation
        {
            From = -BannerHeight,
            To = 80,
            Duration = TimeSpan.FromMilliseconds(400),
            EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.3 }
        };
        _achievementBanner.BeginAnimation(Canvas.TopProperty, slideIn);
    }

    private void HideBanner()
    {
        if (_achievementBanner == null)
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
            if (_achievementBanner != null)
            {
                _canvas.Children.Remove(_achievementBanner);
                _achievementBanner = null;
            }
        };

        _achievementBanner.BeginAnimation(Canvas.TopProperty, slideOut);
    }

    /// <summary>
    /// Gets the count of unlocked achievements.
    /// </summary>
    public int UnlockedCount => _unlockedAchievements.Count;

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

    private static T Freeze<T>(T freezable) where T : Freezable
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
