namespace Terrarium.Desktop.Rendering;

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Terrarium.Logic.Entities;
using Terrarium.Logic.Simulation;

/// <summary>
/// Mini-map showing a bird's eye view of the entire ecosystem.
/// </summary>
public class MiniMap
{
    private readonly Canvas parentCanvas;
    private readonly Border mapBorder;
    private readonly Canvas mapCanvas;
    private readonly Border viewportIndicator;

    private readonly List<Line> gridLines = new();
    private readonly List<Ellipse> dotPool = new();
    private int activeDotCount;

    private readonly SolidColorBrush gridBrush = CreateFrozenBrush(Color.FromArgb(30, 255, 255, 255));
    private readonly SolidColorBrush deadBrush = CreateFrozenBrush(Color.FromRgb(80, 80, 80));
    private readonly SolidColorBrush herbivoreBrush = CreateFrozenBrush(Color.FromRgb(255, 183, 77));
    private readonly SolidColorBrush hungryBrush = CreateFrozenBrush(Color.FromRgb(231, 76, 60));
    private readonly SolidColorBrush carnivoreBrush = CreateFrozenBrush(Color.FromRgb(192, 57, 43));

    private readonly Dictionary<int, SolidColorBrush> brushCache = new();
    private const int BrushCacheMax = 96;
    private bool gridBuilt;
    private double gridScaleX;
    private double gridScaleY;
    private double lastWorldWidth;
    private double lastWorldHeight;

    private const double MapWidth = 160;
    private const double MapHeight = 100;
    private const double Margin = 10;

    private bool isVisible = true;
    private double worldWidth;
    private double worldHeight;

    public bool IsVisible
    {
        get => isVisible;
        set
        {
            isVisible = value;
            mapBorder.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    public MiniMap(Canvas parentCanvas)
    {
        this.parentCanvas = parentCanvas;

        mapCanvas = new Canvas
        {
            Width = MapWidth,
            Height = MapHeight,
            ClipToBounds = true
        };

        viewportIndicator = new Border
        {
            BorderBrush = CreateFrozenBrush(Color.FromArgb(180, 255, 255, 255)),
            BorderThickness = new Thickness(1.5),
            Background = CreateFrozenBrush(Color.FromArgb(30, 255, 255, 255)),
            CornerRadius = new CornerRadius(2)
        };

        mapBorder = new Border
        {
            Width = MapWidth + 4,
            Height = MapHeight + 24,
            Background = CreateFrozenBrush(Color.FromArgb(200, 20, 20, 30)),
            BorderBrush = CreateFrozenBrush(Color.FromArgb(100, 255, 255, 255)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Child = new StackPanel
            {
                Children =
                {
                    new TextBlock
                    {
                        Text = "🗺️ Map",
                        FontSize = 10,
                        Foreground = CreateFrozenBrush(Color.FromRgb(150, 150, 150)),
                        Margin = new Thickness(8, 4, 0, 2),
                        FontWeight = FontWeights.SemiBold
                    },
                    new Border
                    {
                        Child = mapCanvas,
                        Margin = new Thickness(2, 0, 2, 2),
                        Background = CreateFrozenBrush(
                            new LinearGradientBrush(
                                Color.FromRgb(34, 50, 34),
                                Color.FromRgb(28, 42, 28),
                                90)),
                        CornerRadius = new CornerRadius(4)
                    }
                }
            }
        };

        Canvas.SetZIndex(mapBorder, 800);
        this.parentCanvas.Children.Add(mapBorder);

        this.parentCanvas.SizeChanged += (s, e) => UpdatePosition();
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        Canvas.SetLeft(mapBorder, Margin);
        Canvas.SetBottom(mapBorder, Margin);
        Canvas.SetTop(mapBorder, parentCanvas.ActualHeight - MapHeight - 24 - Margin);
    }

    /// <summary>
    /// Updates the mini-map with current world state.
    /// </summary>
    public void Update(World world, double viewportWidth, double viewportHeight)
    {
        if (!isVisible)
        {
            return;
        }

        worldWidth = world.Width;
        worldHeight = world.Height;

        double scaleX = MapWidth / worldWidth;
        double scaleY = MapHeight / worldHeight;

        EnsureTerrainGrid(scaleX, scaleY);
        UpdateEntityDots(world, scaleX, scaleY);

        UpdateViewportIndicator(viewportWidth, viewportHeight, scaleX, scaleY);
    }

    private void EnsureTerrainGrid(double scaleX, double scaleY)
    {
        bool dimsChanged = lastWorldWidth != worldWidth || lastWorldHeight != worldHeight;
        bool scaleChanged = Math.Abs(gridScaleX - scaleX) > 0.0001 || Math.Abs(gridScaleY - scaleY) > 0.0001;

        if (gridBuilt && !dimsChanged && !scaleChanged)
        {
            return;
        }

        foreach (var line in gridLines)
        {
            mapCanvas.Children.Remove(line);
        }
        gridLines.Clear();

        int gridSize = 100;
        for (int x = 0; x < worldWidth; x += gridSize)
        {
            var line = new Line
            {
                X1 = x * scaleX,
                Y1 = 0,
                X2 = x * scaleX,
                Y2 = MapHeight,
                Stroke = gridBrush,
                StrokeThickness = 0.5,
                IsHitTestVisible = false
            };
            gridLines.Add(line);
            mapCanvas.Children.Add(line);
        }

        for (int y = 0; y < worldHeight; y += gridSize)
        {
            var line = new Line
            {
                X1 = 0,
                Y1 = y * scaleY,
                X2 = MapWidth,
                Y2 = y * scaleY,
                Stroke = gridBrush,
                StrokeThickness = 0.5,
                IsHitTestVisible = false
            };
            gridLines.Add(line);
            mapCanvas.Children.Add(line);
        }

        gridBuilt = true;
        gridScaleX = scaleX;
        gridScaleY = scaleY;
        lastWorldWidth = worldWidth;
        lastWorldHeight = worldHeight;
    }

    private void UpdateEntityDots(World world, double scaleX, double scaleY)
    {
        activeDotCount = 0;

        foreach (var plant in world.Plants)
        {
            UpdatePlantDot(plant, scaleX, scaleY);
        }

        foreach (var herbivore in world.Herbivores)
        {
            UpdateHerbivoreDot(herbivore, scaleX, scaleY);
        }

        foreach (var carnivore in world.Carnivores)
        {
            UpdateCarnivoreDot(carnivore, scaleX, scaleY);
        }

        for (int i = activeDotCount; i < dotPool.Count; i++)
        {
            dotPool[i].Visibility = Visibility.Collapsed;
        }
    }

    private void UpdatePlantDot(Plant plant, double scaleX, double scaleY)
    {
        double dotSize = Math.Max(2, plant.Size * 0.3);
        double intensity = plant.IsAlive ? 0.5 + (plant.Health / 200.0) : 0.2;
        byte r = (byte)Math.Clamp(76 * intensity, 0, 255);
        byte g = (byte)Math.Clamp(175 * intensity, 0, 255);
        byte b = (byte)Math.Clamp(80 * intensity, 0, 255);
        var brush = GetCachedBrush(Color.FromRgb(r, g, b));

        var dot = GetDot(dotSize, brush);
        Canvas.SetLeft(dot, (plant.X * scaleX) - (dotSize / 2));
        Canvas.SetTop(dot, (plant.Y * scaleY) - (dotSize / 2));
    }

    private void UpdateHerbivoreDot(Herbivore herbivore, double scaleX, double scaleY)
    {
        double dotSize = 4;
        Brush brush;

        if (!herbivore.IsAlive)
        {
            brush = deadBrush;
        }
        else if (herbivore.Hunger > 70)
        {
            brush = hungryBrush;
        }
        else
        {
            brush = herbivoreBrush;
        }

        var dot = GetDot(dotSize, brush);
        Canvas.SetLeft(dot, (herbivore.X * scaleX) - (dotSize / 2));
        Canvas.SetTop(dot, (herbivore.Y * scaleY) - (dotSize / 2));
    }

    private void UpdateCarnivoreDot(Carnivore carnivore, double scaleX, double scaleY)
    {
        double dotSize = 5;
        Brush brush = carnivore.IsAlive ? carnivoreBrush : deadBrush;

        var dot = GetDot(dotSize, brush);
        Canvas.SetLeft(dot, (carnivore.X * scaleX) - (dotSize / 2));
        Canvas.SetTop(dot, (carnivore.Y * scaleY) - (dotSize / 2));
    }

    private Ellipse GetDot(double dotSize, Brush fill)
    {
        Ellipse dot;
        if (activeDotCount < dotPool.Count)
        {
            dot = dotPool[activeDotCount];
        }
        else
        {
            dot = new Ellipse { IsHitTestVisible = false };
            dotPool.Add(dot);
            mapCanvas.Children.Add(dot);
        }

        dot.Width = dotSize;
        dot.Height = dotSize;
        dot.Fill = fill;
        dot.Visibility = Visibility.Visible;

        activeDotCount++;
        return dot;
    }

    private SolidColorBrush GetCachedBrush(Color color)
    {
        int key = (color.R << 16) | (color.G << 8) | color.B;
        if (brushCache.TryGetValue(key, out var brush))
        {
            return brush;
        }

        if (brushCache.Count >= BrushCacheMax)
        {
            brushCache.Clear();
        }

        brush = CreateFrozenBrush(color);
        brushCache[key] = brush;
        return brush;
    }

    private void UpdateViewportIndicator(double viewportWidth, double viewportHeight, double scaleX, double scaleY)
    {
        // For now, assume viewport shows the entire world
        // In future, this could show a scrollable view area
        double indicatorWidth = Math.Min(viewportWidth, worldWidth) * scaleX;
        double indicatorHeight = Math.Min(viewportHeight, worldHeight) * scaleY;

        viewportIndicator.Width = indicatorWidth;
        viewportIndicator.Height = indicatorHeight;

        // Center the viewport indicator
        double offsetX = (MapWidth - indicatorWidth) / 2;
        double offsetY = (MapHeight - indicatorHeight) / 2;

        Canvas.SetLeft(viewportIndicator, offsetX);
        Canvas.SetTop(viewportIndicator, offsetY);

        // Only add if not already present
        if (!mapCanvas.Children.Contains(viewportIndicator))
        {
            mapCanvas.Children.Add(viewportIndicator);
            Panel.SetZIndex(viewportIndicator, 999);
        }
    }

    private static SolidColorBrush CreateFrozenBrush(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }

    private static T CreateFrozenBrush<T>(T brush)
        where T : Freezable
    {
        brush.Freeze();
        return brush;
    }

    /// <summary>
    /// Toggles mini-map visibility.
    /// </summary>
    public void Toggle()
    {
        IsVisible = !IsVisible;
    }
}
