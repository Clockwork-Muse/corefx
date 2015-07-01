// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Threading;
using Xunit;

namespace System.Linq.Parallel.Tests
{
    public class MaxByTests
    {
        public static IEnumerable<object[]> OnlyOneData(object[] counts)
        {
            Func<int, IEnumerable<int>> positions = x => new[] { 0, x / 2, Math.Max(0, x - 1) }.Distinct();
            foreach (object[] results in UnorderedSources.Ranges(counts.Cast<int>(), positions)) yield return results;
        }

        [Theory]
        [MemberData(nameof(UnorderedSources.Ranges), new[] { 0, 1, 2, 16 }, MemberType = typeof(UnorderedSources))]
        public static void MaxBy(Labeled<ParallelQuery<int>> labeled, int count)
        {
            ParallelQuery<int> query = labeled.Item;
            Assert.Equal(count - 1, query.MaxBy(x => x));
            Assert.Equal(0, query.MaxBy(x => -x));
        }

        [Theory]
        [OuterLoop]
        [MemberData(nameof(UnorderedSources.Ranges), new[] { 1024 * 32, 1024 * 1024 }, MemberType = typeof(UnorderedSources))]
        public static void MaxBy_Longrunning(Labeled<ParallelQuery<int>> labeled, int count)
        {
            MaxBy(labeled, count);
        }

        [Theory]
        [MemberData(nameof(OnlyOneData), new[] { 2, 16 })]
        public static void MaxBy_Specific(Labeled<ParallelQuery<int>> labeled, int count, int position)
        {
            ParallelQuery<int> query = labeled.Item;
            Assert.Equal(position, query.MaxBy(x => x == position ? int.MaxValue : x));
            Assert.Equal(position, query.MaxBy(x => x == position ? 1 : -x));
        }

        [Theory]
        [OuterLoop]
        [MemberData(nameof(OnlyOneData), new[] { 1024 * 1024, 1024 * 1024 * 4 })]
        public static void MaxBy_Specific_Longrunning(Labeled<ParallelQuery<int>> labeled, int count, int position)
        {
            MaxBy_Specific(labeled, count, position);
        }

        [Theory]
        [MemberData(nameof(UnorderedSources.Ranges), new[] { 0, 1, 2, 16 }, MemberType = typeof(UnorderedSources))]
        public static void MaxBy_CustomComparer(Labeled<ParallelQuery<int>> labeled, int count)
        {
            ParallelQuery<int> query = labeled.Item;
            Assert.Equal(0, query.MaxBy(x => x, ReverseComparer.Instance));
            Assert.Equal(count - 1, query.MaxBy(x => -x, ReverseComparer.Instance));
        }

        [Theory]
        [OuterLoop]
        [MemberData(nameof(UnorderedSources.Ranges), new[] { 1024 * 32, 1024 * 1024 }, MemberType = typeof(UnorderedSources))]
        public static void MaxBy_CustomComparator_Longrunning(Labeled<ParallelQuery<int>> labeled, int count)
        {
            MaxBy_CustomComparer(labeled, count);
        }

        [Theory]
        [MemberData(nameof(OnlyOneData), new[] { 2, 16 })]
        public static void MaxBy_Specific_CustomComparator(Labeled<ParallelQuery<int>> labeled, int count, int position)
        {
            ParallelQuery<int> query = labeled.Item;
            Assert.Equal(position, query.MaxBy(x => x == position ? (int?)null : x, new ReverseComparer<int?>()));
            Assert.Equal(position, query.MaxBy(x => x == position ? (int?)null : -x, new ReverseComparer<int?>()));
        }

        [Theory]
        [OuterLoop]
        [MemberData(nameof(OnlyOneData), new[] { 1024 * 1024, 1024 * 1024 * 4 })]
        public static void MaxBy_Specific_CustomComparator_Longrunning(Labeled<ParallelQuery<int>> labeled, int count, int position)
        {
            MaxBy_Specific_CustomComparator(labeled, count, position);
        }

        [Theory]
        [MemberData(nameof(UnorderedSources.Ranges), new[] { 1, 2, 16 }, MemberType = typeof(UnorderedSources))]
        public static void MaxBy_Duplicate(Labeled<ParallelQuery<int>> labeled, int count)
        {
            Assert.InRange(labeled.Item.MaxBy(x => 0), 0, count - 1);
            Assert.InRange(labeled.Item.MaxBy(x => (int?)null), 0, count - 1);
            Assert.InRange(labeled.Item.MaxBy(x => x, new ModularCongruenceComparer(1)), 0, count - 1);
        }

        [Theory]
        [MemberData(nameof(UnorderedSources.Ranges), new[] { 1 }, MemberType = typeof(UnorderedSources))]
        public static void MaxBy_OperationCanceledException_PreCanceled(Labeled<ParallelQuery<int>> labeled, int count)
        {
            CancellationTokenSource cs = new CancellationTokenSource();
            cs.Cancel();

            Functions.AssertIsCanceled(cs, () => labeled.Item.WithCancellation(cs.Token).MaxBy(x => x));
            Functions.AssertIsCanceled(cs, () => labeled.Item.WithCancellation(cs.Token).MaxBy(x => x, Comparer<int>.Default));
        }

        [Theory]
        [MemberData(nameof(UnorderedSources.Ranges), new[] { 2 }, MemberType = typeof(UnorderedSources))]
        public static void MaxBy_AggregateException(Labeled<ParallelQuery<int>> labeled, int count)
        {
            Functions.AssertThrowsWrapped<DeliberateTestException>(() => labeled.Item.MaxBy((Func<int, int>)(x => { throw new DeliberateTestException(); })));
            Functions.AssertThrowsWrapped<DeliberateTestException>(() => labeled.Item.MaxBy(x => x, new FailingComparer()));
        }

        [Theory]
        [MemberData(nameof(UnorderedSources.Ranges), new[] { 2 }, MemberType = typeof(UnorderedSources))]
        public static void MaxBy_AggregateException_NotComparable(Labeled<ParallelQuery<int>> labeled, int count)
        {
            Functions.AssertThrowsWrapped<ArgumentException>(() => labeled.Item.MaxBy(x => new NotComparable(x)));
        }

        [Fact]
        public static void MaxBy_ArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => ((ParallelQuery<int>)null).MaxBy(x => x));
            Assert.Throws<ArgumentNullException>(() => ParallelEnumerable.Range(0, 1).MaxBy((Func<int, int>)null));

            Assert.Throws<ArgumentNullException>(() => ((ParallelQuery<int>)null).MaxBy(x => x, Comparer<int>.Default));
            Assert.Throws<ArgumentNullException>(() => ParallelEnumerable.Range(0, 1).MaxBy((Func<int, int>)null, Comparer<int>.Default));
        }

        private class NotComparable
        {
            private int x;

            public NotComparable(int x)
            {
                this.x = x;
            }
        }
    }
}
