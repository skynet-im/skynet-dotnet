using Skynet.Protocol.Attributes;
using Skynet.Protocol.Model;
using Skynet.Network;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x33, PacketPolicies.ServerToClient)]
    internal class P33DeviceListResponse : Packet
    {
        public List<SessionDetails> SessionDetails { get; set; } = new List<SessionDetails>();

        public override Packet Create() => new P33DeviceListResponse().Init(this);

        protected override void ReadPacketInternal(PacketBuffer buffer, PacketRole role)
        {
            int count = buffer.ReadUInt16();
            for (int i = 0; i < count; i++)
            {
                SessionDetails.Add(new SessionDetails(buffer.ReadInt64(), buffer.ReadDateTime(), buffer.ReadInt32()));
            }
        }

        protected override void WritePacketInternal(PacketBuffer buffer, PacketRole role)
        {
            buffer.WriteUInt16((ushort)SessionDetails.Count);
            foreach (SessionDetails details in SessionDetails)
            {
                buffer.WriteInt64(details.SessionId);
                buffer.WriteDateTime(details.LastConnected);
                buffer.WriteInt32(details.LastVersionCode);
            }
        }
    }
}
