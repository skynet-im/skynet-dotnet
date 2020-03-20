using Skynet.Model;
using Skynet.Network;
using Skynet.Protocol.Attributes;
using Skynet.Protocol.Model;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x2B, PacketPolicies.ServerToClient)]
    [MessageFlags(MessageFlags.Unencrypted | MessageFlags.NoSenderSync)]
    internal class P2BOnlineState : ChannelMessage
    {
        public OnlineState OnlineState { get; set; }
        public DateTime LastActive { get; set; }

        public override Packet Create() => new P2BOnlineState().Init(this);

        protected override void ReadMessage(PacketBuffer buffer)
        {
            OnlineState = (OnlineState)buffer.ReadByte();
            if (OnlineState == OnlineState.Inactive)
                LastActive = buffer.ReadDateTime();
        }

        protected override void WriteMessage(PacketBuffer buffer)
        {
            buffer.WriteByte((byte)OnlineState);
            if (OnlineState == OnlineState.Inactive)
                buffer.WriteDateTime(LastActive);
        }
    }
}
