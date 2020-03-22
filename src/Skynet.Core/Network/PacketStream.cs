﻿using System;
using System.Buffers.Binary;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Skynet.Network
{
    /// <summary>
    /// Provides a reading and writing wrapper to reassemble packets with their ID from a segmented stream.
    /// </summary>
    public sealed class PacketStream : IAsyncDisposable
    {
        private readonly Stream innerStream;
        private readonly bool leaveInnerStreamOpen;

        public PacketStream(Stream innerStream, bool leaveInnerStreamOpen)
        {
            this.innerStream = innerStream;
            this.leaveInnerStreamOpen = leaveInnerStreamOpen;
        }

        /// <summary>
        /// Reads an entire packet from the underlying stream.
        /// </summary>
        /// <exception cref="IOException">Failed to read from the underlying stream.</exception>
        /// <exception cref="ObjectDisposedException">The <see cref="PacketStream"/> has been disposed.</exception>
        public async ValueTask<(byte id, PacketBuffer buffer)> ReadAsync(CancellationToken ct = default)
        {
            if (disposedValue) throw new ObjectDisposedException(nameof(PacketStream));

            byte[] metaBuffer = new byte[4];
            if (!await ReadInternal(metaBuffer, ct).ConfigureAwait(false))
                return default;
            uint packetMeta = BitConverter.ToUInt32(metaBuffer);
            if (!BitConverter.IsLittleEndian)
                packetMeta = BinaryPrimitives.ReverseEndianness(packetMeta);

            byte id = (byte)(packetMeta & 0x000000FF);
            int length = (int)(packetMeta >> 8);

            // Use PacketBuffer with ArrayPool instead of allocating directly
            var packetBuffer = new PacketBuffer(length);
            Memory<byte> contentBuffer = packetBuffer.GetInternalBuffer();
            bool success = false;
            try
            {
                if (!(success = await ReadInternal(contentBuffer, ct).ConfigureAwait(false)))
                    return default;

                packetBuffer.Position = length;

                return (id, packetBuffer);
            }
            finally
            {
                if (!success) packetBuffer.Dispose();
            }
        }

        /// <summary>
        /// Writes a packet to the underlying stream.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The packet payload is larger than 0x00ffffff bytes.</exception>
        /// <exception cref="IOException">Failed to write on the underlying stream.</exception>
        /// <exception cref="ObjectDisposedException">The <see cref="PacketStream"/> has been disposed.</exception>
        public async ValueTask WriteAsync(byte id, Action<PacketBuffer> writeDataCallback, CancellationToken ct = default)
        {
            if (disposedValue) throw new ObjectDisposedException(nameof(PacketStream));
            if (writeDataCallback == null) throw new ArgumentNullException(nameof(writeDataCallback));

            var packetBuffer = new PacketBuffer();
            packetBuffer.WriteInt32(default);
            writeDataCallback(packetBuffer);
            int contentLength = packetBuffer.Position - sizeof(int);
            if (contentLength > 0x00ffffff)
                throw new ArgumentOutOfRangeException(nameof(writeDataCallback), "The packet payload is too large");

            int packetMeta = unchecked((contentLength << 8) | id);
            if (!BitConverter.IsLittleEndian)
                packetMeta = BinaryPrimitives.ReverseEndianness(packetMeta);

            packetBuffer.Position = 0;
            packetBuffer.WriteInt32(packetMeta);
            packetBuffer.Position = sizeof(int) + contentLength;

            await innerStream.WriteAsync(packetBuffer.GetBuffer(), ct).ConfigureAwait(false);
        }

        private async ValueTask<bool> ReadInternal(Memory<byte> buffer, CancellationToken ct)
        {
            if (buffer.Length == 0)
                return true;

            int read = 0;
            do
            {
                int length = await innerStream.ReadAsync(buffer.Slice(read), ct).ConfigureAwait(false);
                if (length == 0)
                    return false;
                read += length;
            } while (read < buffer.Length);

            return true;
        }

        #region IAsyncDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        public async ValueTask DisposeAsync()
        {
            if (!disposedValue)
            {
                if (!leaveInnerStreamOpen)
                    await innerStream.DisposeAsync().ConfigureAwait(false);

                disposedValue = true;
            }
        }
        #endregion
    }
}
