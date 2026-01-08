using Terrarium.Logic.Entities;

namespace Terrarium.Logic.Simulation
{
    /// <summary>
    /// Handles collision detection between entities.
    /// Separates collision logic from entity classes.
    /// </summary>
    public class CollisionDetector
    {
        // Collision radius constants
        private const double PlantCollisionRadius = 15.0;
        private const double CreatureCollisionRadius = 20.0;

        /// <summary>
        /// Checks if two entities are colliding.
        /// </summary>
        public bool AreColliding(WorldEntity entity1, WorldEntity entity2)
        {
            double radius1 = GetCollisionRadius(entity1);
            double radius2 = GetCollisionRadius(entity2);
            double distance = entity1.DistanceTo(entity2);

            return distance < (radius1 + radius2);
        }

        /// <summary>
        /// Gets the collision radius for an entity.
        /// </summary>
        private double GetCollisionRadius(WorldEntity entity)
        {
            return entity switch
            {
                Plant plant => PlantCollisionRadius + plant.Size * 0.5,
                Creature => CreatureCollisionRadius,
                _ => 10.0
            };
        }

        /// <summary>
        /// Finds all entities within a certain radius.
        /// </summary>
        public List<T> FindNearbyEntities<T>(WorldEntity center, IEnumerable<T> entities, double radius)
            where T : WorldEntity
        {
            var nearby = new List<T>();

            foreach (var entity in entities)
            {
                if (entity.Id == center.Id) continue;

                double distance = center.DistanceTo(entity);
                if (distance <= radius)
                {
                    nearby.Add(entity);
                }
            }

            return nearby;
        }

        /// <summary>
        /// Resolves collision between two creatures (pushes them apart).
        /// </summary>
        public void ResolveCreatureCollision(Creature creature1, Creature creature2)
        {
            double dx = creature2.X - creature1.X;
            double dy = creature2.Y - creature1.Y;
            double distance = Math.Sqrt(dx * dx + dy * dy);

            if (distance < CreatureCollisionRadius * 2 && distance > 0)
            {
                // Push creatures apart
                double pushForce = 0.5;
                double pushX = (dx / distance) * pushForce;
                double pushY = (dy / distance) * pushForce;

                creature1.X -= pushX;
                creature1.Y -= pushY;
                creature2.X += pushX;
                creature2.Y += pushY;
            }
        }
    }
}
