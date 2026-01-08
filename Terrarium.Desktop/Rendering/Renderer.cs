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

        // Color constants for shape mode
        private static readonly Brush PlantColor = new SolidColorBrush(Color.FromRgb(34, 139, 34));
        private static readonly Brush HerbivoreColor = new SolidColorBrush(Color.FromRgb(255, 218, 185));
        private static readonly Brush CarnivoreColor = new SolidColorBrush(Color.FromRgb(178, 34, 34));
        private static readonly Brush DeadColor = new SolidColorBrush(Color.FromRgb(128, 128, 128));

        // Animation constants
        private const double ShakeDuration = 0.5;
        private const double ShakeMagnitude = 5.0;

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
            // Remove visuals for dead entities
            var deadIds = _entityVisuals.Keys
                .Where(id => !_entityVisuals.ContainsKey(id))
                .ToList();

            foreach (var id in deadIds)
            {
                if (_entityVisuals.TryGetValue(id, out var visual))
                {
                    _canvas.Children.Remove(visual);
                    _entityVisuals.Remove(id);
                }
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
                // Shape mode: Draw simple plant shape
                var plantGroup = new Canvas();

                // Stem
                var stem = new Rectangle
                {
                    Width = 4,
                    Height = plant.Size,
                    Fill = PlantColor
                };

                // Leaves (ellipse on top)
                var leaves = new Ellipse
                {
                    Width = plant.Size * 0.8,
                    Height = plant.Size * 0.8,
                    Fill = PlantColor
                };

                plantGroup.Children.Add(stem);
                plantGroup.Children.Add(leaves);

                Canvas.SetLeft(leaves, -plant.Size * 0.4 + 2);
                Canvas.SetTop(leaves, -plant.Size * 0.6);

                _canvas.Children.Add(plantGroup);
                _entityVisuals[plant.Id] = plantGroup;
            }
        }

        /// <summary>
        /// Updates plant visual position and appearance.
        /// </summary>
        private void UpdatePlantVisual(Plant plant)
        {
            if (!_entityVisuals.TryGetValue(plant.Id, out var visual)) return;

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
            double healthRatio = plant.Health / 100.0;
            visual.Opacity = plant.IsAlive ? healthRatio : 0.5;

            if (visual is Canvas plantCanvas)
            {
                // Update stem height
                if (plantCanvas.Children[0] is Rectangle stem)
                {
                    stem.Height = plant.Size;
                }

                // Update leaves size
                if (plantCanvas.Children[1] is Ellipse leaves)
                {
                    leaves.Width = plant.Size * 0.8;
                    leaves.Height = plant.Size * 0.8;
                    Canvas.SetLeft(leaves, -plant.Size * 0.4 + 2);
                    Canvas.SetTop(leaves, -plant.Size * 0.6);
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
                CreateCreatureVisual(herbivore, HerbivoreColor);
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
                CreateCreatureVisual(carnivore, CarnivoreColor);
            }

            UpdateCreatureVisual(carnivore);
        }

        /// <summary>
        /// Creates visual representation for a creature.
        /// </summary>
        private void CreateCreatureVisual(Creature creature, Brush color)
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
                // Shape mode: Draw circle with eyes
                var creatureGroup = new Canvas();

                var body = new Ellipse
                {
                    Width = 30,
                    Height = 30,
                    Fill = color,
                    Stroke = Brushes.Black,
                    StrokeThickness = 2
                };

                var leftEye = new Ellipse
                {
                    Width = 6,
                    Height = 6,
                    Fill = Brushes.Black
                };

                var rightEye = new Ellipse
                {
                    Width = 6,
                    Height = 6,
                    Fill = Brushes.Black
                };

                creatureGroup.Children.Add(body);
                creatureGroup.Children.Add(leftEye);
                creatureGroup.Children.Add(rightEye);

                Canvas.SetLeft(leftEye, 8);
                Canvas.SetTop(leftEye, 10);
                Canvas.SetLeft(rightEye, 16);
                Canvas.SetTop(rightEye, 10);

                _canvas.Children.Add(creatureGroup);
                _entityVisuals[creature.Id] = creatureGroup;
            }
        }

        /// <summary>
        /// Updates creature visual position and appearance.
        /// </summary>
        private void UpdateCreatureVisual(Creature creature)
        {
            if (!_entityVisuals.TryGetValue(creature.Id, out var visual)) return;

            Canvas.SetLeft(visual, creature.X - 15);
            Canvas.SetTop(visual, creature.Y - 15);

            // Update opacity based on health
            double healthRatio = creature.Health / 100.0;
            visual.Opacity = creature.IsAlive ? healthRatio : 0.3;

            // Flip visual based on movement direction
            if (visual is Canvas creatureCanvas)
            {
                var scaleTransform = new ScaleTransform
                {
                    ScaleX = creature.VelocityX < 0 ? -1 : 1,
                    CenterX = 15
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
                Width = 40,
                Height = 40
            };

            try
            {
                // Try to load sprite from Assets folder
                var uri = new Uri($"pack://application:,,,/Assets/{fileName}");
                image.Source = new BitmapImage(uri);
            }
            catch
            {
                // Fallback: create colored rectangle if sprite not found
                // This allows the app to run without sprite assets
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
                double newTime = kvp.Value - 0.016; // Approximately one frame at 60 FPS
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
