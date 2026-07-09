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
        AssertClose(5000, segment.RenderedAmount);
        AssertClose(0, segment.StartFraction);
        AssertClose(1, segment.EndFraction);
    }

    [Fact]
    public void MultipleReducersProduceAdjacentNonOverlappingSegments()
    {
        StatBarLayoutResult layout = StatBarLayout.Calculate(new StatBarState(5000, 500, 250, 125, 1000));

        Assert.Equal(
            [StatBarSegmentKind.Energy, StatBarSegmentKind.PenetratingTrauma, StatBarSegmentKind.BluntTrauma, StatBarSegmentKind.Burn, StatBarSegmentKind.Hunger],
            layout.Segments.Select(segment => segment.Kind).ToArray());

        for (int i = 1; i < layout.Segments.Count; i++)
        {
            AssertClose(layout.Segments[i - 1].EndFraction, layout.Segments[i].StartFraction);
            Assert.True(layout.Segments[i].EndFraction <= 1.0001f);
        }
    }

    [Fact]
    public void IncreasingTraumaConsumesEnergyWithoutOverlappingOtherSegments()
    {
        StatBarLayoutResult lowTrauma = StatBarLayout.Calculate(new StatBarState(5000, 500, 250, 0, 500));
        StatBarLayoutResult highTrauma = StatBarLayout.Calculate(new StatBarState(5000, 1250, 250, 0, 500));

        Assert.True(highTrauma.EnergyAmount < lowTrauma.EnergyAmount);
        AssertClose(1250, highTrauma.Segments.Single(segment => segment.Kind == StatBarSegmentKind.PenetratingTrauma).RenderedAmount);

        for (int i = 1; i < highTrauma.Segments.Count; i++)
        {
            AssertClose(highTrauma.Segments[i - 1].EndFraction, highTrauma.Segments[i].StartFraction);
        }
    }

    [Fact]
    public void OverflowClampsLaterSegmentsAtCapacity()
    {
        var state = new StatBarState(5000, new Dictionary<StatBarSegmentKind, float>
        {
            [StatBarSegmentKind.PenetratingTrauma] = 3000,
            [StatBarSegmentKind.BluntTrauma] = 3000,
            [StatBarSegmentKind.Burn] = 1000,
            [StatBarSegmentKind.Hunger] = 1000,
        });

        StatBarLayoutResult layout = StatBarLayout.Calculate(state);

        AssertClose(0, layout.EnergyAmount);
        AssertClose(3000, layout.Segments.Single(segment => segment.Kind == StatBarSegmentKind.PenetratingTrauma).RenderedAmount);
        AssertClose(2000, layout.Segments.Single(segment => segment.Kind == StatBarSegmentKind.BluntTrauma).RenderedAmount);
        Assert.DoesNotContain(layout.Segments, segment => segment.Kind is StatBarSegmentKind.Burn or StatBarSegmentKind.Hunger);
        AssertClose(1, layout.Segments[^1].EndFraction);
    }

    [Fact]
    public void HungerIsFinalRenderedReducerWhenItFits()
    {
        StatBarLayoutResult layout = StatBarLayout.Calculate(new StatBarState(5000, 500, 250, 125, 1000));

        Assert.Equal(StatBarSegmentKind.Hunger, layout.Segments.Last().Kind);
    }

    [Fact]
    public void ParentReducersRenderInConfiguredOrder()
    {
        var state = new StatBarState(5000, StatBarSegmentCatalog.ReducerKinds.ToDictionary(kind => kind, _ => 1f));

        StatBarLayoutResult layout = StatBarLayout.Calculate(state);

        StatBarSegmentKind[] expected = new[] { StatBarSegmentKind.Energy }.Concat(StatBarSegmentCatalog.ReducerKinds).ToArray();
        Assert.Equal(expected, layout.Segments.Select(segment => segment.Kind).ToArray());
    }

    [Fact]
    public void ActiveConditionFlagIsCarriedOntoRenderedSegment()
    {
        var state = new StatBarState(
            5000,
            new Dictionary<StatBarSegmentKind, float>
            {
                [StatBarSegmentKind.PenetratingTrauma] = 500,
                [StatBarSegmentKind.BluntTrauma] = 500,
            },
            [StatBarSegmentKind.PenetratingTrauma]);

        StatBarLayoutResult layout = StatBarLayout.Calculate(state);

        Assert.True(layout.Segments.Single(segment => segment.Kind == StatBarSegmentKind.PenetratingTrauma).ActiveCondition);
        Assert.False(layout.Segments.Single(segment => segment.Kind == StatBarSegmentKind.BluntTrauma).ActiveCondition);
    }

    private static void AssertClose(float expected, float actual) => Assert.True(Math.Abs(expected - actual) < 0.0001f, $"Expected {expected}, got {actual}.");
}
