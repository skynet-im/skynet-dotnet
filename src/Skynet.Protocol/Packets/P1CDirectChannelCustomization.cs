using Skynet.Model;
using Skynet.Network;
using Skynet.Protocol.Attributes;
using Skynet.Protocol.Model;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x1C, PacketPolicies.Duplex)]
    [MessageFlags(MessageFlags.Loopback)]
    public sealed class P1CDirectChannelCustomization : ChannelMessage
    {
        public string? CustomNickname { get; set; }
        public ImageShape ProfileImageShape { get; set; }
        
        public override Packet Create() => new P1CDirectChannelCustomization().Init(this);

        protected override void ReadMessage(PacketBuffer buffer)
        {
            CustomNickname = buffer.ReadShortString();
            ProfileImageShape = (ImageShape)buffer.ReadByte();
        }

        protected override void WriteMessage(PacketBuffer buffer)
        {
            buffer.WriteShortString(CustomNickname);
            buffer.WriteByte((byte)ProfileImageShape);
        }
    }
}
