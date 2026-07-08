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
            PoisonReducer: 0,
            FallReducer: 0,
            SuffocationReducer: 0,
            CrushingReducer: 0,
            ElectricityReducer: 0,
            AcidReducer: 0,
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
        float poisonHealthPoints = 0,
        float fallHealthPoints = 0,
        float suffocationHealthPoints = 0,
        float crushingHealthPoints = 0,
        float electricityHealthPoints = 0,
        float acidHealthPoints = 0,
        float hungerHealthPoints = 0,
        float capacity = StatBarLayout.DefaultCapacity)
    {
        return CreateCategorizedSnapshot(
            currentHealth,
            maxHealth,
            currentSaturation,
            maxSaturation,
            new Dictionary<StatBarSegmentKind, float>
            {
                [StatBarSegmentKind.Damage] = damageHealthPoints,
                [StatBarSegmentKind.Cold] = coldHealthPoints,
                [StatBarSegmentKind.Heat] = heatHealthPoints,
                [StatBarSegmentKind.Poison] = poisonHealthPoints,
                [StatBarSegmentKind.Fall] = fallHealthPoints,
                [StatBarSegmentKind.Suffocation] = suffocationHealthPoints,
                [StatBarSegmentKind.Crushing] = crushingHealthPoints,
                [StatBarSegmentKind.Electricity] = electricityHealthPoints,
                [StatBarSegmentKind.Acid] = acidHealthPoints,
                [StatBarSegmentKind.Hunger] = hungerHealthPoints,
            },
            capacity);
    }

    public static ImmersiveStatsVitalsSnapshot CreateCategorizedSnapshot(
        float currentHealth,
        float maxHealth,
        float currentSaturation,
        float maxSaturation,
        IReadOnlyDictionary<StatBarSegmentKind, float> healthPointReducers,
        float capacity = StatBarLayout.DefaultCapacity)
    {
        float normalizedCapacity = NormalizeCapacity(capacity);
        float healthMax = IsFinite(maxHealth) && maxHealth > 0 ? maxHealth : 0;
        float saturationHunger = CalculateMissingPercentReducer(currentSaturation, maxSaturation, normalizedCapacity);
        float healthHunger = CalculateHealthPointReducer(GetReducer(healthPointReducers, StatBarSegmentKind.Hunger), healthMax, normalizedCapacity);

        return new ImmersiveStatsVitalsSnapshot(
            currentHealth,
            maxHealth,
            currentSaturation,
            maxSaturation,
            normalizedCapacity,
            CalculateHealthPointReducer(GetReducer(healthPointReducers, StatBarSegmentKind.Damage), healthMax, normalizedCapacity),
            CalculateHealthPointReducer(GetReducer(healthPointReducers, StatBarSegmentKind.Cold), healthMax, normalizedCapacity),
            CalculateHealthPointReducer(GetReducer(healthPointReducers, StatBarSegmentKind.Heat), healthMax, normalizedCapacity),
            CalculateHealthPointReducer(GetReducer(healthPointReducers, StatBarSegmentKind.Poison), healthMax, normalizedCapacity),
            CalculateHealthPointReducer(GetReducer(healthPointReducers, StatBarSegmentKind.Fall), healthMax, normalizedCapacity),
            CalculateHealthPointReducer(GetReducer(healthPointReducers, StatBarSegmentKind.Suffocation), healthMax, normalizedCapacity),
            CalculateHealthPointReducer(GetReducer(healthPointReducers, StatBarSegmentKind.Crushing), healthMax, normalizedCapacity),
            CalculateHealthPointReducer(GetReducer(healthPointReducers, StatBarSegmentKind.Electricity), healthMax, normalizedCapacity),
            CalculateHealthPointReducer(GetReducer(healthPointReducers, StatBarSegmentKind.Acid), healthMax, normalizedCapacity),
            saturationHunger + healthHunger);
    }

    public static StatBarState ToStatBarState(ImmersiveStatsVitalsSnapshot snapshot)
    {
        float capacity = NormalizeCapacity(snapshot.Capacity);
        return new StatBarState(capacity, new Dictionary<StatBarSegmentKind, float>
        {
            [StatBarSegmentKind.Damage] = SanitizeReducer(snapshot.DamageReducer),
            [StatBarSegmentKind.Cold] = SanitizeReducer(snapshot.ColdReducer),
            [StatBarSegmentKind.Heat] = SanitizeReducer(snapshot.HeatReducer),
            [StatBarSegmentKind.Poison] = SanitizeReducer(snapshot.PoisonReducer),
            [StatBarSegmentKind.Fall] = SanitizeReducer(snapshot.FallReducer),
            [StatBarSegmentKind.Suffocation] = SanitizeReducer(snapshot.SuffocationReducer),
            [StatBarSegmentKind.Crushing] = SanitizeReducer(snapshot.CrushingReducer),
            [StatBarSegmentKind.Electricity] = SanitizeReducer(snapshot.ElectricityReducer),
            [StatBarSegmentKind.Acid] = SanitizeReducer(snapshot.AcidReducer),
            [StatBarSegmentKind.Hunger] = SanitizeReducer(snapshot.HungerReducer),
        });
    }

    private static float GetReducer(IReadOnlyDictionary<StatBarSegmentKind, float> reducers, StatBarSegmentKind kind)
    {
        return reducers.TryGetValue(kind, out float amount) ? amount : 0;
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
