﻿using Skynet.Protocol.Attributes;
using Skynet.Protocol.Model;
using Skynet.Network;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x07, PacketPolicies.Send)]
    internal sealed class P07CreateSessionResponse : Packet
    {
        public CreateSessionStatus StatusCode { get; set; }
        public long AccountId { get; set; }
        public long SessionId { get; set; }
        public byte[] SessionToken { get; set; }
        public string WebToken { get; set; }

        public override Packet Create() => new P07CreateSessionResponse().Init(this);

        public override void WritePacket(PacketBuffer buffer)
        {
            buffer.WriteByte((byte)StatusCode);
            buffer.WriteInt64(AccountId);
            buffer.WriteInt64(SessionId);
            buffer.WriteRawByteArray(SessionToken);
            buffer.WriteString(WebToken);
        }

        public override string ToString()
        {
            return $"{{{nameof(P07CreateSessionResponse)}: AccountId={AccountId:x8} SessionId={SessionId:x8} StatusCode={StatusCode}}}";
        }
    }
}
