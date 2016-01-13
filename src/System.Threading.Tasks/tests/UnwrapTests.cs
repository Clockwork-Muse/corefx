// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Xunit;

namespace System.Threading.Tasks.Tests
{
    public class UnwrapTests
    {
        /// <summary>Tests unwrap argument validation.</summary>
        [Fact]
        public void ArgumentValidation()
        {
            Assert.Throws<ArgumentNullException>(() => { ((Task<Task>)null).Unwrap(); });
            Assert.Throws<ArgumentNullException>(() => { ((Task<Task<int>>)null).Unwrap(); });
            Assert.Throws<ArgumentNullException>(() => { ((Task<Task<string>>)null).Unwrap(); });
        }

        /// <summary>
        /// Tests Unwrap when both the outer task and non-generic inner task have completed by the time Unwrap is called.
        /// </summary>
        /// <param name="inner">Will be run with a RanToCompletion, Faulted, and Canceled task.</param>
        [Theory]
        [MemberData(nameof(CompletedNonGenericTasks))]
        public void NonGeneric_Completed_Completed(Task inner) 
        {
            Task<Task> outer = Task.FromResult(inner);
            Task unwrappedInner = outer.Unwrap();
            Assert.True(unwrappedInner.IsCompleted);
            Assert.Same(inner, unwrappedInner);
            AssertTasksAreEqual(inner, unwrappedInner);
        }

        /// <summary>
        /// Tests Unwrap when both the outer task and generic inner task have completed by the time Unwrap is called.
        /// </summary>
        /// <param name="inner">The inner task.</param>
        [Theory]
        [MemberData(nameof(CompletedStringTasks))]
        public void Generic_Completed_Completed(Task<string> inner)
        {
            Task<Task<string>> outer = Task.FromResult(inner);
            Task<string> unwrappedInner = outer.Unwrap();
            Assert.True(unwrappedInner.IsCompleted);
            Assert.Same(inner, unwrappedInner);
            AssertTasksAreEqual(inner, unwrappedInner);
        }

        /// <summary>
        /// Tests Unwrap when the non-generic inner task has completed but the outer task has not completed by the time Unwrap is called.
        /// </summary>
        /// <param name="inner">The inner task.</param>
        [Theory]
        [MemberData(nameof(CompletedNonGenericTasks))]
        public void NonGeneric_NotCompleted_Completed(Task inner) 
        {
            var outerTcs = new TaskCompletionSource<Task>();
            Task<Task> outer = outerTcs.Task;

            Task unwrappedInner = outer.Unwrap();
            Assert.False(unwrappedInner.IsCompleted);

            outerTcs.SetResult(inner);
            AssertTasksAreEqual(inner, unwrappedInner);
        }

        /// <summary>
        /// Tests Unwrap when the generic inner task has completed but the outer task has not completed by the time Unwrap is called.
        /// </summary>
        /// <param name="inner">The inner task.</param>
        [Theory]
        [MemberData(nameof(CompletedStringTasks))]
        public void Generic_NotCompleted_Completed(Task<string> inner)
        {
            var outerTcs = new TaskCompletionSource<Task<string>>();
            Task<Task<string>> outer = outerTcs.Task;

            Task<string> unwrappedInner = outer.Unwrap();
            Assert.False(unwrappedInner.IsCompleted);

            outerTcs.SetResult(inner);
            AssertTasksAreEqual(inner, unwrappedInner);
        }

        /// <summary>
        /// Tests Unwrap when the non-generic inner task has not yet completed but the outer task has completed by the time Unwrap is called.
        /// </summary>
        /// <param name="innerStatus">How the inner task should be completed.</param>
        [Theory]
        [InlineData(TaskStatus.RanToCompletion)]
        [InlineData(TaskStatus.Faulted)]
        [InlineData(TaskStatus.Canceled)]
        public void NonGeneric_Completed_NotCompleted(TaskStatus innerStatus) 
        {
            var innerTcs = new TaskCompletionSource<bool>();
            Task inner = innerTcs.Task;

            Task<Task> outer = Task.FromResult(inner);
            Task unwrappedInner = outer.Unwrap();
            Assert.False(unwrappedInner.IsCompleted);

            switch (innerStatus)
            {
                case TaskStatus.RanToCompletion:
                    innerTcs.SetResult(true);
                    break;
                case TaskStatus.Faulted:
                    innerTcs.SetException(new DeliberateTestException());
                    break;
                case TaskStatus.Canceled:
                    innerTcs.SetCanceled();
                    break;
            }

            AssertTasksAreEqual(inner, unwrappedInner);
        }

        /// <summary>
        /// Tests Unwrap when the non-generic inner task has not yet completed but the outer task has completed by the time Unwrap is called.
        /// </summary>
        /// <param name="innerStatus">How the inner task should be completed.</param>
        [Theory]
        [InlineData(TaskStatus.RanToCompletion)]
        [InlineData(TaskStatus.Faulted)]
        [InlineData(TaskStatus.Canceled)]
        public void Generic_Completed_NotCompleted(TaskStatus innerStatus)
        {
            var innerTcs = new TaskCompletionSource<int>();
            Task<int> inner = innerTcs.Task;

            Task<Task<int>> outer = Task.FromResult(inner);
            Task<int> unwrappedInner = outer.Unwrap();
            Assert.False(unwrappedInner.IsCompleted);

            switch (innerStatus)
            {
                case TaskStatus.RanToCompletion:
                    innerTcs.SetResult(42);
                    break;
                case TaskStatus.Faulted:
                    innerTcs.SetException(new DeliberateTestException());
                    break;
                case TaskStatus.Canceled:
                    innerTcs.SetCanceled();
                    break;
            }

            AssertTasksAreEqual(inner, unwrappedInner);
        }

        /// <summary>
        /// Tests Unwrap when neither the non-generic inner task nor the outer task has completed by the time Unwrap is called.
        /// </summary>
        /// <param name="outerCompletesFirst">Whether the outer task or the inner task completes first.</param>
        /// <param name="innerStatus">How the inner task should be completed.</param>
        [Theory]
        [InlineData(true, TaskStatus.RanToCompletion)]
        [InlineData(true, TaskStatus.Canceled)]
        [InlineData(true, TaskStatus.Faulted)]
        [InlineData(false, TaskStatus.RanToCompletion)]
        [InlineData(false, TaskStatus.Canceled)]
        [InlineData(false, TaskStatus.Faulted)]
        public void NonGeneric_NotCompleted_NotCompleted(bool outerCompletesFirst, TaskStatus innerStatus) 
        {
            var innerTcs = new TaskCompletionSource<bool>();
            Task inner = innerTcs.Task;

            var outerTcs = new TaskCompletionSource<Task>();
            Task<Task> outer = outerTcs.Task;

            Task unwrappedInner = outer.Unwrap();
            Assert.False(unwrappedInner.IsCompleted);

            if (outerCompletesFirst)
            {
                outerTcs.SetResult(inner);
                Assert.False(unwrappedInner.IsCompleted);
            }

            switch (innerStatus)
            {
                case TaskStatus.RanToCompletion:
                    innerTcs.SetResult(true);
                    break;
                case TaskStatus.Faulted:
                    innerTcs.SetException(new DeliberateTestException());
                    break;
                case TaskStatus.Canceled:
                    innerTcs.TrySetCanceled(CreateCanceledToken());
                    break;
            }
            
            if (!outerCompletesFirst)
            {
                Assert.False(unwrappedInner.IsCompleted);
                outerTcs.SetResult(inner);
            }

            AssertTasksAreEqual(inner, unwrappedInner);
        }

        /// <summary>
        /// Tests Unwrap when neither the generic inner task nor the outer task has completed by the time Unwrap is called.
        /// </summary>
        /// <param name="outerCompletesFirst">Whether the outer task or the inner task completes first.</param>
        /// <param name="innerStatus">How the inner task should be completed.</param>
        [Theory]
        [InlineData(true, TaskStatus.RanToCompletion)]
        [InlineData(true, TaskStatus.Canceled)]
        [InlineData(true, TaskStatus.Faulted)]
        [InlineData(false, TaskStatus.RanToCompletion)]
        [InlineData(false, TaskStatus.Canceled)]
        [InlineData(false, TaskStatus.Faulted)]
        public void Generic_NotCompleted_NotCompleted(bool outerCompletesFirst, TaskStatus innerStatus)
        {
            var innerTcs = new TaskCompletionSource<int>();
            Task<int> inner = innerTcs.Task;

            var outerTcs = new TaskCompletionSource<Task<int>>();
            Task<Task<int>> outer = outerTcs.Task;

            Task<int> unwrappedInner = outer.Unwrap();
            Assert.False(unwrappedInner.IsCompleted);

            if (outerCompletesFirst)
            {
                outerTcs.SetResult(inner);
                Assert.False(unwrappedInner.IsCompleted);
            }

            switch (innerStatus)
            {
                case TaskStatus.RanToCompletion:
                    innerTcs.SetResult(42);
                    break;
                case TaskStatus.Faulted:
                    innerTcs.SetException(new DeliberateTestException());
                    break;
                case TaskStatus.Canceled:
                    innerTcs.TrySetCanceled(CreateCanceledToken());
                    break;
            }

            if (!outerCompletesFirst)
            {
                Assert.False(unwrappedInner.IsCompleted);
                outerTcs.SetResult(inner);
            }

            AssertTasksAreEqual(inner, unwrappedInner);
        }

        /// <summary>
        /// Tests Unwrap when the outer task for a non-generic inner fails in some way.
        /// </summary>
        /// <param name="outerCompletesFirst">Whether the outer task completes before Unwrap is called.</param>
        /// <param name="outerStatus">How the outer task should be completed (RanToCompletion means returning null).</param>
        [Theory]
        [InlineData(true, TaskStatus.RanToCompletion)]
        [InlineData(true, TaskStatus.Faulted)]
        [InlineData(true, TaskStatus.Canceled)]
        [InlineData(false, TaskStatus.RanToCompletion)]
        [InlineData(false, TaskStatus.Faulted)]
        [InlineData(false, TaskStatus.Canceled)]
        public void NonGeneric_UnsuccessfulOuter(bool outerCompletesBeforeUnwrap, TaskStatus outerStatus)
        {
            var outerTcs = new TaskCompletionSource<Task>();
            Task<Task> outer = outerTcs.Task;

            Task unwrappedInner = null;

            if (!outerCompletesBeforeUnwrap)
                unwrappedInner = outer.Unwrap();

            switch (outerStatus)
            {
                case TaskStatus.RanToCompletion:
                    outerTcs.SetResult(null);
                    break;
                case TaskStatus.Canceled:
                    outerTcs.TrySetCanceled(CreateCanceledToken());
                    break;
                case TaskStatus.Faulted:
                    outerTcs.SetException(new DeliberateTestException());
                    break;
            }

            if (outerCompletesBeforeUnwrap)
                unwrappedInner = outer.Unwrap();

            WaitNoThrow(unwrappedInner);

            switch (outerStatus)
            {
                case TaskStatus.RanToCompletion:
                    Assert.True(unwrappedInner.IsCanceled);
                    break;
                default:
                    AssertTasksAreEqual(outer, unwrappedInner);
                    break;
            }
        }

        /// <summary>
        /// Tests Unwrap when the outer task for a generic inner fails in some way.
        /// </summary>
        /// <param name="outerCompletesFirst">Whether the outer task completes before Unwrap is called.</param>
        /// <param name="outerStatus">How the outer task should be completed (RanToCompletion means returning null).</param>
        [Theory]
        [InlineData(true, TaskStatus.RanToCompletion)]
        [InlineData(true, TaskStatus.Faulted)]
        [InlineData(true, TaskStatus.Canceled)]
        [InlineData(false, TaskStatus.RanToCompletion)]
        [InlineData(false, TaskStatus.Faulted)]
        [InlineData(false, TaskStatus.Canceled)]
        public void Generic_UnsuccessfulOuter(bool outerCompletesBeforeUnwrap, TaskStatus outerStatus)
        {
            var outerTcs = new TaskCompletionSource<Task<int>>();
            Task<Task<int>> outer = outerTcs.Task;

            Task<int> unwrappedInner = null;

            if (!outerCompletesBeforeUnwrap)
                unwrappedInner = outer.Unwrap();

            switch (outerStatus)
            {
                case TaskStatus.RanToCompletion:
                    outerTcs.SetResult(null); // cancellation
                    break;
                case TaskStatus.Canceled:
                    outerTcs.TrySetCanceled(CreateCanceledToken());
                    break;
                case TaskStatus.Faulted:
                    outerTcs.SetException(new DeliberateTestException());
                    break;
            }

            if (outerCompletesBeforeUnwrap)
                unwrappedInner = outer.Unwrap();

            WaitNoThrow(unwrappedInner);

            switch (outerStatus)
            {
                case TaskStatus.RanToCompletion:
                    Assert.True(unwrappedInner.IsCanceled);
                    break;
                default:
                    AssertTasksAreEqual(outer, unwrappedInner);
                    break;
            }
        }

        /// <summary>
        /// Test Unwrap when the outer task for a non-generic inner task is marked as AttachedToParent.
        /// </summary>
        [Fact]
        public void NonGeneric_AttachedToParent()
        {
            Exception error = new InvalidTimeZoneException();
            Task parent = Task.Factory.StartNew(() =>
            {
                var outerTcs = new TaskCompletionSource<Task>(TaskCreationOptions.AttachedToParent);
                Task<Task> outer = outerTcs.Task;

                Task inner = Task.FromException(error);

                Task unwrappedInner = outer.Unwrap();
                Assert.Equal(TaskCreationOptions.AttachedToParent, unwrappedInner.CreationOptions);

                outerTcs.SetResult(inner);
                AssertTasksAreEqual(inner, unwrappedInner);
            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
            WaitNoThrow(parent);
            Assert.Equal(TaskStatus.Faulted, parent.Status);
            Assert.Same(error, parent.Exception.Flatten().InnerException);
        }

        /// <summary>
        /// Test Unwrap when the outer task for a generic inner task is marked as AttachedToParent.
        /// </summary>
        [Fact]
        public void Generic_AttachedToParent()
        {
            Exception error = new InvalidTimeZoneException();
            Task parent = Task.Factory.StartNew(() =>
            {
                var outerTcs = new TaskCompletionSource<Task<object>>(TaskCreationOptions.AttachedToParent);
                Task<Task<object>> outer = outerTcs.Task;

                Task<object> inner = Task.FromException<object>(error);

                Task<object> unwrappedInner = outer.Unwrap();
                Assert.Equal(TaskCreationOptions.AttachedToParent, unwrappedInner.CreationOptions);

                outerTcs.SetResult(inner);
                AssertTasksAreEqual(inner, unwrappedInner);
            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
            WaitNoThrow(parent);
            Assert.Equal(TaskStatus.Faulted, parent.Status);
            Assert.Same(error, parent.Exception.Flatten().InnerException);
        }

        /// <summary>
        /// Test that Unwrap with a non-generic task doesn't use TaskScheduler.Current.
        /// </summary>
        [Fact]
        public void NonGeneric_DefaultSchedulerUsed()
        {
            var scheduler = new QUWITaskScheduler();
            Task.Factory.StartNew(() =>
            {
                int initialCallCount = scheduler.QueueTaskCount;

                Task<Task> outer = Task.Factory.StartNew(() => Task.Run(() => { }),
                    CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
                Task unwrappedInner = outer.Unwrap();
                unwrappedInner.Wait();

                Assert.Equal(initialCallCount, scheduler.QueueTaskCount);
            }, CancellationToken.None, TaskCreationOptions.None, scheduler).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Test that Unwrap with a generic task doesn't use TaskScheduler.Current.
        /// </summary>
        [Fact]
        public void Generic_DefaultSchedulerUsed()
        {
            var scheduler = new QUWITaskScheduler();
            Task.Factory.StartNew(() =>
            {
                int initialCallCount = scheduler.QueueTaskCount;

                Task<Task<int>> outer = Task.Factory.StartNew(() => Task.Run(() => 42),
                    CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
                Task<int> unwrappedInner = outer.Unwrap();
                unwrappedInner.Wait();

                Assert.Equal(initialCallCount, scheduler.QueueTaskCount);
            }, CancellationToken.None, TaskCreationOptions.None, scheduler).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Test that a long chain of Unwraps can execute without overflowing the stack.
        /// </summary>
        [Fact]
        public void RunStackGuardTests()
        {
            const int DiveDepth = 12000;

            Func<int, Task<int>> func = null;
            func = count =>
                ++count < DiveDepth ?
                    Task.Factory.StartNew(() => func(count), CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default).Unwrap() :
                    Task.FromResult(count);

            // This test will overflow if it fails.
            Assert.Equal(DiveDepth, func(0).Result);
        }

        // Make sure that cancellation works for monadic versions of ContinueWith()
        [Fact]
        public static void RunUnwrapTests()
        {
            Task taskRoot = null;
            Task<int> futureRoot = null;

            Task<int> c1 = null;
            Task<int> c2 = null;
            Task<int> c3 = null;
            Task<int> c4 = null;
            Task c5 = null;
            Task c6 = null;
            Task c7 = null;
            Task c8 = null;

            //
            // Basic functionality tests
            //
            taskRoot = new Task(delegate { });
            futureRoot = new Task<int>(delegate { return 10; });
            ManualResetEvent mres = new ManualResetEvent(false);
            Action<Task, bool, string> checkCompletionState = delegate (Task ctask, bool shouldBeCompleted, string scenario)
            {
                if (ctask.IsCompleted != shouldBeCompleted)
                {
                    Assert.True(false, string.Format("RunUnwrapTests: > FAILED.  {0} expected IsCompleted = {1}", scenario, shouldBeCompleted));
                }
            };

            c1 = taskRoot.ContinueWith((antecedent) => { return Task<int>.Factory.StartNew(delegate { mres.WaitOne(); return 1; }); }).Unwrap();
            c2 = futureRoot.ContinueWith((antecedent) => { return Task<int>.Factory.StartNew(delegate { mres.WaitOne(); return 2; }); }).Unwrap();
            var v3 = new Task<Task<int>>(delegate { return Task<int>.Factory.StartNew(delegate { mres.WaitOne(); return 3; }); });
            c3 = v3.Unwrap();
            c4 = Task.Factory.ContinueWhenAll(new Task[] { taskRoot, futureRoot }, completedTasks =>
            {
                int sum = 0;
                for (int i = 0; i < completedTasks.Length; i++)
                {
                    Task tmp = completedTasks[i];
                    if (tmp is Task<int>) sum += ((Task<int>)tmp).Result;
                }
                return Task.Factory.StartNew(delegate { mres.WaitOne(); return sum; });
            }).Unwrap();
            c5 = taskRoot.ContinueWith((antecedent) => { return Task.Factory.StartNew(delegate { mres.WaitOne(); }); }).Unwrap();
            c6 = futureRoot.ContinueWith((antecedent) => { return Task.Factory.StartNew(delegate { mres.WaitOne(); }); }).Unwrap();
            var v7 = new Task<Task>(delegate { return Task.Factory.StartNew(delegate { mres.WaitOne(); }); });
            c7 = v7.Unwrap();
            c8 = Task.Factory.ContinueWhenAny(new Task[] { taskRoot, futureRoot }, winner =>
            {
                return Task.Factory.StartNew(delegate { mres.WaitOne(); });
            }).Unwrap();

            //Debug.WriteLine(" Testing that Unwrap() products do not complete before antecedent starts...");
            checkCompletionState(c1, false, "Task ==> Task<T>, antecedent unstarted");
            checkCompletionState(c2, false, "Task<T> ==> Task<T>, antecedent unstarted");
            checkCompletionState(c3, false, "StartNew ==> Task<T>, antecedent unstarted");
            checkCompletionState(c4, false, "ContinueWhenAll => Task<T>, antecedent unstarted");
            checkCompletionState(c5, false, "Task ==> Task, antecedent unstarted");
            checkCompletionState(c6, false, "Task<T> ==> Task, antecedent unstarted");
            checkCompletionState(c7, false, "StartNew ==> Task, antecedent unstarted");
            checkCompletionState(c8, false, "ContinueWhenAny => Task, antecedent unstarted");

            taskRoot.Start();
            futureRoot.Start();
            v3.Start();
            v7.Start();

            //Debug.WriteLine(" Testing that Unwrap() products do not complete before proxy source completes...");
            checkCompletionState(c1, false, "Task ==> Task<T>, source task incomplete");
            checkCompletionState(c2, false, "Task<T> ==> Task<T>, source task incomplete");
            checkCompletionState(c3, false, "StartNew ==> Task<T>, source task incomplete");
            checkCompletionState(c4, false, "ContinueWhenAll => Task<T>, source task incomplete");
            checkCompletionState(c5, false, "Task ==> Task, source task incomplete");
            checkCompletionState(c6, false, "Task<T> ==> Task, source task incomplete");
            checkCompletionState(c7, false, "StartNew ==> Task, source task incomplete");
            checkCompletionState(c8, false, "ContinueWhenAny => Task, source task incomplete");

            mres.Set();
            Debug.WriteLine("RunUnwrapTests:  Waiting on Unwrap() products... If we hang, something is wrong.");
            Task.WaitAll(new Task[] { c1, c2, c3, c4, c5, c6, c7, c8 });

            //Debug.WriteLine("    Testing that Unwrap() producs have consistent completion state...");
            checkCompletionState(c1, true, "Task ==> Task<T>, Unwrapped task complete");
            checkCompletionState(c2, true, "Task<T> ==> Task<T>, Unwrapped task complete");
            checkCompletionState(c3, true, "StartNew ==> Task<T>, Unwrapped task complete");
            checkCompletionState(c4, true, "ContinueWhenAll => Task<T>, Unwrapped task complete");
            checkCompletionState(c5, true, "Task ==> Task, Unwrapped task complete");
            checkCompletionState(c6, true, "Task<T> ==> Task, Unwrapped task complete");
            checkCompletionState(c7, true, "StartNew ==> Task, Unwrapped task complete");
            checkCompletionState(c8, true, "ContinueWhenAny => Task, Unwrapped task complete");

            if (c1.Result != 1)
            {
                Assert.True(false, string.Format("RunUnwrapTests: > FAILED.  Expected c1.Result = 1, got {0}", c1.Result));
            }

            if (c2.Result != 2)
            {
                Assert.True(false, string.Format("RunUnwrapTests: > FAILED.  Expected c2.Result = 2, got {0}", c2.Result));
            }

            if (c3.Result != 3)
            {
                Assert.True(false, string.Format("RunUnwrapTests: > FAILED.  Expected c3.Result = 3, got {0}", c3.Result));
            }

            if (c4.Result != 10)
            {
                Assert.True(false, string.Format("RunUnwrapTests: > FAILED.  Expected c4.Result = 10, got {0}", c4.Result));
            }

            ////
            //// Test against buggy schedulers
            ////
            //
            //// More specifically, ensure that inline execution via synchronous continuations
            //// causes the predictable exception from the NonInliningTaskScheduler.
            //
            //Task<Task> t1 = null;
            //Task t2 = null;
            //Task hanging1 = new TaskFactory(new NonInliningTaskScheduler()).StartNew(() =>
            //{
            //    // To avoid fast-path optimizations in Unwrap, ensure that both inner
            //    // and outer tasks are not completed before Unwrap is called.  (And a
            //    // good way to do this is to ensure that they are not even started!)
            //    Task inner = new Task(() => { });
            //    t1 = new Task<Task>(() => inner, TaskCreationOptions.AttachedToParent);
            //    t2 = t1.Unwrap();
            //    t1.Start();
            //    inner.Start();
            //});
            //
            //Debug.WriteLine("Buggy Scheduler Test 1 about to wait -- if we hang, we have a problem...");
            //
            //// Wait for task to complete, but do *not* inline it.
            //((IAsyncResult)hanging1).AsyncWaitHandle.WaitOne();
            //
            //try
            //{
            //    hanging1.Wait();
            //    Assert.True(false, string.Format("    > FAILED. Expected an exception."));
            //    return false;
            //}
            //catch (Exception e) { }
            //
            //Task hanging2 = new TaskFactory(new NonInliningTaskScheduler()).StartNew(() =>
            //{
            //    // To avoid fast-path optimizations in Unwrap, ensure that both inner
            //    // and outer tasks are not completed before Unwrap is called.  (And a
            //    // good way to do this is to ensure that they are not even started!)
            //    Task<int> inner = new Task<int>(() => 10);
            //    Task<Task<int>> f1 = new Task<Task<int>>(() => inner, TaskCreationOptions.AttachedToParent);
            //    Task<int> f2 = f1.Unwrap();
            //    f1.Start();
            //    inner.Start();
            //});
            //
            //Debug.WriteLine("Buggy Scheduler Test 2 about to wait -- if we hang, we have a problem...");
            //
            //// Wait for task to complete, but do *not* inline it.
            //((IAsyncResult)hanging2).AsyncWaitHandle.WaitOne();
            //
            //try
            //{
            //    hanging2.Wait();
            //    Assert.True(false, string.Format("    > FAILED. Expected an exception."));
            //    return false;
            //}
            //catch (Exception e) {  }
        }

        [Fact]
        public static void RunUnwrapTests_ExceptionTests()
        {
            Task taskRoot = null;
            Task<int> futureRoot = null;

            Task<int> c1 = null;
            Task<int> c2 = null;
            Task<int> c3 = null;
            Task<int> c4 = null;
            Task c5 = null;
            Task c6 = null;
            Task c7 = null;
            Task c8 = null;

            Action doExc = delegate { throw new Exception("some exception"); };
            //
            // Exception tests
            //
            taskRoot = new Task(delegate { });
            futureRoot = new Task<int>(delegate { return 10; });
            c1 = taskRoot.ContinueWith(delegate (Task t) { doExc(); return Task<int>.Factory.StartNew(delegate { return 1; }); }).Unwrap();
            c2 = futureRoot.ContinueWith(delegate (Task<int> t) { doExc(); return Task<int>.Factory.StartNew(delegate { return 2; }); }).Unwrap();
            c3 = taskRoot.ContinueWith(delegate (Task t) { return Task<int>.Factory.StartNew(delegate { doExc(); return 3; }); }).Unwrap();
            c4 = futureRoot.ContinueWith(delegate (Task<int> t) { return Task<int>.Factory.StartNew(delegate { doExc(); return 4; }); }).Unwrap();
            c5 = taskRoot.ContinueWith(delegate (Task t) { doExc(); return Task.Factory.StartNew(delegate { }); }).Unwrap();
            c6 = futureRoot.ContinueWith(delegate (Task<int> t) { doExc(); return Task.Factory.StartNew(delegate { }); }).Unwrap();
            c7 = taskRoot.ContinueWith(delegate (Task t) { return Task.Factory.StartNew(delegate { doExc(); }); }).Unwrap();
            c8 = futureRoot.ContinueWith(delegate (Task<int> t) { return Task.Factory.StartNew(delegate { doExc(); }); }).Unwrap();
            taskRoot.Start();
            futureRoot.Start();

            Action<Task, string> excTest = delegate (Task ctask, string scenario)
            {
                try
                {
                    ctask.Wait();
                    Assert.True(false, string.Format("RunUnwrapTests: > FAILED.  Exception in {0} did not throw on Wait().", scenario));
                }
                catch (AggregateException) { }
                catch (Exception)
                {
                    Assert.True(false, string.Format("RunUnwrapTests: > FAILED.  Exception in {0} threw wrong exception.", scenario));
                }
                if (ctask.Status != TaskStatus.Faulted)
                {
                    Assert.True(false, string.Format("RunUnwrapTests: > FAILED. Exception in {0} resulted in wrong status: {1}", scenario, ctask.Status));
                }
            };

            excTest(c1, "Task->Task<int> outer delegate");
            excTest(c2, "Task<int>->Task<int> outer delegate");
            excTest(c3, "Task->Task<int> inner delegate");
            excTest(c4, "Task<int>->Task<int> inner delegate");
            excTest(c5, "Task->Task outer delegate");
            excTest(c6, "Task<int>->Task outer delegate");
            excTest(c7, "Task->Task inner delegate");
            excTest(c8, "Task<int>->Task inner delegate");

            try
            {
                taskRoot.Wait();
                futureRoot.Wait();
            }
            catch (Exception e)
            {
                Assert.True(false, string.Format("RunUnwrapTests: > FAILED.  Exception thrown while waiting for task/futureRoots used for exception testing: {0}", e));
            }

            //
            // Exception handling
            //
            var c = Task.Factory.StartNew(() => { }).ContinueWith(_ =>
                Task.Factory.StartNew(() =>
                {
                    Task.Factory.StartNew(delegate { throw new Exception("uh oh #1"); }, TaskCreationOptions.AttachedToParent);
                    Task.Factory.StartNew(delegate { throw new Exception("uh oh #2"); }, TaskCreationOptions.AttachedToParent);
                    Task.Factory.StartNew(delegate { throw new Exception("uh oh #3"); }, TaskCreationOptions.AttachedToParent);
                    Task.Factory.StartNew(delegate { throw new Exception("uh oh #4"); }, TaskCreationOptions.AttachedToParent);
                    return 1;
                })
            ).Unwrap();

            try
            {
                c.Wait();
                Assert.True(false, string.Format("RunUnwrapTests: > FAILED.  Monadic continuation w/ excepted children failed to throw an exception."));
            }
            catch (AggregateException ae)
            {
                if (ae.InnerExceptions.Count != 4)
                {
                    Assert.True(false, string.Format("RunUnwrapTests: > FAILED.  Monadic continuation w/ faulted childred had {0} inner exceptions, expected 4", ae.InnerExceptions.Count));
                    Assert.True(false, string.Format("RunUnwrapTests: > Exception = {0}", ae));
                }
            }
        }

        [Fact]
        public static void RunUnwrapTests_CancellationTests()
        {
            Task taskRoot = null;
            Task<int> futureRoot = null;

            Task<int> c1 = null;
            Task<int> c2 = null;
            Task c5 = null;
            Task c6 = null;
            int c1val = 0;
            int c2val = 0;
            int c5val = 0;
            int c6val = 0;

            //
            // Cancellation tests
            //
            CancellationTokenSource ctsForContainer = new CancellationTokenSource();
            CancellationTokenSource ctsForC1 = new CancellationTokenSource();
            CancellationTokenSource ctsForC2 = new CancellationTokenSource();
            CancellationTokenSource ctsForC5 = new CancellationTokenSource();
            CancellationTokenSource ctsForC6 = new CancellationTokenSource();

            ManualResetEvent mres = new ManualResetEvent(false);

            taskRoot = new Task(delegate { });
            futureRoot = new Task<int>(delegate { return 20; });
            Task container = Task.Factory.StartNew(delegate
            {
                c1 = taskRoot.ContinueWith(delegate (Task antecedent)
                {
                    Task<int> rval = new Task<int>(delegate { c1val = 1; return 10; });
                    return rval;
                }, ctsForC1.Token).Unwrap();

                c2 = futureRoot.ContinueWith(delegate (Task<int> antecedent)
                {
                    Task<int> rval = new Task<int>(delegate { c2val = 1; return 10; });
                    return rval;
                }, ctsForC2.Token).Unwrap();

                c5 = taskRoot.ContinueWith(delegate (Task antecedent)
                {
                    Task rval = new Task(delegate { c5val = 1; });
                    return rval;
                }, ctsForC5.Token).Unwrap();

                c6 = futureRoot.ContinueWith(delegate (Task<int> antecedent)
                {
                    Task rval = new Task(delegate { c6val = 1; });
                    return rval;
                }, ctsForC6.Token).Unwrap();

                mres.Set();

                ctsForContainer.Cancel();
            }, ctsForContainer.Token);

            // Wait for c1, c2 to get initialized.
            mres.WaitOne();

            ctsForC1.Cancel();
            try
            {
                c1.Wait();
                Assert.True(false, string.Format("RunUnwrapTests: > FAILED.  Expected Wait() to throw after cancellation of Task->Task<int>."));
            }
            catch { }
            TaskStatus ts = c1.Status;
            if (ts != TaskStatus.Canceled)
            {
                Assert.True(false, string.Format("RunUnwrapTests: > FAILED.  Direct cancellation of returned Task->Task<int> did not work -- status = {0}", ts));
            }

            ctsForC2.Cancel();
            try
            {
                c2.Wait();
                Assert.True(false, string.Format("RunUnwrapTests: > FAILED.  Expected Wait() to throw after cancellation of Task<int>->Task<int>."));
            }
            catch { }
            ts = c2.Status;
            if (ts != TaskStatus.Canceled)
            {
                Assert.True(false, string.Format("RunUnwrapTests: > FAILED.  Direct cancellation of returned Task<int>->Task<int> did not work -- status = {0}", ts));
            }

            ctsForC5.Cancel();
            try
            {
                c5.Wait();
                Assert.True(false, string.Format("RunUnwrapTests: > FAILED.  Expected Wait() to throw after cancellation of Task->Task."));
            }
            catch { }
            ts = c5.Status;
            if (ts != TaskStatus.Canceled)
            {
                Assert.True(false, string.Format("RunUnwrapTests: > FAILED.  Direct cancellation of returned Task->Task did not work -- status = {0}", ts));
            }

            ctsForC6.Cancel();
            try
            {
                c6.Wait();
                Assert.True(false, string.Format("RunUnwrapTests: > FAILED.  Expected Wait() to throw after cancellation of Task<int>->Task."));
            }
            catch { }
            ts = c6.Status;
            if (ts != TaskStatus.Canceled)
            {
                Assert.True(false, string.Format("RunUnwrapTests: > FAILED.  Direct cancellation of returned Task<int>->Task did not work -- status = {0}", ts));
            }

            Debug.WriteLine("RunUnwrapTests: Waiting for container... if we deadlock, cancellations are not being cleaned up properly.");
            container.Wait();

            taskRoot.Start();
            futureRoot.Start();

            try
            {
                taskRoot.Wait();
                futureRoot.Wait();
            }
            catch (Exception e)
            {
                Assert.True(false, string.Format("RunUnwrapTests: > FAILED.  Exception thrown when root tasks were started and waited upon: {0}", e));
            }

            if (c1val != 0)
            {
                Assert.True(false, string.Format("RunUnwrapTests: > FAILED.  Cancellation of Task->Task<int> failed to stop internal continuation"));
            }

            if (c2val != 0)
            {
                Assert.True(false, string.Format("RunUnwrapTests: > FAILED.  Cancellation of Task<int>->Task<int> failed to stop internal continuation"));
            }

            if (c5val != 0)
            {
                Assert.True(false, string.Format("RunUnwrapTests: > FAILED.  Cancellation of Task->Task failed to stop internal continuation"));
            }

            if (c6val != 0)
            {
                Assert.True(false, string.Format("RunUnwrapTests: > FAILED.  Cancellation of Task<int>->Task failed to stop internal continuation"));
            }
        }

        // Test that exceptions are properly wrapped when thrown in various scenarios.
        // Make sure that "indirect" logic does not add superfluous exception wrapping.
        [Fact]
        public static void RunExceptionWrappingTest()
        {
            Action throwException = delegate { throw new InvalidOperationException(); };

            //
            //
            // Test Monadic ContinueWith()
            //
            //
            Action<Task, string> mcwExceptionChecker = delegate (Task mcwTask, string scenario)
            {
                try
                {
                    mcwTask.Wait();
                    Assert.True(false, string.Format("RunExceptionWrappingTest:    > FAILED.  Wait-on-continuation did not throw for {0}", scenario));
                }
                catch (Exception e)
                {
                    int levels = NestedLevels(e);
                    if (levels != 2)
                    {
                        Assert.True(false, string.Format("RunExceptionWrappingTest:    > FAILED.  Exception had {0} levels instead of 2 for {1}.", levels, scenario));
                    }
                }
            };

            // Test mcw off of Task
            Task t = Task.Factory.StartNew(delegate { });

            // Throw in the returned future
            Task<int> mcw1 = t.ContinueWith(delegate (Task antecedent)
            {
                Task<int> inner = Task<int>.Factory.StartNew(delegate
                {
                    throw new InvalidOperationException();
                });

                return inner;
            }).Unwrap();

            mcwExceptionChecker(mcw1, "Task antecedent, throw in ContinuationFunction");

            // Throw in the continuationFunction
            Task<int> mcw2 = t.ContinueWith(delegate (Task antecedent)
            {
                throwException();
                Task<int> inner = Task<int>.Factory.StartNew(delegate
                {
                    return 0;
                });

                return inner;
            }).Unwrap();

            mcwExceptionChecker(mcw2, "Task antecedent, throw in returned Future");

            // Test mcw off of future
            Task<int> f = Task<int>.Factory.StartNew(delegate { return 0; });

            // Throw in the returned future
            mcw1 = f.ContinueWith(delegate (Task<int> antecedent)
            {
                Task<int> inner = Task<int>.Factory.StartNew(delegate
                {
                    throw new InvalidOperationException();
                });

                return inner;
            }).Unwrap();

            mcwExceptionChecker(mcw1, "Future antecedent, throw in ContinuationFunction");

            // Throw in the continuationFunction
            mcw2 = f.ContinueWith(delegate (Task<int> antecedent)
            {
                throwException();
                Task<int> inner = Task<int>.Factory.StartNew(delegate
                {
                    return 0;
                });

                return inner;
            }).Unwrap();

            mcwExceptionChecker(mcw2, "Future antecedent, throw in returned Future");
        }

        /// <summary>Gets an enumerable of already completed non-generic tasks.</summary>
        public static IEnumerable<object[]> CompletedNonGenericTasks
        {
            get
            {
                yield return new object[] { Task.CompletedTask };
                yield return new object[] { Task.FromCanceled(CreateCanceledToken()) };
                yield return new object[] { Task.FromException(new FormatException()) };

                var tcs = new TaskCompletionSource<int>();
                tcs.SetCanceled(); // cancel task without a token
                yield return new object[] { tcs.Task };
            }
        }

        /// <summary>Gets an enumerable of already completed generic tasks.</summary>
        public static IEnumerable<object[]> CompletedStringTasks
        {
            get
            {
                yield return new object[] { Task.FromResult("Tasks") };
                yield return new object[] { Task.FromCanceled<string>(CreateCanceledToken()) };
                yield return new object[] { Task.FromException<string>(new FormatException()) };

                var tcs = new TaskCompletionSource<string>();
                tcs.SetCanceled(); // cancel task without a token
                yield return new object[] { tcs.Task };
            }
        }

        /// <summary>Asserts that two non-generic tasks are logically equal with regards to completion status.</summary>
        private static void AssertTasksAreEqual(Task expected, Task actual)
        {
            Assert.NotNull(actual);
            WaitNoThrow(actual);

            Assert.Equal(expected.Status, actual.Status);
            switch (expected.Status)
            {
                case TaskStatus.Faulted:
                    Assert.Equal((IEnumerable<Exception>)expected.Exception.InnerExceptions, actual.Exception.InnerExceptions);
                    break;
                case TaskStatus.Canceled:
                    Assert.Equal(GetCanceledTaskToken(expected), GetCanceledTaskToken(actual));
                    break;
            }
        }

        /// <summary>Asserts that two non-generic tasks are logically equal with regards to completion status.</summary>
        private static void AssertTasksAreEqual<T>(Task<T> expected, Task<T> actual)
        {
            AssertTasksAreEqual((Task)expected, actual);
            if (expected.Status == TaskStatus.RanToCompletion)
            {
                if (typeof(T).GetTypeInfo().IsValueType)
                    Assert.Equal(expected.Result, actual.Result);
                else
                    Assert.Same(expected.Result, actual.Result);
            }
        }

        /// <summary>Creates an already canceled token.</summary>
        private static CancellationToken CreateCanceledToken()
        {
            // Create an already canceled token.  We construct a new CTS rather than
            // just using CT's Boolean ctor in order to better validate the right
            // token ends up in the resulting unwrapped task.
            var cts = new CancellationTokenSource();
            cts.Cancel();
            return cts.Token;
        }

        /// <summary>Waits for a task to complete without throwing any exceptions.</summary>
        private static void WaitNoThrow(Task task)
        {
            ((IAsyncResult)task).AsyncWaitHandle.WaitOne();
        }

        /// <summary>Extracts the CancellationToken associated with a task.</summary>
        private static CancellationToken GetCanceledTaskToken(Task task)
        {
            Assert.True(task.IsCanceled);
            TaskCanceledException exc = Assert.Throws<TaskCanceledException>(() => task.GetAwaiter().GetResult());
            return exc.CancellationToken;
        }

    }
}
