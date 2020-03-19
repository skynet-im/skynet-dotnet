using Skynet.Model;
using Skynet.Protocol.Attributes;
using Skynet.Network;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x26, PacketPolicies.Duplex)]
    [AllowedFlags(MessageFlags.Unencrypted)]
    internal sealed class P26Bio : ChannelMessage
    {
        public string PersonalMessage { get; set; }

        public override Packet Create() => new P26Bio().Init(this);

        protected override void ReadMessage(PacketBuffer buffer)
        {
            PersonalMessage = buffer.ReadMediumString();
        }

        protected override void WriteMessage(PacketBuffer buffer)
        {
            buffer.WriteMediumString(PersonalMessage);
        }
    }
}
