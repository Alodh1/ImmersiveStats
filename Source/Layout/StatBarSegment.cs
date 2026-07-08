namespace ImmersiveStats;

public sealed record StatBarSegment(
    StatBarSegmentKind Kind,
    float RequestedAmount,
    float RenderedAmount,
    float StartFraction,
    float EndFraction,
    string ColorKey);
