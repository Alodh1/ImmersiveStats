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
    public float DamageReducer { get; set; }

    [ProtoMember(7)]
    public float ColdReducer { get; set; }

    [ProtoMember(8)]
    public float HeatReducer { get; set; }

    [ProtoMember(9)]
    public float HungerReducer { get; set; }

    internal static ImmersiveStatsVitalsPacket FromSnapshot(ImmersiveStatsVitalsSnapshot snapshot)
    {
        return new ImmersiveStatsVitalsPacket
        {
            CurrentHealth = snapshot.CurrentHealth,
            MaxHealth = snapshot.MaxHealth,
            CurrentSaturation = snapshot.CurrentSaturation,
            MaxSaturation = snapshot.MaxSaturation,
            Capacity = snapshot.Capacity,
            DamageReducer = snapshot.DamageReducer,
            ColdReducer = snapshot.ColdReducer,
            HeatReducer = snapshot.HeatReducer,
            HungerReducer = snapshot.HungerReducer,
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
            DamageReducer,
            ColdReducer,
            HeatReducer,
            HungerReducer);
    }
}
