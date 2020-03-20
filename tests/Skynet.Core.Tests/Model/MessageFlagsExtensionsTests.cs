using Microsoft.VisualStudio.TestTools.UnitTesting;
using Skynet.Model;
using System;

namespace Skynet.Tests.Model
{
    [TestClass]
    public class MessageFlagsExtensionsTests
    {
        [TestMethod]
        public void TestAreValid()
        {
            Assert.IsTrue(MessageFlags.None.AreValid(MessageFlags.None, MessageFlags.None));
            Assert.IsTrue((MessageFlags.Loopback | MessageFlags.Unencrypted).AreValid(MessageFlags.Loopback, MessageFlags.All));

            Assert.IsFalse(MessageFlags.Loopback.AreValid(MessageFlags.Unencrypted, MessageFlags.Loopback | MessageFlags.Unencrypted));
            Assert.IsFalse((MessageFlags.Loopback | MessageFlags.NoSenderSync).AreValid(MessageFlags.Loopback, MessageFlags.Loopback));
        }
    }
}
