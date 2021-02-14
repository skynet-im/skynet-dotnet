using Skynet.Network;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol
{
    public sealed class ChannelMessageFile : IDisposable
    {
        private bool disposed;
        private PoolableMemory thumbnailData;
        private byte[]? key;

        public string Name { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastWriteTime { get; set; }
        public string ThumbnailContentType { get; set; }
        public PoolableMemory ThumbnailData
        {
            get { if (disposed) throw new ObjectDisposedException(nameof(ChannelMessageFile)); return thumbnailData; }
            set { if (disposed) throw new ObjectDisposedException(nameof(ChannelMessageFile)); thumbnailData = value; }
        }
        public string? ContentType { get; set; }
        public long Length { get; set; }
        public byte[]? Key
        {
            get { if (disposed) throw new ObjectDisposedException(nameof(ChannelMessageFile)); return key; }
            set { if (disposed) throw new ObjectDisposedException(nameof(ChannelMessageFile)); key = value; }
        }

        public ChannelMessageFile(PacketBuffer buffer, bool external)
        {
            Name = buffer.ReadShortString();
            CreationTime = buffer.ReadDateTime();
            LastWriteTime = buffer.ReadDateTime();
            ThumbnailContentType = buffer.ReadShortString();
            thumbnailData = buffer.ReadMediumPooledArray();

            if (external)
            {
                ContentType = buffer.ReadShortString();
                Length = buffer.ReadInt64();
                key = buffer.ReadByteArray(32);
            }
        }

        public void Write(PacketBuffer buffer, bool external)
        {
            buffer.WriteShortString(Name);
            buffer.WriteDateTime(CreationTime);
            buffer.WriteDateTime(LastWriteTime);
            buffer.WriteShortString(ThumbnailContentType);
            buffer.WriteMediumByteArray(ThumbnailData.Memory.Span);

            if (external)
            {
                buffer.WriteShortString(ContentType);
                buffer.WriteInt64(Length);
                buffer.WriteByteArray(Key, 32);
            }
        }

        public void Dispose()
        {
            if (!disposed)
            {
                thumbnailData.Return(true);
                key.AsSpan().Clear();

                disposed = true;
            }
        }
    }
}
