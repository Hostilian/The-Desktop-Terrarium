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

        public MovementCalculator(World world) : this(world, random: null) { }

        public MovementCalculator(World world, Random? random)
        {
            _world = world;
            _random = random ?? new Random();
        }

        public void UpdateWandering(Creature creature, double deltaTime)
        {
            _wanderTimer += deltaTime;
            if (_wanderTimer >= WanderChangeInterval)
            {
                _wanderTimer = 0;
                RandomizeDirection(creature);
            }
            EnforceBoundaries(creature);
        }

        public void RandomizeDirection(Creature creature)
        {
            double angle = _random.NextDouble() * Math.PI * 2;
            creature.SetDirection(Math.Cos(angle), Math.Sin(angle));
        }

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

            if (bounced && entity is Creature creature)
            {
                creature.VelocityX = -creature.VelocityX;
                creature.VelocityY = -creature.VelocityY;
            }
        }

        public void MoveToward(Creature creature, double targetX, double targetY, double slowingRadius = DefaultSlowingRadius)
        {
            double dx = targetX - creature.X;
            double dy = targetY - creature.Y;
            double distance = Math.Sqrt(dx * dx + dy * dy);

            if (distance > 0)
            {
                double speedMultiplier = distance < slowingRadius ? distance / slowingRadius : 1.0;
                creature.SetDirection(dx, dy);
                creature.VelocityX *= speedMultiplier;
                creature.VelocityY *= speedMultiplier;
            }
        }
    }
}
