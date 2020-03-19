using Skynet.Protocol.Attributes;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x0F, PacketPolicies.ServerToClient)]
    internal sealed class P0FSyncFinished : Packet
    {
        public override Packet Create() => new P0FSyncFinished().Init(this);
    }
}
