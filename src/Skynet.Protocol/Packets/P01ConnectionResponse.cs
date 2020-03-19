using Skynet.Protocol.Attributes;
using Skynet.Protocol.Model;
using Skynet.Network;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x01, PacketPolicies.ServerToClient | PacketPolicies.Unauthenticated | PacketPolicies.Uninitialized)]
    internal sealed class P01ConnectionResponse : Packet
    {
        public ConnectionState ConnectionState { get; set; }
        public int LatestVersionCode { get; set; }
        public string LatestVersion { get; set; }

        public override Packet Create() => new P01ConnectionResponse().Init(this);

        protected override void ReadPacketInternal(PacketBuffer buffer, PacketRole role)
        {
            ConnectionState = (ConnectionState)buffer.ReadByte();

            if (ConnectionState != ConnectionState.Valid)
            {
                LatestVersionCode = buffer.ReadInt32();
                LatestVersion = buffer.ReadShortString();
            }
        }

        protected override void WritePacketInternal(PacketBuffer buffer, PacketRole role)
        {
            buffer.WriteByte((byte)ConnectionState);

            if (ConnectionState != ConnectionState.Valid)
            {
                buffer.WriteInt32(LatestVersionCode);
                buffer.WriteShortString(LatestVersion);
            }
        }
    }
}
