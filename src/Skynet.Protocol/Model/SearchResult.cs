using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Model
{
    public struct SearchResult
    {
        public long AccountId { get; set; }
        public string AccountName { get; set; }

        public SearchResult(long accountId, string accountName)
        {
            AccountId = accountId;
            AccountName = accountName;
        }
    }
}
