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

        /// <summary>
        /// Attempts to attack and eat a herbivore.
        /// </summary>
        public bool TryEat(Herbivore prey)
        {
            if (prey == null || !prey.IsAlive)
                return false;

            double distance = DistanceTo(prey);
            if (distance <= AttackRange)
            {
                // Attack the prey
                prey.TakeDamage(AttackDamage);

                // If prey is dead, consume it
                if (!prey.IsAlive)
                {
                    Feed(PreyNutritionValue);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Finds the nearest herbivore within detection range.
        /// </summary>
        public Herbivore? FindNearestPrey(IEnumerable<Herbivore> herbivores, double detectionRange = DefaultPreyDetectionRange)
        {
            Herbivore? nearest = null;
            double minDistance = detectionRange;

            foreach (var herbivore in herbivores)
            {
                if (!herbivore.IsAlive)
                    continue;

                double distance = DistanceTo(herbivore);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = herbivore;
                }
            }

            return nearest;
        }

        /// <summary>
        /// Moves toward a target creature.
        /// </summary>
        public void Hunt(Creature target)
        {
            if (target != null && target.IsAlive)
            {
                double dx = target.X - X;
                double dy = target.Y - Y;
                SetDirection(dx, dy);
            }
        }
    }
}
