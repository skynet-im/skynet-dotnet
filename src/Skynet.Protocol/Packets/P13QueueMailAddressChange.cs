using Skynet.Model;
using Skynet.Network;
using Skynet.Protocol.Attributes;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x13, PacketPolicies.ClientToServer)]
    [MessageFlags(MessageFlags.Loopback | MessageFlags.Unencrypted)]
    internal sealed class P13QueueMailAddressChange : ChannelMessage
    {
        public string NewMailAddress { get; set; }

        public override Packet Create() => new P13QueueMailAddressChange().Init(this);

        protected override void ReadMessage(PacketBuffer buffer)
        {
            NewMailAddress = buffer.ReadShortString();
        }

        protected override void WriteMessage(PacketBuffer buffer)
        {
            buffer.WriteShortString(NewMailAddress);
        }
    }
}
