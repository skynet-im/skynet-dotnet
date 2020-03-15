using Skynet.Protocol.Attributes;
using Skynet.Protocol.Model;
using Skynet.Network;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x03, PacketPolicies.Send)]
    internal sealed class P03CreateAccountResponse : Packet
    {
        public CreateAccountStatus StatusCode { get; set; }

        public override Packet Create() => new P03CreateAccountResponse().Init(this);

        public override void WritePacket(PacketBuffer buffer)
        {
            buffer.WriteByte((byte)StatusCode);
        }

        public override string ToString()
        {
            return $"{{{nameof(P03CreateAccountResponse)}: ErrorCode={StatusCode}}}";
        }
    }
}
