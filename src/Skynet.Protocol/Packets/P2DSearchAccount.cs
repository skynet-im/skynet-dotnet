using Skynet.Network;
using Skynet.Protocol.Attributes;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x2D, PacketPolicies.ClientToServer)]
    public sealed class P2DSearchAccount : Packet
    {
        public string? Query { get; set; }

        public override Packet Create() => new P2DSearchAccount().Init(this);

        protected override void ReadPacketInternal(PacketBuffer buffer, PacketRole role)
        {
            Query = buffer.ReadShortString();
        }

        protected override void WritePacketInternal(PacketBuffer buffer, PacketRole role)
        {
            buffer.WriteShortString(Query);
        }
    }
}
