namespace Terrarium.Logic.Entities
{
    /// <summary>
    /// Base class for all entities in the simulation world.
    /// Represents anything that exists at a position in the world.
    /// </summary>
    public abstract class WorldEntity
    {
        private static int _nextId = -1;
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

        protected WorldEntity(double x, double y, int? id = null)
        {
            if (id.HasValue)
            {
                _id = id.Value;
                EnsureNextIdAtLeast(_id);
            }
            else
            {
                _id = System.Threading.Interlocked.Increment(ref _nextId);
            }
            _x = x;
            _y = y;
        }

        private static void EnsureNextIdAtLeast(int restoredId)
        {
            while (true)
            {
                int current = System.Threading.Volatile.Read(ref _nextId);
                if (current >= restoredId)
                {
                    return;
                }

                if (System.Threading.Interlocked.CompareExchange(ref _nextId, restoredId, current) == current)
                {
                    return;
                }
            }
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
