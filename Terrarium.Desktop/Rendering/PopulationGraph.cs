namespace Terrarium.Desktop.Rendering;

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

/// <summary>
/// Displays a real-time population history graph.
/// </summary>
public class PopulationGraph
{
    private readonly Canvas parentCanvas;
    private readonly Border graphContainer;
    private readonly Canvas graphCanvas;
    private readonly List<PopulationSnapshot> history;

    private readonly List<Line> gridLines = new();
    private bool gridBuilt;
    private readonly SolidColorBrush gridLineBrush = CreateFrozenBrush(Color.FromArgb(40, 255, 255, 255));

    private Polyline? plantsGlowLine;
    private Polyline? plantsLine;
    private Polyline? herbGlowLine;
    private Polyline? herbLine;
    private Polyline? carnGlowLine;
    private Polyline? carnLine;

    private PointCollection? plantsPoints;
    private PointCollection? herbPoints;
    private PointCollection? carnPoints;

    private const double GraphWidth = 200;
    private const double GraphHeight = 80;
    private const double Margin = 10;
    private const int MaxHistoryPoints = 60; // 60 seconds of history
    private const double SampleInterval = 1.0;

    private double sampleTimer;
    private bool isVisible = true;

    public bool IsVisible
    {
        get => isVisible;
        set
        {
            isVisible = value;
            graphContainer.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    public PopulationGraph(Canvas parentCanvas)
    {
        this.parentCanvas = parentCanvas;
        history = new List<PopulationSnapshot>();
        sampleTimer = 0;

        graphCanvas = new Canvas
        {
            Width = GraphWidth,
            Height = GraphHeight,
            ClipToBounds = true,
            Background = Brushes.Transparent
        };

        var legend = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(8, 4, 0, 0)
        };
        legend.Children.Add(CreateLegendItem("🌿", Color.FromRgb(76, 175, 80)));
        legend.Children.Add(CreateLegendItem("🐰", Color.FromRgb(255, 183, 77)));
        legend.Children.Add(CreateLegendItem("🐺", Color.FromRgb(192, 57, 43)));

        var content = new StackPanel();
        content.Children.Add(new TextBlock
        {
            Text = "📈 Population",
            FontSize = 10,
            Foreground = CreateFrozenBrush(Color.FromRgb(150, 150, 150)),
            Margin = new Thickness(8, 4, 0, 0),
            FontWeight = FontWeights.SemiBold
        });
        content.Children.Add(legend);
        content.Children.Add(new Border
        {
            Child = graphCanvas,
            Margin = new Thickness(4),
            Background = CreateFrozenBrush(Color.FromArgb(100, 20, 20, 30)),
            CornerRadius = new CornerRadius(4)
        });

        graphContainer = new Border
        {
            Width = GraphWidth + 16,
            Height = GraphHeight + 50,
            Background = CreateFrozenBrush(Color.FromArgb(200, 20, 20, 30)),
            BorderBrush = CreateFrozenBrush(Color.FromArgb(100, 255, 255, 255)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Child = content
        };

        Canvas.SetZIndex(graphContainer, 800);
        this.parentCanvas.Children.Add(graphContainer);
        this.parentCanvas.SizeChanged += (s, e) => UpdatePosition();
        UpdatePosition();

        EnsureGraphVisuals();
    }

    private UIElement CreateLegendItem(string emoji, Color color)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 8, 0) };
        panel.Children.Add(new Ellipse
        {
            Width = 6,
            Height = 6,
            Fill = CreateFrozenBrush(color),
            Margin = new Thickness(0, 0, 3, 0),
            VerticalAlignment = VerticalAlignment.Center
        });
        panel.Children.Add(new TextBlock
        {
            Text = emoji,
            FontSize = 10,
            VerticalAlignment = VerticalAlignment.Center
        });
        return panel;
    }

    private void UpdatePosition()
    {
        double x = parentCanvas.ActualWidth - GraphWidth - 16 - Margin;
        Canvas.SetLeft(graphContainer, Math.Max(Margin, x));
        Canvas.SetTop(graphContainer, 80); // Below the day/night orb area
    }

    /// <summary>
    /// Updates the graph with current population data.
    /// </summary>
    public void Update(double deltaTime, int plants, int herbivores, int carnivores)
    {
        if (!isVisible)
        {
            return;
        }

        sampleTimer += deltaTime;
        if (sampleTimer >= SampleInterval)
        {
            sampleTimer = 0;
            history.Add(new PopulationSnapshot
            {
                Plants = plants,
                Herbivores = herbivores,
                Carnivores = carnivores
            });

            // Trim history
            while (history.Count > MaxHistoryPoints)
            {
                history.RemoveAt(0);
            }

            RedrawGraph();
        }
    }

    private void RedrawGraph()
    {
        EnsureGraphVisuals();

        if (history.Count < 2)
        {
            this.HideLines();
            return;
        }

        // Find max value for scaling
        int maxValue = 1;
        foreach (var snapshot in history)
        {
            maxValue = Math.Max(maxValue, Math.Max(snapshot.Plants, Math.Max(snapshot.Herbivores, snapshot.Carnivores)));
        }
        maxValue = (int)(maxValue * 1.2); // Add 20% headroom

        UpdateLine(history, s => s.Plants, plantsGlowLine!, plantsLine!, plantsPoints!, maxValue);
        UpdateLine(history, s => s.Herbivores, herbGlowLine!, herbLine!, herbPoints!, maxValue);
        UpdateLine(history, s => s.Carnivores, carnGlowLine!, carnLine!, carnPoints!, maxValue);
    }

    private void EnsureGraphVisuals()
    {
        if (!gridBuilt)
        {
            // Draw grid lines once
            for (int i = 1; i <= 3; i++)
            {
                double y = GraphHeight - (GraphHeight * i / 4);
                var gridLine = new Line
                {
                    X1 = 0,
                    Y1 = y,
                    X2 = GraphWidth,
                    Y2 = y,
                    Stroke = gridLineBrush,
                    StrokeThickness = 1,
                    IsHitTestVisible = false
                };
                gridLines.Add(gridLine);
                graphCanvas.Children.Add(gridLine);
            }
            gridBuilt = true;
        }

        if (plantsLine != null)
        {
            return;
        }

        plantsPoints = new PointCollection(MaxHistoryPoints);
        herbPoints = new PointCollection(MaxHistoryPoints);
        carnPoints = new PointCollection(MaxHistoryPoints);

        plantsGlowLine = CreateLine(Color.FromArgb(80, 76, 175, 80), plantsPoints, 4);
        plantsLine = CreateLine(Color.FromRgb(76, 175, 80), plantsPoints, 2);
        herbGlowLine = CreateLine(Color.FromArgb(80, 255, 183, 77), herbPoints, 4);
        herbLine = CreateLine(Color.FromRgb(255, 183, 77), herbPoints, 2);
        carnGlowLine = CreateLine(Color.FromArgb(80, 192, 57, 43), carnPoints, 4);
        carnLine = CreateLine(Color.FromRgb(192, 57, 43), carnPoints, 2);

        // Order: grid, glows, lines
        graphCanvas.Children.Add(plantsGlowLine);
        graphCanvas.Children.Add(herbGlowLine);
        graphCanvas.Children.Add(carnGlowLine);
        graphCanvas.Children.Add(plantsLine);
        graphCanvas.Children.Add(herbLine);
        graphCanvas.Children.Add(carnLine);
    }

    private static Polyline CreateLine(Color strokeColor, PointCollection points, double thickness)
    {
        var brush = new SolidColorBrush(strokeColor);
        brush.Freeze();
        return new Polyline
        {
            Points = points,
            Stroke = brush,
            StrokeThickness = thickness,
            StrokeLineJoin = PenLineJoin.Round,
            IsHitTestVisible = false,
            Visibility = Visibility.Collapsed
        };
    }

    private void HideLines()
    {
        if (plantsLine != null)
        {
            plantsLine.Visibility = Visibility.Collapsed;
        }

        if (plantsGlowLine != null)
        {
            plantsGlowLine.Visibility = Visibility.Collapsed;
        }

        if (herbLine != null)
        {
            herbLine.Visibility = Visibility.Collapsed;
        }

        if (herbGlowLine != null)
        {
            herbGlowLine.Visibility = Visibility.Collapsed;
        }

        if (carnLine != null)
        {
            carnLine.Visibility = Visibility.Collapsed;
        }

        if (carnGlowLine != null)
        {
            carnGlowLine.Visibility = Visibility.Collapsed;
        }
    }

    private static void UpdateLine(
        List<PopulationSnapshot> history,
        Func<PopulationSnapshot, int> getValue,
        Polyline glowLine,
        Polyline line,
        PointCollection points,
        int maxValue)
    {
        points.Clear();
        double xStep = GraphWidth / (MaxHistoryPoints - 1);

        for (int i = 0; i < history.Count; i++)
        {
            double x = i * xStep;
            double y = GraphHeight - (GraphHeight * getValue(history[i]) / maxValue);
            points.Add(new Point(x, Math.Max(0, Math.Min(GraphHeight, y))));
        }

        glowLine.Visibility = Visibility.Visible;
        line.Visibility = Visibility.Visible;
    }

    private static SolidColorBrush CreateFrozenBrush(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }

    /// <summary>
    /// Toggles graph visibility.
    /// </summary>
    public void Toggle()
    {
        IsVisible = !IsVisible;
    }
}

internal class PopulationSnapshot
{
    public int Plants { get; set; }

    public int Herbivores { get; set; }

    public int Carnivores { get; set; }
}
