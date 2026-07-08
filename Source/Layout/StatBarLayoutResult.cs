namespace ImmersiveStats;

public sealed record StatBarLayoutResult(
    float Capacity,
    float EnergyAmount,
    IReadOnlyList<StatBarSegment> Segments);
