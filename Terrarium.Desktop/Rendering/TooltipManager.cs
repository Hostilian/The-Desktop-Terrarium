using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Terrarium.Logic.Entities;

namespace Terrarium.Desktop.Rendering
{
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

            _tooltipBorder = new Border
            {
                Width = TooltipWidth,
                Background = new SolidColorBrush(Color.FromArgb(240, 30, 30, 46)),
                BorderBrush = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255)),
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

        /// <summary>
        /// Shows tooltip for an entity at the specified position.
        /// </summary>
        public void ShowTooltip(WorldEntity entity, double mouseX, double mouseY)
        {
            if (_tooltipBorder == null || _tooltipContent == null)
                return;

            _currentEntity = entity;
            UpdateTooltipContent(entity);

            // Position tooltip near mouse, but keep on screen
            double x = mouseX + TooltipOffset;
            double y = mouseY + TooltipOffset;

            // Adjust if would go off right edge
            if (x + TooltipWidth > _canvas.ActualWidth)
            {
                x = mouseX - TooltipWidth - TooltipOffset;
            }

            // Adjust if would go off bottom
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
                return;

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
                return;

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
                return;

            _tooltipContent.Children.Clear();

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
            if (_tooltipContent == null)
                return;

            // Title
            AddTooltipHeader($"🌿 Plant Lv.{plant.Level}", Color.FromRgb(76, 175, 80));

            // Stats
            AddStatBar("Health", plant.Health, 100, Color.FromRgb(231, 76, 60), Color.FromRgb(46, 204, 113));
            AddStatBar("Water", plant.WaterLevel, 100, Color.FromRgb(52, 152, 219), Color.FromRgb(52, 152, 219));
            AddStatBar("XP", plant.Experience, 100, Color.FromRgb(155, 89, 182), Color.FromRgb(155, 89, 182));
            AddStatText($"Size: {plant.Size:F1}");
            AddStatText($"Age: {plant.Age:F0}s");

            // Status
            string status = plant.IsAlive ? "Healthy" : "Dead";
            if (plant.IsAlive && plant.WaterLevel < 30)
                status = "Thirsty! 💧";
            AddTooltipFooter(status);
        }

        private void UpdateHerbivoreTooltip(Herbivore herbivore)
        {
            if (_tooltipContent == null)
                return;

            // Title
            AddTooltipHeader($"🐰 {herbivore.Type} Lv.{herbivore.Level}", Color.FromRgb(255, 183, 77));

            // Stats
            AddStatBar("Health", herbivore.Health, 100, Color.FromRgb(231, 76, 60), Color.FromRgb(46, 204, 113));
            AddStatBar("Hunger", 100 - herbivore.Hunger, 100, Color.FromRgb(230, 126, 34), Color.FromRgb(46, 204, 113));
            AddStatBar("XP", herbivore.Experience, 100, Color.FromRgb(155, 89, 182), Color.FromRgb(155, 89, 182));
            AddStatText($"Speed: {herbivore.GetEffectiveSpeed():F1}");
            AddStatText($"Age: {herbivore.Age:F0}s");

            // Status
            string status = herbivore.IsAlive ? GetCreatureStatus(herbivore) : "Dead";
            AddTooltipFooter(status);
        }

        private void UpdateCarnivoreTooltip(Carnivore carnivore)
        {
            if (_tooltipContent == null)
                return;

            // Title
            AddTooltipHeader($"🐺 {carnivore.Type} Lv.{carnivore.Level}", Color.FromRgb(120, 120, 130));

            // Stats
            AddStatBar("Health", carnivore.Health, 100, Color.FromRgb(231, 76, 60), Color.FromRgb(46, 204, 113));
            AddStatBar("Hunger", 100 - carnivore.Hunger, 100, Color.FromRgb(230, 126, 34), Color.FromRgb(46, 204, 113));
            AddStatBar("XP", carnivore.Experience, 100, Color.FromRgb(155, 89, 182), Color.FromRgb(155, 89, 182));
            AddStatText($"Speed: {carnivore.GetEffectiveSpeed():F1}");
            AddStatText($"Age: {carnivore.Age:F0}s");

            // Status  
            string status = carnivore.IsAlive ? GetCreatureStatus(carnivore) : "Dead";
            AddTooltipFooter(status);
        }

        private string GetCreatureStatus(Creature creature)
        {
            if (creature.Hunger > 80)
                return "Starving! 🍖";
            if (creature.Hunger > 50)
                return "Hungry";
            if (creature.Health < 30)
                return "Injured! 💔";
            if (Math.Abs(creature.VelocityX) > 0.1 || Math.Abs(creature.VelocityY) > 0.1)
            {
                return creature is Carnivore ? "Hunting 🎯" : "Foraging 🌿";
            }
            return "Resting 😴";
        }

        private void AddTooltipHeader(string text, Color color)
        {
            if (_tooltipContent == null)
                return;

            var header = new TextBlock
            {
                Text = text,
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(color),
                Margin = new Thickness(0, 0, 0, 8)
            };
            _tooltipContent.Children.Add(header);
        }

        private void AddStatBar(string label, double value, double max, Color lowColor, Color highColor)
        {
            if (_tooltipContent == null)
                return;

            var container = new StackPanel { Margin = new Thickness(0, 2, 0, 2) };

            // Label row
            var labelRow = new Grid();
            labelRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            labelRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var labelText = new TextBlock
            {
                Text = label,
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromRgb(150, 150, 150))
            };

            var valueText = new TextBlock
            {
                Text = $"{value:F0}%",
                FontSize = 10,
                Foreground = Brushes.White
            };
            Grid.SetColumn(valueText, 1);

            labelRow.Children.Add(labelText);
            labelRow.Children.Add(valueText);

            // Progress bar
            double ratio = Math.Clamp(value / max, 0, 1);
            Color barColor = InterpolateColor(lowColor, highColor, ratio);

            var barBackground = new Border
            {
                Height = 6,
                Background = new SolidColorBrush(Color.FromRgb(60, 60, 70)),
                CornerRadius = new CornerRadius(3),
                Margin = new Thickness(0, 2, 0, 0)
            };

            var barFill = new Border
            {
                Height = 6,
                Width = (TooltipWidth - 20) * ratio,
                HorizontalAlignment = HorizontalAlignment.Left,
                Background = new SolidColorBrush(barColor),
                CornerRadius = new CornerRadius(3)
            };

            var barGrid = new Grid { Margin = new Thickness(0, 2, 0, 0) };
            barGrid.Children.Add(barBackground);
            barGrid.Children.Add(barFill);

            container.Children.Add(labelRow);
            container.Children.Add(barGrid);
            _tooltipContent.Children.Add(container);
        }

        private void AddStatText(string text)
        {
            if (_tooltipContent == null)
                return;

            var textBlock = new TextBlock
            {
                Text = text,
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(180, 180, 180)),
                Margin = new Thickness(0, 2, 0, 0)
            };
            _tooltipContent.Children.Add(textBlock);
        }

        private void AddTooltipFooter(string status)
        {
            if (_tooltipContent == null)
                return;

            var separator = new Border
            {
                Height = 1,
                Background = new SolidColorBrush(Color.FromRgb(60, 60, 70)),
                Margin = new Thickness(0, 8, 0, 6)
            };
            _tooltipContent.Children.Add(separator);

            var footer = new TextBlock
            {
                Text = status,
                FontSize = 11,
                FontStyle = FontStyles.Italic,
                Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 200))
            };
            _tooltipContent.Children.Add(footer);
        }

        private static Color InterpolateColor(Color from, Color to, double ratio)
        {
            return Color.FromRgb(
                (byte)(from.R + (to.R - from.R) * ratio),
                (byte)(from.G + (to.G - from.G) * ratio),
                (byte)(from.B + (to.B - from.B) * ratio)
            );
        }
    }
}
