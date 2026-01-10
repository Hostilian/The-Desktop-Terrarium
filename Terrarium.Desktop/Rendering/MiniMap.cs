using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Terrarium.Logic.Entities;
using Terrarium.Logic.Simulation;

namespace Terrarium.Desktop.Rendering
{
    /// <summary>
    /// Mini-map showing a bird's eye view of the entire ecosystem.
    /// </summary>
    public class MiniMap
    {
        private readonly Canvas _parentCanvas;
        private readonly Border _mapBorder;
        private readonly Canvas _mapCanvas;
        private readonly Border _viewportIndicator;

        private readonly List<Line> _gridLines = new();
        private readonly List<Ellipse> _dotPool = new();
        private int _activeDotCount;

        private readonly SolidColorBrush _gridBrush = CreateFrozenBrush(Color.FromArgb(30, 255, 255, 255));
        private readonly SolidColorBrush _deadBrush = CreateFrozenBrush(Color.FromRgb(80, 80, 80));
        private readonly SolidColorBrush _herbivoreBrush = CreateFrozenBrush(Color.FromRgb(255, 183, 77));
        private readonly SolidColorBrush _hungryBrush = CreateFrozenBrush(Color.FromRgb(231, 76, 60));
        private readonly SolidColorBrush _carnivoreBrush = CreateFrozenBrush(Color.FromRgb(192, 57, 43));

        private readonly Dictionary<int, SolidColorBrush> _brushCache = new();
        private const int BrushCacheMax = 96;
        private bool _gridBuilt;
        private double _gridScaleX;
        private double _gridScaleY;
        private double _lastWorldWidth;
        private double _lastWorldHeight;

        private const double MapWidth = 160;
        private const double MapHeight = 100;
        private const double Margin = 10;

        private bool _isVisible = true;
        private double _worldWidth;
        private double _worldHeight;

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                _mapBorder.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public MiniMap(Canvas parentCanvas)
        {
            _parentCanvas = parentCanvas;

            // Create map canvas
            _mapCanvas = new Canvas
            {
                Width = MapWidth,
                Height = MapHeight,
                ClipToBounds = true
            };

            // Viewport indicator (shows current view area)
            _viewportIndicator = new Border
            {
                BorderBrush = CreateFrozenBrush(Color.FromArgb(180, 255, 255, 255)),
                BorderThickness = new Thickness(1.5),
                Background = CreateFrozenBrush(Color.FromArgb(30, 255, 255, 255)),
                CornerRadius = new CornerRadius(2)
            };

            // Main border
            _mapBorder = new Border
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
                            Child = _mapCanvas,
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

            // Position in bottom-left corner
            Canvas.SetZIndex(_mapBorder, 800);
            _parentCanvas.Children.Add(_mapBorder);

            _parentCanvas.SizeChanged += (s, e) => UpdatePosition();
            UpdatePosition();
        }

        private void UpdatePosition()
        {
            Canvas.SetLeft(_mapBorder, Margin);
            Canvas.SetBottom(_mapBorder, Margin);
            Canvas.SetTop(_mapBorder, _parentCanvas.ActualHeight - MapHeight - 24 - Margin);
        }

        /// <summary>
        /// Updates the mini-map with current world state.
        /// </summary>
        public void Update(World world, double viewportWidth, double viewportHeight)
        {
            if (!_isVisible)
                return;

            _worldWidth = world.Width;
            _worldHeight = world.Height;

            // Calculate scale
            double scaleX = MapWidth / _worldWidth;
            double scaleY = MapHeight / _worldHeight;

            EnsureTerrainGrid(scaleX, scaleY);
            UpdateEntityDots(world, scaleX, scaleY);

            // Draw viewport indicator
            UpdateViewportIndicator(viewportWidth, viewportHeight, scaleX, scaleY);
        }

        private void EnsureTerrainGrid(double scaleX, double scaleY)
        {
            bool dimsChanged = _lastWorldWidth != _worldWidth || _lastWorldHeight != _worldHeight;
            bool scaleChanged = Math.Abs(_gridScaleX - scaleX) > 0.0001 || Math.Abs(_gridScaleY - scaleY) > 0.0001;

            if (_gridBuilt && !dimsChanged && !scaleChanged)
                return;

            foreach (var line in _gridLines)
            {
                _mapCanvas.Children.Remove(line);
            }
            _gridLines.Clear();

            int gridSize = 100;
            for (int x = 0; x < _worldWidth; x += gridSize)
            {
                var line = new Line
                {
                    X1 = x * scaleX,
                    Y1 = 0,
                    X2 = x * scaleX,
                    Y2 = MapHeight,
                    Stroke = _gridBrush,
                    StrokeThickness = 0.5,
                    IsHitTestVisible = false
                };
                _gridLines.Add(line);
                _mapCanvas.Children.Add(line);
            }

            for (int y = 0; y < _worldHeight; y += gridSize)
            {
                var line = new Line
                {
                    X1 = 0,
                    Y1 = y * scaleY,
                    X2 = MapWidth,
                    Y2 = y * scaleY,
                    Stroke = _gridBrush,
                    StrokeThickness = 0.5,
                    IsHitTestVisible = false
                };
                _gridLines.Add(line);
                _mapCanvas.Children.Add(line);
            }

            _gridBuilt = true;
            _gridScaleX = scaleX;
            _gridScaleY = scaleY;
            _lastWorldWidth = _worldWidth;
            _lastWorldHeight = _worldHeight;
        }

        private void UpdateEntityDots(World world, double scaleX, double scaleY)
        {
            _activeDotCount = 0;

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

            for (int i = _activeDotCount; i < _dotPool.Count; i++)
            {
                _dotPool[i].Visibility = Visibility.Collapsed;
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
            Canvas.SetLeft(dot, plant.X * scaleX - dotSize / 2);
            Canvas.SetTop(dot, plant.Y * scaleY - dotSize / 2);
        }

        private void UpdateHerbivoreDot(Herbivore herbivore, double scaleX, double scaleY)
        {
            double dotSize = 4;
            Brush brush;

            if (!herbivore.IsAlive)
                brush = _deadBrush;
            else if (herbivore.Hunger > 70)
                brush = _hungryBrush;
            else
                brush = _herbivoreBrush;

            var dot = GetDot(dotSize, brush);
            Canvas.SetLeft(dot, herbivore.X * scaleX - dotSize / 2);
            Canvas.SetTop(dot, herbivore.Y * scaleY - dotSize / 2);
        }

        private void UpdateCarnivoreDot(Carnivore carnivore, double scaleX, double scaleY)
        {
            double dotSize = 5;
            Brush brush = carnivore.IsAlive ? _carnivoreBrush : _deadBrush;

            var dot = GetDot(dotSize, brush);
            Canvas.SetLeft(dot, carnivore.X * scaleX - dotSize / 2);
            Canvas.SetTop(dot, carnivore.Y * scaleY - dotSize / 2);
        }

        private Ellipse GetDot(double dotSize, Brush fill)
        {
            Ellipse dot;
            if (_activeDotCount < _dotPool.Count)
            {
                dot = _dotPool[_activeDotCount];
            }
            else
            {
                dot = new Ellipse { IsHitTestVisible = false };
                _dotPool.Add(dot);
                _mapCanvas.Children.Add(dot);
            }

            dot.Width = dotSize;
            dot.Height = dotSize;
            dot.Fill = fill;
            dot.Visibility = Visibility.Visible;

            _activeDotCount++;
            return dot;
        }

        private SolidColorBrush GetCachedBrush(Color color)
        {
            int key = (color.R << 16) | (color.G << 8) | color.B;
            if (_brushCache.TryGetValue(key, out var brush))
                return brush;

            if (_brushCache.Count >= BrushCacheMax)
            {
                _brushCache.Clear();
            }

            brush = CreateFrozenBrush(color);
            _brushCache[key] = brush;
            return brush;
        }

        private void UpdateViewportIndicator(double viewportWidth, double viewportHeight, double scaleX, double scaleY)
        {
            // For now, assume viewport shows the entire world
            // In future, this could show a scrollable view area
            double indicatorWidth = Math.Min(viewportWidth, _worldWidth) * scaleX;
            double indicatorHeight = Math.Min(viewportHeight, _worldHeight) * scaleY;

            _viewportIndicator.Width = indicatorWidth;
            _viewportIndicator.Height = indicatorHeight;

            // Center the viewport indicator
            double offsetX = (MapWidth - indicatorWidth) / 2;
            double offsetY = (MapHeight - indicatorHeight) / 2;

            Canvas.SetLeft(_viewportIndicator, offsetX);
            Canvas.SetTop(_viewportIndicator, offsetY);

            // Only add if not already present
            if (!_mapCanvas.Children.Contains(_viewportIndicator))
            {
                _mapCanvas.Children.Add(_viewportIndicator);
                Panel.SetZIndex(_viewportIndicator, 999);
            }
        }

        private static SolidColorBrush CreateFrozenBrush(Color color)
        {
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }

        private static T CreateFrozenBrush<T>(T brush) where T : Freezable
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
}
