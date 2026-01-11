using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Terrarium.Logic.Simulation;

namespace Terrarium.Desktop.Rendering
{
    /// <summary>
    /// Shows a visual indicator of ecosystem health and balance.
    /// </summary>
    public class EcosystemHealthBar
    {
        private readonly Canvas _canvas;
        private Border? _container;
        private Rectangle? _healthBar;
        private TextBlock? _statusText;
        private TextBlock? _scoreText;

        private LinearGradientBrush? _healthGradient;
        private GradientStop? _healthGradientLightStop;
        private GradientStop? _healthGradientDarkStop;
        private SolidColorBrush? _scoreForegroundBrush;
        private SolidColorBrush? _statusForegroundBrush;

        private const double BarMaxWidth = 180;

        private double _currentHealth;
        private double _displayHealth;
        private double _pulsePhase;

        private const double AnimationSpeed = 3.0;

        public bool IsEnabled { get; set; } = true;

        public EcosystemHealthBar(Canvas canvas)
        {
            _canvas = canvas;
            _currentHealth = 100;
            _displayHealth = 100;
            CreateUI();
        }

        private void CreateUI()
        {
            _container = new Border
            {
                Width = 200,
                Background = CreateFrozenBrush(Color.FromArgb(200, 20, 30, 40)),
                BorderBrush = CreateFrozenBrush(Color.FromRgb(80, 80, 80)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(10)
            };

            var stack = new StackPanel();

            // Title row
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

            _scoreForegroundBrush = new SolidColorBrush(Color.FromRgb(100, 255, 150));
            _scoreText = new TextBlock
            {
                Text = "100%",
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                Foreground = _scoreForegroundBrush
            };
            Grid.SetColumn(_scoreText, 1);
            titleRow.Children.Add(_scoreText);

            stack.Children.Add(titleRow);

            // Health bar background
            var barBg = new Border
            {
                Height = 12,
                Background = CreateFrozenBrush(Color.FromRgb(40, 40, 40)),
                CornerRadius = new CornerRadius(6),
                Margin = new Thickness(0, 6, 0, 6)
            };

            _healthGradientLightStop = new GradientStop(Color.FromRgb(140, 255, 190), 0);
            _healthGradientDarkStop = new GradientStop(Color.FromRgb(100, 255, 150), 1);
            _healthGradient = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(0, 1),
                GradientStops = new GradientStopCollection
                {
                    _healthGradientLightStop,
                    _healthGradientDarkStop
                }
            };

            _healthBar = new Rectangle
            {
                Height = 12,
                RadiusX = 6,
                RadiusY = 6,
                Fill = _healthGradient
            };

            var barContainer = new Grid();
            barContainer.Children.Add(barBg);
            barContainer.Children.Add(_healthBar);
            stack.Children.Add(barContainer);

            // Status text
            _statusForegroundBrush = new SolidColorBrush(Color.FromRgb(100, 255, 150));
            _statusText = new TextBlock
            {
                Text = "✨ Thriving",
                FontSize = 10,
                Foreground = _statusForegroundBrush,
                TextAlignment = TextAlignment.Center
            };
            stack.Children.Add(_statusText);

            _container.Child = stack;

            Canvas.SetLeft(_container, 10);
            Canvas.SetBottom(_container, 80);
            Canvas.SetTop(_container, double.NaN);
            Canvas.SetZIndex(_container, 800);

            _canvas.Children.Add(_container);
        }

        /// <summary>
        /// Calculates and updates the ecosystem health score.
        /// </summary>
        public void Update(double deltaTime, int plantCount, int herbivoreCount, int carnivoreCount)
        {
            if (!IsEnabled)
            {
                if (_container != null)
                    _container.Visibility = Visibility.Collapsed;
                return;
            }

            if (_container != null)
                _container.Visibility = Visibility.Visible;

            // Calculate health score based on ecosystem balance
            _currentHealth = EcosystemHealthScorer.CalculateHealthPercent(plantCount, herbivoreCount, carnivoreCount);

            _displayHealth += (_currentHealth - _displayHealth) * AnimationSpeed * deltaTime;
            _displayHealth = Math.Clamp(_displayHealth, 0, 100);

            _pulsePhase += deltaTime * 2;

            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (_healthBar == null || _scoreText == null || _statusText == null ||
                _healthGradientLightStop == null || _healthGradientDarkStop == null ||
                _scoreForegroundBrush == null || _statusForegroundBrush == null)
                return;

            _healthBar.Width = (_displayHealth / 100) * BarMaxWidth;

            var color = GetHealthColor(_displayHealth);
            var lighterColor = Color.FromRgb(
                (byte)Math.Min(255, color.R + 40),
                (byte)Math.Min(255, color.G + 40),
                (byte)Math.Min(255, color.B + 40));
            _healthGradientLightStop.Color = lighterColor;
            _healthGradientDarkStop.Color = color;

            _scoreText.Text = $"{_displayHealth:F0}%";
            _scoreForegroundBrush.Color = color;

            var (status, statusColor) = GetStatus(_displayHealth);
            _statusText.Text = status;
            _statusForegroundBrush.Color = statusColor;

            // Pulse effect for critical health
            if (_displayHealth < 25 && _container != null)
            {
                double pulse = 0.7 + 0.3 * Math.Sin(_pulsePhase * 3);
                _container.Opacity = pulse;
            }
            else if (_container != null)
            {
                _container.Opacity = 1.0;
            }
        }

        private Color GetHealthColor(double health)
        {
            if (health >= 75)
                return Color.FromRgb(100, 255, 150); // Green
            if (health >= 50)
                return Color.FromRgb(255, 220, 100); // Yellow
            if (health >= 25)
                return Color.FromRgb(255, 150, 100); // Orange
            return Color.FromRgb(255, 80, 80); // Red
        }

        private (string status, Color color) GetStatus(double health)
        {
            if (health >= 80)
                return ("✨ Thriving", Color.FromRgb(100, 255, 150));
            if (health >= 60)
                return ("🌱 Healthy", Color.FromRgb(180, 255, 100));
            if (health >= 40)
                return ("⚖️ Balanced", Color.FromRgb(255, 220, 100));
            if (health >= 25)
                return ("⚠️ Struggling", Color.FromRgb(255, 150, 100));
            return ("🆘 Critical", Color.FromRgb(255, 80, 80));
        }

        private static SolidColorBrush CreateFrozenBrush(Color color)
        {
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }
    }
}
