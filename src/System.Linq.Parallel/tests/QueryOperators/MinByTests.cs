// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Threading;
using Xunit;

namespace System.Linq.Parallel.Tests
{
    public class MinByTests
    {
        public static IEnumerable<object[]> OnlyOneData(int[] counts)
        {
            Func<int, IEnumerable<int>> positions = x => new[] { 0, x / 2, Math.Max(0, x - 1) }.Distinct();
            foreach (object[] results in UnorderedSources.Ranges(counts.Cast<int>(), positions)) yield return results;
        }

        [Theory]
        [MemberData(nameof(UnorderedSources.Ranges), new[] { 0, 1, 2, 16 }, MemberType = typeof(UnorderedSources))]
        public static void MinBy(Labeled<ParallelQuery<int>> labeled, int count)
        {
            ParallelQuery<int> query = labeled.Item;
            Assert.Equal(0, query.MinBy(x => x));
            Assert.Equal(count - 1, query.MinBy(x => -x));
        }

        [Theory]
        [OuterLoop]
        [MemberData(nameof(UnorderedSources.Ranges), new[] { 1024 * 32, 1024 * 1024 }, MemberType = typeof(UnorderedSources))]
        public static void MinBy_Longrunning(Labeled<ParallelQuery<int>> labeled, int count)
        {
            MinBy(labeled, count);
        }

        [Theory]
        [MemberData(nameof(OnlyOneData), new[] { 2, 16 })]
        public static void MinBy_Specific(Labeled<ParallelQuery<int>> labeled, int count, int position)
        {
            ParallelQuery<int> query = labeled.Item;
            Assert.Equal(position, query.MinBy(x => x == position ? int.MinValue : x));
            Assert.Equal(position, query.MinBy(x => x == position ? int.MinValue : -x));
            Assert.Equal(position, query.MinBy(x => x == position ? (int?)null : x));
            Assert.Equal(position, query.MinBy(x => x == position ? (int?)null : -x));
            Assert.Equal(position, query.MinBy(x => x == position ? (int?)null : int.MinValue));
        }

        [Theory]
        [OuterLoop]
        [MemberData(nameof(OnlyOneData), new[] { 1024 * 1024, 1024 * 1024 * 4 })]
        public static void MinBy_Specific_Longrunning(Labeled<ParallelQuery<int>> labeled, int count, int position)
        {
            MinBy_Specific(labeled, count, position);
        }

        [Theory]
        [MemberData(nameof(UnorderedSources.Ranges), new[] { 0, 1, 2, 16 }, MemberType = typeof(UnorderedSources))]
        public static void MinBy_CustomComparer(Labeled<ParallelQuery<int>> labeled, int count)
        {
            ParallelQuery<int> query = labeled.Item;
            Assert.Equal(count - 1, query.MinBy(x => x, ReverseComparer.Instance));
            Assert.Equal(0, query.MinBy(x => -x, ReverseComparer.Instance));
        }

        [Theory]
        [OuterLoop]
        [MemberData(nameof(UnorderedSources.Ranges), new[] { 1024 * 32, 1024 * 1024 }, MemberType = typeof(UnorderedSources))]
        public static void MinBy_CustomComparator_Longrunning(Labeled<ParallelQuery<int>> labeled, int count)
        {
            MinBy_CustomComparer(labeled, count);
        }

        [Theory]
        [MemberData(nameof(OnlyOneData), new[] { 2, 16 })]
        public static void MinBy_Specific_CustomComparator(Labeled<ParallelQuery<int>> labeled, int count, int position)
        {
            ParallelQuery<int> query = labeled.Item;
            Assert.Equal(position, query.MinBy(x => x == position ? x : -x, new ReverseComparer<int?>()));
            Assert.Equal(position, query.MinBy(x => x == position ? x : (int?)null, new ReverseComparer<int?>()));
        }

        [Theory]
        [OuterLoop]
        [MemberData(nameof(OnlyOneData), new[] { 1024 * 1024, 1024 * 1024 * 4 })]
        public static void MinBy_Specific_CustomComparator_Longrunning(Labeled<ParallelQuery<int>> labeled, int count, int position)
        {
            MinBy_Specific_CustomComparator(labeled, count, position);
        }

        [Theory]
        [MemberData(nameof(UnorderedSources.Ranges), new[] { 1, 2, 16 }, MemberType = typeof(UnorderedSources))]
        public static void MinBy_Duplicate(Labeled<ParallelQuery<int>> labeled, int count)
        {
            Assert.InRange(labeled.Item.MinBy(x => 0), 0, count - 1);
            Assert.InRange(labeled.Item.MinBy(x => (int?)null), 0, count - 1);
            Assert.InRange(labeled.Item.MinBy(x => x, new ModularCongruenceComparer(1)), 0, count - 1);
        }

        [Theory]
        [MemberData(nameof(UnorderedSources.Ranges), new[] { 1 }, MemberType = typeof(UnorderedSources))]
        public static void MinBy_OperationCanceledException_PreCanceled(Labeled<ParallelQuery<int>> labeled, int count)
        {
            CancellationTokenSource cs = new CancellationTokenSource();
            cs.Cancel();

            Functions.AssertIsCanceled(cs, () => labeled.Item.WithCancellation(cs.Token).MinBy(x => x));
            Functions.AssertIsCanceled(cs, () => labeled.Item.WithCancellation(cs.Token).MinBy(x => x, Comparer<int>.Default));
        }

        [Theory]
        [MemberData(nameof(UnorderedSources.Ranges), new[] { 2 }, MemberType = typeof(UnorderedSources))]
        public static void MinBy_AggregateException(Labeled<ParallelQuery<int>> labeled, int count)
        {
            Functions.AssertThrowsWrapped<DeliberateTestException>(() => labeled.Item.MinBy((Func<int, int>)(x => { throw new DeliberateTestException(); })));
            Functions.AssertThrowsWrapped<DeliberateTestException>(() => labeled.Item.MinBy(x => x, new FailingComparer()));
        }

        [Theory]
        [MemberData(nameof(UnorderedSources.Ranges), new[] { 2 }, MemberType = typeof(UnorderedSources))]
        public static void MinBy_AggregateException_NotComparable(Labeled<ParallelQuery<int>> labeled, int count)
        {
            Functions.AssertThrowsWrapped<ArgumentException>(() => labeled.Item.MinBy(x => new NotComparable(x)));
        }

        [Fact]
        public static void MinBy_ArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => ((ParallelQuery<int>)null).MinBy(x => x));
            Assert.Throws<ArgumentNullException>(() => ParallelEnumerable.Range(0, 1).MinBy((Func<int, int>)null));

            Assert.Throws<ArgumentNullException>(() => ((ParallelQuery<int>)null).MinBy(x => x, Comparer<int>.Default));
            Assert.Throws<ArgumentNullException>(() => ParallelEnumerable.Range(0, 1).MinBy((Func<int, int>)null, Comparer<int>.Default));
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
