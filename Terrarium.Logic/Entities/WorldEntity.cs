namespace Terrarium.Logic.Entities
{
    /// <summary>
    /// Base class for all entities in the simulation world.
    /// Represents anything that exists at a position in the world.
    /// </summary>
    public abstract class WorldEntity
    {
        private static int nextId = 0;
        private readonly int id;
        private double x;
        private double y;
        private string type;

        /// <summary>
        /// Gets unique identifier for this entity.
        /// </summary>
        public int Id => id;

        /// <summary>
        /// Gets or sets x coordinate position in the world.
        /// </summary>
        public double X
        {
            get => x;
            set => x = value;
        }

        /// <summary>
        /// Gets or sets y coordinate position in the world.
        /// </summary>
        public double Y
        {
            get => y;
            set => y = value;
        }

        /// <summary>
        /// Gets type of the entity (e.g., "Tree", "Deer").
        /// </summary>
        public string Type => type;

        protected WorldEntity(double x, double y, string type)
        {
            id = nextId++;
            this.x = x;
            this.y = y;
            this.type = type;
        }

        /// <summary>
        /// Updates the entity's state for one simulation tick.
        /// </summary>
        public abstract void Update(double deltaTime);

        /// <summary>
        /// Calculates the distance to another entity.
        /// </summary>
        /// <returns></returns>
        public double DistanceTo(WorldEntity other)
        {
            double dx = X - other.X;
            double dy = Y - other.Y;
            return Math.Sqrt((dx * dx) + (dy * dy));
        }
    }
}
