using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
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

                Assert.IsEmpty(loaded.Plants);
                Assert.IsEmpty(loaded.Herbivores);
                Assert.IsEmpty(loaded.Carnivores);
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
        public void SaveManager_FullEcosystemRoundTrip_AllEntityTypesPreserved()
        {
            // Integration test: comprehensive ecosystem save/load round-trip
            string filePath = Path.Combine(Path.GetTempPath(), $"terrarium_integration_{Guid.NewGuid():N}.json");

            try
            {
                // Arrange - Create a diverse ecosystem
                var world = new World(width: 1920, height: 300);

                // Add multiple plants with various states
                var plant1 = new Plant(100, 50, initialSize: 5);
                plant1.Grow(10.0); // Grow for 10 seconds
                world.AddPlant(plant1);

                var plant2 = new Plant(200, 50, initialSize: 15);
                plant2.TakeDamage(30); // Damaged plant
                world.AddPlant(plant2);

                var plant3 = new Plant(300, 50, initialSize: 25);
                plant3.Water(50); // Well-watered plant
                world.AddPlant(plant3);

                // Add herbivores with different states
                var herbivore1 = new Herbivore(150, 100, type: "Sheep");
                herbivore1.SetDirection(1, 0);
                herbivore1.Update(5.0); // Age it
                herbivore1.Feed(20); // Feed it
                world.AddHerbivore(herbivore1);

                var herbivore2 = new Herbivore(250, 100, type: "Rabbit");
                herbivore2.SetDirection(-1, 0.5);
                herbivore2.Update(10.0); // Older, hungrier
                world.AddHerbivore(herbivore2);

                // Add carnivores with different states
                var carnivore1 = new Carnivore(400, 100, type: "Wolf");
                carnivore1.SetDirection(0.5, -0.5);
                carnivore1.Update(3.0);
                carnivore1.Feed(30);
                world.AddCarnivore(carnivore1);

                var carnivore2 = new Carnivore(500, 100, type: "Fox");
                carnivore2.SetDirection(-0.5, 0);
                carnivore2.Update(8.0);
                world.AddCarnivore(carnivore2);

                var saveManager = new SaveManager();

                // Act - Save and reload
                saveManager.SaveWorld(world, fileName: filePath);
                var loaded = saveManager.LoadWorld(fileName: filePath);

                // Assert - Verify counts
                Assert.HasCount(3, loaded.Plants, "Should have 3 plants");
                Assert.HasCount(2, loaded.Herbivores, "Should have 2 herbivores");
                Assert.HasCount(2, loaded.Carnivores, "Should have 2 carnivores");

                // Verify world dimensions
                Assert.AreEqual(1920, loaded.Width, "World width should be preserved");
                Assert.AreEqual(300, loaded.Height, "World height should be preserved");

                // Verify plant states are restored
                var loadedPlant1 = loaded.Plants.FirstOrDefault(p => Math.Abs(p.X - 100) < 1);
                Assert.IsNotNull(loadedPlant1, "Plant 1 should be found at X=100");
                Assert.IsGreaterThan(5.0, loadedPlant1.Size, "Plant 1 should have grown");

                var loadedPlant2 = loaded.Plants.FirstOrDefault(p => Math.Abs(p.X - 200) < 1);
                Assert.IsNotNull(loadedPlant2, "Plant 2 should be found at X=200");
                Assert.IsLessThan(100.0, loadedPlant2.Health, "Plant 2 should be damaged");

                // Verify herbivore states
                var loadedHerb1 = loaded.Herbivores.FirstOrDefault(h => h.Type == "Sheep");
                Assert.IsNotNull(loadedHerb1, "Sheep herbivore should exist");
                Assert.IsGreaterThanOrEqualTo(5.0, loadedHerb1.Age, "Sheep age should be preserved");

                var loadedHerb2 = loaded.Herbivores.FirstOrDefault(h => h.Type == "Rabbit");
                Assert.IsNotNull(loadedHerb2, "Rabbit herbivore should exist");
                Assert.IsGreaterThan(loadedHerb1.Hunger, loadedHerb2.Hunger, "Older rabbit should be hungrier");

                // Verify carnivore states
                var loadedCarn1 = loaded.Carnivores.FirstOrDefault(c => c.Type == "Wolf");
                Assert.IsNotNull(loadedCarn1, "Wolf carnivore should exist");

                var loadedCarn2 = loaded.Carnivores.FirstOrDefault(c => c.Type == "Fox");
                Assert.IsNotNull(loadedCarn2, "Fox carnivore should exist");
                Assert.IsGreaterThan(loadedCarn1.Age, loadedCarn2.Age, "Fox should be older than wolf");
            }
            finally
            {
                // Cleanup
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                string backupPath = filePath + ".bak";
                if (File.Exists(backupPath))
                {
                    File.Delete(backupPath);
                }
            }
        }

        [TestMethod]
        public void SaveManager_TryLoadWorld_ReturnsErrorDetails_OnFailure()
        {
            string filePath = Path.Combine(Path.GetTempPath(), $"terrarium_error_{Guid.NewGuid():N}.json");

            try
            {
                // Write invalid JSON
                File.WriteAllText(filePath, "{ invalid json here }");
                var saveManager = new SaveManager();

                // Act
                bool success = saveManager.TryLoadWorld(filePath, out var world, out var errorDetails);

                // Assert
                Assert.IsFalse(success, "TryLoadWorld should return false for invalid JSON");
                Assert.IsNull(world, "World should be null on failure");
                Assert.IsNotNull(errorDetails, "Error details should be provided");
                StringAssert.Contains(errorDetails, "Exception", "Error should contain exception info");
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
        public void SaveManager_TryLoadWorld_ReturnsTrue_OnSuccess()
        {
            string filePath = Path.Combine(Path.GetTempPath(), $"terrarium_success_{Guid.NewGuid():N}.json");

            try
            {
                // Create and save a valid world
                var world = new World(800, 200);
                world.AddPlant(new Plant(100, 50));
                var saveManager = new SaveManager();
                saveManager.SaveWorld(world, fileName: filePath);

                // Act
                bool success = saveManager.TryLoadWorld(filePath, out var loaded, out var errorDetails);

                // Assert
                Assert.IsTrue(success, "TryLoadWorld should return true for valid save");
                Assert.IsNotNull(loaded, "World should be loaded");
                Assert.IsNull(errorDetails, "No error details on success");
                Assert.HasCount(1, loaded.Plants, "Plant should be restored");
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
        public void SaveManager_LoadWorld_FileNotFound_ThrowsFileNotFoundException()
        {
            var saveManager = new SaveManager();
            string nonExistentFile = Path.Combine(Path.GetTempPath(), $"nonexistent_{Guid.NewGuid():N}.json");

            try
            {
                saveManager.LoadWorld(nonExistentFile);
                Assert.Fail("Expected FileNotFoundException");
            }
            catch (FileNotFoundException)
            {
                // Expected
            }
        }

        [TestMethod]
        public void SaveManager_SaveWorld_CreatesBackupFile()
        {
            string filePath = Path.Combine(Path.GetTempPath(), $"terrarium_backup_{Guid.NewGuid():N}.json");
            string backupPath = filePath + ".bak";

            try
            {
                var world = new World(800, 200);
                world.AddPlant(new Plant(100, 50, initialSize: 10));
                var saveManager = new SaveManager();

                // First save
                saveManager.SaveWorld(world, fileName: filePath);
                Assert.IsTrue(File.Exists(filePath), "Save file should exist");
                Assert.IsFalse(File.Exists(backupPath), "No backup on first save");

                // Modify world and save again
                world.AddPlant(new Plant(200, 50, initialSize: 20));
                saveManager.SaveWorld(world, fileName: filePath);

                // Verify backup was created
                Assert.IsTrue(File.Exists(backupPath), "Backup should be created on subsequent saves");
            }
            finally
            {
                if (File.Exists(filePath)) File.Delete(filePath);
                if (File.Exists(backupPath)) File.Delete(backupPath);
            }
        }

        [TestMethod]
        public void SaveManager_SaveWorld_InvalidWorldDimensions_LoadThrowsInvalidDataException()
        {
            string filePath = Path.Combine(Path.GetTempPath(), $"terrarium_invalid_dims_{Guid.NewGuid():N}.json");

            try
            {
                // Manually write a save with invalid dimensions
                string invalidJson = "{" +
                    "\"SchemaVersion\":1," +
                    "\"Width\":0," +  // Invalid: zero width
                    "\"Height\":200," +
                    "\"Plants\":[]," +
                    "\"Herbivores\":[]," +
                    "\"Carnivores\":[]," +
                    "\"SaveDate\":\"2026-01-11T00:00:00Z\"}";

                File.WriteAllText(filePath, invalidJson);
                var saveManager = new SaveManager();

                try
                {
                    saveManager.LoadWorld(filePath);
                    Assert.Fail("Expected InvalidDataException for zero width");
                }
                catch (InvalidDataException ex)
                {
                    StringAssert.Contains(ex.Message, "invalid", "Should mention invalid dimensions");
                }
            }
            finally
            {
                if (File.Exists(filePath)) File.Delete(filePath);
            }
        }
    }
}
