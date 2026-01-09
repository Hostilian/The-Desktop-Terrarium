using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Terrarium.Desktop.Rendering
{
    /// <summary>
    /// Displays a real-time population history graph.
    /// </summary>
    public class PopulationGraph
    {
        private readonly Canvas _parentCanvas;
        private readonly Border _graphContainer;
        private readonly Canvas _graphCanvas;
        private readonly List<PopulationSnapshot> _history;

        // Graph dimensions
        private const double GraphWidth = 200;
        private const double GraphHeight = 80;
        private const double Margin = 10;
        private const int MaxHistoryPoints = 60; // 60 seconds of history
        private const double SampleInterval = 1.0;

        // UI Layout constants
        private const double ContainerWidthPadding = 16;
        private const double ContainerHeightPadding = 50;
        private const double GraphTopOffset = 80; // Below the day/night orb area
        private const double ZIndexGraphContainer = 800;

        // Colors
        private static readonly Color PlantLineColor = Color.FromRgb(76, 175, 80);
        private static readonly Color HerbivoreLineColor = Color.FromRgb(255, 183, 77);
        private static readonly Color CarnivoreLineColor = Color.FromRgb(192, 57, 43);
        private static readonly Color HeaderTextColor = Color.FromRgb(150, 150, 150);
        private static readonly Color GridLineColor = Color.FromArgb(40, 255, 255, 255);
        private static readonly Color GraphBackgroundColor = Color.FromArgb(100, 20, 20, 30);
        private static readonly Color ContainerBackgroundColor = Color.FromArgb(200, 20, 20, 30);
        private static readonly Color ContainerBorderColor = Color.FromArgb(100, 255, 255, 255);

        // Typography
        private const double HeaderFontSize = 10;
        private const double LegendFontSize = 10;

        // Legend constants
        private const double LegendDotSize = 6;
        private const double LegendDotMargin = 3;
        private const double LegendItemSpacing = 8;

        // Graph styling
        private const double LineThickness = 2;
        private const double GlowLineThickness = 4;
        private const byte GlowAlpha = 80;
        private const double GridLineThickness = 1;
        private const int GridLineCount = 3;
        private const double HeadroomMultiplier = 1.2; // Add 20% headroom

        // Margins and padding
        private const double LegendMarginTop = 4;
        private const double LegendMarginLeft = 8;
        private const double InnerGraphMargin = 4;
        private const double ContainerBorderThickness = 1;
        private const double ContainerCornerRadius = 8;
        private const double GraphCornerRadius = 4;

        private double _sampleTimer;
        private bool _isVisible = true;

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                _graphContainer.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public PopulationGraph(Canvas parentCanvas)
        {
            _parentCanvas = parentCanvas;
            _history = new List<PopulationSnapshot>();
            _sampleTimer = 0;

            // Create graph canvas
            _graphCanvas = new Canvas
            {
                Width = GraphWidth,
                Height = GraphHeight,
                ClipToBounds = true,
                Background = Brushes.Transparent
            };

            // Legend
            var legend = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(LegendMarginLeft, LegendMarginTop, 0, 0)
            };
            legend.Children.Add(CreateLegendItem("🌿", PlantLineColor));
            legend.Children.Add(CreateLegendItem("🐰", HerbivoreLineColor));
            legend.Children.Add(CreateLegendItem("🐺", CarnivoreLineColor));

            // Main container
            var content = new StackPanel();
            content.Children.Add(new TextBlock
            {
                Text = "📈 Population",
                FontSize = HeaderFontSize,
                Foreground = new SolidColorBrush(HeaderTextColor),
                Margin = new Thickness(LegendMarginLeft, LegendMarginTop, 0, 0),
                FontWeight = FontWeights.SemiBold
            });
            content.Children.Add(legend);
            content.Children.Add(new Border
            {
                Child = _graphCanvas,
                Margin = new Thickness(InnerGraphMargin),
                Background = new SolidColorBrush(GraphBackgroundColor),
                CornerRadius = new CornerRadius(GraphCornerRadius)
            });

            _graphContainer = new Border
            {
                Width = GraphWidth + ContainerWidthPadding,
                Height = GraphHeight + ContainerHeightPadding,
                Background = new SolidColorBrush(ContainerBackgroundColor),
                BorderBrush = new SolidColorBrush(ContainerBorderColor),
                BorderThickness = new Thickness(ContainerBorderThickness),
                CornerRadius = new CornerRadius(ContainerCornerRadius),
                Child = content
            };

            // Position in top-right corner
            Canvas.SetZIndex(_graphContainer, ZIndexGraphContainer);
            _parentCanvas.Children.Add(_graphContainer);
            _parentCanvas.SizeChanged += (s, e) => UpdatePosition();
            UpdatePosition();
        }

        private UIElement CreateLegendItem(string emoji, Color color)
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, LegendItemSpacing, 0) };
            panel.Children.Add(new Ellipse
            {
                Width = LegendDotSize,
                Height = LegendDotSize,
                Fill = new SolidColorBrush(color),
                Margin = new Thickness(0, 0, LegendDotMargin, 0),
                VerticalAlignment = VerticalAlignment.Center
            });
            panel.Children.Add(new TextBlock
            {
                Text = emoji,
                FontSize = LegendFontSize,
                VerticalAlignment = VerticalAlignment.Center
            });
            return panel;
        }

        private void UpdatePosition()
        {
            double x = _parentCanvas.ActualWidth - GraphWidth - ContainerWidthPadding - Margin;
            Canvas.SetLeft(_graphContainer, Math.Max(Margin, x));
            Canvas.SetTop(_graphContainer, GraphTopOffset); // Below the day/night orb area
        }

        /// <summary>
        /// Updates the graph with current population data.
        /// </summary>
        public void Update(double deltaTime, int plants, int herbivores, int carnivores)
        {
            if (!_isVisible)
                return;

            _sampleTimer += deltaTime;
            if (_sampleTimer >= SampleInterval)
            {
                _sampleTimer = 0;
                _history.Add(new PopulationSnapshot
                {
                    Plants = plants,
                    Herbivores = herbivores,
                    Carnivores = carnivores
                });

                // Trim history
                while (_history.Count > MaxHistoryPoints)
                {
                    _history.RemoveAt(0);
                }

                RedrawGraph();
            }
        }

        private void RedrawGraph()
        {
            _graphCanvas.Children.Clear();

            if (_history.Count < 2)
                return;

            // Find max value for scaling
            int maxValue = 1;
            foreach (var snapshot in _history)
            {
                maxValue = Math.Max(maxValue, Math.Max(snapshot.Plants, Math.Max(snapshot.Herbivores, snapshot.Carnivores)));
            }
            maxValue = (int)(maxValue * HeadroomMultiplier); // Add headroom

            // Draw grid lines (evenly spaced, excluding top edge)
            for (int i = 1; i <= GridLineCount; i++)
            {
                // Divide by (GridLineCount + 1) to create even spacing without line at top edge
                double y = GraphHeight - (GraphHeight * i / (GridLineCount + 1));
                var gridLine = new Line
                {
                    X1 = 0,
                    Y1 = y,
                    X2 = GraphWidth,
                    Y2 = y,
                    Stroke = new SolidColorBrush(GridLineColor),
                    StrokeThickness = GridLineThickness
                };
                _graphCanvas.Children.Add(gridLine);
            }

            // Draw lines for each population type
            DrawPopulationLine(_history, s => s.Plants, PlantLineColor, maxValue);
            DrawPopulationLine(_history, s => s.Herbivores, HerbivoreLineColor, maxValue);
            DrawPopulationLine(_history, s => s.Carnivores, CarnivoreLineColor, maxValue);
        }

        private void DrawPopulationLine(List<PopulationSnapshot> history, Func<PopulationSnapshot, int> getValue, Color color, int maxValue)
        {
            if (history.Count < 2)
                return;

            var points = new PointCollection();
            double xStep = GraphWidth / (MaxHistoryPoints - 1);

            for (int i = 0; i < history.Count; i++)
            {
                double x = i * xStep;
                double y = GraphHeight - (GraphHeight * getValue(history[i]) / maxValue);
                points.Add(new Point(x, Math.Max(0, Math.Min(GraphHeight, y))));
            }

            var polyline = new Polyline
            {
                Points = points,
                Stroke = new SolidColorBrush(color),
                StrokeThickness = LineThickness,
                StrokeLineJoin = PenLineJoin.Round
            };

            _graphCanvas.Children.Add(polyline);

            // Add glow effect for better visibility
            var glowPolyline = new Polyline
            {
                Points = points,
                Stroke = new SolidColorBrush(Color.FromArgb(GlowAlpha, color.R, color.G, color.B)),
                StrokeThickness = GlowLineThickness,
                StrokeLineJoin = PenLineJoin.Round
            };
            _graphCanvas.Children.Insert(0, glowPolyline);
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
}
