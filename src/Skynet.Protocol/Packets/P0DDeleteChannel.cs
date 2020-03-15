using Skynet.Protocol.Attributes;
using Skynet.Network;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x0D, PacketPolicies.Send)]
    internal sealed class P0DDeleteChannel : Packet
    {
        public long ChannelId { get; set; }

        public override Packet Create() => new P0DDeleteChannel().Init(this);

        public override void WritePacket(PacketBuffer buffer)
        {
            buffer.WriteInt64(ChannelId);
        }
    }
}
