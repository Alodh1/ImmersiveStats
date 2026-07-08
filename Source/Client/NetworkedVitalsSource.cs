using ImmersiveStats.Network;
using ImmersiveStats.Stats;

namespace ImmersiveStats.Client;

internal sealed class NetworkedVitalsSource : IImmersiveStatsVitalsSource
{
    private readonly IImmersiveStatsVitalsSource _fallback;
    private ImmersiveStatsVitalsSnapshot? _serverSnapshot;

    public NetworkedVitalsSource(IImmersiveStatsVitalsSource fallback)
    {
        _fallback = fallback;
    }

    public void Apply(ImmersiveStatsVitalsPacket packet)
    {
        _serverSnapshot = packet.ToSnapshot();
    }

    public bool TryRead(out ImmersiveStatsVitalsSnapshot snapshot)
    {
        if (_serverSnapshot.HasValue)
        {
            snapshot = _serverSnapshot.Value;
            return true;
        }

        return _fallback.TryRead(out snapshot);
    }
}
