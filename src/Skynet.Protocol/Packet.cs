using Skynet.Network;
using System;

namespace Skynet.Protocol
{
    public abstract class Packet
    {
        public byte Id { get; set; }
        public PacketPolicies Policies { get; set; }

        public abstract Packet Create();

        public void ReadPacket(PacketBuffer buffer, PacketRole role)
        {
            if (role == PacketRole.Client && !Policies.HasFlag(PacketPolicies.ServerToClient)
                || role == PacketRole.Server && !Policies.HasFlag(PacketPolicies.ClientToServer))
            {
                throw new InvalidOperationException();
            }

            ReadPacketInternal(buffer, role);
        }

        public void WritePacket(PacketBuffer buffer, PacketRole role)
        {
            if (role == PacketRole.Client && !Policies.HasFlag(PacketPolicies.ClientToServer)
                || role == PacketRole.Server && !Policies.HasFlag(PacketPolicies.ServerToClient))
            {
                throw new InvalidOperationException();
            }

            WritePacketInternal(buffer, role);
        }

        protected virtual void ReadPacketInternal(PacketBuffer buffer, PacketRole role) { }
        protected virtual void WritePacketInternal(PacketBuffer buffer, PacketRole role) { }

        protected Packet Init(Packet source)
        {
            Id = source.Id;
            Policies = source.Policies;
            return this;
        }

        public override string ToString()
        {
            return $"{{{GetType().Name}}}";
        }
    }
}
