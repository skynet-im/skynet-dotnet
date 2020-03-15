using Skynet.Protocol.Attributes;
using Skynet.Network;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x2D, PacketPolicies.Receive)]
    internal sealed class P2DSearchAccount : Packet
    {
        public string Query { get; set; }

        public override Packet Create() => new P2DSearchAccount().Init(this);

        public override void ReadPacket(PacketBuffer buffer)
        {
            Query = buffer.ReadShortString();
        }
    }
}
