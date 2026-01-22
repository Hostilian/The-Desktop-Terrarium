using Xunit;
using Terrarium.Logic.Simulation;

namespace Terrarium.Tests.Simulation
{
    /// <summary>
    /// Comprehensive tests for Faction class to achieve 100% coverage.
    /// </summary>
    public class FactionTests
    {
        [DataTestMethod]
        [DataRow(FactionType.VerdantCollective, "Verdant Collective", "#22AA22")]
        [DataRow(FactionType.AshenLegion, "Ashen Legion", "#AA2222")]
        [DataRow(FactionType.CrystalChoir, "Crystal Choir", "#AA22AA")]
        [DataRow(FactionType.TideWalkers, "Tide Walkers", "#2244AA")]
        [DataRow(FactionType.ScrapbornSwarm, "Scrapborn Swarm", "#AA8822")]
        [DataRow(FactionType.NomadicCovenant, "Nomadic Covenant", "#AAAA22")]
        public void Constructor_InitializesFactionCorrectly(FactionType type, string name, string color)
        {
            // Arrange & Act
            var faction = new Faction(type);

            // Assert
            Assert.AreEqual(type, faction.Type);
            Assert.AreEqual(name, faction.Name);
            Assert.AreEqual(color, faction.Color);
            Assert.NotNull(faction.Abilities);
            Assert.NotEmpty(faction.Abilities);
            Assert.NotNull(faction.Lore);
        }

        [Fact]
        public void GetRelationshipStatus_Allied_ReturnsCorrectStatus()
        {
            // Arrange
            var faction = new Faction(FactionType.VerdantCollective);
            faction.Relationships[FactionType.TideWalkers] = 60;

            // Act
            var status = faction.GetRelationshipStatus(FactionType.TideWalkers);

            // Assert
            Assert.AreEqual("Allied", status);
        }

        [Fact]
        public void GetRelationshipStatus_Friendly_ReturnsCorrectStatus()
        {
            // Arrange
            var faction = new Faction(FactionType.VerdantCollective);
            faction.Relationships[FactionType.NomadicCovenant] = 25;

            // Act
            var status = faction.GetRelationshipStatus(FactionType.NomadicCovenant);

            // Assert
            Assert.AreEqual("Friendly", status);
        }

        [Fact]
        public void GetRelationshipStatus_Neutral_ReturnsCorrectStatus()
        {
            // Arrange
            var faction = new Faction(FactionType.VerdantCollective);
            faction.Relationships[FactionType.ScrapbornSwarm] = 0;

            // Act
            var status = faction.GetRelationshipStatus(FactionType.ScrapbornSwarm);

            // Assert
            Assert.AreEqual("Neutral", status);
        }

        [Fact]
        public void GetRelationshipStatus_Unfriendly_ReturnsCorrectStatus()
        {
            // Arrange
            var faction = new Faction(FactionType.VerdantCollective);
            faction.Relationships[FactionType.TideWalkers] = -30;

            // Act
            var status = faction.GetRelationshipStatus(FactionType.TideWalkers);

            // Assert
            Assert.AreEqual("Unfriendly", status);
        }

        [Fact]
        public void GetRelationshipStatus_Hostile_ReturnsCorrectStatus()
        {
            // Arrange
            var faction = new Faction(FactionType.VerdantCollective);

            // Act - VerdantCollective has -80 relationship with AshenLegion by default
            var status = faction.GetRelationshipStatus(FactionType.AshenLegion);

            // Assert
            Assert.AreEqual("Hostile", status);
        }

        [Fact]
        public void AddResource_CreatesNewResourceType()
        {
            // Arrange
            var faction = new Faction(FactionType.VerdantCollective);

            // Act
            faction.AddResource("Wood", 50);

            // Assert
            Assert.True(faction.Resources.ContainsKey("Wood"));
            Assert.AreEqual(50, faction.Resources["Wood"]);
        }

        [Fact]
        public void AddResource_AddsToExistingResource()
        {
            // Arrange
            var faction = new Faction(FactionType.AshenLegion);
            faction.AddResource("Iron", 30);

            // Act
            faction.AddResource("Iron", 20);

            // Assert
            Assert.AreEqual(50, faction.Resources["Iron"]);
        }

        [Fact]
        public void ConsumeResource_WithSufficientAmount_ReturnsTrue()
        {
            // Arrange
            var faction = new Faction(FactionType.CrystalChoir);
            faction.AddResource("Crystal", 100);

            // Act
            bool consumed = faction.ConsumeResource("Crystal", 40);

            // Assert
            Assert.True(consumed);
            Assert.AreEqual(60, faction.Resources["Crystal"]);
        }

        [Fact]
        public void ConsumeResource_WithInsufficientAmount_ReturnsFalse()
        {
            // Arrange
            var faction = new Faction(FactionType.TideWalkers);
            faction.AddResource("Water", 30);

            // Act
            bool consumed = faction.ConsumeResource("Water", 50);

            // Assert
            Assert.False(consumed);
            Assert.AreEqual(30, faction.Resources["Water"]);
        }

        [Fact]
        public void ConsumeResource_NonexistentResource_ReturnsFalse()
        {
            // Arrange
            var faction = new Faction(FactionType.ScrapbornSwarm);

            // Act
            bool consumed = faction.ConsumeResource("Oil", 10);

            // Assert
            Assert.False(consumed);
        }

        [Fact]
        public void Population_CanBeSetAndRetrieved()
        {
            // Arrange
            var faction = new Faction(FactionType.NomadicCovenant);

            // Act
            faction.Population = 25;

            // Assert
            Assert.AreEqual(25, faction.Population);
        }

        [Fact]
        public void TerritoryControl_CanBeSetAndRetrieved()
        {
            // Arrange
            var faction = new Faction(FactionType.VerdantCollective);

            // Act
            faction.TerritoryControl = 45.5;

            // Assert
            Assert.AreEqual(45.5, faction.TerritoryControl);
        }
    }
}
