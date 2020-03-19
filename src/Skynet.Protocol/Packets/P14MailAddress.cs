using Skynet.Protocol.Attributes;
using Skynet.Network;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x14, PacketPolicies.ServerToClient)]
    internal sealed class P14MailAddress : ChannelMessage
    {
        public string MailAddress { get; set; }

        public override Packet Create() => new P14MailAddress().Init(this);

        protected override void WriteMessage(PacketBuffer buffer)
        {
            buffer.WriteShortString(MailAddress);
        }
    }
}
