﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Skynet.Network;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Skynet.Tests.Network
{
    [TestClass]
    public class PacketStreamTests
    {
        [DataTestMethod]
        [DataRow(0x00000000, 0x00, 0x00, 0x00)]
        [DataRow(0x00000001, 0x01, 0x00, 0x00)]
        [DataRow(0x000000FF, 0xFF, 0x00, 0x00)]
        [DataRow(0x00BEEEEF, 0xEF, 0xEE, 0xBE)]
        [DataRow(0x00FFFFFF, 0xFF, 0xFF, 0xFF)]
        public async Task TestBinaryFormat(int length, int byte1, int byte2, int byte3)
        {
            using var source = new MemoryStream();
            byte[] data = new byte[length];
            var random = new Random();
            random.NextBytes(data);

            await using (var writer = new PacketStream(source, true))
            {
                await writer.WriteAsync(0x7F, data).ConfigureAwait(false);
            }

            byte[] network = source.ToArray();
            Assert.AreEqual(0x7F, network[0]);
            Assert.AreEqual(byte1, network[1]);
            Assert.AreEqual(byte2, network[2]);
            Assert.AreEqual(byte3, network[3]);
            MemoryAssert.AreEqual(data, network.AsSpan().Slice(4));

            source.Position = 0;
            await using var reader = new PacketStream(source, true);
            (bool success, byte id, PoolableMemory buffer) = await reader.ReadAsync().ConfigureAwait(false);
            Assert.IsTrue(success);
            Assert.AreEqual(0x7F, id);
            MemoryAssert.AreEqual(data, buffer.Memory);
            buffer.Return(false);
        }

        [TestMethod]
        public async Task TestReadEndOfStream()
        {
            using var source = new MemoryStream(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }, writable: false);
            await using var stream = new PacketStream(source, leaveInnerStreamOpen: true);

            (bool success, _, _) = await stream.ReadAsync().ConfigureAwait(false);
            Assert.IsFalse(success);
        }

        [TestMethod]
        public async Task TestWriteOutOfBounds()
        {
            using var target = new MemoryStream();
            await using var stream = new PacketStream(target, leaveInnerStreamOpen: true);

            byte[] data = new byte[1 << 24];
            await Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(
                () => stream.WriteAsync(0xFF, data).AsTask()).ConfigureAwait(false);
        }
    }
}
