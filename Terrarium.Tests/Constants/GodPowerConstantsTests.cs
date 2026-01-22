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
        Assert.IsGreaterThan(0.0, GodPowerConstants.LIGHTNING_STRIKE_DAMAGE,
            "Lightning damage must be positive");
    }

    [TestMethod]
    public void LightningStrikeTargetCount_ShouldBePositive()
    {
        Assert.IsGreaterThan(0, GodPowerConstants.LIGHTNING_STRIKE_TARGET_COUNT,
            "Lightning target count must be positive");
    }

    [TestMethod]
    public void MeteorBaseDamage_ShouldBePositive()
    {
        Assert.IsGreaterThan(0.0, GodPowerConstants.METEOR_BASE_DAMAGE,
            "Meteor damage must be positive");
    }

    [TestMethod]
    public void MeteorShowerCount_ShouldBePositive()
    {
        Assert.IsGreaterThan(0, GodPowerConstants.METEOR_SHOWER_COUNT,
            "Meteor count must be positive");
    }

    [TestMethod]
    public void MeteorImpactRadius_ShouldBePositive()
    {
        Assert.IsGreaterThan(0.0, GodPowerConstants.METEOR_IMPACT_RADIUS_PIXELS,
            "Meteor radius must be positive");
    }

    [TestMethod]
    public void PlagueInitialDamage_ShouldBePositive()
    {
        Assert.IsGreaterThan(0.0, GodPowerConstants.PLAGUE_INITIAL_DAMAGE,
            "Plague damage must be positive");
    }

    [TestMethod]
    public void PlaguInfectionCount_ShouldBePositive()
    {
        Assert.IsGreaterThan(0, GodPowerConstants.PLAGUE_INFECTION_COUNT,
            "Plague infection count must be positive");
    }

    [TestMethod]
    public void FertilityBlessingMultiplier_ShouldBeGreaterThanOne()
    {
        Assert.IsGreaterThan(1.0, GodPowerConstants.FERTILITY_BLESSING_MULTIPLIER,
            "Fertility multiplier must be greater than 1 to increase reproduction");
    }

    [TestMethod]
    public void FertilityBlessingDuration_ShouldBePositive()
    {
        Assert.IsGreaterThan(0.0, GodPowerConstants.FERTILITY_BLESSING_DURATION_SECONDS,
            "Fertility duration must be positive");
    }

    [TestMethod]
    public void AbundancePlantCount_ShouldBePositive()
    {
        Assert.IsGreaterThan(0, GodPowerConstants.ABUNDANCE_PLANT_COUNT,
            "Abundance plant count must be positive");
    }

    [TestMethod]
    public void CorruptionTargetCount_ShouldBePositive()
    {
        Assert.IsGreaterThan(0, GodPowerConstants.CORRUPTION_TARGET_COUNT,
            "Corruption target count must be positive");
    }
}
