namespace Terrarium.Logic.Entities
{
    /// <summary>
    /// Base class for all entities in the simulation world.
    /// Represents anything that exists at a position in the world.
    /// </summary>
    public abstract class WorldEntity
    {
        private static int _nextId = 0;
        private readonly int _id;
        private double _x;
        private double _y;

        /// <summary>
        /// Unique identifier for this entity.
        /// </summary>
        public int Id => _id;

        /// <summary>
        /// X coordinate position in the world.
        /// </summary>
        public double X
        {
            get => _x;
            set => _x = value;
        }

        /// <summary>
        /// Y coordinate position in the world.
        /// </summary>
        public double Y
        {
            get => _y;
            set => _y = value;
        }

        protected WorldEntity(double x, double y)
        {
            _id = _nextId++;
            _x = x;
            _y = y;
        }

        /// <summary>
        /// Updates the entity's state for one simulation tick.
        /// </summary>
        public abstract void Update(double deltaTime);

        /// <summary>
        /// Calculates the distance to another entity.
        /// </summary>
        public double DistanceTo(WorldEntity other)
        {
            double dx = X - other.X;
            double dy = Y - other.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }
}
