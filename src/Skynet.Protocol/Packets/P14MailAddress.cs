using Skynet.Model;
using Skynet.Network;
using Skynet.Protocol.Attributes;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x14, PacketPolicies.ServerToClient)]
    [MessageFlags(MessageFlags.Unencrypted)]
    public sealed class P14MailAddress : ChannelMessage
    {
        public string? MailAddress { get; set; }

        public override Packet Create() => new P14MailAddress().Init(this);

        protected override void ReadMessage(PacketBuffer buffer)
        {
            MailAddress = buffer.ReadShortString();
        }

        protected override void WriteMessage(PacketBuffer buffer)
        {
            buffer.WriteShortString(MailAddress);
        }
    }
}
