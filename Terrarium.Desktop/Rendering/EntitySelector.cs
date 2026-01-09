using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Terrarium.Logic.Entities;

namespace Terrarium.Desktop.Rendering
{
    /// <summary>
    /// Allows users to select and track individual creatures.
    /// </summary>
    public class EntitySelector
    {
        private readonly Canvas _canvas;
        private LivingEntity? _selectedEntity;
        private Ellipse? _selectionRing;
        private Border? _infoPanel;
        private readonly Dictionary<string, TextBlock> _infoLabels;

        // UI Layout constants
        private const double InfoPanelWidth = 180;
        private const double ZIndexInfoPanel = 900;

        // Colors
        private static readonly Color InfoPanelBackgroundColor = Color.FromArgb(200, 20, 30, 40);
        private static readonly Color InfoPanelBorderColor = Color.FromRgb(80, 200, 255);
        private static readonly Color TitleTextColor = Color.FromRgb(80, 200, 255);
        private static readonly Color HintTextColor = Color.FromRgb(150, 150, 150);

        // Typography
        private const double TitleFontSize = 12;
        private const double InfoLabelFontSize = 10;
        private const double HintFontSize = 9;

        // Margins and padding
        private const double InfoPanelPadding = 10;
        private const double InfoPanelBorderThickness = 2;
        private const double InfoPanelCornerRadius = 8;
        private const double TitleMarginBottom = 8;
        private const double HintMarginTop = 8;

        public bool IsEnabled { get; set; } = true;
        public LivingEntity? SelectedEntity => _selectedEntity;

        public event EventHandler<LivingEntity>? OnEntitySelected;
        public event EventHandler? OnEntityDeselected;

        public EntitySelector(Canvas canvas)
        {
            _canvas = canvas;
            _infoLabels = new Dictionary<string, TextBlock>();
            CreateInfoPanel();
        }

        private void CreateInfoPanel()
        {
            _infoPanel = new Border
            {
                Width = InfoPanelWidth,
                Background = new SolidColorBrush(InfoPanelBackgroundColor),
                BorderBrush = new SolidColorBrush(InfoPanelBorderColor),
                BorderThickness = new Thickness(InfoPanelBorderThickness),
                CornerRadius = new CornerRadius(InfoPanelCornerRadius),
                Padding = new Thickness(InfoPanelPadding),
                Visibility = Visibility.Collapsed
            };

            var stack = new StackPanel();

            // Title
            var title = new TextBlock
            {
                Text = "🔍 Selected Entity",
                FontSize = TitleFontSize,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(TitleTextColor),
                Margin = new Thickness(0, 0, 0, TitleMarginBottom)
            };
            stack.Children.Add(title);
            _infoLabels["title"] = title;

            // Info labels
            string[] labels = { "type", "health", "age", "hunger", "position", "state" };
            foreach (var label in labels)
            {
                var tb = new TextBlock
                {
                    FontSize = InfoLabelFontSize,
                    Foreground = Brushes.White
                };
                stack.Children.Add(tb);
                _infoLabels[label] = tb;
            }

            // Follow button hint
            var hint = new TextBlock
            {
                Text = "Press F to follow • Esc to deselect",
                FontSize = HintFontSize,
                Foreground = new SolidColorBrush(HintTextColor),
                Margin = new Thickness(0, HintMarginTop, 0, 0),
                TextWrapping = TextWrapping.Wrap
            };
            stack.Children.Add(hint);

            _infoPanel.Child = stack;
            Canvas.SetZIndex(_infoPanel, ZIndexInfoPanel);
            _canvas.Children.Add(_infoPanel);
        }

        /// <summary>
        /// Attempts to select an entity at the given position.
        /// </summary>
        public bool TrySelect(double x, double y, IEnumerable<Plant> plants,
                             IEnumerable<Herbivore> herbivores, IEnumerable<Carnivore> carnivores)
        {
            if (!IsEnabled)
                return false;

            const double selectRadius = 20;
            LivingEntity? nearest = null;
            double nearestDist = double.MaxValue;

            // Check creatures first (on top visually)
            foreach (var carnivore in carnivores)
            {
                if (!carnivore.IsAlive)
                    continue;
                double dist = Math.Sqrt(Math.Pow(carnivore.X - x, 2) + Math.Pow(carnivore.Y - y, 2));
                if (dist < selectRadius && dist < nearestDist)
                {
                    nearest = carnivore;
                    nearestDist = dist;
                }
            }

            foreach (var herbivore in herbivores)
            {
                if (!herbivore.IsAlive)
                    continue;
                double dist = Math.Sqrt(Math.Pow(herbivore.X - x, 2) + Math.Pow(herbivore.Y - y, 2));
                if (dist < selectRadius && dist < nearestDist)
                {
                    nearest = herbivore;
                    nearestDist = dist;
                }
            }

            foreach (var plant in plants)
            {
                if (!plant.IsAlive)
                    continue;
                double dist = Math.Sqrt(Math.Pow(plant.X - x, 2) + Math.Pow(plant.Y - y, 2));
                if (dist < selectRadius && dist < nearestDist)
                {
                    nearest = plant;
                    nearestDist = dist;
                }
            }

            if (nearest != null)
            {
                Select(nearest);
                return true;
            }
            else
            {
                Deselect();
                return false;
            }
        }

        /// <summary>
        /// Selects a specific entity.
        /// </summary>
        public void Select(LivingEntity entity)
        {
            Deselect();

            _selectedEntity = entity;

            // Create selection ring
            _selectionRing = new Ellipse
            {
                Width = 40,
                Height = 40,
                Stroke = new SolidColorBrush(Color.FromRgb(80, 200, 255)),
                StrokeThickness = 2,
                StrokeDashArray = new DoubleCollection { 4, 2 },
                Fill = Brushes.Transparent
            };
            Canvas.SetZIndex(_selectionRing, 500);
            _canvas.Children.Add(_selectionRing);

            _infoPanel!.Visibility = Visibility.Visible;

            OnEntitySelected?.Invoke(this, entity);
        }

        /// <summary>
        /// Deselects the current entity.
        /// </summary>
        public void Deselect()
        {
            if (_selectionRing != null)
            {
                _canvas.Children.Remove(_selectionRing);
                _selectionRing = null;
            }

            if (_selectedEntity != null)
            {
                _selectedEntity = null;
                _infoPanel!.Visibility = Visibility.Collapsed;
                OnEntityDeselected?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Updates the selection visuals.
        /// </summary>
        public void Update(double deltaTime)
        {
            if (!IsEnabled || _selectedEntity == null || !_selectedEntity.IsAlive)
            {
                if (_selectedEntity != null && !_selectedEntity.IsAlive)
                {
                    // Entity died, deselect
                    Deselect();
                }
                return;
            }

            // Update selection ring position
            if (_selectionRing != null)
            {
                Canvas.SetLeft(_selectionRing, _selectedEntity.X - 20);
                Canvas.SetTop(_selectionRing, _selectedEntity.Y - 20);

                // Rotate the dashed ring
                if (_selectionRing.RenderTransform is RotateTransform rotate)
                {
                    rotate.Angle += 30 * deltaTime;
                }
                else
                {
                    _selectionRing.RenderTransform = new RotateTransform(0, 20, 20);
                }
            }

            // Update info panel position (follow entity)
            if (_infoPanel != null)
            {
                Canvas.SetLeft(_infoPanel, _selectedEntity.X + 30);
                Canvas.SetTop(_infoPanel, _selectedEntity.Y - 60);
            }

            // Update info labels
            UpdateInfoLabels();
        }

        private void UpdateInfoLabels()
        {
            if (_selectedEntity == null)
                return;

            string entityType = _selectedEntity switch
            {
                Carnivore _ => "🔴 Carnivore",
                Herbivore _ => "🟢 Herbivore",
                Plant _ => "🌿 Plant",
                _ => "Unknown"
            };

            _infoLabels["title"].Text = entityType;
            _infoLabels["type"].Text = $"ID: #{_selectedEntity.GetHashCode() % 10000:D4}";
            _infoLabels["health"].Text = $"❤️ Health: {_selectedEntity.Health:F0}/100";
            _infoLabels["age"].Text = $"⏳ Age: {_selectedEntity.Age:F1}s";
            _infoLabels["position"].Text = $"📍 Pos: ({_selectedEntity.X:F0}, {_selectedEntity.Y:F0})";

            if (_selectedEntity is Creature creature)
            {
                _infoLabels["hunger"].Text = $"🍖 Hunger: {creature.Hunger:F0}/100";
                _infoLabels["hunger"].Visibility = Visibility.Visible;

                // Determine state
                string state = creature.Hunger > 70 ? "Hungry" :
                              creature.Health < 30 ? "Weak" :
                              "Active";
                _infoLabels["state"].Text = $"📊 State: {state}";
            }
            else
            {
                _infoLabels["hunger"].Visibility = Visibility.Collapsed;
                _infoLabels["state"].Text = $"📊 Growth: {_selectedEntity.Health:F0}%";
            }
        }

        /// <summary>
        /// Gets the center point of the selected entity for camera following.
        /// </summary>
        public Point? GetSelectedEntityCenter()
        {
            if (_selectedEntity == null || !_selectedEntity.IsAlive)
                return null;

            return new Point(_selectedEntity.X, _selectedEntity.Y);
        }

        /// <summary>
        /// Clears selection and resources.
        /// </summary>
        public void Clear()
        {
            Deselect();
        }
    }
}
