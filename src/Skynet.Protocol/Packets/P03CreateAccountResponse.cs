using Skynet.Network;
using Skynet.Protocol.Attributes;
using Skynet.Protocol.Model;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x03, PacketPolicies.ServerToClient)]
    public sealed class P03CreateAccountResponse : Packet
    {
        public CreateAccountStatus StatusCode { get; set; }

        public override Packet Create() => new P03CreateAccountResponse().Init(this);

        protected override void ReadPacketInternal(PacketBuffer buffer, PacketRole role)
        {
            StatusCode = (CreateAccountStatus)buffer.ReadByte();
        }

        protected override void WritePacketInternal(PacketBuffer buffer, PacketRole role)
        {
            buffer.WriteByte((byte)StatusCode);
        }

        public override string ToString()
        {
            return $"{{{nameof(P03CreateAccountResponse)}: ErrorCode={StatusCode}}}";
        }
    }
}
