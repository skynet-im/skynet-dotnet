using Microsoft.VisualStudio.TestTools.UnitTesting;
using Skynet.Protocol.Cryptography;
using Skynet.Tests;
using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace Skynet.Protocol.Tests.Cryptography
{
    [TestClass]
    public class AesStaticTests
    {
        private const string Empty = "";
        private const string HelloWorld = "Hello World!";
        private const string LoremIpsum = @"Lorem ipsum dolor sit amet, consectetur adipiscing elit,
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

        [TestMethod]
        public void TestNullKeys()
        {
            byte[] buffer = new byte[64];
            byte[] hmacKey = new byte[32];
            byte[] aesKey = new byte[32];

            Assert.ThrowsException<ArgumentNullException>(() => AesStatic.EncryptWithHmac(buffer, null!, aesKey));
            Assert.ThrowsException<ArgumentNullException>(() => AesStatic.EncryptWithHmac(buffer, hmacKey, null!));
            Assert.ThrowsException<ArgumentNullException>(() => AesStatic.DecryptWithHmac(buffer, null!, aesKey));
            Assert.ThrowsException<ArgumentNullException>(() => AesStatic.DecryptWithHmac(buffer, hmacKey, null!));
        }

        [TestMethod]
        public void TestIllegalKeySize()
        {
            byte[] buffer = new byte[64];
            byte[] rightKey = new byte[32];
            byte[] wrongKey = new byte[16];

            Assert.ThrowsException<ArgumentException>(() => AesStatic.EncryptWithHmac(buffer, wrongKey, rightKey));
            Assert.ThrowsException<ArgumentException>(() => AesStatic.EncryptWithHmac(buffer, rightKey, wrongKey));
            Assert.ThrowsException<ArgumentException>(() => AesStatic.DecryptWithHmac(buffer, wrongKey, rightKey));
            Assert.ThrowsException<ArgumentException>(() => AesStatic.DecryptWithHmac(buffer, rightKey, wrongKey));
        }

        [TestMethod]
        public void TestInsufficientData()
        {
            byte[] buffer = new byte[16];
            byte[] hmacKey = new byte[32];
            byte[] aesKey = new byte[32];

            Assert.ThrowsException<ArgumentException>(() => AesStatic.DecryptWithHmac(buffer, hmacKey, aesKey));
        }

        [DataTestMethod]
        [DataRow(Empty)]
        [DataRow(HelloWorld)]
        [DataRow(LoremIpsum)]
        public void TestEncryptDecrypt(string text)
        {
            byte[] hmacKey = new byte[32];
            byte[] aesKey = new byte[32];
            RandomNumberGenerator.Fill(hmacKey);
            RandomNumberGenerator.Fill(aesKey);

            ReadOnlyMemory<byte> data = Encoding.UTF8.GetBytes(text);
            ReadOnlyMemory<byte> ciphertext = AesStatic.EncryptWithHmac(data, hmacKey, aesKey);
            ReadOnlyMemory<byte> plaintext = AesStatic.DecryptWithHmac(ciphertext, hmacKey, aesKey);
            MemoryAssert.AreEqual(data, plaintext);
        }

        [DataTestMethod]
        [DataRow(HelloWorld, 13)] // tamper HMAC
        [DataRow(HelloWorld, 35)] // tamper IV
        [DataRow(HelloWorld, 55)] // tamper ciphertext
        [DataRow(LoremIpsum, 13)] // tamper HMAC
        [DataRow(LoremIpsum, 35)] // tamper IV
        [DataRow(LoremIpsum, 55)] // tamper first block
        [DataRow(LoremIpsum, 93)] // tamper later block
        public void TestIntegrityCheck(string text, int tamperIndex)
        {
            byte[] hmacKey = new byte[32];
            byte[] aesKey = new byte[32];
            RandomNumberGenerator.Fill(hmacKey);
            RandomNumberGenerator.Fill(aesKey);

            ReadOnlyMemory<byte> data = Encoding.UTF8.GetBytes(text);
            Memory<byte> ciphertext = MemoryMarshal.AsMemory(AesStatic.EncryptWithHmac(data, hmacKey, aesKey));
            ciphertext.Span[tamperIndex] = (byte)~ciphertext.Span[tamperIndex];

            Assert.ThrowsException<CryptographicException>(() => AesStatic.DecryptWithHmac(ciphertext, hmacKey, aesKey));
        }
    }
}
