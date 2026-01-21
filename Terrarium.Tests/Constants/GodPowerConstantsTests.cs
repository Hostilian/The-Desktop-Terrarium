using Microsoft.VisualStudio.TestTools.UnitTesting;
using Terrarium.Desktop.Constants;

namespace Terrarium.Tests.Constants;

/// <summary>
/// Unit tests for GodPowerConstants to verify all values are properly defined.
/// </summary>
[TestClass]
public class GodPowerConstantsTests
{
    [TestMethod]
    public void LightningStrikeDamage_ShouldBePositive()
    {
        Assert.IsTrue(GodPowerConstants.LIGHTNING_STRIKE_DAMAGE > 0, 
            "Lightning damage must be positive");
    }

    [TestMethod]
    public void LightningStrikeTargetCount_ShouldBePositive()
    {
        Assert.IsTrue(GodPowerConstants.LIGHTNING_STRIKE_TARGET_COUNT > 0,
            "Lightning target count must be positive");
    }

    [TestMethod]
    public void MeteorBaseDamage_ShouldBePositive()
    {
        Assert.IsTrue(GodPowerConstants.METEOR_BASE_DAMAGE > 0,
            "Meteor damage must be positive");
    }

    [TestMethod]
    public void MeteorShowerCount_ShouldBePositive()
    {
        Assert.IsTrue(GodPowerConstants.METEOR_SHOWER_COUNT > 0,
            "Meteor count must be positive");
    }

    [TestMethod]
    public void MeteorImpactRadius_ShouldBePositive()
    {
        Assert.IsTrue(GodPowerConstants.METEOR_IMPACT_RADIUS_PIXELS > 0,
            "Meteor radius must be positive");
    }

    [TestMethod]
    public void PlagueInitialDamage_ShouldBePositive()
    {
        Assert.IsGreaterThan(GodPowerConstants.PLAGUE_INITIAL_DAMAGE, 0.0,
            "Plague damage must be positive");
    }

    [TestMethod]
    public void PlaguInfectionCount_ShouldBePositive()
    {
        Assert.IsGreaterThan(GodPowerConstants.PLAGUE_INFECTION_COUNT, 0,
            "Plague infection count must be positive");
    }

    [TestMethod]
    public void FertilityBlessingMultiplier_ShouldBeGreaterThanOne()
    {
        Assert.IsGreaterThan(GodPowerConstants.FERTILITY_BLESSING_MULTIPLIER, 1.0,
            "Fertility multiplier must be greater than 1 to increase reproduction");
    }

    [TestMethod]
    public void FertilityBlessingDuration_ShouldBePositive()
    {
        Assert.IsGreaterThan(GodPowerConstants.FERTILITY_BLESSING_DURATION_SECONDS, 0.0,
            "Fertility duration must be positive");
    }

    [TestMethod]
    public void AbundancePlantCount_ShouldBePositive()
    {
        Assert.IsGreaterThan(GodPowerConstants.ABUNDANCE_PLANT_COUNT, 0,
            "Abundance plant count must be positive");
    }

    [TestMethod]
    public void CorruptionTargetCount_ShouldBePositive()
    {
        Assert.IsGreaterThan(GodPowerConstants.CORRUPTION_TARGET_COUNT, 0,
            "Corruption target count must be positive");
    }
}
