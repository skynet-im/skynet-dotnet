using Skynet.Network;
using Skynet.Protocol.Attributes;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x08, PacketPolicies.ClientToServer | PacketPolicies.Unauthenticated)]
    public sealed class P08RestoreSession : Packet
    {
        public long SessionId { get; set; }
        public byte[] SessionToken { get; set; }
        public long LastMessageId { get; set; }
        public List<long> Channels { get; set; } = new List<long>();

        public override Packet Create() => new P08RestoreSession().Init(this);

        protected override void ReadPacketInternal(PacketBuffer buffer, PacketRole role)
        {
            SessionId = buffer.ReadInt64();
            SessionToken = buffer.ReadByteArray(32);
            ushort length = buffer.ReadUInt16();
            for (int i = 0; i < length; i++)
            {
                Channels.Add(buffer.ReadInt64());
            }
        }

        protected override void WritePacketInternal(PacketBuffer buffer, PacketRole role)
        {
            buffer.WriteInt64(SessionId);
            buffer.WriteByteArray(SessionToken);
            buffer.WriteUInt16((ushort)Channels.Count);
            foreach (long channelId in Channels)
            {
                buffer.WriteInt64(channelId);
            }
        }

        public override string ToString()
        {
            return $"{{{nameof(P08RestoreSession)}: SessionId={SessionId:x8} LastMessageId={LastMessageId.ToString("x8")}}}";
        }
    }
}
