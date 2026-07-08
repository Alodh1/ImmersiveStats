using Xunit;

namespace ImmersiveStats.Tests;

public sealed class StatBarLayoutTests
{
    [Fact]
    public void EmptyReducersProduceFullEnergy()
    {
        StatBarLayoutResult layout = StatBarLayout.Calculate(StatBarState.Empty);

        StatBarSegment segment = Assert.Single(layout.Segments);
        Assert.Equal(StatBarSegmentKind.Energy, segment.Kind);
        AssertClose(100, segment.RenderedAmount);
        AssertClose(0, segment.StartFraction);
        AssertClose(1, segment.EndFraction);
    }

    [Fact]
    public void MultipleReducersProduceAdjacentNonOverlappingSegments()
    {
        StatBarLayoutResult layout = StatBarLayout.Calculate(new StatBarState(100, 10, 5, 3, 20));

        Assert.Equal(
            [StatBarSegmentKind.Energy, StatBarSegmentKind.Damage, StatBarSegmentKind.Cold, StatBarSegmentKind.Heat, StatBarSegmentKind.Hunger],
            layout.Segments.Select(segment => segment.Kind).ToArray());

        for (int i = 1; i < layout.Segments.Count; i++)
        {
            AssertClose(layout.Segments[i - 1].EndFraction, layout.Segments[i].StartFraction);
            Assert.True(layout.Segments[i].EndFraction <= 1.0001f);
        }
    }

    [Fact]
    public void IncreasingDamageConsumesEnergyWithoutOverlappingOtherSegments()
    {
        StatBarLayoutResult lowDamage = StatBarLayout.Calculate(new StatBarState(100, 10, 5, 0, 10));
        StatBarLayoutResult highDamage = StatBarLayout.Calculate(new StatBarState(100, 25, 5, 0, 10));

        Assert.True(highDamage.EnergyAmount < lowDamage.EnergyAmount);
        AssertClose(25, highDamage.Segments.Single(segment => segment.Kind == StatBarSegmentKind.Damage).RenderedAmount);

        for (int i = 1; i < highDamage.Segments.Count; i++)
        {
            AssertClose(highDamage.Segments[i - 1].EndFraction, highDamage.Segments[i].StartFraction);
        }
    }

    [Fact]
    public void OverflowClampsLaterSegmentsAtCapacity()
    {
        StatBarLayoutResult layout = StatBarLayout.Calculate(new StatBarState(100, 60, 60, 20, 20));

        AssertClose(0, layout.EnergyAmount);
        AssertClose(60, layout.Segments.Single(segment => segment.Kind == StatBarSegmentKind.Damage).RenderedAmount);
        AssertClose(40, layout.Segments.Single(segment => segment.Kind == StatBarSegmentKind.Cold).RenderedAmount);
        Assert.DoesNotContain(layout.Segments, segment => segment.Kind is StatBarSegmentKind.Heat or StatBarSegmentKind.Hunger);
        AssertClose(1, layout.Segments[^1].EndFraction);
    }

    [Fact]
    public void HungerIsFinalRenderedReducerWhenItFits()
    {
        StatBarLayoutResult layout = StatBarLayout.Calculate(new StatBarState(100, 10, 5, 3, 20));

        Assert.Equal(StatBarSegmentKind.Hunger, layout.Segments.Last().Kind);
    }

    private static void AssertClose(float expected, float actual) => Assert.True(Math.Abs(expected - actual) < 0.0001f, $"Expected {expected}, got {actual}.");
}
