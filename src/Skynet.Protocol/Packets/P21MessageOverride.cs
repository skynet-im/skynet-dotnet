using Skynet.Model;
using Skynet.Protocol.Attributes;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x21, PacketPolicies.Duplex)]
    [MessageFlags(MessageFlags.None)]

    internal sealed class P21MessageOverride : ChannelMessage
    {
        public override Packet Create() => new P21MessageOverride().Init(this);
    }
}
