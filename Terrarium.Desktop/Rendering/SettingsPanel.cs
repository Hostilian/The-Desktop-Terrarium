using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Terrarium.Desktop.Rendering
{
    /// <summary>
    /// Settings panel for simulation configuration.
    /// </summary>
    public class SettingsPanel
    {
        private readonly Canvas _parentCanvas;
        private Border? _panelBorder;
        private StackPanel? _settingsContent;

        private static readonly Brush SettingsLabelBrush = CreateFrozenBrush(Color.FromRgb(200, 200, 200));
        private static readonly Brush SeparatorBrush = CreateFrozenBrush(Color.FromRgb(60, 60, 70));
        private static readonly Brush ToggleOnBrush = CreateFrozenBrush(Color.FromRgb(76, 175, 80));
        private static readonly Brush ToggleOffBrush = CreateFrozenBrush(Color.FromRgb(80, 80, 90));
        private bool _isVisible;

        // Settings values
        public double SimulationSpeed { get; private set; } = 1.0;
        public double SpawnRate { get; private set; } = 0.5;
        public bool ShowParticles { get; private set; } = true;
        public bool ShowNotifications { get; private set; } = true;
        public bool ShowWeatherEffects { get; private set; } = true;
        public bool EnableSound { get; private set; } = true;
        public int MaxPlants { get; private set; } = 50;
        public int MaxCreatures { get; private set; } = 30;

        // Events for settings changes
        public event Action<double>? SimulationSpeedChanged;
        public event Action<double>? SpawnRateChanged;
        public event Action<bool>? ParticlesToggled;
        public event Action<bool>? NotificationsToggled;
        public event Action<bool>? WeatherEffectsToggled;
        public event Action<bool>? SoundToggled;
        public event Action<int>? MaxPlantsChanged;
        public event Action<int>? MaxCreaturesChanged;

        private const double PanelWidth = 280;
        private const double PanelHeight = 420;

        public SettingsPanel(Canvas parentCanvas)
        {
            _parentCanvas = parentCanvas;
            CreatePanel();
        }

        private void CreatePanel()
        {
            _settingsContent = new StackPanel
            {
                Margin = new Thickness(15)
            };

            // Header
            var header = new TextBlock
            {
                Text = "⚙️ Settings",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                Margin = new Thickness(0, 0, 0, 15)
            };
            _settingsContent.Children.Add(header);

            // Simulation Speed
            AddSliderSetting("Simulation Speed", "🏃", 0.1, 3.0, SimulationSpeed, value =>
            {
                SimulationSpeed = value;
                SimulationSpeedChanged?.Invoke(value);
            }, "{0:F1}x");

            // Spawn Rate
            AddSliderSetting("Spawn Rate", "🌱", 0.0, 2.0, SpawnRate, value =>
            {
                SpawnRate = value;
                SpawnRateChanged?.Invoke(value);
            }, "{0:F1}x");

            // Max Plants
            AddSliderSetting("Max Plants", "🌿", 10, 100, MaxPlants, value =>
            {
                MaxPlants = (int)value;
                MaxPlantsChanged?.Invoke((int)value);
            }, "{0:F0}");

            // Max Creatures
            AddSliderSetting("Max Creatures", "🐾", 5, 60, MaxCreatures, value =>
            {
                MaxCreatures = (int)value;
                MaxCreaturesChanged?.Invoke((int)value);
            }, "{0:F0}");

            // Separator
            AddSeparator();

            // Visual Settings Header
            var visualHeader = new TextBlock
            {
                Text = "🎨 Visual Effects",
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(180, 180, 180)),
                Margin = new Thickness(0, 5, 0, 10)
            };
            _settingsContent.Children.Add(visualHeader);

            // Toggle Settings
            AddToggleSetting("Particles", "✨", ShowParticles, value =>
            {
                ShowParticles = value;
                ParticlesToggled?.Invoke(value);
            });

            AddToggleSetting("Notifications", "🔔", ShowNotifications, value =>
            {
                ShowNotifications = value;
                NotificationsToggled?.Invoke(value);
            });

            AddToggleSetting("Weather Effects", "🌧️", ShowWeatherEffects, value =>
            {
                ShowWeatherEffects = value;
                WeatherEffectsToggled?.Invoke(value);
            });

            AddToggleSetting("Sound", "🔊", EnableSound, value =>
            {
                EnableSound = value;
                SoundToggled?.Invoke(value);
            });

            // Close button
            var closeButton = new Button
            {
                Content = "Close",
                Width = 100,
                Height = 30,
                Margin = new Thickness(0, 20, 0, 0),
                Background = new SolidColorBrush(Color.FromRgb(70, 70, 80)),
                Foreground = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(100, 100, 110)),
                Cursor = System.Windows.Input.Cursors.Hand
            };
            closeButton.Click += (s, e) => Hide();
            _settingsContent.Children.Add(closeButton);

            // Main panel container
            _panelBorder = new Border
            {
                Width = PanelWidth,
                Height = PanelHeight,
                Background = new LinearGradientBrush(
                    Color.FromArgb(245, 30, 30, 46),
                    Color.FromArgb(245, 24, 24, 37),
                    90),
                BorderBrush = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(12),
                Child = _settingsContent,
                Visibility = Visibility.Collapsed,
                RenderTransform = new TranslateTransform(0, 0),
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.Black,
                    Direction = 270,
                    ShadowDepth = 8,
                    BlurRadius = 15,
                    Opacity = 0.7
                }
            };

            Canvas.SetZIndex(_panelBorder, 900);
            _parentCanvas.Children.Add(_panelBorder);

            // Update position when canvas size changes
            _parentCanvas.SizeChanged += (s, e) => UpdatePosition();
        }

        private void AddSliderSetting(string label, string icon, double min, double max, double initial, Action<double> onChanged, string format)
        {
            if (_settingsContent == null)
                return;

            var container = new StackPanel { Margin = new Thickness(0, 5, 0, 8) };

            // Label row with value
            var labelRow = new Grid();
            labelRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            labelRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var labelText = new TextBlock
            {
                Text = $"{icon} {label}",
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 200))
            };

            var valueText = new TextBlock
            {
                Text = string.Format(format, initial),
                FontSize = 12,
                Foreground = Brushes.White
            };
            Grid.SetColumn(valueText, 1);

            labelRow.Children.Add(labelText);
            labelRow.Children.Add(valueText);

            // Slider
            var slider = new Slider
            {
                Minimum = min,
                Maximum = max,
                Value = initial,
                Margin = new Thickness(0, 5, 0, 0),
                Background = new SolidColorBrush(Color.FromRgb(60, 60, 70))
            };

            slider.ValueChanged += (s, e) =>
            {
                valueText.Text = string.Format(format, e.NewValue);
                onChanged(e.NewValue);
            };

            container.Children.Add(labelRow);
            container.Children.Add(slider);
            _settingsContent.Children.Add(container);
        }

        private void AddToggleSetting(string label, string icon, bool initial, Action<bool> onChanged)
        {
            if (_settingsContent == null)
                return;

            var row = new Grid
            {
                Margin = new Thickness(0, 3, 0, 3)
            };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var labelText = new TextBlock
            {
                Text = $"{icon} {label}",
                FontSize = 12,
                Foreground = SettingsLabelBrush,
                VerticalAlignment = VerticalAlignment.Center
            };

            // Custom toggle switch
            var toggleBorder = new Border
            {
                Width = 44,
                Height = 22,
                CornerRadius = new CornerRadius(11),
                Background = initial ? ToggleOnBrush : ToggleOffBrush,
                Cursor = System.Windows.Input.Cursors.Hand
            };

            var toggleKnob = new Ellipse
            {
                Width = 18,
                Height = 18,
                Fill = Brushes.White,
                HorizontalAlignment = initial ? HorizontalAlignment.Right : HorizontalAlignment.Left,
                Margin = new Thickness(2)
            };

            toggleBorder.Child = toggleKnob;
            Grid.SetColumn(toggleBorder, 1);

            bool currentValue = initial;
            toggleBorder.MouseLeftButtonDown += (s, e) =>
            {
                currentValue = !currentValue;
                toggleKnob.HorizontalAlignment = currentValue ? HorizontalAlignment.Right : HorizontalAlignment.Left;
                toggleBorder.Background = currentValue ? ToggleOnBrush : ToggleOffBrush;
                onChanged(currentValue);
            };

            row.Children.Add(labelText);
            row.Children.Add(toggleBorder);
            _settingsContent.Children.Add(row);
        }

        private void AddSeparator()
        {
            if (_settingsContent == null)
                return;

            var separator = new Border
            {
                Height = 1,
                Background = SeparatorBrush,
                Margin = new Thickness(0, 10, 0, 10)
            };
            _settingsContent.Children.Add(separator);
        }

        private static SolidColorBrush CreateFrozenBrush(Color color)
        {
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }

        private void UpdatePosition()
        {
            if (_panelBorder == null)
                return;

            // Center the panel
            double x = (_parentCanvas.ActualWidth - PanelWidth) / 2;
            double y = (_parentCanvas.ActualHeight - PanelHeight) / 2;

            Canvas.SetLeft(_panelBorder, Math.Max(20, x));
            Canvas.SetTop(_panelBorder, Math.Max(20, y));
        }

        public void Show()
        {
            if (_panelBorder == null)
                return;

            UpdatePosition();
            _panelBorder.Visibility = Visibility.Visible;
            _isVisible = true;

            // Animate in
            var transform = _panelBorder.RenderTransform as TranslateTransform;
            if (transform != null)
            {
                transform.Y = -20;
                var animation = new DoubleAnimation(0, TimeSpan.FromMilliseconds(200))
                {
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };
                transform.BeginAnimation(TranslateTransform.YProperty, animation);
            }
        }

        public void Hide()
        {
            if (_panelBorder == null)
                return;

            _panelBorder.Visibility = Visibility.Collapsed;
            _isVisible = false;
        }

        public void Toggle()
        {
            if (_isVisible)
                Hide();
            else
                Show();
        }

        public bool IsVisible => _isVisible;
    }
}
