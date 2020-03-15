using Skynet.Protocol.Attributes;
using Skynet.Protocol.Model;
using Skynet.Network;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x2C, PacketPolicies.Send)]
    internal class P2CChannelAction : Packet
    {
        public long ChannelId { get; set; }
        public long AccountId { get; set; }
        public ChannelAction Action { get; set; }

        public override Packet Create() => new P2CChannelAction().Init(this);

        public override void WritePacket(PacketBuffer buffer)
        {
            buffer.WriteInt64(ChannelId);
            buffer.WriteInt64(AccountId);
            buffer.WriteByte((byte)Action);
        }
    }
}
