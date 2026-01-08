using Terrarium.Logic.Entities;

namespace Terrarium.Logic.Simulation
{
    /// <summary>
    /// Handles movement calculations and boundary checking.
    /// Prevents the "God Object" anti-pattern by separating movement logic.
    /// </summary>
    public class MovementCalculator
    {
        private readonly World _world;
        private readonly Random _random;

        // Movement behavior constants
        private const double WanderChangeInterval = 2.0;
        private const double BoundaryPadding = 20.0;
        private const double DefaultSlowingRadius = 50.0;

        private double _wanderTimer;

        public MovementCalculator(World world)
        {
            _world = world;
            _random = new Random();
            _wanderTimer = 0;
        }

        /// <summary>
        /// Updates creature movement for wandering behavior.
        /// </summary>
        public void UpdateWandering(Creature creature, double deltaTime)
        {
            _wanderTimer += deltaTime;

            // Change direction periodically
            if (_wanderTimer >= WanderChangeInterval)
            {
                _wanderTimer = 0;
                RandomizeDirection(creature);
            }

            // Keep creature within world bounds
            EnforceBoundaries(creature);
        }

        /// <summary>
        /// Sets a random movement direction for a creature.
        /// </summary>
        public void RandomizeDirection(Creature creature)
        {
            double angle = _random.NextDouble() * Math.PI * 2;
            double dirX = Math.Cos(angle);
            double dirY = Math.Sin(angle);
            creature.SetDirection(dirX, dirY);
        }

        /// <summary>
        /// Keeps entities within world boundaries.
        /// </summary>
        public void EnforceBoundaries(WorldEntity entity)
        {
            bool bounced = false;

            if (entity.X < BoundaryPadding)
            {
                entity.X = BoundaryPadding;
                bounced = true;
            }
            else if (entity.X > _world.Width - BoundaryPadding)
            {
                entity.X = _world.Width - BoundaryPadding;
                bounced = true;
            }

            if (entity.Y < BoundaryPadding)
            {
                entity.Y = BoundaryPadding;
                bounced = true;
            }
            else if (entity.Y > _world.Height - BoundaryPadding)
            {
                entity.Y = _world.Height - BoundaryPadding;
                bounced = true;
            }

            // Reverse direction if creature hit boundary
            if (bounced && entity is Creature creature)
            {
                creature.VelocityX = -creature.VelocityX;
                creature.VelocityY = -creature.VelocityY;
            }
        }

        /// <summary>
        /// Calculates movement toward a target with arrival behavior.
        /// </summary>
        public void MoveToward(Creature creature, double targetX, double targetY, double slowingRadius = DefaultSlowingRadius)
        {
            double dx = targetX - creature.X;
            double dy = targetY - creature.Y;
            double distance = Math.Sqrt(dx * dx + dy * dy);

            if (distance > 0)
            {
                // Slow down when approaching target
                double speedMultiplier = 1.0;
                if (distance < slowingRadius)
                {
                    speedMultiplier = distance / slowingRadius;
                }

                creature.SetDirection(dx, dy);
                creature.VelocityX *= speedMultiplier;
                creature.VelocityY *= speedMultiplier;
            }
        }
    }
}
