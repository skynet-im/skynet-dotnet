using Skynet.Protocol.Attributes;
using Skynet.Protocol.Model;
using Skynet.Network;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x0C, PacketPolicies.ServerToClient)]
    internal sealed class P0CChannelMessageResponse : Packet
    {
        public long ChannelId { get; set; }
        public long TempMessageId { get; set; }
        public MessageSendStatus StatusCode { get; set; }
        public long MessageId { get; set; }
        public long SkipCount { get; set; }
        public DateTime DispatchTime { get; set; }

        public override Packet Create() => new P0CChannelMessageResponse().Init(this);

        protected override void ReadPacketInternal(PacketBuffer buffer, PacketRole role)
        {
            ChannelId = buffer.ReadInt64();
            TempMessageId = buffer.ReadInt64();
            StatusCode = (MessageSendStatus)buffer.ReadByte();
            MessageId = buffer.ReadInt64();
            SkipCount = buffer.ReadInt64();
            DispatchTime = buffer.ReadDateTime();
        }

        protected override void WritePacketInternal(PacketBuffer buffer, PacketRole role)
        {
            buffer.WriteInt64(ChannelId);
            buffer.WriteInt64(TempMessageId);
            buffer.WriteByte((byte)StatusCode);
            buffer.WriteInt64(MessageId);
            buffer.WriteInt64(SkipCount);
            buffer.WriteDateTime(DispatchTime);
        }

        public override string ToString()
        {
            return $"{{{nameof(P0CChannelMessageResponse)}: ErrorCode={StatusCode}}}";
        }
    }
}
