using Skynet.Model;
using Skynet.Network;
using Skynet.Protocol.Attributes;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x26, PacketPolicies.Duplex)]
    [AllowedFlags(MessageFlags.Unencrypted)]
    public sealed class P26Bio : ChannelMessage
    {
        public string? Bio { get; set; }

        public override Packet Create() => new P26Bio().Init(this);

        protected override void ReadMessage(PacketBuffer buffer)
        {
            Bio = buffer.ReadMediumString();
        }

        protected override void WriteMessage(PacketBuffer buffer)
        {
            buffer.WriteMediumString(Bio);
        }
    }
}
