namespace ImmersiveStats.Stats;

internal interface IImmersiveStatsVitalsSource
{
    bool TryRead(out ImmersiveStatsVitalsSnapshot snapshot);
}
