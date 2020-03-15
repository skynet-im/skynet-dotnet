using Skynet.Protocol.Attributes;
using Skynet.Network;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x0B, PacketPolicies.Send)]
    internal class P0BSyncStarted : Packet
    {
        public int MinCount { get; set; }

        public override Packet Create() => new P0BSyncStarted().Init(this);

        public override void WritePacket(PacketBuffer buffer)
        {
            buffer.WriteInt32(MinCount);   
        }
    }
}
