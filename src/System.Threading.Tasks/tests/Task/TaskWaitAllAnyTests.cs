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

        [Fact]
        public static void RunTaskWaitAnyTests()
        {
            int numCores = Environment.ProcessorCount;

            // Basic tests w/ <64 tasks
            CoreWaitAnyTest(0, new bool[] { }, -1);
            CoreWaitAnyTest(0, new bool[] { true }, 0);
            CoreWaitAnyTest(0, new bool[] { true, false, false, false }, 0);

            if (numCores > 1)
                CoreWaitAnyTest(0, new bool[] { false, true, false, false }, 1);

            // Tests w/ >64 tasks, w/ winning index >= 64
            CoreWaitAnyTest(100, new bool[] { true }, 100);
            CoreWaitAnyTest(100, new bool[] { true, false, false, false }, 100);
            if (numCores > 1)
                CoreWaitAnyTest(100, new bool[] { false, true, false, false }, 101);

            // Test w/ >64 tasks, w/ winning index < 64
            CoreWaitAnyTest(62, new bool[] { true, false, false, false }, 62);

            // Test w/ >64 tasks, w/ winning index = WaitHandle.WaitTimeout
            CoreWaitAnyTest(WaitHandle.WaitTimeout, new bool[] { true, false, false, false }, WaitHandle.WaitTimeout);

            // Test that already-completed task is returned
            Task t1 = Task.Factory.StartNew(delegate { });
            t1.Wait();
            int tonsOfIterations = 100000;
            // these are cold tasks... should not have started or run at all.
            Task t2 = new Task(delegate { for (int i = 0; i < tonsOfIterations; i++) { } });
            Task t3 = new Task(delegate { for (int i = 0; i < tonsOfIterations; i++) { } });
            Task t4 = new Task(delegate { for (int i = 0; i < tonsOfIterations; i++) { } });

            if (Task.WaitAny(t2, t1, t3, t4) != 1)
            {
                Assert.True(false, string.Format("RunTaskWaitAnyTests:    > FAILED pre-completed task test.  Wrong index returned."));
            }
        }

        public static void CoreWaitAnyTest(int fillerTasks, bool[] finishMeFirst, int nExpectedReturnCode)
        {
            // We need to do this test in a local TM with # or threads equal to or greater than
            // the number of tasks requested. Otherwise this test can undeservedly fail on dual proc machines

            Task[] tasks = new Task[fillerTasks + finishMeFirst.Length];

            // Create filler tasks
            for (int i = 0; i < fillerTasks; i++) tasks[i] = new Task(delegate { }); // don't start it -- that might make things complicated

            // Create a MRES to gate the finishers
            ManualResetEvent mres = new ManualResetEvent(false);

            // Create worker tasks
            for (int i = 0; i < finishMeFirst.Length; i++)
            {
                tasks[fillerTasks + i] = Task.Factory.StartNew(delegate (object obj)
                {
                    bool finishMe = (bool)obj;
                    if (!finishMe) mres.WaitOne();
                }, (object)finishMeFirst[i]);
            }

            int staRetCode = 0;
            int retCode = Task.WaitAny(tasks);

            Task t = new Task(delegate
            {
                staRetCode = Task.WaitAny(tasks);
            });
            t.Start();
            t.Wait();

            // Release the waiters.
            mres.Set();

            try
            {
                // get rid of the filler tasks by starting them and doing a WaitAll
                for (int i = 0; i < fillerTasks; i++) tasks[i].Start();
                Task.WaitAll(tasks);
            }
            catch (AggregateException)
            {
                // We expect some OCEs if we canceled some filler tasks.
                if (fillerTasks == 0) throw; // we shouldn't see an exception if we don't have filler tasks.
            }

            if (retCode != nExpectedReturnCode)
            {
                Debug.WriteLine("CoreWaitAnyTest:    Testing WaitAny with {0} tasks, expected winner = {1}",
                    fillerTasks + finishMeFirst.Length, nExpectedReturnCode);
                Assert.True(false, string.Format("CoreWaitAnyTest:   > error: WaitAny() return code not matching expected."));
            }

            if (staRetCode != nExpectedReturnCode)
            {
                Debug.WriteLine("CoreWaitAnyTest:    Testing WaitAny with {0} tasks, expected winner = {1}",
                    fillerTasks + finishMeFirst.Length, nExpectedReturnCode);
                Assert.True(false, string.Format("CoreWaitAnyTest:   > error: WaitAny() return code not matching expected for STA Thread."));
            }
        }

        // basic WaitAny validations with Cancellation token
        [Fact]
        public static void RunTaskWaitAnyTests_WithCancellationTokenTests()
        {
            //Test stuck tasks + a cancellation token
            var mre = new ManualResetEvent(false);
            var tokenSrc = new CancellationTokenSource();
            var task1 = Task.Factory.StartNew(() => mre.WaitOne());
            var task2 = Task.Factory.StartNew(() => mre.WaitOne());
            var waiterTask = Task.Factory.StartNew(() => Task.WaitAny(new Task[] { task1, task2 }, tokenSrc.Token));
            tokenSrc.Cancel();
            Assert.Throws<AggregateException>(() => waiterTask.Wait());
            mre.Set();

            Action<int, bool, bool> testWaitAnyWithCT = delegate (int nTasks, bool useSTA, bool preCancel)
            {
                Task[] tasks = new Task[nTasks];

                CancellationTokenSource ctsForTaskCancellation = new CancellationTokenSource();
                for (int i = 0; i < nTasks; i++) { tasks[i] = new Task(delegate { }, ctsForTaskCancellation.Token); }

                CancellationTokenSource ctsForWaitAny = new CancellationTokenSource();
                if (preCancel)
                    ctsForWaitAny.Cancel();
                CancellationToken ctForWaitAny = ctsForWaitAny.Token;
                Task cancelThread = null;
                Task thread = new Task(delegate
                {
                    try
                    {
                        Task.WaitAny(tasks, ctForWaitAny);
                        Debug.WriteLine("WaitAnyWithCancellationTokenTests:    --Testing {0} pending tasks, STA={1}, preCancel={2}", nTasks, useSTA, preCancel);
                        Assert.True(false, string.Format("WaitAnyWithCancellationTokenTests:    > error: WaitAny() w/ {0} tasks should have thrown OCE, threw no exception.", nTasks));
                    }
                    catch (OperationCanceledException) { }
                    catch
                    {
                        Debug.WriteLine("WaitAnyWithCancellationTokenTests:    --Testing {0} pending tasks, STA={1}, preCancel={2}", nTasks, useSTA, preCancel);
                        Assert.True(false, string.Format("    > error: WaitAny() w/ {0} tasks should have thrown OCE, threw different exception.", nTasks));
                    }
                });

                if (!preCancel)
                {
                    cancelThread = new Task(delegate
                    {
                        for (int i = 0; i < 200; i++) { }
                        ctsForWaitAny.Cancel();
                    });
                    cancelThread.Start();
                }
                thread.Start();
                //thread.Join();
                Task.WaitAll(thread);

                //if (!preCancel) cancelThread.Join();

                try
                {
                    for (int i = 0; i < nTasks; i++) tasks[i].Start(); // get rid of all tasks we created
                    Task.WaitAll(tasks);
                }
                catch
                {
                } // ignore any exceptions
            };

            // Test some small number of tasks
            testWaitAnyWithCT(2, false, true);
            testWaitAnyWithCT(2, false, false);
            testWaitAnyWithCT(2, true, true);
            testWaitAnyWithCT(2, true, false);

            // Now test for 63 tasks (max w/o overflowing w/ CT)
            testWaitAnyWithCT(63, false, true);
            testWaitAnyWithCT(63, false, false);
            testWaitAnyWithCT(63, true, true);
            testWaitAnyWithCT(63, true, false);

            // Now test for 100 tasks (overflows WaitAny())
            testWaitAnyWithCT(100, false, true);
            testWaitAnyWithCT(100, false, false);
            testWaitAnyWithCT(100, true, true);
            testWaitAnyWithCT(100, true, false);
        }

        // creates a large number of tasks and does WaitAll on them from a thread of the specified apartment state
        [Fact]
        [OuterLoop]
        public static void RunTaskWaitAllTests()
        {
            Assert.Throws<ArgumentNullException>(() => Task.WaitAll((Task[])null));
            Assert.Throws<ArgumentException>(() => Task.WaitAll(new Task[] { null }));
            Assert.Throws<ArgumentOutOfRangeException>(() => Task.WaitAll(new Task[] { Task.Factory.StartNew(() => { }) }, -2));
            Assert.Throws<ArgumentOutOfRangeException>(() => Task.WaitAll(new Task[] { Task.Factory.StartNew(() => { }) }, TimeSpan.FromMilliseconds(-2)));

            RunTaskWaitAllTest(false, 1);
            RunTaskWaitAllTest(false, 10);
        }

        public static void RunTaskWaitAllTest(bool staThread, int nTaskCount)
        {
            string methodInput = string.Format("RunTaskWaitAllTest:  > WaitAll() Tests for aptState={0}, task count={1}", staThread ? "MTA" : "STA", nTaskCount);
            string excpMsg = "foo";

            int middleCeiling = (int)(nTaskCount / 2);
            if ((nTaskCount % 2) == 1)
                middleCeiling = middleCeiling + 1;
            int nFirstHalfCount = middleCeiling;
            int nSecondHalfCount = nTaskCount - nFirstHalfCount;

            //CancellationTokenSource ctsForSleepAndAckCancelAction = null; // this needs to be allocated every time sleepAndAckCancelAction is about to be used
            Action<object> emptyAction = delegate (Object o) { };
            Action<object> sleepAction = delegate (Object o) { for (int i = 0; i < 200; i++) { } };
            Action<object> longAction = delegate (Object o) { for (int i = 0; i < 400; i++) { } };

            Action<object> sleepAndAckCancelAction = delegate (Object o)
            {
                CancellationToken ct = (CancellationToken)o;
                while (!ct.IsCancellationRequested)
                { }
                throw new OperationCanceledException(ct);   // acknowledge
            };
            Action<object> exceptionThrowAction = delegate (Object o) { throw new Exception(excpMsg); };

            Exception e = null;

            // test case 1: trying: WaitAll() on a group of already completed tasks
            DoRunTaskWaitAllTest(staThread, nTaskCount, emptyAction, true, false, 0, null, 5000, ref e);

            if (e != null)
            {
                Assert.True(false, string.Format(methodInput + ":  RunTaskWaitAllTest:  > error: WaitAll() threw exception unexpectedly."));
            }

            // test case 2: WaitAll() on a a group of tasks half of which is already completed, half of which is blocked when we start the wait
            //Debug.WriteLine("  > trying: WaitAll() on a a group of tasks half of which is already ");
            //Debug.WriteLine("  >         completed, half of which is blocked when we start the wait");
            DoRunTaskWaitAllTest(staThread, nFirstHalfCount, emptyAction, true, false, nSecondHalfCount, sleepAction, 5000, ref e);

            if (e != null)
            {
                Assert.True(false, string.Format(methodInput + " : RunTaskWaitAllTest:  > error: WaitAll() threw exception unexpectedly."));
            }

            // test case 3: WaitAll() on a a group of tasks half of which is Canceled, half of which is blocked when we start the wait
            //Debug.WriteLine("  > trying: WaitAll() on a a group of tasks half of which is Canceled,");
            //Debug.WriteLine("  >         half of which is blocked when we start the wait");
            DoRunTaskWaitAllTest(staThread, nFirstHalfCount, sleepAndAckCancelAction, false, true, nSecondHalfCount, emptyAction, 5000, ref e);

            if (!(e is AggregateException) || !((e as AggregateException).InnerExceptions[0] is TaskCanceledException))
            {
                Assert.True(false, string.Format(methodInput + " : RunTaskWaitAllTest:  > error: WaitAll() didn't throw TaskCanceledException while waiting on a group of already canceled tasks.> {0}", e));
            }

            // test case 4: WaitAll() on a a group of tasks some of which throws an exception
            //Debug.WriteLine("  > trying: WaitAll() on a a group of tasks some of which throws an exception");
            DoRunTaskWaitAllTest(staThread, nFirstHalfCount, exceptionThrowAction, false, false, nSecondHalfCount, sleepAction, 5000, ref e);

            if (!(e is AggregateException) || ((e as AggregateException).InnerExceptions[0].Message != excpMsg))
            {
                Assert.True(false, string.Format(methodInput + "RunTaskWaitAllTest:  > error: WaitAll() didn't throw AggregateException while waiting on a group tasks that throw. > {0}", e));
            }

            //////////////////////////////////////////////////////
            //
            // WaitAll with CancellationToken tests
            //

            // test case 5: WaitAll() on a group of already completed tasks with an unsignaled token
            // this should complete cleanly with no exception
            DoRunTaskWaitAllTestWithCancellationToken(staThread, nTaskCount, true, false, 5000, -1, ref e);

            if (e != null)
            {
                Assert.True(false, string.Format(methodInput + ": RunTaskWaitAllTest:  > error: WaitAll() threw exception unexpectedly."));
            }

            // test case 6: WaitAll() on a group of already completed tasks with an already signaled token
            // this should throw OCE
            DoRunTaskWaitAllTestWithCancellationToken(staThread, nTaskCount, true, false, 5000, 0, ref e);

            if (!(e is OperationCanceledException))
            {
                Assert.True(false, string.Format(methodInput + "RunTaskWaitAllTest:  > error: WaitAll() should have thrown OperationCanceledException."));
            }

            // test case 7: WaitAll() on a group of long tasks with a token that gets canceled after a delay
            // this should throw OCE
            DoRunTaskWaitAllTestWithCancellationToken(staThread, nTaskCount, false, false, 5000, 25, ref e);

            if (!(e is OperationCanceledException))
            {
                Assert.True(false, string.Format(methodInput + "RunTaskWaitAllTest:  > error: WaitAll() should have thrown OperationCanceledException."));
            }
        }

        //
        // the core function for WaitAll tests. Takes 2 types of actions to create tasks, how many copies of each task type
        // to create, whether to wait for the completion of the first group, etc
        //
        public static void DoRunTaskWaitAllTest(bool staThread,
                                                    int numTasksType1,
                                                    Action<object> taskAction1,
                                                    bool bWaitOnAct1,
                                                    bool bCancelAct1,
                                                    int numTasksType2,
                                                    Action<object> taskAction2,
                                                    int timeoutForWaitThread,
                                                    ref Exception refWaitAllException)
        {
            int numTasks = numTasksType1 + numTasksType2;
            Task[] tasks = new Task[numTasks];

            //
            // Test case 1: WaitAll() on a mix of already completed tasks and yet blocked tasks
            //
            for (int i = 0; i < numTasks; i++)
            {
                if (i < numTasksType1)
                {
                    CancellationTokenSource taskCTS = new CancellationTokenSource();

                    //Both setting the cancellationtoken to the new task, and passing it in as the state object so that the delegate can acknowledge using it
                    tasks[i] = Task.Factory.StartNew(taskAction1, (object)taskCTS.Token, taskCTS.Token);
                    if (bCancelAct1) taskCTS.Cancel();

                    try
                    {
                        if (bWaitOnAct1) tasks[i].Wait();
                    }
                    catch { }
                }
                else
                {
                    tasks[i] = Task.Factory.StartNew(taskAction2, null);
                }
            }

            refWaitAllException = null;
            Exception waitAllException = null;
            Task t1 = new Task(delegate ()
            {
                try
                {
                    Task.WaitAll(tasks);
                }
                catch (Exception e)
                {
                    waitAllException = e;
                }
            });

            t1.Start();
            t1.Wait();

            refWaitAllException = waitAllException;
        }

        //
        // the core function for WaitAll tests. Takes 2 types of actions to create tasks, how many copies of each task type
        // to create, whether to wait for the completion of the first group, etc
        //
        public static void DoRunTaskWaitAllTestWithCancellationToken(bool staThread,
                                                    int numTasks,
                                                    bool bWaitOnAct1,
                                                    bool bCancelAct1,
                                                    int timeoutForWaitThread,
                                                    int timeToSignalCancellationToken, // -1 never, 0 beforehand, >0 for a delay
                                                    ref Exception refWaitAllException)
        {
            Task[] tasks = new Task[numTasks];

            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken ct = cts.Token;
            if (timeToSignalCancellationToken == 0)
                cts.Cancel();

            ManualResetEvent mres = new ManualResetEvent(false);

            // If timeToSignalCancellationToken is 0, it means that we pre-signal the cancellation token
            // If timeToSignalCancellationToken is -1, it means that we will never signal the cancellation token
            // Either way, it is OK for the tasks to complete ASAP.
            if (timeToSignalCancellationToken <= 0) mres.Set();

            //
            // Test case 1: WaitAll() on a mix of already completed tasks and yet blocked tasks
            //
            for (int i = 0; i < numTasks; i++)
            {
                CancellationTokenSource taskCTS = new CancellationTokenSource();

                //Both setting the cancellationtoken to the new task, and passing it in as the state object so that the delegate can acknowledge using it
                tasks[i] = Task.Factory.StartNew((obj) => { mres.WaitOne(); }, (object)taskCTS.Token, taskCTS.Token);
                if (bWaitOnAct1) tasks[i].Wait();
                if (bCancelAct1) taskCTS.Cancel();
            }

            if (timeToSignalCancellationToken > 0)
            {
                Task cancelthread = new Task(delegate ()
                {
                    //for (int i = 0; i < timeToSignalCancellationToken; i++) { }
                    Task.Delay(timeToSignalCancellationToken);
                    cts.Cancel();
                });
                cancelthread.Start();
            }

            refWaitAllException = null;
            Exception waitAllException = null;
            Task t1 = new Task(delegate ()
            {
                try
                {
                    Task.WaitAll(tasks, ct);
                }
                catch (Exception e)
                {
                    waitAllException = e;
                }
            });

            t1.Start();
            t1.Wait();

            refWaitAllException = waitAllException;

            // If we delay-signalled the cancellation token, then it is OK to let the tasks complete now.
            if (timeToSignalCancellationToken > 0)
            {
                mres.Set();
                Task.WaitAll(tasks);
            }
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
