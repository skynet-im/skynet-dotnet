using System;
using System.Collections.Generic;

namespace Skynet.Client.Database.Entities.Messages
{
    public class PasswordUpdate
    {
        public long MessageId { get; set; }
        public Message Message { get; set; }

        public byte[] Key { get; set; }
    }
}
