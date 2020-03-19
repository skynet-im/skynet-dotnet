using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Skynet.Tests
{
    internal static class MemoryAssert
    {
        public static void AreEqual<T>(ReadOnlySpan<T> expected, ReadOnlySpan<T> actual) where T : IEquatable<T>
        {
            if (!expected.SequenceEqual(actual))
                Assert.Fail("MemoryAssert.AreEqual failed.");
        }

        public static void AreEqual<T>(ReadOnlyMemory<T> expected, ReadOnlyMemory<T> actual) where T : IEquatable<T>
        {
            if (!expected.Span.SequenceEqual(actual.Span))
                Assert.Fail("MemoryAssert.AreEqual failed.");
        }
    }
}
