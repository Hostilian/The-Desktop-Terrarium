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

        private double _currentHealth;
        private double _displayHealth;
        private double _pulsePhase;

        // Animation constants
        private const double AnimationSpeed = 3.0;

        // UI Layout constants
        private const double ContainerWidth = 200;
        private const double HealthBarHeight = 12;
        private const double HealthBarCornerRadius = 6;

        // Colors
        private static readonly Color ContainerBackgroundColor = Color.FromArgb(200, 20, 30, 40);
        private static readonly Color ContainerBorderColor = Color.FromRgb(80, 80, 80);
        private static readonly Color BarBackgroundColor = Color.FromRgb(40, 40, 40);
        private static readonly Color ScoreTextColor = Color.FromRgb(100, 255, 150);

        // Typography
        private const double TitleFontSize = 11;
        private const double ScoreFontSize = 11;

        // Margins and padding
        private const double ContainerPadding = 10;
        private const double ContainerBorderThickness = 1;
        private const double ContainerCornerRadius = 8;
        private const double BarMarginVertical = 6;

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
                Width = ContainerWidth,
                Background = new SolidColorBrush(ContainerBackgroundColor),
                BorderBrush = new SolidColorBrush(ContainerBorderColor),
                BorderThickness = new Thickness(ContainerBorderThickness),
                CornerRadius = new CornerRadius(ContainerCornerRadius),
                Padding = new Thickness(ContainerPadding)
            };

            var stack = new StackPanel();

            // Title row
            var titleRow = new Grid();
            titleRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            titleRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var title = new TextBlock
            {
                Text = "🌍 Ecosystem Health",
                FontSize = TitleFontSize,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White
            };
            Grid.SetColumn(title, 0);
            titleRow.Children.Add(title);

            _scoreText = new TextBlock
            {
                Text = "100%",
                FontSize = ScoreFontSize,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(ScoreTextColor)
            };
            Grid.SetColumn(_scoreText, 1);
            titleRow.Children.Add(_scoreText);

            stack.Children.Add(titleRow);

            // Health bar background
            var barBg = new Border
            {
                Height = HealthBarHeight,
                Background = new SolidColorBrush(BarBackgroundColor),
                CornerRadius = new CornerRadius(HealthBarCornerRadius),
                Margin = new Thickness(0, BarMarginVertical, 0, BarMarginVertical)
            };

            _healthBar = new Rectangle
            {
                Height = HealthBarHeight,
                RadiusX = HealthBarCornerRadius,
                RadiusY = HealthBarCornerRadius,
                Fill = CreateHealthGradient(100)
            };

            var barContainer = new Grid();
            barContainer.Children.Add(barBg);
            barContainer.Children.Add(_healthBar);
            stack.Children.Add(barContainer);

            // Status text
            _statusText = new TextBlock
            {
                Text = "✨ Thriving",
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromRgb(100, 255, 150)),
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

            // Animate display health
            _displayHealth += (_currentHealth - _displayHealth) * AnimationSpeed * deltaTime;
            _displayHealth = Math.Clamp(_displayHealth, 0, 100);

            _pulsePhase += deltaTime * 2;

            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (_healthBar == null || _scoreText == null || _statusText == null)
                return;

            // Update bar width
            _healthBar.Width = (_displayHealth / 100) * 180;
            _healthBar.Fill = CreateHealthGradient(_displayHealth);

            // Update score text
            _scoreText.Text = $"{_displayHealth:F0}%";
            _scoreText.Foreground = new SolidColorBrush(GetHealthColor(_displayHealth));

            // Update status text
            var (status, color) = GetStatus(_displayHealth);
            _statusText.Text = status;
            _statusText.Foreground = new SolidColorBrush(color);

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

        private LinearGradientBrush CreateHealthGradient(double health)
        {
            var color = GetHealthColor(health);
            var lighterColor = Color.FromRgb(
                (byte)Math.Min(255, color.R + 40),
                (byte)Math.Min(255, color.G + 40),
                (byte)Math.Min(255, color.B + 40));

            return new LinearGradientBrush(lighterColor, color, 90);
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
    }
}
