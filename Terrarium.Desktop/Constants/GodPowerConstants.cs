namespace Terrarium.Desktop.Constants;

/// <summary>
/// Constants for God Power mechanics and effects.
/// These values are carefully balanced for gameplay and should not be changed without testing.
/// </summary>
public static class GodPowerConstants
{
    /// <summary>
    /// Damage dealt to each entity struck by lightning.
    /// Balanced to remove approximately 30% health from an average creature.
    /// </summary>
    public const double LIGHTNING_STRIKE_DAMAGE = 30.0;

    /// <summary>
    /// Number of random entities targeted by a single lightning strike.
    /// Limited to prevent excessive casualties in one strike.
    /// </summary>
    public const int LIGHTNING_STRIKE_TARGET_COUNT = 3;

    /// <summary>
    /// Base damage dealt at the center of a meteor impact.
    /// Damage decreases with distance from impact point.
    /// </summary>
    public const double METEOR_BASE_DAMAGE = 50.0;

    /// <summary>
    /// Number of meteors in a meteor shower event.
    /// Balanced to create significant but not overwhelming destruction.
    /// </summary>
    public const int METEOR_SHOWER_COUNT = 5;

    /// <summary>
    /// Radius in pixels within which entities take damage from a meteor.
    /// Larger radius = more area of effect damage.
    /// </summary>
    public const double METEOR_IMPACT_RADIUS_PIXELS = 50.0;

    /// <summary>
    /// Initial damage dealt to creatures infected by plague.
    /// Lower than instant-kill effects to allow spread/recovery dynamics.
    /// </summary>
    public const double PLAGUE_INITIAL_DAMAGE = 25.0;

    /// <summary>
    /// Maximum number of creatures infected when plague is unleashed.
    /// Limited to prevent population collapse from a single event.
    /// </summary>
    public const int PLAGUE_INFECTION_COUNT = 5;

    /// <summary>
    /// Multiplier applied to reproduction rates during fertility blessing.
    /// Doubles the chance of offspring being produced.
    /// </summary>
    public const double FERTILITY_BLESSING_MULTIPLIER = 2.0;

    /// <summary>
    /// Duration in seconds that fertility blessing remains active.
    /// Long enough to see significant population growth.
    /// </summary>
    public const double FERTILITY_BLESSING_DURATION_SECONDS = 30.0;

    /// <summary>
    /// Number of plants spawned when abundance blessing is activated.
    /// Provides substantial food boost without overcrowding the terrain.
    /// </summary>
    public const int ABUNDANCE_PLANT_COUNT = 10;

    /// <summary>
    /// Maximum number of creatures affected by corruption event.
    /// Limited to create interesting faction dynamics without chaos.
    /// </summary>
    public const int CORRUPTION_TARGET_COUNT = 3;

    /// <summary>
    /// Duration in seconds that divine protection remains active.
    /// Provides temporary safety for creatures.
    /// </summary>
    public const double DIVINE_PROTECTION_DURATION_SECONDS = 30.0;

    /// <summary>
    /// Damage dealt to each plant during famine.
    /// Significant but not instant kill to allow recovery.
    /// </summary>
    public const double FAMINE_DAMAGE = 40.0;

    /// <summary>
    /// Maximum number of creatures driven mad by madness curse.
    /// Limited to create behavioral chaos without total breakdown.
    /// </summary>
    public const int MADNESS_TARGET_COUNT = 5;

    /// <summary>
    /// Damage dealt to creatures affected by madness.
    /// Moderate damage to represent psychological toll.
    /// </summary>
    public const double MADNESS_DAMAGE = 20.0;

    /// <summary>
    /// Duration in seconds that stagnation curse remains active.
    /// Long enough to halt growth temporarily.
    /// </summary>
    public const double STAGNATION_DURATION_SECONDS = 60.0;

    /// <summary>
    /// Number of terrain areas converted to water during flood.
    /// Creates new aquatic zones without flooding everything.
    /// </summary>
    public const int FLOOD_AREA_COUNT = 10;
}
