using ImmersiveStats.Stats;

namespace ImmersiveStats.Client;

internal static class ImmersiveStatsDisplayStateResolver
{
    public static StatBarState Resolve(
        ImmersiveStatsClientConfig config,
        IImmersiveStatsVitalsSource vitalsSource,
        StatBarState fallback)
    {
        if (config.DebugModeEnabled)
        {
            return config.ToState();
        }

        return vitalsSource.TryRead(out ImmersiveStatsVitalsSnapshot snapshot)
            ? ImmersiveStatsVitalsMapper.ToStatBarState(snapshot)
            : fallback;
    }
}
