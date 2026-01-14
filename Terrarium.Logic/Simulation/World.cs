using Terrarium.Logic.Entities;

namespace Terrarium.Logic.Simulation
{
    /// <summary>
    /// Represents the simulation world containing all entities and terrain.
    /// Implements cellular automata for terrain conquest and faction warfare.
    /// </summary>
    public class World
    {
        private readonly List<Plant> _plants;
        private readonly List<Herbivore> _herbivores;
        private readonly List<Carnivore> _carnivores;
        private readonly Random _random;

        // Terrain grid system
        private TerrainType[,] _terrainGrid;
        private readonly int _gridWidth;
        private readonly int _gridHeight;
        private readonly int _cellSize = 20; // Size of each terrain cell in pixels

        // World boundary constants
        public const double MinX = 0;
        public const double MaxX = 1920; // Default screen width
        public const double MinY = 0;
        public const double MaxY = 200; // Bottom strip of screen

        /// <summary>
        /// The type of terrarium.
        /// </summary>
        public TerrariumType TerrariumType { get; }

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
            : this(width, height, TerrariumType.Forest, random: null)
        {
        }

        public World(double width, double height, TerrariumType terrariumType, Random? random = null)
        {
            Width = width;
            Height = height;
            TerrariumType = terrariumType;
            _plants = new List<Plant>();
            _herbivores = new List<Herbivore>();
            _carnivores = new List<Carnivore>();
            _random = random ?? new Random();

            // Initialize terrain grid
            _gridWidth = (int)Math.Ceiling(width / _cellSize);
            _gridHeight = (int)Math.Ceiling(height / _cellSize);
            _terrainGrid = new TerrainType[_gridWidth, _gridHeight];

            // Initialize terrain based on terrarium type
            InitializeTerrain();
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
            string type = TerrariumType switch
            {
                TerrariumType.Forest => "Tree",
                TerrariumType.Desert => "Cactus",
                TerrariumType.Aquatic => "Algae",
                TerrariumType.GodSimulator => "Crystal",
                _ => "Plant"
            };
            var plant = new Plant(x, y, type);
            AddPlant(plant);
            return plant;
        }

        /// <summary>
        /// Spawns a random herbivore in the world.
        /// </summary>
        public Herbivore SpawnRandomHerbivore(string? type = null, FactionType? factionOverride = null)
        {
            double x = _random.NextDouble() * Width;
            double y = _random.NextDouble() * Height;
            string t = type ?? TerrariumType switch
            {
                TerrariumType.Forest => "Deer",
                TerrariumType.Desert => "Lizard",
                TerrariumType.Aquatic => "Fish",
                TerrariumType.GodSimulator => "Phoenix",
                _ => "Sheep"
            };

            // Assign faction based on terrarium type or override
            FactionType faction = factionOverride ?? TerrariumType switch
            {
                TerrariumType.GodSimulator => (FactionType)_random.Next(Enum.GetValues(typeof(FactionType)).Length),
                _ => FactionType.VerdantCollective // Default faction for other modes
            };

            var herbivore = new Herbivore(x, y, t, faction: faction);
            AddHerbivore(herbivore);
            return herbivore;
        }

        /// <summary>
        /// Spawns a random carnivore in the world.
        /// </summary>
        public Carnivore SpawnRandomCarnivore(string? type = null, FactionType? factionOverride = null)
        {
            double x = _random.NextDouble() * Width;
            double y = _random.NextDouble() * Height;
            string t = type ?? TerrariumType switch
            {
                TerrariumType.Forest => "Wolf",
                TerrariumType.Desert => "Snake",
                TerrariumType.Aquatic => "Shark",
                TerrariumType.GodSimulator => "Dragon",
                _ => "Wolf"
            };

            // Assign faction based on terrarium type or override
            FactionType faction = factionOverride ?? TerrariumType switch
            {
                TerrariumType.GodSimulator => (FactionType)_random.Next(Enum.GetValues(typeof(FactionType)).Length),
                _ => FactionType.AshenLegion // Default faction for other modes
            };

            var carnivore = new Carnivore(x, y, t, faction: faction);
            AddCarnivore(carnivore);
            return carnivore;
        }

        public IEnumerable<LivingEntity> GetAllEntities()
        {
            foreach (var plant in _plants) yield return plant;
            foreach (var herbivore in _herbivores) yield return herbivore;
            foreach (var carnivore in _carnivores) yield return carnivore;
        }

        /// <summary>
        /// Initializes the terrain grid based on the terrarium type.
        /// </summary>
        private void InitializeTerrain()
        {
            for (int x = 0; x < _gridWidth; x++)
            {
                for (int y = 0; y < _gridHeight; y++)
                {
                    _terrainGrid[x, y] = TerrariumType switch
                    {
                        TerrariumType.GodSimulator => GenerateProceduralTerrain(x, y),
                        TerrariumType.Forest => TerrainType.VerdantGrowth,
                        TerrariumType.Desert => TerrainType.AshenWasteland,
                        TerrariumType.Aquatic => TerrainType.AquaticDomain,
                        _ => TerrainType.Soil
                    };
                }
            }
        }

        /// <summary>
        /// Generates procedural terrain for God Simulator mode.
        /// Creates varied terrain with faction starting areas.
        /// </summary>
        private TerrainType GenerateProceduralTerrain(int x, int y)
        {
            // Create faction starting areas
            var factionTerrains = new Dictionary<FactionType, TerrainType>
            {
                { FactionType.VerdantCollective, TerrainType.VerdantGrowth },
                { FactionType.AshenLegion, TerrainType.AshenWasteland },
                { FactionType.TideWalkers, TerrainType.AquaticDomain },
                { FactionType.CrystalChoir, TerrainType.StoneWardens },
                { FactionType.NomadicCovenant, TerrainType.CelestialOrder },
                { FactionType.ScrapbornSwarm, TerrainType.NetherCult }
            };

            // Divide world into faction territories
            int factionIndex = (x / (_gridWidth / 6)) % 6;
            var faction = (FactionType)factionIndex;

            // Add some natural variation and neutral areas
            double noise = _random.NextDouble();
            if (noise < 0.1) return TerrainType.Void; // 10% void areas
            if (noise < 0.3) return TerrainType.Soil; // 20% neutral soil
            if (noise < 0.5) return TerrainType.Stone; // 20% stone formations
            if (noise < 0.6) return TerrainType.Water; // 10% water features

            // Return faction-specific terrain
            return factionTerrains[faction];
        }

        /// <summary>
        /// Gets the terrain type at the specified world coordinates.
        /// </summary>
        public TerrainType GetTerrainAt(double worldX, double worldY)
        {
            int gridX = (int)(worldX / _cellSize);
            int gridY = (int)(worldY / _cellSize);

            if (gridX < 0 || gridX >= _gridWidth || gridY < 0 || gridY >= _gridHeight)
                return TerrainType.Void;

            return _terrainGrid[gridX, gridY];
        }

        /// <summary>
        /// Sets the terrain type at the specified world coordinates.
        /// Used by god powers for direct terrain manipulation.
        /// </summary>
        public void SetTerrainAt(double worldX, double worldY, TerrainType terrainType)
        {
            int gridX = (int)(worldX / _cellSize);
            int gridY = (int)(worldY / _cellSize);

            if (gridX >= 0 && gridX < _gridWidth && gridY >= 0 && gridY < _gridHeight)
            {
                _terrainGrid[gridX, gridY] = terrainType;
            }
        }

        /// <summary>
        /// Attempts to convert terrain at the specified location based on faction conquest rules.
        /// Returns true if conversion was successful.
        /// </summary>
        public bool AttemptTerrainConversion(double worldX, double worldY, FactionType conqueringFaction)
        {
            TerrainType currentTerrain = GetTerrainAt(worldX, worldY);
            TerrainType targetTerrain = TerrainProperties.GetFactionTerrain(conqueringFaction);

            // Can't convert void or already owned terrain
            if (currentTerrain == TerrainType.Void || currentTerrain == targetTerrain)
                return false;

            // Apply cellular automata conversion rules
            if (CanConvertTerrain(currentTerrain, targetTerrain))
            {
                SetTerrainAt(worldX, worldY, targetTerrain);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if one terrain type can convert another based on elemental counters.
        /// </summary>
        private bool CanConvertTerrain(TerrainType from, TerrainType to)
        {
            // Elemental counter system:
            // Fire (Ashen) burns Wood (Verdant)
            // Water (Aquatic) floods Earth (Soil/Stone)
            // Stone crushes Nether corruption
            // Celestial purifies Nether
            // Nether corrupts all natural terrain

            return (from, to) switch
            {
                (TerrainType.VerdantGrowth, TerrainType.AshenWasteland) => true, // Fire burns wood
                (TerrainType.Soil, TerrainType.AquaticDomain) => true, // Water floods earth
                (TerrainType.Stone, TerrainType.AquaticDomain) => true, // Water erodes stone
                (TerrainType.Water, TerrainType.StoneWardens) => true, // Stone dams water
                (TerrainType.AshenWasteland, TerrainType.StoneWardens) => true, // Stone contains fire
                (TerrainType.VerdantGrowth, TerrainType.StoneWardens) => true, // Stone crushes plants
                (TerrainType.Soil, TerrainType.StoneWardens) => true, // Stone forms from earth
                (TerrainType.NetherCult, TerrainType.StoneWardens) => true, // Stone seals corruption
                (TerrainType.NetherCult, TerrainType.CelestialOrder) => true, // Celestial purifies corruption
                (TerrainType.AshenWasteland, TerrainType.NetherCult) => true, // Corruption spreads to ash
                (TerrainType.VerdantGrowth, TerrainType.NetherCult) => true, // Corruption spreads to plants
                (TerrainType.Soil, TerrainType.NetherCult) => true, // Corruption spreads to soil
                (TerrainType.Stone, TerrainType.NetherCult) => true, // Corruption spreads to stone
                (TerrainType.Water, TerrainType.NetherCult) => true, // Corruption spreads to water
                (TerrainType.AquaticDomain, TerrainType.NetherCult) => true, // Corruption spreads to aquatic
                _ => false
            };
        }

        /// <summary>
        /// Processes cellular automata terrain conquest for one simulation step.
        /// Entities attempt to convert adjacent terrain based on faction rules.
        /// </summary>
        public void ProcessTerrainConquest()
        {
            // Create a copy of current terrain for this step
            var newTerrain = (TerrainType[,])_terrainGrid.Clone();

            // Process conquest from each entity
            foreach (var entity in GetAllEntities())
            {
                if (!entity.IsAlive || entity is not Creature creature) continue;

                // Get entity's faction terrain
                TerrainType factionTerrain = TerrainProperties.GetFactionTerrain(creature.Faction);

                // Check adjacent cells for conversion opportunities
                int centerX = (int)(entity.X / _cellSize);
                int centerY = (int)(entity.Y / _cellSize);

                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        int gridX = centerX + dx;
                        int gridY = centerY + dy;

                        if (gridX >= 0 && gridX < _gridWidth && gridY >= 0 && gridY < _gridHeight)
                        {
                            TerrainType current = _terrainGrid[gridX, gridY];
                            if (CanConvertTerrain(current, factionTerrain))
                            {
                                newTerrain[gridX, gridY] = factionTerrain;
                            }
                        }
                    }
                }
            }

            _terrainGrid = newTerrain;
        }

        /// <summary>
        /// Gets all terrain cells controlled by a specific faction.
        /// </summary>
        public IEnumerable<(int x, int y, TerrainType terrain)> GetFactionTerritory(FactionType faction)
        {
            TerrainType factionTerrain = TerrainProperties.GetFactionTerrain(faction);

            for (int x = 0; x < _gridWidth; x++)
            {
                for (int y = 0; y < _gridHeight; y++)
                {
                    if (_terrainGrid[x, y] == factionTerrain)
                    {
                        yield return (x, y, _terrainGrid[x, y]);
                    }
                }
            }
        }

        /// <summary>
        /// Calculates territory control percentage for each faction.
        /// </summary>
        public Dictionary<FactionType, double> GetTerritoryControl()
        {
            var control = new Dictionary<FactionType, double>();
            int totalCells = _gridWidth * _gridHeight;

            foreach (FactionType faction in Enum.GetValues(typeof(FactionType)))
            {
                int factionCells = GetFactionTerritory(faction).Count();
                control[faction] = (double)factionCells / totalCells * 100.0;
            }

            return control;
        }
    }
}
