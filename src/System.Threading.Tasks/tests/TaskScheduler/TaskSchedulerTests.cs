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
        private static readonly TimeSpan MaxSafeWait = TimeSpan.FromMinutes(1);

        // Just ensure we eventually complete when many blocked tasks are created.
        [OuterLoop]
        [Fact]
        public static void RunBlockedInjectionTest()
        {
            int processorCount = Environment.ProcessorCount;
            using (Barrier startingLine = new Barrier(processorCount + 1))
            {
                Flag flag = new Flag();

                // This test needs to be run with a local task scheduler, because it needs to perform
                // the verification based on a known number of initially available threads.
                //
                //
                // @TODO: When we reach the _planB branch we need to add a trick here using ThreadPool.SetMaxThread
                //        to bring down the TP worker count. This is because previous activity in the test process might have
                //        injected workers.
                TaskScheduler tm = TaskScheduler.Default;
                TaskFactory factory = new TaskFactory(CancellationToken.None, TaskCreationOptions.None, TaskContinuationOptions.None, tm);

                // Create many tasks blocked on the MRE.
                Task[] tasks = new Task[processorCount];
                for (int i = 0; i < tasks.Length; i++)
                {
                    tasks[i] = factory.StartNew(() =>
                    {
                        startingLine.SignalAndWait();
                        SpinWait.SpinUntil(() => flag.IsTripped);
                    });
                }

                Assert.True(startingLine.SignalAndWait(MaxSafeWait));

                Assert.All(tasks, task => Assert.Equal(TaskStatus.Running, task.Status));

                // TODO: Evaluate use of safety valve.
                // Create one task that signals the MRE, and wait for it.
                Assert.True(factory.StartNew(() => flag.Trip()).Wait(MaxSafeWait));

                // Lastly, wait for the others to complete.
                Assert.True(Task.WaitAll(tasks, MaxSafeWait));
            }
        }

        // The difference between this test and the previous is the internal mechanism being tested.
        // The previous test deals with blocked task escalation (creating an additional worker).
        // This test is about skipping the internal worker pool altogether (due to TaskCreationOptions.LongRunning).
        // Currently the implementations are (nearly) identical, but may diverge in the future.
        [Fact]
        public static void RunLongRunningTaskTests()
        {
            // This is computed such that this number of long-running tasks will result in a back-up
            // without some assistance from TaskScheduler.RunBlocking() or TaskCreationOptions.LongRunning.
            int concurrencyCount = Environment.ProcessorCount * 2;

            using (Barrier startingLine = new Barrier(concurrencyCount + 1))
            {
                Flag flag = new Flag();

                // This test needs to be run with a local task scheduler, because it needs to perform
                // the verification based on a known number of initially available threads.
                //
                //
                // @TODO: When we reach the _planB branch we need to add a trick here using ThreadPool.SetMaxThread
                //        to bring down the TP worker count. This is because previous activity in the test process might have
                //        injected workers.
                TaskScheduler tm = TaskScheduler.Default;
                TaskFactory factory = new TaskFactory(CancellationToken.None, TaskCreationOptions.LongRunning, TaskContinuationOptions.None, tm);

                // Create many tasks blocked on the MRE.
                Task[] tasks = new Task[concurrencyCount];
                for (int i = 0; i < tasks.Length; i++)
                {
                    tasks[i] = factory.StartNew(() =>
                    {
                        startingLine.SignalAndWait();
                        SpinWait.SpinUntil(() => flag.IsTripped);
                    });
                }

                Assert.True(startingLine.SignalAndWait(MaxSafeWait));

                Assert.All(tasks, task => Assert.Equal(TaskStatus.Running, task.Status));

                flag.Trip();

                // Lastly, wait for the others to complete.
                Assert.True(Task.WaitAll(tasks, MaxSafeWait));
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public static void HideScheduler_Factory_StartNew(bool hideScheduler)
        {
            HideScheduler(hideScheduler, (action, scheduler) => new TaskFactory().StartNew(action,
                new CancellationToken(), hideScheduler ? TaskCreationOptions.HideScheduler : TaskCreationOptions.None, scheduler));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public static void HideScheduler_Factory_Int_StartNew(bool hideScheduler)
        {
            HideScheduler(hideScheduler, (action, scheduler) => new TaskFactory<int>().StartNew(() => { action(); return 0; },
                new CancellationToken(), hideScheduler ? TaskCreationOptions.HideScheduler : TaskCreationOptions.None, scheduler));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public static void HideScheduler_Task_Start(bool hideScheduler)
        {
            HideScheduler(hideScheduler, (action, scheduler) =>
                Start(new Task(action, hideScheduler ? TaskCreationOptions.HideScheduler : TaskCreationOptions.None), scheduler));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public static void HideScheduler_Task_Int_Start(bool hideScheduler)
        {
            HideScheduler(hideScheduler, (action, scheduler) =>
                Start(new Task<int>(() => { action(); return 0; }, hideScheduler ? TaskCreationOptions.HideScheduler : TaskCreationOptions.None), scheduler));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public static void HideScheduler_Continuation(bool hideScheduler)
        {
            HideScheduler(hideScheduler, (action, scheduler) => Task.Delay(TimeSpan.FromMilliseconds(5)).ContinueWith(ignore => { action(); },
                new CancellationToken(), hideScheduler ? TaskContinuationOptions.HideScheduler : TaskContinuationOptions.None, scheduler));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public static void HideScheduler_Continuation_Int(bool hideScheduler)
        {
            HideScheduler(hideScheduler, (action, scheduler) => Task.Delay(TimeSpan.FromMilliseconds(5)).ContinueWith(ignore => { action(); return 0; },
                new CancellationToken(), hideScheduler ? TaskContinuationOptions.HideScheduler : TaskContinuationOptions.None, scheduler));
        }

        private static void HideScheduler(bool hideScheduler, Func<Action, TaskScheduler, Task> create)
        {
            using (ManualResetEventSlim mres = new ManualResetEventSlim(false))
            using (Barrier startingLine = new Barrier(3))
            {
                TaskScheduler expectedCustomScheduler = new QUWITaskScheduler();

                TaskScheduler parentScheduler = null;
                TaskScheduler childScheduler = null;

                Task child = null;
                // Delegate creation of the parent task to allow easier testing of multiple creation methods.
                Task parent = create(() =>
                {
                    child = new Task(() =>
                    {
                        childScheduler = TaskScheduler.Current;
                        startingLine.SignalAndWait();
                        mres.Wait();
                    });
                    child.Start();
                    parentScheduler = TaskScheduler.Current;
                    startingLine.SignalAndWait();
                }, expectedCustomScheduler);

                Assert.True(startingLine.SignalAndWait(MaxSafeWait));

                Assert.True(SpinWait.SpinUntil(() => parent.Status != TaskStatus.Running, MaxSafeWait));

                Assert.True(parent.IsCompleted);
                Assert.False(parent.IsCanceled);
                Assert.False(parent.IsFaulted);
                Assert.Null(parent.Exception);
                Assert.Equal(TaskStatus.RanToCompletion, parent.Status);

                Assert.False(child.IsCompleted);
                Assert.Equal(TaskStatus.Running, child.Status);
                Assert.NotNull(childScheduler);
                if (hideScheduler)
                {
                    Assert.NotEqual(expectedCustomScheduler, parentScheduler);
                    Assert.Equal(TaskScheduler.Default, parentScheduler);
                    Assert.NotEqual(expectedCustomScheduler, childScheduler);
                    Assert.Equal(TaskScheduler.Default, childScheduler);
                }
                else
                {
                    Assert.Equal(expectedCustomScheduler, parentScheduler);
                    Assert.Equal(parentScheduler, childScheduler);
                }

                mres.Set();
            }
        }

        private static T Start<T>(T task, TaskScheduler scheduler) where T : Task
        {
            task.Start(scheduler);
            return task;
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
