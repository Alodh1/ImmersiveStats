using ImmersiveStats.Client;
using Xunit;

namespace ImmersiveStats.Tests;

public sealed class ImmersiveStatsClientConfigTests
{
    [Fact]
    public void NormalizeClampsBarBoundsSizeColorsAndDebugValues()
    {
        var config = new ImmersiveStatsClientConfig
        {
            BarX = 900,
            BarY = -40,
            BarWidth = 20,
            BarHeight = 900,
            DebugPenetratingTrauma = -5,
            DebugBluntTrauma = 6000,
            DebugBurn = 250,
            DebugCoreTemperature = 5001,
            DebugToxic = -2,
            DebugAsphyxiation = 50,
            DebugHunger = 5100,
            DamageColor = ImmersiveStatsRgbColor.FromRgb(-10, 300, 42),
            HeatColor = ImmersiveStatsRgbColor.FromRgb(280, -20, 128),
        };

        config.Normalize(800, 600);

        Assert.Equal(ImmersiveStatsClientConfig.MinimumBarWidth, config.BarWidth);
        Assert.Equal(600, config.BarHeight);
        Assert.Equal(640, config.BarX);
        Assert.Equal(0, config.BarY);
        Assert.Equal(0, config.DebugPenetratingTrauma);
        Assert.Equal(5000, config.DebugBluntTrauma);
        Assert.Equal(250, config.DebugBurn);
        Assert.Equal(5000, config.DebugCoreTemperature);
        Assert.Equal(0, config.DebugToxic);
        Assert.Equal(50, config.DebugAsphyxiation);
        Assert.Equal(5000, config.DebugHunger);

        ImmersiveStatsRgbColor blunt = config.GetColor(StatBarSegmentKind.BluntTrauma);
        Assert.Equal(0, blunt.R);
        Assert.Equal(255, blunt.G);
        Assert.Equal(42, blunt.B);

        ImmersiveStatsRgbColor burn = config.GetColor(StatBarSegmentKind.Burn);
        Assert.Equal(255, burn.R);
        Assert.Equal(0, burn.G);
        Assert.Equal(128, burn.B);
    }

    [Fact]
    public void NormalizeCentersDefaultBarWhenNoPositionExists()
    {
        var config = new ImmersiveStatsClientConfig();

        config.Normalize(1000, 700);

        Assert.Equal(280, config.BarX);
        Assert.Equal(302, config.BarY);
    }

    [Fact]
    public void NormalizeRaisesOldSmallBarToNewMinimumHeight()
    {
        var config = new ImmersiveStatsClientConfig
        {
            BarWidth = 440,
            BarHeight = 36,
        };

        config.Normalize(1000, 700);

        Assert.Equal(ImmersiveStatsClientConfig.MinimumBarHeight, config.BarHeight);
    }
}
