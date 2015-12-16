// Licensed to the .NET Foundation under one or more agreements. 
// The .NET Foundation licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information. 

using System.Collections.Generic;
using Xunit;

namespace System.Threading.Tasks.Tests
{
    public static class TaskStatusTests
    {
        /// <summary>
        /// Get constructed Tasks
        /// </summary>
        /// Format is:
        ///  1. Creation func: takes object state, token, and options, which may be ignored
        ///  2. optional object state
        ///  3. optional CancellationToken
        ///  4. optional TaskCreationOptions
        /// <returns>Row of data</returns>
        public static IEnumerable<object[]> Task_Constructors()
        {
            yield return new object[] { (Func<object, CancellationToken?,TaskCreationOptions?, Task>)
                ((state, token, options) => new Task(() => { })), null, null, null };
            yield return new object[] { (Func<object, CancellationToken?,TaskCreationOptions?, Task>)
                ((state, token, options) => new Task(s => { }, state)), new object(), null, null };
            yield return new object[] { (Func<object, CancellationToken?,TaskCreationOptions?, Task>)
                ((state, token, options) => new Task(() => { },  token.Value)), null, new CancellationToken(false), null };
            yield return new object[] { (Func<object, CancellationToken?,TaskCreationOptions?, Task>)
                ((state, token, options) => new Task(s => { }, state, token.Value)), new object(), new CancellationToken(false), null };

            yield return new object[] { (Func<object, CancellationToken?,TaskCreationOptions?, Task>)
                ((state, token, options) => new Task(() => { }, options.Value)), null, null, TaskCreationOptions.None };
            yield return new object[] { (Func<object, CancellationToken?,TaskCreationOptions?, Task>)
                ((state, token, options) => new Task(s => { }, state, options.Value)), new object(), null, TaskCreationOptions.None };
            yield return new object[] { (Func<object, CancellationToken?,TaskCreationOptions?, Task>)
                ((state, token, options) => new Task(() => { }, token.Value, options.Value)), null, new CancellationToken(false), TaskCreationOptions.None };
            yield return new object[] { (Func<object, CancellationToken?,TaskCreationOptions?, Task>)
                ((state, token, options) => new Task(s => { }, state, token.Value, options.Value)), new object(), new CancellationToken(false), TaskCreationOptions.None };
        }

        /// <summary>
        /// Get constructors for tasks that can be canceled.  No token is provided.
        /// </summary>
        /// Format is:
        ///  1. Creation func: takes object state, token, and options, which may be ignored
        ///  2. optional object state
        ///  3. optional TaskCreationOptions
        /// <returns>Row of data</returns>
        public static IEnumerable<object[]> Task_Cancellable_Constructors()
        {
            foreach (object[] data in Task_Constructors())
            {
                if (((CancellationToken?)data[2]).HasValue)
                {
                    yield return new object[] { data[0], data[1], data[3] };
                }
            }
        }

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
        public static void Task_Cancel_Completed()
        {
            CancellationTokenSource source = new CancellationTokenSource();

            Task task = Task.Factory.StartNew(() => { }, source.Token);

            Assert.True(task.Wait(MaxWaitTime));

            source.Cancel();

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
            Task parentTask = null;
            using (ManualResetEventSlim mres = new ManualResetEventSlim())
            using (Barrier startingLine = new Barrier(3))
            {
                parentTask = Task.Factory.StartNew(() =>
                {
                    childTask = new TaskFactory().StartNew(() => { startingLine.SignalAndWait(); mres.Wait(); }, childOptions);
                    startingLine.SignalAndWait();
                    throw new DeliberateTestException();
                });

                startingLine.SignalAndWait();
                Assert.True(SpinWait.SpinUntil(() => parentTask.Status != TaskStatus.Running, MaxWaitTime));
                if (childOptions == TaskCreationOptions.AttachedToParent)
                {
                    Assert.Equal(TaskStatus.WaitingForChildrenToComplete, parentTask.Status);
                }

                mres.Set();
            }

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
            Task parentTask = null;

            using (ManualResetEventSlim mres = new ManualResetEventSlim())
            using (Barrier startingLine = new Barrier(3))
            {
                parentTask = new TaskFactory().StartNew(() =>
               {
                   childTask = new TaskFactory().StartNew(() => { startingLine.SignalAndWait(); mres.Wait(); }, new CancellationToken(), TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
                   startingLine.SignalAndWait();
                   source.Cancel();
                   // Cancel task by manually throwing OCE
                   throw new OperationCanceledException(source.Token);
               }, source.Token);

                startingLine.SignalAndWait();
                SpinWait.SpinUntil(() => parentTask.Status != TaskStatus.Running);
                Assert.Equal(TaskStatus.WaitingForChildrenToComplete, parentTask.Status);

                mres.Set();
            }

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

        [Theory]
        [MemberData("Task_Constructors")]
        public static void Task_Created(Func<object, CancellationToken?, TaskCreationOptions?, Task> create, object state, CancellationToken? token, TaskCreationOptions? options)
        {
            Task task = create(state, token, options);
            Assert.False(task.IsCompleted);
            Assert.False(task.IsCanceled);
            Assert.False(task.IsFaulted);
            Assert.Null(task.Exception);
            Assert.Equal(options.GetValueOrDefault(), task.CreationOptions);
            Assert.Equal(state, task.AsyncState);
            Assert.Equal(TaskStatus.Created, task.Status);
        }

        [Theory]
        [MemberData("Task_Constructors")]
        public static void Task_Completed(Func<object, CancellationToken?, TaskCreationOptions?, Task> create, object state, CancellationToken? token, TaskCreationOptions? options)
        {
            Task task = create(state, token, options);

            task.Start();
            task.Wait();

            Assert.True(task.IsCompleted);
            Assert.False(task.IsCanceled);
            Assert.False(task.IsFaulted);
            Assert.Null(task.Exception);
            Assert.Equal(options.GetValueOrDefault(), task.CreationOptions);
            Assert.Equal(state, task.AsyncState);
            Assert.Equal(TaskStatus.RanToCompletion, task.Status);
        }

        [Theory]
        [MemberData("Task_Constructors")]
        public static void Task_WaitingForActivation(Func<object, CancellationToken?, TaskCreationOptions?, Task> create, object state, CancellationToken? token, TaskCreationOptions? options)
        {
            Task continuation = create(state, token, options).ContinueWith(t => { });

            Assert.Equal(TaskStatus.WaitingForActivation, continuation.Status);
        }

        [Theory]
        [MemberData("Task_Cancellable_Constructors")]
        public static void Task_Canceled_PreCanceledToken(Func<object, CancellationToken?, TaskCreationOptions?, Task> create, object state, TaskCreationOptions? options)
        {
            Task canceled = create(state, new CancellationToken(true), options);
            Assert.True(canceled.IsCompleted);
            Assert.True(canceled.IsCanceled);
            Assert.False(canceled.IsFaulted);
            Assert.Null(canceled.Exception);
            Assert.Equal(TaskStatus.Canceled, canceled.Status);
        }

        [Theory]
        [MemberData("Task_Cancellable_Constructors")]
        public static void Task_Canceled_SourceCanceledAfterCreate(Func<object, CancellationToken?, TaskCreationOptions?, Task> create, object state, TaskCreationOptions? options)
        {
            CancellationTokenSource source = new CancellationTokenSource();
            Task canceled = create(state, source.Token, options);

            Assert.False(canceled.IsCompleted);
            Assert.False(canceled.IsCanceled);
            Assert.False(canceled.IsFaulted);
            Assert.Null(canceled.Exception);
            Assert.Equal(TaskStatus.Created, canceled.Status);

            source.Cancel();

            Assert.True(canceled.IsCompleted);
            Assert.True(canceled.IsCanceled);
            Assert.False(canceled.IsFaulted);
            Assert.Null(canceled.Exception);
            Assert.Equal(TaskStatus.Canceled, canceled.Status);
        }

        [Fact]
        public static void Task_Canceled()
        {
            using (ManualResetEventSlim mres = new ManualResetEventSlim())
            {
                CancellationTokenSource source = new CancellationTokenSource();

                Task cancel = new TaskFactory().StartNew(() =>
                {
                    mres.Set();
                    source.Cancel();
                    source.Token.ThrowIfCancellationRequested();
                }, source.Token);

                // There are multiple potential states a task can go through before it starts running.
                // Wait until it runs, then spin until it is no longer running.
                Assert.True(mres.Wait(MaxWaitTime));
                Assert.True(SpinWait.SpinUntil(() => cancel.Status != TaskStatus.Running, MaxWaitTime));

                Assert.Equal(TaskStatus.Canceled, cancel.Status);
                Assert.True(cancel.IsCompleted);
                Assert.True(cancel.IsCanceled);
                Assert.False(cancel.IsFaulted);
                Assert.Null(cancel.Exception);

                Functions.AssertThrowsWrapped<TaskCanceledException>(() => cancel.Wait());
            }
        }

        [Fact]
        public static void Task_FaultedNotChangedByCancel()
        {
            CancellationTokenSource source = new CancellationTokenSource();

            Task task = new TaskFactory().StartNew(() => { throw new DeliberateTestException(); }, source.Token);
            Functions.AssertThrowsWrapped<DeliberateTestException>(() => task.Wait());

            Assert.True(task.IsCompleted);
            Assert.True(task.IsFaulted);
            Assert.False(task.IsCanceled);
            Assert.Equal(TaskStatus.Faulted, task.Status);

            source.Cancel();

            Assert.True(task.IsCompleted);
            Assert.True(task.IsFaulted);
            Assert.False(task.IsCanceled);
            Assert.Equal(TaskStatus.Faulted, task.Status);
        }

        [Fact]
        public static void Task_Canceled_ChildTask_Faulted_Priority()
        {
            CancellationTokenSource source = new CancellationTokenSource();

            Task task = new TaskFactory().StartNew(() =>
            {
                Task exceptionalChild = new TaskFactory().StartNew(() => { throw new DeliberateTestException(); }, TaskCreationOptions.AttachedToParent);

                source.Cancel();
                throw new OperationCanceledException(source.Token);
            }, source.Token);

            AggregateException outer = Assert.Throws<AggregateException>(() => task.Wait());
            Assert.IsType<DeliberateTestException>(outer.GetBaseException());

            Assert.True(task.IsCompleted);
            Assert.True(task.IsFaulted);
            Assert.False(task.IsCanceled);
            Assert.Equal(TaskStatus.Faulted, task.Status);
        }

        [Fact]
        public static void Task_AllChildrenComplete()
        {
            // More children than hardware threads, means not all children will actually run at the same time
            int children = Environment.ProcessorCount * 10;

            int childrenRan = 0;

            Task parent = new TaskFactory().StartNew(() =>
            {
                for (int i = 0; i < children; i++)
                {
                    new TaskFactory().StartNew(() => Interlocked.Increment(ref childrenRan), TaskCreationOptions.AttachedToParent);
                }
            });

            // Keep main thread alive until children complete
            SpinWait.SpinUntil(() => parent.Status == TaskStatus.RanToCompletion);
            Assert.Equal(children, childrenRan);
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
