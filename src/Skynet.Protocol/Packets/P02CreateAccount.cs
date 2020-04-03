using Skynet.Network;
using Skynet.Protocol.Attributes;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x02, PacketPolicies.ClientToServer | PacketPolicies.Unauthenticated)]
    public sealed class P02CreateAccount : Packet
    {
        public string AccountName { get; set; }
        public byte[] KeyHash { get; set; }

        public override Packet Create() => new P02CreateAccount().Init(this);

        protected override void ReadPacketInternal(PacketBuffer buffer, PacketRole role)
        {
            AccountName = buffer.ReadShortString();
            KeyHash = buffer.ReadByteArray(32);
        }

        protected override void WritePacketInternal(PacketBuffer buffer, PacketRole role)
        {
            buffer.WriteShortString(AccountName);
            buffer.WriteByteArray(KeyHash);
        }

        public override string ToString()
        {
            return $"{{{nameof(P02CreateAccount)}: AccountName={AccountName}}}";
        }
    }
}
