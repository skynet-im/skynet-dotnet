﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Skynet.Network;
using System;
using System.IO;

namespace Skynet.Tests.Network
{
    [TestClass]
    public class PacketBufferTests
    {
        [TestMethod]
        public void TestReadBoundry()
        {
            using var buffer = new PacketBuffer(new byte[16]);
            buffer.ReadInt64();
            buffer.ReadInt32();
            buffer.ReadUInt16();
            buffer.ReadBoolean();
            Assert.ThrowsException<EndOfStreamException>(() => buffer.ReadInt32());
            Assert.ThrowsException<EndOfStreamException>(() => buffer.ReadMediumByteArray());
            Assert.ThrowsException<EndOfStreamException>(() => buffer.ReadByteArray(13));
        }

        [TestMethod]
        public void TestPositionBoundry()
        {
            using var buffer = new PacketBuffer(capacity: 4);
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => buffer.Position = -1);
            buffer.Position = 500;
            Assert.IsTrue(buffer.Capacity >= 500);
        }

        [TestMethod]
        public void TestWriteSizeCheck()
        {
            byte[] empty = Array.Empty<byte>();
            byte[] random1 = new byte[128];
            byte[] random2 = new byte[3072];
            byte[] random3 = new byte[262144];
            Random gen = new Random();
            gen.NextBytes(random1);
            gen.NextBytes(random2);
            gen.NextBytes(random3);

            using var write = new PacketBuffer();
            write.WriteByteArray(empty, empty.Length);
            write.WriteByteArray(random2, random2.Length);
            write.WriteShortByteArray(random1);
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => write.WriteShortByteArray(random2));
            write.WriteMediumByteArray(random2);
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => write.WriteMediumByteArray(random3));
        }

        [TestMethod]
        public void TestReadonlyCheck()
        {
            byte[] bytes = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
            string text = "Hello World!";

            using var write = new PacketBuffer();
            write.WriteUInt16(0x3f7a);
            write.WriteDateTime(DateTime.Now);

            using var read = new PacketBuffer(write.GetBuffer());
            Assert.ThrowsException<InvalidOperationException>(() => read.WriteBoolean(true));
            Assert.ThrowsException<InvalidOperationException>(() => read.WriteByte(0x7f));
            Assert.ThrowsException<InvalidOperationException>(() => read.WriteUInt16(0x900a));
            Assert.ThrowsException<InvalidOperationException>(() => read.WriteInt32(0x6502a14c));
            Assert.ThrowsException<InvalidOperationException>(() => read.WriteInt64(0x1a41a174a64c91aa));
            Assert.ThrowsException<InvalidOperationException>(() => read.WriteDateTime(default));
            Assert.ThrowsException<InvalidOperationException>(() => read.WriteUuid(default));
            Assert.ThrowsException<InvalidOperationException>(() => read.WriteByteArray(bytes, bytes.Length));
            Assert.ThrowsException<InvalidOperationException>(() => read.WriteShortByteArray(bytes));
            Assert.ThrowsException<InvalidOperationException>(() => read.WriteMediumByteArray(bytes));
            Assert.ThrowsException<InvalidOperationException>(() => read.WriteShortString(text));
            Assert.ThrowsException<InvalidOperationException>(() => read.WriteMediumString(text));
        }

        [TestMethod]
        public void TestWriteExpansion()
        {
            using var buffer = new PacketBuffer(8);
            buffer.WriteInt32(-1);
            buffer.WriteBoolean(false);
            buffer.WriteByte(0xf3);
            buffer.WriteMediumByteArray(stackalloc byte[10]);
        }

        [TestMethod]
        public void TestPrimitives()
        {
            using var write = new PacketBuffer();
            write.WriteBoolean(true);
            write.WriteByte(0x7f);
            write.WriteUInt16(0x900a);
            write.WriteInt32(0x6502a14c);
            write.WriteInt64(0x1a41a174a64c91aa);

            using var read = new PacketBuffer(write.GetBuffer());
            Assert.AreEqual(true, read.ReadBoolean());
            Assert.AreEqual(0x7f, read.ReadByte());
            Assert.AreEqual(0x900a, read.ReadUInt16());
            Assert.AreEqual(0x6502a14c, read.ReadInt32());
            Assert.AreEqual(0x1a41a174a64c91aa, read.ReadInt64());
        }

        [TestMethod]
        public void TestStructs()
        {
            DateTime date1 = default;
            DateTime date2 = DateTime.Now;
            Guid uuid1 = default;
            Guid uuid2 = Guid.NewGuid();

            using var write = new PacketBuffer();
            write.WriteDateTime(date1);
            write.WriteDateTime(date2);
            write.WriteUuid(uuid1);
            write.WriteUuid(uuid2);

            using var read = new PacketBuffer(write.GetBuffer());
            Assert.AreEqual(date1, read.ReadDateTime());
            Assert.AreEqual(date2, read.ReadDateTime());
            Assert.AreEqual(uuid1, read.ReadUuid());
            Assert.AreEqual(uuid2, read.ReadUuid());
        }

        [TestMethod]
        public void TestArrays()
        {
            byte[] empty = Array.Empty<byte>();
            byte[] random1 = new byte[128];
            byte[] random2 = new byte[3072];
            byte[] random3 = new byte[262144];
            Random gen = new Random();
            gen.NextBytes(random1);
            gen.NextBytes(random2);
            gen.NextBytes(random3);

            using var write = new PacketBuffer();
            write.WriteByteArray(empty, empty.Length);
            write.WriteByteArray(random1, random1.Length);
            write.WriteShortByteArray(random1);
            write.WriteMediumByteArray(random2);
            write.WriteMediumByteArray(random2);
            write.WriteByteArray(random3, random3.Length);

            using var read = new PacketBuffer(write.GetBuffer());
            MemoryAssert.AreEqual(empty, read.ReadByteArray(0));
            MemoryAssert.AreEqual(random1, read.ReadByteArray(random1.Length));
            MemoryAssert.AreEqual(random1, read.ReadShortByteArray());
            MemoryAssert.AreEqual(random2, read.ReadMediumByteArray());
            PoolableMemory memory1 = read.ReadMediumPooledArray();
            PoolableMemory memory2 = read.ReadPooledArray(random3.Length);
            MemoryAssert.AreEqual(random2, memory1.Memory);
            MemoryAssert.AreEqual(random3, memory2.Memory);
            memory1.Return(false);
            memory2.Return(false);
        }

        [TestMethod]
        public void TestStrings()
        {
            string string1 = "This is a string 🥳";
            string string2 = @"Lorem ipsum dolor sit amet, consectetur adipiscing elit,
sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.
Dolor sed viverra ipsum nunc aliquet bibendum enim. In massa tempor nec feugiat.
Nunc aliquet bibendum enim facilisis gravida. Nisl nunc mi ipsum faucibus vitae aliquet nec ullamcorper.
Amet luctus venenatis lectus magna fringilla. Volutpat maecenas volutpat blandit aliquam etiam erat velit scelerisque in.
Egestas egestas fringilla phasellus faucibus scelerisque eleifend. Sagittis orci a scelerisque purus semper eget duis.
Nulla pharetra diam sit amet nisl suscipit. Sed adipiscing diam donec adipiscing tristique risus nec feugiat in.
Fusce ut placerat orci nulla. Pharetra vel turpis nunc eget lorem dolor. Tristique senectus et netus et malesuada.

Etiam tempor orci eu lobortis elementum nibh tellus molestie. Neque egestas congue quisque egestas.
Egestas integer eget aliquet nibh praesent tristique.Vulputate mi sit amet mauris.Sodales neque sodales ut etiam sit.
Dignissim suspendisse in est ante in. Volutpat commodo sed egestas egestas.Felis donec et odio pellentesque diam.
Pharetra vel turpis nunc eget lorem dolor sed viverra.Porta nibh venenatis cras sed felis eget.
Aliquam ultrices sagittis orci a. Dignissim diam quis enim lobortis. Aliquet porttitor lacus luctus accumsan.
Dignissim convallis aenean et tortor at risus viverra adipiscing at.";
            string string3 = new string('A', 262144);

            using var write = new PacketBuffer();
            write.WriteShortString(string1);
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => write.WriteShortString(string2));
            write.WriteMediumString(string2);
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => write.WriteMediumString(string3));

            using var read = new PacketBuffer(write.GetBuffer());
            Assert.AreEqual(string1, read.ReadShortString());
            Assert.AreEqual(string2, read.ReadMediumString());
        }

        [TestMethod]
        public void TestNullWrites()
        {
            using var write = new PacketBuffer();
            write.WriteByteArray(null, 32);
            write.WriteShortString(null);
            write.WriteMediumString(null);

            using var read = new PacketBuffer(write.GetBuffer());
            ReadOnlySpan<byte> zeros = stackalloc byte[32];
            MemoryAssert.AreEqual(zeros, read.ReadByteArray(32));
            Assert.AreEqual(string.Empty, read.ReadShortString());
            Assert.AreEqual(string.Empty, read.ReadMediumString());
        }
    }
}
