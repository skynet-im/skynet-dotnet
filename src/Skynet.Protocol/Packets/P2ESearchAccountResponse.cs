using Skynet.Protocol.Attributes;
using Skynet.Protocol.Model;
using Skynet.Network;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x2E, PacketPolicies.ServerToClient)]
    internal sealed class P2ESearchAccountResponse : Packet
    {
        public List<SearchResult> Results { get; set; } = new List<SearchResult>();

        public override Packet Create() => new P2ESearchAccountResponse().Init(this);

        protected override void ReadPacketInternal(PacketBuffer buffer, PacketRole role)
        {
            int resultCount = buffer.ReadUInt16();
            for (int i = 0; i < resultCount; i++)
            {
                var result = new SearchResult(buffer.ReadInt64(), buffer.ReadShortString());

                int forwardedCount = buffer.ReadUInt16();
                for (int j = 0; j < forwardedCount; j++)
                {
                    result.ForwardedPackets.Add((buffer.ReadByte(), buffer.ReadMediumByteArray().ToArray()));
                }

                Results.Add(result);
            }
        }

        protected override void WritePacketInternal(PacketBuffer buffer, PacketRole role)
        {
            buffer.WriteUInt16((ushort)Results.Count);
            foreach (SearchResult result in Results)
            {
                buffer.WriteInt64(result.AccountId);
                buffer.WriteShortString(result.AccountName);
                buffer.WriteUInt16((ushort)result.ForwardedPackets.Count);
                foreach ((byte packetId, byte[] packetContent) in result.ForwardedPackets)
                {
                    buffer.WriteByte(packetId);
                    buffer.WriteMediumByteArray(packetContent);
                }
            }
        }
    }
}
