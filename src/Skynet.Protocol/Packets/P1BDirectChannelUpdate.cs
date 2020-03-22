using Skynet.Model;
using Skynet.Protocol.Attributes;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x1B, PacketPolicies.ServerToClient)]
    [MessageFlags(MessageFlags.Unencrypted)]
    public sealed class P1BDirectChannelUpdate : ChannelMessage
    {
        public override Packet Create() => new P1BDirectChannelUpdate().Init(this);
    }
}
