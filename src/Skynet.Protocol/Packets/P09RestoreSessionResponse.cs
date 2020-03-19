using Skynet.Protocol.Attributes;
using Skynet.Protocol.Model;
using Skynet.Network;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x09, PacketPolicies.ServerToClient)]
    internal sealed class P09RestoreSessionResponse : Packet
    {
        public RestoreSessionStatus StatusCode { get; set; }

        public override Packet Create() => new P09RestoreSessionResponse().Init(this);

        protected override void ReadPacketInternal(PacketBuffer buffer, PacketRole role)
        {
            StatusCode = (RestoreSessionStatus)buffer.ReadByte();
        }

        protected override void WritePacketInternal(PacketBuffer buffer, PacketRole role)
        {
            buffer.WriteByte((byte)StatusCode);
        }

        public override string ToString()
        {
            return $"{{{nameof(P09RestoreSessionResponse)}: ErrorCode={StatusCode}}}";
        }
    }
}
