using ProtoBuf;

namespace ImmersiveStats.Network;

[ProtoContract]
public sealed class ImmersiveStatsEditModePacket
{
    [ProtoMember(1)]
    public bool HasExplicitState { get; set; }

    [ProtoMember(2)]
    public bool Enabled { get; set; }
}
