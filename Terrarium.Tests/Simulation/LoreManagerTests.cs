using Xunit;
using Terrarium.Logic.Simulation;
using Terrarium.Logic.Entities;
using System;
using System.Linq;

namespace Terrarium.Tests.Simulation
{
    /// <summary>
    /// Comprehensive tests for LoreManager to achieve 100% coverage.
    /// </summary>
    public class LoreManagerTests
    {
        [TestMethod]
        public void Constructor_InitializesChronicle()
        {
            // Arrange & Act
            var loreManager = new LoreManager();

            // Assert
            Assert.NotNull(loreManager.Chronicle);
            Assert.NotEmpty(loreManager.Chronicle); // Should have ancient events
        }

        [TestMethod]
        public void Constructor_GeneratesAncientHistory()
        {
            // Arrange & Act
            var loreManager = new LoreManager();

            // Assert
            var ancientEvents = loreManager.Chronicle.Where(e => e.Type == LoreEventType.AncientHistory);
            Assert.InRange(ancientEvents.Count(), 3, 5); // Should generate 3-5 ancient events
        }

        [TestMethod]
        public void RecordEvent_AddsEventToChronicle()
        {
            // Arrange
            var loreManager = new LoreManager();
            int initialCount = loreManager.Chronicle.Count;

            // Act
            loreManager.RecordEvent("Test event", LoreEventType.Battle, LoreImportance.Major);

            // Assert
            Assert.AreEqual(initialCount + 1, loreManager.Chronicle.Count);
            Assert.IsTrue(loreManager.Chronicle.Any(e => e.Description == "Test event"));
        }

        [TestMethod]
        public void RecordEvent_LimitsChronicleSize()
        {
            // Arrange
            var loreManager = new LoreManager();

            // Act - add 110 events (should cap at 100)
            for (int i = 0; i < 110; i++)
            {
                loreManager.RecordEvent($"Event {i}", LoreEventType.FactionEvent, LoreImportance.Minor);
            }

            // Assert
            Assert.True(loreManager.Chronicle.Count <= 100);
        }

        [TestMethod]
        public void GenerateEventDescription_ReturnsDescription()
        {
            // Arrange
            var loreManager = new LoreManager();
            var factionManager = new FactionManager();
            var creatures = new[]
            {
                new Herbivore(100, 100, "Deer", faction: FactionType.VerdantCollective),
                new Carnivore(200, 200, "Wolf", faction: FactionType.AshenLegion)
            };
            factionManager.UpdatePopulations(creatures);

            // Act
            string description = loreManager.GenerateEventDescription(factionManager);

            // Assert
            Assert.NotNull(description);
            Assert.NotEmpty(description);
        }

        [TestMethod]
        public void GenerateEventDescription_WithNullFactionManager_ThrowsException()
        {
            // Arrange
            var loreManager = new LoreManager();

            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => 
                loreManager.GenerateEventDescription(null!));
        }

        [TestMethod]
        public void GenerateEventDescription_WithSingleFaction_ReturnsSpecialMessage()
        {
            // Arrange
            var loreManager = new LoreManager();
            var factionManager = new FactionManager();
            var creatures = new[]
            {
                new Herbivore(100, 100, "Deer", faction: FactionType.VerdantCollective)
            };
            factionManager.UpdatePopulations(creatures);

            // Act
            string description = loreManager.GenerateEventDescription(factionManager);

            // Assert
            Assert.IsTrue(description.Contains("Peace reigns"));
        }

        [TestMethod]
        public void TryCreateNamedCharacter_CreatesCharacterRandomly()
        {
            // Arrange
            var random = new Random(42); // Fixed seed for deterministic test
            var loreManager = new LoreManager(random);
            var creature = new Herbivore(100, 100, "Deer", faction: FactionType.VerdantCollective);

            // Act - try many times to ensure we hit the 1/50 chance
            for (int i = 0; i < 200; i++)
            {
                var testCreature = new Herbivore(100, 100, "Deer", faction: FactionType.VerdantCollective);
                loreManager.TryCreateNamedCharacter(testCreature);
            }

            // Assert - should have created at least one named character
            Assert.NotEmpty(loreManager.NamedCharacters);
        }

        [TestMethod]
        public void TryCreateNamedCharacter_WithNullCreature_ThrowsException()
        {
            // Arrange
            var loreManager = new LoreManager();

            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => 
                loreManager.TryCreateNamedCharacter(null!));
        }

        [TestMethod]
        public void RecordCharacterDeath_UpdatesCharacterStatus()
        {
            // Arrange
            var random = new Random(42);
            var loreManager = new LoreManager(random);
            
            // Force create a named character
            for (int i = 0; i < 200; i++)
            {
                var creature = new Herbivore(100 + i, 100, "Deer", faction: FactionType.VerdantCollective);
                loreManager.TryCreateNamedCharacter(creature);
            }

            var namedChar = loreManager.NamedCharacters.Values.FirstOrDefault();
            Assert.NotNull(namedChar);

            // Act
            loreManager.RecordCharacterDeath(namedChar.Id);

            // Assert
            Assert.False(namedChar.IsAlive);
            Assert.NotNull(namedChar.DeathTime);
        }

        [TestMethod]
        public void RecordCharacterDeath_WithNonexistentId_DoesNothing()
        {
            // Arrange
            var loreManager = new LoreManager();

            // Act & Assert - should not throw
            loreManager.RecordCharacterDeath(999999);
        }

        [TestMethod]
        public void GetEntityLore_ForNamedCharacter_ReturnsCustomLore()
        {
            // Arrange
            var random = new Random(42);
            var loreManager = new LoreManager(random);
            
            // Create named character
            for (int i = 0; i < 200; i++)
            {
                var creature = new Herbivore(100 + i, 100, "Deer", faction: FactionType.VerdantCollective);
                loreManager.TryCreateNamedCharacter(creature);
            }

            var namedChar = loreManager.NamedCharacters.Values.FirstOrDefault();
            Assert.NotNull(namedChar);
            var creature2 = new Herbivore(namedChar.Id, 100, "Deer", faction: namedChar.Faction);

            // Act
            string lore = loreManager.GetEntityLore(creature2);

            // Assert
            Assert.IsTrue(lore.Contains(namedChar.Name));
        }

        [TestMethod]
        public void GetEntityLore_ForUnnamedCreature_ReturnsProceduralLore()
        {
            // Arrange
            var loreManager = new LoreManager();
            var creature = new Herbivore(12345, 100, "Deer", faction: FactionType.VerdantCollective);

            // Act
            string lore = loreManager.GetEntityLore(creature);

            // Assert
            Assert.NotNull(lore);
            Assert.NotEmpty(lore);
        }

        [TestMethod]
        public void NamedCharacter_HasAllPropertiesInitialized()
        {
            // Arrange
            var namedChar = new NamedCharacter
            {
                Id = 1,
                Name = "Test Hero",
                Faction = FactionType.VerdantCollective,
                Biography = "A brave warrior",
                BirthTime = DateTime.Now,
                IsAlive = true
            };

            // Assert
            Assert.Equal(1, namedChar.Id);
            Assert.Equal("Test Hero", namedChar.Name);
            Assert.Equal(FactionType.VerdantCollective, namedChar.Faction);
            Assert.NotNull(namedChar.Titles);
            Assert.True(namedChar.IsAlive);
        }

        [TestMethod]
        public void LoreEvent_HasAllProperties()
        {
            // Arrange
            var loreEvent = new LoreEvent
            {
                Timestamp = DateTime.Now.Ticks,
                Description = "Test event",
                Type = LoreEventType.Battle,
                Importance = LoreImportance.Major
            };

            // Assert
            Assert.NotEqual(0, loreEvent.Timestamp);
            Assert.Equal("Test event", loreEvent.Description);
            Assert.Equal(LoreEventType.Battle, loreEvent.Type);
            Assert.Equal(LoreImportance.Major, loreEvent.Importance);
        }
    }
}


