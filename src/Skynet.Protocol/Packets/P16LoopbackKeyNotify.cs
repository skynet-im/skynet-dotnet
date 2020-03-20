using Skynet.Model;
using Skynet.Network;
using Skynet.Protocol.Attributes;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x16, PacketPolicies.Duplex)]
    [MessageFlags(MessageFlags.Loopback)]
    internal sealed class P16LoopbackKeyNotify : ChannelMessage
    {
        public byte[] Key { get; set; }
        public override Packet Create() => new P16LoopbackKeyNotify().Init(this);

        protected override void ReadMessage(PacketBuffer buffer)
        {
            Key = buffer.ReadRawByteArray(64).ToArray();
        }

        protected override void WriteMessage(PacketBuffer buffer)
        {
            buffer.WriteRawByteArray(Key);
        }
    }
}
