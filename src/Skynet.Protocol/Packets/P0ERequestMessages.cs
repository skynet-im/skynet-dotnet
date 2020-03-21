using Skynet.Network;
using Skynet.Protocol.Attributes;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x0E, PacketPolicies.ClientToServer)]
    public sealed class P0ERequestMessages : Packet
    {
        public long ChannelId { get; set; }
        public long After { get; set; }
        public long Before { get; set; }
        public ushort MaxCount { get; set; }

        public override Packet Create() => new P0ERequestMessages().Init(this);

        protected override void ReadPacketInternal(PacketBuffer buffer, PacketRole role)
        {
            ChannelId = buffer.ReadInt64();
            After = buffer.ReadInt64();
            Before = buffer.ReadInt64();
            MaxCount = buffer.ReadUInt16();
        }

        protected override void WritePacketInternal(PacketBuffer buffer, PacketRole role)
        {
            buffer.WriteInt64(ChannelId);
            buffer.WriteInt64(After);
            buffer.WriteInt64(Before);
            buffer.WriteUInt16(MaxCount);
        }
    }
}
