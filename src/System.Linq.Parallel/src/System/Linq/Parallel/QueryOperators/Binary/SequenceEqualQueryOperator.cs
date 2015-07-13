// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// SequenceEqualQueryOperator.cs
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace System.Linq.Parallel
{
    /// <summary>
    /// A SequenceEqual operator combines two input data sources into a single output result.
    /// Because the expectation is that each element
    /// is matched with the element in the other data source at the same ordinal
    /// position, the SequenceEqual operator requires order preservation.
    /// </summary>
    /// <typeparam name="TLeftInput"></typeparam>
    /// <typeparam name="TRightInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    internal sealed class SequenceEqualQueryOperator<TInput> : QueryOperator<bool>
    {
        private readonly IEqualityComparer<TInput> _comparer; // To compare child elements.
        private readonly QueryOperator<TInput> _leftChild;
        private readonly QueryOperator<TInput> _rightChild;
        private readonly bool _prematureMergeLeft = false; // Whether to prematurely merge the left data source
        private readonly bool _prematureMergeRight = false; // Whether to prematurely merge the right data source
        private readonly bool _limitsParallelism = false; // Whether this operator limits parallelism

        //---------------------------------------------------------------------------------------
        // Initializes a new SequenceEqual operator.
        //
        // Arguments:
        //    leftChild     - the left data source from which to pull data.
        //    rightChild    - the right data source from which to pull data.
        //

        internal SequenceEqualQueryOperator(
            ParallelQuery<TInput> leftChildSource, ParallelQuery<TInput> rightChildSource,
            IEqualityComparer<TInput> comparer)
            : this(
                QueryOperator<TInput>.AsQueryOperator(leftChildSource),
                QueryOperator<TInput>.AsQueryOperator(rightChildSource),
                comparer)
        {
        }

        private SequenceEqualQueryOperator(
            QueryOperator<TInput> left, QueryOperator<TInput> right,
           IEqualityComparer<TInput> comparer)
            : base(left.SpecifiedQuerySettings.Merge(right.SpecifiedQuerySettings))
        {
            _leftChild = left;
            _rightChild = right;
            _comparer = comparer ?? EqualityComparer<TInput>.Default;
            _outputOrdered = false;

            OrdinalIndexState leftIndexState = _leftChild.OrdinalIndexState;
            OrdinalIndexState rightIndexState = _rightChild.OrdinalIndexState;

            _prematureMergeLeft = leftIndexState != OrdinalIndexState.Indexable;
            _prematureMergeRight = rightIndexState != OrdinalIndexState.Indexable;
            _limitsParallelism =
                (_prematureMergeLeft && leftIndexState != OrdinalIndexState.Shuffled)
                || (_prematureMergeRight && rightIndexState != OrdinalIndexState.Shuffled);
        }

        //---------------------------------------------------------------------------------------
        // Just opens the current operator, including opening the children and wrapping them with
        // partitions as needed.
        //

        internal override QueryResults<bool> Open(QuerySettings settings, bool preferStriping)
        {
            // We just open our child operators, left and then right.
            QueryResults<TInput> leftChildResults = _leftChild.Open(settings, preferStriping);
            QueryResults<TInput> rightChildResults = _rightChild.Open(settings, preferStriping);

            int partitionCount = settings.DegreeOfParallelism.Value;
            if (_prematureMergeLeft)
            {
                PartitionedStreamMerger<TInput> merger = new PartitionedStreamMerger<TInput>(
                    false, ParallelMergeOptions.FullyBuffered, settings.TaskScheduler, _leftChild.OutputOrdered,
                    settings.CancellationState, settings.QueryId);
                leftChildResults.GivePartitionedStream(merger);
                leftChildResults = new ListQueryResults<TInput>(
                    merger.MergeExecutor.GetResultsAsArray(), partitionCount, preferStriping);
            }

            if (_prematureMergeRight)
            {
                PartitionedStreamMerger<TInput> merger = new PartitionedStreamMerger<TInput>(
                    false, ParallelMergeOptions.FullyBuffered, settings.TaskScheduler, _rightChild.OutputOrdered,
                    settings.CancellationState, settings.QueryId);
                rightChildResults.GivePartitionedStream(merger);
                rightChildResults = new ListQueryResults<TInput>(
                    merger.MergeExecutor.GetResultsAsArray(), partitionCount, preferStriping);
            }

            return new SequenceEqualQueryOperatorResults(leftChildResults, rightChildResults, _comparer, partitionCount, preferStriping);
        }

        //---------------------------------------------------------------------------------------
        // Returns an enumerable that represents the query executing sequentially.
        // Will yield a single-element enumerable with either true or false.

        internal override IEnumerable<bool> AsSequentialQuery(CancellationToken token)
        {
            using (IEnumerator<TInput> leftEnumerator = _leftChild.AsSequentialQuery(token).GetEnumerator())
            using (IEnumerator<TInput> rightEnumerator = _rightChild.AsSequentialQuery(token).GetEnumerator())
            {
                while (leftEnumerator.MoveNext())
                {
                    if (!(rightEnumerator.MoveNext() && _comparer.Equals(leftEnumerator.Current, rightEnumerator.Current)))
                    {
                        // Either the right source is not as long, or the comparison returned false; return a single element.
                        yield return false;
                        yield break;
                    }
                }
                yield return !rightEnumerator.MoveNext();
            }
        }

        //---------------------------------------------------------------------------------------
        // The state of the order index of the results returned by this operator.
        //

        internal override OrdinalIndexState OrdinalIndexState
        {
            get
            {
                return OrdinalIndexState.Indexable;
            }
        }

        //---------------------------------------------------------------------------------------
        // Whether this operator performs a premature merge that would not be performed in
        // a similar sequential operation (i.e., in LINQ to Objects).
        //

        internal override bool LimitsParallelism
        {
            get
            {
                return _limitsParallelism;
            }
        }

        //---------------------------------------------------------------------------------------
        // A special QueryResults class for the SequenceEqual operator. It requires that both of the child
        // QueryResults are indexible.
        //

        internal class SequenceEqualQueryOperatorResults : QueryResults<bool>
        {
            private readonly QueryResults<TInput> _leftChildResults;
            private readonly QueryResults<TInput> _rightChildResults;
            private readonly IEqualityComparer<TInput> _comparer; // To compare child elements.
            private readonly int _count;
            private readonly bool _sameSize;
            private readonly int _partitionCount;
            private readonly bool _preferStriping;

            internal SequenceEqualQueryOperatorResults(
                QueryResults<TInput> leftChildResults, QueryResults<TInput> rightChildResults,
                IEqualityComparer<TInput> comparer, int partitionCount, bool preferStriping)
            {
                _leftChildResults = leftChildResults;
                _rightChildResults = rightChildResults;
                _comparer = comparer;
                _partitionCount = partitionCount;
                _preferStriping = preferStriping;

                Debug.Assert(_leftChildResults.IsIndexible);
                Debug.Assert(_rightChildResults.IsIndexible);

                int leftCount = _leftChildResults.Count;
                int rightCount = _rightChildResults.Count;

                _count = Math.Max(leftCount, rightCount);
                _sameSize = leftCount == rightCount;
            }

            internal override int ElementsCount
            {
                get { return _count; }
            }

            internal override bool IsIndexible
            {
                get { return true; }
            }

            internal override bool GetElement(int index)
            {
                return _sameSize && _comparer.Equals(_leftChildResults.GetElement(index), _rightChildResults.GetElement(index));
            }

            internal override void GivePartitionedStream(IPartitionedStreamRecipient<bool> recipient)
            {
                PartitionedStream<bool, int> partitionedStream = ExchangeUtilities.PartitionDataSource(this, _partitionCount, _preferStriping);
                recipient.Receive(partitionedStream);
            }
        }
    }
}
