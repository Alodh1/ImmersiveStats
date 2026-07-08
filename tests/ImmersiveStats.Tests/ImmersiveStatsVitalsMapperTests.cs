using ImmersiveStats.Client;
using ImmersiveStats.Stats;
using Vintagestory.API.Common;
using Xunit;

namespace ImmersiveStats.Tests;

public sealed class ImmersiveStatsVitalsMapperTests
{
    [Fact]
    public void MissingHealthPercentMapsToDamageReducer()
    {
        StatBarState state = ToState(currentHealth: 65, maxHealth: 100, currentSaturation: 100, maxSaturation: 100);

        AssertClose(35, state.Damage);
        AssertClose(0, state.Hunger);
        AssertClose(100, state.Capacity);
    }

    [Fact]
    public void MissingSaturationPercentMapsToHungerReducer()
    {
        StatBarState state = ToState(currentHealth: 100, maxHealth: 100, currentSaturation: 42, maxSaturation: 100);

        AssertClose(0, state.Damage);
        AssertClose(58, state.Hunger);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    [InlineData(float.NaN)]
    [InlineData(float.PositiveInfinity)]
    public void InvalidMaxValuesClampReducersToZero(float maxValue)
    {
        StatBarState state = ToState(currentHealth: 1, maxValue, currentSaturation: 1, maxValue);

        AssertClose(0, state.Damage);
        AssertClose(0, state.Hunger);
    }

    [Fact]
    public void RealVitalsKeepColdAndHeatAtZeroOutsideDebugMode()
    {
        StatBarState state = ToState(currentHealth: 50, maxHealth: 100, currentSaturation: 50, maxSaturation: 100);

        AssertClose(0, state.Cold);
        AssertClose(0, state.Heat);
    }

    [Fact]
    public void CategorizedHealthLossMapsToSeparateReducers()
    {
        ImmersiveStatsVitalsSnapshot snapshot = ImmersiveStatsVitalsMapper.CreateCategorizedSnapshot(
            currentHealth: 50,
            maxHealth: 100,
            currentSaturation: 100,
            maxSaturation: 100,
            damageHealthPoints: 10,
            coldHealthPoints: 15,
            heatHealthPoints: 20,
            hungerHealthPoints: 5);

        StatBarState state = ImmersiveStatsVitalsMapper.ToStatBarState(snapshot);

        AssertClose(10, state.Damage);
        AssertClose(15, state.Cold);
        AssertClose(20, state.Heat);
        AssertClose(5, state.Hunger);
    }

    [Fact]
    public void HealthHungerAndMissingSaturationShareHungerReducer()
    {
        ImmersiveStatsVitalsSnapshot snapshot = ImmersiveStatsVitalsMapper.CreateCategorizedSnapshot(
            currentHealth: 90,
            maxHealth: 100,
            currentSaturation: 50,
            maxSaturation: 100,
            damageHealthPoints: 0,
            coldHealthPoints: 0,
            heatHealthPoints: 0,
            hungerHealthPoints: 10);

        StatBarState state = ImmersiveStatsVitalsMapper.ToStatBarState(snapshot);

        AssertClose(60, state.Hunger);
    }

    [Fact]
    public void CombinedReducersStillUseLayoutCapacityClamping()
    {
        StatBarState state = ToState(currentHealth: 0, maxHealth: 100, currentSaturation: 0, maxSaturation: 100);
        StatBarLayoutResult layout = StatBarLayout.Calculate(state);

        AssertClose(0, layout.EnergyAmount);
        Assert.Equal([StatBarSegmentKind.Damage], layout.Segments.Select(segment => segment.Kind).ToArray());
        AssertClose(100, layout.Segments[0].RenderedAmount);
    }

    [Theory]
    [InlineData(EnumDamageType.Frost, StatBarSegmentKind.Cold)]
    [InlineData(EnumDamageType.Fire, StatBarSegmentKind.Heat)]
    [InlineData(EnumDamageType.Heat, StatBarSegmentKind.Heat)]
    [InlineData(EnumDamageType.Hunger, StatBarSegmentKind.Hunger)]
    [InlineData(EnumDamageType.Poison, StatBarSegmentKind.Damage)]
    [InlineData(EnumDamageType.Gravity, StatBarSegmentKind.Damage)]
    public void DamageTypesClassifyIntoHudSegments(EnumDamageType damageType, StatBarSegmentKind expected)
    {
        var source = new DamageSource
        {
            Source = EnumDamageSource.Fall,
            Type = damageType,
        };

        Assert.Equal(expected, ImmersiveStatsDamageSourceClassifier.Classify(source));
    }

    [Fact]
    public void BucketsReconcileHealingProportionally()
    {
        var buckets = new ImmersiveStatsDamageBuckets();
        buckets.Add(StatBarSegmentKind.Damage, 10);
        buckets.Add(StatBarSegmentKind.Cold, 30);

        buckets.ReconcileToMissingHealth(20);

        AssertClose(5, buckets.Damage);
        AssertClose(15, buckets.Cold);
    }

    [Fact]
    public void BucketsAssignUnknownExistingMissingHealthToDamage()
    {
        var buckets = new ImmersiveStatsDamageBuckets();

        buckets.ReconcileToMissingHealth(12);

        AssertClose(12, buckets.Damage);
        AssertClose(0, buckets.Cold);
        AssertClose(0, buckets.Heat);
        AssertClose(0, buckets.Hunger);
    }

    [Fact]
    public void DebugModeBypassesWatchedVitalsSource()
    {
        var config = new ImmersiveStatsClientConfig
        {
            DebugModeEnabled = true,
            DebugDamage = 12,
            DebugCold = 3,
            DebugHeat = 4,
            DebugHunger = 5,
        };
        var source = new FakeVitalsSource(ImmersiveStatsVitalsMapper.CreateSnapshot(10, 100, 10, 100));

        StatBarState state = ImmersiveStatsDisplayStateResolver.Resolve(config, source, StatBarState.Empty);

        Assert.False(source.WasRead);
        AssertClose(12, state.Damage);
        AssertClose(3, state.Cold);
        AssertClose(4, state.Heat);
        AssertClose(5, state.Hunger);
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
