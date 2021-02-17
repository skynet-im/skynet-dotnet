using Skynet.Client.Database.Entities.Messages;
using System;
using System.Collections.Generic;

namespace Skynet.Client.Database.Entities
{
    public class Message
    {
        public long MessageId { get; set; }
        public DateTime DispatchTime { get; set; }

        public long ChannelId { get; set; }
        public Channel Channel { get; set; }

        public long? SenderId { get; set; }
        public Account Sender { get; set; }


        public IEnumerable<MessageDependency> Dependencies { get; set; }
        public IEnumerable<MessageDependency> Dependants { get; set; }

        public PasswordUpdate PasswordUpdate { get; set; }
        public DirectChannelCustomization DirectChannelCustomization { get; set; }
        public ChatMessage ChatMessage { get; set; }
        public IEnumerable<ChatMessage> QuotingMessages { get; set; }
    }
}
