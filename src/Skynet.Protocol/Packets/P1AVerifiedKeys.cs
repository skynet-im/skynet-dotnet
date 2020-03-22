using Skynet.Model;
using Skynet.Network;
using Skynet.Protocol.Attributes;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x1A, PacketPolicies.Duplex)]
    [MessageFlags(MessageFlags.Loopback)]
    public sealed class P1AVerifiedKeys : ChannelMessage
    {
        public byte[] Sha256 { get; set; }

        public override Packet Create() => new P1AVerifiedKeys().Init(this);

        protected override void ReadMessage(PacketBuffer buffer)
        {
            Sha256 = buffer.ReadRawByteArray(32).ToArray();
        }

        protected override void WriteMessage(PacketBuffer buffer)
        {
            buffer.WriteRawByteArray(Sha256);
        }
    }
}
