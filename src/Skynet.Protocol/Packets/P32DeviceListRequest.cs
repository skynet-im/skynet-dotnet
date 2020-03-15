using Skynet.Protocol.Attributes;
using Skynet.Network;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Packets
{
    [Packet(0x32, PacketPolicies.Receive)]
    internal class P32DeviceListRequest : Packet
    {
        public override Packet Create() => new P32DeviceListRequest().Init(this);
    }
}
