using Skynet.Model;
using Skynet.Protocol.Attributes;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x1C, PacketPolicies.Duplex)]
    [MessageFlags(MessageFlags.Loopback)]
    internal sealed class P1CDirectChannelCustomization : ChannelMessage
    {
        public override Packet Create() => new P1CDirectChannelCustomization().Init(this);
    }
}
