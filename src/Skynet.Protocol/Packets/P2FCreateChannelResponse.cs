using Skynet.Protocol.Attributes;
using Skynet.Protocol.Model;
using Skynet.Network;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x2F, PacketPolicies.ServerToClient)]
    internal sealed class P2FCreateChannelResponse : Packet
    {
        public long TempChannelId { get; set; }
        public CreateChannelStatus StatusCode { get; set; }
        public long ChannelId { get; set; }
        public DateTime CreationTime { get; set; }

        public override Packet Create() => new P2FCreateChannelResponse().Init(this);

        protected override void ReadPacketInternal(PacketBuffer buffer, PacketRole role)
        {
            TempChannelId = buffer.ReadInt64();
            StatusCode = (CreateChannelStatus)buffer.ReadByte();
            ChannelId = buffer.ReadInt64();
            CreationTime = buffer.ReadDateTime();
        }

        protected override void WritePacketInternal(PacketBuffer buffer, PacketRole role)
        {
            buffer.WriteInt64(TempChannelId);
            buffer.WriteByte((byte)StatusCode);
            buffer.WriteInt64(ChannelId);
            buffer.WriteDateTime(CreationTime);
        }

        public override string ToString()
        {
            return $"{{{nameof(P2FCreateChannelResponse)}: TempId={TempChannelId:x8} ErrorCode={StatusCode} ChannelId={ChannelId.ToString("x8")}}}";
        }
    }
}
