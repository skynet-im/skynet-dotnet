using Skynet.Model;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Attributes
{
    /// <summary>
    /// Defines <see cref="MessageFlags"/> which this channel message must have. Any additional flags are forbidden.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class MessageFlagsAttribute : Attribute
    {
        public MessageFlagsAttribute(MessageFlags flags)
        {
            Flags = flags;
        }

        public MessageFlags Flags { get; }
    }
}
