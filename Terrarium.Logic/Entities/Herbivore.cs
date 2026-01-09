namespace Terrarium.Logic.Entities
{
    /// <summary>
    /// Represents a herbivore creature that eats plants.
    /// </summary>
    public class Herbivore : Creature
    {
        private const double HerbivoreSpeed = 40.0;
        private const double PlantNutritionValue = 30.0;
        private const double EatingRange = 15.0;
        private const double DefaultPlantDetectionRange = 200.0;

        /// <summary>
        /// Type name for the herbivore (e.g., "Sheep", "Rabbit").
        /// </summary>
        public string Type { get; set; }

        public Herbivore(double x, double y, string type = "Sheep")
            : base(x, y, HerbivoreSpeed)
        {
            Type = type;
        }

        /// <summary>
        /// Attempts to eat a plant if within range.
        /// </summary>
        public bool TryEat(Plant plant)
        {
            if (plant == null || !plant.IsAlive)
                return false;

            double distance = DistanceTo(plant);
            if (distance <= EatingRange)
            {
                // Eat the plant
                double nutritionGained = Math.Min(plant.Size, PlantNutritionValue);
                Feed(nutritionGained);

                // Damage the plant
                plant.TakeDamage(nutritionGained);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Finds the nearest plant within detection range.
        /// </summary>
        public Plant? FindNearestPlant(IEnumerable<Plant> plants, double detectionRange = DefaultPlantDetectionRange)
        {
            Plant? nearest = null;
            double minDistance = detectionRange;

            foreach (var plant in plants)
            {
                if (!plant.IsAlive)
                    continue;

                double distance = DistanceTo(plant);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = plant;
                }
            }

            return nearest;
        }

        /// <summary>
        /// Moves toward a target location.
        /// </summary>
        public void MoveToward(double targetX, double targetY)
        {
            double dx = targetX - X;
            double dy = targetY - Y;
            SetDirection(dx, dy);
        }
    }
}
