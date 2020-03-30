using System;
using System.Buffers;
using System.Runtime.InteropServices;

namespace Skynet.Network
{
    public readonly struct PoolableMemory
    {
        // ArrayPool is faster for large arrays: https://adamsitnik.com/Array-Pool/

        private const int RentMinSize = 1024;

        public static PoolableMemory Allocate(int length, bool exactly)
        {
            if (length >= RentMinSize)
            {
                byte[] rented = ArrayPool<byte>.Shared.Rent(length);
                if (exactly)
                    return new PoolableMemory(new Memory<byte>(rented, 0, length), rented);
                else
                    return new PoolableMemory(rented, rented);
            }
            else
            {
                return new PoolableMemory(new byte[length], null);
            }
        }

        public PoolableMemory(Memory<byte> memory, byte[]? rentedBuffer = null)
        {
            Memory = memory;
            RentedBuffer = rentedBuffer;
        }

        public PoolableMemory(ReadOnlyMemory<byte> memory, byte[]? rentedBuffer = null)
        {
            Memory = MemoryMarshal.AsMemory(memory);
            RentedBuffer = rentedBuffer;
        }

        public Memory<byte> Memory { get; }
        public byte[]? RentedBuffer { get; }

        public void Return(bool clearMemory)
        {
            if (RentedBuffer != null)
            {
                ArrayPool<byte>.Shared.Return(RentedBuffer, clearMemory);
            }
            else
            {
                Memory.Span.Clear();
            }
        }
    }
}
