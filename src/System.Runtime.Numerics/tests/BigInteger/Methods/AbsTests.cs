// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using Xunit;

namespace System.Numerics.Tests
{
    public static class AbsTests
    {
        private const int s_samples = 10;

        public static IEnumerable<object[]> Abs_Data()
        {
            Random random = new Random(100);

            yield return new object[] { new BigInteger(0), new BigInteger(0) };
            yield return new object[] { new BigInteger(-1), new BigInteger(1) };
            yield return new object[] { new BigInteger(1), new BigInteger(1) };
            yield return new object[] { new BigInteger(int.MinValue), new BigInteger(int.MaxValue + 1m) };
            yield return new object[] { new BigInteger(int.MinValue - 1m), new BigInteger(int.MaxValue + 2m) };
            yield return new object[] { new BigInteger(int.MinValue + 1m), new BigInteger(int.MaxValue) };
            yield return new object[] { new BigInteger(int.MaxValue), new BigInteger(int.MaxValue) };
            yield return new object[] { new BigInteger(int.MaxValue - 1m), new BigInteger(int.MaxValue - 1m) };
            yield return new object[] { new BigInteger(int.MaxValue + 1m), new BigInteger(int.MaxValue + 1m) };
            yield return new object[] { new BigInteger(long.MinValue), new BigInteger(long.MaxValue + 1m) };
            yield return new object[] { new BigInteger(long.MinValue - 1m), new BigInteger(long.MaxValue + 2m) };
            yield return new object[] { new BigInteger(long.MinValue + 1m), new BigInteger(long.MaxValue) };
            yield return new object[] { new BigInteger(long.MaxValue), new BigInteger(long.MaxValue) };
            yield return new object[] { new BigInteger(long.MaxValue - 1m), new BigInteger(long.MaxValue - 1m) };
            yield return new object[] { new BigInteger(long.MaxValue + 1m), new BigInteger(long.MaxValue + 1m) };

            // Large BigIntegers
            for (int i = 0; i < s_samples; i++)
            {
                byte[] buffer = new byte[random.Next(1, 1024)];
                random.NextBytes(buffer);
                yield return new object[] { new BigInteger(buffer), new BigInteger(buffer.Abs()) };
            }

            // Small BigIntegers
            for (int i = 0; i < s_samples; i++)
            {
                byte[] buffer = new byte[random.Next(1, 2)];
                random.NextBytes(buffer);
                yield return new object[] { new BigInteger(buffer), new BigInteger(buffer.Abs()) };
            }
        }

        [Theory]
        [MemberData(nameof(Abs_Data))]
        public static void AbsTest(BigInteger original, BigInteger expected)
        {
            Assert.Equal(expected, BigInteger.Abs(original));
        }
    }
}
