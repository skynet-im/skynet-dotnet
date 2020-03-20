using Skynet.Model;
using Skynet.Network;
using Skynet.Protocol.Attributes;
using Skynet.Protocol.Model;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x24, PacketPolicies.Duplex)]
    [AllowedFlags(MessageFlags.MediaMessage | MessageFlags.ExternalFile)]

    internal sealed class P24DaystreamMessage : ChannelMessage
    {
        public MessageType MessageType { get; set; }
        public string Text { get; set; }

        public override Packet Create() => new P24DaystreamMessage().Init(this);

        protected override void ReadMessage(PacketBuffer buffer)
        {
            MessageType = (MessageType)buffer.ReadByte();
            Text = buffer.ReadMediumString();
        }

        protected override void WriteMessage(PacketBuffer buffer)
        {
            buffer.WriteByte((byte)MessageType);
            buffer.WriteMediumString(Text);
        }
    }
}
