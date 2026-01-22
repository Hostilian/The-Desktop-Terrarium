namespace Terrarium.Desktop.Rendering;

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Terrarium.Logic.Entities;

/// <summary>
/// Displays emoji indicators above creatures showing their current state/mood.
/// </summary>
public class CreatureMoodIndicator
{
    private readonly Canvas canvas;
    private readonly Dictionary<Creature, MoodVisual> moodVisuals;
    private double updateTimer;

    private readonly HashSet<Creature> existingCreaturesBuffer = new();
    private readonly List<Creature> toRemoveBuffer = new();

    private const double UpdateInterval = 0.5; // Update moods every 0.5 seconds
    private const double IndicatorOffsetY = -25;
    private const double FadeInDuration = 0.2;

    private const double IndicatorOffsetX = -8;
    private const double MoodFontSize = 14;
    private const int MoodZIndex = 500;

    private const double CriticalHealthThreshold = 20.0;
    private const double StarvingHungerThreshold = 80.0;
    private const double HungryHungerThreshold = 50.0;

    private const double HerbivoreFleeSpeedThreshold = 30.0;
    private const double CarnivoreHuntSpeedThreshold = 20.0;

    private const double HappyHealthThreshold = 80.0;
    private const double HappyHungerThreshold = 30.0;
    private const double NeutralHealthThreshold = 50.0;

    public bool IsEnabled { get; set; } = true;

    public bool IsVisible { get; set; } = true;

    public CreatureMoodIndicator(Canvas canvas)
    {
        this.canvas = canvas;
        moodVisuals = new Dictionary<Creature, MoodVisual>();
        updateTimer = 0;
    }

    /// <summary>
    /// Updates mood indicators for all creatures.
    /// </summary>
    public void Update(double deltaTime, IEnumerable<Herbivore> herbivores, IEnumerable<Carnivore> carnivores)
    {
        if (!IsEnabled)
        {
            return;
        }

        if (!IsVisible)
        {
            return;
        }

        updateTimer += deltaTime;
        if (updateTimer < UpdateInterval)
        {
            return;
        }

        updateTimer = 0;

        // Track which creatures still exist
        existingCreaturesBuffer.Clear();

        foreach (var herbivore in herbivores)
        {
            if (!herbivore.IsAlive)
            {
                continue;
            }

            existingCreaturesBuffer.Add(herbivore);
            string mood = GetMoodEmoji(herbivore);
            UpdateOrCreateMoodVisual(herbivore, mood);
        }

        foreach (var carnivore in carnivores)
        {
            if (!carnivore.IsAlive)
            {
                continue;
            }

            existingCreaturesBuffer.Add(carnivore);
            string mood = GetMoodEmoji(carnivore);
            UpdateOrCreateMoodVisual(carnivore, mood);
        }

        // Remove visuals for dead/removed creatures
        toRemoveBuffer.Clear();
        foreach (var kvp in moodVisuals)
        {
            if (!existingCreaturesBuffer.Contains(kvp.Key))
            {
                canvas.Children.Remove(kvp.Value.Visual);
                toRemoveBuffer.Add(kvp.Key);
            }
            else
            {
                Canvas.SetLeft(kvp.Value.Visual, kvp.Key.X + IndicatorOffsetX);
                Canvas.SetTop(kvp.Value.Visual, kvp.Key.Y + IndicatorOffsetY);
            }
        }

        foreach (var creature in toRemoveBuffer)
        {
            moodVisuals.Remove(creature);
        }
    }

    private string GetMoodEmoji(Creature creature)
    {
        // Priority-based mood selection
        if (creature.Health < CriticalHealthThreshold)
        {
            return "💔"; // Very low health
        }

        if (creature.Hunger > StarvingHungerThreshold)
        {
            return "🍖"; // Starving, needs food
        }

        if (creature.Hunger > HungryHungerThreshold)
        {
            return "😋"; // Hungry
        }

        // Check if being chased (for herbivores)
        if (creature is Herbivore && (Math.Abs(creature.VelocityX) > HerbivoreFleeSpeedThreshold || Math.Abs(creature.VelocityY) > HerbivoreFleeSpeedThreshold))
        {
            return "😰"; // Fleeing
        }

        // Check if hunting (for carnivores)
        if (creature is Carnivore && (Math.Abs(creature.VelocityX) > CarnivoreHuntSpeedThreshold || Math.Abs(creature.VelocityY) > CarnivoreHuntSpeedThreshold))
        {
            return "🎯"; // Hunting
        }

        if (creature.Health > HappyHealthThreshold && creature.Hunger < HappyHungerThreshold)
        {
            return "😊"; // Happy and well-fed
        }

        if (creature.Health > NeutralHealthThreshold)
        {
            return "😐"; // Neutral
        }

        return "😟"; // Worried (low-ish health)
    }

    private void UpdateOrCreateMoodVisual(Creature creature, string mood)
    {
        if (moodVisuals.TryGetValue(creature, out var existing))
        {
            if (existing.CurrentMood != mood)
            {
                existing.Visual.Text = mood;
                existing.CurrentMood = mood;
            }
        }
        else
        {
            var textBlock = new TextBlock
            {
                Text = mood,
                FontSize = MoodFontSize,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            Canvas.SetLeft(textBlock, creature.X + IndicatorOffsetX);
            Canvas.SetTop(textBlock, creature.Y + IndicatorOffsetY);
            Canvas.SetZIndex(textBlock, MoodZIndex);

            canvas.Children.Add(textBlock);
            moodVisuals[creature] = new MoodVisual { Visual = textBlock, CurrentMood = mood };
        }
    }

    /// <summary>
    /// Clears all mood indicators.
    /// </summary>
    public void Clear()
    {
        foreach (var kvp in moodVisuals)
        {
            canvas.Children.Remove(kvp.Value.Visual);
        }
        moodVisuals.Clear();
    }
}

internal class MoodVisual
{
    public TextBlock Visual { get; set; } = null!;

    public string CurrentMood { get; set; } = string.Empty;
}
