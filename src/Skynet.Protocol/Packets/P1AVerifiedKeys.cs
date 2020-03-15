using Skynet.Model;
using Skynet.Protocol.Attributes;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x1A, PacketPolicies.Duplex)]
    [MessageFlags(MessageFlags.Loopback)]
    internal sealed class P1AVerifiedKeys : ChannelMessage
    {
        public override Packet Create() => new P1AVerifiedKeys().Init(this);
    }
}
