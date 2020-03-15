using Skynet.Protocol.Attributes;
using Skynet.Protocol.Model;
using Skynet.Network;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x09, PacketPolicies.Send)]
    internal sealed class P09RestoreSessionResponse : Packet
    {
        public RestoreSessionStatus StatusCode { get; set; }

        public override Packet Create() => new P09RestoreSessionResponse().Init(this);

        public override void WritePacket(PacketBuffer buffer)
        {
            buffer.WriteByte((byte)StatusCode);
        }

        public override string ToString()
        {
            return $"{{{nameof(P09RestoreSessionResponse)}: ErrorCode={StatusCode}}}";
        }
    }
}
