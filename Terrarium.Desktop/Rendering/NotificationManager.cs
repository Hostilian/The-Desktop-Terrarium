using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Terrarium.Desktop.Rendering
{
    /// <summary>
    /// Types of notifications.
    /// </summary>
    public enum NotificationType
    {
        Info,
        Birth,
        Death,
        Warning,
        Milestone,
        Achievement,
        Success
    }

    /// <summary>
    /// Manages toast-style notifications for ecosystem events.
    /// </summary>
    public class NotificationManager
    {
        private readonly Canvas _canvas;
        private readonly Queue<NotificationItem> _pendingNotifications;
        private readonly List<NotificationItem> _activeNotifications;

        private static readonly Brush NotificationBorderBrush = CreateFrozenBrush(Color.FromArgb(50, 255, 255, 255));

        private static readonly Brush BirthBackgroundBrush = CreateFrozenBrush(Color.FromArgb(220, 46, 204, 113));
        private static readonly Brush DeathBackgroundBrush = CreateFrozenBrush(Color.FromArgb(220, 128, 128, 128));
        private static readonly Brush WarningBackgroundBrush = CreateFrozenBrush(Color.FromArgb(220, 230, 126, 34));
        private static readonly Brush MilestoneBackgroundBrush = CreateFrozenBrush(Color.FromArgb(220, 155, 89, 182));
        private static readonly Brush AchievementBackgroundBrush = CreateFrozenBrush(Color.FromArgb(220, 241, 196, 15));
        private static readonly Brush InfoBackgroundBrush = CreateFrozenBrush(Color.FromArgb(220, 52, 152, 219));
        private static readonly Brush DefaultBackgroundBrush = CreateFrozenBrush(Color.FromArgb(220, 52, 73, 94));

        private const int MaxActiveNotifications = 3;
        private const double NotificationDuration = 3.0;
        private const double NotificationWidth = 250;
        private const double NotificationHeight = 50;
        private const double NotificationMargin = 10;
        private const double SlideInDuration = 0.3;
        private const double FadeOutDuration = 0.5;

        /// <summary>
        /// Gets or sets whether notifications are enabled.
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        public NotificationManager(Canvas canvas)
        {
            _canvas = canvas;
            _pendingNotifications = new Queue<NotificationItem>();
            _activeNotifications = new List<NotificationItem>();
        }

        /// <summary>
        /// Updates notification timers and positions.
        /// </summary>
        public void Update(double deltaTime)
        {
            if (!IsEnabled)
                return;

            // Update active notifications
            for (int i = _activeNotifications.Count - 1; i >= 0; i--)
            {
                var notification = _activeNotifications[i];
                notification.TimeRemaining -= deltaTime;

                if (notification.TimeRemaining <= 0)
                {
                    RemoveNotification(notification);
                    _activeNotifications.RemoveAt(i);
                }
                else if (notification.TimeRemaining <= FadeOutDuration)
                {
                    // Start fading out
                    if (notification.Visual != null)
                        notification.Visual.Opacity = notification.TimeRemaining / FadeOutDuration;
                }
            }

            // Show pending notifications
            while (_activeNotifications.Count < MaxActiveNotifications && _pendingNotifications.Count > 0)
            {
                var notification = _pendingNotifications.Dequeue();
                ShowNotification(notification);
            }
        }

        /// <summary>
        /// Clears all notifications.
        /// </summary>
        public void ClearAll()
        {
            foreach (var notification in _activeNotifications)
            {
                if (notification.Visual != null)
                    _canvas.Children.Remove(notification.Visual);
            }
            _activeNotifications.Clear();
            _pendingNotifications.Clear();
        }

        /// <summary>
        /// Shows a generic notification.
        /// </summary>
        public void Notify(string message, NotificationType type)
        {
            if (!IsEnabled)
                return;
            QueueNotification(message, type);
        }

        /// <summary>
        /// Queues a birth notification.
        /// </summary>
        public void NotifyBirth(string creatureType)
        {
            if (!IsEnabled)
                return;
            QueueNotification($"🎉 New {creatureType} born!", NotificationType.Birth);
        }

        /// <summary>
        /// Queues a death notification.
        /// </summary>
        public void NotifyDeath(string creatureType, string cause)
        {
            if (!IsEnabled)
                return;
            QueueNotification($"💀 {creatureType} died ({cause})", NotificationType.Death);
        }

        /// <summary>
        /// Queues an ecosystem warning.
        /// </summary>
        public void NotifyWarning(string message)
        {
            if (!IsEnabled)
                return;
            QueueNotification($"⚠️ {message}", NotificationType.Warning);
        }

        /// <summary>
        /// Queues an ecosystem milestone.
        /// </summary>
        public void NotifyMilestone(string name, int value)
        {
            if (!IsEnabled)
                return;
            QueueNotification($"🏆 {name}: {value}", NotificationType.Milestone);
        }

        /// <summary>
        /// Queues a weather change notification.
        /// </summary>
        public void NotifyWeatherChange(bool isStormy)
        {
            if (!IsEnabled)
                return;
            string msg = isStormy ? "⛈️ Storm approaching!" : "☀️ Weather clearing up";
            QueueNotification(msg, isStormy ? NotificationType.Warning : NotificationType.Info);
        }

        /// <summary>
        /// Queues a day phase change notification.
        /// </summary>
        public void NotifyDayPhaseChange(string phase)
        {
            if (!IsEnabled)
                return;
            string icon = phase.ToLower() switch
            {
                "dawn" => "🌅",
                "day" => "☀️",
                "dusk" => "🌇",
                "night" => "🌙",
                _ => "🌍"
            };
            QueueNotification($"{icon} {phase} has arrived", NotificationType.Info);
        }

        private void QueueNotification(string message, NotificationType type)
        {
            var notification = new NotificationItem
            {
                Message = message,
                Type = type,
                TimeRemaining = NotificationDuration
            };

            _pendingNotifications.Enqueue(notification);
        }

        private void ShowNotification(NotificationItem notification)
        {
            // Create visual
            var border = new Border
            {
                Width = NotificationWidth,
                Height = NotificationHeight,
                CornerRadius = new CornerRadius(8),
                Background = GetNotificationBackground(notification.Type),
                BorderBrush = NotificationBorderBrush,
                BorderThickness = new Thickness(1),
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.Black,
                    Direction = 270,
                    ShadowDepth = 3,
                    BlurRadius = 8,
                    Opacity = 0.5
                }
            };

            var textBlock = new TextBlock
            {
                Text = notification.Message,
                Foreground = Brushes.White,
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 12,
                FontWeight = FontWeights.Medium,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(10)
            };

            border.Child = textBlock;
            notification.Visual = border;

            // Position (slide in from right)
            double canvasWidth = _canvas.ActualWidth > 0 ? _canvas.ActualWidth : 1920;
            double targetX = canvasWidth - NotificationWidth - NotificationMargin;
            double targetY = NotificationMargin + (_activeNotifications.Count * (NotificationHeight + NotificationMargin));

            Canvas.SetLeft(border, canvasWidth); // Start off-screen
            Canvas.SetTop(border, targetY);
            Canvas.SetZIndex(border, 1000); // On top of everything

            _canvas.Children.Add(border);
            _activeNotifications.Add(notification);

            // Animate slide in
            var animation = new DoubleAnimation
            {
                From = canvasWidth,
                To = targetX,
                Duration = TimeSpan.FromSeconds(SlideInDuration),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            border.BeginAnimation(Canvas.LeftProperty, animation);
        }

        private void RemoveNotification(NotificationItem notification)
        {
            if (notification.Visual != null)
            {
                _canvas.Children.Remove(notification.Visual);
            }

            // Reposition remaining notifications
            for (int i = 0; i < _activeNotifications.Count; i++)
            {
                if (_activeNotifications[i] != notification && _activeNotifications[i].Visual != null)
                {
                    double targetY = NotificationMargin + (i * (NotificationHeight + NotificationMargin));
                    var animation = new DoubleAnimation
                    {
                        To = targetY,
                        Duration = TimeSpan.FromSeconds(0.2),
                        EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                    };
                    _activeNotifications[i].Visual?.BeginAnimation(Canvas.TopProperty, animation);
                }
            }
        }

        private Brush GetNotificationBackground(NotificationType type)
        {
            return type switch
            {
                NotificationType.Birth => BirthBackgroundBrush,
                NotificationType.Death => DeathBackgroundBrush,
                NotificationType.Warning => WarningBackgroundBrush,
                NotificationType.Milestone => MilestoneBackgroundBrush,
                NotificationType.Achievement => AchievementBackgroundBrush,
                NotificationType.Info => InfoBackgroundBrush,
                _ => DefaultBackgroundBrush
            };
        }

        private static SolidColorBrush CreateFrozenBrush(Color color)
        {
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }
    }

    internal class NotificationItem
    {
        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public double TimeRemaining { get; set; }
        public Border? Visual { get; set; }
    }
}
