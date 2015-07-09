// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;

namespace System.Linq.Parallel.Tests
{
    public class ChunkTests
    {
        private const int ChunkSize = 8;

        [Theory]
        [MemberData(nameof(UnorderedSources.Ranges), new[] { 0, 1, 2, ChunkSize - 1, ChunkSize, ChunkSize * 2 - 1, ChunkSize * 2 }, MemberType = typeof(UnorderedSources))]
        public static void Chunk_Unordered(Labeled<ParallelQuery<int>> labeled, int count)
        {
            ParallelQuery<int> query = labeled.Item;
            IntegerRangeSet seen = new IntegerRangeSet(0, count);
            // When unordered, there are no guarantees about how many chunks there are,
            // or if there are elements in a chunk.
            foreach (ParallelQuery<int> chunk in query.Chunk(ChunkSize))
            {
                int elements = 0;
                foreach (int i in chunk)
                {
                    seen.Add(i);
                    elements++;
                }
                Assert.InRange(elements, 0, ChunkSize);
            }
            seen.AssertComplete();
        }

        [Theory]
        [OuterLoop]
        [MemberData(nameof(UnorderedSources.Ranges), new[] { 1024, 1024 * 16 }, MemberType = typeof(UnorderedSources))]
        public static void Chunk_Unordered_Longrunning(Labeled<ParallelQuery<int>> labeled, int count)
        {
            Chunk_Unordered(labeled, count);
        }

        [Theory]
        [MemberData(nameof(Sources.Ranges), new[] { 0, 1, 2, ChunkSize - 1, ChunkSize, ChunkSize * 2 - 1, ChunkSize * 2 }, MemberType = typeof(Sources))]
        public static void Chunk(Labeled<ParallelQuery<int>> labeled, int count)
        {
            ParallelQuery<int> query = labeled.Item;
            int seen = 0;
            int chunks = 0;
            foreach (ParallelQuery<int> chunk in query.Chunk(ChunkSize))
            {
                chunks++;

                foreach (int i in chunk)
                {
                    Assert.Equal(seen++, i);
                }
                Assert.Equal(Math.Min(chunks * ChunkSize, count), seen);
            }

            Assert.Equal((count + ChunkSize - 1) / ChunkSize, chunks);
        }

        [Theory]
        [OuterLoop]
        [MemberData(nameof(Sources.Ranges), new[] { 1024, 1024 * 16 }, MemberType = typeof(Sources))]
        public static void Chunk_Longrunning(Labeled<ParallelQuery<int>> labeled, int count)
        {
            Chunk(labeled, count);
        }

        [Theory]
        [MemberData(nameof(UnorderedSources.Ranges), new[] { 0, 1, 2, ChunkSize - 1, ChunkSize, ChunkSize * 2 - 1, ChunkSize * 2 }, MemberType = typeof(UnorderedSources))]
        public static void Chunk_Unordered_NotPipelined(Labeled<ParallelQuery<int>> labeled, int count)
        {
            ParallelQuery<int> query = labeled.Item;
            IntegerRangeSet seen = new IntegerRangeSet(0, count);
            foreach (ParallelQuery<int> chunk in query.Chunk(ChunkSize).ToList())
            {
                int elements = 0;
                foreach (int i in chunk.ToList())
                {
                    seen.Add(i);
                    elements++;
                }
                Assert.InRange(elements, 0, ChunkSize);
            }
            seen.AssertComplete();
        }

        [Theory]
        [OuterLoop]
        [MemberData(nameof(UnorderedSources.Ranges), new[] { 1024, 1024 * 16 }, MemberType = typeof(UnorderedSources))]
        public static void Chunk_Unordered_NotPipelined_Longrunning(Labeled<ParallelQuery<int>> labeled, int count)
        {
            Chunk_Unordered_NotPipelined(labeled, count);
        }

        [Theory]
        [MemberData(nameof(Sources.Ranges), new[] { 0, 1, 2, ChunkSize - 1, ChunkSize, ChunkSize * 2 - 1, ChunkSize * 2 }, MemberType = typeof(Sources))]
        public static void Chunk_NotPipelined(Labeled<ParallelQuery<int>> labeled, int count)
        {
            ParallelQuery<int> query = labeled.Item;
            int seen = 0;
            int chunks = 0;
            foreach (ParallelQuery<int> chunk in query.Chunk(ChunkSize).ToList())
            {
                chunks++;

                foreach (int i in chunk.ToList())
                {
                    Assert.Equal(seen++, i);
                }
                Assert.Equal(Math.Min(chunks * ChunkSize, count), seen);
            }

            Assert.Equal((count + ChunkSize - 1) / ChunkSize, chunks);
        }

        [Theory]
        [OuterLoop]
        [MemberData(nameof(Sources.Ranges), new[] { 1024, 1024 * 16 }, MemberType = typeof(Sources))]
        public static void Chunk_NotPipelined_Longrunning(Labeled<ParallelQuery<int>> labeled, int count)
        {
            Chunk_NotPipelined(labeled, count);
        }

        [Theory]
        [MemberData(nameof(Sources.Ranges), new[] { 0, 1, 2, 16 }, MemberType = typeof(Sources))]
        public static void Chunk_ArgumentException(Labeled<ParallelQuery<int>> labeled, int count)
        {
            Assert.Throws<ArgumentException>(() => labeled.Item.Chunk(0));
            Assert.Throws<ArgumentException>(() => labeled.Item.Chunk(-1));
            Assert.Throws<ArgumentException>(() => labeled.Item.Chunk(int.MinValue));
        }

        [Fact]
        public static void Chunk_ArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => ((ParallelQuery<int>)null).Chunk(1));
        }
    }
}
