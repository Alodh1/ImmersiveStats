using ImmersiveStats.Stats;
using Vintagestory.API.Client;
using Vintagestory.API.Datastructures;

namespace ImmersiveStats.Client;

internal sealed class WatchedAttributeVitalsSource : IImmersiveStatsVitalsSource
{
    private readonly ICoreClientAPI _capi;

    public WatchedAttributeVitalsSource(ICoreClientAPI capi)
    {
        _capi = capi;
    }

    public bool TryRead(out ImmersiveStatsVitalsSnapshot snapshot)
    {
        snapshot = default;

        ITreeAttribute? watchedAttributes = _capi.World.Player?.Entity?.WatchedAttributes;
        if (watchedAttributes is null)
        {
            return false;
        }

        ITreeAttribute? health = watchedAttributes.GetTreeAttribute("health");
        ITreeAttribute? hunger = watchedAttributes.GetTreeAttribute("hunger");
        snapshot = ImmersiveStatsVitalsMapper.CreateSnapshot(
            ReadFloat(health, "currenthealth"),
            ReadFloat(health, "maxhealth"),
            ReadFloat(hunger, "currentsaturation"),
            ReadFloat(hunger, "maxsaturation"));
        return true;
    }

    private static float ReadFloat(ITreeAttribute? tree, string key)
    {
        return tree?.TryGetFloat(key) ?? float.NaN;
    }
}
