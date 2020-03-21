using Skynet.Network;
using Skynet.Protocol.Attributes;
using Skynet.Protocol.Model;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x2E, PacketPolicies.ServerToClient)]
    public sealed class P2ESearchAccountResponse : Packet
    {
        public List<SearchResult> Results { get; set; } = new List<SearchResult>();

        public override Packet Create() => new P2ESearchAccountResponse().Init(this);

        protected override void ReadPacketInternal(PacketBuffer buffer, PacketRole role)
        {
            int resultCount = buffer.ReadUInt16();
            for (int i = 0; i < resultCount; i++)
            {
                Results.Add(new SearchResult(buffer.ReadInt64(), buffer.ReadShortString()));
            }
        }

        protected override void WritePacketInternal(PacketBuffer buffer, PacketRole role)
        {
            buffer.WriteUInt16((ushort)Results.Count);
            foreach (SearchResult result in Results)
            {
                buffer.WriteInt64(result.AccountId);
                buffer.WriteShortString(result.AccountName);
            }
        }
    }
}
