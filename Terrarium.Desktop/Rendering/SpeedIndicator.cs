using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Terrarium.Desktop.Rendering
{
    /// <summary>
    /// Shows the current simulation speed with visual feedback.
    /// </summary>
    public class SpeedIndicator
    {
        private readonly Canvas _canvas;
        private Border? _container;
        private TextBlock? _speedText;
        private Rectangle[] _speedBars;
        private SolidColorBrush[] _speedBarBrushes;
        private double _currentSpeed;
        private double _displaySpeed;
        private double _hideTimer;

        private readonly SolidColorBrush _speedTextSlowBrush = new(Color.FromRgb(100, 150, 255));
        private readonly SolidColorBrush _speedTextFastBrush = new(Color.FromRgb(255, 150, 100));
        private readonly SolidColorBrush _speedTextNormalBrush = new(Colors.White);

        private static readonly Color InactiveBarColor = Color.FromRgb(60, 60, 60);

        private const double HideDelay = 3.0;
        private const double AnimationSpeed = 8.0;

        public bool IsEnabled { get; set; } = true;

        public SpeedIndicator(Canvas canvas)
        {
            _canvas = canvas;
            _speedBars = new Rectangle[5];
            _speedBarBrushes = new SolidColorBrush[5];
            _currentSpeed = 1.0;
            _displaySpeed = 1.0;
            CreateUI();
        }

        private void CreateUI()
        {
            _container = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(200, 20, 30, 40)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(100, 100, 100)),
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
                _speedBarBrushes[i] = brush;
                _speedBars[i] = new Rectangle
                {
                    Width = 4,
                    Height = 8 + i * 3,
                    Fill = brush,
                    Margin = new Thickness(2, 0, 2, 0),
                    RadiusX = 1,
                    RadiusY = 1,
                    VerticalAlignment = VerticalAlignment.Bottom
                };
                barsPanel.Children.Add(_speedBars[i]);
            }
            stack.Children.Add(barsPanel);

            // Speed text
            _speedText = new TextBlock
            {
                Text = "1.0x",
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center
            };
            stack.Children.Add(_speedText);

            _container.Child = stack;

            // Position at bottom center
            Canvas.SetZIndex(_container, 750);
            _canvas.Children.Add(_container);

            UpdatePosition();
        }

        private void UpdatePosition()
        {
            if (_container == null)
                return;

            double canvasWidth = _canvas.ActualWidth > 0 ? _canvas.ActualWidth : 800;
            Canvas.SetLeft(_container, (canvasWidth - 120) / 2);
            Canvas.SetBottom(_container, 20);
            Canvas.SetTop(_container, double.NaN);
        }

        /// <summary>
        /// Sets the current simulation speed.
        /// </summary>
        public void SetSpeed(double speed)
        {
            if (Math.Abs(_currentSpeed - speed) > 0.01)
            {
                _currentSpeed = speed;
                _hideTimer = HideDelay;
                Show();
            }
        }

        /// <summary>
        /// Updates the indicator visuals.
        /// </summary>
        public void Update(double deltaTime)
        {
            if (!IsEnabled || _container == null)
                return;

            // Animate display speed
            _displaySpeed += (_currentSpeed - _displaySpeed) * AnimationSpeed * deltaTime;
            _displaySpeed = Math.Clamp(_displaySpeed, 0.1, 5.0);

            // Update text
            if (_speedText != null)
            {
                _speedText.Text = $"{_displaySpeed:F1}x";

                // Color based on speed
                _speedText.Foreground = _displaySpeed switch
                {
                    < 0.5 => _speedTextSlowBrush,
                    > 2.0 => _speedTextFastBrush,
                    _ => _speedTextNormalBrush
                };
            }

            // Update speed bars
            int activeBars = _displaySpeed switch
            {
                < 0.5 => 1,
                < 1.0 => 2,
                < 1.5 => 3,
                < 2.5 => 4,
                _ => 5
            };

            for (int i = 0; i < 5; i++)
            {
                Color barColor = i < activeBars ? GetSpeedColor(_displaySpeed) : InactiveBarColor;
                _speedBarBrushes[i].Color = barColor;
            }

            // Auto-hide
            _hideTimer -= deltaTime;
            if (_hideTimer <= 0)
            {
                Hide();
            }

            UpdatePosition();
        }

        private Color GetSpeedColor(double speed)
        {
            if (speed < 0.8)
                return Color.FromRgb(100, 150, 255); // Blue for slow
            if (speed < 1.2)
                return Color.FromRgb(100, 255, 150); // Green for normal
            if (speed < 2.0)
                return Color.FromRgb(255, 200, 100); // Yellow for fast
            return Color.FromRgb(255, 100, 100); // Red for very fast
        }

        private void Show()
        {
            if (_container == null)
                return;

            var animation = new DoubleAnimation
            {
                To = 1.0,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            _container.BeginAnimation(UIElement.OpacityProperty, animation);
        }

        private void Hide()
        {
            if (_container == null)
                return;

            var animation = new DoubleAnimation
            {
                To = 0.0,
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };
            _container.BeginAnimation(UIElement.OpacityProperty, animation);
        }

        /// <summary>
        /// Forces the indicator to show.
        /// </summary>
        public void ForceShow()
        {
            _hideTimer = HideDelay;
            Show();
        }
    }
}
