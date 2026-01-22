namespace Terrarium.Tests.Simulation
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Terrarium.Logic.Simulation;

    [TestClass]
    /// <summary>
    /// Comprehensive tests for TerrainProperties to achieve 100% coverage.
    /// </summary>
    public class TerrainPropertiesTests
    {
        [TestMethod]

        public void GetProperties_ReturnsCorrectData(TerrainType type, string expectedName, string expectedColor, bool expectedWalkable, int expectedFertility)
        {
            // Act
            var properties = TerrainProperties.GetProperties(type);

            // Assert
            Assert.AreEqual(expectedName, properties.Name);
            Assert.AreEqual(expectedColor, properties.Color);
            Assert.AreEqual(expectedWalkable, properties.Walkable);
            Assert.AreEqual(expectedFertility, properties.Fertility);
            Assert.IsNotNull(properties.Description);
        }

        [TestMethod]

        public void GetControllingFaction_FactionTerrain_ReturnsFaction(TerrainType terrain, FactionType expectedFaction)
        {
            // Act
            var faction = TerrainProperties.GetControllingFaction(terrain);

            // Assert
            Assert.IsNotNull(faction);
            Assert.AreEqual(expectedFaction, faction.Value);
        }

        [TestMethod]

        public void GetControllingFaction_NeutralTerrain_ReturnsNull(TerrainType terrain)
        {
            // Act
            var faction = TerrainProperties.GetControllingFaction(terrain);

            // Assert
            Assert.IsNull(faction);
        }

        [TestMethod]

        public void GetFactionTerrain_ReturnsCorrectTerrain(FactionType faction, TerrainType expectedTerrain)
        {
            // Act
            var terrain = TerrainProperties.GetFactionTerrain(faction);

            // Assert
            Assert.AreEqual(expectedTerrain, terrain);
        }

        [TestMethod]
        public void CanConvert_SoilToVerdantGrowth_WithCorrectFaction_ReturnsTrue()
        {
            // Act
            bool canConvert = TerrainProperties.CanConvert(
                TerrainType.Soil,
                TerrainType.VerdantGrowth,
                FactionType.VerdantCollective);

            // Assert
            Assert.IsTrue(canConvert);
        }

        [TestMethod]
        public void CanConvert_SoilToAshenWasteland_WithCorrectFaction_ReturnsTrue()
        {
            // Act
            bool canConvert = TerrainProperties.CanConvert(
                TerrainType.Soil,
                TerrainType.AshenWasteland,
                FactionType.AshenLegion);

            // Assert
            Assert.IsTrue(canConvert);
        }

        [TestMethod]
        public void CanConvert_WaterToAquaticDomain_WithCorrectFaction_ReturnsTrue()
        {
            // Act
            bool canConvert = TerrainProperties.CanConvert(
                TerrainType.Water,
                TerrainType.AquaticDomain,
                FactionType.TideWalkers);

            // Assert
            Assert.IsTrue(canConvert);
        }

        [TestMethod]
        public void CanConvert_ToEnemyFactionTerrain_ReturnsFalse()
        {
            // Act
            bool canConvert = TerrainProperties.CanConvert(
                TerrainType.Soil,
                TerrainType.VerdantGrowth,
                FactionType.AshenLegion); // Wrong faction

            // Assert
            Assert.IsFalse(canConvert);
        }

        [TestMethod]
        public void CanConvert_InvalidConversion_ReturnsFalse()
        {
            // Act
            bool canConvert = TerrainProperties.CanConvert(
                TerrainType.Stone,
                TerrainType.VerdantGrowth,
                FactionType.VerdantCollective);

            // Assert
            Assert.IsFalse(canConvert);
        }

        [TestMethod]
        public void TerrainData_AllPropertiesCanBeSet()
        {
            // Arrange
            var terrainData = new TerrainData
            {
                Name = "Test Terrain",
                Color = "#FF0000",
                Walkable = true,
                Fertility = 75,
                Description = "Test description"
            };

            // Assert
            Assert.AreEqual("Test Terrain", terrainData.Name);
            Assert.AreEqual("#FF0000", terrainData.Color);
            Assert.IsTrue(terrainData.Walkable);
            Assert.AreEqual(75, terrainData.Fertility);
            Assert.AreEqual("Test description", terrainData.Description);
        }
    }
}
