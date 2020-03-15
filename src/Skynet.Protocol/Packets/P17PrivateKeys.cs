using Skynet.Model;
using Skynet.Protocol.Attributes;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x17, PacketPolicies.Duplex)]
    [MessageFlags(MessageFlags.Loopback)]
    internal sealed class P17PrivateKeys : ChannelMessage
    {
        public override Packet Create() => new P17PrivateKeys().Init(this);
    }
}
