using Skynet.Model;
using Skynet.Protocol.Attributes;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x1D, PacketPolicies.Duplex)]
    [MessageFlags(MessageFlags.NoSenderSync)]

    internal sealed class P1DGroupChannelKeyNotify : ChannelMessage
    {
        public override Packet Create() => new P1DGroupChannelKeyNotify().Init(this);
    }
}
