﻿using Skynet.Model;
using Skynet.Network;
using Skynet.Protocol.Attributes;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x19, PacketPolicies.Duplex)]
    [MessageFlags(MessageFlags.Unencrypted)]
    public sealed class P19ArchiveChannel : ChannelMessage
    {
        public ArchiveMode ArchiveMode { get; set; }

        public override Packet Create() => new P19ArchiveChannel().Init(this);

        protected override void ReadMessage(PacketBuffer buffer)
        {
            ArchiveMode = (ArchiveMode)buffer.ReadByte();
        }

        protected override void WriteMessage(PacketBuffer buffer)
        {
            buffer.WriteByte((byte)ArchiveMode);
        }
    }
}
