using Skynet.Model;
using System;
using System.Collections.Generic;

namespace Skynet.Protocol.Attributes
{
    /// <summary>
    /// Defines <see cref="MessageFlags"/> which this channel message must have.
    /// Use the <see cref="AllowedFlagsAttribute"/> to restrict additional flags.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    internal sealed class RequiredFlagsAttribute : MessageFlagsAttribute
    {
        public RequiredFlagsAttribute(MessageFlags flags)
            : base(flags) { }
    }
}
