using Skynet.Model;
using Skynet.Network;
using Skynet.Protocol.Cryptography;
using Skynet.Protocol.Model;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol
{
    public abstract class ChannelMessage : Packet, IDisposable
    {
        private MessageFlags messageFlags;
        private PoolableMemory? contentBuffer;
        protected bool disposed;

        public byte PacketVersion { get; set; }
        public long ChannelId { get; set; }
        public long SenderId { get; set; }
        public long MessageId { get; set; }
        public long SkipCount { get; set; }
        public DateTime DispatchTime { get; set; }

        /// <summary>
        /// Gets or sets the flags for this channel message.
        /// Values read from a <see cref="PacketBuffer"/> are not validated.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The message flags are not in the specified range.</exception>
        public MessageFlags MessageFlags
        {
            get => messageFlags;
            set
            {
                if (!value.AreValid(RequiredFlags, AllowedFlags))
                    throw new ArgumentOutOfRangeException(nameof(MessageFlags), $"Message flags {value} are invalid for packet 0x{Id:x2}");
                messageFlags = value;
            }
        }

        public long FileId { get; set; }
        public ChannelMessageFile File { get; set; }
        public List<Dependency> Dependencies { get; set; } = new List<Dependency>();

        /// <summary>
        /// Gets or sets the binary packet content.
        /// This property returns null if the message has to be encrypted first by calling <see cref="Encrypt(ReadOnlySpan{byte})"/>.
        /// </summary>
        /// <exception cref="ObjectDisposedException"/>
        public byte[] PacketContent
        {
            get
            {
                if (disposed) throw new ObjectDisposedException(nameof(ChannelMessage));

                if (contentBuffer == null && messageFlags.HasFlag(MessageFlags.Unencrypted))
                    contentBuffer = WriteContent();

                return contentBuffer?.Memory.ToArray();
            }
            set
            {
                if (disposed) throw new ObjectDisposedException(nameof(ChannelMessage));
                contentBuffer = new PoolableMemory(value);
            }
        }

        public MessageFlags RequiredFlags { get; set; } = MessageFlags.None;
        public MessageFlags AllowedFlags { get; set; } = MessageFlags.All;

        public void Decrypt(ReadOnlySpan<byte> key)
        {
            if (disposed) throw new ObjectDisposedException(nameof(ChannelMessage));
            if (key.Length != 64) throw new ArgumentOutOfRangeException(nameof(key), key.Length, "The key must be exactly 64 bytes long.");
            if (contentBuffer == null) throw new InvalidOperationException("You have to call Packet.ReadPacket() on a ChannelMessage before you can decrypt it.");
            if (messageFlags.HasFlag(MessageFlags.Unencrypted)) throw new InvalidOperationException("You cannot decrypt a message with MessageFlags.Unencrypted.");

            byte[] hmacKey = key.Slice(0, 32).ToArray();
            byte[] aesKey = key.Slice(32, 32).ToArray();
            ReadContent(AesStatic.DecryptWithHmac(contentBuffer.Value.Memory, hmacKey, aesKey), clearBuffer: true);
            Array.Clear(hmacKey, 0, 32);
            Array.Clear(aesKey, 0, 32);
        }

        public void Encrypt(ReadOnlySpan<byte> key)
        {
            if (disposed) throw new ObjectDisposedException(nameof(ChannelMessage));
            if (key.Length != 64) throw new ArgumentOutOfRangeException(nameof(key), key.Length, "The key must be exactly 64 bytes long.");
            if (messageFlags.HasFlag(MessageFlags.Unencrypted))
                throw new InvalidOperationException("You cannot encrypt a message with MessageFlags.Unencrypted.");

            byte[] hmacKey = key.Slice(0, 32).ToArray();
            byte[] aesKey = key.Slice(32, 32).ToArray();
            PoolableMemory content = WriteContent();
            contentBuffer = new PoolableMemory(AesStatic.EncryptWithHmac(content.Memory, hmacKey, aesKey));
            content.Return(true);
            Array.Clear(hmacKey, 0, 32);
            Array.Clear(aesKey, 0, 32);
        }

        public void Dispose()
        {
            if (!disposed)
            {
                DisposeMessage();
                disposed = true;
            }
        }

        protected sealed override void ReadPacketInternal(PacketBuffer buffer, PacketRole role)
        {
            if (disposed) throw new ObjectDisposedException(nameof(ChannelMessage));

            PacketVersion = buffer.ReadByte();
            ChannelId = buffer.ReadInt64();
            if (role == PacketRole.Client)
                SenderId = buffer.ReadInt64();
            MessageId = buffer.ReadInt64();
            if (role == PacketRole.Client)
            {
                SkipCount = buffer.ReadInt64();
                DispatchTime = buffer.ReadDateTime();
            }
            messageFlags = (MessageFlags)buffer.ReadByte();
            if (messageFlags.HasFlag(MessageFlags.ExternalFile))
                FileId = buffer.ReadInt64();

            contentBuffer = buffer.ReadMediumPooledArray();

            if (messageFlags.HasFlag(MessageFlags.Unencrypted))
            {
                ReadContent(contentBuffer.Value.Memory, clearBuffer: false);
            }

            int length = buffer.ReadUInt16();
            for (int i = 0; i < length; i++)
            {
                Dependencies.Add(new Dependency(buffer.ReadInt64(), buffer.ReadInt64()));
            }
        }

        protected sealed override void WritePacketInternal(PacketBuffer buffer, PacketRole role)
        {
            if (disposed) throw new ObjectDisposedException(nameof(ChannelMessage));

            buffer.WriteByte(PacketVersion);
            buffer.WriteInt64(ChannelId);
            if (role == PacketRole.Server)
                buffer.WriteInt64(SenderId);
            buffer.WriteInt64(MessageId);
            if (role == PacketRole.Server)
            {
                buffer.WriteInt64(SkipCount);
                buffer.WriteDateTime(DispatchTime);
            }
            buffer.WriteByte((byte)messageFlags);
            if (messageFlags.HasFlag(MessageFlags.ExternalFile))
                buffer.WriteInt64(FileId);

            if (contentBuffer == null)
            {
                if (messageFlags.HasFlag(MessageFlags.Unencrypted))
                    contentBuffer = WriteContent();
                else
                    throw new InvalidOperationException(
                        "Before writing an encrypted message, you must either assign an encrypted PacketContent or call ChannelMessage.Encrypt().");
            }

            buffer.WriteMediumByteArray(contentBuffer.Value.Memory.Span);

            buffer.WriteUInt16((ushort)Dependencies.Count);
            foreach (Dependency dependency in Dependencies)
            {
                buffer.WriteInt64(dependency.AccountId);
                buffer.WriteInt64(dependency.MessageId);
            }
        }

        protected virtual void DisposeMessage()
        {
            contentBuffer?.Return(false);
            File?.Dispose();
        }

        protected ChannelMessage Init(ChannelMessage source)
        {
            Id = source.Id;
            Policies = source.Policies;
            RequiredFlags = source.RequiredFlags;
            AllowedFlags = source.AllowedFlags;
            return this;
        }

        protected virtual void ReadMessage(PacketBuffer buffer) { }
        protected virtual void WriteMessage(PacketBuffer buffer) { }

        private void ReadContent(ReadOnlyMemory<byte> buffer, bool clearBuffer)
        {
            using var contentBuffer = new PacketBuffer(buffer, clearBuffer);
            ReadMessage(contentBuffer);
            if (messageFlags.HasFlag(MessageFlags.MediaMessage))
                File = new ChannelMessageFile(contentBuffer, messageFlags.HasFlag(MessageFlags.ExternalFile));
        }

        private PoolableMemory WriteContent()
        {
            PacketBuffer contentBuffer = new PacketBuffer();
            WriteMessage(contentBuffer);
            if (messageFlags.HasFlag(MessageFlags.MediaMessage))
                File.Write(contentBuffer, messageFlags.HasFlag(MessageFlags.ExternalFile));
            return contentBuffer.GetBufferAndDispose();
        }

        public override string ToString()
        {
            return $"{{{GetType().Name}: ChannelId={ChannelId:x8} MessageId={MessageId:x8} Flags={messageFlags}}}";
        }
    }
}
