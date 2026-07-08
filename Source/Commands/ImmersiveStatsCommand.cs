namespace ImmersiveStats.Commands;

internal enum ImmersiveStatsEditCommandKind
{
    Toggle,
    Set,
}

internal readonly record struct ImmersiveStatsEditCommand(ImmersiveStatsEditCommandKind Kind, bool Enabled)
{
    public static ImmersiveStatsEditCommand Toggle { get; } = new(ImmersiveStatsEditCommandKind.Toggle, false);

    public static ImmersiveStatsEditCommand Set(bool enabled) => new(ImmersiveStatsEditCommandKind.Set, enabled);
}

internal static class ImmersiveStatsCommandParser
{
    public static bool TryParse(string? raw, out ImmersiveStatsEditCommand command, out string error)
    {
        command = ImmersiveStatsEditCommand.Toggle;
        error = string.Empty;

        string trimmed = raw?.Trim() ?? string.Empty;
        if (trimmed.Length == 0)
        {
            return true;
        }

        string[] tokens = trimmed.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length != 1)
        {
            error = ExpectedUsageMessage(tokens[0]);
            return false;
        }

        switch (tokens[0].ToLowerInvariant())
        {
            case "toggle":
                command = ImmersiveStatsEditCommand.Toggle;
                return true;
            case "on":
            case "enable":
            case "enabled":
            case "true":
            case "1":
                command = ImmersiveStatsEditCommand.Set(true);
                return true;
            case "off":
            case "disable":
            case "disabled":
            case "false":
            case "0":
                command = ImmersiveStatsEditCommand.Set(false);
                return true;
            case "set":
                error = "The old set command was removed. Enable debug mode from the HUD settings menu and use the debug sliders instead.";
                return false;
            default:
                error = ExpectedUsageMessage(tokens[0]);
                return false;
        }
    }

    private static string ExpectedUsageMessage(string token)
    {
        return token.Equals("set", StringComparison.OrdinalIgnoreCase)
            ? "The old set command was removed. Enable debug mode from the HUD settings menu and use the debug sliders instead."
            : "Expected no argument, on, off, enable, disable, or toggle.";
    }
}
