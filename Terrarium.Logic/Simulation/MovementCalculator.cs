namespace Terrarium.Logic.Simulation
{
    using Terrarium.Logic.Entities;

    /// <summary>
    /// Handles movement calculations and boundary checking.
    /// Prevents the "God Object" anti-pattern by separating movement logic.
    /// </summary>
    public class MovementCalculator
    {
        private readonly World world;
        private readonly Random random;

        // Movement behavior constants
        private const double WanderChangeInterval = 2.0;
        private const double BoundaryPadding = 20.0;
        private const double DefaultSlowingRadius = 50.0;

        private double wanderTimer;

        public MovementCalculator(World world)
            : this(world, random: null)
        {
        }

        public MovementCalculator(World world, Random? random)
        {
            this.world = world;
            this.random = random ?? new Random();
        }

        public void UpdateWandering(Creature creature, double deltaTime)
        {
            wanderTimer += deltaTime;
            if (wanderTimer >= WanderChangeInterval)
            {
                wanderTimer = 0;
                RandomizeDirection(creature);
            }
            EnforceBoundaries(creature);
        }

        public void UpdateExploration(Creature creature, double deltaTime)
        {
            wanderTimer += deltaTime;
            if (wanderTimer >= WanderChangeInterval * 0.5) // More frequent direction changes for exploration
            {
                wanderTimer = 0;

                // Bias toward unexplored areas (away from center for boundary exploration)
                double centerX = world.Width / 2;
                double centerY = world.Height / 2;
                double distanceFromCenter = Math.Sqrt(Math.Pow(creature.X - centerX, 2) + Math.Pow(creature.Y - centerY, 2));

                if (distanceFromCenter < 100) // If near center, explore outward
                {
                    double angle = random.NextDouble() * Math.PI * 2;
                    creature.SetDirection(Math.Cos(angle), Math.Sin(angle));
                }
                else // If far from center, sometimes head back toward interesting areas
                {
                    if (random.NextDouble() < 0.3) // 30% chance to head toward center
                    {
                        creature.SetDirection(centerX - creature.X, centerY - creature.Y);
                    }
                    else
                    {
                        RandomizeDirection(creature);
                    }
                }
            }
            EnforceBoundaries(creature);
        }

        public void RandomizeDirection(Creature creature)
        {
            if (creature is null)
            {
                throw new ArgumentNullException(nameof(creature));
            }

            double angle = random.NextDouble() * Math.PI * 2;
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
            else if (entity.X > world.Width - BoundaryPadding)
            {
                entity.X = world.Width - BoundaryPadding;
                bounced = true;
            }

            if (entity.Y < BoundaryPadding)
            {
                entity.Y = BoundaryPadding;
                bounced = true;
            }
            else if (entity.Y > world.Height - BoundaryPadding)
            {
                entity.Y = world.Height - BoundaryPadding;
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
            double distance = Math.Sqrt((dx * dx) + (dy * dy));

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
