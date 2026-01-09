using System;
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
                BorderBrush = new SolidColorBrush(Color.FromArgb(180, 255, 255, 255)),
                BorderThickness = new Thickness(1.5),
                Background = new SolidColorBrush(Color.FromArgb(30, 255, 255, 255)),
                CornerRadius = new CornerRadius(2)
            };

            // Main border
            _mapBorder = new Border
            {
                Width = MapWidth + 4,
                Height = MapHeight + 24,
                Background = new SolidColorBrush(Color.FromArgb(200, 20, 20, 30)),
                BorderBrush = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255)),
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
                            Foreground = new SolidColorBrush(Color.FromRgb(150, 150, 150)),
                            Margin = new Thickness(8, 4, 0, 2),
                            FontWeight = FontWeights.SemiBold
                        },
                        new Border
                        {
                            Child = _mapCanvas,
                            Margin = new Thickness(2, 0, 2, 2),
                            Background = new LinearGradientBrush(
                                Color.FromRgb(34, 50, 34),
                                Color.FromRgb(28, 42, 28),
                                90),
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
            if (!_isVisible) return;

            _worldWidth = world.Width;
            _worldHeight = world.Height;

            // Clear previous frame
            _mapCanvas.Children.Clear();

            // Calculate scale
            double scaleX = MapWidth / _worldWidth;
            double scaleY = MapHeight / _worldHeight;

            // Draw terrain features (subtle grid)
            DrawTerrainGrid(scaleX, scaleY);

            // Draw entities
            foreach (var entity in world.GetAllEntities())
            {
                DrawEntityDot(entity, scaleX, scaleY);
            }

            // Draw viewport indicator
            UpdateViewportIndicator(viewportWidth, viewportHeight, scaleX, scaleY);
        }

        private void DrawTerrainGrid(double scaleX, double scaleY)
        {
            var gridBrush = new SolidColorBrush(Color.FromArgb(30, 255, 255, 255));

            // Draw subtle grid lines
            int gridSize = 100;
            for (int x = 0; x < _worldWidth; x += gridSize)
            {
                var line = new Line
                {
                    X1 = x * scaleX,
                    Y1 = 0,
                    X2 = x * scaleX,
                    Y2 = MapHeight,
                    Stroke = gridBrush,
                    StrokeThickness = 0.5
                };
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
                    Stroke = gridBrush,
                    StrokeThickness = 0.5
                };
                _mapCanvas.Children.Add(line);
            }
        }

        private void DrawEntityDot(WorldEntity entity, double scaleX, double scaleY)
        {
            double dotSize;
            Brush dotBrush;

            if (entity is Plant plant)
            {
                dotSize = Math.Max(2, plant.Size * 0.3);
                // Green with varying intensity based on health
                double intensity = plant.IsAlive ? 0.5 + (plant.Health / 200.0) : 0.2;
                dotBrush = new SolidColorBrush(Color.FromRgb(
                    (byte)(76 * intensity),
                    (byte)(175 * intensity),
                    (byte)(80 * intensity)));
            }
            else if (entity is Herbivore herbivore)
            {
                dotSize = 4; // Fixed size for herbivores
                // Orange/yellow
                if (!herbivore.IsAlive)
                {
                    dotBrush = new SolidColorBrush(Color.FromRgb(80, 80, 80));
                }
                else if (herbivore.Hunger > 70)
                {
                    // Pulsing red when hungry
                    dotBrush = new SolidColorBrush(Color.FromRgb(231, 76, 60));
                }
                else
                {
                    dotBrush = new SolidColorBrush(Color.FromRgb(255, 183, 77));
                }
            }
            else if (entity is Carnivore carnivore)
            {
                dotSize = 5; // Fixed size for carnivores (slightly bigger)
                // Red/gray
                if (!carnivore.IsAlive)
                {
                    dotBrush = new SolidColorBrush(Color.FromRgb(80, 80, 80));
                }
                else
                {
                    dotBrush = new SolidColorBrush(Color.FromRgb(192, 57, 43));
                }
            }
            else
            {
                dotSize = 2;
                dotBrush = Brushes.White;
            }

            var dot = new Ellipse
            {
                Width = dotSize,
                Height = dotSize,
                Fill = dotBrush
            };

            Canvas.SetLeft(dot, entity.X * scaleX - dotSize / 2);
            Canvas.SetTop(dot, entity.Y * scaleY - dotSize / 2);

            _mapCanvas.Children.Add(dot);
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
            }
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
