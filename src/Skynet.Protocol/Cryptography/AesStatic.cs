using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Skynet.Protocol.Cryptography
{
    public static class AesStatic
    {
        private static readonly Aes aes = Aes.Create();

        public static ReadOnlyMemory<byte> EncryptWithHmac(ReadOnlyMemory<byte> plaintext, byte[] hmacKey, byte[] aesKey)
        {
            if (hmacKey == null) throw new ArgumentNullException(nameof(hmacKey));
            if (aesKey == null) throw new ArgumentNullException(nameof(aesKey));
            if (hmacKey.Length != 32) throw new ArgumentException("The HMAC key must be exactly 32 bytes long.", nameof(hmacKey));
            if (aesKey.Length != 32) throw new ArgumentException("The AES key must be exaclty 32 bytes long.", nameof(aesKey));

            byte[] iv = new byte[16];
            RandomNumberGenerator.Fill(iv);
            Memory<byte> ciphertext;

            using (ICryptoTransform transform = aes.CreateEncryptor(aesKey, iv))
            {
                if (!MemoryMarshal.TryGetArray(plaintext, out ArraySegment<byte> plainSegment))
                    plainSegment = plaintext.ToArray();

                ciphertext = ProcessData(plainSegment, transform, 32 + 16);
            }

            iv.AsSpan().CopyTo(ciphertext.Span.Slice(32));

            using (var hmac = new HMACSHA256(hmacKey))
            {
                if (!hmac.TryComputeHash(ciphertext.Span.Slice(32), ciphertext.Span.Slice(0, 32), out _))
                    throw new CryptographicException("HMAC calculation failed for unknown reason");
            }

            return ciphertext;
        }

        public static ReadOnlyMemory<byte> DecryptWithHmac(ReadOnlyMemory<byte> ciphertext, byte[] hmacKey, byte[] aesKey)
        {
            if (hmacKey == null) throw new ArgumentNullException(nameof(hmacKey));
            if (aesKey == null) throw new ArgumentNullException(nameof(aesKey));
            if (ciphertext.Length < 64) throw new ArgumentException("The ciphertext must be at least 64 bytes long.", nameof(ciphertext));
            if (hmacKey.Length != 32) throw new ArgumentException("The HMAC key must be exactly 32 bytes long.", nameof(hmacKey));
            if (aesKey.Length != 32) throw new ArgumentException("The AES key must be exaclty 32 bytes long.", nameof(aesKey));

            using (var hmac = new HMACSHA256(hmacKey))
            {
                Span<byte> actualHash = stackalloc byte[32];
                if (!hmac.TryComputeHash(ciphertext.Span.Slice(32), actualHash, out _))
                    throw new CryptographicException("HMAC calculation failed for unknown reason");

                if (!actualHash.SequenceEqual(ciphertext.Span.Slice(0, 32)))
                    throw new CryptographicException("Message Corrupted: The HMAC values are not equal. The encrypted block may be tampered.");
            }

            byte[] iv = ciphertext.Slice(32, 16).ToArray();

            ReadOnlyMemory<byte> encrypted = ciphertext.Slice(48);
            if (!MemoryMarshal.TryGetArray(encrypted, out ArraySegment<byte> cipherSegment))
                cipherSegment = encrypted.ToArray();

            using ICryptoTransform transform = aes.CreateDecryptor(aesKey, iv);
            return ProcessData(cipherSegment, transform, 0);
        }

        private static ArraySegment<byte> ProcessData(ArraySegment<byte> buffer, ICryptoTransform transform, int outputIndex)
        {
            if (buffer.Count > 16)
            {
                int first = buffer.Count - buffer.Count % 16;
                byte[] output = new byte[outputIndex + first + 16];
                int outlen = transform.TransformBlock(buffer.Array, buffer.Offset, first, output, outputIndex);
                byte[] last = transform.TransformFinalBlock(buffer.Array, buffer.Offset + first, buffer.Count - first);
                Array.Copy(last, 0, output, outputIndex + outlen, last.Length);
                outlen += last.Length;
                return new ArraySegment<byte>(output, 0, outputIndex + outlen);
            }
            else
            {
                byte[] last = transform.TransformFinalBlock(buffer.Array, buffer.Offset, buffer.Count);
                if (outputIndex == 0)
                    return last;

                byte[] output = new byte[outputIndex + last.Length];
                Array.Copy(last, 0, output, outputIndex, last.Length);
                return output;
            }
        }
    }
}
