using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Model
{
    public struct Dependency
    {
        public Dependency(long accountId, long messageId)
        {
            AccountId = accountId;
            MessageId = messageId;
        }

        public long AccountId { get; set; }
        public long MessageId { get; set; }
    }
}
