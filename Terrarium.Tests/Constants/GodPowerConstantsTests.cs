namespace Terrarium.Tests.Constants;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Terrarium.Desktop.Constants;

/// <summary>
/// Unit tests for GodPowerConstants to verify all values are properly defined.
/// </summary>
[TestClass]
public class GodPowerConstantsTests
{
    [TestMethod]
    public void LightningStrikeDamage_ShouldBePositive()
    {
        Assert.IsGreaterThan(
            0.0,
            GodPowerConstants.LIGHTNINGSTRIKEDAMAGE,
            "Lightning damage must be positive");
    }

    [TestMethod]
    public void LightningStrikeTargetCount_ShouldBePositive()
    {
        Assert.IsGreaterThan(
            0,
            GodPowerConstants.LIGHTNINGSTRIKETARGETCOUNT,
            "Lightning target count must be positive");
    }

    [TestMethod]
    public void MeteorBaseDamage_ShouldBePositive()
    {
        Assert.IsGreaterThan(
            0.0,
            GodPowerConstants.METEORBASEDAMAGE,
            "Meteor damage must be positive");
    }

    [TestMethod]
    public void MeteorShowerCount_ShouldBePositive()
    {
        Assert.IsGreaterThan(
            0,
            GodPowerConstants.METEORSHOWERCOUNT,
            "Meteor count must be positive");
    }

    [TestMethod]
    public void MeteorImpactRadius_ShouldBePositive()
    {
        Assert.IsGreaterThan(
            0.0,
            GodPowerConstants.METEORIMPACTRADIUSPIXELS,
            "Meteor radius must be positive");
    }

    [TestMethod]
    public void PlagueInitialDamage_ShouldBePositive()
    {
        Assert.IsGreaterThan(
            0.0,
            GodPowerConstants.PLAGUEINITIALDAMAGE,
            "Plague damage must be positive");
    }

    [TestMethod]
    public void PlaguInfectionCount_ShouldBePositive()
    {
        Assert.IsGreaterThan(
            0,
            GodPowerConstants.PLAGUEINFECTIONCOUNT,
            "Plague infection count must be positive");
    }

    [TestMethod]
    public void FertilityBlessingMultiplier_ShouldBeGreaterThanOne()
    {
        Assert.IsGreaterThan(
            1.0,
            GodPowerConstants.FERTILITYBLESSINGMULTIPLIER,
            "Fertility multiplier must be greater than 1 to increase reproduction");
    }

    [TestMethod]
    public void FertilityBlessingDuration_ShouldBePositive()
    {
        Assert.IsGreaterThan(
            0.0,
            GodPowerConstants.FERTILITYBLESSINGDURATIONSECONDS,
            "Fertility duration must be positive");
    }

    [TestMethod]
    public void AbundancePlantCount_ShouldBePositive()
    {
        Assert.IsGreaterThan(
            0,
            GodPowerConstants.ABUNDANCEPLANTCOUNT,
            "Abundance plant count must be positive");
    }

    [TestMethod]
    public void CorruptionTargetCount_ShouldBePositive()
    {
        Assert.IsGreaterThan(
            0,
            GodPowerConstants.CORRUPTIONTARGETCOUNT,
            "Corruption target count must be positive");
    }
}
