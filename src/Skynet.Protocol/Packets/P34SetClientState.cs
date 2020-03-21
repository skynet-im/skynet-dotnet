using Skynet.Network;
using Skynet.Protocol.Attributes;
using Skynet.Protocol.Model;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x34, PacketPolicies.ClientToServer)]
    public sealed class P34SetClientState : Packet
    {
        public OnlineState OnlineState { get; set; }
        public ChannelAction Action { get; set; }
        public long ChannelId { get; set; }

        public override Packet Create() => new P34SetClientState().Init(this);

        protected override void ReadPacketInternal(PacketBuffer buffer, PacketRole role)
        {
            OnlineState = (OnlineState)buffer.ReadByte();
            Action = (ChannelAction)buffer.ReadByte();
            if (Action != ChannelAction.None)
                ChannelId = buffer.ReadInt64();
        }

        protected override void WritePacketInternal(PacketBuffer buffer, PacketRole role)
        {
            buffer.WriteByte((byte)OnlineState);
            buffer.WriteByte((byte)Action);
            if (Action != ChannelAction.None)
                buffer.WriteInt64(ChannelId);
        }
    }
}
