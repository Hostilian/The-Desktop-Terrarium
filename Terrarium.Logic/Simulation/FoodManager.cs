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

        private const double MinEcosystemHealth = 0.0;
        private const double MaxEcosystemHealth = 1.0;
        private const double IdealPlantRatio = 0.6;
        private const double IdealHerbivoreRatio = 0.3;
        private const double HealthAverageDivisor = 2.0;

        private double _plantSpawnTimer;

        public FoodManager(World world)
        {
            _world = world;
            _random = new Random();
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

        /// <summary>
        /// Attempts to spawn a new plant if conditions are met.
        /// </summary>
        private void TrySpawnPlant()
        {
            int plantCount = _world.Plants.Count;

            if (plantCount < MinPlantCount ||
                (plantCount < MaxPlantCount && _random.NextDouble() < PlantSpawnChance))
            {
                _world.SpawnRandomPlant();
            }
        }

        /// <summary>
        /// Initializes the world with starting plants.
        /// </summary>
        public void InitializeStartingFood()
        {
            for (int i = 0; i < MinPlantCount; i++)
            {
                _world.SpawnRandomPlant();
            }
        }

        /// <summary>
        /// Checks if the ecosystem is balanced.
        /// </summary>
        public bool IsEcosystemBalanced()
        {
            int plantCount = _world.Plants.Count;
            int herbivoreCount = _world.Herbivores.Count;
            int carnivoreCount = _world.Carnivores.Count;

            // Basic balance: plants > herbivores > carnivores
            return plantCount >= herbivoreCount &&
                   herbivoreCount >= carnivoreCount;
        }

        /// <summary>
        /// Gets ecosystem health status (0.0 to 1.0).
        /// </summary>
        public double GetEcosystemHealth()
        {
            int plantCount = _world.Plants.Count;
            int herbivoreCount = _world.Herbivores.Count;
            int totalCount = plantCount + herbivoreCount + _world.Carnivores.Count;

            if (totalCount == 0) return MinEcosystemHealth;

            // Ideal ratio: 60% plants, 30% herbivores, 10% carnivores
            double plantRatio = (double)plantCount / totalCount;
            double herbivoreRatio = (double)herbivoreCount / totalCount;

            double plantScore = MaxEcosystemHealth - Math.Abs(plantRatio - IdealPlantRatio);
            double herbivoreScore = MaxEcosystemHealth - Math.Abs(herbivoreRatio - IdealHerbivoreRatio);

            return (plantScore + herbivoreScore) / HealthAverageDivisor;
        }
    }
}
