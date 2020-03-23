using Skynet.Network;
using Skynet.Protocol.Attributes;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x06, PacketPolicies.ClientToServer | PacketPolicies.Unauthenticated)]
    public sealed class P06CreateSession : Packet
    {
        public string AccountName { get; set; }
        public byte[] KeyHash { get; set; }
        public string FcmRegistrationToken { get; set; }

        public override Packet Create() => new P06CreateSession().Init(this);

        protected override void ReadPacketInternal(PacketBuffer buffer, PacketRole role)
        {
            AccountName = buffer.ReadShortString();
            KeyHash = buffer.ReadByteArray(32);
            FcmRegistrationToken = buffer.ReadMediumString();
        }

        protected override void WritePacketInternal(PacketBuffer buffer, PacketRole role)
        {
            buffer.WriteShortString(AccountName);
            buffer.WriteByteArray(KeyHash);
            buffer.WriteMediumString(FcmRegistrationToken);
        }

        public override string ToString()
        {
            return $"{{{nameof(P06CreateSession)}: AccountName={AccountName}}}";
        }
    }
}
