using System;
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

        private readonly bool clearBuffer;

        private byte[]? rented;
        private Memory<byte> buffer;
        private int position;
        private bool disposed;

        public PacketBuffer(bool clearBuffer = false) : this(DefaultCapacity, clearBuffer) { }

        public PacketBuffer(int capacity, bool clearBuffer = false)
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));

            Allocate(capacity);
            CanWrite = true;
            this.clearBuffer = clearBuffer;
        }

        public PacketBuffer(ReadOnlyMemory<byte> buffer, bool clearBuffer = false)
        {
            this.buffer = MemoryMarshal.AsMemory(buffer);
            CanWrite = false;
            this.clearBuffer = clearBuffer;
        }

        public PacketBuffer(Memory<byte> buffer, bool clearBuffer = false)
        {
            this.buffer = buffer;
            CanWrite = true;
            this.clearBuffer = clearBuffer;
        }

        public PacketBuffer(PoolableMemory buffer, bool writeable, bool clearBuffer = false)
        {
            this.buffer = buffer.Memory;
            rented = buffer.RentedBuffer;
            CanWrite = writeable;
            this.clearBuffer = clearBuffer;
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

            return buffer.Slice(0, position);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PrepareRead(int length)
        {
            if (disposed) throw new ObjectDisposedException(nameof(PacketBuffer));
            if (position + length > Capacity) throw new EndOfStreamException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PrepareWrite(int length)
        {
            if (disposed) throw new ObjectDisposedException(nameof(PacketBuffer));
            if (!CanWrite) throw new InvalidOperationException("Attempted to write on a readonly PacketBuffer");

            EnsureCapacity(position + length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureCapacity(int length)
        {
            if (length > Capacity)
            {
                int size = Math.Max(buffer.Length * 2, position + length);
                Allocate(size);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Allocate(int length)
        {
            if (length > 0)
            {
                PoolableMemory old = new PoolableMemory(buffer, rented);
                PoolableMemory memory = PoolableMemory.Allocate(length, false);
                old.Memory.CopyTo(memory.Memory);
                old.Return(clearBuffer);
                buffer = memory.Memory;
                rented = memory.RentedBuffer;
            }
            else
            {
                buffer = Memory<byte>.Empty;
            }
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
            buffer.Span[position] = value ? (byte)1 : (byte)0;
            position++;
        }

        public byte ReadByte()
        {
            PrepareRead(sizeof(byte));
            byte value = buffer.Span[position];
            position += sizeof(byte);
            return value;
        }
        public void WriteByte(byte value)
        {
            PrepareWrite(sizeof(byte));
            buffer.Span[position] = value;
            position += sizeof(byte);
        }

        public ushort ReadUInt16()
        {
            PrepareRead(sizeof(ushort));
            ushort value = Unsafe.ReadUnaligned<ushort>(ref buffer.Span[position]);
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
            Unsafe.WriteUnaligned(ref buffer.Span[position], value);
            position += sizeof(ushort);
        }

        public int ReadInt32()
        {
            PrepareRead(sizeof(int));
            int value = Unsafe.ReadUnaligned<int>(ref buffer.Span[position]);
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
            Unsafe.WriteUnaligned(ref buffer.Span[position], value);
            position += sizeof(int);
        }

        public long ReadInt64()
        {
            PrepareRead(sizeof(long));
            long value = Unsafe.ReadUnaligned<long>(ref buffer.Span[position]);
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
            Unsafe.WriteUnaligned(ref buffer.Span[position], value);
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
            ReadOnlySpan<byte> uuid = InternalReadArray(16).Span;
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

            WriteByteArray(uuid);
        }

        #endregion

        #region arrays
        public byte[] ReadByteArray(int length)
        {
            PrepareRead(length);
            byte[] value = new byte[length];
            buffer.Span.Slice(position, length).CopyTo(value);
            position += length;
            return value;
        }
        public PoolableMemory ReadPooledArray(int length)
        {
            PrepareRead(length);
            PoolableMemory value = PoolableMemory.Allocate(length, true);
            buffer.Slice(position, length).CopyTo(value.Memory);
            position += sizeof(ushort) + length;
            return value;
        }
        private ReadOnlyMemory<byte> InternalReadArray(int length)
        {
            PrepareRead(length);
            ReadOnlyMemory<byte> value = buffer.Slice(position, length);
            position += length;
            return value;
        }
        public void WriteByteArray(ReadOnlySpan<byte> buffer)
        {
            PrepareWrite(buffer.Length);
            buffer.CopyTo(this.buffer.Span.Slice(position));
            position += buffer.Length;
        }
        public byte[] ReadShortByteArray()
        {
            PrepareRead(sizeof(byte));
            int length = buffer.Span[position];
            PrepareRead(sizeof(byte) + length);
            byte[] value = new byte[length];
            buffer.Span.Slice(position + sizeof(byte), length).CopyTo(value);
            position += sizeof(byte) + length;
            return value;
        }
        private ReadOnlyMemory<byte> InternalReadShortArray()
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
            buffer.Span[position] = (byte)array.Length;
            array.CopyTo(buffer.Span.Slice(position + sizeof(byte), array.Length));
            position += sizeof(byte) + array.Length;
        }

        public byte[] ReadMediumByteArray()
        {
            PrepareRead(sizeof(ushort));
            ushort length = Unsafe.ReadUnaligned<ushort>(ref buffer.Span[position]);
            if (!BitConverter.IsLittleEndian)
                length = BinaryPrimitives.ReverseEndianness(length);
            PrepareRead(sizeof(ushort) + length);
            byte[] value = new byte[length];
            buffer.Span.Slice(position + sizeof(ushort), length).CopyTo(value);
            position += sizeof(ushort) + length;
            return value;
        }
        public PoolableMemory ReadMediumPooledArray()
        {
            PrepareRead(sizeof(ushort));
            ushort length = Unsafe.ReadUnaligned<ushort>(ref buffer.Span[position]);
            if (!BitConverter.IsLittleEndian)
                length = BinaryPrimitives.ReverseEndianness(length);
            PrepareRead(sizeof(ushort) + length);
            PoolableMemory value = PoolableMemory.Allocate(length, true);
            buffer.Slice(position + sizeof(ushort), length).CopyTo(value.Memory);
            position += sizeof(ushort) + length;
            return value;
        }
        private ReadOnlyMemory<byte> InternalReadMediumArray()
        {
            PrepareRead(sizeof(ushort));
            ushort length = Unsafe.ReadUnaligned<ushort>(ref buffer.Span[position]);
            if (!BitConverter.IsLittleEndian)
                length = BinaryPrimitives.ReverseEndianness(length);
            PrepareRead(sizeof(ushort) + length);
            ReadOnlyMemory<byte> value = buffer.Slice(position + sizeof(ushort), length);
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
            Unsafe.WriteUnaligned(ref buffer.Span[position], length);
            array.CopyTo(buffer.Span.Slice(position + sizeof(ushort), array.Length));
            position += sizeof(ushort) + array.Length;
        }
        #endregion
        #region strings
        public string ReadShortString()
        {
            ReadOnlySpan<byte> buffer = InternalReadShortArray().Span;
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
            ReadOnlySpan<byte> buffer = InternalReadMediumArray().Span;
            return encoding.GetString(buffer);
        }
        public void WriteMediumString(ReadOnlySpan<char> text)
        {
            Span<byte> buffer = stackalloc byte[ushort.MaxValue];
            encoding.GetEncoder().Convert(text, buffer, true, out _, out int bytesUsed, out bool completed);
            if (!completed) throw new ArgumentOutOfRangeException(nameof(text));
            WriteMediumByteArray(buffer.Slice(0, bytesUsed));
        }
        #endregion

        public void Dispose()
        {
            if (!disposed)
            {
                new PoolableMemory(buffer, rented).Return(clearBuffer);
                buffer = default;
                rented = null;

                disposed = true;
            }
        }

        public PoolableMemory GetBufferAndDispose()
        {
            if (disposed) throw new ObjectDisposedException(nameof(PacketBuffer));

            disposed = true;

            return new PoolableMemory(buffer.Slice(0, position), rented);
        }
    }
}
