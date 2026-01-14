using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Terrarium.Desktop.Rendering;

/// <summary>
/// Settings panel for simulation configuration.
/// </summary>
public class SettingsPanel
{
    private readonly Canvas _parentCanvas;
    private Border? _panelBorder;
    private StackPanel? _settingsContent;

    // Static fields with proper prefix
    private static readonly Brush _settingsLabelBrush = CreateFrozenBrush(Color.FromRgb(200, 200, 200));
    private static readonly Brush _visualHeaderBrush = CreateFrozenBrush(Color.FromRgb(180, 180, 180));
    private static readonly Brush _separatorBrush = CreateFrozenBrush(Color.FromRgb(60, 60, 70));
    private static readonly Brush _toggleOnBrush = CreateFrozenBrush(Color.FromRgb(76, 175, 80));
    private static readonly Brush _toggleOffBrush = CreateFrozenBrush(Color.FromRgb(80, 80, 90));
    private static readonly Brush _sliderBackgroundBrush = CreateFrozenBrush(Color.FromRgb(60, 60, 70));

    private static readonly Brush _closeButtonBackgroundBrush = CreateFrozenBrush(Color.FromRgb(70, 70, 80));
    private static readonly Brush _closeButtonBorderBrush = CreateFrozenBrush(Color.FromRgb(100, 100, 110));

    private static readonly LinearGradientBrush _panelBackgroundBrush = Freeze(new LinearGradientBrush(
        Color.FromArgb(245, 30, 30, 46),
        Color.FromArgb(245, 24, 24, 37),
        90));
    private static readonly Brush _panelBorderBrush = CreateFrozenBrush(Color.FromArgb(100, 255, 255, 255));

    private static readonly System.Windows.Media.Effects.DropShadowEffect _panelShadowEffect = Freeze(new System.Windows.Media.Effects.DropShadowEffect
    {
        Color = Colors.Black,
        Direction = 270,
        ShadowDepth = 8,
        BlurRadius = 15,
        Opacity = 0.7
    });
    private bool _isVisible;

    public double SimulationSpeed { get; private set; } = 1.0;
    public double SpawnRate { get; private set; } = 0.5;
    public bool ShowParticles { get; private set; } = true;
    public bool ShowNotifications { get; private set; } = true;
    public bool ShowWeatherEffects { get; private set; } = true;
    public bool EnableSound { get; private set; } = true;
    public bool SimpleMode { get; private set; } = true;
    public int MaxPlants { get; private set; } = 50;
    public int MaxCreatures { get; private set; } = 30;

    public event Action<double>? SimulationSpeedChanged;
    public event Action<double>? SpawnRateChanged;
    public event Action<bool>? ParticlesToggled;
    public event Action<bool>? NotificationsToggled;
    public event Action<bool>? WeatherEffectsToggled;
    public event Action<bool>? SoundToggled;
    public event Action<bool>? SimpleModeToggled;
    public event Action<int>? MaxPlantsChanged;
    public event Action<int>? MaxCreaturesChanged;

    private const double _panelWidth = 280;
    private const double _panelHeight = 420;

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
            Foreground = _visualHeaderBrush,
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

        AddToggleSetting("Simple Mode", "🎯", SimpleMode, value =>
        {
            SimpleMode = value;
            SimpleModeToggled?.Invoke(value);
        });

        var closeButton = new Button
        {
            Content = "Close",
            Width = 100,
            Height = 30,
            Margin = new Thickness(0, 20, 0, 0),
            Background = _closeButtonBackgroundBrush,
            Foreground = Brushes.White,
            BorderBrush = _closeButtonBorderBrush,
            Cursor = System.Windows.Input.Cursors.Hand
        };
        closeButton.Click += (s, e) => Hide();
        _settingsContent.Children.Add(closeButton);

        _panelBorder = new Border
        {
            Width = _panelWidth,
            Height = _panelHeight,
            Background = _panelBackgroundBrush,
            BorderBrush = _panelBorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12),
            Child = _settingsContent,
            Visibility = Visibility.Collapsed,
            RenderTransform = new TranslateTransform(0, 0),
            Effect = _panelShadowEffect
        };

        Canvas.SetZIndex(_panelBorder, 900);
        _parentCanvas.Children.Add(_panelBorder);

        // Update position when canvas size changes
        _parentCanvas.SizeChanged += (s, e) => UpdatePosition();
    }

    private void AddSliderSetting(string label, string icon, double min, double max, double initial, Action<double> onChanged, string format)
    {
        if (_settingsContent == null)
        {
            return;
        }

        var container = new StackPanel { Margin = new Thickness(0, 5, 0, 8) };

        // Label row with value
        var labelRow = new Grid();
        labelRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        labelRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var labelText = new TextBlock
        {
            Text = $"{icon} {label}",
            FontSize = 12,
            Foreground = _settingsLabelBrush
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
            Background = _sliderBackgroundBrush
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
        {
            return;
        }

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
            Foreground = _settingsLabelBrush,
            VerticalAlignment = VerticalAlignment.Center
        };

        // Custom toggle switch
        var toggleBorder = new Border
        {
            Width = 44,
            Height = 22,
            CornerRadius = new CornerRadius(11),
            Background = initial ? _toggleOnBrush : _toggleOffBrush,
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
            toggleBorder.Background = currentValue ? _toggleOnBrush : _toggleOffBrush;
            onChanged(currentValue);
        };

        row.Children.Add(labelText);
        row.Children.Add(toggleBorder);
        _settingsContent.Children.Add(row);
    }

    private void AddSeparator()
    {
        if (_settingsContent == null)
        {
            return;
        }

        var separator = new Border
        {
            Height = 1,
            Background = _separatorBrush,
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

    private static T Freeze<T>(T freezable) where T : Freezable
    {
        if (freezable.CanFreeze)
        {
            freezable.Freeze();
        }
        return freezable;
    }

    private void UpdatePosition()
    {
        if (_panelBorder == null)
        {
            return;
        }

        // Center the panel
        double x = (_parentCanvas.ActualWidth - _panelWidth) / 2;
        double y = (_parentCanvas.ActualHeight - _panelHeight) / 2;

        Canvas.SetLeft(_panelBorder, Math.Max(20, x));
        Canvas.SetTop(_panelBorder, Math.Max(20, y));
    }

    public void Show()
    {
        if (_panelBorder == null)
        {
            return;
        }

        UpdatePosition();
        _panelBorder.Visibility = Visibility.Visible;
        _isVisible = true;

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
        {
            return;
        }

        _panelBorder.Visibility = Visibility.Collapsed;
        _isVisible = false;
    }

    public void Toggle()
    {
        if (_isVisible)
        {
            Hide();
        }
        else
        {
            Show();
        }
    }

    public bool IsVisible => _isVisible;
}
