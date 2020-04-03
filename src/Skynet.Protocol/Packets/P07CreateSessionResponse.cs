using Skynet.Network;
using Skynet.Protocol.Attributes;
using Skynet.Protocol.Model;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x07, PacketPolicies.ServerToClient)]
    public sealed class P07CreateSessionResponse : Packet
    {
        public CreateSessionStatus StatusCode { get; set; }
        public long AccountId { get; set; }
        public long SessionId { get; set; }
        public byte[] SessionToken { get; set; }
        public string WebToken { get; set; }

        public override Packet Create() => new P07CreateSessionResponse().Init(this);

        protected override void ReadPacketInternal(PacketBuffer buffer, PacketRole role)
        {
            StatusCode = (CreateSessionStatus)buffer.ReadByte();
            AccountId = buffer.ReadInt64();
            SessionId = buffer.ReadInt64();
            SessionToken = buffer.ReadByteArray(32);
            WebToken = buffer.ReadMediumString();
        }

        protected override void WritePacketInternal(PacketBuffer buffer, PacketRole role)
        {
            buffer.WriteByte((byte)StatusCode);
            buffer.WriteInt64(AccountId);
            buffer.WriteInt64(SessionId);
            buffer.WriteByteArray(SessionToken);
            buffer.WriteMediumString(WebToken);
        }

        public override string ToString()
        {
            return $"{{{nameof(P07CreateSessionResponse)}: AccountId={AccountId:x8} SessionId={SessionId:x8} StatusCode={StatusCode}}}";
        }
    }
}
