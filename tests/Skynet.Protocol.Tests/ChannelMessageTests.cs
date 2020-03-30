using Microsoft.VisualStudio.TestTools.UnitTesting;
using Skynet.Model;
using Skynet.Network;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Skynet.Protocol.Tests
{
    [TestClass]
    public class ChannelMessageTests
    {
        private const string text = "Hello World!";

        [TestMethod]
        public void TestUnencrypted()
        {
            using var message = new FakeMessage { Text = text, MessageFlags = MessageFlags.Unencrypted };
            using var buffer = new PacketBuffer();

            message.WritePacket(buffer, PacketRole.Client);
            
            buffer.Position = 0;

            using var received = new FakeMessage();
            received.ReadPacket(buffer, PacketRole.Server);

            Assert.AreEqual(text, received.Text);
        }

        [TestMethod]
        public void TestEncrypted()
        {
            using var message = new FakeMessage { Text = text };
            Span<byte> key = stackalloc byte[64];
            RandomNumberGenerator.Fill(key);

            using var buffer = new PacketBuffer();

            message.Encrypt(key);
            message.WritePacket(buffer, PacketRole.Client);

            buffer.Position = 0;

            using var received = new FakeMessage();
            received.ReadPacket(buffer, PacketRole.Server);
            received.Decrypt(key);

            Assert.AreEqual(text, received.Text);
        }

        private class FakeMessage : ChannelMessage
        {
            public FakeMessage()
            {
                Id = 0x7f;
                Policies = PacketPolicies.Duplex;
                AllowedFlags = MessageFlags.Unencrypted;
            }

            public string Text { get; set; }

            public override Packet Create() => new FakeMessage();

            protected override void ReadMessage(PacketBuffer buffer)
            {
                Text = buffer.ReadMediumString();
            }

            protected override void WriteMessage(PacketBuffer buffer)
            {
                buffer.WriteMediumString(Text);
            }
        }
    }
}
