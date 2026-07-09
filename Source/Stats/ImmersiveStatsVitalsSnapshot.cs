namespace ImmersiveStats.Stats;

internal readonly record struct ImmersiveStatsVitalsSnapshot(
    float CurrentHealth,
    float MaxHealth,
    float CurrentSaturation,
    float MaxSaturation,
    float Capacity,
    float PenetratingTraumaReducer,
    float BluntTraumaReducer,
    float BurnReducer,
    float CoreTemperatureReducer,
    float ToxicReducer,
    float AsphyxiationReducer,
    float HungerReducer,
    bool PenetratingTraumaActive,
    bool BluntTraumaActive,
    bool BurnActive,
    bool CoreTemperatureActive);
