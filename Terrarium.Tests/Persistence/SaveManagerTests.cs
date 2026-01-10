using Microsoft.VisualStudio.TestTools.UnitTesting;
using Terrarium.Logic.Entities;
using Terrarium.Logic.Persistence;
using Terrarium.Logic.Simulation;

namespace Terrarium.Tests.Persistence
{
    [TestClass]
    public class SaveManagerTests
    {
        [TestMethod]
        public void SaveManager_SaveLoad_RestoresVitalStatsAndCreatureState()
        {
            string filePath = Path.Combine(Path.GetTempPath(), $"terrarium_test_{Guid.NewGuid():N}.json");

            try
            {
                // Arrange
                var world = new World(width: 800, height: 200);

                var plant = new Plant(100, 100, initialSize: 10);
                plant.TakeDamage(25);
                plant.Update(deltaTime: 2.5);
                world.AddPlant(plant);

                var herbivore = new Herbivore(150, 100, type: "Sheep");
                herbivore.SetDirection(1, 0);
                herbivore.Update(deltaTime: 1.0);
                herbivore.Feed(10);
                herbivore.Update(deltaTime: 3.0);
                world.AddHerbivore(herbivore);

                var saveManager = new SaveManager();

                // Act
                saveManager.SaveWorld(world, fileName: filePath);
                var loaded = saveManager.LoadWorld(fileName: filePath);

                // Assert
                Assert.HasCount(1, loaded.Plants);
                Assert.HasCount(1, loaded.Herbivores);

                var loadedPlant = loaded.Plants[0];
                Assert.AreEqual(plant.Health, loadedPlant.Health, 0.0001, "Plant health should be restored");
                Assert.AreEqual(plant.Age, loadedPlant.Age, 0.0001, "Plant age should be restored");
                Assert.AreEqual(plant.IsAlive, loadedPlant.IsAlive, "Plant alive state should match restored health");

                var loadedHerbivore = loaded.Herbivores[0];
                Assert.AreEqual(herbivore.Health, loadedHerbivore.Health, 0.0001, "Herbivore health should be restored");
                Assert.AreEqual(herbivore.Age, loadedHerbivore.Age, 0.0001, "Herbivore age should be restored");
                Assert.AreEqual(herbivore.Hunger, loadedHerbivore.Hunger, 0.0001, "Herbivore hunger should be restored");
                Assert.AreEqual(herbivore.VelocityX, loadedHerbivore.VelocityX, 0.0001, "Herbivore velocityX should be restored");
                Assert.AreEqual(herbivore.VelocityY, loadedHerbivore.VelocityY, 0.0001, "Herbivore velocityY should be restored");
            }
            finally
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }

        [TestMethod]
        public void SaveManager_LoadWorld_InvalidJson_ThrowsInvalidDataException()
        {
            string filePath = Path.Combine(Path.GetTempPath(), $"terrarium_test_{Guid.NewGuid():N}.json");

            try
            {
                File.WriteAllText(filePath, "not valid json");
                var saveManager = new SaveManager();

                try
                {
                    saveManager.LoadWorld(filePath);
                    Assert.Fail("Expected InvalidDataException to be thrown for invalid JSON.");
                }
                catch (InvalidDataException)
                {
                    // expected
                }
            }
            finally
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }

        [TestMethod]
        public void SaveManager_LoadWorld_NullLists_DefaultToEmptyCollections()
        {
            string filePath = Path.Combine(Path.GetTempPath(), $"terrarium_test_{Guid.NewGuid():N}.json");

            try
            {
                string json = "{" +
                              "\"SchemaVersion\":1," +
                              "\"Width\":800," +
                              "\"Height\":200," +
                              "\"Plants\":null," +
                              "\"Herbivores\":null," +
                              "\"Carnivores\":null," +
                              "\"SaveDate\":\"2026-01-10T00:00:00Z\"" +
                              "}";

                File.WriteAllText(filePath, json);
                var saveManager = new SaveManager();

                var loaded = saveManager.LoadWorld(filePath);

                Assert.IsTrue(loaded.Plants.Count == 0);
                Assert.IsTrue(loaded.Herbivores.Count == 0);
                Assert.IsTrue(loaded.Carnivores.Count == 0);
            }
            finally
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }
    }
}
