using Terrarium.Logic.Entities;

namespace Terrarium.Logic.Simulation
{
    /// <summary>
    /// Represents the simulation world containing all entities.
    /// </summary>
    public class World
    {
        private readonly List<Plant> _plants;
        private readonly List<Herbivore> _herbivores;
        private readonly List<Carnivore> _carnivores;
        private readonly Random _random;

        // World boundary constants
        public const double MinX = 0;
        public const double MaxX = 1920; // Default screen width
        public const double MinY = 0;
        public const double MaxY = 200; // Bottom strip of screen

        /// <summary>
        /// All plants in the world.
        /// </summary>
        public IReadOnlyList<Plant> Plants => _plants.AsReadOnly();

        /// <summary>
        /// All herbivores in the world.
        /// </summary>
        public IReadOnlyList<Herbivore> Herbivores => _herbivores.AsReadOnly();

        /// <summary>
        /// All carnivores in the world.
        /// </summary>
        public IReadOnlyList<Carnivore> Carnivores => _carnivores.AsReadOnly();

        /// <summary>
        /// Width of the world.
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// Height of the world.
        /// </summary>
        public double Height { get; set; }

        public World(double width = MaxX, double height = MaxY)
            : this(width, height, random: null)
        {
        }

        public World(double width, double height, Random? random)
        {
            Width = width;
            Height = height;
            _plants = new List<Plant>();
            _herbivores = new List<Herbivore>();
            _carnivores = new List<Carnivore>();
            _random = random ?? new Random();
        }

        public void AddPlant(Plant plant) => _plants.Add(plant);

        public void AddHerbivore(Herbivore herbivore) => _herbivores.Add(herbivore);

        public void AddCarnivore(Carnivore carnivore) => _carnivores.Add(carnivore);

        public void RemoveDeadEntities()
        {
            _plants.RemoveAll(p => !p.IsAlive);
            _herbivores.RemoveAll(h => !h.IsAlive);
            _carnivores.RemoveAll(c => !c.IsAlive);
        }

        /// <summary>
        /// Spawns a random plant in the world.
        /// </summary>
        public Plant SpawnRandomPlant()
        {
            double x = _random.NextDouble() * Width;
            double y = _random.NextDouble() * Height;
            var plant = new Plant(x, y);
            AddPlant(plant);
            return plant;
        }

        /// <summary>
        /// Spawns a random herbivore in the world.
        /// </summary>
        public Herbivore SpawnRandomHerbivore(string type = "Sheep")
        {
            double x = _random.NextDouble() * Width;
            double y = _random.NextDouble() * Height;
            var herbivore = new Herbivore(x, y, type);
            AddHerbivore(herbivore);
            return herbivore;
        }

        /// <summary>
        /// Spawns a random carnivore in the world.
        /// </summary>
        public Carnivore SpawnRandomCarnivore(string type = "Wolf")
        {
            double x = _random.NextDouble() * Width;
            double y = _random.NextDouble() * Height;
            var carnivore = new Carnivore(x, y, type);
            AddCarnivore(carnivore);
            return carnivore;
        }

        public IEnumerable<LivingEntity> GetAllEntities()
        {
            foreach (var plant in _plants) yield return plant;
            foreach (var herbivore in _herbivores) yield return herbivore;
            foreach (var carnivore in _carnivores) yield return carnivore;
        }
    }
}
