using Skynet.Protocol.Attributes;
using Skynet.Network;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x1B, PacketPolicies.Send)]
    internal sealed class P1BDirectChannelUpdate : ChannelMessage
    {
        public override Packet Create() => new P1BDirectChannelUpdate().Init(this);
    }
}
