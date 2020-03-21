using Skynet.Model;
using Skynet.Protocol.Attributes;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x2A, PacketPolicies.Duplex)]
    [RequiredFlags(MessageFlags.Loopback)]
    [AllowedFlags(MessageFlags.Loopback | MessageFlags.MediaMessage | MessageFlags.ExternalFile)]
    public sealed class P2ABackgroundImage : ChannelMessage
    {
        public override Packet Create() => new P2ABackgroundImage().Init(this);
    }
}
