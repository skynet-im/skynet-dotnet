using System;
using System.Buffers;
using System.Buffers.Binary;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Skynet.Network
{
    /// <summary>
    /// Reads and writes primitive data types in a little endian binary format.
    /// </summary>
    public sealed class PacketBuffer : IDisposable
    {
        private const int DefaultCapacity = 256;
        private static readonly Encoding encoding =
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

        private byte[]? rented;
        private Memory<byte> buffer;
        private int position;
        private bool disposed;

        public PacketBuffer() : this(DefaultCapacity) { }

        public PacketBuffer(int capacity)
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));

            Allocate(capacity);
            CanWrite = true;
        }

        public PacketBuffer(ReadOnlyMemory<byte> buffer)
        {
            this.buffer = MemoryMarshal.AsMemory(buffer);
            CanWrite = false;
        }

        public PacketBuffer(Memory<byte> buffer)
        {
            this.buffer = buffer;
            CanWrite = true;
        }

        public bool CanWrite { get; }

        public int Capacity
        {
            get
            {
                if (disposed) throw new ObjectDisposedException(nameof(PacketBuffer));
                return buffer.Length;
            }
        }

        public int Position
        {
            get
            {
                if (disposed) throw new ObjectDisposedException(nameof(PacketBuffer));
                return position;
            }

            set
            {
                if (disposed) throw new ObjectDisposedException(nameof(PacketBuffer));
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
                EnsureCapacity(value);
                position = value;
            }
        }

        public ReadOnlyMemory<byte> GetBuffer()
        {
            if (disposed) throw new ObjectDisposedException(nameof(PacketBuffer));

            return buffer.Slice(0, Position);
        }

        public Memory<byte> GetInternalBuffer()
        {
            if (disposed) throw new ObjectDisposedException(nameof(PacketBuffer));

            return buffer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PrepareRead(int length)
        {
            if (disposed) throw new ObjectDisposedException(nameof(PacketBuffer));
            if (Position + length > Capacity) throw new EndOfStreamException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PrepareWrite(int length)
        {
            if (disposed) throw new ObjectDisposedException(nameof(PacketBuffer));
            if (!CanWrite) throw new InvalidOperationException("Attempted to write on a readonly PacketBuffer");

            EnsureCapacity(Position + length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureCapacity(int length)
        {
            if (length > Capacity)
            {
                int size = Math.Max(buffer.Length * 2, Position + length);
                Allocate(size);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Allocate(int length)
        {
            if (length > 1024)
            {
                // ArrayPool is faster for large arrays: https://adamsitnik.com/Array-Pool/
                rented = ArrayPool<byte>.Shared.Rent(length);
                buffer.CopyTo(rented);
                buffer = rented;
            }
            else if (length > 0)
            {
                byte[] newBuffer = new byte[length];
                buffer.CopyTo(newBuffer);
                buffer = newBuffer;
            }
            else
            {
                buffer = Memory<byte>.Empty;
            }
        }

        public ReadOnlyMemory<byte> ReadRawByteArray(int count)
        {
            PrepareRead(count);
            ReadOnlyMemory<byte> value = buffer.Slice(Position, count);
            position += count;
            return value;
        }
        public void WriteRawByteArray(ReadOnlySpan<byte> array)
        {
            PrepareWrite(array.Length);
            array.CopyTo(buffer.Slice(Position).Span);
            position += array.Length;
        }

        #region primitives
        public bool ReadBoolean()
        {
            PrepareRead(sizeof(byte));
            bool value = buffer.Span[Position] != 0;
            position++;
            return value;
        }
        public void WriteBoolean(bool value)
        {
            PrepareWrite(sizeof(byte));
            buffer.Span[Position] = value ? (byte)1 : (byte)0;
            position++;
        }

        public byte ReadByte()
        {
            PrepareRead(sizeof(byte));
            byte value = buffer.Span[Position];
            position += sizeof(byte);
            return value;
        }
        public void WriteByte(byte value)
        {
            PrepareWrite(sizeof(byte));
            buffer.Span[Position] = value;
            position += sizeof(byte);
        }

        public ushort ReadUInt16()
        {
            PrepareRead(sizeof(ushort));
            ushort value = Unsafe.ReadUnaligned<ushort>(ref buffer.Span[Position]);
            if (!BitConverter.IsLittleEndian)
                value = BinaryPrimitives.ReverseEndianness(value);
            position += sizeof(ushort);
            return value;
        }
        public void WriteUInt16(ushort value)
        {
            PrepareWrite(sizeof(ushort));
            if (!BitConverter.IsLittleEndian)
                value = BinaryPrimitives.ReverseEndianness(value);
            Unsafe.WriteUnaligned(ref buffer.Span[Position], value);
            position += sizeof(ushort);
        }

        public int ReadInt32()
        {
            PrepareRead(sizeof(int));
            int value = Unsafe.ReadUnaligned<int>(ref buffer.Span[Position]);
            if (!BitConverter.IsLittleEndian)
                value = BinaryPrimitives.ReverseEndianness(value);
            position += sizeof(int);
            return value;
        }
        public void WriteInt32(int value)
        {
            PrepareWrite(sizeof(int));
            if (!BitConverter.IsLittleEndian)
                value = BinaryPrimitives.ReverseEndianness(value);
            Unsafe.WriteUnaligned(ref buffer.Span[Position], value);
            position += sizeof(int);
        }

        public long ReadInt64()
        {
            PrepareRead(sizeof(long));
            long value = Unsafe.ReadUnaligned<long>(ref buffer.Span[Position]);
            if (!BitConverter.IsLittleEndian)
                value = BinaryPrimitives.ReverseEndianness(value);
            position += sizeof(long);
            return value;
        }
        public void WriteInt64(long value)
        {
            PrepareWrite(sizeof(long));
            if (!BitConverter.IsLittleEndian)
                value = BinaryPrimitives.ReverseEndianness(value);
            Unsafe.WriteUnaligned(ref buffer.Span[Position], value);
            position += sizeof(long);
        }
        #endregion
        #region structs
        public DateTime ReadDateTime()
        {
            return DateTime.FromBinary(ReadInt64());
        }
        public void WriteDateTime(DateTime value)
        {
            WriteInt64(value.ToBinary());
        }

        /// <summary>
        /// Reads an universally unique identifier in big endian format.
        /// </summary>
        public Guid ReadUuid()
        {
            ReadOnlySpan<byte> uuid = ReadRawByteArray(16).Span;
            Span<byte> guid = stackalloc byte[16];

            guid[15] = uuid[15]; // hoist bounds checks

            guid[00] = uuid[03];
            guid[01] = uuid[02];
            guid[02] = uuid[01];
            guid[03] = uuid[00];

            guid[04] = uuid[05];
            guid[05] = uuid[04];

            guid[06] = uuid[07];
            guid[07] = uuid[06];

            guid[08] = uuid[08];
            guid[09] = uuid[09];
            guid[10] = uuid[10];
            guid[11] = uuid[11];
            guid[12] = uuid[12];
            guid[13] = uuid[13];
            guid[14] = uuid[14];

            return new Guid(guid);
        }

        /// <summary>
        /// Writes an universally unique identifier in big endian format.
        /// </summary>
        public void WriteUuid(Guid value)
        {
            Span<byte> guid = stackalloc byte[16];
            value.TryWriteBytes(guid);
            Span<byte> uuid = stackalloc byte[16];

            uuid[15] = guid[15]; // hoist bounds checks

            uuid[00] = guid[03];
            uuid[01] = guid[02];
            uuid[02] = guid[01];
            uuid[03] = guid[00];

            uuid[04] = guid[05];
            uuid[05] = guid[04];

            uuid[06] = guid[07];
            uuid[07] = guid[06];

            uuid[08] = guid[08];
            uuid[09] = guid[09];
            uuid[10] = guid[10];
            uuid[11] = guid[11];
            uuid[12] = guid[12];
            uuid[13] = guid[13];
            uuid[14] = guid[14];

            WriteRawByteArray(uuid);
        }

        #endregion

        #region arrays
        public ReadOnlyMemory<byte> ReadShortByteArray()
        {
            PrepareRead(sizeof(byte));
            int length = buffer.Span[Position];
            PrepareRead(sizeof(byte) + length);
            ReadOnlyMemory<byte> value = buffer.Slice(Position + sizeof(byte), length);
            position += sizeof(byte) + length;
            return value;
        }
        public void WriteShortByteArray(ReadOnlySpan<byte> array)
        {
            if (array.Length > byte.MaxValue) throw new ArgumentOutOfRangeException(nameof(array));

            PrepareWrite(sizeof(byte) + array.Length);
            buffer.Span[Position] = (byte)array.Length;
            array.CopyTo(buffer.Span.Slice(Position + sizeof(byte), array.Length));
            position += sizeof(byte) + array.Length;
        }

        public ReadOnlyMemory<byte> ReadMediumByteArray()
        {
            PrepareRead(sizeof(ushort));
            ushort length = Unsafe.ReadUnaligned<ushort>(ref buffer.Span[Position]);
            if (!BitConverter.IsLittleEndian)
                length = BinaryPrimitives.ReverseEndianness(length);
            PrepareRead(sizeof(ushort) + length);
            ReadOnlyMemory<byte> value = buffer.Slice(Position + sizeof(ushort), length);
            position += sizeof(ushort) + length;
            return value;
        }
        public void WriteMediumByteArray(ReadOnlySpan<byte> array)
        {
            if (array.Length > ushort.MaxValue) throw new ArgumentOutOfRangeException(nameof(array));

            PrepareWrite(sizeof(ushort) + array.Length);
            ushort length = (ushort)array.Length;
            if (!BitConverter.IsLittleEndian)
                length = BinaryPrimitives.ReverseEndianness(length);
            Unsafe.WriteUnaligned(ref buffer.Span[Position], length);
            array.CopyTo(buffer.Span.Slice(Position + sizeof(ushort), array.Length));
            position += sizeof(ushort) + array.Length;
        }

        public ReadOnlyMemory<byte> ReadLongByteArray()
        {
            PrepareRead(sizeof(int));
            int length = Unsafe.ReadUnaligned<int>(ref buffer.Span[Position]);
            if (!BitConverter.IsLittleEndian)
                length = BinaryPrimitives.ReverseEndianness(length);
            if (length < 0) throw new InvalidDataException();
            PrepareRead(sizeof(int) + length);
            ReadOnlyMemory<byte> value = buffer.Slice(Position + sizeof(int), length);
            position += sizeof(int) + length;
            return value;
        }
        public void WriteLongByteArray(ReadOnlySpan<byte> array)
        {
            if (array.Length > int.MaxValue) throw new ArgumentOutOfRangeException(nameof(array));

            PrepareWrite(sizeof(int) + array.Length);
            int length = array.Length;
            if (!BitConverter.IsLittleEndian)
                length = BinaryPrimitives.ReverseEndianness(length);
            Unsafe.WriteUnaligned(ref buffer.Span[Position], length);
            array.CopyTo(buffer.Span.Slice(Position + sizeof(int)));
            position += sizeof(int) + array.Length;
        }
        #endregion
        #region strings
        public string ReadShortString()
        {
            ReadOnlySpan<byte> buffer = ReadShortByteArray().Span;
            return encoding.GetString(buffer);
        }
        public void WriteShortString(ReadOnlySpan<char> text)
        {
            Span<byte> buffer = stackalloc byte[byte.MaxValue];
            encoding.GetEncoder().Convert(text, buffer, true, out _, out int bytesUsed, out bool completed);
            if (!completed) throw new ArgumentOutOfRangeException(nameof(text));
            WriteShortByteArray(buffer.Slice(0, bytesUsed));
        }

        public string ReadMediumString()
        {
            ReadOnlySpan<byte> buffer = ReadMediumByteArray().Span;
            return encoding.GetString(buffer);
        }
        public void WriteMediumString(ReadOnlySpan<char> text)
        {
            Span<byte> buffer = stackalloc byte[ushort.MaxValue];
            encoding.GetEncoder().Convert(text, buffer, true, out _, out int bytesUsed, out bool completed);
            if (!completed) throw new ArgumentOutOfRangeException(nameof(text));
            WriteMediumByteArray(buffer.Slice(0, bytesUsed));
        }

        public string ReadLongString()
        {
            ReadOnlySpan<byte> buffer = ReadLongByteArray().Span;
            return encoding.GetString(buffer);
        }
        public void WriteLongString(ReadOnlySpan<char> text)
        {
            int length = encoding.GetEncoder().GetByteCount(text, true);
            if (length > int.MaxValue) throw new ArgumentOutOfRangeException(nameof(text));
            Span<byte> buffer = length <= ushort.MaxValue ? stackalloc byte[length] : new byte[length];
            encoding.GetEncoder().Convert(text, buffer, true, out _, out int bytesUsed, out bool completed);
            WriteLongByteArray(buffer.Slice(0, bytesUsed));
        }
        #endregion

        public void Dispose()
        {
            if (!disposed)
            {
                if (rented != null)
                {
                    ArrayPool<byte>.Shared.Return(rented);
                    rented = null;
                }

                buffer = default;

                disposed = true;
            }
        }
    }
}
