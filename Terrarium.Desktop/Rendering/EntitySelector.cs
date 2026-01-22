namespace Terrarium.Desktop.Rendering;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Terrarium.Logic.Entities;

/// <summary>
/// Allows users to select and track individual creatures.
/// </summary>
public class EntitySelector
{
    private readonly Canvas canvas;
    private LivingEntity? selectedEntity;
    private Ellipse? selectionRing;
    private Border? infoPanel;
    private readonly Dictionary<string, TextBlock> infoLabels;

    private static readonly Brush AccentBrush = CreateFrozenBrush(Color.FromRgb(80, 200, 255));
    private static readonly Brush HintBrush = CreateFrozenBrush(Color.FromRgb(150, 150, 150));
    private static readonly Brush PanelBackgroundBrush = CreateFrozenBrush(Color.FromArgb(200, 20, 30, 40));

    public bool IsEnabled { get; set; } = true;

    public LivingEntity? SelectedEntity => selectedEntity;

    public event EventHandler<LivingEntity>? OnEntitySelected;

    public event EventHandler? OnEntityDeselected;

    public EntitySelector(Canvas canvas)
    {
        this.canvas = canvas;
        infoLabels = new Dictionary<string, TextBlock>();
        CreateInfoPanel();
    }

    private void CreateInfoPanel()
    {
        infoPanel = new Border
        {
            Width = 180,
            Background = PanelBackgroundBrush,
            BorderBrush = AccentBrush,
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(10),
            Visibility = Visibility.Collapsed
        };

        var stack = new StackPanel();

        // Title
        var title = new TextBlock
        {
            Text = "🔍 Selected Entity",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = AccentBrush,
            Margin = new Thickness(0, 0, 0, 8)
        };
        stack.Children.Add(title);
        infoLabels["title"] = title;

        // Info labels
        string[] labels = { "type", "health", "age", "hunger", "position", "state" };
        foreach (var label in labels)
        {
            var tb = new TextBlock
            {
                FontSize = 10,
                Foreground = Brushes.White
            };
            stack.Children.Add(tb);
            infoLabels[label] = tb;
        }

        // Follow button hint
        var hint = new TextBlock
        {
            Text = "Press F to follow • Esc to deselect",
            FontSize = 9,
            Foreground = HintBrush,
            Margin = new Thickness(0, 8, 0, 0),
            TextWrapping = TextWrapping.Wrap
        };
        stack.Children.Add(hint);

        infoPanel.Child = stack;
        Canvas.SetZIndex(infoPanel, 900);
        canvas.Children.Add(infoPanel);
    }

    /// <summary>
    /// Attempts to select an entity at the given position.
    /// </summary>
    /// <returns></returns>
    public bool TrySelect(double x, double y, IEnumerable<Plant> plants,
                         IEnumerable<Herbivore> herbivores, IEnumerable<Carnivore> carnivores)
    {
        if (!IsEnabled)
        {
            return false;
        }

        const double selectRadius = 20;
        LivingEntity? nearest = null;
        double nearestDist = double.MaxValue;

        // Check creatures first (on top visually)
        foreach (var carnivore in carnivores)
        {
            if (!carnivore.IsAlive)
            {
                continue;
            }

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
            {
                continue;
            }

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
            {
                continue;
            }

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

        selectedEntity = entity;

        // Create selection ring
        selectionRing = new Ellipse
        {
            Width = 40,
            Height = 40,
            Stroke = AccentBrush,
            StrokeThickness = 2,
            StrokeDashArray = new DoubleCollection { 4, 2 },
            Fill = Brushes.Transparent
        };
        Canvas.SetZIndex(selectionRing, 500);
        canvas.Children.Add(selectionRing);

        infoPanel!.Visibility = Visibility.Visible;

        OnEntitySelected?.Invoke(this, entity);
    }

    /// <summary>
    /// Deselects the current entity.
    /// </summary>
    public void Deselect()
    {
        if (selectionRing != null)
        {
            canvas.Children.Remove(selectionRing);
            selectionRing = null;
        }

        if (selectedEntity != null)
        {
            selectedEntity = null;
            infoPanel!.Visibility = Visibility.Collapsed;
            OnEntityDeselected?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Updates the selection visuals.
    /// </summary>
    public void Update(double deltaTime)
    {
        if (!IsEnabled || selectedEntity == null || !selectedEntity.IsAlive)
        {
            if (selectedEntity != null && !selectedEntity.IsAlive)
            {
                // Entity died, deselect
                Deselect();
            }
            return;
        }

        // Update selection ring position
        if (selectionRing != null)
        {
            Canvas.SetLeft(selectionRing, selectedEntity.X - 20);
            Canvas.SetTop(selectionRing, selectedEntity.Y - 20);

            // Rotate the dashed ring
            if (selectionRing.RenderTransform is RotateTransform rotate)
            {
                rotate.Angle += 30 * deltaTime;
            }
            else
            {
                selectionRing.RenderTransform = new RotateTransform(0, 20, 20);
            }
        }

        // Update info panel position (follow entity)
        if (infoPanel != null)
        {
            Canvas.SetLeft(infoPanel, selectedEntity.X + 30);
            Canvas.SetTop(infoPanel, selectedEntity.Y - 60);
        }

        // Update info labels
        UpdateInfoLabels();
    }

    private void UpdateInfoLabels()
    {
        if (selectedEntity == null)
        {
            return;
        }

        string entityType = selectedEntity switch
        {
            Carnivore _ => "🔴 Carnivore",
            Herbivore _ => "🟢 Herbivore",
            Plant _ => "🌿 Plant",
            _ => "Unknown"
        };

        infoLabels["title"].Text = entityType;
        infoLabels["type"].Text = $"ID: #{selectedEntity.GetHashCode() % 10000:D4}";
        infoLabels["health"].Text = $"❤️ Health: {selectedEntity.Health:F0}/100";
        infoLabels["age"].Text = $"⏳ Age: {selectedEntity.Age:F1}s";
        infoLabels["position"].Text = $"📍 Pos: ({selectedEntity.X:F0}, {selectedEntity.Y:F0})";

        if (selectedEntity is Creature creature)
        {
            infoLabels["hunger"].Text = $"🍖 Hunger: {creature.Hunger:F0}/100";
            infoLabels["hunger"].Visibility = Visibility.Visible;

            // Determine state
            string state = creature.Hunger > 70 ? "Hungry" :
                          creature.Health < 30 ? "Weak" :
                          "Active";
            infoLabels["state"].Text = $"📊 State: {state}";
        }
        else
        {
            infoLabels["hunger"].Visibility = Visibility.Collapsed;
            infoLabels["state"].Text = $"📊 Growth: {selectedEntity.Health:F0}%";
        }
    }

    /// <summary>
    /// Gets the center point of the selected entity for camera following.
    /// </summary>
    /// <returns></returns>
    public Point? GetSelectedEntityCenter()
    {
        if (selectedEntity == null || !selectedEntity.IsAlive)
        {
            return null;
        }

        return new Point(selectedEntity.X, selectedEntity.Y);
    }

    private static SolidColorBrush CreateFrozenBrush(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }

    /// <summary>
    /// Clears selection and resources.
    /// </summary>
    public void Clear()
    {
        Deselect();
    }
}
