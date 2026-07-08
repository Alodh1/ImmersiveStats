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
    public float PoisonReducer { get; set; }

    [ProtoMember(10)]
    public float FallReducer { get; set; }

    [ProtoMember(11)]
    public float SuffocationReducer { get; set; }

    [ProtoMember(12)]
    public float CrushingReducer { get; set; }

    [ProtoMember(13)]
    public float ElectricityReducer { get; set; }

    [ProtoMember(14)]
    public float AcidReducer { get; set; }

    [ProtoMember(15)]
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
            PoisonReducer = snapshot.PoisonReducer,
            FallReducer = snapshot.FallReducer,
            SuffocationReducer = snapshot.SuffocationReducer,
            CrushingReducer = snapshot.CrushingReducer,
            ElectricityReducer = snapshot.ElectricityReducer,
            AcidReducer = snapshot.AcidReducer,
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
            PoisonReducer,
            FallReducer,
            SuffocationReducer,
            CrushingReducer,
            ElectricityReducer,
            AcidReducer,
            HungerReducer);
    }
}
