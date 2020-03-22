using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class PacketAttribute : Attribute
    {
        public PacketAttribute(byte packedId, PacketPolicies packetPolicies)
        {
            PacketId = packedId;
            PacketPoliies = packetPolicies;
        }

        public byte PacketId { get; }
        public PacketPolicies PacketPoliies { get; }
    }
}
