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

        public bool AreColliding(WorldEntity entity1, WorldEntity entity2) =>
            entity1.DistanceTo(entity2) < (GetCollisionRadius(entity1) + GetCollisionRadius(entity2));

        private double GetCollisionRadius(WorldEntity entity) => entity switch
        {
            Plant plant => PlantCollisionRadius + plant.Size * PlantSizeRadiusMultiplier,
            Creature => CreatureCollisionRadius,
            _ => DefaultCollisionRadius
        };

        public List<T> FindNearbyEntities<T>(WorldEntity center, IEnumerable<T> entities, double radius)
            where T : WorldEntity
        {
            var nearby = new List<T>();
            foreach (var entity in entities)
            {
                if (entity.Id != center.Id && center.DistanceTo(entity) <= radius)
                    nearby.Add(entity);
            }
            return nearby;
        }

        public void ResolveCreatureCollision(Creature creature1, Creature creature2)
        {
            double dx = creature2.X - creature1.X;
            double dy = creature2.Y - creature1.Y;
            double distance = Math.Sqrt(dx * dx + dy * dy);

            if (distance >= CreatureCollisionRadius * 2) return;

            if (distance == 0)
            {
                distance = MinCollisionDistance;
                dx = ArbitraryPushDirectionX;
                dy = ArbitraryPushDirectionY;
            }

            double pushX = (dx / distance) * CreaturePushForce;
            double pushY = (dy / distance) * CreaturePushForce;

            creature1.X -= pushX;
            creature1.Y -= pushY;
            creature2.X += pushX;
            creature2.Y += pushY;
        }

        public void ResolveCreatureCollisions(IReadOnlyList<Creature> creatures)
        {
            ArgumentNullException.ThrowIfNull(creatures);
            if (creatures.Count <= 1) return;

            double cellSize = CreatureCollisionRadius * CreatureCollisionCellSizeMultiplier;
            if (cellSize <= 0) return;

            var grid = new Dictionary<(int x, int y), List<Creature>>();

            foreach (var creature in creatures)
            {
                if (!creature.IsAlive) continue;

                var key = GetCellKey(creature.X, creature.Y, cellSize);
                if (!grid.TryGetValue(key, out var list))
                {
                    list = new List<Creature>();
                    grid[key] = list;
                }
                list.Add(creature);
            }

            foreach (var cell in grid.Values)
                ResolvePairsInSameCell(cell);

            foreach (var kvp in grid)
            {
                CheckNeighborCell(kvp.Key, (1, 0), kvp.Value, grid);
                CheckNeighborCell(kvp.Key, (0, 1), kvp.Value, grid);
                CheckNeighborCell(kvp.Key, (1, 1), kvp.Value, grid);
                CheckNeighborCell(kvp.Key, (-1, 1), kvp.Value, grid);
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
                if (!a.IsAlive) continue;

                for (int j = i + 1; j < cell.Count; j++)
                {
                    var b = cell[j];
                    if (b.IsAlive && AreColliding(a, b))
                        ResolveCreatureCollision(a, b);
                }
            }
        }

        private void CheckNeighborCell((int x, int y) key, (int dx, int dy) offset, List<Creature> cellCreatures, Dictionary<(int x, int y), List<Creature>> grid)
        {
            var neighborKey = (key.x + offset.dx, key.y + offset.dy);
            if (!grid.TryGetValue(neighborKey, out var neighbor)) return;

            foreach (var a in cellCreatures)
            {
                if (!a.IsAlive) continue;
                foreach (var b in neighbor)
                {
                    if (b.IsAlive && AreColliding(a, b))
                        ResolveCreatureCollision(a, b);
                }
            }
        }
    }
}
