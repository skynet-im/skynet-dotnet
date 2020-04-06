using Skynet.Model;
using Skynet.Network;
using Skynet.Protocol.Attributes;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x15, PacketPolicies.Duplex)]
    [MessageFlags(MessageFlags.Loopback | MessageFlags.Unencrypted)]
    public sealed class P15PasswordUpdate : ChannelMessage
    {
        public byte[] PreviousKeyHash { get; set; }
        public byte[] KeyHash { get; set; }
        public byte[] KeyHistory { get; set; }

        public override Packet Create() => new P15PasswordUpdate().Init(this);

        protected override void ReadMessage(PacketBuffer buffer)
        {
            PreviousKeyHash = buffer.ReadByteArray(32);
            KeyHash = buffer.ReadByteArray(32);
            KeyHistory = buffer.ReadMediumByteArray();
        }

        protected override void WriteMessage(PacketBuffer buffer)
        {
            buffer.WriteByteArray(PreviousKeyHash, 32);
            buffer.WriteByteArray(KeyHash, 32);
            buffer.WriteMediumByteArray(KeyHistory);
        }
    }
}
