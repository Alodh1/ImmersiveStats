using ImmersiveStats.Client;
using ImmersiveStats.Network;
using ImmersiveStats.Stats;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Xunit;

namespace ImmersiveStats.Tests;

public sealed class ImmersiveStatsVitalsMapperTests
{
    [Fact]
    public void FallbackMissingHealthMapsToBluntEnergy()
    {
        StatBarState state = ToState(currentHealth: 18, maxHealth: 20, currentSaturation: 5000, maxSaturation: 5000);

        AssertClose(500, state.BluntTrauma);
        AssertClose(0, state.Hunger);
        AssertClose(5000, state.Capacity);
    }

    [Fact]
    public void MissingSatietyMapsToHungerEnergyAgainstFiveThousandCapacity()
    {
        StatBarState state = ToState(currentHealth: 20, maxHealth: 20, currentSaturation: 1500, maxSaturation: 5000);

        AssertClose(0, state.BluntTrauma);
        AssertClose(3500, state.Hunger);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    [InlineData(float.NaN)]
    [InlineData(float.PositiveInfinity)]
    public void InvalidMaxHealthClampsFallbackHealthDamageToZero(float maxHealth)
    {
        StatBarState state = ToState(currentHealth: 1, maxHealth, currentSaturation: 5000, maxSaturation: 5000);

        AssertClose(0, state.BluntTrauma);
        AssertClose(0, state.Hunger);
    }

    [Fact]
    public void CategorizedEnergyMapsToParentReducersAndActiveFlags()
    {
        ImmersiveStatsVitalsSnapshot snapshot = ImmersiveStatsVitalsMapper.CreateCategorizedSnapshot(
            currentHealth: 50,
            maxHealth: 100,
            currentSaturation: 5000,
            maxSaturation: 5000,
            new Dictionary<StatBarSegmentKind, float>
            {
                [StatBarSegmentKind.PenetratingTrauma] = 100,
                [StatBarSegmentKind.BluntTrauma] = 200,
                [StatBarSegmentKind.Burn] = 300,
                [StatBarSegmentKind.CoreTemperature] = 400,
                [StatBarSegmentKind.Toxic] = 500,
                [StatBarSegmentKind.Asphyxiation] = 600,
                [StatBarSegmentKind.Hunger] = 700,
            },
            [StatBarSegmentKind.PenetratingTrauma, StatBarSegmentKind.Burn]);

        StatBarState state = ImmersiveStatsVitalsMapper.ToStatBarState(snapshot);

        AssertClose(100, state.PenetratingTrauma);
        AssertClose(200, state.BluntTrauma);
        AssertClose(300, state.Burn);
        AssertClose(400, state.CoreTemperature);
        AssertClose(500, state.Toxic);
        AssertClose(600, state.Asphyxiation);
        AssertClose(700, state.Hunger);
        Assert.True(state.IsConditionActive(StatBarSegmentKind.PenetratingTrauma));
        Assert.True(state.IsConditionActive(StatBarSegmentKind.Burn));
        Assert.False(state.IsConditionActive(StatBarSegmentKind.BluntTrauma));
    }

    [Fact]
    public void CombinedReducersStillUseLayoutCapacityClamping()
    {
        var state = new StatBarState(5000, new Dictionary<StatBarSegmentKind, float>
        {
            [StatBarSegmentKind.PenetratingTrauma] = 6000,
            [StatBarSegmentKind.Hunger] = 5000,
        });
        StatBarLayoutResult layout = StatBarLayout.Calculate(state);

        AssertClose(0, layout.EnergyAmount);
        Assert.Equal([StatBarSegmentKind.PenetratingTrauma], layout.Segments.Select(segment => segment.Kind).ToArray());
        AssertClose(5000, layout.Segments[0].RenderedAmount);
    }

    [Theory]
    [InlineData(EnumDamageType.PiercingAttack, StatBarSegmentKind.PenetratingTrauma)]
    [InlineData(EnumDamageType.SlashingAttack, StatBarSegmentKind.PenetratingTrauma)]
    [InlineData(EnumDamageType.BluntAttack, StatBarSegmentKind.BluntTrauma)]
    [InlineData(EnumDamageType.Gravity, StatBarSegmentKind.BluntTrauma)]
    [InlineData(EnumDamageType.Crushing, StatBarSegmentKind.BluntTrauma)]
    [InlineData(EnumDamageType.Injury, StatBarSegmentKind.BluntTrauma)]
    [InlineData(EnumDamageType.Fire, StatBarSegmentKind.Burn)]
    [InlineData(EnumDamageType.Frost, StatBarSegmentKind.Burn)]
    [InlineData(EnumDamageType.Heat, StatBarSegmentKind.Burn)]
    [InlineData(EnumDamageType.Poison, StatBarSegmentKind.Toxic)]
    [InlineData(EnumDamageType.Suffocation, StatBarSegmentKind.Asphyxiation)]
    [InlineData(EnumDamageType.Hunger, StatBarSegmentKind.Hunger)]
    public void DamageTypesClassifyIntoParentHudSegments(EnumDamageType damageType, StatBarSegmentKind expected)
    {
        var source = new DamageSource
        {
            Source = EnumDamageSource.Unknown,
            Type = damageType,
        };

        Assert.True(ImmersiveStatsDamageSourceClassifier.TryClassifyImmediate(source, out StatBarSegmentKind actual));
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(EnumDamageSource.Fall, EnumDamageType.Injury, StatBarSegmentKind.BluntTrauma)]
    [InlineData(EnumDamageSource.Drown, EnumDamageType.Injury, StatBarSegmentKind.Asphyxiation)]
    [InlineData(EnumDamageSource.Player, EnumDamageType.BluntAttack, StatBarSegmentKind.BluntTrauma)]
    [InlineData(EnumDamageSource.Entity, EnumDamageType.SlashingAttack, StatBarSegmentKind.PenetratingTrauma)]
    [InlineData(EnumDamageSource.Block, EnumDamageType.PiercingAttack, StatBarSegmentKind.PenetratingTrauma)]
    public void DamageSourcesClassifyIntoParentHudSegments(EnumDamageSource sourceKind, EnumDamageType damageType, StatBarSegmentKind expected)
    {
        var source = new DamageSource
        {
            Source = sourceKind,
            Type = damageType,
        };

        Assert.True(ImmersiveStatsDamageSourceClassifier.TryClassifyImmediate(source, out StatBarSegmentKind actual));
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(EnumDamageType.Electricity)]
    [InlineData(EnumDamageType.Acid)]
    public void OutOfScopeDamageTypesAreNotTrackedThisPass(EnumDamageType damageType)
    {
        var source = new DamageSource
        {
            Source = EnumDamageSource.Unknown,
            Type = damageType,
        };

        Assert.False(ImmersiveStatsDamageSourceClassifier.TryClassifyImmediate(source, out _));
    }

    [Fact]
    public void WeatherFrostIsHandledByTemperatureExposureInsteadOfImmediateDamage()
    {
        var source = new DamageSource
        {
            Source = EnumDamageSource.Weather,
            Type = EnumDamageType.Frost,
        };

        Assert.False(ImmersiveStatsDamageSourceClassifier.TryClassifyImmediate(source, out _));
    }

    [Fact]
    public void HealthDamageConvertsToEnergy()
    {
        AssertClose(875, ImmersiveStatsVitalsMapper.HealthDamageToEnergy(3.5f));
    }

    [Fact]
    public void HungerCapacityUpgradeKeepsCurrentSaturationRaw()
    {
        var hunger = new TreeAttribute();
        hunger.SetFloat("currentsaturation", 1500);
        hunger.SetFloat("maxsaturation", 1500);
        bool markedDirty = false;

        bool changed = ImmersiveStatsHungerCapacity.EnsureTargetCapacity(hunger, () => markedDirty = true);

        Assert.True(changed);
        Assert.True(markedDirty);
        AssertClose(1500, hunger.GetFloat("currentsaturation"));
        AssertClose(5000, hunger.GetFloat("maxsaturation"));
    }

    [Fact]
    public void TimedConditionPreservesRemainingEnergyWhenRetriggered()
    {
        var condition = new ImmersiveStatsTimedEnergyCondition();
        condition.Trigger(120, 120);

        AssertClose(60, condition.Tick(60));
        condition.Trigger(60, 120);

        AssertClose(120, condition.RemainingEnergy);
        AssertClose(120, condition.RemainingSeconds);
    }

    [Fact]
    public void ThermalExposureAccumulatesAndRecoversAfterSafeDelay()
    {
        var condition = new ImmersiveStatsThermalExposureCondition(thresholdCelsius: 35, energyPerDegreeSecond: 2);

        AssertClose(20, condition.Update(bodyTemperature: 34, deltaTime: 10));
        AssertClose(20, condition.Amount);

        AssertClose(0, condition.Update(bodyTemperature: 36, deltaTime: 9));
        AssertClose(20, condition.Amount);

        float firstRecoveryDelta = condition.Update(bodyTemperature: 36, deltaTime: 1);
        Assert.True(firstRecoveryDelta < 0);

        condition.Update(bodyTemperature: 36, deltaTime: 120);
        AssertClose(0, condition.Amount);
        Assert.False(condition.Active);
    }

    [Fact]
    public void VitalsPacketRoundTripsParentReducersAndActiveFlags()
    {
        ImmersiveStatsVitalsSnapshot snapshot = ImmersiveStatsVitalsMapper.CreateCategorizedSnapshot(
            currentHealth: 10,
            maxHealth: 100,
            currentSaturation: 5000,
            maxSaturation: 5000,
            new Dictionary<StatBarSegmentKind, float>
            {
                [StatBarSegmentKind.PenetratingTrauma] = 1,
                [StatBarSegmentKind.BluntTrauma] = 2,
                [StatBarSegmentKind.Burn] = 3,
                [StatBarSegmentKind.CoreTemperature] = 4,
                [StatBarSegmentKind.Toxic] = 5,
                [StatBarSegmentKind.Asphyxiation] = 6,
                [StatBarSegmentKind.Hunger] = 7,
            },
            [StatBarSegmentKind.PenetratingTrauma, StatBarSegmentKind.BluntTrauma, StatBarSegmentKind.Burn, StatBarSegmentKind.CoreTemperature]);

        ImmersiveStatsVitalsSnapshot roundTripped = ImmersiveStatsVitalsPacket.FromSnapshot(snapshot).ToSnapshot();

        Assert.Equal(snapshot, roundTripped);
    }

    [Theory]
    [InlineData(35, 28, 1, false)]
    [InlineData(36, 28, 1, true)]
    [InlineData(70, 56, 2, false)]
    [InlineData(72, 56, 2, true)]
    public void IconFitRequiresIconPlusPadding(double segmentWidth, double iconSize, double scale, bool expected)
    {
        Assert.Equal(expected, SegmentIconRenderer.CanFitIcon(segmentWidth, iconSize, scale));
    }

    [Fact]
    public void DebugModeBypassesWatchedVitalsSource()
    {
        var config = new ImmersiveStatsClientConfig
        {
            DebugModeEnabled = true,
            DebugPenetratingTrauma = 120,
            DebugBluntTrauma = 30,
            DebugBurn = 40,
            DebugCoreTemperature = 50,
            DebugToxic = 60,
            DebugAsphyxiation = 70,
            DebugHunger = 80,
        };
        var source = new FakeVitalsSource(ImmersiveStatsVitalsMapper.CreateSnapshot(10, 100, 10, 5000));

        StatBarState state = ImmersiveStatsDisplayStateResolver.Resolve(config, source, StatBarState.Empty);

        Assert.False(source.WasRead);
        AssertClose(120, state.PenetratingTrauma);
        AssertClose(30, state.BluntTrauma);
        AssertClose(40, state.Burn);
        AssertClose(50, state.CoreTemperature);
        AssertClose(60, state.Toxic);
        AssertClose(70, state.Asphyxiation);
        AssertClose(80, state.Hunger);
    }

    private static StatBarState ToState(float currentHealth, float maxHealth, float currentSaturation, float maxSaturation)
    {
        ImmersiveStatsVitalsSnapshot snapshot = ImmersiveStatsVitalsMapper.CreateSnapshot(currentHealth, maxHealth, currentSaturation, maxSaturation);
        return ImmersiveStatsVitalsMapper.ToStatBarState(snapshot);
    }

    private static void AssertClose(float expected, float actual)
    {
        Assert.True(Math.Abs(expected - actual) < 0.0001f, $"Expected {expected}, got {actual}.");
    }

    private sealed class FakeVitalsSource : IImmersiveStatsVitalsSource
    {
        private readonly ImmersiveStatsVitalsSnapshot _snapshot;

        public FakeVitalsSource(ImmersiveStatsVitalsSnapshot snapshot)
        {
            _snapshot = snapshot;
        }

        public bool WasRead { get; private set; }

        public bool TryRead(out ImmersiveStatsVitalsSnapshot snapshot)
        {
            WasRead = true;
            snapshot = _snapshot;
            return true;
        }
    }
}
