namespace Terrarium.Desktop.Constants;

/// <summary>
/// Constants for UI interaction and user experience.
/// </summary>
public static class UIConstants
{
    /// <summary>
    /// Tolerance radius in pixels for detecting entity clicks.
    /// Larger values make it easier to click small entities.
    /// Separate constants for different entity types due to varying visual sizes.
    /// </summary>
    public const double ENTITY_CLICK_TOLERANCE_PIXELS = 30.0;

    /// <summary>
    /// Click tolerance specifically for creatures (herbivores and carnivores).
    /// Slightly smaller than plants due to creatures being more visually distinct.
    /// </summary>
    public const double CREATURE_CLICK_TOLERANCE_PIXELS = 25.0;

    /// <summary>
    /// Default simulation speed multipliers available to the user.
    /// Provides common speed presets: normal, 2x, 5x, and 10x.
    /// </summary>
    public static readonly double[] SIMULATION_SPEED_PRESETS = { 1.0, 2.0, 5.0, 10.0 };
}
