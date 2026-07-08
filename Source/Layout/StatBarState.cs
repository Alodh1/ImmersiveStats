namespace ImmersiveStats;

public sealed record StatBarState(
    float Capacity,
    float Damage,
    float Cold,
    float Heat,
    float Hunger)
{
    public static StatBarState Empty { get; } = new(StatBarLayout.DefaultCapacity, 0, 0, 0, 0);
}
