// Licensed to the .NET Foundation under one or more agreements. 
// The .NET Foundation licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information. 

// Summary: Test suite for the below scenario:
// An array of tasks that can have different workloads
// WaitAny and WaitAll
//      - with/without timeout is performed on them
//      - the allowed delta for cases with timeout:10 ms
// Scheduler used : current ( ThreadPool)
//
// Observations:
// 1. The input data for tasks does not contain any Exceptional or Cancelled tasks.
// 2. WaitAll on array with cancelled tasks can be found at: Functional\TPL\YetiTests\TaskWithYeti\TaskWithYeti.cs
// 3. WaitAny/WaitAll with token tests can be found at:Functional\TPL\YetiTests\TaskCancellation\TaskCancellation.cs

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xunit;

namespace System.Threading.Tasks.Tests
{
    public static class TaskWaitAllAnyTests
    {
        private static readonly TimeSpan MaxSafeTimeout = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan DelayRange = TimeSpan.FromMilliseconds(3);

        /// <summary>
        /// Gets a row of data for task tests.
        /// </summary>
        /// <returns>A row of test data.</returns>
        public static IEnumerable<object[]> Task_Data()
        {
            yield return new object[] { new TimeSpan[] { /* nothing */ } };

            foreach (TimeSpan work in new[] { Workloads.VeryLight, Workloads.Light, Workloads.Medium })
            {
                yield return new object[] { new[] { work } };

                foreach (TimeSpan load in new[] { Workloads.VeryLight, Workloads.Light, Workloads.Medium })
                {
                    yield return new object[] { new[] { work, load } };
                }
            }

            yield return new object[] { new[] { Workloads.Medium, Workloads.Light, Workloads.Medium, Workloads.VeryLight, Workloads.Medium } };
        }

        /// <summary>
        /// Gets a row of data for task tests.
        /// </summary>
        /// The format is:
        ///  1. A list of task workloads
        ///  2. The maximum duration to wait (-1 means no timeout)
        /// <returns>A row of test data.</returns>
        public static IEnumerable<object[]> Task_Wait_Data()
        {
            foreach (object[] data in Task_Data())
            {
                foreach (TimeSpan wait in new[] { Waits.Infinite, Waits.Long, Waits.Short, Waits.Instant })
                {
                    yield return new object[] { data[0], wait };
                }
            }
        }

        /// <summary>
        /// Gets a row of data for task tests.
        /// </summary>
        /// <returns>A row of test data.</returns>
        public static IEnumerable<object[]> Task_Data_Longrunning()
        {
            foreach (TimeSpan work in new[] { Workloads.Heavy, Workloads.VeryHeavy })
            {
                yield return new object[] { new[] { work } };

                foreach (TimeSpan load in new[] { Workloads.VeryLight, Workloads.Light, Workloads.Medium })
                {
                    yield return new object[] { new[] { work, load } };
                }
            }

            yield return new object[] { new[] { Workloads.Medium, Workloads.Light, Workloads.Heavy, Workloads.Medium, Workloads.VeryHeavy, Workloads.VeryLight, Workloads.Medium } };
        }

        /// <summary>
        /// Gets a row of data for task tests.
        /// </summary>
        /// The format is:
        ///  1. A list of task workloads
        ///  2. The maximum duration to wait (-1 means no timeout)
        /// <returns>A row of test data.</returns>
        public static IEnumerable<object[]> Task_Wait_Data_Longrunning()
        {
            foreach (object[] data in Task_Data_Longrunning())
            {
                foreach (TimeSpan wait in new[] { Waits.Infinite, Waits.Long, Waits.Short })
                {
                    yield return new object[] { data[0], wait };
                }
            }
        }

        /// <summary>
        /// Gets a row of data for canceled task tests.
        /// </summary>
        /// Shorter waits can't be used due to potential for race conditions
        /// The format is:
        ///  1. A list of task workloads
        ///  2. The maximum duration to wait (-1 means no timeout)
        /// <returns>A row of test data.</returns>
        public static IEnumerable<object[]> Task_Wait_Cancel_Data()
        {
            foreach (object[] data in Task_Data())
            {
                foreach (TimeSpan wait in new[] { Waits.Infinite, TimeSpan.FromMinutes(1) })
                {
                    yield return new object[] { data[0], wait };
                }
            }
        }

        /// <summary>
        /// Gets a row of data for canceled task tests.
        /// </summary>
        /// Shorter waits can't be used due to potential for race conditions
        /// The format is:
        ///  1. The number of tasks
        ///  2. The first task to complete
        /// <returns>A row of test data.</returns>
        public static IEnumerable<object[]> Task_Specific_Data()
        {
            foreach (int tasks in new[] { 0, 1, 2, 10, 64, 100 })
            {
                foreach (int completes in new[] { 0, tasks / 2, tasks - 1 })
                {
                    yield return new object[] { tasks, completes };
                }
            }
        }

        /// <summary>
        /// Gets a row of data for specific tasks finishing for WaitAny tests.
        /// </summary>
        /// Shorter waits can't be used due to potential for race conditions
        /// The format is:
        ///  1. Number of tasks
        ///  2. Which task completes
        ///  3. The maximum duration to wait (-1 means no timeout)
        /// <returns>A row of test data.</returns>
        public static IEnumerable<object[]> Task_Wait_Specific_Data()
        {
            foreach (object[] data in Task_Specific_Data())
            {
                foreach (TimeSpan wait in new[] { Waits.Infinite, TimeSpan.FromMinutes(1) })
                {
                    yield return new object[] { data[0], data[1], wait };
                }
            }
        }

        [Theory]
        [MemberData("Task_Data")]
        public static void Task_WaitAll(TimeSpan[] loads)
        {
            WaitAll(loads, Waits.Infinite, (tasks, i) => { Task.WaitAll(tasks); return true; });
        }

        [Theory]
        [OuterLoop]
        [MemberData("Task_Data_Longrunning")]
        public static void Task_WaitAll_Longrunning(TimeSpan[] loads)
        {
            Task_WaitAll(loads);
        }

        [Theory]
        [MemberData("Task_Data")]
        public static void Task_WaitAll_CancelTask(TimeSpan[] loads)
        {
            WaitAll_CancelTask(loads, Waits.Infinite, (tasks, i) => Task.WaitAll(tasks));
        }

        [Theory]
        [MemberData("Task_Data")]
        public static void Task_WaitAll_FaultTask(TimeSpan[] loads)
        {
            WaitAll_FaultTask(loads, Waits.Infinite, (tasks, i) => Task.WaitAll(tasks));
        }

        [Theory]
        [MemberData("Task_Data")]
        public static void Task_WaitAll_Token(TimeSpan[] loads)
        {
            WaitAll(loads, Waits.Infinite, (tasks, i) => { Task.WaitAll(tasks, new CancellationToken()); return true; });
        }

        [Theory]
        [OuterLoop]
        [MemberData("Task_Data_Longrunning")]
        public static void Task_WaitAll_Token_Longrunning(TimeSpan[] loads)
        {
            Task_WaitAll_Token(loads);
        }

        [Theory]
        [MemberData("Task_Data")]
        public static void Task_WaitAll_Token_Cancel(TimeSpan[] loads)
        {
            Cancel(loads, Waits.Infinite, (tasks, i, token) => Task.WaitAll(tasks, token));
        }

        [Theory]
        [MemberData("Task_Data")]
        public static void Task_WaitAll_Token_CancelTask(TimeSpan[] loads)
        {
            WaitAll_CancelTask(loads, Waits.Infinite, (tasks, i) => Task.WaitAll(tasks, new CancellationTokenSource().Token));
        }

        [Theory]
        [MemberData("Task_Data")]
        public static void Task_WaitAll_Token_FaultTask(TimeSpan[] loads)
        {
            WaitAll_FaultTask(loads, Waits.Infinite, (tasks, i) => Task.WaitAll(tasks, new CancellationTokenSource().Token));
        }

        [Theory]
        [MemberData("Task_Wait_Data")]
        public static void Task_WaitAll_TimeSpan(TimeSpan[] loads, TimeSpan wait)
        {
            WaitAll(loads, wait, (tasks, w) => Task.WaitAll(tasks, w));
        }

        [Theory]
        [OuterLoop]
        [MemberData("Task_Wait_Data_Longrunning")]
        public static void Task_WaitAll_TimeSpan_Longrunning(TimeSpan[] loads, TimeSpan wait)
        {
            Task_WaitAll_TimeSpan(loads, wait);
        }

        [Theory]
        [MemberData("Task_Wait_Cancel_Data")]
        public static void Task_WaitAll_Timespan_CancelTask(TimeSpan[] loads, TimeSpan wait)
        {
            WaitAll_CancelTask(loads, wait, (tasks, w) => Task.WaitAll(tasks, w));
        }

        [Theory]
        [MemberData("Task_Wait_Cancel_Data")]
        public static void Task_WaitAll_TimeSpan_FaultTask(TimeSpan[] loads, TimeSpan wait)
        {
            WaitAll_FaultTask(loads, wait, (tasks, w) => Task.WaitAll(tasks, w));
        }

        [Theory]
        [MemberData("Task_Wait_Data")]
        public static void Task_WaitAll_Millisecond(TimeSpan[] loads, TimeSpan wait)
        {
            WaitAll(loads, wait, (tasks, w) => Task.WaitAll(tasks, (int)w.TotalMilliseconds));
        }

        [Theory]
        [OuterLoop]
        [MemberData("Task_Wait_Data_Longrunning")]
        public static void Task_WaitAll_Millisecond_Longrunning(TimeSpan[] loads, TimeSpan wait)
        {
            Task_WaitAll_Millisecond(loads, wait);
        }

        [Theory]
        [MemberData("Task_Wait_Cancel_Data")]
        public static void Task_WaitAll_Millisecond_CancelTask(TimeSpan[] loads, TimeSpan wait)
        {
            WaitAll_CancelTask(loads, wait, (tasks, w) => Task.WaitAll(tasks, (int)w.TotalMilliseconds));
        }

        [Theory]
        [MemberData("Task_Wait_Cancel_Data")]
        public static void Task_WaitAll_Millisecond_FaultTask(TimeSpan[] loads, TimeSpan wait)
        {
            WaitAll_FaultTask(loads, wait, (tasks, w) => Task.WaitAll(tasks, (int)w.TotalMilliseconds));
        }

        [Theory]
        [MemberData("Task_Wait_Data")]
        public static void Task_WaitAll_Millisecond_Token(TimeSpan[] loads, TimeSpan wait)
        {
            WaitAll(loads, wait, (tasks, w) => Task.WaitAll(tasks, (int)w.TotalMilliseconds, new CancellationToken()));
        }

        [Theory]
        [OuterLoop]
        [MemberData("Task_Wait_Data_Longrunning")]
        public static void Task_WaitAll_Millisecond_Token_Longrunning(TimeSpan[] loads, TimeSpan wait)
        {
            Task_WaitAll_Millisecond_Token(loads, wait);
        }

        [Theory]
        [MemberData("Task_Wait_Cancel_Data")]
        public static void Task_WaitAll_Millisecond_Token_Cancel(TimeSpan[] loads, TimeSpan wait)
        {
            Cancel(loads, wait, (tasks, w, token) => Task.WaitAll(tasks, (int)w.TotalMilliseconds, token));
        }

        [Theory]
        [MemberData("Task_Wait_Cancel_Data")]
        public static void Task_WaitAll_Millisecond_Token_CancelTask(TimeSpan[] loads, TimeSpan wait)
        {
            WaitAll_CancelTask(loads, wait, (tasks, w) => Task.WaitAll(tasks, (int)w.TotalMilliseconds, new CancellationTokenSource().Token));
        }

        [Theory]
        [MemberData("Task_Wait_Cancel_Data")]
        public static void Task_WaitAll_Millisecond_Token_FaultTask(TimeSpan[] loads, TimeSpan wait)
        {
            WaitAll_FaultTask(loads, wait, (tasks, w) => Task.WaitAll(tasks, (int)w.TotalMilliseconds, new CancellationTokenSource().Token));
        }

        [Fact]
        public static void WaitAll_Argument_Exception()
        {
            Assert.Throws<ArgumentException>(() => Task.WaitAll(new Task[] { null }));
            Assert.Throws<ArgumentException>(() => Task.WaitAll(new Task[] { Task.CompletedTask, null }));

            Assert.Throws<ArgumentException>(() => Task.WaitAll(new Task[] { null }, 0));
            Assert.Throws<ArgumentException>(() => Task.WaitAll(new Task[] { Task.CompletedTask, null }, 0));

            Assert.Throws<ArgumentException>(() => Task.WaitAll(new Task[] { null }, TimeSpan.Zero));
            Assert.Throws<ArgumentException>(() => Task.WaitAll(new Task[] { Task.CompletedTask, null }, TimeSpan.Zero));

            Assert.Throws<ArgumentException>(() => Task.WaitAll(new Task[] { null }, new CancellationToken()));
            Assert.Throws<ArgumentException>(() => Task.WaitAll(new Task[] { Task.CompletedTask, null }, new CancellationToken()));

            Assert.Throws<ArgumentException>(() => Task.WaitAll(new Task[] { null }, 0, new CancellationToken()));
            Assert.Throws<ArgumentException>(() => Task.WaitAll(new Task[] { Task.CompletedTask, null }, 0, new CancellationToken()));
        }

        [Fact]
        public static void WaitAll_ArgumentOutOfRange_Exception()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Task.WaitAll(new Task[] { }, -2));
            Assert.Throws<ArgumentOutOfRangeException>(() => Task.WaitAll(new Task[] { Task.CompletedTask }, -2));

            TimeSpan invalid = TimeSpan.FromMilliseconds(-2);
            Assert.Throws<ArgumentOutOfRangeException>(() => Task.WaitAll(new Task[] { }, invalid));
            Assert.Throws<ArgumentOutOfRangeException>(() => Task.WaitAll(new Task[] { Task.CompletedTask }, invalid));

            Assert.Throws<ArgumentOutOfRangeException>(() => Task.WaitAll(new Task[] { }, -2, new CancellationToken()));
            Assert.Throws<ArgumentOutOfRangeException>(() => Task.WaitAll(new Task[] { Task.CompletedTask }, -2, new CancellationToken()));
        }

        [Fact]
        public static void WaitAll_PreCanceled_Token()
        {
            Assert.Throws<OperationCanceledException>(() => Task.WaitAll(new[] { new Task(() => { /* nothing */ }) }, new CancellationToken(true)));
            Assert.Throws<OperationCanceledException>(() => Task.WaitAll(new[] { new Task(() => { /* nothing */ }) }, 1, new CancellationToken(true)));
            Assert.Throws<OperationCanceledException>(() => Task.WaitAll(new[] { new Task(() => { /* nothing */ }) }, -1, new CancellationToken(true)));
            Assert.Throws<OperationCanceledException>(() => Task.WaitAll(new[] { new Task(() => { /* nothing */ }) }, int.MaxValue, new CancellationToken(true)));
            // Cancellation status is checked _before_ wait/completion
            Assert.Throws<OperationCanceledException>(() => Task.WaitAll(new[] { new Task(() => { /* nothing */ }) }, 0, new CancellationToken(true)));
            Assert.Throws<OperationCanceledException>(() => Task.WaitAll(new[] { Task.CompletedTask }, -1, new CancellationToken(true)));
        }

        [Fact]
        public static void WaitAll_CanceledTask()
        {
            Functions.AssertThrowsWrapped<TaskCanceledException>(() => Task.WaitAll(new[] { Task.FromCanceled(new CancellationToken(true)) }));
            Functions.AssertThrowsWrapped<TaskCanceledException>(() => Task.WaitAll(new[] { Task.FromCanceled(new CancellationToken(true)), Task.CompletedTask }));
            Functions.AssertThrowsWrapped<TaskCanceledException>(() => Task.WaitAll(new[] { Task.FromCanceled(new CancellationToken(true)) }, 1));
            Functions.AssertThrowsWrapped<TaskCanceledException>(() => Task.WaitAll(new[] { Task.FromCanceled(new CancellationToken(true)), Task.CompletedTask }, 1));
            Functions.AssertThrowsWrapped<TaskCanceledException>(() => Task.WaitAll(new[] { Task.FromCanceled(new CancellationToken(true)) }, new CancellationToken(false)));
            Functions.AssertThrowsWrapped<TaskCanceledException>(() => Task.WaitAll(new[] { Task.FromCanceled(new CancellationToken(true)), Task.CompletedTask }, new CancellationToken(false)));
            Functions.AssertThrowsWrapped<TaskCanceledException>(() => Task.WaitAll(new[] { Task.FromCanceled(new CancellationToken(true)) }, 1, new CancellationToken(false)));
            Functions.AssertThrowsWrapped<TaskCanceledException>(() => Task.WaitAll(new[] { Task.FromCanceled(new CancellationToken(true)), Task.CompletedTask }, 1, new CancellationToken(false)));
        }

        [Fact]
        public static void WaitAll_FaultedTask()
        {
            Functions.AssertThrowsWrapped<DeliberateTestException>(() => Task.WaitAll(new[] { Task.FromException(new DeliberateTestException()) }));
            Functions.AssertThrowsWrapped<DeliberateTestException>(() => Task.WaitAll(new[] { Task.FromException(new DeliberateTestException()), Task.CompletedTask }));
            Functions.AssertThrowsWrapped<DeliberateTestException>(() => Task.WaitAll(new[] { Task.FromException(new DeliberateTestException()) }, 1));
            Functions.AssertThrowsWrapped<DeliberateTestException>(() => Task.WaitAll(new[] { Task.FromException(new DeliberateTestException()), Task.CompletedTask }, 1));
            Functions.AssertThrowsWrapped<DeliberateTestException>(() => Task.WaitAll(new[] { Task.FromException(new DeliberateTestException()) }, new CancellationToken(false)));
            Functions.AssertThrowsWrapped<DeliberateTestException>(() => Task.WaitAll(new[] { Task.FromException(new DeliberateTestException()), Task.CompletedTask }, new CancellationToken(false)));
            Functions.AssertThrowsWrapped<DeliberateTestException>(() => Task.WaitAll(new[] { Task.FromException(new DeliberateTestException()) }, 1, new CancellationToken(false)));
            Functions.AssertThrowsWrapped<DeliberateTestException>(() => Task.WaitAll(new[] { Task.FromException(new DeliberateTestException()), Task.CompletedTask }, 1, new CancellationToken(false)));
        }

        private static void WaitAll(TimeSpan[] loads, TimeSpan wait, Func<Task[], TimeSpan, bool> call)
        {
            TimeSpan minimum = loads.Any() ? loads.Max() : TimeSpan.Zero;
            TimeSpan expected = loads.Aggregate(TimeSpan.Zero, (acc, load) => acc + load);

            Stopwatch timer = null;
            bool completed = false;
            Task[] tasks = null;
            // tracker for times a particular task is entered
            int[] called = new int[loads.Length];

            using (Barrier b = new Barrier(loads.Length + 1))
            {
                tasks = CreateAndStartTasks(loads, b, called);
                b.SignalAndWait(MaxSafeTimeout);
                timer = Stopwatch.StartNew();

                completed = call(tasks, wait);
                timer.Stop();
            }

            if (wait == Waits.Infinite || completed)
            {
                Assert.True(completed);
                Assert.All(tasks, task => AssertTaskComplete(task));
                Assert.All(called, run => Assert.Equal(1, run));
                ExpectAndReport(timer.Elapsed, minimum, expected);
            }
            else
            {
                // Given how scheduling and threading in general works,
                // the only guarantee is that WaitAll returned false.
                //   Any of the following may be true:
                //     - Between WaitAll timing out and Asserts, all tasks may start AND complete.
                //     - Tasks may complete after WaitAll times out internally, but before it returns.
                Assert.False(completed);
                Assert.All(called, run => Assert.True(run == 0 || run == 1));
            }
        }

        private static void WaitAll_CancelTask(TimeSpan[] loads, TimeSpan wait, Action<Task[], TimeSpan> call)
        {
            using (Barrier b = new Barrier(loads.Length + 1))
            {
                CancellationTokenSource source = new CancellationTokenSource();
                int canceledIndex = loads.Length / 2;

                Task[] tasks = loads.Select((load, index) => new TaskFactory().StartNew(() =>
                {
                    b.SignalAndWait();
                    if (index == canceledIndex)
                    {
                        source.Cancel();
                        source.Token.ThrowIfCancellationRequested();
                    }
                    // Spin until the cancellation is requested.
                    SpinWait.SpinUntil(() => source.IsCancellationRequested);
                }, source.Token)).ToArray();
                b.SignalAndWait();

                Stopwatch timer = Stopwatch.StartNew();
                if (tasks.Any())
                {
                    // Exception isn't thrown until all tasks complete.
                    Functions.AssertThrowsWrapped<TaskCanceledException>(() => call(tasks, wait));

                    Task canceledTask = tasks[canceledIndex];
                    Assert.True(canceledTask.IsCompleted);
                    Assert.True(canceledTask.IsCanceled);
                    Assert.False(canceledTask.IsFaulted);
                    Assert.Equal(TaskStatus.Canceled, canceledTask.Status);
                    timer.Stop();
                }
                else
                {
                    // If no tasks, the call completes immediately, and successfully.
                    call(tasks, wait);
                    timer.Stop();
                }

                ExpectAndReport(timer.Elapsed, TimeSpan.FromMilliseconds(3), wait);
            }
        }

        private static void WaitAll_FaultTask(TimeSpan[] loads, TimeSpan wait, Action<Task[], TimeSpan> call)
        {
            using (Barrier b = new Barrier(loads.Length + 1))
            {
                Flag flag = new Flag();
                int faultedIndex = loads.Length / 2;

                Task[] tasks = loads.Select((load, index) => new TaskFactory().StartNew(() =>
                {
                    b.SignalAndWait();
                    if (index == faultedIndex)
                    {
                        flag.Trip();
                        throw new DeliberateTestException();
                    }
                    SpinWait.SpinUntil(() => flag.IsTripped);
                })).ToArray();
                b.SignalAndWait();

                Stopwatch timer = Stopwatch.StartNew();
                if (tasks.Any())
                {
                    // Exception isn't thrown until all tasks complete.
                    Functions.AssertThrowsWrapped<DeliberateTestException>(() => call(tasks, wait));

                    Task faultedTask = tasks[faultedIndex];
                    Assert.True(faultedTask.IsCompleted);
                    Assert.False(faultedTask.IsCanceled);
                    Assert.True(faultedTask.IsFaulted);
                    Assert.Equal(TaskStatus.Faulted, faultedTask.Status);
                    timer.Stop();
                }
                else
                {
                    // If no tasks, the call completes immediately, and successfully.
                    call(tasks, wait);
                    timer.Stop();
                }

                ExpectAndReport(timer.Elapsed, TimeSpan.FromMilliseconds(3), wait);
            }
        }

        [Theory]
        [MemberData("Task_Data")]
        public static void Task_WaitAny(TimeSpan[] loads)
        {
            WaitAny(loads, Waits.Infinite, (tasks, i) => Task.WaitAny(tasks));
        }

        [Theory]
        [OuterLoop]
        [MemberData("Task_Data_Longrunning")]
        public static void Task_WaitAny_Longrunning(TimeSpan[] loads)
        {
            Task_WaitAny(loads);
        }

        [Theory]
        [MemberData("Task_Data")]
        public static void Task_WaitAny_CancelTask(TimeSpan[] loads)
        {
            WaitAny_CancelTask(loads, Waits.Infinite, (tasks, i) => Task.WaitAny(tasks));
        }

        [Theory]
        [MemberData("Task_Data")]
        public static void Task_WaitAny_FaultTask(TimeSpan[] loads)
        {
            WaitAny_FaultTask(loads, Waits.Infinite, (tasks, w) => Task.WaitAny(tasks));
        }

        [Theory]
        [OuterLoop]
        [MemberData("Task_Specific_Data")]
        public static void Task_WaitAny_SpecificTask(int taskCount, int completes)
        {
            WaitAny_SpecificTask(taskCount, completes, Waits.Infinite, (tasks, w) => Task.WaitAny(tasks));
        }

        [Theory]
        [MemberData("Task_Data")]
        public static void Task_WaitAny_Token(TimeSpan[] loads)
        {
            WaitAny(loads, Waits.Infinite, (tasks, i) => Task.WaitAny(tasks, new CancellationToken()));
        }

        [Theory]
        [OuterLoop]
        [MemberData("Task_Data_Longrunning")]
        public static void Task_WaitAny_Token_Longrunning(TimeSpan[] loads)
        {
            Task_WaitAny_Token(loads);
        }

        [Theory]
        [MemberData("Task_Data")]
        public static void Task_WaitAny_Token_Cancel(TimeSpan[] loads)
        {
            Cancel(loads, Waits.Infinite, (tasks, i, token) => Task.WaitAny(tasks, token));
        }

        [Theory]
        [MemberData("Task_Data")]
        public static void Task_WaitAny_Token_CancelTask(TimeSpan[] loads)
        {
            WaitAny_CancelTask(loads, Waits.Infinite, (tasks, i) => Task.WaitAny(tasks, new CancellationTokenSource().Token));
        }

        [Theory]
        [MemberData("Task_Data")]
        public static void Task_WaitAny_Token_FaultTask(TimeSpan[] loads)
        {
            WaitAny_FaultTask(loads, Waits.Infinite, (tasks, w) => Task.WaitAny(tasks, new CancellationTokenSource().Token));
        }

        [Theory]
        [OuterLoop]
        [MemberData("Task_Specific_Data")]
        public static void Task_WaitAny_Token_SpecificTask(int taskCount, int completes)
        {
            WaitAny_SpecificTask(taskCount, completes, Waits.Infinite, (tasks, w) => Task.WaitAny(tasks, new CancellationTokenSource().Token));
        }

        [Theory]
        [MemberData("Task_Wait_Data")]
        public static void Task_WaitAny_TimeSpan(TimeSpan[] loads, TimeSpan wait)
        {
            WaitAny(loads, wait, (tasks, w) => Task.WaitAny(tasks, w));
        }

        [Theory]
        [OuterLoop]
        [MemberData("Task_Wait_Data_Longrunning")]
        public static void Task_WaitAny_TimeSpan_Longrunning(TimeSpan[] loads, TimeSpan wait)
        {
            Task_WaitAny_TimeSpan(loads, wait);
        }

        [Theory]
        [MemberData("Task_Wait_Cancel_Data")]
        public static void Task_WaitAny_TimeSpan_CancelTask(TimeSpan[] loads, TimeSpan wait)
        {
            WaitAny_CancelTask(loads, wait, (tasks, w) => Task.WaitAny(tasks, w));
        }

        [Theory]
        [MemberData("Task_Wait_Cancel_Data")]
        public static void Task_WaitAny_TimeSpan_FaultTask(TimeSpan[] loads, TimeSpan wait)
        {
            WaitAny_FaultTask(loads, wait, (tasks, w) => Task.WaitAny(tasks, w));
        }

        [Theory]
        [OuterLoop]
        [MemberData("Task_Wait_Specific_Data")]
        public static void Task_WaitAny_TimeSpan_SpecificTask(int taskCount, int completes, TimeSpan wait)
        {
            WaitAny_SpecificTask(taskCount, completes, wait, (tasks, w) => Task.WaitAny(tasks, w));
        }

        [Theory]
        [MemberData("Task_Wait_Data")]
        public static void Task_WaitAny_Millisecond(TimeSpan[] loads, TimeSpan wait)
        {
            WaitAny(loads, wait, (tasks, w) => Task.WaitAny(tasks, (int)w.TotalMilliseconds));
        }

        [Theory]
        [OuterLoop]
        [MemberData("Task_Wait_Data_Longrunning")]
        public static void Task_WaitAny_Millisecond_Longrunning(TimeSpan[] loads, TimeSpan wait)
        {
            Task_WaitAny_Millisecond(loads, wait);
        }

        [Theory]
        [MemberData("Task_Wait_Cancel_Data")]
        public static void Task_WaitAny_Millisecond_CancelTask(TimeSpan[] loads, TimeSpan wait)
        {
            WaitAny_CancelTask(loads, wait, (tasks, w) => Task.WaitAny(tasks, (int)w.TotalMilliseconds));
        }

        [Theory]
        [MemberData("Task_Wait_Cancel_Data")]
        public static void Task_WaitAny_Millisecond_FaultTask(TimeSpan[] loads, TimeSpan wait)
        {
            WaitAny_FaultTask(loads, wait, (tasks, w) => Task.WaitAny(tasks, (int)w.TotalMilliseconds));
        }

        [Theory]
        [OuterLoop]
        [MemberData("Task_Wait_Specific_Data")]
        public static void Task_WaitAny_Millisecond_SpecificTask(int taskCount, int completes, TimeSpan wait)
        {
            WaitAny_SpecificTask(taskCount, completes, wait, (tasks, w) => Task.WaitAny(tasks, (int)w.TotalMilliseconds));
        }

        [Theory]
        [MemberData("Task_Wait_Data")]
        public static void Task_WaitAny_Millisecond_Token(TimeSpan[] loads, TimeSpan wait)
        {
            WaitAny(loads, wait, (tasks, w) => Task.WaitAny(tasks, (int)w.TotalMilliseconds, new CancellationToken()));
        }

        [Theory]
        [OuterLoop]
        [MemberData("Task_Wait_Data_Longrunning")]
        public static void Task_WaitAny_Millisecond_Token_Longrunning(TimeSpan[] loads, TimeSpan wait)
        {
            Task_WaitAny_Millisecond_Token(loads, wait);
        }

        [Theory]
        [MemberData("Task_Wait_Cancel_Data")]
        public static void Task_WaitAny_Millisecond_Token_Cancel(TimeSpan[] loads, TimeSpan wait)
        {
            Cancel(loads, wait, (tasks, w, token) => Task.WaitAny(tasks, (int)w.TotalMilliseconds, token));
        }

        [Theory]
        [MemberData("Task_Wait_Cancel_Data")]
        public static void Task_WaitAny_Millisecond_Token_CancelTask(TimeSpan[] loads, TimeSpan wait)
        {
            WaitAny_CancelTask(loads, wait, (tasks, w) => Task.WaitAny(tasks, (int)w.TotalMilliseconds, new CancellationTokenSource().Token));
        }

        [Theory]
        [MemberData("Task_Wait_Cancel_Data")]
        public static void Task_WaitAny_Millisecond_Token_FaultTask(TimeSpan[] loads, TimeSpan wait)
        {
            WaitAny_FaultTask(loads, wait, (tasks, w) => Task.WaitAny(tasks, (int)w.TotalMilliseconds, new CancellationTokenSource().Token));
        }

        [Theory]
        [OuterLoop]
        [MemberData("Task_Wait_Specific_Data")]
        public static void Task_WaitAny_Millisecond_Token_SpecificTask(int taskCount, int completes, TimeSpan wait)
        {
            WaitAny_SpecificTask(taskCount, completes, wait, (tasks, w) => Task.WaitAny(tasks, (int)w.TotalMilliseconds, new CancellationTokenSource().Token));
        }

        [Fact]
        public static void WaitAny_Argument_Exception()
        {
            Assert.Throws<ArgumentException>(() => Task.WaitAny(new Task[] { null }));
            Assert.Throws<ArgumentException>(() => Task.WaitAny(new Task[] { Task.CompletedTask, null }));

            Assert.Throws<ArgumentException>(() => Task.WaitAny(new Task[] { null }, 0));
            Assert.Throws<ArgumentException>(() => Task.WaitAny(new Task[] { Task.CompletedTask, null }, 0));

            Assert.Throws<ArgumentException>(() => Task.WaitAny(new Task[] { null }, TimeSpan.Zero));
            Assert.Throws<ArgumentException>(() => Task.WaitAny(new Task[] { Task.CompletedTask, null }, TimeSpan.Zero));

            Assert.Throws<ArgumentException>(() => Task.WaitAny(new Task[] { null }, new CancellationToken()));
            Assert.Throws<ArgumentException>(() => Task.WaitAny(new Task[] { Task.CompletedTask, null }, new CancellationToken()));

            Assert.Throws<ArgumentException>(() => Task.WaitAny(new Task[] { null }, 0, new CancellationToken()));
            Assert.Throws<ArgumentException>(() => Task.WaitAny(new Task[] { Task.CompletedTask, null }, 0, new CancellationToken()));
        }

        [Fact]
        public static void WaitAny_ArgumentOutOfRange_Exception()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Task.WaitAny(new Task[] { }, -2));
            Assert.Throws<ArgumentOutOfRangeException>(() => Task.WaitAny(new Task[] { Task.CompletedTask }, -2));

            TimeSpan invalid = TimeSpan.FromMilliseconds(-2);
            Assert.Throws<ArgumentOutOfRangeException>(() => Task.WaitAny(new Task[] { }, invalid));
            Assert.Throws<ArgumentOutOfRangeException>(() => Task.WaitAny(new Task[] { Task.CompletedTask }, invalid));

            Assert.Throws<ArgumentOutOfRangeException>(() => Task.WaitAny(new Task[] { }, -2, new CancellationToken()));
            Assert.Throws<ArgumentOutOfRangeException>(() => Task.WaitAny(new Task[] { Task.CompletedTask }, -2, new CancellationToken()));
        }

        [Fact]
        public static void WaitAny_PreCanceled_Token()
        {
            Assert.Throws<OperationCanceledException>(() => Task.WaitAny(new[] { new Task(() => { /* nothing */ }) }, new CancellationToken(true)));
            Assert.Throws<OperationCanceledException>(() => Task.WaitAny(new[] { new Task(() => { /* nothing */ }) }, 1, new CancellationToken(true)));
            Assert.Throws<OperationCanceledException>(() => Task.WaitAny(new[] { new Task(() => { /* nothing */ }) }, -1, new CancellationToken(true)));
            Assert.Throws<OperationCanceledException>(() => Task.WaitAny(new[] { new Task(() => { /* nothing */ }) }, int.MaxValue, new CancellationToken(true)));
            // Cancellation status is checked _before_ wait/completion
            Assert.Throws<OperationCanceledException>(() => Task.WaitAny(new[] { new Task(() => { /* nothing */ }) }, 0, new CancellationToken(true)));
            Assert.Throws<OperationCanceledException>(() => Task.WaitAny(new[] { Task.CompletedTask }, -1, new CancellationToken(true)));
        }

        [Fact]
        public static void WaitAny_CanceledTask()
        {
            Assert.Equal(0, Task.WaitAny(new[] { Task.FromCanceled(new CancellationToken(true)) }));
            Assert.Equal(0, Task.WaitAny(new[] { Task.CompletedTask, Task.FromCanceled(new CancellationToken(true)) }));
            Assert.Equal(0, Task.WaitAny(new[] { Task.FromCanceled(new CancellationToken(true)), Task.CompletedTask }));
            Assert.Equal(0, Task.WaitAny(new[] { Task.FromCanceled(new CancellationToken(true)) }, 1));
            Assert.Equal(0, Task.WaitAny(new[] { Task.CompletedTask, Task.FromCanceled(new CancellationToken(true)) }, 1));
            Assert.Equal(0, Task.WaitAny(new[] { Task.FromCanceled(new CancellationToken(true)), Task.CompletedTask }, 1));
            Assert.Equal(0, Task.WaitAny(new[] { Task.FromCanceled(new CancellationToken(true)) }, new CancellationToken(false)));
            Assert.Equal(0, Task.WaitAny(new[] { Task.CompletedTask, Task.FromCanceled(new CancellationToken(true)) }, new CancellationToken(false)));
            Assert.Equal(0, Task.WaitAny(new[] { Task.FromCanceled(new CancellationToken(true)), Task.CompletedTask }, new CancellationToken(false)));
            Assert.Equal(0, Task.WaitAny(new[] { Task.FromCanceled(new CancellationToken(true)) }, 1, new CancellationToken(false)));
            Assert.Equal(0, Task.WaitAny(new[] { Task.CompletedTask, Task.FromCanceled(new CancellationToken(true)) }, 1, new CancellationToken(false)));
            Assert.Equal(0, Task.WaitAny(new[] { Task.FromCanceled(new CancellationToken(true)), Task.CompletedTask }, 1, new CancellationToken(false)));
        }

        [Fact]
        public static void WaitAny_FaultedTask()
        {
            Assert.Equal(0, Task.WaitAny(new[] { Task.FromException(new DeliberateTestException()) }));
            Assert.Equal(0, Task.WaitAny(new[] { Task.CompletedTask, Task.FromException(new DeliberateTestException()) }));
            Assert.Equal(0, Task.WaitAny(new[] { Task.FromException(new DeliberateTestException()), Task.CompletedTask }));
            Assert.Equal(0, Task.WaitAny(new[] { Task.FromException(new DeliberateTestException()) }, 1));
            Assert.Equal(0, Task.WaitAny(new[] { Task.CompletedTask, Task.FromException(new DeliberateTestException()) }, 1));
            Assert.Equal(0, Task.WaitAny(new[] { Task.FromException(new DeliberateTestException()), Task.CompletedTask }, 1));
            Assert.Equal(0, Task.WaitAny(new[] { Task.FromException(new DeliberateTestException()) }, new CancellationToken(false)));
            Assert.Equal(0, Task.WaitAny(new[] { Task.CompletedTask, Task.FromException(new DeliberateTestException()) }, new CancellationToken(false)));
            Assert.Equal(0, Task.WaitAny(new[] { Task.FromException(new DeliberateTestException()), Task.CompletedTask }, new CancellationToken(false)));
            Assert.Equal(0, Task.WaitAny(new[] { Task.FromException(new DeliberateTestException()) }, 1, new CancellationToken(false)));
            Assert.Equal(0, Task.WaitAny(new[] { Task.CompletedTask, Task.FromException(new DeliberateTestException()) }, 1, new CancellationToken(false)));
            Assert.Equal(0, Task.WaitAny(new[] { Task.FromException(new DeliberateTestException()), Task.CompletedTask }, 1, new CancellationToken(false)));
        }

        private static void WaitAny(TimeSpan[] loads, TimeSpan wait, Func<Task[], TimeSpan, int> call)
        {
            TimeSpan expected = loads.Any() ? loads.Min() : TimeSpan.Zero;
            TimeSpan maximum = loads.Any() ? loads.Max() : TimeSpan.Zero;

            Stopwatch timer = null;
            int completed = -1;
            Task[] tasks = null;
            // tracker for times a particular task is entered
            int[] called = new int[loads.Length];

            using (Barrier b = new Barrier(loads.Length + 1))
            {
                tasks = CreateAndStartTasks(loads, b, called);
                b.SignalAndWait(MaxSafeTimeout);
                timer = Stopwatch.StartNew();

                completed = call(tasks, wait);
                timer.Stop();
            }

            if (loads.Any() && (wait == Waits.Infinite || completed >= 0))
            {
                // A task was returned, but any of the following may be true:
                //     - The task returned may not be the first to 'finish'.
                //     - The task returned may not be the earliest in the list (of those that finished).
                Assert.InRange(completed, 0, loads.Length - 1);
                AssertTaskComplete(tasks[completed]);
                Assert.Equal(1, called[completed]);
                ExpectAndReport(timer.Elapsed, expected, maximum);
            }
            else
            {
                // Given how scheduling and threading in general works,
                // the only guarantee is that WaitAny returned "not found" (-1).
                //   Any of the following may be true:
                //     - Between WaitAny timing out and Asserts, any/all tasks may start AND complete.
                //     - Tasks may complete after WaitAny times out internally, but before it returns.
                Assert.Equal(-1, completed);
            }
            Assert.All(called, run => Assert.True(run == 0 || run == 1));
        }

        private static void Cancel(TimeSpan[] loads, TimeSpan wait, Action<Task[], TimeSpan, CancellationToken> call)
        {
            using (Barrier b = new Barrier(loads.Length + 1))
            {
                Flag flag = new Flag();
                Task[] tasks = loads.Select(load => new TaskFactory().StartNew(() => { b.SignalAndWait(); SpinWait.SpinUntil(() => flag.IsTripped); })).ToArray();
                b.SignalAndWait();
                Assert.All(tasks, task => Assert.Equal(TaskStatus.Running, task.Status));

                CancellationTokenSource source = new CancellationTokenSource();
                source.CancelAfter(TimeSpan.FromMilliseconds(3));

                Stopwatch timer = Stopwatch.StartNew();
                if (tasks.Any())
                {
                    Assert.Throws<OperationCanceledException>(() => call(tasks, wait, source.Token));
                }
                else
                {
                    // Empty set of tasks returns immediately
                    call(tasks, wait, source.Token);
                }
                timer.Stop();

                // Whether wait succeeded or not is immaterial to task completion
                Assert.All(tasks, task => Assert.Equal(TaskStatus.Running, task.Status));
                ExpectAndReport(timer.Elapsed, TimeSpan.FromMilliseconds(3), wait);
                flag.Trip();
            }
        }

        private static void WaitAny_CancelTask(TimeSpan[] loads, TimeSpan wait, Func<Task[], TimeSpan, int> call)
        {
            using (Barrier b = new Barrier(loads.Length + 1))
            {
                Flag flag = new Flag();
                CancellationTokenSource source = new CancellationTokenSource();
                int canceledIndex = loads.Length / 2;

                Task[] tasks = loads.Select((load, index) => new TaskFactory().StartNew(() =>
                {
                    b.SignalAndWait();
                    if (index == canceledIndex)
                    {
                        source.Cancel();
                        source.Token.ThrowIfCancellationRequested();
                    }

                    SpinWait.SpinUntil(() => flag.IsTripped);
                }, source.Token)).ToArray();
                b.SignalAndWait();

                Stopwatch timer = Stopwatch.StartNew();
                int completed = call(tasks, wait);
                timer.Stop();

                if (tasks.Any())
                {
                    Assert.Equal(canceledIndex, completed);

                    Task canceledTask = tasks[canceledIndex];
                    Assert.True(canceledTask.IsCompleted);
                    Assert.True(canceledTask.IsCanceled);
                    Assert.False(canceledTask.IsFaulted);
                    Assert.Equal(TaskStatus.Canceled, canceledTask.Status);
                    Functions.AssertThrowsWrapped<TaskCanceledException>(() => canceledTask.Wait());
                }
                else
                {
                    Assert.Equal(-1, completed);
                }

                ExpectAndReport(timer.Elapsed, TimeSpan.FromMilliseconds(3), wait);
                flag.Trip();
            }
        }

        private static void WaitAny_FaultTask(TimeSpan[] loads, TimeSpan wait, Func<Task[], TimeSpan, int> call)
        {
            using (Barrier b = new Barrier(loads.Length + 1))
            {
                Flag flag = new Flag();
                int faultedIndex = loads.Length / 2;

                Task[] tasks = loads.Select((load, index) => new TaskFactory().StartNew(() =>
                {
                    b.SignalAndWait();
                    if (index == faultedIndex)
                    {
                        throw new DeliberateTestException();
                    }

                    SpinWait.SpinUntil(() => flag.IsTripped);
                })).ToArray();
                b.SignalAndWait();

                Stopwatch timer = Stopwatch.StartNew();
                int completed = call(tasks, wait);
                timer.Stop();

                if (tasks.Any())
                {
                    Assert.Equal(faultedIndex, completed);

                    Task faultedTask = tasks[faultedIndex];
                    Assert.True(faultedTask.IsCompleted);
                    Assert.False(faultedTask.IsCanceled);
                    Assert.True(faultedTask.IsFaulted);
                    Assert.Equal(TaskStatus.Faulted, faultedTask.Status);
                    Functions.AssertThrowsWrapped<DeliberateTestException>(() => faultedTask.Wait());
                }
                else
                {
                    Assert.Equal(-1, completed);
                }

                ExpectAndReport(timer.Elapsed, TimeSpan.FromMilliseconds(3), wait);
                flag.Trip();
            }
        }

        private static void WaitAny_SpecificTask(int taskCount, int completedIndex, TimeSpan wait, Func<Task[], TimeSpan, int> call)
        {
            using (Barrier b = new Barrier(taskCount + 1))
            {
                Flag flag = new Flag();
                CancellationTokenSource source = new CancellationTokenSource();

                Task[] tasks = Enumerable.Range(0, taskCount).Select((load, index) => new TaskFactory().StartNew(() =>
                {
                    b.SignalAndWait();
                    if (index != completedIndex)
                    {
                        SpinWait.SpinUntil(() => flag.IsTripped);
                    }
                })).ToArray();
                b.SignalAndWait();

                Stopwatch timer = Stopwatch.StartNew();
                int completed = call(tasks, wait);
                timer.Stop();

                if (tasks.Any())
                {
                    Assert.Equal(completedIndex, completed);

                    Task faultedTask = tasks[completedIndex];
                    Assert.True(faultedTask.IsCompleted);
                    Assert.False(faultedTask.IsCanceled);
                    Assert.False(faultedTask.IsFaulted);
                    Assert.Equal(TaskStatus.RanToCompletion, faultedTask.Status);
                    Assert.All(tasks.Where((t, index) => index != completedIndex), task => Assert.Equal(TaskStatus.Running, task.Status));
                }
                else
                {
                    Assert.Equal(-1, completed);
                }

                ExpectAndReport(timer.Elapsed, TimeSpan.FromMilliseconds(3), wait);
                flag.Trip();
            }
        }

        private static void AssertTaskComplete(Task task)
        {
            Assert.Equal(TaskStatus.RanToCompletion, task.Status);
            Assert.True(task.IsCompleted);
            Assert.False(task.IsCanceled);
            Assert.False(task.IsFaulted);
            Assert.Null(task.Exception);
        }

        private static Task[] CreateAndStartTasks(TimeSpan[] loads, Barrier b, int[] called)
        {
            return loads.Select((load, index) => Task.Factory.StartNew(() => Work(load, b, ref called[index]))).ToArray();
        }

        private static void Work(TimeSpan duration, Barrier b, ref int called)
        {
            Assert.Equal(1, Interlocked.Increment(ref called));
            b.SignalAndWait(MaxSafeTimeout);
            Stopwatch timer = Stopwatch.StartNew();
            Assert.True(SpinWait.SpinUntil(() => timer.Elapsed >= duration, MaxSafeTimeout));
        }

        private static void ExpectAndReport(TimeSpan actual, TimeSpan minimum, TimeSpan maximum)
        {
            if (actual < minimum - DelayRange || actual > maximum + DelayRange)
            {
                Debug.WriteLine("Elapsed time outside of expected range: ({0} - {1}), Actual: {2}", minimum, maximum, actual);
            }
        }
    }
}
