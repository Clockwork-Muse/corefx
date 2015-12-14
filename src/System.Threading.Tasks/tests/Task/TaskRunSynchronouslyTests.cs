// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// TaskRunSync.cs
//
//
// Test class using UnitTestDriver that ensures that the Runsynchronously method works as excepted
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System.Collections.Generic;
using Xunit;

namespace System.Threading.Tasks.Tests
{
    public static class TaskRunSynchronouslyTests
    {
        [Theory]
        [InlineData(TaskCreationOptions.None)]
        [InlineData(TaskCreationOptions.LongRunning)]
        public static void RunSynchronously_Default(TaskCreationOptions options)
        {
            int currentThreadId = Environment.CurrentManagedThreadId;
            int? taskThreadId = null;

            Task task = new Task(() => taskThreadId = Environment.CurrentManagedThreadId, options);

            task.RunSynchronously();

            Assert.True(taskThreadId.HasValue);
            Assert.Equal(currentThreadId, taskThreadId);

            Assert.True(task.IsCompleted);
            Assert.False(task.IsFaulted);
            Assert.False(task.IsCanceled);
            Assert.Equal(TaskStatus.RanToCompletion, task.Status);

            Assert.True(task.Wait(0));
        }

        [Theory]
        [InlineData(true, TaskCreationOptions.None)]
        [InlineData(false, TaskCreationOptions.None)]
        [InlineData(true, TaskCreationOptions.LongRunning)]
        [InlineData(false, TaskCreationOptions.LongRunning)]
        public static void RunSynchronously_Scheduler(bool executeInline, TaskCreationOptions options)
        {
            int currentThreadId = Environment.CurrentManagedThreadId;
            int? taskThreadId = null;
            TaskScheduler observedScheduler = null;

            Task task = new Task(() =>
            {
                taskThreadId = Environment.CurrentManagedThreadId;
                observedScheduler = TaskScheduler.Current;
            }, options);
            QUWITaskScheduler scheduler = new QUWITaskScheduler(executeInline);

            task.RunSynchronously(scheduler);

            Assert.True(taskThreadId.HasValue);
            if (executeInline)
            {
                Assert.Equal(currentThreadId, taskThreadId);
            }
            else
            {
                Assert.NotEqual(currentThreadId, taskThreadId);
            }
            Assert.Equal(1, scheduler.TryExecuteTaskInlineCount);
            Assert.Equal(executeInline ? 0 : 1, scheduler.QueueTaskCount);
            Assert.Equal(scheduler, observedScheduler);

            Assert.True(task.IsCompleted);
            Assert.False(task.IsFaulted);
            Assert.False(task.IsCanceled);
            Assert.Equal(TaskStatus.RanToCompletion, task.Status);

            Assert.True(task.Wait(0));
        }

        [Theory]
        [InlineData(TaskCreationOptions.None)]
        [InlineData(TaskCreationOptions.LongRunning)]
        public static void RunSynchronously_TaskCanceled_Default(TaskCreationOptions options)
        {
            TaskCanceled(options, willCancel => willCancel.RunSynchronously());
        }

        [Theory]
        [InlineData(true, TaskCreationOptions.None)]
        [InlineData(false, TaskCreationOptions.None)]
        [InlineData(true, TaskCreationOptions.LongRunning)]
        [InlineData(false, TaskCreationOptions.LongRunning)]
        public static void RunSynchronously_TaskCanceled_Scheduler(bool executeInline, TaskCreationOptions options)
        {
            Action<Task> withScheduler = willCancel =>
            {
                QUWITaskScheduler scheduler = new QUWITaskScheduler(executeInline);
                willCancel.RunSynchronously(scheduler);
                Assert.Equal(1, scheduler.TryExecuteTaskInlineCount);
                Assert.Equal(executeInline ? 0 : 1, scheduler.QueueTaskCount);
            };

            TaskCanceled(options, withScheduler);
        }

        private static void TaskCanceled(TaskCreationOptions options, Action<Task> call)
        {
            CancellationTokenSource source = new CancellationTokenSource();
            Task willCancel = new Task(() => { source.Cancel(); source.Token.ThrowIfCancellationRequested(); }, source.Token, options);

            call(willCancel);

            Assert.True(willCancel.IsCompleted);
            Assert.False(willCancel.IsFaulted);
            Assert.True(willCancel.IsCanceled);
            Assert.Equal(TaskStatus.Canceled, willCancel.Status);
            Assert.Null(willCancel.Exception);

            Functions.AssertThrowsWrapped<TaskCanceledException>(() => willCancel.Wait());
        }

        [Theory]
        [InlineData(TaskCreationOptions.None)]
        [InlineData(TaskCreationOptions.LongRunning)]
        public static void RunSynchronously_TaskFaulted_Default(TaskCreationOptions options)
        {
            TaskFaulted(options, willFault => willFault.RunSynchronously());
        }

        [Theory]
        [InlineData(true, TaskCreationOptions.None)]
        [InlineData(false, TaskCreationOptions.None)]
        [InlineData(true, TaskCreationOptions.LongRunning)]
        [InlineData(false, TaskCreationOptions.LongRunning)]
        public static void RunSynchronously_TaskFaulted_Scheduler(bool executeInline, TaskCreationOptions options)
        {
            Action<Task> withScheduler = willFault =>
            {
                QUWITaskScheduler scheduler = new QUWITaskScheduler(executeInline);
                willFault.RunSynchronously(scheduler);
                Assert.Equal(1, scheduler.TryExecuteTaskInlineCount);
                Assert.Equal(executeInline ? 0 : 1, scheduler.QueueTaskCount);
            };

            TaskCanceled(options, withScheduler);
        }

        private static void TaskFaulted(TaskCreationOptions options, Action<Task> call)
        {
            Task willFault = new Task(() => { throw new DeliberateTestException(); }, options);

            // Exception is not thrown during RunSynchronously
            call(willFault);

            Assert.True(willFault.IsCompleted);
            Assert.True(willFault.IsFaulted);
            Assert.False(willFault.IsCanceled);
            Assert.Equal(TaskStatus.Faulted, willFault.Status);
            AggregateException ae = Assert.IsType<AggregateException>(willFault.Exception);
            Assert.IsType<DeliberateTestException>(ae.InnerException);

            Functions.AssertThrowsWrapped<DeliberateTestException>(() => willFault.Wait());
        }

        [Fact]
        public static void RunSynchronously_ArgumentNull()
        {
            Assert.Throws<ArgumentNullException>(() => new Task(() => { }).RunSynchronously(null));
        }

        [Fact]
        public static void RunSynchronously_InvalidOperation()
        {
            Assert.Throws<InvalidOperationException>(() => CompletedTask().RunSynchronously());
            Assert.Throws<InvalidOperationException>(() => CompletedTask().RunSynchronously(new NotInvokedScheduler()));
            Assert.Throws<InvalidOperationException>(() => Task.FromResult(0).RunSynchronously());
            Assert.Throws<InvalidOperationException>(() => Task.FromResult(0).RunSynchronously(new NotInvokedScheduler()));
            Assert.Throws<InvalidOperationException>(() => Task.FromException(new DeliberateTestException()).RunSynchronously());
            Assert.Throws<InvalidOperationException>(() => Task.FromException(new DeliberateTestException()).RunSynchronously(new NotInvokedScheduler()));
            Assert.Throws<InvalidOperationException>(() => Task.FromCanceled(new CancellationToken(true)).RunSynchronously());
            Assert.Throws<InvalidOperationException>(() => Task.FromCanceled(new CancellationToken(true)).RunSynchronously(new NotInvokedScheduler()));

            Assert.Throws<InvalidOperationException>(() => new Task(() => { }).ContinueWith(i => { }).RunSynchronously());
            Assert.Throws<InvalidOperationException>(() => new Task(() => { }).ContinueWith(i => { }).RunSynchronously(new NotInvokedScheduler()));
            Assert.Throws<InvalidOperationException>(() => new Task(() => { }).ContinueWith(i => 0).RunSynchronously());
            Assert.Throws<InvalidOperationException>(() => new Task(() => { }).ContinueWith(i => 0).RunSynchronously(new NotInvokedScheduler()));
            Assert.Throws<InvalidOperationException>(() => new Task<int>(() => 0).ContinueWith(i => { }).RunSynchronously());
            Assert.Throws<InvalidOperationException>(() => new Task<int>(() => 0).ContinueWith(i => { }).RunSynchronously(new NotInvokedScheduler()));
            Assert.Throws<InvalidOperationException>(() => new Task<int>(() => 0).ContinueWith(i => 0).RunSynchronously());
            Assert.Throws<InvalidOperationException>(() => new Task<int>(() => 0).ContinueWith(i => 0).RunSynchronously(new NotInvokedScheduler()));
            Assert.Throws<InvalidOperationException>(() => new TaskCompletionSource<int>().Task.RunSynchronously());
            Assert.Throws<InvalidOperationException>(() => new TaskCompletionSource<int>().Task.RunSynchronously(new NotInvokedScheduler()));

            Flag flag = new Flag();
            try
            {
                Assert.Throws<InvalidOperationException>(() => Task.Run(() => { while (!flag.IsTripped) { } }).RunSynchronously());
                Assert.Throws<InvalidOperationException>(() => Task.Run(() => { while (!flag.IsTripped) { } }).RunSynchronously(new NotInvokedScheduler()));
            }
            finally
            {
                flag.Trip();
            }
        }

        /// <summary>
        /// Get a unique completed task.
        /// </summary>
        /// Task.CompletedTask returns a cached instance;
        ///  this version returns a unique one on each call.
        /// <returns>A unique completed task</returns>
        private static Task CompletedTask()
        {
            Task completed = Task.Run(() => { /* do nothing */ });
            completed.Wait();
            return completed;
        }

        /// <summary>
        /// This scheduler throws on every operation.
        /// </summary>
        private class NotInvokedScheduler : TaskScheduler
        {
            protected override IEnumerable<Task> GetScheduledTasks()
            {
                throw new ShouldNotBeInvokedException();
            }

            protected override void QueueTask(Task task)
            {
                throw new ShouldNotBeInvokedException();
            }

            protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
            {
                throw new ShouldNotBeInvokedException();
            }
        }
    }
}
