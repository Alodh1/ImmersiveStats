using System.Globalization;
using System.Text;
using ImmersiveStats.Client;
using ImmersiveStats.Debug;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace ImmersiveStats;

public sealed class ImmersiveStatsModSystem : ModSystem
{
    private readonly DebugStatBarState _debugState = new();
    private ImmersiveStatsHud? _hud;

    public override void StartClientSide(ICoreClientAPI api)
    {
        _hud = new ImmersiveStatsHud(api, _debugState);
        _hud.TryOpen(false);
        RegisterCommands(api);
    }

    public override void Dispose()
    {
        _hud?.Dispose();
        _hud = null;
        base.Dispose();
    }

    private void RegisterCommands(ICoreClientAPI api)
    {
        var parsers = api.ChatCommands.Parsers;

        api.ChatCommands.Create("immersivestats")
            .WithDescription("Immersive Stats prototype HUD controls.")
            .HandleWith(_ => TextCommandResult.Success(BuildStatusMessage(_debugState.Current)))
            .BeginSubCommand("set")
                .WithDescription("Set debug reducer values. Example: /immersivestats set damage 20 cold 5 heat 0 hunger 15")
                .WithArgs(parsers.OptionalAll("name value pairs"))
                .HandleWith(OnSetCommand)
            .EndSubCommand();
    }

    private TextCommandResult OnSetCommand(TextCommandCallingArgs args)
    {
        string raw = args.Parsers?.Count > 0
            ? args.Parsers[0].GetValue()?.ToString() ?? string.Empty
            : string.Empty;

        if (string.IsNullOrWhiteSpace(raw))
        {
            return TextCommandResult.Success(BuildStatusMessage(_debugState.Current));
        }

        if (!TryApplyPairs(_debugState.Current, raw, out StatBarState next, out string error))
        {
            return TextCommandResult.Error(error);
        }

        _debugState.Current = next;
        return TextCommandResult.Success(BuildStatusMessage(next));
    }

    private static bool TryApplyPairs(StatBarState current, string raw, out StatBarState next, out string error)
    {
        next = current;
        error = string.Empty;

        string[] tokens = raw.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length % 2 != 0)
        {
            error = "Expected name/value pairs, e.g. damage 20 cold 5 heat 0 hunger 15.";
            return false;
        }

        float damage = current.Damage;
        float cold = current.Cold;
        float heat = current.Heat;
        float hunger = current.Hunger;

        for (int i = 0; i < tokens.Length; i += 2)
        {
            string name = tokens[i].ToLowerInvariant();
            string rawValue = tokens[i + 1];

            if (!float.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out float value) ||
                float.IsNaN(value) ||
                float.IsInfinity(value) ||
                value < 0)
            {
                error = $"Value for '{tokens[i]}' must be a non-negative number.";
                return false;
            }

            switch (name)
            {
                case "damage":
                case "dmg":
                    damage = value;
                    break;
                case "cold":
                    cold = value;
                    break;
                case "heat":
                    heat = value;
                    break;
                case "hunger":
                case "hun":
                    hunger = value;
                    break;
                default:
                    error = $"Unknown reducer '{tokens[i]}'. Expected damage, cold, heat, or hunger.";
                    return false;
            }
        }

        next = current with
        {
            Damage = damage,
            Cold = cold,
            Heat = heat,
            Hunger = hunger,
        };
        return true;
    }

    private static string BuildStatusMessage(StatBarState state)
    {
        StatBarLayoutResult layout = StatBarLayout.Calculate(state);
        var builder = new StringBuilder();
        builder.Append(CultureInfo.InvariantCulture, $"Energy {layout.EnergyAmount:0.##}/{layout.Capacity:0.##}");

        foreach (StatBarSegment segment in layout.Segments.Where(segment => segment.Kind != StatBarSegmentKind.Energy))
        {
            builder.Append(CultureInfo.InvariantCulture, $", {segment.Kind.ToString().ToLowerInvariant()} {segment.RenderedAmount:0.##}/{segment.RequestedAmount:0.##}");
        }

        return builder.ToString();
    }
}
