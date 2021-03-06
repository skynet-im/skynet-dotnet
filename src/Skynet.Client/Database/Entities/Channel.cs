﻿using Skynet.Model;
using System;
using System.Collections.Generic;

namespace Skynet.Client.Database.Entities
{
    public class Channel
    {
        public long ChannelId { get; set; }
        public ChannelType ChannelType { get; set; }
        public DateTime CreationTime { get; set; }

        public long? OwnerId { get; set; }
        public Account Owner { get; set; }

        public IEnumerable<Message> Messages { get; set; }
        public IEnumerable<ChannelMember> ChannelMembers { get; set; }
    }
}
