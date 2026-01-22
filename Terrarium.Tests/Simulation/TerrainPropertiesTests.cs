using Xunit;
using Terrarium.Logic.Simulation;
using Assert = Xunit.Assert;

namespace Terrarium.Tests.Simulation
{
    /// <summary>
    /// Comprehensive tests for TerrainProperties to achieve 100% coverage.
    /// </summary>
    public class TerrainPropertiesTests
    {
        [DataTestMethod]
        [InlineData(TerrainType.Void, "Void", "#000000", false, 0)]
        [InlineData(TerrainType.Soil, "Soil", "#8B4513", true, 100)]
        [InlineData(TerrainType.Stone, "Stone", "#696969", true, 10)]
        [InlineData(TerrainType.Water, "Water", "#4169E1", false, 30)]
        [InlineData(TerrainType.VerdantGrowth, "Verdant Growth", "#228B22", true, 150)]
        [InlineData(TerrainType.AshenWasteland, "Ashen Wasteland", "#2F2F2F", true, 5)]
        public void GetProperties_ReturnsCorrectData(TerrainType type, string expectedName, string expectedColor, bool expectedWalkable, int expectedFertility)
        {
            // Act
            var properties = TerrainProperties.GetProperties(type);

            // Assert
            Assert.Equal(expectedName, properties.Name);
            Assert.Equal(expectedColor, properties.Color);
            Assert.Equal(expectedWalkable, properties.Walkable);
            Assert.Equal(expectedFertility, properties.Fertility);
            Assert.NotNull(properties.Description);
        }

        [DataTestMethod]
        [InlineData(TerrainType.VerdantGrowth, FactionType.VerdantCollective)]
        [InlineData(TerrainType.AshenWasteland, FactionType.AshenLegion)]
        [InlineData(TerrainType.AquaticDomain, FactionType.TideWalkers)]
        [InlineData(TerrainType.StoneWardens, FactionType.CrystalChoir)]
        [InlineData(TerrainType.CelestialOrder, FactionType.NomadicCovenant)]
        [InlineData(TerrainType.NetherCult, FactionType.ScrapbornSwarm)]
        public void GetControllingFaction_FactionTerrain_ReturnsFaction(TerrainType terrain, FactionType expectedFaction)
        {
            // Act
            var faction = TerrainProperties.GetControllingFaction(terrain);

            // Assert
            Assert.NotNull(faction);
            Assert.Equal(expectedFaction, faction.Value);
        }

        [DataTestMethod]
        [InlineData(TerrainType.Void)]
        [InlineData(TerrainType.Soil)]
        [InlineData(TerrainType.Stone)]
        [InlineData(TerrainType.Water)]
        public void GetControllingFaction_NeutralTerrain_ReturnsNull(TerrainType terrain)
        {
            // Act
            var faction = TerrainProperties.GetControllingFaction(terrain);

            // Assert
            Assert.Null(faction);
        }

        [DataTestMethod]
        [InlineData(FactionType.VerdantCollective, TerrainType.VerdantGrowth)]
        [InlineData(FactionType.AshenLegion, TerrainType.AshenWasteland)]
        [InlineData(FactionType.TideWalkers, TerrainType.AquaticDomain)]
        [InlineData(FactionType.CrystalChoir, TerrainType.StoneWardens)]
        [InlineData(FactionType.NomadicCovenant, TerrainType.CelestialOrder)]
        [InlineData(FactionType.ScrapbornSwarm, TerrainType.NetherCult)]
        public void GetFactionTerrain_ReturnsCorrectTerrain(FactionType faction, TerrainType expectedTerrain)
        {
            // Act
            var terrain = TerrainProperties.GetFactionTerrain(faction);

            // Assert
            Assert.Equal(expectedTerrain, terrain);
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
            Assert.True(canConvert);
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
            Assert.True(canConvert);
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
            Assert.True(canConvert);
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
            Assert.False(canConvert);
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
            Assert.False(canConvert);
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
            Assert.Equal("Test Terrain", terrainData.Name);
            Assert.Equal("#FF0000", terrainData.Color);
            Assert.True(terrainData.Walkable);
            Assert.Equal(75, terrainData.Fertility);
            Assert.Equal("Test description", terrainData.Description);
        }
    }
}


