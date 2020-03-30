using Skynet.Model;
using Skynet.Network;
using Skynet.Protocol.Attributes;
using Skynet.Protocol.Model;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x17, PacketPolicies.Duplex)]
    [MessageFlags(MessageFlags.Loopback)]
    public sealed class P17PrivateKeys : ChannelMessage
    {
        public KeyFormat SignatureKeyFormat { get; set; }
        public byte[] SignatureKey { get; set; }
        public KeyFormat DerivationKeyFormat { get; set; }
        public byte[] DerivationKey { get; set; }

        public override Packet Create() => new P17PrivateKeys().Init(this);

        protected override void ReadMessage(PacketBuffer buffer)
        {
            SignatureKeyFormat = (KeyFormat)buffer.ReadByte();
            SignatureKey = buffer.ReadMediumByteArray();
            DerivationKeyFormat = (KeyFormat)buffer.ReadByte();
            DerivationKey = buffer.ReadMediumByteArray();
        }

        protected override void WriteMessage(PacketBuffer buffer)
        {
            buffer.WriteByte((byte)SignatureKeyFormat);
            buffer.WriteMediumByteArray(SignatureKey);
            buffer.WriteByte((byte)DerivationKeyFormat);
            buffer.WriteMediumByteArray(DerivationKey);
        }

        protected override void DisposeMessage()
        {
            base.DisposeMessage();

            Array.Clear(SignatureKey, 0, SignatureKey.Length);
            Array.Clear(DerivationKey, 0, DerivationKey.Length);
        }
    }
}
