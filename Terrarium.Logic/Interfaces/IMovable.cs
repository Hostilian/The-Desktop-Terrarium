namespace Terrarium.Logic.Interfaces;

/// <summary>
/// Interface for entities that can move around the world.
/// </summary>
public interface IMovable
{
    /// <summary>
    /// Gets or sets current X velocity.
    /// </summary>
    public double VelocityX { get; set; }

    /// <summary>
    /// Gets or sets current Y velocity.
    /// </summary>
    public double VelocityY { get; set; }

    /// <summary>
    /// Gets maximum movement speed.
    /// </summary>
    public double Speed { get; }

    /// <summary>
    /// Updates the position based on current velocity.
    /// </summary>
    public void Move(double deltaTime);

    /// <summary>
    /// Sets movement direction toward a target point.
    /// </summary>
    public void SetDirection(double targetX, double targetY);
}
