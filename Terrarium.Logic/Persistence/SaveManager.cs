using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Terrarium.Logic.Entities;
using Terrarium.Logic.Simulation;

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
        public void SaveWorld(World world, string fileName = DefaultSaveFileName)
        {
            var saveData = new WorldSaveData
            {
                Width = world.Width,
                Height = world.Height,
                Plants = world.Plants.Select(EntityToData).ToList(),
                Herbivores = world.Herbivores.Select(EntityToData).ToList(),
                Carnivores = world.Carnivores.Select(EntityToData).ToList(),
                SaveDate = DateTime.Now
            };

            string json = JsonSerializer.Serialize(saveData, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(fileName, json);
        }

        /// <summary>
        /// Loads world state from a JSON file.
        /// </summary>
        public World LoadWorld(string fileName = DefaultSaveFileName)
        {
            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException($"Save file not found: {fileName}");
            }

            string json = File.ReadAllText(fileName);
            var saveData = JsonSerializer.Deserialize<WorldSaveData>(json);

            if (saveData == null)
            {
                throw new InvalidOperationException("Failed to deserialize save data");
            }

            var world = new World(saveData.Width, saveData.Height);

            foreach (var plantData in saveData.Plants)
            {
                var plant = new Plant(
                    plantData.X,
                    plantData.Y,
                    initialSize: plantData.Size ?? DefaultPlantSizeFallback,
                    id: plantData.Id);

                RestoreEntityData(plant, plantData);
                world.AddPlant(plant);
            }

            foreach (var herbData in saveData.Herbivores)
            {
                var herbivore = new Herbivore(
                    herbData.X,
                    herbData.Y,
                    type: herbData.Type ?? "Sheep",
                    id: herbData.Id);

                RestoreEntityData(herbivore, herbData);
                world.AddHerbivore(herbivore);
            }

            foreach (var carnData in saveData.Carnivores)
            {
                var carnivore = new Carnivore(
                    carnData.X,
                    carnData.Y,
                    type: carnData.Type ?? "Wolf",
                    id: carnData.Id);

                RestoreEntityData(carnivore, carnData);
                world.AddCarnivore(carnivore);
            }

            return world;
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
        /// Checks if a save file exists.
        /// </summary>
        public bool SaveFileExists(string fileName = DefaultSaveFileName) => File.Exists(fileName);
    }

    /// <summary>
    /// Data structure for saving world state.
    /// </summary>
    public class WorldSaveData
    {
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
