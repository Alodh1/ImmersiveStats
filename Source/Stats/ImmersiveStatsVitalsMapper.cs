namespace ImmersiveStats.Stats;

internal static class ImmersiveStatsVitalsMapper
{
    public const float EnergyPerHealthPoint = 250f;
    public const float TargetSaturationCapacity = StatBarLayout.DefaultCapacity;

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
            PenetratingTraumaReducer: 0,
            BluntTraumaReducer: CalculateMissingHealthEnergy(currentHealth, maxHealth),
            BurnReducer: 0,
            CoreTemperatureReducer: 0,
            ToxicReducer: 0,
            AsphyxiationReducer: 0,
            CalculateMissingSatietyEnergy(currentSaturation),
            PenetratingTraumaActive: false,
            BluntTraumaActive: false,
            BurnActive: false,
            CoreTemperatureActive: false);
    }

    public static ImmersiveStatsVitalsSnapshot CreateCategorizedSnapshot(
        float currentHealth,
        float maxHealth,
        float currentSaturation,
        float maxSaturation,
        IReadOnlyDictionary<StatBarSegmentKind, float> energyReducers,
        IEnumerable<StatBarSegmentKind>? activeConditions = null,
        float capacity = StatBarLayout.DefaultCapacity)
    {
        float normalizedCapacity = NormalizeCapacity(capacity);
        HashSet<StatBarSegmentKind> active = new(activeConditions ?? []);

        return new ImmersiveStatsVitalsSnapshot(
            currentHealth,
            maxHealth,
            currentSaturation,
            maxSaturation,
            normalizedCapacity,
            SanitizeReducer(GetReducer(energyReducers, StatBarSegmentKind.PenetratingTrauma)),
            SanitizeReducer(GetReducer(energyReducers, StatBarSegmentKind.BluntTrauma)),
            SanitizeReducer(GetReducer(energyReducers, StatBarSegmentKind.Burn)),
            SanitizeReducer(GetReducer(energyReducers, StatBarSegmentKind.CoreTemperature)),
            SanitizeReducer(GetReducer(energyReducers, StatBarSegmentKind.Toxic)),
            SanitizeReducer(GetReducer(energyReducers, StatBarSegmentKind.Asphyxiation)),
            SanitizeReducer(GetReducer(energyReducers, StatBarSegmentKind.Hunger) + CalculateMissingSatietyEnergy(currentSaturation)),
            active.Contains(StatBarSegmentKind.PenetratingTrauma),
            active.Contains(StatBarSegmentKind.BluntTrauma),
            active.Contains(StatBarSegmentKind.Burn),
            active.Contains(StatBarSegmentKind.CoreTemperature));
    }

    public static StatBarState ToStatBarState(ImmersiveStatsVitalsSnapshot snapshot)
    {
        float capacity = NormalizeCapacity(snapshot.Capacity);
        var activeConditions = new List<StatBarSegmentKind>();
        if (snapshot.PenetratingTraumaActive)
        {
            activeConditions.Add(StatBarSegmentKind.PenetratingTrauma);
        }

        if (snapshot.BluntTraumaActive)
        {
            activeConditions.Add(StatBarSegmentKind.BluntTrauma);
        }

        if (snapshot.BurnActive)
        {
            activeConditions.Add(StatBarSegmentKind.Burn);
        }

        if (snapshot.CoreTemperatureActive)
        {
            activeConditions.Add(StatBarSegmentKind.CoreTemperature);
        }

        return new StatBarState(capacity, new Dictionary<StatBarSegmentKind, float>
        {
            [StatBarSegmentKind.PenetratingTrauma] = SanitizeReducer(snapshot.PenetratingTraumaReducer),
            [StatBarSegmentKind.BluntTrauma] = SanitizeReducer(snapshot.BluntTraumaReducer),
            [StatBarSegmentKind.Burn] = SanitizeReducer(snapshot.BurnReducer),
            [StatBarSegmentKind.CoreTemperature] = SanitizeReducer(snapshot.CoreTemperatureReducer),
            [StatBarSegmentKind.Toxic] = SanitizeReducer(snapshot.ToxicReducer),
            [StatBarSegmentKind.Asphyxiation] = SanitizeReducer(snapshot.AsphyxiationReducer),
            [StatBarSegmentKind.Hunger] = SanitizeReducer(snapshot.HungerReducer),
        }, activeConditions);
    }

    public static float HealthDamageToEnergy(float healthDamage)
    {
        return IsFinite(healthDamage) && healthDamage > 0 ? healthDamage * EnergyPerHealthPoint : 0;
    }

    private static float GetReducer(IReadOnlyDictionary<StatBarSegmentKind, float> reducers, StatBarSegmentKind kind)
    {
        return reducers.TryGetValue(kind, out float amount) ? amount : 0;
    }

    private static float CalculateMissingHealthEnergy(float currentHealth, float maxHealth)
    {
        if (!IsFinite(maxHealth) || maxHealth <= 0 || !IsFinite(currentHealth))
        {
            return 0;
        }

        float clampedCurrent = Math.Min(maxHealth, Math.Max(0, currentHealth));
        return (maxHealth - clampedCurrent) * EnergyPerHealthPoint;
    }

    private static float CalculateMissingSatietyEnergy(float currentSaturation)
    {
        if (!IsFinite(currentSaturation))
        {
            return 0;
        }

        return Math.Max(0, TargetSaturationCapacity - Math.Max(0, currentSaturation));
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
