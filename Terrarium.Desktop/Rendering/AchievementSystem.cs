using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Terrarium.Desktop.Rendering
{
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

        private const double DisplayDuration = 4.0;
        private const double BannerWidth = 320;
        private const double BannerHeight = 80;

        public event Action<string, string>? OnAchievementUnlocked;

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
            // Population achievements
            TryUnlock("first_birth", "🎉 First Birth", "Welcome the first creature to the world!", totalBirths >= 1);
            TryUnlock("population_10", "👨‍👩‍👧‍👦 Growing Family", "Reach 10 total creatures", peakPopulation >= 10);
            TryUnlock("population_25", "🏘️ Village", "Reach 25 total creatures", peakPopulation >= 25);
            TryUnlock("population_50", "🌆 City", "Reach 50 total creatures", peakPopulation >= 50);

            // Birth achievements
            TryUnlock("births_10", "👶 Nursery", "Witness 10 births", totalBirths >= 10);
            TryUnlock("births_50", "🏥 Maternity Ward", "Witness 50 births", totalBirths >= 50);
            TryUnlock("births_100", "🎊 Baby Boom", "Witness 100 births", totalBirths >= 100);

            // Survival achievements
            TryUnlock("survivor", "💪 Survivor", "Have at least one of each species alive", 
                      currentPlants > 0 && currentHerbivores > 0 && currentCarnivores > 0);

            // Time achievements
            TryUnlock("time_5min", "⏰ Getting Started", "Run simulation for 5 minutes", simulationTime >= 300);
            TryUnlock("time_30min", "🕐 Dedicated Observer", "Run simulation for 30 minutes", simulationTime >= 1800);
            TryUnlock("time_1hour", "🏆 Ecosystem Master", "Run simulation for 1 hour", simulationTime >= 3600);

            // Plant achievements
            TryUnlock("plants_20", "🌳 Forest", "Grow 20 plants", currentPlants >= 20);
            TryUnlock("plants_40", "🌲 Jungle", "Grow 40 plants", currentPlants >= 40);

            // Balance achievements
            int totalCreatures = currentHerbivores + currentCarnivores;
            if (totalCreatures >= 10)
            {
                double herbivoreRatio = (double)currentHerbivores / totalCreatures;
                TryUnlock("balance", "⚖️ Perfect Balance", "Maintain 60-80% herbivore ratio with 10+ creatures",
                          herbivoreRatio >= 0.6 && herbivoreRatio <= 0.8);
            }

            // Predator achievements
            TryUnlock("apex_predator", "🦁 Apex Predators", "Have 5+ carnivores alive", currentCarnivores >= 5);
        }

        private void TryUnlock(string id, string title, string description, bool condition)
        {
            if (condition && !_unlockedAchievements.Contains(id))
            {
                _unlockedAchievements.Add(id);
                _pendingAchievements.Enqueue(new Achievement { Id = id, Title = title, Description = description });
                OnAchievementUnlocked?.Invoke(title, description);
            }
        }

        /// <summary>
        /// Updates achievement display.
        /// </summary>
        public void Update(double deltaTime)
        {
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
                Foreground = new SolidColorBrush(Color.FromRgb(255, 215, 0)) // Gold
            };

            var descText = new TextBlock
            {
                Text = achievement.Description,
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
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
                Background = new LinearGradientBrush(
                    Color.FromArgb(240, 40, 40, 55),
                    Color.FromArgb(240, 30, 30, 42),
                    90),
                BorderBrush = new SolidColorBrush(Color.FromRgb(255, 215, 0)),
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(12),
                Child = content,
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Color.FromRgb(255, 215, 0),
                    Direction = 0,
                    ShadowDepth = 0,
                    BlurRadius = 20,
                    Opacity = 0.4
                }
            };

            // Position at top center
            double x = (_canvas.ActualWidth - BannerWidth) / 2;
            Canvas.SetLeft(_achievementBanner, x);
            Canvas.SetTop(_achievementBanner, -BannerHeight);
            Canvas.SetZIndex(_achievementBanner, 950);

            _canvas.Children.Add(_achievementBanner);

            // Slide in animation
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
            if (_achievementBanner == null) return;

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
        public int TotalAchievements => 14;
    }

    internal class Achievement
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
