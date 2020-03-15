using Skynet.Model;
using Skynet.Protocol.Attributes;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x20, PacketPolicies.Duplex)]
    [AllowedFlags(MessageFlags.MediaMessage | MessageFlags.ExternalFile)]

    internal sealed class P20ChatMessage : ChannelMessage
    {
        public override Packet Create() => new P20ChatMessage().Init(this);
    }
}
