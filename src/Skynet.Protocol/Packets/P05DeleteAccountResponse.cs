using Skynet.Protocol.Attributes;
using Skynet.Protocol.Model;
using Skynet.Network;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x05, PacketPolicies.Send)]
    internal sealed class P05DeleteAccountResponse : Packet
    {
        public DeleteAccountStatus StatusCode { get; set; }

        public override Packet Create() => new P05DeleteAccountResponse().Init(this);

        public override void WritePacket(PacketBuffer buffer)
        {
            buffer.WriteByte((byte)StatusCode);
        }
    }
}
