using Skynet.Protocol.Attributes;
using Skynet.Protocol.Model;
using Skynet.Network;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x2B, PacketPolicies.Send)]
    internal class P2BOnlineState : ChannelMessage
    {
        public OnlineState OnlineState { get; set; }
        public DateTime LastActive { get; set; }

        public override Packet Create() => new P2BOnlineState().Init(this);

        protected override void WriteMessage(PacketBuffer buffer)
        {
            buffer.WriteByte((byte)OnlineState);
            if (OnlineState == OnlineState.Inactive)
                buffer.WriteDateTime(LastActive);
        }
    }
}
