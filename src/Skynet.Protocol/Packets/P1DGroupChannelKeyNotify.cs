using Skynet.Model;
using Skynet.Network;
using Skynet.Protocol.Attributes;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x1D, PacketPolicies.Duplex)]
    [MessageFlags(MessageFlags.NoSenderSync)]

    public sealed class P1DGroupChannelKeyNotify : ChannelMessage
    {
        public long GroupChannelId { get; set; }
        public byte[] NewKey { get; set; }
        public byte[] HistoryKey { get; set; }

        public override Packet Create() => new P1DGroupChannelKeyNotify().Init(this);

        protected override void ReadMessage(PacketBuffer buffer)
        {
            ChannelId = buffer.ReadInt64();
            NewKey = buffer.ReadByteArray(64);
            HistoryKey = buffer.ReadByteArray(64);
        }

        protected override void WriteMessage(PacketBuffer buffer)
        {
            buffer.WriteInt64(ChannelId);
            buffer.WriteByteArray(NewKey);
            buffer.WriteByteArray(HistoryKey);
        }
    }
}
