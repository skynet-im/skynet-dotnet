using Skynet.Protocol.Model;
using System;
using System.Collections.Generic;

namespace Skynet.Client.Database.Entities.Messages
{
    public class DirectChannelCustomization
    {
        public long MessageId { get; set; }
        public Message Message { get; set; }

        public string CustomNickname { get; set; }
        public ImageShape ProfileImageShape { get; set; }
    }
}
