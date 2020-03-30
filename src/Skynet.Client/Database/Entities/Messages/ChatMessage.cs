using Skynet.Protocol.Model;
using System;
using System.Collections.Generic;

namespace Skynet.Client.Database.Entities.Messages
{
    public class ChatMessage
    {
        public long MessageId { get; set; }
        public Message Message { get; set; }

        public MessageType MessageType { get; set; }
        public string Text { get; set; }
        public long? QuotedMessageId { get; set; }
        public Message QuotedMessage { get; set; }
    }
}
