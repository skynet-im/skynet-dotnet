using Skynet.Network;
using Skynet.Protocol.Attributes;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x04, PacketPolicies.ClientToServer)]
    public sealed class P04DeleteAccount : Packet
    {
        public byte[] KeyHash { get; set; }

        public override Packet Create() => new P04DeleteAccount().Init(this);

        protected override void ReadPacketInternal(PacketBuffer buffer, PacketRole role)
        {
            KeyHash = buffer.ReadRawByteArray(32).ToArray();
        }

        protected override void WritePacketInternal(PacketBuffer buffer, PacketRole role)
        {
            buffer.WriteRawByteArray(KeyHash);
        }
    }
}
