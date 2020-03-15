using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Model
{
    internal struct SessionDetails
    {
        public SessionDetails(long sessionId, DateTime lastConnected, int lastVersionCode)
        {
            SessionId = sessionId;
            LastConnected = lastConnected;
            LastVersionCode = lastVersionCode;
        }

        public long SessionId { get; set; }
        public DateTime LastConnected { get; set; }
        public int LastVersionCode { get; set; }
    }
}
