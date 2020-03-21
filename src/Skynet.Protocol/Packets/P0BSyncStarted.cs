using Skynet.Network;
using Skynet.Protocol.Attributes;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x0B, PacketPolicies.ServerToClient)]
    public sealed class P0BSyncStarted : Packet
    {
        public int MinCount { get; set; }

        public override Packet Create() => new P0BSyncStarted().Init(this);

        protected override void ReadPacketInternal(PacketBuffer buffer, PacketRole role)
        {
            MinCount = buffer.ReadInt32();
        }

        protected override void WritePacketInternal(PacketBuffer buffer, PacketRole role)
        {
            buffer.WriteInt32(MinCount);   
        }
    }
}
