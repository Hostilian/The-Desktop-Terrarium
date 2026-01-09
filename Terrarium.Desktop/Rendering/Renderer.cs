using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Terrarium.Logic.Entities;
using Terrarium.Logic.Simulation;

namespace Terrarium.Desktop.Rendering
{
    /// <summary>
    /// Handles all rendering logic - separates presentation from simulation.
    /// Supports both shape-based and sprite-based rendering.
    /// </summary>
    public class Renderer
    {
        private readonly Canvas _canvas;
        private readonly Dictionary<int, UIElement> _entityVisuals;
        private readonly Dictionary<int, double> _plantShakeTimers;
        private readonly Random _random;

        // Rendering mode
        private bool _useSpriteMode = false; // Set to true to use sprites instead of shapes

        // Modern color palette for shape mode
        private static readonly Brush PlantStemColor = new SolidColorBrush(Color.FromRgb(46, 125, 50));
        private static readonly Brush PlantLeafColor = new SolidColorBrush(Color.FromRgb(76, 175, 80));
        private static readonly Brush PlantLeafHighlight = new SolidColorBrush(Color.FromRgb(129, 199, 132));
        private static readonly Brush PlantAccentColor = new SolidColorBrush(Color.FromRgb(244, 67, 54));

        private static readonly Brush HerbivoreBodyColor = new SolidColorBrush(Color.FromRgb(255, 183, 77)); // Warm orange
        private static readonly Brush HerbivoreBellyColor = new SolidColorBrush(Color.FromRgb(255, 224, 178)); // Light cream
        private static readonly Brush HerbivoreEarColor = new SolidColorBrush(Color.FromRgb(255, 138, 128)); // Pink
        private static readonly Brush HerbivoreOutlineColor = new SolidColorBrush(Color.FromRgb(230, 150, 50));

        private static readonly Brush CarnivoreBodyColor = new SolidColorBrush(Color.FromRgb(120, 120, 130)); // Gray
        private static readonly Brush CarnivoreFurColor = new SolidColorBrush(Color.FromRgb(150, 150, 160)); // Light gray
        private static readonly Brush CarnivoreAccentColor = new SolidColorBrush(Color.FromRgb(200, 80, 80)); // Red accent
        private static readonly Brush CarnivoreOutlineColor = new SolidColorBrush(Color.FromRgb(90, 90, 100));
        private static readonly Brush CarnivoreEyeColor = new SolidColorBrush(Color.FromRgb(255, 193, 7));

        private static readonly Brush DeadColor = new SolidColorBrush(Color.FromRgb(128, 128, 128));

        // Animation constants
        private const double ShakeDuration = 0.5;
        private const double ShakeMagnitude = 5.0;
        private const double ApproxFrameDeltaSecondsAt60Fps = 0.016;

        // Common numeric constants
        private const double PercentMax = 100.0;

        // Creature visual tuning (shape mode)
        private const double CreatureAnchorOffset = 15.0;
        private const double CreatureDeadOpacity = 0.3;
        private const double DefaultSpriteSize = 40.0;

        // Plant visual tuning (shape mode)
        private const double PlantStemWidth = 4.0;
        private const double PlantStemCornerRadius = 2.0;
        private const double PlantLeavesSizeRatio = 0.9;
        private const double PlantLeavesLeftOffsetRatio = 0.45;
        private const double PlantLeavesTopOffsetRatio = 0.7;
        private const double PlantLeavesLeftNudge = 2.0;
        private const double PlantHighlightSizeRatio = 0.5;
        private const double PlantHighlightLeftOffsetRatio = 0.25;
        private const double PlantHighlightTopOffsetRatio = 0.5;
        private const double PlantHighlightOpacity = 0.7;
        private const double PlantAccentSize = 6.0;
        private const double PlantAccentOpacity = 0.9;
        private const double PlantAccentLeftOffsetRatio = 0.2;
        private const double PlantAccentTopOffsetRatio = 0.4;
        private const double PlantMinAliveOpacity = 0.5;
        private const double PlantDeadOpacity = 0.3;

        public Renderer(Canvas canvas)
        {
            _canvas = canvas;
            _entityVisuals = new Dictionary<int, UIElement>();
            _plantShakeTimers = new Dictionary<int, double>();
            _random = new Random();
        }

        /// <summary>
        /// Clears all rendered elements.
        /// </summary>
        public void Clear()
        {
            // Remove any orphaned visuals (e.g., removed from canvas elsewhere).
            var orphanIds = _entityVisuals
                .Where(kvp => !_canvas.Children.Contains(kvp.Value))
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var id in orphanIds)
            {
                _entityVisuals.Remove(id);
                _plantShakeTimers.Remove(id);
            }
        }

        /// <summary>
        /// Renders the entire world.
        /// </summary>
        public void RenderWorld(World world, double weatherIntensity)
        {
            // Update plant shake timers
            UpdateShakeAnimations();

            // Render weather effects
            RenderWeather(weatherIntensity);

            // Render all entities
            foreach (var plant in world.Plants)
            {
                RenderPlant(plant);
            }

            foreach (var herbivore in world.Herbivores)
            {
                RenderHerbivore(herbivore);
            }

            foreach (var carnivore in world.Carnivores)
            {
                RenderCarnivore(carnivore);
            }

            // Remove visuals for dead entities
            CleanupDeadEntities(world);
        }

        /// <summary>
        /// Renders a plant entity.
        /// </summary>
        private void RenderPlant(Plant plant)
        {
            if (!_entityVisuals.ContainsKey(plant.Id))
            {
                CreatePlantVisual(plant);
            }

            UpdatePlantVisual(plant);
        }

        /// <summary>
        /// Creates visual representation for a plant.
        /// </summary>
        private void CreatePlantVisual(Plant plant)
        {
            if (_useSpriteMode)
            {
                // Sprite mode: Load plant image
                var image = CreateSpriteImage("plant.png");
                _canvas.Children.Add(image);
                _entityVisuals[plant.Id] = image;
            }
            else
            {
                // Shape mode: Draw modern plant shape with multiple layers
                var plantGroup = new Canvas();

                // Stem with gradient effect
                var stem = new Rectangle
                {
                    Width = PlantStemWidth,
                    Height = plant.Size,
                    Fill = PlantStemColor,
                    RadiusX = PlantStemCornerRadius,
                    RadiusY = PlantStemCornerRadius
                };

                // Main leaf cluster (larger ellipse)
                var leaves = new Ellipse
                {
                    Width = plant.Size * PlantLeavesSizeRatio,
                    Height = plant.Size * PlantLeavesSizeRatio,
                    Fill = PlantLeafColor
                };

                // Highlight leaf (smaller, lighter)
                var leafHighlight = new Ellipse
                {
                    Width = plant.Size * PlantHighlightSizeRatio,
                    Height = plant.Size * PlantHighlightSizeRatio,
                    Fill = PlantLeafHighlight,
                    Opacity = PlantHighlightOpacity
                };

                // Small berry or flower accent
                var accent = new Ellipse
                {
                    Width = PlantAccentSize,
                    Height = PlantAccentSize,
                    Fill = PlantAccentColor,
                    Opacity = PlantAccentOpacity
                };

                plantGroup.Children.Add(stem);
                plantGroup.Children.Add(leaves);
                plantGroup.Children.Add(leafHighlight);
                plantGroup.Children.Add(accent);

                Canvas.SetLeft(leaves, -plant.Size * PlantLeavesLeftOffsetRatio + PlantLeavesLeftNudge);
                Canvas.SetTop(leaves, -plant.Size * PlantLeavesTopOffsetRatio);
                Canvas.SetLeft(leafHighlight, -plant.Size * PlantHighlightLeftOffsetRatio + PlantLeavesLeftNudge);
                Canvas.SetTop(leafHighlight, -plant.Size * PlantHighlightTopOffsetRatio);
                Canvas.SetLeft(accent, plant.Size * PlantAccentLeftOffsetRatio);
                Canvas.SetTop(accent, -plant.Size * PlantAccentTopOffsetRatio);

                _canvas.Children.Add(plantGroup);
                _entityVisuals[plant.Id] = plantGroup;
            }
        }

        /// <summary>
        /// Updates plant visual position and appearance.
        /// </summary>
        private void UpdatePlantVisual(Plant plant)
        {
            if (!_entityVisuals.TryGetValue(plant.Id, out var visual))
                return;

            double x = plant.X;
            double y = plant.Y;

            // Apply shake animation if active
            if (_plantShakeTimers.TryGetValue(plant.Id, out double shakeTimer))
            {
                x += Math.Sin(shakeTimer * 20) * ShakeMagnitude;
            }

            Canvas.SetLeft(visual, x);
            Canvas.SetTop(visual, y);

            // Update size and color based on health
            double healthRatio = plant.Health / PercentMax;
            visual.Opacity = plant.IsAlive ? Math.Max(PlantMinAliveOpacity, healthRatio) : PlantDeadOpacity;

            if (visual is Canvas plantCanvas && plantCanvas.Children.Count >= 3)
            {
                // Update stem height
                if (plantCanvas.Children[0] is Rectangle stem)
                {
                    stem.Height = plant.Size;
                }

                // Update main leaves size
                if (plantCanvas.Children[1] is Ellipse leaves)
                {
                    leaves.Width = plant.Size * PlantLeavesSizeRatio;
                    leaves.Height = plant.Size * PlantLeavesSizeRatio;
                    Canvas.SetLeft(leaves, -plant.Size * PlantLeavesLeftOffsetRatio + PlantLeavesLeftNudge);
                    Canvas.SetTop(leaves, -plant.Size * PlantLeavesTopOffsetRatio);
                }

                // Update highlight leaves
                if (plantCanvas.Children[2] is Ellipse highlight)
                {
                    highlight.Width = plant.Size * PlantHighlightSizeRatio;
                    highlight.Height = plant.Size * PlantHighlightSizeRatio;
                    Canvas.SetLeft(highlight, -plant.Size * PlantHighlightLeftOffsetRatio + PlantLeavesLeftNudge);
                    Canvas.SetTop(highlight, -plant.Size * PlantHighlightTopOffsetRatio);
                }
            }
        }

        /// <summary>
        /// Renders a herbivore entity.
        /// </summary>
        private void RenderHerbivore(Herbivore herbivore)
        {
            if (!_entityVisuals.ContainsKey(herbivore.Id))
            {
                CreateCreatureVisual(herbivore);
            }

            UpdateCreatureVisual(herbivore);
        }

        /// <summary>
        /// Renders a carnivore entity.
        /// </summary>
        private void RenderCarnivore(Carnivore carnivore)
        {
            if (!_entityVisuals.ContainsKey(carnivore.Id))
            {
                CreateCreatureVisual(carnivore);
            }

            UpdateCreatureVisual(carnivore);
        }

        /// <summary>
        /// Creates visual representation for a creature.
        /// </summary>
        private void CreateCreatureVisual(Creature creature)
        {
            if (_useSpriteMode)
            {
                string spriteFile = creature is Herbivore ? "herbivore.png" : "carnivore.png";
                var image = CreateSpriteImage(spriteFile);
                _canvas.Children.Add(image);
                _entityVisuals[creature.Id] = image;
            }
            else
            {
                // Shape mode: Draw cute creature with modern design
                var creatureGroup = new Canvas();

                if (creature is Herbivore)
                {
                    // Herbivore: Cute bunny-like creature
                    // Body
                    var body = new Ellipse
                    {
                        Width = 28,
                        Height = 24,
                        Fill = HerbivoreBodyColor,
                        Stroke = HerbivoreOutlineColor,
                        StrokeThickness = 1.5
                    };

                    // Belly highlight
                    var belly = new Ellipse
                    {
                        Width = 16,
                        Height = 12,
                        Fill = HerbivoreBellyColor,
                        Opacity = 0.8
                    };

                    // Left ear
                    var leftEar = new Ellipse
                    {
                        Width = 8,
                        Height = 16,
                        Fill = HerbivoreEarColor
                    };

                    // Right ear  
                    var rightEar = new Ellipse
                    {
                        Width = 8,
                        Height = 16,
                        Fill = HerbivoreEarColor
                    };

                    // Eyes
                    var leftEye = new Ellipse { Width = 6, Height = 6, Fill = Brushes.White };
                    var rightEye = new Ellipse { Width = 6, Height = 6, Fill = Brushes.White };
                    var leftPupil = new Ellipse { Width = 3, Height = 3, Fill = Brushes.Black };
                    var rightPupil = new Ellipse { Width = 3, Height = 3, Fill = Brushes.Black };

                    // Nose
                    var nose = new Ellipse { Width = 4, Height = 3, Fill = HerbivoreEarColor };
                    

                    creatureGroup.Children.Add(leftEar);
                    creatureGroup.Children.Add(rightEar);
                    creatureGroup.Children.Add(body);
                    creatureGroup.Children.Add(belly);
                    creatureGroup.Children.Add(leftEye);
                    creatureGroup.Children.Add(rightEye);
                    creatureGroup.Children.Add(leftPupil);
                    creatureGroup.Children.Add(rightPupil);
                    creatureGroup.Children.Add(nose);

                    Canvas.SetLeft(leftEar, 5);
                    Canvas.SetTop(leftEar, -10);
                    Canvas.SetLeft(rightEar, 15);
                    Canvas.SetTop(rightEar, -10);
                    Canvas.SetLeft(body, 1);
                    Canvas.SetTop(body, 3);
                    Canvas.SetLeft(belly, 7);
                    Canvas.SetTop(belly, 10);
                    Canvas.SetLeft(leftEye, 7);
                    Canvas.SetTop(leftEye, 8);
                    Canvas.SetLeft(rightEye, 17);
                    Canvas.SetTop(rightEye, 8);
                    Canvas.SetLeft(leftPupil, 9);
                    Canvas.SetTop(leftPupil, 10);
                    Canvas.SetLeft(rightPupil, 19);
                    Canvas.SetTop(rightPupil, 10);
                    Canvas.SetLeft(nose, 13);
                    Canvas.SetTop(nose, 16);
                }
                else
                {
                    // Carnivore: Wolf-like creature
                    // Body
                    var body = new Ellipse
                    {
                        Width = 32,
                        Height = 26,
                        Fill = CarnivoreBodyColor,
                        Stroke = CarnivoreOutlineColor,
                        StrokeThickness = 2
                    };

                    // Snout
                    var snout = new Ellipse
                    {
                        Width = 14,
                        Height = 10,
                        Fill = CarnivoreFurColor
                    };

                    // Left ear (pointy)
                    var leftEar = new Polygon
                    {
                        Points = new PointCollection { new Point(0, 12), new Point(6, 0), new Point(12, 12) },
                        Fill = CarnivoreBodyColor,
                        Stroke = CarnivoreOutlineColor,
                        StrokeThickness = 1
                    };

                    // Right ear
                    var rightEar = new Polygon
                    {
                        Points = new PointCollection { new Point(0, 12), new Point(6, 0), new Point(12, 12) },
                        Fill = CarnivoreBodyColor,
                        Stroke = CarnivoreOutlineColor,
                        StrokeThickness = 1
                    };

                    // Eyes (menacing)
                    var leftEye = new Ellipse { Width = 7, Height = 5, Fill = CarnivoreEyeColor }; // Yellow
                    var rightEye = new Ellipse { Width = 7, Height = 5, Fill = CarnivoreEyeColor };
                    var leftPupil = new Ellipse { Width = 3, Height = 4, Fill = Brushes.Black };
                    var rightPupil = new Ellipse { Width = 3, Height = 4, Fill = Brushes.Black };

                    // Nose
                    var nose = new Ellipse { Width = 5, Height = 4, Fill = Brushes.Black };

                    creatureGroup.Children.Add(leftEar);
                    creatureGroup.Children.Add(rightEar);
                    creatureGroup.Children.Add(body);
                    creatureGroup.Children.Add(snout);
                    creatureGroup.Children.Add(leftEye);
                    creatureGroup.Children.Add(rightEye);
                    creatureGroup.Children.Add(leftPupil);
                    creatureGroup.Children.Add(rightPupil);
                    creatureGroup.Children.Add(nose);

                    Canvas.SetLeft(leftEar, 2);
                    Canvas.SetTop(leftEar, -6);
                    Canvas.SetLeft(rightEar, 18);
                    Canvas.SetTop(rightEar, -6);
                    Canvas.SetLeft(body, 0);
                    Canvas.SetTop(body, 4);
                    Canvas.SetLeft(snout, 9);
                    Canvas.SetTop(snout, 18);
                    Canvas.SetLeft(leftEye, 6);
                    Canvas.SetTop(leftEye, 10);
                    Canvas.SetLeft(rightEye, 19);
                    Canvas.SetTop(rightEye, 10);
                    Canvas.SetLeft(leftPupil, 8);
                    Canvas.SetTop(leftPupil, 10);
                    Canvas.SetLeft(rightPupil, 21);
                    Canvas.SetTop(rightPupil, 10);
                    Canvas.SetLeft(nose, 14);
                    Canvas.SetTop(nose, 20);
                }

                _canvas.Children.Add(creatureGroup);
                _entityVisuals[creature.Id] = creatureGroup;
            }
        }

        /// <summary>
        /// Updates creature visual position and appearance.
        /// </summary>
        private void UpdateCreatureVisual(Creature creature)
        {
            if (!_entityVisuals.TryGetValue(creature.Id, out var visual))
                return;

            Canvas.SetLeft(visual, creature.X - CreatureAnchorOffset);
            Canvas.SetTop(visual, creature.Y - CreatureAnchorOffset);

            // Update opacity based on health
            double healthRatio = creature.Health / PercentMax;
            visual.Opacity = creature.IsAlive ? healthRatio : CreatureDeadOpacity;

            // Flip visual based on movement direction
            if (visual is Canvas creatureCanvas)
            {
                var scaleTransform = new ScaleTransform
                {
                    ScaleX = creature.VelocityX < 0 ? -1 : 1,
                    CenterX = CreatureAnchorOffset
                };
                creatureCanvas.RenderTransform = scaleTransform;
            }
        }

        /// <summary>
        /// Creates a sprite image (placeholder for actual sprite loading).
        /// </summary>
        private Image CreateSpriteImage(string fileName)
        {
            var image = new Image
            {
                Width = DefaultSpriteSize,
                Height = DefaultSpriteSize
            };

            try
            {
                // Try to load sprite from Assets folder
                var uri = new Uri($"pack://application:,,,/Assets/{fileName}");
                image.Source = new BitmapImage(uri);
            }
            catch (Exception ex)
            {
                // Fallback: create colored rectangle if sprite not found
                // This allows the app to run without sprite assets
                Debug.WriteLine($"Failed to load sprite '{fileName}': {ex.Message}");
            }

            return image;
        }

        /// <summary>
        /// Renders weather effects overlay.
        /// </summary>
        private void RenderWeather(double intensity)
        {
            // Weather effects can be added here (rain particles, wind effects, etc.)
            // For now, we'll just darken the background during storms
            // This would be implemented in a more sophisticated weather system
        }

        /// <summary>
        /// Triggers shake animation for a plant.
        /// </summary>
        public void TriggerPlantShake(Plant plant)
        {
            _plantShakeTimers[plant.Id] = ShakeDuration;
        }

        /// <summary>
        /// Updates shake animations.
        /// </summary>
        private void UpdateShakeAnimations()
        {
            var expiredShakes = new List<int>();

            foreach (var kvp in _plantShakeTimers.ToList())
            {
                double newTime = kvp.Value - ApproxFrameDeltaSecondsAt60Fps;
                if (newTime <= 0)
                {
                    expiredShakes.Add(kvp.Key);
                }
                else
                {
                    _plantShakeTimers[kvp.Key] = newTime;
                }
            }

            foreach (var id in expiredShakes)
            {
                _plantShakeTimers.Remove(id);
            }
        }

        /// <summary>
        /// Removes visuals for dead entities.
        /// </summary>
        private void CleanupDeadEntities(World world)
        {
            var aliveIds = world.GetAllEntities().Select(e => e.Id).ToHashSet();
            var deadIds = _entityVisuals.Keys.Where(id => !aliveIds.Contains(id)).ToList();

            foreach (var id in deadIds)
            {
                if (_entityVisuals.TryGetValue(id, out var visual))
                {
                    _canvas.Children.Remove(visual);
                    _entityVisuals.Remove(id);
                    _plantShakeTimers.Remove(id);
                }
            }
        }
    }
}
