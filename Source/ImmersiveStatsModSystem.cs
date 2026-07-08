using ImmersiveStats.Client;
using ImmersiveStats.Commands;
using ImmersiveStats.Debug;
using ImmersiveStats.Network;
using ImmersiveStats.Server;
using ImmersiveStats.Stats;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace ImmersiveStats;

public sealed class ImmersiveStatsModSystem : ModSystem
{
    private const string NetworkChannelName = "immersivestats";

    private readonly DebugStatBarState _debugState = new();
    private ImmersiveStatsClientConfig? _clientConfig;
    private ImmersiveStatsHud? _hud;
    private NetworkedVitalsSource? _clientVitalsSource;
    private ImmersiveStatsServerVitalsTracker? _serverVitalsTracker;
    private IClientNetworkChannel? _clientChannel;
    private IServerNetworkChannel? _serverChannel;

    public override void StartClientSide(ICoreClientAPI api)
    {
        _clientConfig = LoadClientConfig(api);
        _debugState.Current = _clientConfig.DebugModeEnabled ? _clientConfig.ToState() : StatBarState.Empty;
        _clientVitalsSource = new NetworkedVitalsSource(new WatchedAttributeVitalsSource(api));

        _clientChannel = api.Network
            .RegisterChannel(NetworkChannelName)
            .RegisterMessageType<ImmersiveStatsEditModePacket>()
            .RegisterMessageType<ImmersiveStatsVitalsPacket>();
        _clientChannel.SetMessageHandler<ImmersiveStatsEditModePacket>(OnEditModePacket);
        _clientChannel.SetMessageHandler<ImmersiveStatsVitalsPacket>(OnVitalsPacket);

        _hud = new ImmersiveStatsHud(api, _debugState, _clientConfig, _clientVitalsSource, new VanillaStatbarSuppressor(api), () => SaveClientConfig(api));
        _hud.TryOpen(false);
        RegisterClientFallbackCommand(api);
    }

    public override void StartServerSide(ICoreServerAPI api)
    {
        _serverChannel = api.Network
            .RegisterChannel(NetworkChannelName)
            .RegisterMessageType<ImmersiveStatsEditModePacket>()
            .RegisterMessageType<ImmersiveStatsVitalsPacket>();
        _serverVitalsTracker = new ImmersiveStatsServerVitalsTracker(api, _serverChannel);
        _serverVitalsTracker.Start();

        RegisterServerCommand(api);
    }

    public override void Dispose()
    {
        _serverVitalsTracker?.Dispose();
        _hud?.Dispose();
        _hud = null;
        _clientVitalsSource = null;
        _serverVitalsTracker = null;
        _clientChannel = null;
        _serverChannel = null;
        _clientConfig = null;
        base.Dispose();
    }

    private static ImmersiveStatsClientConfig LoadClientConfig(ICoreClientAPI api)
    {
        ImmersiveStatsClientConfig config = api.LoadModConfig<ImmersiveStatsClientConfig>(ImmersiveStatsClientConfig.FileName)
            ?? new ImmersiveStatsClientConfig();
        config.Normalize(GetViewportWidth(api), GetViewportHeight(api));
        api.StoreModConfig(config, ImmersiveStatsClientConfig.FileName);
        return config;
    }

    private void SaveClientConfig(ICoreClientAPI api)
    {
        if (_clientConfig is null)
        {
            return;
        }

        _clientConfig.Normalize(GetViewportWidth(api), GetViewportHeight(api));
        api.StoreModConfig(_clientConfig, ImmersiveStatsClientConfig.FileName);
    }

    private void RegisterClientFallbackCommand(ICoreClientAPI api)
    {
        api.ChatCommands.GetOrCreate("immersivestats")
            .WithDescription("Toggle Immersive Stats HUD edit mode. Use /immersivestats on servers that load the mod server-side.")
            .RequiresPrivilege(Privilege.chat)
            .WithArgs(api.ChatCommands.Parsers.OptionalAll("on|off"))
            .HandleWith(HandleClientCommand);
    }

    private void RegisterServerCommand(ICoreServerAPI api)
    {
        api.ChatCommands.GetOrCreate("immersivestats")
            .WithDescription("Toggle Immersive Stats HUD edit mode for your client.")
            .RequiresPlayer()
            .RequiresPrivilege(Privilege.chat)
            .WithArgs(api.ChatCommands.Parsers.OptionalAll("on|off"))
            .HandleWith(HandleServerCommand);
    }

    private TextCommandResult HandleClientCommand(TextCommandCallingArgs args)
    {
        if (!ImmersiveStatsCommandParser.TryParse(GetRawCommandArgument(args), out ImmersiveStatsEditCommand command, out string error))
        {
            return TextCommandResult.Error(error);
        }

        if (_hud is null)
        {
            return TextCommandResult.Error("Immersive Stats HUD is not available.");
        }

        bool enabled = _hud.ApplyEditModeCommand(command);
        return TextCommandResult.Success($"Immersive Stats edit mode {(enabled ? "enabled" : "disabled")}.");
    }

    private TextCommandResult HandleServerCommand(TextCommandCallingArgs args)
    {
        if (!ImmersiveStatsCommandParser.TryParse(GetRawCommandArgument(args), out ImmersiveStatsEditCommand command, out string error))
        {
            return TextCommandResult.Error(error);
        }

        if (args.Caller.Player is not IServerPlayer player)
        {
            return TextCommandResult.Error("Immersive Stats edit mode can only be toggled by a player.");
        }

        _serverChannel?.SendPacket(ToPacket(command), player);
        return TextCommandResult.Success(DescribeServerRequest(command));
    }

    private void OnEditModePacket(ImmersiveStatsEditModePacket packet)
    {
        _hud?.ApplyEditModePacket(packet);
    }

    private void OnVitalsPacket(ImmersiveStatsVitalsPacket packet)
    {
        _clientVitalsSource?.Apply(packet);
    }

    private static string GetRawCommandArgument(TextCommandCallingArgs args)
    {
        return args.Parsers.Count > 0
            ? args[0]?.ToString() ?? string.Empty
            : string.Empty;
    }

    private static ImmersiveStatsEditModePacket ToPacket(ImmersiveStatsEditCommand command)
    {
        return new ImmersiveStatsEditModePacket
        {
            HasExplicitState = command.Kind == ImmersiveStatsEditCommandKind.Set,
            Enabled = command.Enabled,
        };
    }

    private static string DescribeServerRequest(ImmersiveStatsEditCommand command)
    {
        return command.Kind == ImmersiveStatsEditCommandKind.Toggle
            ? "Immersive Stats edit mode toggle sent."
            : $"Immersive Stats edit mode {(command.Enabled ? "enable" : "disable")} sent.";
    }

    private static double GetViewportWidth(ICoreClientAPI api)
    {
        return api.Render.FrameWidth > 0 ? api.Render.FrameWidth / Vintagestory.API.Config.RuntimeEnv.GUIScale : 1280;
    }

    private static double GetViewportHeight(ICoreClientAPI api)
    {
        return api.Render.FrameHeight > 0 ? api.Render.FrameHeight / Vintagestory.API.Config.RuntimeEnv.GUIScale : 720;
    }
}
