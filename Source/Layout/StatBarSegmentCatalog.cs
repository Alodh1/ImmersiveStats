namespace ImmersiveStats;

public static class StatBarSegmentCatalog
{
    private static readonly StatBarSegmentKind[] ReducerKindsValue =
    [
        StatBarSegmentKind.PenetratingTrauma,
        StatBarSegmentKind.BluntTrauma,
        StatBarSegmentKind.Burn,
        StatBarSegmentKind.CoreTemperature,
        StatBarSegmentKind.Toxic,
        StatBarSegmentKind.Asphyxiation,
        StatBarSegmentKind.Hunger,
    ];

    public static IReadOnlyList<StatBarSegmentKind> ReducerKinds => ReducerKindsValue;

    public static string DisplayName(StatBarSegmentKind kind) => kind switch
    {
        StatBarSegmentKind.Energy => "Energy",
        StatBarSegmentKind.PenetratingTrauma => "Penetrating Trauma",
        StatBarSegmentKind.BluntTrauma => "Blunt Trauma",
        StatBarSegmentKind.Burn => "Burn",
        StatBarSegmentKind.CoreTemperature => "Core Temperature",
        StatBarSegmentKind.Toxic => "Toxic",
        StatBarSegmentKind.Asphyxiation => "Asphyxiation",
        StatBarSegmentKind.Hunger => "Hunger",
        _ => kind.ToString(),
    };

    public static bool IsReducer(StatBarSegmentKind kind)
    {
        return ReducerKindsValue.Contains(kind);
    }
}
