// Licensed to the .NET Foundation under one or more agreements. 
// The .NET Foundation licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information. 


// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// Test class using UnitTestDriver that ensures all the public ctor of Task, Future and
// promise are properly working
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System.Collections.Generic;
using Xunit;

namespace System.Threading.Tasks.Tests
{
    public static class TaskCreateTests
    {
        /// <summary>
        /// Get FutureT tasks to start
        /// </summary>
        /// Format of returned data is:
        /// 1. String label (ignored)
        /// 2. Task
        /// <returns>Test data</returns>
        public static IEnumerable<object[]> FutureT_Data()
        {
            yield return new object[] { "Task<bool>", new Task<bool>(() => true) };
            yield return new object[] { "Task<bool>|CancellationToken", new Task<bool>(() => true, new CancellationTokenSource().Token) };
            yield return new object[] { "Task<bool>|state", new Task<bool>(state => { Assert.Equal(1, state); return true; }, 1) };
            yield return new object[] { "Task<bool>|state|CancellationToken",
                new Task<bool>(state => { Assert.Equal(1, state); return true; }, 1, new CancellationTokenSource().Token) };

            foreach (TaskCreationOptions options in new[] { TaskCreationOptions.None, TaskCreationOptions.LongRunning })
            {
                yield return new object[] { "Task<bool>|" + options, new Task<bool>(() => true, options) };
                yield return new object[] { "Task<bool>|CancellationToken|"+options,
                 new Task<bool>(() => true, new CancellationTokenSource().Token, options) };

                yield return new object[] { "Task<bool>|state|"+options,
                new Task<bool>(state => { Assert.Equal(1, state); return true; }, 1, options) };
                yield return new object[] { "Task<bool>|state|CancellationToken|"+options,
                new Task<bool>(state => { Assert.Equal(1, state); return true; }, 1, new CancellationTokenSource().Token, options) };
            }
        }

        /// <summary>
        /// Get Tasks to start
        /// </summary>
        /// Format of returned data is:
        /// 1. String label (ignored)
        /// 2. Func to create a task that modifies a flag
        /// <returns>Test data</returns>
        public static IEnumerable<object[]> Task_Data()
        {
            yield return new object[] { "Task", (Func<Flag, Task>)(flag => new Task(() => flag.Trip())) };
            yield return new object[] { "Task|CancellationToken", (Func<Flag, Task>)(flag => new Task(() => flag.Trip(), new CancellationTokenSource().Token)) };
            yield return new object[] { "Task|state", (Func<Flag, Task>)(flag => new Task(state => { Assert.Equal(1, state); flag.Trip(); }, 1)) };
            yield return new object[] { "Task|state|CancellationToken",
                (Func<Flag, Task>)(flag => new Task(state => { Assert.Equal(1, state); flag.Trip(); }, 1, new CancellationTokenSource().Token)) };

            foreach (TaskCreationOptions options in new[] { TaskCreationOptions.None, TaskCreationOptions.LongRunning })
            {
                yield return new object[] { "Task|" + options, (Func<Flag, Task>)(flag => new Task(() => flag.Trip(), options)) };
                yield return new object[] { "Task|CancellationToken|" + options,
                (Func<Flag, Task>)( flag => new Task(() => flag.Trip(), new CancellationTokenSource().Token, options)) };

                yield return new object[] { "Task|state|"+options,
                (Func<Flag, Task>)(flag => new Task(state => { Assert.Equal(1, state); flag.Trip(); }, 1, options)) };
                yield return new object[] { "Task|state|CancellationToken|"+options,
                (Func<Flag, Task>)(flag => new Task(state => { Assert.Equal(1, state); flag.Trip(); }, 1, new CancellationTokenSource().Token, options)) };
            }
        }

        /// <summary>
        /// Get FutureT tasks submitted via StartNew
        /// </summary>
        /// Format of returned data is:
        /// 1. String label (ignored)
        /// 2. Task (already running)
        /// <returns>Test data</returns>
        public static IEnumerable<object[]> FutureT_Started_Data()
        {
            yield return new object[] { "Task<bool>", Task<bool>.Factory.StartNew(() => true) };
            yield return new object[] { "Task<bool>|CancellationToken", Task<bool>.Factory.StartNew(() => true, new CancellationTokenSource().Token) };
            yield return new object[] { "Task<bool>|state", Task<bool>.Factory.StartNew(state => { Assert.Equal(1, state); return true; }, 1) };
            yield return new object[] { "Task<bool>|state|CancellationToken",
                Task<bool>.Factory.StartNew(state => { Assert.Equal(1, state); return true; }, 1, new CancellationTokenSource().Token) };

            yield return new object[] { "StartNew<bool>", Task.Factory.StartNew(() => true) };
            yield return new object[] { "StartNew<bool>|CancellationToken", Task.Factory.StartNew(() => true, new CancellationTokenSource().Token) };
            yield return new object[] { "StartNew<bool>|state", Task.Factory.StartNew(state => { Assert.Equal(1, state); return true; }, 1) };
            yield return new object[] { "StartNew<bool>|state|CancellationToken",
                Task.Factory.StartNew(state => { Assert.Equal(1, state); return true; }, 1, new CancellationTokenSource().Token) };
            foreach (TaskCreationOptions options in new[] { TaskCreationOptions.None, TaskCreationOptions.LongRunning })
            {
                yield return new object[] { "Task<bool>|" + options, Task<bool>.Factory.StartNew(() => true, options) };
                yield return new object[] { "Task<bool>|CancellationToken|"+options+"|TaskScheduler",
                Task<bool>.Factory.StartNew(() => true, new CancellationTokenSource().Token, options, TaskScheduler.Default) };
                yield return new object[] { "Task<bool>|state|"+options,
                Task<bool>.Factory.StartNew(state => { Assert.Equal(1, state); return true; }, 1, options) };
                yield return new object[] { "Task<bool>|state|CancellationToken|"+options+"|TaskScheduler",
                Task<bool>.Factory.StartNew(state => { Assert.Equal(1, state); return true; }, 1, new CancellationTokenSource().Token, options, TaskScheduler.Default) };

                yield return new object[] { "StartNew<bool>|" + options, Task.Factory.StartNew(() => true, options) };
                yield return new object[] { "StartNew<bool>|CancellationToken|"+options+"|TaskScheduler",
                Task.Factory.StartNew(() => true, new CancellationTokenSource().Token, options, TaskScheduler.Default) };
                yield return new object[] { "StartNew<bool>|state|"+options,
                Task.Factory.StartNew(state => { Assert.Equal(1, state); return true; }, 1, options) };
                yield return new object[] { "StartNew<bool>|state|CancellationToken|"+options+"|TaskScheduler",
                Task.Factory.StartNew(state => { Assert.Equal(1, state); return true; }, 1, new CancellationTokenSource().Token, options, TaskScheduler.Default) };
            }
        }

        /// <summary>
        /// Get Tasks to be submitted via StartNew
        /// </summary>
        /// Format of returned data is:
        /// 1. String label (ignored)
        /// 2. Func to create a running task that modifies a flag
        /// <returns>Test data</returns>
        public static IEnumerable<object[]> Task_Started_Data()
        {
            yield return new object[] { "Task", (Func<Flag, Task>)(flag => Task.Factory.StartNew(() => flag.Trip())) };
            yield return new object[] { "Task|CancellationToken", (Func<Flag, Task>)(flag => Task.Factory.StartNew(() => flag.Trip(), new CancellationTokenSource().Token)) };
            yield return new object[] { "Task|state", (Func<Flag, Task>)(flag => Task.Factory.StartNew(state => { Assert.Equal(1, state); flag.Trip(); }, 1)) };
            yield return new object[] { "Task|state|CancellationToken",
                (Func<Flag, Task>)(flag => Task.Factory.StartNew(state => { Assert.Equal(1, state); flag.Trip(); }, 1, new CancellationTokenSource().Token)) };
            foreach (TaskCreationOptions options in new[] { TaskCreationOptions.None, TaskCreationOptions.LongRunning })
            {
                yield return new object[] { "Task|" + options, (Func<Flag, Task>)(flag => Task.Factory.StartNew(() => flag.Trip(), options)) };
                yield return new object[] { "Task|CancellationToken|"+options+"|TaskScheduler",
                (Func<Flag, Task>)( flag => Task.Factory.StartNew(() => flag.Trip(), new CancellationTokenSource().Token, options,TaskScheduler.Default)) };

                yield return new object[] { "Task|state|"+options,
                (Func<Flag, Task>)(flag => Task.Factory.StartNew(state => { Assert.Equal(1, state); flag.Trip(); }, 1, options)) };
                yield return new object[] { "Task|state|CancellationToken|"+options+"|TaskScheduler",
                (Func<Flag, Task>)(flag => Task.Factory.StartNew(state => { Assert.Equal(1, state); flag.Trip(); }, 1, new CancellationTokenSource().Token, options, TaskScheduler.Default)) };
            }
        }

        [Theory]
        [MemberData("Task_Data")]
        public static void Task_Create_Test(string label, Func<Flag, Task> create)
        {
            Future_Create_Test(label, create(new Flag()));
        }

        [Theory]
        [MemberData("FutureT_Data")]
        public static void Future_Create_Test<T>(string label, T task) where T : Task
        {
            Assert.NotNull(task);
            Assert.False(task.IsCanceled);
            Assert.False(task.IsCompleted);
            Assert.False(task.IsFaulted);
            Assert.Equal(TaskStatus.Created, task.Status);
            // Required so Xunit doesn't complain during dispose
            task.RunSynchronously();
        }

        [Fact]
        public static void TaskCancellable_Test()
        {
            AssertCancellable(token => new Task<bool>(() => true, token));
            AssertCancellable(token => new Task<bool>(() => true, token, TaskCreationOptions.None));
            AssertCancellable(token => new Task<bool>(ignored => true, new object(), token, TaskCreationOptions.None));
            AssertCancellable(token => new Task(() => { }, token));
            AssertCancellable(token => new Task(() => { }, token, TaskCreationOptions.None));
            AssertCancellable(token => new Task(ignored => { }, new object(), token, TaskCreationOptions.None));
        }

        private static void AssertCancellable<T>(Func<CancellationToken, T> create) where T : Task
        {
            CancellationTokenSource source = new CancellationTokenSource();
            T task = create(source.Token);
            source.Cancel();

            Functions.AssertCanceled(task, source.Token);
        }

        [Fact]
        public static void TaskCanceled_Test()
        {
            AssertCanceled(token => new Task<bool>(() => true, token));
            AssertCanceled(token => new Task<bool>(() => true, token, TaskCreationOptions.None));
            AssertCanceled(token => new Task<bool>(ignored => true, new object(), token, TaskCreationOptions.None));
            AssertCanceled(token => new Task(() => { }, token));
            AssertCanceled(token => new Task(() => { }, token, TaskCreationOptions.None));
            AssertCanceled(token => new Task(ignored => { }, new object(), token, TaskCreationOptions.None));
        }

        private static void AssertCanceled<T>(Func<CancellationToken, T> create) where T : Task
        {
            CancellationTokenSource source = new CancellationTokenSource();
            source.Cancel();
            T task = create(source.Token);

            Functions.AssertCanceled(task, source.Token);
        }

        [Fact]
        public static void Create_Promise_Test()
        {
            Assert.NotNull(new TaskCompletionSource<object>(new object(), TaskCreationOptions.None).Task);
            Assert.NotNull(new TaskCompletionSource<object>(TaskCreationOptions.None).Task);
            Assert.NotNull(new TaskCompletionSource<object>(new object()).Task);
            Assert.NotNull(new TaskCompletionSource<object>().Task);
        }

        [Theory]
        [MemberData("FutureT_Started_Data")]
        public async static void Future_StartNew_Test(string label, Task<bool> task)
        {
            Assert.True(await task);
        }

        [Theory]
        [MemberData("Task_Started_Data")]
        public static void Task_StartNew_Test(string label, Func<Flag, Task> create)
        {
            AssertCompletes(create);
        }

        private static void AssertCompletes(Func<Flag, Task> create)
        {
            Flag flag = new Flag();
            Task task = create(flag);
            // The returned task was previously started.  Attempting to start it a second time is an error.
            Assert.Throws<InvalidOperationException>(() => task.Start());
            task.Wait();
            Assert.True(flag.IsTripped);
        }

        [Theory]
        [MemberData("FutureT_Data")]
        public async static void Future_Start_Test(string label, Task<bool> future)
        {
            Assert.True(await Start(future));
        }

        [Theory]
        [MemberData("FutureT_Data")]
        public async static void Future_Start_Scheduler_Test(string label, Task<bool> future)
        {
            Assert.True(await Start(future, TaskScheduler.Default));
        }

        [Theory]
        [MemberData("Task_Data")]
        public static void Task_Start_Test(string label, Func<Flag, Task> create)
        {
            AssertCompletes(flag => Start(create(flag)));
        }

        [Theory]
        [MemberData("Task_Data")]
        public static void Task_Start_Scheduler_Test(string label, Func<Flag, Task> create)
        {
            AssertCompletes(flag => Start(create(flag), TaskScheduler.Default));
        }

        [Theory]
        [InlineData(TaskCreationOptions.None)]
        [InlineData(TaskCreationOptions.LongRunning)]
        public static void Task_ArgumentNull(TaskCreationOptions options)
        {
            Assert.Throws<ArgumentNullException>(() => { new Task(null); });
            Assert.Throws<ArgumentNullException>(() => { new Task(null, new object()); });
            Assert.Throws<ArgumentNullException>(() => { new Task(null, new CancellationToken()); });
            Assert.Throws<ArgumentNullException>(() => { new Task(null, options); });
            Assert.Throws<ArgumentNullException>(() => { new Task(null, new object()); });
            Assert.Throws<ArgumentNullException>(() => { new Task(null, new CancellationToken(), options); });
            Assert.Throws<ArgumentNullException>(() => { new Task(null, new object(), new CancellationToken()); });
            Assert.Throws<ArgumentNullException>(() => { new Task(null, new object(), options); });
            Assert.Throws<ArgumentNullException>(() => { new Task(null, new object(), new CancellationToken(), options); });

            Assert.Throws<ArgumentNullException>(() => { new Task<int>(null); });
            Assert.Throws<ArgumentNullException>(() => { new Task<int>(null, new object()); });
            Assert.Throws<ArgumentNullException>(() => { new Task<int>(null, new CancellationToken()); });
            Assert.Throws<ArgumentNullException>(() => { new Task<int>(null, options); });
            Assert.Throws<ArgumentNullException>(() => { new Task<int>(null, new object()); });
            Assert.Throws<ArgumentNullException>(() => { new Task<int>(null, new CancellationToken(), options); });
            Assert.Throws<ArgumentNullException>(() => { new Task<int>(null, new object(), new CancellationToken()); });
            Assert.Throws<ArgumentNullException>(() => { new Task<int>(null, new object(), options); });
            Assert.Throws<ArgumentNullException>(() => { new Task<int>(null, new object(), new CancellationToken(), options); });
        }

        private static T Start<T>(T task) where T : Task
        {
            task.Start();
            return task;
        }

        private static T Start<T>(T task, TaskScheduler scheduler) where T : Task
        {
            task.Start(scheduler);
            return task;
        }

        [Fact]
        public static void StartOnContinueInvalid_Tests()
        {
            Task t = new Task(() => { }).ContinueWith(ignore => { });
            Assert.Throws<InvalidOperationException>(() => t.Start());
        }

        [Fact]
        public static void MultipleStartInvalid_Tests()
        {
            Task t = new Task(() => { });
            t.Start();
            Assert.Throws<InvalidOperationException>(() => t.Start());
        }

        [Fact]
        public static void StartOnPromiseInvalid_Tests()
        {
            TaskCompletionSource<object> completionSource = new TaskCompletionSource<object>();
            Task<object> task = completionSource.Task;
            Assert.Throws<InvalidOperationException>(() => task.Start());
        }

        [Fact]
        public async static void ArgumentNullException_Tests()
        {
            Assert.Throws<ArgumentNullException>(() => { new Task(null); });
            Assert.Throws<ArgumentNullException>(() => { new Task<object>(null); });
            await Assert.ThrowsAsync<ArgumentNullException>(() => Task.Factory.StartNew(null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => Task<object>.Factory.StartNew(null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => Task.Factory.StartNew<object>(null));

            // Task scheduler cannot be null
            Assert.Throws<ArgumentNullException>(() => new Task(() => { }).Start(null));
            Assert.Throws<ArgumentNullException>(() => new Task<object>(() => new object()).Start(null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => Task.Factory.StartNew(() => { }, new CancellationToken(), TaskCreationOptions.None, null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => Task<object>.Factory.StartNew(() => new object(), new CancellationToken(), TaskCreationOptions.None, null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => Task.Factory.StartNew<object>(() => new object(), new CancellationToken(), TaskCreationOptions.None, null));
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(32)]
        [InlineData(128)]
        public async static void InvalidTaskCreationOption_Tests(int option)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => { new Task(() => { }, (TaskCreationOptions)option); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { new Task<object>(() => new object(), (TaskCreationOptions)option); });
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => Task.Factory.StartNew(() => { }, (TaskCreationOptions)option));
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => Task<object>.Factory.StartNew(() => new object(), (TaskCreationOptions)option));
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => Task.Factory.StartNew<object>(() => new object(), (TaskCreationOptions)option));
        }
    }
}
