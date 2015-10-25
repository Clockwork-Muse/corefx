// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xunit;

namespace System.Threading.Tasks.Tests
{
    public static class TaskWaitTests
    {
        private static readonly TimeSpan DelayRange = TimeSpan.FromMilliseconds(3);

        /// <summary>
        /// Task duration workloads.
        /// </summary>
        /// Format is:
        ///  1. Workload
        ///  2. Option
        /// <returns>A row of test data.</returns>
        public static IEnumerable<object[]> Task_Data()
        {
            foreach (TaskCreationOptions option in new[] { TaskCreationOptions.None, TaskCreationOptions.LongRunning })
            {
                yield return new object[] { Workloads.VeryLight, option };
                yield return new object[] { Workloads.Light, option };
                yield return new object[] { Workloads.Medium, option };
                yield return new object[] { Workloads.Heavy, option };
                yield return new object[] { Workloads.VeryHeavy, option };
            }
        }

        /// <summary>
        /// Task duration workloads and wait times
        /// </summary>
        /// The format is:
        ///  1. A list of task workloads
        ///  2. The maximum duration to wait (-1 means no timeout)
        ///  3. Options
        /// <returns>A row of test data.</returns>
        public static IEnumerable<object[]> Task_Wait_Data()
        {
            foreach (object[] data in Task_Data())
            {
                foreach (TimeSpan wait in new[] { Waits.Infinite, Waits.Long, Waits.Short, Waits.Instant })
                {
                    yield return new object[] { data[0], wait, data[1] };
                }
            }
        }

        /// <summary>
        /// Task duration wait times.
        /// </summary>
        /// Format is:
        ///  1. wait time
        ///  2. Option
        /// <returns>A row of test data.</returns>
        public static IEnumerable<object[]> Task_Wait_Cancel_Data()
        {
            foreach (TaskCreationOptions option in new[] { TaskCreationOptions.None, TaskCreationOptions.LongRunning })
            {
                yield return new object[] { TimeSpan.FromMinutes(1), option };
                yield return new object[] { Waits.Infinite, option };
            }
        }

        [Theory]
        [MemberData("Task_Data")]
        public static void Task_Wait(TimeSpan load, TaskCreationOptions option)
        {
            Wait(load, Waits.Infinite, option, (task, i) => { task.Wait(); return true; });
        }

        [Theory]
        [MemberData("Task_Data")]
        public static void Task_Wait_Nested(TimeSpan load, TaskCreationOptions option)
        {
            Wait_Nested(load, Waits.Infinite, option, (task, i) => { task.Wait(); return true; });
        }

        [Theory]
        [MemberData("Task_Data")]
        public static void Task_Wait_Child(TimeSpan load, TaskCreationOptions option)
        {
            Wait_Child(load, Waits.Infinite, option, (task, i) => { task.Wait(); return true; });
        }

        [Theory]
        [MemberData("Task_Data")]
        public static void Task_Wait_Token(TimeSpan load, TaskCreationOptions option)
        {
            Wait(load, Waits.Infinite, option, (task, i) => { task.Wait(new CancellationTokenSource().Token); return true; });
        }

        [Theory]
        [MemberData("Task_Data")]
        public static void Task_Wait_Nested_Token(TimeSpan load, TaskCreationOptions option)
        {
            Wait_Nested(load, Waits.Infinite, option, (task, i) => { task.Wait(new CancellationTokenSource().Token); return true; });
        }

        [Theory]
        [MemberData("Task_Data")]
        public static void Task_Wait_Child_Token(TimeSpan load, TaskCreationOptions option)
        {
            Wait_Child(load, Waits.Infinite, option, (task, i) => { task.Wait(new CancellationTokenSource().Token); return true; });
        }

        [Theory]
        [InlineData(TaskCreationOptions.None)]
        [InlineData(TaskCreationOptions.LongRunning)]
        public static void Task_Wait_Token_Cancel(TaskCreationOptions option)
        {
            Cancel(Waits.Infinite, option, (task, i, token) => task.Wait(token));
        }

        [Theory]
        [MemberData("Task_Wait_Data")]
        public static void Task_Wait_TimeSpan(TimeSpan load, TimeSpan wait, TaskCreationOptions option)
        {
            Wait(load, wait, option, (task, w) => task.Wait(w));
        }

        [Theory]
        [MemberData("Task_Wait_Data")]
        public static void Task_Wait_Nested_TimeSpan(TimeSpan load, TimeSpan wait, TaskCreationOptions option)
        {
            Wait_Nested(load, wait, option, (task, w) => task.Wait(w));
        }

        [Theory]
        [MemberData("Task_Wait_Data")]
        public static void Task_Wait_Child_TimeSpan(TimeSpan load, TimeSpan wait, TaskCreationOptions option)
        {
            Wait_Child(load, wait, option, (task, w) => task.Wait(w));
        }

        [Theory]
        [MemberData("Task_Wait_Data")]
        public static void Task_Wait_Millisecond(TimeSpan load, TimeSpan wait, TaskCreationOptions option)
        {
            Wait(load, wait, option, (task, w) => task.Wait((int)w.TotalMilliseconds));
        }

        [Theory]
        [MemberData("Task_Wait_Data")]
        public static void Task_Wait_Nested_Millisecond(TimeSpan load, TimeSpan wait, TaskCreationOptions option)
        {
            Wait_Nested(load, wait, option, (task, w) => task.Wait((int)w.TotalMilliseconds));
        }

        [Theory]
        [MemberData("Task_Wait_Data")]
        public static void Task_Wait_Child_Millisecond(TimeSpan load, TimeSpan wait, TaskCreationOptions option)
        {
            Wait_Child(load, wait, option, (task, w) => task.Wait((int)w.TotalMilliseconds));
        }

        [Theory]
        [MemberData("Task_Wait_Data")]
        public static void Task_Wait_Millisecond_Token(TimeSpan load, TimeSpan wait, TaskCreationOptions option)
        {
            Wait(load, wait, option, (task, w) => task.Wait((int)w.TotalMilliseconds, new CancellationTokenSource().Token));
        }

        [Theory]
        [MemberData("Task_Wait_Data")]
        public static void Task_Wait_Nested_Millisecond_Token(TimeSpan load, TimeSpan wait, TaskCreationOptions option)
        {
            Wait_Nested(load, wait, option, (task, w) => task.Wait((int)w.TotalMilliseconds, new CancellationTokenSource().Token));
        }

        [Theory]
        [MemberData("Task_Wait_Data")]
        public static void Task_Wait_Child_Millisecond_Token(TimeSpan load, TimeSpan wait, TaskCreationOptions option)
        {
            Wait_Child(load, wait, option, (task, w) => task.Wait((int)w.TotalMilliseconds, new CancellationTokenSource().Token));
        }

        [Theory]
        [MemberData("Task_Wait_Cancel_Data")]
        public static void Task_Wait_Millisecond_Token_Cancel(TimeSpan wait, TaskCreationOptions option)
        {
            Cancel(wait, option, (task, w, token) => task.Wait((int)w.TotalMilliseconds, token));
        }

        [Fact]
        public static void Argument_Exception()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Task(() => { }).Wait(-2));
            Assert.Throws<ArgumentOutOfRangeException>(() => Task.CompletedTask.Wait(-2));

            TimeSpan invalid = TimeSpan.FromMilliseconds(-2);
            Assert.Throws<ArgumentOutOfRangeException>(() => new Task(() => { }).Wait(invalid));
            Assert.Throws<ArgumentOutOfRangeException>(() => Task.CompletedTask.Wait(invalid));


            Assert.Throws<ArgumentOutOfRangeException>(() => new Task(() => { }).Wait(-2, new CancellationToken()));
            Assert.Throws<ArgumentOutOfRangeException>(() => Task.CompletedTask.Wait(-2, new CancellationToken()));
        }

        [Fact]
        public static void PreCanceled_Token()
        {
            Assert.Throws<OperationCanceledException>(() => new Task(() => { /* nothing */ }).Wait(new CancellationToken(true)));

            Assert.Throws<OperationCanceledException>(() => new Task(() => { /* nothing */ }).Wait(1, new CancellationToken(true)));
            Assert.Throws<OperationCanceledException>(() => new Task(() => { /* nothing */ }).Wait(-1, new CancellationToken(true)));
            Assert.Throws<OperationCanceledException>(() => new Task(() => { /* nothing */ }).Wait(int.MaxValue, new CancellationToken(true)));
            // wait is checked before cancellation status:
            Assert.False(new Task(() => { /* nothing */ }).Wait(0, new CancellationToken(true)));
            Assert.True(Task.CompletedTask.Wait(-1, new CancellationToken(true)));
        }

        [Fact]
        public static void Thrown_Exception()
        {
            Functions.AssertThrowsWrapped<DeliberateTestException>(() => Task.FromException(new DeliberateTestException()).Wait());
            Functions.AssertThrowsWrapped<DeliberateTestException>(() => Task.FromException(new DeliberateTestException()).Wait(0));
            Functions.AssertThrowsWrapped<DeliberateTestException>(() => Task.FromException(new DeliberateTestException()).Wait(TimeSpan.Zero));
            Functions.AssertThrowsWrapped<DeliberateTestException>(() => Task.FromException(new DeliberateTestException()).Wait(0, new CancellationToken()));
            Functions.AssertThrowsWrapped<DeliberateTestException>(() => Task.FromException(new DeliberateTestException()).Wait(new CancellationToken()));
            Functions.AssertThrowsWrapped<DeliberateTestException>(() => Task.FromException(new DeliberateTestException()).Wait(0, new CancellationToken(true)));
            Functions.AssertThrowsWrapped<DeliberateTestException>(() => Task.FromException(new DeliberateTestException()).Wait(new CancellationToken(true)));
        }

        private static void Wait(TimeSpan load, TimeSpan wait, TaskCreationOptions option, Func<Task, TimeSpan, bool> call)
        {
            Stopwatch timer = null;
            bool completed = false;
            Task task = null;
            // tracker for times a particular task is entered
            Flag flag = new Flag();

            task = new TaskFactory().StartNew(() => Functions.SpinAndDo(load, () => flag.Trip()), option);

            timer = Stopwatch.StartNew();
            completed = call(task, wait);
            timer.Stop();

            AssertCompleteOrTimedOut(completed, wait, flag, task);
            ExpectAndReport(timer.Elapsed, load, wait);
        }

        private static void Cancel(TimeSpan wait, TaskCreationOptions option, Action<Task, TimeSpan, CancellationToken> call)
        {
            using (ManualResetEventSlim startup = new ManualResetEventSlim())
            {
                Flag flag = new Flag();
                Task task = new TaskFactory().StartNew(() =>
                 {
                     startup.Set();
                     SpinWait.SpinUntil(() => flag.IsTripped);
                 }, option);
                startup.Wait();
                Assert.Equal(TaskStatus.Running, task.Status);

                CancellationTokenSource source = new CancellationTokenSource();
                source.CancelAfter(TimeSpan.FromMilliseconds(3));

                Stopwatch timer = Stopwatch.StartNew();
                Assert.Throws<OperationCanceledException>(() => call(task, wait, source.Token));
                timer.Stop();

                // Whether wait succeeded or not is immaterial to task completion
                Assert.Equal(TaskStatus.Running, task.Status);
                ExpectAndReport(timer.Elapsed, TimeSpan.FromMilliseconds(3), wait);
                flag.Trip();
            }
        }

        private static void Wait_Nested(TimeSpan load, TimeSpan wait, TaskCreationOptions option, Func<Task, TimeSpan, bool> call)
        {
            using (ManualResetEventSlim inner = new ManualResetEventSlim())
            {
                bool completed = false;
                Flag flag = new Flag();
                Flag started = new Flag();
                Task nested = null;

                Task outer = new TaskFactory().StartNew(() =>
                {
                    nested = new TaskFactory().StartNew(() => { started.Trip(); inner.Wait(); }, option);
                    Functions.SpinAndDo(load, () => flag.Trip());
                }, option);

                completed = call(outer, wait);

                // The difference between a nested and child task is that a nested task is independent of the parent;
                // The nested task should still be "running".
                SpinWait.SpinUntil(() => nested != null && started.IsTripped);
                Assert.Equal(TaskStatus.Running, nested.Status);
                // release inner task to finish.
                inner.Set();


                AssertCompleteOrTimedOut(completed, wait, flag, outer);
            }
        }

        private static void Wait_Child(TimeSpan load, TimeSpan wait, TaskCreationOptions option, Func<Task, TimeSpan, bool> call)
        {
            using (ManualResetEventSlim inner = new ManualResetEventSlim())
            {
                bool completed = false;
                Flag flag = new Flag();
                Flag started = new Flag();
                Task child = null;

                Task parent = new TaskFactory().StartNew(() =>
                {
                    child = new TaskFactory().StartNew(() =>
                    {
                        started.Trip();
                        inner.Wait();
                        SpinWait.SpinUntil(() => false, load);
                    }, option | TaskCreationOptions.AttachedToParent);
                    // ensure child task starts
                    SpinWait.SpinUntil(() => child != null && started.IsTripped);
                    flag.Trip();
                }, option);

                SpinWait.SpinUntil(() => flag.IsTripped && parent.Status != TaskStatus.Running);
                Assert.Equal(TaskStatus.WaitingForChildrenToComplete, parent.Status);

                // release inner task to finish.
                inner.Set();
                completed = call(parent, wait);

                AssertCompleteOrTimedOut(completed, wait, flag, parent);
            }
        }

        private static void AssertCompleteOrTimedOut(bool completed, TimeSpan wait, Flag ran, Task task)
        {
            if (wait == Waits.Infinite || completed)
            {
                Assert.True(completed);
                AssertTaskComplete(task);
                Assert.True(ran.IsTripped);
            }
            else
            {
                // Given how scheduling and threading in general works,
                // the only guarantee is that Wait returned false.
                //   Any of the following may be true:
                //     - Between Wait timing out and Asserts, the task may start AND complete.
                //     - Task may complete after Wait times out internally, but before it returns.
                Assert.False(completed);
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

        private static void ExpectAndReport(TimeSpan actual, TimeSpan load, TimeSpan wait)
        {
            if (actual < load - DelayRange || (wait != Waits.Infinite && actual > wait + DelayRange))
            {
                Debug.WriteLine("Elapsed time outside of expected range: ({0} - {1}), Actual: {2}", load, wait, actual);
            }
        }
    }
}
