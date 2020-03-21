using Skynet.Model;
using Skynet.Network;
using Skynet.Protocol.Attributes;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x0A, PacketPolicies.Duplex)]
    public sealed class P0ACreateChannel : Packet
    {
        public long ChannelId { get; set; }
        public ChannelType ChannelType { get; set; }
        public long OwnerId { get; set; }
        public DateTime CreationTime { get; set; }
        public long CounterpartId { get; set; }

        public override Packet Create() => new P0ACreateChannel().Init(this);

        protected override void ReadPacketInternal(PacketBuffer buffer, PacketRole role)
        {
            ChannelId = buffer.ReadInt64();
            ChannelType = (ChannelType)buffer.ReadByte();
            if (role == PacketRole.Client)
            {
                OwnerId = buffer.ReadInt64();
                CreationTime = buffer.ReadDateTime();
            }
            if (ChannelType == ChannelType.Direct)
                CounterpartId = buffer.ReadInt64();
        }

        protected override void WritePacketInternal(PacketBuffer buffer, PacketRole role)
        {
            buffer.WriteInt64(ChannelId);
            buffer.WriteByte((byte)ChannelType);
            if (role == PacketRole.Server)
            {
                buffer.WriteInt64(OwnerId);
                buffer.WriteDateTime(CreationTime);
            }
            if (ChannelType == ChannelType.Direct)
                buffer.WriteInt64(CounterpartId);
        }

        public override string ToString()
        {
            return $"{{{nameof(P0ACreateChannel)}: ChannelId={ChannelId:x8} Type={ChannelType} Owner={OwnerId:x8} Counterpart={CounterpartId.ToString("x8")}}}";
        }
    }
}
