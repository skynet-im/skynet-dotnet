using Skynet.Network;
using Skynet.Protocol.Attributes;
using Skynet.Protocol.Model;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x05, PacketPolicies.ServerToClient)]
    public sealed class P05DeleteAccountResponse : Packet
    {
        public DeleteAccountStatus StatusCode { get; set; }

        public override Packet Create() => new P05DeleteAccountResponse().Init(this);

        protected override void ReadPacketInternal(PacketBuffer buffer, PacketRole role)
        {
            StatusCode = (DeleteAccountStatus)buffer.ReadByte();
        }

        protected override void WritePacketInternal(PacketBuffer buffer, PacketRole role)
        {
            buffer.WriteByte((byte)StatusCode);
        }
    }
}
