using ImmersiveStats.Commands;
using Xunit;

namespace ImmersiveStats.Tests;

public sealed class ImmersiveStatsCommandParserTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("toggle")]
    public void ParseToggleCommands(string raw)
    {
        Assert.True(ImmersiveStatsCommandParser.TryParse(raw, out ImmersiveStatsEditCommand command, out string error), error);

        Assert.Equal(ImmersiveStatsEditCommandKind.Toggle, command.Kind);
    }

    [Theory]
    [InlineData("on", true)]
    [InlineData("enable", true)]
    [InlineData("disable", false)]
    [InlineData("off", false)]
    public void ParseExplicitStateCommands(string raw, bool expected)
    {
        Assert.True(ImmersiveStatsCommandParser.TryParse(raw, out ImmersiveStatsEditCommand command, out string error), error);

        Assert.Equal(ImmersiveStatsEditCommandKind.Set, command.Kind);
        Assert.Equal(expected, command.Enabled);
    }

    [Fact]
    public void RejectsRemovedSetCommand()
    {
        Assert.False(ImmersiveStatsCommandParser.TryParse("set damage 20", out _, out string error));

        Assert.Contains("set command was removed", error);
    }
}
