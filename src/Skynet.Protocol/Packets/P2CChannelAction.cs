using Skynet.Network;
using Skynet.Protocol.Attributes;
using Skynet.Protocol.Model;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x2C, PacketPolicies.ServerToClient)]
    public sealed class P2CChannelAction : Packet
    {
        public long ChannelId { get; set; }
        public long AccountId { get; set; }
        public ChannelAction Action { get; set; }

        public override Packet Create() => new P2CChannelAction().Init(this);

        protected override void ReadPacketInternal(PacketBuffer buffer, PacketRole role)
        {
            ChannelId = buffer.ReadInt64();
            AccountId = buffer.ReadInt64();
            Action = (ChannelAction)buffer.ReadByte();
        }

        protected override void WritePacketInternal(PacketBuffer buffer, PacketRole role)
        {
            buffer.WriteInt64(ChannelId);
            buffer.WriteInt64(AccountId);
            buffer.WriteByte((byte)Action);
        }
    }
}
