namespace ImmersiveStats;

public static class StatBarSegmentCatalog
{
    private static readonly StatBarSegmentKind[] ReducerKindsValue =
    [
        StatBarSegmentKind.Damage,
        StatBarSegmentKind.Cold,
        StatBarSegmentKind.Heat,
        StatBarSegmentKind.Poison,
        StatBarSegmentKind.Fall,
        StatBarSegmentKind.Suffocation,
        StatBarSegmentKind.Crushing,
        StatBarSegmentKind.Electricity,
        StatBarSegmentKind.Acid,
        StatBarSegmentKind.Hunger,
    ];

    public static IReadOnlyList<StatBarSegmentKind> ReducerKinds => ReducerKindsValue;

    public static string DisplayName(StatBarSegmentKind kind) => kind switch
    {
        StatBarSegmentKind.Energy => "Energy",
        StatBarSegmentKind.Damage => "Injury",
        StatBarSegmentKind.Cold => "Cold",
        StatBarSegmentKind.Heat => "Heat",
        StatBarSegmentKind.Poison => "Poison",
        StatBarSegmentKind.Fall => "Fall",
        StatBarSegmentKind.Suffocation => "Suffocation",
        StatBarSegmentKind.Crushing => "Crushing",
        StatBarSegmentKind.Electricity => "Electricity",
        StatBarSegmentKind.Acid => "Acid",
        StatBarSegmentKind.Hunger => "Hunger",
        _ => kind.ToString(),
    };

    public static bool IsReducer(StatBarSegmentKind kind)
    {
        return ReducerKindsValue.Contains(kind);
    }
}
