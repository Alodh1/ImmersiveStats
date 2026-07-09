using Vintagestory.API.Datastructures;

namespace ImmersiveStats.Stats;

internal static class ImmersiveStatsHungerCapacity
{
    private const float Epsilon = 0.001f;

    public static bool EnsureTargetCapacity(ITreeAttribute? hunger, Action markDirty)
    {
        if (hunger is null)
        {
            return false;
        }

        float maxSaturation = hunger.TryGetFloat("maxsaturation") ?? float.NaN;
        if (Math.Abs(maxSaturation - ImmersiveStatsVitalsMapper.TargetSaturationCapacity) <= Epsilon)
        {
            return false;
        }

        hunger.SetFloat("maxsaturation", ImmersiveStatsVitalsMapper.TargetSaturationCapacity);
        markDirty();
        return true;
    }
}
