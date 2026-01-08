namespace Terrarium.Logic.Interfaces
{
    /// <summary>
    /// Interface for entities that can move around the world.
    /// </summary>
    public interface IMovable
    {
        /// <summary>
        /// Current X velocity.
        /// </summary>
        double VelocityX { get; set; }

        /// <summary>
        /// Current Y velocity.
        /// </summary>
        double VelocityY { get; set; }

        /// <summary>
        /// Maximum movement speed.
        /// </summary>
        double Speed { get; }

        /// <summary>
        /// Updates the position based on current velocity.
        /// </summary>
        void Move(double deltaTime);

        /// <summary>
        /// Sets movement direction toward a target point.
        /// </summary>
        void SetDirection(double targetX, double targetY);
    }
}
