using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Skynet.Tests
{
    public static class MemoryAssert
    {
        public static void AreEqual<T>(ReadOnlySpan<T> expected, ReadOnlySpan<T> actual) where T : IEquatable<T>
        {
            if (!expected.SequenceEqual(actual))
                throw new AssertFailedException("MemoryAssert.AreEqual failed.");
        }

        public static void AreEqual<T>(ReadOnlyMemory<T> expected, ReadOnlyMemory<T> actual) where T : IEquatable<T>
        {
            if (!expected.Span.SequenceEqual(actual.Span))
                throw new AssertFailedException("MemoryAssert.AreEqual failed.");
        }

        public static void AreEqual<T>(T[] expected, ReadOnlySpan<T> actual) where T : IEquatable<T>
        {
            if (!expected.AsSpan().SequenceEqual(actual))
                throw new AssertFailedException("MemoryAssert.AreEqual failed.");
        }

        public static void AreEqual<T>(T[] expected, ReadOnlyMemory<T> actual) where T : IEquatable<T>
        {
            if (!expected.AsSpan().SequenceEqual(actual.Span))
                throw new AssertFailedException("MemoryAssert.AreEqual failed.");
        }

        public static void AreEqual<T>(T[] expected, T[] actual) where T : IEquatable<T>
        {
            if (!expected.AsSpan().SequenceEqual(actual))
                throw new AssertFailedException("MemoryAssert.AreEqual failed.");
        }
    }
}
