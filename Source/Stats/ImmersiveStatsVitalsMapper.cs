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
            ColdReducer: 0,
            HeatReducer: 0,
            CalculateMissingPercentReducer(currentSaturation, maxSaturation, normalizedCapacity));
    }

    public static ImmersiveStatsVitalsSnapshot CreateCategorizedSnapshot(
        float currentHealth,
        float maxHealth,
        float currentSaturation,
        float maxSaturation,
        float damageHealthPoints,
        float coldHealthPoints,
        float heatHealthPoints,
        float hungerHealthPoints,
        float capacity = StatBarLayout.DefaultCapacity)
    {
        float normalizedCapacity = NormalizeCapacity(capacity);
        float healthMax = IsFinite(maxHealth) && maxHealth > 0 ? maxHealth : 0;
        float saturationHunger = CalculateMissingPercentReducer(currentSaturation, maxSaturation, normalizedCapacity);
        float healthHunger = CalculateHealthPointReducer(hungerHealthPoints, healthMax, normalizedCapacity);

        return new ImmersiveStatsVitalsSnapshot(
            currentHealth,
            maxHealth,
            currentSaturation,
            maxSaturation,
            normalizedCapacity,
            CalculateHealthPointReducer(damageHealthPoints, healthMax, normalizedCapacity),
            CalculateHealthPointReducer(coldHealthPoints, healthMax, normalizedCapacity),
            CalculateHealthPointReducer(heatHealthPoints, healthMax, normalizedCapacity),
            saturationHunger + healthHunger);
    }

    public static StatBarState ToStatBarState(ImmersiveStatsVitalsSnapshot snapshot)
    {
        float capacity = NormalizeCapacity(snapshot.Capacity);
        return new StatBarState(
            capacity,
            SanitizeReducer(snapshot.DamageReducer),
            SanitizeReducer(snapshot.ColdReducer),
            SanitizeReducer(snapshot.HeatReducer),
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

    private static float CalculateHealthPointReducer(float healthPoints, float maxHealth, float capacity)
    {
        if (maxHealth <= 0 || !IsFinite(healthPoints))
        {
            return 0;
        }

        return Math.Max(0, healthPoints) / maxHealth * capacity;
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
