namespace Terrarium.Logic.Entities
{
    /// <summary>
    /// Represents a carnivore creature that hunts herbivores.
    /// </summary>
    public class Carnivore : Creature
    {
        private const double CarnivoreSpeed = 60.0;
        private const double PreyNutritionValue = 50.0;
        private const double AttackRange = 20.0;
        private const double AttackDamage = 80.0;
        private const double DefaultPreyDetectionRange = 300.0;

        /// <summary>
        /// Type name for the carnivore (e.g., "Wolf", "Fox").
        /// </summary>
        public string Type { get; set; }

        public Carnivore(double x, double y, string type = "Wolf")
            : base(x, y, CarnivoreSpeed)
        {
            Type = type;
        }

        public bool TryEat(Herbivore prey)
        {
            if (prey == null || !prey.IsAlive || DistanceTo(prey) > AttackRange)
                return false;

            prey.TakeDamage(AttackDamage);
            if (!prey.IsAlive)
            {
                Feed(PreyNutritionValue);
                return true;
            }
            return false;
        }

        public Herbivore? FindNearestPrey(IEnumerable<Herbivore> herbivores, double detectionRange = DefaultPreyDetectionRange)
        {
            Herbivore? nearest = null;
            double minDistance = detectionRange;

            foreach (var herbivore in herbivores)
            {
                if (!herbivore.IsAlive) continue;
                double distance = DistanceTo(herbivore);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = herbivore;
                }
            }

            return nearest;
        }

        public void Hunt(Creature target)
        {
            if (target != null && target.IsAlive)
                SetDirection(target.X - X, target.Y - Y);
        }
    }
}
