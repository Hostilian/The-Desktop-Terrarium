namespace Terrarium.Desktop.Constants;

/// <summary>
/// Constants for rendering and visual presentation.
/// </summary>
public static class RenderingConstants
{
    /// <summary>
    /// Target frames per second for the rendering loop.
    /// 60 FPS provides smooth animation and is the standard for desktop applications.
    /// Higher values increase CPU usage without perceptible quality improvement.
    /// </summary>
    public const int DEFAULT_RENDER_FPS = 60;

    /// <summary>
    /// Milliseconds per second constant for time calculations.
    /// Used to convert between seconds and milliseconds in timing logic.
    /// </summary>
    public const double MILLISECONDS_PER_SECOND = 1000.0;

    /// <summary>
    /// Interval in milliseconds at which the rendering timer fires.
    /// Calculated as 1000ms / 60fps ≈ 16.67ms per frame.
    /// </summary>
    public const double RENDER_INTERVAL_MS = MILLISECONDS_PER_SECOND / DEFAULT_RENDER_FPS;

    /// <summary>
    /// Interval in milliseconds for updating system monitoring statistics.
    /// 2000ms (2 seconds) balances responsiveness with performance overhead.
    /// Updates too frequently waste CPU; too slowly reduce usefulness.
    /// </summary>
    public const double SYSTEM_MONITOR_UPDATE_INTERVAL_MS = 2000.0;
}
