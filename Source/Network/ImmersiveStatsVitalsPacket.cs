using ImmersiveStats.Stats;
using ProtoBuf;

namespace ImmersiveStats.Network;

[ProtoContract]
public sealed class ImmersiveStatsVitalsPacket
{
    [ProtoMember(1)]
    public float CurrentHealth { get; set; }

    [ProtoMember(2)]
    public float MaxHealth { get; set; }

    [ProtoMember(3)]
    public float CurrentSaturation { get; set; }

    [ProtoMember(4)]
    public float MaxSaturation { get; set; }

    [ProtoMember(5)]
    public float Capacity { get; set; }

    [ProtoMember(6)]
    public float PenetratingTraumaReducer { get; set; }

    [ProtoMember(7)]
    public float BluntTraumaReducer { get; set; }

    [ProtoMember(8)]
    public float BurnReducer { get; set; }

    [ProtoMember(9)]
    public float CoreTemperatureReducer { get; set; }

    [ProtoMember(10)]
    public float ToxicReducer { get; set; }

    [ProtoMember(11)]
    public float AsphyxiationReducer { get; set; }

    [ProtoMember(12)]
    public float HungerReducer { get; set; }

    [ProtoMember(13)]
    public bool PenetratingTraumaActive { get; set; }

    [ProtoMember(14)]
    public bool BluntTraumaActive { get; set; }

    [ProtoMember(15)]
    public bool BurnActive { get; set; }

    [ProtoMember(16)]
    public bool CoreTemperatureActive { get; set; }

    internal static ImmersiveStatsVitalsPacket FromSnapshot(ImmersiveStatsVitalsSnapshot snapshot)
    {
        return new ImmersiveStatsVitalsPacket
        {
            CurrentHealth = snapshot.CurrentHealth,
            MaxHealth = snapshot.MaxHealth,
            CurrentSaturation = snapshot.CurrentSaturation,
            MaxSaturation = snapshot.MaxSaturation,
            Capacity = snapshot.Capacity,
            PenetratingTraumaReducer = snapshot.PenetratingTraumaReducer,
            BluntTraumaReducer = snapshot.BluntTraumaReducer,
            BurnReducer = snapshot.BurnReducer,
            CoreTemperatureReducer = snapshot.CoreTemperatureReducer,
            ToxicReducer = snapshot.ToxicReducer,
            AsphyxiationReducer = snapshot.AsphyxiationReducer,
            HungerReducer = snapshot.HungerReducer,
            PenetratingTraumaActive = snapshot.PenetratingTraumaActive,
            BluntTraumaActive = snapshot.BluntTraumaActive,
            BurnActive = snapshot.BurnActive,
            CoreTemperatureActive = snapshot.CoreTemperatureActive,
        };
    }

    internal ImmersiveStatsVitalsSnapshot ToSnapshot()
    {
        return new ImmersiveStatsVitalsSnapshot(
            CurrentHealth,
            MaxHealth,
            CurrentSaturation,
            MaxSaturation,
            Capacity,
            PenetratingTraumaReducer,
            BluntTraumaReducer,
            BurnReducer,
            CoreTemperatureReducer,
            ToxicReducer,
            AsphyxiationReducer,
            HungerReducer,
            PenetratingTraumaActive,
            BluntTraumaActive,
            BurnActive,
            CoreTemperatureActive);
    }
}
