using ImmersiveStats.Client;
using Xunit;

namespace ImmersiveStats.Tests;

public sealed class HudPlacementMathTests
{
    [Fact]
    public void MoveClampsBarInsideViewport()
    {
        HudRect result = HudPlacementMath.Move(new HudRect(100, 100, 200, 60), 900, -200, 640, 360, 160, 36);

        Assert.Equal(440, result.X);
        Assert.Equal(0, result.Y);
        Assert.Equal(200, result.Width);
        Assert.Equal(60, result.Height);
    }

    [Fact]
    public void ResizeFromBottomRightClampsToViewport()
    {
        HudRect result = HudPlacementMath.Resize(new HudRect(100, 80, 200, 60), HudResizeHandle.BottomRight, 500, 400, 420, 260, 160, 36);

        Assert.Equal(100, result.X);
        Assert.Equal(80, result.Y);
        Assert.Equal(320, result.Width);
        Assert.Equal(180, result.Height);
    }

    [Fact]
    public void ResizeFromTopLeftKeepsMinimumSize()
    {
        HudRect result = HudPlacementMath.Resize(new HudRect(100, 80, 200, 60), HudResizeHandle.TopLeft, 180, 50, 640, 360, 160, 36);

        Assert.Equal(140, result.X);
        Assert.Equal(104, result.Y);
        Assert.Equal(160, result.Width);
        Assert.Equal(36, result.Height);
    }
}
