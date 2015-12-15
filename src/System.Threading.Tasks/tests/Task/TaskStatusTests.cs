// Licensed to the .NET Foundation under one or more agreements. 
// The .NET Foundation licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information. 

using System.Collections.Generic;
using Xunit;

namespace System.Threading.Tasks.Tests
{
    public static class TaskStatusTests
    {
        // Maximum wait time to avoid permanent deadlock in tests.
        private const int MaxWaitTime = 1000;

        [Fact]
        public static void Promise_WaitingForActivation()
        {
            Assert.Equal(TaskStatus.WaitingForActivation, new TaskCompletionSource<int>().Task.Status);
        }

        [Fact]
        public static void Promise_Completed()
        {
            TaskCompletionSource<int> completion = new TaskCompletionSource<int>();
            completion.SetResult(1);
            Assert.Equal(TaskStatus.RanToCompletion, completion.Task.Status);
            Assert.Equal(1, completion.Task.Result);
        }

        [Fact]
        public static void Promise_Faulted()
        {
            TaskCompletionSource<int> completion = new TaskCompletionSource<int>();
            completion.SetException(new DeliberateTestException());
            Assert.Equal(TaskStatus.Faulted, completion.Task.Status);
            Assert.NotNull(completion.Task.Exception);
            Assert.All(completion.Task.Exception.InnerExceptions, e => Assert.IsType<DeliberateTestException>(e));
        }

        [Fact]
        public static void Task_Cancel_Scheduled()
        {
            // Custom scheduler to control timing of cancel
            CancelWaitingToRunTaskScheduler scheduler = new CancelWaitingToRunTaskScheduler();
            Task task = Task.Factory.StartNew(() => { }, scheduler.Token, TaskCreationOptions.None, scheduler);

            // This version is used for clarity:
            //   - Assert.ThrowsAsync<TaskCanceledException>(() => task); would appear mysterious
            Assert.Throws<TaskCanceledException>(() => task.GetAwaiter().GetResult());
        }

        [Fact]
        public static void Task_Cancel_Created()
        {
            CancellationTokenSource source = new CancellationTokenSource();

            Task task = new Task(() => { }, source.Token);

            Assert.Equal(TaskStatus.Created, task.Status);
            Assert.False(task.IsCanceled);

            source.Cancel();

            Assert.Equal(TaskStatus.Canceled, task.Status);
            Assert.True(task.IsCanceled);
        }

        [Fact]
        public static void Task_Cancel_Running()
        {
            CancellationTokenSource source = new CancellationTokenSource();

            Task task = Task.Factory.StartNew(() => source.Cancel(), source.Token);

            Assert.True(task.Wait(MaxWaitTime));

            Assert.Equal(TaskStatus.RanToCompletion, task.Status);
            Assert.True(task.IsCompleted);
            Assert.False(task.IsCanceled);
        }

        [Fact]
        public static void Task_Faulted()
        {
            Task task = Task.Factory.StartNew(() => { throw new DeliberateTestException(); });
            Functions.AssertThrowsWrapped<DeliberateTestException>(() => task.Wait());
        }

        [Fact]
        public static void Task_Completed()
        {
            using (ManualResetEventSlim outer = new ManualResetEventSlim(false))
            using (ManualResetEventSlim inner = new ManualResetEventSlim(false))
            {
                Task task = Task.Factory.StartNew(() =>
                {
                    // Signal the test thread the task started running,
                    // then wait for it's check to complete
                    outer.Set();
                    Assert.True(inner.Wait(MaxWaitTime));
                });

                Assert.True(outer.Wait(MaxWaitTime));

                // Check task is running, then let it complete
                Assert.Equal(TaskStatus.Running, task.Status);
                inner.Set();

                // Wait for task to complete before checking final status.
                Assert.True(task.Wait(MaxWaitTime));
                Assert.Equal(TaskStatus.RanToCompletion, task.Status);
                Assert.True(task.IsCompleted);
            }
        }

        [Fact]
        public static void Task_ChildTask_Completed()
        {
            using (ManualResetEventSlim test = new ManualResetEventSlim(false))
            using (ManualResetEventSlim parent = new ManualResetEventSlim(false))
            using (ManualResetEventSlim child = new ManualResetEventSlim(false))
            {
                Task childTask = null;

                Task parentTask = Task.Factory.StartNew(() =>
                {
                    // Start the child thread,
                    // then wait for it's check to complete
                    childTask = Task.Factory.StartNew(() =>
                    {
                        test.Set();
                        Assert.True(child.Wait(MaxWaitTime));
                    }, TaskCreationOptions.AttachedToParent);
                    Assert.True(parent.Wait(MaxWaitTime));
                });

                Assert.True(test.Wait(MaxWaitTime));

                // Check tasks are running, then let them complete
                Assert.Equal(TaskStatus.Running, parentTask.Status);
                Assert.Equal(TaskStatus.Running, childTask.Status);
                // Let parent task 'finish', and wait for child.
                parent.Set();
                while (parentTask.Status == TaskStatus.Running) { /* do nothing */ };
                Assert.Equal(TaskStatus.WaitingForChildrenToComplete, parentTask.Status);

                child.Set();

                // Wait for tasks to complete before checking final status.
                Assert.True(Task.WaitAll(new[] { parentTask, childTask }, MaxWaitTime));
                Assert.Equal(TaskStatus.RanToCompletion, parentTask.Status);
                Assert.True(parentTask.IsCompleted);
                Assert.Equal(TaskStatus.RanToCompletion, childTask.Status);
                Assert.True(childTask.IsCompleted);
            }
        }

        [Fact]
        public static void Task_ChildTask_Completed_SourceCanceled()
        {
            CancellationTokenSource source = new CancellationTokenSource();

            Task childTask = null;

            Task parentTask = Task.Factory.StartNew(() =>
            {
                // Start the child thread,
                // then wait for it's check to complete
                childTask = Task.Factory.StartNew(() =>
                {
                    // Need to cancel once child task started, or created canceled.
                    source.Cancel();
                }, source.Token, TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
            }, source.Token);

            // Wait for tasks to complete before checking final status.
            Assert.True(parentTask.Wait(MaxWaitTime));
            Assert.Equal(TaskStatus.RanToCompletion, parentTask.Status);
            Assert.True(parentTask.IsCompleted);
            Assert.False(parentTask.IsCanceled);
            Assert.Equal(TaskStatus.RanToCompletion, childTask.Status);
            Assert.True(childTask.IsCompleted);
            Assert.False(childTask.IsCanceled);
        }

        [Fact]
        public static void Task_ChildTask_Canceled()
        {
            CancellationTokenSource source = new CancellationTokenSource();

            Task childTask = null;

            Task parentTask = Task.Factory.StartNew(() =>
            {
                // Start the child thread,
                // then wait for it's check to complete
                childTask = Task.Factory.StartNew(() =>
                {
                    source.Cancel();
                    // Cancel child task by manually throwing OCE
                    throw new OperationCanceledException(source.Token);
                }, source.Token, TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
            }, source.Token);

            // Wait for tasks to complete before checking final status.
            Assert.True(parentTask.Wait(MaxWaitTime));
            Assert.Equal(TaskStatus.RanToCompletion, parentTask.Status);
            Assert.True(parentTask.IsCompleted);
            Assert.False(parentTask.IsCanceled);
            Assert.Equal(TaskStatus.Canceled, childTask.Status);
            Assert.True(childTask.IsCompleted);
            Assert.True(childTask.IsCanceled);
        }

        [Theory]
        [InlineData(TaskCreationOptions.None)]
        [InlineData(TaskCreationOptions.AttachedToParent)]
        public static void Task_Faulted_ChildTask_Completed(TaskCreationOptions childOptions)
        {
            Task childTask = null;

            Task parentTask = Task.Factory.StartNew(() =>
            {
                childTask = Task.Factory.StartNew(() => { }, childOptions);

                throw new DeliberateTestException();
            });

            // Wait for tasks to complete before checking final status.
            Functions.AssertThrowsWrapped<DeliberateTestException>(() => parentTask.Wait(MaxWaitTime));
            Assert.Equal(TaskStatus.Faulted, parentTask.Status);
            Assert.True(parentTask.IsCompleted);
            Assert.True(parentTask.IsFaulted);
            Assert.Equal(TaskStatus.RanToCompletion, childTask.Status);
            Assert.True(childTask.IsCompleted);
        }

        [Theory]
        [InlineData(TaskCreationOptions.None)]
        [InlineData(TaskCreationOptions.AttachedToParent)]
        public static void Task_ChildTask_Faulted(TaskCreationOptions childOptions)
        {
            Task childTask = null;

            Task parentTask = Task.Factory.StartNew(() =>
            {
                childTask = Task.Factory.StartNew(() => { throw new DeliberateTestException(); }, childOptions);
            });

            if (childOptions == TaskCreationOptions.AttachedToParent)
            {
                AggregateException ae = Assert.Throws<AggregateException>(() => parentTask.Wait(MaxWaitTime));
                // Picks up internal exception from child task
                AggregateException inner = Assert.IsType<AggregateException>(Assert.Single(ae.InnerExceptions));
                Assert.IsType<DeliberateTestException>(Assert.Single(inner.InnerExceptions));
                Assert.Equal(TaskStatus.Faulted, parentTask.Status);
                Assert.True(parentTask.IsFaulted);
            }
            else
            {
                parentTask.Wait(MaxWaitTime);
                Assert.Equal(TaskStatus.RanToCompletion, parentTask.Status);
                Assert.False(parentTask.IsFaulted);
                // If not attatched, child task may run after/longer
                while (childTask.Status == TaskStatus.Running) { /* Do nothing */ }
            }
            Assert.True(parentTask.IsCompleted);
            Assert.Equal(TaskStatus.Faulted, childTask.Status);
            Assert.IsType<DeliberateTestException>(Assert.Single(childTask.Exception.InnerExceptions));
            Assert.True(childTask.IsCompleted);
            Assert.True(childTask.IsFaulted);
        }

        [Fact]
        public static void Task_Canceled_ChildTask_Completed()
        {
            CancellationTokenSource source = new CancellationTokenSource();

            Task childTask = null;

            Task parentTask = Task.Factory.StartNew(() =>
            {
                childTask = Task.Factory.StartNew(() => { }, new CancellationToken(), TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
                source.Cancel();
                // Cancel task by manually throwing OCE
                throw new OperationCanceledException(source.Token);
            }, source.Token);

            // Wait for tasks to complete before checking final status.
            Functions.AssertThrowsWrapped<TaskCanceledException>(() => parentTask.Wait(MaxWaitTime));
            Assert.Equal(TaskStatus.Canceled, parentTask.Status);
            Assert.True(parentTask.IsCompleted);
            Assert.True(parentTask.IsCanceled);
            Assert.Equal(TaskStatus.RanToCompletion, childTask.Status);
            Assert.True(childTask.IsCompleted);
            Assert.False(childTask.IsCanceled);
        }

        [Fact]
        public static void Task_Canceled_ChildTask_Canceled()
        {
            CancellationTokenSource source = new CancellationTokenSource();

            Task childTask = null;

            Task parentTask = Task.Factory.StartNew(() =>
            {
                childTask = Task.Factory.StartNew(() =>
                {
                    source.Cancel();
                    throw new OperationCanceledException(source.Token);
                }, source.Token, TaskCreationOptions.AttachedToParent, TaskScheduler.Default);

                // Require source.Cancel() in both places to ward against race conditions.
                source.Cancel();
                // Cancel task by manually throwing OCE
                throw new OperationCanceledException(source.Token);
            }, source.Token);

            // Type made more specific by task platform
            Functions.AssertThrowsWrapped<TaskCanceledException>(() => parentTask.Wait(MaxWaitTime));
            Assert.Equal(TaskStatus.Canceled, parentTask.Status);
            Assert.True(parentTask.IsCompleted);
            Assert.True(parentTask.IsCanceled);
            Assert.Equal(TaskStatus.Canceled, childTask.Status);
            Assert.True(childTask.IsCompleted);
            Assert.True(childTask.IsCanceled);
        }

        // Test that TaskStatus values returned from Task.Status are what they should be.
        // TODO: Test WaitingToRun, Blocked.
        [Fact]
        public static void RunTaskStatusTests()
        {
            Task t;
            TaskStatus ts;
            ManualResetEvent mre = new ManualResetEvent(false);

            //
            // Test for TaskStatus.Created
            //
            {
                t = new Task(delegate { });
                ts = t.Status;
                if (ts != TaskStatus.Created)
                {
                    Assert.True(false, string.Format("RunTaskStatusTests - TaskStatus.Create:    > FAILED.  Expected Created status, got {0}", ts));
                }
                if (t.IsCompleted)
                {
                    Assert.True(false, string.Format("RunTaskStatusTests - TaskStatus.Create:    > FAILED.  Expected IsCompleted to be false."));
                }
            }

            //
            // Test for TaskStatus.WaitingForActivation
            //
            {
                Task ct = t.ContinueWith(delegate { });
                ts = ct.Status;
                if (ts != TaskStatus.WaitingForActivation)
                {
                    Assert.True(false, string.Format("RunTaskStatusTests - TaskStatus.WaitingForActivation:    > FAILED.  Expected WaitingForActivation status (continuation), got {0}", ts));
                }
                if (ct.IsCompleted)
                {
                    Assert.True(false, string.Format("RunTaskStatusTests - TaskStatus.WaitingForActivation:    > FAILED.  Expected IsCompleted to be false."));
                }

                TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
                ts = tcs.Task.Status;
                if (ts != TaskStatus.WaitingForActivation)
                {
                    Assert.True(false, string.Format("RunTaskStatusTests - TaskStatus.WaitingForActivation:    > FAILED.  Expected WaitingForActivation status (TCS), got {0}", ts));
                }
                if (tcs.Task.IsCompleted)
                {
                    Assert.True(false, string.Format("RunTaskStatusTests - TaskStatus.WaitingForActivation:    > FAILED.  Expected IsCompleted to be false."));
                }
                tcs.TrySetCanceled();
            }

            //
            // Test for TaskStatus.Canceled for unstarted task being created with an already signaled CTS (this became a case of interest with the TPL Cancellation DCR)
            //
            {
                CancellationTokenSource cts = new CancellationTokenSource();
                CancellationToken token = cts.Token;
                cts.Cancel();
                t = new Task(delegate { }, token);  // should immediately transition into cancelled state

                ts = t.Status;
                if (ts != TaskStatus.Canceled)
                {
                    Assert.True(false, string.Format("RunTaskStatusTests - TaskStatus.Canceled (unstarted Task) (already signaled CTS):    > FAILED.  Expected Canceled status, got {0}", ts));
                }
            }

            //
            // Test for TaskStatus.Canceled for unstarted task being created with an already signaled CTS (this became a case of interest with the TPL Cancellation DCR)
            //
            {
                CancellationTokenSource cts = new CancellationTokenSource();
                CancellationToken token = cts.Token;

                t = new Task(delegate { }, token);  // should immediately transition into cancelled state
                cts.Cancel();

                ts = t.Status;
                if (ts != TaskStatus.Canceled)
                {
                    Assert.True(false, string.Format("RunTaskStatusTests - TaskStatus.Canceled (unstarted Task) (CTS signaled after ctor):   > FAILED.  Expected Canceled status, got {0}", ts));
                }
            }

            //
            // Test that Task whose CT gets canceled while it's running but
            // which doesn't throw an OCE to acknowledge cancellation will end up in RunToCompletion state
            //
            {
                CancellationTokenSource ctsource = new CancellationTokenSource();
                CancellationToken ctoken = ctsource.Token;

                t = Task.Factory.StartNew(delegate { ctsource.Cancel(); }, ctoken); // cancel but don't acknowledge
                try { t.Wait(); }
                catch { }

                ts = t.Status;
                if (ts != TaskStatus.RanToCompletion)
                {
                    Assert.True(false, string.Format("RunTaskStatusTests - Internal Cancellation:    > FAILED.  Expected RanToCompletion status, got {0}", ts));
                }
                if (!t.IsCompleted)
                {
                    Assert.True(false, string.Format("RunTaskStatusTests - Internal Cancellation:    > FAILED.  Expected IsCompleted to be true."));
                }
            }

            mre.Reset();

            //
            // Test for TaskStatus.Running
            //
            ManualResetEvent mre2 = new ManualResetEvent(false);
            t = Task.Factory.StartNew(delegate { mre2.Set(); mre.WaitOne(); });
            mre2.WaitOne();
            mre2.Reset();
            ts = t.Status;
            if (ts != TaskStatus.Running)
            {
                Assert.True(false, string.Format("RunTaskStatusTests - TaskStatus.Running:    > FAILED.  Expected Running status, got {0}", ts));
            }
            if (t.IsCompleted)
            {
                Assert.True(false, string.Format("RunTaskStatusTests - TaskStatus.Running:    > FAILED.  Expected IsCompleted to be false."));
            }

            // Causes previously created task to finish
            mre.Set();

            //
            // Test for TaskStatus.WaitingForChildrenToComplete
            //
            mre.Reset();
            ManualResetEvent childCreatedMre = new ManualResetEvent(false);
            t = Task.Factory.StartNew(delegate
            {
                Task child = Task.Factory.StartNew(delegate { mre.WaitOne(); }, TaskCreationOptions.AttachedToParent);
                childCreatedMre.Set();
            });

            // This makes sure that task started running on a TP thread and created the child task
            childCreatedMre.WaitOne();
            // and this makes sure the delegate quit and the first stage of t.Finish() executed
            while (t.Status == TaskStatus.Running) { }

            ts = t.Status;
            if (ts != TaskStatus.WaitingForChildrenToComplete)
            {
                Assert.True(false, string.Format("RunTaskStatusTests - TaskStatus.WaitingForChildrenToComplete:    > FAILED.  Expected WaitingForChildrenToComplete status, got {0}", ts));
            }
            if (t.IsCompleted)
            {
                Assert.True(false, string.Format("RunTaskStatusTests - TaskStatus.WaitingForChildrenToComplete:    > FAILED.  Expected IsCompleted to be false."));
            }

            // Causes previously created Task(s) to finish
            mre.Set();

            //
            // Test for TaskStatus.RanToCompletion
            //
            {
                t = Task.Factory.StartNew(delegate { });
                t.Wait();
                ts = t.Status;
                if (ts != TaskStatus.RanToCompletion)
                {
                    Assert.True(false, string.Format("RunTaskStatusTests - TaskStatus.RanToCompletion:    > FAILED.  Expected RanToCompletion status, got {0}", ts));
                }
                if (!t.IsCompleted)
                {
                    Assert.True(false, string.Format("RunTaskStatusTests - TaskStatus.RanToCompletion:    > FAILED.  Expected IsCompleted to be true."));
                }
            }
        }

        // Test that TaskStatus values returned from Task.Status are what they should be.
        [Fact]
        public static void RunTaskStatusTests_NegativeTests()
        {
            Task t;
            TaskStatus ts;

            //
            // Test for TaskStatus.Canceled for post-start cancellation
            //
            {
                ManualResetEvent taskStartMRE = new ManualResetEvent(false);
                CancellationTokenSource cts = new CancellationTokenSource();
                t = Task.Factory.StartNew(delegate
                {
                    taskStartMRE.Set();
                    while (!cts.Token.IsCancellationRequested) { }
                    throw new OperationCanceledException(cts.Token);
                }, cts.Token);

                taskStartMRE.WaitOne(); //make sure the task starts running before we cancel it
                cts.Cancel();

                // wait on the task to make sure the acknowledgement is fully processed
                try { t.Wait(); }
                catch { }

                ts = t.Status;
                if (ts != TaskStatus.Canceled)
                {
                    Assert.True(false, string.Format("RunTaskStatusTests - TaskStatus.Canceled:    > FAILED.  Expected Canceled status, got {0}", ts));
                }
                if (!t.IsCompleted)
                {
                    Assert.True(false, string.Format("RunTaskStatusTests - TaskStatus.Canceled:    > FAILED.  Expected IsCompleted to be true."));
                }
            }

            //
            // Make sure that AcknowledgeCancellation() works correctly
            //
            {
                CancellationTokenSource ctsource = new CancellationTokenSource();
                CancellationToken ctoken = ctsource.Token;

                t = Task.Factory.StartNew(delegate
                {
                    while (!ctoken.IsCancellationRequested) { }
                    throw new OperationCanceledException(ctoken);
                }, ctoken);
                ctsource.Cancel();

                try { t.Wait(); }
                catch { }

                ts = t.Status;
                if (ts != TaskStatus.Canceled)
                {
                    Assert.True(false, string.Format("RunTaskStatusTests - AcknowledgeCancellation:     > FAILED.  Expected Canceled after AcknowledgeCancellation, got {0}", ts));
                }
            }

            // Test that cancellation acknowledgement does not slip past WFCTC improperly
            {
                CancellationTokenSource cts = new CancellationTokenSource();
                bool innerStarted = false;
                SpinWait sw = new SpinWait();
                ManualResetEvent mreFaulted = new ManualResetEvent(false);
                mreFaulted.Reset();
                Task tCanceled = Task.Factory.StartNew(delegate
                {
                    Task tInner = Task.Factory.StartNew(delegate { mreFaulted.WaitOne(); }, TaskCreationOptions.AttachedToParent);
                    innerStarted = true;

                    cts.Cancel();
                    throw new OperationCanceledException(cts.Token);
                }, cts.Token);

                // and this makes sure the delegate quit and the first stage of t.Finish() executed
                while (!innerStarted || tCanceled.Status == TaskStatus.Running)
                    sw.SpinOnce();

                ts = tCanceled.Status;
                if (ts != TaskStatus.WaitingForChildrenToComplete)
                {
                    Assert.True(false, string.Format("RunTaskStatusTests - TaskStatus.WaitingForChildrenToComplete:    > canceledTask FAILED.  Expected status = WaitingForChildrenToComplete, got {0}.", ts));
                }
                if (tCanceled.IsCanceled)
                {
                    Assert.True(false, string.Format("RunTaskStatusTests - TaskStatus.WaitingForChildrenToComplete:    > canceledTask FAILED.  IsFaulted is true before children have completed."));
                }
                if (tCanceled.IsCompleted)
                {
                    Assert.True(false, string.Format("RunTaskStatusTests - TaskStatus.WaitingForChildrenToComplete:    > canceledTask FAILED.  IsCompleted is true before children have completed."));
                }

                mreFaulted.Set();
                try { tCanceled.Wait(); }
                catch { }
            }

            //
            // Test for TaskStatus.Faulted
            //
            {
                try
                {
                    CancellationTokenSource cts = new CancellationTokenSource();
                    CancellationToken ct = cts.Token;
                    t = Task.Factory.StartNew(delegate { throw new Exception("Some Unhandled Exception"); }, ct);
                    t.Wait();
                    cts.Cancel(); // Should have NO EFFECT on status, since task already completed/faulted.
                }
                catch { }
                ts = t.Status;
                if (ts != TaskStatus.Faulted)
                {
                    Assert.True(false, string.Format("RunTaskStatusTests - TaskStatus.Faulted:    > FAILED.  Expected Faulted status, got {0}", ts));
                }
                if (!t.IsCompleted)
                {
                    Assert.True(false, string.Format("RunTaskStatusTests - TaskStatus.Faulted:    > FAILED.  Expected IsCompleted to be true."));
                }
            }

            // Test that an exception does not skip past WFCTC improperly
            {
                ManualResetEvent mreFaulted = new ManualResetEvent(false);
                bool innerStarted = false;

                // I Think SpinWait has been implemented on all future platforms because
                // it is in the Contract.
                // So we can ignore this Thread.SpinWait(100);

                SpinWait sw = new SpinWait();
                Task tFaulted = Task.Factory.StartNew(delegate
                {
                    Task tInner = Task.Factory.StartNew(delegate { mreFaulted.WaitOne(); }, TaskCreationOptions.AttachedToParent);
                    innerStarted = true;
                    throw new Exception("oh no!");
                });

                // this makes sure the delegate quit and the first stage of t.Finish() executed
                while (!innerStarted || tFaulted.Status == TaskStatus.Running)
                    sw.SpinOnce();
                ts = tFaulted.Status;
                if (ts != TaskStatus.WaitingForChildrenToComplete)
                {
                    Assert.True(false, string.Format("RunTaskStatusTests - TaskStatus.WaitingForChildrenToComplete:    > faultedTask FAILED.  Expected status = WaitingForChildrenToComplete, got {0}.", ts));
                }
                if (tFaulted.IsFaulted)
                {
                    Assert.True(false, string.Format("RunTaskStatusTests - TaskStatus.WaitingForChildrenToComplete:    > faultedTask FAILED.  IsFaulted is true before children have completed."));
                }
                if (tFaulted.IsCompleted)
                {
                    Assert.True(false, string.Format("RunTaskStatusTests - TaskStatus.WaitingForChildrenToComplete:    > faultedTask FAILED.  IsCompleted is true before children have completed."));
                }

                mreFaulted.Set();
                try { tFaulted.Wait(); }
                catch { }
            }

            // Make sure that Faulted trumps Canceled
            {
                CancellationTokenSource cts = new CancellationTokenSource();
                CancellationToken ct = cts.Token;

                t = Task.Factory.StartNew(delegate
                {
                    Task exceptionalChild = Task.Factory.StartNew(delegate { throw new Exception("some exception"); }, TaskCreationOptions.AttachedToParent); //this should push an exception in our list

                    cts.Cancel();
                    throw new OperationCanceledException(ct);
                }, ct);

                try { t.Wait(); }
                catch { }

                ts = t.Status;
                if (ts != TaskStatus.Faulted)
                {
                    Assert.True(false, string.Format("RunTaskStatusTests - TaskStatus.Faulted trumps Cancelled:    > FAILED.  Expected Faulted to trump Canceled"));
                }
            }
        }

        // Just runs a task and waits on it.
        [Fact]
        public static void RunTaskWaitTest()
        {
            // wait on non-exceptional task
            Task t = Task.Factory.StartNew(delegate { });
            t.Wait();

            if (!t.IsCompleted)
            {
                Assert.True(false, string.Format("RunTaskWaitTest:  > error: task reported back !IsCompleted"));
            }

            // wait on non-exceptional delay started task
            t = new Task(delegate { });
            t.Start();
            //Timer tmr = new Timer((o) => t.Start(), null, 100, Timeout.Infinite);
            t.Wait();

            // This keeps a reference to the Timer so that it does not get GC'd
            // while we are waiting.
            //tmr.Dispose();

            if (!t.IsCompleted)
            {
                Assert.True(false, string.Format("RunTaskWaitTest:  > error: constructed task reported back !IsCompleted"));
            }

            // This keeps a reference to the Timer so that it does not get GC'd
            // while we are waiting.
            //tmr.Dispose();

            // wait on a task that has children
            int numChildren = 10;
            CountdownEvent cntEv = new CountdownEvent(numChildren);
            t = Task.Factory.StartNew(() =>
            {
                for (int i = 0; i < numChildren; i++)
                    Task.Factory.StartNew(() => { cntEv.Signal(); }, TaskCreationOptions.AttachedToParent);
            });

            t.Wait();
            if (!cntEv.IsSet)
            {
                Assert.True(false, string.Format("RunTaskWaitTest:  > error: Wait on a task with attached children returned before all children completed."));
            }
        }

        /// <summary>
        /// Custom task scheduler that cancels tasks after queuing but before execution.
        /// </summary>
        /// This scheduler is intended to be used only once.
        private class CancelWaitingToRunTaskScheduler : TaskScheduler
        {
            private CancellationTokenSource _cancellation = new CancellationTokenSource();

            public CancellationToken Token { get { return _cancellation.Token; } }

            protected override void QueueTask(Task task)
            {
                _cancellation.Cancel();
                TryExecuteTask(task);
            }

            protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
            {
                return false;
            }

            protected override IEnumerable<Task> GetScheduledTasks()
            {
                yield break;
            }
        }
    }
}
