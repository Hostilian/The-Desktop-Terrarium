using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Terrarium.Logic.Entities;
using Terrarium.Logic.Simulation;

namespace Terrarium.Desktop.Rendering;

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

    private readonly List<int> _orphanIdsBuffer = new();
    private readonly List<int> _expiredShakeIdsBuffer = new();
    private readonly List<int> _shakeKeyBuffer = new();
    private readonly HashSet<int> _aliveIdsBuffer = new();
    private readonly List<int> _deadIdsBuffer = new();

    // Rendering mode
    private readonly bool _useSpriteMode = false; // Set to true to use sprites instead of shapes

    // Modern color palette for shape mode
    private Brush? PlantStemColor;
    private Brush? PlantLeafColor;
    private Brush? PlantLeafHighlight;
    private Brush? PlantAccentColor;

    private Brush? HerbivoreBodyColor;
    private Brush? HerbivoreBellyColor;
    private Brush? HerbivoreEarColor;
    private Brush? HerbivoreOutlineColor;

    private Brush? CarnivoreBodyColor;
    private Brush? CarnivoreFurColor;
    private Brush? CarnivoreAccentColor;
    private Brush? CarnivoreOutlineColor;
    private Brush? CarnivoreEyeColor;

    private static readonly Brush DeadColor = CreateFrozenBrush(Color.FromRgb(128, 128, 128));

    // Animation constants
    private const double ShakeDuration = 0.5;
    private const double ShakeMagnitude = 5.0;
    private const double ShakeFrequencyRadiansPerSecond = 20.0;
    private const double ApproxFrameDeltaSecondsAt60Fps = 0.016;

    // Common numeric constants
    private const double PercentMax = 100.0;

    // Creature visual tuning (shape mode)
    private const double CreatureAnchorOffset = 15.0;
    private const double CreatureDeadOpacity = 0.3;
    private const double DefaultSpriteSize = 40.0;

    // Herbivore shape (bunny-like)
    private const double HerbivoreBodyWidth = 28.0;
    private const double HerbivoreBodyHeight = 24.0;
    private const double HerbivoreBodyStrokeThickness = 1.5;
    private const double HerbivoreBellyWidth = 16.0;
    private const double HerbivoreBellyHeight = 12.0;
    private const double HerbivoreBellyOpacity = 0.8;
    private const double HerbivoreEarWidth = 8.0;
    private const double HerbivoreEarHeight = 16.0;
    private const double HerbivoreEyeSize = 6.0;
    private const double HerbivorePupilSize = 3.0;
    private const double HerbivoreNoseWidth = 4.0;
    private const double HerbivoreNoseHeight = 3.0;

    private const double HerbivoreLeftEarX = 5.0;
    private const double HerbivoreEarY = -10.0;
    private const double HerbivoreRightEarX = 15.0;
    private const double HerbivoreBodyX = 1.0;
    private const double HerbivoreBodyY = 3.0;
    private const double HerbivoreBellyX = 7.0;
    private const double HerbivoreBellyY = 10.0;
    private const double HerbivoreLeftEyeX = 7.0;
    private const double HerbivoreEyesY = 8.0;
    private const double HerbivoreRightEyeX = 17.0;
    private const double HerbivoreLeftPupilX = 9.0;
    private const double HerbivorePupilsY = 10.0;
    private const double HerbivoreRightPupilX = 19.0;
    private const double HerbivoreNoseX = 13.0;
    private const double HerbivoreNoseY = 16.0;

    // Carnivore shape (wolf-like)
    private const double CarnivoreBodyWidth = 32.0;
    private const double CarnivoreBodyHeight = 26.0;
    private const double CarnivoreBodyStrokeThickness = 2.0;
    private const double CarnivoreSnoutWidth = 14.0;
    private const double CarnivoreSnoutHeight = 10.0;
    private const double CarnivoreEarStrokeThickness = 1.0;
    private const double CarnivoreEyeWidth = 7.0;
    private const double CarnivoreEyeHeight = 5.0;
    private const double CarnivorePupilWidth = 3.0;
    private const double CarnivorePupilHeight = 4.0;
    private const double CarnivoreNoseWidth = 5.0;
    private const double CarnivoreNoseHeight = 4.0;

    private const double CarnivoreLeftEarX = 2.0;
    private const double CarnivoreEarY = -6.0;
    private const double CarnivoreRightEarX = 18.0;
    private const double CarnivoreBodyX = 0.0;
    private const double CarnivoreBodyY = 4.0;
    private const double CarnivoreSnoutX = 9.0;
    private const double CarnivoreSnoutY = 18.0;
    private const double CarnivoreLeftEyeX = 6.0;
    private const double CarnivoreEyesY = 10.0;
    private const double CarnivoreRightEyeX = 19.0;
    private const double CarnivoreLeftPupilX = 8.0;
    private const double CarnivorePupilsY = 10.0;
    private const double CarnivoreRightPupilX = 21.0;
    private const double CarnivoreNoseX = 14.0;
    private const double CarnivoreNoseY = 20.0;

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

    private const int PlantStemChildIndex = 0;
    private const int PlantLeavesChildIndex = 1;
    private const int PlantHighlightChildIndex = 2;

    // Carnivore ear geometry
    private const double CarnivoreEarBaseY = 12.0;
    private const double CarnivoreEarApexX = 6.0;
    private const double CarnivoreEarApexY = 0.0;
    private const double CarnivoreEarBaseWidth = 12.0;

    private static readonly PointCollection CarnivoreEarPoints = new()
    {
        new Point(0, CarnivoreEarBaseY),
        new Point(CarnivoreEarApexX, CarnivoreEarApexY),
        new Point(CarnivoreEarBaseWidth, CarnivoreEarBaseY)
    };

    public Renderer(Canvas canvas, TerrariumType terrariumType)
    {
        _canvas = canvas;
        _entityVisuals = new Dictionary<int, UIElement>();
        _plantShakeTimers = new Dictionary<int, double>();
        _random = new Random();

        // Set colors based on terrarium type
        SetThemeColors(terrariumType);
        CreateBackgroundScenery(terrariumType);
    }

    private void SetThemeColors(TerrariumType terrariumType)
    {
        switch (terrariumType)
        {
            case TerrariumType.Forest:
                PlantStemColor = CreateFrozenBrush(Color.FromRgb(34, 139, 34)); // Dark green
                PlantLeafColor = CreateFrozenBrush(Color.FromRgb(76, 175, 80)); // Green
                PlantLeafHighlight = CreateFrozenBrush(Color.FromRgb(129, 199, 132)); // Light green
                PlantAccentColor = CreateFrozenBrush(Color.FromRgb(244, 67, 54)); // Red
                HerbivoreBodyColor = CreateFrozenBrush(Color.FromRgb(255, 183, 77)); // Orange
                HerbivoreBellyColor = CreateFrozenBrush(Color.FromRgb(255, 224, 178)); // Light orange
                HerbivoreEarColor = CreateFrozenBrush(Color.FromRgb(255, 138, 128)); // Pink
                HerbivoreOutlineColor = CreateFrozenBrush(Color.FromRgb(230, 150, 50)); // Brown
                CarnivoreBodyColor = CreateFrozenBrush(Color.FromRgb(120, 120, 130)); // Gray
                CarnivoreFurColor = CreateFrozenBrush(Color.FromRgb(150, 150, 160)); // Light gray
                CarnivoreAccentColor = CreateFrozenBrush(Color.FromRgb(200, 80, 80)); // Red
                CarnivoreOutlineColor = CreateFrozenBrush(Color.FromRgb(90, 90, 100)); // Dark gray
                CarnivoreEyeColor = CreateFrozenBrush(Color.FromRgb(255, 193, 7)); // Yellow
                break;
            case TerrariumType.Desert:
                PlantStemColor = CreateFrozenBrush(Color.FromRgb(210, 180, 140)); // Tan
                PlantLeafColor = CreateFrozenBrush(Color.FromRgb(222, 184, 135)); // Burlywood
                PlantLeafHighlight = CreateFrozenBrush(Color.FromRgb(245, 222, 179)); // Wheat
                PlantAccentColor = CreateFrozenBrush(Color.FromRgb(255, 140, 0)); // Dark orange
                HerbivoreBodyColor = CreateFrozenBrush(Color.FromRgb(210, 180, 140)); // Tan
                HerbivoreBellyColor = CreateFrozenBrush(Color.FromRgb(222, 184, 135)); // Burlywood
                HerbivoreEarColor = CreateFrozenBrush(Color.FromRgb(160, 82, 45)); // Sienna
                HerbivoreOutlineColor = CreateFrozenBrush(Color.FromRgb(139, 69, 19)); // Saddle brown
                CarnivoreBodyColor = CreateFrozenBrush(Color.FromRgb(105, 105, 105)); // Dim gray
                CarnivoreFurColor = CreateFrozenBrush(Color.FromRgb(128, 128, 128)); // Gray
                CarnivoreAccentColor = CreateFrozenBrush(Color.FromRgb(139, 69, 19)); // Saddle brown
                CarnivoreOutlineColor = CreateFrozenBrush(Color.FromRgb(47, 79, 79)); // Dark slate gray
                CarnivoreEyeColor = CreateFrozenBrush(Color.FromRgb(255, 215, 0)); // Gold
                break;
            case TerrariumType.Aquatic:
                PlantStemColor = CreateFrozenBrush(Color.FromRgb(70, 130, 180)); // Steel blue
                PlantLeafColor = CreateFrozenBrush(Color.FromRgb(100, 149, 237)); // Cornflower blue
                PlantLeafHighlight = CreateFrozenBrush(Color.FromRgb(135, 206, 250)); // Light sky blue
                PlantAccentColor = CreateFrozenBrush(Color.FromRgb(0, 206, 209)); // Dark turquoise
                HerbivoreBodyColor = CreateFrozenBrush(Color.FromRgb(70, 130, 180)); // Steel blue
                HerbivoreBellyColor = CreateFrozenBrush(Color.FromRgb(100, 149, 237)); // Cornflower blue
                HerbivoreEarColor = CreateFrozenBrush(Color.FromRgb(0, 191, 255)); // Deep sky blue
                HerbivoreOutlineColor = CreateFrozenBrush(Color.FromRgb(25, 25, 112)); // Midnight blue
                CarnivoreBodyColor = CreateFrozenBrush(Color.FromRgb(0, 0, 139)); // Dark blue
                CarnivoreFurColor = CreateFrozenBrush(Color.FromRgb(0, 0, 205)); // Medium blue
                CarnivoreAccentColor = CreateFrozenBrush(Color.FromRgb(255, 0, 0)); // Red
                CarnivoreOutlineColor = CreateFrozenBrush(Color.FromRgb(0, 0, 0)); // Black
                CarnivoreEyeColor = CreateFrozenBrush(Color.FromRgb(255, 255, 0)); // Yellow
                break;
            case TerrariumType.GodSimulator:
                PlantStemColor = CreateFrozenBrush(Color.FromRgb(138, 43, 226)); // Blue violet
                PlantLeafColor = CreateFrozenBrush(Color.FromRgb(186, 85, 211)); // Medium orchid
                PlantLeafHighlight = CreateFrozenBrush(Color.FromRgb(221, 160, 221)); // Plum
                PlantAccentColor = CreateFrozenBrush(Color.FromRgb(255, 20, 147)); // Deep pink
                HerbivoreBodyColor = CreateFrozenBrush(Color.FromRgb(255, 215, 0)); // Gold
                HerbivoreBellyColor = CreateFrozenBrush(Color.FromRgb(255, 255, 0)); // Yellow
                HerbivoreEarColor = CreateFrozenBrush(Color.FromRgb(255, 105, 180)); // Hot pink
                HerbivoreOutlineColor = CreateFrozenBrush(Color.FromRgb(75, 0, 130)); // Indigo
                CarnivoreBodyColor = CreateFrozenBrush(Color.FromRgb(220, 20, 60)); // Crimson
                CarnivoreFurColor = CreateFrozenBrush(Color.FromRgb(255, 69, 0)); // Red orange
                CarnivoreAccentColor = CreateFrozenBrush(Color.FromRgb(255, 0, 255)); // Magenta
                CarnivoreOutlineColor = CreateFrozenBrush(Color.FromRgb(25, 25, 25)); // Very dark gray
                CarnivoreEyeColor = CreateFrozenBrush(Color.FromRgb(0, 255, 255)); // Cyan
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(terrariumType), terrariumType, null);
        }
    }

    private void CreateBackgroundScenery(TerrariumType terrariumType)
    {
        // Clear existing background elements
        var backgroundElements = _canvas.Children.OfType<UIElement>()
            .Where(e => e is Rectangle || e is Ellipse || e is Polygon)
            .Where(e => !_entityVisuals.ContainsValue(e))
            .ToList();
        foreach (var element in backgroundElements)
        {
            _canvas.Children.Remove(element);
        }

        // Add theme-specific background scenery
        switch (terrariumType)
        {
            case TerrariumType.Forest:
                CreateForestScenery();
                break;
            case TerrariumType.Desert:
                CreateDesertScenery();
                break;
            case TerrariumType.Aquatic:
                CreateAquaticScenery();
                break;
            case TerrariumType.GodSimulator:
                CreateGodSimulatorScenery();
                break;
        }
    }

    private void CreateForestScenery()
    {
        var random = new Random(42); // Fixed seed for consistent scenery

        // Add distant trees
        for (int i = 0; i < 15; i++)
        {
            var treeX = random.Next(0, 800);
            var treeY = random.Next(400, 550);

            // Tree trunk
            var trunk = new Rectangle
            {
                Width = 8,
                Height = 40,
                Fill = CreateFrozenBrush(Color.FromRgb(101, 67, 33)),
                Opacity = 0.6
            };
            Canvas.SetLeft(trunk, treeX);
            Canvas.SetTop(trunk, treeY);
            _canvas.Children.Insert(0, trunk);

            // Tree foliage
            var foliage = new Ellipse
            {
                Width = 35,
                Height = 35,
                Fill = CreateFrozenBrush(Color.FromRgb(34, 139, 34)),
                Opacity = 0.4
            };
            Canvas.SetLeft(foliage, treeX - 13);
            Canvas.SetTop(foliage, treeY - 25);
            _canvas.Children.Insert(0, foliage);
        }

        // Add bushes
        for (int i = 0; i < 20; i++)
        {
            var bushX = random.Next(0, 800);
            var bushY = random.Next(450, 580);

            var bush = new Ellipse
            {
                Width = random.Next(20, 40),
                Height = random.Next(15, 25),
                Fill = CreateFrozenBrush(Color.FromRgb(76, 175, 80)),
                Opacity = 0.3
            };
            Canvas.SetLeft(bush, bushX);
            Canvas.SetTop(bush, bushY);
            _canvas.Children.Insert(0, bush);
        }
    }

    private void CreateDesertScenery()
    {
        var random = new Random(42);

        // Add cacti
        for (int i = 0; i < 12; i++)
        {
            var cactusX = random.Next(0, 800);
            var cactusY = random.Next(450, 550);

            // Cactus body
            var body = new Rectangle
            {
                Width = 12,
                Height = 35,
                Fill = CreateFrozenBrush(Color.FromRgb(85, 107, 47)),
                Opacity = 0.5
            };
            Canvas.SetLeft(body, cactusX);
            Canvas.SetTop(body, cactusY);
            _canvas.Children.Insert(0, body);

            // Cactus arms
            if (random.Next(2) == 0)
            {
                var arm = new Rectangle
                {
                    Width = 8,
                    Height = 15,
                    Fill = CreateFrozenBrush(Color.FromRgb(85, 107, 47)),
                    Opacity = 0.5
                };
                Canvas.SetLeft(arm, cactusX - 6);
                Canvas.SetTop(arm, cactusY + 10);
                _canvas.Children.Insert(0, arm);
            }
        }

        // Add rocks
        for (int i = 0; i < 25; i++)
        {
            var rockX = random.Next(0, 800);
            var rockY = random.Next(480, 590);

            var rock = new Ellipse
            {
                Width = random.Next(15, 35),
                Height = random.Next(10, 20),
                Fill = CreateFrozenBrush(Color.FromRgb(169, 169, 169)),
                Opacity = 0.4
            };
            Canvas.SetLeft(rock, rockX);
            Canvas.SetTop(rock, rockY);
            _canvas.Children.Insert(0, rock);
        }
    }

    private void CreateAquaticScenery()
    {
        var random = new Random(42);

        // Add coral
        for (int i = 0; i < 18; i++)
        {
            var coralX = random.Next(0, 800);
            var coralY = random.Next(450, 550);

            var coral = new Polygon
            {
                Points = new PointCollection
                {
                    new Point(0, 20),
                    new Point(8, 0),
                    new Point(16, 15),
                    new Point(12, 25),
                    new Point(4, 22)
                },
                Fill = CreateFrozenBrush(Color.FromRgb(255, 20, 147)),
                Opacity = 0.4
            };
            Canvas.SetLeft(coral, coralX);
            Canvas.SetTop(coral, coralY);
            _canvas.Children.Insert(0, coral);
        }

        // Add seaweed
        for (int i = 0; i < 30; i++)
        {
            var seaweedX = random.Next(0, 800);
            var seaweedY = random.Next(480, 590);

            var seaweed = new Rectangle
            {
                Width = 3,
                Height = random.Next(20, 40),
                Fill = CreateFrozenBrush(Color.FromRgb(0, 128, 0)),
                Opacity = 0.3
            };
            Canvas.SetLeft(seaweed, seaweedX);
            Canvas.SetTop(seaweed, seaweedY);
            _canvas.Children.Insert(0, seaweed);
        }

        // Add bubbles
        for (int i = 0; i < 15; i++)
        {
            var bubbleX = random.Next(0, 800);
            var bubbleY = random.Next(400, 580);

            var bubble = new Ellipse
            {
                Width = random.Next(5, 12),
                Height = random.Next(5, 12),
                Fill = CreateFrozenBrush(Color.FromRgb(173, 216, 230)),
                Opacity = 0.2
            };
            Canvas.SetLeft(bubble, bubbleX);
            Canvas.SetTop(bubble, bubbleY);
            _canvas.Children.Insert(0, bubble);
        }
    }

    private void CreateGodSimulatorScenery()
    {
        var random = new Random(42);

        // Add floating crystals
        for (int i = 0; i < 12; i++)
        {
            var crystalX = random.Next(0, 800);
            var crystalY = random.Next(400, 550);

            var crystal = new Polygon
            {
                Points = new PointCollection
                {
                    new Point(8, 0),
                    new Point(16, 8),
                    new Point(8, 16),
                    new Point(0, 8)
                },
                Fill = CreateFrozenBrush(Color.FromRgb(138, 43, 226)),
                Opacity = 0.6
            };
            Canvas.SetLeft(crystal, crystalX);
            Canvas.SetTop(crystal, crystalY);
            _canvas.Children.Insert(0, crystal);
        }

        // Add energy orbs
        for (int i = 0; i < 20; i++)
        {
            var orbX = random.Next(0, 800);
            var orbY = random.Next(420, 580);

            var orb = new Ellipse
            {
                Width = random.Next(8, 20),
                Height = random.Next(8, 20),
                Fill = CreateFrozenBrush(Color.FromRgb(255, 20, 147)),
                Opacity = 0.4
            };
            Canvas.SetLeft(orb, orbX);
            Canvas.SetTop(orb, orbY);
            _canvas.Children.Insert(0, orb);
        }

        // Add mystical runes/symbols
        for (int i = 0; i < 8; i++)
        {
            var runeX = random.Next(0, 800);
            var runeY = random.Next(450, 550);

            var rune = new Rectangle
            {
                Width = random.Next(15, 25),
                Height = random.Next(15, 25),
                Fill = CreateFrozenBrush(Color.FromRgb(255, 105, 180)),
                Opacity = 0.3
            };
            Canvas.SetLeft(rune, runeX);
            Canvas.SetTop(rune, runeY);
            _canvas.Children.Insert(0, rune);
        }
    }

    /// <summary>
    /// Sets the render quality level.
    /// </summary>
    public void SetRenderQuality(string quality)
    {
        // Placeholder for render quality settings
        // Could affect anti-aliasing, texture quality, etc.
    }

    /// <summary>
    /// Enables or disables background scenery.
    /// </summary>
    public void SetShowScenery(bool show)
    {
        // Toggle visibility of scenery elements
        var sceneryElements = _canvas.Children.OfType<UIElement>()
            .Where(e => e is Rectangle || e is Ellipse || e is Polygon)
            .Where(e => !_entityVisuals.ContainsValue(e));

        foreach (var element in sceneryElements)
        {
            element.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    /// <summary>
    /// Enables or disables entity shadows/effects.
    /// </summary>
    public void SetShowShadows(bool show)
    {
        // This would affect drop shadows and other effects
        // For now, just store the setting
        _showShadows = show;
    }

    private bool _showShadows = true;

    /// <summary>
    /// Clears all rendered elements.
    /// </summary>
    public void Clear()
    {
        // Remove any orphaned visuals (e.g., removed from canvas elsewhere).
        _orphanIdsBuffer.Clear();

        foreach (var kvp in _entityVisuals)
        {
            if (!_canvas.Children.Contains(kvp.Value))
            {
                _orphanIdsBuffer.Add(kvp.Key);
            }
        }

        foreach (var id in _orphanIdsBuffer)
        {
            _entityVisuals.Remove(id);
            _plantShakeTimers.Remove(id);
        }
    }

    /// <summary>
    /// Renders the entire world.
    /// </summary>
    public void RenderWorld(World world, double weatherIntensity, Point mousePosition, bool mouseInCanvas)
    {
        // Update plant shake timers
        UpdateShakeAnimations();

        // Render weather effects
        RenderWeather(weatherIntensity);

        // Render all entities with mouse interaction
        foreach (var plant in world.Plants)
        {
            RenderPlant(plant, mousePosition, mouseInCanvas);
        }

        foreach (var herbivore in world.Herbivores)
        {
            RenderHerbivore(herbivore, mousePosition, mouseInCanvas);
        }

        foreach (var carnivore in world.Carnivores)
        {
            RenderCarnivore(carnivore, mousePosition, mouseInCanvas);
        }

        // Remove visuals for dead entities
        CleanupDeadEntities(world);
    }

    /// <summary>
    /// Renders a plant entity.
    /// </summary>
    private void RenderPlant(Plant plant, Point mousePosition, bool mouseInCanvas)
    {
        if (!_entityVisuals.ContainsKey(plant.Id))
        {
            CreatePlantVisual(plant);
        }

        UpdatePlantVisual(plant, mousePosition, mouseInCanvas);
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
                Fill = PlantStemColor!,
                RadiusX = PlantStemCornerRadius,
                RadiusY = PlantStemCornerRadius
            };

            // Main leaf cluster (larger ellipse)
            var leaves = new Ellipse
            {
                Width = plant.Size * PlantLeavesSizeRatio,
                Height = plant.Size * PlantLeavesSizeRatio,
                Fill = PlantLeafColor!
            };

            // Highlight leaf (smaller, lighter)
            var leafHighlight = new Ellipse
            {
                Width = plant.Size * PlantHighlightSizeRatio,
                Height = plant.Size * PlantHighlightSizeRatio,
                Fill = PlantLeafHighlight!,
                Opacity = PlantHighlightOpacity
            };

            // Small berry or flower accent
            var accent = new Ellipse
            {
                Width = PlantAccentSize,
                Height = PlantAccentSize,
                Fill = PlantAccentColor!,
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
    private void UpdatePlantVisual(Plant plant, Point mousePosition, bool mouseInCanvas)
    {
        if (!_entityVisuals.TryGetValue(plant.Id, out var visual))
        {
            return;
        }

        double x = plant.X;
        double y = plant.Y;

        // Apply shake animation if active
        if (_plantShakeTimers.TryGetValue(plant.Id, out double shakeTimer))
        {
            x += Math.Sin(shakeTimer * ShakeFrequencyRadiansPerSecond) * ShakeMagnitude;
        }

        Canvas.SetLeft(visual, x);
        Canvas.SetTop(visual, y);

        // Update size and color based on health
        double healthRatio = plant.Health / PercentMax;
        visual.Opacity = plant.IsAlive ? Math.Max(PlantMinAliveOpacity, healthRatio) : PlantDeadOpacity;

        // Apply mouse glow effect
        if (mouseInCanvas && visual is Canvas glowCanvas)
        {
            double distance = Math.Sqrt(Math.Pow(mousePosition.X - (x + plant.Size / 2), 2) +
                                       Math.Pow(mousePosition.Y - (y + plant.Size), 2));
            double glowIntensity = Math.Max(0, 1 - distance / 100); // Glow within 100 pixels

            if (glowIntensity > 0)
            {
                var glowEffect = new DropShadowEffect
                {
                    Color = Colors.Yellow,
                    BlurRadius = 10 * glowIntensity,
                    ShadowDepth = 0,
                    Opacity = glowIntensity * 0.5
                };
                glowCanvas.Effect = glowEffect;
            }
            else
            {
                glowCanvas.Effect = null;
            }
        }
        else if (visual is Canvas noGlowCanvas)
        {
            noGlowCanvas.Effect = null;
        }

        if (visual is Canvas plantCanvas && plantCanvas.Children.Count >= 3)
        {
            // Update stem height
            if (plantCanvas.Children[PlantStemChildIndex] is Rectangle stem)
            {
                stem.Height = plant.Size;
            }

            // Update main leaves size
            if (plantCanvas.Children[PlantLeavesChildIndex] is Ellipse leaves)
            {
                leaves.Width = plant.Size * PlantLeavesSizeRatio;
                leaves.Height = plant.Size * PlantLeavesSizeRatio;
                Canvas.SetLeft(leaves, -plant.Size * PlantLeavesLeftOffsetRatio + PlantLeavesLeftNudge);
                Canvas.SetTop(leaves, -plant.Size * PlantLeavesTopOffsetRatio);
            }

            // Update highlight leaves
            if (plantCanvas.Children[PlantHighlightChildIndex] is Ellipse highlight)
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
    private void RenderHerbivore(Herbivore herbivore, Point mousePosition, bool mouseInCanvas)
    {
        if (!_entityVisuals.ContainsKey(herbivore.Id))
        {
            CreateCreatureVisual(herbivore);
        }

        UpdateCreatureVisual(herbivore, mousePosition, mouseInCanvas);
    }

    /// <summary>
    /// Renders a carnivore entity.
    /// </summary>
    private void RenderCarnivore(Carnivore carnivore, Point mousePosition, bool mouseInCanvas)
    {
        if (!_entityVisuals.ContainsKey(carnivore.Id))
        {
            CreateCreatureVisual(carnivore);
        }

        UpdateCreatureVisual(carnivore, mousePosition, mouseInCanvas);
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
                    Width = HerbivoreBodyWidth,
                    Height = HerbivoreBodyHeight,
                    Fill = HerbivoreBodyColor!,
                    Stroke = HerbivoreOutlineColor!,
                    StrokeThickness = HerbivoreBodyStrokeThickness
                };

                // Belly highlight
                var belly = new Ellipse
                {
                    Width = HerbivoreBellyWidth,
                    Height = HerbivoreBellyHeight,
                    Fill = HerbivoreBellyColor!,
                    Opacity = HerbivoreBellyOpacity
                };

                // Left ear
                var leftEar = new Ellipse
                {
                    Width = HerbivoreEarWidth,
                    Height = HerbivoreEarHeight,
                    Fill = HerbivoreEarColor!
                };

                // Right ear
                var rightEar = new Ellipse
                {
                    Width = HerbivoreEarWidth,
                    Height = HerbivoreEarHeight,
                    Fill = HerbivoreEarColor!
                };

                // Eyes
                var leftEye = new Ellipse { Width = HerbivoreEyeSize, Height = HerbivoreEyeSize, Fill = Brushes.White };
                var rightEye = new Ellipse { Width = HerbivoreEyeSize, Height = HerbivoreEyeSize, Fill = Brushes.White };
                var leftPupil = new Ellipse { Width = HerbivorePupilSize, Height = HerbivorePupilSize, Fill = Brushes.Black };
                var rightPupil = new Ellipse { Width = HerbivorePupilSize, Height = HerbivorePupilSize, Fill = Brushes.Black };

                // Nose
                var nose = new Ellipse { Width = HerbivoreNoseWidth, Height = HerbivoreNoseHeight, Fill = HerbivoreEarColor! };


                creatureGroup.Children.Add(leftEar);
                creatureGroup.Children.Add(rightEar);
                creatureGroup.Children.Add(body);
                creatureGroup.Children.Add(belly);
                creatureGroup.Children.Add(leftEye);
                creatureGroup.Children.Add(rightEye);
                creatureGroup.Children.Add(leftPupil);
                creatureGroup.Children.Add(rightPupil);
                creatureGroup.Children.Add(nose);

                Canvas.SetLeft(leftEar, HerbivoreLeftEarX);
                Canvas.SetTop(leftEar, HerbivoreEarY);
                Canvas.SetLeft(rightEar, HerbivoreRightEarX);
                Canvas.SetTop(rightEar, HerbivoreEarY);
                Canvas.SetLeft(body, HerbivoreBodyX);
                Canvas.SetTop(body, HerbivoreBodyY);
                Canvas.SetLeft(belly, HerbivoreBellyX);
                Canvas.SetTop(belly, HerbivoreBellyY);
                Canvas.SetLeft(leftEye, HerbivoreLeftEyeX);
                Canvas.SetTop(leftEye, HerbivoreEyesY);
                Canvas.SetLeft(rightEye, HerbivoreRightEyeX);
                Canvas.SetTop(rightEye, HerbivoreEyesY);
                Canvas.SetLeft(leftPupil, HerbivoreLeftPupilX);
                Canvas.SetTop(leftPupil, HerbivorePupilsY);
                Canvas.SetLeft(rightPupil, HerbivoreRightPupilX);
                Canvas.SetTop(rightPupil, HerbivorePupilsY);
                Canvas.SetLeft(nose, HerbivoreNoseX);
                Canvas.SetTop(nose, HerbivoreNoseY);
            }
            else
            {
                // Carnivore: Wolf-like creature
                // Body
                var body = new Ellipse
                {
                    Width = CarnivoreBodyWidth,
                    Height = CarnivoreBodyHeight,
                    Fill = CarnivoreBodyColor!,
                    Stroke = CarnivoreOutlineColor!,
                    StrokeThickness = CarnivoreBodyStrokeThickness
                };

                // Snout
                var snout = new Ellipse
                {
                    Width = CarnivoreSnoutWidth,
                    Height = CarnivoreSnoutHeight,
                    Fill = CarnivoreFurColor!
                };

                // Left ear (pointy)
                var leftEar = new Polygon
                {
                    Points = CarnivoreEarPoints,
                    Fill = CarnivoreBodyColor!,
                    Stroke = CarnivoreOutlineColor!,
                    StrokeThickness = CarnivoreEarStrokeThickness
                };

                // Right ear
                var rightEar = new Polygon
                {
                    Points = CarnivoreEarPoints,
                    Fill = CarnivoreBodyColor!,
                    Stroke = CarnivoreOutlineColor!,
                    StrokeThickness = CarnivoreEarStrokeThickness
                };

                // Eyes (menacing)
                var leftEye = new Ellipse { Width = CarnivoreEyeWidth, Height = CarnivoreEyeHeight, Fill = CarnivoreEyeColor! }; // Yellow
                var rightEye = new Ellipse { Width = CarnivoreEyeWidth, Height = CarnivoreEyeHeight, Fill = CarnivoreEyeColor! };
                var leftPupil = new Ellipse { Width = CarnivorePupilWidth, Height = CarnivorePupilHeight, Fill = Brushes.Black };
                var rightPupil = new Ellipse { Width = CarnivorePupilWidth, Height = CarnivorePupilHeight, Fill = Brushes.Black };

                // Nose
                var nose = new Ellipse { Width = CarnivoreNoseWidth, Height = CarnivoreNoseHeight, Fill = Brushes.Black };

                creatureGroup.Children.Add(leftEar);
                creatureGroup.Children.Add(rightEar);
                creatureGroup.Children.Add(body);
                creatureGroup.Children.Add(snout);
                creatureGroup.Children.Add(leftEye);
                creatureGroup.Children.Add(rightEye);
                creatureGroup.Children.Add(leftPupil);
                creatureGroup.Children.Add(rightPupil);
                creatureGroup.Children.Add(nose);

                Canvas.SetLeft(leftEar, CarnivoreLeftEarX);
                Canvas.SetTop(leftEar, CarnivoreEarY);
                Canvas.SetLeft(rightEar, CarnivoreRightEarX);
                Canvas.SetTop(rightEar, CarnivoreEarY);
                Canvas.SetLeft(body, CarnivoreBodyX);
                Canvas.SetTop(body, CarnivoreBodyY);
                Canvas.SetLeft(snout, CarnivoreSnoutX);
                Canvas.SetTop(snout, CarnivoreSnoutY);
                Canvas.SetLeft(leftEye, CarnivoreLeftEyeX);
                Canvas.SetTop(leftEye, CarnivoreEyesY);
                Canvas.SetLeft(rightEye, CarnivoreRightEyeX);
                Canvas.SetTop(rightEye, CarnivoreEyesY);
                Canvas.SetLeft(leftPupil, CarnivoreLeftPupilX);
                Canvas.SetTop(leftPupil, CarnivorePupilsY);
                Canvas.SetLeft(rightPupil, CarnivoreRightPupilX);
                Canvas.SetTop(rightPupil, CarnivorePupilsY);
                Canvas.SetLeft(nose, CarnivoreNoseX);
                Canvas.SetTop(nose, CarnivoreNoseY);
            }

            _canvas.Children.Add(creatureGroup);
            _entityVisuals[creature.Id] = creatureGroup;
        }
    }

    /// <summary>
    /// Updates creature visual position and appearance.
    /// </summary>
    private void UpdateCreatureVisual(Creature creature, Point mousePosition, bool mouseInCanvas)
    {
        if (!_entityVisuals.TryGetValue(creature.Id, out var visual))
        {
            return;
        }

        Canvas.SetLeft(visual, creature.X - CreatureAnchorOffset);
        Canvas.SetTop(visual, creature.Y - CreatureAnchorOffset);

        // Update opacity based on health
        double healthRatio = creature.Health / PercentMax;
        visual.Opacity = creature.IsAlive ? healthRatio : CreatureDeadOpacity;

        // Apply mouse glow effect
        if (mouseInCanvas && visual is Canvas glowCanvas)
        {
            double distance = Math.Sqrt(Math.Pow(mousePosition.X - creature.X, 2) +
                                       Math.Pow(mousePosition.Y - creature.Y, 2));
            double glowIntensity = Math.Max(0, 1 - distance / 80); // Glow within 80 pixels for creatures

            if (glowIntensity > 0)
            {
                var glowEffect = new DropShadowEffect
                {
                    Color = creature is Herbivore ? Colors.LightBlue : Colors.Red,
                    BlurRadius = 15 * glowIntensity,
                    ShadowDepth = 0,
                    Opacity = glowIntensity * 0.7
                };
                glowCanvas.Effect = glowEffect;
            }
            else
            {
                glowCanvas.Effect = null;
            }
        }
        else if (visual is Canvas noGlowCanvas)
        {
            noGlowCanvas.Effect = null;
        }

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
        _expiredShakeIdsBuffer.Clear();
        _shakeKeyBuffer.Clear();

        foreach (var id in _plantShakeTimers.Keys)
        {
            _shakeKeyBuffer.Add(id);
        }

        foreach (var id in _shakeKeyBuffer)
        {
            double newTime = _plantShakeTimers[id] - ApproxFrameDeltaSecondsAt60Fps;
            if (newTime <= 0)
            {
                _expiredShakeIdsBuffer.Add(id);
            }
            else
            {
                _plantShakeTimers[id] = newTime;
            }
        }

        foreach (var id in _expiredShakeIdsBuffer)
        {
            _plantShakeTimers.Remove(id);
        }
    }

    /// <summary>
    /// Removes visuals for dead entities.
    /// </summary>
    private void CleanupDeadEntities(World world)
    {
        _aliveIdsBuffer.Clear();
        foreach (var entity in world.GetAllEntities())
        {
            _aliveIdsBuffer.Add(entity.Id);
        }

        _deadIdsBuffer.Clear();
        foreach (var id in _entityVisuals.Keys)
        {
            if (!_aliveIdsBuffer.Contains(id))
            {
                _deadIdsBuffer.Add(id);
            }
        }

        foreach (var id in _deadIdsBuffer)
        {
            if (_entityVisuals.TryGetValue(id, out var visual))
            {
                _canvas.Children.Remove(visual);
                _entityVisuals.Remove(id);
                _plantShakeTimers.Remove(id);
            }
        }
    }

    private static SolidColorBrush CreateFrozenBrush(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }
}
