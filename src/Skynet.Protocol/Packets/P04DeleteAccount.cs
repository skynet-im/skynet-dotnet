using Skynet.Protocol.Attributes;
using Skynet.Network;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x04, PacketPolicies.Receive)]
    internal sealed class P04DeleteAccount : Packet
    {
        public byte[] KeyHash { get; set; }

        public override Packet Create() => new P04DeleteAccount().Init(this);

        public override void ReadPacket(PacketBuffer buffer)
        {
            KeyHash = buffer.ReadRawByteArray(32).ToArray();
        }
    }
}
