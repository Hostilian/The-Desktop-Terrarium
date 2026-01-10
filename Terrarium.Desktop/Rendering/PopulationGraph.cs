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

        private readonly List<Line> _gridLines = new();
        private bool _gridBuilt;
        private readonly SolidColorBrush _gridLineBrush = CreateFrozenBrush(Color.FromArgb(40, 255, 255, 255));

        private Polyline? _plantsGlowLine;
        private Polyline? _plantsLine;
        private Polyline? _herbGlowLine;
        private Polyline? _herbLine;
        private Polyline? _carnGlowLine;
        private Polyline? _carnLine;

        private PointCollection? _plantsPoints;
        private PointCollection? _herbPoints;
        private PointCollection? _carnPoints;

        private const double GraphWidth = 200;
        private const double GraphHeight = 80;
        private const double Margin = 10;
        private const int MaxHistoryPoints = 60; // 60 seconds of history
        private const double SampleInterval = 1.0;

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
                Margin = new Thickness(8, 4, 0, 0)
            };
            legend.Children.Add(CreateLegendItem("🌿", Color.FromRgb(76, 175, 80)));
            legend.Children.Add(CreateLegendItem("🐰", Color.FromRgb(255, 183, 77)));
            legend.Children.Add(CreateLegendItem("🐺", Color.FromRgb(192, 57, 43)));

            // Main container
            var content = new StackPanel();
            content.Children.Add(new TextBlock
            {
                Text = "📈 Population",
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromRgb(150, 150, 150)),
                Margin = new Thickness(8, 4, 0, 0),
                FontWeight = FontWeights.SemiBold
            });
            content.Children.Add(legend);
            content.Children.Add(new Border
            {
                Child = _graphCanvas,
                Margin = new Thickness(4),
                Background = new SolidColorBrush(Color.FromArgb(100, 20, 20, 30)),
                CornerRadius = new CornerRadius(4)
            });

            _graphContainer = new Border
            {
                Width = GraphWidth + 16,
                Height = GraphHeight + 50,
                Background = new SolidColorBrush(Color.FromArgb(200, 20, 20, 30)),
                BorderBrush = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Child = content
            };

            // Position in top-right corner
            Canvas.SetZIndex(_graphContainer, 800);
            _parentCanvas.Children.Add(_graphContainer);
            _parentCanvas.SizeChanged += (s, e) => UpdatePosition();
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
                Fill = new SolidColorBrush(color),
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
            double x = _parentCanvas.ActualWidth - GraphWidth - 16 - Margin;
            Canvas.SetLeft(_graphContainer, Math.Max(Margin, x));
            Canvas.SetTop(_graphContainer, 80); // Below the day/night orb area
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
            EnsureGraphVisuals();

            if (_history.Count < 2)
            {
                HideLines();
                return;
            }

            // Find max value for scaling
            int maxValue = 1;
            foreach (var snapshot in _history)
            {
                maxValue = Math.Max(maxValue, Math.Max(snapshot.Plants, Math.Max(snapshot.Herbivores, snapshot.Carnivores)));
            }
            maxValue = (int)(maxValue * 1.2); // Add 20% headroom

            UpdateLine(_history, s => s.Plants, _plantsGlowLine!, _plantsLine!, _plantsPoints!, maxValue);
            UpdateLine(_history, s => s.Herbivores, _herbGlowLine!, _herbLine!, _herbPoints!, maxValue);
            UpdateLine(_history, s => s.Carnivores, _carnGlowLine!, _carnLine!, _carnPoints!, maxValue);
        }

        private void EnsureGraphVisuals()
        {
            if (!_gridBuilt)
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
                        Stroke = _gridLineBrush,
                        StrokeThickness = 1,
                        IsHitTestVisible = false
                    };
                    _gridLines.Add(gridLine);
                    _graphCanvas.Children.Add(gridLine);
                }
                _gridBuilt = true;
            }

            if (_plantsLine != null)
                return;

            _plantsPoints = new PointCollection(MaxHistoryPoints);
            _herbPoints = new PointCollection(MaxHistoryPoints);
            _carnPoints = new PointCollection(MaxHistoryPoints);

            _plantsGlowLine = CreateLine(Color.FromArgb(80, 76, 175, 80), _plantsPoints, 4);
            _plantsLine = CreateLine(Color.FromRgb(76, 175, 80), _plantsPoints, 2);
            _herbGlowLine = CreateLine(Color.FromArgb(80, 255, 183, 77), _herbPoints, 4);
            _herbLine = CreateLine(Color.FromRgb(255, 183, 77), _herbPoints, 2);
            _carnGlowLine = CreateLine(Color.FromArgb(80, 192, 57, 43), _carnPoints, 4);
            _carnLine = CreateLine(Color.FromRgb(192, 57, 43), _carnPoints, 2);

            // Order: grid, glows, lines
            _graphCanvas.Children.Add(_plantsGlowLine);
            _graphCanvas.Children.Add(_herbGlowLine);
            _graphCanvas.Children.Add(_carnGlowLine);
            _graphCanvas.Children.Add(_plantsLine);
            _graphCanvas.Children.Add(_herbLine);
            _graphCanvas.Children.Add(_carnLine);
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
            if (_plantsLine != null)
                _plantsLine.Visibility = Visibility.Collapsed;
            if (_plantsGlowLine != null)
                _plantsGlowLine.Visibility = Visibility.Collapsed;
            if (_herbLine != null)
                _herbLine.Visibility = Visibility.Collapsed;
            if (_herbGlowLine != null)
                _herbGlowLine.Visibility = Visibility.Collapsed;
            if (_carnLine != null)
                _carnLine.Visibility = Visibility.Collapsed;
            if (_carnGlowLine != null)
                _carnGlowLine.Visibility = Visibility.Collapsed;
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
}
