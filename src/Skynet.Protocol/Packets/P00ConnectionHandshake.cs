using Skynet.Network;
using Skynet.Protocol.Attributes;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x00, PacketPolicies.ClientToServer | PacketPolicies.Unauthenticated | PacketPolicies.Uninitialized)]
    public sealed class P00ConnectionHandshake : Packet
    {
        public int ProtocolVersion { get; set; }
        public string ApplicationIdentifier { get; set; }
        public int VersionCode { get; set; }

        public override Packet Create() => new P00ConnectionHandshake().Init(this);

        protected override void ReadPacketInternal(PacketBuffer buffer, PacketRole role)
        {
            ProtocolVersion = buffer.ReadInt32();
            ApplicationIdentifier = buffer.ReadShortString();
            VersionCode = buffer.ReadInt32();
        }

        protected override void WritePacketInternal(PacketBuffer buffer, PacketRole role)
        {
            buffer.WriteInt32(ProtocolVersion);
            buffer.WriteShortString(ApplicationIdentifier);
            buffer.WriteInt32(VersionCode);
        }
    }
}
