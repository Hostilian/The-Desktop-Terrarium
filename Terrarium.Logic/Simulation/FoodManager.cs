using Terrarium.Logic.Entities;

namespace Terrarium.Logic.Simulation
{
    /// <summary>
    /// Manages food spawning and ecosystem balance.
    /// Prevents "God Object" by separating resource management.
    /// </summary>
    public class FoodManager
    {
        private readonly World _world;
        private readonly Random _random;

        // Food spawning constants
        private const int MinPlantCount = 5;
        private const int MaxPlantCount = 20;
        private const double PlantSpawnInterval = 5.0;
        private const double PlantSpawnChance = 0.3;

        private double _plantSpawnTimer;

        /// <summary>
        /// Multiplier applied to the base plant spawn chance (clamped).
        /// Useful for seasonal effects and balancing.
        /// </summary>
        public double PlantSpawnChanceMultiplier { get; set; } = 1.0;

        public FoodManager(World world, Random? random = null)
        {
            _world = world;
            _random = random ?? new Random();
            _plantSpawnTimer = 0;
        }

        /// <summary>
        /// Updates food spawning logic.
        /// </summary>
        public void Update(double deltaTime)
        {
            _plantSpawnTimer += deltaTime;

            if (_plantSpawnTimer >= PlantSpawnInterval)
            {
                _plantSpawnTimer = 0;
                TrySpawnPlant();
            }
        }

        private void TrySpawnPlant()
        {
            int plantCount = _world.Plants.Count;
            double adjustedChance = Math.Clamp(PlantSpawnChance * PlantSpawnChanceMultiplier, 0.0, 1.0);

            if (plantCount < MinPlantCount || (plantCount < MaxPlantCount && _random.NextDouble() < adjustedChance))
                _world.SpawnRandomPlant();
        }

        public void InitializeStartingFood()
        {
            for (int i = 0; i < MinPlantCount; i++)
                _world.SpawnRandomPlant();
        }

        public bool IsEcosystemBalanced()
        {
            int plantCount = _world.Plants.Count;
            int herbivoreCount = _world.Herbivores.Count;
            int carnivoreCount = _world.Carnivores.Count;
            return plantCount >= herbivoreCount && herbivoreCount >= carnivoreCount;
        }

        public double GetEcosystemHealth() => EcosystemHealthScorer.CalculateHealth01(
            _world.Plants.Count,
            _world.Herbivores.Count,
            _world.Carnivores.Count);
    }
}
