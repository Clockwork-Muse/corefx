// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;

namespace System.Threading.Tasks.Tests
{
    public static class RunTests
    {
        private static readonly TimeSpan MaxSafeWait = TimeSpan.FromMinutes(1);

        [Fact]
        public static void Run_DenyChildAttach()
        {
            // Task.Run has DenyChildAttach, to prevent children tagging along
            Assert.Equal(TaskCreationOptions.DenyChildAttach, Task.Run(() => { }).CreationOptions);
            Assert.Equal(TaskCreationOptions.DenyChildAttach, Task.Run(() => 0).CreationOptions);
        }

        [Fact]
        public static void Run_Task()
        {
            Run_Task(action => Task.Run(action));
        }

        [Fact]
        public static void Run_Task_Nested()
        {
            Run_Task(action => Task.Run(() => Task.Run(action)));
        }

        [Fact]
        public static void Run_Task_Token()
        {
            Run_Task(action => Task.Run(action, new CancellationTokenSource().Token));
        }

        [Fact]
        public static void Run_Task_Nested_Token()
        {
            Run_Task(action => Task.Run(() => Task.Run(action), new CancellationTokenSource().Token));
        }

        private static void Run_Task(Func<Action, Task> create)
        {
            using (Barrier startingLine = new Barrier(2))
            {
                Flag flag = new Flag();
                Action action = () =>
                {
                    startingLine.SignalAndWait();
                    flag.Trip();
                };

                Task task = create(action);
                // Make sure task has started before continuing.
                startingLine.SignalAndWait();

                Assert.True(SpinWait.SpinUntil(() => task.IsCompleted, MaxSafeWait));

                Functions.AssertComplete(task);
                Assert.True(flag.IsTripped);
            }
        }

        [Fact]
        public static void Run_Future()
        {
            Run_Future(action => Task.Run(action));
        }

        [Fact]
        public static void Run_Future_Nested()
        {
            Run_Future(action => Task.Run(() => Task.Run(action)));
        }

        [Fact]
        public static void Run_Future_Token()
        {
            Run_Future(action => Task.Run(action, new CancellationTokenSource().Token));
        }

        [Fact]
        public static void Run_Future_Nested_Token()
        {
            Run_Future(action => Task.Run(() => Task.Run(action), new CancellationTokenSource().Token));
        }

        private static void Run_Future(Func<Func<bool>, Task<bool>> create)
        {
            using (Barrier startingLine = new Barrier(2))
            {
                Func<bool> action = () =>
                {
                    startingLine.SignalAndWait();
                    return true;
                };

                Task<bool> task = create(action);
                // Make sure task has started before continuing.
                startingLine.SignalAndWait();

                Assert.True(SpinWait.SpinUntil(() => task.IsCompleted, MaxSafeWait));
                Functions.AssertComplete(task, true);
            }
        }

        [Fact]
        public static void Run_Task_Token_PreCanceled()
        {
            Run_PreCanceled((action, token) => Task.Run(action, token));
        }

        [Fact]
        public static void Run_Task_Token_Nested_PreCanceled()
        {
            Run_PreCanceled((action, token) => Task.Run(() => Task.Run(action, token)));
        }

        [Fact]
        public static void Run_Future_Token_PreCanceled()
        {
            Run_PreCanceled((action, token) => Task.Run(() => { action(); return true; }, token));
        }

        [Fact]
        public static void Run_Future_Token_Nested_PreCanceled()
        {
            Run_PreCanceled((action, token) => Task.Run(() => Task.Run(() => { action(); return true; }, token)));
        }

        private static void Run_PreCanceled<T>(Func<Action, CancellationToken, T> create) where T : Task
        {
            CancellationTokenSource source = new CancellationTokenSource();
            source.Cancel();

            // The inner task will never be invoked.
            T task = create(() => { throw new ShouldNotBeInvokedException(); }, source.Token);

            // Task is stuck into the thread pool and scheduled.
            Assert.True(SpinWait.SpinUntil(() => task.IsCompleted, MaxSafeWait));
            Functions.AssertCanceled(task, source.Token);
        }

        [Fact]
        public static void Run_Task_Nested_Token_PreCanceled()
        {
            Run_PreCanceled_Nested((action, token) => Task.Run(() => Task.Run(action), token));
        }

        [Fact]
        public static void Run_Future_Nested_Token_PreCanceled()
        {
            Run_PreCanceled_Nested((action, token) => Task.Run(() => Task.Run(() => { action(); return true; }), token));
        }

        private static void Run_PreCanceled_Nested<T>(Func<Action, CancellationToken, T> create) where T : Task
        {
            CancellationTokenSource source = new CancellationTokenSource();
            source.Cancel();

            T task = create(() => { throw new ShouldNotBeInvokedException(); }, source.Token);

            // The created task in these cases is created canceled, via internal FromCancellation.
            Functions.AssertCanceled(task, source.Token);
        }

        [Fact]
        public static void Run_Task_Token_Cancel()
        {
            Run_Cancel((action, token) => Task.Run(action, token), TaskStatus.Running);
        }

        [Fact]
        public static void Run_Task_Token_Nested_Cancel()
        {
            Run_Cancel((action, token) => Task.Run(() => Task.Run(action, token)), TaskStatus.WaitingForActivation);
        }

        [Fact]
        public static void Run_Future_Token_Cancel()
        {
            Run_Cancel((action, token) => Task.Run(() => { action(); return true; }, token), TaskStatus.Running);
        }

        [Fact]
        public static void Run_Future_Token_Nested_Cancel()
        {
            Run_Cancel((action, token) => Task.Run(() => Task.Run(() => { action(); return true; }, token)), TaskStatus.WaitingForActivation);
        }

        private static void Run_Cancel<T>(Func<Action, CancellationToken, T> create, TaskStatus status) where T : Task
        {
            using (Barrier startingLine = new Barrier(2))
            {
                CancellationTokenSource source = new CancellationTokenSource();

                T task = create(() =>
                {
                    startingLine.SignalAndWait();
                    // Run until token canceled and we throw.
                    while (true) source.Token.ThrowIfCancellationRequested();
                }, source.Token);

                Assert.True(startingLine.SignalAndWait(MaxSafeWait));

                Assert.False(task.IsCompleted);
                Assert.False(task.IsCanceled);
                // Nested/proxy tasks are promises and thus aren't "started", even if the internal task is running.
                Assert.Equal(status, task.Status);

                source.Cancel();
                Assert.True(SpinWait.SpinUntil(() => task.IsCompleted, MaxSafeWait));

                Functions.AssertCanceled(task, source.Token);
            }
        }

        [Fact]
        public static void Run_Task_Fault()
        {
            Run_Fault(action => Task.Run(action), TaskStatus.Running);
        }

        [Fact]
        public static void Run_Task_Nested_Fault()
        {
            Run_Fault(action => Task.Run(() => Task.Run(action)), TaskStatus.WaitingForActivation);
        }

        [Fact]
        public static void Run_Task_Outer_Fault()
        {
            Run_Fault(action => Task.Run(() => { action(); return Task.Run(() => { }); }), TaskStatus.WaitingForActivation);
        }

        [Fact]
        public static void Run_Task_Token_Fault()
        {
            Run_Fault(action => Task.Run(action, new CancellationTokenSource().Token), TaskStatus.Running);
        }

        [Fact]
        public static void Run_Task_Nested_Token_Fault()
        {
            Run_Fault(action => Task.Run(() => Task.Run(action), new CancellationTokenSource().Token), TaskStatus.WaitingForActivation);
        }

        [Fact]
        public static void Run_Task_Outer_Token_Fault()
        {
            Run_Fault(action => Task.Run(() => { action(); return Task.Run(() => { }); }, new CancellationTokenSource().Token), TaskStatus.WaitingForActivation);
        }

        [Fact]
        public static void Run_Future_Fault()
        {
            Run_Fault(action => Task.Run(() => { action(); return true; }), TaskStatus.Running);
        }

        [Fact]
        public static void Run_Future_Nested_Fault()
        {
            Run_Fault(action => Task.Run(() => Task.Run(() => { action(); return true; })), TaskStatus.WaitingForActivation);
        }

        [Fact]
        public static void Run_Future_Outer_Fault()
        {
            Run_Fault(action => Task.Run(() => { action(); return Task.Run(() => true); }), TaskStatus.WaitingForActivation);
        }

        [Fact]
        public static void Run_Future_Token_Fault()
        {
            Run_Fault(action => Task.Run(() => { action(); return true; }, new CancellationTokenSource().Token), TaskStatus.Running);
        }

        [Fact]
        public static void Run_Future_Nested_Token_Fault()
        {
            Run_Fault(action => Task.Run(() => Task.Run(() => { action(); return true; }), new CancellationTokenSource().Token), TaskStatus.WaitingForActivation);
        }

        [Fact]
        public static void Run_Future_outer_Token_Fault()
        {
            Run_Fault(action => Task.Run(() => { action(); return Task.Run(() => true); }, new CancellationTokenSource().Token), TaskStatus.WaitingForActivation);
        }

        private static void Run_Fault<T>(Func<Action, T> create, TaskStatus status) where T : Task
        {
            using (Barrier startingLine = new Barrier(2))
            {
                Flag flag = new Flag();
                T task = create(() =>
                {
                    startingLine.SignalAndWait();
                    // Run until tripped
                    while (!flag.IsTripped) { /* do nothing */ }
                    throw new DeliberateTestException();
                });

                Assert.True(startingLine.SignalAndWait(MaxSafeWait));

                Assert.False(task.IsCompleted);
                Assert.False(task.IsCanceled);
                // Nested/proxy tasks are promises and thus aren't "started", even if the internal task is running.
                Assert.Equal(status, task.Status);

                flag.Trip();
                Assert.True(SpinWait.SpinUntil(() => task.IsCompleted, MaxSafeWait));

                Functions.AssertFaulted<DeliberateTestException>(task);
            }
        }

        [Fact]
        public static void Run_Task_Token_Nested_UnwrapDoesntCancel()
        {
            Run_UnwrapDoesntCancel((action, token) => Task.Run(() => Task.Run(action), token));
        }

        [Fact]
        public static void Run_Future_Token_Nested_UnwrapDoesntCancel()
        {
            Run_UnwrapDoesntCancel((action, token) => Task.Run(() => Task.Run(() => { action(); return true; }), token));
        }

        private static void Run_UnwrapDoesntCancel<T>(Func<Action, CancellationToken, T> create) where T : Task
        {
            // The point of this test is to make sure that, if the outer call to Run references the canceled token,
            // Unwrapping the proxy task doesn't cancel it if the corresponding CE is thrown.

            CancellationTokenSource source = new CancellationTokenSource();

            T task = create(() =>
            {
                source.Cancel();
                source.Token.ThrowIfCancellationRequested();
            }, source.Token);

            Assert.True(SpinWait.SpinUntil(() => task.IsCompleted, MaxSafeWait));

            Functions.AssertFaulted<OperationCanceledException>(task);
            OperationCanceledException oce = Assert.IsType<OperationCanceledException>(task.Exception.InnerException);
            Assert.Equal(source.Token, oce.CancellationToken);
        }

        [Fact]
        public static void Run_Task_FastPath_Complete()
        {
            Run_Task_FastPath_Complete(completed => Task.Run(() => completed));
        }

        [Fact]
        public static void Run_Task_Token_FastPath_Complete()
        {
            Run_Task_FastPath_Complete(completed => Task.Run(() => completed, new CancellationTokenSource().Token));
        }

        private static void Run_Task_FastPath_Complete(Func<Task, Task> create)
        {
            Task completed = new TaskFactory().StartNew(() => { /* do nothing */ });
            completed.Wait();

            Task task = create(completed);
            // The fast path isn't part of create, but rather during unwrap.  Task is still put on the scheduler.
            Assert.True(SpinWait.SpinUntil(() => task.IsCompleted, MaxSafeWait));

            Functions.AssertComplete(task);
        }

        [Fact]
        public static void Run_Future_FastPath_Complete()
        {
            Run_Future_FastPath_Complete(completed => Task.Run(() => completed));
        }

        [Fact]
        public static void Run_Future_Token_FastPath_Complete()
        {
            Run_Future_FastPath_Complete(completed => Task.Run(() => completed, new CancellationTokenSource().Token));
        }

        private static void Run_Future_FastPath_Complete(Func<Task<bool>, Task<bool>> create)
        {
            Task<bool> completed = new TaskFactory().StartNew(() => true);
            completed.Wait();

            Task<bool> task = create(completed);
            // The fast path isn't part of create, but rather during unwrap.  Task is still put on the scheduler.
            Assert.True(SpinWait.SpinUntil(() => task.IsCompleted, MaxSafeWait));

            Functions.AssertComplete(task, true);
        }

        [Fact]
        public static void Run_Task_FastPath_Cancel()
        {
            Run_Task_FastPath_Cancel(canceled => Task.Run(() => canceled));
        }

        [Fact]
        public static void Run_Task_Token_FastPath_Cancel()
        {
            Run_Task_FastPath_Cancel(canceled => Task.Run(() => canceled, new CancellationTokenSource().Token));
        }

        private static void Run_Task_FastPath_Cancel(Func<Task, Task> create)
        {
            CancellationTokenSource source = new CancellationTokenSource();

            Task completed = new TaskFactory().StartNew(() =>
            {
                source.Cancel();
                source.Token.ThrowIfCancellationRequested();
            }, source.Token);
            Functions.AssertThrowsWrapped<TaskCanceledException>(() => completed.Wait());

            Task task = create(completed);
            // The fast path isn't part of create, but rather during unwrap.  Task is still put on the scheduler.
            Assert.True(SpinWait.SpinUntil(() => task.IsCompleted, MaxSafeWait));

            Functions.AssertCanceled(task, source.Token);
        }

        [Fact]
        public static void Run_Future_FastPath_Cancel()
        {
            Run_Future_FastPath_Cancel(canceled => Task.Run(() => canceled));
        }

        [Fact]
        public static void Run_Future_Token_FastPath_Cancel()
        {
            Run_Future_FastPath_Cancel(canceled => Task.Run(() => canceled, new CancellationTokenSource().Token));
        }

        private static void Run_Future_FastPath_Cancel(Func<Task<int>, Task<int>> create)
        {
            CancellationTokenSource source = new CancellationTokenSource();

            Task<int> completed = new TaskFactory().StartNew<int>(() =>
            {
                source.Cancel();
                source.Token.ThrowIfCancellationRequested();
                return 0;
            }, source.Token);
            Functions.AssertThrowsWrapped<TaskCanceledException>(() => completed.Wait());

            Task<int> task = create(completed);
            // The fast path isn't part of create, but rather during unwrap.  Task is still put on the scheduler.
            Assert.True(SpinWait.SpinUntil(() => task.IsCompleted, MaxSafeWait));

            Functions.AssertCanceled(task, source.Token);
        }

        [Fact]
        public static void Run_Task_FastPath_Fault()
        {
            Run_Task_FastPath_Fault(faulted => Task.Run(() => faulted));
        }

        [Fact]
        public static void Run_Task_Token_FastPath_Fault()
        {
            Run_Task_FastPath_Fault(faulted => Task.Run(() => faulted, new CancellationTokenSource().Token));
        }

        private static void Run_Task_FastPath_Fault(Func<Task, Task> create)
        {
            Task completed = new TaskFactory().StartNew(() => { throw new DeliberateTestException(); });
            Functions.AssertThrowsWrapped<DeliberateTestException>(() => completed.Wait());

            Task task = create(completed);
            // The fast path isn't part of create, but rather during unwrap.  Task is still put on the scheduler.
            Assert.True(SpinWait.SpinUntil(() => task.IsCompleted, MaxSafeWait));

            Functions.AssertFaulted<DeliberateTestException>(task);
        }

        [Fact]
        public static void Run_Future_FastPath_Fault()
        {
            Run_Future_FastPath_Fault(faulted => Task.Run(() => faulted));
        }

        [Fact]
        public static void Run_Future_Token_FastPath_Fault()
        {
            Run_Future_FastPath_Fault(faulted => Task.Run(() => faulted, new CancellationTokenSource().Token));
        }

        private static void Run_Future_FastPath_Fault(Func<Task<int>, Task<int>> create)
        {
            Task<int> completed = new TaskFactory().StartNew<int>(() => { throw new DeliberateTestException(); });
            Functions.AssertThrowsWrapped<DeliberateTestException>(() => completed.Wait());

            Task<int> task = create(completed);
            // The fast path isn't part of create, but rather during unwrap.  Task is still put on the scheduler.
            Assert.True(SpinWait.SpinUntil(() => task.IsCompleted, MaxSafeWait));

            Functions.AssertFaulted<DeliberateTestException>(task);
        }

        [Fact]
        public static void Run_Task_Outer_Cancel()
        {
            Run_Task_Outer_Cancel(cancel => Task.Run(cancel));
        }

        [Fact]
        public static void Run_Task_Token_Outer_Cancel()
        {
            Run_Task_Outer_Cancel(cancel => Task.Run(cancel, new CancellationTokenSource().Token));
        }

        private static void Run_Task_Outer_Cancel(Func<Func<Task>, Task> create)
        {
            CancellationTokenSource source = new CancellationTokenSource();

            Func<Task> cancel = () =>
            {
                throw new OperationCanceledException();
            };

            Task task = create(cancel);
            Assert.True(SpinWait.SpinUntil(() => task.IsCompleted, MaxSafeWait));

            Assert.True(task.IsCompleted);
            Assert.True(task.IsCanceled);
            Assert.False(task.IsFaulted);
            Assert.Null(task.Exception);
            Assert.Equal(TaskStatus.Canceled, task.Status);

            AggregateException ae = Assert.Throws<AggregateException>(() => task.Wait());
            TaskCanceledException tce = Assert.IsType<TaskCanceledException>(ae.InnerException);
        }

        [Fact]
        public static void Run_Future_Outer_Cancel()
        {
            Run_Future_Outer_Cancel(cancel => Task.Run(cancel));
        }

        [Fact]
        public static void Run_Future_Token_Outer_Cancel()
        {
            Run_Future_Outer_Cancel(cancel => Task.Run(cancel, new CancellationTokenSource().Token));
        }

        private static void Run_Future_Outer_Cancel(Func<Func<Task<int>>, Task<int>> create)
        {
            CancellationTokenSource source = new CancellationTokenSource();

            Func<Task<int>> cancel = () =>
            {
                throw new OperationCanceledException();
            };

            Task<int> task = create(cancel);
            Assert.True(SpinWait.SpinUntil(() => task.IsCompleted, MaxSafeWait));

            Assert.True(task.IsCompleted);
            Assert.True(task.IsCanceled);
            Assert.False(task.IsFaulted);
            Assert.Null(task.Exception);
            Assert.Equal(TaskStatus.Canceled, task.Status);

            AggregateException ae = Assert.Throws<AggregateException>(() => task.Wait());
            TaskCanceledException tce = Assert.IsType<TaskCanceledException>(ae.InnerException);
        }
    }
}
