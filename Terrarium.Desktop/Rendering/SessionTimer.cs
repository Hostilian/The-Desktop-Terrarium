using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Terrarium.Desktop.Rendering
{
    /// <summary>
    /// Displays session statistics including runtime and day count.
    /// </summary>
    public class SessionTimer
    {
        private readonly Canvas _canvas;
        private Border? _container;
        private TextBlock? _timeText;
        private TextBlock? _dayText;
        private TextBlock? _generationText;

        private static readonly Brush ContainerBackgroundBrush = CreateFrozenBrush(Color.FromArgb(180, 20, 30, 40));
        private static readonly Brush ContainerBorderBrush = CreateFrozenBrush(Color.FromRgb(70, 70, 70));
        private static readonly Brush DayTextBrush = CreateFrozenBrush(Color.FromRgb(200, 200, 200));
        private static readonly Brush GenerationBaseBrush = CreateFrozenBrush(Color.FromRgb(180, 220, 255));

        private static readonly SolidColorBrush TimeGreenBrush = CreateFrozenBrush(Color.FromRgb(100, 255, 150));
        private static readonly SolidColorBrush MilestoneGoldBrush = CreateFrozenBrush(Color.FromRgb(255, 215, 0));
        private static readonly SolidColorBrush MilestoneOrangeBrush = CreateFrozenBrush(Color.FromRgb(255, 180, 100));
        private static readonly SolidColorBrush MilestoneGreenBrush = CreateFrozenBrush(Color.FromRgb(100, 255, 180));

        private double _sessionTime;
        private int _dayCount;
        private int _generationCount;

        private const double DayDuration = 120.0; // 2 minutes per in-game day

        public bool IsEnabled { get; set; } = true;

        public SessionTimer(Canvas canvas)
        {
            _canvas = canvas;
            _sessionTime = 0;
            _dayCount = 1;
            _generationCount = 1;
            CreateUI();
        }

        private void CreateUI()
        {
            _container = new Border
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

            _timeText = new TextBlock
            {
                Text = "00:00",
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0)
            };
            stack.Children.Add(_timeText);

            // Day counter
            var dayIcon = new TextBlock
            {
                Text = "📅",
                FontSize = 11,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 4, 0)
            };
            stack.Children.Add(dayIcon);

            _dayText = new TextBlock
            {
                Text = "Day 1",
                FontSize = 11,
                Foreground = DayTextBrush,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0)
            };
            stack.Children.Add(_dayText);

            // Generation counter
            var genIcon = new TextBlock
            {
                Text = "🧬",
                FontSize = 11,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 4, 0)
            };
            stack.Children.Add(genIcon);

            _generationText = new TextBlock
            {
                Text = "Gen 1",
                FontSize = 11,
                Foreground = GenerationBaseBrush,
                VerticalAlignment = VerticalAlignment.Center
            };
            stack.Children.Add(_generationText);

            _container.Child = stack;

            // Position at top center
            Canvas.SetZIndex(_container, 700);
            _canvas.Children.Add(_container);

            UpdatePosition();
        }

        private void UpdatePosition()
        {
            if (_container == null)
                return;

            double canvasWidth = _canvas.ActualWidth > 0 ? _canvas.ActualWidth : 800;
            Canvas.SetLeft(_container, (canvasWidth - 200) / 2);
            Canvas.SetTop(_container, 15);
        }

        /// <summary>
        /// Updates the session timer display.
        /// </summary>
        public void Update(double deltaTime)
        {
            if (!IsEnabled || _container == null)
            {
                if (_container != null)
                    _container.Visibility = Visibility.Collapsed;
                return;
            }

            _container.Visibility = Visibility.Visible;
            _sessionTime += deltaTime;

            // Calculate days
            int newDayCount = (int)(_sessionTime / DayDuration) + 1;
            if (newDayCount != _dayCount)
            {
                _dayCount = newDayCount;
                if (_dayText != null)
                {
                    _dayText.Text = $"Day {_dayCount}";
                }
            }

            // Update time display (MM:SS format)
            if (_timeText != null)
            {
                int minutes = (int)(_sessionTime / 60);
                int seconds = (int)(_sessionTime % 60);
                _timeText.Text = $"{minutes:D2}:{seconds:D2}";

                // Color based on session length
                Brush desiredBrush = Brushes.White;
                if (_sessionTime > 3600) // Over 1 hour
                {
                    desiredBrush = MilestoneGoldBrush;
                }
                else if (_sessionTime > 1800) // Over 30 minutes
                {
                    desiredBrush = TimeGreenBrush;
                }

                if (!ReferenceEquals(_timeText.Foreground, desiredBrush))
                {
                    _timeText.Foreground = desiredBrush;
                }
            }

            UpdatePosition();
        }

        /// <summary>
        /// Increments the generation counter (called when creatures reproduce).
        /// </summary>
        public void IncrementGeneration()
        {
            _generationCount++;
            if (_generationText != null)
            {
                _generationText.Text = $"Gen {_generationCount}";

                // Milestone colors
                Brush? desiredBrush = null;
                if (_generationCount >= 100)
                {
                    desiredBrush = MilestoneGoldBrush;
                }
                else if (_generationCount >= 50)
                {
                    desiredBrush = MilestoneOrangeBrush;
                }
                else if (_generationCount >= 20)
                {
                    desiredBrush = MilestoneGreenBrush;
                }

                if (desiredBrush != null && !ReferenceEquals(_generationText.Foreground, desiredBrush))
                {
                    _generationText.Foreground = desiredBrush;
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
        public double GetSessionTime() => _sessionTime;

        /// <summary>
        /// Gets the current day count.
        /// </summary>
        public int GetDayCount() => _dayCount;

        /// <summary>
        /// Sets the session time (for loading saved games).
        /// </summary>
        public void SetSessionTime(double time)
        {
            _sessionTime = time;
            _dayCount = (int)(time / DayDuration) + 1;
        }

        /// <summary>
        /// Sets the generation count (for loading saved games).
        /// </summary>
        public void SetGenerationCount(int count)
        {
            _generationCount = count;
            if (_generationText != null)
            {
                _generationText.Text = $"Gen {_generationCount}";
            }
        }

        /// <summary>
        /// Resets the timer for a new session.
        /// </summary>
        public void Reset()
        {
            _sessionTime = 0;
            _dayCount = 1;
            _generationCount = 1;
            if (_timeText != null)
                _timeText.Text = "00:00";
            if (_dayText != null)
                _dayText.Text = "Day 1";
            if (_generationText != null)
                _generationText.Text = "Gen 1";
        }
    }
}
