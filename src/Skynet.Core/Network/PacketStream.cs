using System;
using System.Buffers;
using System.Buffers.Binary;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Skynet.Network
{
    /// <summary>
    /// Provides a reading and writing wrapper to reassemble packets with their ID from a segmented stream.
    /// </summary>
    internal class PacketStream : IAsyncDisposable
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
        public async ValueTask<(bool success, byte id, ReadOnlyMemory<byte> buffer)> ReadAsync(CancellationToken ct = default)
        {
            if (disposedValue) throw new ObjectDisposedException(nameof(PacketStream));

            byte[] buffer = new byte[4];
            if (!await ReadInternal(buffer, ct).ConfigureAwait(false))
                return default;
            uint packetMeta = BitConverter.ToUInt32(buffer);
            if (!BitConverter.IsLittleEndian)
                packetMeta = BinaryPrimitives.ReverseEndianness(packetMeta);

            byte id = (byte)(packetMeta & 0x000000FF);
            int length = (int)(packetMeta >> 8);

            buffer = new byte[length];
            if (!await ReadInternal(buffer, ct).ConfigureAwait(false))
                return default;

            return (true, id, buffer);
        }

        /// <summary>
        /// Writes a packet to the underlying stream.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The packet payload is larger than 0x00ffffff bytes.</exception>
        /// <exception cref="IOException">Failed to write on the underlying stream.</exception>
        /// <exception cref="ObjectDisposedException">The <see cref="PacketStream"/> has been disposed.</exception>
        public async ValueTask WriteAsync(byte id, ReadOnlyMemory<byte> buffer, CancellationToken ct = default)
        {
            if (disposedValue) throw new ObjectDisposedException(nameof(PacketStream));
            if (buffer.Length > 0x00ffffff) throw new ArgumentOutOfRangeException(nameof(buffer), "The packet payload is too large");

            int writeBufferLength = sizeof(int) + buffer.Length;
            byte[] cached = ArrayPool<byte>.Shared.Rent(writeBufferLength);
            Memory<byte> writeBuffer = new Memory<byte>(cached, 0, writeBufferLength);

            uint packetMeta = (uint)buffer.Length << 8 | id;
            if (!BitConverter.IsLittleEndian)
                packetMeta = BinaryPrimitives.ReverseEndianness(packetMeta);

            BitConverter.TryWriteBytes(cached, packetMeta);

            buffer.CopyTo(writeBuffer.Slice(sizeof(int)));

            await innerStream.WriteAsync(writeBuffer, ct).ConfigureAwait(false);

            ArrayPool<byte>.Shared.Return(cached);
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
