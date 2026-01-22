namespace Terrarium.Desktop.Rendering;

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Terrarium.Logic.Entities;

/// <summary>
/// Manages tooltip display for creature/plant information on hover.
/// </summary>
public class TooltipManager
{
    private readonly Canvas canvas;
    private Border? tooltipBorder;
    private StackPanel? tooltipContent;
    private WorldEntity? currentEntity;
    private bool isVisible;

    private TextBlock? headerText;
    private SolidColorBrush? headerForeground;

    private StackPanel? barAContainer;
    private TextBlock? barALabelText;
    private TextBlock? barAValueText;
    private Border? barAFill;
    private SolidColorBrush? barAFillBrush;

    private StackPanel? barBContainer;
    private TextBlock? barBLabelText;
    private TextBlock? barBValueText;
    private Border? barBFill;
    private SolidColorBrush? barBFillBrush;

    private TextBlock? statText1;
    private TextBlock? statText2;
    private TextBlock? statText3;
    private Border? footerSeparator;
    private TextBlock? footerText;

    private const double BarMaxWidth = TooltipWidth - 20;

    private const double TooltipWidth = 180;
    private const double TooltipOffset = 15;

    public TooltipManager(Canvas canvas)
    {
        this.canvas = canvas;
        CreateTooltipVisual();
    }

    private void CreateTooltipVisual()
    {
        tooltipContent = new StackPanel
        {
            Margin = new Thickness(10, 8, 10, 8)
        };

        BuildTooltipTemplate();

        tooltipBorder = new Border
        {
            Width = TooltipWidth,
            Background = CreateFrozenBrush(Color.FromArgb(240, 30, 30, 46)),
            BorderBrush = CreateFrozenBrush(Color.FromArgb(100, 255, 255, 255)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Child = tooltipContent,
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

        Canvas.SetZIndex(tooltipBorder, 999);
        canvas.Children.Add(tooltipBorder);
    }

    private void BuildTooltipTemplate()
    {
        if (tooltipContent == null)
        {
            return;
        }

        tooltipContent.Children.Clear();

        headerForeground = new SolidColorBrush(Colors.White);
        headerText = new TextBlock
        {
            Text = string.Empty,
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Foreground = headerForeground,
            Margin = new Thickness(0, 0, 0, 8)
        };
        tooltipContent.Children.Add(headerText);

        barAContainer = CreateStatBar(out barALabelText, out barAValueText, out barAFill, out barAFillBrush);
        tooltipContent.Children.Add(barAContainer);

        barBContainer = CreateStatBar(out barBLabelText, out barBValueText, out barBFill, out barBFillBrush);
        tooltipContent.Children.Add(barBContainer);

        statText1 = CreateStatText();
        statText2 = CreateStatText();
        statText3 = CreateStatText();
        tooltipContent.Children.Add(statText1);
        tooltipContent.Children.Add(statText2);
        tooltipContent.Children.Add(statText3);

        footerSeparator = new Border
        {
            Height = 1,
            Background = CreateFrozenBrush(Color.FromRgb(60, 60, 70)),
            Margin = new Thickness(0, 8, 0, 6)
        };
        tooltipContent.Children.Add(footerSeparator);

        footerText = new TextBlock
        {
            Text = string.Empty,
            FontSize = 11,
            FontStyle = FontStyles.Italic,
            Foreground = CreateFrozenBrush(Color.FromRgb(200, 200, 200))
        };
        tooltipContent.Children.Add(footerText);
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
        if (tooltipBorder == null || tooltipContent == null)
        {
            return;
        }

        currentEntity = entity;
        UpdateTooltipContent(entity);

        double x = mouseX + TooltipOffset;
        double y = mouseY + TooltipOffset;

        if (x + TooltipWidth > canvas.ActualWidth)
        {
            x = mouseX - TooltipWidth - TooltipOffset;
        }

        double tooltipHeight = tooltipBorder.ActualHeight > 0 ? tooltipBorder.ActualHeight : 120;
        if (y + tooltipHeight > canvas.ActualHeight)
        {
            y = mouseY - tooltipHeight - TooltipOffset;
        }

        Canvas.SetLeft(tooltipBorder, Math.Max(5, x));
        Canvas.SetTop(tooltipBorder, Math.Max(5, y));

        tooltipBorder.Visibility = Visibility.Visible;
        isVisible = true;
    }

    /// <summary>
    /// Hides the tooltip.
    /// </summary>
    public void HideTooltip()
    {
        if (tooltipBorder == null)
        {
            return;
        }

        tooltipBorder.Visibility = Visibility.Collapsed;
        currentEntity = null;
        isVisible = false;
    }

    /// <summary>
    /// Updates tooltip position if visible.
    /// </summary>
    public void UpdatePosition(double mouseX, double mouseY)
    {
        if (!isVisible || tooltipBorder == null)
        {
            return;
        }

        double x = mouseX + TooltipOffset;
        double y = mouseY + TooltipOffset;

        if (x + TooltipWidth > canvas.ActualWidth)
        {
            x = mouseX - TooltipWidth - TooltipOffset;
        }

        double tooltipHeight = tooltipBorder.ActualHeight > 0 ? tooltipBorder.ActualHeight : 120;
        if (y + tooltipHeight > canvas.ActualHeight)
        {
            y = mouseY - tooltipHeight - TooltipOffset;
        }

        Canvas.SetLeft(tooltipBorder, Math.Max(5, x));
        Canvas.SetTop(tooltipBorder, Math.Max(5, y));
    }

    /// <summary>
    /// Updates tooltip content if the entity data has changed.
    /// </summary>
    public void Update()
    {
        if (isVisible && currentEntity != null)
        {
            UpdateTooltipContent(currentEntity);
        }
    }

    private void UpdateTooltipContent(WorldEntity entity)
    {
        if (tooltipContent == null)
        {
            return;
        }

        if (headerText == null)
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
        if (headerText == null || headerForeground == null || barAContainer == null || barBContainer == null ||
            barALabelText == null || barAValueText == null || barAFill == null || barAFillBrush == null ||
            barBLabelText == null || barBValueText == null || barBFill == null || barBFillBrush == null ||
            statText1 == null || statText2 == null || statText3 == null || footerSeparator == null || footerText == null)
        {
            return;
        }

        headerText.Text = "🌿 Plant";
        headerForeground.Color = Color.FromRgb(76, 175, 80);

        barAContainer.Visibility = Visibility.Visible;
        barBContainer.Visibility = Visibility.Visible;
        statText3.Visibility = Visibility.Collapsed;

        UpdateBar(barALabelText, barAValueText, barAFill, barAFillBrush,
            "Health", plant.Health, 100, Color.FromRgb(231, 76, 60), Color.FromRgb(46, 204, 113));

        UpdateBar(barBLabelText, barBValueText, barBFill, barBFillBrush,
            "Water", plant.WaterLevel, 100, Color.FromRgb(52, 152, 219), Color.FromRgb(52, 152, 219));

        statText1.Text = $"Size: {plant.Size:F1}";
        statText2.Text = $"Age: {plant.Age:F0}s";

        string status = plant.IsAlive ? "Healthy" : "Dead";
        if (plant.IsAlive && plant.WaterLevel < 30)
        {
            status = "Thirsty! 💧";
        }

        footerText.Text = status;
    }

    private void UpdateHerbivoreTooltip(Herbivore herbivore)
    {
        if (headerText == null || headerForeground == null || barAContainer == null || barBContainer == null ||
            barALabelText == null || barAValueText == null || barAFill == null || barAFillBrush == null ||
            barBLabelText == null || barBValueText == null || barBFill == null || barBFillBrush == null ||
            statText1 == null || statText2 == null || statText3 == null || footerSeparator == null || footerText == null)
        {
            return;
        }

        headerText.Text = $"🐰 {herbivore.Type}";
        headerForeground.Color = Color.FromRgb(255, 183, 77);

        barAContainer.Visibility = Visibility.Visible;
        barBContainer.Visibility = Visibility.Visible;
        statText3.Visibility = Visibility.Collapsed;

        UpdateBar(barALabelText, barAValueText, barAFill, barAFillBrush,
            "Health", herbivore.Health, 100, Color.FromRgb(231, 76, 60), Color.FromRgb(46, 204, 113));

        UpdateBar(barBLabelText, barBValueText, barBFill, barBFillBrush,
            "Hunger", 100 - herbivore.Hunger, 100, Color.FromRgb(230, 126, 34), Color.FromRgb(46, 204, 113));

        statText1.Text = $"Speed: {herbivore.Speed:F1}";
        statText2.Text = $"Age: {herbivore.Age:F0}s";

        footerText.Text = herbivore.IsAlive ? GetCreatureStatus(herbivore) : "Dead";
    }

    private void UpdateCarnivoreTooltip(Carnivore carnivore)
    {
        if (headerText == null || headerForeground == null || barAContainer == null || barBContainer == null ||
            barALabelText == null || barAValueText == null || barAFill == null || barAFillBrush == null ||
            barBLabelText == null || barBValueText == null || barBFill == null || barBFillBrush == null ||
            statText1 == null || statText2 == null || statText3 == null || footerSeparator == null || footerText == null)
        {
            return;
        }

        headerText.Text = $"🐺 {carnivore.Type}";
        headerForeground.Color = Color.FromRgb(120, 120, 130);

        barAContainer.Visibility = Visibility.Visible;
        barBContainer.Visibility = Visibility.Visible;
        statText3.Visibility = Visibility.Collapsed;

        UpdateBar(barALabelText, barAValueText, barAFill, barAFillBrush,
            "Health", carnivore.Health, 100, Color.FromRgb(231, 76, 60), Color.FromRgb(46, 204, 113));

        UpdateBar(barBLabelText, barBValueText, barBFill, barBFillBrush,
            "Hunger", 100 - carnivore.Hunger, 100, Color.FromRgb(230, 126, 34), Color.FromRgb(46, 204, 113));

        statText1.Text = $"Speed: {carnivore.Speed:F1}";
        statText2.Text = $"Age: {carnivore.Age:F0}s";

        footerText.Text = carnivore.IsAlive ? GetCreatureStatus(carnivore) : "Dead";
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
            (byte)(from.R + ((to.R - from.R) * ratio)),
            (byte)(from.G + ((to.G - from.G) * ratio)),
            (byte)(from.B + ((to.B - from.B) * ratio)));
    }

    private static SolidColorBrush CreateFrozenBrush(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }
}
