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
            DebugDamage = -5,
            DebugCold = 140,
            DebugHeat = 50,
            DebugHunger = 101,
            DamageColor = ImmersiveStatsRgbColor.FromRgb(-10, 300, 42),
        };

        config.Normalize(800, 600);

        Assert.Equal(ImmersiveStatsClientConfig.MinimumBarWidth, config.BarWidth);
        Assert.Equal(600, config.BarHeight);
        Assert.Equal(640, config.BarX);
        Assert.Equal(0, config.BarY);
        Assert.Equal(0, config.DebugDamage);
        Assert.Equal(100, config.DebugCold);
        Assert.Equal(50, config.DebugHeat);
        Assert.Equal(100, config.DebugHunger);

        ImmersiveStatsRgbColor damage = config.GetColor(StatBarSegmentKind.Damage);
        Assert.Equal(0, damage.R);
        Assert.Equal(255, damage.G);
        Assert.Equal(42, damage.B);
    }

    [Fact]
    public void NormalizeCentersDefaultBarWhenNoPositionExists()
    {
        var config = new ImmersiveStatsClientConfig();

        config.Normalize(1000, 700);

        Assert.Equal(280, config.BarX);
        Assert.Equal(321, config.BarY);
    }
}
