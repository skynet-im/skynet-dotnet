using System;
using System.Collections.Generic;

namespace Skynet.Client.Database.Entities
{
    public class Account
    {
        public long AccountId { get; set; }

        public IEnumerable<Channel> OwnedChannels { get; set; }
        public IEnumerable<ChannelMember> ChannelMemberships { get; set; }
        public IEnumerable<Message> SentMessages { get; set; }
    }
}
