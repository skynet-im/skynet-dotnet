using Skynet.Protocol.Attributes;
using Skynet.Protocol.Model;
using Skynet.Network;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x34, PacketPolicies.Receive)]
    internal class P34SetClientState : Packet
    {
        public OnlineState OnlineState { get; set; }
        public ChannelAction Action { get; set; }
        public long ChannelId { get; set; }

        public override Packet Create() => new P34SetClientState().Init(this);

        public override void ReadPacket(PacketBuffer buffer)
        {
            OnlineState = (OnlineState)buffer.ReadByte();
            Action = (ChannelAction)buffer.ReadByte();
            if (Action != ChannelAction.None)
                ChannelId = buffer.ReadInt64();
        }
    }
}
