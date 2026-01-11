using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Terrarium.Logic.Entities;

namespace Terrarium.Logic.Persistence
{
    /// <summary>
    /// Handles saving and loading simulation state.
    /// </summary>
    public class SaveManager
    {
        private const string DefaultSaveFileName = "terrarium_save.json";
        private const double DefaultPlantSizeFallback = 10.0;

        /// <summary>
        /// Saves the current world state to a JSON file.
        /// </summary>
        /// <param name="world">The world to save.</param>
        /// <param name="fileName">The file path to save to. Defaults to <see cref="DefaultSaveFileName"/>.</param>
        public void SaveWorld(Simulation.World world, string fileName = DefaultSaveFileName)
        {
            ArgumentNullException.ThrowIfNull(world);

            var saveData = new WorldSaveData
            {
                SchemaVersion = WorldSaveData.CurrentSchemaVersion,
                Width = world.Width,
                Height = world.Height,
                Plants = world.Plants.Select(EntityToData).ToList(),
                Herbivores = world.Herbivores.Select(EntityToData).ToList(),
                Carnivores = world.Carnivores.Select(EntityToData).ToList(),
                SaveDate = DateTime.Now
            };

            string json = JsonSerializer.Serialize(saveData, new JsonSerializerOptions { WriteIndented = true });

            string? directory = Path.GetDirectoryName(fileName);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            string tempFileName = fileName + ".tmp";
            File.WriteAllText(tempFileName, json);

            if (File.Exists(fileName))
                File.Copy(fileName, fileName + ".bak", overwrite: true);

            File.Move(tempFileName, fileName, overwrite: true);
        }

        /// <summary>
        /// Saves the current world state to a JSON file asynchronously.
        /// </summary>
        /// <param name="world">The world to save.</param>
        /// <param name="fileName">The file path to save to. Defaults to <see cref="DefaultSaveFileName"/>.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous save operation.</returns>
        public async Task SaveWorldAsync(Simulation.World world, string fileName = DefaultSaveFileName, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(world);

            var saveData = new WorldSaveData
            {
                SchemaVersion = WorldSaveData.CurrentSchemaVersion,
                Width = world.Width,
                Height = world.Height,
                Plants = world.Plants.Select(p => EntityToData(p)).ToList(),
                Herbivores = world.Herbivores.Select(h => EntityToData(h)).ToList(),
                Carnivores = world.Carnivores.Select(c => EntityToData(c)).ToList(),
                SaveDate = DateTime.Now
            };

            string json = JsonSerializer.Serialize(saveData, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            string? directory = Path.GetDirectoryName(fileName);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string tempFileName = fileName + ".tmp";
            await File.WriteAllTextAsync(tempFileName, json, cancellationToken).ConfigureAwait(false);

            if (File.Exists(fileName))
            {
                string backupFileName = fileName + ".bak";
                File.Copy(fileName, backupFileName, overwrite: true);
            }

            File.Move(tempFileName, fileName, overwrite: true);
        }

        /// <summary>
        /// Loads world state from a JSON file.
        /// </summary>
        /// <param name="fileName">The file path to load from. Defaults to <see cref="DefaultSaveFileName"/>.</param>
        /// <returns>The loaded world.</returns>
        /// <exception cref="FileNotFoundException">Thrown when the save file does not exist.</exception>
        /// <exception cref="InvalidDataException">Thrown when the save file is corrupted or invalid.</exception>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
        public Simulation.World LoadWorld(string fileName = DefaultSaveFileName)
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException($"Save file not found: {fileName}");

            string json = File.ReadAllText(fileName);
            WorldSaveData? saveData;
            try
            {
                saveData = JsonSerializer.Deserialize<WorldSaveData>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (JsonException ex)
            {
                throw new InvalidDataException("Save file is corrupted or not valid JSON.", ex);
            }

            if (saveData == null)
                throw new InvalidOperationException("Failed to deserialize save data");

            if (saveData.Width <= 0 || saveData.Height <= 0)
                throw new InvalidDataException("Save file contains invalid world dimensions.");

            var world = new Simulation.World(saveData.Width, saveData.Height);

            foreach (var plantData in saveData.Plants ?? Enumerable.Empty<EntitySaveData>())
            {
                var plant = new Plant(plantData.X, plantData.Y, plantData.Size ?? DefaultPlantSizeFallback);
                RestoreEntityData(plant, plantData);
                world.AddPlant(plant);
            }

            foreach (var herbData in saveData.Herbivores ?? Enumerable.Empty<EntitySaveData>())
            {
                var herbivore = new Herbivore(herbData.X, herbData.Y, herbData.Type ?? "Sheep");
                RestoreEntityData(herbivore, herbData);
                world.AddHerbivore(herbivore);
            }

            foreach (var carnData in saveData.Carnivores ?? Enumerable.Empty<EntitySaveData>())
            {
                var carnivore = new Carnivore(carnData.X, carnData.Y, carnData.Type ?? "Wolf");
                RestoreEntityData(carnivore, carnData);
                world.AddCarnivore(carnivore);
            }

            return world;
        }

        /// <summary>
        /// Loads world state from a JSON file asynchronously.
        /// </summary>
        /// <param name="fileName">The file path to load from. Defaults to <see cref="DefaultSaveFileName"/>.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous load operation, containing the loaded world.</returns>
        /// <exception cref="FileNotFoundException">Thrown when the save file does not exist.</exception>
        /// <exception cref="InvalidDataException">Thrown when the save file is corrupted or invalid.</exception>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
        public async Task<Simulation.World> LoadWorldAsync(string fileName = DefaultSaveFileName, CancellationToken cancellationToken = default)
        {
            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException($"Save file not found: {fileName}");
            }

            string json = await File.ReadAllTextAsync(fileName, cancellationToken).ConfigureAwait(false);
            WorldSaveData? saveData;
            try
            {
                saveData = JsonSerializer.Deserialize<WorldSaveData>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (JsonException ex)
            {
                throw new InvalidDataException("Save file is corrupted or not valid JSON.", ex);
            }

            if (saveData == null)
            {
                throw new InvalidOperationException("Failed to deserialize save data");
            }

            if (saveData.Width <= 0 || saveData.Height <= 0)
            {
                throw new InvalidDataException("Save file contains invalid world dimensions.");
            }

            var world = new Simulation.World(saveData.Width, saveData.Height);

            // Restore plants
            foreach (var plantData in saveData.Plants ?? new List<EntitySaveData>())
            {
                var plant = new Plant(plantData.X, plantData.Y, plantData.Size ?? DefaultPlantSizeFallback);
                RestoreEntityData(plant, plantData);
                world.AddPlant(plant);
            }

            // Restore herbivores
            foreach (var herbData in saveData.Herbivores ?? new List<EntitySaveData>())
            {
                var herbivore = new Herbivore(herbData.X, herbData.Y, herbData.Type ?? "Sheep");
                RestoreEntityData(herbivore, herbData);
                world.AddHerbivore(herbivore);
            }

            // Restore carnivores
            foreach (var carnData in saveData.Carnivores ?? new List<EntitySaveData>())
            {
                var carnivore = new Carnivore(carnData.X, carnData.Y, carnData.Type ?? "Wolf");
                RestoreEntityData(carnivore, carnData);
                world.AddCarnivore(carnivore);
            }

            return world;
        }

        /// <summary>
        /// Attempts to load world state from a JSON file without throwing.
        /// The thrown exception details (including stack trace) are returned in <paramref name="errorDetails"/>.
        /// </summary>
        /// <param name="fileName">The file path to load from. Defaults to <see cref="DefaultSaveFileName"/>.</param>
        /// <param name="world">The loaded world, or null if loading failed.</param>
        /// <param name="errorDetails">Error details if loading failed, or null if successful.</param>
        /// <returns>True if loading succeeded, false otherwise.</returns>
        public bool TryLoadWorld(string fileName, out Simulation.World? world, out string? errorDetails)
        {
            try
            {
                world = LoadWorld(fileName);
                errorDetails = null;
                return true;
            }
            catch (Exception ex)
            {
                world = null;
                errorDetails = ex.ToString();
                return false;
            }
        }

        /// <summary>
        /// Converts an entity to save data.
        /// </summary>
        private EntitySaveData EntityToData(LivingEntity entity)
        {
            var data = new EntitySaveData
            {
                Id = entity.Id,
                X = entity.X,
                Y = entity.Y,
                Health = entity.Health,
                Age = entity.Age
            };

            if (entity is Plant plant)
            {
                data.Size = plant.Size;
                data.WaterLevel = plant.WaterLevel;
            }
            else if (entity is Creature creature)
            {
                data.Hunger = creature.Hunger;
                data.VelocityX = creature.VelocityX;
                data.VelocityY = creature.VelocityY;

                if (creature is Herbivore herbivore)
                {
                    data.Type = herbivore.Type;
                }
                else if (creature is Carnivore carnivore)
                {
                    data.Type = carnivore.Type;
                }
            }

            return data;
        }

        /// <summary>
        /// Restores entity data from save data.
        /// </summary>
        private void RestoreEntityData(LivingEntity entity, EntitySaveData data)
        {
            entity.X = data.X;
            entity.Y = data.Y;
            entity.RestoreVitalStats(data.Health, data.Age);

            if (entity is Plant plant && data.WaterLevel.HasValue)
            {
                plant.Water(data.WaterLevel.Value - plant.WaterLevel);
            }
            else if (entity is Creature creature)
            {
                creature.RestoreCreatureState(
                    hunger: data.Hunger ?? creature.Hunger,
                    velocityX: data.VelocityX,
                    velocityY: data.VelocityY);
            }
        }

        /// <summary>
        /// Attempts to load world state from a JSON file asynchronously without throwing.
        /// </summary>
        /// <param name="fileName">The file path to load from. Defaults to <see cref="DefaultSaveFileName"/>.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>A tuple containing success status, the loaded world (or null), and error details (or null).</returns>
        public async Task<(bool Success, Simulation.World? World, string? ErrorDetails)> TryLoadWorldAsync(string fileName = DefaultSaveFileName, CancellationToken cancellationToken = default)
        {
            try
            {
                var world = await LoadWorldAsync(fileName, cancellationToken).ConfigureAwait(false);
                return (true, world, null);
            }
            catch (Exception ex)
            {
                return (false, null, ex.ToString());
            }
        }

        /// <summary>
        /// Checks if a save file exists.
        /// </summary>
        /// <param name="fileName">The file path to check. Defaults to <see cref="DefaultSaveFileName"/>.</param>
        /// <returns>True if the file exists, false otherwise.</returns>
        public bool SaveFileExists(string fileName = DefaultSaveFileName)
        {
            return File.Exists(fileName);
        }
    }

    /// <summary>
    /// Data structure for saving world state.
    /// </summary>
    public class WorldSaveData
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion { get; set; } = CurrentSchemaVersion;
        public double Width { get; set; }
        public double Height { get; set; }
        public List<EntitySaveData> Plants { get; set; } = new();
        public List<EntitySaveData> Herbivores { get; set; } = new();
        public List<EntitySaveData> Carnivores { get; set; } = new();
        public DateTime SaveDate { get; set; }
    }

    /// <summary>
    /// Data structure for saving entity state.
    /// </summary>
    public class EntitySaveData
    {
        public int? Id { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Health { get; set; }
        public double Age { get; set; }
        public double? Hunger { get; set; }
        public double VelocityX { get; set; }
        public double VelocityY { get; set; }
        public double? Size { get; set; }
        public double? WaterLevel { get; set; }
        public string? Type { get; set; }
    }
}
