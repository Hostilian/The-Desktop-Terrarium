namespace Terrarium.Logic.Interfaces;

    /// <summary>
    /// Interface for entities that can move around the world.
    /// </summary>
    public interface IMovable
    {
        /// <summary>
        /// Current X velocity.
        /// </summary>
        public double VelocityX { get; set; }

        /// <summary>
        /// Current Y velocity.
        /// </summary>
        public double VelocityY { get; set; }

        /// <summary>
        /// Maximum movement speed.
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
