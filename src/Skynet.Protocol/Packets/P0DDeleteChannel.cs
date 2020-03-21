using Skynet.Network;
using Skynet.Protocol.Attributes;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x0D, PacketPolicies.ServerToClient)]
    public sealed class P0DDeleteChannel : Packet
    {
        public long ChannelId { get; set; }

        public override Packet Create() => new P0DDeleteChannel().Init(this);

        protected override void ReadPacketInternal(PacketBuffer buffer, PacketRole role)
        {
            ChannelId = buffer.ReadInt64();
        }

        protected override void WritePacketInternal(PacketBuffer buffer, PacketRole role)
        {
            buffer.WriteInt64(ChannelId);
        }
    }
}
