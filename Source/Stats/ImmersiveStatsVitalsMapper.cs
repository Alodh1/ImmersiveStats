namespace ImmersiveStats.Stats;

internal static class ImmersiveStatsVitalsMapper
{
    public static ImmersiveStatsVitalsSnapshot CreateSnapshot(
        float currentHealth,
        float maxHealth,
        float currentSaturation,
        float maxSaturation,
        float capacity = StatBarLayout.DefaultCapacity)
    {
        float normalizedCapacity = NormalizeCapacity(capacity);
        return new ImmersiveStatsVitalsSnapshot(
            currentHealth,
            maxHealth,
            currentSaturation,
            maxSaturation,
            normalizedCapacity,
            CalculateMissingPercentReducer(currentHealth, maxHealth, normalizedCapacity),
            CalculateMissingPercentReducer(currentSaturation, maxSaturation, normalizedCapacity));
    }

    public static StatBarState ToStatBarState(ImmersiveStatsVitalsSnapshot snapshot)
    {
        float capacity = NormalizeCapacity(snapshot.Capacity);
        return new StatBarState(
            capacity,
            SanitizeReducer(snapshot.DamageReducer),
            Cold: 0,
            Heat: 0,
            SanitizeReducer(snapshot.HungerReducer));
    }

    private static float CalculateMissingPercentReducer(float current, float max, float capacity)
    {
        if (!IsFinite(max) || max <= 0 || !IsFinite(current))
        {
            return 0;
        }

        float clampedCurrent = Math.Min(max, Math.Max(0, current));
        return (1 - clampedCurrent / max) * capacity;
    }

    private static float NormalizeCapacity(float capacity)
    {
        return IsFinite(capacity) && capacity > 0 ? capacity : StatBarLayout.DefaultCapacity;
    }

    private static float SanitizeReducer(float reducer)
    {
        return IsFinite(reducer) ? Math.Max(0, reducer) : 0;
    }

    private static bool IsFinite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
