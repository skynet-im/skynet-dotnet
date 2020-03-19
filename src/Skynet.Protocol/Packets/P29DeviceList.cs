using Skynet.Protocol.Attributes;
using Skynet.Protocol.Model;
using Skynet.Network;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x29, PacketPolicies.ServerToClient)]
    internal sealed class P29DeviceList : ChannelMessage
    {
        public List<SessionInformation> Sessions { get; set; } = new List<SessionInformation>();

        public override Packet Create() => new P29DeviceList().Init(this);

        protected override void WriteMessage(PacketBuffer buffer)
        {
            buffer.WriteUInt16((ushort)Sessions.Count);
            foreach (SessionInformation session in Sessions)
            {
                buffer.WriteInt64(session.SessionId);
                buffer.WriteDateTime(session.CreationTime);
                buffer.WriteShortString(session.ApplicationIdentifier);
            }
        }
    }
}
