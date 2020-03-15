using Skynet.Protocol.Attributes;
using Skynet.Network;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x00, PacketPolicies.Receive | PacketPolicies.Unauthenticated | PacketPolicies.Uninitialized)]
    internal sealed class P00ConnectionHandshake : Packet
    {
        public int ProtocolVersion { get; set; }
        public string ApplicationIdentifier { get; set; }
        public int VersionCode { get; set; }

        public override Packet Create() => new P00ConnectionHandshake().Init(this);

        public override void ReadPacket(PacketBuffer buffer)
        {
            ProtocolVersion = buffer.ReadInt32();
            ApplicationIdentifier = buffer.ReadShortString();
            VersionCode = buffer.ReadInt32();
        }
    }
}
