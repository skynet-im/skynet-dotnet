using System;
using System.Collections.Generic;

namespace Skynet.Network
{
    public interface IPacket
    {
        byte Id { get; }
        void ReadPacket(PacketBuffer buffer);
        void WritePacket(PacketBuffer buffer);
    }
}
