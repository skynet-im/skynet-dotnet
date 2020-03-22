using System;

namespace Skynet.Model
{
    [Flags]
    public enum MessageFlags
    {
        None = 0,
        Loopback = 1,
        Unencrypted = 2,
        NoSenderSync = 4,
        MediaMessage = 8,
        ExternalFile = 16,
        All = Loopback | Unencrypted | NoSenderSync | MediaMessage | ExternalFile
    }
}
