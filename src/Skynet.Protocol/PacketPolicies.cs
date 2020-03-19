using System;

namespace Skynet.Protocol
{
    [Flags]
    public enum PacketPolicies
    {
        None = 0,
        ClientToServer = 1,
        ServerToClient = 2,
        Duplex = ClientToServer | ServerToClient,
        Unauthenticated = 4,
        Uninitialized = 8
    }
}
