using Skynet.Model;
using Skynet.Network;
using Skynet.Protocol.Attributes;
using Skynet.Protocol.Model;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x21, PacketPolicies.Duplex)]
    [MessageFlags(MessageFlags.None)]

    internal sealed class P21MessageOverride : ChannelMessage
    {
        public OverrideAction Action { get; set; }
        public string NewText { get; set; }

        public override Packet Create() => new P21MessageOverride().Init(this);

        protected override void ReadMessage(PacketBuffer buffer)
        {
            Action = (OverrideAction)buffer.ReadByte();
            if (Action == OverrideAction.Edit)
                NewText = buffer.ReadMediumString();
        }

        protected override void WriteMessage(PacketBuffer buffer)
        {
            buffer.WriteByte((byte)Action);
            if (Action == OverrideAction.Edit)
                buffer.WriteMediumString(NewText);
        }
    }
}
