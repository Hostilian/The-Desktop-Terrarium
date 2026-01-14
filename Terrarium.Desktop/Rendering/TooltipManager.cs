using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Terrarium.Logic.Entities;

namespace Terrarium.Desktop.Rendering;

/// <summary>
/// Manages tooltip display for creature/plant information on hover.
/// </summary>
public class TooltipManager
{
    private readonly Canvas _canvas;
    private Border? _tooltipBorder;
    private StackPanel? _tooltipContent;
    private WorldEntity? _currentEntity;
    private bool _isVisible;

    private TextBlock? _headerText;
    private SolidColorBrush? _headerForeground;

    private StackPanel? _barAContainer;
    private TextBlock? _barALabelText;
    private TextBlock? _barAValueText;
    private Border? _barAFill;
    private SolidColorBrush? _barAFillBrush;

    private StackPanel? _barBContainer;
    private TextBlock? _barBLabelText;
    private TextBlock? _barBValueText;
    private Border? _barBFill;
    private SolidColorBrush? _barBFillBrush;

    private TextBlock? _statText1;
    private TextBlock? _statText2;
    private TextBlock? _statText3;
    private Border? _footerSeparator;
    private TextBlock? _footerText;

    private const double BarMaxWidth = TooltipWidth - 20;

    private const double TooltipWidth = 180;
    private const double TooltipOffset = 15;

    public TooltipManager(Canvas canvas)
    {
        _canvas = canvas;
        CreateTooltipVisual();
    }

    private void CreateTooltipVisual()
    {
        _tooltipContent = new StackPanel
        {
            Margin = new Thickness(10, 8, 10, 8)
        };

        BuildTooltipTemplate();

        _tooltipBorder = new Border
        {
            Width = TooltipWidth,
            Background = CreateFrozenBrush(Color.FromArgb(240, 30, 30, 46)),
            BorderBrush = CreateFrozenBrush(Color.FromArgb(100, 255, 255, 255)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Child = _tooltipContent,
            Visibility = Visibility.Collapsed,
            Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = Colors.Black,
                Direction = 270,
                ShadowDepth = 4,
                BlurRadius = 10,
                Opacity = 0.6
            }
        };

        Canvas.SetZIndex(_tooltipBorder, 999);
        _canvas.Children.Add(_tooltipBorder);
    }

    private void BuildTooltipTemplate()
    {
        if (_tooltipContent == null)
        {
            return;
        }

        _tooltipContent.Children.Clear();

        _headerForeground = new SolidColorBrush(Colors.White);
        _headerText = new TextBlock
        {
            Text = string.Empty,
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Foreground = _headerForeground,
            Margin = new Thickness(0, 0, 0, 8)
        };
        _tooltipContent.Children.Add(_headerText);

        _barAContainer = CreateStatBar(out _barALabelText, out _barAValueText, out _barAFill, out _barAFillBrush);
        _tooltipContent.Children.Add(_barAContainer);

        _barBContainer = CreateStatBar(out _barBLabelText, out _barBValueText, out _barBFill, out _barBFillBrush);
        _tooltipContent.Children.Add(_barBContainer);

        _statText1 = CreateStatText();
        _statText2 = CreateStatText();
        _statText3 = CreateStatText();
        _tooltipContent.Children.Add(_statText1);
        _tooltipContent.Children.Add(_statText2);
        _tooltipContent.Children.Add(_statText3);

        _footerSeparator = new Border
        {
            Height = 1,
            Background = CreateFrozenBrush(Color.FromRgb(60, 60, 70)),
            Margin = new Thickness(0, 8, 0, 6)
        };
        _tooltipContent.Children.Add(_footerSeparator);

        _footerText = new TextBlock
        {
            Text = string.Empty,
            FontSize = 11,
            FontStyle = FontStyles.Italic,
            Foreground = CreateFrozenBrush(Color.FromRgb(200, 200, 200))
        };
        _tooltipContent.Children.Add(_footerText);
    }

    private static TextBlock CreateStatText()
    {
        return new TextBlock
        {
            Text = string.Empty,
            FontSize = 11,
            Foreground = CreateFrozenBrush(Color.FromRgb(180, 180, 180)),
            Margin = new Thickness(0, 2, 0, 0)
        };
    }

    private static StackPanel CreateStatBar(
        out TextBlock labelText,
        out TextBlock valueText,
        out Border barFill,
        out SolidColorBrush fillBrush)
    {
        var container = new StackPanel { Margin = new Thickness(0, 2, 0, 2) };

        var labelRow = new Grid();
        labelRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        labelRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        labelText = new TextBlock
        {
            Text = string.Empty,
            FontSize = 10,
            Foreground = CreateFrozenBrush(Color.FromRgb(150, 150, 150))
        };

        valueText = new TextBlock
        {
            Text = string.Empty,
            FontSize = 10,
            Foreground = Brushes.White
        };
        Grid.SetColumn(valueText, 1);

        labelRow.Children.Add(labelText);
        labelRow.Children.Add(valueText);

        var barBackground = new Border
        {
            Height = 6,
            Background = CreateFrozenBrush(Color.FromRgb(60, 60, 70)),
            CornerRadius = new CornerRadius(3),
            Margin = new Thickness(0, 2, 0, 0)
        };

        fillBrush = new SolidColorBrush(Color.FromRgb(46, 204, 113));
        barFill = new Border
        {
            Height = 6,
            Width = 0,
            HorizontalAlignment = HorizontalAlignment.Left,
            Background = fillBrush,
            CornerRadius = new CornerRadius(3)
        };

        var barGrid = new Grid { Margin = new Thickness(0, 2, 0, 0) };
        barGrid.Children.Add(barBackground);
        barGrid.Children.Add(barFill);

        container.Children.Add(labelRow);
        container.Children.Add(barGrid);

        return container;
    }

    /// <summary>
    /// Shows tooltip for an entity at the specified position.
    /// </summary>
    public void ShowTooltip(WorldEntity entity, double mouseX, double mouseY)
    {
        if (_tooltipBorder == null || _tooltipContent == null)
        {
            return;
        }

        _currentEntity = entity;
        UpdateTooltipContent(entity);

        double x = mouseX + TooltipOffset;
        double y = mouseY + TooltipOffset;

        if (x + TooltipWidth > _canvas.ActualWidth)
        {
            x = mouseX - TooltipWidth - TooltipOffset;
        }

        double tooltipHeight = _tooltipBorder.ActualHeight > 0 ? _tooltipBorder.ActualHeight : 120;
        if (y + tooltipHeight > _canvas.ActualHeight)
        {
            y = mouseY - tooltipHeight - TooltipOffset;
        }

        Canvas.SetLeft(_tooltipBorder, Math.Max(5, x));
        Canvas.SetTop(_tooltipBorder, Math.Max(5, y));

        _tooltipBorder.Visibility = Visibility.Visible;
        _isVisible = true;
    }

    /// <summary>
    /// Hides the tooltip.
    /// </summary>
    public void HideTooltip()
    {
        if (_tooltipBorder == null)
        {
            return;
        }

        _tooltipBorder.Visibility = Visibility.Collapsed;
        _currentEntity = null;
        _isVisible = false;
    }

    /// <summary>
    /// Updates tooltip position if visible.
    /// </summary>
    public void UpdatePosition(double mouseX, double mouseY)
    {
        if (!_isVisible || _tooltipBorder == null)
        {
            return;
        }

        double x = mouseX + TooltipOffset;
        double y = mouseY + TooltipOffset;

        if (x + TooltipWidth > _canvas.ActualWidth)
        {
            x = mouseX - TooltipWidth - TooltipOffset;
        }

        double tooltipHeight = _tooltipBorder.ActualHeight > 0 ? _tooltipBorder.ActualHeight : 120;
        if (y + tooltipHeight > _canvas.ActualHeight)
        {
            y = mouseY - tooltipHeight - TooltipOffset;
        }

        Canvas.SetLeft(_tooltipBorder, Math.Max(5, x));
        Canvas.SetTop(_tooltipBorder, Math.Max(5, y));
    }

    /// <summary>
    /// Updates tooltip content if the entity data has changed.
    /// </summary>
    public void Update()
    {
        if (_isVisible && _currentEntity != null)
        {
            UpdateTooltipContent(_currentEntity);
        }
    }

    private void UpdateTooltipContent(WorldEntity entity)
    {
        if (_tooltipContent == null)
        {
            return;
        }

        if (_headerText == null)
        {
            BuildTooltipTemplate();
        }

        if (entity is Plant plant)
        {
            UpdatePlantTooltip(plant);
        }
        else if (entity is Herbivore herbivore)
        {
            UpdateHerbivoreTooltip(herbivore);
        }
        else if (entity is Carnivore carnivore)
        {
            UpdateCarnivoreTooltip(carnivore);
        }
    }

    private void UpdatePlantTooltip(Plant plant)
    {
        if (_headerText == null || _headerForeground == null || _barAContainer == null || _barBContainer == null ||
            _barALabelText == null || _barAValueText == null || _barAFill == null || _barAFillBrush == null ||
            _barBLabelText == null || _barBValueText == null || _barBFill == null || _barBFillBrush == null ||
            _statText1 == null || _statText2 == null || _statText3 == null || _footerSeparator == null || _footerText == null)
        {
            return;
        }

        _headerText.Text = "🌿 Plant";
        _headerForeground.Color = Color.FromRgb(76, 175, 80);

        _barAContainer.Visibility = Visibility.Visible;
        _barBContainer.Visibility = Visibility.Visible;
        _statText3.Visibility = Visibility.Collapsed;

        UpdateBar(_barALabelText, _barAValueText, _barAFill, _barAFillBrush,
            "Health", plant.Health, 100, Color.FromRgb(231, 76, 60), Color.FromRgb(46, 204, 113));

        UpdateBar(_barBLabelText, _barBValueText, _barBFill, _barBFillBrush,
            "Water", plant.WaterLevel, 100, Color.FromRgb(52, 152, 219), Color.FromRgb(52, 152, 219));

        _statText1.Text = $"Size: {plant.Size:F1}";
        _statText2.Text = $"Age: {plant.Age:F0}s";

        string status = plant.IsAlive ? "Healthy" : "Dead";
        if (plant.IsAlive && plant.WaterLevel < 30)
        {
            status = "Thirsty! 💧";
        }

        _footerText.Text = status;
    }

    private void UpdateHerbivoreTooltip(Herbivore herbivore)
    {
        if (_headerText == null || _headerForeground == null || _barAContainer == null || _barBContainer == null ||
            _barALabelText == null || _barAValueText == null || _barAFill == null || _barAFillBrush == null ||
            _barBLabelText == null || _barBValueText == null || _barBFill == null || _barBFillBrush == null ||
            _statText1 == null || _statText2 == null || _statText3 == null || _footerSeparator == null || _footerText == null)
        {
            return;
        }

        _headerText.Text = $"🐰 {herbivore.Type}";
        _headerForeground.Color = Color.FromRgb(255, 183, 77);

        _barAContainer.Visibility = Visibility.Visible;
        _barBContainer.Visibility = Visibility.Visible;
        _statText3.Visibility = Visibility.Collapsed;

        UpdateBar(_barALabelText, _barAValueText, _barAFill, _barAFillBrush,
            "Health", herbivore.Health, 100, Color.FromRgb(231, 76, 60), Color.FromRgb(46, 204, 113));

        UpdateBar(_barBLabelText, _barBValueText, _barBFill, _barBFillBrush,
            "Hunger", 100 - herbivore.Hunger, 100, Color.FromRgb(230, 126, 34), Color.FromRgb(46, 204, 113));

        _statText1.Text = $"Speed: {herbivore.Speed:F1}";
        _statText2.Text = $"Age: {herbivore.Age:F0}s";

        _footerText.Text = herbivore.IsAlive ? GetCreatureStatus(herbivore) : "Dead";
    }

    private void UpdateCarnivoreTooltip(Carnivore carnivore)
    {
        if (_headerText == null || _headerForeground == null || _barAContainer == null || _barBContainer == null ||
            _barALabelText == null || _barAValueText == null || _barAFill == null || _barAFillBrush == null ||
            _barBLabelText == null || _barBValueText == null || _barBFill == null || _barBFillBrush == null ||
            _statText1 == null || _statText2 == null || _statText3 == null || _footerSeparator == null || _footerText == null)
        {
            return;
        }

        _headerText.Text = $"🐺 {carnivore.Type}";
        _headerForeground.Color = Color.FromRgb(120, 120, 130);

        _barAContainer.Visibility = Visibility.Visible;
        _barBContainer.Visibility = Visibility.Visible;
        _statText3.Visibility = Visibility.Collapsed;

        UpdateBar(_barALabelText, _barAValueText, _barAFill, _barAFillBrush,
            "Health", carnivore.Health, 100, Color.FromRgb(231, 76, 60), Color.FromRgb(46, 204, 113));

        UpdateBar(_barBLabelText, _barBValueText, _barBFill, _barBFillBrush,
            "Hunger", 100 - carnivore.Hunger, 100, Color.FromRgb(230, 126, 34), Color.FromRgb(46, 204, 113));

        _statText1.Text = $"Speed: {carnivore.Speed:F1}";
        _statText2.Text = $"Age: {carnivore.Age:F0}s";

        _footerText.Text = carnivore.IsAlive ? GetCreatureStatus(carnivore) : "Dead";
    }

    private static void UpdateBar(
        TextBlock labelText,
        TextBlock valueText,
        Border barFill,
        SolidColorBrush fillBrush,
        string label,
        double value,
        double max,
        Color lowColor,
        Color highColor)
    {
        double clampedValue = Math.Clamp(value, 0, max);
        double ratio = max <= 0 ? 0 : Math.Clamp(clampedValue / max, 0, 1);

        labelText.Text = label;
        valueText.Text = $"{clampedValue:F0}%";

        barFill.Width = BarMaxWidth * ratio;
        fillBrush.Color = InterpolateColor(lowColor, highColor, ratio);
    }

    private string GetCreatureStatus(Creature creature)
    {
        if (creature.Hunger > 80)
        {
            return "Starving! 🍖";
        }

        if (creature.Hunger > 50)
        {
            return "Hungry";
        }

        if (creature.Health < 30)
        {
            return "Injured! 💔";
        }

        if (Math.Abs(creature.VelocityX) > 0.1 || Math.Abs(creature.VelocityY) > 0.1)
        {
            return creature is Carnivore ? "Hunting 🎯" : "Foraging 🌿";
        }
        return "Resting 😴";
    }

    private static Color InterpolateColor(Color from, Color to, double ratio)
    {
        return Color.FromRgb(
            (byte)(from.R + (to.R - from.R) * ratio),
            (byte)(from.G + (to.G - from.G) * ratio),
            (byte)(from.B + (to.B - from.B) * ratio)
        );
    }

    private static SolidColorBrush CreateFrozenBrush(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }
}
