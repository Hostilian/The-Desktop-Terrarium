namespace Terrarium.Desktop.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using Terrarium.Desktop.Constants;
using Terrarium.Logic.Entities;
using Terrarium.Logic.Simulation;

/// <summary>
/// Service responsible for executing god power effects on the simulation.
/// Encapsulates all divine intervention logic to keep it separate from UI concerns.
/// </summary>
public class GodPowerService
{
    private readonly SimulationEngine simulationEngine;
    private readonly Random random;

    /// <summary>
    /// Initializes a new instance of the <see cref="GodPowerService"/> class.
    /// Initializes a new instance of the GodPowerService.
    /// </summary>
    /// <param name="simulationEngine">The simulation engine to apply god powers to.</param>
    public GodPowerService(SimulationEngine simulationEngine)
    {
        this.simulationEngine = simulationEngine ?? throw new ArgumentNullException(nameof(simulationEngine));
        random = new Random();
    }

    /// <summary>
    /// Strikes random entities with lightning.
    /// </summary>
    /// <returns>Number of entities struck.</returns>
    public int ExecuteLightningStrike()
    {
        var allEntities = simulationEngine.World.GetAllEntities().ToList();
        if (allEntities.Count == 0) return 0;

        int struck = 0;
        for (int i = 0; i < Math.Min(GodPowerConstants.LIGHTNINGSTRIKETARGETCOUNT, allEntities.Count); i++)
        {
            var entity = allEntities[random.Next(allEntities.Count)];
            entity.TakeDamage(GodPowerConstants.LIGHTNINGSTRIKEDAMAGE);
            struck++;
        }
        return struck;
    }

    /// <summary>
    /// Unleashes a meteor shower across the world.
    /// </summary>
    /// <returns>Number of entities damaged.</returns>
    public int ExecuteMeteorShower()
    {
        var allEntities = simulationEngine.World.GetAllEntities().ToList();
        if (allEntities.Count == 0) return 0;

        int damaged = 0;
        for (int i = 0; i < GodPowerConstants.METEORSHOWERCOUNT; i++)
        {
            double impactX = random.NextDouble() * simulationEngine.World.Width;
            double impactY = random.NextDouble() * simulationEngine.World.Height;

            foreach (var entity in allEntities)
            {
                double distance = CalculateDistance(entity.X, entity.Y, impactX, impactY);
                if (distance <= GodPowerConstants.METEORIMPACTRADIUSPIXELS)
                {
                    double damage = GodPowerConstants.METEORBASEDAMAGE * (1 - (distance / GodPowerConstants.METEORIMPACTRADIUSPIXELS));
                    entity.TakeDamage(damage);
                    damaged++;
                }
            }
        }
        return damaged;
    }

    /// <summary>
    /// Infects random creatures with plague.
    /// </summary>
    /// <returns>Number of creatures infected.</returns>
    public int ExecutePlague()
    {
        var allCreatures = simulationEngine.World.GetAllEntities().OfType<Creature>().ToList();
        if (allCreatures.Count == 0) return 0;

        int infected = 0;
        for (int i = 0; i < Math.Min(GodPowerConstants.PLAGUEINFECTIONCOUNT, allCreatures.Count); i++)
        {
            var creature = allCreatures[random.Next(allCreatures.Count)];
            creature.TakeDamage(GodPowerConstants.PLAGUEINITIALDAMAGE);
            infected++;
        }
        return infected;
    }

    /// <summary>
    /// Applies a fertility blessing that temporarily boosts reproduction rates.
    /// </summary>
    /// <returns>The duration in seconds that the blessing will last.</returns>
    public double ApplyFertilityBlessing()
    {
        simulationEngine.ReproductionManager.HerbivoreReproductionChanceMultiplier *= GodPowerConstants.FERTILITYBLESSINGMULTIPLIER;
        simulationEngine.ReproductionManager.CarnivoreReproductionChanceMultiplier *= GodPowerConstants.FERTILITYBLESSINGMULTIPLIER;
        return GodPowerConstants.FERTILITYBLESSINGDURATIONSECONDS;
    }

    /// <summary>
    /// Removes a fertility blessing effect.
    /// </summary>
    public void RemoveFertilityBlessing()
    {
        simulationEngine.ReproductionManager.HerbivoreReproductionChanceMultiplier /= GodPowerConstants.FERTILITYBLESSINGMULTIPLIER;
        simulationEngine.ReproductionManager.CarnivoreReproductionChanceMultiplier /= GodPowerConstants.FERTILITYBLESSINGMULTIPLIER;
    }

    /// <summary>
    /// Creates abundance by spawning additional plants.
    /// </summary>
    /// <returns>Number of plants created.</returns>
    public int CreateAbundance()
    {
        for (int i = 0; i < GodPowerConstants.ABUNDANCEPLANTCOUNT; i++)
        {
            simulationEngine.World.SpawnRandomPlant();
        }
        return GodPowerConstants.ABUNDANCEPLANTCOUNT;
    }

    /// <summary>
    /// Corrupts random creatures by changing their faction allegiance.
    /// </summary>
    /// <returns>Number of creatures corrupted.</returns>
    public int ExecuteCorruption()
    {
        var allCreatures = simulationEngine.World.GetAllEntities().OfType<Creature>().ToList();
        if (allCreatures.Count == 0) return 0;

        var factionTypes = Enum.GetValues<FactionType>().ToArray();
        int corrupted = 0;

        for (int i = 0; i < Math.Min(GodPowerConstants.CORRUPTIONTARGETCOUNT, allCreatures.Count); i++)
        {
            var creature = allCreatures[random.Next(allCreatures.Count)];
            var currentFaction = creature.Faction;

            FactionType newFaction;
            do
            {
                newFaction = factionTypes[random.Next(factionTypes.Length)];
            }
            while (newFaction == currentFaction && factionTypes.Length > 1);

            creature.Faction = newFaction;
            corrupted++;
        }
        return corrupted;
    }

    /// <summary>
    /// Spawns a random plant in the world.
    /// </summary>
    /// <returns>The spawned plant.</returns>
    public Plant SpawnPlant()
    {
        return simulationEngine.World.SpawnRandomPlant();
    }

    /// <summary>
    /// Spawns a random herbivore in the world.
    /// </summary>
    /// <param name="faction">Optional faction to assign. If null, uses random faction.</param>
    /// <returns>The spawned herbivore.</returns>
    public Herbivore SpawnHerbivore(FactionType? faction = null)
    {
        var actualFaction = faction ?? GetRandomFaction();
        return simulationEngine.World.SpawnRandomHerbivore("Rabbit", actualFaction);
    }

    /// <summary>
    /// Spawns a random carnivore in the world.
    /// </summary>
    /// <param name="faction">Optional faction to assign. If null, uses random faction.</param>
    /// <returns>The spawned carnivore.</returns>
    public Carnivore SpawnCarnivore(FactionType? faction = null)
    {
        var actualFaction = faction ?? GetRandomFaction();
        return simulationEngine.World.SpawnRandomCarnivore("Wolf", actualFaction);
    }

    private FactionType GetRandomFaction()
    {
        var factions = Enum.GetValues<FactionType>();
        return factions[random.Next(factions.Length)];
    }

    private static double CalculateDistance(double x1, double y1, double x2, double y2)
    {
        return Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
    }
}
