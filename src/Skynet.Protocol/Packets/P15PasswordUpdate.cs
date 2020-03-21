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
        public ReadOnlyMemory<byte> LoopbackKeyNotify { get; set; }
        public byte[] KeyHash { get; set; }

        public override Packet Create() => new P15PasswordUpdate().Init(this);

        protected override void ReadMessage(PacketBuffer buffer)
        {
            LoopbackKeyNotify = buffer.ReadMediumByteArray();
            KeyHash = buffer.ReadRawByteArray(32).ToArray();
        }

        protected override void WriteMessage(PacketBuffer buffer)
        {
            buffer.WriteRawByteArray(KeyHash);
        }
    }
}
