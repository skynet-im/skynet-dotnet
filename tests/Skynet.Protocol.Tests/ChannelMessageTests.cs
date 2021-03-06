﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            using var buffer = new PacketBuffer();

            using (var message = new FakeMessage { Text = text, MessageFlags = MessageFlags.Unencrypted })
            {
                message.WritePacket(buffer, PacketRole.Client);
            }

            buffer.Position = 0;
            byte[]? content;

            using (var received = new FakeMessage())
            {
                received.ReadPacket(buffer, PacketRole.Server);
                content = received.PacketContent;
                
                Assert.AreEqual(text, received.Text);
            }

            // Reading a packet must not alter its contents
            Assert.IsNotNull(content);
            using var contentBuffer = new PacketBuffer(content);
            Assert.AreEqual(text, contentBuffer.ReadMediumString());
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


        [TestMethod]
        public void TestPacketContentUnencrypted()
        {
            byte[]? content;
            using (var message = new FakeMessage { Text = text, MessageFlags = MessageFlags.Unencrypted })
                content = message.PacketContent;

            Assert.IsNotNull(content);
            using var buffer = new PacketBuffer(content);
            Assert.AreEqual(text, buffer.ReadMediumString());
        }

        [TestMethod]
        public void TestPacketContentEncrypted()
        {
            using var message = new FakeMessage { Text = text };

            Assert.IsNull(message.PacketContent);
        }

        private class FakeMessage : ChannelMessage
        {
            public FakeMessage()
            {
                Id = 0x7f;
                Policies = PacketPolicies.Duplex;
                AllowedFlags = MessageFlags.Unencrypted;
            }

            public string? Text { get; set; }

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
