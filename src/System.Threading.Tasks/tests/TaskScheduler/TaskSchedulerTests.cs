// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security;
using Xunit;

namespace System.Threading.Tasks.Tests
{
    //
    // Task scheduler basics.
    //
    public static class TaskSchedulerTests
    {
        // Just ensure we eventually complete when many blocked tasks are created.
        [OuterLoop]
        [Fact]
        public static void RunBlockedInjectionTest()
        {
            Debug.WriteLine("* RunBlockedInjectionTest() -- if it deadlocks, it failed");

            using (ManualResetEvent mre = new ManualResetEvent(false))
            {
                // This test needs to be run with a local task scheduler, because it needs to perform
                // the verification based on a known number of initially available threads.
                //
                //
                // @TODO: When we reach the _planB branch we need to add a trick here using ThreadPool.SetMaxThread
                //        to bring down the TP worker count. This is because previous activity in the test process might have
                //        injected workers.
                TaskScheduler tm = TaskScheduler.Default;

                // Create many tasks blocked on the MRE.

                int processorCount = Environment.ProcessorCount;
                Task[] tasks = new Task[processorCount];
                for (int i = 0; i < tasks.Length; i++)
                {
                    tasks[i] = Task.Factory.StartNew(() => mre.WaitOne(), CancellationToken.None, TaskCreationOptions.None, tm);
                }

                // TODO: Evaluate use of safety valve.
                // Create one task that signals the MRE, and wait for it.
                Assert.True(Task.Factory.StartNew(() => mre.Set(), CancellationToken.None, TaskCreationOptions.None, tm).Wait(TimeSpan.FromMinutes(10)));

                // Lastly, wait for the others to complete.
                Assert.True(Task.WaitAll(tasks, TimeSpan.FromMinutes(10)));
            }
        }

        [Fact]
        public static void RunLongRunningTaskTests()
        {
            TaskScheduler tm = TaskScheduler.Default;
            // This is computed such that this number of long-running tasks will result in a back-up
            // without some assistance from TaskScheduler.RunBlocking() or TaskCreationOptions.LongRunning.

            int ntasks = Environment.ProcessorCount * 2;

            Task[] tasks = new Task[ntasks];
            ManualResetEvent mre = new ManualResetEvent(false); // could just use a bool?
            CountdownEvent cde = new CountdownEvent(ntasks); // to count the number of Tasks that successfully start
            for (int i = 0; i < ntasks; i++)
            {
                tasks[i] = Task.Factory.StartNew(delegate
                {
                    cde.Signal(); // indicate that task has begun execution
                    Debug.WriteLine("Signalled");
                    while (!mre.WaitOne(0)) ;
                }, CancellationToken.None, TaskCreationOptions.LongRunning, tm);
            }
            bool waitSucceeded = cde.Wait(5000);
            foreach (Task task in tasks)
                Debug.WriteLine("Status: " + task.Status);
            int count = cde.CurrentCount;
            int initialCount = cde.InitialCount;
            if (!waitSucceeded)
            {
                Debug.WriteLine("Wait failed. CDE.CurrentCount: {0}, CDE.Initial Count: {1}", count, initialCount);
                Assert.True(false, string.Format("RunLongRunningTaskTests - TaskCreationOptions.LongRunning:    > FAILED.  Timed out waiting for tasks to start."));
            }

            mre.Set();
            Task.WaitAll(tasks);
        }

        [Fact]
        public static void RunHideSchedulerTests()
        {
            TaskScheduler[] schedules = new TaskScheduler[2];
            schedules[0] = TaskScheduler.Default;

            for (int i = 0; i < schedules.Length; i++)
            {
                bool useCustomTs = (i == 1);
                TaskScheduler outerTs = schedules[i]; // useCustomTs ? customTs : TaskScheduler.Default;
                // If we are running CoreCLR, then schedules[1] = null, and we should continue in this case.
                if (i == 1 && outerTs == null)
                    continue;

                for (int j = 0; j < 2; j++)
                {
                    bool hideScheduler = (j == 0);
                    TaskCreationOptions creationOptions = hideScheduler ? TaskCreationOptions.HideScheduler : TaskCreationOptions.None;
                    TaskContinuationOptions continuationOptions = hideScheduler ? TaskContinuationOptions.HideScheduler : TaskContinuationOptions.None;
                    TaskScheduler expectedInnerTs = hideScheduler ? TaskScheduler.Default : outerTs;

                    Action<string> commonAction = delegate (string setting)
                    {
                        Assert.Equal(TaskScheduler.Current, expectedInnerTs);

                        // And just for completeness, make sure that inner tasks are started on the correct scheduler
                        TaskScheduler tsInner1 = null, tsInner2 = null;

                        Task tInner = Task.Factory.StartNew(() =>
                        {
                            tsInner1 = TaskScheduler.Current;
                        });
                        Task continuation = tInner.ContinueWith(_ =>
                        {
                            tsInner2 = TaskScheduler.Current;
                        });

                        Task.WaitAll(tInner, continuation);

                        Assert.Equal(tsInner1, expectedInnerTs);
                        Assert.Equal(tsInner2, expectedInnerTs);
                    };

                    Task outerTask = Task.Factory.StartNew(() =>
                    {
                        commonAction("task");
                    }, CancellationToken.None, creationOptions, outerTs);
                    Task outerContinuation = outerTask.ContinueWith(_ =>
                    {
                        commonAction("continuation");
                    }, CancellationToken.None, continuationOptions, outerTs);

                    Task.WaitAll(outerTask, outerContinuation);

                    // Check that the option was internalized by the task/continuation
                    Assert.True(hideScheduler == ((outerTask.CreationOptions & TaskCreationOptions.HideScheduler) != 0), "RunHideSchedulerTests:  FAILED.  CreationOptions mismatch on outerTask");
                    Assert.True(hideScheduler == ((outerContinuation.CreationOptions & TaskCreationOptions.HideScheduler) != 0), "RunHideSchedulerTests:  FAILED.  CreationOptions mismatch on outerContinuation");
                } // end j-loop, for hideScheduler setting
            } // ending i-loop, for customTs setting
        }

        [Fact]
        public static void BuggyScheduler_Start_Test()
        {
            BuggyTaskScheduler bts = new BuggyTaskScheduler();
            Task task = new Task(() => { /* do nothing */ });

            Assert.Throws<TaskSchedulerException>(() => task.Start(bts));
            Assert.Equal(TaskStatus.Faulted, task.Status);
            AggregateException ae = Assert.Throws<AggregateException>(() => task.Wait());
            Assert.IsType<TaskSchedulerException>(ae.InnerException);
        }

        [Fact]
        public static void BuggyScheduler_RunSynchronously_Test()
        {
            BuggyTaskScheduler bts = new BuggyTaskScheduler();
            Task task = new Task(() => { /* do nothing */ });

            Assert.Throws<TaskSchedulerException>(() => task.RunSynchronously(bts));
            Assert.Equal(TaskStatus.Faulted, task.Status);
            AggregateException ae = Assert.Throws<AggregateException>(() => task.Wait());
            Assert.IsType<TaskSchedulerException>(ae.InnerException);
        }

        [Fact]
        public async static void BuggyScheduler_StartNew_Test()
        {
            BuggyTaskScheduler bts = new BuggyTaskScheduler();

            await Assert.ThrowsAsync<TaskSchedulerException>(() => Task.Factory.StartNew(() => { /* do nothing */ },
                CancellationToken.None, TaskCreationOptions.None, bts));
        }

        [Fact]
        public static void BuggyScheduler_ContinueWith_Synchronous_Test()
        {
            BuggyTaskScheduler bts = new BuggyTaskScheduler();

            Task completedTask = Task.Factory.StartNew(() => { /* do nothing */ });
            completedTask.Wait();

            Task continuation = completedTask.ContinueWith(ignore => { /* do nothing */ }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, bts);

            AggregateException ae = Assert.Throws<AggregateException>(() => continuation.Wait());
            Assert.IsType<TaskSchedulerException>(ae.InnerException);
        }

        [Fact]
        public static void BuggyScheduler_ContinueWith_Test()
        {
            BuggyTaskScheduler bts = new BuggyTaskScheduler();

            Task completedTask = Task.Factory.StartNew(() => { /* do nothing */ });
            completedTask.Wait();

            Task continuation = completedTask.ContinueWith(ignore => { /* do nothing */ }, CancellationToken.None, TaskContinuationOptions.None, bts);

            AggregateException ae = Assert.Throws<AggregateException>(() => continuation.Wait());
            Assert.IsType<TaskSchedulerException>(ae.InnerException);
        }

        [Fact]
        public static void BuggyScheduler_Inlining_Test()
        {
            // won't throw on QueueTask
            BuggyTaskScheduler bts2 = new BuggyTaskScheduler(false);

            Task task = new Task(() => { /* do nothing */ });
            task.Start(bts2);

            Assert.Throws<TaskSchedulerException>(() => task.Wait());
        }

        [Fact]
        [OuterLoop]
        public static void SynchronizationContext_TaskScheduler_Wait_Test()
        {
            // Remember the current SynchronizationContext, so it can be restored
            SynchronizationContext previousSC = SynchronizationContext.Current;
            try
            {
                // Now make up a "real" SynchronizationContext and install it
                SimpleSynchronizationContext newSC = new SimpleSynchronizationContext();
                SynchronizationContext.SetSynchronizationContext(newSC);

                // Create a scheduler based on the current SC
                TaskScheduler scTS = TaskScheduler.FromCurrentSynchronizationContext();

                //
                // Launch a Task on scTS, make sure that it is processed in the expected fashion
                //
                bool sideEffect = false;
                Task task = Task.Factory.StartNew(() => { sideEffect = true; }, CancellationToken.None, TaskCreationOptions.None, scTS);

                task.Wait();

                Assert.True(task.IsCompleted, "Expected task to have completed");
                Assert.True(sideEffect, "Task appears not to have run");
                Assert.Equal(1, newSC.PostCount);

                Assert.Equal(1, scTS.MaximumConcurrencyLevel);
            }
            finally
            {
                // restore original SC
                SynchronizationContext.SetSynchronizationContext(previousSC);
            }
        }

        [Fact]
        [OuterLoop]
        public static void SynchronizationContext_TaskScheduler_Synchronous_Test()
        {
            // Remember the current SynchronizationContext, so it can be restored
            SynchronizationContext previousSC = SynchronizationContext.Current;
            try
            {
                // Now make up a "real" SynchronizationContext and install it
                SimpleSynchronizationContext newSC = new SimpleSynchronizationContext();
                SynchronizationContext.SetSynchronizationContext(newSC);

                // Create a scheduler based on the current SC
                TaskScheduler scTS = TaskScheduler.FromCurrentSynchronizationContext();

                //
                // Run a Task synchronously on scTS, make sure that it completes
                //
                bool sideEffect = false;
                Task syncTask = new Task(() => { sideEffect = true; });

                syncTask.RunSynchronously(scTS);

                Assert.True(syncTask.IsCompleted, "Expected task to have completed");
                Assert.True(sideEffect, "Task appears not to have run");
                Assert.Equal(0, newSC.PostCount);

                //
                // Miscellaneous things to test
                //
                Assert.Equal(1, scTS.MaximumConcurrencyLevel);
            }
            finally
            {
                // restore original SC
                SynchronizationContext.SetSynchronizationContext(previousSC);
            }
        }

        [Fact]
        public static void RunSynchronizationContextTaskSchedulerTests_Negative()
        {
            // Remember the current SynchronizationContext, so it can be restored
            SynchronizationContext previousSC = SynchronizationContext.Current;
            try
            {
                //
                // Test exceptions on construction of SCTaskScheduler
                //
                SynchronizationContext.SetSynchronizationContext(null);
                Assert.Throws<InvalidOperationException>(
                   () => { TaskScheduler.FromCurrentSynchronizationContext(); });
            }
            finally
            {
                // restore original SC
                SynchronizationContext.SetSynchronizationContext(previousSC);
            }
        }

        [Fact]
        public static void GetTaskSchedulersForDebugger_ReturnsDefaultScheduler()
        {
            MethodInfo getTaskSchedulersForDebuggerMethod = typeof(TaskScheduler).GetTypeInfo().GetDeclaredMethod("GetTaskSchedulersForDebugger");
            TaskScheduler[] foundSchedulers = getTaskSchedulersForDebuggerMethod.Invoke(null, null) as TaskScheduler[];
            Assert.NotNull(foundSchedulers);
            Assert.Contains(TaskScheduler.Default, foundSchedulers);
        }

        [ConditionalFact(nameof(DebuggerIsAttached))]
        public static void GetTaskSchedulersForDebugger_DebuggerAttached_ReturnsAllSchedulers()
        {
            MethodInfo getTaskSchedulersForDebuggerMethod = typeof(TaskScheduler).GetTypeInfo().GetDeclaredMethod("GetTaskSchedulersForDebugger");

            var cesp = new ConcurrentExclusiveSchedulerPair();
            TaskScheduler[] foundSchedulers = getTaskSchedulersForDebuggerMethod.Invoke(null, null) as TaskScheduler[];
            Assert.NotNull(foundSchedulers);
            Assert.Contains(TaskScheduler.Default, foundSchedulers);
            Assert.Contains(cesp.ConcurrentScheduler, foundSchedulers);
            Assert.Contains(cesp.ExclusiveScheduler, foundSchedulers);

            GC.KeepAlive(cesp);
        }

        [ConditionalFact(nameof(DebuggerIsAttached))]
        public static void GetScheduledTasksForDebugger_DebuggerAttached_ReturnsTasksFromCustomSchedulers()
        {
            var nonExecutingScheduler = new BuggyTaskScheduler(faultQueues: false);

            Task[] queuedTasks =
                (from i in Enumerable.Range(0, 10)
                 select Task.Factory.StartNew(() => { }, CancellationToken.None, TaskCreationOptions.None, nonExecutingScheduler)).ToArray();

            MethodInfo getScheduledTasksForDebuggerMethod = typeof(TaskScheduler).GetTypeInfo().GetDeclaredMethod("GetScheduledTasksForDebugger");
            Task[] foundTasks = getScheduledTasksForDebuggerMethod.Invoke(nonExecutingScheduler, null) as Task[];
            Assert.Superset(new HashSet<Task>(queuedTasks), new HashSet<Task>(foundTasks));

            GC.KeepAlive(nonExecutingScheduler);
        }

        private static bool DebuggerIsAttached { get { return Debugger.IsAttached; } }

        #region Helper Methods / Helper Classes

        // Buggy task scheduler to make sure that QueueTask()/TryExecuteTaskInline()
        // exceptions are handled correctly.  Used in RunBuggySchedulerTests() below.
        [SecuritySafeCritical]
        private class BuggyTaskScheduler : TaskScheduler
        {
            private readonly ConcurrentQueue<Task> _tasks = new ConcurrentQueue<Task>();

            private bool _faultQueues;

            [SecurityCritical]
            protected override void QueueTask(Task task)
            {
                if (_faultQueues)
                    throw new InvalidOperationException("I don't queue tasks!");
                // else do nothing other than store the task -- still a pretty buggy scheduler!!
                _tasks.Enqueue(task);
            }

            [SecurityCritical]
            protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
            {
                throw new ArgumentException("I am your worst nightmare!");
            }

            [SecurityCritical]
            protected override IEnumerable<Task> GetScheduledTasks()
            {
                return _tasks;
            }

            public BuggyTaskScheduler()
                : this(true)
            {
            }

            public BuggyTaskScheduler(bool faultQueues)
            {
                _faultQueues = faultQueues;
            }
        }

        private class SimpleSynchronizationContext : SynchronizationContext
        {
            private int _postCount = 0;

            public override void Post(SendOrPostCallback d, object state)
            {
                Interlocked.Increment(ref _postCount);
                base.Post(d, state);
            }

            public int PostCount { get { return _postCount; } }
        }

        #endregion
    }
}
