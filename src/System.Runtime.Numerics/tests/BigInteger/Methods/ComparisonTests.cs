// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using Xunit;

namespace System.Numerics.Tests
{
    public static class ComparisonTests
    {
        private const int LessThan = -1;
        private const int Equal = 0;
        private const int GreaterThan = 1;

        private const int NumberOfRandomIterations = 10;

        private static IEnumerable<object[]> CompareToBigInteger_Data()
        {
            // BigInteger.Zero, BigInteger constructed from float/double/decimal
            yield return new object[] { BigInteger.Zero, Equal, new BigInteger(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 }) };
            yield return new object[] { BigInteger.Zero, Equal, -1 * new BigInteger(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 }) };
            yield return new object[] { BigInteger.Zero, Equal, (BigInteger)(0f) };
            yield return new object[] { BigInteger.Zero, Equal, (BigInteger)(0d) };
            yield return new object[] { BigInteger.Zero, Equal, (BigInteger)(0m) };

            // BigInteger.One, BigInteger constructed from float/double/decimal
            yield return new object[] { BigInteger.One, Equal, new BigInteger(new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 }) };
            yield return new object[] { BigInteger.One, Equal, (BigInteger)(1f) };
            yield return new object[] { BigInteger.One, Equal, (BigInteger)(1d) };
            yield return new object[] { BigInteger.One, Equal, (BigInteger)(1m) };

            // Inputs from BigInteger Properties
            yield return new object[] { BigInteger.MinusOne, Equal, BigInteger.MinusOne };
            yield return new object[] { BigInteger.MinusOne, LessThan, BigInteger.Zero };
            yield return new object[] { BigInteger.MinusOne, LessThan, BigInteger.One };
            yield return new object[] { BigInteger.MinusOne, GreaterThan, -1 * (BigInteger)ulong.MaxValue };
            yield return new object[] { BigInteger.MinusOne, GreaterThan, (BigInteger)(-2) };
            yield return new object[] { BigInteger.MinusOne, LessThan, (BigInteger)ulong.MaxValue + 1 };
            yield return new object[] { BigInteger.MinusOne, LessThan, (BigInteger)2 };
            yield return new object[] { BigInteger.MinusOne, GreaterThan, BigInteger.MinusOne - 1 };

            yield return new object[] { BigInteger.Zero, Equal, BigInteger.Zero };
            yield return new object[] { BigInteger.Zero, GreaterThan, -1 * ((BigInteger)ulong.MaxValue + 1) };
            yield return new object[] { BigInteger.Zero, GreaterThan, -1 * ((BigInteger)ulong.MaxValue - 1) };
            yield return new object[] { BigInteger.Zero, LessThan, (BigInteger)ulong.MaxValue + 1 };
            yield return new object[] { BigInteger.Zero, LessThan, (BigInteger)uint.MaxValue - 1 };

            yield return new object[] { BigInteger.One, Equal, BigInteger.One };
            yield return new object[] { BigInteger.One, GreaterThan, BigInteger.MinusOne };
            yield return new object[] { BigInteger.One, GreaterThan, BigInteger.Zero };
            yield return new object[] { BigInteger.One, GreaterThan, -1 * ((BigInteger)int.MaxValue + 1) };
            yield return new object[] { BigInteger.One, GreaterThan, -1 * ((BigInteger)int.MaxValue - 1) };
            yield return new object[] { BigInteger.One, LessThan, (BigInteger)ulong.MaxValue + 1 };
            yield return new object[] { BigInteger.One, LessThan, (BigInteger)int.MaxValue - 1 };

            //1 Inputs Around the boundary of uint
            yield return new object[] { -1L * (BigInteger)uint.MaxValue - 1, Equal, -1L * (BigInteger)uint.MaxValue - 1 };
            yield return new object[] { -1L * (BigInteger)uint.MaxValue, GreaterThan, (-1L * (BigInteger)uint.MaxValue) - 1L };
            yield return new object[] { (BigInteger)uint.MaxValue, GreaterThan, -1L * (BigInteger)uint.MaxValue };
            yield return new object[] { (BigInteger)uint.MaxValue, Equal, (BigInteger)uint.MaxValue };
            yield return new object[] { (BigInteger)uint.MaxValue, LessThan, (BigInteger)uint.MaxValue + 1 };
            yield return new object[] { (BigInteger)ulong.MaxValue, Equal, (BigInteger)ulong.MaxValue };
            yield return new object[] { (BigInteger)ulong.MaxValue + 1, GreaterThan, (BigInteger)ulong.MaxValue };

            //Other cases
            yield return new object[] { -1L * ((BigInteger)int.MaxValue + 1), Equal, -1L * ((BigInteger)int.MaxValue + 1) };
            yield return new object[] { (BigInteger)int.MaxValue + 1, GreaterThan, -1L * ((BigInteger)int.MaxValue + 1) };
            yield return new object[] { (BigInteger)long.MaxValue + 1, GreaterThan, (BigInteger)uint.MaxValue };
            yield return new object[] { (BigInteger)int.MaxValue + 1, LessThan, ((BigInteger)int.MaxValue) + 2 };
            yield return new object[] { -1L * ((BigInteger)int.MaxValue - 1), Equal, -1L * ((BigInteger)int.MaxValue - 1) };
            yield return new object[] { (BigInteger)int.MaxValue - 1, GreaterThan, -1L * ((BigInteger)int.MaxValue - 1) };
            yield return new object[] { (BigInteger)int.MaxValue - 1, LessThan, (BigInteger)uint.MaxValue - 1 };
            yield return new object[] { (BigInteger)int.MaxValue - 2, LessThan, ((BigInteger)int.MaxValue) - 1 };
        }

        public static IEnumerable<object[]> CompareToBigIntegerRandom_Data()
        {
            Random random = new Random(100);

            for (int i = 0; i < NumberOfRandomIterations; ++i)
            {
                byte[] data = GetRandomByteArrayNotZero(random);
                yield return new object[] { new BigInteger(data), Equal, new BigInteger(data) };
                yield return new object[] { new BigInteger(data), data.IsNegative() ? LessThan : GreaterThan, -1L * new BigInteger(data) };

                byte[] left = BigIntegerCalculator.FromRandomData(random: random);
                byte[] right = BigIntegerCalculator.FromRandomData(random: random);

                yield return new object[] { new BigInteger(left), left.CompareTo(right), new BigInteger(right) };
            }
        }

        public static IEnumerable<object[]> CompareToLong_Data()
        {
            //Basic Checks
            yield return new object[] { BigInteger.MinusOne, Equal, (int)-1 };
            yield return new object[] { BigInteger.Zero, Equal, (int)0 };
            yield return new object[] { BigInteger.One, Equal, (int)1 };

            //BigInteger vs. int
            yield return new object[] { (BigInteger)int.MaxValue + 1, GreaterThan, int.MaxValue };
            yield return new object[] { (BigInteger)ulong.MaxValue + 1, GreaterThan, int.MaxValue };
            yield return new object[] { (BigInteger)short.MinValue - 1, LessThan, int.MaxValue };
            yield return new object[] { (BigInteger)int.MaxValue - 1, LessThan, int.MaxValue };
            yield return new object[] { (BigInteger)int.MaxValue, Equal, int.MaxValue };

            //BigInteger vs. long
            yield return new object[] { (BigInteger)long.MaxValue - 1, LessThan, long.MaxValue };
            yield return new object[] { (BigInteger)ulong.MaxValue + 100, GreaterThan, long.MaxValue };
            yield return new object[] { (BigInteger)short.MinValue - 1, LessThan, long.MaxValue };
            yield return new object[] { (BigInteger)long.MaxValue, Equal, long.MaxValue };
            yield return new object[] { (BigInteger)long.MaxValue + 1, GreaterThan, long.MaxValue };
        }

        public static IEnumerable<object[]> CompareToUnsignedLong_Data()
        {
            //Basic Checks
            yield return new object[] { BigInteger.Zero, Equal, (int)0 };
            yield return new object[] { BigInteger.One, Equal, (int)1 };

            //BigInteger vs. uint
            yield return new object[] { (BigInteger)uint.MaxValue + 1, GreaterThan, uint.MaxValue };
            yield return new object[] { (BigInteger)long.MaxValue + 1, GreaterThan, uint.MaxValue };
            yield return new object[] { (BigInteger)short.MinValue - 1, LessThan, uint.MaxValue };
            yield return new object[] { (BigInteger)uint.MaxValue - 1, LessThan, uint.MaxValue };
            yield return new object[] { (BigInteger)uint.MaxValue, Equal, uint.MaxValue };

            //BigInteger vs. ulong
            yield return new object[] { (BigInteger)ulong.MaxValue + 1, GreaterThan, ulong.MaxValue };
            yield return new object[] { (BigInteger)ulong.MaxValue + 100, GreaterThan, ulong.MaxValue };
            yield return new object[] { (BigInteger)short.MinValue - 1, LessThan, ulong.MaxValue };
            yield return new object[] { (BigInteger)short.MaxValue - 1, LessThan, ulong.MaxValue };
            yield return new object[] { (BigInteger)int.MaxValue + 1, LessThan, ulong.MaxValue };
            yield return new object[] { (BigInteger)ulong.MaxValue - 1, LessThan, ulong.MaxValue };
            yield return new object[] { (BigInteger)ulong.MaxValue, Equal, ulong.MaxValue };
        }

        [Fact]
        public static void NotEqualTests()
        {
            // The difference between this call and those used in the other tests is that this
            // explicitly casts to object, which evokes a different overload.
            Assert.False(BigInteger.Zero.Equals((object)0m));
            Assert.False(BigInteger.One.Equals((object)1L));

            Assert.False(BigInteger.Zero.Equals(null));
            Assert.False(BigInteger.Zero.Equals("0"));
            Assert.False(BigInteger.Zero.Equals('0'));
        }

        public static void IComparable_Invalid(string paramName)
        {
            IComparable comparable = new BigInteger();
            Assert.Equal(1, comparable.CompareTo(null));
            Assert.Throws<ArgumentException>(paramName, () => comparable.CompareTo(0)); // Obj is not a BigInteger
        }

        [Fact]
        [SkipOnTargetFramework(TargetFrameworkMonikers.Netcoreapp | TargetFrameworkMonikers.Uap)]
        public static void IComparable_Invalid_net46()
        {
            IComparable_Invalid(null);
        }

        [Fact]
        [SkipOnTargetFramework(TargetFrameworkMonikers.NetFramework)]
        public static void IComparable_Invalid_netcore()
        {
            IComparable_Invalid("obj");
        }

        [Theory]
        [MemberData(nameof(CompareToBigInteger_Data))]
        [MemberData(nameof(CompareToBigIntegerRandom_Data))]
        public static void CompareToBigIntegerTest(BigInteger x, int expectedResult, BigInteger y)
        {
            bool expectedEquals = Equal == expectedResult;
            bool expectedLessThan = expectedResult <= LessThan;
            bool expectedGreaterThan = expectedResult >= GreaterThan;

            Assert.Equal(expectedEquals, x == y);
            Assert.Equal(expectedEquals, y == x);

            Assert.Equal(!expectedEquals, x != y);
            Assert.Equal(!expectedEquals, y != x);

            Assert.Equal(expectedEquals, x.Equals(y));
            Assert.Equal(expectedEquals, y.Equals(x));

            Assert.Equal(expectedEquals, x.Equals((object)y));
            Assert.Equal(expectedEquals, y.Equals((object)x));

            AssertComparisonInRange(expectedResult, x.CompareTo(y));
            AssertComparisonInRange(-expectedResult, y.CompareTo(x));

            IComparable comparableX = x;
            IComparable comparableY = y;
            AssertComparisonInRange(expectedResult, comparableX.CompareTo(y));
            AssertComparisonInRange(-expectedResult, comparableY.CompareTo(x));

            AssertComparisonInRange(expectedResult, BigInteger.Compare(x, y));
            AssertComparisonInRange(-expectedResult, BigInteger.Compare(y, x));

            if (expectedEquals)
            {
                Assert.Equal(x.GetHashCode(), y.GetHashCode());
            }

            Assert.Equal(x.GetHashCode(), x.GetHashCode());
            Assert.Equal(y.GetHashCode(), y.GetHashCode());

            Assert.Equal(expectedLessThan, x < y);
            Assert.Equal(expectedGreaterThan, y < x);

            Assert.Equal(expectedGreaterThan, x > y);
            Assert.Equal(expectedLessThan, y > x);

            Assert.Equal(expectedLessThan || expectedEquals, x <= y);
            Assert.Equal(expectedGreaterThan || expectedEquals, y <= x);

            Assert.Equal(expectedGreaterThan || expectedEquals, x >= y);
            Assert.Equal(expectedLessThan || expectedEquals, y >= x);
        }

        [Theory]
        [MemberData(nameof(CompareToLong_Data))]
        public static void CompareToLongTest(BigInteger x, int expectedResult, long y)
        {
            bool expectedEquals = 0 == expectedResult;
            bool expectedLessThan = expectedResult < 0;
            bool expectedGreaterThan = expectedResult > 0;

            Assert.Equal(expectedEquals, x == y);
            Assert.Equal(expectedEquals, y == x);

            Assert.Equal(!expectedEquals, x != y);
            Assert.Equal(!expectedEquals, y != x);

            Assert.Equal(expectedEquals, x.Equals(y));

            AssertComparisonInRange(expectedResult, x.CompareTo(y));

            if (expectedEquals)
            {
                Assert.Equal(x.GetHashCode(), ((BigInteger)y).GetHashCode());
            }

            Assert.Equal(x.GetHashCode(), x.GetHashCode());
            Assert.Equal(((BigInteger)y).GetHashCode(), ((BigInteger)y).GetHashCode());

            Assert.Equal(expectedLessThan, x < y);
            Assert.Equal(expectedGreaterThan, y < x);

            Assert.Equal(expectedGreaterThan, x > y);
            Assert.Equal(expectedLessThan, y > x);

            Assert.Equal(expectedLessThan || expectedEquals, x <= y);
            Assert.Equal(expectedGreaterThan || expectedEquals, y <= x);

            Assert.Equal(expectedGreaterThan || expectedEquals, x >= y);
            Assert.Equal(expectedLessThan || expectedEquals, y >= x);

            CompareToBigIntegerTest(x, expectedResult, y);
        }

        [Theory]
        [MemberData(nameof(CompareToUnsignedLong_Data))]
        public static void CompareToUnsignedLongTest(BigInteger x, int expectedResult, ulong y)
        {
            bool expectedEquals = 0 == expectedResult;
            bool expectedLessThan = expectedResult < 0;
            bool expectedGreaterThan = expectedResult > 0;

            Assert.Equal(expectedEquals, x == y);
            Assert.Equal(expectedEquals, y == x);

            Assert.Equal(!expectedEquals, x != y);
            Assert.Equal(!expectedEquals, y != x);

            Assert.Equal(expectedEquals, x.Equals(y));

            AssertComparisonInRange(expectedResult, x.CompareTo(y));

            if (expectedEquals)
            {
                Assert.Equal(x.GetHashCode(), ((BigInteger)y).GetHashCode());
            }

            Assert.Equal(x.GetHashCode(), x.GetHashCode());
            Assert.Equal(((BigInteger)y).GetHashCode(), ((BigInteger)y).GetHashCode());

            Assert.Equal(expectedLessThan, x < y);
            Assert.Equal(expectedGreaterThan, y < x);

            Assert.Equal(expectedGreaterThan, x > y);
            Assert.Equal(expectedLessThan, y > x);

            Assert.Equal(expectedLessThan || expectedEquals, x <= y);
            Assert.Equal(expectedGreaterThan || expectedEquals, y <= x);

            Assert.Equal(expectedGreaterThan || expectedEquals, x >= y);
            Assert.Equal(expectedLessThan || expectedEquals, y >= x);

            CompareToBigIntegerTest(x, expectedResult, y);
        }

        private static void AssertComparisonInRange(int expected, int actual)
        {
            if (expected == Equal)
            {
                Assert.Equal(expected, actual);
            }
            else if (expected <= LessThan)
            {
                Assert.InRange(actual, int.MinValue, LessThan);
            }
            else if (expected >= GreaterThan)
            {
                Assert.InRange(actual, GreaterThan, int.MaxValue);
            }
        }

        private static byte[] GetRandomByteArrayNotZero(Random random)
        {
            byte[] data;
            do
            {
                data = BigIntegerCalculator.FromRandomData(1024, random);
            }
            while (data.IsZero());

            return data;
        }
    }
}
