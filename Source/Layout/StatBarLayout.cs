namespace ImmersiveStats;

public static class StatBarLayout
{
    public const float DefaultCapacity = 5000f;
    private const float Epsilon = 0.0001f;

    public static StatBarLayoutResult Calculate(StatBarState state)
    {
        float capacity = SanitizeCapacity(state.Capacity);
        if (capacity <= Epsilon)
        {
            return new StatBarLayoutResult(0, 0, []);
        }

        Dictionary<StatBarSegmentKind, float> requested = StatBarSegmentCatalog.ReducerKinds
            .ToDictionary(kind => kind, kind => SanitizeAmount(state.GetReducer(kind)));

        Dictionary<StatBarSegmentKind, float> rendered = new();
        float remaining = capacity;

        foreach (StatBarSegmentKind kind in StatBarSegmentCatalog.ReducerKinds)
        {
            float amount = Math.Min(requested[kind], remaining);
            rendered[kind] = amount;
            remaining -= amount;
        }

        var segments = new List<StatBarSegment>();
        float cursor = 0;

        if (remaining > Epsilon)
        {
            float end = remaining / capacity;
            segments.Add(new StatBarSegment(
                StatBarSegmentKind.Energy,
                remaining,
                remaining,
                0,
                end,
                "energy",
                ActiveCondition: false));
            cursor = end;
        }

        foreach (StatBarSegmentKind kind in StatBarSegmentCatalog.ReducerKinds)
        {
            float amount = rendered[kind];
            if (amount <= Epsilon)
            {
                continue;
            }

            float width = amount / capacity;
            float end = Math.Min(1, cursor + width);
            segments.Add(new StatBarSegment(
                kind,
                requested[kind],
                amount,
                cursor,
                end,
                ColorKey(kind),
                state.IsConditionActive(kind)));
            cursor = end;
        }

        return new StatBarLayoutResult(capacity, remaining, segments);
    }

    private static float SanitizeCapacity(float value)
    {
        if (float.IsNaN(value) || float.IsInfinity(value))
        {
            return DefaultCapacity;
        }

        return Math.Max(0, value);
    }

    private static float SanitizeAmount(float value)
    {
        if (float.IsNaN(value) || float.IsInfinity(value))
        {
            return 0;
        }

        return Math.Max(0, value);
    }

    private static string ColorKey(StatBarSegmentKind kind) => kind switch
    {
        StatBarSegmentKind.Energy => "energy",
        StatBarSegmentKind.PenetratingTrauma => "penetrating",
        StatBarSegmentKind.BluntTrauma => "blunt",
        StatBarSegmentKind.Burn => "burn",
        StatBarSegmentKind.CoreTemperature => "coretemp",
        StatBarSegmentKind.Toxic => "toxic",
        StatBarSegmentKind.Asphyxiation => "asphyxiation",
        StatBarSegmentKind.Hunger => "hunger",
        _ => "unknown",
    };
}
