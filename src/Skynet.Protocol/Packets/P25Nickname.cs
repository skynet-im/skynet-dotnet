using Skynet.Model;
using Skynet.Protocol.Attributes;
using Skynet.Network;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x25, PacketPolicies.Duplex)]
    [AllowedFlags(MessageFlags.Unencrypted)]
    internal sealed class P25Nickname : ChannelMessage
    {
        public string Nickname { get; set; }

        public override Packet Create() => new P25Nickname().Init(this);

        protected override void ReadMessage(PacketBuffer buffer)
        {
            Nickname = buffer.ReadShortString();
        }

        protected override void WriteMessage(PacketBuffer buffer)
        {
            buffer.WriteShortString(Nickname);
        }
    }
}
