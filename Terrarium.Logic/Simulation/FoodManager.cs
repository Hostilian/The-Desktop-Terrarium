namespace Terrarium.Logic.Simulation
{
    using Terrarium.Logic.Entities;

    /// <summary>
    /// Manages food spawning and ecosystem balance.
    /// Prevents "God Object" by separating resource management.
    /// </summary>
    public class FoodManager
    {
        private readonly World world;
        private readonly Random random;

        // Food spawning constants
        private const int MinPlantCount = 5;
        private const int MaxPlantCount = 20;
        private const double PlantSpawnInterval = 5.0;
        private const double PlantSpawnChance = 0.3;

        private double plantSpawnTimer;

        /// <summary>
        /// Gets or sets multiplier applied to the base plant spawn chance (clamped).
        /// Useful for seasonal effects and balancing.
        /// </summary>
        public double PlantSpawnChanceMultiplier { get; set; } = 1.0;

        public FoodManager(World world, Random? random = null)
        {
            this.world = world;
            this.random = random ?? new Random();
            plantSpawnTimer = 0;
        }

        /// <summary>
        /// Updates food spawning logic.
        /// </summary>
        public void Update(double deltaTime)
        {
            plantSpawnTimer += deltaTime;

            if (plantSpawnTimer >= PlantSpawnInterval)
            {
                plantSpawnTimer = 0;
                TrySpawnPlant();
            }
        }

        private void TrySpawnPlant()
        {
            int plantCount = world.Plants.Count;
            double adjustedChance = Math.Clamp(PlantSpawnChance * PlantSpawnChanceMultiplier, 0.0, 1.0);

            if (plantCount < MinPlantCount || (plantCount < MaxPlantCount && random.NextDouble() < adjustedChance))
                world.SpawnRandomPlant();
        }

        public void InitializeStartingFood()
        {
            for (int i = 0; i < MinPlantCount; i++)
                world.SpawnRandomPlant();
        }

        public bool IsEcosystemBalanced()
        {
            int plantCount = world.Plants.Count;
            int herbivoreCount = world.Herbivores.Count;
            int carnivoreCount = world.Carnivores.Count;
            return plantCount >= herbivoreCount && herbivoreCount >= carnivoreCount;
        }

        public double GetEcosystemHealth() => EcosystemHealthScorer.CalculateHealth01(
            world.Plants.Count,
            world.Herbivores.Count,
            world.Carnivores.Count);
    }
}
