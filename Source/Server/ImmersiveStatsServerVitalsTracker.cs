using ImmersiveStats.Network;
using ImmersiveStats.Stats;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;

namespace ImmersiveStats.Server;

internal sealed class ImmersiveStatsServerVitalsTracker : IDisposable
{
    private readonly ICoreServerAPI _api;
    private readonly IServerNetworkChannel _channel;

    public ImmersiveStatsServerVitalsTracker(ICoreServerAPI api, IServerNetworkChannel channel)
    {
        _api = api;
        _channel = channel;
    }

    public void Start()
    {
        _api.Event.PlayerNowPlaying += OnPlayerNowPlaying;
        _api.Event.PlayerRespawn += OnPlayerRespawn;
    }

    public void Dispose()
    {
        _api.Event.PlayerNowPlaying -= OnPlayerNowPlaying;
        _api.Event.PlayerRespawn -= OnPlayerRespawn;
    }

    public void Send(Entity entity, ImmersiveStatsVitalsSnapshot snapshot)
    {
        if (entity is not EntityPlayer { Player: IServerPlayer player })
        {
            return;
        }

        _channel.SendPacket(ImmersiveStatsVitalsPacket.FromSnapshot(snapshot), player);
    }

    private void OnPlayerNowPlaying(IServerPlayer player)
    {
        Attach(player);
    }

    private void OnPlayerRespawn(IServerPlayer player)
    {
        Attach(player);
    }

    private void Attach(IServerPlayer player)
    {
        EntityPlayer? entity = player.Entity;
        if (entity is null)
        {
            return;
        }

        ImmersiveStatsDamageTrackerBehavior? behavior = entity.GetBehavior<ImmersiveStatsDamageTrackerBehavior>();
        if (behavior is null)
        {
            behavior = new ImmersiveStatsDamageTrackerBehavior(entity, this);
            entity.AddBehavior(behavior);
        }

        behavior.SyncNow();
    }
}
