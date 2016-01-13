// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using Xunit;

namespace System.Threading.Tasks.Tests
{
    //
    // ContinueWith tests.
    // TODO: Revisit ContinueWhenAny/ContinueWhenAll and compare tests.
    public static class TaskContinueWithTests
    {
        #region ContinueWith Tests

        private static readonly TimeSpan MaxSafeWait = TimeSpan.FromMinutes(1);

        [Fact]
        public static void AsyncState_Null()
        {
            Assert.Null(Wait(Start(new Task(() => { })).ContinueWith(_ => { })).AsyncState);
            Assert.Null(Wait(Start(new Task(() => { })).ContinueWith(_ => { }, new CancellationTokenSource().Token)).AsyncState);
            Assert.Null(Wait(Start(new Task(() => { })).ContinueWith(_ => { }, TaskContinuationOptions.None)).AsyncState);
            Assert.Null(Wait(Start(new Task(() => { })).ContinueWith(_ => { }, TaskScheduler.Default)).AsyncState);
            Assert.Null(Wait(Start(new Task(() => { })).ContinueWith(_ => { }, new CancellationTokenSource().Token, TaskContinuationOptions.None, TaskScheduler.Default)).AsyncState);

            Assert.Null(Wait(Start(new Task(() => { })).ContinueWith(_ => 0)).AsyncState);
            Assert.Null(Wait(Start(new Task(() => { })).ContinueWith(_ => 0, new CancellationTokenSource().Token)).AsyncState);
            Assert.Null(Wait(Start(new Task(() => { })).ContinueWith(_ => 0, TaskContinuationOptions.None)).AsyncState);
            Assert.Null(Wait(Start(new Task(() => { })).ContinueWith(_ => 0, TaskScheduler.Default)).AsyncState);
            Assert.Null(Wait(Start(new Task(() => { })).ContinueWith(_ => 0, new CancellationTokenSource().Token, TaskContinuationOptions.None, TaskScheduler.Default)).AsyncState);

            Assert.Null(Wait(Start(new Task<int>(() => 0)).ContinueWith(_ => { })).AsyncState);
            Assert.Null(Wait(Start(new Task<int>(() => 0)).ContinueWith(_ => { }, new CancellationTokenSource().Token)).AsyncState);
            Assert.Null(Wait(Start(new Task<int>(() => 0)).ContinueWith(_ => { }, TaskContinuationOptions.None)).AsyncState);
            Assert.Null(Wait(Start(new Task<int>(() => 0)).ContinueWith(_ => { }, TaskScheduler.Default)).AsyncState);
            Assert.Null(Wait(Start(new Task<int>(() => 0)).ContinueWith(_ => { }, new CancellationTokenSource().Token, TaskContinuationOptions.None, TaskScheduler.Default)).AsyncState);

            Assert.Null(Wait(Start(new Task<int>(() => 0)).ContinueWith(_ => 0)).AsyncState);
            Assert.Null(Wait(Start(new Task<int>(() => 0)).ContinueWith(_ => 0, new CancellationTokenSource().Token)).AsyncState);
            Assert.Null(Wait(Start(new Task<int>(() => 0)).ContinueWith(_ => 0, TaskContinuationOptions.None)).AsyncState);
            Assert.Null(Wait(Start(new Task<int>(() => 0)).ContinueWith(_ => 0, TaskScheduler.Default)).AsyncState);
            Assert.Null(Wait(Start(new Task<int>(() => 0)).ContinueWith(_ => 0, new CancellationTokenSource().Token, TaskContinuationOptions.None, TaskScheduler.Default)).AsyncState);
        }

        [Fact]
        public static void AsyncState_NotNull()
        {
            object provided = new object();
            Assert.Equal(provided, Wait(Start(new Task(() => { })).ContinueWith((t, o) => { }, provided)).AsyncState);
            Assert.Equal(provided, Wait(Start(new Task(() => { })).ContinueWith((t, o) => { }, provided, new CancellationTokenSource().Token)).AsyncState);
            Assert.Equal(provided, Wait(Start(new Task(() => { })).ContinueWith((t, o) => { }, provided, TaskContinuationOptions.None)).AsyncState);
            Assert.Equal(provided, Wait(Start(new Task(() => { })).ContinueWith((t, o) => { }, provided, TaskScheduler.Default)).AsyncState);
            Assert.Equal(provided, Wait(Start(new Task(() => { })).ContinueWith((t, o) => { }, provided, new CancellationTokenSource().Token, TaskContinuationOptions.None, TaskScheduler.Default)).AsyncState);

            Assert.Equal(provided, Wait(Start(new Task(() => { })).ContinueWith((t, o) => 0, provided)).AsyncState);
            Assert.Equal(provided, Wait(Start(new Task(() => { })).ContinueWith((t, o) => 0, provided, new CancellationTokenSource().Token)).AsyncState);
            Assert.Equal(provided, Wait(Start(new Task(() => { })).ContinueWith((t, o) => 0, provided, TaskContinuationOptions.None)).AsyncState);
            Assert.Equal(provided, Wait(Start(new Task(() => { })).ContinueWith((t, o) => 0, provided, TaskScheduler.Default)).AsyncState);
            Assert.Equal(provided, Wait(Start(new Task(() => { })).ContinueWith((t, o) => 0, provided, new CancellationTokenSource().Token, TaskContinuationOptions.None, TaskScheduler.Default)).AsyncState);

            Assert.Equal(provided, Wait(Start(new Task<int>(() => 0)).ContinueWith((t, o) => { }, provided)).AsyncState);
            Assert.Equal(provided, Wait(Start(new Task<int>(() => 0)).ContinueWith((t, o) => { }, provided, new CancellationTokenSource().Token)).AsyncState);
            Assert.Equal(provided, Wait(Start(new Task<int>(() => 0)).ContinueWith((t, o) => { }, provided, TaskContinuationOptions.None)).AsyncState);
            Assert.Equal(provided, Wait(Start(new Task<int>(() => 0)).ContinueWith((t, o) => { }, provided, TaskScheduler.Default)).AsyncState);
            Assert.Equal(provided, Wait(Start(new Task<int>(() => 0)).ContinueWith((t, o) => { }, provided, new CancellationTokenSource().Token, TaskContinuationOptions.None, TaskScheduler.Default)).AsyncState);

            Assert.Equal(provided, Wait(Start(new Task<int>(() => 0)).ContinueWith((t, o) => 0, provided)).AsyncState);
            Assert.Equal(provided, Wait(Start(new Task<int>(() => 0)).ContinueWith((t, o) => 0, provided, new CancellationTokenSource().Token)).AsyncState);
            Assert.Equal(provided, Wait(Start(new Task<int>(() => 0)).ContinueWith((t, o) => 0, provided, TaskContinuationOptions.None)).AsyncState);
            Assert.Equal(provided, Wait(Start(new Task<int>(() => 0)).ContinueWith((t, o) => 0, provided, TaskScheduler.Default)).AsyncState);
            Assert.Equal(provided, Wait(Start(new Task<int>(() => 0)).ContinueWith((t, o) => 0, provided, new CancellationTokenSource().Token, TaskContinuationOptions.None, TaskScheduler.Default)).AsyncState);
        }

        private static T Wait<T>(T task) where T : Task
        {
            task.Wait();
            return task;
        }

        private static T Start<T>(T task) where T : Task
        {
            task.Start();
            return task;
        }

        [Fact]
        public static void ContinuationOptions_Default()
        {
            Assert.Equal(TaskCreationOptions.None, new Task(() => { }).ContinueWith(_ => { }).CreationOptions);
            Assert.Equal(TaskCreationOptions.None, new Task(() => { }).ContinueWith((t, o) => { }, new object()).CreationOptions);
            Assert.Equal(TaskCreationOptions.None, new Task(() => { }).ContinueWith(_ => { }, new CancellationTokenSource().Token).CreationOptions);
            Assert.Equal(TaskCreationOptions.None, new Task(() => { }).ContinueWith((t, o) => { }, new object(), new CancellationTokenSource().Token).CreationOptions);
            Assert.Equal(TaskCreationOptions.None, new Task(() => { }).ContinueWith(_ => { }, TaskScheduler.Default).CreationOptions);
            Assert.Equal(TaskCreationOptions.None, new Task(() => { }).ContinueWith((t, o) => { }, new object(), TaskScheduler.Default).CreationOptions);

            Assert.Equal(TaskCreationOptions.None, new Task(() => { }).ContinueWith(_ => 0).CreationOptions);
            Assert.Equal(TaskCreationOptions.None, new Task(() => { }).ContinueWith((t, o) => 0, new object()).CreationOptions);
            Assert.Equal(TaskCreationOptions.None, new Task(() => { }).ContinueWith(_ => 0, new CancellationTokenSource().Token).CreationOptions);
            Assert.Equal(TaskCreationOptions.None, new Task(() => { }).ContinueWith((t, o) => 0, new object(), new CancellationTokenSource().Token).CreationOptions);
            Assert.Equal(TaskCreationOptions.None, new Task(() => { }).ContinueWith(_ => 0, TaskScheduler.Default).CreationOptions);
            Assert.Equal(TaskCreationOptions.None, new Task(() => { }).ContinueWith((t, o) => 0, new object(), TaskScheduler.Default).CreationOptions);

            Assert.Equal(TaskCreationOptions.None, new Task<int>(() => 0).ContinueWith(_ => { }).CreationOptions);
            Assert.Equal(TaskCreationOptions.None, new Task<int>(() => 0).ContinueWith((t, o) => { }, new object()).CreationOptions);
            Assert.Equal(TaskCreationOptions.None, new Task<int>(() => 0).ContinueWith(_ => { }, new CancellationTokenSource().Token).CreationOptions);
            Assert.Equal(TaskCreationOptions.None, new Task<int>(() => 0).ContinueWith((t, o) => { }, new object(), new CancellationTokenSource().Token).CreationOptions);
            Assert.Equal(TaskCreationOptions.None, new Task<int>(() => 0).ContinueWith(_ => { }, TaskScheduler.Default).CreationOptions);
            Assert.Equal(TaskCreationOptions.None, new Task<int>(() => 0).ContinueWith((t, o) => { }, new object(), TaskScheduler.Default).CreationOptions);

            Assert.Equal(TaskCreationOptions.None, new Task<int>(() => 0).ContinueWith(_ => 0).CreationOptions);
            Assert.Equal(TaskCreationOptions.None, new Task<int>(() => 0).ContinueWith((t, o) => 0, new object()).CreationOptions);
            Assert.Equal(TaskCreationOptions.None, new Task<int>(() => 0).ContinueWith(_ => 0, new CancellationTokenSource().Token).CreationOptions);
            Assert.Equal(TaskCreationOptions.None, new Task<int>(() => 0).ContinueWith((t, o) => 0, new object(), new CancellationTokenSource().Token).CreationOptions);
            Assert.Equal(TaskCreationOptions.None, new Task<int>(() => 0).ContinueWith(_ => 0, TaskScheduler.Default).CreationOptions);
            Assert.Equal(TaskCreationOptions.None, new Task<int>(() => 0).ContinueWith((t, o) => 0, new object(), TaskScheduler.Default).CreationOptions);
        }

        [Theory]
        [InlineData(TaskCreationOptions.AttachedToParent, TaskContinuationOptions.AttachedToParent)]
        [InlineData(TaskCreationOptions.DenyChildAttach, TaskContinuationOptions.DenyChildAttach)]
        [InlineData(TaskCreationOptions.HideScheduler, TaskContinuationOptions.HideScheduler)]
        [InlineData(TaskCreationOptions.LongRunning, TaskContinuationOptions.LongRunning)]
        [InlineData(TaskCreationOptions.PreferFairness, TaskContinuationOptions.PreferFairness)]
        [InlineData(TaskCreationOptions.RunContinuationsAsynchronously, TaskContinuationOptions.RunContinuationsAsynchronously)]
        [InlineData(TaskCreationOptions.None, TaskContinuationOptions.None)]
        // Several options have no (later externally visible) equivalent
        [InlineData(TaskCreationOptions.None, TaskContinuationOptions.ExecuteSynchronously)]
        [InlineData(TaskCreationOptions.None, TaskContinuationOptions.NotOnCanceled)]
        [InlineData(TaskCreationOptions.None, TaskContinuationOptions.NotOnFaulted)]
        [InlineData(TaskCreationOptions.None, TaskContinuationOptions.NotOnRanToCompletion)]
        [InlineData(TaskCreationOptions.None, TaskContinuationOptions.OnlyOnCanceled)]
        [InlineData(TaskCreationOptions.None, TaskContinuationOptions.OnlyOnFaulted)]
        [InlineData(TaskCreationOptions.None, TaskContinuationOptions.OnlyOnRanToCompletion)]
        [InlineData(TaskCreationOptions.None, TaskContinuationOptions.LazyCancellation)]
        public static void ContinuationOptions_Given(TaskCreationOptions create, TaskContinuationOptions cont)
        {
            Assert.Equal(create, new Task(() => { }).ContinueWith((t, o) => { }, new object(), cont).CreationOptions);
            Assert.Equal(create, new Task(() => { }).ContinueWith(_ => { }, cont).CreationOptions);
            Assert.Equal(create, new Task(() => { }).ContinueWith((t, o) => { }, new object(), new CancellationTokenSource().Token, cont, TaskScheduler.Default).CreationOptions);
            Assert.Equal(create, new Task(() => { }).ContinueWith(_ => { }, new CancellationTokenSource().Token, cont, TaskScheduler.Default).CreationOptions);

            Assert.Equal(create, new Task(() => { }).ContinueWith((t, o) => 0, new object(), cont).CreationOptions);
            Assert.Equal(create, new Task(() => { }).ContinueWith(_ => 0, cont).CreationOptions);
            Assert.Equal(create, new Task(() => { }).ContinueWith((t, o) => 0, new object(), new CancellationTokenSource().Token, cont, TaskScheduler.Default).CreationOptions);
            Assert.Equal(create, new Task(() => { }).ContinueWith(_ => 0, new CancellationTokenSource().Token, cont, TaskScheduler.Default).CreationOptions);

            Assert.Equal(create, new Task<int>(() => 0).ContinueWith((t, o) => { }, new object(), cont).CreationOptions);
            Assert.Equal(create, new Task<int>(() => 0).ContinueWith(_ => { }, cont).CreationOptions);
            Assert.Equal(create, new Task<int>(() => 0).ContinueWith((t, o) => { }, new object(), new CancellationTokenSource().Token, cont, TaskScheduler.Default).CreationOptions);
            Assert.Equal(create, new Task<int>(() => 0).ContinueWith(_ => { }, new CancellationTokenSource().Token, cont, TaskScheduler.Default).CreationOptions);

            Assert.Equal(create, new Task<int>(() => 0).ContinueWith((t, o) => 0, new object(), cont).CreationOptions);
            Assert.Equal(create, new Task<int>(() => 0).ContinueWith(_ => 0, cont).CreationOptions);
            Assert.Equal(create, new Task<int>(() => 0).ContinueWith((t, o) => 0, new object(), new CancellationTokenSource().Token, cont, TaskScheduler.Default).CreationOptions);
            Assert.Equal(create, new Task<int>(() => 0).ContinueWith(_ => 0, new CancellationTokenSource().Token, cont, TaskScheduler.Default).CreationOptions);
        }

        [Fact]
        public static void TaskScheduler_Used()
        {
            TaskScheduler_Used(TaskScheduler.Default, new Task(() => { }), (task, capture, scheduler) => task.ContinueWith(_ => { capture(); }));
            TaskScheduler_Used(TaskScheduler.Default, new Task(() => { }), (task, capture, scheduler) => task.ContinueWith(_ => { capture(); }, new CancellationTokenSource().Token));
            TaskScheduler_Used(TaskScheduler.Default, new Task(() => { }), (task, capture, scheduler) => task.ContinueWith(_ => { capture(); }, TaskContinuationOptions.None));
            TaskScheduler_Used(new QUWITaskScheduler(), new Task(() => { }), (task, capture, scheduler) => task.ContinueWith(_ => { capture(); }, scheduler));
            TaskScheduler_Used(new QUWITaskScheduler(), new Task(() => { }), (task, capture, scheduler) => task.ContinueWith(_ => { capture(); }, new CancellationTokenSource().Token, TaskContinuationOptions.None, scheduler));

            TaskScheduler_Used(TaskScheduler.Default, new Task(() => { }), (task, capture, scheduler) => task.ContinueWith(_ => { capture(); return 0; }));
            TaskScheduler_Used(TaskScheduler.Default, new Task(() => { }), (task, capture, scheduler) => task.ContinueWith(_ => { capture(); return 0; }, new CancellationTokenSource().Token));
            TaskScheduler_Used(TaskScheduler.Default, new Task(() => { }), (task, capture, scheduler) => task.ContinueWith(_ => { capture(); return 0; }, TaskContinuationOptions.None));
            TaskScheduler_Used(new QUWITaskScheduler(), new Task(() => { }), (task, capture, scheduler) => task.ContinueWith(_ => { capture(); return 0; }, scheduler));
            TaskScheduler_Used(new QUWITaskScheduler(), new Task(() => { }), (task, capture, scheduler) => task.ContinueWith(_ => { capture(); return 0; }, new CancellationTokenSource().Token, TaskContinuationOptions.None, scheduler));

            TaskScheduler_Used(TaskScheduler.Default, new Task<int>(() => 0), (task, capture, scheduler) => task.ContinueWith(_ => { capture(); }));
            TaskScheduler_Used(TaskScheduler.Default, new Task<int>(() => 0), (task, capture, scheduler) => task.ContinueWith(_ => { capture(); }, new CancellationTokenSource().Token));
            TaskScheduler_Used(TaskScheduler.Default, new Task<int>(() => 0), (task, capture, scheduler) => task.ContinueWith(_ => { capture(); }, TaskContinuationOptions.None));
            TaskScheduler_Used(new QUWITaskScheduler(), new Task<int>(() => 0), (task, capture, scheduler) => task.ContinueWith(_ => { capture(); }, scheduler));
            TaskScheduler_Used(new QUWITaskScheduler(), new Task<int>(() => 0), (task, capture, scheduler) => task.ContinueWith(_ => { capture(); }, new CancellationTokenSource().Token, TaskContinuationOptions.None, scheduler));

            TaskScheduler_Used(TaskScheduler.Default, new Task<int>(() => 0), (task, capture, scheduler) => task.ContinueWith(_ => { capture(); return 0; }));
            TaskScheduler_Used(TaskScheduler.Default, new Task<int>(() => 0), (task, capture, scheduler) => task.ContinueWith(_ => { capture(); return 0; }, new CancellationTokenSource().Token));
            TaskScheduler_Used(TaskScheduler.Default, new Task<int>(() => 0), (task, capture, scheduler) => task.ContinueWith(_ => { capture(); return 0; }, TaskContinuationOptions.None));
            TaskScheduler_Used(new QUWITaskScheduler(), new Task<int>(() => 0), (task, capture, scheduler) => task.ContinueWith(_ => { capture(); return 0; }, scheduler));
            TaskScheduler_Used(new QUWITaskScheduler(), new Task<int>(() => 0), (task, capture, scheduler) => task.ContinueWith(_ => { capture(); return 0; }, new CancellationTokenSource().Token, TaskContinuationOptions.None, scheduler));

            TaskScheduler_Used(TaskScheduler.Default, new Task(() => { }), (task, capture, scheduler) => task.ContinueWith((t, o) => { capture(); }, new object()));
            TaskScheduler_Used(TaskScheduler.Default, new Task(() => { }), (task, capture, scheduler) => task.ContinueWith((t, o) => { capture(); }, new object(), new CancellationTokenSource().Token));
            TaskScheduler_Used(TaskScheduler.Default, new Task(() => { }), (task, capture, scheduler) => task.ContinueWith((t, o) => { capture(); }, new object(), TaskContinuationOptions.None));
            TaskScheduler_Used(new QUWITaskScheduler(), new Task(() => { }), (task, capture, scheduler) => task.ContinueWith((t, o) => { capture(); }, new object(), scheduler));
            TaskScheduler_Used(new QUWITaskScheduler(), new Task(() => { }), (task, capture, scheduler) => task.ContinueWith((t, o) => { capture(); }, new object(), new CancellationTokenSource().Token, TaskContinuationOptions.None, scheduler));

            TaskScheduler_Used(TaskScheduler.Default, new Task(() => { }), (task, capture, scheduler) => task.ContinueWith((t, o) => { capture(); return 0; }, new object()));
            TaskScheduler_Used(TaskScheduler.Default, new Task(() => { }), (task, capture, scheduler) => task.ContinueWith((t, o) => { capture(); return 0; }, new object(), new CancellationTokenSource().Token));
            TaskScheduler_Used(TaskScheduler.Default, new Task(() => { }), (task, capture, scheduler) => task.ContinueWith((t, o) => { capture(); return 0; }, new object(), TaskContinuationOptions.None));
            TaskScheduler_Used(new QUWITaskScheduler(), new Task(() => { }), (task, capture, scheduler) => task.ContinueWith((t, o) => { capture(); return 0; }, new object(), scheduler));
            TaskScheduler_Used(new QUWITaskScheduler(), new Task(() => { }), (task, capture, scheduler) => task.ContinueWith((t, o) => { capture(); return 0; }, new object(), new CancellationTokenSource().Token, TaskContinuationOptions.None, scheduler));

            TaskScheduler_Used(TaskScheduler.Default, new Task<int>(() => 0), (task, capture, scheduler) => task.ContinueWith((t, o) => { capture(); }, new object()));
            TaskScheduler_Used(TaskScheduler.Default, new Task<int>(() => 0), (task, capture, scheduler) => task.ContinueWith((t, o) => { capture(); }, new object(), new CancellationTokenSource().Token));
            TaskScheduler_Used(TaskScheduler.Default, new Task<int>(() => 0), (task, capture, scheduler) => task.ContinueWith((t, o) => { capture(); }, new object(), TaskContinuationOptions.None));
            TaskScheduler_Used(new QUWITaskScheduler(), new Task<int>(() => 0), (task, capture, scheduler) => task.ContinueWith((t, o) => { capture(); }, new object(), scheduler));
            TaskScheduler_Used(new QUWITaskScheduler(), new Task<int>(() => 0), (task, capture, scheduler) => task.ContinueWith((t, o) => { capture(); }, new object(), new CancellationTokenSource().Token, TaskContinuationOptions.None, scheduler));

            TaskScheduler_Used(TaskScheduler.Default, new Task<int>(() => 0), (task, capture, scheduler) => task.ContinueWith((t, o) => { capture(); return 0; }, new object()));
            TaskScheduler_Used(TaskScheduler.Default, new Task<int>(() => 0), (task, capture, scheduler) => task.ContinueWith((t, o) => { capture(); return 0; }, new object(), new CancellationTokenSource().Token));
            TaskScheduler_Used(TaskScheduler.Default, new Task<int>(() => 0), (task, capture, scheduler) => task.ContinueWith((t, o) => { capture(); return 0; }, new object(), TaskContinuationOptions.None));
            TaskScheduler_Used(new QUWITaskScheduler(), new Task<int>(() => 0), (task, capture, scheduler) => task.ContinueWith((t, o) => { capture(); return 0; }, new object(), scheduler));
            TaskScheduler_Used(new QUWITaskScheduler(), new Task<int>(() => 0), (task, capture, scheduler) => task.ContinueWith((t, o) => { capture(); return 0; }, new object(), new CancellationTokenSource().Token, TaskContinuationOptions.None, scheduler));
        }

        private static void TaskScheduler_Used<T, U>(TaskScheduler expected, T task, Func<T, Action, TaskScheduler, U> create) where T : Task where U : Task
        {
            TaskScheduler seen = null;
            Action capture = () => { seen = TaskScheduler.Current; };
            U continuation = create(task, capture, expected);

            task.Start();
            continuation.Wait();
            // Verify that we saw the same scheduler we supplied.
            // In some cases none was supplied (no parameter used) - this becomes TaskScheduler.Default
            Assert.Equal(expected, seen);
        }

        // Stresses on multiple continuations from a single antecedent
        [Theory]
        // All "leftover" continuations will be immediately scheduled.
        // All "cancel" continuations will be immediately canceled.
        [InlineData(5000, 10000, 0, 0)]
        [InlineData(10000, 10000, 0, 0)]
        [InlineData(15000, 10000, 0, 0)]
        // Some "leftover" continuations will be queued when antecedent completes
        // All "cancel" continuations will be immediately canceled
        [InlineData(5000, 10000, 100, 0)]
        [InlineData(10000, 10000, 100, 0)]
        [InlineData(15000, 10000, 100, 0)]
        // Some "leftover" continuations will be queued when antecedent completes
        // Some "cancel" continuations will be queued when the token is signaled
        [InlineData(5000, 10000, 1000, 100)]
        [InlineData(10000, 10000, 1000, 100)]
        [InlineData(15000, 10000, 1000, 100)]
        // All "leftover" continuations should be queued when antecedent completes
        // There may or may not be leftover "cancel" continuations when the antecedent completes
        [InlineData(5000, 10000, 10000, 9900)]
        [InlineData(10000, 10000, 10000, 9900)]
        [InlineData(15000, 10000, 10000, 9900)]
        // All continuations should be queued when antecedent completes
        [InlineData(5000, 10000, 10000, 10000)]
        [InlineData(10000, 10000, 10000, 10000)]
        [InlineData(15000, 10000, 10000, 10000)]
        public static void ContinueWithTortureTest(int numCanceled, int numLeftover, int completeAfter, int cancelAfter)
        {
            //Debug.WriteLine("    - ContinueWithTortureTest(numCanceled={0}, numLeftover={1}, completeAfter={2}, cancelAfter={3})",
            //    numCanceled, numLeftover, completeAfter, cancelAfter);

            // The TCS mechanism gives us an easy way to start (and complete) antecedent
            TaskCompletionSource<bool> antecedentTcs = new TaskCompletionSource<bool>();
            Task antecedent = antecedentTcs.Task;

            int normalCount = 0; // incremented by "normal" or "leftover" continuations
            int canceledCount = 0; // incremented by "cancel" continuations
            CancellationTokenSource cts = new CancellationTokenSource(); // CTS to cancel

            // These TCS/continuation combos will serve to initiate antecedent completion or CTS signaling asynchronously
            TaskCompletionSource<bool> completionTcs = new TaskCompletionSource<bool>();
            completionTcs.Task.ContinueWith(_ => { antecedentTcs.TrySetResult(true); }, TaskContinuationOptions.PreferFairness);
            TaskCompletionSource<bool> cancellationTcs = new TaskCompletionSource<bool>();
            cancellationTcs.Task.ContinueWith(_ => { cts.Cancel(); }, TaskContinuationOptions.PreferFairness);

            // Keep track of continuations so that you can wait on them
            Task[] normalContinuations = new Task[numLeftover];
            Task[] cancelContinuations = new Task[numCanceled];

            // Take early action if either threshold is set at 0
            if (completeAfter == 0) antecedentTcs.TrySetResult(true);
            if (cancelAfter == 0) cts.Cancel();

            // These are the actions to take in "to be run" and "to be canceled" continuations, respectively
            Action<Task> normalAction = (task) => { Interlocked.Increment(ref normalCount); };
            Action<Task> cancelAction = (task) => { Interlocked.Increment(ref canceledCount); };

            // Simultaneously start adding both "to be run" continuations and "to be canceled" continuations
            Task taskA = Task.Factory.StartNew(
                () =>
                {
                    for (int i = 0; i < numCanceled; i++)
                    {
                        // Use both synchronous and asynchronous continuations
                        TaskContinuationOptions tco = ((i % 2) == 0) ? TaskContinuationOptions.None : TaskContinuationOptions.ExecuteSynchronously;

                        // The cancelAction should run exactly once per "to be canceled" continuation -- either in the first continuation or,
                        // if the first continuation is canceled, in the second continuation.
                        cancelContinuations[i] = antecedent.ContinueWith(cancelAction, cts.Token, tco, TaskScheduler.Default)
                            .ContinueWith(cancelAction, tco | TaskContinuationOptions.OnlyOnCanceled);
                    }
                });

            Task taskB = Task.Factory.StartNew(
                () =>
                {
                    for (int i = 0; i < numLeftover; i++)
                    {
                        // Use both synchronous and asynchronous continuations
                        TaskContinuationOptions tco = ((i % 2) == 0) ? TaskContinuationOptions.None : TaskContinuationOptions.ExecuteSynchronously;
                        normalContinuations[i] = antecedent.ContinueWith(normalAction, tco);

                        // If you've hit completeAfter or cancelAfter, take the approriate action
                        if ((i + 1) == completeAfter) completionTcs.TrySetResult(true); // Asynchronously completes the antecedent
                        if ((i + 1) == cancelAfter) cancellationTcs.TrySetResult(true); // Asynchronously initiates cancellation of "to be canceled" tasks
                    }
                });

            Task.WaitAll(taskA, taskB);
            Task.WaitAll(normalContinuations);

            try
            {
                Task.WaitAll(cancelContinuations);
            }
            catch (AggregateException ae)
            {
                // We may get AE<TCE> from WaitAll on cancelContinuations.  If so, just eat it.
                EnsureExceptionIsAEofTCE(ae,
                   "ContinueWithTortureTest: > FAILED.  Did not expect anything exception AE<TCE> from cancelContinuations.Wait()");
            }

            Assert.True(normalCount == numLeftover,
               "ContinueWithTortureTest: > FAILED! normalCount mismatch (exp " + numLeftover + " got " + normalCount + ")");
            Assert.True(canceledCount == numCanceled,
               "ContinueWithTortureTest: > FAILED! canceledCount mismatch (exp + " + numCanceled + " got " + canceledCount + ")");
        }

        [Fact]
        public static void Task_ContinueWith_Task_PreCanceledToken()
        {
            ContinueWith_PreCanceledToken(new Task(() => { }), (task, token) => task.ContinueWith(t => { }, token));
            ContinueWith_PreCanceledToken(new Task(() => { }), (task, token) => task.ContinueWith(t => { }, token, TaskContinuationOptions.None, TaskScheduler.Default));
            ContinueWith_PreCanceledToken(new Task(() => { }), (task, token) => task.ContinueWith((t, o) => { }, null, token));
            ContinueWith_PreCanceledToken(new Task(() => { }), (task, token) => task.ContinueWith((t, o) => { }, null, token, TaskContinuationOptions.None, TaskScheduler.Default));
        }

        [Fact]
        public static void Task_ContinueWith_Future_PreCanceledToken()
        {
            ContinueWith_PreCanceledToken(new Task(() => { }), (task, token) => task.ContinueWith(t => 0, token));
            ContinueWith_PreCanceledToken(new Task(() => { }), (task, token) => task.ContinueWith(t => 0, token, TaskContinuationOptions.None, TaskScheduler.Default));
            ContinueWith_PreCanceledToken(new Task(() => { }), (task, token) => task.ContinueWith((t, o) => 0, null, token));
            ContinueWith_PreCanceledToken(new Task(() => { }), (task, token) => task.ContinueWith((t, o) => 0, null, token, TaskContinuationOptions.None, TaskScheduler.Default));
        }

        [Fact]
        public static void Future_ContinueWith_Task_PreCanceledToken()
        {
            ContinueWith_PreCanceledToken(new Task<int>(() => 0), (task, token) => task.ContinueWith(t => { }, token));
            ContinueWith_PreCanceledToken(new Task<int>(() => 0), (task, token) => task.ContinueWith(t => { }, token, TaskContinuationOptions.None, TaskScheduler.Default));
            ContinueWith_PreCanceledToken(new Task<int>(() => 0), (task, token) => task.ContinueWith((t, o) => { }, null, token));
            ContinueWith_PreCanceledToken(new Task<int>(() => 0), (task, token) => task.ContinueWith((t, o) => { }, null, token, TaskContinuationOptions.None, TaskScheduler.Default));
        }

        [Fact]
        public static void Future_ContinueWith_Future_PreCanceledToken()
        {
            ContinueWith_PreCanceledToken(new Task<int>(() => 0), (task, token) => task.ContinueWith(t => 0, token));
            ContinueWith_PreCanceledToken(new Task<int>(() => 0), (task, token) => task.ContinueWith(t => 0, token, TaskContinuationOptions.None, TaskScheduler.Default));
            ContinueWith_PreCanceledToken(new Task<int>(() => 0), (task, token) => task.ContinueWith((t, o) => 0, null, token));
            ContinueWith_PreCanceledToken(new Task<int>(() => 0), (task, token) => task.ContinueWith((t, o) => 0, null, token, TaskContinuationOptions.None, TaskScheduler.Default));
        }

        private static void ContinueWith_PreCanceledToken<T, U>(T task, Func<T, CancellationToken, U> cont) where T : Task where U : Task
        {
            CancellationTokenSource source = new CancellationTokenSource();
            source.Cancel();
            U continuation = cont(task, source.Token);

            Functions.AssertCanceled(continuation, source.Token);
        }

        [Fact]
        public static void Task_ContinueWith_Task_LazyCancellation_PreCanceledToken()
        {
            ContinueWith_LazyCancellation_PreCanceledToken(mres => new Task(() => { mres.Wait(); }), (task, token) => task.ContinueWith(t => { }, token, TaskContinuationOptions.LazyCancellation, TaskScheduler.Default));
            ContinueWith_LazyCancellation_PreCanceledToken(mres => new Task(() => { mres.Wait(); }), (task, token) => task.ContinueWith((t, o) => { }, null, token, TaskContinuationOptions.LazyCancellation, TaskScheduler.Default));
        }

        [Fact]
        public static void Task_ContinueWith_Future_LazyCancellation_PreCanceledToken()
        {
            ContinueWith_LazyCancellation_PreCanceledToken(mres => new Task(() => { mres.Wait(); }), (task, token) => task.ContinueWith(t => 0, token, TaskContinuationOptions.LazyCancellation, TaskScheduler.Default));
            ContinueWith_LazyCancellation_PreCanceledToken(mres => new Task(() => { mres.Wait(); }), (task, token) => task.ContinueWith((t, o) => 0, null, token, TaskContinuationOptions.LazyCancellation, TaskScheduler.Default));
        }

        [Fact]
        public static void Future_ContinueWith_Task_LazyCancellation_PreCanceledToken()
        {
            ContinueWith_LazyCancellation_PreCanceledToken(mres => new Task<int>(() => { mres.Wait(); return 0; }), (task, token) => task.ContinueWith(t => { }, token, TaskContinuationOptions.LazyCancellation, TaskScheduler.Default));
            ContinueWith_LazyCancellation_PreCanceledToken(mres => new Task<int>(() => { mres.Wait(); return 0; }), (task, token) => task.ContinueWith((t, o) => { }, null, token, TaskContinuationOptions.LazyCancellation, TaskScheduler.Default));
        }

        [Fact]
        public static void Future_ContinueWith_Future_LazyCancellation_PreCanceledToken()
        {
            ContinueWith_LazyCancellation_PreCanceledToken(mres => new Task<int>(() => { mres.Wait(); return 0; }), (task, token) => task.ContinueWith(t => 0, token, TaskContinuationOptions.LazyCancellation, TaskScheduler.Default));
            ContinueWith_LazyCancellation_PreCanceledToken(mres => new Task<int>(() => { mres.Wait(); return 0; }), (task, token) => task.ContinueWith((t, o) => 0, null, token, TaskContinuationOptions.LazyCancellation, TaskScheduler.Default));
        }

        private static void ContinueWith_LazyCancellation_PreCanceledToken<T, U>(Func<ManualResetEventSlim, T> init, Func<T, CancellationToken, U> cont) where T : Task where U : Task
        {
            using (ManualResetEventSlim mres = new ManualResetEventSlim(false))
            {
                CancellationTokenSource source = new CancellationTokenSource();
                source.Cancel();

                T task = init(mres);
                U continuation = cont(task, source.Token);

                Assert.False(task.IsCompleted);
                Assert.Equal(TaskStatus.Created, task.Status);
                Assert.False(continuation.IsCompleted);
                Assert.Equal(TaskStatus.WaitingForActivation, continuation.Status);

                task.Start();

                Assert.True(SpinWait.SpinUntil(() => task.Status == TaskStatus.Running, MaxSafeWait));

                Assert.False(continuation.IsCompleted);
                Assert.Equal(TaskStatus.WaitingForActivation, continuation.Status);

                mres.Set();
                task.Wait();

                // Completion may not be immediate (subject to threading whims), but should be "soon".
                Assert.True(SpinWait.SpinUntil(() => continuation.IsCompleted, MaxSafeWait));

                Functions.AssertCanceled(continuation, source.Token);
            }
        }

        [Fact]
        public static void RunContinuationChainingTest()
        {
            int x = 0;
            int y = 0;

            Task t1 = new Task(delegate { x = 1; });
            Task t2 = t1.ContinueWith(delegate (Task t) { y = 1; });
            Task<int> t3 = t2.ContinueWith(delegate (Task t) { return 5; });
            Task<int> t4 = t3.ContinueWith(delegate (Task<int> t) { return Task<int>.Factory.StartNew(delegate { return 10; }); }).Unwrap();
            Task<string> t5 = t4.ContinueWith(delegate (Task<int> t) { return Task<string>.Factory.StartNew(delegate { for (int i = 0; i < 400; i++) ; return "worked"; }); }).Unwrap();

            try
            {
                t1.Start();
                if (!t5.Result.Equals("worked"))
                {
                    Assert.True(false, string.Format("RunContinuationChainingTest: > FAILED! t5.Result should be \"worked\", is {0}", t5.Result));
                }
                if (t4.Result != 10)
                {
                    Assert.True(false, string.Format("RunContinuationChainingTest: > FAILED! t4.Result should be 10, is {0}", t4.Result));
                }
                if (t3.Result != 5)
                {
                    Assert.True(false, string.Format("RunContinuationChainingTest: > FAILED! t3.Result should be 5, is {0}", t3.Result));
                }
                if (y != 1)
                {
                    Assert.True(false, string.Format("RunContinuationChainingTest: > FAILED! y should be 1, is {0}", y));
                }
                if (x != 1)
                {
                    Assert.True(false, string.Format("RunContinuationChainingTest: > FAILED! x should be 1, is {0}", x));
                }
            }
            catch (Exception e)
            {
                Assert.True(false, string.Format("RunContinuationChainingTest: > FAILED! Exception = {0}", e));
            }
        }

        [Fact]
        public static void RunContinueWithParamsTest_Cancellation()
        {
            //
            // Test whether parentage/cancellation is working correctly
            //
            Task c1b = null, c1c = null;
            Task c2b = null, c2c = null;

            Task container = new Task(delegate
            {
                CancellationTokenSource cts = new CancellationTokenSource();
                Task child1 = new Task(delegate { }, cts.Token, TaskCreationOptions.AttachedToParent);
                Task child2 = new Task(delegate { }, TaskCreationOptions.AttachedToParent);

                c1b = child1.ContinueWith((_) => { }, TaskContinuationOptions.NotOnCanceled | TaskContinuationOptions.AttachedToParent);
                c1c = child1.ContinueWith((_) => { }, TaskContinuationOptions.AttachedToParent);

                c2b = child2.ContinueWith((_) => { }, TaskContinuationOptions.NotOnRanToCompletion | TaskContinuationOptions.AttachedToParent);
                c2c = child2.ContinueWith((_) => { }, TaskContinuationOptions.AttachedToParent);

                cts.Cancel(); // should cancel the unstarted child task
                child2.Start();
            });

            container.Start();
            try { container.Wait(); }
            catch { }

            if (c1b.Status != TaskStatus.Canceled)
            {
                Assert.True(false, string.Format("RunContinueWithParamsTest: > FAILED.  Continuation task w/NotOnCanceled should have been canceled when antecedent was canceled."));
            }
            if (c1c.Status != TaskStatus.RanToCompletion)
            {
                Assert.True(false, string.Format("RunContinueWithParamsTest: > FAILED.  Continuation task w/ canceled antecedent should have run to completion."));
            }
            if (c2b.Status != TaskStatus.Canceled)
            {
                Assert.True(false, string.Format("RunContinueWithParamsTest: > FAILED.  Continuation task w/NotOnRanToCompletion should have been canceled when antecedent completed."));
            }
            c2c.Wait();
            if (c2c.Status != TaskStatus.RanToCompletion)
            {
                Assert.True(false, string.Format("RunContinueWithParamsTest: > FAILED.  Continuation task w/ completed antecedent should have run to completion."));
            }
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0x1000000)]
        [InlineData(TaskContinuationOptions.LongRunning | TaskContinuationOptions.ExecuteSynchronously)]
        [InlineData(TaskContinuationOptions.NotOnRanToCompletion | TaskContinuationOptions.NotOnFaulted | TaskContinuationOptions.NotOnCanceled)]
        [InlineData(TaskContinuationOptions.OnlyOnCanceled | TaskContinuationOptions.NotOnCanceled)]
        [InlineData(TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.NotOnFaulted)]
        [InlineData(TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.NotOnRanToCompletion)]
        [InlineData(TaskContinuationOptions.OnlyOnCanceled | TaskContinuationOptions.OnlyOnRanToCompletion)]
        [InlineData(TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.OnlyOnRanToCompletion)]
        [InlineData(TaskContinuationOptions.OnlyOnCanceled | TaskContinuationOptions.OnlyOnFaulted)]
        public static void ContinueWith_IllegalContinuationOptions(TaskContinuationOptions options)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => { new Task(() => { /* do nothing */}).ContinueWith(_ => { }, options); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { new Task(() => { /* do nothing */}).ContinueWith(_ => 0, options); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { new Task<int>(() => 0).ContinueWith(_ => { }, options); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { new Task<int>(() => 0).ContinueWith(_ => 0, options); });
        }

        [Fact]
        public static void Task_ContinueWith_Task_CancelBeforeRun()
        {
            ContinueWith_CancelBeforeRun(flag => new Task(() => { flag.Trip(); }), (task, token, flag) => task.ContinueWith(t => { flag.Trip(); }, token));
            ContinueWith_CancelBeforeRun(flag => new Task(() => { flag.Trip(); }), (task, token, flag) => task.ContinueWith(t => { flag.Trip(); }, token, TaskContinuationOptions.None, TaskScheduler.Default));
            ContinueWith_CancelBeforeRun(flag => new Task(() => { flag.Trip(); }), (task, token, flag) => task.ContinueWith((t, o) => { flag.Trip(); }, null, token));
            ContinueWith_CancelBeforeRun(flag => new Task(() => { flag.Trip(); }), (task, token, flag) => task.ContinueWith((t, o) => { flag.Trip(); }, null, token, TaskContinuationOptions.None, TaskScheduler.Default));
        }

        [Fact]
        public static void Task_ContinueWith_Future_CancelBeforeRun()
        {
            ContinueWith_CancelBeforeRun(flag => new Task(() => { flag.Trip(); }), (task, token, flag) => task.ContinueWith(t => { flag.Trip(); return 0; }, token));
            ContinueWith_CancelBeforeRun(flag => new Task(() => { flag.Trip(); }), (task, token, flag) => task.ContinueWith(t => { flag.Trip(); return 0; }, token, TaskContinuationOptions.None, TaskScheduler.Default));
            ContinueWith_CancelBeforeRun(flag => new Task(() => { flag.Trip(); }), (task, token, flag) => task.ContinueWith((t, o) => { flag.Trip(); return 0; }, null, token));
            ContinueWith_CancelBeforeRun(flag => new Task(() => { flag.Trip(); }), (task, token, flag) => task.ContinueWith((t, o) => { flag.Trip(); return 0; }, null, token, TaskContinuationOptions.None, TaskScheduler.Default));
        }

        [Fact]
        public static void Future_ContinueWith_Task_CancelBeforeRun()
        {
            ContinueWith_CancelBeforeRun(flag => new Task<int>(() => { flag.Trip(); return 0; }), (task, token, flag) => task.ContinueWith(t => { flag.Trip(); }, token));
            ContinueWith_CancelBeforeRun(flag => new Task<int>(() => { flag.Trip(); return 0; }), (task, token, flag) => task.ContinueWith(t => { flag.Trip(); }, token, TaskContinuationOptions.None, TaskScheduler.Default));
            ContinueWith_CancelBeforeRun(flag => new Task<int>(() => { flag.Trip(); return 0; }), (task, token, flag) => task.ContinueWith((t, o) => { flag.Trip(); }, null, token));
            ContinueWith_CancelBeforeRun(flag => new Task<int>(() => { flag.Trip(); return 0; }), (task, token, flag) => task.ContinueWith((t, o) => { flag.Trip(); }, null, token, TaskContinuationOptions.None, TaskScheduler.Default));
        }

        [Fact]
        public static void Future_ContinueWith_Future_CancelBeforeRun()
        {
            ContinueWith_CancelBeforeRun(flag => new Task<int>(() => { flag.Trip(); return 0; }), (task, token, flag) => task.ContinueWith(t => { flag.Trip(); return 0; }, token));
            ContinueWith_CancelBeforeRun(flag => new Task<int>(() => { flag.Trip(); return 0; }), (task, token, flag) => task.ContinueWith(t => { flag.Trip(); return 0; }, token, TaskContinuationOptions.None, TaskScheduler.Default));
            ContinueWith_CancelBeforeRun(flag => new Task<int>(() => { flag.Trip(); return 0; }), (task, token, flag) => task.ContinueWith((t, o) => { flag.Trip(); return 0; }, null, token));
            ContinueWith_CancelBeforeRun(flag => new Task<int>(() => { flag.Trip(); return 0; }), (task, token, flag) => task.ContinueWith((t, o) => { flag.Trip(); return 0; }, null, token, TaskContinuationOptions.None, TaskScheduler.Default));
        }

        // Test what happens when you cancel a task in the middle of a continuation chain.
        private static void ContinueWith_CancelBeforeRun<T, U>(Func<Flag, T> init, Func<T, CancellationToken, Flag, U> cont) where T : Task where U : Task
        {
            Flag initial = new Flag();
            Flag middle = new Flag();

            CancellationTokenSource source = new CancellationTokenSource();

            T i = init(initial);
            U continuation = cont(i, source.Token, middle);
            Task<bool> end = continuation.ContinueWith(_ => true);

            Assert.False(i.IsCompleted);
            Assert.False(continuation.IsCompleted);
            Assert.False(end.IsCompleted);

            // Cancel the middle task in the chain.  Should fire off end.
            source.Cancel();
            Assert.True(SpinWait.SpinUntil(() => end.IsCompleted, MaxSafeWait));

            Functions.AssertComplete(end, true);
            Functions.AssertCanceled(continuation, source.Token);

            // Start the first task in the chain.  Should hold off from kicking off (canceled) middle continuation.
            i.Start();
            Assert.True(SpinWait.SpinUntil(() => initial.IsTripped, MaxSafeWait));

            Assert.False(middle.IsTripped);
            Functions.AssertCanceled(continuation, source.Token);
        }

        [Fact]
        public static void Task_ContinueWith_Task_CancelDuringRun()
        {
            ContinueWith_CancelDuringRun(new Task(() => { }), (task, token, cancel) => task.ContinueWith(t => { cancel(); }, token));
            ContinueWith_CancelDuringRun(new Task(() => { }), (task, token, cancel) => task.ContinueWith(t => { cancel(); }, token, TaskContinuationOptions.None, TaskScheduler.Default));
            ContinueWith_CancelDuringRun(new Task(() => { }), (task, token, cancel) => task.ContinueWith(t => { cancel(); }, token, TaskContinuationOptions.LazyCancellation, TaskScheduler.Default));
            ContinueWith_CancelDuringRun(new Task(() => { }), (task, token, cancel) => task.ContinueWith((t, o) => { cancel(); }, null, token));
            ContinueWith_CancelDuringRun(new Task(() => { }), (task, token, cancel) => task.ContinueWith((t, o) => { cancel(); }, null, token, TaskContinuationOptions.None, TaskScheduler.Default));
            ContinueWith_CancelDuringRun(new Task(() => { }), (task, token, cancel) => task.ContinueWith((t, o) => { cancel(); }, null, token, TaskContinuationOptions.LazyCancellation, TaskScheduler.Default));
        }

        [Fact]
        public static void Task_ContinueWith_Future_CancelDuringRun()
        {
            ContinueWith_CancelDuringRun(new Task(() => { }), (task, token, cancel) => task.ContinueWith(t => { cancel(); return 0; }, token));
            ContinueWith_CancelDuringRun(new Task(() => { }), (task, token, cancel) => task.ContinueWith(t => { cancel(); return 0; }, token, TaskContinuationOptions.None, TaskScheduler.Default));
            ContinueWith_CancelDuringRun(new Task(() => { }), (task, token, cancel) => task.ContinueWith(t => { cancel(); return 0; }, token, TaskContinuationOptions.LazyCancellation, TaskScheduler.Default));
            ContinueWith_CancelDuringRun(new Task(() => { }), (task, token, cancel) => task.ContinueWith((t, o) => { cancel(); return 0; }, null, token));
            ContinueWith_CancelDuringRun(new Task(() => { }), (task, token, cancel) => task.ContinueWith((t, o) => { cancel(); return 0; }, null, token, TaskContinuationOptions.None, TaskScheduler.Default));
            ContinueWith_CancelDuringRun(new Task(() => { }), (task, token, cancel) => task.ContinueWith((t, o) => { cancel(); return 0; }, null, token, TaskContinuationOptions.LazyCancellation, TaskScheduler.Default));
        }

        [Fact]
        public static void Future_ContinueWith_Task_CancelDuringRun()
        {
            ContinueWith_CancelDuringRun(new Task<int>(() => 0), (task, token, cancel) => task.ContinueWith(t => { cancel(); }, token));
            ContinueWith_CancelDuringRun(new Task<int>(() => 0), (task, token, cancel) => task.ContinueWith(t => { cancel(); }, token, TaskContinuationOptions.None, TaskScheduler.Default));
            ContinueWith_CancelDuringRun(new Task<int>(() => 0), (task, token, cancel) => task.ContinueWith(t => { cancel(); }, token, TaskContinuationOptions.LazyCancellation, TaskScheduler.Default));
            ContinueWith_CancelDuringRun(new Task<int>(() => 0), (task, token, cancel) => task.ContinueWith((t, o) => { cancel(); }, null, token));
            ContinueWith_CancelDuringRun(new Task<int>(() => 0), (task, token, cancel) => task.ContinueWith((t, o) => { cancel(); }, null, token, TaskContinuationOptions.None, TaskScheduler.Default));
            ContinueWith_CancelDuringRun(new Task<int>(() => 0), (task, token, cancel) => task.ContinueWith((t, o) => { cancel(); }, null, token, TaskContinuationOptions.LazyCancellation, TaskScheduler.Default));
        }

        [Fact]
        public static void Future_ContinueWith_Future_CancelDuringRun()
        {
            ContinueWith_CancelDuringRun(new Task<int>(() => 0), (task, token, cancel) => task.ContinueWith(t => { cancel(); return 0; }, token));
            ContinueWith_CancelDuringRun(new Task<int>(() => 0), (task, token, cancel) => task.ContinueWith(t => { cancel(); return 0; }, token, TaskContinuationOptions.None, TaskScheduler.Default));
            ContinueWith_CancelDuringRun(new Task<int>(() => 0), (task, token, cancel) => task.ContinueWith(t => { cancel(); return 0; }, token, TaskContinuationOptions.LazyCancellation, TaskScheduler.Default));
            ContinueWith_CancelDuringRun(new Task<int>(() => 0), (task, token, cancel) => task.ContinueWith((t, o) => { cancel(); return 0; }, null, token));
            ContinueWith_CancelDuringRun(new Task<int>(() => 0), (task, token, cancel) => task.ContinueWith((t, o) => { cancel(); return 0; }, null, token, TaskContinuationOptions.None, TaskScheduler.Default));
            ContinueWith_CancelDuringRun(new Task<int>(() => 0), (task, token, cancel) => task.ContinueWith((t, o) => { cancel(); return 0; }, null, token, TaskContinuationOptions.LazyCancellation, TaskScheduler.Default));
        }

        // Test what happens when you cancel a task in the middle of a continuation chain.
        private static void ContinueWith_CancelDuringRun<T, U>(T task, Func<T, CancellationToken, Action, U> cont) where T : Task where U : Task
        {
            CancellationTokenSource source = new CancellationTokenSource();

            Action cancel = () =>
            {
                source.Cancel();
                source.Token.ThrowIfCancellationRequested();
            };

            U continuation = cont(task, source.Token, cancel);

            // Created not-complete
            Assert.False(task.IsCompleted);
            Assert.False(continuation.IsCompleted);

            // Start the initial task; continuation will auto-fire and cancel itself.
            task.Start();

            Assert.True(SpinWait.SpinUntil(() => continuation.IsCompleted, MaxSafeWait));
            Functions.AssertCanceled(continuation, source.Token);

            Functions.AssertComplete(task);
        }

        [Fact]
        public static void RunContinueWithAllParamsTestsNoState()
        {
            for (int i = 0; i < 2; i++)
            {
                bool preCanceled = (i == 0);
                CancellationTokenSource cts = new CancellationTokenSource();
                CancellationToken ct = cts.Token;
                if (preCanceled) cts.Cancel();

                for (int j = 0; j < 2; j++)
                {
                    bool longRunning = (j == 0);
                    TaskContinuationOptions tco = longRunning ? TaskContinuationOptions.LongRunning : TaskContinuationOptions.None;

                    for (int k = 0; k < 2; k++)
                    {
                        bool antecedentIsFuture = (k == 0);
                        Task antecedent = null;

                        for (int z = 0; z < 2; z++)
                        {
                            bool preCompletedTask = (z == 0);
                            if (preCompletedTask)
                            {
                                if (antecedentIsFuture) antecedent = Task<int>.Factory.StartNew(() => 5);
                                else antecedent = Task.Factory.StartNew(() => { });
                                antecedent.Wait();
                            }

                            for (int x = 0; x < 2; x++)
                            {
                                bool continuationIsFuture = (x == 0);

                                //
                                // Test ContinueWith() overloads that take all parameters
                                //
                                {
                                    Task continuation = null;

                                    if (!preCompletedTask)
                                    {
                                        if (antecedentIsFuture) antecedent = new Task<int>(() => 5);
                                        else antecedent = new Task(() => { });
                                    }

                                    if (continuationIsFuture)
                                    {
                                        if (antecedentIsFuture)
                                        {
                                            //Debug.WriteLine(" - Future = {2}Future.CW(func, ct({0}), tco({1}), TS.Default)", preCanceled ? "signaled" : "unsignaled", tco, preCompletedTask ? "C" : "U");
                                            continuation = ((Task<int>)antecedent).ContinueWith(_ => 5, ct, tco, TaskScheduler.Default);
                                        }
                                        else
                                        {
                                            //Debug.WriteLine(" - Future = {2}Task.CW(func, ct({0}), tco({1}), TS.Default)", preCanceled ? "signaled" : "unsignaled", tco, preCompletedTask ? "C" : "U");
                                            continuation = antecedent.ContinueWith(_ => 5, ct, tco, TaskScheduler.Default);
                                        }
                                    }
                                    else
                                    {
                                        if (antecedentIsFuture)
                                        {
                                            //Debug.WriteLine(" - Task = {2}Future.CW(action, ct({0}), tco({1}), TS.Default)", preCanceled ? "signaled" : "unsignaled", tco, preCompletedTask ? "C" : "U");
                                            continuation = ((Task<int>)antecedent).ContinueWith(_ => { }, ct, tco, TaskScheduler.Default);
                                        }
                                        else
                                        {
                                            //Debug.WriteLine(" - Task = {2}Task.CW(action, ct({0}), tco({1}), TS.Default)", preCanceled ? "signaled" : "unsignaled", tco, preCompletedTask ? "C" : "U");
                                            continuation = antecedent.ContinueWith(_ => { }, ct, tco, TaskScheduler.Default);
                                        }
                                    }

                                    // Start the antecedent, if necessary, or the continuation will never run.
                                    if (!preCompletedTask) antecedent.Start();

                                    Exception ex = null;
                                    int result = 0;

                                    try
                                    {
                                        continuation.Wait();
                                        if (continuationIsFuture) result = ((Task<int>)continuation).Result;
                                    }
                                    catch (Exception e)
                                    {
                                        ex = e;
                                    }

                                    Assert.True((continuation.Status == TaskStatus.RanToCompletion) || preCanceled,
                                       "RunContinueWithAllParamsTestsNoState: Expected continuation end as RanToCompletion unless pre-canceled");
                                    Assert.True((continuation.Status == TaskStatus.Canceled) || !preCanceled,
                                       "RunContinueWithAllParamsTestsNoState: Expected continuation to end as Canceled when pre-canceled");
                                    Assert.True((ex == null) || preCanceled,
                                       "RunContinueWithAllParamsTestsNoState: Got Wait() exception w/o pre-cancellation");
                                    Assert.True(continuation.CreationOptions == (TaskCreationOptions)tco,
                                       "RunContinueWithAllParamsTestsNoState: mis-matched CreationOptions");
                                    Assert.True((result == 5) || (!continuationIsFuture || preCanceled),
                                       "RunContinueWithAllParamsTestsNoState: Expected valid result from non-canceled Future continuation");
                                    if (preCanceled)
                                    {
                                        Assert.True(
                                           (ex != null) &&
                                           (ex.GetType() == typeof(AggregateException)) &&
                                           (((AggregateException)ex).InnerExceptions[0].GetType() == typeof(TaskCanceledException)),
                                           "RunContinueWithAllParamsTestsNoState: Expected AE(TCE) for Wait after pre-cancellation");
                                    }
                                }

                                //
                                // Test ContinueWith() overloads that take CancellationToken
                                //
                                {
                                    Task continuation = null;
                                    if (!preCompletedTask)
                                    {
                                        if (antecedentIsFuture) antecedent = new Task<int>(() => 5);
                                        else antecedent = new Task(() => { });
                                    }

                                    if (continuationIsFuture)
                                    {
                                        if (antecedentIsFuture)
                                        {
                                            //Debug.WriteLine(" - Future = {1}Future.CW(func, ct({0}))", preCanceled ? "signaled" : "unsignaled", preCompletedTask ? "C" : "U");
                                            continuation = ((Task<int>)antecedent).ContinueWith(_ => 5, ct);
                                        }
                                        else
                                        {
                                            //Debug.WriteLine(" - Future = {1}Task.CW(func, ct({0}))", preCanceled ? "signaled" : "unsignaled", preCompletedTask ? "C" : "U");
                                            continuation = antecedent.ContinueWith(_ => 5, ct);
                                        }
                                    }
                                    else
                                    {
                                        if (antecedentIsFuture)
                                        {
                                            //Debug.WriteLine(" - Task = {1}Future.CW(action, ct({0}))", preCanceled ? "signaled" : "unsignaled", preCompletedTask ? "C" : "U");
                                            continuation = ((Task<int>)antecedent).ContinueWith(_ => { }, ct);
                                        }
                                        else
                                        {
                                            //Debug.WriteLine(" - Task = {1}Task.CW(action, ct({0}))", preCanceled ? "signaled" : "unsignaled", preCompletedTask ? "C" : "U");
                                            continuation = antecedent.ContinueWith(_ => { }, ct);
                                        }
                                    }

                                    // Start the antecedent, if necessary, or the continuation will never run.
                                    if (!preCompletedTask) antecedent.Start();

                                    Exception ex = null;
                                    int result = 0;

                                    try
                                    {
                                        continuation.Wait();
                                        if (continuationIsFuture) result = ((Task<int>)continuation).Result;
                                    }
                                    catch (Exception e)
                                    {
                                        ex = e;
                                    }

                                    Assert.True((continuation.Status == TaskStatus.RanToCompletion) || preCanceled,
                                       "RunContinueWithAllParamsTestsNoState overloads: Expected continuation end as RanToCompletion unless pre-canceled");
                                    Assert.True((continuation.Status == TaskStatus.Canceled) || !preCanceled,
                                       "RunContinueWithAllParamsTestsNoState overloads: Expected continuation to end as Canceled when pre-canceled");
                                    Assert.True((ex == null) || preCanceled,
                                       "RunContinueWithAllParamsTestsNoState overloads: Got Wait() exception w/o pre-cancellation");
                                    Assert.True((result == 5) || (!continuationIsFuture || preCanceled),
                                       "RunContinueWithAllParamsTestsNoState overloads: Expected valid result from non-canceled Future continuation");
                                    if (preCanceled)
                                    {
                                        Assert.True(
                                           (ex != null) &&
                                           (ex.GetType() == typeof(AggregateException)) &&
                                           (((AggregateException)ex).InnerExceptions[0].GetType() == typeof(TaskCanceledException)),
                                           "RunContinueWithAllParamsTestsNoState overloads: Expected AE(TCE) for Wait after pre-cancellation");
                                    }
                                }

                                //
                                // Test ContinueWith() overloads that take TaskCreationOptions
                                //
                                {
                                    Task continuation = null;
                                    if (!preCompletedTask)
                                    {
                                        if (antecedentIsFuture) antecedent = new Task<int>(() => 5);
                                        else antecedent = new Task(() => { });
                                    }

                                    if (continuationIsFuture)
                                    {
                                        if (antecedentIsFuture)
                                        {
                                            continuation = ((Task<int>)antecedent).ContinueWith(_ => 5, tco);
                                        }
                                        else
                                        {
                                            continuation = antecedent.ContinueWith(_ => 5, tco);
                                        }
                                    }
                                    else
                                    {
                                        if (antecedentIsFuture)
                                        {
                                            continuation = ((Task<int>)antecedent).ContinueWith(_ => { }, tco);
                                        }
                                        else
                                        {
                                            continuation = antecedent.ContinueWith(_ => { }, tco);
                                        }
                                    }

                                    // Start the antecedent, if necessary, or the continuation will never run.
                                    if (!preCompletedTask) antecedent.Start();

                                    Exception ex = null;
                                    int result = 0;

                                    try
                                    {
                                        continuation.Wait();
                                        if (continuationIsFuture) result = ((Task<int>)continuation).Result;
                                    }
                                    catch (Exception e)
                                    {
                                        ex = e;
                                    }

                                    Assert.True(continuation.Status == TaskStatus.RanToCompletion,
                                       "RunContinueWithAllParamsTestsNoState: Expected continuation to end as RanToCompletion");
                                    Assert.True(ex == null,
                                       "RunContinueWithAllParamsTestsNoState: Got Wait() exception");
                                    Assert.True(continuation.CreationOptions == (TaskCreationOptions)tco,
                                       "RunContinueWithAllParamsTestsNoState: Mis-matched CreationOptions");
                                    Assert.True((result == 5) || (!continuationIsFuture),
                                       "RunContinueWithAllParamsTestsNoState: Expected valid result from Future continuation");
                                }

                                //
                                // The ContinueWith overloads that take a TaskScheduler are already being tested in RunContinueWithTMTests().
                                // So I won't add a block of such tests here.
                                //
                            }
                        }
                    }
                }
            }
        }

        [Fact]
        public static void TestNoDeadlockOnContinueWith()
        {
            Debug.WriteLine("TestNoDeadlockOnContinueWith:  shouldn't deadlock if it passes.");
            const int ITERATIONS = 1000;
            var tasks = new Task<int>[ITERATIONS];

            for (int i = 0; i < ITERATIONS; i++)
            {
                tasks[i] = Choose(CancellationToken.None);
            }

            try { Task.WaitAll(tasks); }
            catch (AggregateException ae) { ae.Handle(e => e is TaskCanceledException); }
            Debug.WriteLine("Success!");
        }

        [Fact]
        public static void RunLazyCancellationTests()
        {
            for (int i = 0; i < 2; i++)
            {
                bool useLazyCancellation = (i == 0);
                TaskContinuationOptions options = useLazyCancellation ? TaskContinuationOptions.LazyCancellation : TaskContinuationOptions.None;

                for (int j = 0; j < 3; j++)
                {
                    bool useContinueWith = (j == 0);
                    bool useContinueWhenAny = (j == 1);
                    bool useContinueWhenAll = (j == 2);
                    string type = useContinueWith ? "ContinueWith" : useContinueWhenAny ? "ContinueWhenAny" : "ContinueWhenAll";
                    Debug.WriteLine("    ** Options = " + options + ", continuation type = " + type);

                    {
                        CancellationTokenSource cts = new CancellationTokenSource();
                        ManualResetEventSlim mres1 = new ManualResetEventSlim(false);
                        ManualResetEventSlim mres2 = new ManualResetEventSlim(false);
                        Task t1 = new Task(() => { });

                        Task c1 = null;
                        if (useContinueWith)
                        {
                            c1 = t1.ContinueWith(_ => { mres1.Set(); mres2.Wait(); }, cts.Token, options, TaskScheduler.Default);
                        }
                        else if (useContinueWhenAny)
                        {
                            c1 = Task.Factory.ContinueWhenAny(new Task[] { t1 }, _ => { mres1.Set(); mres2.Wait(); }, cts.Token, options, TaskScheduler.Default);
                        }
                        else
                        {
                            c1 = Task.Factory.ContinueWhenAll(new Task[] { t1 }, _ => { mres1.Set(); mres2.Wait(); }, cts.Token, options, TaskScheduler.Default);
                        }

                        t1.Start();
                        mres1.Wait(); // Wait for continuation to start
                        cts.Cancel(); // Cancel the continuation
                        mres2.Set();  // Allow continuation to end

                        Debug.WriteLine("T3:  About to wait on c1.");
                        try
                        {
                            c1.Wait();
                        }
                        catch (Exception e)
                        {
                            Assert.True(false, string.Format("RunLazyCancellationTests: ERROR. Did not expect c1.Wait() to throw an exception, got " + e.ToString()));
                        }
                    }
                    Debug.WriteLine("Finished successfully.");
                }
            }
        }

        [Fact]
        public static void RunLazyCancellationTests_Negative()
        {
            for (int i = 0; i < 2; i++)
            {
                bool useLazyCancellation = (i == 0);
                TaskContinuationOptions options = useLazyCancellation ? TaskContinuationOptions.LazyCancellation : TaskContinuationOptions.None;

                for (int j = 0; j < 3; j++)
                {
                    bool useContinueWith = (j == 0);
                    bool useContinueWhenAny = (j == 1);
                    bool useContinueWhenAll = (j == 2);
                    string type = useContinueWith ? "ContinueWith" : useContinueWhenAny ? "ContinueWhenAny" : "ContinueWhenAll";
                    Debug.WriteLine("    ** Options = " + options + ", continuation type = " + type);

                    {
                        CancellationTokenSource cts = new CancellationTokenSource();
                        Task t1 = new Task(() => { });
                        cts.Cancel();

                        Task c1 = null;
                        if (useContinueWith)
                        {
                            c1 = t1.ContinueWith(_ => { }, cts.Token, options, TaskScheduler.Default);
                        }
                        else if (useContinueWhenAny)
                        {
                            c1 = Task.Factory.ContinueWhenAny(new Task[] { t1 }, _ => { }, cts.Token, options, TaskScheduler.Default);
                        }
                        else
                        {
                            c1 = Task.Factory.ContinueWhenAll(new Task[] { t1 }, _ => { }, cts.Token, options, TaskScheduler.Default);
                        }

                        Assert.True(c1.IsCompleted != useLazyCancellation,
                           "RunLazyCancellationTests: Before t1.Start(), c1.IsCompleted = " + c1.IsCompleted);

                        t1.Start();
                        Debug.WriteLine("T1:  About to wait on c1.");
                        EnsureTaskCanceledExceptionThrown(() => { c1.Wait(); },
                           "RunLazyCancellationTests:  Expected TCE on c1.Wait after antecedent started");
                    }
                    {
                        CancellationTokenSource cts = new CancellationTokenSource();
                        Task t1 = new Task(() => { });

                        Task c1 = null;
                        if (useContinueWith)
                        {
                            c1 = t1.ContinueWith(_ => { }, cts.Token, options, TaskScheduler.Default);
                        }
                        else if (useContinueWhenAny)
                        {
                            c1 = Task.Factory.ContinueWhenAny(new Task[] { t1 }, _ => { }, cts.Token, options, TaskScheduler.Default);
                        }
                        else
                        {
                            c1 = Task.Factory.ContinueWhenAll(new Task[] { t1 }, _ => { }, cts.Token, options, TaskScheduler.Default);
                        }
                        cts.Cancel();

                        Assert.True(c1.IsCompleted != useLazyCancellation,
                           "RunLazyCancellationTests: Before t1.Start(), c1.IsCompleted = " + c1.IsCompleted);

                        t1.Start();
                        Debug.WriteLine("T2:  About to wait on c1.");
                        EnsureTaskCanceledExceptionThrown(() => { c1.Wait(); },
                           "RunLazyCancellationTests:  Expected TCE on c1.Wait after antecedent started");
                    }
                }
            }
        }

        [Fact]
        public static void RunStackGuardTests()
        {
            const int DIVE_DEPTH = 12000;

            // Test stack guard with ContinueWith.
            {
                Func<Task, Task> func = completed => completed.ContinueWith(delegate { }, TaskContinuationOptions.ExecuteSynchronously);
                var tcs = new TaskCompletionSource<bool>();
                var t = (Task)tcs.Task;
                for (int i = 0; i < DIVE_DEPTH; i++) t = func(t);
                tcs.TrySetResult(true);
                t.Wait();
            }

            // These tests will have stack overflowed if they failed.
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public static void TestNoDeadlockOnContinueWithExecuteSynchronously(bool useWaitAll)
        {
            // Verify that Task.Wait can return before all continuations scheduled
            // with ExecuteSynchronously complete

            Task task1 = new Task(() => { });

            var barrier = new Barrier(2);
            Task task2 = task1.ContinueWith((_task) => 
            {
                barrier.SignalAndWait(); // alert caller that we've started running
                barrier.SignalAndWait(); // wait for caller to be done waiting
            }, TaskContinuationOptions.ExecuteSynchronously);

            task1.Start();
            barrier.SignalAndWait(); // wait for task to start running

            // Wait should return once the task is complete, regardless of what other 
            // continuations were scheduled off of it.
            if (useWaitAll)
            {
                Task.WaitAll(task1);
            }
            else
            {
                task1.Wait();
            }

            barrier.SignalAndWait(); // alert task that we're done waiting
            task2.Wait();
        }

        [Fact]
        public static void ContinueWith_ArgumentNull()
        {
            Assert.Throws<ArgumentNullException>(() => { new Task(() => { }).ContinueWith((Action<Task>)null); });
            Assert.Throws<ArgumentNullException>(() => { new Task(() => { }).ContinueWith((Action<Task>)null, new CancellationTokenSource().Token); });
            Assert.Throws<ArgumentNullException>(() => { new Task(() => { }).ContinueWith((Action<Task>)null, TaskContinuationOptions.None); });
            Assert.Throws<ArgumentNullException>(() => { new Task(() => { }).ContinueWith((Action<Task>)null, TaskScheduler.Default); });
            Assert.Throws<ArgumentNullException>(() => { new Task(() => { }).ContinueWith(_ => { }, (TaskScheduler)null); });
            Assert.Throws<ArgumentNullException>(() => { new Task(() => { }).ContinueWith((Action<Task>)null, new CancellationTokenSource().Token, TaskContinuationOptions.None, TaskScheduler.Default); });
            Assert.Throws<ArgumentNullException>(() => { new Task(() => { }).ContinueWith(_ => { }, new CancellationTokenSource().Token, TaskContinuationOptions.None, (TaskScheduler)null); });

            Assert.Throws<ArgumentNullException>(() => { new Task(() => { }).ContinueWith((Func<Task, int>)null); });
            Assert.Throws<ArgumentNullException>(() => { new Task(() => { }).ContinueWith((Func<Task, int>)null, new CancellationTokenSource().Token); });
            Assert.Throws<ArgumentNullException>(() => { new Task(() => { }).ContinueWith((Func<Task, int>)null, TaskContinuationOptions.None); });
            Assert.Throws<ArgumentNullException>(() => { new Task(() => { }).ContinueWith((Func<Task, int>)null, TaskScheduler.Default); });
            Assert.Throws<ArgumentNullException>(() => { new Task(() => { }).ContinueWith(_ => 0, (TaskScheduler)null); });
            Assert.Throws<ArgumentNullException>(() => { new Task(() => { }).ContinueWith((Func<Task, int>)null, new CancellationTokenSource().Token, TaskContinuationOptions.None, TaskScheduler.Default); });
            Assert.Throws<ArgumentNullException>(() => { new Task(() => { }).ContinueWith(_ => 0, new CancellationTokenSource().Token, TaskContinuationOptions.None, (TaskScheduler)null); });

            Assert.Throws<ArgumentNullException>(() => { new Task<int>(() => 0).ContinueWith((Action<Task<int>>)null); });
            Assert.Throws<ArgumentNullException>(() => { new Task<int>(() => 0).ContinueWith((Action<Task<int>>)null, new CancellationTokenSource().Token); });
            Assert.Throws<ArgumentNullException>(() => { new Task<int>(() => 0).ContinueWith((Action<Task<int>>)null, TaskContinuationOptions.None); });
            Assert.Throws<ArgumentNullException>(() => { new Task<int>(() => 0).ContinueWith((Action<Task<int>>)null, TaskScheduler.Default); });
            Assert.Throws<ArgumentNullException>(() => { new Task<int>(() => 0).ContinueWith(_ => { }, (TaskScheduler)null); });
            Assert.Throws<ArgumentNullException>(() => { new Task<int>(() => 0).ContinueWith((Action<Task<int>>)null, new CancellationTokenSource().Token, TaskContinuationOptions.None, TaskScheduler.Default); });
            Assert.Throws<ArgumentNullException>(() => { new Task<int>(() => 0).ContinueWith(_ => { }, new CancellationTokenSource().Token, TaskContinuationOptions.None, (TaskScheduler)null); });

            Assert.Throws<ArgumentNullException>(() => { new Task<int>(() => 0).ContinueWith((Func<Task<int>, int>)null); });
            Assert.Throws<ArgumentNullException>(() => { new Task<int>(() => 0).ContinueWith((Func<Task<int>, int>)null, new CancellationTokenSource().Token); });
            Assert.Throws<ArgumentNullException>(() => { new Task<int>(() => 0).ContinueWith((Func<Task<int>, int>)null, TaskContinuationOptions.None); });
            Assert.Throws<ArgumentNullException>(() => { new Task<int>(() => 0).ContinueWith((Func<Task<int>, int>)null, TaskScheduler.Default); });
            Assert.Throws<ArgumentNullException>(() => { new Task<int>(() => 0).ContinueWith(_ => 0, (TaskScheduler)null); });
            Assert.Throws<ArgumentNullException>(() => { new Task<int>(() => 0).ContinueWith((Func<Task<int>, int>)null, new CancellationTokenSource().Token, TaskContinuationOptions.None, TaskScheduler.Default); });
            Assert.Throws<ArgumentNullException>(() => { new Task<int>(() => 0).ContinueWith(_ => 0, new CancellationTokenSource().Token, TaskContinuationOptions.None, (TaskScheduler)null); });

            Assert.Throws<ArgumentNullException>(() => { new Task(() => { }).ContinueWith((Action<Task, object>)null, new object()); });
            Assert.Throws<ArgumentNullException>(() => { new Task(() => { }).ContinueWith((Action<Task, object>)null, new object(), new CancellationTokenSource().Token); });
            Assert.Throws<ArgumentNullException>(() => { new Task(() => { }).ContinueWith((Action<Task, object>)null, new object(), TaskContinuationOptions.None); });
            Assert.Throws<ArgumentNullException>(() => { new Task(() => { }).ContinueWith((Action<Task, object>)null, new object(), TaskScheduler.Default); });
            Assert.Throws<ArgumentNullException>(() => { new Task(() => { }).ContinueWith((t, o) => { }, new object(), (TaskScheduler)null); });
            Assert.Throws<ArgumentNullException>(() => { new Task(() => { }).ContinueWith((Action<Task, object>)null, new object(), new CancellationTokenSource().Token, TaskContinuationOptions.None, TaskScheduler.Default); });
            Assert.Throws<ArgumentNullException>(() => { new Task(() => { }).ContinueWith((t, o) => { }, new object(), new CancellationTokenSource().Token, TaskContinuationOptions.None, (TaskScheduler)null); });

            Assert.Throws<ArgumentNullException>(() => { new Task(() => { }).ContinueWith((Func<Task, object, int>)null, new object()); });
            Assert.Throws<ArgumentNullException>(() => { new Task(() => { }).ContinueWith((Func<Task, object, int>)null, new object(), new CancellationTokenSource().Token); });
            Assert.Throws<ArgumentNullException>(() => { new Task(() => { }).ContinueWith((Func<Task, object, int>)null, new object(), TaskContinuationOptions.None); });
            Assert.Throws<ArgumentNullException>(() => { new Task(() => { }).ContinueWith((Func<Task, object, int>)null, new object(), TaskScheduler.Default); });
            Assert.Throws<ArgumentNullException>(() => { new Task(() => { }).ContinueWith((t, o) => 0, new object(), (TaskScheduler)null); });
            Assert.Throws<ArgumentNullException>(() => { new Task(() => { }).ContinueWith((Func<Task, object, int>)null, new object(), new CancellationTokenSource().Token, TaskContinuationOptions.None, TaskScheduler.Default); });
            Assert.Throws<ArgumentNullException>(() => { new Task(() => { }).ContinueWith((t, o) => 0, new object(), new CancellationTokenSource().Token, TaskContinuationOptions.None, (TaskScheduler)null); });

            Assert.Throws<ArgumentNullException>(() => { new Task<int>(() => 0).ContinueWith((Action<Task<int>, object>)null, new object()); });
            Assert.Throws<ArgumentNullException>(() => { new Task<int>(() => 0).ContinueWith((Action<Task<int>, object>)null, new object(), new CancellationTokenSource().Token); });
            Assert.Throws<ArgumentNullException>(() => { new Task<int>(() => 0).ContinueWith((Action<Task<int>, object>)null, new object(), TaskContinuationOptions.None); });
            Assert.Throws<ArgumentNullException>(() => { new Task<int>(() => 0).ContinueWith((Action<Task<int>, object>)null, new object(), TaskScheduler.Default); });
            Assert.Throws<ArgumentNullException>(() => { new Task<int>(() => 0).ContinueWith((t, o) => { }, new object(), (TaskScheduler)null); });
            Assert.Throws<ArgumentNullException>(() => { new Task<int>(() => 0).ContinueWith((Action<Task<int>, object>)null, new object(), new CancellationTokenSource().Token, TaskContinuationOptions.None, TaskScheduler.Default); });
            Assert.Throws<ArgumentNullException>(() => { new Task<int>(() => 0).ContinueWith((t, o) => { }, new object(), new CancellationTokenSource().Token, TaskContinuationOptions.None, (TaskScheduler)null); });

            Assert.Throws<ArgumentNullException>(() => { new Task<int>(() => 0).ContinueWith((Func<Task<int>, object, int>)null, new object()); });
            Assert.Throws<ArgumentNullException>(() => { new Task<int>(() => 0).ContinueWith((Func<Task<int>, object, int>)null, new object(), new CancellationTokenSource().Token); });
            Assert.Throws<ArgumentNullException>(() => { new Task<int>(() => 0).ContinueWith((Func<Task<int>, object, int>)null, new object(), TaskContinuationOptions.None); });
            Assert.Throws<ArgumentNullException>(() => { new Task<int>(() => 0).ContinueWith((Func<Task<int>, object, int>)null, new object(), TaskScheduler.Default); });
            Assert.Throws<ArgumentNullException>(() => { new Task<int>(() => 0).ContinueWith((t, o) => 0, new object(), (TaskScheduler)null); });
            Assert.Throws<ArgumentNullException>(() => { new Task<int>(() => 0).ContinueWith((Func<Task<int>, object, int>)null, new object(), new CancellationTokenSource().Token, TaskContinuationOptions.None, TaskScheduler.Default); });
            Assert.Throws<ArgumentNullException>(() => { new Task<int>(() => 0).ContinueWith((t, o) => 0, new object(), new CancellationTokenSource().Token, TaskContinuationOptions.None, (TaskScheduler)null); });
        }

        #endregion

        #region Helper Methods

        // Ensures that the specified action throws a AggregateException wrapping a TaskCanceledException
        public static void EnsureTaskCanceledExceptionThrown(Action action, string message)
        {
            Exception exception = null;

            try
            {
                action();
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            EnsureExceptionIsAEofTCE(exception, message);
        }

        // Ensures that the specified exception is an AggregateException wrapping a TaskCanceledException
        public static void EnsureExceptionIsAEofTCE(Exception exception, string message)
        {
            if (exception == null)
            {
                Assert.True(false, string.Format(message + " (no exception thrown)")); ;
            }
            else if (exception.GetType() != typeof(AggregateException))
            {
                Assert.True(false, string.Format(message + " (didn't throw aggregate exception)"));
            }
            else if (((AggregateException)exception).InnerException.GetType() != typeof(TaskCanceledException))
            {
                exception = ((AggregateException)exception).InnerException;
                Assert.True(false, string.Format(message + " (threw " + exception.GetType().Name + " instead of TaskCanceledException)"));
            }
        }

        private static Task<Int32> Choose(CancellationToken cancellationToken)
        {
            // Set up completion structures
            //var boxedCompleted = new StrongBox<Task>(); // Acts as both completion marker and sync obj for targets
            var result = new TaskCompletionSource<int>();

            // Set up teardown cancellation.  We will request cancellation when a) the supplied options token
            // has cancellation requested or b) when we actually complete somewhere in order to tear down
            // the rest of our configured set up.
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, CancellationToken.None);

            Task branch1 = CreateChooseBranch<int>(cts, result, TaskScheduler.Default);
            Task branch2 = CreateChooseBranch<int>(cts, result, TaskScheduler.Default);
            Task.Factory.ContinueWhenAll(new[] { branch1, branch2 }, tasks =>
            {
                result.TrySetCanceled();
                cts.Dispose();
            }, CancellationToken.None, (TaskContinuationOptions)TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);

            // Return the task representing the choice
            return result.Task;
        }

        private static Task CreateChooseBranch<T>(CancellationTokenSource cts,
            TaskCompletionSource<int> result, TaskScheduler scheduler)
        {
            // If the cancellation token is already canceled, there is no need to create and link a target.
            // Insted, directly return a canceled task
            if (cts.IsCancellationRequested)
            {
                var canceledTaskSource = new TaskCompletionSource<object>();
                canceledTaskSource.TrySetCanceled();
                return canceledTaskSource.Task;
            }

            {
                // WE ARE CREATING A BUNCH OF TASKS THAT SHARE THE SAME CANCELLATION TOKEN
                var t = Task<T>.Factory.StartNew(() => { return default(T); }, cts.Token);
                t.ContinueWith(delegate { }, cts.Token, (TaskContinuationOptions)TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);

                return Task<T>.Factory.StartNew(() => { if (!cts.IsCancellationRequested) cts.Cancel(); return default(T); }, cts.Token);
            }
        }

        #endregion
    }
}
