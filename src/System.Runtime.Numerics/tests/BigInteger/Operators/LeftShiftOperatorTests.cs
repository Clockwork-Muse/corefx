// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using Xunit;

namespace System.Numerics.Tests
{
    public static class LeftShiftOperatorTests
    {
        private const int s_samples = 10;

        public static IEnumerable<object[]> LeftShift_Data()
        {
            Random random = new Random(100);

            for (int i = 0; i < s_samples; i++)
            {
                foreach (int shift in new[] { 0, 32, -32, 32 * 7, random.Next(ushort.MaxValue), random.Next(1, 32), random.Next(-ushort.MaxValue, 0), random.Next(-31, 0) })
                {
                    byte[] original = BigIntegerCalculator.FromRandomData(random: random);
                    yield return new object[] { original, shift, original.ShiftLeft(shift) };
                    original = BigIntegerCalculator.FromRandomData(bytes: 2, random: random);
                    yield return new object[] { original, shift, original.ShiftLeft(shift) };
                }
            }
        }

        public static IEnumerable<object[]> LeftShiftToZero_Data()
        {
            Random random = new Random(100);

            // The chance of getting a positive or negative array is 50/50,
            // assuming the high bit is set independently.
            for (int i = 0; i < 2 * s_samples; i++)
            {
                byte[] original = BigIntegerCalculator.FromRandomData(bytes: 100, random: random);
                yield return new object[] { original, -original.GetHighestSetBit(), new byte[] { 0x00 } };
                yield return new object[] { original, random.Next(-256, -1) - original.GetHighestSetBit(), new byte[] { 0x00 } };
            }
        }

        public static IEnumerable<object[]> LeftShiftToNegativeOne_Data()
        {
            Random random = new Random(100);

            for (int i = 0; i < s_samples; i++)
            {
                byte[] original = BigIntegerCalculator.FromRandomData(bytes: 100, random: random, criteria: data => data.IsNegative());
                yield return new object[] { original, -(original.Length * 8 - 1), new byte[] { 0xff } };
            }
        }

        public static IEnumerable<object[]> LeftShift_D()
        {
            yield return new object[] { BigIntegerCalculator.From(int.MaxValue), 1, BigIntegerCalculator.From((long)int.MaxValue << 1) };
            yield return new object[] { BigIntegerCalculator.From(int.MaxValue), -1, BigIntegerCalculator.From((long)int.MaxValue >> 1) };
            yield return new object[] { BigIntegerCalculator.From(int.MaxValue), -8, BigIntegerCalculator.From((long)int.MaxValue >> 8) };
            yield return new object[] { BigIntegerCalculator.From(int.MaxValue), -31, BigIntegerCalculator.From((long)int.MaxValue >> 31) };
        }

        [Theory]
        [MemberData(nameof(LeftShift_Data))]
        // [MemberData(nameof(LeftShiftToZero_Data))]
        // [MemberData(nameof(LeftShiftToNegativeOne_Data))]
        public static void LeftShiftTest(byte[] original, int bits, byte[] expected)
        {
            BigInteger orig = new BigInteger(original);
            BigInteger exp = new BigInteger(expected);
            Assert.Equal(expected, original.ShiftLeft(bits));
            Assert.Equal(exp, orig << bits);
        }
    }
}