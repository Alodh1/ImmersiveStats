namespace ImmersiveStats.Stats;

internal readonly record struct ImmersiveStatsVitalsSnapshot(
    float CurrentHealth,
    float MaxHealth,
    float CurrentSaturation,
    float MaxSaturation,
    float Capacity,
    float DamageReducer,
    float ColdReducer,
    float HeatReducer,
    float PoisonReducer,
    float FallReducer,
    float SuffocationReducer,
    float CrushingReducer,
    float ElectricityReducer,
    float AcidReducer,
    float HungerReducer);
