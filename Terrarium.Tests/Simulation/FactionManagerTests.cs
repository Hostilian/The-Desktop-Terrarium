
using Terrarium.Logic.Simulation;
using Terrarium.Logic.Entities;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace Terrarium.Tests.Simulation
{
    /// <summary>
    /// Comprehensive unit tests for FactionManager to achieve 100% code coverage.
    /// </summary>
    public class FactionManagerTests
    {
        [TestMethod]
        public void Constructor_InitializesAllFactions()
        {
            // Arrange & Act
            var manager = new FactionManager();

            // Assert
            Assert.IsNotNull(manager.Factions);
            var factionCount = System.Enum.GetValues(typeof(FactionType)).Length;
            Assert.AreEqual(factionCount, manager.Factions.Count);
        }

        [TestMethod]
        public void GetFaction_ReturnsCorrectFaction()
        {
            // Arrange
            var manager = new FactionManager();

            // Act
            var faction = manager.GetFaction(FactionType.VerdantCollective);

            // Assert
            Assert.IsNotNull(faction);
            Assert.AreEqual(FactionType.VerdantCollective, faction.Type);
        }

        [TestMethod]
        public void UpdatePopulations_CountsCreaturesCorrectly()
        {
            // Arrange
            var manager = new FactionManager();
            var creatures = new List<Creature>
            {
                new Herbivore(100, 100, "Deer", faction: FactionType.VerdantCollective),
                new Herbivore(200, 200, "Deer", faction: FactionType.VerdantCollective),
                new Carnivore(300, 300, "Wolf", faction: FactionType.AshenLegion),
                new Herbivore(400, 400, "Phoenix", faction: FactionType.TideWalkers)
            };

            // Act
            manager.UpdatePopulations(creatures);

            // Assert
            Assert.AreEqual(2, manager.GetFaction(FactionType.VerdantCollective).Population);
            Assert.AreEqual(1, manager.GetFaction(FactionType.AshenLegion).Population);
            Assert.AreEqual(1, manager.GetFaction(FactionType.TideWalkers).Population);
            Assert.AreEqual(0, manager.GetFaction(FactionType.CrystalChoir).Population);
        }

        [TestMethod]
        public void UpdatePopulations_ResetsPopulationsFirst()
        {
            // Arrange
            var manager = new FactionManager();
            var creatures1 = new List<Creature>
            {
                new Herbivore(100, 100, "Deer", faction: FactionType.VerdantCollective),
                new Herbivore(200, 200, "Deer", faction: FactionType.VerdantCollective)
            };

            manager.UpdatePopulations(creatures1);

            var creatures2 = new List<Creature>
            {
                new Carnivore(300, 300, "Wolf", faction: FactionType.AshenLegion)
            };

            // Act
            manager.UpdatePopulations(creatures2);

            // Assert - population should be reset
            Assert.AreEqual(0, manager.GetFaction(FactionType.VerdantCollective).Population);
            Assert.AreEqual(1, manager.GetFaction(FactionType.AshenLegion).Population);
        }

        [TestMethod]
        public void GetDominantFaction_ReturnsHighestPopulation()
        {
            // Arrange
            var manager = new FactionManager();
            var creatures = new List<Creature>
            {
                new Herbivore(100, 100, "Deer", faction: FactionType.VerdantCollective),
                new Herbivore(200, 200, "Deer", faction: FactionType.VerdantCollective),
                new Herbivore(300, 300, "Deer", faction: FactionType.VerdantCollective),
                new Carnivore(400, 400, "Wolf", faction: FactionType.AshenLegion)
            };

            manager.UpdatePopulations(creatures);

            // Act
            var dominant = manager.GetDominantFaction();

            // Assert
            Assert.IsNotNull(dominant);
            Assert.AreEqual(FactionType.VerdantCollective, dominant.Type);
            Assert.AreEqual(3, dominant.Population);
        }

        [TestMethod]
        public void GetDominantFaction_WithNoCreatures_ReturnsFirstFaction()
        {
            // Arrange
            var manager = new FactionManager();
            manager.UpdatePopulations(new List<Creature>());

            // Act
            var dominant = manager.GetDominantFaction();

            // Assert - should return first faction even with 0 population
            Assert.IsNotNull(dominant);
            Assert.AreEqual(0, dominant.Population);
        }

        [TestMethod]
        public void GetFactionsByPopulation_ReturnsSortedList()
        {
            // Arrange
            var manager = new FactionManager();
            var creatures = new List<Creature>
            {
                new Herbivore(100, 100, "Deer", faction: FactionType.VerdantCollective),
                new Carnivore(200, 200, "Wolf", faction: FactionType.AshenLegion),
                new Carnivore(300, 300, "Wolf", faction: FactionType.AshenLegion),
                new Carnivore(400, 400, "Wolf", faction: FactionType.AshenLegion),
                new Herbivore(500, 500, "Phoenix", faction: FactionType.TideWalkers),
                new Herbivore(600, 600, "Phoenix", faction: FactionType.TideWalkers)
            };

            manager.UpdatePopulations(creatures);

            // Act
            var sortedFactions = manager.GetFactionsByPopulation().ToList();

            // Assert
            Assert.IsTrue(sortedFactions[0].Population >= sortedFactions[1].Population);
            Assert.IsTrue(sortedFactions[1].Population >= sortedFactions[2].Population);
            Assert.AreEqual(3, sortedFactions[0].Population);
        }

        [TestMethod]
        public void AreHostile_SameFaction_ReturnsFalse()
        {
            // Arrange
            var manager = new FactionManager();

            // Act
            bool hostile = manager.AreHostile(FactionType.VerdantCollective, FactionType.VerdantCollective);

            // Assert
            Assert.IsFalse(hostile);
        }

        [TestMethod]
        public void AreHostile_WithNegativeRelationship_ReturnsTrue()
        {
            // Arrange
            var manager = new FactionManager();
            var faction1 = manager.GetFaction(FactionType.VerdantCollective);
            var faction2 = manager.GetFaction(FactionType.AshenLegion);
            
            // Set hostile relationship
            faction1.Relationships[FactionType.AshenLegion] = -60;

            // Act
            bool hostile = manager.AreHostile(FactionType.VerdantCollective, FactionType.AshenLegion);

            // Assert
            Assert.IsTrue(hostile);
        }

        [TestMethod]
        public void AreHostile_WithPositiveRelationship_ReturnsFalse()
        {
            // Arrange
            var manager = new FactionManager();
            var faction1 = manager.GetFaction(FactionType.VerdantCollective);
            var faction2 = manager.GetFaction(FactionType.TideWalkers);
            
            // Set friendly relationship
            faction1.Relationships[FactionType.TideWalkers] = 20;
            faction2.Relationships[FactionType.VerdantCollective] = 20;

            // Act
            bool hostile = manager.AreHostile(FactionType.VerdantCollective, FactionType.TideWalkers);

            // Assert
            Assert.IsFalse(hostile);
        }

        [TestMethod]
        public void AreAllied_SameFaction_ReturnsTrue()
        {
            // Arrange
            var manager = new FactionManager();

            // Act
            bool allied = manager.AreAllied(FactionType.VerdantCollective, FactionType.VerdantCollective);

            // Assert
            Assert.IsTrue(allied);
        }

        [TestMethod]
        public void AreAllied_WithHighPositiveRelationship_ReturnsTrue()
        {
            // Arrange
            var manager = new FactionManager();
            var faction1 = manager.GetFaction(FactionType.VerdantCollective);
            var faction2 = manager.GetFaction(FactionType.TideWalkers);
            
            // Set alliance relationship
            faction1.Relationships[FactionType.TideWalkers] = 60;
            faction2.Relationships[FactionType.VerdantCollective] = 60;

            // Act
            bool allied = manager.AreAllied(FactionType.VerdantCollective, FactionType.TideWalkers);

            // Assert
            Assert.IsTrue(allied);
        }

        [TestMethod]
        public void AreAllied_WithLowRelationship_ReturnsFalse()
        {
            // Arrange
            var manager = new FactionManager();
            var faction1 = manager.GetFaction(FactionType.VerdantCollective);
            var faction2 = manager.GetFaction(FactionType.AshenLegion);
            
            // Set neutral relationship
            faction1.Relationships[FactionType.AshenLegion] = 0;
            faction2.Relationships[FactionType.VerdantCollective] = 0;

            // Act
            bool allied = manager.AreAllied(FactionType.VerdantCollective, FactionType.AshenLegion);

            // Assert
            Assert.IsFalse(allied);
        }

        [TestMethod]
        public void AreAllied_WithAsymmetricRelationship_ReturnsFalse()
        {
            // Arrange
            var manager = new FactionManager();
            var faction1 = manager.GetFaction(FactionType.VerdantCollective);
            var faction2 = manager.GetFaction(FactionType.TideWalkers);
            
            // One likes the other, but not mutual
            faction1.Relationships[FactionType.TideWalkers] = 60;
            faction2.Relationships[FactionType.VerdantCollective] = 30;

            // Act
            bool allied = manager.AreAllied(FactionType.VerdantCollective, FactionType.TideWalkers);

            // Assert
            Assert.IsFalse(allied);
        }
    }
}





