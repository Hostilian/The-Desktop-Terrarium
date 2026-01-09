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
        private const double PlantSizeRadiusMultiplier = 0.5;
        private const double DefaultCollisionRadius = 10.0;

        // Collision resolution constants
        private const double MinCollisionDistance = 0.1;
        private const double ArbitraryPushDirectionX = 1.0;
        private const double ArbitraryPushDirectionY = 0.5;
        private const double CreaturePushForce = 0.5;

        // Spatial hashing constants (for performance)
        private const double CreatureCollisionCellSizeMultiplier = 2.0;

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
                Plant plant => PlantCollisionRadius + plant.Size * PlantSizeRadiusMultiplier,
                Creature => CreatureCollisionRadius,
                _ => DefaultCollisionRadius
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
                if (entity.Id == center.Id)
                    continue;

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

            // Handle collision (including zero distance)
            if (distance < CreatureCollisionRadius * 2)
            {
                // If creatures are at exact same position, push them in opposite directions
                if (distance == 0)
                {
                    distance = MinCollisionDistance; // Small value to prevent division by zero
                    dx = ArbitraryPushDirectionX; // Push in arbitrary directions
                    dy = ArbitraryPushDirectionY;
                }

                // Push creatures apart
                double pushX = (dx / distance) * CreaturePushForce;
                double pushY = (dy / distance) * CreaturePushForce;

                creature1.X -= pushX;
                creature1.Y -= pushY;
                creature2.X += pushX;
                creature2.Y += pushY;
            }
        }

        /// <summary>
        /// Resolves collisions between a set of creatures using spatial hashing to reduce comparisons.
        /// </summary>
        public void ResolveCreatureCollisions(IReadOnlyList<Creature> creatures)
        {
            if (creatures == null)
                throw new ArgumentNullException(nameof(creatures));

            if (creatures.Count <= 1)
                return;

            double cellSize = CreatureCollisionRadius * CreatureCollisionCellSizeMultiplier;
            if (cellSize <= 0)
                return;

            var grid = new Dictionary<(int x, int y), List<Creature>>();

            foreach (var creature in creatures)
            {
                if (!creature.IsAlive)
                    continue;

                var key = GetCellKey(creature.X, creature.Y, cellSize);
                if (!grid.TryGetValue(key, out var list))
                {
                    list = new List<Creature>();
                    grid[key] = list;
                }

                list.Add(creature);
            }

            // Within-cell pairs
            foreach (var cell in grid.Values)
            {
                ResolvePairsInSameCell(cell);
            }

            // Cross-cell pairs: only check a subset of neighbors to avoid double-processing.
            foreach (var kvp in grid)
            {
                var key = kvp.Key;
                var cellCreatures = kvp.Value;

                CheckNeighborCell(key, (1, 0), cellCreatures, grid);
                CheckNeighborCell(key, (0, 1), cellCreatures, grid);
                CheckNeighborCell(key, (1, 1), cellCreatures, grid);
                CheckNeighborCell(key, (-1, 1), cellCreatures, grid);
            }
        }

        private static (int x, int y) GetCellKey(double x, double y, double cellSize)
        {
            int cellX = (int)Math.Floor(x / cellSize);
            int cellY = (int)Math.Floor(y / cellSize);
            return (cellX, cellY);
        }

        private void ResolvePairsInSameCell(List<Creature> cell)
        {
            for (int i = 0; i < cell.Count; i++)
            {
                var a = cell[i];
                if (!a.IsAlive)
                    continue;

                for (int j = i + 1; j < cell.Count; j++)
                {
                    var b = cell[j];
                    if (!b.IsAlive)
                        continue;

                    if (AreColliding(a, b))
                    {
                        ResolveCreatureCollision(a, b);
                    }
                }
            }
        }

        private void CheckNeighborCell(
            (int x, int y) key,
            (int dx, int dy) offset,
            List<Creature> cellCreatures,
            Dictionary<(int x, int y), List<Creature>> grid)
        {
            var neighborKey = (key.x + offset.dx, key.y + offset.dy);
            if (!grid.TryGetValue(neighborKey, out var neighbor))
                return;

            for (int i = 0; i < cellCreatures.Count; i++)
            {
                var a = cellCreatures[i];
                if (!a.IsAlive)
                    continue;

                for (int j = 0; j < neighbor.Count; j++)
                {
                    var b = neighbor[j];
                    if (!b.IsAlive)
                        continue;

                    if (AreColliding(a, b))
                    {
                        ResolveCreatureCollision(a, b);
                    }
                }
            }
        }
    }
}
