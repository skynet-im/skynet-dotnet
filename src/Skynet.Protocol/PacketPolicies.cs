using System;

namespace Skynet.Protocol
{
    [Flags]
    public enum PacketPolicies
    {
        None = 0,
        Receive = 1,
        Send = 2,
        Duplex = Receive | Send,
        Unauthenticated = 4,
        Uninitialized = 8
    }
}
