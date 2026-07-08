namespace ImmersiveStats.Stats;

internal readonly record struct ImmersiveStatsVitalsSnapshot(
    float CurrentHealth,
    float MaxHealth,
    float CurrentSaturation,
    float MaxSaturation,
    float Capacity,
    float DamageReducer,
    float HungerReducer);
