using Skynet.Model;
using Skynet.Network;
using Skynet.Protocol.Attributes;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x27, PacketPolicies.Duplex)]
    [AllowedFlags(MessageFlags.Unencrypted | MessageFlags.MediaMessage | MessageFlags.ExternalFile)]
    internal sealed class P27ProfileImage : ChannelMessage
    {
        public string Caption { get; set; }

        public override Packet Create() => new P27ProfileImage().Init(this);

        protected override void ReadMessage(PacketBuffer buffer)
        {
            Caption = buffer.ReadMediumString();
        }

        protected override void WriteMessage(PacketBuffer buffer)
        {
            buffer.WriteMediumString(Caption);
        }
    }
}
