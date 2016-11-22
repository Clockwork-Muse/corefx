// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace System.Numerics.Tests
{
    public static class ConstructorTests
    {
        private const int s_samples = 10;

        [Fact]
        public static void ConstructorDefaultTest()
        {
            BigInteger def = new BigInteger();
            Assert.Equal(new byte[] { 0x00 }, def.ToByteArray());
        }

        public static IEnumerable<object[]> ConstructorInt_Data()
        {
            Random random = new Random(100);

            yield return new object[] { int.MinValue };
            yield return new object[] { short.MinValue };
            yield return new object[] { sbyte.MinValue };
            yield return new object[] { -16 };
            yield return new object[] { -5 };
            yield return new object[] { -1 };
            yield return new object[] { 0 };
            yield return new object[] { 1 };
            yield return new object[] { 5 };
            yield return new object[] { 16 };
            yield return new object[] { byte.MaxValue };
            yield return new object[] { sbyte.MaxValue };
            yield return new object[] { short.MaxValue };
            yield return new object[] { ushort.MaxValue };
            yield return new object[] { int.MaxValue };

            for (int i = 0; i < s_samples; i++)
            {
                // Random positive
                yield return new object[] { random.Next(2, int.MaxValue - 1) };
                // Random negative
                yield return new object[] { random.Next(int.MinValue + 1, -2) };
            }
        }

        [Theory]
        [MemberData(nameof(ConstructorInt_Data))]
        public static void ConstructorIntTests(int value)
        {
            byte[] expected = BigIntegerCalculator.From(value);

            BigInteger bigInteger = new BigInteger(value);

            Assert.Equal(expected, bigInteger.ToByteArray());
        }

        public static IEnumerable<object[]> ConstructorLong_Data()
        {
            yield return new object[] { long.MinValue };
            yield return new object[] { int.MinValue - 1L };
            yield return new object[] { int.MaxValue + 1L };
            yield return new object[] { uint.MaxValue };
            yield return new object[] { long.MaxValue };

            Random random = new Random(100);

            for (int i = 0; i < s_samples; i++)
            {
                // Random positive
                yield return new object[] { (long)random.Next(2, int.MaxValue - 1) << 32 };
                // Random negative
                yield return new object[] { (long)random.Next(int.MinValue + 1, -2) << 32 };
            }
        }

        [Theory]
        [MemberData(nameof(ConstructorInt_Data))]
        [MemberData(nameof(ConstructorLong_Data))]
        public static void ConstructorLongTests(long value)
        {
            byte[] expected = BigIntegerCalculator.From(value);

            BigInteger bigInteger = new BigInteger(value);

            Assert.Equal(expected, bigInteger.ToByteArray());
        }

        public static IEnumerable<object[]> ConstructorUInt_Data()
        {
            Random random = new Random(100);

            yield return new object[] { 0 };
            yield return new object[] { 1 };
            yield return new object[] { 5 };
            yield return new object[] { 16 };
            yield return new object[] { ushort.MaxValue };
            yield return new object[] { uint.MaxValue };

            for (int i = 0; i < s_samples; i++)
            {
                // Random positive
                yield return new object[] { (uint)random.Next(1, int.MaxValue) << 1 };
            }
        }

        [Theory]
        [MemberData(nameof(ConstructorUInt_Data))]
        public static void ConstructorUIntTests(uint value)
        {
            byte[] expected = BigIntegerCalculator.From(value);

            BigInteger bigInteger = new BigInteger(value);

            Assert.Equal(expected, bigInteger.ToByteArray());
        }

        public static IEnumerable<object[]> ConstructorULong_Data()
        {
            yield return new object[] { uint.MaxValue + 1UL };
            yield return new object[] { ulong.MaxValue };

            Random random = new Random(100);

            for (int i = 0; i < s_samples; i++)
            {
                // Random positive
                yield return new object[] { (ulong)random.Next(1, int.MaxValue) << 32 };
            }
        }

        [Theory]
        [MemberData(nameof(ConstructorUInt_Data))]
        [MemberData(nameof(ConstructorULong_Data))]
        public static void ConstructorULongTests(ulong value)
        {
            byte[] expected = BigIntegerCalculator.From(value);

            BigInteger bigInteger = new BigInteger(value);
            Assert.Equal(expected, bigInteger.ToByteArray());
        }

        public static IEnumerable<object[]> ConstructorFloat_Data()
        {
            Random random = new Random(100);

            yield return new object[] { float.MinValue };
            yield return new object[] { float.MaxValue };
            yield return new object[] { float.Epsilon };
            // Smallest Exponent
            yield return new object[] { (float)Math.Pow(2, -126) };
            // Largest Exponent
            yield return new object[] { (float)Math.Pow(2, 127) };
            // Largest number less than 1
            yield return new object[] { Enumerable.Range(1, 24).Aggregate(0f, (acc, i) => acc + (float)Math.Pow(2, -i)) };
            // Smallest number greater than 1
            yield return new object[] { 1 + (float)Math.Pow(2, -23) };
            // Largest number less than 2
            yield return new object[] { 1 + Enumerable.Range(1, 23).Aggregate(0f, (acc, i) => acc + (float)Math.Pow(2, -i)) };

            for (int i = 0; i < s_samples; i++)
            {
                // Random positive
                yield return new object[] { (float)random.NextDouble() * float.MaxValue };
                // Random negative
                yield return new object[] { (float)random.NextDouble() * float.MaxValue - float.MaxValue };
                // Small Random Positive with fractional part
                yield return new object[] { random.Next(0, 100) + (float)random.NextDouble() };
                // Small Random Negative with fractional part
                yield return new object[] { random.Next(-100, 0) - (float)random.NextDouble() };
                // Large Random Positive with fractional part
                yield return new object[] { (float)random.NextDouble() * float.MaxValue + (float)random.NextDouble() };
                // Large Random Negative with fractional part
                yield return new object[] { (float)random.NextDouble() * -(float.MaxValue - 1) - (float)random.NextDouble() };
            }
        }

        [Theory]
        [MemberData(nameof(ConstructorInt_Data))]
        [MemberData(nameof(ConstructorFloat_Data))]
        public static void ConstructorFloatTests(float value)
        {
            byte[] expected = BigIntegerCalculator.From(value);

            BigInteger bigInteger = new BigInteger(value);

            Assert.Equal(expected, bigInteger.ToByteArray());
        }

        [Fact]
        public static void ConstructorFloat_OverflowTests()
        {
            if (BitConverter.IsLittleEndian)
            {
                float NaN2 = BitConverter.ToSingle(BitConverter.GetBytes(0x7FC00000), 0);
                Assert.Throws<OverflowException>(() => new BigInteger(NaN2));
            }

            Assert.Throws<OverflowException>(() => new BigInteger(float.NegativeInfinity));
            Assert.Throws<OverflowException>(() => new BigInteger(float.PositiveInfinity));
            Assert.Throws<OverflowException>(() => new BigInteger(float.NaN));
        }

        public static IEnumerable<object[]> ConstructorDouble_Data()
        {
            Random random = new Random(100);

            yield return new object[] { double.MinValue };
            yield return new object[] { float.MinValue - 1.0 };
            yield return new object[] { float.MaxValue + 1.0 };
            yield return new object[] { double.MaxValue };
            yield return new object[] { double.Epsilon };

            // Smallest Exponent
            yield return new object[] { Math.Pow(2, -1022) };
            // Largest Exponent
            yield return new object[] { Math.Pow(2, 1023) };
            // Largest number less than 1
            yield return new object[] { Enumerable.Range(1, 53).Aggregate(0.0, (acc, i) => acc + Math.Pow(2, -i)) };
            // Smallest number greater than 1
            yield return new object[] { 1 + Math.Pow(2, -52) };
            // Largest number less than 2
            yield return new object[] { 2 + Enumerable.Range(1, 52).Aggregate(0.0, (acc, i) => acc + Math.Pow(2, -i)) };

            for (int i = 0; i < s_samples; i++)
            {
                // Random positive
                yield return new object[] { random.NextDouble() * double.MaxValue };
                // Random negative
                yield return new object[] { random.NextDouble() * double.MaxValue - double.MaxValue };
                // Small Random Positive with fractional part
                yield return new object[] { random.Next(0, 100) + random.NextDouble() };
                // Small Random Negative with fractional part
                yield return new object[] { random.Next(-100, 0) - random.NextDouble() };
                // Large Random Positive with fractional part
                yield return new object[] { (long.MaxValue / 100 * random.NextDouble()) + random.NextDouble() };
                // Large Random Negative with fractional part
                yield return new object[] { (-(long.MaxValue / 100) * random.NextDouble()) - random.NextDouble() };
            }
        }

        [Theory]
        [MemberData(nameof(ConstructorInt_Data))]
        [MemberData(nameof(ConstructorLong_Data))]
        [MemberData(nameof(ConstructorFloat_Data))]
        [MemberData(nameof(ConstructorDouble_Data))]
        public static void ConstructorDoubleTests(double value)
        {
            byte[] expected = BigIntegerCalculator.From(value);

            BigInteger bigInteger = new BigInteger(value);

            Assert.Equal(expected, bigInteger.ToByteArray());
        }

        [Fact]
        public static void ConstructorDouble_OverflowTests()
        {
            if (BitConverter.IsLittleEndian)
            {
                double NaN2 = BitConverter.ToDouble(BitConverter.GetBytes(0x7FF8000000000000), 0);
                Assert.Throws<OverflowException>(() => new BigInteger(NaN2));
            }

            Assert.Throws<OverflowException>(() => new BigInteger(double.NegativeInfinity));
            Assert.Throws<OverflowException>(() => new BigInteger(double.PositiveInfinity));
            Assert.Throws<OverflowException>(() => new BigInteger(double.NaN));
        }

        public static IEnumerable<object[]> ConstructorDecimal_Data()
        {
            Random random = new Random(100);

            yield return new object[] { decimal.MinValue };
            yield return new object[] { decimal.MaxValue };

            // Smallest Exponent
            yield return new object[] { new decimal(1, 0, 0, false, 0) };
            // Largest Exponent
            yield return new object[] { new decimal(1, 0, 0, false, 28) };
            // Largest Exponent and zero integer
            yield return new object[] { new decimal(0, 0, 0, false, 28) };
            // Largest number less than 1
            yield return new object[] { 1 - new decimal(1, 0, 0, false, 28) };
            // Smallest number greater than 1
            yield return new object[] { 1 + new decimal(1, 0, 0, false, 28) };
            // Largest number less than 2
            yield return new object[] { 2 - new decimal(1, 0, 0, false, 28) };

            for (int i = 0; i < s_samples; i++)
            {
                // Random positive
                yield return new object[]
                {
                    new decimal(
                        random.Next(int.MinValue, int.MaxValue),
                        random.Next(int.MinValue, int.MaxValue),
                        random.Next(int.MinValue, int.MaxValue),
                        false, (byte)random.Next(0, 28))
                };
                // Random negative
                yield return new object[]
                {
                    new decimal(
                        random.Next(int.MinValue, int.MaxValue),
                        random.Next(int.MinValue, int.MaxValue),
                        random.Next(int.MinValue, int.MaxValue),
                        true, (byte)random.Next(0, 28))
                };
            }
        }

        [Theory]
        [MemberData(nameof(ConstructorInt_Data))]
        [MemberData(nameof(ConstructorLong_Data))]
        [MemberData(nameof(ConstructorDecimal_Data))]
        public static void ConstructorDecimalTests(decimal value)
        {
            byte[] expected = BigIntegerCalculator.From(value);

            BigInteger bigInteger = new BigInteger(value);

            Assert.Equal(expected, bigInteger.ToByteArray());
        }

        /// <summary>
        /// byte array source/expected result data
        /// </summary>
        /// <remarks>
        /// The data returned is source data, then expected result
        /// </remarks>
        /// <returns>The test data</returns>
        public static IEnumerable<object[]> ConstructorByteArray_Data()
        {
            Random random = new Random(100);

            // Empty is 0
            yield return new object[] { new byte[] { }, new byte[] { 0x00 } };
            // 0
            yield return new object[] { new byte[] { 0x00 }, new byte[] { 0x00 } };
            yield return new object[] { new byte[] { 0x01 }, new byte[] { 0x01 } };
            // Negative 1
            yield return new object[] { new byte[] { 0xff }, new byte[] { 0xff } };
            // Positive 256
            yield return new object[] { new byte[] { 0xff, 0x00 }, new byte[] { 0xff, 0x00 } };
            // UInt32.MaxValue
            yield return new object[] { new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0x00 }, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0x00 } };
            // UInt32.MaxValue + 1
            yield return new object[] { new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0x01 }, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0x01 } };
            // UInt64.MaxValue
            yield return new object[] {
                new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00 },
                new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00 }
            };
            // UInt64.MaxValue + 1
            yield return new object[] {
                new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x01 },
                new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x01 }
            };
            // UInt64.MaxValue + UInt64.MaxValue + 1
            yield return new object[] {
                new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x01 },
                new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x01 }
            };

            for (int i = 0; i < s_samples; i++)
            {
                byte[] buffer = BigIntegerCalculator.FromRandomData(1, random);
                yield return new object[] { buffer, buffer };
            }

            // One int
            // All 0s, 0
            yield return new object[] { new byte[] { 0x00, 0x00, 0x00, 0x00 }, new byte[] { 0x00 } };
            // All set, -1
            yield return new object[] { new byte[] { 0xff, 0xff, 0xff, 0xff }, new byte[] { 0xff } };

            // Multiple ints/longs
            // All 0s, 0
            yield return new object[] {
                new byte[] {
                    0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00,
                },
                new byte[] { 0x00 }
            };
            // All set, -1
            yield return new object[] {
                new byte[] {
                    0xff, 0xff, 0xff, 0xff,
                    0xff, 0xff, 0xff, 0xff,
                    0xff, 0xff, 0xff, 0xff,
                    0xff, 0xff, 0xff, 0xff,
                    0xff, 0xff, 0xff, 0xff,
                    0xff, 0xff, 0xff, 0xff,
                },
                new byte[] { 0xff }
            };

            // Array with a lot of leading zeros
            for (int i = 0; i < s_samples; i++)
            {
                byte[] buffer = BigIntegerCalculator.FromRandomData(4, random);

                byte[] source = new byte[]
                {
                    buffer[0],
                    buffer[1],
                    buffer[2],
                    buffer[3],
                    0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00,
                };

                // If the high bit was set, we need to keep the positive value by adding an additional byte.
                byte[] expected =
                    (buffer.IsNegative() ?
                        new byte[] {
                            buffer[0],
                            buffer[1],
                            buffer[2],
                            buffer[3],
                            0x00
                        }
                        : new byte[] {
                            buffer[0],
                            buffer[1],
                            buffer[2],
                            buffer[3]
                        });

                yield return new object[] { source, expected };
            }

            foreach (int size in new[] { 4, 5, 8, 9, 16, 17 })
            {
                for (int i = 0; i < s_samples; i++)
                {
                    byte[] data = BigIntegerCalculator.FromRandomData(size, random);
                    bool negative = data.IsNegative();
                    // Unmodified
                    yield return new object[] { data, data };

                    byte[] modified = new byte[size + 1];
                    Array.Copy(data, modified, size);
                    // In cases where the data would be ambiguous, an extra clarifying byte is appended.
                    // The most common case is when a high bit is set, but the number is not negative.
                    modified[(size + 1) - 1] = (byte)(negative ? 0x00 : 0xff);

                    yield return new object[] { modified, modified };
                }
            }

            // Random > UInt64
            for (int i = 0; i < s_samples; i++)
            {
                byte[] buffer = BigIntegerCalculator.FromRandomData(random: random);
                yield return new object[] { buffer, buffer };
            }
        }

        [Theory]
        [MemberData(nameof(ConstructorByteArray_Data))]
        public static void ConstructorByteArrayTests(byte[] value, byte[] expected)
        {
            BigInteger bigInteger = new BigInteger(value);

            Assert.Equal(expected, bigInteger.ToByteArray());
        }

        /// <summary>
        /// byte array source/expected result data
        /// </summary>
        /// <remarks>
        /// The data returned is source data, then expected result
        /// </remarks>
        /// <returns>The test data</returns>
        public static IEnumerable<object[]> ConstructorByteArray_Longrunning_Data()
        {
            Random random = new Random(100);

            // Array is _really_ large
            for (int i = 0; i < s_samples; i++)
            {
                byte[] buffer = BigIntegerCalculator.FromRandomData(random.Next(16384, 2097152), random);
                yield return new object[] { buffer, buffer };
            }
        }

        [Theory]
        [OuterLoop]
        [MemberData(nameof(ConstructorByteArray_Longrunning_Data))]
        public static void ConstructorByteArrayLongrunningTests(byte[] value, byte[] expected)
        {
            BigInteger bigInteger = new BigInteger(value);

            Assert.Equal(expected, bigInteger.ToByteArray());
        }

        [Fact]
        public static void ConstructorByteArray_ArgumentNullTests()
        {
            Assert.Throws<ArgumentNullException>(() => new BigInteger((byte[])null));
        }
    }
}
