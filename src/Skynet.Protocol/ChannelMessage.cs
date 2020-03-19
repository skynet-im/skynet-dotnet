using Skynet.Model;
using Skynet.Protocol.Model;
using Skynet.Network;
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

        public ReadOnlyMemory<byte> PacketContent { get; set; }

        public MessageFlags RequiredFlags { get; set; } = MessageFlags.None;
        public MessageFlags AllowedFlags { get; set; } = MessageFlags.All;

        public override Packet Create() => new ChannelMessage().Init(this);

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
                var contentBuffer = new PacketBuffer(PacketContent);
                ReadMessage(contentBuffer);
                if (MessageFlags.HasFlag(MessageFlags.MediaMessage))
                    File = new ChannelMessageFile(contentBuffer, MessageFlags.HasFlag(MessageFlags.ExternalFile));
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

            if (GetType() == typeof(ChannelMessage))
            {
                PacketBuffer contentBuffer = new PacketBuffer();
                WriteMessage(contentBuffer);
                if (MessageFlags.HasFlag(MessageFlags.MediaMessage))
                    File.Write(contentBuffer, MessageFlags.HasFlag(MessageFlags.ExternalFile));
                PacketContent = contentBuffer.GetBuffer();
            }

            buffer.WriteMediumByteArray(PacketContent.Span);
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

        protected virtual void ReadMessage(PacketBuffer buffer)
        {
            if (!Policies.HasFlag(PacketPolicies.ClientToServer))
                throw new InvalidOperationException();
        }

        protected virtual void WriteMessage(PacketBuffer buffer)
        {
            if (!Policies.HasFlag(PacketPolicies.ServerToClient))
                throw new InvalidOperationException();
        }

        public override string ToString()
        {
            return $"{{{GetType().Name}: ChannelId={ChannelId:x8} MessageId={MessageId:x8} Flags={MessageFlags}}}";
        }
    }
}
