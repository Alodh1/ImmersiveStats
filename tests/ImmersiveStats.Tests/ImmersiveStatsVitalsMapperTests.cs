using ImmersiveStats.Client;
using ImmersiveStats.Stats;
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
    public void CombinedReducersStillUseLayoutCapacityClamping()
    {
        StatBarState state = ToState(currentHealth: 0, maxHealth: 100, currentSaturation: 0, maxSaturation: 100);
        StatBarLayoutResult layout = StatBarLayout.Calculate(state);

        AssertClose(0, layout.EnergyAmount);
        Assert.Equal([StatBarSegmentKind.Damage], layout.Segments.Select(segment => segment.Kind).ToArray());
        AssertClose(100, layout.Segments[0].RenderedAmount);
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
