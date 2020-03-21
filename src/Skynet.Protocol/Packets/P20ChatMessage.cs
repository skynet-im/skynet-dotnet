using Skynet.Model;
using Skynet.Network;
using Skynet.Protocol.Attributes;
using Skynet.Protocol.Model;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x20, PacketPolicies.Duplex)]
    [AllowedFlags(MessageFlags.MediaMessage | MessageFlags.ExternalFile)]

    public sealed class P20ChatMessage : ChannelMessage
    {
        public MessageType MessageType { get; set; }
        public string Text { get; set; }
        public long QuotedMessageId { get; set; }

        public override Packet Create() => new P20ChatMessage().Init(this);

        protected override void ReadMessage(PacketBuffer buffer)
        {
            MessageType = (MessageType)buffer.ReadByte();
            Text = buffer.ReadMediumString();
            QuotedMessageId = buffer.ReadInt64();
        }

        protected override void WriteMessage(PacketBuffer buffer)
        {
            buffer.WriteByte((byte)MessageType);
            buffer.WriteMediumString(Text);
            buffer.WriteInt64(QuotedMessageId);
        }
    }
}
