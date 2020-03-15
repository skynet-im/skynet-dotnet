using Skynet.Model;
using Skynet.Protocol.Attributes;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x16, PacketPolicies.Duplex)]
    [MessageFlags(MessageFlags.Loopback)]
    internal sealed class P16LoopbackKeyNotify : ChannelMessage
    {
        public override Packet Create() => new P16LoopbackKeyNotify().Init(this);
    }
}
