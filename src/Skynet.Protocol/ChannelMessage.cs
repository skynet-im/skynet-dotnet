using Skynet.Model;
using Skynet.Network;
using Skynet.Protocol.Cryptography;
using Skynet.Protocol.Model;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol
{
    internal class ChannelMessage : Packet
    {
        public byte PacketVersion { get; set; }
        public long ChannelId { get; set; }
        public long SenderId { get; set; }
        public long MessageId { get; set; }
        public long SkipCount { get; set; }
        public DateTime DispatchTime { get; set; }
        public MessageFlags MessageFlags { get; set; }
        public long FileId { get; set; }
        public ChannelMessageFile File { get; set; }
        public List<Dependency> Dependencies { get; set; } = new List<Dependency>();

        public ReadOnlyMemory<byte>? PacketContent { get; set; }

        public MessageFlags RequiredFlags { get; set; } = MessageFlags.None;
        public MessageFlags AllowedFlags { get; set; } = MessageFlags.All;

        public override Packet Create() => new ChannelMessage().Init(this);

        public void Decrypt(ReadOnlySpan<byte> key)
        {
            if (key.Length != 64) throw new ArgumentOutOfRangeException(nameof(key), key.Length, "The key must be exactly 64 bytes long.");
            if (PacketContent == null) throw new InvalidOperationException("You cannot decrypt a message that has not been read");

            byte[] hmacKey = key.Slice(0, 32).ToArray();
            byte[] aesKey = key.Slice(32, 32).ToArray();
            ReadContent(AesStatic.EncryptWithHmac(PacketContent.Value, hmacKey, aesKey));
            Array.Clear(hmacKey, 0, 32);
            Array.Clear(aesKey, 0, 32);
        }

        public void Encrypt(ReadOnlySpan<byte> key)
        {
            if (key.Length != 64) throw new ArgumentOutOfRangeException(nameof(key), key.Length, "The key must be exactly 64 bytes long.");

            byte[] hmacKey = key.Slice(0, 32).ToArray();
            byte[] aesKey = key.Slice(32, 32).ToArray();
            PacketContent = AesStatic.EncryptWithHmac(WriteContent(), hmacKey, aesKey);
            Array.Clear(hmacKey, 0, 32);
            Array.Clear(aesKey, 0, 32);
        }

        protected sealed override void ReadPacketInternal(PacketBuffer buffer, PacketRole role)
        {
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
            MessageFlags = (MessageFlags)buffer.ReadByte();
            if (MessageFlags.HasFlag(MessageFlags.ExternalFile))
                FileId = buffer.ReadInt64();

            PacketContent = buffer.ReadMediumByteArray();

            if (MessageFlags.HasFlag(MessageFlags.Unencrypted))
            {
                ReadContent(PacketContent.Value);
            }

            int length = buffer.ReadUInt16();
            for (int i = 0; i < length; i++)
            {
                Dependencies.Add(new Dependency(buffer.ReadInt64(), buffer.ReadInt64()));
            }
        }

        protected sealed override void WritePacketInternal(PacketBuffer buffer, PacketRole role)
        {
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
            buffer.WriteByte((byte)MessageFlags);
            if (MessageFlags.HasFlag(MessageFlags.ExternalFile))
                buffer.WriteInt64(FileId);

            if (PacketContent == null)
            {
                if (MessageFlags.HasFlag(MessageFlags.Unencrypted))
                    PacketContent = WriteContent();
                else
                    throw new InvalidOperationException("A message without MessageFlags.Unencrypted has be encrypted before it can be written.");
            }
            
            buffer.WriteMediumByteArray(PacketContent.Value.Span);

            buffer.WriteUInt16((ushort)Dependencies.Count);
            foreach (Dependency dependency in Dependencies)
            {
                buffer.WriteInt64(dependency.AccountId);
                buffer.WriteInt64(dependency.MessageId);
            }
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

        private void ReadContent(ReadOnlyMemory<byte> buffer)
        {
            var contentBuffer = new PacketBuffer(buffer);
            ReadMessage(contentBuffer);
            if (MessageFlags.HasFlag(MessageFlags.MediaMessage))
                File = new ChannelMessageFile(contentBuffer, MessageFlags.HasFlag(MessageFlags.ExternalFile));
        }

        private ReadOnlyMemory<byte> WriteContent()
        {
            PacketBuffer contentBuffer = new PacketBuffer();
            WriteMessage(contentBuffer);
            if (MessageFlags.HasFlag(MessageFlags.MediaMessage))
                File.Write(contentBuffer, MessageFlags.HasFlag(MessageFlags.ExternalFile));
            return contentBuffer.GetBuffer();
        }

        public override string ToString()
        {
            return $"{{{GetType().Name}: ChannelId={ChannelId:x8} MessageId={MessageId:x8} Flags={MessageFlags}}}";
        }
    }
}
